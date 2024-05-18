using MassTransit;
using Contracts;
using AutoMapper;
using SearchService.Models;
using MongoDB.Entities;

namespace SearchService.Consumer
{
    public class AuctionCreatedConsumer : IConsumer<AuctionCreated>
    {
        private IMapper _mapper;

        public AuctionCreatedConsumer(IMapper mapper)
        {
            _mapper = mapper;
        }

        public async Task Consume(ConsumeContext<AuctionCreated> context)
        {
            Console.WriteLine("---> Consuming auction created " + context.Message.Id);

            var item = _mapper.Map<Item>(context.Message);

            // this line is example, how to handle exception
            //if (item.Model == "Foo") throw new ArgumentException("Cannot sell cars with name of Foo");

            await item.SaveAsync();
        }
    }
}
