using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text;

// OK
namespace APIMonitor {
    public class ProcessCapture : IEnumerable<DataFile.Call> {
        public uint ID { get; }
        public ProcessInfoFile Info { get; }
        public CallsFile Calls { get; }
        public DataFile Data { get; }

        public string Path
            => $"process/{ID}";

        public int Count => Data.Count;

        public DataFile.Call this[int i] => Data[i];

        internal ProcessCapture(ZipArchive zip, uint id) {
            ID = id;
            Info = new ProcessInfoFile(zip.GetEntry($"{Path}/info"));
            Calls = new CallsFile(zip.GetEntry($"{Path}/calls"));
            Data = new DataFile(zip.GetEntry($"{Path}/data"), Calls.Offsets);
        }

        public void Save(ZipArchive zip) {
            zip.AddEntry($"{Path}/info", Info.ToArray());
            if (Calls.Offsets.Count > 0) {
                zip.AddEntry($"{Path}/calls", Calls.ToArray());
                zip.AddEntry($"{Path}/data", Data.ToArray());
            }
        }

        public override string ToString() {
            var sb = new StringBuilder();

            sb.AppendLine(Info.ToString());
            sb.AppendLine(Calls.ToString());
            sb.AppendLine(Data.ToString());

            return sb.ToString();
        }

        public IEnumerator<DataFile.Call> GetEnumerator()
            => Data?.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}