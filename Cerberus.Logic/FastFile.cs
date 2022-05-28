using System;
using System.Collections.Generic;
using System.IO;
using PhilLibX.IO;
using System.Security.Cryptography;
using System.IO.Compression;
using BigEndianBinaryReader;

namespace Cerberus.Logic
{
    public static class FastFile
    {
        /// <summary>
        /// Black Ops III Fast File Search Needle
        /// </summary>
        private static readonly byte[] NeedleBo3 = { 0x80, 0x47, 0x53, 0x43, 0x0d, 0x0a, 0x00, 0x03 };

        /// <summary>
        /// Decodes Deflate byte array to Memory Stream
        /// </summary>
        /// <param name="data">Byte Array of Deflate Data</param>
        /// <returns>Decoded Memory Stream</returns>
        public static MemoryStream Decode(byte[] data)
        {
            MemoryStream output = new MemoryStream();
            MemoryStream input = new MemoryStream(data);

            using (DeflateStream deflateStream = new DeflateStream(input, CompressionMode.Decompress))
            {
                deflateStream.CopyTo(output);
            }

            output.Flush();
            output.Position = 0;

            return output;
        }

        public static List<string> Decompress(string filePath, string outputPath)
        {
            var reader = new Reader(filePath);
            var writer = new BinaryWriter(File.Create(outputPath));
            var magic = reader.ReadUInt64();
            var version = reader.ReadUInt32();

            if(magic != 0x5441666630303030 || version != 0x25E)
            {
                throw new Exception("Invalid Fast File Magic.");
            }

            DecompressBO3(reader, writer);
            reader.Close();
            writer.Close();

            return ExtractScriptsBo3(outputPath);
        }

        /// <summary>
        /// Decompresses a Black Ops III Fast File
        /// </summary>
        private static void DecompressBO3(Reader reader, BinaryWriter writer)
        {
            var flags = reader.ReadBytes(4);

            // Validate the flags, we only support ZLIB, PC, and Non-Encrypted FFs
            if (flags[1] != 1)
            {
                throw new Exception("Invalid Fast File Compression. Only ZLIB Fast Files are supported.");
            }
            if (flags[2] != 4)
            {
                throw new Exception("Invalid Fast File Platform. Only PC Fast Files are supported.");
            }
            if (flags[3] != 0)
            {
                throw new Exception("Encrypted Fast Files are not supported");
            }

            reader.SetPosition(0x90);
            var size = reader.ReadInt64();
            reader.SetPosition(0x248);

            var consumed = 0;
            while (consumed < size)
            {
                // Read Block Header
                var compressedSize   = reader.ReadInt32();
                var decompressedSize = reader.ReadInt32();
                var blockSize        = reader.ReadInt32();
                var blockPosition    = reader.ReadInt32();

                // Validate the block position, it should match
                if(blockPosition != reader.GetPosition() - 0x10)
                {
                    throw new Exception("Block Position does not match Stream Position.");
                }

                // Check for padding blocks
                if(decompressedSize == 0)
                {
                    reader.AddToPosition(Utility.ComputePadding((int)reader.GetPosition(), 0x80000));
                    continue;
                }
                writer.Write(Decode(reader.ReadBytes(compressedSize)).ToArray());

                consumed += decompressedSize;

                reader.SetPosition(blockPosition + blockSize + 0x10); //set to next block header
            }
        }

        /// <summary>
        /// Extracts scripts from a Black Ops III Fast File
        /// </summary>
        private static List<string> ExtractScriptsBo3(string FilePath)
        {
            Reader reader = new Reader(FilePath);
        
            var results = new List<string>();
            var offsets = FindBytes(reader, NeedleBo3);
        
            foreach(var offset in offsets)
            {
                reader.SetPosition(offset + 0x28);
                var size = reader.ReadUInt32(); //fixup ptr usually is the size of the gsc file

                reader.SetPosition(offset + 0x34);
                var namePtr = reader.ReadUInt16(); //gsc name ptr

                reader.SetPosition(offset + namePtr);
                var name = reader.ReadNullTerminatedString();

                var outputPath = "ExtractedScripts\\Black Ops III\\" + name + "c";
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

                reader.SetPosition(offset);
                File.WriteAllBytes(outputPath, reader.ReadBytes((int)size));
        
                results.Add(outputPath);
            }

            reader.Close();


            return results;
        }

        /// <summary>
        /// Finds occurences of bytes
        /// </summary>
        /// <param name="br">Reader</param>
        /// <param name="needle">Byte Array Needle to search for</param>
        /// <param name="firstOccurence">Stops at first result</param>
        /// <returns>Resulting offsets</returns>
        private static long[] FindBytes(Reader br, byte[] needle, bool firstOccurence = false)
        {
            // List of offsets in file.
            List<long> offsets = new List<long>();
            long readBegin = br.GetPosition();
            long readSize = br.GetLength() - readBegin;
            int needleIndex = 0;
            long bytesRead = 0;

            // Read chunk of file
            while (bytesRead < readSize)
            {
                // Check if current bytes match
                if (needle[needleIndex] == br.ReadByte())
                {
                    // Increment
                    needleIndex++;
        
                    // Check if we have a match
                    if (needleIndex == needle.Length)
                    {
                        // Add Offset
                        offsets.Add(br.GetPosition() - needle.Length);
        
                        // Reset Index
                        needleIndex = 0;
        
                        // If only first occurence, end search
                        if (firstOccurence)
                            return offsets.ToArray();
                    }
                }
                else
                {
                    // Reset Index
                    needleIndex = 0;
                }

                bytesRead++;
            }
            // Return offsets as an array
            return offsets.ToArray();
        }
    }
}
