#region Copyright Notice

//    Copyright 2011-2013 Eleftherios Aslanoglou
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

#endregion

#region Using Directives

using System;
using System.IO;
using System.Linq;
using LeftosCommonLibrary;
using NonByteAlignedBinaryRW;

#endregion

namespace FindThatHex
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Find That Hex");
            Console.WriteLine("\tby Lefteris \"Leftos\" Aslanoglou");
            Console.WriteLine();
            Console.WriteLine("Usage: FindThatHex.exe <path> <string> <start_offset>");
            Console.WriteLine("All parameters are optional, but if any exist, they must be in the order shown.");
            Console.WriteLine();
            MemoryStream fs;
            var cki = new ConsoleKeyInfo();
            if (args.Length == 0)
            {
                do
                {
                    Console.WriteLine("Function: ");
                    Console.WriteLine("1. Find Offset");
                    Console.WriteLine("2. Shift File");
                    Console.WriteLine();
                    cki = Console.ReadKey();
                    Console.WriteLine();
                    Console.WriteLine();
                } while (cki.KeyChar != '1' && cki.KeyChar != '2');
            }
            if (args.Length > 0 || cki.KeyChar == '1')
            {
                string s = String.Empty;
                if (args.Length > 0)
                {
                    try
                    {
                        fs = new MemoryStream(File.ReadAllBytes(args[0]));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Could not open file.");
                        Console.WriteLine(ex);
                        Console.ReadKey();
                        return;
                    }
                }
                else
                {
                    Console.WriteLine("Enter the path to the file to be searched:");
                    string f = Console.ReadLine();
                    f = f.Replace("\n", "").Replace("\"", "");
                    try
                    {
                        fs = new MemoryStream(File.ReadAllBytes(f));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Could not open file.");
                        Console.WriteLine(ex);
                        Console.ReadKey();
                        return;
                    }
                }
                bool found;
                using (var br = new NonByteAlignedBinaryReader(fs))
                {
                    if (args.Length > 1)
                    {
                        s = args[1];
                    }
                    else
                    {
                        Console.Write("Enter the hex string to be found: ");
                        s = Console.ReadLine();
                    }
                    s = s.ToUpperInvariant();
                    char[] ca = s.ToCharArray();
                    string valid = "0123456789ABCDEF";
                    foreach (var c in ca)
                    {
                        if (!valid.Contains(c))
                        {
                            Console.WriteLine("Hex string contains invalid character \"" + c + "\"");
                            Console.ReadKey();
                            return;
                        }
                    }
                    if (args.Length > 2)
                    {
                        try
                        {
                            br.BaseStream.Position = Convert.ToInt32(args[2]);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            Console.ReadKey();
                            return;
                        }
                    }
                    else
                    {
                        Console.Write("Enter the starting offset: ");
                        try
                        {
                            br.BaseStream.Position = Convert.ToInt32(Console.ReadLine());
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            Console.ReadKey();
                            return;
                        }
                    }

                    found = true;
                    byte s1;
                    byte s2 = Convert.ToByte(s.Substring(0, 2), 16);
                    byte[] sba = Tools.HexStringToByteArray(s);
                    while (true)
                    {
                        PrintProgress(br);
                        s1 = br.ReadNonByteAlignedByte();
                        //Console.WriteLine("Compared {0} to {1} (at {2} +{3})", s1, s2, br.BaseStream.Position - 1, br.InBytePosition);
                        while (s1 != s2)
                        {
                            br.MoveStreamPosition(0, -7);
                            if (br.BaseStream.Length - br.BaseStream.Position == 1 && br.InBytePosition > 0)
                            {
                                found = false;
                                break;
                            }
                            PrintProgress(br);
                            s1 = br.ReadNonByteAlignedByte();
                            //Console.WriteLine("Compared {0} to {1} (at {2} +{3})", s1, s2, br.BaseStream.Position - 1, br.InBytePosition);
                        }

                        if (!found)
                        {
                            break;
                        }

                        br.BaseStream.Position--;
                        long distanceFromEnd = br.BaseStream.Length - br.BaseStream.Position;
                        if (distanceFromEnd < s.Length/2 || (distanceFromEnd == s.Length/2 && br.InBytePosition > 0))
                        {
                            found = false;
                            break;
                        }
                        if (br.ReadNonByteAlignedBytes(s.Length/2).SequenceEqual(sba))
                        {
                            Console.WriteLine("Found at {0} +{1}!", (br.BaseStream.Position - (s.Length/2)), br.InBytePosition);
                        }
                        else
                        {
                            //Console.Write("Was at {0} +{1}, ", br.BaseStream.Position, br.InBytePosition);
                            br.MoveStreamPosition(0 - (s.Length/2), 1);
                            //Console.WriteLine("now at {0} +{1}.", br.BaseStream.Position, br.InBytePosition);
                        }
                    }
                }

                if (!found)
                {
                    Console.WriteLine("Hex string not found after last occurrence, if any.");
                    Console.ReadKey();
                }
            }
            else if (cki.KeyChar == '2')
            {
                Console.WriteLine("Enter the path to the file to be re-aligned:");
                string f = Console.ReadLine();
                f = f.Replace("\n", "").Replace("\"", "");
                try
                {
                    fs = new MemoryStream(File.ReadAllBytes(f));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Could not open file.");
                    Console.WriteLine(ex);
                    Console.ReadKey();
                    return;
                }

                using (var br = new NonByteAlignedBinaryReader(fs))
                {
                    Console.Write("Enter the starting offset in bytes: ");
                    try
                    {
                        br.BaseStream.Position = Convert.ToInt32(Console.ReadLine());
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        Console.ReadKey();
                        return;
                    }

                    Console.Write("Enter the bit to start reading from: ");
                    try
                    {
                        br.InBytePosition = Convert.ToInt32(Console.ReadLine());
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        Console.ReadKey();
                        return;
                    }

                    try
                    {
                        BinaryWriter bw;
                        using (bw = new BinaryWriter(new FileStream(f + ".shifted", FileMode.Create)))
                        {
                            while (br.BaseStream.Length - br.BaseStream.Position > 1)
                            {
                                bw.Write(br.ReadNonByteAlignedByte());
                            }
                            var lastByte = br.ReadNonByteAlignedBits(8 - br.InBytePosition).PadRight(8, '0');
                            bw.Write(Convert.ToByte(lastByte, 2));
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        Console.ReadKey();
                        return;
                    }
                }
            }
        }

        private static void PrintProgress(NonByteAlignedBinaryReader br)
        {
            if (br.BaseStream.Position%500000 == 0 && br.InBytePosition == 0)
            {
                Console.WriteLine("..at {0}/{1} ({2}%)...", br.BaseStream.Position, br.BaseStream.Length,
                                  br.BaseStream.Position*100/br.BaseStream.Length);
            }
        }
    }
}