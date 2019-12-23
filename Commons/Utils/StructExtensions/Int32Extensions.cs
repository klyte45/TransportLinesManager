namespace Klyte.Commons.Utils
{
    public static class Int32Extensions
    {
        public static int ParseOrDefault(string val, int defaultVal)
        {
            try
            {
                return int.Parse(val);
            }
            catch
            {
                return defaultVal;
            }
        }
    }
}
