using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CrestApps.RetsSdk.Helpers
{
    public class Str
    {
        public static string GetValue(string startWith, string[] lines, char splitter = '=')
        {
            foreach (var line in lines)
            {
                var value = line.Trim();

                if (!value.StartsWith(startWith, StringComparison.CurrentCultureIgnoreCase) || !line.Contains(splitter))
                {
                    continue;
                }

                return value.Split(splitter)[1].Trim();
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets a null if the giving string is null or whitespace or a trimmed string
        /// </summary>
        /// <param name="value"></param>
        /// <param name="trimStart"></param>
        /// <param name="trimEnd"></param>
        /// <returns></returns>
        public static string NullOrString(string value, bool trimStart = true, bool trimEnd = true)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            if (trimStart)
            {
                value = value.TrimStart();
            }

            if (trimEnd)
            {
                value = value.TrimEnd();
            }

            return value;
        }

        public static string Md5(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// Adds a space after each Capital Letter.
        /// "HelloWorldThisIsATest" would then be "Hello World This Is A Test"
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string AddSpacesToWords(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            return Regex.Replace(text, "([A-Z])([a-z]*)", " $1$2").Trim();
        }

        /// <summary>
        /// Add ordinal to a giving number
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static string AddOrdinal(int num)
        {
            if (num <= 0)
            {
                return num.ToString();
            }

            switch (num % 100)
            {
                case 11:
                case 12:
                case 13:
                    return num + "th";
            }

            switch (num % 10)
            {
                case 1:
                    return num + "st";
                case 2:
                    return num + "nd";
                case 3:
                    return num + "rd";
                default:
                    return num + "th";
            }

        }


        public static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        public static string TrimEnd(string subject, string pattern)
        {
            return TrimEnd(subject, pattern, StringComparison.CurrentCulture);
        }

        public static string Random(int length = 40)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwzyz";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string ToLower(string str, string defaultValue = "")
        {
            if (str != null)
            {
                return str.ToLower();
            }

            return defaultValue;
        }

        public static string TrimEnd(string subject, string pattern, StringComparison type)
        {
            if (string.IsNullOrWhiteSpace(subject) || subject == pattern)
            {
                return string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(pattern) && subject.EndsWith(pattern, type))
            {
                int index = subject.Length - pattern.Length;

                return subject.Substring(0, index);
            }

            return subject;
        }


        public static int CountOccurrences(string text, string pattern)
        {
            int count = 0;

            int i = 0;

            while ((i = text.IndexOf(pattern, i)) != -1)
            {
                i += pattern.Length;
                count++;
            }

            return count;
        }

        public static string StringOrNull(string str, bool trim = true)
        {

            if (string.IsNullOrWhiteSpace(str))
            {
                return null;
            }

            if (trim)
            {
                return str.Trim();
            }

            return str;
        }

        public static string UppercaseFirst(string word, bool lowercaseTheRest = true)
        {
            if (string.IsNullOrEmpty(word))
            {
                return word;
            }

            string final = char.ToUpper(word[0]).ToString();

            if (lowercaseTheRest)
            {
                return final + word.Substring(1).ToLower();
            }

            return final + word.Substring(1);
        }

        public static string AppendOnce(string orginal, string toAppend = "/")
        {
            if (orginal == null || orginal.EndsWith(toAppend))
            {
                return orginal;
            }

            return orginal + toAppend;
        }


        public static string PrependOnce(string orginal, string toAppend = "/")
        {
            if (orginal == null || orginal.StartsWith(toAppend))
            {
                return orginal;
            }

            return toAppend + orginal;
        }

        public static string TrimStart(string subject, string pattern)
        {
            return TrimStart(subject, pattern, StringComparison.CurrentCulture);
        }

        public static string TrimStart(string subject, string pattern, StringComparison type)
        {
            if (string.IsNullOrWhiteSpace(subject) || subject == pattern)
            {
                return string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(pattern) && subject.StartsWith(pattern, type))
            {
                return subject.Substring(pattern.Length);
            }

            return subject;
        }

        public static string SubstringUntil(string str, string until, bool trim = true)
        {
            string substring = str;

            if (str != null && !string.IsNullOrEmpty(until))
            {
                int index = str.IndexOf(until);

                if (index >= 0)
                {
                    substring = str.Substring(0, index);
                }
            }

            if (trim)
            {
                substring = substring.Trim();
            }

            return substring;
        }

        public static string ConvertNewLinesToBr(string str, bool isHtml5 = true)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return "";
            }

            string newValue = isHtml5 ? "<br>" : "<br />";

            return str.Replace("\r\n", newValue)
                      .Replace("\n", newValue)
                      .Replace("\r", newValue);
        }

        public static string ReplaceFirst(string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }
    }
}
