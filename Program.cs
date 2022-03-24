using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml;
using APIMonitor;

/*
FOR X64:
[X] info
[ ] definitions
[X] log/monitoring.txt
[X] process/0/info
[X] process/0/calls
[ ] process/0/data
[ ] process/icons.bmp
*/

namespace APIMonitor {
    class Program {
        static void Main(string[] args) {
            /*if (args.Length != 1 || !File.Exists(args[0])) {
                Console.WriteLine($"Usage: {AppDomain.CurrentDomain.FriendlyName} [API Monitor export file]");
                Environment.Exit(1);
            }*/

            var capture = new CaptureFile(args[0]);
            /*var functions = capture
                .SelectMany(x => x)
                .GroupBy(x => x.FunctionID)
                .ToDictionary(
                    x => x.First(),
                    x => capture.Definitions.GetFunctionInfo(x.First().FunctionID))
                .ToDictionary(
                    x => capture.Definitions.GetString(x.Value.NameOffset),
                    x => x.Value)
                .ToArray();
            var types = functions
                .GroupBy(x => x.Value.ReturnTypeOffset)
                .ToDictionary(
                    x => x.First().Value,
                    x => capture.Definitions.GetTypeInfo(x.Key))
                .ToArray();

            Console.WriteLine(string.Join("\n", functions.Select(x => $"{x.Key}\n{x.Value}")));
            Console.WriteLine(capture[0][89]);*/
            //Console.WriteLine(string.Join("\n", functions.Select(x => capture.Definitions.GetString(x.Value.NameOffset))));

            //Console.WriteLine(string.Join("\n", types));
            //Console.WriteLine(string.Join("\n", types.Select(x => $"{x.Key}\n{capture.Definitions.GetUnicodeString(x.Value.NameOffset)}")));

            /*Console.WriteLine(string.Join("\n", functions
                .GroupBy(x => x.Value.Unknown6)
                .OrderBy(x => x.Key)
                .Select(x => $"{x.Key}: [{x.Count(),5}]{{ {string.Join(" ", x.Take(10).Select(y => $"[{y.Key.ProcessNo},{y.Key.CallID + 1,7}]"))} }}")));*/

            /*Console.WriteLine(string.Join("\n", functions
                .GroupBy(x => x.Value.Unknown4)
                .OrderBy(x => x.Key)
                .Select(x => $"{x.Key:X4}: [{x.Count(),5}] {{ {string.Join(" ", x.Take(10).Select(y => $"[{y.Key.ProcessNo},{y.Key.CallID + 1, 5}]"))} }}")));*/

            //Console.WriteLine($"{capture[0][0].ModuleID:X}");
            //Console.WriteLine(capture.Definitions.GetFunctionInfo(capture[0][0].FunctionID));

            //Console.WriteLine(capture.Definitions.GetFunctionInfo(capture[0][29].FunctionID));
            //File.WriteAllBytes("log/DeviceIoControl.bin", capture.Definitions.GetFunctionInfo((int)capture[0][29].FunctionID));

            /*foreach (var call in capture.SelectMany(x => x))
                call.Unknown1 = 3;

            capture.Save("log/out.apmx64");
            Process.Start(new ProcessStartInfo("cmd") { Arguments = "/c start ./log/out.apmx64", WindowStyle = ProcessWindowStyle.Hidden });*/

            //File.WriteAllBytes("log/409390.bin", capture[0][409390].ToArray());
            //File.WriteAllBytes("log/414549.bin", capture[0][414549].ToArray());

            /*Console.WriteLine(string.Join("\n", capture
                .SelectMany(x => x)
                .GroupBy(x => x.Unknown5)
                .OrderBy(x => x.Key)
                .Select(x => $"{x.Key:X16}: {{ {string.Join(" ", x.Take(20).OrderBy(y => y.CallID).Select(y => $"{y.ProcessNo,2}|{y.CallID + 1,7}"))} }}")));*/
            //Console.WriteLine(capture.SelectMany(x => x).Count(x => x.Unknown5 == 0));
            //Console.WriteLine(capture[0][0]);

            /*var fx = capture.SelectMany(x => x).Where(x => capture.Definitions.GetFunctionName(x) == "CM_Get_Device_Interface_Property_ExW");
            Console.WriteLine(string.Join("\n", fx));
            Console.WriteLine(capture.Definitions.GetFunctionInfo(fx.First()));*/
            Console.WriteLine(capture[0][3]);
            Console.WriteLine(capture[0][12]);
        }
    }
}
