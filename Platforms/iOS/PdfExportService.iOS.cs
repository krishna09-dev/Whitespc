using System.Net;
using System.Text.RegularExpressions;
using CoreGraphics;
using Foundation;
using UIKit;
using whitespc.Models;
using whitespc.Services;

namespace whitespc.Platforms.iOS;

public sealed class PdfExportService : IPdfExportService
{
    public byte[] ExportToPdf(IReadOnlyList<JournalEntry> entries, string title)
    {
        // A4 size in points (72 dpi): 595 x 842
        var pageRect = new CGRect(0, 0, 595, 842);

        var format = new UIGraphicsPdfRendererFormat();
        var renderer = new UIGraphicsPdfRenderer(pageRect, format);

        using var data = renderer.CreatePdf(ctx =>
        {
            ctx.BeginPage();

            var margin = 40;
            nfloat y = margin;
            var contentWidth = pageRect.Width - margin * 2;

            y = DrawLine(title, UIFont.BoldSystemFontOfSize(18), margin, y, contentWidth);
            y += 10;

            foreach (var e in entries)
            {
                var header = $"{e.EntryDate:yyyy-MM-dd}  •  {e.Title}";
                y = DrawLine(header, UIFont.BoldSystemFontOfSize(12), margin, y, contentWidth);

                var plain = HtmlToPlainText(e.Content ?? "");
                y = DrawParagraph(plain, UIFont.SystemFontOfSize(11), margin, y, contentWidth);

                y += 14;

                if (y > pageRect.Height - margin)
                {
                    ctx.BeginPage();
                    y = margin;
                }
            }
        });

        return data.ToArray();
    }

    private static nfloat DrawLine(string text, UIFont font, nfloat x, nfloat y, nfloat width)
    {
        var attr = new UIStringAttributes { Font = font, ForegroundColor = UIColor.Black };

        var ns = new NSString(text ?? "");
        var bounding = ns.GetBoundingRect(
            new CGSize(width, nfloat.MaxValue),
            NSStringDrawingOptions.UsesLineFragmentOrigin,
            attr,
            null);

        ns.DrawString(new CGRect(x, y, width, bounding.Size.Height), attr);
        return y + bounding.Size.Height;
    }

    private static nfloat DrawParagraph(string text, UIFont font, nfloat x, nfloat y, nfloat width)
    {
        var attr = new UIStringAttributes { Font = font, ForegroundColor = UIColor.Black };

        var ns = new NSString(text ?? "");
        var bounding = ns.GetBoundingRect(
            new CGSize(width, nfloat.MaxValue),
            NSStringDrawingOptions.UsesLineFragmentOrigin,
            attr,
            null);

        ns.DrawString(new CGRect(x, y, width, bounding.Size.Height), attr);
        return y + bounding.Size.Height;
    }

    private static string HtmlToPlainText(string html)
    {
        var decoded = WebUtility.HtmlDecode(html);
        decoded = Regex.Replace(decoded, "<.*?>", string.Empty);
        decoded = Regex.Replace(decoded, @"\s+\n", "\n");
        decoded = Regex.Replace(decoded, @"\n\s+", "\n");
        decoded = Regex.Replace(decoded, @"[ \t]{2,}", " ");
        return decoded.Trim();
    }
}