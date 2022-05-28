using System;
using System.Text;
using System.IO;

namespace BigEndianBinaryReader
{
    public class Reader
    {
        public BinaryReader FileReader;

        public Reader(string file)
        {
            FileReader = new BinaryReader(File.OpenRead(file));
        }

        public void Close()
        {
            FileReader.Close();
        }
        public int PeekChar()
        {
            return FileReader.PeekChar();
        }

        public bool ReadBoolean()
        {
            return FileReader.ReadBoolean();
        }

        public byte ReadByte()
        {
            return FileReader.ReadByte();
        }

        public byte[] ReadBytes(int count)
        {
            return FileReader.ReadBytes(count);
        }

        public short ReadInt16()
        {
            var data = FileReader.ReadBytes(2);
            Array.Reverse(data);
            return BitConverter.ToInt16(data, 0);
        }

        public ushort ReadUInt16()
        {
            var data = FileReader.ReadBytes(2);
            Array.Reverse(data);
            return BitConverter.ToUInt16(data, 0);
        }

        public int ReadInt32()
        {
            var data = FileReader.ReadBytes(4);
            Array.Reverse(data);
            return BitConverter.ToInt32(data, 0);
        }
        public uint ReadUInt32()
        {
            var data = FileReader.ReadBytes(4);
            Array.Reverse(data);
            return BitConverter.ToUInt32(data, 0);
        }

        public long ReadInt64()
        {
            var data = FileReader.ReadBytes(8);
            Array.Reverse(data);
            return BitConverter.ToInt64(data, 0);
        }

        public ulong ReadUInt64()
        {
            var data = FileReader.ReadBytes(8);
            Array.Reverse(data);
            return BitConverter.ToUInt64(data, 0);
        }
        public float ReadFloat()
        {
            var data = FileReader.ReadBytes(4);
            Array.Reverse(data);
            return BitConverter.ToSingle(data, 0);
        }

        public double ReadDouble()
        {
            var data = FileReader.ReadBytes(8);
            Array.Reverse(data);
            return BitConverter.ToDouble(data, 0);
        }

        public string ReadNullTerminatedString(int MaxSize = -1)
        {
            StringBuilder Str = new StringBuilder();
            int Size = 0;
            int Char;

            while ((Char = FileReader.ReadByte()) != 0x0 && Size != MaxSize)
            {
                Str.Append(Convert.ToChar(Char));
                Size++;
            }
            
            return Str.ToString();
        }

        public long GetPosition()
        {
            return FileReader.BaseStream.Position;
        }

        public void SetPosition(long Pos)
        {
            FileReader.BaseStream.Position = Pos;
        }

        public void AddToPosition(long Len)
        {
            FileReader.BaseStream.Position += Len;
        }

        public long GetLength()
        {
            return FileReader.BaseStream.Length;
        }
		
		public Stream GetBaseStream()
        {
            return FileReader.BaseStream;
        }
    }
}
