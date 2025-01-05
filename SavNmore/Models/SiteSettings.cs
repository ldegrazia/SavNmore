using System.ComponentModel.DataAnnotations;

namespace savnmore.Models
{
    public class SiteSettings
    {
        [Display(Name = "ApplicationName")]
        public string ApplicationName { get; set; }
        [Display(Name = "LogFile")]
        public string LogFile { get; set; }
        [Display(Name = "Number Of Items Per Page")]
        public int NumberOfItemsPerPage { get; set; }
        [Display(Name = "DropRecreateDatabase?")]
        public bool DropRecreateDatabase { get; set; }
        [Display(Name = "Create Sample Roles And Users?")]
        public bool CreateSampleRolesAndUsers { get; set; }
        [Display(Name = "Number Of Sample Users")]
        public int NumberOfSampleUsers { get; set; }
        [Display(Name = "Domain Url")]
        public string DomainUrl { get; set; }
        [Display(Name = "Domain Email Suffix")]
        public string DomainEmailSuffix { get; set; }

        [Display(Name = "Use Password Strength Indicator?")]
        public bool UsePasswordStrength { get; set; }
        [Display(Name = "Throw an error wehn deleting populated roles?")]
        public bool ThrowErrorOnDeletingPopulatedRoles { get; set; }
        [Display(Name = "Role Images Root Path")]
        public string RoleImagesRootPath { get; set; }
        [Display(Name = "Default Role Photo")]
        public string DefaultRolePhoto { get; set; }

        [Display(Name = "User Images Root Path")]
        public string UserImagesRootPath { get; set; }

        [Display(Name = "Default User Photo")]
        public string DefaultUserPhoto { get; set; }

        [Display(Name = "SmtpServer")]
        public string SmtpServer { get; set; }
        [Display(Name = "SmtpServerPort")]
        public int SmtpServerPort { get; set; }

        [Display(Name = "Send Welcome Email?")]
        public bool SendWelcomeEmail { get; set; }
        [Display(Name = "Welcome Email Sender")]
        public string WelcomeEmailSender { get; set; }
        [Display(Name = "Welcome Email Subject")]
        public string WelcomeEmailSubject { get; set; }
        [Display(Name = "Welcome Email Body")]
        public string WelcomeEmailBody { get; set; }

        [Display(Name = "Send Reset Password Email?")]
        public bool SendResetPasswordEmail { get; set; }
        [Display(Name = "Reset Password Email Sender")]
        public string ResetPasswordSender { get; set; }
        [Display(Name = "Reset Password  Email Subject")]
        public string ResetPasswordSubject { get; set; }
        [Display(Name = "Reset Password Email Body")]
        public string ResetPasswordEmailBody { get; set; }
        [Display(Name = "Reset Password Link")]
        public string ResetPasswordLink { get; set; }

        [Display(Name = "Email ResetLink Marker")]
        public string EmailResetLinkMarker { get; set; }

        [Display(Name = "Password Reset Expire In Days")]
        public int PasswordResetExpireInDays { get; set; }

        [Display(Name = "User Name Marker")]
        public string UserNameMarker { get; set; }
        [Display(Name = "User Email Marker")]
        public string UserEmailMarker { get; set; }
        [Display(Name = "Domain Url Marker")]
        public string DomainUrlMarker { get; set; }

        [Display(Name = "Database Provider")]
        public string DatabaseProvider { get; set; }
        [Display(Name = "Connection String")]
        public string ConnectionString { get; set; }
        [Display(Name = "Database Server Version")]
        public string DatabaseVersion { get; set; }



    }
}