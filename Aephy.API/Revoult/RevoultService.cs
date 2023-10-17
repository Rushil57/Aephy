using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using static Aephy.API.Models.AdminViewModel;
using Stripe;
using System.Net;
using RestSharp;
using RevolutAPI.Models.BusinessApi.Counterparties;
using RevolutAPI.Models.BusinessApi.Payment;
using RevolutAPI.Models.BusinessApi.Account;
//using RevolutAPI.Models.BusinessApi.Counterparties;

namespace Aephy.API.Revoult
{
    public class RevoultService : IRevoultService
    {
        private AuthToken _authToken;

        private string _client_assertion;

        public RevoultService()
        {
            using (StreamReader r = new StreamReader("./RevoultResources/JWT.json"))
            {
                string json = r.ReadToEnd();
                _authToken = JsonConvert.DeserializeObject<AuthToken>(json);
            }

            using (StreamReader r = new StreamReader("./RevoultResources/client_assertion.txt"))
            {
                _client_assertion = r.ReadToEnd();
            }
        }

      
        public async Task<bool> RefreshToken()
        {
            try
            {
                var options = new RestClientOptions("https://sandbox-b2b.revolut.com")
                {
                    MaxTimeout = -1,

                };
                var client = new RestClient(options);

                var request = new RestRequest("/api/1.0/auth/token", Method.Post);
                request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                request.AddHeader("Accept", "*/*");
                request.AddParameter("application/x-www-form-urlencoded", $"grant_type=refresh_token&refresh_token={_authToken.refresh_token}&client_assertion_type=urn:ietf:params:oauth:client-assertion-type:jwt-bearer&client_assertion={_client_assertion}", ParameterType.RequestBody);
                RestResponse response = await client.ExecuteAsync(request);
                if (response.ResponseStatus != ResponseStatus.Error)
                {
                    var authNew = JsonConvert.DeserializeObject<AuthToken>(response.Content);

                    _authToken.access_token = authNew.access_token;
                    _authToken.token_type = authNew.token_type;
                    _authToken.expires_in = authNew.expires_in;

                    authNew.refresh_token = _authToken.refresh_token;
                    string output = Newtonsoft.Json.JsonConvert.SerializeObject(authNew, Newtonsoft.Json.Formatting.Indented);
                    System.IO.File.WriteAllText(@"./RevoultResources/JWT.json", output);
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

            return true;
        }
        public async Task<AddNonRevolutCounterpartyResp> AddInternationalCounterParty(AddNonRevolutCounterpartyReq addCounterpartyReq)
        {
            try
            {
                var options = new RestClientOptions("https://sandbox-b2b.revolut.com")
                {
                    MaxTimeout = -1,
                };
                var client = new RestClient(options);
                var request = new RestRequest("/api/1.0/counterparty", Method.Post);
                request.AddHeader("Accept", "application/json");
                request.AddHeader("Accept", "application/json");
                request.AddHeader("Authorization", string.Format("Bearer {0}", _authToken.access_token));

                //var body = JsonConvert.SerializeObject(addCounterpartyReq);
                var FreelancerName = addCounterpartyReq.IndividualName.FirstName + " " + addCounterpartyReq.IndividualName.LastName;

                var address = @"{" +
                @"  ""street_line1"": ""##street_line1##""," + "\n" +
                @"  ""street_line2"": ""##street_line2##""," + "\n" +
                @"  ""region"": ""##region##""," + "\n" +
                @"  ""city"": ""##city##""," + "\n" +
                @"  ""country"": ""##country##""," + "\n" +
                @"  ""postcode"": ""##postcode##""}";


                address = address.Replace("##street_line1##", addCounterpartyReq.Address.StreetLine1);
                address = address.Replace("##street_line2##", addCounterpartyReq.Address.StreetLine2);
                address = address.Replace("##region##", addCounterpartyReq.Address.Region);
                address = address.Replace("##city##", addCounterpartyReq.Address.City);
                address = address.Replace("##country##", addCounterpartyReq.Address.Country);
                address = address.Replace("##postcode##", addCounterpartyReq.Address.Postcode);

                var body = @"{" + "\n" +
                @"  ""profile_type"": ""##profile_type##""," + "\n" +
                @"  ""company_name"": ""##company_name##""," + "\n" +
                @"  ""name"": ""##name##""," + "\n" +
                @"  ""revtag"": ""##revtag##""," + "\n" +
                @"  ""bank_country"": ""##bank_country##""," + "\n" +
                @"  ""currency"": ""##currency##""," + "\n" +
                @"  ""iban"": ""##iban##""," + "\n" +
                @"  ""bic"": ""##bic##""," + "\n" +
                @"  ""account_no"": ""##account_no##""," + "\n" +
                @"  ""ifsc"": ""##ifsc##""," + "\n" +
                @"  ""country"": ""##country##""," + "\n" +
                @"  ""address"": ##address##" + "\n" +
                @"}";

                body = body.Replace("##profile_type##", "personal");
                body = body.Replace("##company_name##", FreelancerName);
                body = body.Replace("##name##", addCounterpartyReq.IndividualName.FirstName + " " + addCounterpartyReq.IndividualName.LastName);
                //body = body.Replace("##revtag##", string.Empty);
                body = body.Replace("##bank_country##", addCounterpartyReq.BankCountry);
                body = body.Replace("##currency##", addCounterpartyReq.Currency);
                body = body.Replace("##iban##", addCounterpartyReq.Iban);
                body = body.Replace("##bic##", addCounterpartyReq.Bic);
                body = body.Replace("##account_no##", addCounterpartyReq.AccountNo);
                body = body.Replace("##ifsc##", addCounterpartyReq.IFSC);
                body = body.Replace("##country##", addCounterpartyReq.Address.Country);
                //body = body.Replace("##address##", address);

                //body = "{\n        \"company_name\": \"John Smith Co.\",\n        \"bank_country\": \"GB\",\n        \"currency\": \"GBP\",\n        \"account_no\": \"12345678\",\n        \"sort_code\": \"223344\"\n      }";
                //body = "{\n\t\"company_name\": \"John Smith India\",\n\t\"bank_country\": \"IN\",\n\t\"currency\": \"EUR\",\n\t\"bic\": \"IDIBINBBXXX\",\n\t\"account_no\": \"12345678\",\n\t\"ifsc\": \"SBIN0005943\",\n";

                // For India
                //body = "{\n\t\"company_name\": \"John Smith India\",\n\t\"bank_country\": \"IN\",\n\t\"currency\": \"EUR\",\n\t\"bic\": \"IDIBINBBXXX\",\n\t\"account_no\": \"12345678\",\n\t\"address\":    ";
                //body += address + " }";


                // For US
                //body = "{\n\t\"company_name\": \"John Smith US\",\n\t\"bank_country\": \"US\",\n\t\"currency\": \"USD\",\n\t\"routing_number\": \"082000510\",\n\t\"account_no\": \"13719713158835300\",\n\t\"address\":    ";
                //body += address + " }";

                // For SWIFT - non IBAN country CHINA    
                //body = "{\n\t\"company_name\": \"John Smith CHINA\",\n\t\"bank_country\": \"CN\",\n\t\"currency\": \"EUR\",\n\t\"bic\": \"BKCHCNBJ\",\n\t\"account_no\": \"13719713158835300\",\n\t\"address\":    ";
                //body += address + " }";


                // For SWIFT - MX   
                //body = "{\n\t\"company_name\": \"John Smith MX\",\n\t\"bank_country\": \"MX\",\n\t\"currency\": \"EUR\",\n\t\"bic\": \"BDEMMXMMXXX\",\n\t\"clabe\": \"032180000118359719\",\n\t\"address\":    ";
                //body += address + " }";

                // For AUSTRALIA - AU   
                //body = "{\n\t\"company_name\": \"John Smith AU\",\n\t\"bank_country\": \"AU\",\n\t\"currency\": \"EUR\",\n\t\"bic\": \"ABNAAU2BOBU\",\n\t\"account_no\": \"011623852\",\n\t\"address\":    ";
                //body += address + " }";
                if(addCounterpartyReq.BankCountry == "IN")
                {
                    body = "{\n\t\"company_name\": \""+ FreelancerName + "\",\n\t\"bank_country\": \""+addCounterpartyReq.BankCountry+"\",\n\t\"currency\": \""+addCounterpartyReq.Currency+"\",\n\t\"bic\": \""+addCounterpartyReq.Bic+"\",\n\t\"account_no\": \""+addCounterpartyReq.AccountNo+"\",\n\t\"address\":    " + address + " }";
                }
                else if (addCounterpartyReq.BankCountry == "US")
                {
                    body = "{\n\t\"company_name\": \""+ FreelancerName + "\",\n\t\"bank_country\": \""+addCounterpartyReq.BankCountry+"\",\n\t\"currency\": \""+addCounterpartyReq.Currency+"\",\n\t\"routing_number\": \""+addCounterpartyReq.RoutingNumber+"\",\n\t\"account_no\": \""+addCounterpartyReq.AccountNo+"\",\n\t\"address\":    " + address + " }";
                }
                else if (addCounterpartyReq.BankCountry == "MX")
                {
                    body = "{\n\t\"company_name\": \""+ FreelancerName + "\",\n\t\"bank_country\": \""+addCounterpartyReq.BankCountry+"\",\n\t\"currency\": \""+addCounterpartyReq.Currency+"\",\n\t\"bic\": \""+addCounterpartyReq.Bic+"\",\n\t\"clabe\": \""+addCounterpartyReq.Clabe+"\",\n\t\"address\":    "+ address + " }";
                }
                else if (addCounterpartyReq.BankCountry == "AU")
                {
                    body = "{\n\t\"company_name\": \""+ FreelancerName + "\",\n\t\"bank_country\": \""+addCounterpartyReq.BankCountry+"\",\n\t\"currency\": \""+addCounterpartyReq.Currency+"\",\n\t\"bic\": \""+addCounterpartyReq.Bic+"\",\n\t\"account_no\": \""+addCounterpartyReq.AccountNo+"\",\n\t\"address\":    "+ address + " }";
                }
                else if(addCounterpartyReq.IbanMandantory)
                {
                    body = "{\n\t\"company_name\": \""+ FreelancerName + "\",\n\t\"bank_country\": \"" + addCounterpartyReq.BankCountry + "\",\n\t\"currency\": \"" + addCounterpartyReq.Currency + "\",\n\t\"bic\": \"" + addCounterpartyReq.Bic + "\",\n\t\"iban\": \"" + addCounterpartyReq.Iban + "\",\n\t\"address\":    "+ address + "  }";
                }
                else
                {
                    body = "{\n\t\"company_name\": \""+ FreelancerName + "\",\n\t\"bank_country\": \"" + addCounterpartyReq.BankCountry + "\",\n\t\"currency\": \"" + addCounterpartyReq.Currency + "\",\n\t\"bic\": \"" + addCounterpartyReq.Bic + "\",\n\t\"account_no\": \"" + addCounterpartyReq.AccountNo + "\",\n\t\"address\":    " + address + "  }";
                }


                request.AddStringBody(body, DataFormat.Json);


                var test = "";

                RestResponse response = await client.ExecuteAsync(request);

                if (response.ResponseStatus != ResponseStatus.Error)
                {
                    return JsonConvert.DeserializeObject<AddNonRevolutCounterpartyResp>(response.Content);
                }
                else
                {
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        await RefreshToken();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return null;
        }

        public async Task<List<GetCounterpartyResp>> GetCounterparties()
        {
            try
            {
            RequestAgain:
                var options = new RestClientOptions("https://sandbox-b2b.revolut.com")
                {
                    MaxTimeout = -1,
                };
                var client = new RestClient(options);
                var request = new RestRequest("/api/1.0/counterparties", Method.Get);
                request.AddHeader("Accept", "application/json");
                request.AddHeader("Authorization", string.Format("Bearer {0}", _authToken.access_token));
                RestResponse response = await client.ExecuteAsync(request);

                if (response.ResponseStatus != ResponseStatus.Error)
                {
                    return JsonConvert.DeserializeObject<List<GetCounterpartyResp>>(response.Content);
                }
                else
                {
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        await RefreshToken();
                        goto RequestAgain;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return null;
        }

        public async Task<CreatePaymentResp> CreatePayment(CreatePaymentReq createPaymentReq)
        {
            try
            {
            RequestAgain:
                var options = new RestClientOptions("https://sandbox-b2b.revolut.com")
                {
                    MaxTimeout = -1,
                };
                var client = new RestClient(options);
                var request = new RestRequest("/api/1.0/pay", Method.Post);
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Accept", "application/json");
                request.AddHeader("Authorization", string.Format("Bearer {0}", _authToken.access_token));
                var body = @"{" + "\n" +
                @"  ""request_id"": ""##RequestId##""," + "\n" +
                @"  ""account_id"": ""##SenderAccountId##""," + "\n" +
                @"  ""receiver"": {" + "\n" +
                @"    ""counterparty_id"": ""##ReceiverCounterPrtyId##""," + "\n" +
                @"    ""account_id"": ""##ReceiverAccountId##""" + "\n" +
                @"  }," + "\n" +
                @"  ""amount"": ##amount##," + "\n" +
                @"  ""charge_bearer"": ""shared""," + "\n" +
                @"  ""currency"": ""##currency##""," + "\n" +
                @"  ""reference"": ""##reference##""" + "\n" +
                @"}";

                body = body.Replace("##RequestId##", createPaymentReq.RequestId);
                body = body.Replace("##SenderAccountId##", createPaymentReq.AccountId);
                body = body.Replace("##ReceiverCounterPrtyId##", createPaymentReq.Receiver.CounterpartyId);
                body = body.Replace("##ReceiverAccountId##", createPaymentReq.Receiver.AccountId);
                body = body.Replace("##amount##", createPaymentReq.Amount.ToString());
                body = body.Replace("##currency##", createPaymentReq.Currency);
                body = body.Replace("##reference##", createPaymentReq.Reference);

                request.AddStringBody(body, DataFormat.Json);
                RestResponse response = await client.ExecuteAsync(request);
                var createPaymentResp = new CreatePaymentResp();
                if (response.ResponseStatus != ResponseStatus.Error)
                {
                    createPaymentResp = JsonConvert.DeserializeObject<CreatePaymentResp>(response.Content);
                }
                else
                {
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        await RefreshToken();
                        goto RequestAgain;
                    }
                }


                //var id = test.Id;
                ////
                //var options1 = new RestClientOptions("https://sandbox-b2b.revolut.com")
                //{
                //    MaxTimeout = -1,
                //};
                //var client1 = new RestClient(options1);
                //var request1 = new RestRequest(string.Format("https://sandbox-b2b.revolut.com/api/1.0/transaction/{0}", id), Method.Get);
                //request1.AddHeader("Accept", "application/json");
                //request1.AddHeader("Authorization", "Bearer " + _authToken.access_token);
                //RestResponse response1 = await client1.ExecuteAsync(request1);
                //Console.WriteLine(response1.Content);


                //var options2 = new RestClientOptions("https://sandbox-merchant.revolut.com")
                //{
                //    MaxTimeout = -1,
                //};
                //var client2 = new RestClient(options2);
                //var request2 = new RestRequest("https://sandbox-merchant.revolut.com/api/orders/652949b3-fa7b-a334-9eff-64a8dae96fcb", Method.Get);
                //request2.AddHeader("Accept", "application/json");
                //request2.AddHeader("Revolut-Api-Version", "2023-09-01");
                //request2.AddHeader("Authorization", "Bearer sk_u8VvFPDvr2eor1R-Ti_4fXa1J2G7jeVEyB8AXndKu7yaT20UkLlLsBDM3naKRzY4");
                //RestResponse response2 = await client2.ExecuteAsync(request2);
                //Console.WriteLine(response2.Content);


                return createPaymentResp;
            }
            catch (Exception ex)
            {

            }

            return null;
        }
        public async Task GetOrderDetails(string id)
        {
            var options2 = new RestClientOptions("https://sandbox-merchant.revolut.com")
            {
                MaxTimeout = -1,
            };
            var client2 = new RestClient(options2);
            var request2 = new RestRequest("https://sandbox-merchant.revolut.com/api/orders/"+id, Method.Get);
            request2.AddHeader("Accept", "application/json");
            request2.AddHeader("Revolut-Api-Version", "2023-09-01");
            request2.AddHeader("Authorization", "Bearer sk_u8VvFPDvr2eor1R-Ti_4fXa1J2G7jeVEyB8AXndKu7yaT20UkLlLsBDM3naKRzY4");
            RestResponse response2 = await client2.ExecuteAsync(request2);
            Console.WriteLine(response2.Content);


            var options = new RestClientOptions("https://sandbox-merchant.revolut.com")
            {
                MaxTimeout = -1,
            };
            var client = new RestClient(options);
            var request = new RestRequest("https://sandbox-merchant.revolut.com/api/1.0/orders", Method.Get);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Revolut-Api-Version", "2023-09-01");
            request.AddHeader("Authorization", "Bearer sk_u8VvFPDvr2eor1R-Ti_4fXa1J2G7jeVEyB8AXndKu7yaT20UkLlLsBDM3naKRzY4");
            RestResponse response = await client.ExecuteAsync(request);
            Console.WriteLine(response.Content);
        }

        public async Task<List<GetAccountResp>> RetrieveAllAccounts()
        {
            try
            {
            RequestAgain:
                var options = new RestClientOptions("https://sandbox-b2b.revolut.com")
                {
                    MaxTimeout = -1,
                };
                var client = new RestClient(options);
                var request = new RestRequest("/api/1.0/accounts", Method.Get);
                request.AddHeader("Accept", "application/json");
                request.AddHeader("Authorization", string.Format("Bearer {0}", _authToken.access_token));
                RestResponse response = await client.ExecuteAsync(request);

                if (response.ResponseStatus != ResponseStatus.Error)
                {
                    var list = JsonConvert.DeserializeObject<List<GetAccountResp>>(response.Content);
                    return list;
                }
                else
                {
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        await RefreshToken();
                        goto RequestAgain;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return null;
        }

        public async Task<RefundPaymentRequest> RefundToClient(RefundPaymentRequest model)
        {
            try
            {
                var options = new RestClientOptions("https://sandbox-merchant.revolut.com")
                {
                    MaxTimeout = -1,
                };
                var client = new RestClient(options);
                var request = new RestRequest("https://sandbox-merchant.revolut.com/api/1.0/orders/" + model.OrderId+"/refund", Method.Post);
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Accept", "application/json");
                request.AddHeader("Authorization", "Bearer sk_u8VvFPDvr2eor1R-Ti_4fXa1J2G7jeVEyB8AXndKu7yaT20UkLlLsBDM3naKRzY4");
               // request.AddHeader("Authorization", string.Format("Bearer {0}", _authToken.access_token));
                var body = @"{" + "\n" +
                @"  ""amount"": "+ model.Amount + "," + "\n" +
                @"  ""description"": ""Test Data""" + "\n" +
                @"}";
                request.AddStringBody(body, DataFormat.Json);
                RestResponse response = await client.ExecuteAsync(request);
                if (response.StatusCode == HttpStatusCode.Created)
                {
                    var data = JsonConvert.DeserializeObject<RefundPaymentRequest>(response.Content);
                    return data;
                }
                return null;
                //Console.WriteLine(response.Content);
            }
            catch (Exception ex)
            {

                throw;
            }
            
        }
    }
}
