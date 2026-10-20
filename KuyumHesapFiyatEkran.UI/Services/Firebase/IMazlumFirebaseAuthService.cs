namespace KuyumHesapFiyatEkran.UI.Services.Firebase;

public interface IMazlumFirebaseAuthService
{
    bool IsAuthenticated { get; }

    string? UserEmail { get; }

    Task<MazlumAuthResult> SignInAsync(
        string email,
        string password,
        bool remember);

    Task SignOutAsync();

    Task RestoreSessionAsync();

    Task<string?> GetIdTokenAsync();
}

public sealed class MazlumAuthResult
{
    public bool Success { get; init; }

    public string? Email { get; init; }

    public string? ErrorMessage { get; init; }
}
