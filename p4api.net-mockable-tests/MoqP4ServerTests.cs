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
using Moq;
using Perforce.P4;

namespace Perforce.P4.Tests.Mocking
{
    /// <summary>
    /// Demonstrates using Moq to dynamically create mock P4Server implementations.
    /// This shows an alternative to the MockP4Server class for more flexible testing.
    /// </summary>
    public class MoqP4ServerTests
    {
        [Fact]
        public void Connection_WithMoqMock_CanInjectBehavior()
        {
            // Arrange
            var mockServer = new Mock<IP4Server>();
            mockServer.Setup(s => s.IsConnected()).Returns(true);
            mockServer.Setup(s => s.User).Returns("moquser");
            mockServer.Setup(s => s.Client).Returns("moqclient");

            var serverAddress = new ServerAddress("localhost:1666");

            // Act
            var connection = new Connection(
                new Server(serverAddress),
                multithreaded: false,
                p4ServerFactory: () => mockServer.Object
            );

            // Assert
            Assert.True(mockServer.Object.IsConnected());
            Assert.Equal("moquser", mockServer.Object.User);
            Assert.Equal("moqclient", mockServer.Object.Client);
        }

        [Fact]
        public void MoqMock_CanVerifyMethodCalls()
        {
            // Arrange
            var mockServer = new Mock<IP4Server>();
            mockServer.Setup(s => s.Disconnect());

            // Act
            mockServer.Object.Disconnect();

            // Assert
            mockServer.Verify(s => s.Disconnect(), Times.Once);
        }

        [Fact]
        public void MoqMock_CanSetupGetCmdId()
        {
            // Arrange
            var mockServer = new Mock<IP4Server>();
            mockServer.Setup(s => s.getCmdId()).Returns(12345u);

            // Act
            var cmdId = mockServer.Object.getCmdId();

            // Assert
            Assert.Equal(12345u, cmdId);
        }

        [Fact]
        public void MoqMock_CanThrowExceptions()
        {
            // Arrange
            var mockServer = new Mock<IP4Server>();
            mockServer.Setup(s => s.RunCommand(
                It.IsAny<string>(),
                It.IsAny<uint>(),
                It.IsAny<bool>(),
                It.IsAny<string[]>(),
                It.IsAny<int>()
            )).Throws<P4Exception>();

            // Act & Assert
            Assert.Throws<P4Exception>(() => 
                mockServer.Object.RunCommand("test", 1, false, new string[0], 0)
            );
        }

        [Fact]
        public void MoqMock_Property_CanBeMocked()
        {
            // Arrange
            var mockServer = new Mock<IP4Server>();
            mockServer.SetupProperty(s => s.Port, "localhost:1666");

            // Act
            var port = mockServer.Object.Port;

            // Assert
            Assert.Equal("localhost:1666", port);
        }

        [Fact]
        public void MoqMock_CanSimulateCommandSequence()
        {
            // Arrange
            var mockServer = new Mock<IP4Server>();
            var cmdIds = new uint[] { 1001, 1002, 1003 };
            var cmdIdQueue = new Queue<uint>(cmdIds);

            mockServer.Setup(s => s.getCmdId())
                .Returns(() => cmdIdQueue.Dequeue());

            // Act
            var id1 = mockServer.Object.getCmdId();
            var id2 = mockServer.Object.getCmdId();
            var id3 = mockServer.Object.getCmdId();

            // Assert
            Assert.Equal(1001u, id1);
            Assert.Equal(1002u, id2);
            Assert.Equal(1003u, id3);
        }
    }
}
