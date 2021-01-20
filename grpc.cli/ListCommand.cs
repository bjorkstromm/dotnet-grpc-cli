using System.Threading.Tasks;
using Spectre.Cli;
using Grpc.Reflection.V1Alpha;
using System.Threading;
using System;
using System.Linq;
using Google.Protobuf.Reflection;

namespace grpc.client
{
    internal class ListCommand : AsyncCommand<ListSettings>
    {
        public override async Task<int> ExecuteAsync(CommandContext context, ListSettings settings)
        {
            var address = settings.Address;

            if (!address.StartsWith("http://") &&
                !address.StartsWith("https://"))
            {
                address = $"https://{address}";
            }

            if (address.StartsWith("http://"))
            {
                // This switch must be set before creating the GrpcChannel/HttpClient.
                AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            }

            var server = address;
            using var channel = Grpc.Net.Client.GrpcChannel.ForAddress(server);
            var client = new ServerReflection.ServerReflectionClient(channel);
            using var stream = client.ServerReflectionInfo();

            if (string.IsNullOrEmpty(settings.Service))
            {
                await stream.RequestStream.WriteAsync(new ServerReflectionRequest { ListServices = "ls" });
                await stream.ResponseStream.MoveNext(CancellationToken.None);

                foreach (var service in stream.ResponseStream.Current.ListServicesResponse.Service)
                {
                    Console.WriteLine(service.Name);
                }

                await stream.RequestStream.CompleteAsync();
            }
            else
            {
                await stream.RequestStream.WriteAsync(new ServerReflectionRequest { FileContainingSymbol = settings.Service });
                await stream.ResponseStream.MoveNext(CancellationToken.None);
                var descriptors = FileDescriptor.BuildFromByteStrings(stream.ResponseStream.Current.FileDescriptorResponse.FileDescriptorProto);
                await stream.RequestStream.CompleteAsync();

                var service = descriptors
                    .SelectMany(x => x.Services)
                    .FirstOrDefault(x => string.Equals(x.FullName, settings.Service));

                if (service is object)
                {
                    Console.WriteLine($"filename: {service.File.Name}");
                    Console.WriteLine($"package: {service.File.Package}");
                    Console.WriteLine($"service {service.Name} {{");
                    foreach (var method in service.Methods)
                    {
                        Console.WriteLine($"  {GetMethod(method)}");
                    }
                    Console.WriteLine("}");
                }
                else
                {
                    var method = descriptors
                        .SelectMany(x => x.Services)
                        .SelectMany(x => x.Methods)
                        .FirstOrDefault(x => string.Equals(x.FullName, settings.Service));

                    Console.WriteLine(GetMethod(method));
                }

                static string GetMethod(MethodDescriptor method) =>
                    $"rpc {method.Name}({(method.IsClientStreaming ? "stream " : string.Empty)}{method.InputType.FullName})" +
                    $" returns ({(method.IsServerStreaming ? "stream " : string.Empty)}{method.OutputType.FullName}) {{}}";
            }

            return 0;
        }
    }
}