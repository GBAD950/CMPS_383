namespace Products_383.Features
{
    public class SaleEvent
    {

        public int Id { get; set; }
        public string Name { get; set; }   

        public List<SaleEventProduct> Products { get; set; }
    }
}
