using System.Text;

namespace BotPlayer.Client;

public class myWriter
{
    private sbyte[] _buffer = new sbyte[2048];

    private int _lenght = 2048;

    private int _posWrite;

    public myWriter()
    {
    }

    public myWriter(int len)
    {
        _buffer = new sbyte[len];
        _lenght = len;
    }

    public void WriteByte(sbyte value)
    {
        WriteSByte(value);
    }

    public void WriteByte(int value)
    {
        WriteSByte((sbyte)value);
    }

    public void WriteSByte(sbyte value)
    {
        CheckLenght(0);
        _buffer[_posWrite++] = value;
    }

    public void WriteSByteUncheck(sbyte value)
    {
        _buffer[_posWrite++] = value;
    }

    public void WriteSByte(sbyte[] value)
    {
        CheckLenght(value.Length);
        for (var i = 0; i < value.Length; i++) WriteSByteUncheck(value[i]);
    }

    public void WriteShort(short value)
    {
        CheckLenght(2);
        for (var num = 1; num >= 0; num--) WriteSByteUncheck((sbyte)(value >> (num * 8)));
    }

    public void WriteInt(int value)
    {
        CheckLenght(4);
        for (var num = 3; num >= 0; num--) WriteSByteUncheck((sbyte)(value >> (num * 8)));
    }

    public void WriteLong(long value)
    {
        CheckLenght(8);
        for (var num = 7; num >= 0; num--) WriteSByteUncheck((sbyte)(value >> (num * 8)));
    }

    public void WriteBoolean(bool value)
    {
        WriteSByte((sbyte)(value ? 1 : 0));
    }

    public void WriteUTF(string value)
    {
        var unicode = Encoding.Unicode;
        var encoding = Encoding.GetEncoding(65001);
        var bytes = unicode.GetBytes(value);
        var array = Encoding.Convert(unicode, encoding, bytes);
        WriteShort((short)array.Length);
        CheckLenght(array.Length);
        for (var i = 0; i < array.Length; i++)
        {
            var value2 = (sbyte)array[i];
            WriteSByteUncheck(value2);
        }
    }

    public void Write(ref sbyte[] data, int arg1, int arg2)
    {
        if (data == null) return;
        for (var i = 0; i < arg2; i++)
        {
            WriteSByte(data[i + arg1]);
            if (_posWrite > _buffer.Length) break;
        }
    }

    public void Write(sbyte[] value)
    {
        WriteSByte(value);
    }

    public sbyte[] GetData()
    {
        if (_posWrite <= 0) return null;
        var array = new sbyte[_posWrite];
        for (var i = 0; i < _posWrite; i++) array[i] = _buffer[i];
        return array;
    }

    public void CheckLenght(int ltemp)
    {
        if (_posWrite + ltemp > _lenght)
        {
            var array = new sbyte[_lenght + 1024 + ltemp];
            for (var i = 0; i < _lenght; i++) array[i] = _buffer[i];
            _buffer = null;
            _buffer = array;
            _lenght += 1024 + ltemp;
        }
    }

    public void Close()
    {
        _buffer = null;
    }
}