using System.Security.Cryptography;
using System.Text;

namespace PharmaFind.Api.Services;

public static class PasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100_000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;
    private const string Pbkdf2Prefix = "$pbkdf2$v1$";

    /// <summary>
    /// Hashes a password using PBKDF2 with SHA-256 and a cryptographically secure random salt.
    /// </summary>
    public static string Hash(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Le mot de passe ne peut pas être vide.", nameof(password));

        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            Algorithm,
            HashSize);

        return $"{Pbkdf2Prefix}{Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    /// <summary>
    /// Verifies a password against a stored hash (supports both modern PBKDF2 and legacy SHA-256 hashes).
    /// </summary>
    public static bool Verify(string password, string storedHash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedHash))
            return false;

        // Modern PBKDF2 format
        if (storedHash.StartsWith(Pbkdf2Prefix, StringComparison.Ordinal))
        {
            var parts = storedHash[Pbkdf2Prefix.Length..].Split('$');
            if (parts.Length != 2) return false;

            try
            {
                byte[] salt = Convert.FromBase64String(parts[0]);
                byte[] expectedHash = Convert.FromBase64String(parts[1]);

                byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(
                    password,
                    salt,
                    Iterations,
                    Algorithm,
                    expectedHash.Length);

                return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
            }
            catch
            {
                return false;
            }
        }

        // Legacy SHA256 verification (for backward compatibility with demo seed data)
        var legacyBytes = SHA256.HashData(Encoding.UTF8.GetBytes(password + "_PharmaFindSalt_2026"));
        var legacyExpected = Convert.FromBase64String(storedHash);
        return CryptographicOperations.FixedTimeEquals(legacyBytes, legacyExpected);
    }
}
