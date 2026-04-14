namespace Nghex.Core.Extension
{
    public static class IntExtend
    {
        public static int ToInt(this string value)
        {
            return int.TryParse(value, out int result) ? result : 0;
        }

        public static int ToInt(this string value, int defaultValue)
        {
            return int.TryParse(value, out int result) ? result : defaultValue;
        }

        public static int ToInt(this string value, Func<int> defaultValueFactory)
        {
            return int.TryParse(value, out int result) ? result : defaultValueFactory();
        }
    }
}