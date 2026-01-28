using System.Net;
using System.Text.RegularExpressions;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.ApplicationModel;
using UIKit;
using whitespc.Models;
using whitespc.Services;

namespace whitespc.Platforms.MacCatalyst;

public sealed class PdfExportService : IPdfExportService
{
    public async Task<byte[]> ExportToPdfAsync(IReadOnlyList<JournalEntry> entries, string title)
    {
        // ✅ Safe: can run anywhere (no UIKit here)
        var safeTitle = title ?? "";
        var prepared = entries
            .Select(e => new PreparedEntry(
                e.EntryDate,                 // DateOnly ✅
                e.Title ?? "",
                HtmlToPlainText(e.Content ?? "")
            ))
            .ToList();

        // ✅ REQUIRED: UIKit drawing must run on UI thread
        return await MainThread.InvokeOnMainThreadAsync(() =>
        {
            var pageRect = new CGRect(0, 0, 595, 842); // A4
            var format = new UIGraphicsPdfRendererFormat();
            var renderer = new UIGraphicsPdfRenderer(pageRect, format);

            using var data = renderer.CreatePdf(ctx =>
            {
                ctx.BeginPage();

                var margin = 40;
                nfloat y = margin;
                var contentWidth = pageRect.Width - margin * 2;

                y = DrawLine(safeTitle, UIFont.BoldSystemFontOfSize(18), margin, y, contentWidth);
                y += 10;

                foreach (var e in prepared)
                {
                    var header = $"{e.Date:yyyy-MM-dd}  •  {e.Title}";
                    y = DrawLine(header, UIFont.BoldSystemFontOfSize(12), margin, y, contentWidth);

                    y = DrawParagraph(e.Body, UIFont.SystemFontOfSize(11), margin, y, contentWidth);
                    y += 14;

                    if (y > pageRect.Height - margin)
                    {
                        ctx.BeginPage();
                        y = margin;
                    }
                }
            });

            return data.ToArray();
        });
    }

    private sealed record PreparedEntry(DateOnly Date, string Title, string Body);

    private static nfloat DrawLine(string text, UIFont font, nfloat x, nfloat y, nfloat width)
    {
        var attr = new UIStringAttributes
        {
            Font = font,
            ForegroundColor = UIColor.Black
        };

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
        var attr = new UIStringAttributes
        {
            Font = font,
            ForegroundColor = UIColor.Black
        };

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