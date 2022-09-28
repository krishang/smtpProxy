using System;

namespace MailServer {
  /// <summary>A simple test application for email server classes.</summary>
  class App {
    /// <summary>Starts a new server and stops when user presses "enter".</summary>
    [STAThread]
    static void Main(string[] args) {
      // Sample uses port 8080 in order not to clash with existing SMTP service
      // on your machine. Default port for SMTP is 25.
      Server server = new Server(System.Net.IPAddress.Any, 25);
      server.Start();		
	
      Console.WriteLine("Press enter to stop server");
      string input = Console.ReadLine();
      server.Stop();
			
    }
  }
}