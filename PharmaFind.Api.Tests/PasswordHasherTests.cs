using PharmaFind.Api.Services;
using Xunit;

namespace PharmaFind.Api.Tests;

public class PasswordHasherTests
{
    [Fact]
    public void Hash_GeneratesDifferentHashesForSamePasswordDueToSalt()
    {
        string pwd = "SecretPassword123!";
        string hash1 = PasswordHasher.Hash(pwd);
        string hash2 = PasswordHasher.Hash(pwd);

        Assert.NotEqual(hash1, hash2);
        Assert.StartsWith("$pbkdf2$v1$", hash1);
        Assert.StartsWith("$pbkdf2$v1$", hash2);
    }

    [Fact]
    public void Verify_ValidPassword_ReturnsTrue()
    {
        string pwd = "CorrectPassword2026";
        string hash = PasswordHasher.Hash(pwd);

        bool result = PasswordHasher.Verify(pwd, hash);

        Assert.True(result);
    }

    [Fact]
    public void Verify_WrongPassword_ReturnsFalse()
    {
        string pwd = "CorrectPassword2026";
        string hash = PasswordHasher.Hash(pwd);

        bool result = PasswordHasher.Verify("WrongPassword", hash);

        Assert.False(result);
    }

    [Fact]
    public void Verify_LegacySha256Hash_ReturnsTrueForDemoPassword()
    {
        // Compute legacy hash using seeder logic: SHA256("Demo1234_PharmaFindSalt_2026")
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes("Demo1234_PharmaFindSalt_2026"));
        var computedLegacy = Convert.ToBase64String(bytes);

        bool result = PasswordHasher.Verify("Demo1234", computedLegacy);
        Assert.True(result);

        bool wrongResult = PasswordHasher.Verify("WrongDemo", computedLegacy);
        Assert.False(wrongResult);
    }
}
