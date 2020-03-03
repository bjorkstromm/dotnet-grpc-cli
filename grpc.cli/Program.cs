using Spectre.Cli;
using System.Threading.Tasks;

namespace grpc.client
{
    class Program
    {
        static Task<int> Main(string[] args)
        {
            var app = new CommandApp();

            app.Configure(config =>
            {
                config.AddCommand<ListCommand>("ls");
                config.AddCommand<DumpCommand>("dump");
            });

            return app.RunAsync(args);
        }
    }
}
