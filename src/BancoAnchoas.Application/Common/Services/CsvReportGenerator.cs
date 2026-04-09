using System.Globalization;
using System.Text;
using BancoAnchoas.Application.Common.Interfaces;
using BancoAnchoas.Application.Features.Stock.DTOs;

namespace BancoAnchoas.Application.Common.Services;

public class CsvReportGenerator : IReportGenerator
{
    public string ContentType => "text/csv";
    public string FileExtension => "csv";

    public byte[] Generate(IReadOnlyList<StockMovementDto> movements)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Id,Fecha,Tipo,Producto,Sector,SectorOrigen,Solicitante,Cantidad,TipoAjuste,Razón,Notas,Usuario");

        foreach (var m in movements)
        {
            sb.AppendLine(string.Join(",",
                m.Id,
                m.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                m.Type,
                Escape(m.ProductName),
                Escape(m.SectorName),
                Escape(m.FromSectorName ?? ""),
                Escape(m.RequesterName ?? ""),
                m.Quantity,
                m.AdjustmentType?.ToString() ?? "",
                m.Reason?.ToString() ?? "",
                Escape(m.Notes ?? ""),
                m.UserId));
        }

        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    }

    private static string Escape(string value)
    {
        // Sanitize formula injection: prefix with single quote if value starts with =, +, -, @
        if (value.Length > 0 && value[0] is '=' or '+' or '-' or '@')
            value = "'" + value;

        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
