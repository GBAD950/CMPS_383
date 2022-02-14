using Microsoft.AspNetCore.Mvc;
using SP22.P04.Web.Data;
using SP22.P04.Web.Features.Products;
using SP22.P04.Web.Features.Sales;

namespace SP22.P04.Web.Controllers;

[Route("api/sale-events")]
[ApiController]
public class SaleEventsController : ControllerBase
{
    private readonly DataContext dataContext;

    public SaleEventsController(DataContext dataContext)
    {
        this.dataContext = dataContext;
    }

    [HttpGet("active")]
    public ActionResult<SaleEventDto> GetActiveSale()
    {
        var saleEvents = dataContext.Set<SaleEvent>();
        var now = DateTime.Now;
        var result = GetSaleEventDtos(saleEvents).FirstOrDefault(x => x.StartUtc <= now && now <= x.EndUtc);
        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpGet("{id}")]
    public ActionResult<SaleEventDto> GetSaleEventById(int id)
    {
        var saleEvents = dataContext.Set<SaleEvent>();
        var result = GetSaleEventDtos(saleEvents).FirstOrDefault(x => x.Id == id);
        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPost]
    public ActionResult<SaleEventDto> CreateSalesEvent(SaleEventDto saleEventDto)
    {
        if (saleEventDto.StartUtc >= saleEventDto.EndUtc)
        {
            return BadRequest();
        }

        var hasOverlap = dataContext
            .Set<SaleEvent>()
            .Any(x => x.StartUtc <= saleEventDto.StartUtc && saleEventDto.StartUtc <= x.EndUtc ||
                      x.StartUtc <= saleEventDto.EndUtc && saleEventDto.EndUtc <= x.EndUtc ||
                      saleEventDto.StartUtc <= x.StartUtc && x.StartUtc <= saleEventDto.EndUtc ||
                      saleEventDto.StartUtc <= x.EndUtc && x.EndUtc <= saleEventDto.EndUtc);

        if (hasOverlap)
        {
            return BadRequest();
        }

        var saleEvent = new SaleEvent
        {
            Name = saleEventDto.Name,
            StartUtc = saleEventDto.StartUtc,
            EndUtc = saleEventDto.EndUtc
        };

        dataContext.Add(saleEvent);
        dataContext.SaveChanges();
        saleEventDto.Id = saleEvent.Id;

        return CreatedAtAction(nameof(GetSaleEventById), new { id = saleEventDto.Id }, saleEventDto);
    }

    [HttpPut("{saleEventId}/add-product/{productId}")]
    public ActionResult<SaleEventDto> AddProductToSale(int saleEventId, int productId, SaleEventProductDto body)
    {
        var saleEvents = dataContext.Set<SaleEvent>();
        var products = dataContext.Set<Product>();

        var product = products.FirstOrDefault(x => x.Id == productId);
        var saleEvent = saleEvents.FirstOrDefault(x => x.Id == saleEventId);

        if (product == null || saleEvent == null)
        {
            return NotFound();
        }

        var productSales = dataContext.Set<SaleEventProduct>();
        productSales.Add(new SaleEventProduct
        {
            Product = product,
            SaleEvent = saleEvent,
            SaleEventPrice = body.SaleEventPrice
        });

        dataContext.SaveChanges();

        return Ok();
    }

    private static IQueryable<SaleEventDto> GetSaleEventDtos(IQueryable<SaleEvent> saleEvents)
    {
        return saleEvents
            .Select(x => new SaleEventDto
            {
                Id = x.Id,
                Name = x.Name,
                StartUtc = x.StartUtc,
                EndUtc = x.EndUtc,
            });
    }
}