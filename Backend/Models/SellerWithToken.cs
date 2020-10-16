namespace Backend.Models
{
    public class SellerWithToken
    {
        public string Token { get; set; }
        public int Id { get; set; }
        public string EmailAddress { get; set; }
        public long PhoneNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
        public string StoreName { get; set; }
        public string StoreUrl { get; set; }
        public string Description { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }

        public SellerWithToken(Seller seller)
        {
            this.Id = seller.Id;
            this.EmailAddress = seller.EmailAddress;
            this.PhoneNumber = seller.PhoneNumber;
            this.FirstName = seller.FirstName;
            this.LastName = seller.LastName;
            this.Password = seller.Password;
            this.StoreName = seller.StoreName;
            this.StoreUrl = seller.StoreUrl;
            this.Description = seller.Description;
            this.Street = seller.Street;
            this.City = seller.City;
            this.State = seller.State;
        }
    }
}
