namespace KuyumHesapFiyatEkran.UI.Services.Firebase;

public sealed class MazlumFirebaseOptions
{
    public string ProjectId { get; set; } = "mazlum-kuyumculuk";

    public string FirestoreBaseUrl =>
        $"https://firestore.googleapis.com/v1/projects/{ProjectId}/databases/(default)/documents";
}
