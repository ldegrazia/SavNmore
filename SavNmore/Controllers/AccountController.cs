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
using System.IO;
using System.Web.Mvc;
using System.Web.Security;
using savnmore;
using savnmore.Models;
using savnmore.Services;

namespace savnmore.Controllers
{
    public class AccountController : Controller
    {
        EfRoleProvider _roleProvider;
        UserService _userService;
        //
        // GET: /Account/LogOn
        //[RequireHttps] Consider using ssl
        public ActionResult LogOn()
        {
            ////for testing
            ////FormsAuthentication.SetAuthCookie(Constants.Admin, false);
            ////return RedirectToAction("Index", "Home");

            return View();

        }

        //
        // POST: /Account/LogOn

        [HttpPost]
        public ActionResult LogOn(LogOnModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                _userService = new UserService(HttpContext.Session[Constants.SessionAppNameKey].ToString());

                if (_userService.LogOn(model.UserName, model.Password))
                {
                    if (UserService.IsUserNameAnEmail(model.UserName))
                    {
                        //get the real username
                        model.UserName = _userService.GetUserName(model.UserName);
                    }
                    FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);
                    if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                        && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("", ErrorConstants.LogonFailure);
            }
            return View(model);   // If we got this far, something failed, redisplay form
        }

        //
        // GET: /Account/LogOff

        public ActionResult LogOff()
        {
            try
            {
                _userService = new UserService(HttpContext.Session[Constants.SessionAppNameKey].ToString());
                _userService.LogOff(User.Identity.Name);
                FormsAuthentication.SignOut();

            }
            catch (Exception ex)
            {
                Logger.WriteLine(MessageType.Error, ex.Message);
            }
            return RedirectToAction("Index", "Home");
        }



        //
        // GET: /Account/Register

        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register

        [HttpPost]
        public ActionResult Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _userService = new UserService(HttpContext.Session[Constants.SessionAppNameKey].ToString());
                    var registerdUser = _userService.Register(model, null);
                    _roleProvider = new EfRoleProvider(HttpContext.Session[Constants.SessionAppNameKey].ToString());
                    _roleProvider.AddToUsersRole(registerdUser.UserId);
                    FormsAuthentication.SetAuthCookie(model.UserName, false /* createPersistentCookie */);
                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return View(model);  // If we got this far, something failed, redisplay form
        }

        //
        // GET: /Account/ChangePassword

        [Authorize]
        public ActionResult ChangePassword()
        {

            return View();
        }

        //
        // POST: /Account/ChangePassword

        [Authorize]
        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            //only the Admin can change the admin password
            if (ModelState.IsValid)
            {
                bool changePasswordSucceeded;
                try
                {
                    _userService = new UserService(HttpContext.Session[Constants.SessionAppNameKey].ToString());
                    changePasswordSucceeded = _userService.ChangePassword(User.Identity.Name, model.OldPassword,
                                                                 model.Password);

                }
                catch (Exception ex)
                {
                    Logger.WriteLine(MessageType.Error, ErrorConstants.PasswordChangeFailure + ex.Message);
                    changePasswordSucceeded = false;
                }

                if (changePasswordSucceeded)
                {
                    return RedirectToAction("ChangePasswordSuccess");
                }
                ModelState.AddModelError("", ErrorConstants.PasswordChangeFailure);
            }
            return View(model);// If we got this far, something failed, redisplay form
        }

        //
        // GET: /Account/ChangePasswordSuccess

        public ActionResult ChangePasswordSuccess()
        {
            return View();
        }

        public ViewResult ForgotPassword()
        {
            var m = new ResetPasswordModel();
            return View(m);
        }
        /// <summary>
        /// Takes the user to the reset password form
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ForgotPassword(ResetPasswordModel model)
        {

            //use the user service to get the user
            try
            {
                _userService = new UserService(HttpContext.Session[Constants.SessionAppNameKey].ToString());
                User u = _userService.GetByEmail(model.Email);
                _userService.RequestPasswordReset(u.UserId);
            }
            catch (Exception ex)
            {

                Logger.WriteLine(MessageType.Error, ErrorConstants.ErrorSendingPasswordResetLink + ex.Message);
            }
            return RedirectToAction("PasswordLinkSent", "Account");
        }
        /// <summary>
        /// Informs the user their password has been reset if email is valid
        /// </summary>
        /// <returns></returns>
        public ViewResult PasswordLinkSent()
        {

            return View();
        }
        /// <summary>
        /// If the request is invalid
        /// </summary>
        /// <returns></returns>
        public ViewResult PasswordResetInvalid()
        {

            return View();
        }

        /// <summary>
        /// Resets the password
        /// </summary>
        /// <param name="reset"></param>
        /// <param name="username"></param>
        /// <param name="hash"> </param>
        /// <returns></returns>
        public ActionResult ResetPassword(string reset, string username, string hash)
        {
            if ((reset != null) && (username != null) && (hash != null))
            {
                try
                {
                    //use the default user service to get the userid for this request
                    //if it is invalid, throw an error

                    int resetReqId = Convert.ToInt32(reset);

                    _userService = new UserService(ConfigurationManager.AppSettings[Constants.SessionAppNameKey]);
                    var currentUser = _userService.GetRequestUser(resetReqId, hash);
                    //is this a validResetRequest?
                    if (_userService.IsValidPasswordResetRequest(currentUser.UserId, resetReqId, hash))
                    {
                        //the request is valid, get the old password for the user and let them change the password
                        var newChange = new PasswordResetModel { RequestId = resetReqId, UserName = currentUser.UserName, UserId = currentUser.UserId };
                        HttpContext.Session[Constants.ApplicationNameKey] = currentUser.AppName;//set the conext of the application for the user
                        return View(newChange);
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteLine(MessageType.Error, ErrorConstants.CouldNotResetPassword + ex.Message);
                }
            }
            return RedirectToAction("PasswordResetInvalid");
        }
        /// <summary>
        /// Tries to reset the password to the new credentials
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ResetPassword(PasswordResetModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _userService = new UserService(HttpContext.Session[Constants.SessionAppNameKey].ToString());
                    User u = _userService.Get(model.UserId);//try to get the userid 
                    _userService.ResetPassword(u.UserId, model.Password); //update the password for the user
                    _userService.RemovePasswordResetRequests(u.UserId);
                    return RedirectToAction("ChangePasswordSuccess"); //redirect to Succes Page
                }
                catch (Exception ex)
                {
                    Logger.WriteLine(MessageType.Error, ErrorConstants.CouldNotResetPassword + ex.Message);
                }
            }
            return View(model);  // If we got this far, something failed, redisplay form
        }
        /// <summary>
        /// Displays the users information
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public ViewResult MyAccount()
        {
            //get the current logged in user
            _userService = new UserService(HttpContext.Session[Constants.SessionAppNameKey].ToString());
            var usr = _userService.Get(User.Identity.Name);
            //see if the user is admin
            _roleProvider = new EfRoleProvider(HttpContext.Session[Constants.SessionAppNameKey].ToString());
            ViewBag.IsAdmin = (_roleProvider.IsUserInRole(usr.UserName, Constants.AdministratorsRole));
            //get the users's role
            _roleProvider = new EfRoleProvider(HttpContext.Session[Constants.SessionAppNameKey].ToString());
            ViewBag.Roles = _roleProvider.GetRolesForUser(usr.UserId);
            return View(usr);
        }
        /// <summary>
        /// Edits the users information
        /// </summary>
        /// <param name="changedUser"></param>
        /// <param name="form"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        public ViewResult MyAccount(User changedUser, FormCollection form)
        {
            //get the current logged in user
            _userService = new UserService(HttpContext.Session[Constants.SessionAppNameKey].ToString());
            _roleProvider = new EfRoleProvider(HttpContext.Session[Constants.SessionAppNameKey].ToString());
            ViewBag.Roles = _roleProvider.GetRolesForUser(changedUser.UserId);
            ViewBag.IsAdmin = (_roleProvider.IsUserInRole(changedUser.UserId, Constants.AdministratorsRole));
            try
            {
                var usr = _userService.Update(changedUser, Request.Files[Constants.Photo]);
                ViewBag.Result = Constants.ChangesSaved;
                return View(usr);
            }
            catch (Exception ex)
            {
                ViewBag.Result = ex.Message;
            }
            //something happend return the original state
            var currentUser = _userService.Get(User.Identity.Name);
            return View(currentUser);

        }
        /// <summary>
        /// Cancels a users account
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public ActionResult CancelAccount()
        {
            //get this user and make sure they want to cancel
            _userService = new UserService(HttpContext.Session[Constants.SessionAppNameKey].ToString());
            var userCurrent = _userService.Get(User.Identity.Name);
            if (userCurrent.UserName == Constants.Admin)
            {
                return Content(ErrorConstants.CannotDeleteAdministrator);
            }

            return View(userCurrent);
        }
        /// <summary>
        /// Cancels a users account
        /// </summary>
        /// <param name="canceledUser"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        public ActionResult CancelAccount(User canceledUser)
        {
            //get this user and make sure they want to cancel
            _userService = new UserService(HttpContext.Session[Constants.SessionAppNameKey].ToString());
            //double check admin
            var userCurrent = _userService.Get(canceledUser.UserId);
            if (userCurrent.IsReadOnly)
            {
                return Content(ErrorConstants.CannotChangeReadonlyUsers);
            }
            _roleProvider = new EfRoleProvider(HttpContext.Session[Constants.SessionAppNameKey].ToString());
            _userService.LogOff(canceledUser.UserId);
            _roleProvider.RemoveUserFromAllRoles(canceledUser.UserId);
            _userService.Delete(canceledUser.UserId);

            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }
        /// <summary>
        /// Displays the users photo, or nothing if there is no username
        /// </summary>
        /// <param name="name"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ActionResult UserPhoto(string name, string userName)
        {
            var ups = new UserPhotoService(userName);
            try
            {

                return new FileStreamResult(new FileStream(ups.GetPhoto(name), FileMode.Open), ups.GetContentType(name));
            }
            catch (Exception)
            {
                return null;
            }

        }

    }
}