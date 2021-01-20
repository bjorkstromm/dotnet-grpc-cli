using Google.Protobuf.Reflection;
using Grpc.Reflection.V1Alpha;
using Spectre.Cli;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace grpc.client
{
    internal class DumpCommand : AsyncCommand<DumpSettings>
    {
        public override async Task<int> ExecuteAsync(CommandContext context, DumpSettings settings)
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
            var channel = Grpc.Net.Client.GrpcChannel.ForAddress(server);
            var client = new ServerReflection.ServerReflectionClient(channel);
            var stream = client.ServerReflectionInfo();

            await stream.RequestStream.WriteAsync(new ServerReflectionRequest { FileContainingSymbol = settings.Service });
            await stream.ResponseStream.MoveNext(CancellationToken.None);
            var descriptors = FileDescriptor.BuildFromByteStrings(stream.ResponseStream.Current.FileDescriptorResponse.FileDescriptorProto);
            await stream.RequestStream.CompleteAsync();

            // Output
            if (!string.IsNullOrWhiteSpace(settings.Output))
            {
                settings.Output = Path.GetFullPath(settings.Output);

                if (!Directory.Exists(settings.Output))
                {
                    Directory.CreateDirectory(settings.Output);
                }
            }

            foreach (var descriptor in descriptors)
            {
                if (IsWellKnownType(descriptor))
                {
                    continue;
                }

                TextWriter writer;

                if (string.IsNullOrWhiteSpace(settings.Output))
                {
                    writer = Console.Out;
                    await writer.WriteLineAsync("---");
                    await writer.WriteAsync("File: ");
                    await writer.WriteLineAsync(descriptor.Name);
                    await writer.WriteLineAsync("---");
                }
                else
                {
                    var path = Path.Join(settings.Output, descriptor.Name);
                    
                    var directory = Path.GetDirectoryName(path);
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    writer = File.CreateText(path);
                }

                await WriteFileDescriptor(descriptor, writer);
                await writer.DisposeAsync();
            }

            return 0;
        }

        private static bool IsWellKnownType(FileDescriptor descriptor)
        {
            return descriptor.Name.StartsWith("google/protobuf/")
                   && descriptor.Package.Equals("google.protobuf");
        }

        private const string NoIndent = "";
        private const string Indent = "  ";

        private async Task WriteFileDescriptor(FileDescriptor descriptor, TextWriter writer)
        {
            // Syntax
            await writer.WriteLineAsync("syntax = \"proto3\";");

            // Dependencies
            foreach (var dependency in descriptor.Dependencies)
            {
                await writer.WriteLineAsync($"import \"{dependency.Name}\";");
            }

            // Package
            await writer.WriteLineAsync($"package {descriptor.Package};");

            // Empty line
            await writer.WriteLineAsync();

            // Enums
            foreach (var @enum in descriptor.EnumTypes)
            {
                await WriteEnumDescriptor(@enum, writer);
                await writer.WriteLineAsync();
            }

            // Messages
            foreach (var message in descriptor.MessageTypes)
            {
                await WriteMessageDescriptor(message, writer);
                await writer.WriteLineAsync();
            }

            // Messages
            foreach (var service in descriptor.Services)
            {
                await WriteServiceDescriptor(service, writer);
                await writer.WriteLineAsync();
            }
        }

        private async Task WriteServiceDescriptor(ServiceDescriptor service, TextWriter writer, string indentation = NoIndent)
        {
            await writer.WriteLineAsync($"service {service.Name} {{");
            foreach (var method in service.Methods)
            {
                await WriteMethodDescription(method, writer, indentation + Indent);
            }
            await writer.WriteLineAsync($"{indentation}}}");
        }

        private async Task WriteMethodDescription(MethodDescriptor method, TextWriter writer, string indentation = NoIndent)
        {
            await writer.WriteAsync($"{indentation} rpc {method.Name}(");

            if (method.IsClientStreaming)
            {
                await writer.WriteAsync("stream ");
            }
            await writer.WriteAsync($"{method.InputType.FullName}) returns (");
            if (method.IsServerStreaming)
            {
                await writer.WriteAsync("stream ");
            }
            await writer.WriteLineAsync($"{method.OutputType.FullName});");
        }

        private async Task WriteEnumDescriptor(EnumDescriptor @enum, TextWriter writer, string indentation = NoIndent)
        {
            await writer.WriteAsync(indentation);
            await writer.WriteLineAsync($"enum {@enum.Name} {{");

            foreach (var value in @enum.Values)
            {
                await writer.WriteAsync(indentation + Indent);
                await writer.WriteLineAsync($" {value.Name} = {value.Number};");
            }
            await writer.WriteLineAsync($"{indentation}}}");
        }

        private async Task WriteMessageDescriptor(MessageDescriptor message, TextWriter writer, string indentation = NoIndent)
        {
            await writer.WriteAsync(indentation);
            await writer.WriteLineAsync($"message {message.Name} {{");

            foreach (var @enum in message.EnumTypes)
            {
                await WriteEnumDescriptor(@enum, writer, indentation + Indent);
            }

            foreach (var nestedType in message.NestedTypes)
            {
                await WriteMessageDescriptor(nestedType, writer, indentation + Indent);
            }

            foreach (var field in message.Fields.InDeclarationOrder().Where(f => f.ContainingOneof is null))
            {
                await WriteFieldDescriptor(field, writer, indentation + Indent);
            }

            foreach (var oneof in message.Oneofs)
            {
                await WriteOneOfDescriptor(oneof, writer, indentation + Indent);
            }

            await writer.WriteLineAsync($"{indentation}}}");
        }

        private async Task WriteOneOfDescriptor(OneofDescriptor oneof, TextWriter writer, string indentation = NoIndent)
        {
            await writer.WriteLineAsync($"{indentation}oneof {oneof.Name} {{");
            foreach (var field in oneof.Fields)
            {
                await WriteFieldDescriptor(field, writer, indentation + Indent);
            }
            await writer.WriteLineAsync($"{indentation}}}");
        }

        private async Task WriteFieldDescriptor(FieldDescriptor field, TextWriter writer, string indentation = NoIndent)
        {
            await writer.WriteAsync(indentation);

            if (field.IsRepeated)
            {
                await writer.WriteAsync("repeated ");
            }

            switch (field.FieldType)
            {
                case FieldType.Double:
                case FieldType.Float:
                case FieldType.Int32:
                case FieldType.Int64:
                case FieldType.UInt32:
                case FieldType.UInt64:
                case FieldType.SInt32:
                case FieldType.SInt64:
                case FieldType.Fixed32:
                case FieldType.Fixed64:
                case FieldType.SFixed32:
                case FieldType.SFixed64:
                case FieldType.Bool:
                case FieldType.String:
                case FieldType.Bytes:
                    await writer.WriteAsync(field.FieldType.ToString().ToLowerInvariant());
                    break;
                case FieldType.Group:
                    break;
                case FieldType.Message:
                    await writer.WriteAsync(field.MessageType.FullName);
                    break;
                case FieldType.Enum:
                    await writer.WriteAsync(field.EnumType.FullName);
                    break;
            }

            await writer.WriteLineAsync($" {field.Name} = {field.FieldNumber};");
        }
    }
}
