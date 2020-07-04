# dotnet-grpc-cli
A .NET Global Tool equivalent of grpc_cli. Inspired by [grpc_cli](https://github.com/grpc/grpc/blob/master/doc/command_line_tool.md)

# Install
```
dotnet tool install -g dotnet-grpc-cli
```

# List services

`dotnet grpc-cli ls <address>`

Example:
```
dotnet grpc-cli ls http://localhost:10042
Shared_CS.Calculator
MegaCorp.TimeService
```

# List methods in service

`dotnet grpc-cli ls <address> <service>`

Example:
```
dotnet grpc-cli ls http://localhost:10042 MegaCorp.TimeService
filename: MegaCorp.TimeService.proto
package: MegaCorp
service TimeService {
  rpc Subscribe(ProtoBuf.Grpc.Internal.Empty) returns (stream MegaCorp.TimeResult) {}
}
```

# Dump service in proto format

`dotnet grpc-cli dump <address> <service>`

Example:
```
dotnet grpc-cli dump http://localhost:10042 MegaCorp.TimeService
syntax = "proto3";
package = ProtoBuf.Grpc.Internal;
message Empty {
}


syntax = "proto3";
package = google.protobuf;
message Timestamp {
  int64 seconds = 1;
  int32 nanos = 2;
}


syntax = "proto3";
package = MegaCorp;
message TimeResult {
  Timestamp Time = 1;
}


syntax = "proto3";
package = MegaCorp;
service TimeService {
   rpc Subscribe(Empty) returns (stream TimeResult);
}
```