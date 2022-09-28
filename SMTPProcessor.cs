using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Configuration;

namespace MailServer {
  /// <summary>
  /// SMTPProcessor implements the SMTP communication. It will be called by Server
  /// class and does not care how a client has connected.
  /// </summary>
  public class SMTPProcessor {
		
    // To be replaced with your folder! Todo: put in config file
    private const string _mailFolder = @"C:\temp\";
    // Following used to add some additional text to the subject header if needed
    private const string sPassWord = "";    
    private Socket _socket; // Socket will be assigned in constructor
    private bool _quitRequested = false; // Flag indicates if client has called QUIT
    private bool _isDataPartStarted = false; // Flag indicates if client has started DATA part
    private Guid _messageID = Guid.NewGuid(); // Unique message identifier    
    private StreamWriter _outputStream; // Output stream for writing mail to harddisk

    /// <summary>Constructor takes an established socket connection.</summary>
    /// <param name="socket">Established connection to client</param>
    public SMTPProcessor(Socket socket) {
      _socket = socket;			
    }

    /// <summary>Starts communication with client.</summary>
    public void Process() {
			
      string clientMessage = string.Empty;
      string serverMessage = string.Empty;
      SendMail sm = new SendMail();  
      // Create stream classes for reading and writing data.
      NetworkStream networkStream = new NetworkStream(_socket);
      StreamReader streamReader = new StreamReader(networkStream);
      StreamWriter streamWriter = new StreamWriter(networkStream);

      // Create output file to store the message. Very simple implementation ....
      _outputStream = File.CreateText(_mailFolder + _messageID.ToString() + ".EML");
      sm.PathFile = _mailFolder + _messageID.ToString() + ".EML";
      sm.MailServer = ConfigurationManager.AppSettings["MailServer"].ToString();
      sm.SenderDetail = ConfigurationManager.AppSettings["SenderDetail"].ToString();
      sm.ReceiverDetail = ConfigurationManager.AppSettings["ReceiverDetail"].ToString();
      sm.MailSubject = ConfigurationManager.AppSettings["MailSubject"].ToString();


      try {			
        // AutoFlush will write data to client immediately
        streamWriter.AutoFlush = true;

        // Send welcome message first
        string welcome = "220 " + System.Environment.MachineName + " SMS mail proxy Service ready at " + DateTime.Now.ToString();
        streamWriter.WriteLine(welcome);

        // Start loop and handle commands
        while (!_quitRequested) {
          clientMessage = streamReader.ReadLine();					
          serverMessage = EvaluateCommand(clientMessage);								
          if (serverMessage != "")
            streamWriter.WriteLine(serverMessage);							
        }
      }
      catch (Exception ex) {
        Console.WriteLine("An Exception occured: " + ex.ToString());
      }
      finally {
        streamWriter.Close();
        streamReader.Close();
        networkStream.Close();
        _socket.Close();
        _outputStream.Close();
        sm.GetDetails();
        sm.SendMailNow();
      }
    }

    /// <summary>Evaluates incoming request and returns server's response.</summary>
    /// <param name="clientMessage">Client's request. ToDo: GetDetails should be divided into separate methods for each command once more commands will be implemented.</param>
    /// <returns>Message to send back to client</returns>
    private string EvaluateCommand(string clientMessage) {
      string serverMessage = string.Empty;

      // *** DATA ***
      if (clientMessage.ToUpper() == "DATA") {
        _isDataPartStarted = true;
        serverMessage = "354 Please start mail input; end with <CRLF>.<CRLF>";
      }	
      else if (_isDataPartStarted  && clientMessage == ".") {
        _isDataPartStarted  = false;								
        serverMessage = "250 Mail queued for delivery. MessageId: " + 
          _messageID.ToString();
      }
      else if (_isDataPartStarted ) {
        
          if (clientMessage.ToUpper().StartsWith("SUBJECT:"))
            {
            _outputStream.WriteLine(clientMessage+ sPassWord);

            }
          else        
            _outputStream.WriteLine(clientMessage);
      }			
		
        // *** HELO ***
      else if (clientMessage.ToUpper().StartsWith("HELO ") || clientMessage.ToUpper() == "HELO") {				
        serverMessage = "250 " + System.Environment.MachineName + " Hello [" + Dns.Resolve(System.Environment.MachineName).AddressList[0].ToString() + "]";				
        // saidHello = true;
      }
			
        // *** QUIT ***				
      else if (clientMessage.ToUpper() == "QUIT") {
        serverMessage = "221 Closing connection. Good bye!";
        _quitRequested = true;
      }

        // *** MAIL FROM ***				
      else if (clientMessage.ToUpper().StartsWith("MAIL FROM:")) {
        string mailFrom = clientMessage.Substring(clientMessage.IndexOf(":") + 1);
        _outputStream.WriteLine(clientMessage);
        serverMessage  = "250 2.1.0 " + mailFrom + "....Sender OK";				
      }
      // Include text in header  
      
      // *** RCPT TO ***				
      else if (clientMessage.ToUpper().StartsWith("RCPT TO:"))
      {
          string mailTo = clientMessage.Substring(clientMessage.IndexOf(":") + 1);
          _outputStream.WriteLine(clientMessage);
          serverMessage = "250 2.1.5 " + mailTo;

      }
      else
          serverMessage = "502 Command not implemented";

      return serverMessage;
    }
  }
}