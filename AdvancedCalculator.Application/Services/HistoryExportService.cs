using System.Text;
using AdvancedCalculator.Core.Models;

namespace AdvancedCalculator.Application.Services;

public class HistoryExportService
{
    public static string ExportToCsv(IEnumerable<CalculationRecord> records)
    {
        var sb = new StringBuilder();
        sb.AppendLine("ID,Date (UTC),Mode,Expression,Result,IsPinned");
        foreach (var r in records)
        {
            string cleanExp = r.Expression.Replace("\"", "\"\"");
            string cleanRes = r.Result.Replace("\"", "\"\"");
            sb.AppendLine($"{r.Id},\"{r.CreatedAtUtc:yyyy-MM-dd HH:mm:ss}\",\"{r.Mode}\",\"{cleanExp}\",\"{cleanRes}\",{r.IsPinned}");
        }
        return sb.ToString();
    }

    public static string ExportToText(IEnumerable<CalculationRecord> records)
    {
        var sb = new StringBuilder();
        sb.AppendLine("================ CALCULATOR HISTORY ================");
        sb.AppendLine($"Export Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine("====================================================");
        sb.AppendLine();

        foreach (var r in records)
        {
            sb.AppendLine($"[{r.CreatedAtUtc:yyyy-MM-dd HH:mm:ss}] [{r.Mode}] {(r.IsPinned ? "[★ PINNED]" : "")}");
            sb.AppendLine($"  Expression : {r.Expression}");
            sb.AppendLine($"  Result     : {r.Result}");
            sb.AppendLine(new string('-', 40));
        }
        return sb.ToString();
    }
}
