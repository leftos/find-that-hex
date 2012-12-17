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
                string s1, s2;
                while (true)
                {
                    while ((s1 = Tools.ByteArrayToHexString(br.ReadNonByteAlignedBytes(1)).ToUpperInvariant()) !=
                           (s2 = s.Substring(0,2)))
                    {
                        //Console.WriteLine("Compared {0} to {1} (at {2} +{3})", s1, s2, br.BaseStream.Position - 1, br.InBytePosition);
                        br.MoveStreamPosition(0, -7);
                        if (br.BaseStream.Length - br.BaseStream.Position == 1 && br.InBytePosition > 0)
                        {
                            found = false;
                            break;
                        }
                    }

                    if (!found)
                    {
                        break;
                    }

                    br.BaseStream.Position--;
                    if (Tools.ByteArrayToHexString(br.ReadNonByteAlignedBytes(s.Length/2)).ToUpperInvariant() == s)
                    {
                        Console.WriteLine("Found at {0} +{1}!", (br.BaseStream.Position - (s.Length / 2)), br.InBytePosition);
                    }
                }
            }

            if (!found)
            {
                Console.WriteLine("Hex string not found.");
                Console.ReadKey();
            }
        }
    }
}
