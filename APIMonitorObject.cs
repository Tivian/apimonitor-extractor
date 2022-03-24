using System;
using System.Collections;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace APIMonitor {
    public abstract class APIMonitorObject {
        public APIMonitorObject() { }

        public APIMonitorObject(byte[] raw) : this(raw, 0) { }

        public APIMonitorObject(byte[] raw, long offset) {
            using var memory = new MemoryStream(raw);
            using var reader = new BinaryReader(memory);
            memory.Seek(offset, SeekOrigin.Begin);
            Init(reader, raw.Length);
        }

        public APIMonitorObject(Stream stream) {
            if (stream == null) {
                Init();
            } else {
                using var reader = new BinaryReader(stream);
                Init(reader, stream.Length);
            }
        }

        public APIMonitorObject(ZipArchiveEntry entry) {
            if (entry == null) {
                Init();
            } else {
                using var stream = entry.Open();
                using var reader = new BinaryReader(stream);
                Init(reader, entry.Length);
            }
        }

        public APIMonitorObject(BinaryReader reader, long length) {
            if (reader == null || length <= 0) {
                Init();
            } else {
                Init(reader, length);
            }
        }

        protected virtual void Init() { }
        protected virtual void Init(BinaryReader reader, long length) {
            Init();
        }

        public abstract byte[] ToArray();

        public override string ToString() {
            var sb = new StringBuilder();
            sb.AppendLine($"  {GetType().Name}");
            sb.AppendLine(new string('=', 48));

            var paramMaxLen = 0;
            try {
                paramMaxLen = GetType().GetFields().Select(x => x.Name.Length).Max();
            } catch { }
            var nameFmt = $" {{0,{paramMaxLen}}} = ";

            foreach (var field in GetType().GetFields()) {
                var obj = field.GetValue(this);
                sb.Append(string.Format(nameFmt, field.Name));

                if (obj is Array) {
                    sb.AppendLine($"[{(obj as Array).Length:X}]{{ {string.Join(" ", (obj as Array).OfType<Object>().Select(x => $"{x:X2}"))} }}");
                } else if (obj is IList) {
                    var arr = obj as IList;
                    var subtype = arr.GetType().GetGenericArguments()[0];
                    sb.AppendLine($"IList<{subtype}>[{(obj as IList).Count}]");
                    
                    if (arr.Count > 0 && arr[0] is APIMonitorObject) {
                        foreach (var elem in arr)
                            sb.AppendLine($"{elem}");
                    } else if (subtype.IsNumeric()) {
                        sb.AppendLine($"{{ {string.Join(", ", arr.OfType<dynamic>())} }}");
                    }
                } else if (obj.GetType().IsNumeric()) {
                    sb.AppendLine(string.Format($"{{0:X{Marshal.SizeOf(obj) * 2}}}", obj));
                } else {
                    sb.AppendLine($"{obj}");
                }
            }

            return sb.ToString();
        }
    }
}