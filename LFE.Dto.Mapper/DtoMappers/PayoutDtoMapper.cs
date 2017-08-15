using System;
using System.Linq;
using LFE.Core.Enums;
using LFE.Core.Extensions;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Dto.Mapper.Helper;
using LFE.Model;

namespace LFE.Dto.Mapper.DtoMappers
{
    public static class PayoutDtoMapper
    {
        public static PayoutExecutionDTO Entity2ExecutionDto(this PO_PayoutExecutions entity,BaseUserInfoDTO createdBy)
        {
            return new PayoutExecutionDTO
            {
                ExecutionId = entity.ExecutionId
                ,Year       = entity.PayoutYear
                ,Month      = entity.PayoutMonth
                ,Status     = Utils.ParseEnum<BillingEnums.ePayoutStatuses>(entity.StatusId)                
                ,AddOn      = entity.AddOn
                ,UpdatedOn  = entity.UpdateOn
                ,ExecutedBy = createdBy
            };
        }

        public static PayoutExecutionDTO Entity2ExecutionDto(this PO_PayoutExecutionToken entity)
        {
            return new PayoutExecutionDTO
            {
                ExecutionId         = entity.ExecutionId
                ,Year               = entity.PayoutYear
                ,Month              = entity.PayoutMonth
                ,PayoutDate         = new DateTime(entity.PayoutYear,entity.PayoutMonth,1)
                ,Status             = Utils.ParseEnum<BillingEnums.ePayoutStatuses>(entity.StatusId)
                ,TotalRows          = entity.TotalRows
                ,TotCompletedalRows = entity.TotalCompletedRows
                ,AddOn              = entity.AddOn
                ,UpdatedOn          = entity.UpdateOn
                ,ExecutedBy         = new BaseUserInfoDTO(entity.CreatedBy != null ? (int)entity.CreatedBy : -1, entity.CreatedBy != null ? string.Format("{0} {1}",entity.CreatorFirstName,entity.CreatorLastName) : string.Empty)
                ,UpdatedBy          = new BaseUserInfoDTO(entity.UpdatedBy != null ? (int)entity.UpdatedBy: -1, entity.UpdatedBy != null ? string.Format("{0} {1}",entity.UpdatedByFirstName,entity.UpdatedByLastName) : string.Empty)
            };
        }

        public static PayoutStatmentDTO Entity2PayoutStatmentDto(this PO_PayoutStatmentToken entity)
        {
            return new PayoutStatmentDTO
            {
                 ExecutionId        = entity.ExecutionId
                ,PayoutId           = entity.PayoutId
                ,Beneficiary        = new BaseUserInfoDTO(entity.UserId, string.Format("{0} {1}", entity.FirstName, entity.LastName))
                ,Currency           = new BaseCurrencyDTO { CurrencyId = entity.CurrencyId,ISO = entity.ISO,Symbol = entity.Symbol}
                ,PayKey             = entity.PayKey
                ,Amount             = entity.Payout
                ,PaypalEmail        = entity.PaypalEmail
                ,PayoutType         = entity.PayoutTypeId != null ? Utils.ParseEnum<BillingEnums.ePayoutTypes>(entity.PayoutTypeId.ToString()) : (BillingEnums.ePayoutTypes?) null
                ,Error              = entity.ErrorMessage
                ,Status             = Utils.ParseEnum<BillingEnums.ePayoutStatuses>(entity.StatusId)
                ,AddOn              = entity.AddOn
                ,UpdatedOn          = entity.UpdateOn
                ,ExecutedBy         = new BaseUserInfoDTO(entity.CreatedBy != null ? (int)entity.CreatedBy : -1, entity.CreatedBy != null ? string.Format("{0} {1}",entity.CreatorFirstName,entity.CreatorLastName) : string.Empty)
                ,UpdatedBy          = new BaseUserInfoDTO(entity.UpdatedBy != null ? (int)entity.UpdatedBy: -1, entity.UpdatedBy != null ? string.Format("{0} {1}",entity.UpdatedByFirstName,entity.UpdatedByLastName) : string.Empty)
            };
        }


        public static DashboardPayoutToken Entity2DashboardPayoutToken(this PO_MonthlyPayoutToken entity)
        {
            return new DashboardPayoutToken
            {
                Sales        = (entity.TotalSales + entity.RefundProgramReleased).FormatMoney(0)
                ,Fees        = (entity.TotalFees + entity.AffiliateFees  + entity.LfeCommissions + entity.TotalRefunded - entity.TotalRefundedFees).FormatMoney(0)
                ,Mbg         = (entity.RefundProgramHold).FormatMoney(0)
               // ,TotalPayout = (entity.Payout).FormatMoney(0)
                ,Currency    = entity.Entity2BaseCurrencyDto()
            };
        }

        public static AdminPayoutToken PayoutCurrencySummaryDto2AdminPayoutToken(this PayoutCurrencySummaryDTO token)
        {
            var sales = token.Rows.Sum(x => x.TotalSales).FormatMoney(0);

            var ppFees = token.Rows.Sum(x => x.Fees).FormatMoney(0);

            var refund = token.Rows.Sum(x => x.Refund).FormatMoney(0);

            var mbgHold = token.Rows.Sum(x => x.RefundProgrammToHold).FormatMoney(0);

            var mbgRelease = token.Rows.Sum(x => x.RefundProgrammToRelease).FormatMoney(0);

            var lfeCommission = token.Rows.Sum(x => x.LfeCommission).FormatMoney(0);

            var totalSales = sales - ppFees - refund - mbgHold + mbgRelease;

            return  new AdminPayoutToken
                    {
                        Currency       = token.Currency
                        ,TotalSales    = totalSales
                        ,LicenseFee    = 0
                        ,LfeCommission = lfeCommission
                    };
        }

        public static PayoutUserMonthlyStatementDTO Entity2ReportRowDto(this PO_MonthlyPayoutToken entity,int year,int month,BillingEnums.ePayoutStatuses status)
        {

            //var totalAuthorSales          = entity.Author_total_sales;
            //var affiliateTotalSales       = entity.Affiliate_total_sales + entity.By_Affiliate_total_sales;
            //var totalAffiliateCommission  = entity.Affiliate_total_non_rgp_commission + entity.By_Affiliate_total_net_non_rgp_sales;
            //var rgp2keep                  = entity.Author_total_rgp_sales + entity.Affiliate_total_rgp_sales + entity.By_Affiliate_total_rgp_sales;
            //var rgp2release               = entity.Author_Released_total_rgp_sales + entity.Affiliate_Released_total_net_rgp_sales + entity.By_Affiliate_Released_total_rgp_commission;
            //var totalRefunded             = entity.Author_total_refunded + entity.Affiliate_total_net_refunded + entity.By_Affiliate_total_net_refunded;
            //var totalRefundedFees         = entity.Author_fee_refunded + entity.Affiliate_fee_net_refunded + entity.By_Affiliate_fee_net_refunded;
            //var totalFees                 = entity.Author_total_non_rgp_fee + entity.Affiliate_total_net_non_rgp_fee + entity.By_Affiliate_total_net_non_rgp_fee + 
            //                                entity.Author_Released_total_rgp_fee + entity.Affiliate_Released_total_net_rgp_fee + entity.By_Affiliate_Released_total_net_rgp_fee;
            

            //var token = new PayoutUserMonthlyStatementDTO
            //{
            //     TotalSales              = (totalAuthorSales + affiliateTotalSales).FormatMoney(2)
            //    ,Sales                   = totalAuthorSales.FormatMoney(2)
            //    ,AffiliateSales          = affiliateTotalSales.FormatMoney(2)
            //    ,RefundProgrammToHold    = rgp2keep.FormatMoney(2)
            //    ,RefundProgrammToRelease = rgp2release.FormatMoney(2)
            //    ,AffiliateCommission     = totalAffiliateCommission.FormatMoney(2)
            //    ,Refund                  = totalRefunded.FormatMoney(2)
            //    ,Fees                    = totalFees.FormatMoney(2) 
            //    ,RefundFees              = totalRefundedFees.FormatMoney(2)
            //    ,Year                    = year
            //    ,Month                   = month
            //    ,Currency                = entity.Entity2BaseCurrencyDto()
            //    ,PayoutStatus            = status
            //};

            //token.AffiliateFees = token.AffiliateSales - token.AffiliateCommission;

            //var balance = token.Sales.SalesToBalance(token.Fees,token.Refund,token.RefundFees,token.AffiliateCommission,token.RefundProgrammToHold,token.RefundProgrammToRelease);

            //token.Balance = balance;

            //token.Commission = token.Balance.BalanceToLfeCommission();

            //token.Payout = token.Balance.BalanceToPayout(token.Commission);//Sales.ToPayout(token.Fees , token.Commission , token.Refund);

            var token = new PayoutUserMonthlyStatementDTO
            {
                 TotalSales              = entity.TotalSales.FormatMoney(2)
                ,Sales                   = entity.Author_total_sales.FormatMoney(2)
                ,AffiliateSales          = entity.AffiliateTotalSales.FormatMoney(2)
                ,RefundProgrammToHold    = entity.RefundProgramHold.FormatMoney(2)
                ,RefundProgrammToRelease = entity.RefundProgramReleased.FormatMoney(2)
                ,AffiliateCommission     = entity.AffiliateCommission.FormatMoney(2)
                ,Fees                    = entity.TotalFees.FormatMoney(2) 
                ,Refund                  = entity.TotalRefunded.FormatMoney(2)                
                ,RefundFees              = entity.TotalRefundedFees.FormatMoney(2)
                ,AffiliateFees           = entity.AffiliateFees.FormatMoney(2)
                ,Balance                 = entity.Balance.FormatMoney(2)
                ,LfeCommission           = entity.LfeCommissions.FormatMoney(2)
                ,Payout                  = entity.Payout.FormatMoney(2)
                ,Year                    = year
                ,Month                   = month
                ,Currency                = entity.Entity2BaseCurrencyDto()
                ,PayoutStatus            = status
            };

            if (entity.UserId != null)
            {
                token.SellerId = (int) entity.UserId;
                token.Seller   = new BaseUserInfoDTO
                               {
                                   UserId    = (int) entity.UserId
                                   ,Email    = entity.Email
                                   ,FullName = entity.Entity2FullName()
                               };
            }

           

            if (entity.PayoutTypeId == null) return token;

            token.PayoutSettings = new PayoutSettingsDTO
            {
                PayoutType = Utils.ParseEnum<BillingEnums.ePayoutTypes>(entity.PayoutTypeId.ToString())
                ,Email = entity.PaypalEmail                
            };

            if (entity.PayoutAddressID == null) return token;

            token.PayoutSettings.Address = String.Format("{0} {1},{2}, {3} {4}, {5}, {6} {7}",entity.PayoutFirstName,entity.PayoutLastName,entity.PostalCode,entity.CountryName,entity.StateName,entity.CityName,entity.Street1,entity.Street2);

            return token;
        }

        public static PayoutUserMonthlyStatementDTO Entity2ReportRowDto(this ADMIN_SaleSummaryToken entity,int year,int month)
        {
       
            var token = new PayoutUserMonthlyStatementDTO
            {
                Sales       = entity.total.FormatMoney(2)
                ,Refund     = entity.refund.FormatMoney(2)
                ,Fees       = entity.fee.FormatMoney(2) 
                ,RefundFees = entity.refundFee.FormatMoney(2)
                ,Balance    = entity.total.SalesToBalance(entity.fee,entity.refund,entity.refundFee)
                ,Year       = year
                ,Month      = month
                ,Currency   = entity.Entity2BaseCurrencyDto()
            };

            if (entity.SellerUserId != null)
            {
                token.SellerId = (int) entity.SellerUserId;
                token.Seller   = new BaseUserInfoDTO
                               {
                                   UserId    = (int) entity.SellerUserId
                                   ,Email    = entity.Email
                                   ,FullName = entity.Entity2FullName()
                               };
            }

            token.LfeCommission = token.Balance.BalanceToLfeCommission();

            token.Payout = token.Balance.BalanceToPayout(token.LfeCommission);//Sales.ToPayout(token.Fees , token.Commission , token.Refund);

            if (entity.PayoutTypeId == null) return token;

            token.PayoutSettings = new PayoutSettingsDTO
            {
                PayoutType = Utils.ParseEnum<BillingEnums.ePayoutTypes>(entity.PayoutTypeId.ToString())
                ,Email = entity.PaypalEmail                
            };

            if (entity.PayoutAddressID == null) return token;

            token.PayoutSettings.Address = String.Format("{0} {1},{2}, {3} {4}, {5}, {6} {7}",entity.AddressFirstName,entity.AddressLastName,entity.PostalCode,entity.CountryName,entity.StateName,entity.CityName,entity.Street1,entity.Street2);

            return token;
         }

        public static BaseCurrencyDTO Entity2BaseCurrencyDto(this BASE_CurrencyToken entity)
        {
            return new BaseCurrencyDTO
            {
                CurrencyId    = entity.CurrencyId ?? Constants.DEFAULT_CURRENCY_ID
                ,CurrencyName = entity.CurrencyName
                ,ISO          = entity.ISO
                ,Symbol       = entity.Symbol
            };
        }
    }
}
