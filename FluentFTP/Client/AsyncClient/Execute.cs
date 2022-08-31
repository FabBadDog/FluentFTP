﻿using System;
using System.IO;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Linq;
using System.Net;
using FluentFTP.Helpers;
using System.Threading;
using System.Threading.Tasks;

namespace FluentFTP {
	public partial class AsyncFtpClient {

#if ASYNC
		/// <summary>
		/// Performs an asynchronous execution of the specified command
		/// </summary>
		/// <param name="command">The command to execute</param>
		/// <param name="token">The token that can be used to cancel the entire process</param>
		/// <returns>The servers reply to the command</returns>
		public async Task<FtpReply> Execute(string command, CancellationToken token) {
			FtpReply reply;

			if (Config.StaleDataCheck && Status.AllowCheckStaleData) {
#if NETSTANDARD
				await ReadStaleData(true, false, true, token);
#else
				ReadStaleData(true, false, true);
#endif
			}

			if (!IsConnected) {
				if (command == "QUIT") {
					LogWithPrefix(FtpTraceLevel.Info, "Not sending QUIT because the connection has already been closed.");
					return new FtpReply() {
						Code = "200",
						Message = "Connection already closed."
					};
				}

				await Connect(token);
			}

			// hide sensitive data from logs
			var commandTxt = command;
			if (command.StartsWith("USER", StringComparison.Ordinal)) {
				commandTxt = "USER ***";
			}

			if (command.StartsWith("PASS", StringComparison.Ordinal)) {
				commandTxt = "PASS ***";
			}

			// A CWD will invalidate the cached value.
			if (command.StartsWith("CWD ", StringComparison.Ordinal)) {
				Status.LastWorkingDir = null;
			}

			Log(FtpTraceLevel.Info, "Command:  " + commandTxt);

			// send command to FTP server
			await m_stream.WriteLineAsync(m_textEncoding, command, token);
			LastCommandTimestamp = DateTime.UtcNow;
			reply = await GetReply(token);

			return reply;
		}
#endif
	}
}