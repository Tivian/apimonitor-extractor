using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using SixLabors.ImageSharp;

// OK
namespace APIMonitor {
    public class CaptureFile : IEnumerable<ProcessCapture> {
        public const string CAPUTE_PREAMBLE = "\r\n\r\n\r\n\t"
            + "API Monitor 64-bit Capture\r\n\t"
            + "(c) 2011-2013, Rohitab Batra <rohitab@rohitab.com>\r\n\t" 
            + "http://www.rohitab.com/apimonitor/"
            + "\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n"
            + "\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n"
            + "\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n"
            + "RBAPM";

        public InfoFile Info { get; private set; }
        public DefinitionsFile Definitions { get; set; }
        public MonitoringFile Monitoring { get; private set; }
        private List<ProcessCapture> Processes;
        public Image Icons { get; private set; }

        public int Count => Processes.Count;

        public ProcessCapture this[int i]
            => (i < 0 || i >= Count) ? null : Processes[i];
        
        public CaptureFile(string path) {
            using var file = new FileStream(path, FileMode.Open);
            using var zip = new ZipArchive(file);
            Parse(zip);
        }

        public CaptureFile(ZipArchive zip) {
            Parse(zip);
        }
        
        private void Parse(ZipArchive zip) {
            Info = new InfoFile(zip.GetEntry("info"));
            Definitions = new DefinitionsFile(zip.GetEntry("definitions"));

            if (zip.GetEntry("log/monitoring.txt") != null)
                Monitoring = new MonitoringFile(zip.GetEntry("log/monitoring.txt"));

            Processes = new List<ProcessCapture>();
            var numberOfCaptures = 0;
            try {
                numberOfCaptures = zip.Entries.Select(x => x.FullName)
                    .Where(x => x.StartsWith("process") && !x.Contains('.'))
                    .Select(x => int.Parse(x.Split('/')[1]))
                    .Max() + 1;
            } catch { }

            if (numberOfCaptures > 0)
                Icons = Image.Load(zip.GetEntry("process/icons.bmp").ToArray());

            for (uint i = 0; i < numberOfCaptures; i++)
                Processes.Add(new ProcessCapture(zip, i));
        }

        public void Save(string filename) {
            File.WriteAllBytes(filename, ToArray());
        }

        public IEnumerator<ProcessCapture> GetEnumerator()
            => Processes.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public byte[] ToArray() {
            using var memory = new MemoryStream();
            using (var zip = new ZipArchive(memory, ZipArchiveMode.Create)) {
                zip.AddEntry("definitions", Definitions.ToArray());
                if (Monitoring != null)
                    zip.AddEntry("log/monitoring.txt", Monitoring.ToArray());
                
                foreach (var proc in Processes)
                    proc.Save(zip);

                if (Icons != null)
                    zip.AddEntry("process/icons.bmp", Icons.ToArray());
                
                zip.AddEntry("info", Info.ToArray());
            }

            var raw = memory.ToArray();
            var preamble = Encoding.ASCII.GetBytes(CAPUTE_PREAMBLE);
            var output = new byte[raw.Length + preamble.Length];
            Array.Copy(preamble, output, preamble.Length);
            Array.Copy(raw, 0, output, preamble.Length, raw.Length);
            ShiftOffsets(ref output);

            return output;
        }

        public override string ToString() {
            var sb = new StringBuilder();

            sb.AppendLine(Info.ToString());
            sb.AppendLine(Definitions.ToString());
            if (Monitoring != null)
                sb.AppendLine(Monitoring.ToString());

            foreach (var proc in Processes)
                sb.AppendLine(proc.ToString());

            return sb.ToString();
        }

        private void ShiftOffsets(ref byte[] raw)
            => ShiftOffsets(ref raw, CAPUTE_PREAMBLE.Length);

        // API Monitor adds preamble at the start of the archive which
        //  shifts all offsets by the number of bytes in the preamble.
        private void ShiftOffsets(ref byte[] raw, int offset) {
            using var memory = new MemoryStream(raw);
            using var reader = new BinaryReader(memory);
            using var writer = new BinaryWriter(memory);

            // Assuming that archive don't have file comment.
            memory.Seek(-12, SeekOrigin.End);
            var numberOfEntries = reader.ReadUInt16();

            memory.Seek(-6, SeekOrigin.End);
            var centralDirOffset = reader.ReadInt32() + offset;
            memory.Seek(-4, SeekOrigin.Current);
            // Updates central directiory offset by the offset
            writer.Write(centralDirOffset);
            memory.Seek(centralDirOffset, SeekOrigin.Begin);

            for (var i = 0; i < numberOfEntries; i++) {
                // Skips unnecessary data
                memory.Seek(28, SeekOrigin.Current);
                // Needs to read these lengths to skip the data later on
                var fileNameLength = reader.ReadInt16();
                var extraFieldLength = reader.ReadInt16();
                var commentLength = reader.ReadInt16();
                var toSkip = fileNameLength + extraFieldLength + commentLength;

                // Update offset of the zip entry
                memory.Seek(8, SeekOrigin.Current);
                var entryOffset = reader.ReadInt32() + offset;
                memory.Seek(-4, SeekOrigin.Current);
                writer.Write(entryOffset);

                // Skips file name, extra field and file comment
                memory.Seek(toSkip, SeekOrigin.Current);
            }
        }
    }
}