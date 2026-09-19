using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Create_Monster.Class
{
    public class DataOutputStream
    {
        public DataOutputStream(int len)
        {
            w = new myWriter(len);
        }

        public DataOutputStream()
        {
        }

        public void writeShort(short i)
        {
            w.writeShort(i);
        }

        public void writeInt(int i)
        {
            w.writeInt(i);
        }

        public void write(sbyte[] data)
        {
            w.writeSByte(data);
        }

        public sbyte[] toByteArray()
        {
            return w.getData();
        }

        public void close()
        {
            w.Close();
        }

        public void writeByte(sbyte b)
        {
            w.writeByte(b);
        }

        public void writeUTF(string name)
        {
            w.writeUTF(name);
        }

        public void writeBoolean(bool b)
        {
            w.writeBoolean(b);
        }

        private myWriter w = new myWriter();
    }
}
