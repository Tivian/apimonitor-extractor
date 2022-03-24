using System.IO;
using System.IO.Compression;
using System.Text;
using Force.Crc32;

// TODO
namespace APIMonitor {
    // Represents 'info' file
    public class InfoFile : APIMonitorObject {
        public uint Version;
        public string Name;
        public uint Architecture;
        public uint IsPortable;
        public uint Unknown4; // always 6
        public uint Unknown5; // always 2
        public uint Unknown6; // always 1
        public uint Unknown7; // always 1
        public uint Unknown8; // always 1033 [409h]
        public uint Checksum;

        public InfoFile() { }
        public InfoFile(Stream stream) : base(stream) { }
        public InfoFile(ZipArchiveEntry entry) : base(entry) { }

        protected override void Init(BinaryReader reader, long length) {
            Version = reader.ReadUInt32();
            var nameLength = reader.ReadInt32();
            Name = Encoding.Unicode.GetString(reader.ReadBytes(nameLength * 2));
            Architecture = reader.ReadUInt32();
            IsPortable = reader.ReadUInt32();
            Unknown4 = reader.ReadUInt32();
            Unknown5 = reader.ReadUInt32();
            Unknown6 = reader.ReadUInt32();
            Unknown7 = reader.ReadUInt32();
            Unknown8 = reader.ReadUInt32();
            Checksum = reader.ReadUInt32();
        }

        public override byte[] ToArray() {
            using var mem = new MemoryStream();
            using var writer = new BinaryWriter(mem);

            writer.Write(Version);
            writer.Write(Name.Length);
            writer.Write(Encoding.Unicode.GetBytes(Name));
            writer.Write(Architecture);
            writer.Write(IsPortable);
            writer.Write(Unknown4);
            writer.Write(Unknown5);
            writer.Write(Unknown6);
            writer.Write(Unknown7);
            writer.Write(Unknown8);
            writer.Write(Crc32Algorithm.Compute(mem.ToArray()));
            
            return mem.ToArray();
        }
    }
}