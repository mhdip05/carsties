﻿using MassTransit;
using Contracts;
using AuctionService.Data;
using AuctionService.Entities;

namespace AuctionService.Consumer
{
    public class ActionFinishedConsumer : IConsumer<AuctionFinished>
    {
        private readonly AuctionDbContext _dbContext;

        public ActionFinishedConsumer(AuctionDbContext context)
        {
            _dbContext = context;
        }

        public async Task Consume(ConsumeContext<AuctionFinished> context)
        {
            Console.WriteLine("--> Consuming Action Finished");

            var auction = await _dbContext.Auctions.FindAsync(context.Message.AuctionId);

            if (context.Message.ItemSold)
            {
                auction.Winner = context.Message.Winner;
                auction.SoldAmount = context.Message.Amount;
            }

            auction.Status = auction.SoldAmount > auction.ReservePrice ? Status.Finished : Status.ReserveNotMet;

            await _dbContext.SaveChangesAsync();
        }
    }
}
