# ProgrammingChallenge2

Welcome to the 2. Programming Challenge.

The task in this challenge is to implement a data serializer/deserializer which minimizes the amount of data send between an edge device and the cloud.

## Background

Lets imagine your company has 10000 Iot devices deployed in the field.
They are connected via mobile data to your cloud infrastructure.
Each of these devices sends 1 status update per second to your cloud.
By using the default `JsonCodec.cs` provided as an example implementation this would yield approximately:

800MB per Month * 10000 devices = 8TB data per Month

If each GB of data costs around 2€ this would yield a monthly cost of:

8TB * 2€ / 1GB = 16.000€

Somehow your boss thinks this is a lot and gives you the task of reducing data usage while keeping the update interval of 1 second.

## How to contribute

1. In the folder `Codecs` create a folder with your name.
1. In this folder create a class which implement the `ICodecFactory.cs` interface. This class is used to create serializer and deserializer instances and gives your implementation a name.
1. Write a class which implements `IDecoder.cs` and a class which implements `IEncoder.cs` and use these implementations in your `ICodecFactory.cs`
   1. The encoder transforms a instance of `IotDevice` into a byte array.
   1. This byte array is transmitted to the cloud
   1. The decoder transform the byte array back to an `IotDevice`
1. A starting point for the implementation can be seen in the folder `Codecs/Jan.Reinhardt/`
1. To test your implementation adjust the `Program.cs` file as seen bellow. Change the initialisation of the `codecFactory` variable.
1. Run `dotnet run` to see how many bytes your solution needs for the given amount of messages.
1. To submit your solution just send a PR to the Github repo.

> All participants work with the same repository, so please only commit changes in your own folder. Keep your changes to the `Program.cs` only locally. Expect for bugfixes or other improvements :-)

```csharp
var codecFactory = new Codecs.<YourName>.<YourFactory>();
```

## Notes

- The `TransmissionSequence.Run` method has a `debug` parameter. Set this to `true` in `Program.cs` to get insight into your solution.
- Because this is only a test app it is easily possible to cheat. Please find a solution which would also work in the real world (Encoder and Decoder are on different computers)
- The solution only needs to work for the `DataSource.cs` provided in this repo and not for any data source possible.
- Other networking problems such as message integrity, message order, missing messages, etc can be ignored for this task.
- By default all available codec factories will be run
- Use the RunOnlyThis attribute to run only your codec factory (see `Codecs/Jan.Reinhard/JsonCodecFactory.cs`)

## End of the competition

The competition ends at 27. August 2020.

After the competition ends the solution which manages to get the data across with the minimum amount of bytes wins!

Happy coding!
