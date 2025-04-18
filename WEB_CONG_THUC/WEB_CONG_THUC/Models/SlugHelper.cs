using System.Globalization;
using System.Text.RegularExpressions;
using System.Text;

namespace WEB_CONG_THUC.Models
{

    public static class SlugHelper
    {
        public static string GenerateSlug(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            input = input.ToLower().Trim();

            // Chuẩn hóa Unicode (loại bỏ dấu tiếng Việt)
            input = RemoveDiacritics(input);

            // Thay thế ký tự không phải chữ cái/thanh ngang bằng dấu gạch ngang
            input = Regex.Replace(input, @"[^a-z0-9\s-]", "");
            input = Regex.Replace(input, @"\s+", "-").Trim('-');

            return input;
        }

        private static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();
            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }
            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }

}
