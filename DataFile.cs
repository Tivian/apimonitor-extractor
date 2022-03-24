using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

// TODO
namespace APIMonitor {
    // Represents 'process/[Process #]/data' file
    public class DataFile : APIMonitorObject, IEnumerable<DataFile.Call> {
        public class Call : APIMonitorObject {
            public class StackItem : APIMonitorObject {
                public uint HasLocation;
                public uint IsOrdinal;
                public ulong BaseAddress; // name of module is based on this
                public ulong Address;
                public ulong Offset; // -1 if no offset ?
                public ushort IsNamePresent;
                public string Name;
                public uint Ordinal;

                public StackItem(BinaryReader reader, long length) : base(reader, length) { }

                protected override void Init() {
                    Name = "";
                }

                protected override void Init(BinaryReader reader, long length) {
                    base.Init(reader, length);

                    HasLocation = reader.ReadUInt32();
                    IsOrdinal = reader.ReadUInt32();
                    BaseAddress = reader.ReadUInt64();
                    Address = reader.ReadUInt64();
                    Offset = reader.ReadUInt64();

                    if (HasLocation == 1) {
                        if (IsOrdinal == 1) {
                            Ordinal = reader.ReadUInt32();
                        } else {
                            IsNamePresent = reader.ReadUInt16();
                            var strLen = reader.ReadInt16();
                            Name = Encoding.ASCII.GetString(reader.ReadBytes(strLen));
                        }
                    }
                }

                public override byte[] ToArray() {
                    using var memory = new MemoryStream();
                    using var writer = new BinaryWriter(memory);

                    writer.Write(HasLocation);
                    writer.Write(IsOrdinal);
                    writer.Write(BaseAddress);
                    writer.Write(Address);
                    writer.Write(Offset);

                    if (HasLocation == 1) {
                        if (IsOrdinal == 1) {
                            writer.Write(Ordinal);
                        } else {
                            writer.Write(IsNamePresent);
                            writer.Write((short)Name.Length);
                            writer.Write(Encoding.ASCII.GetBytes(Name));
                        }
                    }

                    return memory.ToArray();
                }
            }

            public class Parameter : APIMonitorObject {
                public Parameter(BinaryReader reader, long length) : base(reader, length) { }

                protected override void Init(BinaryReader reader, long length) {
                    base.Init(reader, length);

                }

                public override byte[] ToArray() {
                    using var memory = new MemoryStream();
                    using var writer = new BinaryWriter(memory);



                    return memory.ToArray();
                }
            }

            public byte HasPostValues;
            public byte HasStackCalls;
            public byte HasOverlapped;
            public byte Unknown1; // only 0, 1, 2 or 3 values spotted [possibly an enum ?] [can be 0]
            public uint ProcessNo;
            public int CallID; // +1 to get "CallNo"
            public int NextCall; // other than 0xFFFFFFFF means next call at the same level
            public uint ThreadID;
            public uint ThreadNo;
            public uint Depth;
            public int ParentCall; // 0xFFFFFFFF means no parent
            public uint PreCallBlockSize; // or maybe it is indeed function ID as almost any other data doens't fit requirements
            public uint Unknown2; // probably some kind of ID [can be 0]
            public long FunctionID; // offset to definition file [0xA0 length struct ?]
            public ulong BaseAddress; // used to determine module
            public uint Unknown3; // [can be 0]
            public uint Unknown4; // [can be 0]
            public ulong StackAddress;
            public DateTime Time;
            public uint IsSuccess;
            public uint ErrorCode;
            public uint PostCallBlockSize; // equals PreCallBlockSize if call have return value
            public uint ReturnBlockSize;
            public ulong Unknown5; // Some kind of checksum or ID [can be 0]
            public byte CallStackSize;
            public byte Unknown6; // 0 if value below is not 0 [UNUSED]
            public byte Unknown7; // 0 if value above is not 0 [why?] [UNUSED]
            public byte Unknown8; // always 0 ? [UNUSED]
            public uint ModuleID; // per definition file [used in conjuction with StackAddress for stack calls window]
            public ulong PreCallOffset; // should point to NumberOfArguments parameter
            public ulong PostCallOffset; // can be 0
            public ulong ReturnOffset; // can be 0 if function returns void
            public ulong CallStackOffset; // can be 0 if CallStackSize is 0
            public ulong OverlappedBlockSize;
            public ulong OverlappedOffset;
            public byte[] PreCallBlock;
            public byte[] PostCallBlock;
            public byte[] ReturnBlock;
            public List<StackItem> CallStack;
            public byte[] OverlappedBlock; // always 48 bytes + data
            private bool OverlappedBug = false;
            /* There should be:
                - Category
                - COM API [bool]
                - Unicode API [bool]
                - External API [bool]
                - 
            */

            internal Call(byte[] raw) {
                using var memory = new MemoryStream(raw);
                using var reader = new BinaryReader(memory);
                Init(reader, raw.Length);
            }

            public Call(BinaryReader reader, long length) : base(reader, length) { }

            protected override void Init() {
                PreCallBlock = new byte[0];
                PostCallBlock = new byte[0];
                ReturnBlock = new byte[0];
                OverlappedBlock = new byte[0];
                CallStack = new List<StackItem>();
            }

            protected override void Init(BinaryReader reader, long length) {
                base.Init(reader, length);

                HasPostValues = reader.ReadByte();
                HasStackCalls = reader.ReadByte();
                HasOverlapped = reader.ReadByte();
                Unknown1 = reader.ReadByte();
                ProcessNo = reader.ReadUInt32();
                CallID = reader.ReadInt32();
                NextCall = reader.ReadInt32();
                ThreadID = reader.ReadUInt32();
                ThreadNo = reader.ReadUInt32();
                Depth = reader.ReadUInt32();
                ParentCall = reader.ReadInt32();
                PreCallBlockSize = reader.ReadUInt32();
                Unknown2 = reader.ReadUInt32();
                FunctionID = reader.ReadInt64();
                BaseAddress = reader.ReadUInt64();
                Unknown3 = reader.ReadUInt32();
                Unknown4 = reader.ReadUInt32();
                StackAddress = reader.ReadUInt64();
                Time = DateTime.FromFileTime(reader.ReadInt64());
                IsSuccess = reader.ReadUInt32();
                ErrorCode = reader.ReadUInt32();
                PostCallBlockSize = reader.ReadUInt32();
                ReturnBlockSize = reader.ReadUInt32();
                Unknown5 = reader.ReadUInt64();
                CallStackSize = reader.ReadByte();
                Unknown6 = reader.ReadByte();
                Unknown7 = reader.ReadByte();
                Unknown8 = reader.ReadByte();

                ModuleID = reader.ReadUInt32();
                PreCallOffset = reader.ReadUInt64();
                PostCallOffset = reader.ReadUInt64();
                ReturnOffset = reader.ReadUInt64();
                CallStackOffset = reader.ReadUInt64();

                if (HasOverlapped == 1) {
                    OverlappedBlockSize = reader.ReadUInt64();
                    OverlappedOffset = reader.ReadUInt64();
                }

                // TODO: fix this by properly parsing 'definitions' file
                var numberOfArguments = reader.ReadByte();
                if (numberOfArguments == 0) { // how the fuck API Monitor knows when overlapped function calls occured?
                    reader.BaseStream.Seek(-1, SeekOrigin.Current);
                    var first = reader.ReadInt64();
                    var second = reader.ReadInt64();
                    if (first == 0 && second == 0) { // overlapped function without result
                        OverlappedBug = true;
                        numberOfArguments = reader.ReadByte();
                    } else { // actual function without any arguments
                        reader.BaseStream.Seek(-15, SeekOrigin.Current);
                    }
                }
                reader.BaseStream.Seek(-1, SeekOrigin.Current);

                // TODO: replace usage of offset to BlockSize parameters
                var paramsBlockSize = (int)((CallStackOffset != 0) ?
                    (CallStackOffset - PreCallOffset) : (ulong)(length - reader.BaseStream.Position));

                if (PreCallOffset != 0) {
                    int size = paramsBlockSize;
                    if (PostCallOffset != 0)
                        size = (int)(PostCallOffset - PreCallOffset);
                    else if (ReturnOffset != 0)
                        size = (int)(ReturnOffset - PreCallOffset);
                    PreCallBlock = reader.ReadBytes(size);
                }

                if (PostCallOffset != 0) {
                    int size = paramsBlockSize - PreCallBlock.Length;
                    if (ReturnOffset != 0)
                        size = (int)(ReturnOffset - PostCallOffset);
                    PostCallBlock = reader.ReadBytes(size);
                }

                if (ReturnOffset != 0) {
                    var offset = PreCallBlock.Length + PostCallBlock.Length;
                    int size = paramsBlockSize - offset;
                    ReturnBlock = reader.ReadBytes(size);
                }

                //for (var i = 0; i < NumberOfArguments; i++)
                    //Parameters.Add(new Parameter(reader, length));

                for (var i = 0; i < CallStackSize; i++)
                    CallStack.Add(new StackItem(reader, length));

                if (HasOverlapped == 1)
                    OverlappedBlock = reader.ReadBytes((int)(length - reader.BaseStream.Position));

                if (reader.BaseStream.Position != length)
                    throw new IOException($"Unknown {length - reader.BaseStream.Position} bytes at #[{ProcessNo}|{CallID + 1}]");
                //UnknownBlock = reader.ReadBytes((int)(length - reader.BaseStream.Position));
            }

            public override byte[] ToArray() {
                using var memory = new MemoryStream();
                using var writer = new BinaryWriter(memory);

                writer.Write(HasPostValues);
                writer.Write(HasStackCalls);
                writer.Write(HasOverlapped);
                writer.Write(Unknown1);
                writer.Write(ProcessNo);
                writer.Write(CallID);
                writer.Write(NextCall);
                writer.Write(ThreadID);
                writer.Write(ThreadNo);
                writer.Write(Depth);
                writer.Write(ParentCall);
                writer.Write(PreCallBlockSize);
                writer.Write(Unknown2);
                writer.Write(FunctionID);
                writer.Write(BaseAddress);
                writer.Write(Unknown3);
                writer.Write(Unknown4);
                writer.Write(StackAddress);
                writer.Write(Time.ToFileTime());
                writer.Write(IsSuccess);
                writer.Write(ErrorCode);
                writer.Write(PostCallBlockSize);
                writer.Write(ReturnBlockSize);
                writer.Write(Unknown5);
                writer.Write(CallStackSize);
                writer.Write(Unknown6);
                writer.Write(Unknown7);
                writer.Write(Unknown8);

                writer.Write(ModuleID);
                writer.Write(PreCallOffset);
                writer.Write(PostCallOffset);
                writer.Write(ReturnOffset);
                writer.Write(CallStackOffset);

                if (HasOverlapped == 1 || OverlappedBug) {
                    writer.Write(OverlappedBlockSize);
                    writer.Write(OverlappedOffset);
                }

                //writer.Write(NumberOfArguments);
                //writer.Write(ParametersBlock);
                writer.Write(PreCallBlock);
                writer.Write(PostCallBlock);
                writer.Write(ReturnBlock);

                //foreach (var item in Parameters)
                    //writer.Write(item.ToArray());

                foreach (var item in CallStack)
                    writer.Write(item.ToArray());

                if (HasOverlapped == 1)
                    writer.Write(OverlappedBlock);

                //writer.Write(UnknownBlock);

                return memory.ToArray();
            }
        }

        public List<Call> Calls { get; private set; }

        public int Count => Calls.Count;

        public Call this[int i]
            => (i < 0 || i >= Calls.Count) ? null : Calls[i];

        public DataFile() { }
        public DataFile(Stream stream) : base(stream) { }
        public DataFile(ZipArchiveEntry entry, IList<ulong> offsets) : base(entry) {
            Init();

            if (entry == null || offsets == null)
                return;

            using var stream = entry.Open();
            using var reader = new BinaryReader(stream);

            var list = new List<ulong>(offsets);
            list.Add((ulong)entry.Length);
            for (var i = 1; i < list.Count; i++) {
                var length = list[i] - list[i - 1];
                var raw = reader.ReadBytes((int)length);
                Calls.Add(new Call(raw));
            }
        }

        protected override void Init() {
            Calls = new List<Call>();
        }

        public override byte[] ToArray() {
            using var mem = new MemoryStream();
            using var writer = new BinaryWriter(mem);
            
            foreach (var call in Calls)
                writer.Write(call.ToArray());

            return mem.ToArray();
        }

        public IEnumerator<DataFile.Call> GetEnumerator()
            => Calls.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}