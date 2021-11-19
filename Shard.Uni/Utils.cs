namespace Shard.Uni
{
    public static class Utils
    {
        public static class Strings
        {
            public static string Capitalize(string word)
            {
                return word.Substring(0, 1).ToUpper() + word.Substring(1).ToLower();
            }
        }
    }
}
