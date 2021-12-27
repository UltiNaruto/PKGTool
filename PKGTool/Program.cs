using HashLib.Checksum;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PKGTool
{
    class Program
    {
        struct Argument
        {
            public String cmd;
            public String[] args;
        }

        static void Usage()
        {
            Console.WriteLine("PKG Tool");
            Console.WriteLine("-----------------------");
            Console.WriteLine();
            Console.WriteLine("Extract : PKGTool -x <input path> [-o <output path>] [-i]");
            Console.WriteLine("Create : PKGTool -c <input path> [-o <output path>]");
        }

        static bool ParseArguments(String[] args, out List<Argument> outArgs)
        {
            int i;
            outArgs = new List<Argument>();

            for (i = 0; i < args.Length; i++)
            {
                if (i < args.Length - 1)
                {
                    if (args[i] == "-c")
                    {
                        if (outArgs.Any(a => a.cmd == "-x"))
                        {
                            Console.WriteLine("Cannot compress when extracting a file already!");
                            return false;
                        }

                        if (args[i + 1].StartsWith("-"))
                            return false;

                        outArgs.Add(new Argument()
                        {
                            cmd = args[i],
                            args = new String[] { args[i + 1] }
                        });
                        i++;
                        continue;
                    }

                    if (args[i] == "-x")
                    {
                        if (outArgs.Any(a => a.cmd == "-c"))
                        {
                            Console.WriteLine("Cannot extract when compressing a file already!");
                            return false;
                        }

                        if (args[i + 1].StartsWith("-"))
                            return false;

                        outArgs.Add(new Argument()
                        {
                            cmd = args[i],
                            args = new String[] { args[i + 1] }
                        });
                        i++;
                        continue;
                    }

                    if (args[i] == "-o")
                    {
                        if (!outArgs.Any(a => a.cmd == "-c") && !outArgs.Any(a => a.cmd == "-x"))
                        {
                            Console.WriteLine("Are you compressing or extracting?");
                            return false;
                        }

                        if (args[i + 1].StartsWith("-"))
                            return false;

                        outArgs.Add(new Argument()
                        {
                            cmd = args[i],
                            args = new String[] { args[i + 1] }
                        });
                        i++;
                        continue;
                    }

                    if (args[i] == "-i")
                    {
                        if (outArgs.Any(a => a.cmd == "-x"))
                            return false;

                        if (!args[i+1].StartsWith("-"))
                            return false;

                        outArgs.Add(new Argument()
                        {
                            cmd = args[i],
                            args = new String[0]
                        });
                    }
                }
                else
                {
                    if (args[i] == "-c" ||
                        args[i] == "-x" ||
                        args[i] == "-o")
                    {
                        return false;
                    }

                    if (args[i] == "-i")
                    {
                        if (!outArgs.Any(a => a.cmd == "-x"))
                            return false;

                        outArgs.Add(new Argument()
                        {
                            cmd = args[i],
                            args = new String[0]
                        });
                    }
                }
            }

            if (!outArgs.Any(a => a.cmd == "-c") && !outArgs.Any(a => a.cmd == "-x"))
                return false;
            return true;
        }

        static String GenerateFileName(UInt64 ID, MemoryStream File)
        {
            String fn = String.Empty;
            BinaryReader reader = new BinaryReader(File);
            String magic = Encoding.ASCII.GetString(reader.ReadBytes(4), 0, 4);
            switch(magic)
            {
                case "\x1BLua":
                    fn = $"{ID:X16}.lc";
                    break;
                case "\x6F\x7F\xF3\x73":
                    fn = $"{ID:X16}.bapd";
                    break;
                case "\xFB\x42\x9B\x06":
                    fn = $"{ID:X16}.bmscu";
                    break;
                case "BTXT":
                    fn = $"{ID:X16}.txt";
                    break;
                case "CWAV":
                    fn = $"{ID:X16}.bcwav";
                    break;
                case "FGRP":
                    fn = $"{ID:X16}.bfgrp";
                    break;
                case "FSAR":
                    fn = $"{ID:X16}.bfsar";
                    break;
                case "LSND":
                    fn = $"{ID:X16}.blsnd";
                    break;
                case "LUT\x01":
                    fn = $"{ID:X16}.blut";
                    break;
                case "MANM":
                    fn = $"{ID:X16}.bcskla";
                    break;
                case "MFNT":
                    fn = $"{ID:X16}.bfont";
                    break;
                case "MMDL":
                    fn = $"{ID:X16}.bcmdl";
                    break;
                case "MNAV":
                    fn = $"{ID:X16}.bmnav";
                    break;
                case "MPSI":
                    fn = $"{ID:X16}.bpsi";
                    break;
                case "MPSY":
                    fn = $"{ID:X16}.bcptl";
                    break;
                case "MSAD":
                    fn = $"{ID:X16}.bmsad";
                    break;
                case "MSAS":
                    fn = $"{ID:X16}.bmsas";
                    break;
                case "MSCD":
                    fn = $"{ID:X16}.bmscd";
                    break;
                case "MSHD":
                    fn = $"{ID:X16}.bshdat";
                    break;
                case "MSUR":
                    fn = $"{ID:X16}.bsmat";
                    break;
                case "MTXT":
                    fn = $"{ID:X16}.bctex";
                    break;
                case "MUCT":
                    fn = $"{ID:X16}.buct";
                    break;
                default:
                    fn = $"{ID:X16}.bin";
                    break;
            }
            File.Position = 0;
            return fn;
        }

        static void Main(string[] argv)
        {
            if(!ParseArguments(argv, out List<Argument> arguments))
            {
                Usage();
                return;
            }

            bool isCompressing = arguments.Any(a => a.cmd == "-c");
            bool isExtracting = arguments.Any(a => a.cmd == "-x");
            bool specifyOutputFolder = arguments.Any(a => a.cmd == "-o");
            bool ignoreDataSectionSizeCheck = arguments.Any(a => a.cmd == "-i");
            Dictionary<String, UInt64> AssetIDByFilePath = JObject.Parse(Encoding.UTF8.GetString(Properties.Resources.resource_infos)).ToObject<Dictionary<String, UInt64>>();
            Dictionary<UInt64, String> AssetFilePathByID = AssetIDByFilePath.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
            CRC64 crc = new CRC64();
            FileStream tmp = null;
            String fn = String.Empty;
            String filePath = String.Empty;
            String inPath = String.Empty;
            String outPath = String.Empty;

            try
            {
                if (isCompressing)
                    inPath = arguments.Where(a => a.cmd == "-c").FirstOrDefault().args[0];
                else if (isExtracting)
                    inPath = arguments.Where(a => a.cmd == "-x").FirstOrDefault().args[0];

                if(specifyOutputFolder)
                    outPath = arguments.Where(a => a.cmd == "-o").FirstOrDefault().args[0];
                else
                    outPath = Path.Combine(Directory.GetCurrentDirectory(), Path.GetFileNameWithoutExtension(inPath));

                Dread.FileFormats.PKG pkg = new Dread.FileFormats.PKG();
                if (isExtracting)
                {
                    pkg.IgnoreDataSectionSizeCheck = ignoreDataSectionSizeCheck;

                    if (!File.Exists(inPath))
                        throw new FileNotFoundException($"Couldn't find the file {inPath}");

                    pkg.import(File.OpenRead(inPath));

                    if (!Directory.Exists(outPath))
                        Directory.CreateDirectory(outPath);

                    using(var list = new StreamWriter(Path.Combine(outPath, "files.list")))
                    {
                        foreach (var file in pkg.Files)
                        {
                            if (AssetFilePathByID.ContainsKey(file.Key))
                            {
                                fn = AssetFilePathByID[file.Key];
                                filePath = Path.Combine(outPath, fn.Replace('/', Path.DirectorySeparatorChar));
                                if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                            }
                            else
                                fn = GenerateFileName(file.Key, file.Value);
                            Console.WriteLine($"Extracting {fn}...");
                            tmp = File.Open(Path.Combine(outPath, fn), FileMode.Create, FileAccess.Write);
                            file.Value.CopyTo(tmp);
                            file.Value.Position = 0L;
                            tmp.Close();
                            list.WriteLine(fn);
                        }
                    }

                    pkg.Close();
                }
                else if (isCompressing)
                {
                    if (!Directory.Exists(inPath))
                        throw new FileNotFoundException($"Couldn't find the folder {inPath}");

                    if (!File.Exists(Path.Combine(inPath, "files.list")))
                        throw new FileNotFoundException($"Couldn't find the file files.list");

                    if (Path.GetExtension(outPath) == String.Empty)
                        outPath += ".pkg";
                    else if (Path.GetExtension(outPath).ToLower() != ".pkg")
                        outPath = Path.ChangeExtension(outPath, ".pkg");

                    using (var list = new StreamReader(Path.Combine(inPath, "files.list")))
                    {
                        while (!list.EndOfStream)
                        {
                            fn = list.ReadLine().TrimEnd('\r', '\n');
                            try {
                                if (Path.GetFileNameWithoutExtension(fn).Length == 16)
                                {
                                    pkg.Files.Add(new KeyValuePair<UInt64, MemoryStream>(Convert.ToUInt64(Path.GetFileNameWithoutExtension(fn), 16), new MemoryStream(File.ReadAllBytes(Path.Combine(inPath, fn)))));
                                }
                                else
                                {
                                    pkg.Files.Add(new KeyValuePair<UInt64, MemoryStream>(crc.ComputeAsValue(fn), new MemoryStream(File.ReadAllBytes(Path.Combine(inPath, fn)))));
                                }
                            } catch {
                                pkg.Files.Add(new KeyValuePair<UInt64, MemoryStream>(crc.ComputeAsValue(fn), new MemoryStream(File.ReadAllBytes(Path.Combine(inPath, fn)))));
                            }
                            Console.WriteLine($"Adding {fn}...");
                        }
                    }

                    tmp = File.Open(outPath, FileMode.Create, FileAccess.Write);
                    pkg.export(tmp);
                    tmp.Close();
                    pkg.Close();
                }
            } catch (Exception ex) {
                Console.WriteLine("An error occured!");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine();
                Console.WriteLine();
            }
        }
    }
}
