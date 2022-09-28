using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Net.Sockets;

namespace MailServer {

  /// <summary>
  /// Server class starts a TcpListener and creates a new thread for each incoming
  /// connection. Communication between client and server will be done in SMTPProcessor.
  /// This class has no knowledge about SMTP, it's a generic socket server and can
  /// be used for other protocols as well.
  /// </summary>
	public class Server {		
		private readonly int _port;
		private readonly IPAddress _serverAddress;
		private TcpListener _tcpListener;
		
    /// <summary>Constructor for initializing server.</summary>
    /// <param name="serverAddress">IP address to listen on</param>
    /// <param name="port">Port number to listen on</param>
		public Server(IPAddress serverAddress, int port) {
			_port = port;
			_serverAddress = serverAddress;
		}

		/// <summary>
		/// Starts server listening for incoming requests. This will be done
		/// in a separate thread in order not to block the request.
		/// </summary>
		public void Start() {			
			try {
        // Create and start new TcpListener object
				_tcpListener = new TcpListener(_serverAddress, _port);				
				_tcpListener.Start();
				Console.WriteLine("Server Ready - Listening for new connections ...") ;

        // Call private StartListen method in new thread. Underlying application 
        // will not be blocked after calling this Start method.
				Thread thread = new Thread(new ThreadStart(StartListen));
				thread.Start() ;
			}
			catch(Exception e) {
				Console.WriteLine("An Exception occured while listening :" + e.ToString());
			}
		}

		/// <summary>Signals server to stop processing.</summary>
		public void Stop() {
			_tcpListener.Stop();
		}      

    /// <summary>
    /// Wait for new client connection and start SMTPProcessor in a new thread
    /// for handling the SMTP communication. This method contains the main loop.    
    /// </summary>
		private void StartListen() {
			try {
        // Endless loop will only be stopped by calling Stop() method. Stop() calls
        // a TcpListener.Stop() which stops all active connections and throws an exception.
				while(true) {				
					Socket socket = _tcpListener.AcceptSocket();					
					socket.Blocking = true;	
					if(socket.Connected) {
						Console.WriteLine("Client connected: {0}", socket.RemoteEndPoint);
            // Create and start a new SMTPProcessor in a new thread. This enables
            // server to handle multiple connections.
						SMTPProcessor smtpProcessor = new SMTPProcessor(socket);
						Thread thread = new Thread(new ThreadStart(smtpProcessor.Process));					
						thread.Start();					
					}				
				}
			}
      // SocketException will be thrown by TcpListener.Stop(). This handling can be
      // replaced by using "flags" in order to find out if no active connection exists ...
			catch (SocketException ex) {				
				Console.WriteLine("SocketException: {0}", ex);				
			}			
		}		
	}
}