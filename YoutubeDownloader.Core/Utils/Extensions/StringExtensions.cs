namespace YoutubeDownloader.Core.Utils.Extensions;

public static class StringExtensions
{
    public static string? NullIfEmptyOrWhiteSpace(this string str) =>
        !string.IsNullOrEmpty(str.Trim()) ? str : null;
    /// <summary>
    /// 判断字符串是否为Null、空
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static bool IsNull(this string s)
    {
        return string.IsNullOrWhiteSpace(s);
    }
    /// <summary>
    /// 判断字符串是否不为Null、空
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static bool NotNull(this string s)
    {
    return !string.IsNullOrWhiteSpace(s);
    }
}
