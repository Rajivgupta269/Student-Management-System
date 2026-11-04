using System.Text;

namespace ScholarHub.Services;

public static class CsvWriter
{
    // Escapes quotes/commas/newlines and neutralises spreadsheet formulas (=, +, -, @).
    public static string Cell(object? value)
    {
        var text = value?.ToString() ?? "";
        if (text.Length > 0 && "=+-@".Contains(text[0])) text = "'" + text;
        return "\"" + text.Replace("\"", "\"\"") + "\"";
    }

    public static byte[] Build(IEnumerable<string> header, IEnumerable<IEnumerable<object?>> rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", header.Select(h => Cell(h))));
        foreach (var row in rows) sb.AppendLine(string.Join(",", row.Select(Cell)));
        // UTF-8 with BOM so Excel opens it correctly
        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    }
}
