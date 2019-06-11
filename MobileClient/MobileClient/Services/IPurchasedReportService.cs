using MobileClient.Models;
using System.Collections.Generic;

namespace MobileClient.Services
{
    public interface IPurchasedReportService
    {
        void AddPurchasedReport(PurchasedReportModel model);
        List<PurchasedReportModel> GetPurchasedReports(string userId);
    }
}