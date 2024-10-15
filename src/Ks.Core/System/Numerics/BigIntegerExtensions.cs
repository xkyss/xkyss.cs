namespace System.Numerics;

public static class BigIntegerExtensions
{
    public static int ToInt(this BigInteger @this)
    {
        return (int)@this;
    }
}
