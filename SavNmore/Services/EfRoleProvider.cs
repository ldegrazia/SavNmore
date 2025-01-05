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
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Security;
using System.Data;
using savnmore;
using savnmore.Models;
using System.Collections.Specialized;
namespace savnmore.Services
{
    /// <summary>
    /// Implementation of RoleProvider
    /// </summary>
    public partial class EfRoleProvider : RoleProvider
    {

        private UserService _userSvc;
        public override void Initialize(string name, NameValueCollection config)
        {
            base.Initialize(name, config);
            _userSvc = new UserService(_appName);
        }
        /// <summary>
        /// Default constructor has app name - app key
        /// </summary>
        public EfRoleProvider()
        {
            _userSvc = new UserService(_appName);
        }
        public EfRoleProvider(string applicationName)
        {
            _appName = applicationName;
            _userSvc = new UserService(_appName);
        }
        private readonly savnmoreEntities _db = new savnmoreEntities();
        /// <summary>
        /// Returns the app names in the database for the roles
        /// </summary>
        /// <returns></returns>
        public List<string> GetAppNames()
        {
            var apps = from t in _db.Roles select t.AppName;
            return apps.Distinct().ToList();
        }
        //= new UserService(appName);
        /// <summary>
        /// Returns the role by id or nothing
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public Role Get(int roleId)
        {
            try
            {
                return _db.Roles.Single(t => t.RoleId == roleId);
            }
            catch (Exception)
            {

                return new Role();
            }
        }
        /// <summary>
        /// Creates an administrators role for the application name
        /// </summary>
        /// <param name="appName"></param>
        /// <returns></returns>
        public static Role CreateAdminRole(string appName)
        {
            return CreateRole(appName, Constants.AdministratorsRole, Constants.AdministratorsRoleDescription, true);
        }
        /// <summary>
        /// Creates the users role
        /// </summary>
        /// <param name="appName"></param>
        /// <returns></returns>
        public static Role CreateUsersRole(string appName)
        {
            return CreateRole(appName, Constants.UsersRole, Constants.UsersRoleDescription, true);
        }
        /// <summary>
        /// Creates a role using the parameters
        /// </summary>
        /// <param name="appName"></param>
        /// <param name="roleName"></param>
        /// <param name="roleDescription"></param>
        /// <param name="isReadonly"></param>
        /// <returns></returns>
        public static Role CreateRole(string appName, string roleName, string roleDescription, bool isReadonly)
        {
            return new Role
            {
                Name = roleName,
                Description = roleDescription,
                AppName = appName,
                IsReadOnly = isReadonly
            };
        }
        /// <summary>
        /// Returns the role by name or nothing
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Role Get(string name)
        {
            try
            {
                return _db.Roles.Single(t => t.Name == name && t.AppName == _appName);
            }
            catch (Exception)
            {
                return new Role();
            }
        }
        /// <summary>
        /// Returns all the roles order by name
        /// </summary>
        /// <returns></returns>
        public List<Role> Get()
        {
            try
            {
                return _db.Roles.Where(t => t.AppName == _appName).OrderBy(t => t.Name).ToList();
            }
            catch (Exception)
            {

                return new List<Role>();
            }
        }
        /// <summary>
        /// Returns true if the user is in the role
        /// </summary>
        /// <param name="username"></param>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public override bool IsUserInRole(string username, string roleName)
        {
            try
            {
                //get the user from the userName
                var usr = _userSvc.Get(username);
                var role = _db.Roles.Single(t => t.Name == roleName && t.AppName == _appName);
                //see if this user is in the role
                return (from t in _db.UserRoles where t.UserId == usr.UserId && t.RoleId == role.RoleId select t).Any();
            }
            catch (Exception ex)
            {

                Logger.WriteLine(MessageType.Error, ex.Message);
            }
            return false;
        }
        /// <summary>
        /// Returns true if the user is in the role
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public bool IsUserInRole(int userId, string roleName)
        {
            try
            {
                //get the user from the userName
                var usr = _userSvc.Get(userId);
                return IsUserInRole(usr.UserName, roleName);
            }
            catch (Exception ex)
            {
                //not in role
                Logger.WriteLine(MessageType.Information, ex.Message);
            }
            return false;

        }
        /// <summary>
        /// Returns true if the user is in the role
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public bool IsUserInRole(int userId, int roleId)
        {
            try
            {
                return (from t in _db.UserRoles where t.UserId == userId && t.RoleId == roleId select t).Any();
            }
            catch (Exception ex)
            {
                Logger.WriteLine(MessageType.Information, ex.Message);
            }
            return false;

        }
        /// <summary>
        /// Returns the list of roles for the user
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public override string[] GetRolesForUser(string username)
        {
            var usr = _userSvc.Get(username);
            return GetRolesForUser(usr.UserId);
        }
        /// <summary>
        /// Gets the list of user's roles by userid
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public string[] GetRolesForUser(int userId)
        {
            var membership = from t in _db.UserRoles where t.UserId == userId select t;
            if (!membership.Any())
            {
                return null;
            }
            return membership.Select(ur => _db.Roles.Distinct().FirstOrDefault(t => t.RoleId == ur.RoleId).Name).ToArray();
        }

        /// <summary>
        /// Creates a new role with an empty photo
        /// </summary>
        /// <param name="roleName"></param>
        public override void CreateRole(string roleName)
        {
            if (RoleExists(roleName)) { throw new Exception(ErrorConstants.RoleExists); }
            var r = new Role { Photo = string.Empty, Name = roleName, Description = Constants.NewRoleDescription, AppName = _appName };
            _db.Roles.Add(r);
            _db.SaveChanges();
        }
        /// <summary>
        /// Creates a new role if the role name doesn't exist
        /// </summary>
        /// <param name="newRole"></param>
        public void CreateRole(Role newRole)
        {
            if (RoleExists(newRole.Name)) { throw new Exception(ErrorConstants.RoleExists); }
            var r = new Role
            {
                Photo = string.Empty,
                Name = newRole.Name,
                Description = newRole.Description,
                AppName = _appName,
                IsReadOnly = newRole.IsReadOnly
            };
            _db.Roles.Add(r);
            _db.SaveChanges();
        }
        /// <summary>
        /// Creates a role with a photo
        /// </summary>
        /// <param name="role"></param>
        /// <param name="rolePhoto"></param>
        public void CreateRole(Role role, HttpPostedFileBase rolePhoto)
        {
            if (RoleExists(role.Name)) { throw new Exception(ErrorConstants.RoleExists); }
            var rps = new RolePhotoService();
            string picture = rps.SavePhoto(rolePhoto);
            var r = new Role
                        {
                            Photo = picture,
                            Name = role.Name,
                            Description = role.Description,
                            AppName = _appName,
                            IsReadOnly = role.IsReadOnly
                        };
            _db.Roles.Add(r);
            _db.SaveChanges();
        }
        /// <summary>
        /// Updates the role details. If the role is Administrators, the name cannot change.
        /// </summary>
        /// <param name="updatedRole"></param>
        /// <param name="rolePhoto"></param>
        public void UpdateRole(Role updatedRole, HttpPostedFileBase rolePhoto)
        {
            //get the role
            Role rol = _db.Roles.Single(t => t.RoleId == updatedRole.RoleId);
            var rps = new RolePhotoService();
            string picture = rps.SavePhoto(rolePhoto);
            rps.DeletePhoto(rol.Photo);//delete the old photo
            rol.Photo = picture;

            if (rol.Name.ToUpper() != updatedRole.Name.ToUpper())
            {
                if (rol.IsReadOnly)
                {
                    throw new Exception(ErrorConstants.CannotChangeReadonlyRoles);
                }
            }
            rol.Name = updatedRole.Name;
            rol.AppName = _appName;
            rol.Description = updatedRole.Description;
            _db.Entry(rol).State = (System.Data.Entity.EntityState)EntityState.Modified;
            _db.SaveChanges();
        }
        /// <summary>
        /// Tries to delete a role, if the role is populated may throw an error if ThrowErrorOnDeletingPopulatedRoles is set in webconfig
        /// </summary>
        /// <param name="roleName"></param>
        /// <param name="throwOnPopulatedRole"></param>
        /// <returns></returns>
        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            var role = _db.Roles.Single(t => t.Name == roleName && t.AppName == _appName);
            if (role.IsReadOnly)
            {
                throw new Exception(ErrorConstants.CannotChangeReadonlyRoles);
            }
            var membership = from t in _db.UserRoles where t.RoleId == role.RoleId select t;
            if (membership.Any())
            {
                if (throwOnPopulatedRole)
                {
                    throw new Exception(ErrorConstants.RoleIsPopulated);
                }
            }
            //delete the role and the membership
            _db.Roles.Remove(role);
            //delete the photo for the role
            if (!string.IsNullOrEmpty(role.Photo))
            {
                var rps = new RolePhotoService();
                rps.DeletePhoto(role.Photo);
            }
            foreach (var userRole in membership)
            {
                _db.UserRoles.Remove(userRole);
            }
            _db.SaveChanges();
            return true;
        }
        /// <summary>
        ///  Tries to delete a role, if the role is populated may throw an error if ThrowErrorOnDeletingPopulatedRoles is set in webconfig
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="throwOnPopulatedRole"></param>
        /// <returns></returns>
        public bool DeleteRole(int roleId, bool throwOnPopulatedRole)
        {
            var role = _db.Roles.Single(t => t.RoleId == roleId && t.AppName == _appName);
            return DeleteRole(role.Name, throwOnPopulatedRole);
        }
        /// <summary>
        /// returns true if the role name is in use
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public override bool RoleExists(string roleName)
        {
            var rols = from t in _db.Roles where t.AppName == _appName && t.Name == roleName select t;
            return rols.Any();
        }
        /// <summary>
        /// Adds all users not already in the role to the role with the roleid
        /// </summary>
        /// <param name="roleId"></param>
        public void AddAllUsersToRole(int roleId)
        {
            var notin = GetUsersNotInRole(roleId);
            foreach (var usr in notin)
            {
                AddUserToRole(usr.UserId, roleId);
            }
        }
        /// <summary>
        /// Adds the list of users to the list of roles
        /// </summary>
        /// <param name="usernames"></param>
        /// <param name="roleNames"></param>
        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            var roles = roleNames.Select(roelName => _db.Roles.Single(t => t.Name == roelName && t.AppName == _appName)).ToList();
            var users = usernames.Select(username => _db.Users.Single(t => t.UserName == username && t.AppName == _appName)).ToList();
            foreach (User u in users)
            {
                var usr = u;
                foreach (Role r in roles)
                {
                    var rol = r;
                    var membership = from t in _db.UserRoles
                                     where t.RoleId == rol.RoleId && t.UserId == usr.UserId
                                     select t;
                    if (!membership.Any())
                    {
                        var newMember = new UserRole { UserId = u.UserId, RoleId = r.RoleId };
                        _db.UserRoles.Add(newMember);
                    }
                }
                _db.SaveChanges();
            }
        }
        /// <summary>
        /// If the user is not in the role, the user is added
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="roleId"></param>
        public void AddUserToRole(int userId, int roleId)
        {
            if (IsUserInRole(userId, roleId))
            {
                return;
            }
            var rol = new UserRole { UserId = userId, RoleId = roleId };
            _db.UserRoles.Add(rol);
            _db.SaveChanges();
        }
        /// <summary>
        /// Adds the user to the Users role if they are not already a member
        /// </summary>
        /// <param name="userId"></param>
        public void AddToUsersRole(int userId)
        {
            //get the users role
            var usersRole = Get(Constants.UsersRole);
            AddUserToRole(userId, usersRole.RoleId);
        }
        /// <summary>
        /// Removes the list of users from the list of roles, Will not remove the admin frmo the admin role
        /// </summary>
        /// <param name="usernames"></param>
        /// <param name="roleNames"></param>
        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            if (usernames == null || roleNames == null)
            {
                return;
            }
            var roles = roleNames.Select(roleName => _db.Roles.Single(t => t.Name == roleName && t.AppName == _appName)).ToList();
            var users = usernames.Select(username => _db.Users.Single(t => t.UserName == username && t.AppName == _appName)).ToList();

            foreach (var u in users)
            {
                foreach (var r in roles)
                {
                    if (r.IsReadOnly && u.IsReadOnly)
                    {
                        //skip this user, they are readonly and the role is read only
                        continue;
                    }
                    RemoveUserFromRole(u.UserId, r.RoleId);
                }
            }
        }
        /// <summary>
        /// Finds the member role record and removes it
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="roleId"></param>
        public void RemoveUserFromRole(int userId, int roleId)
        {
            //make sure they are not admin
            var membership = from t in _db.UserRoles
                             where t.RoleId == roleId && t.UserId == userId
                             select t;

            foreach (var m in membership)
            {
                _db.UserRoles.Remove(m);
            }
            _db.SaveChanges();
        }
        /// <summary>
        /// Returns true if this role is readonly
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public bool IsRoleReadOnly(int roleId)
        {
            var role = Get(roleId);
            return role.IsReadOnly;
        }
        /// <summary>
        /// Removes every user in the role, leaves readonly users in readonly roles
        /// </summary>
        /// <param name="roleId"></param>
        public void RemoveAllUsersFromRole(int roleId)
        {
            //get the users for the role
            var role = _db.Roles.Single(t => t.RoleId == roleId && t.AppName == _appName);
            var users = GetUsersInRole(role.RoleId);
            foreach (var usr in users)
            {
                if (usr.IsReadOnly && role.IsReadOnly)
                {
                    continue; //user and role are readonly
                }
                RemoveUserFromRole(usr.UserId, roleId);
            }
        }
        /// <summary>
        /// Remvoes all user from all roles, leaves readonly users in readonly roles alone
        /// </summary>
        /// <param name="userId"></param>
        public void RemoveUserFromAllRoles(int userId)
        {
            //get the roles for the user
            var rols = GetRolesForUser(userId);
            var svc = new UserService(_appName);
            var userName = svc.Get(userId).UserName;
            string[] usernames = { userName };
            RemoveUsersFromRoles(usernames, rols);
        }
        /// <summary>
        /// Returns all the user names that are in the role
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public override string[] GetUsersInRole(string roleName)
        {
            var roles = _db.Roles.Single(usrRole => usrRole.Name == roleName && usrRole.AppName == _appName);
            var memebership = from t in _db.UserRoles where t.RoleId == roles.RoleId select t;
            //get all the users if not null
            if (!memebership.Any())
            {
                return new string[0];
            }
            //get all the users 
            var usrs = memebership.Select(b => _db.Users.FirstOrDefault(t => t.UserId == b.UserId).UserName).ToList();
            return usrs.Distinct().ToArray();
        }
        /// <summary>
        /// Returns all the roles ordered by name
        /// </summary>
        /// <returns></returns>
        public override string[] GetAllRoles()
        {
            var j = _db.Roles.Where(t => t.AppName == _appName).Distinct().OrderBy(f => f.Name);
            return j.Select(t => t.Name).ToArray();
        }
        /// <summary>
        /// Returns all the roles by name with paging
        /// </summary>
        /// <param name="pager"></param>
        /// <returns></returns>
        public List<Role> GetAll(Pager pager)
        {
            return _db.Roles.Where(t => t.AppName == _appName).Skip((pager.Page - 1) * pager.Perpage).Take(pager.Perpage).OrderBy(f => f.Name).ToList();
        }

        /// <summary>
        /// Returns all the users in the role
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public List<User> GetUsersInRole(int roleId)
        {
            //join the users and the roles table with userroles
            //select the users fromt he users table not in the join
            var q = from c in _db.UserRoles
                    join o in _db.Roles on c.RoleId equals roleId
                    join i in _db.Users on c.UserId equals i.UserId
                    select i;
            return q.Distinct().ToList();
        }
        /// <summary>
        /// Finds members of the role that have matched the term
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="searchTerm"></param>
        /// <returns></returns>
        public List<User> Search(int roleId, string searchTerm)
        {
            var usrs = GetUsersInRole(roleId);
            var result = from c in usrs
                         where (c.UserName ?? "").Contains(searchTerm) ||
                             (c.FirstName ?? "").Contains(searchTerm) ||
                             (c.LastName ?? "").Contains(searchTerm)
                         select c;
            return result.Distinct().OrderBy(f => f.UserName).ToList();
        }
        /// <summary>
        /// Returns just the count of members
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public int GetMemberCount(int roleId)
        {
            return GetUsersInRole(roleId).Count();
        }
        /// <summary>
        /// Returns the users in the role with paging
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="pager"></param>
        /// <returns></returns>
        public List<User> GetUsersInRole(int roleId, Pager pager)
        {
            //join the users and the roles table with userroles
            //select the users fromt he users table not in the join
            var q = from c in _db.UserRoles
                    join o in _db.Roles on c.RoleId equals roleId
                    join i in _db.Users on c.UserId equals i.UserId
                    select i;
            return q.Distinct().OrderBy(f => f.UserName).Skip((pager.Page - 1) * pager.Perpage).Take(pager.Perpage).ToList();
        }
        /// <summary>
        /// Returns a list of users that are NOT in the specified role.
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public List<User> GetUsersNotInRole(string roleName)
        {
            //join the users and the roles table with userroles
            //select the users fromt he users table not in the join
            var roles = _db.Roles.Single(usrRole => usrRole.Name == roleName && usrRole.AppName == _appName);
            return GetUsersNotInRole(roles.RoleId);
        }

        /// <summary>
        /// Pages the list of users not in the role already found
        /// </summary>
        /// <param name="users"> </param>
        /// <param name="pager"></param>
        /// <returns></returns>
        public List<User> GetUsersPaged(List<User> users, Pager pager)
        {
            return users.OrderBy(f => f.UserName).Skip((pager.Page - 1) * pager.Perpage).Take(pager.Perpage).ToList();
        }
        /// <summary>
        /// Returns the users not in the role
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public List<User> GetUsersNotInRole(int roleId)
        {
            //join the users and the roles table with userroles
            //select the users fromt he users table not in the join             
            var q = from c in _db.UserRoles
                    join o in _db.Roles on c.RoleId equals roleId
                    join i in _db.Users on c.UserId equals i.UserId
                    select i;
            return _db.Users.Except(q).ToList();
        }
        /// <summary>
        /// Returns a list of users that are NOT in the specified role with the provided search username.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public IEnumerable<object> FindUsersNotInRole(string userName, int roleId)
        {
            var notin = from i in _db.Users
                        where !_db.UserRoles.Any(userrole => userrole.UserId == i.UserId && userrole.RoleId == roleId)
                        && i.AppName == _appName
                        select i;
            return from c in notin where c.UserName.Contains(userName) select new { id = c.UserId, label = c.UserName };
        }

        /// <summary>
        /// Returns a list of users NOT in the role with paging.
        /// </summary>
        /// <param name="roleId"> </param>
        /// <param name="pager"></param>
        /// <returns></returns>
        public List<User> GetUsersNotInRole(int roleId, Pager pager)
        {
            var notin = GetUsersNotInRole(roleId);
            return notin.OrderBy(f => f.UserName).Skip((pager.Page - 1) * pager.Perpage).Take(pager.Perpage).ToList();
        }
        /// <summary>
        /// Returns all the usernames that are in the role.
        /// </summary>
        /// <param name="roleName"></param>
        /// <param name="usernameToMatch"></param>
        /// <returns></returns>
        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            //get the role for this rolename
            var roles = _db.Roles.Single(usrRole => usrRole.Name == roleName && usrRole.AppName == _appName);
            var q = from c in _db.UserRoles
                    join o in _db.Roles on c.RoleId equals roles.RoleId
                    join i in _db.Users on c.UserId equals i.UserId
                    select i;
            return q.Select(t => t.UserName).ToArray();
        }

        private string _appName = ConfigurationManager.AppSettings[Constants.ApplicationNameKey];
        /// <summary>
        /// Future use for multiple applications with one membership class
        /// </summary>
        public override string ApplicationName
        {
            get { return _appName; }
            set { _appName = value; }
        }
        /// <summary>
        /// Creates a new application with a deafult admin and default admin role.
        /// </summary>
        /// <param name="newName"></param>
        /// <returns></returns>
        public string CreateNewApplication(string newName)
        {
            if (string.IsNullOrEmpty(newName))
            {
                throw new Exception(ErrorConstants.AppNameNotValid);
            }
            var validator = new Regex(Constants.ValidEntries);
            if (!validator.IsMatch(newName))
            {
                throw new Exception(ErrorConstants.EntryInvalid);
            }
            if (newName.Contains(" "))
            {
                throw new Exception(ErrorConstants.EntryInvalid);
            }
            //make sure the appname is not in use
            var currentNames = GetAppNames();
            if (currentNames.Contains(newName))
            {
                return ErrorConstants.AppNameExists;
            }
            //its good so create it
            try
            {
                return CreateNewAdmin(newName);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public string CreateNewAdmin(string newAppName)
        {
            string results = string.Empty;
            try
            {
                //Create the admin   
                var admin = UserService.CreateUser(newAppName, Constants.Admin, Constants.Admin, string.Empty, Constants.AdminPassword,
                                      Constants.Admin + ConfigurationManager.AppSettings[Constants.DomainEmailSuffixKey],
                                       true);
                _db.Users.Add(admin);
                _db.SaveChanges();
                _db.Roles.Add(EfRoleProvider.CreateAdminRole(newAppName));  //Create the Administrators Role
                _db.Roles.Add(EfRoleProvider.CreateUsersRole(newAppName));  //Create the users role
                _db.SaveChanges();
                //add the admin
                var adminusers = from p in _db.Users where p.UserName == Constants.Admin && p.AppName == newAppName select p;
                var newroles = from p in _db.Roles where p.AppName == newAppName select p;
                foreach (User usr2 in adminusers)
                {
                    foreach (Role r in newroles)
                    {
                        var newUserrole = new UserRole
                        {
                            RoleId = r.RoleId,
                            UserId = usr2.UserId,
                        };
                        _db.UserRoles.Add(newUserrole);
                        Logger.WriteLine(MessageType.Information, Constants.Adding + usr2.UserName + Constants.To + r.Name);
                    }
                }
                _db.SaveChanges();
                results = @"Created new Application " + newAppName + ". Created readonly user " + Constants.Admin + ", password is " + Constants.AdminPassword + ". Created readonly role " + Constants.AdministratorsRole;
                return results;
            }
            catch (Exception ex)
            {
                results += ex.Message;
                return results;
            }
        }
        /// <summary>
        /// Returns true if the user is in the Administrator's role of the main application
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool IsUserSuperAdmin(int userId)
        {
            try
            {
                //here we want to see if this user is in the administrators role of the main application name
                var usr = _db.Users.Single(t => t.UserId == userId);
                //get the admin role of the main appname
                string mainApp = ConfigurationManager.AppSettings[Constants.ApplicationNameKey].ToString(CultureInfo.InvariantCulture);
                var rol = _db.Roles.Single(t => t.Name == Constants.AdministratorsRole && t.AppName == mainApp);
                return IsUserInRole(usr.UserId, rol.RoleId);
            }
            catch (Exception)
            {
                return false;
            }

        }
    }
}
