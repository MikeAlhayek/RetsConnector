using System;

namespace RetsSdk.Helpers
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

        public static string TrimStart(string target, string trimString)
        {
            if (string.IsNullOrEmpty(trimString))
            {
                return target;
            }

            string result = target;
            while (result.StartsWith(trimString))
            {
                result = result.Substring(trimString.Length);
            }

            return result;
        }

        public static string TrimEnd(string target, string trimString)
        {
            if (string.IsNullOrEmpty(trimString))
            {
                return target;
            }

            string result = target;

            while (result.EndsWith(trimString))
            {
                result = result.Substring(0, result.Length - trimString.Length);
            }

            return result;
        }


        public static string AppendOnce(string orginal, string toAppend)
        {
            if (orginal == null || orginal.EndsWith(toAppend))
            {
                return orginal;
            }

            return orginal + toAppend;
        }


        public static string PrependOnce(string orginal, string toAppend)
        {
            if (orginal == null || orginal.StartsWith(toAppend))
            {
                return orginal;
            }

            return toAppend + orginal;
        }
    }
}
