using System;
using System.Globalization;

namespace GameScripts
{
    public static class Converter
    {
        public static readonly char[] FirstSeparator = ",".ToCharArray();
        public static readonly char[] SecondSeparator = ";".ToCharArray();
        public static readonly char[] ThirdSeparator = "#".ToCharArray();
        public static readonly char[] FourthSeparator = "|".ToCharArray();
        public static readonly char[] FifthSeparator = ":".ToCharArray();

        // 初始化为 “不依赖任何地区的文化信息” 的标准文化（Invariant Culture）
        public static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

        //测试印度 public static readonly CultureInfo Invariant = new CultureInfo("en-IN");
        //测试德国 public static readonly CultureInfo Invariant = new CultureInfo("de-DE");

        // ------------------------------
        // 1️⃣ 转整数 int
        // ------------------------------
        public static int ToInt(string str, int defaultValue = 0)
        {
            if (TryToInt(str, out int value)) return value;
            return defaultValue;
        }

        public static bool TryToInt(string str, out int value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(str)) return false;
            str = CleanNumberString(str, true);

            // 整数解析偶尔可能会因为截断后超出 int 范围而失败
            return int.TryParse(str, NumberStyles.Integer, Invariant, out value);
        }

        public static int ToInt(object obj, int defaultValue = 0)
        {
            if (obj == null) return defaultValue;

            switch (obj)
            {
                case int i: return i;
                case long l: return (int)l;
                case float f: return (int)f;
                case double d: return (int)d;
                case decimal m: return (int)m;
                case bool b: return b ? 1 : 0;
                case Enum e: return Convert.ToInt32(e);
                case string s: return ToInt(s, defaultValue);
                default:
                    try
                    {
                        return Convert.ToInt32(obj, Invariant);
                    }
                    catch
                    {
                        return defaultValue;
                    }
            }
        }

        // ------------------------------
        // 2️⃣ 转长整数 long
        // ------------------------------
        public static long ToLong(string str, long defaultValue = 0)
        {
            if (TryToLong(str, out long value)) return value;
            return defaultValue;
        }

        public static bool TryToLong(string str, out long value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(str)) return false;
            str = CleanNumberString(str, true);
            return long.TryParse(str, NumberStyles.Integer, Invariant, out value);
        }

        public static long ToLong(object obj, long defaultValue = 0)
        {
            if (obj == null) return defaultValue;

            switch (obj)
            {
                case long l: return l;
                case int i: return i;
                case float f: return (long)f;
                case double d: return (long)d;
                case decimal m: return (long)m;
                case bool b: return b ? 1L : 0L;
                case Enum e: return Convert.ToInt64(e);
                case string s: return ToLong(s, defaultValue);
                default:
                    try
                    {
                        return Convert.ToInt64(obj, Invariant);
                    }
                    catch
                    {
                        return defaultValue;
                    }
            }
        }

        // ------------------------------
        // 3️⃣ 转浮点 float
        // ------------------------------
        public static float ToFloat(string str, float defaultValue = 0f)
        {
            if (TryToFloat(str, out float value)) return value;
            return defaultValue;
        }

        public static bool TryToFloat(string str, out float value)
        {
            value = 0f;
            if (string.IsNullOrWhiteSpace(str)) return false;
            str = CleanNumberString(str, false);
            return float.TryParse(str, NumberStyles.Float, Invariant, out value);
        }

        public static float ToFloat(object obj, float defaultValue = 0f)
        {
            if (obj == null) return defaultValue;

            switch (obj)
            {
                case float f: return f;
                case double d: return (float)d;
                case decimal m: return (float)m;
                case int i: return i;
                case long l: return l;
                case bool b: return b ? 1f : 0f;
                case Enum e: return Convert.ToSingle(e);
                case string s: return ToFloat(s, defaultValue);
                default:
                    try
                    {
                        return Convert.ToSingle(obj, Invariant);
                    }
                    catch
                    {
                        return defaultValue;
                    }
            }
        }

        // ------------------------------
        // 4️⃣ 转双精度 double
        // ------------------------------
        public static double ToDouble(string str, double defaultValue = 0)
        {
            if (TryToDouble(str, out double value)) return value;
            return defaultValue;
        }

        public static bool TryToDouble(string str, out double value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(str)) return false;
            str = CleanNumberString(str, false);
            return double.TryParse(str, NumberStyles.Float, Invariant, out value);
        }

        public static double ToDouble(object obj, double defaultValue = 0)
        {
            if (obj == null) return defaultValue;

            switch (obj)
            {
                case double d: return d;
                case float f: return f;
                case decimal m: return (double)m;
                case int i: return i;
                case long l: return l;
                case bool b: return b ? 1d : 0d;
                case Enum e: return Convert.ToDouble(e);
                case string s: return ToDouble(s, defaultValue);
                default:
                    try
                    {
                        return Convert.ToDouble(obj, Invariant);
                    }
                    catch
                    {
                        return defaultValue;
                    }
            }
        }

        // ------------------------------
        // 5️⃣ 转高精度 decimal
        // ------------------------------
        public static decimal ToDecimal(string str, decimal defaultValue = 0)
        {
            if (TryToDecimal(str, out decimal value)) return value;
            return defaultValue;
        }

        public static bool TryToDecimal(string str, out decimal value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(str)) return false;
            str = CleanNumberString(str, false);
            return decimal.TryParse(str, NumberStyles.Float, Invariant, out value);
        }

        public static decimal ToDecimal(object obj, decimal defaultValue = 0)
        {
            if (obj == null) return defaultValue;

            switch (obj)
            {
                case decimal m: return m;
                case double d: return (decimal)d;
                case float f: return (decimal)f;
                case int i: return i;
                case long l: return l;
                case bool b: return b ? 1m : 0m;
                case Enum e: return Convert.ToDecimal(e);
                case string s: return ToDecimal(s, defaultValue);
                default:
                    try
                    {
                        return Convert.ToDecimal(obj, Invariant);
                    }
                    catch
                    {
                        return defaultValue;
                    }
            }
        }

        // ------------------------------
        // 6️⃣ 转布尔
        // ------------------------------
        public static bool ToBool(string str, bool defaultValue = false)
        {
            if (TryToBool(str, out bool value)) return value;
            return defaultValue;
        }

        public static bool TryToBool(string str, out bool value)
        {
            value = false;
            if (string.IsNullOrWhiteSpace(str)) return false;
            str = str.Trim().ToLowerInvariant();
            if (str == "true" || str == "1" || str == "yes" || str == "on")
            {
                value = true;
                return true;
            }

            if (str == "false" || str == "0" || str == "no" || str == "off")
            {
                value = false;
                return true;
            }

            return false;
        }

        public static bool ToBool(object obj, bool defaultValue = false)
        {
            if (obj == null) return defaultValue;

            switch (obj)
            {
                case bool b: return b;
                case int i: return i != 0;
                case long l: return l != 0;
                case float f: return f != 0f; // 修复 Epsilon 问题
                case double d: return d != 0d; // 修复 Epsilon 问题
                case decimal m: return m != 0m;
                case Enum e: return Convert.ToInt32(e) != 0;
                case string s: return ToBool(s, defaultValue);
                default:
                    try
                    {
                        return Convert.ToBoolean(obj, Invariant);
                    }
                    catch
                    {
                        return defaultValue;
                    }
            }
        }

        // ------------------------------
        // 7️⃣ 强大的泛型转换 (支持 Nullable, Enum, DateTime, Guid)
        // ------------------------------
        public static T To<T>(object obj, T defaultValue = default)
        {
            if (obj == null || obj == DBNull.Value) return defaultValue;

            // 如果类型本身匹配，直接返回
            if (obj is T tValue) return tValue;

            Type targetType = typeof(T);
            Type underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            string str = obj.ToString();

            try
            {
                if (underlyingType == typeof(string)) return (T)(object)str;
                if (underlyingType == typeof(int)) return (T)(object)ToInt(str, Convert.ToInt32(defaultValue));
                if (underlyingType == typeof(long)) return (T)(object)ToLong(str, Convert.ToInt64(defaultValue));
                if (underlyingType == typeof(float)) return (T)(object)ToFloat(str, Convert.ToSingle(defaultValue));
                if (underlyingType == typeof(double)) return (T)(object)ToDouble(str, Convert.ToDouble(defaultValue));
                if (underlyingType == typeof(decimal))
                    return (T)(object)ToDecimal(str, Convert.ToDecimal(defaultValue));
                if (underlyingType == typeof(bool)) return (T)(object)ToBool(str, Convert.ToBoolean(defaultValue));

                // 扩展支持
                if (underlyingType.IsEnum) return (T)Enum.Parse(underlyingType, str, true);
                if (underlyingType == typeof(DateTime)) return (T)(object)DateTime.Parse(str, Invariant);
                if (underlyingType == typeof(Guid)) return (T)(object)Guid.Parse(str);

                return (T)Convert.ChangeType(obj, underlyingType, Invariant);
            }
            catch
            {
                return defaultValue;
            }
        }

        // ------------------------------
        // 8️⃣ 格式化输出
        // ------------------------------
        public static string ToStringInvariant(float value, string format = "G") => value.ToString(format, Invariant);
        public static string ToStringInvariant(double value, string format = "G") => value.ToString(format, Invariant);
        public static string ToStringInvariant(decimal value, string format = "G") => value.ToString(format, Invariant);
        public static string ToStringInvariant(int value) => value.ToString(Invariant);
        public static string ToStringInvariant(long value) => value.ToString(Invariant);

        // ------------------------------
        // 9️⃣ 内部方法：清理数字字符串 (修复越界和截断Bug)
        // ------------------------------
        private static string CleanNumberString(string str, bool isInteger = false)
        {
            if (string.IsNullOrWhiteSpace(str)) return "";

            str = str.Trim().Replace(" ", "").Replace("'", "");

            if (isInteger)
            {
                // 如果转整数，遇到第一个小数点/逗号，直接截断后面的内容
                // 避免 "123.45" 变成 12345
                int dotIndex = str.IndexOfAny(new[] { '.', ',' });
                if (dotIndex >= 0)
                {
                    str = str.Substring(0, dotIndex);
                }

                return str;
            }

            // 浮点数处理逻辑
            int lastDot = str.LastIndexOf('.');
            int lastComma = str.LastIndexOf(',');
            int decimalPos = Math.Max(lastDot, lastComma);

            if (decimalPos >= 0)
            {
                // 将字符串拆分为整数部分和小数部分，分别去除所有的 . 和 ,
                string intPart = str.Substring(0, decimalPos).Replace(".", "").Replace(",", "");
                string decPart = str.Substring(decimalPos + 1).Replace(".", "").Replace(",", "");
                return intPart + "." + decPart;
            }

            return str;
        }
    }
}