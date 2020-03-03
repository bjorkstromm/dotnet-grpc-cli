using System.ServiceModel;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using ProtoBuf;

namespace test_server_protobuf_net
{
    [ProtoContract]
    public class SearchRequest
    {
        [ProtoMember(1)]
        public string Name { get; set; }
    }

    [ProtoContract]
    public class Person
    {
        [ProtoMember(1)]
        public int Id { get; set; }
        [ProtoMember(2)]
        public string Name { get; set; }
        [ProtoMember(3)]
        public Address Address { get; set; }
    }

    [ProtoContract]
    public class Address
    {
        [ProtoMember(1)]
        public string Line1 { get; set; }
        [ProtoMember(2)]
        public string Line2 { get; set; }
    }

    [ServiceContract]
    public interface IMyAmazingService
    {
        ValueTask<Person> SearchAsync(SearchRequest request);
    }

    public class AmazingService : IMyAmazingService
    {
        private readonly ILogger<AmazingService> _logger;
        public AmazingService(ILogger<AmazingService> logger)
        {
            _logger = logger;
        }

        public ValueTask<Person> SearchAsync(SearchRequest request)
        {
            return new ValueTask<Person>(new Person());
        }
    }
}
