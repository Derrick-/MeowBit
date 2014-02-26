using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace dotBitNS
{
    public static class StringUtils
    {
        static CultureInfo cultureInfo = System.Threading.Thread.CurrentThread.CurrentCulture;

        public static string RandomName(int len = 8)
        {
            if (len < 1) throw new ArgumentException("Length must be greater than 0", "len");

            StringBuilder sb = new StringBuilder(len);
            for (int i = 0; i < len; i++)
                sb.Append(RandomAlphaUpperChar());
            return sb.ToString();
        }

        static Random random = new Random();

        public static char RandomAlphaUpperChar()
        {
            return (char)(random.Next((int)'A', (int)'Z'));
        }



        internal static string GenerateSecureRandomKey(int length)
        {
            return SecureRandomString(length);
        }

        internal static string SecureRandomString(int length, string allowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789")
        {
            if (length < 0) throw new ArgumentOutOfRangeException("length", "length cannot be less than zero.");
            if (string.IsNullOrEmpty(allowedChars)) throw new ArgumentException("allowedChars may not be empty.");

            const int byteSize = 0x100;
            var allowedCharSet = new HashSet<char>(allowedChars).ToArray();
            if (byteSize < allowedCharSet.Length) throw new ArgumentException(String.Format("allowedChars may contain no more than {0} characters.", byteSize));

            // Guid.NewGuid and System.Random are not particularly random. By using a
            // cryptographically-secure random number generator, the caller is always
            // protected, regardless of use.
            using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
            {
                var result = new StringBuilder();
                var buf = new byte[128];
                while (result.Length < length)
                {
                    rng.GetBytes(buf);
                    for (var i = 0; i < buf.Length && result.Length < length; ++i)
                    {
                        // Divide the byte into allowedCharSet-sized groups. If the
                        // random value falls into the last group and the last group is
                        // too small to choose from the entire allowedCharSet, ignore
                        // the value in order to avoid biasing the result.
                        var outOfRangeStart = byteSize - (byteSize % allowedCharSet.Length);
                        if (outOfRangeStart <= buf[i]) continue;
                        result.Append(allowedCharSet[buf[i] % allowedCharSet.Length]);
                    }
                }
                return result.ToString();
            }
        }

        internal static string TitleCase(string title)
        {
            if (string.IsNullOrWhiteSpace(title)) return title;
            TextInfo textInfo = cultureInfo.TextInfo;
            return textInfo.ToTitleCase(title);
        }
    }
}