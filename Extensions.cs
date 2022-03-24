using System;
using System.IO;
using System.IO.Compression;
using SixLabors.ImageSharp;

namespace APIMonitor {
    public static class Extensions {
        public static bool IsNumeric(this Type type)
            => type == typeof(decimal)
            || type == typeof(int) 
            || type == typeof(uint)
            || type == typeof(long)
            || type == typeof(ulong)
            || type == typeof(short)
            || type == typeof(ushort)
            || type == typeof(byte)
            || type == typeof(sbyte)
            || type == typeof(float)
            || type == typeof(double);
        
        public static void AddEntry(this ZipArchive zip, string name, byte[] raw) {
            using var memory = new MemoryStream(raw);
            using var stream = zip.CreateEntry(name, CompressionLevel.Optimal).Open();
            memory.CopyTo(stream);
        }

        public static byte[] ToArray(this ZipArchiveEntry entry) {
            using var stream = entry.Open();
            return stream.ToArray();
        }

        public static byte[] ToArray(this Image img) {
            using var memory = new MemoryStream();
            img.SaveAsBmp(memory);
            return memory.ToArray();
        }

        public static byte[] ToArray(this Stream stream) {
            using var memory = new MemoryStream();
            stream.CopyTo(memory);
            return memory.ToArray();
        }
    }
}