using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace UpRaise.Extensions
{





    public static class StringExtensions
    {
        public static string EncodeToBase64(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(value);
            var emailTokenUrlEncoded = $"{System.Convert.ToBase64String(plainTextBytes)}";
            return emailTokenUrlEncoded;
        }

        public static string DecodeFromBase64(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var base64EncodedBytes = System.Convert.FromBase64String(value);
            var emailTokenUrlDecoded = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);

            return emailTokenUrlDecoded;
        }


        public static string EmptyIfNull(this object value)
        {
            if (value == null)
                return "";
            return value.ToString();
        }

        public static string Left(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            maxLength = Math.Abs(maxLength);

            return (value.Length <= maxLength
                   ? value
                   : value.Substring(0, maxLength)
                   );
        }

        public static string StripHTML(this string HTMLText)
        {
            if (string.IsNullOrWhiteSpace(HTMLText))
                return HTMLText;

            HTMLText = HTMLText.Replace("</p>", "\r\n</p>");

            var reg = new Regex("<[^>]+>", RegexOptions.IgnoreCase);
            return reg.Replace(HTMLText, "");
        }

        public static bool IsHtml(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            var tagRegex = new Regex(@"<\s*([^ >]+)[^>]*>.*?<\s*/\s*\1\s*>");
            bool hasTags = tagRegex.IsMatch(text);
            return hasTags;
        }

        public static string LeftTrim(this string text, int maxChars, bool addEllipse)
        {
            if (string.IsNullOrWhiteSpace(text) || text.Length < maxChars)
                return text;

            var txt = text.Left(maxChars) + (addEllipse ? "..." : "");
            return txt;
        }

        public static string[] ConvertJSONToList(this string jsonString)
        {
            try
            {
                if (string.IsNullOrEmpty(jsonString))
                    return null;

                var data = JsonConvert.DeserializeObject<string[]>(jsonString);
                return data;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"{ex.Message}");
            }
            return null;
        }

        public static string ConvertListToJson(this string[] listOfData)
        {
            try
            {
                if (listOfData == null || !listOfData.Any())
                    return null;

                var data = JsonConvert.SerializeObject(listOfData);
                return data;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"{ex.Message}");
            }
            return null;
        }


        public static bool IsValidEmail(this string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Normalize the domain
                email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                                      RegexOptions.None, TimeSpan.FromMilliseconds(200));

                // Examines the domain part of the email and normalizes it.
                string DomainMapper(Match match)
                {
                    // Use IdnMapping class to convert Unicode domain names.
                    var idn = new IdnMapping();

                    // Pull out and process domain name (throws ArgumentException on invalid)
                    string domainName = idn.GetAscii(match.Groups[2].Value);

                    return match.Groups[1].Value + domainName;
                }
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }

            try
            {
                return Regex.IsMatch(email,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }



    }
}
