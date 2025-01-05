using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using savnmore;

namespace savnmore.Models
{
    /// <summary>
    /// Represents the information collected to change a password
    /// </summary>
    public class ChangePasswordModel
    {
        /// <summary>
        /// The user's current password
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }
        /// <summary>
        /// The new password, must be a certain length
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = Constants.MinimuPasswordLength)]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string Password { get; set; }
        /// <summary>
        /// Confirmation of new password
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("Password", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
    /// <summary>
    /// Class represents what is needed to rest a password 
    /// </summary>
    public class PasswordResetModel
    {
        /// <summary>
        /// The userid this password reset is for
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// The password request key
        /// </summary>
        public int RequestId { get; set; }
        /// <summary>
        /// The password request hash
        /// </summary>
        public string HashId { get; set; }
        /// <summary>
        /// The user's name
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// The new password
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string Password { get; set; }
        /// <summary>
        /// Confirmation of new password
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("Password", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
    /// <summary>
    /// Represents the credentials for a logon
    /// </summary>
    public class LogOnModel
    {
        /// <summary>
        /// The username or email
        /// </summary>
        [Required]
        [Display(Name = "User name")]
        [RegularExpression(Constants.ValidEntries, ErrorMessage = ErrorConstants.EntryInvalid)]
        public string UserName { get; set; }
        /// <summary>
        /// The user's password
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
        /// <summary>
        /// Wether or not to createa persistent cookie
        /// </summary>
        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
    /// <summary>
    /// Class represents the forms fields to edit a user
    /// </summary>
    public class EditUser : IReadOnly
    {
        /// <summary>
        /// The user's key
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// The username
        /// </summary>
        [Display(Name = "Username")]
        [Required]
        [RegularExpression(Constants.ValidEntries, ErrorMessage = ErrorConstants.EntryInvalid)]
        public string UserName { get; set; }
        /// <summary>
        /// The users new first name
        /// </summary>
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        /// <summary>
        /// The users new lastname
        /// </summary>
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        /// <summary>
        /// The email address
        /// </summary>
        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email address")]
        [RegularExpression(Constants.EmailRegex, ErrorMessage = ErrorConstants.NotAValidEmail)]
        public string Email { get; set; }


        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
        [Display(Name = "Photo")]
        public string Photo { get; set; }
        [Display(Name = "Is Online")]
        public bool IsOnline { get; set; }
        [Display(Name = "Last Logon")]
        [DataType(DataType.DateTime)]
        public DateTime? LastLogon { get; set; }

        /// <summary>
        /// Only the user themselves can edit or delete readonly users. Readonly users cannot be renamed.
        /// </summary>
        [Required]
        [Display(Name = "Is Read Only")]
        public bool IsReadOnly { get; set; }
    }
    public class RegisterModel
    {
        [Required]
        [Display(Name = "User name")]
        [RegularExpression(Constants.ValidEntries, ErrorMessage = ErrorConstants.EntryInvalid)]
        public string UserName { get; set; }
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email address")]
        [RegularExpression(Constants.EmailRegex, ErrorMessage = ErrorConstants.NotAValidEmail)]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = Constants.MinimuPasswordLength)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

}
