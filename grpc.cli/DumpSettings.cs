using Spectre.Cli;

namespace grpc.client
{
    internal class DumpSettings : CommandSettings
    {
        public DumpSettings()
        {
        }

        [CommandArgument(0, "<ADDRESS>")]
        public string Address { get; set; }

        [CommandArgument(1, "<SERVICE>")]
        public string Service { get; set; }

        [CommandOption( "-o|--output <OUTPUT>")]
        public string Output { get; set; }
    }
}