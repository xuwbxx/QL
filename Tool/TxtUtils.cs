using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Tool
{
    public class TxtUtils
    {
        private static Encoding? _gb2312;
        private static bool _initialized;

        private static Encoding GetGB2312()
        {
            if (!_initialized)
            {
                try
                {
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                    _gb2312 = Encoding.GetEncoding("GB2312");
                }
                catch
                {
                    _gb2312 = Encoding.Default;
                }
                _initialized = true;
            }
            return _gb2312!;
        }

        public static string Read(string path)
        {
            string result = string.Empty;
            using (StreamReader streamReader = new StreamReader(path, Encoding.Default))
            {
                result = streamReader.ReadToEnd();
            }

            return result;
        }

        public static string GetChineseSpell(string strText)
        {
            int length = strText.Length;
            string text = "";
            for (int i = 0; i < length; i++)
            {
                text += getSpell(strText.Substring(i, 1));
            }

            return text.ToUpper();
        }

        private static string getSpell(string cnChar)
        {
            byte[] bytes = GetGB2312().GetBytes(cnChar);
            if (bytes.Length > 1)
            {
                int num = bytes[0];
                int num2 = bytes[1];
                int num3 = (num << 8) + num2;
                int[] array = new int[26]
                {
                45217, 45253, 45761, 46318, 46826, 47010, 47297, 47614, 48119, 48119,
                49062, 49324, 49896, 50371, 50614, 50622, 50906, 51387, 51446, 52218,
                52698, 52698, 52698, 52980, 53689, 54481
                };
                for (int i = 0; i < 26; i++)
                {
                    int num4 = 55290;
                    if (i != 25)
                    {
                        num4 = array[i + 1];
                    }

                    if (array[i] <= num3 && num3 < num4)
                    {
                        return ((char)(97 + i)).ToString();
                    }
                }

                return "*";
            }

            return cnChar;
        }

        public static string StringReadNumber(string text)
        {
            string pattern = "\\d+";
            MatchCollection matchCollection = Regex.Matches(text, pattern);
            string text2 = "";
            foreach (Match item in matchCollection)
            {
                text2 += item.Value;
            }

            return text2;
        }
    }
}
