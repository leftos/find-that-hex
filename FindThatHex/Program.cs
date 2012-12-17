using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LeftosCommonLibrary;
using NonByteAlignedBinaryRW;

namespace FindThatHex
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Find That Hex");
            Console.WriteLine("\tby Lefteris \"Leftos\" Aslanoglou");
            Console.WriteLine();
            Console.WriteLine("Usage: FindThatHex.exe <path> <string> <start_offset>");
            Console.WriteLine("All parameters are optional, but if any exist, they must be in the order shown.");
            Console.WriteLine();
            FileStream fs;
            string s = String.Empty;
            if (args.Length > 0)
            {
                try
                {
                    fs = File.OpenRead(args[0]);
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
                    fs = File.OpenRead(f);
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
            using (NonByteAlignedBinaryReader br = new NonByteAlignedBinaryReader(fs))
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
                foreach (char c in ca)
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
                    if (br.BaseStream.Position % 50000 == 0 && br.InBytePosition == 0)
                    {
                        Console.WriteLine("..at " + br.BaseStream.Position + "...");
                    }
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
                        if (br.BaseStream.Position % 50000 == 0 && br.InBytePosition == 0)
                        {
                            Console.WriteLine("..at " + br.BaseStream.Position + "...");
                        }
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
                        Console.WriteLine("Found at {0} +{1}!", (br.BaseStream.Position - (s.Length / 2)), br.InBytePosition);
                    }
                    else
                    {
                        //Console.Write("Was at {0} +{1}, ", br.BaseStream.Position, br.InBytePosition);
                        br.MoveStreamPosition(0 - (s.Length / 2), 1);
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
    }
}
