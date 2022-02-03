namespace Products_383.Features
{
    public class ProductCheck
    {

        public static int UniqueId(List<ProductDto> products)
        {
            var id = 1;
            foreach (var product in products)
            {
                if (products.Count == 0)
                {
                    return id;
                }
                else
                {
                    id++;
                }
            }

            return id;
        }

        public static bool Verification(ProductDto product)
        {
            var isVerified = false;

            if (product.Name.Length > 120 || product.Name == "")
            {
                return isVerified;
            }
            else if (product.Price <= 0 || product.SalePrice < 0)
            {
                return isVerified;
            }
            else
            {
                isVerified = true;
            }

            return isVerified;
        }

        public static bool CheckNull(ProductDto product)
        {
            var isNulled = false;

            if (product.Name == null || product.Description == null)
            {
                return isNulled;
            }
            else
            {
                isNulled = true;
            }

            return isNulled;
        }

    }
}
