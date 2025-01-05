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

namespace savnmore
{
    /// <summary>
    /// Various constants used throughout the code.
    /// Change any text values you would like for your version
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// The session key
        /// </summary>
        public const string SessionAppNameKey = "_App_Name";
        //User defaults
        /// <summary>
        /// The Administrator user name 
        /// </summary>
        public const string Admin = "Admin";
        /// <summary>
        /// The administrator password default
        /// </summary>
        public const string AdminPassword = "AstrongPassw0rd";
        //Sample user details 
        public const string SampleUserName = "user";
        public const string SampleUserPassword = "Apassw0rd";
        // Role defaults
        public const string AdministratorsRole = "Administrators";
        public const string AdministratorsRoleDescription = "Administrators have full control";
        public const string UsersRole = "Users";
        public const string UsersRoleDescription = "Regular users of the site";
        public const string Level1Role = "Level 1";
        public const string Level1RoleDescription = "Level 1 users of the site";
        public const string Level2Role = "Level 2";
        public const string Level2RoleDescription = "Level 2 users of the site";
        public const string PremiumRole = "Premium Users";
        public const string PremiumRoleRoleDescription = "Premium users of the site";

        // forms authentication storing password routines. Consider encryption of machine keys
        public const string HashMethod = "Md5";

        //web config keys
        /// <summary>
        /// The database connection string key, also resides in the Project Entities class
        /// </summary>
        public const string ConnectionStringKey = "ApplicationServices";
        /// <summary>
        /// The default application name key in the web config
        /// </summary>
        public const string ApplicationNameKey = "ApplicationName";
        //key to lo file in wobconfig
        public const string LogFileKey = "LogFile";
        //EF initializations
        public const string DropRecreateDatabaseKey = "DropRecreateDatabase";
        public const string CreateSampleRolesAndUsersKey = "CreateSampleRolesAndUsers";
        public const string NumberOfSampleUsersKey = "NumberOfSampleUsers";

        public const string DomainEmailSuffixKey = "DomainEmailSuffix";
        public const string NewRoleDescription = "New Role";
        public const string ResetQueryParam = "&reset=";
        public const string HashResetParam = "&hash=";
        public const int MinimuPasswordLength = 8;

        public const string UsePasswordStrengthKey = "UsePasswordStrength";

        public const string NumberOfItemsPerPageKey = "NumberOfItemsPerPage";

        public const string DefaultUserPhotoKey = "DefaultUserPhoto";
        public const string UserImagesRootPathKey = "UserImagesRootPath";
        public const string DefaultRolePhotoKey = "DefaultRolePhoto";
        public const string RolesImagesRootPathKey = "RoleImagesRootPath";
        public const string RolePhotoServiceSessionKey = "RolePhotoService";
        public const string PasswordResetExpireInDaysKey = "PasswordResetExpireInDays";
        public const string WelcomeEmailSenderKey = "WelcomeEmailSender";
        public const string WelcomeEmailSubjectKey = "WelcomeEmailSubject";
        public const string WelcomeEmailBodyKey = "WelcomeEmailBody";
        public const string UserEmailMarkerKey = "UserEmailMarker";

        public const string DomainUrlKey = "DomainUrl";
        public const string DomainUrlMarkerKey = "DomainUrlMarker";
        public const string SmtpServerKey = "SmtpServer";
        public const string SmtpServerPortKey = "SmtpServerPort";
        public const string SendWelcomeEmailKey = "SendWelcomeEmail";
        public const string ResetPasswordSubjectKey = "ResetPasswordSubject";
        public const string ResetPasswordSenderKey = "ResetPasswordSender";
        public const string ResetPasswordLinkKey = "ResetPasswordLink";
        public const string SendResetPasswordEmailKey = "SendResetPasswordEmail";
        public const string ResetPasswordEmailBodyKey = "ResetPasswordEmailBody";
        public const string UserNameMarkerKey = "UserNameMarker";
        public const string EmailResetLinkMarkerKey = "EmailResetLinkMarker";

        public const string ThrowErrorOnDeletingPopulatedRolesKey = "ThrowErrorOnDeletingPopulatedRoles";


        public const string Photo = "photo";
        public const string EmailRegex =
            "^([a-zA-Z0-9_\\-\\.]+)@[a-z0-9-]+(\\.[a-z0-9-]+)*(\\.[a-zA-Z]{2,3})$";
        public const string UrlRegex = @"^(http|https|ftp)\://[a-zA-Z0-9\-\.]+\.[a-zA-Z]{2,3}(:[a-zA-Z0-9]*)?/?([a-zA-Z0-9\-\._\?\,\'/\\\+&amp;%\$#\=~])*[^\.\,\)\(\s]$";
        public const string ValidEntries = @"^[a-zA-Z0-9_@.-]*$";
        public const string AtSymbol = "@";
        public const string ChangesSaved = "Your changes have been saved.";

        public const string ApplicationVersion = "1.0.1.1";
        public const string ApplicationAuthor = "Reboot Inc.";
        public const string ApplicationMoreInfo = "http://www.RebootMyCode.com";
        public const string ApplicationDisclaimer = "© Copyright 2012, All Rights Reserved.";

        //logging strings
        public const string Adding = "Adding ";
        public const string To = " to ";
        public const string LoggedInAs = "You are logged in as ";
        public const string AddedAllUsers = "Added all users.";
        public const string RemovedAllMembers = "Removed all members.";

        public const string SqlServerCe = "SqlServerCe.4.0";
        public const string EmailResetSent = " Email reset link sent.";
        public const string EmailWelcomeSent = " Welcome email sent.";
        public const string LogginSentTo = " sent to ";
    }
    /// <summary>
    /// string ids in form submissions
    /// </summary>
    public static class FormKeys
    {
        public const string ChangeAppName = "changeappname";
        public const string NewAppName = "newappname";
        public const string DeleteInputs = "deleteInputs";
    }

    /// <summary>
    /// Class for errors that are displayed to users. There are other messages in utilities.js
    /// </summary>
    public static class ErrorConstants
    {
        public const string LogonFailed = " Logon Failed. ";
        public const string PasswordResetFailed = " Password Reset Failed. ";
        public const string NoSuchEmail = "No such email exists.";
        public const string CheckEntryAndTryAgain = "Check entries and try again.";
        public const string EmailExists = "Email exists.";
        public const string UserNameExists = "Username exists.";
        public const string RoleExists = "Role exists.";
        public const string CouldNotCreateUser = "Could not create user";
        public const string RoleIsPopulated = "Users are in the role, cannot delete.";
        public const string NotAValidEmail = "Not a valid email address.";
        public const string YouCannotDeleteyourself = "You cannot delete yourself.";
        public const string CouldNotResetPassword = "Could not reset password:";
        public const string ErrorSendingPasswordResetLink = " Error sending password rest link:";
        public const string CouldNotSendWelcomeEmail = "Could not send welcome email:";
        public const string CouldNotRegisterUser = " Could not register user:";
        public const string PasswordChangeFailure = "The current password is incorrect or the new password is invalid.";
        public const string LogonFailure = "The user name or password provided is incorrect.";
        public const string CouldNotMarkUserOffline = "Could not mark user offline.";
        public const string ChooseAtLeastOneRole = "Please choose at least one role for the user.";
        public const string CannotRenameAdministratorsRole = "You cannot rename the Administrator's role.";
        public const string CannotDeleteAdministratorsRole = "You cannot delete the Administrator's role.";
        public const string CannotDeleteAdministrator = "You cannot delete the Administrator.";
        public const string CannotChangeReadonlyUsers = "Readonly users cannot be renamed or deleted. They, themselves, can edit only certain properties.";
        public const string CannotChangeReadonlyRoles = "Readonly roles cannot be renamed or deleted. Readonly members cannot be removed.";
        public const string CannotRenameTheAdministrator = "You cannot rename the Administrator.";
        public const string CannotRemoveTheAdministrator = "You cannot remove the Administrator from the Administrator role.";
        public const string NothingSelected = "Nothing Selected.";
        public const string Unavailable = "Unavailable";
        public const string AppNameExists = "New Application Name Exists.";
        public const string AppNameNotValid = "New Application Name Is Not Valid.";
        public const string EntryInvalid = "Your entry has some invalid characters.";

    }


}
