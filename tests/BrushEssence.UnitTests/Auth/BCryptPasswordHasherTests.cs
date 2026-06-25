using BrushEssence.Infrastructure.Identity;
using Xunit;

namespace BrushEssence.UnitTests.Auth;

public class BCryptPasswordHasherTests
{
    private readonly BCryptPasswordHasher _hasher = new();

    [Fact]
    public void Hash_then_verify_succeeds()
    {
        var hash = _hasher.Hash("Sup3rSecret");

        Assert.NotEqual("Sup3rSecret", hash);
        Assert.True(_hasher.Verify("Sup3rSecret", hash));
    }

    [Fact]
    public void Verify_fails_for_wrong_password()
    {
        var hash = _hasher.Hash("Sup3rSecret");

        Assert.False(_hasher.Verify("WrongPassword1", hash));
    }

    [Fact]
    public void Verify_returns_false_for_malformed_hash()
    {
        Assert.False(_hasher.Verify("whatever", "not-a-bcrypt-hash"));
    }
}
