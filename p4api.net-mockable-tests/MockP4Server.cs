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
using System.Collections.Generic;
using Perforce.P4;

namespace Perforce.P4.Tests.Mocking
{
    /// <summary>
    /// Example mock implementation of IP4Server for unit testing consuming projects.
    /// 
    /// This class demonstrates how to create a mockable P4Server for testing without
    /// requiring a real Perforce server. Replace with your own implementation or use
    /// Moq to create dynamic mocks.
    /// </summary>
    public class MockP4Server : IP4Server
    {
        private uint _nextCmdId = 1000;
        private Dictionary<uint, TaggedObjectList> _commandTaggedOutput = new();
        private Dictionary<uint, P4ClientErrorList> _commandErrors = new();
        private Dictionary<uint, P4ClientInfoMessageList> _commandInfoResults = new();
        private Dictionary<uint, string> _commandTextOutput = new();
        private Dictionary<uint, byte[]> _commandBinaryOutput = new();

        public MockP4Server()
        {
            IsConnected_Value = true;
        }

        #region Properties

        public string User { get; set; } = "testuser";
        public string Password { get; set; } = "";
        public string Client { get; set; } = "testclient";
        public string Port { get; set; } = "localhost:1666";
        public string CurrentWorkingDirectory { get; set; } = "";
        public string CharacterSet { get; set; } = "utf8";
        public string Config { get; } = "";
        public string ProgramName { get; set; } = "p4api.net-mockable-tests";
        public string ProgramVersion { get; set; } = "1.0";
        public bool UseUnicode { get; } = true;
        public int ApiLevel { get; } = 9999; // Mock high API level
        public bool RequiresLogin { get; } = false;
        public P4ClientError ConnectionError { get; set; }
        public ProgressHandler Progress { get; set; }
        public P4Server.ResolveHandlerDelegate ResolveHandler { get; set; }
        public P4Server.ResolveAHandlerDelegate ResolveAHandler { get; set; }
        public P4Server.PromptHandlerDelegate PromptHandler { get; set; }
        public IKeepAlive KeepAlive { get; set; }

        // Mock-specific properties
        public bool IsConnected_Value { get; set; }

        #endregion

        #region Events

        public event P4Server.TaggedOutputDelegate TaggedOutputReceived;
        public event P4Server.ErrorDelegate ErrorReceived;
        public event P4Server.InfoResultsDelegate InfoResultsReceived;
        public event P4Server.TextResultsDelegate TextResultsReceived;
        public event P4Server.BinaryResultsDelegate BinaryResultsReceived;
        public event P4Server.CommandEchoDelegate CommandEcho;
        public event P4Server.ResponseTimeEchoDelegate ResponseTimeEcho;

        #endregion

        #region Methods

        public bool Login(string password, StringList options)
        {
            Password = password;
            return true;
        }

        public bool Logout(StringList options, string user = null)
        {
            return true;
        }

        public bool IsConnected()
        {
            return IsConnected_Value;
        }

        public bool UrlHandled()
        {
            return false;
        }

        public void Reconnect()
        {
            IsConnected_Value = true;
        }

        public void Close()
        {
            IsConnected_Value = false;
        }

        public void Disconnect()
        {
            IsConnected_Value = false;
        }

        public uint getCmdId()
        {
            return ++_nextCmdId;
        }

        public bool RunCommand(string cmd, uint cmdId, bool tagged, string[] args, int argc)
        {
            CommandEcho?.Invoke($"Running: {cmd}");
            
            // Simulate command execution - fire appropriate output events
            if (_commandTaggedOutput.ContainsKey(cmdId))
            {
                TaggedOutputReceived?.Invoke(cmdId, 0, null);
            }
            
            if (_commandErrors.ContainsKey(cmdId))
            {
                // Fire error event for each error
                var errors = _commandErrors[cmdId];
                if (errors != null)
                {
                    foreach (var err in errors)
                    {
                        ErrorReceived?.Invoke(cmdId, (int)err.ErrorSeverity, err.ErrorNumber, err.ErrorMessage);
                    }
                }
            }

            return !(_commandErrors.ContainsKey(cmdId) && _commandErrors[cmdId]?.Count > 0);
        }

        public void CancelCommand(uint cmdId)
        {
            // Mock implementation - just clear the command
            _commandTaggedOutput.Remove(cmdId);
        }

        public int GetParallelOperationCount()
        {
            return 0;
        }

        public TaggedObjectList GetTaggedOutput(uint cmdId)
        {
            return _commandTaggedOutput.ContainsKey(cmdId) 
                ? _commandTaggedOutput[cmdId] 
                : null;
        }

        public P4ClientErrorList GetErrorResults(uint cmdId)
        {
            return _commandErrors.ContainsKey(cmdId)
                ? _commandErrors[cmdId]
                : null;
        }

        public P4ClientInfoMessageList GetInfoResults(uint cmdId)
        {
            return _commandInfoResults.ContainsKey(cmdId)
                ? _commandInfoResults[cmdId]
                : null;
        }

        public string GetTextResults(uint cmdId)
        {
            return _commandTextOutput.ContainsKey(cmdId)
                ? _commandTextOutput[cmdId]
                : null;
        }

        public byte[] GetBinaryResults(uint cmdId)
        {
            return _commandBinaryOutput.ContainsKey(cmdId)
                ? _commandBinaryOutput[cmdId]
                : null;
        }

        public void EchoCommand(string cmd, StringList args)
        {
            CommandEcho?.Invoke($"{cmd} {string.Join(" ", args ?? new StringList())}");
        }

        public void EchoResponseTime(string cmd, StringList args, TimeSpan responseTime)
        {
            ResponseTimeEcho?.Invoke($"{cmd} completed in {responseTime.TotalMilliseconds}ms");
        }

        public void EchoCommand(string str)
        {
            CommandEcho?.Invoke(str);
        }

        public void SetDataSet(uint cmdId, string value)
        {
            // Mock implementation - no-op
        }

        public string GetDataSet(uint cmdId)
        {
            return "";
        }

        public void SetConnectionData(string port, string user, string password, string client)
        {
            Port = port;
            User = user;
            Password = password;
            Client = client;
        }

        public void SetProgressHandler(ProgressHandler handler)
        {
            Progress = handler;
        }

        public void ResetProgressHandler()
        {
            Progress = null;
        }

        public void SetProtocol(string key, string val)
        {
            // Mock implementation - no-op
        }

        public void SetThreadOwner(int threadId)
        {
            // Mock implementation - no-op
        }

        public string GetTicketFile()
        {
            return "";
        }

        public void SetTicketFile(string ticketFile)
        {
            // Mock implementation - no-op
        }

        public void PauseRunCmdTimer(uint cmdId)
        {
            // Mock implementation - no-op
        }

        public void ContinueRunCmdTimer(uint cmdId)
        {
            // Mock implementation - no-op
        }

        public bool IsCommandPaused(uint cmdId)
        {
            return false;
        }

        public void Dispose()
        {
            _commandTaggedOutput.Clear();
            _commandErrors.Clear();
            _commandInfoResults.Clear();
            _commandTextOutput.Clear();
            _commandBinaryOutput.Clear();
        }

        #endregion

        #region Helper Methods for Testing

        /// <summary>
        /// Set the tagged output that will be returned for a command
        /// </summary>
        public void SetTaggedOutput(uint cmdId, TaggedObjectList output)
        {
            _commandTaggedOutput[cmdId] = output;
        }

        /// <summary>
        /// Set the errors that will be returned for a command
        /// </summary>
        public void SetErrorResults(uint cmdId, P4ClientErrorList errors)
        {
            _commandErrors[cmdId] = errors;
        }

        /// <summary>
        /// Set the text output that will be returned for a command
        /// </summary>
        public void SetTextOutput(uint cmdId, string output)
        {
            _commandTextOutput[cmdId] = output;
        }

        /// <summary>
        /// Set the binary output that will be returned for a command
        /// </summary>
        public void SetBinaryOutput(uint cmdId, byte[] output)
        {
            _commandBinaryOutput[cmdId] = output;
        }

        #endregion
    }
}
