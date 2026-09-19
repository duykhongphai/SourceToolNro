using System.Text;

namespace CreateSkillNro.Class;

public class myWriter
{
    public sbyte[] buffer = new sbyte[2048];

    private int lenght = 2048;

    private int posWrite;

    public myWriter()
    {
    }

    public myWriter(int len)
    {
        buffer = new sbyte[len];
        lenght = len;
    }

    public void writeSByte(sbyte value)
    {
        checkLenght(0);
        var array = buffer;
        var num = posWrite;
        posWrite = num + 1;
        array[num] = value;
    }

    public void writeSByteUncheck(sbyte value)
    {
        var array = buffer;
        var num = posWrite;
        posWrite = num + 1;
        array[num] = value;
    }

    public void writeByte(sbyte value)
    {
        writeSByte(value);
    }

    public void writeByte(int value)
    {
        writeSByte((sbyte)value);
    }

    public void writeChar(char value)
    {
        writeSByte(0);
        writeSByte((sbyte)value);
    }

    public void writeUnsignedByte(byte value)
    {
        writeSByte((sbyte)value);
    }

    public void writeUnsignedByte(byte[] value)
    {
        checkLenght(value.Length);
        for (var i = 0; i < value.Length; i++) writeSByteUncheck((sbyte)value[i]);
    }

    public void writeSByte(sbyte[] value)
    {
        checkLenght(value.Length);
        for (var i = 0; i < value.Length; i++) writeSByteUncheck(value[i]);
    }

    public void writeShort(short value)
    {
        checkLenght(2);
        for (var i = 1; i >= 0; i--) writeSByteUncheck((sbyte)(value >> (i * 8)));
    }

    public void writeShort(int value)
    {
        checkLenght(2);
        var num = (short)value;
        for (var i = 1; i >= 0; i--) writeSByteUncheck((sbyte)(num >> (i * 8)));
    }

    public void writeUnsignedShort(ushort value)
    {
        checkLenght(2);
        for (var i = 1; i >= 0; i--) writeSByteUncheck((sbyte)(value >> (i * 8)));
    }

    public void writeInt(int value)
    {
        checkLenght(4);
        for (var i = 3; i >= 0; i--) writeSByteUncheck((sbyte)(value >> (i * 8)));
    }

    public void writeLong(long value)
    {
        checkLenght(8);
        for (var i = 7; i >= 0; i--) writeSByteUncheck((sbyte)(value >> (i * 8)));
    }

    public void writeBoolean(bool value)
    {
        writeSByte((sbyte)(value ? 1 : 0));
    }

    public void writeBool(bool value)
    {
        writeSByte((sbyte)(value ? 1 : 0));
    }

    public void writeString(string value)
    {
        var array = value.ToCharArray();
        writeShort((short)array.Length);
        checkLenght(array.Length);
        for (var i = 0; i < array.Length; i++) writeSByteUncheck((sbyte)array[i]);
    }

    public void writeUTF(string value)
    {
        var unicode = Encoding.Unicode;
        var encoding = Encoding.GetEncoding(65001);
        var bytes = unicode.GetBytes(value);
        var array = Encoding.Convert(unicode, encoding, bytes);
        writeShort((short)array.Length);
        checkLenght(array.Length);
        foreach (sbyte value2 in array) writeSByteUncheck(value2);
    }

    public void write(ref sbyte[] data, int arg1, int arg2)
    {
        if (data != null)
            for (var i = 0; i < arg2; i++)
            {
                writeSByte(data[i + arg1]);
                var flag2 = posWrite > buffer.Length;
                if (flag2) break;
            }
    }

    public void write(sbyte[] value)
    {
        writeSByte(value);
    }

    public sbyte[] getData()
    {
        if (posWrite > 0)
        {
            var array = new sbyte[posWrite];
            for (var i = 0; i < posWrite; i++) array[i] = buffer[i];
            return array;
        }

        return new sbyte[0];
    }

    public void checkLenght(int ltemp)
    {
        if (posWrite + ltemp > lenght)
        {
            var array = new sbyte[lenght + 1024 + ltemp];
            for (var i = 0; i < lenght; i++) array[i] = buffer[i];
            buffer = new sbyte[0];
            buffer = array;
            lenght += 1024 + ltemp;
        }
    }

    public void Close()
    {
        buffer = new sbyte[0];
    }
}