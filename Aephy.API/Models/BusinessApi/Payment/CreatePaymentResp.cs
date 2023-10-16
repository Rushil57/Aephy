using System;

namespace RevolutAPI.Models.BusinessApi.Payment
{
    public class CreatePaymentResp
    {
        public string Id { get; set; }
        public string State { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }
    //public class GetAccountResp
    //{
    //    public string Id { get; set; }
    //    public string Name { get; set; }
    //    public double Balance { get; set; }
    //    public string Currency { get; set; }
    //    public string State { get; set; }
    //    public bool Public { get; set; }
    //    public DateTime UpdatedAt { get; set; }
    //    public DateTime CreatedAt { get; set; }
    //}
}