using Microsoft.Maui.Graphics;

namespace KuyumHesapFiyatEkran.UI.Brands.MazlumKuyumculuk.Pages;

public partial class MazlumSplashPage : ContentPage
{
    private readonly AppShell _appShell;
    private bool _navigated;

    public MazlumSplashPage(AppShell appShell)
    {
        InitializeComponent();

        _appShell = appShell;

        NavigationPage.SetHasNavigationBar(this, false);
        Shell.SetNavBarIsVisible(this, false);

        TopGoldDecoration.Drawable =
            new GoldWaveDrawable(isBottom: false);

        BottomGoldDecoration.Drawable =
            new GoldWaveDrawable(isBottom: true);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_navigated)
            return;

        _navigated = true;

        await Task.Delay(1600);

        if (Application.Current?.Windows.Count > 0)
            Application.Current.Windows[0].Page = _appShell;
    }

    private sealed class GoldWaveDrawable : IDrawable
    {
        private readonly bool _isBottom;

        private static readonly Color Gold =
            Color.FromArgb("#C8A33A");

        private static readonly Color SoftGold =
            Color.FromArgb("#E6D18A");

        public GoldWaveDrawable(bool isBottom)
        {
            _isBottom = isBottom;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.SaveState();

            if (_isBottom)
            {
                canvas.Translate(
                    dirtyRect.Width,
                    dirtyRect.Height);

                canvas.Rotate(180);
            }

            DrawFineWaves(canvas);
            DrawMainRibbon(canvas);

            canvas.RestoreState();
        }

        private static void DrawFineWaves(ICanvas canvas)
        {
            canvas.StrokeSize = 0.8f;
            canvas.StrokeColor = SoftGold;
            canvas.Alpha = 0.48f;

            for (var i = 0; i < 13; i++)
            {
                var offset = i * 7f;

                var path = new PathF();

                path.MoveTo(
                    -35,
                    25 + offset);

                path.CurveTo(
                    45,
                    5 + offset,
                    85,
                    115 + offset,
                    175,
                    75 + offset);

                path.CurveTo(
                    225,
                    52 + offset,
                    255,
                    5 + offset,
                    330,
                    -15 + offset);

                canvas.DrawPath(path);
            }

            canvas.Alpha = 1f;
        }

        private static void DrawMainRibbon(ICanvas canvas)
        {
            canvas.StrokeColor = Gold;
            canvas.StrokeSize = 2.2f;
            canvas.Alpha = 0.92f;

            var main = new PathF();

            main.MoveTo(
                -30,
                100);

            main.CurveTo(
                55,
                135,
                90,
                50,
                165,
                55);

            main.CurveTo(
                225,
                58,
                245,
                18,
                330,
                -10);

            canvas.DrawPath(main);

            canvas.StrokeColor = SoftGold;
            canvas.StrokeSize = 1.2f;
            canvas.Alpha = 0.70f;

            var second = new PathF();

            second.MoveTo(
                -25,
                118);

            second.CurveTo(
                55,
                155,
                105,
                70,
                170,
                72);

            second.CurveTo(
                235,
                75,
                260,
                25,
                330,
                5);

            canvas.DrawPath(second);

            canvas.Alpha = 1f;
        }
    }
}
