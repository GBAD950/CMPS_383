//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using SP22.P03.Web.Data;
//using SP22.P03.Web.Data.Models;
//using SP22.P03.Web.Features.Products;
//using System.Linq.Expressions;

//namespace SP22.P03.Web.Controllers
//{
//    [Route("api/sale-event-products")]
//    [ApiController]
//    public class SaleEventProduct : ControllerBase
//    {
//        private readonly DataContext db;
//        private readonly string url = "http://localhost";
//        private readonly decimal SaleEventPrice;

//        public SaleEventProduct(DataContext db)
//        {
//            this.db = db;
//        }

//        public static Expression<Func<SaleEventProduct, SaleEventProductDto>> MapperMethod()
//        {
//            return sale_product => new SaleEventProductDto
//            {
//                SaleEventPrice = sale_product.SaleEventPrice
//            };
//        }

//        [HttpGet]
//        public IEnumerable<SaleEventProductDto> GetSaleEventProducts()
//        {
//            return db.Set<SaleEventProduct>().Select(MapperMethod()).ToList();
//        }

//        [HttpGet]
//        [Route("sale_event_product")]
//        public IEnumerable<SaleEventProductDto> GetAllSaleEventProducts()
//        {
//            return db.Set<SaleEventProduct>().Select(MapperMethod());
//        }
//    }
//}
