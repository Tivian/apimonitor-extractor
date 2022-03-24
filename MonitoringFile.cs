using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

// OK
namespace APIMonitor {
    // Represents 'log/monitoring.txt' file
    public class MonitoringFile : APIMonitorObject {
        public List<string> Lines;

        public MonitoringFile() { }
        public MonitoringFile(Stream stream) : base(stream) { }
        public MonitoringFile(ZipArchiveEntry entry) : base(entry) { }

        protected override void Init() {
            Lines = new List<string>();
        }

        protected override void Init(BinaryReader reader, long length) {
            base.Init(reader, length);
            using var streamReader = new StreamReader(reader.BaseStream);

            while (!streamReader.EndOfStream)
                Lines.Add(streamReader.ReadLine());
        }

        public override byte[] ToArray() {
            using var mem = new MemoryStream();
            using var writer = new BinaryWriter(mem);
            var enc = Encoding.Unicode;
            
            writer.Write(enc.GetPreamble()); // This file uses BOM
            writer.Write(enc.GetBytes(string.Join("\r\n", Lines)));

            return mem.ToArray();
        }

        public override string ToString()
            => $"{base.ToString()}\n{string.Join("\n", Lines)}";
    }
}