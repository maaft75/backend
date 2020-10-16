namespace Backend.Models
{
    public class BuyerWithToken
    {
        public string Token { get; set; }
        public int Id { get; set; }

        public BuyerWithToken(Buyer buyer)
        {
            this.Id = buyer.Id;
        }
    }
}
