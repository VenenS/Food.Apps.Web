namespace ITWebNet.Food.Site
{
    public static class StringExtensions
    {
        private static readonly string _key = "46a9afb7cebc042936c9edf2451df7b7";

        /// <summary>
        /// Шифрует строку.
        /// </summary>
        public static string Protect(this string value) => Crypto.Encrypt<string>(_key, value);

        /// <summary>
        /// Расшифровывает строку, зашифрованную с помощью <see cref="Protect(string)"/>.
        /// </summary>
        public static string Unprotect(this string value) => Crypto.Decrypt<string>(_key, value);
    }
}
