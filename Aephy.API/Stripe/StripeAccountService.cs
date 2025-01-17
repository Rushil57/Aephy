﻿using Aephy.API.DBHelper;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Stripe;
using Stripe.Checkout;

namespace Aephy.API.Stripe
{
    public class StripeAccountService : IStripeAccountService
    {
        public string CreateStripeAccount(string apiKey)
        {
            StripeConfiguration.ApiKey = apiKey;
            var accountOptions = new AccountCreateOptions
            {
                Type = "express",
                Capabilities = new AccountCapabilitiesOptions
                {
                    CardPayments = new AccountCapabilitiesCardPaymentsOptions
                    {
                        Requested = true
                    },
                    Transfers = new AccountCapabilitiesTransfersOptions
                    {
                        Requested = true
                    }
                }
            };

            try
            {
                var accountService = new AccountService();
                return accountService.Create(accountOptions).Id;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        public bool IsComplete(string connectedAccountId)
        {
            try
            {
                var service = new AccountService();
                var response = service.Get(connectedAccountId);

                if (response.Capabilities.Transfers == "active")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        
        public Session CreateCheckoutSession(SolutionMilestone mileStone, string projectPrice, string successUrl, string cancelUrl)
        {
            var ProjectPrice = Convert.ToInt64(projectPrice);
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string>
                {
                    "card",
                },
                LineItems = new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                UnitAmount = Convert.ToInt64(ProjectPrice * 100), // Amount in cents ($100)
                                Currency = "EUR",
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = mileStone.Title,
                                }

                            },
                            Quantity = 1,
                        },
                    },
                Mode = "payment",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                // This is meta data which can be used to store any information which will be available on payment success/Cancel.
                Metadata = new Dictionary<string, string>
                    {
                        { "contractId", "contractId" },
                        { "freelancerId1", "123" },
                        { "freelancerId2", "123" },
                        { "Architect", "123" }
                    },
                //AutomaticTax = new SessionAutomaticTaxOptions
                //{
                //    Enabled = true
                //},
                TaxIdCollection = new SessionTaxIdCollectionOptions
                {
                    
                    Enabled = true
                }
            };

            try
            {
                var service = new SessionService();
                return service.Create(options);
            }
            catch
            {
                return null;
            }
        }

        public Session CreateProjectCheckoutSession(string projectPrice,string successUrl, string cancelUrl)
        {
            var ProjectPrice = Convert.ToInt64(projectPrice);
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string>
                    {
                        "card",
                    },
                LineItems = new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                           PriceData = new SessionLineItemPriceDataOptions
                            {
                                UnitAmount = Convert.ToInt64(ProjectPrice * 100), // Amount in cents ($100)
                                Currency = "EUR",
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = "test",
                                }

                            },
                            Quantity = 1,
                        },
                    },
                Mode = "payment",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                // This is meta data which can be used to store any information which will be available on payment success/Cancel.
                Metadata = new Dictionary<string, string>
                    {
                        { "contractId", "contractId" },
                        { "freelancerId1", "123" },
                        { "freelancerId2", "123" },
                        { "Architect", "123" }
                    },
                //AutomaticTax = new SessionAutomaticTaxOptions
                //{
                //    Enabled = true
                //},
                TaxIdCollection = new SessionTaxIdCollectionOptions
                {
                    Enabled = true
                }
            };

            try
            {
                var service = new SessionService();
                return service.Create(options);
            }
            catch
            {
                return null;
            }
        }

        public Contract.SessionStatuses GetSesssionStatus(Session session)
        {
            switch (session.Status)
            {
                case "complete":
                    return Contract.SessionStatuses.Complete;
                case "expired":
                    return Contract.SessionStatuses.Expired;
                case "open":
                    return Contract.SessionStatuses.Open;
                default:
                    return Contract.SessionStatuses.NotCreated;
            }
        }

        public Contract.PaymentStatuses GetPaymentStatus(Session session)
        {
            switch (session.PaymentStatus)
            {
                case "paid":
                    return Contract.PaymentStatuses.Paid;
                case "unpaid":
                    return Contract.PaymentStatuses.UnPaid;
                case "no_payment_required":
                    return Contract.PaymentStatuses.NoPaymentRequired;
                default:
                    return Contract.PaymentStatuses.ContractCreated;
            }
        }
        public Session GetCheckOutSesssion(string sessionId)
        {
            try
            {
                var service = new SessionService();
                return service.Get(sessionId);
            }
            catch
            {
                return null;
            }
        }

        public PaymentIntent GetPaymentIntent(string paymentIntentId)
        {
            try
            {
                var paymentIntentService = new PaymentIntentService();
                return paymentIntentService.Get(paymentIntentId);
            }
            catch
            {
                return null;
            }
        }

        public string CreateTransferonCharge(long amount, string currency, string destination, string sourceTransaction, string transferGroup)
        {
            var options = new TransferCreateOptions
            {
                Amount = amount,
                Currency = currency,
                Destination = destination,
                SourceTransaction = sourceTransaction,
                TransferGroup = transferGroup
            };

            try
            {
                var service = new TransferService();
                var transfer = service.Create(options);

                return transfer.Id;
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        public Refund RefundAmountToClient(string lastChargeId, long amount)
        {
            var options = new RefundCreateOptions
            {
                Charge = lastChargeId,
                Amount = amount
            };

            try
            {
                var service = new RefundService();
                return service.Create(options);
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        public Session GetTaxDetails(string sessionId)
        {
            try
            {
                var taxDetails = new SessionService();
                return taxDetails.Get(sessionId);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public Session GetStripeFeedetails(string paymentIntedId)
        {
            //
            var options = new PaymentIntentGetOptions();
            options.AddExpand("charges.data.balance_transaction");

            var service = new PaymentIntentService();
            PaymentIntent paymentIntent = service.Get(paymentIntedId, options);

           // List<BalanceTransactionFeeDetail> feeDetails = paymentIntent.Charges.Data[0].BalanceTransaction.FeeDetails;

            return null;
            //
        }
    }
}
