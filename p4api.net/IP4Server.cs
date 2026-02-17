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

/*******************************************************************************
 * Name		: IP4Server.cs
 *
 * Author	: Dustyn Gilliland (Electronic Arts, dgilliland@ea.com)
 *
 * Description	: Interface for P4Server to enable mocking and dependency injection
 *                for consuming projects performing unit tests without a real P4 server.
 *
 ******************************************************************************/

using System;

namespace Perforce.P4
{
    /// <summary>
    ///     Interface for P4Server connection and command execution.
    ///     This interface enables consuming projects to mock P4Server behavior for unit testing
    ///     without requiring a connection to a real Perforce server. Implementations can inject
    ///     mock servers via Connection constructor using a factory delegate.
    /// </summary>
    public interface IP4Server : IDisposable
    {
        #region Properties

        /// <summary>
        ///     The user name used by the connection
        /// </summary>
        string User { get; set; }

        /// <summary>
        ///     The user's password used by the connection
        /// </summary>
        string Password { get; set; }

        /// <summary>
        ///     The workspace (client) used by the connection
        /// </summary>
        string Client { get; set; }

        /// <summary>
        ///     The hostname:port used by the connection
        /// </summary>
        string Port { get; set; }

        /// <summary>
        ///     The current working directory (cwd) used by the p4 server
        /// </summary>
        string CurrentWorkingDirectory { get; set; }

        /// <summary>
        ///     The character set used by the connection
        /// </summary>
        string CharacterSet { get; set; }

        /// <summary>
        ///     The config file used by the connection
        /// </summary>
        string Config { get; }

        /// <summary>
        ///     The program name used by the connection
        /// </summary>
        string ProgramName { get; set; }

        /// <summary>
        ///     The program version used by the connection
        /// </summary>
        string ProgramVersion { get; set; }

        /// <summary>
        ///     Get whether the server connection is using Unicode
        /// </summary>
        bool UseUnicode { get; }

        /// <summary>
        ///     What API level does the server support?
        /// </summary>
        int ApiLevel { get; }

        /// <summary>
        ///     Does the server require a login before running commands?
        /// </summary>
        bool RequiresLogin { get; }

        /// <summary>
        ///     Error information from the last connection attempt
        /// </summary>
        P4ClientError ConnectionError { get; }

        /// <summary>
        ///     Progress handler for reporting progress events during command execution
        /// </summary>
        ProgressHandler Progress { get; set; }

        /// <summary>
        ///     Handler for resolve callbacks
        /// </summary>
        P4Server.ResolveHandlerDelegate ResolveHandler { get; set; }

        /// <summary>
        ///     Handler for resolve (alternative) callbacks
        /// </summary>
        P4Server.ResolveAHandlerDelegate ResolveAHandler { get; set; }

        /// <summary>
        ///     Handler for prompt callbacks
        /// </summary>
        P4Server.PromptHandlerDelegate PromptHandler { get; set; }

        /// <summary>
        ///     Keep-alive handler for monitoring long-running commands
        /// </summary>
        IKeepAlive KeepAlive { get; set; }

        #endregion

        #region Events

        /// <summary>
        ///     Event broadcast when tagged output is received
        /// </summary>
        event P4Server.TaggedOutputDelegate TaggedOutputReceived;

        /// <summary>
        ///     Event broadcast when errors are received
        /// </summary>
        event P4Server.ErrorDelegate ErrorReceived;

        /// <summary>
        ///     Event broadcast when info results are received
        /// </summary>
        event P4Server.InfoResultsDelegate InfoResultsReceived;

        /// <summary>
        ///     Event broadcast when text results are received
        /// </summary>
        event P4Server.TextResultsDelegate TextResultsReceived;

        /// <summary>
        ///     Event broadcast when binary data is received
        /// </summary>
        event P4Server.BinaryResultsDelegate BinaryResultsReceived;

        /// <summary>
        ///     Event broadcast for command execution echo
        /// </summary>
        event P4Server.CommandEchoDelegate CommandEcho;

        /// <summary>
        ///     Event broadcast for command execution with response time echo
        /// </summary>
        event P4Server.ResponseTimeEchoDelegate ResponseTimeEcho;

        #endregion

        #region Methods

        /// <summary>
        ///     Login to the server with the specified password
        /// </summary>
        bool Login(string password, StringList options);

        /// <summary>
        ///     Logout from the server
        /// </summary>
        bool Logout(StringList options, string user = null);

        /// <summary>
        ///     Check if currently connected to the server
        /// </summary>
        bool IsConnected();

        /// <summary>
        ///     Check if the server has a URL for Perforce service
        /// </summary>
        bool UrlHandled();

        /// <summary>
        ///     Reconnect to the server
        /// </summary>
        void Reconnect();

        /// <summary>
        ///     Close the connection to the server
        /// </summary>
        void Close();

        /// <summary>
        ///     Disconnect from the server
        /// </summary>
        void Disconnect();

        /// <summary>
        ///     Get a unique command ID for command execution
        /// </summary>
        uint getCmdId();

        /// <summary>
        ///     Run a P4 command on the server
        /// </summary>
        bool RunCommand(string cmd,
            uint cmdId,
            bool tagged,
            string[] args,
            int argc);

        /// <summary>
        ///     Cancel an in-flight command
        /// </summary>
        void CancelCommand(uint cmdId);

        /// <summary>
        ///     Get the number of parallel operations currently running
        /// </summary>
        int GetParallelOperationCount();

        /// <summary>
        ///     Get tagged output for a command
        /// </summary>
        TaggedObjectList GetTaggedOutput(uint cmdId);

        /// <summary>
        ///     Get error results for a command
        /// </summary>
        P4ClientErrorList GetErrorResults(uint cmdId);

        /// <summary>
        ///     Get info results for a command
        /// </summary>
        P4ClientInfoMessageList GetInfoResults(uint cmdId);

        /// <summary>
        ///     Get text results for a command
        /// </summary>
        string GetTextResults(uint cmdId);

        /// <summary>
        ///     Get binary results for a command
        /// </summary>
        byte[] GetBinaryResults(uint cmdId);

        /// <summary>
        ///     Broadcast a command execution on the CommandEcho event
        /// </summary>
        void EchoCommand(string cmd, StringList args);

        /// <summary>
        ///     Broadcast a command execution with response time on the ResponseTimeEcho event
        /// </summary>
        void EchoResponseTime(string cmd, StringList args, TimeSpan responseTime);

        /// <summary>
        ///     Broadcast a string on the CommandEcho event
        /// </summary>
        void EchoCommand(string str);

        /// <summary>
        ///     Set data set for use by a command
        /// </summary>
        void SetDataSet(uint cmdId, string value);

        /// <summary>
        ///     Get data set for a command
        /// </summary>
        string GetDataSet(uint cmdId);

        /// <summary>
        ///     Set connection data (port, user, password, client)
        /// </summary>
        void SetConnectionData(string port, string user, string password, string client);

        /// <summary>
        ///     Set the progress handler for reporting progress events
        /// </summary>
        void SetProgressHandler(ProgressHandler handler);

        /// <summary>
        ///     Reset the progress handler (disable progress callbacks)
        /// </summary>
        void ResetProgressHandler();

        /// <summary>
        ///     Set protocol parameter for connection
        /// </summary>
        void SetProtocol(string key, string val);

        /// <summary>
        ///     Set the thread owner for this server instance
        /// </summary>
        void SetThreadOwner(int threadId);

        /// <summary>
        ///     Get the ticket file location
        /// </summary>
        string GetTicketFile();

        /// <summary>
        ///     Set the ticket file location
        /// </summary>
        void SetTicketFile(string ticketFile);

        /// <summary>
        ///     Pause the run command timer for the specified command
        /// </summary>
        void PauseRunCmdTimer(uint cmdId);

        /// <summary>
        ///     Continue the run command timer for the specified command
        /// </summary>
        void ContinueRunCmdTimer(uint cmdId);

        /// <summary>
        ///     Check if a command is currently paused
        /// </summary>
        bool IsCommandPaused(uint cmdId);

        #endregion
    }
}
