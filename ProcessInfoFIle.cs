using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Force.Crc32;

// TODO
namespace APIMonitor {
    // Represents 'process/[Process #]/info' file
    public class ProcessInfoFile : APIMonitorObject {
        public class ThreadInfo : APIMonitorObject {
            public uint ThreadID;
            public uint ThreadNo;
            public uint IsWorker;
            public ulong StartAddress;
            public string Location;
            public ulong Offset;
            public ulong ModuleBase;
            public string Module;
            public DateTime StartTime;

            internal ThreadInfo(BinaryReader reader, long length) : base(reader, length) { }

            protected override void Init() {
                Location = "";
                Module = "";
            }
            
            protected override void Init(BinaryReader reader, long length) {
                base.Init(reader, length);

                ThreadID = reader.ReadUInt32();
                ThreadNo = reader.ReadUInt32();
                IsWorker = reader.ReadUInt32();
                StartAddress = reader.ReadUInt64();
                var strLen = reader.ReadInt32();
                Location = Encoding.ASCII.GetString(reader.ReadBytes(strLen));
                Offset = reader.ReadUInt64();
                ModuleBase = reader.ReadUInt64();
                strLen = reader.ReadInt32();
                Module = Encoding.Unicode.GetString(reader.ReadBytes(strLen * 2));
                StartTime = DateTime.FromFileTime(reader.ReadInt64());
            }

            public override byte[] ToArray() {
                using var mem = new MemoryStream();
                using var writer = new BinaryWriter(mem);

                writer.Write(ThreadID);
                writer.Write(ThreadNo);
                writer.Write(IsWorker);
                writer.Write(StartAddress);
                writer.Write(Location.Length);
                writer.Write(Encoding.ASCII.GetBytes(Location));
                writer.Write(Offset);
                writer.Write(ModuleBase);
                writer.Write(Module.Length);
                writer.Write(Encoding.Unicode.GetBytes(Module));
                writer.Write(StartTime.ToFileTime());

                return mem.ToArray();
            }
        }

        public class ModuleInfo : APIMonitorObject {
            public uint IsLoaded;
            public ulong BaseAddress;
            public string Module;

            internal ModuleInfo(BinaryReader reader, long length) : base(reader, length) { }

            protected override void Init() {
                Module = "";
            }

            protected override void Init(BinaryReader reader, long length) {
                base.Init(reader, length);

                IsLoaded = reader.ReadUInt32();
                BaseAddress = reader.ReadUInt64();
                var strlen = reader.ReadInt32();
                Module = Encoding.Unicode.GetString(reader.ReadBytes(strlen * 2));
            }

            public override byte[] ToArray() {
                using var mem = new MemoryStream();
                using var writer = new BinaryWriter(mem);

                writer.Write(IsLoaded);
                writer.Write(BaseAddress);
                writer.Write(Module.Length);
                writer.Write(Encoding.Unicode.GetBytes(Module));

                return mem.ToArray();
            }
        }

        public uint Unknown1; // always 1 [values other than 1 and 0 hides the process]
        public uint ID;
        public uint ProcessID;
        public ulong BaseAddress;
        public string Filename;
        public string CommandLine;
        public string ProcessName; // nullable
        public uint Unknown3; // always 0 ? [setting all 1's does nothing]
        public DateTime StartTime;
        public DateTime StopTime;
        public List<ThreadInfo> Threads;
        public List<ModuleInfo> Modules;
        public uint Checksum;

        public ProcessInfoFile() { }
        public ProcessInfoFile(Stream stream) : base(stream) { }
        public ProcessInfoFile(ZipArchiveEntry entry) : base(entry) { }

        protected override void Init() {
            Filename = "";
            CommandLine = "";
            ProcessName = "";
            Threads = new List<ThreadInfo>();
            Modules = new List<ModuleInfo>();
        }

        protected override void Init(BinaryReader reader, long length) {
            base.Init(reader, length);

            Unknown1 = reader.ReadUInt32();
            ID = reader.ReadUInt32();
            ProcessID = reader.ReadUInt32();
            BaseAddress = reader.ReadUInt64();
            var strLen = reader.ReadInt32();
            Filename = Encoding.Unicode.GetString(reader.ReadBytes(strLen * 2));
            strLen = reader.ReadInt32();
            CommandLine = Encoding.Unicode.GetString(reader.ReadBytes(strLen * 2));
            strLen = reader.ReadInt32();
            ProcessName = Encoding.Unicode.GetString(reader.ReadBytes(strLen * 2));
            Unknown3 = reader.ReadUInt32();
            StartTime = DateTime.FromFileTime(reader.ReadInt64());
            StopTime = DateTime.FromFileTime(reader.ReadInt64());

            var numberOfThreads = reader.ReadInt32();
            for (var i = 0; i < numberOfThreads; i++)
                Threads.Add(new ThreadInfo(reader, length));

            var numberOfModules = reader.ReadInt32();
            for (var i = 0; i < numberOfModules; i++)
                Modules.Add(new ModuleInfo(reader, length));

            Checksum = reader.ReadUInt32();
        }

        public override byte[] ToArray() {
            using var mem = new MemoryStream();
            using var writer = new BinaryWriter(mem);

            writer.Write(Unknown1);
            writer.Write(ID);
            writer.Write(ProcessID);
            writer.Write(BaseAddress);
            writer.Write(Filename.Length);
            writer.Write(Encoding.Unicode.GetBytes(Filename));
            writer.Write(CommandLine.Length);
            writer.Write(Encoding.Unicode.GetBytes(CommandLine));
            writer.Write(ProcessName.Length);
            writer.Write(Encoding.Unicode.GetBytes(ProcessName));
            writer.Write(Unknown3);
            writer.Write(StartTime.ToFileTime());
            writer.Write(StopTime.ToFileTime());

            writer.Write(Threads.Count);
            foreach (var obj in Threads)
                writer.Write(obj.ToArray());

            writer.Write(Modules.Count);
            foreach (var obj in Modules)
                writer.Write(obj.ToArray());

            writer.Write(Crc32Algorithm.Compute(mem.ToArray()));

            return mem.ToArray();
        }
    }
}