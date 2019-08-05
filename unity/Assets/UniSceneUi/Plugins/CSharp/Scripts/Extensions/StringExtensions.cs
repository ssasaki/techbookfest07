namespace UniSceneUi
{
    public static class StringExtensions
    {
        public static string Formats(this string format, params object[] values)
        {
            return string.Format(format, values);
        }

        public static bool IsNullOrEmpty(this string source)
        {
            return string.IsNullOrEmpty(source);
        }
    }
}