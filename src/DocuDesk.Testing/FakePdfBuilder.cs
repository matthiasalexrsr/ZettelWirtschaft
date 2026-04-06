using System.Text;

namespace DocuDesk.Testing;

public static class FakePdfBuilder
{
    public static byte[] CreateWithPageCount(int pageCount)
    {
        if (pageCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageCount), pageCount, "Page count must be positive.");
        }

        var builder = new StringBuilder();
        builder.AppendLine("%PDF-1.4");
        builder.AppendLine("1 0 obj << /Type /Catalog /Pages 2 0 R >> endobj");
        builder.AppendLine($"2 0 obj << /Type /Pages /Count {pageCount} >> endobj");

        for (var index = 0; index < pageCount; index++)
        {
            builder.AppendLine($"{index + 3} 0 obj << /Type /Page /Parent 2 0 R >> endobj");
        }

        builder.AppendLine("%%EOF");
        return Encoding.ASCII.GetBytes(builder.ToString());
    }
}
