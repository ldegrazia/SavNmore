//================================================================================
// Reboot, Inc. Entity Framework Membership for .NET
//================================================================================
// NO unauthorized distribution of any copy of this code (including any related 
// documentation) is allowed.
// 
// The Reboot. Inc. name, trademarks and/or logo(s) of Reboot, Inc. shall not be used to 
// name (even as a part of another name), endorse and/or promote products derived 
// from this code without prior written permission from Reboot, Inc.
// 
// The use, copy, and/or distribution of this code is subject to the terms of the 
// Reboot, Inc. License Agreement. This code shall not be used, copied, 
// and/or distributed under any other license agreement.
// 
//                                         
// THIS CODE IS PROVIDED BY REBOOT, INC. 
// (“Reboot”) “AS IS” WITHOUT ANY WARRANTY OF ANY KIND. REBBOT, INC. HEREBY DISCLAIMS 
// ALL EXPRESS, IMPLIED, OR STATUTORY CONDITIONS, REPRESENTATIONS AND WARRANTIES 
// WITH RESPECT TO THIS CODE (OR ANY PART THEREOF), INCLUDING, BUT NOT LIMITED TO, 
// IMPLIED WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE OR 
// NON-INFRINGEMENT. REBOOT, INC. AND ITS SUPPLIERS SHALL NOT BE LIABLE FOR ANY DAMAGE 
// SUFFERED AS A RESULT OF USING THIS CODE. IN NO EVENT SHALL REBOOT, INC AND ITS 
// SUPPLIERS BE LIABLE FOR ANY DIRECT, INDIRECT, CONSEQUENTIAL, ECONOMIC, 
// INCIDENTAL, OR SPECIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, ANY LOST 
// REVENUES OR PROFITS).
// 
//                                         
// Copyright © 2012 Reboot, Inc. All rights reserved.
//================================================================================
using System;
using System.Configuration;
using System.Net.Mail;
 
using savnmore.Models;
 

namespace savnmore.Services
{
    public partial class EmailService
    {
        public const string ContactFormEmailSender = "info@savnmore.com";

        public const bool SendContactEmail = false;
        public static void SendContactFormEmail(ContactForm form)
        {
            var email = new MailMessage { From = new MailAddress(form.Email) };
            email.To.Add(ContactFormEmailSender);
            email.Subject = "Message from visitor";
            email.IsBodyHtml = true;
            email.Body = form.Subject + "<br/>" + form.Message;

            var smtpClient = new SmtpClient(ConfigurationManager.AppSettings[Constants.SmtpServerKey]);

            Logger.WriteLine(MessageType.Information, email.Subject + Constants.LogginSentTo + ContactFormEmailSender);
             

            if (SendContactEmail)
            {
                smtpClient.Send(email);
            }
        }
        /// <summary>
        /// Sends the welcome email using the settings in the webconfig
        /// </summary>
        /// <param name="user"></param>
        public static void SendWelcomeEmail(User user)
        {
            //replace red here with constants
            var email = new MailMessage { From = new MailAddress(ConfigurationManager.AppSettings[Constants.WelcomeEmailSenderKey]) };

            email.To.Add(new MailAddress(user.Email));
            string subject = ConfigurationManager.AppSettings[Constants.WelcomeEmailSubjectKey];
            email.Subject = subject.Replace(ConfigurationManager.AppSettings[Constants.DomainUrlMarkerKey], ConfigurationManager.AppSettings[Constants.DomainUrlKey]);
            email.IsBodyHtml = true;

            email.Body = FormulateWelcomeMessage(user.UserName, user.Email);
            var smtpClient = new SmtpClient(ConfigurationManager.AppSettings[Constants.SmtpServerKey], Convert.ToInt32(ConfigurationManager.AppSettings[Constants.SmtpServerPortKey]));
            Logger.WriteLine(MessageType.Information, email.Subject + Constants.LogginSentTo + user.Email);
            if (Convert.ToBoolean(ConfigurationManager.AppSettings[Constants.SendWelcomeEmailKey]))
            {
                try
                {
                    smtpClient.Send(email);
                    Logger.WriteLine(MessageType.Information, user.Email + Constants.EmailWelcomeSent);
                }
                catch (Exception ex)
                {
                    Logger.WriteLine(MessageType.Error, user.Email + ErrorConstants.CouldNotSendWelcomeEmail + ex.Message);
                }
            }
        }

        /// <summary>
        /// Sends the password reset link to the user using the setings in the webconfig
        /// </summary>
        /// <param name="user"></param>
        /// <param name="resetId"></param>
        /// <param name="hash"> </param>
        public static void SendResetEmail(User user, int resetId, string hash)
        {

            var email = new MailMessage { From = new MailAddress(ConfigurationManager.AppSettings[Constants.ResetPasswordSenderKey]) };

            email.To.Add(new MailAddress(user.Email));

            email.Subject = ConfigurationManager.AppSettings[Constants.ResetPasswordSubjectKey];
            email.IsBodyHtml = true;
            string link = ConfigurationManager.AppSettings[Constants.ResetPasswordLinkKey] + user.UserName + Constants.ResetQueryParam + resetId
                + Constants.HashResetParam + hash;
            email.Body = FormulateMessage(user.UserName, link);
            var smtpClient = new SmtpClient(ConfigurationManager.AppSettings[Constants.SmtpServerKey], Convert.ToInt32(ConfigurationManager.AppSettings[Constants.SmtpServerPortKey]));

            Logger.WriteLine(MessageType.Information, link + Constants.LogginSentTo + user.Email);
            if (Convert.ToBoolean(ConfigurationManager.AppSettings[Constants.SendResetPasswordEmailKey]))
            {
                try
                {
                    smtpClient.Send(email);
                    Logger.WriteLine(MessageType.Information, user.Email + Constants.EmailResetSent + link);
                }
                catch (Exception ex)
                {
                    Logger.WriteLine(MessageType.Error, user.Email + ErrorConstants.ErrorSendingPasswordResetLink + ex.Message);
                }
            }
        }
        /// <summary>
        /// Creates the Reset Password email message, replacing with the username and target link
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="targetLink"></param>
        /// <returns></returns>
        public static string FormulateMessage(string userName, string targetLink)
        {
            string message = ConfigurationManager.AppSettings[Constants.ResetPasswordEmailBodyKey];
            message = message.Replace(ConfigurationManager.AppSettings[Constants.UserNameMarkerKey], userName);
            message = message.Replace(ConfigurationManager.AppSettings[Constants.EmailResetLinkMarkerKey], targetLink);
            message = message.Replace(ConfigurationManager.AppSettings[Constants.DomainUrlMarkerKey], ConfigurationManager.AppSettings[Constants.DomainUrlKey]);
            return message;
        }
        /// <summary>
        /// Creates a welcome email message for the new user
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        public static string FormulateWelcomeMessage(string userName, string email)
        {
            string message = ConfigurationManager.AppSettings[Constants.WelcomeEmailBodyKey];
            message = message.Replace(ConfigurationManager.AppSettings[Constants.UserNameMarkerKey], userName);
            message = message.Replace(ConfigurationManager.AppSettings[Constants.UserEmailMarkerKey], email);
            message = message.Replace(ConfigurationManager.AppSettings[Constants.DomainUrlMarkerKey], ConfigurationManager.AppSettings[Constants.DomainUrlKey]);
            return message;
        }
        public static void EmailShoppingList(string to)
        {
            var email = new MailMessage { From = new MailAddress(ConfigurationManager.AppSettings[Constants.WelcomeEmailSenderKey]) };
            email.To.Add(to);           
            email.Subject = "My savnmore.com Shopping List for " + DateTime.Now.ToShortDateString();
            email.IsBodyHtml = true;
            ShoppingListService sl = new ShoppingListService();
            var l = sl.GetList();
            string froml = "Hello,<br/>" + to + " has sent you a shopping list.";
            string footer = "<br/>Check out these savings and more at http://www.savnmore.com.<br/>Thank you for using savnmore.com";
            email.Body = froml + l.PrintList() + footer ; 
            var smtpClient = new SmtpClient(ConfigurationManager.AppSettings[Constants.SmtpServerKey]);
            Logger.WriteLine(MessageType.Information, email.Subject + Constants.LogginSentTo + to);             
            if (SendContactEmail)
            {
                smtpClient.Send(email);
            }
        }
    }
}
