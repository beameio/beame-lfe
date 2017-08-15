using System;
using LFE.Core.Enums;
using LFE.DataTokens;
using LFE.Dto.Mapper.Helper;
using LFE.Model;

namespace LFE.Dto.Mapper.EntityMapper
{
    public static class PayoutEntityMapper
    {
        public static PO_PayoutExecutions ToPayoutExecutionsEntity(this int year, int month)
        {
            return  new PO_PayoutExecutions
                    {
                        PayoutYear   = year
                        ,PayoutMonth = month
                        ,StatusId    = (byte)BillingEnums.ePayoutStatuses.WAIT
                        ,AddOn       = DateTime.Now
                        ,CreatedBy   = DtoExtensions.CurrentUserId
                    };
        }

        public static void UpdateStatetmentData(this PO_UserPayoutStatments entity, Users userEntity)
        {
            entity.PayoutTypeId = userEntity.PayoutTypeId;
            entity.PaypalEmail  = userEntity.PaypalEmail;
            entity.UpdateOn     = DateTime.Now;
            entity.UpdatedBy    = DtoExtensions.CurrentUserId;
        }

        public static void UpdateStatetmentData(this PO_UserPayoutStatments entity, MassPaymentItemToken token)
        {
            entity.StatusId   = (byte)(token.status.ToLower() == "completed" ? BillingEnums.ePayoutStatuses.COMPLETED : BillingEnums.ePayoutStatuses.FAILED);
            entity.PayKey     = token.masspay_txn_id;
            entity.PaymentFee = token.mc_fee;
            entity.UpdateOn   = DateTime.Now;
            entity.UpdatedBy  = DtoExtensions.CurrentUserId;
        }
    }
}
