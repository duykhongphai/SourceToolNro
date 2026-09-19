using Create_Monster.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Create_Monster.Class
{
    public class DataInputStream
    {
        public DataInputStream(sbyte[] data)
        {
            r = new myReader(data);
        }

        public short readShort()
        {
            return r.readShort();
        }

        public int readInt()
        {
            return r.readInt();
        }

        public int read()
        {
            return r.readUnsignedByte();
        }

        public void read(ref sbyte[] data)
        {
            r.read(ref data);
        }

        public void close()
        {
            r.Close();
        }

        public void Close()
        {
            r.Close();
        }

        public string readUTF()
        {
            return r.readUTF();
        }

        public sbyte readByte()
        {
            return r.readByte();
        }

        public long readLong()
        {
            return r.readLong();
        }

        public bool readBoolean()
        {
            return r.readBoolean();
        }

        public int readUnsignedByte()
        {
            return (byte)r.readByte();
        }

        public int readUnsignedShort()
        {
            return r.readUnsignedShort();
        }

        public void readFully(ref sbyte[] data)
        {
            r.read(ref data);
        }

        public int available()
        {
            return r.available();
        }

        internal void read(ref sbyte[] byteData, int p, int size)
        {
            throw new NotImplementedException();
        }

        public myReader r;

        private const int INTERVAL = 5;

        private const int MAXTIME = 500;

    }
}
