using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.Reflection;
using ProtoBuf.Grpc;

namespace grpc.reflection.v1alpha
{
    public class ReflectionService : IServerReflection
    {
        readonly List<string> services;
        readonly SymbolRegistry symbolRegistry;

        /// <summary>
        /// Creates a new instance of <c>ReflectionServiceIml</c>.
        /// </summary>
        public ReflectionService(IEnumerable<string> services, SymbolRegistry symbolRegistry)
        {
            this.services = new List<string>(services);
            this.symbolRegistry = symbolRegistry;
        }

        /// <summary>
        /// Creates a new instance of <c>ReflectionServiceIml</c>.
        /// </summary>
        public ReflectionService(IEnumerable<ServiceDescriptorProto> serviceDescriptors)
        {
            this.services = new List<string>(serviceDescriptors.Select((serviceDescriptor) => serviceDescriptor.Name));
            this.symbolRegistry = SymbolRegistry.FromFiles(serviceDescriptors.Select((serviceDescriptor) => serviceDescriptor.File));
        }

        /// <summary>
        /// Creates a new instance of <c>ReflectionServiceIml</c>.
        /// </summary>
        public ReflectionService(params ServiceDescriptor[] serviceDescriptors) : this((IEnumerable<ServiceDescriptor>)serviceDescriptors)
        {
        }

        public async IAsyncEnumerable<ServerReflectionResponse> ServerReflectionInfoAsync(IAsyncEnumerable<ServerReflectionRequest> values, CallContext context = default)
        {
            await foreach (var value in values)
            {
                var response = ProcessRequest(value);
                yield return response;
            }
        }

        private ServerReflectionResponse ProcessRequest(ServerReflectionRequest value)
        {
            if (value.ShouldSerializeListServices())
            {
                return ListServices();
            }

            throw new NotSupportedException();
        }

        private ServerReflectionResponse ListServices()
        {
            return new ServerReflectionResponse
            {
                ListServicesResponse = new ListServiceResponse
                {
                    Services = { _services.Select(x => new ServiceResponse(x.Name)) }
                }
            };
        }
    }
}
