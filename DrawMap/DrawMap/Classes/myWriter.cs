using System;
using System.Text;

namespace DrawMap.Classes;

public class BinaryDataWriter
{
    private sbyte[] _buffer = new sbyte[12048];
    private int _length = 12048;
    private int _posWrite;

    public BinaryDataWriter()
    {
    }

    public BinaryDataWriter(int len)
    {
        _buffer = new sbyte[len];
        _length = len;
    }

    public void writeSByte(sbyte value)
    {
        checkLength(0);
        _buffer[_posWrite++] = value;
    }

    private void writeSByteUncheck(sbyte value)
    {
        _buffer[_posWrite++] = value;
    }

    public void writeByte(sbyte value) => writeSByte(value);

    public void writeByte(int value) => writeSByte((sbyte)value);

    public void writeChar(char value)
    {
        writeSByte(0);
        writeSByte((sbyte)value);
    }

    public void writeUnsignedByte(byte value) => writeSByte((sbyte)value);

    public void writeUnsignedByte(byte[] value)
    {
        checkLength(value.Length);
        for (var i = 0; i < value.Length; i++) writeSByteUncheck((sbyte)value[i]);
    }

    public void writeSByte(sbyte[] value)
    {
        checkLength(value.Length);
        for (var i = 0; i < value.Length; i++) writeSByteUncheck(value[i]);
    }

    public void writeShort(short value)
    {
        checkLength(2);
        for (var i = 1; i >= 0; i--) writeSByteUncheck((sbyte)(value >> (i * 8)));
    }

    public void writeShort(int value)
    {
        checkLength(2);
        var num = (short)value;
        for (var i = 1; i >= 0; i--) writeSByteUncheck((sbyte)(num >> (i * 8)));
    }

    public void writeUnsignedShort(ushort value)
    {
        checkLength(2);
        for (var i = 1; i >= 0; i--) writeSByteUncheck((sbyte)(value >> (i * 8)));
    }

    public void writeInt(int value)
    {
        checkLength(4);
        for (var i = 3; i >= 0; i--) writeSByteUncheck((sbyte)(value >> (i * 8)));
    }

    public void writeLong(long value)
    {
        checkLength(8);
        for (var i = 7; i >= 0; i--) writeSByteUncheck((sbyte)(value >> (i * 8)));
    }

    public void writeBoolean(bool value) => writeSByte((sbyte)(value ? 1 : 0));

    public void writeString(string value)
    {
        var array = value.ToCharArray();
        writeShort((short)array.Length);
        checkLength(array.Length);
        for (var i = 0; i < array.Length; i++) writeSByteUncheck((sbyte)array[i]);
    }

    public void writeUTF(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        writeShort((short)bytes.Length);
        checkLength(bytes.Length);
        foreach (sbyte b in bytes) writeSByteUncheck(b);
    }

    public void write(ref sbyte[] data, int offset, int count)
    {
        if (data == null) return;
        for (var i = 0; i < count; i++)
        {
            writeSByte(data[i + offset]);
            if (_posWrite > _buffer.Length) break;
        }
    }

    public void write(sbyte[] value) => writeSByte(value);

    public sbyte[] getData()
    {
        if (_posWrite <= 0) return Array.Empty<sbyte>();
        var array = new sbyte[_posWrite];
        Buffer.BlockCopy(_buffer, 0, array, 0, _posWrite);
        return array;
    }

    public void checkLength(int extra)
    {
        if (_posWrite + extra > _length)
        {
            var newLength = _length + 1024 + extra;
            var newBuffer = new sbyte[newLength];
            Buffer.BlockCopy(_buffer, 0, newBuffer, 0, _posWrite);
            _buffer = newBuffer;
            _length = newLength;
        }
    }

    public void Close()
    {
        _buffer = null;
    }
}