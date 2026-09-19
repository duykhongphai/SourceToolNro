using System.Runtime.CompilerServices;

namespace DrawMap.Classes;

public static class ByteHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static sbyte[] AsSBytes(byte[] bytes) => Unsafe.As<byte[], sbyte[]>(ref bytes);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] AsBytes(sbyte[] sbytes) => Unsafe.As<sbyte[], byte[]>(ref sbytes);
}
