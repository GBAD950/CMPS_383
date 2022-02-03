namespace Products_383.Features
{
    public class SaleEventProduct
    {

        public int id { get; set; } 
        public int ProductId { get; set; }

        public virtual Product Product { get; set; }

        public int SaleEventID { get; set; }
        public virtual SaleEvent SaleEvents { get; set; }
    }
}