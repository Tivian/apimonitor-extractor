using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

// OK
namespace APIMonitor {
    // Represents 'process/[Process #]/calls' file
    public class CallsFile : APIMonitorObject {
        public List<ulong> Offsets;

        public CallsFile() { }
        public CallsFile(Stream stream) : base(stream) { }
        public CallsFile(ZipArchiveEntry entry) : base(entry) { }

        public ulong this[int i]
            => (i < 0 || i >= Offsets.Count) ? 0 : Offsets[i];

        protected override void Init() {
            Offsets = new List<ulong>();
        }

        protected override void Init(BinaryReader reader, long length) {
            base.Init(reader, length);

            for (var i = 0; i < length; i += sizeof(ulong))
                Offsets.Add(reader.ReadUInt64());
        }

        public override byte[] ToArray() {
            using var mem = new MemoryStream();
            using var writer = new BinaryWriter(mem);
            
            foreach (var offset in Offsets)
                writer.Write(offset);

            return mem.ToArray();
        }
    }
}