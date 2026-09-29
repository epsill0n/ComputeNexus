using System;
using System.IO;
using System.Threading.Tasks;
using ClosedXML.Excel;
using ComputeNexus.Domain.Entities;
using ComputeNexus.Repository.Interface;
using ComputeNexus.Service.Interface;
using Microsoft.EntityFrameworkCore;

namespace ComputeNexus.Service.Implementation;

public class ExcelExportService : IExcelExportService
{
    private readonly IRepository<ComputeInstance> _instanceRepository;
    private readonly IRepository<Payment> _paymentRepository;

    public ExcelExportService(
        IRepository<ComputeInstance> instanceRepository,
        IRepository<Payment> paymentRepository)
    {
        _instanceRepository = instanceRepository;
        _paymentRepository = paymentRepository;
    }

    public async Task<byte[]> ExportBillingReportAsync(string userId, DateTime from, DateTime to)
    {
        var instances = await _instanceRepository.GetAllAsync(
            selector: x => x,
            predicate: x => x.UserId == userId
                            && x.StartedAt.HasValue
                            && x.StartedAt >= from
                            && x.StartedAt <= to,
            include: x => x
                .Include(i => i.User)
                .Include(i => i.HardwareNode)
                .Include(i => i.Template)
        );

        var instancesList = instances.ToList();


        var payments = await _paymentRepository.GetAllAsync(
            selector: x => x,
            predicate: x => x.UserId == userId 
                          && x.PaidAt.HasValue 
                          && x.PaidAt >= from 
                          && x.PaidAt <= to
        );

        var paymentsList = payments.ToList();

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Billing Report");

        // instances summary
        var headers = new[] 
        { 
            "Instance Name", "Status", "GPU Count", "CPU Cores", 
            "Memory (GB)", "Storage (GB)", "Hardware Node", "Template",
            "Hourly Rate ($)", "Started At", "Stopped At", 
            "Total Hours", "Total Cost ($)"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cell(1, i + 1).Value = headers[i];
        }


        var headerRange = ws.Range(1, 1, 1, headers.Length);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#4F46E5");
        headerRange.Style.Font.FontColor = XLColor.White;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        int row = 2;
        decimal totalCostAll = 0;
        decimal totalHoursAll = 0;

        foreach (var instance in instancesList)
        {
            var totalHours = instance.StartedAt.HasValue && instance.StoppedAt.HasValue
                ? (instance.StoppedAt.Value - instance.StartedAt.Value).TotalHours
                : (instance.StartedAt.HasValue 
                    ? (DateTime.UtcNow - instance.StartedAt.Value).TotalHours 
                    : 0);

            var totalCost = (decimal)totalHours * instance.HourlyRate;
            totalCostAll += totalCost;
            totalHoursAll += (decimal)totalHours;

            ws.Cell(row, 1).Value = instance.Name;
            ws.Cell(row, 2).Value = instance.Status.ToString();
            ws.Cell(row, 3).Value = instance.GpuCount;
            ws.Cell(row, 4).Value = instance.CpuCores;
            ws.Cell(row, 5).Value = instance.MemoryGb;
            ws.Cell(row, 6).Value = instance.StorageGb;
            ws.Cell(row, 7).Value = instance.HardwareNode?.Name ?? "Not Assigned";
            ws.Cell(row, 8).Value = instance.Template?.Name ?? "Custom";
            ws.Cell(row, 9).Value = instance.HourlyRate;
            ws.Cell(row, 10).Value = instance.StartedAt?.ToString("yyyy-MM-dd HH:mm") ?? "N/A";
            ws.Cell(row, 11).Value = instance.StoppedAt?.ToString("yyyy-MM-dd HH:mm") ?? "Still Running";
            ws.Cell(row, 12).Value = Math.Round(totalHours, 2);
            ws.Cell(row, 13).Value = Math.Round(totalCost, 2);


            var statusCell = ws.Cell(row, 2);
            switch (instance.Status)
            {
                case Domain.Enums.InstanceStatus.Running:
                    statusCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#22C55E"); // Green
                    break;
                case Domain.Enums.InstanceStatus.Stopped:
                    statusCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#EAB308"); // Yellow
                    break;
                case Domain.Enums.InstanceStatus.Error:
                    statusCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#EF4444"); // Red
                    break;
                default:
                    statusCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#6B7280"); // Gray
                    break;
            }
            statusCell.Style.Font.FontColor = XLColor.White;

            row++;
        }


        if (instancesList.Any())
        {
            ws.Cell(row, 1).Value = "TOTAL";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 12).Value = Math.Round(totalHoursAll, 2);
            ws.Cell(row, 12).Style.Font.Bold = true;
            ws.Cell(row, 13).Value = Math.Round(totalCostAll, 2);
            ws.Cell(row, 13).Style.Font.Bold = true;
            ws.Cell(row, 13).Style.Fill.BackgroundColor = XLColor.FromHtml("#4F46E5");
            ws.Cell(row, 13).Style.Font.FontColor = XLColor.White;
        }


        ws.Column(9).Style.NumberFormat.Format = "$#,##0.00";
        ws.Column(13).Style.NumberFormat.Format = "$#,##0.00";
        ws.Column(10).Style.DateFormat.Format = "yyyy-MM-dd HH:mm";
        ws.Column(11).Style.DateFormat.Format = "yyyy-MM-dd HH:mm";


        ws.Columns().AdjustToContents();
        ws.RangeUsed()?.SetAutoFilter();
        ws.SheetView.FreezeRows(1);

        // payment history
        if (paymentsList.Any())
        {
            var wsPayments = workbook.Worksheets.Add("Payments");
            var paymentHeaders = new[] { "Payment ID", "Amount ($)", "Status", "Method", "Paid At" };

            for (int i = 0; i < paymentHeaders.Length; i++)
            {
                wsPayments.Cell(1, i + 1).Value = paymentHeaders[i];
            }


            var paymentHeaderRange = wsPayments.Range(1, 1, 1, paymentHeaders.Length);
            paymentHeaderRange.Style.Font.Bold = true;
            paymentHeaderRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#10B981");
            paymentHeaderRange.Style.Font.FontColor = XLColor.White;
            paymentHeaderRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            int paymentRow = 2;
            foreach (var payment in paymentsList)
            {
                wsPayments.Cell(paymentRow, 1).Value = payment.Id.ToString();
                wsPayments.Cell(paymentRow, 2).Value = payment.Amount;
                wsPayments.Cell(paymentRow, 3).Value = payment.Status.ToString();
                wsPayments.Cell(paymentRow, 4).Value = payment.Method.ToString();
                wsPayments.Cell(paymentRow, 5).Value = payment.PaidAt?.ToString("yyyy-MM-dd HH:mm") ?? "Pending";
                paymentRow++;
            }

            wsPayments.Column(2).Style.NumberFormat.Format = "$#,##0.00";
            wsPayments.Columns().AdjustToContents();
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}