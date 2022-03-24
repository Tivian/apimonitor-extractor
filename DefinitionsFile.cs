using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Force.Crc32;
using static APIMonitor.DataFile;

// TODO
namespace APIMonitor {
    // Represents 'definitions' file
    public class DefinitionsFile : APIMonitorObject {
        public class FunctionInfo : APIMonitorObject { // supposed to have 0x50 bytes
            public byte Unknown1; // some ID?
            public byte Unknown2; // only 0x00 or 0x10 ever spotted
            public byte NumberOfArguments;
            public byte Unknown3; // always 0 ?
            public byte Unknown4; // only multiples of 16
            public byte HasUnknownArguments;
            public uint Unknown5; // some kind of offset ?
            public ushort Unknown6; // always 0
            public uint Unknown7; // 0 - 3 [purpose unknown] [0 for custom dll]
            public uint Unknown8; // might be a bit field [0 for custom dll]
            public uint Unknown9; // looks like enum for module type [0 - custom, 1 - system, 2 - application(???)] [0 for custom dll]
            public long NameOffset;
            public long ArgumentListOffset; // not always true [TO CHECK]
            public long UnknownOffset1;
            public ulong Unknown10; // always 0
            public long ReturnTypeOffset;
            public ulong Unknown11; // always 0
            public ulong Unknown12; // 0 or 0x7FFFFFFF or 0xFFFFFFFFFFFFFFFF

            public FunctionInfo(byte[] raw, long offset) : base(raw, offset) { }

            public FunctionInfo(BinaryReader reader, long length) : base(reader, length) { }

            protected override void Init(BinaryReader reader, long length) {
                Unknown1 = reader.ReadByte();
                Unknown2 = reader.ReadByte();
                NumberOfArguments = reader.ReadByte();
                Unknown3 = reader.ReadByte();
                Unknown4 = reader.ReadByte();
                HasUnknownArguments = reader.ReadByte();
                Unknown5 = reader.ReadUInt32();
                Unknown6 = reader.ReadUInt16();
                Unknown7 = reader.ReadUInt32();
                Unknown8 = reader.ReadUInt32();
                Unknown9 = reader.ReadUInt32();
                NameOffset = reader.ReadInt64();
                ArgumentListOffset = reader.ReadInt64();
                UnknownOffset1 = reader.ReadInt64();
                Unknown10 = reader.ReadUInt64();
                ReturnTypeOffset = reader.ReadInt64();
                Unknown11 = reader.ReadUInt64();
                Unknown12 = reader.ReadUInt64();
            }

            public override byte[] ToArray() {
                using var memory = new MemoryStream();
                using var writer = new BinaryWriter(memory);

                writer.Write(Unknown1);
                writer.Write(Unknown2);
                writer.Write(NumberOfArguments);
                writer.Write(Unknown3);
                writer.Write(Unknown4);
                writer.Write(HasUnknownArguments);
                writer.Write(Unknown5);
                writer.Write(Unknown6);
                writer.Write(Unknown7);
                writer.Write(Unknown8);
                writer.Write(Unknown9);
                writer.Write(NameOffset);
                writer.Write(ArgumentListOffset);
                writer.Write(UnknownOffset1);
                writer.Write(Unknown10);
                writer.Write(ReturnTypeOffset);
                writer.Write(Unknown11);
                writer.Write(Unknown12);

                return memory.ToArray();
            }
        }

        public class TypeInfo : APIMonitorObject {
            public long NameOffset;
            public ulong TypeSize; // TO CHECK [probably enum]
            public long UnknownOffset1;
            public long UnknownOffset2;
            public long ParentTypeOffset; // TO CHECK
            public long UnknownOffset3;

            public TypeInfo(byte[] raw, long offset) : base(raw, offset) { }

            public TypeInfo(BinaryReader reader, long length) : base(reader, length) { }

            protected override void Init(BinaryReader reader, long length) {
                NameOffset = reader.ReadInt64();
                TypeSize = reader.ReadUInt64();
                UnknownOffset1 = reader.ReadInt64();
                UnknownOffset2 = reader.ReadInt64();
                ParentTypeOffset = reader.ReadInt64();
                UnknownOffset3 = reader.ReadInt64();
            }

            public override byte[] ToArray() {
                using var memory = new MemoryStream();
                using var writer = new BinaryWriter(memory);

                writer.Write(NameOffset);
                writer.Write(TypeSize);
                writer.Write(UnknownOffset1);
                writer.Write(UnknownOffset2);
                writer.Write(ParentTypeOffset);
                writer.Write(UnknownOffset3);

                return memory.ToArray();
            }
        }

        public const string DEFINITION_PREAMBLE = "\r\n\r\n\r\n\t"
            + "API Monitor Definitions\r\n\t"
            + "(c) 2011-2013, Rohitab Batra <rohitab@rohitab.com>\r\n\t"
            + "http://www.rohitab.com/apimonitor/"
            + "\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n"
            + "\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n"
            + "\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n"
            + "\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n"
            + "\r\n\r\n\r\n\r\n\r\n\r\n";

        public string Preamble;
        public byte[] UnknownBlock;
        public uint Checksum;

        public DefinitionsFile() { }
        public DefinitionsFile(Stream stream) : base(stream) { }
        public DefinitionsFile(ZipArchiveEntry entry) : base(entry) { }

        protected override void Init() {
            UnknownBlock = new byte[0];
        }

        protected override void Init(BinaryReader reader, long length) {
            base.Init(reader, length);
            Preamble = Encoding.ASCII.GetString(reader.ReadBytes(DEFINITION_PREAMBLE.Length));
            if (Preamble != DEFINITION_PREAMBLE)
                throw new IOException("Incorrect definitions preamble!");

            UnknownBlock = reader.ReadBytes((int)length - Preamble.Length - 4);
            Checksum = reader.ReadUInt32();
        }

        public FunctionInfo GetFunctionInfo(Call call) {
            return GetFunctionInfo(call.FunctionID);
        }

        public FunctionInfo GetFunctionInfo(long offset) {
            return new FunctionInfo(UnknownBlock, offset - Preamble.Length);
        }

        public string GetFunctionName(Call call) {
            return GetString(GetFunctionInfo(call).NameOffset);
        }

        public TypeInfo GetTypeInfo(long offset) {
            return new TypeInfo(UnknownBlock, offset - Preamble.Length);
        }

        public string[] GetStringList(long offset, int count) {
            var arr = new string[count];
            for (var i = 0; i < count; i++)
                arr[i] = GetString(offset + (i == 0 ? 0 : arr[i - 1].Length));
            return arr;
        }

        public string GetString(long offset) {
            using var memory = new MemoryStream(UnknownBlock);
            using var reader = new BinaryReader(memory);
            memory.Seek(offset - Preamble.Length, SeekOrigin.Begin);
            string str = "";
            while (reader.PeekChar() != 0)
                str += (char)reader.ReadByte();
            return str;
        }

        public string GetUnicodeString(long offset) {
            using var memory = new MemoryStream(UnknownBlock);
            using var reader = new BinaryReader(memory);
            memory.Seek(offset - Preamble.Length, SeekOrigin.Begin);
            string str = "";
            while (true) {
                var ch = (char)reader.ReadUInt16();
                if (ch == 0x0000)
                    break;
                str += ch;
            }
            return str;
        }

        public override byte[] ToArray() {
            using var memory = new MemoryStream();
            using var writer = new BinaryWriter(memory);

            writer.Write(Encoding.ASCII.GetBytes(Preamble));
            writer.Write(UnknownBlock);
            writer.Write(Crc32Algorithm.Compute(memory.ToArray()));

            return memory.ToArray();
        }
    }
}
/*
Should contain info about libraries, functions and types
library:
    - name
    - list of functions
function:
    - name
    - return type
    - number of arguments
    - arguments:
        - position of argument
        - type of argument
        - name of arguemnt
    - calling convention
    - success condition [?]
    - dll name
    - category [eg. deprecated]
type:
    - name
    - underlying type
    - possible values
*/

/*
Categories:	835
Variables:	19678
DLLs:		222
APIs:		15885
COM Interfaces:	1826
COM Methods:	22262

Sum: 607078
*/