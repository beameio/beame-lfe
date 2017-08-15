using System.Collections.Generic;
using LFE.DataTokens;
using System;

namespace LFE.Application.Services.Interfaces
{
    public interface IPayoutServices : IDisposable
    {
        bool RunBatchAgain(int executionId, out string error);
        bool RunSinglePaymentAgain(int payoutId, out string error);
        bool ExecuteMonthlyPayout(int year, int month, List<BasePayoutSelectionToken> include, out int execId, out string error);
        bool ExecuteSinglePayment(PayoutStatmentDTO token, out string error);
        PayoutExecutionDTO GetPayoutExecutionDto(int year, int month);
        SaleSummaryReportDTO GetMonthlyPayoutReport(int year, int month, int? userId, short? currencyId);

        List<PayoutExecutionDTO> GetPayoutExecutions(int? executionId);
        List<PayoutStatmentDTO> GetPayoutStatments(int executionId);

        List<PayoutCurrencySummaryDTO> GetPayoutCurrencySummaryRows(int year, int month, int? userId, short? currencyId,int executionId);
    }
}
