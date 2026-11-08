using System.Reflection;
using KuyumHesapFiyatEkran.UI;
using KuyumHesapFiyatEkran.UI.Pages;
using KuyumHesapFiyatEkran.UI.Services;
using KuyumHesapFiyatEkran.Core.Features.LiveRates.Queries.GetLiveRates;

var store = new LiveRatesStore { CurrentRates = new GetLiveRatesQueryResponse
{
    Sarrafiye = new List<LiveRateItem> { new() { Name = "GRAM ALTIN", Buy = "100", Sell = "110" } }
} };
var page = new SalihHomePage(store);
var flags = BindingFlags.Instance | BindingFlags.NonPublic;
var render = typeof(SalihHomePage).GetMethod("Render", flags)!;
var ratesField = typeof(SalihHomePage).GetField("rates", flags)!;
var populate = typeof(SalihHomePage).GetMethod("PopulateRates", flags)!;

for (var i = 0; i < 30; i++)
{
    var previousRates = (VerticalStackLayout)ratesField.GetValue(page)!;
    var previousParent = (Layout)previousRates.Parent;
    AppThemeManager.Mode = i % 2 == 0 ? AslanThemeMode.Light : AslanThemeMode.Dark;
    render.Invoke(page, null);
    var currentRates = (VerticalStackLayout)ratesField.GetValue(page)!;
    if (ReferenceEquals(previousRates, currentRates) && previousParent.Children.Contains(currentRates)
        && !ReferenceEquals(currentRates.Parent, previousParent))
    {
        throw new Exception("Home rebuild reuses a price table still owned by the previous layout.");
    }
    if (currentRates.Parent is not Layout currentParent || !currentParent.Children.Contains(currentRates))
        throw new Exception("The price table is missing from the rebuilt home page.");
    populate.Invoke(page, null);
    if (currentRates.Children.Count != 2)
        throw new Exception("Returning home must show the header and the cached price row.");
}
Console.WriteLine("PASS: 30 home rebuilds, including theme changes, preserve single ownership of the price table.");
store.CurrentRates = null;
populate.Invoke(page, null);
if (((VerticalStackLayout)ratesField.GetValue(page)!).Children.Count != 1)
    throw new Exception("Missing prices must show the empty state.");
Console.WriteLine("PASS: cached prices and empty state render after rebuilding home.");

var calculator = new GoldCalculatorPage(store);
var applyCalculatorTheme = typeof(GoldCalculatorPage).GetMethod("ApplyTheme", flags)!;
var controls = typeof(GoldCalculatorPage).GetFields(flags | BindingFlags.DeclaredOnly)
    .Where(field => typeof(View).IsAssignableFrom(field.FieldType))
    .Select(field => (View)field.GetValue(calculator)!)
    .ToList();
for (var i = 0; i < 30; i++)
{
    var previousParents = controls.Select(control => control.Parent).ToList();
    AppThemeManager.Mode = i % 2 == 0 ? AslanThemeMode.Light : AslanThemeMode.Dark;
    applyCalculatorTheme.Invoke(calculator, null);
    for (var index = 0; index < controls.Count; index++)
    {
        var control = controls[index];
        if (previousParents[index] is Layout oldParent && oldParent.Children.Contains(control)
            && !ReferenceEquals(control.Parent, oldParent))
            throw new Exception($"Calculator reattaches {control.GetType().Name} without removing its previous parent.");
    }
}
Console.WriteLine("PASS: calculator controls retain a single parent through 30 theme updates.");
