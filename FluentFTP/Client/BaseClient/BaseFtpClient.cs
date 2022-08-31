﻿using FluentFTP.Client.Modules;
using FluentFTP.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentFTP.Client.BaseClient {
	public partial class BaseFtpClient : IDisposable, IInternalFtpClient {

		#region Constructor

		public BaseFtpClient(FtpConfig config) {
			CurrentListParser = new FtpListParser(this);
			Config = config != null ? config : new FtpConfig();
		}

		#endregion

		#region Clone

		/// <summary>
		/// Clones the FTP client control connection. Used for opening multiple data streams.
		/// You will need to manually connect after cloning.
		/// </summary>
		/// <returns>A new FTP client connection with the same property settings as this one.</returns>
		public BaseFtpClient Clone() {
			var newClone = Create();

			newClone.m_isClone = true;

			CloneClient(this, newClone);

			newClone.CurrentDataType = CurrentDataType;
			newClone.ForceSetDataType = true;

			return newClone;
		}

		private static void CloneClient(BaseFtpClient read, BaseFtpClient write) {

			// configure new connection as clone of self
			write.Host = read.Host;
			write.Port = read.Port;
			write.Credentials = read.Credentials;
			write.ServerHandler = read.ServerHandler;
			write.Encoding = read.Encoding;

			// copy config
			write.Config = read.Config.Clone();

			// copy capabilities
			try {
				write.SetFeatures(read.Capabilities);
			}
			catch (Exception ex) { }

			// always accept certificate no matter what because if code execution ever
			// gets here it means the certificate on the control connection object being
			// cloned was already accepted.
			write.ValidateCertificate += new FtpSslValidation(
				delegate (BaseFtpClient obj, FtpSslValidationEventArgs e) { e.Accept = true; });

		}


		#endregion

		#region Destructor

		/// <summary>
		/// Disposes and disconnects this FTP client if it was auto-created for an internal operation.
		/// </summary>
		public void AutoDispose() {
			if (Status.AutoDispose) {
				Dispose();
			}
		}
		/// <summary>
		/// Check if the host parameter is valid
		/// </summary>
		/// <param name="host"></param>
		protected string ValidateHost(Uri host) {
			if (host == null) {
				throw new ArgumentNullException(nameof(host), "Host is required");
			}
#if !NETSTANDARD
			if (host.Scheme != Uri.UriSchemeFtp) {
				throw new ArgumentException("Host is not a valid FTP path");
			}
#endif
			return host.Host;
		}

		/// <summary>
		/// Creates a new instance of this class. Useful in FTP proxy classes.
		/// </summary>
		protected virtual BaseFtpClient Create() {
			return new BaseFtpClient(null);
		}

		/// <summary>
		/// Disconnects from the server, releases resources held by this
		/// object.
		/// </summary>
		public virtual void Dispose() {
			lock (m_lock) {
				if (IsDisposed) {
					return;
				}

				// Fix: Hard catch and suppress all exceptions during disposing as there are constant issues with this method
				try {
					LogFunction(nameof(Dispose));
					LogWithPrefix(FtpTraceLevel.Verbose, "Disposing FtpClient object...");
				}
				catch (Exception ex) {
				}

				try {
					if (IsConnected) {
						((IInternalFtpClient)this).DisconnectInternal();
					}
				}
				catch (Exception ex) {
				}

				if (m_stream != null) {
					try {
						m_stream.Dispose();
					}
					catch (Exception ex) {
					}

					m_stream = null;
				}

				try {
					m_credentials = null;
					m_textEncoding = null;
					m_host = null;
				}
				catch (Exception ex) {
				}

				IsDisposed = true;
				GC.SuppressFinalize(this);
			}
		}

		void IInternalFtpClient.DisconnectInternal() {
		}

		void IInternalFtpClient.ConnectInternal() {
		}

		/// <summary>
		/// Finalizer
		/// </summary>
		~BaseFtpClient() {
			Dispose();
		}

		#endregion



	}
}
