[![Support](https://img.shields.io/badge/Support-Official-green.svg)](mailto:support@perforce.com)

# p4api.net

P4API.NET is a wrapper for the Helix Core C/C++ API to be used in C# and other .NET languages.

[BUILD.md](BUILD.md)
[P4Bridge.md](p4bridge/P4Bridge.md)
[BridgeUnitTest.md](p4bridge-unit-test/BridgeUnitTest.md)
## Mockable Testing Support

Consuming projects can now unit test their code without requiring a real Perforce server connection. P4API.NET provides an `IP4Server` interface that enables dependency injection of mock implementations.

### Using Mocks in Your Tests

Create a mock `IP4Server` implementation and inject it via the `Connection` constructor:

```csharp
// Create mock (manually or using Moq)
var mockServer = new Mock<IP4Server>();
mockServer.Setup(s => s.IsConnected()).Returns(true);

// Inject into Connection
var connection = new Connection(
    new Server(new ServerAddress("localhost:1666")),
    multithreaded: false,
    p4ServerFactory: () => mockServer.Object
);

// Now test your Repository code without a real server
var repository = new Repository(server, connection);
var result = repository.GetBranchSpecs(options);  // Uses mock, no real server needed
```

### Example Implementations

- **Manual Mock**: See `p4api.net-mockable-tests/MockP4Server.cs` for a complete manual implementation
- **Moq Framework**: See `p4api.net-mockable-tests/MoqP4ServerTests.cs` for Moq usage examples
- **Integration Tests**: See `p4api.net-mockable-tests/ConnectionMockingTests.cs` for Connection integration examples

For more details, see the [p4api.net-mockable-tests](p4api.net-mockable-tests/) project and its test files.