using System;
using System.Text;

namespace DrawMap.Classes;

public class BinaryDataReader
{
    private sbyte[] _buffer;
    private int _posMark;
    private int _posRead;

    public BinaryDataReader()
    {
    }

    public BinaryDataReader(sbyte[] data)
    {
        _buffer = data;
    }

    public sbyte readByte()
    {
        if (_posRead < _buffer.Length) return _buffer[_posRead++];
        _posRead = _buffer.Length;
        throw new Exception("EOF");
    }

    public void mark(int readlimit)
    {
        _posMark = _posRead;
    }

    public void reset()
    {
        _posRead = _posMark;
    }

    public byte readUnsignedByte()
    {
        var b = readByte();
        return b > 0 ? (byte)b : (byte)(b + 256);
    }

    public short readShort()
    {
        short num = 0;
        for (var i = 0; i < 2; i++)
        {
            num = (short)(num << 8);
            num |= (short)(255 & _buffer[_posRead++]);
        }
        return num;
    }

    public ushort readUnsignedShort()
    {
        ushort num = 0;
        for (var i = 0; i < 2; i++)
        {
            num = (ushort)(num << 8);
            num |= (ushort)(255 & _buffer[_posRead++]);
        }
        return num;
    }

    public int readInt()
    {
        var num = 0;
        for (var i = 0; i < 4; i++)
        {
            num <<= 8;
            num |= 255 & _buffer[_posRead++];
        }
        return num;
    }

    public long readLong()
    {
        var num = 0L;
        for (var i = 0; i < 8; i++)
        {
            num <<= 8;
            num |= 255 & _buffer[_posRead++];
        }
        return num;
    }

    public bool readBool() => readByte() > 0;

    public string readUTF()
    {
        var num = readShort();
        var array = new byte[num];
        for (var i = 0; i < num; i++)
        {
            var b = readByte();
            array[i] = b > 0 ? (byte)b : (byte)(b + 256);
        }
        return Encoding.UTF8.GetString(array);
    }

    public int read()
    {
        if (_posRead < _buffer.Length) return readByte();
        return -1;
    }

    public int read(ref sbyte[] data)
    {
        if (data == null) return 0;
        var num = 0;
        for (var i = 0; i < data.Length; i++)
        {
            data[i] = readByte();
            if (_posRead > _buffer.Length) return -1;
            num++;
        }
        return num;
    }

    public void readFully(ref sbyte[] data)
    {
        if (data == null || data.Length + _posRead > _buffer.Length) return;
        for (var i = 0; i < data.Length; i++) data[i] = readByte();
    }

    public int available() => _buffer.Length - _posRead;

    public void Close()
    {
        _buffer = null;
    }

    public void read(ref sbyte[] data, int offset, int count)
    {
        if (data == null) return;
        for (var i = 0; i < count; i++)
        {
            data[i + offset] = readByte();
            if (_posRead > _buffer.Length) return;
        }
    }
}