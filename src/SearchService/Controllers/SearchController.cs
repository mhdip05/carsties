using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using SearchService.DTO;
using SearchService.Models;

namespace SearchService.Controllers
{
    [ApiController]
    [Route("api/search")]
    public class SearchController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<Item>>> SearchItems([FromQuery] FilterDto filterDto)
        {
            var query = DB.PagedSearch<Item, Item>();

            query.Sort(x => x.Ascending(a => a.Make));

            if (!string.IsNullOrWhiteSpace(filterDto.SearchTerm))
            {
                query.Match(Search.Full, filterDto.SearchTerm).SortByTextScore();
            }

            query = filterDto.OrderBy switch
            {
                "make" => query.Sort(x => x.Ascending(a => a.Make)),
                "new" => query.Sort(x => x.Descending(a => a.CreatedAt)),
                _ => query.Sort(x => x.Ascending(a => a.AuctionEnd))
            };

            query = filterDto.FilterBy switch
            {
                "finished" => query.Match(x => x.AuctionEnd < DateTime.UtcNow),
                "endingSoon" => query.Match(x => x.AuctionEnd < DateTime.UtcNow.AddHours(6) && x.AuctionEnd > DateTime.UtcNow),
                _ => query.Match(x => x.AuctionEnd > DateTime.UtcNow)
            };

            if (!string.IsNullOrEmpty(filterDto.Winner))
            {
                query.Match(x => x.Winner == filterDto.Winner);
            }

            if (!string.IsNullOrEmpty(filterDto.Seller))
            {
                query.Match(x => x.Seller == filterDto.Seller);
            }

            query.PageNumber(filterDto.PageNumber);
            query.PageSize(filterDto.PageSize);

            var result = await query.ExecuteAsync();

            return Ok(new
            {
                results = result.Results,
                pageCount = result.PageCount,
                totalCount = result.TotalCount
            });
        }
    }
}
