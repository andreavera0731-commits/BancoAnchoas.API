using BancoAnchoas.Application.Common.Interfaces;
using BancoAnchoas.Application.Features.Stock.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BancoAnchoas.Application.Common.Services;

public class PdfReportGenerator : IReportGenerator
{
    public string ContentType => "application/pdf";
    public string FileExtension => "pdf";

    public byte[] Generate(IReadOnlyList<StockMovementDto> movements)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(8));

                page.Header().Text("Reporte de Movimientos de Stock")
                    .SemiBold().FontSize(16).FontColor(Colors.Blue.Darken2);

                page.Content().PaddingVertical(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(30);   // Id
                        columns.ConstantColumn(100);  // Fecha
                        columns.ConstantColumn(65);   // Tipo
                        columns.RelativeColumn(2);    // Producto
                        columns.RelativeColumn(1.5f); // Sector
                        columns.RelativeColumn(1.5f); // Solicitante
                        columns.ConstantColumn(50);   // Cantidad
                        columns.RelativeColumn(2);    // Notas
                    });

                    table.Header(header =>
                    {
                        var headerStyle = TextStyle.Default.SemiBold().FontColor(Colors.White);

                        foreach (var h in new[] { "Id", "Fecha", "Tipo", "Producto", "Sector", "Solicitante", "Cant.", "Notas" })
                        {
                            header.Cell().Background(Colors.Blue.Darken2).Padding(4).Text(h).Style(headerStyle);
                        }
                    });

                    foreach (var m in movements)
                    {
                        var cells = new[]
                        {
                            m.Id.ToString(),
                            m.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                            m.Type.ToString(),
                            m.ProductName,
                            m.SectorName,
                            m.RequesterName ?? "—",
                            m.Quantity.ToString(),
                            m.Notes ?? ""
                        };

                        foreach (var cell in cells)
                        {
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2)
                                .Padding(3).Text(cell);
                        }
                    }
                });

                page.Footer().AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Generado: ");
                        x.Span(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm UTC"));
                        x.Span(" — Página ");
                        x.CurrentPageNumber();
                        x.Span(" de ");
                        x.TotalPages();
                    });
            });
        });

        return document.GeneratePdf();
    }
}
