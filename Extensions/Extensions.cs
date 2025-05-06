using System;
using System.Text;

namespace LogicTree
{
    /// <summary>
    /// 提供字符串扩展方法的静态类
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// 返回将该字符串转换为蛇形命名(snake_case)的结果        
        /// </summary>
        /// <remarks>
        /// 注意：如果数字之后存在的是单个字符，则不会进行拆分，这是为了保证某些单词的连贯（例如"2D"）。
        /// </remarks>
        /// <param name="input">需要转换的字符串</param>
        /// <returns>该字符串蛇形命名的结果</returns>
        public static string ToSnakeCase(this string input)
        {
            StringBuilder sb = new();
            char[] chars = input.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                // 处理空格，直接转换为下划线
                if (chars[i] == ' ')
                {
                    sb.Append('_');
                }
                // 大写字母：
                // 当后接小写字母且前有连续大写时添加下划线（处理类似"HTTPRequest"的情况）
                // 统一转换为小写字母
                else if (char.IsUpper(chars[i]))
                {
                    if (i + 1 < chars.Length && char.IsLower(chars[i + 1]) &&
                        i > 0 && char.IsUpper(chars[i - 1]))
                    {
                        sb.Append('_');
                    }
                    sb.Append(char.ToLower(chars[i]));
                }
                // 小写字母：
                // 当前字符在数字之后且后续有多个小写字母时添加下划线（处理类似"test2data"的情况）                
                // 当前字符后接大写字母时添加下划线
                else if (char.IsLower(chars[i]))
                {
                    if (i > 0 && char.IsDigit(chars[i - 1]) && i + 1 < chars.Length && char.IsLower(chars[i + 1]))
                    {
                        sb.Append('_');
                    }
                    sb.Append(chars[i]);
                    if (i + 1 < chars.Length && char.IsUpper(chars[i + 1]))
                    {
                        sb.Append('_');
                    }
                }
                // 数字：
                // 当数字前有字母时添加下划线
                else if (char.IsDigit(chars[i]))
                {
                    if (i > 0 && (char.IsUpper(chars[i - 1]) || char.IsLower(chars[i - 1])))
                    {
                        sb.Append('_');
                    }
                    sb.Append(chars[i]);
                }
                // 其他字符原样添加
                else
                {
                    sb.Append(chars[i]);
                }

            }
            return sb.ToString();
        }

        /// <summary>
        /// 返回将该字符串转换为大帕斯卡命名(PascalCase)的结果
        /// </summary>
        /// <param name="input">需要转换的字符串</param>
        /// <returns>该字符串大帕斯卡命名的结果</returns>
        public static string ToPascalCase(this string input)
        {
            StringBuilder sb = new();
            char[] chars = input.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                if (chars[i] == '_' || chars[i] == ' ')
                {
                    continue;
                }
                else if (char.IsLower(chars[i]))
                {
                    if ((i == 0) ||
                        (i > 0 && (chars[i - 1] == '_' || chars[i - 1] == ' ')))
                    {
                        sb.Append(char.ToUpper(chars[i]));
                    }
                    else
                    {
                        sb.Append(chars[i]);
                    }
                }
                else
                {
                    sb.Append(chars[i]);
                }
            }
            return sb.ToString();
        }
    }

    /// <summary>  
    /// 提供枚举扩展方法的静态类  
    /// </summary>  
    public static class EnumExtensions
    {
        /// <summary>  
        /// 返回将该枚举值的字符串表示转换为蛇形命名(snake_case)的结果  
        /// </summary>  
        /// <param name="input">需要转换的枚举值</param>  
        /// <returns>该枚举值字符串表示的蛇形命名结果</returns>  
        public static string ToSnakeCase(this Enum input)
            => input.ToString().ToSnakeCase();
    }
}
/// <summary>  
/// 提供数学相关方法的静态类  
/// </summary>  
public static class Mathf
{
    /// <summary>  
    /// 确定两个双精度浮点数是否近似相等  
    /// </summary>  
    /// <param name="a">第一个双精度浮点数</param>  
    /// <param name="b">第二个双精度浮点数</param>  
    /// <returns>如果两个数近似相等，则返回 true；否则返回 false</returns>  
    public static bool IsEqualApprox(double a, double b)
    {
        if (a == b)
        {
            return true;
        }

        double num = 1E-14 * Math.Abs(a);
        if (num < 1E-14)
        {
            num = 1E-14;
        }

        return Math.Abs(a - b) < num;
    }
}
