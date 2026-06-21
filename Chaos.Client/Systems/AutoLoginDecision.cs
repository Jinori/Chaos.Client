// Chaos.Client/Systems/AutoLoginDecision.cs
namespace Chaos.Client.Systems;

/// <summary>Pure decision for the login screen: given the (possibly null) auto-login env values,
/// decide whether to pre-fill the username and whether to auto-submit. Isolated from MonoGame so
/// the rule is trivial to reason about and reuse.</summary>
public readonly record struct AutoLoginDecision(bool ShouldFill, string Username, bool ShouldSubmit, string? Password)
{
    public static AutoLoginDecision Decide(string? username, string? password)
    {
        if (string.IsNullOrWhiteSpace(username))
            return new AutoLoginDecision(false, "", false, null);

        var hasPassword = !string.IsNullOrEmpty(password);
        return new AutoLoginDecision(true, username, hasPassword, hasPassword ? password : null);
    }
}
