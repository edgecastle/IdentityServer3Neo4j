namespace Edgecastle.IdentityServer3.Neo4j
{
    /// <summary>
    /// Performs cryptographic operations on a password
    /// </summary>
    public static class PasswordSecurity
    {
        private const int BCryptWorkFactor = 10; // Recommended

        /// <summary>
        /// Hashes the input
        /// </summary>
        /// <param name="input">The text to hash</param>
        /// <returns>The hash</returns>
        public static string Hash(string input)
        {
            return BCrypt.Net.BCrypt.HashPassword(
                input: input,
                workFactor: BCryptWorkFactor
            );
        }

        /// <summary>
        /// Verifies a hash against plaintext
        /// </summary>
        /// <param name="input">The plaintext to verify</param>
        /// <param name="hash">The hash value to compare to</param>
        /// <returns>True, if a match, otherwise false.</returns>
        public static bool Verify(string input, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(
                text: input,
                hash: hash
            );
        }
    }
}