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
using System.Collections.Generic;
using Perforce.P4;

namespace Perforce.P4.Tests.Mocking
{
    /// <summary>
    /// Demonstrates using mocked P4Server with Repository methods.
    /// Shows how consuming projects can test their usage of Repository
    /// (GetUsers, GetUser, GetClients, etc.) without a real Perforce server.
    /// </summary>
    public class RepositoryMockingTests
    {
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

        private Connection CreateConnectionWithMock(Mock<IP4Server> mockServer)
        {
            var serverAddress = new ServerAddress("localhost:1666");
            return new Connection(
                new Server(serverAddress),
                multithreaded: false,
                p4ServerFactory: () => mockServer.Object
            );
        }

        private Repository CreateRepository(Mock<IP4Server> mockServer)
        {
            var connection = CreateConnectionWithMock(mockServer);
            var server = new Server(new ServerAddress("localhost:1666"));
            return new Repository(server, connection);
        }

        [Fact]
        public void Repository_GetUsers_WithMockedServer_ReturnsExpectedData()
        {
            // Arrange
            var mockServer = CreateMockP4Server();
            
            // Setup mock to track command execution
            mockServer.Setup(s => s.RunCommand(
                It.Is<string>(cmd => cmd == "users"),
                It.IsAny<uint>(),
                It.IsAny<bool>(),
                It.IsAny<string[]>(),
                It.IsAny<int>()
            )).Returns(true);

            // Create mock tagged output for users command
            var taggedOutput = new TaggedObjectList();
            var user1 = new TaggedObject();
            user1["User"] = "jsmith";
            user1["Email"] = "john.smith@example.com";
            user1["FullName"] = "John Smith";
            user1["Type"] = "standard";
            taggedOutput.Add(user1);

            var user2 = new TaggedObject();
            user2["User"] = "mwilson";
            user2["Email"] = "mary.wilson@example.com";
            user2["FullName"] = "Mary Wilson";
            user2["Type"] = "standard";
            taggedOutput.Add(user2);

            mockServer.Setup(s => s.GetTaggedOutput(It.IsAny<uint>()))
                .Returns(taggedOutput);

            var repository = CreateRepository(mockServer);

            // Act
            var users = repository.GetUsers(null);

            // Assert
            Assert.NotNull(users);
            Assert.Equal(2, users.Count);
            Assert.Equal("jsmith", users[0].Name);
            Assert.Equal("John Smith", users[0].FullName);
            Assert.Equal("mary.wilson@example.com", users[1].EmailAddress);

            // Verify the mock was called with expected command
            mockServer.Verify(s => s.RunCommand(
                It.Is<string>(cmd => cmd == "users"),
                It.IsAny<uint>(),
                It.IsAny<bool>(),
                It.IsAny<string[]>(),
                It.IsAny<int>()
            ), Times.Once);
        }

        [Fact]
        public void Repository_GetUser_WithMockedServer_ReturnsSpecificUser()
        {
            // Arrange
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
            taggedOutput.Add(userObj);

            mockServer.Setup(s => s.GetTaggedOutput(It.IsAny<uint>()))
                .Returns(taggedOutput);

            var repository = CreateRepository(mockServer);

            // Act
            var user = repository.GetUser("jsmith", null);

            // Assert
            Assert.NotNull(user);
            Assert.Equal("jsmith", user.Name);
            Assert.Equal("John Smith", user.FullName);
            Assert.Equal("john.smith@example.com", user.EmailAddress);
        }

        [Fact]
        public void Repository_GetClients_WithMockedServer_ReturnsMultipleClients()
        {
            // Arrange
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

            var repository = CreateRepository(mockServer);

            // Act
            var clients = repository.GetClients(null);

            // Assert
            Assert.NotNull(clients);
            Assert.Equal(2, clients.Count);
            Assert.Equal("workspace1", clients[0].Name);
            Assert.Equal("jsmith", clients[0].Owner);
            Assert.Equal("C:\\dev\\project1", clients[0].Root);
        }

        [Fact]
        public void Repository_GetClient_WithMockedServer_ReturnsSingleClient()
        {
            // Arrange
            var mockServer = CreateMockP4Server();
            
            mockServer.Setup(s => s.RunCommand(
                It.Is<string>(cmd => cmd == "client"),
                It.IsAny<uint>(),
                It.IsAny<bool>(),
                It.IsAny<string[]>(),
                It.IsAny<int>()
            )).Returns(true);

            var taggedOutput = new TaggedObjectList();
            var clientObj = new TaggedObject();
            clientObj["Client"] = "workspace1";
            clientObj["Owner"] = "jsmith";
            clientObj["Description"] = "Development workspace for project1";
            clientObj["Root"] = "C:\\dev\\project1";
            clientObj["Host"] = "devmachine01";
            taggedOutput.Add(clientObj);

            mockServer.Setup(s => s.GetTaggedOutput(It.IsAny<uint>()))
                .Returns(taggedOutput);

            var repository = CreateRepository(mockServer);

            // Act
            var client = repository.GetClient("workspace1", null);

            // Assert
            Assert.NotNull(client);
            Assert.Equal("workspace1", client.Name);
            Assert.Equal("jsmith", client.Owner);
            Assert.Equal("C:\\dev\\project1", client.Root);
        }

        [Fact]
        public void Repository_GetDepots_WithMockedServer_ReturnsDepotList()
        {
            // Arrange
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

            var repository = CreateRepository(mockServer);

            // Act
            var depots = repository.GetDepots(null);

            // Assert
            Assert.NotNull(depots);
            Assert.Equal(2, depots.Count);
            Assert.Equal("main", depots[0].Name);
            Assert.Equal("local", depots[0].Type);
            Assert.Equal("archive", depots[1].Name);
        }

        [Fact]
        public void Repository_Command_WithMockerErrors_HandlesErrorResults()
        {
            // Arrange
            var mockServer = CreateMockP4Server();
            
            // Simulate a command failure
            mockServer.Setup(s => s.RunCommand(
                It.IsAny<string>(),
                It.IsAny<uint>(),
                It.IsAny<bool>(),
                It.IsAny<string[]>(),
                It.IsAny<int>()
            )).Returns(false); // Command failed

            var errorList = new P4ClientErrorList();
            var error = new P4ClientError { 
                ErrorMessage = "User jsmith not found.",
                ErrorNumber = 2,
                ErrorSeverity = ErrorSeverity.E_FAILED
            };
            errorList.Add(error);

            mockServer.Setup(s => s.GetErrorResults(It.IsAny<uint>()))
                .Returns(errorList);

            var repository = CreateRepository(mockServer);

            // Act & Assert - exception should be thrown
            var exception = Assert.Throws<P4Exception>(() =>
            {
                repository.GetUser("jsmith", null);
            });
            
            Assert.NotNull(exception);
        }

        [Fact]
        public void Repository_CreateUser_WithMockedServer_VerifiesUserCreation()
        {
            // Arrange
            var mockServer = CreateMockP4Server();
            
            mockServer.Setup(s => s.RunCommand(
                It.Is<string>(cmd => cmd == "user"),
                It.IsAny<uint>(),
                It.IsAny<bool>(),
                It.IsAny<string[]>(),
                It.IsAny<int>()
            )).Returns(true);

            mockServer.Setup(s => s.GetTextResults(It.IsAny<uint>()))
                .Returns("User jsmith saved.");

            var repository = CreateRepository(mockServer);

            // Act
            var newUser = new User
            {
                Name = "jsmith",
                FullName = "John Smith",
                EmailAddress = "john.smith@example.com"
            };

            repository.CreateUser(newUser, null);

            // Assert - Verify P4Server.RunCommand was called with "user" command
            mockServer.Verify(s => s.RunCommand(
                It.Is<string>(cmd => cmd == "user"),
                It.IsAny<uint>(),
                It.IsAny<bool>(),
                It.IsAny<string[]>(),
                It.IsAny<int>()
            ), Times.Once);
        }

        [Fact]
        public void Repository_GetBranchSpecs_WithMockedServer_ReturnsBranchList()
        {
            // Arrange
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

            var repository = CreateRepository(mockServer);

            // Act
            var branches = repository.GetBranchSpecs(null);

            // Assert
            Assert.NotNull(branches);
            Assert.Equal(2, branches.Count);
            Assert.Equal("main-dev", branches[0].Name);
            Assert.Equal("release-v2", branches[1].Name);
        }

        [Fact]
        public void Repository_MockedServerCanSimulateSequence_OfOperations()
        {
            // Arrange - Create a sequence of operations
            var mockServer = CreateMockP4Server();
            var repository = CreateRepository(mockServer);

            // Setup for multiple calls with different responses
            var firstCallOutput = new TaggedObjectList();
            var obj1 = new TaggedObject();
            obj1["User"] = "user1";
            firstCallOutput.Add(obj1);

            var secondCallOutput = new TaggedObjectList();
            var obj2 = new TaggedObject();
            obj2["User"] = "user2";
            secondCallOutput.Add(obj2);

            // Queue up responses for sequential calls
            mockServer.SetupSequence(s => s.GetTaggedOutput(It.IsAny<uint>()))
                .Returns(firstCallOutput)
                .Returns(secondCallOutput);

            mockServer.Setup(s => s.RunCommand(
                It.IsAny<string>(),
                It.IsAny<uint>(),
                It.IsAny<bool>(),
                It.IsAny<string[]>(),
                It.IsAny<int>()
            )).Returns(true);

            // Act - Make sequential calls
            var user1 = repository.GetUser("user1", null);
            var user2 = repository.GetUser("user2", null);

            // Assert
            Assert.NotNull(user1);
            Assert.NotNull(user2);
            Assert.Equal("user1", user1.Name);
            Assert.Equal("user2", user2.Name);
        }

        [Fact]
        public void Repository_CanTestApplicationLogicWithMockedServer()
        {
            // This demonstrates testing APPLICATION code that uses Repository,
            // not testing Repository itself

            // Arrange
            var mockServer = CreateMockP4Server();

            // Mock GetUsers to return a specific list
            mockServer.Setup(s => s.RunCommand(
                It.Is<string>(cmd => cmd == "users"),
                It.IsAny<uint>(),
                It.IsAny<bool>(),
                It.IsAny<string[]>(),
                It.IsAny<int>()
            )).Returns(true);

            var usersOutput = new TaggedObjectList();
            var user1 = new TaggedObject();
            user1["User"] = "jsmith";
            user1["Type"] = "standard";
            usersOutput.Add(user1);

            var user2 = new TaggedObject();
            user2["User"] = "svc_account";
            user2["Type"] = "service";
            usersOutput.Add(user2);

            mockServer.Setup(s => s.GetTaggedOutput(It.IsAny<uint>()))
                .Returns(usersOutput);

            var repository = CreateRepository(mockServer);

            // Act - Example application logic: count non-service users
            var allUsers = repository.GetUsers(null);
            var standardUsers = allUsers.FindAll(u => u.Type == UserType.Standard);

            // Assert
            Assert.Equal(2, allUsers.Count);
            Assert.Single(standardUsers);
        }
    }
}
