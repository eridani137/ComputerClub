using System.IO;
using ClosedXML.Excel;
using ComputerClub.Converters;
using ComputerClub.Infrastructure;
using ComputerClub.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace ComputerClub.Services;

public class ShiftReportService(ApplicationDbContext context)
{
    public async Task<byte[]> GenerateReportAsync(
        DateTime shiftStart,
        DateTime shiftEnd,
        CancellationToken ct = default)
    {
        var payments = await context.Payments
            .Include(p => p.Client)
            .Where(p => p.CreatedAt >= shiftStart && p.CreatedAt < shiftEnd)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);

        var topUpCash = payments.Where(p => p.Type == PaymentType.TopUpCash).Sum(p => p.Amount);
        var topUpCard = payments.Where(p => p.Type == PaymentType.TopUpCard).Sum(p => p.Amount);
        var charges = payments.Where(p => p.Type == PaymentType.Charge).Sum(p => p.Amount); // отрицательные
        var refunds = payments.Where(p => p.Type == PaymentType.Refund).Sum(p => p.Amount);
        var totalTopUp = topUpCash + topUpCard;
        var netCash = totalTopUp + charges + refunds;

        using var wb = new XLWorkbook();

        var ws = wb.AddWorksheet("Сводка");

        ws.Cell("A1").Value = $"ОТЧЁТ ПО СМЕНЕ: {App.CurrentUser?.UserName?.ToUpper() ?? "UNKNOWN"}";
        ws.Cell("A1").Style.Font.Bold = true;
        ws.Cell("A1").Style.Font.FontSize = 18;
        ws.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        ws.Cell("A1").Style.Fill.BackgroundColor = XLColor.FromHtml("#2E4057");
        ws.Cell("A1").Style.Font.FontColor = XLColor.White;
        ws.Range("A1:B1").Merge();


        ws.Cell("A2").Value = "Начало смены:";
        ws.Cell("B2").Value = shiftStart.ToLocalTime().ToString("dd.MM.yyyy HH:mm");

        ws.Cell("A3").Value = "Конец смены:";
        ws.Cell("B3").Value = shiftEnd.ToLocalTime().ToString("dd.MM.yyyy HH:mm");

        ws.Cell("A4").Value = "Сформирован:";
        ws.Cell("B4").Value = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");

        ws.Row(5).Height = 8;

        ws.Cell("A6").Value = "Показатель";
        ws.Cell("B6").Value = "Сумма (₽)";
        StyleHeader(ws.Range("A6:B6"));

        var rows = new (string label, decimal value, string? color)[]
        {
            ("Пополнения наличными", topUpCash, "#D6F5E3"),
            ("Пополнения по карте", topUpCard, "#D6EAF8"),
            ("Итого пополнений", totalTopUp, "#A9DFBF"),
            ("Списания за сессии", charges, "#FADBD8"),
            ("Возвраты", refunds, "#FEF9E7"),
            ("Чистая касса", netCash, "#E8F8F5"),
        };

        for (var i = 0; i < rows.Length; i++)
        {
            var value = rows[i];
            var row = i + 7;
            ws.Cell(row, 1).Value = value.label;
            ws.Cell(row, 2).Value = value.value;
            ws.Cell(row, 2).Style.NumberFormat.Format = "#,##0.00";

            if (value.color is not null)
            {
                ws.Range(row, 1, row, 2).Style.Fill.BackgroundColor = XLColor.FromHtml(value.color);
            }

            if (i == rows.Length - 1)
            {
                ws.Range(row, 1, row, 2).Style.Font.Bold = true;
                ws.Range(row, 1, row, 2).Style.Font.FontSize = 12;
            }
        }

        ws.Column("A").Width = 30;
        ws.Column("B").Width = 18;
        ws.Column("B").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

        ws.Range($"A6:B{6 + rows.Length}").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        ws.Range($"A6:B{6 + rows.Length}").Style.Border.InsideBorder = XLBorderStyleValues.Hair;

        
        var wd = wb.AddWorksheet("Детализация");

        wd.Cell("A1").Value = "Дата/Время";
        wd.Cell("B1").Value = "Клиент";
        wd.Cell("C1").Value = "Тип операции";
        wd.Cell("D1").Value = "Сессия №";
        wd.Cell("E1").Value = "Сумма (₽)";
        StyleHeader(wd.Range("A1:E1"));

        for (var i = 0; i < payments.Count; i++)
        {
            var payment = payments[i];
            var row = i + 2;

            wd.Cell(row, 1).Value = payment.CreatedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm:ss");
            wd.Cell(row, 2).Value = payment.Client?.FullName ?? payment.Client?.UserName ?? "—";
            wd.Cell(row, 3).Value = PaymentTypeToStringConverter.Convert(payment.Type);
            wd.Cell(row, 4).Value = payment.SessionId.HasValue ? payment.SessionId.Value.ToString() : "—";
            wd.Cell(row, 5).Value = payment.Amount;
            wd.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";

            var bg = payment.Type switch
            {
                PaymentType.TopUpCash => XLColor.FromHtml("#D6F5E3"),
                PaymentType.TopUpCard => XLColor.FromHtml("#D6EAF8"),
                PaymentType.Charge => XLColor.FromHtml("#FADBD8"),
                PaymentType.Refund => XLColor.FromHtml("#FEF9E7"),
                _ => XLColor.NoColor
            };
            wd.Range(row, 1, row, 5).Style.Fill.BackgroundColor = bg;
        }

        wd.Columns().AdjustToContents();
        if (payments.Count > 0)
        {
            wd.Range($"A1:E{payments.Count + 1}").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            wd.Range($"A1:E{payments.Count + 1}").Style.Border.InsideBorder = XLBorderStyleValues.Hair;
        }

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    private static void StyleHeader(IXLRange range)
    {
        range.Style.Font.Bold = true;
        range.Style.Fill.BackgroundColor = XLColor.FromHtml("#2E4057");
        range.Style.Font.FontColor = XLColor.White;
        range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
    }
}