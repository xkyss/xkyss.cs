using System.Diagnostics.CodeAnalysis;

namespace Ks.Core;

public static partial class Check
{
    public static Int16 Positive(
        Int16 value,
        [NotNull] string parameterName)
    {
        if (value == 0)
        {
            throw new ArgumentException($"{parameterName} is equal to zero");
        }
        else if (value < 0)
        {
            throw new ArgumentException($"{parameterName} is less than zero");
        }
        return value;
    }

    public static Int32 Positive(
        Int32 value,
        [NotNull] string parameterName)
    {
        if (value == 0)
        {
            throw new ArgumentException($"{parameterName} is equal to zero");
        }
        else if (value < 0)
        {
            throw new ArgumentException($"{parameterName} is less than zero");
        }
        return value;
    }

    public static Int64 Positive(
        Int64 value,
        [NotNull] string parameterName)
    {
        if (value == 0)
        {
            throw new ArgumentException($"{parameterName} is equal to zero");
        }
        else if (value < 0)
        {
            throw new ArgumentException($"{parameterName} is less than zero");
        }
        return value;
    }

    public static float Positive(
        float value,
        [NotNull] string parameterName)
    {
        if (value == 0)
        {
            throw new ArgumentException($"{parameterName} is equal to zero");
        }
        else if (value < 0)
        {
            throw new ArgumentException($"{parameterName} is less than zero");
        }
        return value;
    }

    public static double Positive(
        double value,
        [NotNull] string parameterName)
    {
        if (value == 0)
        {
            throw new ArgumentException($"{parameterName} is equal to zero");
        }
        else if (value < 0)
        {
            throw new ArgumentException($"{parameterName} is less than zero");
        }
        return value;
    }

    public static decimal Positive(
        decimal value,
        [NotNull] string parameterName)
    {
        if (value == 0)
        {
            throw new ArgumentException($"{parameterName} is equal to zero");
        }
        else if (value < 0)
        {
            throw new ArgumentException($"{parameterName} is less than zero");
        }
        return value;
    }


    public static Int16 Range(
        Int16 value,
        [NotNull] string parameterName,
        Int16 minimumValue,
        Int16 maximumValue = Int16.MaxValue)
    {
        if (value < minimumValue || value > maximumValue)
        {
            throw new ArgumentException($"{parameterName} is out of range min: {minimumValue} - max: {maximumValue}");
        }

        return value;
    }
    public static Int32 Range(
        Int32 value,
        [NotNull] string parameterName,
        Int32 minimumValue,
        Int32 maximumValue = Int32.MaxValue)
    {
        if (value < minimumValue || value > maximumValue)
        {
            throw new ArgumentException($"{parameterName} is out of range min: {minimumValue} - max: {maximumValue}");
        }

        return value;
    }

    public static Int64 Range(
        Int64 value,
        [NotNull] string parameterName,
        Int64 minimumValue,
        Int64 maximumValue = Int64.MaxValue)
    {
        if (value < minimumValue || value > maximumValue)
        {
            throw new ArgumentException($"{parameterName} is out of range min: {minimumValue} - max: {maximumValue}");
        }

        return value;
    }


    public static float Range(
        float value,
        [NotNull] string parameterName,
        float minimumValue,
        float maximumValue = float.MaxValue)
    {
        if (value < minimumValue || value > maximumValue)
        {
            throw new ArgumentException($"{parameterName} is out of range min: {minimumValue} - max: {maximumValue}");
        }
        return value;
    }


    public static double Range(
        double value,
        [NotNull] string parameterName,
        double minimumValue,
        double maximumValue = double.MaxValue)
    {
        if (value < minimumValue || value > maximumValue)
        {
            throw new ArgumentException($"{parameterName} is out of range min: {minimumValue} - max: {maximumValue}");
        }

        return value;
    }


    public static decimal Range(
        decimal value,
        [NotNull] string parameterName,
        decimal minimumValue,
        decimal maximumValue = decimal.MaxValue)
    {
        if (value < minimumValue || value > maximumValue)
        {
            throw new ArgumentException($"{parameterName} is out of range min: {minimumValue} - max: {maximumValue}");
        }

        return value;
    }
}
