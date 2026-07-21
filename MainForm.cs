using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using CheckCpuV.Services;

namespace CheckCpuV;

public sealed class MainForm : Form
{
    private static readonly Color HeaderTop = Color.FromArgb(0x4A, 0x55, 0x60);
    private static readonly Color HeaderBottom = Color.FromArgb(0x3A, 0x43, 0x4C);
    private static readonly Color BannerOk = Color.FromArgb(0x2F, 0x80, 0xED);
    private static readonly Color BannerWarn = Color.FromArgb(0xF0, 0x8C, 0x00);
    private static readonly Color BannerBad = Color.FromArgb(0xC6, 0x28, 0x28);
    private static readonly Color OkGreen = Color.FromArgb(0x2E, 0xAA, 0x4A);
    private static readonly Color BadRed = Color.FromArgb(0xE5, 0x39, 0x35);
    private static readonly Color CardBorder = Color.FromArgb(0xD0, 0xD0, 0xD0);
    private static readonly Color Muted = Color.FromArgb(0x66, 0x66, 0x66);
    private static readonly Color PageBg = Color.FromArgb(0xF0, 0xF0, 0xF0);

    private readonly Font _titleFont;
    private readonly Font _versionFont;
    private readonly Font _bodyFont;
    private readonly Font _cardTitleFont;
    private readonly Font _cardValueFont;
    private readonly Font _featureFont;
    private readonly Font _logoCpuFont;
    private readonly Font _logoVFont;
    private readonly Font _footerFont;

    private CpuInfo _info = new();
    private RectangleF _footerLinkRect;
    private bool _footerHover;

    private const string FooterText = "© NeetheCheeBao";
    private const string FooterUrl = "https://github.com/NeetheCheeBao/Check-CPU-V";

    public MainForm()
    {
        string fontName = PickFont("Microsoft YaHei UI", "Microsoft YaHei", "微软雅黑", "SimSun", "Tahoma");

        _titleFont = new Font(fontName, 14f, FontStyle.Bold, GraphicsUnit.Point);
        _versionFont = new Font(fontName, 9f, FontStyle.Regular, GraphicsUnit.Point);
        _bodyFont = new Font(fontName, 9.5f, FontStyle.Regular, GraphicsUnit.Point);
        _cardTitleFont = new Font(fontName, 9.5f, FontStyle.Regular, GraphicsUnit.Point);
        _cardValueFont = new Font(fontName, 22f, FontStyle.Bold, GraphicsUnit.Point);
        _featureFont = new Font(fontName, 9.5f, FontStyle.Regular, GraphicsUnit.Point);
        _logoCpuFont = new Font(fontName, 8f, FontStyle.Bold, GraphicsUnit.Point);
        _logoVFont = new Font(fontName, 18f, FontStyle.Bold, GraphicsUnit.Point);
        _footerFont = new Font(fontName, 8f, FontStyle.Regular, GraphicsUnit.Point);

        Text = "Check CPU-V";
        ClientSize = new Size(420, 500);
        MinimumSize = new Size(400, 480);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        DoubleBuffered = true;
        BackColor = PageBg;
        Font = _bodyFont;

        try
        {
            var exeIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            if (exeIcon != null)
                Icon = exeIcon;
        }
        catch
        {
        }

        try
        {
            _info = CpuDetector.Detect();
        }
        catch (Exception ex)
        {
            _info = new CpuInfo
            {
                ProcessorName = "检测失败",
                StatusMessage = "检测失败: " + ex.Message,
                StatusOk = false
            };
        }

        Resize += (_, __) => Invalidate();
        Paint += OnPaint;
        MouseMove += OnMouseMove;
        MouseLeave += (_, __) =>
        {
            if (!_footerHover) return;
            _footerHover = false;
            Cursor = Cursors.Default;
            Invalidate(Rectangle.Round(_footerLinkRect));
        };
        MouseClick += OnMouseClick;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _titleFont.Dispose();
            _versionFont.Dispose();
            _bodyFont.Dispose();
            _cardTitleFont.Dispose();
            _cardValueFont.Dispose();
            _featureFont.Dispose();
            _logoCpuFont.Dispose();
            _logoVFont.Dispose();
            _footerFont.Dispose();
        }
        base.Dispose(disposing);
    }

    private static string PickFont(params string[] names)
    {
        using var collection = new InstalledFontCollection();
        foreach (var name in names)
        {
            foreach (var f in collection.Families)
            {
                if (string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase))
                    return f.Name;
            }
        }
        return SystemFonts.MessageBoxFont.FontFamily.Name;
    }

    private void OnPaint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

        int w = ClientSize.Width;
        int h = ClientSize.Height;

        int headerH = 88;
        using (var brush = new LinearGradientBrush(
                   new Rectangle(0, 0, w, headerH), HeaderTop, HeaderBottom, LinearGradientMode.Vertical))
        {
            g.FillRectangle(brush, 0, 0, w, headerH);
        }

        int logoSize = 56;
        int logoX = 16;
        int logoY = (headerH - logoSize) / 2;
        var logoRect = new Rectangle(logoX, logoY, logoSize, logoSize);
        using (var logoBrush = new LinearGradientBrush(logoRect,
                   Color.FromArgb(0x5B, 0x9B, 0xD5), Color.FromArgb(0x2E, 0x6F, 0xAE), 45f))
        {
            g.FillEllipse(logoBrush, logoRect);
        }
        using (var pen = new Pen(Color.FromArgb(0x8E, 0xC5, 0xF0), 1.5f))
        {
            g.DrawEllipse(pen, logoX + 2, logoY + 2, logoSize - 5, logoSize - 5);
        }

        DrawCenteredText(g, "CPU", _logoCpuFont, Color.White,
            new Rectangle(logoX, logoY + 8, logoSize, 16));
        DrawCenteredText(g, "V", _logoVFont, Color.White,
            new Rectangle(logoX, logoY + 18, logoSize, 32));

        int textLeft = logoX + logoSize + 12;
        using (var white = new SolidBrush(Color.White))
        using (var gray = new SolidBrush(Color.FromArgb(0xB8, 0xC0, 0xC8)))
        using (var sub = new SolidBrush(Color.FromArgb(0xD0, 0xD6, 0xDC)))
        {
            g.DrawString("Check CPU-V", _titleFont, white, textLeft, logoY + 6);
            var titleSize = g.MeasureString("Check CPU-V", _titleFont);
            g.DrawString("v1.0.0", _versionFont, gray, textLeft + titleSize.Width + 2, logoY + 12);

            var nameRect = new RectangleF(textLeft, logoY + 34, w - textLeft - 12, 36);
            g.DrawString(_info.ProcessorName ?? "", _bodyFont, sub, nameRect);
        }

        int bannerTop = headerH + 12;
        int bannerH = 38;
        var bannerRect = new Rectangle(12, bannerTop, w - 24, bannerH);
        Color bannerColor = _info.VirtSupported
            ? (_info.VirtEnabled ? BannerOk : BannerWarn)
            : BannerBad;
        using (var path = RoundedRect(bannerRect, 2))
        using (var brush = new SolidBrush(bannerColor))
        {
            g.FillPath(brush, path);
        }
        DrawCenteredText(g, _info.StatusMessage ?? "", _bodyFont, Color.White, bannerRect);

        int cardsTop = bannerTop + bannerH + 10;
        int footerH = 28;
        int featuresH = 78;
        int cardsBottom = h - footerH - featuresH - 4;
        int cardsH = Math.Max(160, cardsBottom - cardsTop);
        int gap = 10;
        int cardW = (w - 24 - gap) / 2;
        int cardH = (cardsH - gap) / 2;

        string supportTitle = _info.VirtBrandName + " 支持状态";
        string enabledTitle = _info.VirtBrandName + " 启用状态";

        DrawInfoCard(g, new Rectangle(12, cardsTop, cardW, cardH),
            "处理器架构", _info.ProcessorArch, isIcon: false, ok: false);
        DrawInfoCard(g, new Rectangle(12 + cardW + gap, cardsTop, cardW, cardH),
            "操作系统架构", _info.OsArch, isIcon: false, ok: false);
        DrawInfoCard(g, new Rectangle(12, cardsTop + cardH + gap, cardW, cardH),
            supportTitle, null, isIcon: true, ok: _info.VirtSupported);
        DrawInfoCard(g, new Rectangle(12 + cardW + gap, cardsTop + cardH + gap, cardW, cardH),
            enabledTitle, null, isIcon: true, ok: _info.VirtEnabled);

        int featTop = cardsTop + cardsH + 8;
        DrawFeature(g, 20, featTop, _info.DepEnabled, "数据执行保护 (DEP)");
        DrawFeature(g, 20, featTop + 24, _info.SlatSupported, "二级地址转换 (SLAT)");
        DrawFeature(g, 20, featTop + 48, _info.VmMonitorMode, "虚拟机监视器模式扩展");

        using (var footerBrush = new SolidBrush(Color.FromArgb(0xE8, 0xE8, 0xE8)))
        {
            g.FillRectangle(footerBrush, 0, h - footerH, w, footerH);
        }

        var footSize = g.MeasureString(FooterText, _footerFont);
        float footX = w - footSize.Width - 12;
        float footY = h - footerH + (footerH - footSize.Height) / 2f;
        _footerLinkRect = new RectangleF(footX, footY, footSize.Width, footSize.Height);

        Color linkColor = _footerHover
            ? Color.FromArgb(0x2F, 0x80, 0xED)
            : Color.FromArgb(0x88, 0x88, 0x88);
        using (var footBrush = new SolidBrush(linkColor))
        using (var underlinePen = new Pen(linkColor, 1f))
        {
            g.DrawString(FooterText, _footerFont, footBrush, footX, footY);
            if (_footerHover)
            {
                float uy = footY + footSize.Height - 2;
                g.DrawLine(underlinePen, footX + 2, uy, footX + footSize.Width - 4, uy);
            }
        }
    }

    private void OnMouseMove(object? sender, MouseEventArgs e)
    {
        bool hover = _footerLinkRect.Contains(e.Location);
        if (hover == _footerHover) return;
        _footerHover = hover;
        Cursor = hover ? Cursors.Hand : Cursors.Default;
        Invalidate();
    }

    private void OnMouseClick(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left) return;
        if (!_footerLinkRect.Contains(e.Location)) return;

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = FooterUrl,
                UseShellExecute = true
            });
        }
        catch
        {
        }
    }

    private void DrawInfoCard(Graphics g, Rectangle rect, string title, string? value, bool isIcon, bool ok)
    {
        using (var path = RoundedRect(rect, 6))
        using (var bg = new SolidBrush(Color.White))
        using (var border = new Pen(CardBorder))
        {
            g.FillPath(bg, path);
            g.DrawPath(border, path);
        }

        var titleRect = new Rectangle(rect.X + 8, rect.Y + 12, rect.Width - 16, 28);
        DrawCenteredText(g, title, _cardTitleFont, Muted, titleRect);

        if (isIcon)
        {
            int size = 44;
            int cx = rect.X + rect.Width / 2;
            int cy = rect.Y + rect.Height / 2 + 10;
            DrawStatusIcon(g, cx, cy, size, ok);
        }
        else
        {
            var valueRect = new Rectangle(rect.X + 8, rect.Y + 40, rect.Width - 16, rect.Height - 52);
            DrawCenteredText(g, value ?? "—", _cardValueFont, Color.FromArgb(0x22, 0x22, 0x22), valueRect);
        }
    }

    private void DrawStatusIcon(Graphics g, int cx, int cy, int size, bool ok)
    {
        Color c = ok ? OkGreen : BadRed;
        var rect = new Rectangle(cx - size / 2, cy - size / 2, size, size);
        using (var pen = new Pen(c, 3.2f) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round })
        {
            g.DrawEllipse(pen, rect);

            if (ok)
            {
                PointF[] pts =
                {
                    new PointF(cx - size * 0.22f, cy + size * 0.02f),
                    new PointF(cx - size * 0.05f, cy + size * 0.20f),
                    new PointF(cx + size * 0.26f, cy - size * 0.18f)
                };
                g.DrawLines(pen, pts);
            }
            else
            {
                float o = size * 0.18f;
                g.DrawLine(pen, cx - o, cy - o, cx + o, cy + o);
                g.DrawLine(pen, cx + o, cy - o, cx - o, cy + o);
            }
        }
    }

    private void DrawFeature(Graphics g, int x, int y, bool ok, string text)
    {
        int iconSize = 16;
        int iconTop = y + 1;
        DrawFeatureMark(g, x, iconTop, iconSize, ok);

        using (var textBrush = new SolidBrush(Color.FromArgb(0x33, 0x33, 0x33)))
        {
            float textY = y + (iconSize + 2 - _featureFont.GetHeight(g)) / 2f;
            g.DrawString(text, _featureFont, textBrush, x + iconSize + 10, textY);
        }
    }

    private static void DrawFeatureMark(Graphics g, int x, int y, int size, bool ok)
    {
        Color c = ok ? OkGreen : BadRed;
        float thickness = Math.Max(2.6f, size * 0.22f);

        using var pen = new Pen(c, thickness)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
            LineJoin = LineJoin.Round
        };

        if (ok)
        {
            var p1 = new PointF(x + size * 0.08f, y + size * 0.52f);
            var p2 = new PointF(x + size * 0.38f, y + size * 0.82f);
            var p3 = new PointF(x + size * 0.95f, y + size * 0.18f);
            g.DrawLines(pen, new[] { p1, p2, p3 });
        }
        else
        {
            float pad = size * 0.12f;
            g.DrawLine(pen, x + pad, y + pad, x + size - pad, y + size - pad);
            g.DrawLine(pen, x + size - pad, y + pad, x + pad, y + size - pad);
        }
    }

    private static void DrawCenteredText(Graphics g, string text, Font font, Color color, Rectangle bounds)
    {
        if (string.IsNullOrEmpty(text)) return;

        using var brush = new SolidBrush(color);
        var format = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center,
            FormatFlags = StringFormatFlags.NoClip,
            Trimming = StringTrimming.EllipsisCharacter
        };
        g.DrawString(text, font, brush, bounds, format);
    }

    private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
    {
        int d = radius * 2;
        var path = new GraphicsPath();
        if (radius <= 0)
        {
            path.AddRectangle(bounds);
            return path;
        }
        path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
        path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
        path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}
