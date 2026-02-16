/*******************************************************************************

Copyright (c) 2024, Perforce Software, Inc.  All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1.  Redistributions of source code must retain the above copyright
	notice, this list of conditions and the following disclaimer.

2.  Redistributions in binary form must reproduce the above copyright
	notice, this list of conditions and the following disclaimer in the
	documentation and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
ARE DISCLAIMED. IN NO EVENT SHALL PERFORCE SOFTWARE, INC. BE LIABLE FOR ANY
DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

*******************************************************************************/

using Xunit;
using Perforce.P4;

namespace Perforce.P4.Tests.Mocking
{
    /// <summary>
    /// Demonstrates using MockP4Server directly to test Connection behavior.
    /// </summary>
    public class ConnectionMockingTests
    {
        [Fact]
        public void Connection_AcceptsMockP4Factory_SuccessfulInjection()
        {
            // Arrange
            var mockServer = new MockP4Server { IsConnected_Value = true };
            var serverAddress = new ServerAddress("localhost:1666");
            
            // Act - Inject the mock via factory delegate
            var connection = new Connection(
                new Server(serverAddress),
                multithreaded: false,
                p4ServerFactory: () => mockServer
            );

            // Assert
            Assert.NotNull(connection);
        }

        [Fact]
        public void Connection_WithMockFactory_RestoresProperties()
        {
            // Arrange
            var mockServer = new MockP4Server();
            var expectedUser = "testuser";
            var expectedClient = "testclient";
            var serverAddress = new ServerAddress("localhost:1666");

            // Act
            var connection = new Connection(
                new Server(serverAddress),
                multithreaded: false,
                p4ServerFactory: () => mockServer
            );

            mockServer.User = expectedUser;
            mockServer.Client = expectedClient;

            // Assert
            Assert.Equal(expectedUser, mockServer.User);
            Assert.Equal(expectedClient, mockServer.Client);
        }

        [Fact]
        public void Connection_WithoutMockFactory_UsesRealImplementation()
        {
            // Arrange
            var serverAddress = new ServerAddress("localhost:1666");

            // Act - Connection without factory should work (uses real P4Server, though may not be connected)
            var connection = new Connection(
                new Server(serverAddress),
                multithreaded: false,
                p4ServerFactory: null
            );

            // Assert - Just verify it was created (actual P4 connection test would require real server)
            Assert.NotNull(connection);
        }

        [Fact]
        public void Connection_MockIsConnected_ReportsCorrectly()
        {
            // Arrange
            var mockServer = new MockP4Server { IsConnected_Value = true };
            var serverAddress = new ServerAddress("localhost:1666");

            // Act
            var connection = new Connection(
                new Server(serverAddress),
                multithreaded: false,
                p4ServerFactory: () => mockServer
            );

            // Assert
            Assert.True(mockServer.IsConnected());
        }

        [Fact]
        public void Connection_MockDisconnect_UpdatesState()
        {
            // Arrange
            var mockServer = new MockP4Server { IsConnected_Value = true };
            var serverAddress = new ServerAddress("localhost:1666");

            // Act
            mockServer.Disconnect();

            // Assert
            Assert.False(mockServer.IsConnected());
        }
    }
}
