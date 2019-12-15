using Spectre.Cli;

namespace grpc.client
{
    internal class ListSettings : CommandSettings
    {
        public ListSettings()
        {
        }

        [CommandArgument(0, "<ADDRESS>")]
        public string Address { get; set; }

        [CommandArgument(1, "[SERVICE]")]
        public string Service { get; set; }
    }
}