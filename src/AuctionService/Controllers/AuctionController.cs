using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Metrics;

namespace AuctionService.Controllers
{
    [ApiController]
    [Route("api/auctions")]
    public class AuctionController : ControllerBase
    {
        private readonly AuctionDbContext _context;
        private readonly IMapper _mapper;
        private readonly IPublishEndpoint _publishEndPoint;

        public AuctionController(AuctionDbContext contex, IMapper mapper, IPublishEndpoint publishEndpoint)
        {
            _context = contex;
            _mapper = mapper;
            _publishEndPoint = publishEndpoint;
        }

        [HttpGet]
        public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string date)
        {
            var query = _context.Auctions.OrderBy(x => x.Item.Make).AsQueryable();

            if (!string.IsNullOrEmpty(date))
            {
                query = query.Where(x => x.UpdatedAt
                                          .CompareTo(DateTime.Parse(date)
                                          .ToUniversalTime()) > 0);
            }

            return await query.ProjectTo<AuctionDto>(_mapper.ConfigurationProvider).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
        {
            var auction = await _context.Auctions.Include(x => x.Item).FirstOrDefaultAsync(x => x.Id == id);

            if (auction == null) return NotFound();

            return _mapper.Map<AuctionDto>(auction);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
        {
            var auction = _mapper.Map<Auction>(auctionDto);

            auction.Seller = User.Identity.Name;

            // since we installed mastransit entity frame work 
            // these 3 line will be treated like a transaction
            // either they all work or none of them work like, 
            // if the services bus down, if that fail the whole transaction will fail
            _context.Auctions.Add(auction);
            var newAction = _mapper.Map<AuctionDto>(auction);
            await _publishEndPoint.Publish(_mapper.Map<AuctionCreated>(newAction));

            var result = await _context.SaveChangesAsync() > 0;

            /*
                // publish this data as a message to massTransit
                // we are publishing message after saving the data
                var newAction = _mapper.Map<AuctionDto>(auction);
                await _publishEndPoint.Publish(_mapper.Map<AuctionCreated>(newAction));
            */

            if (!result) return BadRequest("Could not save change to the DB");

            return CreatedAtAction(nameof(GetAuctionById), new { auction.Id }, newAction);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
        {
            var auction = await _context.Auctions.Include(x => x.Item)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (auction == null) return NotFound();

            // TODO: check seller == username
            if (auction.Seller != User.Identity.Name) return Forbid();

            auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
            auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
            auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
            auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
            auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;

            // publish the message
            await _publishEndPoint.Publish(_mapper.Map<AuctionUpdated>(auction));

            var result = await _context.SaveChangesAsync() > 0;

            if (result) return Ok();

            return BadRequest("Problem saving changes");
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAcution(Guid id)
        {
            var auction = await _context.Auctions.FindAsync(id);

            if (auction == null) return NotFound();

            //TODO : check seller == username
            if (auction.Seller != User.Identity.Name) return Forbid();

            _context.Auctions.Remove(auction);

            // publish the message 
            await _publishEndPoint.Publish<AuctionDeleted>(new { Id = auction.Id.ToString()});

            var result = await _context.SaveChangesAsync() > 0;

            if (!result) return BadRequest("Could not update db");

            return Ok();
        }

    }
}

