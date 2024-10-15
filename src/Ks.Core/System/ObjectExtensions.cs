using System.Web;

namespace System;

public static class ObjectExtensions
{
    public static string ToQueryString(this object @this)
    {
        var properties = @this.GetType().GetProperties()
            .Where(x => !x.GetValue(@this, null).IsNullOrEmpty())
            .Select(x => $"{x.Name}={HttpUtility.UrlEncode(x.GetValue(@this, null)?.ToString())}");

        var s = string.Join("&", properties);
        return s;
    }

    public static bool IsNullOrEmpty(this object? @this)
    {
        if (@this == null)
        {
            return true;
        }
        
        if (@this is string s)
        {
            return string.IsNullOrEmpty(s);
        }

        return false;
    }
}
