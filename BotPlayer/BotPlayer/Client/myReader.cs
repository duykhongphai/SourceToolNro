using System;
using System.Text;

namespace BotPlayer.Client;

public class myReader
{
    private sbyte[] _buffer;

    private int _posMark;

    private int _posRead;

    public myReader(sbyte[] data)
    {
        _buffer = data;
    }

    public sbyte ReadSByte()
    {
        if (_posRead < _buffer.Length) return _buffer[_posRead++];
        _posRead = _buffer.Length;
        throw new Exception(" loi doc sbyte eof ");
    }

    public sbyte ReadByte()
    {
        return ReadSByte();
    }

    public void Mark(int readlimit)
    {
        _posMark = _posRead;
    }

    public void Reset()
    {
        _posRead = _posMark;
    }

    public byte ReadUnsignedByte()
    {
        return ConvertSbyteToByte(ReadSByte());
    }

    public short ReadShort()
    {
        short num = 0;
        for (var i = 0; i < 2; i++)
        {
            num <<= 8;
            num |= (short)(0xFF & _buffer[_posRead++]);
        }

        return num;
    }

    public ushort ReadUnsignedShort()
    {
        ushort num = 0;
        for (var i = 0; i < 2; i++)
        {
            num <<= 8;
            num |= (ushort)(0xFFu & (uint)_buffer[_posRead++]);
        }

        return num;
    }

    public int ReadInt()
    {
        var num = 0;
        for (var i = 0; i < 4; i++)
        {
            num <<= 8;
            num |= 0xFF & _buffer[_posRead++];
        }

        return num;
    }

    public long ReadLong()
    {
        var num = 0L;
        for (var i = 0; i < 8; i++)
        {
            num <<= 8;
            num |= 0xFF & _buffer[_posRead++];
        }

        return num;
    }

    public bool ReadBool()
    {
        return ReadSByte() > 0;
    }

    private string readStringUTF()
    {
        var num = ReadShort();
        var array = new byte[num];
        for (var i = 0; i < num; i++) array[i] = ConvertSbyteToByte(ReadSByte());
        var uTF8Encoding = new UTF8Encoding();
        return uTF8Encoding.GetString(array);
    }

    public string ReadUTF()
    {
        return readStringUTF();
    }

    public int Read()
    {
        if (_posRead < _buffer.Length) return ReadSByte();
        return -1;
    }

    public int Read(ref sbyte[] data)
    {
        if (data == null) return 0;
        var num = 0;
        for (var i = 0; i < data.Length; i++)
        {
            data[i] = ReadSByte();
            if (_posRead > _buffer.Length) return -1;
            num++;
        }

        return num;
    }

    public void ReadFully(ref sbyte[] data)
    {
        if (data != null && data.Length + _posRead <= _buffer.Length)
            for (var i = 0; i < data.Length; i++)
                data[i] = ReadSByte();
    }

    public int Available()
    {
        return _buffer.Length - _posRead;
    }

    private static byte ConvertSbyteToByte(sbyte var)
    {
        if (var > 0) return (byte)var;
        return (byte)(var + 256);
    }

    public static byte[] ConvertSbyteToByte(sbyte[] var)
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
        _buffer = null;
    }

    public void Read(ref sbyte[] data, int arg1, int arg2)
    {
        if (data == null) return;
        for (var i = 0; i < arg2; i++)
        {
            data[i + arg1] = ReadSByte();
            if (_posRead > _buffer.Length) break;
        }
    }
}