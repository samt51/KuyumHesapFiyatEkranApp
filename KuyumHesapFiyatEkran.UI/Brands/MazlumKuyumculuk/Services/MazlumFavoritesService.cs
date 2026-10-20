namespace KuyumHesapFiyatEkran.UI.Brands.MazlumKuyumculuk.Services;

public sealed class MazlumFavoritesService
{
    private const string PreferenceKey =
        "mazlum-favorite-price-ids";

    public bool IsFavorite(string id)
    {
        return GetIds().Contains(id);
    }

    public bool Toggle(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return false;

        var ids = GetIds();

        if (!ids.Add(id))
        {
            ids.Remove(id);
            Save(ids);
            return false;
        }

        Save(ids);
        return true;
    }

    public HashSet<string> GetIds()
    {
        var value = Preferences.Default.Get(
            PreferenceKey,
            string.Empty);

        return value
            .Split(
                ',',
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries)
            .ToHashSet(
                StringComparer.OrdinalIgnoreCase);
    }

    private static void Save(
        IEnumerable<string> ids)
    {
        Preferences.Default.Set(
            PreferenceKey,
            string.Join(",", ids));
    }
}
