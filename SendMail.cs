using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net.Mail;
namespace MailServer
{
    public class SendMail
    {
        private string _pathFile;
        private string MessageLine = string.Empty;
        private string MailFrom = string.Empty;
        private MailMessage mail = new MailMessage();
        private string MailBody = string.Empty;

        public  string SenderDetail=string.Empty;
        public  string ReceiverDetail = string.Empty;
        public  string MailSubject = string.Empty;
        /// <summary>
        /// This should be the DNS name of the SMTP server or the IP address
        /// </summary>
        public string MailServer = string.Empty;
        /// <summary>
        /// The File name and also the path needs to be sent
        /// </summary>
        public string PathFile
        {
            get
            {

                throw new System.NotImplementedException();


            }
            set
            {
                _pathFile = value;
            }
        }

        public void GetDetails()
        {

            StreamReader txtReader = new StreamReader(_pathFile);
            try
            {

                

                while (!txtReader.EndOfStream)
                {
                    MessageLine = txtReader.ReadLine();


                    if (MessageLine.ToUpper().StartsWith("MAIL FROM:"))
                        MailFrom = MessageLine.Substring(11).Replace(">", "");


                    if (MessageLine.ToUpper().StartsWith("RCPT TO:"))

                         if (!mail.To.ToString().Contains(MessageLine.Substring(9).Replace(">", "")))
                        mail.To.Add(MessageLine.Substring(9).Replace(">", ""));

                    if (MessageLine.ToUpper().StartsWith("X-L"))
                    {
                        // this is the space
                        MessageLine = txtReader.ReadLine();
                        // This is the data
                        MessageLine = txtReader.ReadLine();

                        this.MailBody = MessageLine;
                        // Got all I need we are done here exit loop

                        txtReader.ReadToEnd();
                    }
                }
               
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);


            }
            finally
            {
                txtReader.Close();

            }

         

        }

        public void SendMailNow()
        {

            try
            {
               
                mail.From = new MailAddress(this.MailFrom, this.SenderDetail);
                mail.Subject = this.MailSubject;
                mail.Body = this.MailBody;
                mail.IsBodyHtml = false;
                SmtpClient SmtpMail = new SmtpClient(this.MailServer);
                SmtpMail.Send(mail);
                Console.WriteLine("Mail to:"+ this.mail.To.ToString() +" delivered to SMTP host:" + this.MailServer);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + " please check file:" + _pathFile);
            }
        }


    }
}
    

