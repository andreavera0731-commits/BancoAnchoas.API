using BancoAnchoas.Application.Common.Interfaces;
using BancoAnchoas.Application.Features.Stock.DTOs;
using ClosedXML.Excel;

namespace BancoAnchoas.Application.Common.Services;

public class ExcelReportGenerator : IReportGenerator
{
    public string ContentType => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    public string FileExtension => "xlsx";

    public byte[] Generate(IReadOnlyList<StockMovementDto> movements)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Movimientos");

        var headers = new[]
        {
            "Id", "Fecha", "Tipo", "Producto", "Sector", "Sector Origen",
            "Solicitante", "Cantidad", "Tipo Ajuste", "Razón", "Notas", "Usuario"
        };

        for (var i = 0; i < headers.Length; i++)
        {
            var cell = sheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
        }

        for (var row = 0; row < movements.Count; row++)
        {
            var m = movements[row];
            var r = row + 2;
            sheet.Cell(r, 1).Value = m.Id;
            sheet.Cell(r, 2).Value = m.CreatedAt;
            sheet.Cell(r, 3).Value = m.Type.ToString();
            sheet.Cell(r, 4).Value = m.ProductName;
            sheet.Cell(r, 5).Value = m.SectorName;
            sheet.Cell(r, 6).Value = m.FromSectorName ?? "";
            sheet.Cell(r, 7).Value = m.RequesterName ?? "";
            sheet.Cell(r, 8).Value = m.Quantity;
            sheet.Cell(r, 9).Value = m.AdjustmentType?.ToString() ?? "";
            sheet.Cell(r, 10).Value = m.Reason?.ToString() ?? "";
            sheet.Cell(r, 11).Value = m.Notes ?? "";
            sheet.Cell(r, 12).Value = m.UserId;
        }

        sheet.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }
}
