using LFE.Application.Services.Base;
using LFE.Application.Services.Helper;
using LFE.Application.Services.Interfaces;
using LFE.Core.Enums;
using LFE.Core.Extensions;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Dto.Mapper.DtoMappers;
using LFE.Dto.Mapper.EntityMapper;
using LFE.Dto.Mapper.Helper;
using LFE.Model;
using PayPal.AdaptivePayments;
using PayPal.AdaptivePayments.Model;
using PayPal.Exception;
using PayPal.PayPalAPIInterfaceService.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using PayPal.Api;
using PayPal.PayPalAPIInterfaceService;
using EnumUtils = PayPal.PayPalAPIInterfaceService.Model.EnumUtils;

namespace LFE.Application.Services.ExternalProviders
{
    public class PaypalServices : ServiceBase, IPaypalServices, IPaypalIpnServices, IPaypalManageServies, IPaypalPaymentServices
    {
        private static readonly bool _isDaily = bool.Parse(Utils.GetKeyValue("useTestDailySubscription"));

        #region private helpers
        #region paypal requests logs
        private bool CreateCcPaymentRequest(Guid requestId, PaypalCreditCardPaymentDTO token, BillingEnums.ePaypalRequestTypes type, out string error)
        {
            try
            {
                PaypalPaymentRequestsRepository.Add(token.DirectCcToken2PaypalPaymentRequestDto(CurrentUserId, requestId, type).Dto2PaypalPaymentRequestEntity());

                return PaypalPaymentRequestsRepository.UnitOfWork.CommitAndRefreshChanges(out error);
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("save direct cc payment request1", ex, CommonEnums.LoggerObjectTypes.PayPal);
                return false;
            }

        }

        private bool CreateCcPaymentRequest(Guid requestId, SubscriptionWithSavedCardDTO token, decimal amount, BillingEnums.ePaypalRequestTypes type, out string error)
        {
            try
            {
                PaypalPaymentRequestsRepository.Add(token.DirectCcToken2PaypalPaymentRequestDto(amount, requestId, type).Dto2PaypalPaymentRequestEntity());

                return PaypalPaymentRequestsRepository.UnitOfWork.CommitAndRefreshChanges(out error);
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("save direct cc payment request2", ex, CommonEnums.LoggerObjectTypes.PayPal);
                return false;
            }
        }

        private bool CreatePaymentRequestFromSourceRequest(Guid requestId, PAYPAL_PaymentRequests sourceRequest, decimal amount, BillingEnums.ePaypalRequestTypes type, out string error)
        {
            try
            {
                PaypalPaymentRequestsRepository.Add(sourceRequest.SourceRequest2PaypalPaymentRequestDto(requestId, amount, type).Dto2PaypalPaymentRequestEntity());

                return PaypalPaymentRequestsRepository.UnitOfWork.CommitAndRefreshChanges(out error);
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("save refund payment request", ex, CommonEnums.LoggerObjectTypes.PayPal);
                return false;
            }
        }

        private bool CreatePaymentRequestFromOrderLine(Guid requestId, vw_SALE_OrderLines orderLine, BillingEnums.ePaypalRequestTypes type, out string error)
        {
            try
            {
                PaypalPaymentRequestsRepository.Add(orderLine.OrderLine2PaypalCancelSubscriptionRequestDto(requestId, type).Dto2PaypalPaymentRequestEntity());

                return PaypalPaymentRequestsRepository.UnitOfWork.CommitAndRefreshChanges(out error);
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("save refund payment request", ex, CommonEnums.LoggerObjectTypes.PayPal);
                return false;
            }
        }

        private bool CreateCcRecurringPaymentRequest(Guid requestId, PaypalPaymentRequestDTO token, out string error)
        {
            try
            {
                PaypalPaymentRequestsRepository.Add(token.PaypalPaymentRequestDTO2PaypalPaymentRequestDto(CurrentUserId, requestId, BillingEnums.ePaypalRequestTypes.RECURRING_PAYMENT_EXECUTION).Dto2PaypalPaymentRequestEntity());

                return PaypalPaymentRequestsRepository.UnitOfWork.CommitAndRefreshChanges(out error);
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("save direct cc payment request3", ex, CommonEnums.LoggerObjectTypes.PayPal);
                return false;
            }

        }

        private bool CreateAccountPaymentRequest(Guid requestId, string payment_id, PayPalCreatePaymentDTO token, BillingEnums.ePaypalRequestTypes type, out string error)
        {
            try
            {
                PaypalPaymentRequestsRepository.Add(token.AccountPaymentToken2PaypalPaymentRequestDto(CurrentUserId, requestId, payment_id, type).Dto2PaypalPaymentRequestEntity());

                return PaypalPaymentRequestsRepository.UnitOfWork.CommitAndRefreshChanges(out error);
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("save account payment request", ex, CommonEnums.LoggerObjectTypes.PayPal);
                return false;
            }

        }

        private bool UpdateRequestExecutionPaymentId(Guid requestId, string paymentId, string transactionId, out string error)
        {
            try
            {
                var entity = PaypalPaymentRequestsRepository.GetById(requestId);

                if (entity == null)
                {
                    error = "request entity not found";
                    return false;
                }

                entity.EntityUpdateExecutionPaymentId(paymentId, transactionId);

                return PaypalPaymentRequestsRepository.UnitOfWork.CommitAndRefreshChanges(out error);
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("account payment request::update execution_payment_id", ex, CommonEnums.LoggerObjectTypes.PayPal);
                return false;
            }
        }

        private void UpdateRequestErrorStatus(Guid requestId, string error)
        {
            try
            {
                var entity = PaypalPaymentRequestsRepository.GetById(requestId);

                if (entity == null) return;

                entity.EntityUpdateRequestError(error);

                PaypalPaymentRequestsRepository.UnitOfWork.CommitAndRefreshChanges();
            }
            catch (Exception ex)
            {
                Logger.Error("account payment request::update request error status", ex, CommonEnums.LoggerObjectTypes.PayPal);
            }
        }

        private PAYPAL_PaymentRequests GetRequestCreatePaymentId(Guid requestId, out string error)
        {
            error = string.Empty;
            try
            {
                var entity = PaypalPaymentRequestsRepository.GetById(requestId);

                if (entity != null) return entity;

                error = "request entity not found";
                return null;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("account payment request::get payment_id", ex, CommonEnums.LoggerObjectTypes.PayPal);
                return null;
            }
        }
        #endregion

        #region credit card save/get/remove
        private bool CreateSaveCreditCardRequest(Guid requestId, out string error)
        {
            try
            {
                PaypalPaymentRequestsRepository.Add(requestId.SaveCCRequest2PaypalPaymentRequestDto(CurrentUserId).Dto2PaypalPaymentRequestEntity());

                return PaypalPaymentRequestsRepository.UnitOfWork.CommitAndRefreshChanges(out error);
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("save payment agreement request", ex, CommonEnums.LoggerObjectTypes.PayPal);
                return false;
            }

        }

        private bool SaveUserCreditCard(int userId, string card_token, string number, string card_type, Guid instrumentId, int addressId, out string error)
        {
            try
            {
                var id = instrumentId;
                var entity = UserPaymentInstrumentsRepository.Get(x => x.InstrumentId == id);

                if (entity == null)
                {
                    UserPaymentInstrumentsRepository.Add(userId.UserIdToUserPrefrenceEntityWithCreditCard(instrumentId, addressId, card_token, number.Substring(number.Length - 4), card_type));
                }
                else
                {
                    entity.UpdateCardToken(card_token, true);
                }

                return UserPaymentInstrumentsRepository.UnitOfWork.CommitAndRefreshChanges(out error);
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("save user cc", ex, CommonEnums.LoggerObjectTypes.PayPal);
                return false;
            }
        }

        private string GetStoredCardPaypalToken(Guid instrumentId, out string error)
        {
            error = string.Empty;
            try
            {
                var entity = UserPaymentInstrumentsRepository.Get(x => x.InstrumentId == instrumentId);
                return entity != null ? entity.PaypalCcToken : string.Empty;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("get saved cc token::check if card exists::", ex, CommonEnums.LoggerObjectTypes.PayPal);
                return string.Empty;
            }
        }
        #endregion

        #region billing agreement token save/get/remove
        private bool CreateRecurringPaymentAgreementRequest(PayPalAgreementDTO dto, Guid requestId, out string error)
        {
            try
            {
                PaypalPaymentRequestsRepository.Add(dto.PaymentAgreementRequest2PaypalPaymentRequestDto(requestId, CurrentUserId).Dto2PaypalPaymentRequestEntity());

                return PaypalPaymentRequestsRepository.UnitOfWork.CommitAndRefreshChanges(out error);
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("save payment agreement request", ex, CommonEnums.LoggerObjectTypes.PayPal);
                return false;
            }

        }

        private bool CreateRecurringPaymentAgreementRequest(Guid requestId, Guid sourceRequestId, out PaypalPaymentRequestDTO token, out string error)
        {
            token = null;
            try
            {
                var sourceRequest = PaypalPaymentRequestsRepository.GetById(sourceRequestId);

                if (sourceRequest == null)
                {
                    error = "source request not found";
                    return false;
                }

                token = sourceRequest.PaypalRequestEntity2PaypalPaymentRequestDto(requestId);

                PaypalPaymentRequestsRepository.Add(token.Dto2PaypalPaymentRequestEntity());

                return PaypalPaymentRequestsRepository.UnitOfWork.CommitAndRefreshChanges(out error);
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("save payment agreement request", ex, CommonEnums.LoggerObjectTypes.PayPal);
                return false;
            }

        }

        private bool UpdateRequestRecurringToken(Guid requestId, string token, out string error)
        {
            try
            {
                var entity = PaypalPaymentRequestsRepository.GetById(requestId);

                if (entity == null)
                {
                    error = "save request token:: entity not found";
                    return false;
                }

                entity.EntityUpdateReccuringRequestToken(token);

                return PaypalPaymentRequestsRepository.UnitOfWork.CommitAndRefreshChanges(out error);
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("account payment request::update request error status", ex, CommonEnums.LoggerObjectTypes.PayPal);
                return false;
            }
        }

        private CreateRecurringPaymentsProfileRequestType PopulateReccuringRequest(string agreement_token, ReccuringPaymentDTO dto, out string error)
        {
            error = string.Empty;


            var request = new CreateRecurringPaymentsProfileRequestType();

            var currency = (CurrencyCodeType)Enum.Parse(typeof(CurrencyCodeType), dto.Currency.ISO);

            var profileDetails = new CreateRecurringPaymentsProfileRequestDetailsType();

            request.CreateRecurringPaymentsProfileRequestDetails = profileDetails;
            // A timestamped token, the value of which was returned in the response to the first call to SetExpressCheckout. You can also use the token returned in the SetCustomerBillingAgreement response. Either this token or a credit card number is required. If you include both token and credit card number, the token is used and credit card number is ignored Call CreateRecurringPaymentsProfile once for each billing agreement included in SetExpressCheckout request and use the same token for each call. Each CreateRecurringPaymentsProfile request creates a single recurring payments profile.
            // Note: Tokens expire after approximately 3 hours.
            if (!string.IsNullOrEmpty(agreement_token))
            {
                profileDetails.Token = agreement_token;
            }
            // Credit card information for recurring payments using direct payments. Either a token or a credit card number is required. If you include both token and credit card number, the token is used and credit card number is ignored.
            else if (dto.creditCard != null)
            {
                var cc = dto.creditCard.CreditCardDto2MerchantApiCreditCard();
                // (Required) Credit card number.
                // Card Verification Value, version 2. Your Merchant Account settings determine whether this field is required. To comply with credit card processing regulations, you must not store this value after a transaction has been completed.
                // (Required) Credit card expiration month.
                // (Required) Credit card expiration year.
                profileDetails.CreditCard = cc;
            }
            else
            {
                error = "payment method required";
                return null;
            }


            // Populate Recurring Payments Profile Details
            var rpProfileDetails = _isDaily ? new RecurringPaymentsProfileDetailsType(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")) : new RecurringPaymentsProfileDetailsType(dto.billingStartDate.ToString("yyyy-MM-ddTHH:mm:ss"));


            profileDetails.RecurringPaymentsProfileDetails = rpProfileDetails;
            // (Optional) Full name of the person receiving the product or service paid for by the recurring payment. If not present, the name in the buyer's PayPal account is used.
            if (!string.IsNullOrEmpty(dto.subscriberName))
            {
                rpProfileDetails.SubscriberName = dto.subscriberName;
            }


            // (Required) Describes the recurring payments schedule, including the regular payment period, whether there is a trial period, and the number of payments that can fail before a profile is suspended.
            var scheduleDetails = new ScheduleDetailsType();
            // (Required) Description of the recurring payment.
            // Note: You must ensure that this field matches the corresponding billing agreement description included in the SetExpressCheckout request.
            if (!string.IsNullOrEmpty(dto.profileDescription))
            {
                scheduleDetails.Description = dto.profileDescription;
            }


            // (Required) Regular payment period for this schedule.

            // Number of billing periods that make up one billing cycle; 
            // required if you specify an optional trial period.
            // The combination of billing frequency and billing period must be 
            // less than or equal to one year. For example, if the billing cycle is Month,
            // the maximum value for billing frequency is 12. Similarly, 
            // if the billing cycle is Week, the maximum value for billing frequency is 52.
            // Note:
            // If the billing period is SemiMonth, the billing frequency must be 1.

            var frequency = _isDaily ? 1 : dto.billingPeriod.RecurringPaymentPeriod2Frequency();


            //Billing amount for each billing cycle during this payment period; 
            //required if you specify an optional trial period. 
            //This amount does not include shipping and tax amounts.
            //Note:
            //All amounts in the CreateRecurringPaymentsProfile request must have 
            //the same currency.
            //Character length and limitations: 
            //Value is a positive number which cannot exceed $10,000 USD in any currency. 
            //It includes no currency symbol. 
            //It must have 2 decimal places, the decimal separator must be a period (.),
            //and the optional thousands separator must be a comma (,).
            var paymentAmount = new BasicAmountType(currency, Math.Round(dto.billingAmount, CurrencyToDecimal(dto.Currency.ISO)).ToString(CultureInfo.InvariantCulture));

            var period = _isDaily ? BillingPeriodType.DAY : (BillingPeriodType)Enum.Parse(typeof(BillingPeriodType), dto.billingPeriod.ToString());

            //Number of billing periods that make up one billing cycle; 
            //required if you specify an optional trial period.
            //The combination of billing frequency and billing period must be 
            //less than or equal to one year. For example, if the billing cycle is Month,
            //the maximum value for billing frequency is 12. Similarly, 
            //if the billing cycle is Week, the maximum value for billing frequency is 52.
            //Note:
            //If the billing period is SemiMonth, the billing frequency must be 1.
            //var numCycles = Convert.ToInt32(dto.totalBillingCycles);

            //Unit for billing during this subscription period; 
            //required if you specify an optional trial period. 
            //It is one of the following values: [Day, Week, SemiMonth, Month, Year]
            //For SemiMonth, billing is done on the 1st and 15th of each month.
            //Note:
            //The combination of BillingPeriod and BillingFrequency cannot exceed one year.
            var paymentPeriod = new BillingPeriodDetailsType(period, frequency, paymentAmount);

            scheduleDetails.PaymentPeriod = paymentPeriod;

            scheduleDetails.ActivationDetails = new ActivationDetailsType
            {
                InitialAmount = new BasicAmountType(currency, dto.initialAmount.ToString(CultureInfo.InvariantCulture)),
                //TODO what's a logic on failure??
                FailedInitialAmountAction = FailedPaymentActionType.CANCELONFAILURE
            };

            profileDetails.ScheduleDetails = scheduleDetails;

            return request;
        }

        private bool UpdateRecurringPaymentAgreement(PAYPAL_PaymentRequests sourceRequestEntity, string profileId, decimal amount, string iso, out string error)
        {
            try
            {
                var requestId = Guid.NewGuid();
                if (!CreatePaymentRequestFromSourceRequest(requestId, sourceRequestEntity, amount, BillingEnums.ePaypalRequestTypes.RECURRING_PAYMENT_UPDATE_EXECUTION, out error)) return false;

                var request = new UpdateRecurringPaymentsProfileRequestType();
                PopulateUpdateRecccuringProfileRequest(request, profileId, amount, iso, "coupon discount finished");

                // Invoke the API
                var wrapper = new UpdateRecurringPaymentsProfileReq
                {
                    UpdateRecurringPaymentsProfileRequest = request
                };

                var service = PayPalConfiguration.PayPalMerchantAPIService;
                var updatePrrofileResponse = service.UpdateRecurringPaymentsProfile(wrapper);

                if (updatePrrofileResponse.Ack == AckCodeType.SUCCESS)
                {

                    return UpdatePaypalRequestStatus(requestId, BillingEnums.ePaymentRequestStatus.approved, out error);
                }

                error = updatePrrofileResponse.Errors.FormatMerchantApiErrors();
                UpdateRequestErrorStatus(requestId, error);
                return false;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("Update recurring profile", ex, CommonEnums.LoggerObjectTypes.PayPal);
                return false;
            }
        }

        private void PopulateUpdateRecccuringProfileRequest(UpdateRecurringPaymentsProfileRequestType request, string profileId, decimal amount, string iso, string note)
        {
            var currency = (CurrencyCodeType)Enum.Parse(typeof(CurrencyCodeType), iso);
            // Set EC-Token or Credit card requestDetails
            var profileDetails = new UpdateRecurringPaymentsProfileRequestDetailsType();
            request.UpdateRecurringPaymentsProfileRequestDetails = profileDetails;

            // (Required) Recurring payments profile ID returned in the CreateRecurringPaymentsProfile response.
            profileDetails.ProfileID = profileId;

            profileDetails.Note = note;

            // Populate Recurring Payments Profile Details

            // (Optional) Billing amount for each cycle in the subscription period, not including shipping and tax amounts.
            // Note: For recurring payments with Express Checkout, the payment amount can be increased by no more than 20% every 180 days (starting when the profile is created).

            profileDetails.Amount = new BasicAmountType(currency, Math.Round(amount, CurrencyToDecimal(iso)).ToString(CultureInfo.InvariantCulture));

            #region update payment period, not in use
            // (Optional) The regular payment period for this schedule.
            //if (billingAmount.Value != string.Empty && billingFrequency.Value != string.Empty && totalBillingCycles.Value != string.Empty)
            //{
            //    var paymentPeriod = new BillingPeriodDetailsType_Update();
            //    //Unit for billing during this subscription period; 
            //    //required if you specify an optional trial period. 
            //    //It is one of the following values: [Day, Week, SemiMonth, Month, Year]
            //    //For SemiMonth, billing is done on the 1st and 15th of each month.
            //    //Note:
            //    //The combination of BillingPeriod and BillingFrequency cannot exceed one year.
            //    paymentPeriod.BillingPeriod = (BillingPeriodType)
            //        Enum.Parse(typeof(BillingPeriodType), billingPeriod.SelectedValue);
            //    // Number of billing periods that make up one billing cycle; 
            //    // required if you specify an optional trial period.
            //    // The combination of billing frequency and billing period must be 
            //    // less than or equal to one year. For example, if the billing cycle is Month,
            //    // the maximum value for billing frequency is 12. Similarly, 
            //    // if the billing cycle is Week, the maximum value for billing frequency is 52.
            //    // Note:
            //    // If the billing period is SemiMonth, the billing frequency must be 1.
            //    paymentPeriod.BillingFrequency = Convert.ToInt32(billingFrequency.Value);
            //    //Billing amount for each billing cycle during this payment period; 
            //    //required if you specify an optional trial period. 
            //    //This amount does not include shipping and tax amounts.
            //    //Note:
            //    //All amounts in the CreateRecurringPaymentsProfile request must have 
            //    //the same currency.
            //    //Character length and limitations: 
            //    //Value is a positive number which cannot exceed $10,000 USD in any currency. 
            //    //It includes no currency symbol. 
            //    //It must have 2 decimal places, the decimal separator must be a period (.),
            //    //and the optional thousands separator must be a comma (,).
            //    paymentPeriod.Amount = new BasicAmountType(currency, billingAmount.Value);

            //    paymentPeriod.TotalBillingCycles = Convert.ToInt32(totalBillingCycles.Value);
            //    //(Optional) Shipping amount for each billing cycle during this payment period.
            //    //Note:
            //    //All amounts in the request must have the same currency.
            //    if (trialShippingAmount.Value != string.Empty)
            //    {
            //        paymentPeriod.ShippingAmount = new BasicAmountType(currency, shippingAmount.Value);
            //    }
            //    //(Optional) Tax amount for each billing cycle during this payment period.
            //    //Note:
            //    //All amounts in the request must have the same currency.
            //    //Character length and limitations: 
            //    //Value is a positive number which cannot exceed $10,000 USD in any currency.
            //    //It includes no currency symbol. It must have 2 decimal places, 
            //    //the decimal separator must be a period (.), and the optional 
            //    //thousands separator must be a comma (,).
            //    if (trialTaxAmount.Value != string.Empty)
            //    {
            //        paymentPeriod.TaxAmount = new BasicAmountType(currency, taxAmount.Value);
            //    }
            //    profileDetails.PaymentPeriod = paymentPeriod;
            //} 
            #endregion
        }

        //public  bool UpdateRecurringProfilePeriod()
        //{
        //    try
        //    {
        //        var profiles = OrderLinesViewRepository.GetMany(x=>x.OrderStatusId==(byte)BillingEnums.eOrderStatuses.ACTIVE 
        //                                                        && x.PaymentTermId==(byte)BillingEnums.eOrderLineTypes.SUBSCRIPTION 
        //                                                        && x.PaymentMethodId==(byte)BillingEnums.ePaymentMethods.Paypal).ToList();
        //         var currency = (CurrencyCodeType)Enum.Parse(typeof(CurrencyCodeType), "USD");
        //        foreach (var line in profiles)
        //        {
        //            var request = new UpdateRecurringPaymentsProfileRequestType();

        //            var profileDetails = new UpdateRecurringPaymentsProfileRequestDetailsType();
        //            request.UpdateRecurringPaymentsProfileRequestDetails = profileDetails;

        //            // (Required) Recurring payments profile ID returned in the CreateRecurringPaymentsProfile response.
        //            profileDetails.ProfileID = line.PaypalProfileID;

        //            var paymentPeriod = new BillingPeriodDetailsType_Update
        //            {
        //                BillingPeriod    = BillingPeriodType.MONTH,
        //                BillingFrequency = 1,
        //                Amount           = new BasicAmountType(currency, Math.Round(line.Price, 2).ToString())
        //            };

        //            profileDetails.PaymentPeriod = paymentPeriod;

        //            var wrapper = new UpdateRecurringPaymentsProfileReq
        //            {
        //                UpdateRecurringPaymentsProfileRequest = request
        //            };

        //            var service = PayPalConfiguration.PayPalMerchantAPIService;
        //            var updatePrrofileResponse = service.UpdateRecurringPaymentsProfile(wrapper);

        //            if (updatePrrofileResponse.Ack != AckCodeType.SUCCESS)
        //            {
        //                Logger.Error("Update profile period::" + line.PaypalProfileID + "::" + updatePrrofileResponse.Errors.FormatMerchantApiErrors());
        //            }
        //        }


        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Error("Update profile period", ex, CommonEnums.LoggerObjectTypes.PayPal);
        //        return false;
        //    }
        //}
        #endregion

        #region refund
        private bool DoRestRefund(string transaction_id, decimal amount2Refund, string iso, out string refundTrxId, out string error)
        {
            error = string.Empty;
            refundTrxId = string.Empty;
            try
            {
                var amount = new PayPal.Api.Amount
                {
                    currency = string.IsNullOrEmpty(iso) ? "USD" : iso,
                    total = $"{amount2Refund:0.00}"
                };

                // ###Refund
                // A refund transaction.
                // Use the amount to create
                // a refund object
                var refund = new Refund { amount = amount };
                // ###Sale
                // A sale transaction.
                // Create a Sale object with the
                // given sale transaction id.
                var sale = new Sale
                {
                    id = transaction_id
                };

                var apiContext = PayPalConfiguration.GetAPIContext();
                var refundedSale = sale.Refund(apiContext, refund);

                refundTrxId = refundedSale.id;

                return refundedSale.state.ToLower() == "completed";
            }
            catch (PayPalException ex)
            {
                error = ex.FormatPaypalException();
                Logger.Error("rest refund trx::paypal exception::execute payment::", ex, CommonEnums.LoggerObjectTypes.PayPal);
                return false;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("rest refund trx::", ex, CommonEnums.LoggerObjectTypes.PayPal);
                return false;
            }
        }

        private bool DoMerchantRefund(string transaction_id, decimal amount, decimal amount2Refund, string iso, out string refundTrxId, out decimal? fee, out string error)
        {
            error = string.Empty;
            refundTrxId = string.Empty;
            fee = null;
            try
            {
                // (Required) Unique identifier of the transaction to be refunded.
                // Note: Either the transaction ID or the payer ID must be specified.
                var request = new RefundTransactionRequestType { TransactionID = transaction_id };
                // Type of refund you are making. It is one of the following values:
                // * Full – Full refund (default).
                // * Partial – Partial refund.
                // * ExternalDispute – External dispute. (Value available since version 82.0)
                // * Other – Other type of refund. (Value available since version 82.0)
                if (amount2Refund < amount)
                {
                    request.RefundType = RefundType.PARTIAL;
                    // (Optional) Refund amount. The amount is required if RefundType is Partial.
                    // Note: If RefundType is Full, do not set the amount.
                    var currencyCode = Utils.ParseEnum<CurrencyCodeType>(string.IsNullOrEmpty(iso) ? "USD" : iso);
                    request.Amount = new BasicAmountType(currencyCode, $"{amount2Refund:0.00}");
                }

                // Invoke the API
                var wrapper = new RefundTransactionReq { RefundTransactionRequest = request };

                var service = PayPalConfiguration.PayPalMerchantAPIService;

                var refundTransactionResponse = service.RefundTransaction(wrapper);

                refundTrxId = refundTransactionResponse.RefundTransactionID;


                decimal refundFee;
                if (refundTransactionResponse.FeeRefundAmount?.value != null && decimal.TryParse(refundTransactionResponse.FeeRefundAmount.value, out refundFee))
                {
                    fee = refundFee;
                }


                return refundTransactionResponse.Ack == AckCodeType.SUCCESS;
            }
            catch (PayPalException ex)
            {
                error = ex.FormatPaypalException();
                Logger.Error("merchant refund trx::paypal exception::execute payment::", ex, CommonEnums.LoggerObjectTypes.PayPal);
                return false;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("merchant refund trx::", ex, CommonEnums.LoggerObjectTypes.PayPal);
                return false;
            }
        }

        public bool CancelRecurringPayments(string profileId, out string error)
        {
            error = string.Empty;

            try
            {
                var request = new ManageRecurringPaymentsProfileStatusRequestType();
                var details = new ManageRecurringPaymentsProfileStatusRequestDetailsType();
                request.ManageRecurringPaymentsProfileStatusRequestDetails = details;
                // (Required) Recurring payments profile ID returned in the CreateRecurringPaymentsProfile response.
                details.ProfileID = profileId;
                // (Required) The action to be performed to the recurring payments profile. Must be one of the following:
                // * Cancel – Only profiles in Active or Suspended state can be canceled.
                // * Suspend – Only profiles in Active state can be suspended.
                // * Reactivate – Only profiles in a suspended state can be reactivated.
                details.Action = StatusChangeActionType.CANCEL;

                // Invoke the API
                var wrapper = new ManageRecurringPaymentsProfileStatusReq
                {
                    ManageRecurringPaymentsProfileStatusRequest = request
                };

                var service = PayPalConfiguration.PayPalMerchantAPIService;
                // # API call 
                // Invoke the ManageRecurringPaymentsProfileStatus method in service wrapper object  
                var manageProfileStatusResponse = service.ManageRecurringPaymentsProfileStatus(wrapper);

                if (manageProfileStatusResponse.Ack == AckCodeType.SUCCESS) return true;

                error = manageProfileStatusResponse.Errors.Aggregate(error, (current, message) => current + ((string.IsNullOrEmpty(current) ? "" : ",") + message.ShortMessage));

                return false;
            }
            catch (PayPalException ex)
            {
                error = ex.FormatPaypalException();
                Logger.Error("cancel recurring payment::paypal exception::execute payment::", ex, CommonEnums.LoggerObjectTypes.PayPal);
                return false;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("cancel recurring payment::", ex, CommonEnums.LoggerObjectTypes.PayPal);
                return false;
            }
        }
        #endregion

        #region subscription
        private decimal GetPaymentAmount(vw_SALE_OrderLinePayments payment, out bool updateSubscriptionAmount, out string error)
        {
            updateSubscriptionAmount = false;
            error = string.Empty;
            try
            {
                if (payment.CouponInstanceId == null) return payment.TotalPrice;

                if (payment.SubscriptionMonths == null)
                {
                    error = "subscription months value missing in payment";
                    return -1;
                }

                var totalCompleted = OrderLinePaymentRepository.Count(x => x.OrderLineId == payment.OrderLineId && x.TypeId == (byte)BillingEnums.ePaymentTypes.PERIOD_SUBSCRIPTION && x.StatusId == (byte)BillingEnums.ePaymentStatuses.COMPLETED);

                updateSubscriptionAmount = totalCompleted == (int)payment.SubscriptionMonths;

                return totalCompleted < payment.SubscriptionMonths ? payment.TotalPrice : payment.Price;

            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                return -1;
            }
        }
        #endregion

        private void SavePurchaseCompleteEvent(Guid requestId, string sessionId)
        {
            try
            {
                var sourceRequest = PaypalPaymentRequestsRepository.GetById(requestId);

                if (sourceRequest != null)
                {
                    var token = sourceRequest.PaypalRequestEntity2PaypalPaymentRequestDto(requestId);
                    WriteEventRecord(token.UserId, CommonEnums.eUserEvents.PURCHASE_COMPLETE, sessionId, null, token.TrackingID, token.CourseId, token.BundleId);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("SavePurchaseCompleteEvent",ex,CommonEnums.LoggerObjectTypes.PayPal);
            }
        }

       
        #endregion

        #region IPaypalServices implementation

        #region helper

        public bool UpdatePaypalRequestStatus(Guid requestId, BillingEnums.ePaymentRequestStatus status, out string error)
        {
            try
            {
                var entity = PaypalPaymentRequestsRepository.Get(x => x.RequestId == requestId);

                if (entity == null)
                {
                    error = "entity not found";
                    return false;
                }

                entity.EntityUpdateRequestStatus(status);

                return PaypalPaymentRequestsRepository.UnitOfWork.CommitAndRefreshChanges(out error);
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("update request status::", ex, CommonEnums.LoggerObjectTypes.PayPal);
                return false;
            }
        }
        #endregion

        //paypal account payment
        public BaseRequestItemInfo GetItemInfoFromPaymentRequest(Guid requestId)
        {
            string error;
            var request = GetRequestCreatePaymentId(requestId, out error);

            if (request == null) return new BaseRequestItemInfo { IsValid = false, Message = error };

            return new BaseRequestItemInfo
            {
                PriceLineId = request.PriceLineId ?? -1
                ,
                TrackingId = request.TrackingID
                ,
                ItemName = GetItemNameByIds(request.CourseId, request.BundleId)
                ,
                IsValid = true
            };
        }

        public bool CreatePaypalAccountPayment(PayPalCreatePaymentDTO dto, out string approval_url, out string error)
        {
            approval_url = string.Empty;
            var requestId = Guid.NewGuid();

            try
            {
                var qs = "&requestId=" + requestId;

                var urls = new RedirectUrls
                {
                    return_url = dto.success_url + qs,
                    cancel_url = dto.cancel_url + qs
                };

                Courses courseEntity = null;
                if (dto.courseId != null)
                {
                    courseEntity = CourseRepository.GetById((int)dto.courseId);
                }

                CRS_Bundles bundleEntity = null;
                if (dto.bundleId != null)
                {
                    bundleEntity = BundleRepository.GetById((int)dto.bundleId);
                }

                var payment = dto.BasePaymentDto2RestApiPayment(BillingEnums.ePaymentAction.Sale, BillingEnums.ePaymentMethods.Paypal, CurrencyToBoolean(dto.currency), urls, courseEntity, bundleEntity);

                var apiContext = PayPalConfiguration.GetAPIContext();

                var createdPayment = payment.Create(apiContext);

                if (createdPayment.state != "created")
                {
                    error = "payment not created";

                    return false;
                }

                var approvalUrl = createdPayment.links.FirstOrDefault(x => x.rel == "approval_url");

                if (approvalUrl == null)
                {
                    error = "approval_url missing";
                    return false;
                }

                approval_url = approvalUrl.href;

                return CreateAccountPaymentRequest(requestId, createdPayment.id, dto, BillingEnums.ePaypalRequestTypes.ACCOUNT_PAYMENT, out error);
            }
            catch (PayPalException ex)
            {
                error = ex.FormatPaypalException();
                UpdateRequestErrorStatus(requestId, error);
                Logger.Error("create paypal payment::paypal exception::" + error, ex, CommonEnums.LoggerObjectTypes.PayPal);
                return false;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                UpdateRequestErrorStatus(requestId, error);
                Logger.Error("create paypal payment::general exception::" + error, ex, CommonEnums.LoggerObjectTypes.PayPal);
                return false;
            }
        }

        public bool ExecutePayPalPayment(Guid requestId, string payerId, out string error, string sessionId = null)
        {

            try
            {
                var request = GetRequestCreatePaymentId(requestId, out error);

                if (request == null) return false;

                var payment = new Payment { id = request.CreatePaymentId };

                var pymntExecution = new PaymentExecution { payer_id = payerId };

                var apiContext = PayPalConfiguration.GetAPIContext();

                var executedPayment = payment.Execute(apiContext, pymntExecution);

                if (executedPayment.state == "approved")
                {
                    SavePurchaseCompleteEvent(requestId, sessionId);

                    return UpdateRequestExecutionPaymentId(requestId, executedPayment.id, executedPayment.Payment2TransactionId(), out error) && UpdatePaypalRequestStatus(requestId, BillingEnums.ePaymentRequestStatus.approved, out error);
                }

                error = "execution failed";
                UpdateRequestErrorStatus(requestId, error);
                return false;
            }
            catch (PayPalException ex)
            {
                error = ex.FormatPaypalException();
                UpdateRequestErrorStatus(requestId, error);
                Logger.Error("account payment request::paypal exception::execute payment::" + error + "::" + requestId, ex, CommonEnums.LoggerObjectTypes.PayPal);
                return false;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                UpdateRequestErrorStatus(requestId, error);
                Logger.Error("account payment request::execute payment::" + requestId, ex, CommonEnums.LoggerObjectTypes.PayPal);
                return false;
            }
        }

        //direct credit card payment
        public bool ExecuteDirectCreditCardPayment(PaypalCreditCardPaymentDTO dto, out Guid requestId, out string error)
        {
            requestId = Guid.NewGuid();

            try
            {
                var email = dto.GetCurrentUserEmail();

                var payer = dto.CreditCard2RestApiPayer(email);

                Courses courseEntity = null;

                if (dto.courseId != null)
                {
                    courseEntity = CourseRepository.GetById((int)dto.courseId);
                }

                CRS_Bundles bundleEntity = null;

                if (dto.bundleId != null)
                {
                    bundleEntity = BundleRepository.GetById((int)dto.bundleId);
                }

                var payment = dto.BasePaymentDto2RestApiPayment(BillingEnums.ePaymentAction.Sale, BillingEnums.ePaymentMethods.Credit_Card,CurrencyToBoolean(dto.currency) ,null, courseEntity, bundleEntity, payer);

                if (!CreateCcPaymentRequest(requestId, dto, BillingEnums.ePaypalRequestTypes.CC_DIRECT, out error)) return false;

                var apiContext = PayPalConfiguration.GetAPIContext();

                var executedPayment = payment.Create(apiContext);

                if (executedPayment.state == "approved")
                {
                    // var paymentDetails = Payment.Get(apiContext, executedPayment.id);

                    return UpdateRequestExecutionPaymentId(requestId, executedPayment.id, executedPayment.Payment2TransactionId(), out error) && UpdatePaypalRequestStatus(requestId, BillingEnums.ePaymentRequestStatus.approved, out error);
                }

                error = "execution failed";
                UpdateRequestErrorStatus(requestId, error);
                return false;
            }
            catch (PayPalException ex)
            {
                error = ex.FormatPaypalException();
                UpdateRequestErrorStatus(requestId, error);
                Logger.Error("direct cc payment::paypal exception::execute payment::" + error, ex, CommonEnums.LoggerObjectTypes.PayPal);
                return false;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                UpdateRequestErrorStatus(requestId, error);
                Logger.Error("direct cc payment::execute payment::", ex, CommonEnums.LoggerObjectTypes.PayPal);
                return false;
            }
        }

        //saved credit card services
        public bool SaveCreditCard2Paypal(CreditCardDTO dto, int userId, out Guid instrumentId, out string error)
        {
            instrumentId = Guid.Empty;
            try
            {

                if (dto.BillingAddress == null || dto.BillingAddress.AddressId < 0)
                {
                    error = "address required";
                    return false;
                }

                var requestId = Guid.NewGuid();

                if (!CreateSaveCreditCardRequest(requestId, out error)) return false;

                var cc = dto.CreditCardDto2RestApiCreditCard();

                var apiContext = PayPalConfiguration.GetAPIContext();

                var response = cc.Create(apiContext);

                if (response.state != "ok")
                {
                    error = "create cc api failed";
                    UpdateRequestErrorStatus(requestId, error);
                    return false;
                }
                var card_id = response.id;
                instrumentId = Guid.NewGuid();
                return SaveUserCreditCard(userId, card_id, dto.CardNumber, dto.Type.ToString(), instrumentId, dto.BillingAddress.AddressId, out error) && UpdatePaypalRequestStatus(requestId, BillingEnums.ePaymentRequestStatus.approved, out error);
            }
            catch (PayPalException ex)
            {
                error = ex.FormatPaypalException();
                Logger.Error("save cc ::paypal exception::" + error, ex, CommonEnums.LoggerObjectTypes.PayPal);
                return false;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("save cc ::", ex, CommonEnums.LoggerObjectTypes.PayPal);
                return false;
            }
        }

        public bool ExecuteSavedCreditCardPayment(PaypalCreditCardPaymentDTO dto, int userId, Guid instrumentId, out Guid requestId, out string error)
        {
            requestId = Guid.NewGuid();

            try
            {
                var cardId = GetStoredCardPaypalToken(instrumentId, out error);

                if (string.IsNullOrEmpty(cardId)) return false;

                var payer = cardId.CreditCard2RestApiPayer();

                Courses courseEntity = null;

                if (dto.courseId != null)
                {
                    courseEntity = CourseRepository.GetById((int)dto.courseId);
                }

                CRS_Bundles bundleEntity = null;

                if (dto.bundleId != null)
                {
                    bundleEntity = BundleRepository.GetById((int)dto.bundleId);
                }

                var payment = dto.BasePaymentDto2RestApiPayment(BillingEnums.ePaymentAction.Sale, BillingEnums.ePaymentMethods.Credit_Card, CurrencyToBoolean(dto.currency),null, courseEntity, bundleEntity, payer);

                if (!CreateCcPaymentRequest(requestId, dto, BillingEnums.ePaypalRequestTypes.CC_PAY_WITH_SAVED, out error)) return false;

                var apiContext = PayPalConfiguration.GetAPIContext();

                var card = CreditCard.Get(apiContext, payer.funding_instruments[0].credit_card_token.credit_card_id);

                if (card.state.ToLower() != "ok")
                {
                    error = "credit card state " + card.state;
                    return false;
                }

                var executedPayment = payment.Create(apiContext);

                if (executedPayment.state == "approved")
                {
                    // Sale selling = Sale.Get(apiContext, executedPayment.Payment2TransactionId());
                    //var paymentDetails = Payment.Get(apiContext, executedPayment.id);
                    // var pd = GetTransactionDetails(executedPayment.Payment2TransactionId());
                    return UpdateRequestExecutionPaymentId(requestId, executedPayment.id, executedPayment.Payment2TransactionId(), out error) && UpdatePaypalRequestStatus(requestId, BillingEnums.ePaymentRequestStatus.approved, out error);
                }

                error = "execution failed";
                UpdateRequestErrorStatus(requestId, error);
                return false;
            }
            catch (PayPalException ex)
            {
                error = ex.FormatPaypalException();
                UpdateRequestErrorStatus(requestId, error);
                Logger.Error("direct cc payment::paypal exception::execute payment::" + error, ex, CommonEnums.LoggerObjectTypes.PayPal);
                requestId = Guid.Empty;
                return false;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                UpdateRequestErrorStatus(requestId, error);
                Logger.Error("direct cc payment::execute payment::", ex, CommonEnums.LoggerObjectTypes.PayPal);
                return false;
            }
        }

        public bool ExecuteSubscriptionScheduledPaymentWithStoredCreditCard(DateTime? scheduledDate, out string error)
        {
            error = string.Empty;
            try
            {
                var today = DateTime.Now.Date;
                var list = OrderLinePaymentsViewRepository.GetMany(x => (x.ScheduledDate <= (scheduledDate ?? today))
                                                                    && (x.StatusId == (byte)BillingEnums.ePaymentStatuses.SCHEDULED)
                                                                    && (x.OrderStatusId == (byte)BillingEnums.eOrderStatuses.ACTIVE)
                                                                    && (x.PaymentMethodId == (byte)BillingEnums.ePaymentMethods.Saved_Instrument))
                                                            .Select(x => x.PaymentEntity2ScheduledPaymentSummaryToken())
                                                            .OrderByDescending(x => x.ScheduledDate)
                                                            .ToList();


                foreach (var payment in list)
                {
                    ExecuteSubscriptionScheduledPaymentWithStoredCreditCard(payment.PaymentId, out error);
                }

                return true;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("ExecuteSubscriptionScheduledPaymentWithStoredCreditCard::payment scheduled date::" + scheduledDate, ex, CommonEnums.LoggerObjectTypes.PayPal);
                return false;
            }
        }

        public bool ExecuteSubscriptionScheduledPaymentWithStoredCreditCard(int paymentId, out string error)
        {
            var requestId = Guid.NewGuid();
            try
            {
                #region validate and set params
                var paymentEntity = OrderLinePaymentsViewRepository.Get(x => x.PaymentId == paymentId);

                if (paymentEntity == null)
                {
                    error = "payment entity not found";
                    return false;
                }

                if (paymentEntity.ScheduledDate > DateTime.Now.Date)
                {
                    error = "Scheduled Date is grater as Today";
                    return false;
                }

                if (paymentEntity.InstrumentId == null)
                {
                    error = "payment instrumentId not found";
                    return false;
                }

                var cardId = GetStoredCardPaypalToken((Guid)paymentEntity.InstrumentId, out error);

                if (string.IsNullOrEmpty(cardId))
                {
                    error = "credit card not found";
                    return false;
                }

                #endregion

                #region set objects
                var payer = cardId.CreditCard2RestApiPayer();

                Courses courseEntity = null;

                if (paymentEntity.CourseId != null)
                {
                    courseEntity = CourseRepository.GetById((int)paymentEntity.CourseId);
                }

                CRS_Bundles bundleEntity = null;

                if (paymentEntity.BundleId != null)
                {
                    bundleEntity = BundleRepository.GetById((int)paymentEntity.BundleId);
                }

                var desc = $"{paymentEntity.ItemName} - payment {paymentEntity.PaymentNumber}";

                var dto = paymentEntity.PaymentEntity2SubscriptionWithSavedCardDto(desc, (Guid)paymentEntity.InstrumentId);

                var payment = dto.BasePaymentDto2RestApiPayment(BillingEnums.ePaymentAction.Sale, BillingEnums.ePaymentMethods.Credit_Card, CurrencyToBoolean(dto.currency),null, courseEntity, bundleEntity, payer);

                if (!CreateCcPaymentRequest(requestId, dto, paymentEntity.Amount, BillingEnums.ePaypalRequestTypes.SUBSCRIPTION_PAYMENT_WITH_SAVED_CC, out error))
                {
                    SendAdminMail("Subscription Payment with Saved Credit Card error", "payment request creation failed", error);
                    return false;
                }
                #endregion

                #region execute payment

                var apiContext = PayPalConfiguration.GetAPIContext();

                var card = CreditCard.Get(apiContext, payer.funding_instruments[0].credit_card_token.credit_card_id);

                if (card.state.ToLower() != "ok")
                {
                    //TODO Suspend order? check logic
                    error = "credit card state " + card.state;
                    SendAdminMail("Subscription Payment with Saved Credit Card error", "credit card state not valid", error);
                    return false;
                }

                var executedPayment = payment.Create(apiContext);

                if (executedPayment.state == "approved")
                {
                    var updated = UpdateRequestExecutionPaymentId(requestId, executedPayment.id, executedPayment.Payment2TransactionId(), out error)
                                    && UpdatePaypalRequestStatus(requestId, BillingEnums.ePaymentRequestStatus.approved, out error)
                                    && UpdateSubscriptionPayment(paymentId, DateTime.Now, BillingEnums.ePaymentStatuses.COMPLETED, out error);

                    if (!updated)
                    {
                        SendAdminMail("Subscription Payment with Saved Credit Card error", "update request state", error);
                    }
                }
                else
                {
                    //TODO Suspend order? check logic
                    error = "payment execution failed";
                    SendAdminMail("Subscription Payment with Saved Credit Card error", "execution state " + executedPayment.state, error);
                    return false;
                }
                #endregion

                #region save trx

                var transactionSaved = SaveSaleTransaction(paymentEntity.OrderLineId,
                                                          paymentId,
                                                          null,
                                                          BillingEnums.eTransactionTypes.PeriodSubscriptionPayment,
                                                          paymentEntity.Amount,
                                                          DateTime.Now,
                                                          executedPayment.Payment2TransactionId(),
                                                          null,
                                                          requestId,
                                                          Utils.GetEnumDescription(BillingEnums.eTransactionTypes.PeriodSubscriptionPayment) + ":: subscription payment " + paymentEntity.PaymentNumber,
                                                          null,
                                                          out error);

                if (!transactionSaved)
                {
                    SendAdminMail("Subscription Payment with Saved Credit Card::create payment sale transaction failed", "subscription payment", error);
                    return false;
                }
                #endregion

                #region create next payment
                int nextPaymentId;
                bool updateSubscription;
                var nextPaymentAmount = GetPaymentAmount(paymentEntity, out updateSubscription, out error);

                if (nextPaymentAmount < 0)
                {
                    SendAdminMail("Subscription Payment with Saved Credit Card::next payment amount validation failed", "subscription payment", error);
                    return false;
                }

                if (CreateOrderLinePayment(paymentEntity.OrderLineId, nextPaymentAmount, null, paymentEntity.ScheduledDate.AddMonths(1), (short)(paymentEntity.PaymentNumber + 1), BillingEnums.ePaymentStatuses.SCHEDULED, BillingEnums.ePaymentTypes.PERIOD_SUBSCRIPTION, out nextPaymentId, out error)) return true;

                SendAdminMail("Subscription Payment with Saved Credit Card::create order line payment failed", "subscription payment", error);
                return false;

                #endregion
            }
            catch (PayPalException ex)
            {
                error = ex.FormatPaypalException();
                UpdateRequestErrorStatus(requestId, error);
                Logger.Error("ExecuteSubscriptionScheduledPaymentWithStoredCreditCard::paypal exception::execute payment::", ex, CommonEnums.LoggerObjectTypes.PayPal);
                return false;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("ExecuteSubscriptionScheduledPaymentWithStoredCreditCard::period payment by paymentId::execute payment::", ex, CommonEnums.LoggerObjectTypes.PayPal);
                return false;
            }
        }

        public bool ExecuteSubscriptionPaymentWithStoredCreditCard(SubscriptionWithSavedCardDTO dto, out Guid requestId, out string error)
        {
            requestId = Guid.NewGuid();

            try
            {
                var cardId = GetStoredCardPaypalToken(dto.PaymentInstrumentId, out error);

                if (string.IsNullOrEmpty(cardId))
                {
                    error = "credit card not found";
                    return false;
                }

                var payer = cardId.CreditCard2RestApiPayer();

                Courses courseEntity = null;

                if (dto.courseId != null)
                {
                    courseEntity = CourseRepository.GetById((int)dto.courseId);
                }

                CRS_Bundles bundleEntity = null;

                if (dto.bundleId != null)
                {
                    bundleEntity = BundleRepository.GetById((int)dto.bundleId);
                }

                var payment = dto.BasePaymentDto2RestApiPayment(BillingEnums.ePaymentAction.Sale, BillingEnums.ePaymentMethods.Credit_Card, CurrencyToBoolean(dto.currency),null, courseEntity, bundleEntity, payer);

                if (!CreateCcPaymentRequest(requestId, dto, dto.InitialAmount, BillingEnums.ePaypalRequestTypes.SUBSCRIPTION_PAYMENT_WITH_SAVED_CC, out error)) return false;

                var apiContext = PayPalConfiguration.GetAPIContext();

                var card = CreditCard.Get(apiContext, payer.funding_instruments[0].credit_card_token.credit_card_id);

                if (card.state.ToLower() != "ok")
                {
                    error = "credit card state " + card.state;
                    return false;
                }

                var executedPayment = payment.Create(apiContext);

                if (executedPayment.state == "approved")
                {
                    //TODO FUTUTRED USE , get trx fee, currently brings null?? (2013-12-21)
                    // Sale selling = Sale.Get(apiContext, executedPayment.Payment2TransactionId());
                    //var paymentDetails = Payment.Get(apiContext, executedPayment.id);
                    // var pd = GetTransactionDetails(executedPayment.Payment2TransactionId());
                    return UpdateRequestExecutionPaymentId(requestId, executedPayment.id, executedPayment.Payment2TransactionId(), out error) && UpdatePaypalRequestStatus(requestId, BillingEnums.ePaymentRequestStatus.approved, out error);
                }

                error = "execution failed";
                UpdateRequestErrorStatus(requestId, error);
                return false;
            }
            catch (PayPalException ex)
            {
                error = ex.FormatPaypalException();
                UpdateRequestErrorStatus(requestId, error);
                Logger.Error("ExecuteSubscriptionPaymentWithStredCreditCard::paypal exception::execute payment::", ex, CommonEnums.LoggerObjectTypes.PayPal);
                requestId = Guid.Empty;
                return false;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                UpdateRequestErrorStatus(requestId, error);
                Logger.Error("ExecuteSubscriptionPaymentWithStredCreditCard::execute payment::", ex, CommonEnums.LoggerObjectTypes.PayPal);
                requestId = Guid.Empty;
                return false;
            }
        }

        #region Recurring payments
        /// <summary>
        /// STEP I => create payment agreement in case of paypal subscription 
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="requestId"></param>
        /// <param name="approval_url"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public bool CreateRecurringPaymentAgreement(PayPalAgreementDTO dto, Guid requestId, out string approval_url, out string error)
        {
            approval_url = string.Empty;
            try
            {
                //create request record
                if (!CreateRecurringPaymentAgreementRequest(dto, requestId, out error))
                {
                    error = "save paypal request failed";
                    return false;
                }

                #region create request token
                var paymentDetail = new PaymentDetailsType
                {
                    PaymentAction = (PaymentActionCodeType)EnumUtils.GetValue("Sale", typeof(PaymentActionCodeType)),
                    OrderTotal = new BasicAmountType((CurrencyCodeType)EnumUtils.GetValue(dto.Currency.ISO, typeof(CurrencyCodeType)), "0")
                    //,NotifyURL = IPN_URL
                };

                var paymentDetails = new List<PaymentDetailsType> { paymentDetail };

                var qs = "&requestId=" + requestId;

                var ecDetails = new SetExpressCheckoutRequestDetailsType
                {
                    ReturnURL = dto.success_url + qs,
                    CancelURL = dto.cancel_url + qs,
                    PaymentDetails = paymentDetails,
                    BuyerEmail = dto.buyerEmail
                };

                const BillingCodeType billingCodeType = BillingCodeType.RECURRINGPAYMENTS;
                var baType = new BillingAgreementDetailsType(billingCodeType)
                {
                    BillingAgreementDescription = dto.description
                };
                ecDetails.BillingAgreementDetails.Add(baType);

                var request = new SetExpressCheckoutRequestType
                {
                    Version = "109.0",
                    SetExpressCheckoutRequestDetails = ecDetails
                };

                var wrapper = new SetExpressCheckoutReq { SetExpressCheckoutRequest = request };
                #endregion

                // call service
                var service = PayPalConfiguration.PayPalMerchantAPIService;
                var response = service.SetExpressCheckout(wrapper);

                //success call
                if (response.Ack == AckCodeType.SUCCESS)
                {
                    approval_url = string.Format(PayPalConfiguration.PAYPAL_AGREEMENT_CHECKOUT_URL, response.Token);

                    return UpdateRequestRecurringToken(requestId, response.Token, out error);
                }

                //in case when failed
                error = response.Errors.FormatMerchantApiErrors();
                UpdateRequestErrorStatus(requestId, error);
                return false;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("", ex, CommonEnums.LoggerObjectTypes.PayPal);
                return false;
            }
        }

        public bool ExecuteCourseSubscriptionPayalRecurringPayment(int userId, Guid sourceRequestId, string agreement_token, CreditCardDTO credit_card, out RecurringPaymentExecutionResultToken result, out string error, string sessionId = null)
        {
            result = new RecurringPaymentExecutionResultToken();
            try
            {
                PaypalPaymentRequestDTO dto;
                var requestId = Guid.NewGuid();
                //create request record
                if (!CreateRecurringPaymentAgreementRequest(requestId, sourceRequestId, out dto, out error)) return false;

                var executed = ExecuteCourseSubscriptionRecurringPayment(dto, userId, requestId, agreement_token, credit_card, out result, out error);

                if (!executed) return false;

                WriteEventRecord(userId, CommonEnums.eUserEvents.PURCHASE_COMPLETE, sessionId, null, dto.TrackingID, dto.CourseId, dto.BundleId);

                return true;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("ExecuteCourseSubscriptionRecurringPayment ", ex, CommonEnums.LoggerObjectTypes.PayPal);
                return false;
            }
        }

        /// <summary>
        /// STEP II => execute subscription payment with paypal account with agreement_token from step I
        /// Using source requestId from step I  
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="sourceRequestId"></param>
        /// <param name="agreement_token"></param>
        /// <param name="result"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public bool ExecuteCourseSubscriptionPayalRecurringPayment(int userId, Guid sourceRequestId, string agreement_token, out RecurringPaymentExecutionResultToken result, out string error)
        {
            result = new RecurringPaymentExecutionResultToken();
            try
            {
                PaypalPaymentRequestDTO dto;
                var requestId = Guid.NewGuid();
                //create request record
                return CreateRecurringPaymentAgreementRequest(requestId, sourceRequestId, out dto, out error) && ExecuteCourseSubscriptionRecurringPayment(dto, userId, requestId, agreement_token, null, out result, out error);
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("ExecuteCourseSubscriptionRecurringPayment ", ex, CommonEnums.LoggerObjectTypes.PayPal);
                return false;
            }
        }

        public bool ExecuteCourseSubscriptionCcRecurringPayment(PaypalPaymentRequestDTO dto, int userId, Guid requestId, CreditCardDTO credit_card, out RecurringPaymentExecutionResultToken result, out string error)
        {
            result = new RecurringPaymentExecutionResultToken();
            try
            {
                //create request record
                return CreateCcRecurringPaymentRequest(requestId, dto, out error) && ExecuteCourseSubscriptionRecurringPayment(dto, userId, requestId, null, credit_card, out result, out error);
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("ExecuteCourseSubscriptionRecurringPayment ", ex, CommonEnums.LoggerObjectTypes.PayPal);
                return false;
            }
        }

        /// <summary>
        /// Create subscription payment profile, when payment processing with credit card ( from user input) or paypal account
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="requestId"></param>
        /// <param name="dto"></param>
        /// <param name="agreement_token"></param>
        /// <param name="credit_card"></param>
        /// <param name="result"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public bool ExecuteCourseSubscriptionRecurringPayment(PaypalPaymentRequestDTO dto, int userId, Guid requestId, string agreement_token, CreditCardDTO credit_card, out RecurringPaymentExecutionResultToken result, out string error)
        {
            result = new RecurringPaymentExecutionResultToken();
            try
            {
                #region validations
                if (dto.CourseId == null && dto.BundleId == null)
                {
                    error = "courseId or bundleId missing in source request";
                    return false;
                }

                switch (dto.PaymentMethod)
                {
                    case BillingEnums.ePaymentMethods.Paypal:
                        if (string.IsNullOrEmpty(agreement_token))
                        {
                            error = "agreement_token details required";
                            return false;
                        }
                        break;
                    case BillingEnums.ePaymentMethods.Credit_Card:
                        if (credit_card == null)
                        {
                            error = "credit card details required";
                            return false;
                        }
                        break;
                    default:
                        error = "payment method not supported";
                        return false;

                }

                var user = UserRepository.GetById(userId);

                if (user == null)
                {
                    error = "user entity not found";
                    return false;
                }

                string itemName;

                if (dto.CourseId != null)
                {
                    var courseEntity = CourseRepository.GetById((int)dto.CourseId);

                    if (courseEntity == null)
                    {
                        error = "course entity not found";
                        return false;
                    }

                    itemName = courseEntity.CourseName;
                }
                else if (dto.BundleId != null)
                {
                    var bundleEntity = BundleRepository.GetById((int)dto.BundleId);

                    if (bundleEntity == null)
                    {
                        error = "course entity not found";
                        return false;
                    }

                    itemName = bundleEntity.BundleName;
                }
                else
                {
                    error = "courseId or bundleId missing in source request";
                    return false;
                }

                #endregion

                //create recurring payment token from request entity
                var token = new ReccuringPaymentDTO
                {
                    PaymentMethod = BillingEnums.ePaymentMethods.Paypal
                    ,
                    billingAmount = dto.Amount
                    ,
                    billingPeriod = BillingEnums.eBillingPeriodType.MONTH
                    ,
                    billingStartDate = DateTime.Now.ToBillingStartDate()
                    ,
                    initialAmount = dto.Amount.CalculateInitialCourseMonthlySubscriptionAmount()
                    ,
                    profileDescription = itemName.ItemName2SubscriptionDescription()
                    ,
                    subscriberName = user.Entity2FullName()
                    ,
                    creditCard = credit_card
                    ,
                    Currency = GetItemPriceCurrencyToken(dto.PriceLineId)
                };

                //create merchant api recurring payment request
                var createRPProfileRequest = PopulateReccuringRequest(agreement_token, token, out error);

                if (createRPProfileRequest == null) return false;


                var service = PayPalConfiguration.PayPalMerchantAPIService;

                // Invoke the API
                var wrapper = new CreateRecurringPaymentsProfileReq
                {
                    CreateRecurringPaymentsProfileRequest = createRPProfileRequest
                };

                // # API call 
                // Invoke the CreateRecurringPaymentsProfile method in service wrapper object  
                var response = service.CreateRecurringPaymentsProfile(wrapper);

                //success call
                if (response.Ack == AckCodeType.SUCCESS)
                {
                    result.ProfileDescription = token.profileDescription;
                    result.BillingAmount = token.billingAmount;
                    result.InitialAmount = token.initialAmount;
                    result.PaypalProfileId = response.CreateRecurringPaymentsProfileResponseDetails.ProfileID;
                    result.FirstBillingDate = token.billingStartDate;
                    result.Success = true;
                    result.RequestId = requestId;

                    return UpdateRequestRecurringToken(requestId, response.CreateRecurringPaymentsProfileResponseDetails.ProfileID, out error);
                }

                //in case when failed
                error = response.Errors.FormatMerchantApiErrors();
                UpdateRequestErrorStatus(requestId, error);
                return false;

            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("execute ", ex, CommonEnums.LoggerObjectTypes.PayPal);
                return false;
            }
        }
        #endregion

        #region future use
        //public bool CreatePaymentAgreement(PayPalAgreementDTO dto, out string approval_url, out string error)
        //{
        //    approval_url = string.Empty;
        //    var requestId = Guid.NewGuid();
        //    error = string.Empty;
        //    try
        //    {
        //        ////create request record
        //        //if (!CreatePaymentAgreementRequest(requestId, out error)) return false;

        //        //#region create request token
        //        //// Create request object
        //        //var request = new SetCustomerBillingAgreementRequestType();

        //        //var requestDetails = new SetCustomerBillingAgreementRequestDetailsType
        //        //{
        //        //    // (Optional) Email address of the buyer as entered during checkout. PayPal uses this value to pre-fill the PayPal membership sign-up portion of the PayPal login page.
        //        //    BuyerEmail = dto.buyerEmail,
        //        //    // (Required) URL to which the buyer's browser is returned after choosing to pay with PayPal.
        //        //    // Note: PayPal recommends that the value be the final review page on which the buyer confirms the billing agreement.
        //        //    ReturnURL = dto.success_url,
        //        //    // (Required) URL to which the customer is returned if he does not approve the use of PayPal to pay you.
        //        //    // Note: PayPal recommends that the value be the original page on which the customer chose to pay with PayPal or establish a billing agreement.
        //        //    CancelURL = dto.cancel_url
        //        //};


        //        //var baDetails = new BillingAgreementDetailsType
        //        //{
        //        //    // (Required) Details of the billing agreement such as the billing type, billing agreement description, and payment type.
        //        //    // Description of goods or services associated with the billing agreement. This field is required for each recurring payment billing agreement. PayPal recommends that the description contain a brief summary of the billing agreement terms and conditions. For example, buyer is billed at "9.99 per month for 2 years."
        //        //    BillingAgreementDescription = dto.description,
        //        //    // (Required) Type of billing agreement. For recurring payments, this field must be set to RecurringPayments. In this case, you can specify up to ten billing agreements. Other defined values are not valid.
        //        //    // Type of billing agreement for reference transactions. You must have permission from PayPal to use this field. This field must be set to one of the following values:
        //        //    // * MerchantInitiatedBilling- PayPal creates a billing agreement for each transaction associated with buyer. You must specify version 54.0 or higher to use this option.
        //        //    // * MerchantInitiatedBillingSingleAgreement- PayPal creates a single billing agreement for all transactions associated with buyer. Use this value unless you need per-transaction billing agreements. You must specify version 58.0 or higher to use this option.
        //        //    BillingType = BillingCodeType.MERCHANTINITIATEDBILLING
        //        //};

        //        //requestDetails.BillingAgreementDetails = baDetails;
        //        //request.SetCustomerBillingAgreementRequestDetails = requestDetails;

        //        //// Invoke the API
        //        //var wrapper = new SetCustomerBillingAgreementReq {SetCustomerBillingAgreementRequest = request};

        //        //#endregion

        //        //// call service
        //        //var service = PayPalConfiguration.PayPalMerchantAPIService;
        //        //var response = service.SetCustomerBillingAgreement(wrapper);

        //        ////success call
        //        //if (response.Ack == AckCodeType.SUCCESS)
        //        //{
        //        //    approval_url = String.Format(PayPalConfiguration.PAYPAL_AGREEMENT_CHECKOUT_URL, response.Token);

        //        //    return UpdateRequestReccuringToken(requestId, response.Token, out error);
        //        //}

        //        ////in case when failed
        //        //error = response.Errors.FormatMerchantApiErrors();
        //        //UpdateRequestErrorStatus(requestId,error);
        //        return false;
        //    }
        //    catch (Exception ex)
        //    {
        //        error = Utils.FormatError(ex);
        //        Logger.Error("", ex, CommonEnums.LoggerObjectTypes.PayPal);
        //        return false;
        //    }
        //}
        #endregion

        #endregion

        #region IPaypalManageServies implementation
        public PaypalRecurringProfileToken GetSubscriptionProfileDetails(string profileId, out string error)
        {
            error = string.Empty;
            try
            {
                var request = new GetRecurringPaymentsProfileDetailsRequestType { ProfileID = profileId };

                var wrapper = new GetRecurringPaymentsProfileDetailsReq
                {
                    GetRecurringPaymentsProfileDetailsRequest = request
                };

                var service = PayPalConfiguration.PayPalMerchantAPIService;

                var response = service.GetRecurringPaymentsProfileDetails(wrapper);

                return response.PaypalResponse2PaypalRecurringProfileToken();
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                return null;
            }

        }

        public List<PaymentInstrumentViewDTO> GetUserPaymentInstruments(int userId)
        {
            try
            {
                var cards = UserPaymentInstrumentsRepository.GetMany(x => x.UserId == userId && x.PaymentMethodId == (short)BillingEnums.ePaymentMethods.Credit_Card && x.IsActive).ToList();

                var apiContext = PayPalConfiguration.GetAPIContext();

                var list = new List<PaymentInstrumentViewDTO>(); // (from card in cards let cc = CreditCard.Get(apiContext, card.PaypalCcToken) select cc.RectApiCreditCard2PaymentInstrumentDto(card.InstrumentId, card.DisplayName)).ToList();

                foreach (var card in cards)
                {
                    try
                    {
                        var cc = CreditCard.Get(apiContext, card.PaypalCcToken);

                        if (cc.state.ToLower() != "ok") continue;

                        list.Add(cc.RectApiCreditCard2PaymentInstrumentDto(card.InstrumentId, card.DisplayName));
                    }
                    catch (Exception)
                    {/**/}
                }

                return list.ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("get user payment instruments", ex, CommonEnums.LoggerObjectTypes.PayPal);
                return new List<PaymentInstrumentViewDTO>();
            }
        }

        public bool DeleteCreditCard(Guid paymentInstrumentId, out string error)
        {
            try
            {
                var entity = UserPaymentInstrumentsRepository.GetById(paymentInstrumentId);

                if (entity == null)
                {
                    error = "entity not found";
                    return false;
                }

                var cc = new CreditCard { id = entity.PaypalCcToken };
                var apiContext = PayPalConfiguration.GetAPIContext();
                cc.Delete(apiContext);

                entity.UpdateCardToken(null, false);

                return UserPaymentInstrumentsRepository.UnitOfWork.CommitAndRefreshChanges(out error);
            }
            catch (PayPalException ex)
            {
                error = ex.FormatPaypalException();
                Logger.Error("delete cc ::paypal exception::execute payment::", ex, CommonEnums.LoggerObjectTypes.PayPal);
                return false;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("delete cc::", ex, CommonEnums.LoggerObjectTypes.PayPal);
                return false;
            }
        }

        public bool RefundPaymentTransaction(RefundOrderLinePaymentDTO token, out string error)
        {
            int refundId;
            return RefundPaymentTransaction(token, out refundId, out error);
        }

        public bool RefundPaymentTransaction(RefundOrderLinePaymentDTO token, out int refundId, out string error)
        {
            refundId = -1;
            try
            {
                //0. validate
                var refundedPaymentEntity = OrderLinePaymentsViewRepository.Get(x => x.PaymentId == token.PaymentId);

                if (refundedPaymentEntity == null)
                {
                    error = "payment entity not found";
                    return false;
                }

                if (token.TransactionId == null)
                {
                    error = "transactionId required";
                    return false;
                }

                var refundedTransactionEntity = TransactionRepository.GetById((int)token.TransactionId);

                if (refundedTransactionEntity == null)
                {
                    error = "transaction entity not found";
                    return false;
                }

                if (refundedTransactionEntity.RequestId == null)
                {
                    error = "reference requestId missing";
                    return false;
                }

                var sourceRequestEntity = PaypalPaymentRequestsRepository.GetById((Guid)refundedTransactionEntity.RequestId);

                if (sourceRequestEntity == null)
                {
                    error = "source request entity not found";
                    return false;
                }

                //1. get trx type : direct payment/subscription/saved card subscription
                var type = Utils.ParseEnum<BillingEnums.eTransactionTypes>(refundedTransactionEntity.TransactionTypeId.ToString());
                string refundTrxId;
                var requestId = Guid.NewGuid();
                decimal? fee = null;
                BillingEnums.ePaypalRequestTypes paypalRequestType;
                string profileId = null;
                switch (type)
                {
                    // course purchases with REST API
                    case BillingEnums.eTransactionTypes.DirectCreditCardPayment:
                    case BillingEnums.eTransactionTypes.SavedCreditCardPayment:
                    case BillingEnums.eTransactionTypes.DirectPaymentTransaction:
                        paypalRequestType = BillingEnums.ePaypalRequestTypes.REST_API_REFUND;
                        break;
                    case BillingEnums.eTransactionTypes.InitialSubscriptionPayment:
                    case BillingEnums.eTransactionTypes.PeriodSubscriptionPayment:

                        var trxInfo = TransactionsViewRepository.Get(x => x.TransactionId == token.TransactionId);

                        if (trxInfo == null)
                        {
                            error = "Transaction Info not found";
                            return false;
                        }

                        profileId = refundedPaymentEntity.PaypalProfileID;

                        var paymentMethod = Utils.ParseEnum<BillingEnums.ePaymentMethods>(trxInfo.PaymentMethodId.ToString());

                        paypalRequestType = paymentMethod == BillingEnums.ePaymentMethods.Saved_Instrument ? BillingEnums.ePaypalRequestTypes.REST_API_REFUND : BillingEnums.ePaypalRequestTypes.MERCHANT_API_REFUND;

                        break;
                    default:
                        error = "Unsupportable transaction type";
                        return false;
                }

                //create paypal request record
                if (!CreatePaymentRequestFromSourceRequest(requestId, sourceRequestEntity, token.Amount, paypalRequestType, out error)) return false;

                //do refund
                var result = paypalRequestType == BillingEnums.ePaypalRequestTypes.REST_API_REFUND ? DoRestRefund(refundedTransactionEntity.ExternalTransactionID, token.Amount, refundedPaymentEntity.ISO, out refundTrxId, out error) :
                                                                                                        DoMerchantRefund(refundedTransactionEntity.ExternalTransactionID, refundedTransactionEntity.Amount, token.Amount, refundedPaymentEntity.ISO, out refundTrxId, out fee, out error);
                if (!result)
                {
                    UpdateRequestErrorStatus(requestId, error ?? "unexpected API error");
                    return false;
                }

                UpdatePaypalRequestStatus(requestId, BillingEnums.ePaymentRequestStatus.approved, out error);

                //save transaction record

                if (!CreateOrderLinePaymentRefund(refundedPaymentEntity.PaymentId, token.Amount, DateTime.Now, token.Amount < refundedTransactionEntity.Amount ? BillingEnums.ePaymentTypes.PARTIAL_REFUND : BillingEnums.ePaymentTypes.REFUND, out refundId, out error))
                {
                    SendAdminMail("Refund::create refund record failed", "subscription payment", error);
                    return false;
                }

                var trxType = token.Amount < refundedTransactionEntity.Amount ? BillingEnums.eTransactionTypes.PartialRefund : BillingEnums.eTransactionTypes.Refund;

                var transactionSaved = SaveSaleTransaction(refundedPaymentEntity.OrderLineId,
                                                            null,
                                                            refundId,
                                                            trxType,
                                                            token.Amount,
                                                            DateTime.Now,
                                                            refundTrxId,
                                                            fee ?? 0,
                                                            requestId,
                                                            Utils.GetEnumDescription(trxType) + ":: refund",
                                                            refundedTransactionEntity.TransactionId,
                                                            out error);

                if (!transactionSaved)
                {
                    SendAdminMail("Refund::create transaction failed", "subscription payment", error);
                    return false;
                }

                //var trxEntity = refundedTransactionEntity.PaypalRequestEntity2UserRefundTransactions(sourceRequestEntity, requestId, refundTrxId, token.Amount, fee ?? 0);

                return !token.DeniedAccess || string.IsNullOrEmpty(profileId) || CancelRecurringPayments(profileId, out error);
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("refund trx::", ex, token.PaymentId, CommonEnums.LoggerObjectTypes.PayPal);
                return false;
            }
        }

        public bool CancelSubscription(int lineId, out string error)
        {
            try
            {
                var orderLineEntity = OrderLinesViewRepository.Get(x => x.LineId == lineId);

                if (orderLineEntity == null)
                {
                    error = "order line entity not found";
                    return false;
                }

                if (string.IsNullOrEmpty(orderLineEntity.PaypalProfileID))
                {
                    error = "PaypalProfileID not found";
                    return false;
                }

                var requestId = Guid.NewGuid();

                //create paypal request record
                if (!CreatePaymentRequestFromOrderLine(requestId, orderLineEntity, BillingEnums.ePaypalRequestTypes.RECURRING_PAYMENT_CANCEL, out error)) return false;

                if (!CancelRecurringPayments(orderLineEntity.PaypalProfileID, out error))
                {
                    UpdateRequestErrorStatus(requestId, error);
                    return false;
                }

                UpdatePaypalRequestStatus(requestId, BillingEnums.ePaymentRequestStatus.approved, out error);

                var transactionSaved = SaveSaleTransaction(orderLineEntity.LineId,
                                                            null,
                                                            null,
                                                            BillingEnums.eTransactionTypes.CancelSubscription,
                                                            0,
                                                            DateTime.Now,
                                                            string.Empty,
                                                            0,
                                                            requestId,
                                                            Utils.GetEnumDescription(BillingEnums.eTransactionTypes.CancelSubscription) + ":: cancel subscription",
                                                            null,
                                                            out error);

                if (transactionSaved) return true;

                SendAdminMail("Cancel Subscription::" + orderLineEntity.PaypalProfileID + "::create transaction failed::" + orderLineEntity.OrderNumber, "subscription cancel", error);

                return false;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("cancel subscription::", ex, lineId, CommonEnums.LoggerObjectTypes.PayPal);
                return false;
            }
        }

        public GetTransactionDetailsResponseType GetMerchantTransactionDetails(string trxId)
        {
            try
            {
                var request = new GetTransactionDetailsRequestType { TransactionID = trxId };
                // (Required) Unique identifier of a transaction.
                // Note: The details for some kinds of transactions cannot be retrieved with GetTransactionDetails. You cannot obtain details of bank transfer withdrawals, for example.

                // Invoke the API
                var wrapper = new GetTransactionDetailsReq { GetTransactionDetailsRequest = request };

                var service = PayPalConfiguration.PayPalMerchantAPIService;

                // # API call 
                // Invoke the GetTransactionDetails method in service wrapper object  
                var transactionDetails = service.GetTransactionDetails(wrapper);

                return transactionDetails;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public Sale GetRestSaleDetails(string trxId)
        {
            try
            {
                var apiContext = PayPalConfiguration.GetAPIContext();

                var sale = Sale.Get(apiContext, trxId);

                return sale;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public bool UpdateMerchantTrxFees(out int found, out int updated, out string error)
        {
            error = string.Empty;
            found = 0;
            updated = 0;
            try
            {
                var types = new List<byte>
                {
                    (byte) BillingEnums.eTransactionTypes.InitialSubscriptionPayment,
                    (byte) BillingEnums.eTransactionTypes.DirectCreditCardPayment,
                    (byte) BillingEnums.eTransactionTypes.SavedCreditCardPayment,
                    (byte) BillingEnums.eTransactionTypes.InitialSubscriptionPayment,
                    (byte) BillingEnums.eTransactionTypes.PeriodSubscriptionPayment,
                    (byte) BillingEnums.eTransactionTypes.DirectPaymentTransaction
                };

                var list = TransactionsViewRepository.GetMany(x => x.Fee == 0 && x.Amount > 0 &&
                                                                    x.TransactionDate.Year >= 2014 &&
                                                                    types.Contains(x.TransactionTypeId)).ToList();

                found = list.Count;

                Logger.Debug("Transaction Fee updated process::lines found " + list.Count);

                foreach (var row in list)
                {
                    Logger.Debug("Begin Fee updated for trxId " + row.ExternalTransactionID);

                    var trx = GetMerchantTrxDetails(row.ExternalTransactionID);

                    if (trx == null || trx.Ack != AckCodeType.SUCCESS)
                    {
                        Logger.Warn("Begin Fee updated for trxId " + row.ExternalTransactionID + "::error" + (trx != null ? trx.Errors.FormatMerchantApiErrors() : "get trx details failed "));
                        continue;
                    }

                    var fee = trx.PaymentTransactionDetails.PaymentInfo.FeeAmount?.value;

                    if (fee == null)
                    {
                        Logger.Warn("Fee for trxId " + row.ExternalTransactionID + " not found");
                        continue;
                    }

                    var entity = TransactionRepository.GetById(row.TransactionId);

                    if (entity == null)
                    {
                        Logger.Warn("Transaction entity for trxId " + row.ExternalTransactionID + " not found");
                        continue;
                    }

                    entity.Fee = Convert.ToDecimal(fee);

                    entity.UpdateDate = DateTime.Now;

                    if (!TransactionRepository.UnitOfWork.CommitAndRefreshChanges(out error))
                    {
                        Logger.Warn("Update for trxId " + row.ExternalTransactionID + "::" + error);
                        continue;
                    }

                    updated++;

                    Logger.Debug("Fee updated for trxId " + row.ExternalTransactionID);
                }

                return true;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("Update trx Fees", ex, CommonEnums.LoggerObjectTypes.PayPal);
                return false;
            }
        }

        //public Payment GetRestPaymentDetails(string paymentId)
        //{
        //    try
        //    {
        //        var apiContext = PayPalConfiguration.GetAPIContext();

        //        var payment = Payment.Get(apiContext, paymentId);
        //       // Sale selling = Sale.Get(apiContext, paymentId);
        //        return payment;
        //    }
        //    catch (PayPalException ex)
        //    {
        //        var error = ex.FormatPaypalException();
        //        return null;
        //    }
        //    catch (Exception ex)
        //    {
        //        return null;
        //    }
        //}

        public GetTransactionDetailsResponseType GetMerchantTrxDetails(string trxId)
        {
            try
            {
                var request = new GetTransactionDetailsRequestType { TransactionID = trxId };
                // (Required) Unique identifier of a transaction.
                // Note: The details for some kinds of transactions cannot be retrieved with GetTransactionDetails. You cannot obtain details of bank transfer withdrawals, for example.

                // Invoke the API
                var wrapper = new GetTransactionDetailsReq { GetTransactionDetailsRequest = request };

                var service = PayPalConfiguration.PayPalMerchantAPIService;

                // # API call 
                // Invoke the GetTransactionDetails method in service wrapper object  
                var transactionDetails = service.GetTransactionDetails(wrapper);

                return transactionDetails;
            }
            catch (Exception)
            {
                return null;
            }
        }
        #endregion

        #region IPaypalIpnServices implementation

        private void SaveIpnLog(IpnResponseToken response)
        {
            try
            {
                PaypalIpnLogRepository.Add(response.Response2PaypalIpnLog());
                PaypalIpnLogRepository.UnitOfWork.CommitAndRefreshChanges();
            }
            catch (Exception ex)
            {
                Logger.Error("save ipn log", ex, CommonEnums.LoggerObjectTypes.PayPal);
            }
        }

        private bool HandleRecurringProfileCancelRequest(int lineId, out string error)
        {
            error = string.Empty;
            try
            {
                return SHARED_CancelOrder(lineId, out error) && SHARED_BlockUserCourseAccess(lineId, true, out error);
            }
            catch (Exception ex)
            {
                Logger.Error("HandleProfileCancelRequest", ex, CommonEnums.LoggerObjectTypes.PayPal);
                return false;
            }
        }

        private bool HandleRecurringProfileCreationRequest(IpnResponseToken response, string profileId, SALE_OrderLines orderLineEntity, out string error)
        {
            error = string.Empty;
            try
            {
                var createTrx = true;
                #region validate required inputs
                var amount = response.initial_payment_amount;

                if (amount == null || amount == 0)
                {
                    Logger.Warn("IPN::process subscription -" + profileId + "::recurring_payment_profile_created::initial amount missing");
                    createTrx = false;
                }
                if (string.IsNullOrEmpty(response.initial_payment_status) || response.initial_payment_status != BillingEnums.ePaypalPaymentStatus.Completed.ToString())
                {
                    Logger.Warn("IPN::process subscription -" + profileId + "::recurring_payment_profile_created::initial payment status=" + response.initial_payment_status);
                    createTrx = false;
                }

                if (string.IsNullOrEmpty(response.initial_payment_txn_id))
                {
                    Logger.Warn("IPN::process subscription -" + profileId + "::recurring_payment_profile_created::initial payment txn_id missing");
                    createTrx = false;
                }
                #endregion

                #region find source request
                var paypalRequestEntity = PaypalPaymentRequestsRepository.Get(x => x.RecurringRequestToken == profileId && x.RequestTypeId == (byte)BillingEnums.ePaypalRequestTypes.RECURRING_PAYMENT_EXECUTION);
                if (paypalRequestEntity == null)
                {
                    Logger.Warn("IPN::process subscription -" + profileId + "::recurring_payment_profile_created::source paypal request entity not found");

                    return false;
                }

                Guid? requestId = paypalRequestEntity.RequestId;
                #endregion

                #region get subscription payment row
                var paymentId = GetSubscriptionPaymentId(orderLineEntity.LineId, 1);

                if (paymentId < 0)
                {
                    error = "initial payment entity not found";
                    Logger.Warn("IPN::process subscription -" + profileId + "::recurring_payment_profile_created::save subscription payment::" + error);
                    return false;
                }
                #endregion

                #region create initial payment trx

                if (createTrx)
                {
                    var fee = response.mc_fee ?? 0;

                    var transactionSaved = SaveSaleTransaction(orderLineEntity.LineId,
                                                                paymentId,
                                                                null,
                                                                BillingEnums.eTransactionTypes.InitialSubscriptionPayment,
                                                                (decimal)amount,
                                                                DateTime.Now,
                                                                response.initial_payment_txn_id,
                                                                fee,
                                                                requestId,
                                                                Utils.GetEnumDescription(BillingEnums.eTransactionTypes.InitialSubscriptionPayment) +
                                                                ":: subscription payment",
                                                                null,
                                                                out error);

                    if (!transactionSaved)
                    {
                        Logger.Warn("IPN::process subscription -" + profileId + "::save transaction error::" + error);
                        return false;
                    }
                }
                #endregion

                if (UpdateSubscriptionPayment(paymentId, DateTime.Now, BillingEnums.ePaymentStatuses.COMPLETED, out error)) return true;

                Logger.Warn("IPN::process subscription -" + profileId + "::update payment error::" + error);

                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("HandleProfileCreationRequest", ex, CommonEnums.LoggerObjectTypes.PayPal);
                return false;
            }
        }

        private bool HandleRerccurringPaymentRequest(IpnResponseToken response, string profileId, SALE_OrderLines orderLineEntity, out string error)
        {
            error = string.Empty;
            try
            {
                #region validate required inputs
                var amount = response.mc_gross;
                if (amount == null || amount == 0)
                {
                    Logger.Warn("IPN::process subscription -" + profileId + "::recurring_payment::amount missing");

                    return false;
                }
                if (string.IsNullOrEmpty(response.payment_status) || response.payment_status != BillingEnums.ePaypalPaymentStatus.Completed.ToString())
                {
                    Logger.Warn("IPN::process subscription -" + profileId + "::recurring_payment::payment status=" + response.initial_payment_status);

                    return false;
                }

                if (string.IsNullOrEmpty(response.txn_id))
                {
                    Logger.Warn("IPN::process subscription -" + profileId + "::recurring_payment::txn_id missing");

                    return false;
                }
                var fee = response.mc_fee ?? 0;
                #endregion

                #region find source request
                var paypalRequestEntity = PaypalPaymentRequestsRepository.Get(x => x.RecurringRequestToken == profileId && x.RequestTypeId == (byte)BillingEnums.ePaypalRequestTypes.RECURRING_PAYMENT_EXECUTION);
                if (paypalRequestEntity == null)
                {
                    Logger.Warn("IPN::process subscription -" + profileId + "::recurring_payment::source paypal request entity not found");

                    return false;
                }
                Guid? requestId = paypalRequestEntity.RequestId;
                #endregion

                #region get subscription payment row
                short paymentNum;

                var paymentId = GetScheduledSubscriptionPaymentId(orderLineEntity.LineId, out paymentNum);

                if (paymentId < 0)
                {
                    error = "payment entity not found";
                    Logger.Warn("IPN::process subscription -" + profileId + "::recurring_payment::save subscription payment::" + error);
                    return false;
                }

                var id = paymentId;
                var paymentEntity = OrderLinePaymentsViewRepository.Get(x => x.PaymentId == id);

                if (paymentEntity == null)
                {
                    error = "payment entity not found";
                    Logger.Warn("IPN::process subscription -" + profileId + "::recurring_payment::get subscription payment::" + error);
                    return false;
                }
                #endregion

                #region create trx

                var transactionSaved = SaveSaleTransaction(orderLineEntity.LineId,
                                                            paymentId,
                                                            null,
                                                            BillingEnums.eTransactionTypes.PeriodSubscriptionPayment,
                                                            (decimal)amount,
                                                            DateTime.Now,
                                                            response.txn_id,
                                                            fee,
                                                            requestId,
                                                            Utils.GetEnumDescription(BillingEnums.eTransactionTypes.PeriodSubscriptionPayment) + ":: subscription payment",
                                                            null,
                                                            out error);
                if (!transactionSaved)
                {
                    Logger.Warn("IPN::process subscription -" + profileId + "::update transaction failed");
                    return false;
                }

                var paymentDate = response.payment_date.ParsePaypalDate(DateTime.Now);

                if (!UpdateSubscriptionPayment(paymentId, paymentDate, BillingEnums.ePaymentStatuses.COMPLETED, out error))
                {
                    Logger.Warn("IPN::process subscription -" + profileId + "::update payment failed");
                    return false;
                }


                var nextBillingDate = response.next_payment_date.ParsePaypalDate(paymentDate.AddMonths(1));

                //var periodAmount = (response.amount_per_cycle ?? (decimal) amount);

                //if (periodAmount == 0)
                //{
                //    Logger.Warn("IPN::process subscription -" + profileId + "::period amount not found");

                //    return false;
                //}

                bool updateSubscription;
                var nextPaymentAmount = GetPaymentAmount(paymentEntity, out updateSubscription, out error);

                if (nextPaymentAmount < 0)
                {
                    Logger.Warn("IPN::process subscription -" + profileId + "::next payment amount calculation failure");
                    return false;
                }

                if (!updateSubscription) return CreateOrderLinePayment(orderLineEntity.LineId, nextPaymentAmount, null, nextBillingDate, (short)(paymentNum + 1), BillingEnums.ePaymentStatuses.SCHEDULED, BillingEnums.ePaymentTypes.PERIOD_SUBSCRIPTION, out paymentId, out error);

                return UpdateRecurringPaymentAgreement(paypalRequestEntity, profileId, nextPaymentAmount, OrderLineRepository.FindLineCurrencyISO(orderLineEntity.LineId), out error) && CreateOrderLinePayment(orderLineEntity.LineId, nextPaymentAmount, null, nextBillingDate, (short)(paymentNum + 1), BillingEnums.ePaymentStatuses.SCHEDULED, BillingEnums.ePaymentTypes.PERIOD_SUBSCRIPTION, out paymentId, out error);

                #endregion
            }
            catch (Exception ex)
            {
                Logger.Error("HandleRerecurringPaymentRequest", ex, CommonEnums.LoggerObjectTypes.PayPal);
                return false;
            }
        }

        private bool HandleTransactionDetailRequest(IpnResponseToken response, out string error)
        {
            try
            {
                var fee = response.mc_fee ?? (response.payment_fee ?? 0);

                if (fee == 0)
                {
                    error = "fee value equal 0";
                    return false;
                }

                var trxId = response.txn_id;

                if (string.IsNullOrEmpty(trxId))
                {
                    error = "trxId required";
                    return false;
                }

                //find transaction
                var trxEntities = TransactionRepository.GetMany(x => x.ExternalTransactionID == trxId).ToList();

                if (trxEntities.Count.Equals(0))
                {
                    error = "transaction entity not found";
                    return false;
                }

                if (trxEntities.Count > 1)
                {
                    error = "multiple transaction records found for trxId " + trxId;
                    return false;
                }

                var trxEntity = trxEntities[0];

                trxEntity.Fee = fee;
                trxEntity.UpdateDate = DateTime.Now;

                return TransactionRepository.UnitOfWork.CommitAndRefreshChanges(out error);

            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("HandleTransactionDetailRequest", ex, CommonEnums.LoggerObjectTypes.PayPal);
                return false;
            }
        }

        private bool HandleRefundTransactionRequest(IpnResponseToken response, out string error)
        {
            error = string.Empty;
            #region refund
            #region validation

            var trxRefundEntity = TransactionRepository.Get(x => x.ExternalTransactionID == response.txn_id);
            decimal mc_fee;
            if (trxRefundEntity != null)
            {
                //update fee
                mc_fee = response.mc_fee != null ? Math.Abs((decimal)response.mc_fee) : 0;

                if (mc_fee == 0) return true;

                trxRefundEntity.Fee = mc_fee;

                return TransactionRepository.UnitOfWork.CommitAndRefreshChanges(out error);
            }


            if (string.IsNullOrEmpty(response.parent_txn_id))
            {
                error = "IPN::Parent transaction for refund IPN call not found";
                return false;
            }
            if (response.mc_gross == null)
            {
                error = "IPN::amount field mc_gross empty";
                return false;
            }
            //try find refunded transaction
            var refundedTransactionEntity = TransactionsViewRepository.Get(x => x.ExternalTransactionID == response.parent_txn_id);

            if (refundedTransactionEntity == null)
            {
                error = "IPN::Parent transaction " + response.parent_txn_id + "for refund IPN call not found";
                return false;
            }

            if (refundedTransactionEntity.RequestId == null)
            {
                error = "IPN::reference requestId missing::" + response.parent_txn_id + "::" + refundedTransactionEntity.TransactionId;
                return false;
            }

            var refundedPaymentEntity = OrderLinePaymentsViewRepository.Get(x => x.PaymentId == refundedTransactionEntity.PaymentId);

            if (refundedPaymentEntity == null)
            {
                error = "IPN::payment entity not found::" + response.parent_txn_id + "::" + refundedTransactionEntity.TransactionId;
                return false;
            }
            #endregion

            var mc_gross = Math.Abs((decimal)response.mc_gross);

            #region Do refund
            int refundId;
            var refundDate = response.payment_date.ParsePaypalDate(DateTime.Now);
            if (!CreateOrderLinePaymentRefund(refundedPaymentEntity.PaymentId, mc_gross, refundDate, mc_gross < refundedTransactionEntity.Amount ? BillingEnums.ePaymentTypes.PARTIAL_REFUND : BillingEnums.ePaymentTypes.REFUND, out refundId, out error))
            {
                error = "IPN::REFUND::" + response.parent_txn_id + "::" + refundedTransactionEntity.PaymentId + ":: failed with " + error;
                return false;
            }

            var trxType = mc_gross < refundedTransactionEntity.Amount ? BillingEnums.eTransactionTypes.PartialRefund : BillingEnums.eTransactionTypes.Refund;

            mc_fee = response.mc_fee != null ? Math.Abs((decimal)response.mc_fee) : 0;

            var transactionSaved = SaveSaleTransaction(refundedPaymentEntity.OrderLineId,
                                                        null,
                                                        refundId,
                                                        trxType,
                                                        mc_gross,
                                                        DateTime.Now,
                                                        response.txn_id,
                                                        mc_fee,
                                                        null,
                                                        Utils.GetEnumDescription(trxType) + ":: refund",
                                                        refundedTransactionEntity.TransactionId,
                                                        out error);

            if (!transactionSaved)
            {
                error = "IPN::REFUND::" + response.parent_txn_id + "::" + refundedTransactionEntity.PaymentId + "::Save trx failed with " + error;
                return false;
            }
            #endregion

            #region update payment
            var paymentType = Utils.ParseEnum<BillingEnums.ePaymentTypes>(refundedPaymentEntity.PaymentTypeCode);

            switch (paymentType)
            {
                case BillingEnums.ePaymentTypes.PERIOD_SUBSCRIPTION:
                    if (refundedPaymentEntity.ScheduledDate > refundDate)
                    {
                        var paymentEntity = OrderLinePaymentRepository.GetById(refundedPaymentEntity.PaymentId);

                        paymentEntity.PaymentDate = null;
                        paymentEntity.StatusId = (byte)BillingEnums.ePaymentStatuses.SCHEDULED;
                        if (OrderLinePaymentRepository.UnitOfWork.CommitAndRefreshChanges(out error))
                        {
                            error = "IPN::REFUND::Order No" + refundedPaymentEntity.OrderNumber + "::Payment status reset to Scheduled";
                            return true;
                        }
                        error = "IPN::REFUND::update payment failed " + error;
                        return false;
                    }
                    return true;
                case BillingEnums.ePaymentTypes.ONE_TIME:
                    if (mc_gross == refundedPaymentEntity.Amount)
                    {
                        var blocked = SHARED_CancelOrder(refundedPaymentEntity.OrderLineId, out error) && SHARED_BlockUserCourseAccess(refundedPaymentEntity.OrderLineId, false, out error);

                        if (blocked) return true;

                        error = "IPN::REFUND::cancel order/block access failed with " + error;
                        return false;

                    }
                    break;
            }

            #endregion
            #endregion

            return true;
        }

        public void HandleIpnResponse(IpnResponseToken response)
        {
            Logger.Debug("IPN call::enter HandleIpnResponse::");

            SaveIpnLog(response);

            try
            {
                //check 1 - if it recurring payment
                if (response.recurring_payment_id != null)
                {
                    #region recurring payments
                    var profileId = response.recurring_payment_id;

                    //get subscription info

                    var orderLineEntity = OrderLineRepository.Get(x => x.PaypalProfileID == profileId);

                    if (orderLineEntity == null)
                    {
                        Logger.Warn("IPN::process subscription -" + profileId + "::orderLine entity not found");

                        return;
                    }

                    if (response.txn_type != null)
                    {
                        var type = Utils.ParseEnum<BillingEnums.eIpnResponseTypes>(response.txn_type);

                        bool saved;
                        string error;
                        switch (type)
                        {
                            case BillingEnums.eIpnResponseTypes.recurring_payment_profile_created:
                                saved = HandleRecurringProfileCreationRequest(response, profileId, orderLineEntity, out error);
                                if (!saved)
                                {
                                    Logger.Error("IPN:parse message failed");
                                    SendAdminMail("Profile create request failure", "recurring subscription payment", error);
                                }
                                break;
                            case BillingEnums.eIpnResponseTypes.recurring_payment:
                                saved = HandleRerccurringPaymentRequest(response, profileId, orderLineEntity, out error);
                                if (!saved)
                                {
                                    Logger.Error("IPN:parse message failed");
                                    SendAdminMail("Payment request failure", "recurring subscription payment", error);
                                }
                                break;
                            case BillingEnums.eIpnResponseTypes.recurring_payment_profile_cancel:
                                Logger.Debug("IPN call::Cancel recurring profile::" + profileId);
                                saved = HandleRecurringProfileCancelRequest(orderLineEntity.LineId, out error);
                                if (!saved)
                                {
                                    Logger.Error("IPN:parse message failed");
                                    SendAdminMail("Payment request failure", "recurring subscription payment", error);
                                }
                                else
                                {
                                    Logger.Debug("IPN call::Cancel recurring profile::" + profileId + "::success");
                                }
                                break;
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(response.reason_code) && response.reason_code.ToLower() == "refund")
                        {
                            string error;
                            var saved = HandleRefundTransactionRequest(response, out error);
                            if (saved) return;
                            Logger.Error("IPN:parse message failed");
                            SendAdminMail("Profile create request failure", "recurring subscription payment", error);
                        }
                        else
                        {
                            Logger.Warn("IPN::process subscription -" + profileId + "::unknown message kind with txn_type blank");
                        }

                    }
                    #endregion
                }
                else
                {
                    if (response.txn_type != null)
                    {
                        #region paypal/cart payments
                        try
                        {
                            var type = Utils.ParseEnum<BillingEnums.eIpnResponseTypes>(response.txn_type);
                            switch (type)
                            {
                                case BillingEnums.eIpnResponseTypes.paypal_here:
                                case BillingEnums.eIpnResponseTypes.cart:
                                    Logger.Debug("IPN call::enter HandleTransactionDetailRequest::" + response.txn_id);
                                    string error;
                                    var saved = HandleTransactionDetailRequest(response, out error);
                                    if (!saved)
                                    {
                                        Logger.Error("IPN:parse message failed");
                                        SendAdminMail("Transaction Detail request failure", response.txn_id, error);
                                    }
                                    else
                                    {
                                        Logger.Debug("IPN call::HandleTransactionDetailRequest::" + response.txn_id + "::success");
                                    }
                                    return;
                                default:
                                    Logger.Warn("IPN::non subscription response, unknown txn_type " + response.txn_type);
                                    return;
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Error("IPN::Payment::" + response.txn_type, ex, CommonEnums.LoggerObjectTypes.PayPal);
                        }
                        #endregion
                    }
                    else if (!string.IsNullOrEmpty(response.reason_code) && response.reason_code.ToLower() == "refund")
                    {
                        string error;

                        var saved = HandleRefundTransactionRequest(response, out error);

                        if (saved) return;

                        Logger.Error("IPN:parse message failed");
                        SendAdminMail("Profile create request failure", "recurring subscription payment", error);
                        return;

                    }
                    Logger.Warn("IPN::non subscription response, unknown logic");
                }
            }
            catch (Exception ex)
            {
                var error = Utils.FormatError(ex);
                Logger.Error("HandleIpnResponse::" + error, ex, CommonEnums.LoggerObjectTypes.PayPal);
            }
        }

        public void HandleIpnMasspayResponse(List<MassPaymentItemToken> list)
        {
            try
            {
                foreach (var token in list)
                {

                    int payoutId;

                    if (!int.TryParse(token.unique_id, out payoutId))
                    {
                        Logger.Warn("IPN::mass-payment::statement with " + token.unique_id + " is invalid");
                        continue;
                    }

                    var entity = UserPayoutStatmentsRepository.GetById(payoutId);

                    if (entity == null)
                    {
                        Logger.Warn("IPN::mass-payment::entity with payoutId " + payoutId + " not found");
                        continue;
                    }

                    var currentStatus = Utils.ParseEnum<BillingEnums.ePayoutStatuses>(entity.StatusId);

                    if (currentStatus != BillingEnums.ePayoutStatuses.WAIT_4_IPN) continue;

                    entity.UpdateStatetmentData(token);
                    string error;
                    if (!UserPayoutStatmentsRepository.UnitOfWork.CommitAndRefreshChanges(out error))
                    {
                        Logger.Warn("IPN::mass-payment::entity with payoutId " + payoutId + " not saved with " + error);
                    }
                }
            }
            catch (Exception ex)
            {

                var error = Utils.FormatError(ex);
                Logger.Error("HandleIpnMasspayResponse::" + error, ex, CommonEnums.LoggerObjectTypes.PayPal);
            }
        }

        #endregion

        #region IPaypalPaymentServices implementation
        public bool ExecuteMassPayment(List<MassPayRequestItemType> list, out string error)
        {
            error = string.Empty;
            try
            {
                var request = new MassPayRequestType();

                request.MassPayItem.AddRange(list);

                var wrapper = new MassPayReq { MassPayRequest = request };

                var configurationMap = PayPalConfiguration.sdkConfig;

                var service = new PayPalAPIInterfaceServiceService(configurationMap);

                var massPayResponse = service.MassPay(wrapper);

                if (massPayResponse.Ack.Equals(AckCodeType.SUCCESS)) return true;

                if (massPayResponse.Errors == null || massPayResponse.Errors.Count <= 0) return true;

                error = massPayResponse.Errors.Aggregate(error, (current, er) => current + (er.ShortMessage + ","));

                return false;
            }
            catch (PayPalException ex)
            {
                error = ex.FormatPaypalException();
                Logger.Error("ExecuteMassPayment::paypal exception", ex, CommonEnums.LoggerObjectTypes.PayPal);
                return false;
            }
            catch (Exception ex)
            {
                error = FormatError(ex);
                Logger.Error("ExecuteMassPayment::", ex, CommonEnums.LoggerObjectTypes.PayPal);
                return false;
            }
        }
        public bool ExecutePayment(PaypalPaymentExecutionToken token, out string payKey, out string error)
        {
            error = string.Empty;
            payKey = string.Empty;
            var receiverList = new ReceiverList { receiver = new List<Receiver>() };

            string[] amt = { token.Amount.FormatMoney(CurrencyToDecimal(token.Currency.ISO)).ToString(CultureInfo.InvariantCulture) };

            string[] receiverEmail = { token.ReceiverEmail };

            string[] invoiceId = { token.InvoiceId };


            for (var i = 0; i < amt.Length; i++)
            {
                var rec = new Receiver(Convert.ToDecimal(amt[i]));
                if (receiverEmail[i] != string.Empty)
                    rec.email = receiverEmail[i];

                if (invoiceId[i] != string.Empty)
                    rec.invoiceId = invoiceId[i];

                receiverList.receiver.Add(rec);

            }

            var url = Utils.GetKeyValue("baseUrl") + "/Billing/PaypalPaymentResponse";

            var request = new PayRequest(new RequestEnvelope("en_US"), "PAY", url, token.Currency.ISO, receiverList, url)
            {
                senderEmail = PayPalConfiguration.ADAPTIVE_PAYMENT_SENDER_EMAIL
            };

            PayResponse response;
            try
            {
                var configurationMap = PayPalConfiguration.sdkConfig;

                var service = new AdaptivePaymentsService(configurationMap);

                response = service.Pay(request);
            }
            catch (PayPalException ex)
            {
                error = ex.FormatPaypalException();
                Logger.Error("ExecutePayment::paypal exception::execute payment::", ex, CommonEnums.LoggerObjectTypes.PayPal);
                return false;
            }
            catch (Exception ex)
            {
                error = FormatError(ex);
                return false;
            }

            if (response.responseEnvelope.ack == AckCode.SUCCESS)
            {
                payKey = response.payKey;
                return true;
            }

            error = response.error.Aggregate(error, (current, err) => current + (err.message + ","));


            return false;

        }
        #endregion
              
    }
  
}
