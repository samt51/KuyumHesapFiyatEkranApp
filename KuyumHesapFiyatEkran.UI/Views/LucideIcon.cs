namespace KuyumHesapFiyatEkran.UI.Views;

public enum LucideIconKind { House, Star, Calculator, Phone, Info, CircleDollarSign, MapPin, Bell, Menu, ChevronLeft, RefreshCw, TrendingUp, TrendingDown, Globe, Clock3, WhatsApp, ExternalLink, ChevronRight, ShieldCheck, Gem, Sun, Moon, SunMoon, Check }

public sealed class LucideIcon : GraphicsView
{
    private LucideIconKind kind; private double size = 24; private Color iconColor = Color.FromArgb("#17202B"); private bool filled;
    public LucideIcon() { InputTransparent = true; Rebuild(); }
    public LucideIcon(LucideIconKind kind, double size = 24, Color? color = null, bool filled = false) { this.kind=kind; this.size=size; iconColor=color??Color.FromArgb("#17202B"); this.filled=filled; InputTransparent=true; Rebuild(); }
    public LucideIconKind Kind { get=>kind; set { kind=value; Rebuild(); } }
    public double Size { get=>size; set { size=value; Rebuild(); } }
    public Color IconColor { get=>iconColor; set { iconColor=value; Rebuild(); } }
    public bool Filled { get=>filled; set { filled=value; Rebuild(); } }
    private void Rebuild() { WidthRequest=size; HeightRequest=size; HorizontalOptions=LayoutOptions.Center; VerticalOptions=LayoutOptions.Center; Drawable=new IconDrawable(kind,iconColor,filled); Invalidate(); }

    private sealed class IconDrawable(LucideIconKind kind, Color color, bool filled) : IDrawable
    {
        public void Draw(ICanvas c, RectF r)
        {
            var s=Math.Min(r.Width,r.Height)/24f; c.SaveState(); c.Translate(r.Center.X-12*s,r.Center.Y-12*s); c.Scale(s,s); c.StrokeColor=color; c.StrokeSize=1.9f; c.StrokeLineCap=LineCap.Round; c.StrokeLineJoin=LineJoin.Round; c.FillColor=color;
            switch(kind)
            {
                case LucideIconKind.House: Poly(c,[(3,11),(12,3),(21,11),(21,21),(15,21),(15,15),(9,15),(9,21),(3,21)],true); break;
                case LucideIconKind.Star: Star(c); break;
                case LucideIconKind.Calculator: c.DrawRoundedRectangle(3,2,18,20,2); c.DrawRectangle(7,6,10,4); foreach(var p in new[]{(8f,14f),(12f,14f),(16f,14f),(8f,18f),(12f,18f),(16f,18f)}) c.FillCircle(p.Item1,p.Item2,.7f); break;
                case LucideIconKind.Phone: Phone(c); break;
                case LucideIconKind.Info: Circle(c,12,12,10); Line(c,12,10,12,16); c.FillCircle(12,7,.75f); break;
                case LucideIconKind.CircleDollarSign: Circle(c,12,12,10); break;
                case LucideIconKind.MapPin: Pin(c); break;
                case LucideIconKind.Bell: Bell(c); break;
                case LucideIconKind.Menu: Line(c,4,6,20,6); Line(c,4,12,20,12); Line(c,4,18,20,18); break;
                case LucideIconKind.ChevronLeft: Poly(c,[(15,18),(9,12),(15,6)],false); break;
                case LucideIconKind.ChevronRight: Poly(c,[(9,18),(15,12),(9,6)],false); break;
                case LucideIconKind.RefreshCw: c.DrawArc(4,4,16,16,-40,285,false,false); Poly(c,[(20,4),(20,11),(13,11)],false); break;
                case LucideIconKind.TrendingUp: Poly(c,[(3,17),(9,11),(13,15),(21,7)],false); Poly(c,[(15,7),(21,7),(21,13)],false); break;
                case LucideIconKind.TrendingDown: Poly(c,[(3,7),(9,13),(13,9),(21,17)],false); Poly(c,[(15,17),(21,17),(21,11)],false); break;
                case LucideIconKind.Globe: Circle(c,12,12,10); Line(c,2,12,22,12); c.DrawEllipse(7,2,10,20); break;
                case LucideIconKind.Clock3: Circle(c,12,12,10); Line(c,12,6,12,12); Line(c,12,12,16,12); break;
                case LucideIconKind.WhatsApp: Bubble(c); break;
                case LucideIconKind.ExternalLink: c.DrawRoundedRectangle(3,6,15,15,2); Line(c,10,14,21,3); Poly(c,[(15,3),(21,3),(21,9)],false); break;
                case LucideIconKind.ShieldCheck: Shield(c); Poly(c,[(8.5f,12),(11,14.5f),(16,9.5f)],false); break;
                case LucideIconKind.Gem: Poly(c,[(6,3),(18,3),(22,9),(12,21),(2,9)],true); Line(c,2,9,22,9); Poly(c,[(6,3),(9,9),(12,3),(15,9),(18,3)],false); Line(c,9,9,12,21); Line(c,15,9,12,21); break;
                case LucideIconKind.Sun: Circle(c,12,12,4); foreach(var p in new[]{(12f,2f,12f,5f),(12f,19f,12f,22f),(2f,12f,5f,12f),(19f,12f,22f,12f),(4.9f,4.9f,7f,7f),(17f,17f,19.1f,19.1f),(19.1f,4.9f,17f,7f),(7f,17f,4.9f,19.1f)}) Line(c,p.Item1,p.Item2,p.Item3,p.Item4); break;
                case LucideIconKind.Moon: var moon=new PathF(); moon.MoveTo(20.5f,14.2f); moon.CurveTo(18.8f,15,17,15.2f,15.4f,14.6f); moon.CurveTo(11.8f,13.4f,9.9f,9.5f,11.1f,5.9f); moon.CurveTo(11.6f,4.4f,12.5f,3.1f,13.8f,2.2f); moon.CurveTo(8.2f,1.2f,3,5.5f,3,11.2f); moon.CurveTo(3,17.1f,7.8f,21.9f,13.7f,21.9f); moon.CurveTo(17.2f,21.9f,20.3f,20.2f,22,17.4f); moon.CurveTo(22.6f,16.4f,21.6f,13.7f,20.5f,14.2f); moon.Close(); c.DrawPath(moon); break;
                case LucideIconKind.SunMoon: Circle(c,8,8,3); Line(c,8,1.5f,8,3); Line(c,1.5f,8,3,8); Line(c,12.5f,3.5f,11.5f,4.5f); var sm=new PathF();sm.MoveTo(20.5f,15);sm.CurveTo(17,16,13.5f,13,14,9.5f);sm.CurveTo(10.5f,11,10,16,12.5f,19);sm.CurveTo(15,22,19.5f,21,22,18);sm.CurveTo(22.8f,17,21.6f,14.7f,20.5f,15);sm.Close();c.DrawPath(sm);break;
                case LucideIconKind.Check: Poly(c,[(4,12),(9,17),(20,6)],false); break;
            }
            c.RestoreState();
        }
        private void Star(ICanvas c){var pts=new List<(float,float)>();for(var i=0;i<10;i++){var a=-MathF.PI/2+i*MathF.PI/5;var radius=i%2==0?9.5f:4.2f;pts.Add((12+MathF.Cos(a)*radius,12+MathF.Sin(a)*radius));}Poly(c,pts.ToArray(),true,filled);}
        private static void Phone(ICanvas c){var p=new PathF();p.MoveTo(5,3);p.LineTo(8,3);p.CurveTo(9,3,9.5f,4,9.7f,5);p.LineTo(10.2f,7.2f);p.CurveTo(10.4f,8,10,8.6f,9.5f,9.1f);p.LineTo(8.2f,10.3f);p.CurveTo(9.7f,13.2f,11.8f,15.3f,14.7f,16.8f);p.LineTo(15.9f,15.5f);p.CurveTo(16.4f,15,17,14.7f,17.8f,14.9f);p.LineTo(20,15.4f);p.CurveTo(21,15.6f,21.8f,16.2f,21.8f,17.2f);p.LineTo(21.8f,20);p.CurveTo(21.8f,21.2f,20.8f,22,19.7f,22);p.CurveTo(10.5f,21.2f,2.8f,13.5f,2,4.3f);p.CurveTo(2,3.5f,3,3,5,3);c.DrawPath(p);}
        private static void Pin(ICanvas c){var p=new PathF();p.MoveTo(20,10);p.CurveTo(20,15,12,22,12,22);p.CurveTo(12,22,4,15,4,10);p.CurveTo(4,5.6f,7.6f,2,12,2);p.CurveTo(16.4f,2,20,5.6f,20,10);p.Close();c.DrawPath(p);Circle(c,12,10,2.5f);}
        private static void Bell(ICanvas c){var p=new PathF();p.MoveTo(18,8);p.CurveTo(18,4.7f,15.3f,2,12,2);p.CurveTo(8.7f,2,6,4.7f,6,8);p.CurveTo(6,15,3,15,3,17);p.LineTo(21,17);p.CurveTo(21,15,18,15,18,8);c.DrawPath(p);Line(c,10,21,14,21);}
        private static void Bubble(ICanvas c){var p=new PathF();p.MoveTo(21,11.5f);p.CurveTo(21,16.2f,17.2f,20,12.5f,20);p.CurveTo(11,20,9.6f,19.6f,8.4f,19);p.LineTo(3,20);p.LineTo(4.1f,14.9f);p.CurveTo(3.6f,13.8f,3.3f,12.7f,3.3f,11.5f);p.CurveTo(3.3f,6.8f,7.1f,3,11.8f,3);p.CurveTo(16.9f,3,21,6.8f,21,11.5f);p.Close();c.DrawPath(p);Poly(c,[(8,8),(9.2f,10.8f),(12.2f,13.8f),(15.5f,15)],false);}
        private static void Shield(ICanvas c){var p=new PathF();p.MoveTo(12,2);p.LineTo(20,5);p.LineTo(20,12.5f);p.CurveTo(20,17,16.7f,20,12,22);p.CurveTo(7.3f,20,4,17,4,12.5f);p.LineTo(4,5);p.Close();c.DrawPath(p);}
        private static void Circle(ICanvas c,float x,float y,float r)=>c.DrawEllipse(x-r,y-r,r*2,r*2);
        private static void Line(ICanvas c,float x1,float y1,float x2,float y2)=>c.DrawLine(x1,y1,x2,y2);
        private static void Poly(ICanvas c,(float x,float y)[] pts,bool close,bool fill=false){var p=new PathF();p.MoveTo(pts[0].x,pts[0].y);foreach(var a in pts.Skip(1))p.LineTo(a.x,a.y);if(close)p.Close();if(fill)c.FillPath(p);c.DrawPath(p);}
    }
}
