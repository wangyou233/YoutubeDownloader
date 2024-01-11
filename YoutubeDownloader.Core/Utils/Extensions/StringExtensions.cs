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
            /// <summary>
        /// 与字符串进行比较，忽略大小写
        /// </summary>
        /// <param name="s"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool EqualsIgnoreCase(this string s, string value)
        {
            return s.Equals(value, StringComparison.OrdinalIgnoreCase);
        }
        
        /// <summary>
        /// 首字母转小写
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string FirstCharToLower(this string s)
        {
            if(string.IsNullOrEmpty(s))
                return s;

            string str = s.First().ToString().ToLower() + s.Substring(1);
            return str;
        }
        /// <summary>
        /// 首字母转大写
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string FirstCharToUpper(this string s)
        {
            if(string.IsNullOrEmpty(s))
                return s;

            string str = s.First().ToString().ToUpper() + s.Substring(1);
            return str;
        }
          public static string ToPath(this string s)
        {
            if(s.IsNull())
                return string.Empty;

            return s.Replace(@"\", "/");
        }
         public static string UrlEncode(this string str)
        {
            var encoding = UTF8Encoding.UTF8;
            byte[] bytes = encoding.GetBytes(str);
            int IsSafe = 0;
            int NoSafe = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                char ch = (char) bytes[i];
                if (ch == ' ')
                {
                    IsSafe++;
                }
                else if(!IsSafeChar(ch))
                {
                    NoSafe++;
                }
            }
            if (IsSafe == 0 && NoSafe == 0)
            {
                return str;
            }
            byte[] buffer = new byte[bytes.Length + (NoSafe * 2)];
            int num1 = 0;
            for (int j = 0; j < bytes.Length; j++)
            {
                byte num2 = bytes[j];
                char ch2 = (char) num2;
                if (IsSafeChar(ch2))
                {
                    buffer[num1++] = num2;
                }
                else if(ch2 == ' ')
                {
                    buffer[num1++] = 0x2B;
                }
                else
                {
                    buffer[num1++] = 0x25;
                    buffer[num1++] = (byte) IntToHex((num2 >> 4) & 15);
                    buffer[num1++] = (byte) IntToHex(num2 & 15);
                }
            }
            return encoding.GetString(buffer);
        }
         public static T ToEnum<T>(this string name)
        {
            return (T)System.Enum.Parse(typeof(T), name);
        }
         private static bool IsSafeChar(char ch)
        {
            if((((ch <'a')||(ch >'z'))&&((ch <'A')||(ch >'Z')))&&((ch <'0')||(ch >'9')))
            {
                switch(ch)
                {
                    case'-':
                    case'.':
                        break;//
                    case'+':
                    case',':
                        return false;//
                    default://
                        if(ch !='_')
                        {
                            return false;
                        }
                        break;
                }
            }
            return true;
        }
        private static char IntToHex(int n)
        {
            if(n <=9)
            {
                return(char)(n +0x30);
            }
            return(char)((n -10)+0x41);
        }
}
