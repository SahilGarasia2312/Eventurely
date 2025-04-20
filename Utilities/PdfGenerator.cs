using Eventurely.Web.Models;
using Eventurely.Models.ViewModels; // Add this for BadgeViewModel
using System.IO;

namespace Eventurely.Web.Utilities
{
    public class PdfGenerator
    {
        public byte[] GenerateBadgePdf(BadgeViewModel badge)
        {
            using var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write($"Badge for {badge.UserName} - {badge.EventTitle}");
            writer.Flush();
            return stream.ToArray();
        }
    }
}