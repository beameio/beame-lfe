using System.Collections.Generic;
using System.Web.Mvc;
using LFE.Application.Services.Base;
using LFE.Application.Services.Interfaces;
using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Dto.Mapper.DtoMappers;
using LFE.Dto.Mapper.EntityMapper;
using LFE.Model;
using System;
using System.Linq;
using LFE.Application.Services.Helper;
using PayPal.PayPalAPIInterfaceService.Model;

namespace LFE.Application.Services
{
    public class PayoutServices : ServiceBase, IPayoutServices
    {

        private readonly IPaypalPaymentServices _paypalPaymentServices;

        public PayoutServices()
        {
            _paypalPaymentServices = DependencyResolver.Current.GetService<IPaypalPaymentServices>();
        }

        #region private helpers
        private MassPayRequestItemType CreateMassPayRequestItem(PayoutStatmentDTO token, string note, out string error)
        {
            error = string.Empty;

            if (token.PayoutType == null)
            {
                error = "Payout option not defined for user";
                UpdateUserPayoutRecord(token.PayoutId, BillingEnums.ePayoutStatuses.FAILED, error);
                return null;
            }

            if (token.PayoutType != BillingEnums.ePayoutTypes.PAYPAL)
            {
                error = "Payout Paypal option not defined for user";
                UpdateUserPayoutRecord(token.PayoutId, BillingEnums.ePayoutStatuses.FAILED, error);
                return null;
            }

            if (String.IsNullOrEmpty(token.PaypalEmail))
            {
                error = "Paypal Email not defined for user";
                UpdateUserPayoutRecord(token.PayoutId, BillingEnums.ePayoutStatuses.FAILED, error);
                return null;
            }


            if (token.Amount <= 0)
            {
                error = "Amount should be grater as zero";
                UpdateUserPayoutRecord(token.PayoutId, BillingEnums.ePayoutStatuses.FAILED, error);
                return null;
            }

            var item = new MassPayRequestItemType
            {
                ReceiverEmail  = token.PaypalEmail
                ,UniqueId      = token.PayoutId.ToString()
                ,Note          = note
                ,Amount        = new BasicAmountType
                                    {
                                        value       = token.Amount.FormatMoney(2).ToString()
                                        ,currencyID = token.Currency.Currency2PaypalCurrencyCode()
                                    }
            };

            return item;
        }
        private BaseUserInfoDTO GetUser(int? userId)
        {
            if (userId == null) return new BaseUserInfoDTO {UserId = -1, FullName = string.Empty};

            var entity = UserRepository.GetById((int)userId);

            return entity == null ? new BaseUserInfoDTO { UserId = -1, FullName = string.Empty } : entity.Entity2BaseUserInfoDto();
        }

        private BillingEnums.ePayoutStatuses GetPayoutStatus(int? userId, int executionId)
        {
            try
            {
                if(executionId < 0 || userId == null) return BillingEnums.ePayoutStatuses.WAIT; 

                var entity = UserPayoutStatmentsRepository.Get(x => x.UserId == (int)userId && x.ExecutionId == executionId);

                return entity == null ? BillingEnums.ePayoutStatuses.WAIT : Utils.ParseEnum<BillingEnums.ePayoutStatuses>(entity.StatusId);
            }
            catch (Exception)
            {
                return BillingEnums.ePayoutStatuses.Unknown;
            }
        }

        private void UpdateUserPayoutRecord(int payoutId,BillingEnums.ePayoutStatuses? status, string message,string payKey = null)
        {
            try
            {
                var entity = UserPayoutStatmentsRepository.GetById(payoutId);

                if (entity == null) return;

                if (status != null) entity.StatusId = (byte) status;

                if (!string.IsNullOrEmpty(message)) entity.ErrorMessage = message;

                if (status == BillingEnums.ePayoutStatuses.COMPLETED && string.IsNullOrEmpty(message)) entity.ErrorMessage = string.Empty;

                if (!string.IsNullOrEmpty(payKey)) entity.PayKey = payKey;

                UserPayoutStatmentsRepository.UnitOfWork.CommitAndRefreshChanges();

            }
            catch (Exception ex)
            {
                Logger.Error("UpdateUserPayoutRecord", ex, CommonEnums.LoggerObjectTypes.Payout);
            }
        }

        private void UpdatePayoutExecutionRecord(int executionId, BillingEnums.ePayoutStatuses status)
        {
            try
            {
                var entity = PayoutExecutionsRepository.GetById(executionId);

                if (entity == null) return;

                entity.StatusId = (byte)status;
                entity.UpdateOn = DateTime.Now;
                entity.UpdatedBy = CurrentUserId;

                PayoutExecutionsRepository.UnitOfWork.CommitAndRefreshChanges();

            }
            catch (Exception ex)
            {
                Logger.Error("UpdatePayoutExecutionRecord" , ex, CommonEnums.LoggerObjectTypes.Payout);
            }
        }
        #endregion


        #region interface implementation

        public bool RunBatchAgain(int executionId, out string error)
        {
            try
            {
                UserPayoutStatmentsRepository.Delete(x => x.ExecutionId == executionId && (x.StatusId == (byte)BillingEnums.ePayoutStatuses.SKIP || x.StatusId == (byte)BillingEnums.ePayoutStatuses.FAILED));

                if (!UserPayoutStatmentsRepository.UnitOfWork.CommitAndRefreshChanges(out error)) return false;

                var entity = PayoutExecutionsRepository.GetById(executionId);

                if (entity== null)
                {
                    error = "Payout Execution entity not found. Contact development team";
                    return false;
                }
                int execId;

                return ExecuteMonthlyPayout(entity.PayoutYear,entity.PayoutMonth,new List<BasePayoutSelectionToken>(),out execId,out error);
            }
            catch (Exception ex)
            {
                error = FormatError(ex);
                Logger.Error("RunBatchAgain", ex,executionId, CommonEnums.LoggerObjectTypes.Payout);
                return false;
            }
        }

        public bool RunSinglePaymentAgain(int payoutId, out string error)
        {
            try
            {
                var entity = UserPayoutStatmentsRepository.GetById(payoutId);

                if (entity == null)
                {
                    error = "Payout Execution entity not found. Contact development team";
                    return false;
                }

                var userEntity = UserRepository.GetById(entity.UserId);

                if (userEntity == null)
                {
                    error = "User entity not found. Contact development team";
                    return false;
                }

                entity.UpdateStatetmentData(userEntity);

                if (!UserPayoutStatmentsRepository.UnitOfWork.CommitAndRefreshChanges(out error)) return false;

                var token = GetPayoutStatments(entity.ExecutionId).FirstOrDefault(x => x.PayoutId == payoutId);

                if (token == null)
                {
                    error = "Statement token not found. Contact development team";
                    return false;
                }

                //return ExecuteSinglePayment(token,out error);
                var item = CreateMassPayRequestItem(token, String.Format("{0}-{1} Payment", entity.PO_PayoutExecutions.PayoutYear, entity.PO_PayoutExecutions.PayoutMonth), out error);


                var executed = _paypalPaymentServices.ExecuteMassPayment(new List<MassPayRequestItemType>
                                {
                                    item
                                }, out error);



                UpdateUserPayoutRecord(token.PayoutId, executed ? BillingEnums.ePayoutStatuses.WAIT_4_IPN : BillingEnums.ePayoutStatuses.FAILED, executed ? string.Empty : error);

                return executed;
            }
            catch (Exception ex)
            {
                error = FormatError(ex);
                Logger.Error("RunBatchAgain", ex, payoutId, CommonEnums.LoggerObjectTypes.Payout);
                return false;
            }
        }

        public bool ExecuteMonthlyPayout(int year, int month, List<BasePayoutSelectionToken> include, out int execId, out string error)
        {
            error = string.Empty;
            execId = -1;
            try
            {
                var today = DateTime.Now;

                var firstCurrent = new DateTime(today.Year, today.Month, 1);
                var firstReport = new DateTime(year, month, 1);

                if (firstCurrent.Date <= firstReport.Date)
                {
                    error = "payout for future period not allowed";
                    return false;
                }

                using (var context = new lfeAuthorEntities())
                {
                    //create data
                    context.sp_PO_SaveMonthlyPayoutStatment(year, month, LFE_COMMISSION_PERCENT, CurrentUserId);


                    var token = GetPayoutExecutionDto(year, month);


                    if (token.ExecutionId < 0)
                    {
                        error = "Data saving failed. Contact development team";
                        return false;
                    }

                    execId = token.ExecutionId;

                    if (include.Count > 0)
                    {
                        var statments = UserPayoutStatmentsRepository.GetMany(x => x.ExecutionId == token.ExecutionId && x.StatusId == (byte)BillingEnums.ePayoutStatuses.WAIT).ToList();

                        foreach (var statement in statments.Where(statement => !include.Any(x => x.userId == statement.UserId && x.currId == statement.CurrencyId)))
                        {
                            UpdateUserPayoutRecord(statement.PayoutId, BillingEnums.ePayoutStatuses.SKIP, "skipped by admin");
                        }
                    }

                    var list2Pay = GetPayoutStatments(token.ExecutionId).Where(x => x.Status == BillingEnums.ePayoutStatuses.WAIT).ToArray();

                    var paymentItems = new List<MassPayRequestItemType>();

                    foreach (var po_token in list2Pay)
                    {
                        // ExecuteSinglePayment(po_token, out error);
                        var item = CreateMassPayRequestItem(po_token, String.Format("{0}-{1} Payment", year, month), out error);

                        if (item != null) paymentItems.Add(item);
                    }
                    
                    if (paymentItems.Any())
                    {
                        var currencies = paymentItems.GroupBy(x => new { x.Amount.currencyID }).Select(x => x.Key.currencyID).ToArray();

                        foreach (var currency in currencies)
                        {
                            var currencyItems = paymentItems.Where(x => x.Amount.currencyID == currency).ToList();

                            var executed = _paypalPaymentServices.ExecuteMassPayment(currencyItems, out error);

                            var status = executed ? BillingEnums.ePayoutStatuses.WAIT_4_IPN : BillingEnums.ePayoutStatuses.FAILED;

                            foreach (var row in currencyItems)
                            {
                                UpdateUserPayoutRecord(Convert.ToInt32(row.UniqueId), status, executed ? string.Empty : error, null);
                            }
                        }
                    }


                    var payouts = UserPayoutStatmentsRepository.GetMany(x => x.ExecutionId == token.ExecutionId).ToArray();

                    var successCount = payouts.Count(x => x.StatusId == (byte)BillingEnums.ePayoutStatuses.WAIT_4_IPN);

                    if (successCount.Equals(0))
                    {
                        UpdatePayoutExecutionRecord(token.ExecutionId, BillingEnums.ePayoutStatuses.FAILED);
                        return true;
                    }

                    if (successCount < payouts.Count())
                    {
                        UpdatePayoutExecutionRecord(token.ExecutionId, BillingEnums.ePayoutStatuses.PARTIALLY);
                        return true;
                    }

                    UpdatePayoutExecutionRecord(token.ExecutionId, BillingEnums.ePayoutStatuses.COMPLETED);


                    return true;
                }
            }
            catch (Exception ex)
            {
                error = FormatError(ex);
                Logger.Error("ExecuteMonthlyPayout::" + year + "::" + month, ex, CommonEnums.LoggerObjectTypes.Payout);
                return false;
            }
        }

        //public bool ExecuteMonthlyPayout(int year, int month, List<BasePayoutSelectionToken> include, out int execId, out string error)
        //{
        //    error = string.Empty;
        //    execId = -1;
        //    try
        //    {
        //        var today = DateTime.Now;

        //        var firstCurrent = new DateTime(today.Year, today.Month, 1);
        //        var firstReport = new DateTime(year, month, 1);

        //        if (firstCurrent.Date <= firstReport.Date)
        //        {
        //            error = "payout for future period not allowed";
        //            return false;
        //        }

        //        using (var context = new lfeAuthorEntities())
        //        {
        //            //create data
        //            context.sp_PO_SaveMonthlyPayoutStatment(year, month, LFE_COMMISSION_PERCENT, CurrentUserId);


        //            var token = GetPayoutExecutionDto(year, month);


        //            if (token.ExecutionId < 0)
        //            {
        //                error = "Data saving failed. Contact development team";
        //                return false;
        //            }

        //            execId = token.ExecutionId;

        //            if (include.Count > 0)
        //            {
        //                var statments = UserPayoutStatmentsRepository.GetMany(x => x.ExecutionId == token.ExecutionId && x.StatusId == (byte)BillingEnums.ePayoutStatuses.WAIT).ToList();

        //                foreach (var statement in statments.Where(statement => !include.Any(x => x.userId == statement.UserId && x.currId == statement.CurrencyId)))
        //                {
        //                    UpdateUserPayoutRecord(statement.PayoutId, BillingEnums.ePayoutStatuses.SKIP, "skipped by admin");
        //                }    
        //            }                    

        //            var list2Pay = GetPayoutStatments(token.ExecutionId).Where(x=>x.Status == BillingEnums.ePayoutStatuses.WAIT).ToArray();

        //            foreach (var po_token in list2Pay)
        //            {
        //                ExecuteSinglePayment(po_token, out error);
        //            }

                    

        //            var payouts = UserPayoutStatmentsRepository.GetMany(x=>x.ExecutionId == token.ExecutionId).ToArray();

        //            var successCount = payouts.Count(x => x.StatusId == (byte) BillingEnums.ePayoutStatuses.COMPLETED);

        //            if (successCount.Equals(0))
        //            {
        //                UpdatePayoutExecutionRecord(token.ExecutionId,BillingEnums.ePayoutStatuses.FAILED);
        //                return true;
        //            }

        //            if(successCount < payouts.Count())
        //            {
        //                UpdatePayoutExecutionRecord(token.ExecutionId, BillingEnums.ePayoutStatuses.PARTIALLY);
        //                return true;
        //            }

        //            UpdatePayoutExecutionRecord(token.ExecutionId, BillingEnums.ePayoutStatuses.COMPLETED);
                   

        //            return true;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        error = FormatError(ex);
        //        Logger.Error("ExecuteMonthlyPayout::"+year+"::"+month, ex, CommonEnums.LoggerObjectTypes.Payout);
        //        return false;
        //    }
        //}

        public bool ExecuteSinglePayment(PayoutStatmentDTO token, out string error )
        {
            if (token.PayoutType == null)
            {
                error = "Payout option not defined for user";
                UpdateUserPayoutRecord(token.PayoutId, BillingEnums.ePayoutStatuses.FAILED, error);
                return false;
            }

            if (token.PayoutType != BillingEnums.ePayoutTypes.PAYPAL)
            {
                error = "Payout Paypal option not defined for user";
                UpdateUserPayoutRecord(token.PayoutId, BillingEnums.ePayoutStatuses.FAILED, error);
                return false;
            }

            if (String.IsNullOrEmpty(token.PaypalEmail))
            {
                error = "Paypal Email not defined for user";
                UpdateUserPayoutRecord(token.PayoutId, BillingEnums.ePayoutStatuses.FAILED, error);
                return false;
            }


            if (token.Amount <= 0)
            {
                error = "Amount should be grater as zero";
                UpdateUserPayoutRecord(token.PayoutId, BillingEnums.ePayoutStatuses.FAILED, error);
                return false;
            }

            var payment = new PaypalPaymentExecutionToken
            {
                ReceiverEmail = token.PaypalEmail,
                Amount        = token.Amount,
                InvoiceId     = token.PayoutId.ToString(),
                Currency      = new BaseCurrencyDTO { ISO = token.Currency.ISO }
            };

            string payKey;

            var result = _paypalPaymentServices.ExecutePayment(payment,out payKey ,out error);

            UpdateUserPayoutRecord(token.PayoutId, result ? BillingEnums.ePayoutStatuses.COMPLETED : BillingEnums.ePayoutStatuses.FAILED, result ? string.Empty : error,payKey);

            return result;
        }

        public PayoutExecutionDTO GetPayoutExecutionDto(int year, int month)
        {
            try
            {
                var entity = PayoutExecutionsRepository.Get(x => x.PayoutYear == year && x.PayoutMonth == month);

                return entity == null ? new PayoutExecutionDTO(year, month) : entity.Entity2ExecutionDto(GetUser(entity.CreatedBy));
            }
            catch (Exception ex)
            {
                Logger.Error("getPayoutExecutionDto", ex, CommonEnums.LoggerObjectTypes.Payout);
                return null;
            }
        }

        public SaleSummaryReportDTO GetMonthlyPayoutReport(int year, int month, int? userId, short? currencyId)
        {
            try
            {
                var token = new SaleSummaryReportDTO
                                {
                                    Year = year,
                                    Month = month
                                };

                var payoutToken = GetPayoutExecutionDto(year, month);

                token.PayoutExecution = payoutToken;

                var rows = GetPayoutCurrencySummaryRows(year, month, userId, currencyId, payoutToken.ExecutionId);

                if (rows.Count <= 0)
                {
                    token.Message = "no rows found";
                    token.IsValid = true;

                    return token;
                }

                token.CurrencyRows = rows;

                token.IsValid = true;

                return token;

                //using (var context = new lfeAuthorEntities())
                //{
                //    var rows = context.sp_PO_GetMonthlyPayoutReport(year, month, LFE_COMMISSION_PERCENT, userId, currencyId).ToList();

                //    if (rows.Count <= 0)
                //    {
                //        token.Message = "no rows found";
                //        token.IsValid = true;

                //        return token;
                //    }

                //    var currencies = rows.GroupBy(x=>new{x.CurrencyId,x.CurrencyName,x.ISO,x.Symbol}).Select(x=>new BaseCurrencyDTO
                //                                                                                                {
                //                                                                                                    CurrencyId    = x.Key.CurrencyId
                //                                                                                                    ,CurrencyName = x.Key.CurrencyName 
                //                                                                                                    ,ISO          = x.Key.ISO
                //                                                                                                    ,Symbol       = x.Key.Symbol
                //                                                                                                }).ToList();
                //    foreach (var currency in currencies)
                //    {
                //        var cur = currency;
                //        token.CurrencyRows.Add(new PayoutCurrencySummaryDTO
                //        {
                //            Currency = currency
                //            ,Rows = rows.Where(x => x.CurrencyId == cur.CurrencyId).Select(x => x.Entity2ReportRowDto(year, month,GetPayoutStatus(x.UserId,payoutToken.ExecutionId))).OrderByDescending(x => x.TotalSales).ToList()
                //        });
                //    }
                //}

               
            }
            catch (Exception ex)
            {
                var error = Utils.FormatError(ex);
                Logger.Error("get sale summary report", ex, CommonEnums.LoggerObjectTypes.Reports);

                return new SaleSummaryReportDTO
                {
                    IsValid = false
                    ,Message = error
                };
            }
        }

        public List<PayoutCurrencySummaryDTO> GetPayoutCurrencySummaryRows(int year, int month, int? userId, short? currencyId, int executionId)
        {
            try
            {
                var list = new List<PayoutCurrencySummaryDTO>();

                using (var context = new lfeAuthorEntities())
                {
                    var rows = context.sp_PO_GetMonthlyPayoutReport(year, month, LFE_COMMISSION_PERCENT, userId, currencyId).ToList();

                    var currencies = rows.GroupBy(x=>new{x.CurrencyId,x.CurrencyName,x.ISO,x.Symbol}).Select(x=>new BaseCurrencyDTO
                                                                                                                {
                                                                                                                    CurrencyId    = x.Key.CurrencyId
                                                                                                                    ,CurrencyName = x.Key.CurrencyName 
                                                                                                                    ,ISO          = x.Key.ISO
                                                                                                                    ,Symbol       = x.Key.Symbol
                                                                                                                }).ToList();
                    foreach (var currency in currencies)
                    {
                        var cur = currency;
                        list.Add(new PayoutCurrencySummaryDTO
                        {
                            Currency = currency
                            ,Rows = rows.Where(x => x.CurrencyId == cur.CurrencyId).Select(x => x.Entity2ReportRowDto(year, month,GetPayoutStatus(x.UserId,executionId))).OrderByDescending(x => x.TotalSales).ToList()
                        });
                    }
                }

                return list;
            }
            catch (Exception ex)
            {
                Logger.Error("GetPayoutCurrencySummaryRows", ex, CommonEnums.LoggerObjectTypes.Reports);
                return new List<PayoutCurrencySummaryDTO>();
            } 
        } 

        //reporting
        public List<PayoutExecutionDTO> GetPayoutExecutions(int? executionId)
        {
            try
            {
                return PayoutExecutionsRepository.GetPayoutExecutions(executionId).Select(x=>x.Entity2ExecutionDto()).OrderByDescending(x=>x.PayoutDate).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("GetPayoutExecutions", ex, executionId, CommonEnums.LoggerObjectTypes.Reports);

                return new List<PayoutExecutionDTO>();
            }
        }

        public List<PayoutStatmentDTO> GetPayoutStatments(int executionId)
        {
            try
            {
                return UserPayoutStatmentsRepository.GetPayoutStatments(executionId).Select(x => x.Entity2PayoutStatmentDto()).OrderByDescending(x => x.Amount).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("GetPayoutStatment", ex, executionId, CommonEnums.LoggerObjectTypes.Reports);

                return new List<PayoutStatmentDTO>();
            }
        }
        #endregion
    }
}
