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
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Security;
using savnmore;
using savnmore.Models;


namespace savnmore.Services
{
    /// <summary>
    /// Main class responsible for user membership functions
    /// </summary>
    public partial class UserService
    {
        /// <summary>
        /// Returns the app names in the database
        /// </summary>
        /// <returns></returns>
        public List<string> GetAppNames()
        {
            var apps = from t in _db.Users select t.AppName;
            return apps.Distinct().ToList();

        }
        private readonly string _appName;
        /// <summary>
        /// Default constructor with app name = setting in ConfigurationManager.AppSettings[Constants.ApplicationNameKey];
        /// </summary>
        public UserService()
        {
            _appName = ConfigurationManager.AppSettings[Constants.ApplicationNameKey];
        }
        /// <summary>
        /// Sets the app name to someting else
        /// </summary>
        /// <param name="applicationName"></param>
        public UserService(string applicationName)
        {
            _appName = applicationName;
        }
        private readonly savnmoreEntities _db = new savnmoreEntities();
        /// <summary>
        /// Creates a test user
        /// </summary>
        /// <returns></returns>
        public User Create()
        {
            var usr = DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture);
            var usrsvc = new UserPhotoService(usr);
            return new User
            {
                Email = usr + ConfigurationManager.AppSettings[Constants.DomainEmailSuffixKey],
                Password = FormsAuthentication.HashPasswordForStoringInConfigFile(usr, Constants.HashMethod),
                Photo = usrsvc.SavePhoto(null),
                UserName = usr,
                AppName = _appName,
                IsReadOnly = false
            };
        }
        /// <summary>
        /// Creates a new user with the parameters
        /// </summary>
        /// <param name="appName"></param>
        /// <param name="userName"></param>
        /// <param name="fName"></param>
        /// <param name="lName"></param>
        /// <param name="pwd"></param>
        /// <param name="email"></param>
        /// <param name="isReadonly"></param>
        /// <returns></returns>
        public static User CreateUser(string appName, string userName, string fName, string lName, string pwd, string email, bool isReadonly)
        {
            return new User
           {
               UserName = userName,
               FirstName = fName,
               LastName = lName,
               Password = FormsAuthentication.HashPasswordForStoringInConfigFile(pwd, Constants.HashMethod),
               Email = email,
               AppName = appName,
               IsReadOnly = isReadonly
           };
        }
        /// <summary>
        /// Creates teh administrator
        /// </summary>
        /// <param name="appName"></param>
        /// <returns></returns>
        public static User CreateAdmin(string appName)
        {
            string email = Constants.Admin + ConfigurationManager.AppSettings[Constants.DomainEmailSuffixKey];
            return CreateUser(appName, Constants.Admin, Constants.Admin, Constants.Admin, Constants.AdminPassword, email, true);
        }

        /// <summary>
        /// Attempts to add the user if the username and email do not already exist
        /// </summary>
        /// <param name="registerModel"></param>
        public void AddUser(RegisterModel registerModel)
        {
            if (UserNameExists(registerModel.UserName)) { throw new Exception(ErrorConstants.UserNameExists); }
            if (EmailExists(registerModel.Email)) { throw new Exception(ErrorConstants.EmailExists); }
            var newUser = new User
            {

                Email = registerModel.Email,
                Password = FormsAuthentication.HashPasswordForStoringInConfigFile(registerModel.Password, Constants.HashMethod),
                UserName = registerModel.UserName,
                LastName = registerModel.LastName,
                FirstName = registerModel.FirstName,
                AppName = _appName,
                IsReadOnly = false
            };

            _db.Users.Add(newUser);
            //var ur = new UserRole { RoleId = roleId, UserId = newUser.UserId };
            //_db.UserRoles.Add(ur);
            _db.SaveChanges();
        }

        /// <summary>
        /// Pages the list of users not in the role already found
        /// </summary>
        /// <param name="users"> </param>
        /// <param name="pager"></param>
        /// <returns></returns>
        public List<User> GetUsersPaged(List<User> users, Pager pager)
        {
            return users.Where(t => t.AppName == _appName).OrderBy(f => f.UserName).Skip((pager.Page - 1) * pager.Perpage).Take(pager.Perpage).ToList();

        }
        /// <summary>
        /// Returns a user object with no password set
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public User GetSecure(string userName)
        {
            var user = Get(userName);
            user.Password = string.Empty;
            return user;
        }
        /// <summary>
        ///  Returns a user object with no password set
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public User GetSecure(string userName, string password)
        {
            var pswd = FormsAuthentication.HashPasswordForStoringInConfigFile(password, Constants.HashMethod);
            var user = _db.Users.Single(p => p.UserName == userName && p.Password == pswd && p.AppName == _appName);
            user.Password = string.Empty;
            return user;
        }
        /// <summary>
        /// Returns all users ordered by name
        /// </summary>
        /// <returns></returns>
        public List<User> GetAll()
        {
            return _db.Users.Where(t => t.AppName == _appName).OrderBy(f => f.FirstName).ToList();
        }
        /// <summary>
        /// Returns a selectlist of users matching the username
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public IEnumerable<object> Find(string userName)
        {
            var result = from c in _db.Users where c.UserName.Contains(userName) && c.AppName == _appName select new { id = c.UserId, label = c.UserName };
            return result;
        }
        /// <summary>
        /// Searches the username first name and last name 
        /// </summary>
        /// <param name="searchTerm"></param>
        /// <returns></returns>
        public List<User> Search(string searchTerm)
        {
            var result = from c in _db.Users
                         where c.UserName.Contains(searchTerm) ||
                             c.FirstName.Contains(searchTerm) ||
                             c.LastName.Contains(searchTerm)
                             && c.AppName == _appName
                         select c;

            return result.Distinct().OrderBy(f => f.UserName).ToList();
        }
        /// <summary>
        /// Returns users ordered by firstname with paging
        /// </summary>
        /// <param name="pager"></param>
        /// <returns></returns>
        public List<User> GetAll(Pager pager)
        {
            return _db.Users.Where(t => t.AppName == _appName).Skip((pager.Page - 1) * pager.Perpage).Take(pager.Perpage).OrderBy(f => f.FirstName).ToList();
        }
        /// <summary>
        /// Returns a user with the specified id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public User Get(int id)
        {
            return _db.Users.Single(p => p.UserId == id);
        }
        /// <summary>
        /// Returns a user with the username
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public User Get(string userName)
        {
            return _db.Users.Where(t => t.AppName == _appName).Single(p => p.UserName == userName);
        }
        /// <summary>
        /// Returns the username associated with the email address
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public string GetUserName(string email)
        {
            return _db.Users.Where(t => t.AppName == _appName).Single(p => p.Email == email).UserName;
        }
        /// <summary>
        /// Returns a user with the email address, or throws an error stating no such email
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public User GetByEmail(string email)
        {
            try
            {
                return _db.Users.Where(t => t.AppName == _appName).Single(p => p.Email == email);
            }
            catch
            {

                throw new Exception(ErrorConstants.NoSuchEmail);
            }
        }
        /// <summary>
        /// An administration call which allows properties of the user to be edited
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public EditUser AdminGetUser(int userId)
        {
            try
            {
                User usr = _db.Users.Find(userId);
                var edituser = new EditUser
                                        {
                                            Email = usr.Email,
                                            UserId = usr.UserId,
                                            UserName = usr.UserName,
                                            FirstName = usr.FirstName,
                                            LastName = usr.LastName,
                                            IsOnline = usr.IsOnline,
                                            LastLogon = usr.LastLogon,
                                            Photo = usr.Photo,
                                            Password = string.Empty,//dont show
                                            IsReadOnly = usr.IsReadOnly
                                        };
                return edituser;
            }
            catch (Exception)
            {

                return new EditUser();
            }


        }
        /// <summary>
        /// Updates a user's details and photo. Throws error if email or username is changed and they exist.
        /// </summary>
        /// <param name="newUser"></param>
        /// <param name="userPhoto"></param>
        /// <returns></returns>
        public User Update(User newUser, HttpPostedFileBase userPhoto)
        {
            var usr = _db.Users.Where(t => t.AppName == _appName).Single(p => p.UserId == newUser.UserId);
            usr.FirstName = newUser.FirstName;
            usr.LastName = newUser.LastName;
            usr.AppName = _appName;
            //do they want to change their email address?
            if (usr.Email.ToUpper() != newUser.Email.ToUpper())
            {
                //is this new email taken?
                if (EmailExists(newUser.Email)) { throw new Exception(ErrorConstants.EmailExists); }
                //its not taken so they can update it
                usr.Email = newUser.Email;
            }
            //do they want to change their username?
            if (usr.UserName.ToUpper() != newUser.UserName.ToUpper())
            {
                if (usr.IsReadOnly) { throw new Exception(ErrorConstants.CannotChangeReadonlyUsers); }
                //is this new userid taken?
                if (UserNameExists(newUser.UserName)) { throw new Exception(ErrorConstants.UserNameExists); }
                //its not taken so they can update it
                usr.UserName = newUser.UserName;
            }
            PhotoService ps = new UserPhotoService(usr.UserName);
            usr.Photo = ps.UpdatePhoto(usr.Photo, userPhoto);
            _db.Entry(usr).State = (System.Data.Entity.EntityState)EntityState.Modified;
            _db.SaveChanges();
            return usr;
        }
        /// <summary>
        /// Admin update allowing the users password,online and lastlogon fields to be modified
        /// </summary>
        /// <param name="newUser"></param>
        /// <param name="userPhoto"></param>
        /// <returns></returns>
        public User AdminUpdate(EditUser newUser, HttpPostedFileBase userPhoto)
        {
            var usr = _db.Users.Where(t => t.AppName == _appName).Single(p => p.UserId == newUser.UserId);
            usr.LastName = newUser.LastName;
            usr.FirstName = newUser.FirstName;
            usr.AppName = _appName;
            //do they want to change their email address?
            if (usr.Email.ToUpper() != newUser.Email.ToUpper())
            {
                //is this new email taken?
                if (EmailExists(newUser.Email)) { throw new Exception(ErrorConstants.EmailExists); }

                //its not taken so they can update it
                usr.Email = newUser.Email;
            }
            //do they want to change their username?
            if (usr.UserName.ToUpper() != newUser.UserName.ToUpper())
            {
                if (usr.IsReadOnly)
                {
                    throw new Exception(ErrorConstants.CannotChangeReadonlyUsers);
                }
                if (usr.UserName == Constants.Admin) { throw new Exception(ErrorConstants.CannotRenameTheAdministrator); }
                if (UserNameExists(newUser.UserName)) { throw new Exception(ErrorConstants.UserNameExists); }
                //its not taken so they can update it
                usr.UserName = newUser.UserName;
            }
            PhotoService ps = new UserPhotoService(usr.UserName);
            usr.Photo = ps.UpdatePhoto(usr.Photo, userPhoto);
            if (!string.IsNullOrEmpty(newUser.Password))
            {
                usr.Password = FormsAuthentication.HashPasswordForStoringInConfigFile(newUser.Password, Constants.HashMethod);
            }
            if (newUser.LastLogon.HasValue)
            {
                usr.LastLogon = newUser.LastLogon.Value;
            }
            usr.IsOnline = newUser.IsOnline;
            if (usr.IsOnline)
            {
                usr.LastLogon = DateTime.Now;
            }
            _db.Entry(usr).State = (System.Data.Entity.EntityState)EntityState.Modified;
            _db.SaveChanges();
            return usr;
        }
        /// <summary>
        /// Marks a user online and updates lastlogon
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool MarkOnline(int userId)
        {
            var usr = _db.Users.Single(p => p.UserId == userId);
            usr.IsOnline = true;
            usr.LastLogon = DateTime.Now;
            _db.Entry(usr).State = (System.Data.Entity.EntityState)EntityState.Modified;
            return true;
        }
        /// <summary>
        /// Marks a user offline by userid
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool MarkOffline(int userId)
        {
            var usr = _db.Users.Single(p => p.UserId == userId);
            usr.IsOnline = false;
            _db.Entry(usr).State = (System.Data.Entity.EntityState)EntityState.Modified;
            _db.SaveChanges();
            return true;
        }
        /// <summary>
        /// Marks a user offline by username
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public bool MarkOffline(string userName)
        {
            var usr = _db.Users.Where(t => t.AppName == _appName).Single(p => p.UserName == userName);
            usr.IsOnline = false;
            _db.Entry(usr).State = (System.Data.Entity.EntityState)EntityState.Modified;
            _db.SaveChanges();
            return true;
        }
        /// <summary>
        /// Deletes a user and removes their photo, leaves thier user directory
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Delete(int id)
        {

            var usr = _db.Users.Single(p => p.UserId == id);
            if (usr.IsReadOnly)
            {
                throw new Exception(ErrorConstants.CannotChangeReadonlyUsers);
            }
            PhotoService psvc = new UserPhotoService(usr.UserName);
            psvc.DeletePhoto(usr.Photo);
            psvc.DeleteAllPhotos();
            _db.Users.Remove(usr);
            _db.SaveChanges();
            return true;
        }
        /// <summary>
        /// Deletes a user by username, deletes the photo but leaves the directory
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public bool Delete(string userName)
        {
            var usr = _db.Users.Where(t => t.AppName == _appName).Single(p => p.UserName == userName);
            return Delete(usr.UserId);
        }
        /// <summary>
        /// Returns true if the email exists
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public bool EmailExists(string email)
        {
            return _db.Users.Where(t => t.AppName == _appName).Any(p => p.Email == email);
        }
        /// <summary>
        /// Checks if the entered username is an email
        /// </summary>
        /// <param name="enteredName"></param>
        /// <returns></returns>
        public static bool IsUserNameAnEmail(string enteredName)
        {
            return enteredName.Contains(Constants.AtSymbol);
        }
        /// <summary>
        /// Checks to see if the username exists
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public bool UserNameExists(string userName)
        {
            return _db.Users.Where(t => t.AppName == _appName).Any(p => p.UserName == userName);
        }
        /// <summary>
        /// Logs on a user by checking their credentials, returns false if check fails
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool LogOn(string userName, string password)
        {
            try
            {
                var pswd = FormsAuthentication.HashPasswordForStoringInConfigFile(password, Constants.HashMethod);
                User theuser;
                if (userName.Contains(Constants.AtSymbol))
                {
                    theuser = _db.Users.Single(p => p.Email == userName && p.Password == pswd && p.AppName == _appName);
                }
                else
                {
                    theuser = _db.Users.Where(t => t.AppName == _appName).Single(p => p.UserName == userName && p.Password == pswd);
                }
                if (theuser != null)
                {
                    theuser.IsOnline = true;
                    theuser.LastLogon = DateTime.Now;
                    _db.Entry(theuser).State = (System.Data.Entity.EntityState)EntityState.Modified;
                    _db.SaveChanges();
                }
                return theuser != null;
            }
            catch (Exception ex)
            {
                Logger.WriteLine(MessageType.Error, userName + "," + password + ErrorConstants.LogonFailed + ex.Message);
            }
            return false;
        }
        /// <summary>
        /// Checks the user's credentials
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool ValidateUser(string userName, string password)
        {
            return LogOn(userName, password);
        }
        /// <summary>
        /// Checks the existing password for the user and if check passes the password is changed.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="oldPassword"></param>
        /// <param name="newpassword"></param>
        /// <returns></returns>
        public bool ChangePassword(string userName, string oldPassword, string newpassword)
        {
            try
            {
                //get the user with this old password
                var oldpswd = FormsAuthentication.HashPasswordForStoringInConfigFile(oldPassword, Constants.HashMethod);
                var newpswd = FormsAuthentication.HashPasswordForStoringInConfigFile(newpassword, Constants.HashMethod);
                var usr = _db.Users.Where(t => t.AppName == _appName).Single(p => p.UserName == userName && p.Password == oldpswd);
                if (usr == null)
                {
                    Logger.WriteLine(MessageType.Error, ErrorConstants.PasswordChangeFailure);
                    return false;
                }

                usr.Password = newpswd;
                _db.Entry(usr).State = (System.Data.Entity.EntityState)EntityState.Modified;
                _db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Logger.WriteLine(MessageType.Error, ErrorConstants.PasswordChangeFailure + ex.Message);
                return false;
            }

        }
        /// <summary>
        /// Tries to Register a new user and sends welcome email if sending a welcome email is set in webconfig
        /// </summary>
        /// <param name="newUser"></param>
        /// <param name="userPhoto"></param>
        /// <returns></returns>
        public User Register(RegisterModel newUser, HttpPostedFileBase userPhoto)
        {
            if (EmailExists(newUser.Email)) { throw new Exception(ErrorConstants.EmailExists); }
            if (UserNameExists(newUser.UserName)) { throw new Exception(ErrorConstants.UserNameExists); }
            var u = new User
                         {
                             //UserId = Guid.NewGuid(),
                             Email = newUser.Email,
                             Password = FormsAuthentication.HashPasswordForStoringInConfigFile(newUser.Password,
                                                                                        Constants.HashMethod),
                             UserName = newUser.UserName,
                             LastLogon = DateTime.Now,
                             LastName = string.Empty,
                             FirstName = string.Empty,
                             IsOnline = true,
                             AppName = _appName,
                             IsReadOnly = false
                         };
            PhotoService ps = new UserPhotoService(newUser.UserName); //save the users profile photo
            u.Photo = ps.SavePhoto(userPhoto);
            try
            {
                _db.Users.Add(u);
                _db.SaveChanges();

            }
            catch (Exception ex)
            {
                Logger.WriteLine(MessageType.Error, newUser.UserName + ErrorConstants.CouldNotRegisterUser + ex.Message);
                throw new Exception(ErrorConstants.CouldNotCreateUser);
            }
            try
            {
                EmailService.SendWelcomeEmail(u);
                //add the user to the users role by default
            }
            catch (Exception ex)
            {
                Logger.WriteLine(MessageType.Error, ErrorConstants.CouldNotSendWelcomeEmail + ex.Message);
            }

            return u;
        }
        /// <summary>
        /// Adds a reset entry for the user and sets it to expire the number of days set in PasswordResetExpireInDays webconfig key
        /// </summary>
        /// <param name="forUser"></param>
        /// <returns></returns>
        public int RequestPasswordReset(int forUser)
        {
            //here we need to add an entry for password reseting
            RemovePasswordResetRequests(forUser); //clear existing resets if any
            var newRequest = new PasswordResetRequest
            {

                UserId = forUser,
                HashId = DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture),
                ExpiresOn = DateTime.Now.AddDays(Convert.ToInt32(ConfigurationManager.AppSettings[Constants.PasswordResetExpireInDaysKey]))
            };
            _db.PasswordResetRequests.Add(newRequest);
            _db.SaveChanges();
            var user = Get(forUser);
            EmailService.SendResetEmail(user, newRequest.RequestId, newRequest.HashId);
            return newRequest.RequestId;
        }
        /// <summary>
        /// Returns the password reset request for the user if it exists
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="hashId"></param>
        /// <returns></returns>
        public PasswordResetRequest GetPasswordResetRequest(int userId, string hashId)
        {
            //here we need to return the password request
            try
            {
                var rq = _db.PasswordResetRequests.Single(p => p.UserId == userId && p.HashId == hashId);
                if (rq != null)
                {
                    return rq;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLine(MessageType.Error, ex.Message);
            }
            return null;
        }

        /// <summary>
        /// Returns the userID for the request, or throws error
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="hashId"> </param>
        /// <returns></returns>
        public User GetRequestUser(int requestId, string hashId)
        {
            //return the userId for this request
            //if its not valid , then its not valid
            var requestUserId = _db.PasswordResetRequests.Single(t => t.RequestId == requestId && t.HashId == hashId).UserId;
            return Get(requestUserId);
        }

        /// <summary>
        /// Returns true if the request is valid and has not expired.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="requestId"></param>
        /// <param name="hashId"> </param>
        /// <returns></returns>
        public bool IsValidPasswordResetRequest(int userId, int requestId, string hashId)
        {
            //here we need to check that the request is valid
            try
            {
                var rq = _db.PasswordResetRequests.Single(p => p.UserId == userId && p.RequestId == requestId && p.HashId == hashId);
                if (rq != null)
                {
                    DateTime nowTime = DateTime.Now;
                    //Greater than zero  t1 is later than t2. 
                    return DateTime.Compare(rq.ExpiresOn, nowTime) > 0;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLine(MessageType.Error, ex.Message);
            }
            return false;
        }

        /// <summary>
        /// Removes the request for a reset for the user
        /// </summary>
        /// <param name="requestId"></param>
        ///  /// <param name="userId"></param>
        /// <param name="hashId"> </param>
        public void RemovePasswordResetRequest(int requestId, int userId, string hashId)
        {
            try
            {
                var rq = _db.PasswordResetRequests.Single(p => p.RequestId == requestId && p.UserId == userId && p.HashId == hashId);
                if (rq != null)
                {
                    _db.PasswordResetRequests.Remove(rq);
                    _db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLine(MessageType.Error, ex.Message);
            }
        }
        /// <summary>
        /// Removes all password reset requests for this user
        /// </summary>
        /// <param name="userId"></param>
        public void RemovePasswordResetRequests(int userId)
        {
            try
            {
                var rq = _db.PasswordResetRequests.Where(p => p.UserId == userId);
                foreach (var r in rq)
                {
                    _db.PasswordResetRequests.Remove(r);

                }
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                Logger.WriteLine(MessageType.Error, ex.Message);
            }
        }
        /// <summary>
        /// Resets the password for the user 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="newPassword"></param>
        public void ResetPassword(int userId, string newPassword)
        {
            try
            {
                var newpswd = FormsAuthentication.HashPasswordForStoringInConfigFile(newPassword, Constants.HashMethod);
                var usr = _db.Users.Single(p => p.UserId == userId);
                usr.Password = newpswd;
                _db.Entry(usr).State = (System.Data.Entity.EntityState)EntityState.Modified;
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                Logger.WriteLine(MessageType.Error, ex.Message);
            }
        }
        /// <summary>
        /// Updates the IsOnline property to false
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool LogOff(int userId)
        {
            try
            {
                var usr = Get(userId);
                usr.IsOnline = false;
                _db.Entry(usr).State = (System.Data.Entity.EntityState)EntityState.Modified;
                _db.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// Updates the IsOnline property to false
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public bool LogOff(string userName)
        {
            try
            {
                var usr = Get(userName);
                usr.IsOnline = false;
                _db.Entry(usr).State = (System.Data.Entity.EntityState)EntityState.Modified;
                _db.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }
        /// <summary>
        /// Returns users that are marked online with paging
        /// </summary>
        /// <returns></returns>
        public List<User> WhosOnline()
        {
            var usersOnline = _db.Users.Where(t => (t.IsOnline && t.AppName == _appName)).ToList();
            return usersOnline.OrderBy(f => f.FirstName).ToList();
        }
        /// <summary>
        /// Returns the latest logon dates first
        /// </summary>

        /// <returns></returns>
        public List<User> LatestLogonTimes()
        {
            return _db.Users.OrderByDescending(t => t.LastLogon).ToList();

        }
        /// <summary>
        /// retuns the total count of users
        /// </summary>
        /// <returns></returns>
        public int UserCount()
        {
            return (from t in _db.Users where t.AppName == _appName select t).Count();
        }
        /// <summary>
        /// Returns all the password rest requests
        /// </summary>
        /// <returns></returns>
        public List<PasswordResetRequest> GetAllPasswordResetRequests()
        {
            return _db.PasswordResetRequests.OrderBy(t => t.UserId).OrderByDescending(t => t.ExpiresOn).ToList();
        }
        /// <summary>
        /// Returns true if this user is readonly
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool IsUserReadOnly(int userId)
        {

            var user = Get(userId);
            return user.IsReadOnly;

        }
    }
}
