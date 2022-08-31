﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FluentFTP.Streams;
using FluentFTP.Helpers;
using FluentFTP.Exceptions;
using FluentFTP.Client.Modules;
using System.Threading;
using System.Threading.Tasks;

namespace FluentFTP {
	public partial class FtpClient {

		/// <summary>
		/// Downloads the specified file into the specified stream.
		/// High-level API that takes care of various edge cases internally.
		/// Supports very large files since it downloads data in chunks.
		/// </summary>
		/// <param name="outStream">The stream that the file will be written to. Provide a new MemoryStream if you only want to read the file into memory.</param>
		/// <param name="remotePath">The full or relative path to the file on the server</param>
		/// <param name="restartPosition">The size of the existing file in bytes, or 0 if unknown. The download restarts from this byte index.</param>
		/// <param name="progress">Provide a callback to track download progress.</param>
		/// <returns>If true then the file was downloaded, false otherwise.</returns>
		public bool DownloadStream(Stream outStream, string remotePath, long restartPosition = 0, Action<FtpProgress> progress = null) {
			// verify args
			if (outStream == null) {
				throw new ArgumentException("Required parameter is null or blank.", "outStream");
			}

			if (remotePath.IsBlank()) {
				throw new ArgumentException("Required parameter is null or blank.", "remotePath");
			}

			remotePath = remotePath.GetFtpPath();

			LogFunction(nameof(DownloadStream), new object[] { remotePath });

			// download the file from the server
			return DownloadFileInternal(null, remotePath, outStream, restartPosition, progress, new FtpProgress(1, 0), 0, false);
		}

	}
}
