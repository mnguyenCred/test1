using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Net.Mail;
using Navy.Utilities;

namespace AppTestProject
{
    [TestClass]
    public class UtililtyTesting
    {
        [TestMethod]
        public void TestEmail()
        {
            MailMessage email = new MailMessage();
            string fromEmail="";
            var toEmail = "mnguyen@credentialengine.org";
            string subject="test one";
            string message = "a test email via smtp";

            
            EmailManager.SendEmail( toEmail, subject, message );


        }
    }
}
