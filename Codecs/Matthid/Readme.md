# Readme

Best encoding is `CustomCodecFactory` which enables all tricks.

## Used optimizations (CustomCodecFactory)

- Do not send unit strings (as they are always the same)
- Send float instead of double (enough for the check, I think you can even improve this further)
- Guid as bytes (-> 16 bytes)
- (optional) Send uptime increment as single byte
- (optional) Msg & ID as always the same, so only send the first time and null otherwise (we could further save 1 bits per message...).
- (optional) Improve string encoding (use a 5 bit encoding, skip `00000`)
- (optional) Remove spaces from status message (always at the same position)

## Further Factories

- `ProtobufCodecFactory`: Was implemented to compare protocol overhead for this scenario (My stand is: ProtoBuf is good enough for such use-cases). Some optimizations have been added.
- `StupidProtobufCodecFactory`: To "see" how plain protobuf compares (without any optimizations 

## Generate Messages.cs

1. Run `dotnet run` (to restore packages)
2. Open cmd/powershell/git bash and navigate to this dir
3. Run `nuget_cache/packages/google.protobuf.tools/3.12.4/tools/windows_x64/protoc.exe --csharp_out=. messages.proto`
