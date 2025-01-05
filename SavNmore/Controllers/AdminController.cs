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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using savnmore;
using savnmore.Models;
using savnmore.Services;

namespace savnmore.Controllers
{
    [Authorize(Roles = Constants.AdministratorsRole)]
    public class AdminController : Controller
    {
        private readonly savnmoreEntities _db = new savnmoreEntities();
        EfRoleProvider _roleProvider;
        UserService _userService;
        /// <summary>
        /// Main landing page for Administration
        /// </summary>
        /// <returns></returns>
        public ViewResult Index()
        {
            _roleProvider = new EfRoleProvider(HttpContext.Session[Constants.SessionAppNameKey].ToString());
            _userService = new UserService(HttpContext.Session[Constants.SessionAppNameKey].ToString());
            var currentUser = _userService.Get(User.Identity.Name);
            ViewBag.NumberOfUsers = _userService.UserCount();
            ViewBag.NumberOfUsersOnline = _userService.WhosOnline().Count();
            ViewBag.NumberOfRoles = _roleProvider.GetAllRoles().Count();
            ViewBag.SiteName = ConfigurationManager.AppSettings[Constants.DomainUrlKey];
            ViewBag.Version = Constants.ApplicationVersion;
            ViewBag.CurrentAppName = HttpContext.Session[Constants.SessionAppNameKey].ToString();
            ViewBag.IsSuperAdmin = _roleProvider.IsUserSuperAdmin(currentUser.UserId);
            return View();
        }
        public ViewResult Roles()
        {
            _roleProvider = new EfRoleProvider(HttpContext.Session[Constants.SessionAppNameKey].ToString());

            var roles = _roleProvider.Get();
            foreach (var role in roles)
            {
                role.MemberCount = _roleProvider.GetMemberCount(role.RoleId);
            }
            return View(roles);
        }
        /// <summary>
        /// Post method that adds all users to the role 
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddAllUsers(int roleId)
        {
            _roleProvider = new EfRoleProvider(HttpContext.Session[Constants.SessionAppNameKey].ToString());
            _roleProvider.AddAllUsersToRole(roleId);
            return RedirectToAction("Details", new { id = roleId, result = Constants.AddedAllUsers });
        }
        /// <summary>
        /// Removes all members from the role, but the admin
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public ActionResult RemoveAllMembers(int roleId)
        {
            _roleProvider = new EfRoleProvider(HttpContext.Session[Constants.SessionAppNameKey].ToString());

            try
            {
                _roleProvider.RemoveAllUsersFromRole(roleId);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Details", new { id = roleId, result = ex.Message });
            }
            return RedirectToAction("Details", new { id = roleId, result = Constants.RemovedAllMembers });
        }
        /// <summary>
        /// Returns the details of the role
        /// </summary>
        /// <param name="id"></param>
        /// <param name="itemcount"></param>
        /// <param name="page"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public ViewResult Details(int id, int? itemcount, int? page, string result)
        {
            ViewBag.Role = _db.Roles.Single(p => p.RoleId == id);
            _roleProvider = new EfRoleProvider(HttpContext.Session[Constants.SessionAppNameKey].ToString());

            var pager = new Pager();
            if (page.HasValue)
            {
                pager.Page = page.Value;
            }
            var notin = _roleProvider.GetUsersInRole(id);
            pager.ItemCount = notin.Count();
            var q = _roleProvider.GetUsersPaged(notin, pager);
            ViewBag.Pager = pager;
            ViewBag.Result = result;
            return View(q);
        }
        [HttpPost]
        public ActionResult Details(int id, FormCollection form)
        {
            int page;
            try
            {
                page = Convert.ToInt32(form.Get("pagenum").ToString(CultureInfo.InvariantCulture));
            }
            catch
            {
                page = 1;
            }
            return RedirectToAction("Details", new { id, page });
        }
        /// <summary>
        /// The view to create a new role
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            var newRole = new Role { AppName = HttpContext.Session[Constants.SessionAppNameKey].ToString() };
            return View(newRole);
        }
        /// <summary>
        /// Post method when creating a role
        /// </summary>
        /// <param name="newrole"></param>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Create(Role newrole, FormCollection form)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _roleProvider = new EfRoleProvider(HttpContext.Session[Constants.SessionAppNameKey].ToString());
                    _roleProvider.CreateRole(newrole, Request.Files[Constants.Photo]);
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            return View(newrole);
        }

        /// <summary>
        /// View returned when creating a new user
        /// </summary>
        /// <returns></returns>
        public ViewResult CreateUser()
        {
            ViewBag.AvailableRoles = GetRoles();
            return View();
        }
        /// <summary>
        /// Post method when creating a new user
        /// </summary>
        /// <param name="registerModel">New user details in the form of a register model</param>
        /// <param name="collection">Contains the roleid to add the user to</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CreateUser(RegisterModel registerModel, FormCollection collection)
        {
            ViewBag.AvailableRoles = GetRoles();
            if (ModelState.IsValid)
            {
                _userService = new UserService(HttpContext.Session[Constants.SessionAppNameKey].ToString());
                _roleProvider = new EfRoleProvider(HttpContext.Session[Constants.SessionAppNameKey].ToString());
                User newUser;
                try
                {
                    _userService.Register(registerModel, Request.Files[Constants.Photo]);
                    newUser = _userService.Get(registerModel.UserName);
                    _roleProvider.AddToUsersRole(newUser.UserId); //Always add the user to the Users Role
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                    return View(registerModel);
                }
                try
                {
                    int roleId = int.Parse(collection.Get("RoleId").ToString(CultureInfo.InvariantCulture));
                    _roleProvider.AddUserToRole(newUser.UserId, roleId); //add to the selected role


                }
                catch (Exception ex)
                {
                    Logger.WriteLine(MessageType.Warning, ex.Message);//no role chosen, so its fine
                }
                return RedirectToAction("Users", new { result = Constants.ChangesSaved });
            }

            return View(registerModel);

        }
        /// <summary>
        /// View when a user wants to edit a role
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Edit(int id)
        {
            _roleProvider = new EfRoleProvider(HttpContext.Session[Constants.SessionAppNameKey].ToString());
            Role role = _roleProvider.Get(id);
            return View(role);
        }
        /// <summary>
        /// Post method when editing a role
        /// </summary>
        /// <param name="changedRole">Role to change</param>
        /// <param name="form">Contains the photo</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Edit(Role changedRole, FormCollection form)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _roleProvider = new EfRoleProvider(HttpContext.Session[Constants.SessionAppNameKey].ToString());
                    _roleProvider.UpdateRole(changedRole, Request.Files[Constants.Photo]);
                    return RedirectToAction("Edit", new { id = changedRole.RoleId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return View(changedRole);
        }

        /// <summary>
        /// View to delete a user
        /// </summary>
        /// <param name="userId">the User id</param>
        /// <returns></returns>
        public ActionResult Delete(int userId)
        {
            _userService = new UserService(HttpContext.Session[Constants.SessionAppNameKey].ToString());

            User user = _userService.Get(userId);
            if (user.UserName == User.Identity.Name)
            {
                return Content(ErrorConstants.YouCannotDeleteyourself);
            }
            if (user.IsReadOnly)
            {
                return Content(ErrorConstants.CannotChangeReadonlyUsers);
            }
            return View(user);
        }
        /// <summary>
        /// Post method when deleting a user
        /// </summary>
        /// <param name="userId">Userid to delete</param>
        /// <param name="noform"></param>
        /// <returns></returns>
        [HttpPost, ActionName("Delete")]
        public ActionResult Delete(int userId, FormCollection noform)
        {
            //try to delete this user from all of their roles
            string result = Constants.ChangesSaved;
            try
            {
                _roleProvider = new EfRoleProvider(HttpContext.Session[Constants.SessionAppNameKey].ToString());
                _roleProvider.RemoveUserFromAllRoles(userId);
                _userService = new UserService(HttpContext.Session[Constants.SessionAppNameKey].ToString());
                _userService.Delete(userId);
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return RedirectToAction("Users", new { result });
        }

        /// <summary>
        /// Deletes a checked list of users
        /// </summary>
        /// <param name="noform">Contains the ids of users to delete</param>
        /// <param name="cameFrom"> </param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeleteMultipleUsers(FormCollection noform, string cameFrom)
        {
            if (string.IsNullOrEmpty(cameFrom))
            {
                cameFrom = "Users";
            }
            string result;
            string[] idstoDelete;
            try
            {
                idstoDelete = noform.Get(FormKeys.DeleteInputs).ToString(CultureInfo.InvariantCulture).Split(','); //get inputs
            }
            catch (Exception)
            {
                result = ErrorConstants.NothingSelected;
                return RedirectToAction(cameFrom, new { result });
            }
            _userService = new UserService(HttpContext.Session[Constants.SessionAppNameKey].ToString());

            _roleProvider = new EfRoleProvider(HttpContext.Session[Constants.SessionAppNameKey].ToString());
            int deleted = 0;
            foreach (var s in idstoDelete)
            {
                try
                {
                    int id = int.Parse(s);
                    _roleProvider.RemoveUserFromAllRoles(id); //remove the users roles
                    _userService.Delete(id);
                    deleted++;
                }
                catch (Exception ex)
                {
                    if (ex.Message == ErrorConstants.CannotChangeReadonlyUsers)
                    {
                        //that's fine.
                    }
                }
            }
            result = StringHelper.Pluralise("Deleted " + deleted + " user", deleted);
            return RedirectToAction(cameFrom, new { result });
        }
        /// <summary>
        /// View returned when a user wants to delete a role
        /// </summary>
        /// <param name="id">Roleid to delete</param>
        /// <returns></returns>
        public ActionResult DeleteRole(int id)
        {
            _roleProvider = new EfRoleProvider(HttpContext.Session[Constants.SessionAppNameKey].ToString());
            var role = _roleProvider.Get(id);
            if (role.IsReadOnly)
            {
                return Content(ErrorConstants.CannotChangeReadonlyRoles);
            }
            return View(role);
        }
        /// <summary>
        /// Post method to delete a role
        /// </summary>
        /// <param name="roleId">Roleid to delete</param>
        /// <param name="noform"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeleteRole(int roleId, FormCollection noform)
        {
            try
            {
                _roleProvider = new EfRoleProvider(HttpContext.Session[Constants.SessionAppNameKey].ToString());
                _roleProvider.DeleteRole(roleId, Convert.ToBoolean(ConfigurationManager.AppSettings[Constants.ThrowErrorOnDeletingPopulatedRolesKey]));
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
            return RedirectToAction("Index");
        }
        /// <summary>
        /// Removes a user from a role
        /// </summary>
        /// <param name="roleId">The role to remove the user from</param>
        /// <param name="userId">The user to remove</param>
        /// <returns></returns>
        public ActionResult RemoveUserFromRole(int roleId, int userId)
        {
            string result;
            try
            {
                _userService = new UserService(HttpContext.Session[Constants.SessionAppNameKey].ToString());
                if (_userService.IsUserReadOnly(userId))
                {
                    throw new Exception(ErrorConstants.CannotChangeReadonlyUsers);
                }
                _roleProvider = new EfRoleProvider(HttpContext.Session[Constants.SessionAppNameKey].ToString());
                _roleProvider.RemoveUserFromRole(userId, roleId);
                result = "User removed";
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return RedirectToAction("Details", new { id = roleId, result });
        }
        /// <summary>
        /// Removes a set of users from a role
        /// </summary>
        /// <param name="roleId">The role id affected</param>
        /// <param name="collection">The collection of userids to remove</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult RemoveMultipleUsers(int roleId, FormCollection collection)
        {
            string result;

            string[] idstoRemove;
            try
            {
                idstoRemove = collection.Get(FormKeys.DeleteInputs).ToString(CultureInfo.InvariantCulture).Split(','); //get inputs

            }
            catch (Exception)
            {
                result = ErrorConstants.NothingSelected;

                return RedirectToAction("Details", new { id = roleId, result });
            }
            _roleProvider = new EfRoleProvider(HttpContext.Session[Constants.SessionAppNameKey].ToString());
            _userService = new UserService(HttpContext.Session[Constants.SessionAppNameKey].ToString());

            int deleted = 0;
            foreach (string s in idstoRemove)
            {
                try
                {
                    int userId = Convert.ToInt32(s);
                    if (_userService.IsUserReadOnly(userId))
                    {
                        continue; //do not remove this user
                    }
                    _roleProvider.RemoveUserFromRole(int.Parse(s), roleId); //remove the users roles
                    deleted++;
                }
                catch (Exception ex)
                {

                    result = ex.Message;
                    return RedirectToAction("Details", new { id = roleId, result });
                }
            }
            result = StringHelper.Pluralise("Removed " + deleted + " user", deleted);
            return RedirectToAction("Details", new { id = roleId, result });
        }
        /// <summary>
        /// Displays users in the role with paging
        /// </summary>
        /// <param name="itemcount">The total records</param>
        /// <param name="page">Current page number</param>
        /// <param name="result">Any action result from a previous operation</param>
        /// <returns></returns>
        public ActionResult Users(int? itemcount, int? page, string result)
        {
            var pager = new Pager();
            if (page.HasValue)
            {
                pager.Page = page.Value;
            }
            _userService = new UserService(HttpContext.Session[Constants.SessionAppNameKey].ToString());
            var users = _userService.GetAll();
            pager.ItemCount = itemcount.HasValue ? itemcount.Value : users.Count();

            ViewBag.Pager = pager;
            ViewBag.Result = result;


            var usrs = _userService.GetUsersPaged(users, pager);
            return View(usrs);
        }
        /// <summary>
        /// Advances to the page of users
        /// </summary>
        /// <param name="form">Contains the page number</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Users(FormCollection form)
        {
            int page;
            try
            {
                page = Convert.ToInt32(form.Get("pagenum").ToString(CultureInfo.InvariantCulture));
            }
            catch
            {
                page = 1;
            }
            return RedirectToAction("Users", new { page });
        }
        /// <summary>
        /// Edits a user 
        /// </summary>
        /// <param name="userId">The user Id to edit</param>
        /// <returns></returns>
        public ViewResult EditUser(int userId)
        {
            ViewBag.AvailableRoles = GetRoles();
            _userService = new UserService(HttpContext.Session[Constants.SessionAppNameKey].ToString());
            EditUser edituser = _userService.AdminGetUser(userId);
            _roleProvider = new EfRoleProvider(HttpContext.Session[Constants.SessionAppNameKey].ToString());

            ViewBag.Roles = _roleProvider.GetRolesForUser(userId);
            return View(edituser);

        }
        /// <summary>
        /// The post method when searching for a user
        /// </summary>
        /// <param name="collection">Contains the search term</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Search(FormCollection collection)
        {
            string term;
            try
            {
                term = collection.Get("search").ToString(CultureInfo.InvariantCulture);
            }
            catch
            {
                return RedirectToAction("Users");
            }
            var svc = new UserService();
            var users = svc.Search(term);
            return View(users);

        }
        /// <summary>
        /// Search method for roles
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SearchRoles(FormCollection collection)
        {
            string term;
            int id;
            try
            {
                term = collection.Get("search").ToString(CultureInfo.InvariantCulture);
                id = int.Parse(collection.Get("roleId").ToString(CultureInfo.InvariantCulture));

            }
            catch
            {
                return RedirectToAction("Index");
            }
            _roleProvider = new EfRoleProvider(HttpContext.Session[Constants.SessionAppNameKey].ToString());

            ViewBag.Role = _roleProvider.Get(id);
            var membersLike = _roleProvider.Search(id, term);
            return View(membersLike);

        }
        /// <summary>
        /// Edits a user from a drop down list or autocomplete
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult EditFromDdlUser(FormCollection collection)
        {
            int userId;
            try
            {
                userId = int.Parse(collection.Get("dduserId").ToString(CultureInfo.InvariantCulture));

            }
            catch
            {
                return RedirectToAction("Index");
            }
            return RedirectToAction("EditUser", new { userId });

        }
        /// <summary>
        /// Edits a user with a new photo
        /// </summary>
        /// <param name="changedUser">New user object</param>
        /// <param name="collection">Contains the new photo</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult EditUser(EditUser changedUser, FormCollection collection)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    int roleId = int.Parse(collection.Get("RoleId").ToString(CultureInfo.InvariantCulture));
                    _roleProvider = new EfRoleProvider(HttpContext.Session[Constants.SessionAppNameKey].ToString());

                    _roleProvider.AddUserToRole(changedUser.UserId, roleId);

                }
                catch (Exception ex)
                {
                    //no role chosen
                    Logger.WriteLine(MessageType.Error, ex.Message);
                }
                try
                {
                    //update the user
                    _userService = new UserService(HttpContext.Session[Constants.SessionAppNameKey].ToString());

                    _userService.AdminUpdate(changedUser, Request.Files[Constants.Photo]);

                    string result = changedUser.UserName + " updated.";
                    return RedirectToAction("Users", new { result });
                }
                catch (Exception ex)
                {

                    ModelState.AddModelError("", ex.Message);
                }
            }
            return View(changedUser);
        }
        /// <summary>
        /// Returns the view of who is online
        /// </summary>
        /// <returns></returns>
        public ViewResult WhosOnline()
        {
            _userService = new UserService(HttpContext.Session[Constants.SessionAppNameKey].ToString());
            var usersOnline = _userService.WhosOnline();
            return View(usersOnline);
        }
        /// <summary>
        /// Returns all password reset requests that are open
        /// </summary>
        /// <returns></returns>
        public ViewResult PasswordResets()
        {
            _userService = new UserService(HttpContext.Session[Constants.SessionAppNameKey].ToString());

            var usersResetRequests = _userService.GetAllPasswordResetRequests();
            return View(usersResetRequests);
        }
        /// <summary>
        /// Removes all password reset requests
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public ActionResult RemovePasswordReset(int userId)
        {
            try
            {
                _userService = new UserService(HttpContext.Session[Constants.SessionAppNameKey].ToString());

                _userService.RemovePasswordResetRequests(userId);
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
            return RedirectToAction("PasswordResets");
        }

        /// <summary>
        /// Orders the users by last logon time to find stale users
        /// </summary>
        /// <param name="page"></param>
        /// <param name="cameFrom"> </param>
        /// <param name="result"> </param>
        /// <returns></returns>
        public ViewResult LatestLastLogons(int? page, string cameFrom, string result)
        {
            _userService = new UserService(HttpContext.Session[Constants.SessionAppNameKey].ToString());

            var pager = new Pager();
            if (page.HasValue)
            {
                pager.Page = page.Value;
            }
            var q = _userService.LatestLogonTimes();
            pager.ItemCount = q.Count();
            ViewBag.Pager = pager;
            ViewBag.Result = result;
            return View(_userService.GetUsersPaged(q, pager));
        }
        /// <summary>
        /// Orders the users by last logon time to find stale users, advancing to the requested page number
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult LatestLastLogons(FormCollection form)
        {
            int page;
            try
            {
                page = Convert.ToInt32(form.Get("pagenum").ToString(CultureInfo.InvariantCulture));
            }
            catch
            {
                page = 1;
            }
            return RedirectToAction("LatestLastLogons", new { page });
        }
        /// <summary>
        /// Returns the view of setie settings
        /// </summary>
        /// <returns></returns>
        public ActionResult SiteSettings()
        {
            var svc = new SiteSettingsService();
            try
            {
                var settings = svc.Get();
                return View(settings);
            }
            catch (Exception ex)
            {

                return Content(ex.Message);
            }
        }
        protected override void Dispose(bool disposing)
        {
            _db.Dispose();
            base.Dispose(disposing);
        }
        /// <summary>
        /// Returns a select list of roles
        /// </summary>
        /// <returns></returns>
        public SelectList GetRoles()
        {
            _roleProvider = new EfRoleProvider(HttpContext.Session[Constants.SessionAppNameKey].ToString());

            var j = _roleProvider.Get();
            var items = new List<SelectListItem>();
            foreach (var r in j)
            {
                items.Add(new SelectListItem { Text = r.Name, Value = r.RoleId.ToString(CultureInfo.InvariantCulture) });
            }
            return new SelectList(items, "Value", "Text");
        }
        /// <summary>
        /// Returns the role photo, or the default photo
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ActionResult RolePhoto(string name)
        {
            var rps = new RolePhotoService();
            try
            {
                return new FileStreamResult(new FileStream(rps.GetPhoto(name), FileMode.Open), rps.GetContentType(name));

            }
            catch (Exception)
            {

                return Content(string.Empty);
            }
        }
        /// <summary>
        /// Returns the user photo or the default
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
                return new FileStreamResult(new FileStream(ups.GetDefaultPhotoFullPath(), FileMode.Open), ups.GetContentType(ups.GetDefaultPhoto()));

            }
        }

        /// <summary>
        /// Adds users to the role
        /// </summary>
        /// <param name="roleId">The roleid </param>
        /// <param name="page">Current page number</param>
        /// <param name="result">Any result from the last operation</param>
        /// <returns></returns>
        public ActionResult AddUsers(int roleId, int? page, string result)
        {
            //get all the users not in the role with paging so they can be added in bulk            
            _roleProvider = new EfRoleProvider(HttpContext.Session[Constants.SessionAppNameKey].ToString());

            var pager = new Pager();
            var q = _roleProvider.GetUsersNotInRole(roleId);
            pager.ItemCount = q.Count();
            if (page.HasValue)
            {
                pager.Page = page.Value;
            }

            ViewBag.Role = _roleProvider.Get(roleId);
            var notin = _roleProvider.GetUsersPaged(q, pager);
            ViewBag.Pager = pager;
            ViewBag.Result = result;
            return View(notin);
        }

        /// <summary>
        /// Poseted during paging to add a user
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="form"> </param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddUsers(int roleId, FormCollection form)
        {
            //get all the users not in the role with paging so they can be added in bulk                      
            int page;
            try
            {
                page = Convert.ToInt32(form.Get("pagenum").ToString(CultureInfo.InvariantCulture));
            }
            catch
            {
                page = 1;
            }
            return RedirectToAction("AddUsers", new { roleId, page });
        }

        /// <summary>
        /// Post method to add a collection of users
        /// </summary>
        /// <param name="roleId"> </param>
        /// <param name="noform">User ids in a form to add</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddMultipleUsers(int roleId, FormCollection noform)
        {
            string result;
            string[] idstoAdd;
            try
            {
                idstoAdd = noform.Get(FormKeys.DeleteInputs).ToString(CultureInfo.InvariantCulture).Split(','); //get inputs

            }
            catch (Exception)
            {
                result = ErrorConstants.NothingSelected;
                return RedirectToAction("AddUsers", new { roleId, result });
            }

            _roleProvider = new EfRoleProvider(HttpContext.Session[Constants.SessionAppNameKey].ToString());

            int added = 0;
            foreach (var s in idstoAdd)
            {
                try
                {
                    int id = int.Parse(s);
                    _roleProvider.AddUserToRole(id, roleId); //remove the users roles

                    added++;
                }
                catch (Exception ex)
                {
                    result = ex.Message + ",Only added " + added + StringHelper.Pluralise("user", added);
                    return RedirectToAction("AddUsers", new { roleId, result });
                }
            }
            result = StringHelper.Pluralise("Added " + added + " user", added);
            return RedirectToAction("AddUsers", new { roleId, result });
        }
        /// <summary>
        /// Adds one user to one role
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="roleId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddUser(int userId, int roleId)
        {
            string result;
            try
            {
                _roleProvider = new EfRoleProvider(HttpContext.Session[Constants.SessionAppNameKey].ToString());

                _roleProvider.AddUserToRole(userId, roleId);
                result = "Added user";
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return RedirectToAction("AddUsers", new { id = roleId, result });
        }
        /// <summary>
        /// Json call to find users by name for autocomplete
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        public ActionResult FindUser(string term)
        {
            _userService = new UserService(HttpContext.Session[Constants.SessionAppNameKey].ToString());

            var users = _userService.Find(term);
            return Json(users.ToArray(), JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Finds users not in a role for autocomplete
        /// </summary>
        /// <param name="term"></param>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public ActionResult FindUserNotInRole(string term, int roleId)
        {
            _roleProvider = new EfRoleProvider(HttpContext.Session[Constants.SessionAppNameKey].ToString());

            var users = _roleProvider.FindUsersNotInRole(term, roleId);
            return Json(users.ToArray(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult ApplicationName(string result)
        {
            //here we can create a new application name
            ViewBag.AvailableAppNames = GetAppNames(); //we will show them a drop down of app names.
            ViewBag.CurrentAppName = HttpContext.Session[Constants.SessionAppNameKey].ToString();
            //we will show them their current app name.
            ViewBag.Result = result;

            //we will show a text box that will let them create a new app name
            return View();
        }
        [HttpPost]
        public ActionResult ApplicationName(FormCollection collection)
        {
            ViewBag.AvailableAppNames = GetAppNames(); //we will show them a drop down of app names.
            ViewBag.CurrentAppName = HttpContext.Session[Constants.SessionAppNameKey].ToString();

            string newName;
            try
            {
                newName = collection.Get(FormKeys.ChangeAppName).ToString(CultureInfo.InvariantCulture);

            }
            catch (Exception)
            {
                ViewBag.Result = ErrorConstants.NothingSelected;
                return View();
            }
            //log them out of thier current application name
            _userService = new UserService(HttpContext.Session[Constants.SessionAppNameKey].ToString());
            _userService.LogOff(User.Identity.Name);

            FormsAuthentication.SignOut();
            HttpContext.Session[Constants.SessionAppNameKey] = newName;
            //redirect to the logonpage

            return RedirectToAction("Logon", "Account");
        }
        /// <summary>
        /// Returns a select list of roles
        /// </summary>
        /// <returns></returns>
        public SelectList GetAppNames()
        {
            _roleProvider = new EfRoleProvider(HttpContext.Session[Constants.SessionAppNameKey].ToString());

            var j = _roleProvider.GetAppNames();
            var items = new List<SelectListItem>();
            foreach (var r in j)
            {
                items.Add(new SelectListItem { Text = r, Value = r });
            }
            return new SelectList(items, "Value", "Text");
        }

        [HttpPost]
        public ActionResult CreateAppName(FormCollection collection)
        {
            //here, we see if the appname exists,
            //if it exists we throw an error
            try
            {
                string newName = collection.Get(FormKeys.NewAppName).ToString(CultureInfo.InvariantCulture);
                _roleProvider = new EfRoleProvider(HttpContext.Session[Constants.SessionAppNameKey].ToString());
                //creat3 the admin for this appname, create the admin role and add the new admin
                var result = _roleProvider.CreateNewApplication(newName);
                return RedirectToAction("ApplicationName", new { result });
            }
            catch (Exception ex)
            {

                return RedirectToAction("ApplicationName", new { result = ex.Message });
            }

        }
        /// <summary>
        /// Dumps the log contents to the disply
        /// </summary>
        /// <returns></returns>
        public ActionResult DumpLog()
        {
            return Content(Logger.DumpLog());
        }
        /// <summary>
        /// Deletes teh log
        /// </summary>
        /// <returns></returns>
        public ActionResult DeleteLog()
        {
            return Content(Logger.DeleteLog());
        }
    }
}
