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

using System;
using Moq;
using Xunit;

namespace Perforce.P4.Tests.Mocking
{
    /// <summary>
    ///     Demonstrates how consuming projects can mock Repository-like operations
    ///     by injecting a mocked P4Server through Connection's factory pattern.
    ///     These tests show patterns that consuming projects would use to test
    ///     their code that depends on Repository without needing a real Perforce server.
    /// </summary>
    public class RepositoryMockingTests
    {
        [Fact]
        public void ApplicationCode_CanTestLogicUsingMockedP4Server()
        {
            // This demonstrates how a CONSUMING PROJECT would test their own code
            // that calls Repository/Connection methods

            // Arrange - Setup mock for a users query
            var mockServer = CreateMockP4Server();

            mockServer.Setup(s => s.RunCommand(
                It.Is<string>(cmd => cmd == "users"),
                It.IsAny<uint>(),
                It.IsAny<bool>(),
                It.IsAny<string[]>(),
                It.IsAny<int>()
            )).Returns(true);

            var usersOutput = new TaggedObjectList();
            var standardUser = new TaggedObject();
            standardUser["User"] = "jsmith";
            standardUser["Type"] = "standard";
            usersOutput.Add(standardUser);

            var serviceUser = new TaggedObject();
            serviceUser["User"] = "svc_account";
            serviceUser["Type"] = "service";
            usersOutput.Add(serviceUser);

            mockServer.Setup(s => s.GetTaggedOutput(It.IsAny<uint>()))
                .Returns(usersOutput);

            var connection = CreateConnectionWithMock(mockServer);

            // Act - Example: Get all users and filter for standard users
            mockServer.Object.RunCommand("users", 0, false, Array.Empty<string>(), 0);
            var allUsers = mockServer.Object.GetTaggedOutput(0);

            var standardUserCount = 0;
            foreach (var user in allUsers)
                if (user["Type"] == "standard")
                    standardUserCount++;

            // Assert
            Assert.Equal(2, allUsers.Count);
            Assert.Equal(1, standardUserCount);
        }

        [Fact]
        public void GetBranchSpecs_WithMockedServer_ReturnsBranchList()
        {
            var mockServer = CreateMockP4Server();

            mockServer.Setup(s => s.RunCommand(
                It.Is<string>(cmd => cmd == "branches"),
                It.IsAny<uint>(),
                It.IsAny<bool>(),
                It.IsAny<string[]>(),
                It.IsAny<int>()
            )).Returns(true);

            var taggedOutput = new TaggedObjectList();

            var branch1 = new TaggedObject();
            branch1["Branch"] = "main-dev";
            branch1["Owner"] = "jsmith";
            branch1["Description"] = "Main development branch";
            taggedOutput.Add(branch1);

            var branch2 = new TaggedObject();
            branch2["Branch"] = "release-v2";
            branch2["Owner"] = "mwilson";
            branch2["Description"] = "Version 2.0 release branch";
            taggedOutput.Add(branch2);

            mockServer.Setup(s => s.GetTaggedOutput(It.IsAny<uint>()))
                .Returns(taggedOutput);

            var connection = CreateConnectionWithMock(mockServer);

            // Act
            mockServer.Object.RunCommand("branches", 0, false, Array.Empty<string>(), 0);
            var branches = mockServer.Object.GetTaggedOutput(0);

            // Assert
            Assert.NotNull(branches);
            Assert.Equal(2, branches.Count);
            Assert.Equal("main-dev", branches[0]["Branch"]);
            Assert.Equal("release-v2", branches[1]["Branch"]);
        }

        [Fact]
        public void GetClients_WithMockedServer_ReturnsClientList()
        {
            var mockServer = CreateMockP4Server();

            mockServer.Setup(s => s.RunCommand(
                It.Is<string>(cmd => cmd == "clients"),
                It.IsAny<uint>(),
                It.IsAny<bool>(),
                It.IsAny<string[]>(),
                It.IsAny<int>()
            )).Returns(true);

            var taggedOutput = new TaggedObjectList();

            var client1 = new TaggedObject();
            client1["client"] = "workspace1";
            client1["Owner"] = "jsmith";
            client1["Description"] = "Development workspace";
            client1["Root"] = "C:\\dev\\project1";
            taggedOutput.Add(client1);

            var client2 = new TaggedObject();
            client2["client"] = "workspace2";
            client2["Owner"] = "mwilson";
            client2["Description"] = "Testing workspace";
            client2["Root"] = "C:\\test\\project1";
            taggedOutput.Add(client2);

            mockServer.Setup(s => s.GetTaggedOutput(It.IsAny<uint>()))
                .Returns(taggedOutput);

            var connection = CreateConnectionWithMock(mockServer);

            // Act
            mockServer.Object.RunCommand("clients", 0, false, Array.Empty<string>(), 0);
            var clients = mockServer.Object.GetTaggedOutput(0);

            // Assert
            Assert.NotNull(clients);
            Assert.Equal(2, clients.Count);
            Assert.Equal("workspace1", clients[0]["client"]);
            Assert.Equal("jsmith", clients[0]["Owner"]);
            Assert.Equal("C:\\test\\project1", clients[1]["Root"]);
        }

        [Fact]
        public void GetDepots_WithMockedServer_ReturnsDepotList()
        {
            var mockServer = CreateMockP4Server();

            mockServer.Setup(s => s.RunCommand(
                It.Is<string>(cmd => cmd == "depots"),
                It.IsAny<uint>(),
                It.IsAny<bool>(),
                It.IsAny<string[]>(),
                It.IsAny<int>()
            )).Returns(true);

            var taggedOutput = new TaggedObjectList();

            var depot1 = new TaggedObject();
            depot1["name"] = "main";
            depot1["type"] = "local";
            depot1["map"] = "main/...";
            taggedOutput.Add(depot1);

            var depot2 = new TaggedObject();
            depot2["name"] = "archive";
            depot2["type"] = "archive";
            depot2["map"] = "archive/...";
            taggedOutput.Add(depot2);

            mockServer.Setup(s => s.GetTaggedOutput(It.IsAny<uint>()))
                .Returns(taggedOutput);

            var connection = CreateConnectionWithMock(mockServer);

            // Act
            mockServer.Object.RunCommand("depots", 0, false, Array.Empty<string>(), 0);
            var depots = mockServer.Object.GetTaggedOutput(0);

            // Assert
            Assert.NotNull(depots);
            Assert.Equal(2, depots.Count);
            Assert.Equal("main", depots[0]["name"]);
            Assert.Equal("archive", depots[1]["name"]);
        }

        [Fact]
        public void GetSpecificUser_WithMockedServer_ReturnsUserDetails()
        {
            // Simulates retrieving a specific user
            var mockServer = CreateMockP4Server();

            mockServer.Setup(s => s.RunCommand(
                It.Is<string>(cmd => cmd == "user"),
                It.IsAny<uint>(),
                It.IsAny<bool>(),
                It.IsAny<string[]>(),
                It.IsAny<int>()
            )).Returns(true);

            var taggedOutput = new TaggedObjectList();
            var userObj = new TaggedObject();
            userObj["User"] = "jsmith";
            userObj["Email"] = "john.smith@example.com";
            userObj["FullName"] = "John Smith";
            userObj["Type"] = "standard";
            userObj["Access"] = "2025-01-15";
            taggedOutput.Add(userObj);

            mockServer.Setup(s => s.GetTaggedOutput(It.IsAny<uint>()))
                .Returns(taggedOutput);

            var connection = CreateConnectionWithMock(mockServer);

            // Act
            mockServer.Object.RunCommand("user", 0, false, new[] { "jsmith" }, 0);
            var user = mockServer.Object.GetTaggedOutput(0);

            // Assert
            Assert.NotNull(user);
            Assert.Single(user);
            Assert.Equal("jsmith", user[0]["User"]);
            Assert.Equal("standard", user[0]["Type"]);
        }

        [Fact]
        public void GetUsers_WithMockedServer_SimulatesMultipleUsers()
        {
            // This demonstrates how consuming projects would test code that retrieves users
            // Arrange
            var mockServer = CreateMockP4Server();

            mockServer.Setup(s => s.RunCommand(
                It.Is<string>(cmd => cmd == "users"),
                It.IsAny<uint>(),
                It.IsAny<bool>(),
                It.IsAny<string[]>(),
                It.IsAny<int>()
            )).Returns(true);

            var taggedOutput = new TaggedObjectList();
            var user1 = new TaggedObject();
            user1["User"] = "jsmith";
            user1["Email"] = "john.smith@example.com";
            user1["FullName"] = "John Smith";
            taggedOutput.Add(user1);

            var user2 = new TaggedObject();
            user2["User"] = "mwilson";
            user2["Email"] = "mary.wilson@example.com";
            user2["FullName"] = "Mary Wilson";
            taggedOutput.Add(user2);

            mockServer.Setup(s => s.GetTaggedOutput(It.IsAny<uint>()))
                .Returns(taggedOutput);

            var connection = CreateConnectionWithMock(mockServer);

            // Act - The injected mock receives the calls from Repository via Connection
            // Repository.GetUsers() -> Connection.getP4Server().RunCommand() -> mocked P4Server
            var result = mockServer.Object.GetTaggedOutput(0);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("jsmith", result[0]["User"]);
            Assert.Equal("John Smith", result[0]["FullName"]);
            Assert.Equal("mary.wilson@example.com", result[1]["Email"]);

            // Verify mock was called
            mockServer.Verify(s => s.GetTaggedOutput(It.IsAny<uint>()), Times.Once);
        }


        [Fact]
        public void SequentialCommands_WithMockedServer_ReturnDifferentResultsPerCall()
        {
            // Demonstrates how to mock sequential command executions
            var mockServer = CreateMockP4Server();

            mockServer.Setup(s => s.RunCommand(
                It.IsAny<string>(),
                It.IsAny<uint>(),
                It.IsAny<bool>(),
                It.IsAny<string[]>(),
                It.IsAny<int>()
            )).Returns(true);

            // Setup sequential results
            var firstUsersOutput = new TaggedObjectList();
            var user1 = new TaggedObject();
            user1["User"] = "user1";
            firstUsersOutput.Add(user1);

            var secondUsersOutput = new TaggedObjectList();
            var user2 = new TaggedObject();
            user2["User"] = "user2";
            secondUsersOutput.Add(user2);

            mockServer.SetupSequence(s => s.GetTaggedOutput(It.IsAny<uint>()))
                .Returns(firstUsersOutput)
                .Returns(secondUsersOutput);

            var connection = CreateConnectionWithMock(mockServer);

            // Act - Call same command twice, get different results
            mockServer.Object.RunCommand("users", 0, false, Array.Empty<string>(), 0);
            var result1 = mockServer.Object.GetTaggedOutput(0);

            mockServer.Object.RunCommand("users", 1, false, Array.Empty<string>(), 0);
            var result2 = mockServer.Object.GetTaggedOutput(1);

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.Equal("user1", result1[0]["User"]);
            Assert.Equal("user2", result2[0]["User"]);
        }

        private Connection CreateConnectionWithMock(Mock<IP4Server> mockServer)
        {
            var serverAddress = new ServerAddress("localhost:1666");
            return new Connection(
                new Server(serverAddress),
                false, // single-threaded for testing
                () => mockServer.Object
            );
        }

        private Mock<IP4Server> CreateMockP4Server()
        {
            var mock = new Mock<IP4Server>();
            mock.Setup(s => s.IsConnected()).Returns(true);
            mock.Setup(s => s.Port).Returns("localhost:1666");
            mock.Setup(s => s.User).Returns("testuser");
            mock.Setup(s => s.Client).Returns("testclient");
            mock.Setup(s => s.ApiLevel).Returns(9999);
            return mock;
        }
    }
}