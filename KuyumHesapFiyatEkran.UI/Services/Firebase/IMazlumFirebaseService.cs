namespace KuyumHesapFiyatEkran.UI.Services.Firebase;

public interface IMazlumFirebaseService
{
    Task<MazlumFirebaseRatesResult> GetRatesAsync(
        CancellationToken cancellationToken = default);

    Task<List<MazlumFirebaseKurItem>> GetKurCatalogAsync(
        CancellationToken cancellationToken = default);

    Task<List<MazlumFirebaseBranch>> GetBranchesAsync(
        CancellationToken cancellationToken = default);

    Task<MazlumFirebasePageContent> GetPageAsync(
        string id,
        MazlumFirebasePageContent fallback,
        CancellationToken cancellationToken = default);

    Task SavePageAsync(
        string id,
        MazlumFirebasePageContent content,
        string idToken,
        CancellationToken cancellationToken = default);

    Task<MazlumFirebaseContact> GetContactAsync(
        CancellationToken cancellationToken = default);

    Task SaveContactAsync(
        MazlumFirebaseContact contact,
        string idToken,
        CancellationToken cancellationToken = default);

    Task<string> SendBroadcastAsync(
        string title,
        string body,
        string? createdBy,
        string idToken,
        CancellationToken cancellationToken = default);

}
