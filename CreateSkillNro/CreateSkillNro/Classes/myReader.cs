using System;
using System.Text;

namespace CreateSkillNro.Classes;

public class myReader
{
    public sbyte[] buffer;

    private int posMark;

    private int posRead;

    public myReader()
    {
    }

    public myReader(sbyte[] data)
    {
        buffer = data;
    }

    public sbyte readSByte()
    {
        if (posRead < buffer.Length) return buffer[posRead++];
        posRead = buffer.Length;
        throw new Exception(" loi doc sbyte eof ");
    }

    public sbyte readsbyte()
    {
        return readSByte();
    }

    public sbyte readByte()
    {
        return readSByte();
    }

    public void mark(int readlimit)
    {
        posMark = posRead;
    }

    public void reset()
    {
        posRead = posMark;
    }

    public byte readUnsignedByte()
    {
        return convertSbyteToByte(readSByte());
    }

    public short readShort()
    {
        short num = 0;
        for (var i = 0; i < 2; i++)
        {
            num = (short)(num << 8);
            num |= (short)(255 & buffer[posRead++]);
        }

        return num;
    }

    public ushort readUnsignedShort()
    {
        ushort num = 0;
        for (var i = 0; i < 2; i++)
        {
            num = (ushort)(num << 8);
            num |= (ushort)(255 & buffer[posRead++]);
        }

        return num;
    }

    public int readInt()
    {
        var num = 0;
        for (var i = 0; i < 4; i++)
        {
            num <<= 8;
            num |= 255 & buffer[posRead++];
        }

        return num;
    }

    public long readLong()
    {
        var num = 0L;
        for (var i = 0; i < 8; i++)
        {
            num <<= 8;
            num |= 255 & buffer[posRead++];
        }

        return num;
    }

    public bool readBool()
    {
        return readSByte() > 0;
    }

    public bool readBoolean()
    {
        return readSByte() > 0;
    }

    public string readString()
    {
        var num = readShort();
        var array = new byte[num];
        for (var i = 0; i < num; i++) array[i] = convertSbyteToByte(readSByte());
        var utf8Encoding = new UTF8Encoding();
        return utf8Encoding.GetString(array);
    }

    public string readStringUTF()
    {
        var num = readShort();
        var array = new byte[num];
        for (var i = 0; i < num; i++) array[i] = convertSbyteToByte(readSByte());
        var utf8Encoding = new UTF8Encoding();
        return utf8Encoding.GetString(array);
    }

    public string readUTF()
    {
        return readStringUTF();
    }

    public int read()
    {
        if (posRead < buffer.Length) return readSByte();
        return -1;
    }

    public int read(ref sbyte[] data)
    {
        if (data == null) return 0;
        var num = 0;
        for (var i = 0; i < data.Length; i++)
        {
            data[i] = readSByte();
            if (posRead > buffer.Length) return -1;
            num++;
        }

        return num;
    }

    public void readFully(ref sbyte[] data)
    {
        if (data == null || data.Length + posRead > buffer.Length) return;
        for (var i = 0; i < data.Length; i++) data[i] = readSByte();
    }

    public int available()
    {
        return buffer.Length - posRead;
    }

    public static byte convertSbyteToByte(sbyte var)
    {
        if (var > 0) return (byte)var;
        return (byte)(var + 256);
    }

    public static byte[] convertSbyteToByte(sbyte[] var)
    {
        var array = new byte[var.Length];
        for (var i = 0; i < var.Length; i++)
            if (var[i] > 0)
                array[i] = (byte)var[i];
            else
                array[i] = (byte)(var[i] + 256);

        return array;
    }

    public void Close()
    {
        buffer = null;
    }

    public void close()
    {
        buffer = null;
    }

    public void read(ref sbyte[] data, int arg1, int arg2)
    {
        if (data == null) return;
        for (var i = 0; i < arg2; i++)
        {
            data[i + arg1] = readSByte();
            if (posRead > buffer.Length) return;
        }
    }
}