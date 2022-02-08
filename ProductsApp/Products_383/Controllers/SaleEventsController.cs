using Microsoft.AspNetCore.Mvc;
using Products_383.Data;
using Products_383.Data.Models;
using Products_383.Features.Products;
using System.Linq.Expressions;

namespace Products_383.Controllers
{
    [Route("api/sale-events")]
    [ApiController]
    public class SaleEventsController : ControllerBase
    {
        private readonly DataContext db;
        private readonly string url = "http://localhost";

        public SaleEventsController(DataContext db)
        {
            this.db = db;
        }

        public static Expression<Func<SaleEvent, SaleEventDto>> MapperMethod()
        {
            return saleEvent => new SaleEventDto
            {
                Id = saleEvent.Id,
                Name = saleEvent.Name,
                StartUtc = saleEvent.StartUtc,
                EndUtc = saleEvent.EndUtc
            };
        }

        [HttpGet]
        public IEnumerable<SaleEventDto> GetSaleEvents()
        {
            return db.Set<SaleEvent>().Select(MapperMethod()).ToList();
        }

        //[HttpGet]
        //[Route("sale-events")]
        //public IEnumerable<SaleEventDto> GetSaleEvents()
        //{
        //    return db.Set<SaleEvent>().Select(MapperMethod()).ToList();
        //}

        [HttpGet]
        [Route("{id}")]
        public ActionResult<SaleEventDto> GetSale(int id)
        {
            var item = db.Set<SaleEvent>().Where(x => x.Id == id).Select(MapperMethod()).FirstOrDefault();
            if (item == null)
            {
                return NotFound(id);
            }
            else
            {
                return Ok(item);
            }
        }

        [HttpPost]
        public ActionResult<SaleEventDto> createSaleEvent(SaleEventDto sale_event)
        {
            var item = db.Set<SaleEvent>().Add(new SaleEvent()
            {
                Name = sale_event.Name,
                StartUtc = sale_event.StartUtc,
                EndUtc = sale_event.EndUtc,
            });
            db.SaveChanges();
            sale_event.Id = item.Entity.Id;

            return Created($"{url}/api/sale-events/{item.Entity.Id}", sale_event);
        }

        [HttpPut("{id}")]
        public ActionResult<SaleEventDto> Update(int id, SaleEventDto sale_event)
        {
            var item = db.Set<SaleEvent>().Where(x => x.Id == id).FirstOrDefault();
            if (item == null)
            {
                return NotFound();
            }
            item.Name = sale_event.Name;
            item.StartUtc = sale_event.StartUtc;
            item.EndUtc = sale_event.EndUtc;
            db.SaveChanges();
            return Ok();
        }

        [HttpDelete]
        public ActionResult<SaleEventDto> Delete(int id)
        {
            var item = db.Set<SaleEvent>().Where(x => x.Id == id).FirstOrDefault();
            if (item == null)
            {
                return NotFound();
            }

            db.Set<SaleEvent>().Remove(item);
            db.SaveChanges();

            return Ok();
        }
    }
}
