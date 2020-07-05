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
---
File: ProtoBuf.Grpc.Internal.Empty.proto
---
syntax = "proto3";
package ProtoBuf.Grpc.Internal;

message Empty {
}

---
File: MegaCorp.TimeResult.proto
---
syntax = "proto3";
import "google/protobuf/timestamp.proto";
package MegaCorp;

message TimeResult {
  Timestamp Time = 1;
}

---
File: MegaCorp.TimeService.proto
---
syntax = "proto3";
import "ProtoBuf.Grpc.Internal.Empty.proto";
import "MegaCorp.TimeResult.proto";
package MegaCorp;

service TimeService {
   rpc Subscribe(Empty) returns (stream TimeResult);
}
```

# Write proto to disk

`dotnet grpc-cli dump <address> <service> -o <directory>`

Example:
```
dotnet grpc-cli dump http://localhost:10042 MegaCorp.TimeService -o ./protos
```