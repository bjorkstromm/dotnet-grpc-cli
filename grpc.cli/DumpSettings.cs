using Spectre.Console.Cli;

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

        public override ValidationResult Validate()
        {
            var result = string.IsNullOrWhitespace(Address)
                ? ValidationResult.Error($"{nameof(Address)} cannot be blank.")
                : ValidationResult.Success();

            if (!result.Successful)
                return result;

            result = string.IsNullOrWhitespace(Service)
                ? ValidationResult.Error($"{nameof(Service)} cannot be blank.")
                : ValidationResult.Success();

            return result;
        }
    }
}
