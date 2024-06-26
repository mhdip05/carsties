﻿using Contracts;
using MassTransit;

namespace AuctionService.Consumer
{
    public class AuctionCreatedFaultConsumer : IConsumer<Fault<AuctionCreated>>
    {
        // s-4-ch-44--> Exception handling
        public async Task Consume(ConsumeContext<Fault<AuctionCreated>> context)
        {
            Console.WriteLine("--> Consuming faulty Creation");

            var exception = context.Message.Exceptions.First();

            if (exception.ExceptionType == "System.ArgumentException")
            {
                  context.Message.Message.Model = "FooBar";
                  await context.Publish(context.Message.Message);
            }
            if (exception.ExceptionType == "System.TimeoutException")
            {
                await context.Publish(context.Message.Message);
            }
            else
            {
                Console.WriteLine("Not an argument exception - update error dashboard somewhere");
            }
        }
    }
}
