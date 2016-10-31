namespace Crunch.Extensions
{
    public static class StringExtensions {
        public static string FormatWith(this string input, params object[] args) {
            return string.Format(input, args);
        }
    }
}