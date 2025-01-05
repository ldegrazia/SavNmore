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
using savnmore;
using savnmore.Models;

namespace savnmore.Services
{
    /// <summary>
    /// Helper to display plural counts
    /// </summary>
    public static class StringHelper
    {
        /// <summary>
        /// if the count is not one, the string is returned with 's.'
        /// </summary>
        /// <param name="message"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static string Pluralise(string message, int count)
        {
            if (string.IsNullOrEmpty(message))
            {
                return message;
            }
            if (count == 1)
            {
                return message + ".";
            }
            return message + "s.";
        }
    }
    /// <summary>
    /// Class represents all the various site settings from teh webconfig
    /// </summary>
    public class SiteSettingsService
    {
        public SiteSettings Get()
        {
            //find the site settings from the web config and return
            SiteSettings ss = new SiteSettings();
            ss.ApplicationName = ConfigurationManager.AppSettings[Constants.ApplicationNameKey];
            ss.LogFile = ConfigurationManager.AppSettings[Constants.LogFileKey];
            ss.CreateSampleRolesAndUsers = Convert.ToBoolean(ConfigurationManager.AppSettings[Constants.CreateSampleRolesAndUsersKey]);
            ss.DefaultRolePhoto = ConfigurationManager.AppSettings[Constants.DefaultRolePhotoKey];
            ss.DefaultUserPhoto = ConfigurationManager.AppSettings[Constants.DefaultUserPhotoKey];
            ss.DomainEmailSuffix = ConfigurationManager.AppSettings[Constants.DomainEmailSuffixKey];
            ss.DomainUrl = ConfigurationManager.AppSettings[Constants.DomainUrlKey];
            ss.DomainUrlMarker = ConfigurationManager.AppSettings[Constants.DomainUrlMarkerKey];
            ss.DropRecreateDatabase = Convert.ToBoolean(ConfigurationManager.AppSettings[Constants.DropRecreateDatabaseKey]);
            ss.EmailResetLinkMarker = ConfigurationManager.AppSettings[Constants.EmailResetLinkMarkerKey];
            ss.NumberOfItemsPerPage = Convert.ToInt32(ConfigurationManager.AppSettings[Constants.NumberOfItemsPerPageKey]);
            ss.NumberOfSampleUsers = Convert.ToInt32(ConfigurationManager.AppSettings[Constants.NumberOfSampleUsersKey]);
            ss.PasswordResetExpireInDays = Convert.ToInt32(ConfigurationManager.AppSettings[Constants.PasswordResetExpireInDaysKey]);
            ss.ResetPasswordEmailBody = ConfigurationManager.AppSettings[Constants.ResetPasswordEmailBodyKey];
            ss.ResetPasswordLink = ConfigurationManager.AppSettings[Constants.ResetPasswordLinkKey];
            ss.ResetPasswordSender = ConfigurationManager.AppSettings[Constants.ResetPasswordSenderKey];
            ss.ResetPasswordSubject = ConfigurationManager.AppSettings[Constants.ResetPasswordSubjectKey];
            ss.RoleImagesRootPath = ConfigurationManager.AppSettings[Constants.RolesImagesRootPathKey];
            ss.SendResetPasswordEmail =
                Convert.ToBoolean(ConfigurationManager.AppSettings[Constants.SendResetPasswordEmailKey]);
            ss.SendWelcomeEmail =
                Convert.ToBoolean(ConfigurationManager.AppSettings[Constants.SendWelcomeEmailKey]);
            ss.SmtpServer = ConfigurationManager.AppSettings[Constants.SmtpServerKey];
            ss.SmtpServerPort = Convert.ToInt32(ConfigurationManager.AppSettings[Constants.SmtpServerPortKey]);
            ss.ThrowErrorOnDeletingPopulatedRoles =
                Convert.ToBoolean(ConfigurationManager.AppSettings[Constants.ThrowErrorOnDeletingPopulatedRolesKey]);
            ss.UsePasswordStrength = Convert.ToBoolean(ConfigurationManager.AppSettings[Constants.UsePasswordStrengthKey]);
            ss.UserEmailMarker = ConfigurationManager.AppSettings[Constants.UserEmailMarkerKey];
            ss.UserImagesRootPath = ConfigurationManager.AppSettings[Constants.UserImagesRootPathKey];
            ss.UserNameMarker = ConfigurationManager.AppSettings[Constants.UserNameMarkerKey];
            ss.WelcomeEmailBody = ConfigurationManager.AppSettings[Constants.WelcomeEmailBodyKey];
            ss.WelcomeEmailSender = ConfigurationManager.AppSettings[Constants.WelcomeEmailSenderKey];
            ss.WelcomeEmailSubject = ConfigurationManager.AppSettings[Constants.WelcomeEmailSubjectKey];


            ss.DatabaseProvider = ConfigurationManager.ConnectionStrings[Constants.ConnectionStringKey].ProviderName;
            ss.ConnectionString = ConfigurationManager.ConnectionStrings[Constants.ConnectionStringKey].ConnectionString;

            var db = new savnmoreEntities();
            try
            {
                if (db.Database.Connection.State == System.Data.ConnectionState.Closed)
                {
                    db.Database.Connection.Open();
                }
                ss.DatabaseVersion = db.Database.Connection.ServerVersion;
            }
            catch
            {
                ss.DatabaseVersion = ErrorConstants.Unavailable;
            }
            finally
            {
                db.Database.Connection.Close();
            }


            return ss;



        }
    }
}