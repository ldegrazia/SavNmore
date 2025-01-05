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
using System.Data.Entity;
using System.Diagnostics;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using savnmore;
using savnmore.Models;
using savnmore.Services;

namespace savnmore
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
            /***********************************************************************************************/
            //Will Drop and recreate the database if model changes and value is true.
            //if using this in a production system, please remove this check
            /***********************************************************************************************/
            if (Convert.ToBoolean(ConfigurationManager.AppSettings[Constants.DropRecreateDatabaseKey]))
            {
                /*********************************************************************/
                //If you are not using Sql Server Compact, 
                //this check and the driver
                //along with SqlServerCompactInitializer class can be safely removed.
                //To rebuild entirely on model changes with Sql CE, please drop any existing tables.
                /*********************************************************************/
                if (ConfigurationManager.ConnectionStrings[Constants.ConnectionStringKey].ProviderName.Contains(Constants.SqlServerCe))
                {
                    //using SqlServer Compact
                    SqlServerCompactInitializer.SetupEdmMetadataTable();
                }
                Database.SetInitializer(new savnmoreInitializer(ConfigurationManager.AppSettings[Constants.ApplicationNameKey]));
            }
        

        }
        protected void Session_Start(Object sender, EventArgs e)
        {

            //here we have to set up the app name
            Session[Constants.SessionAppNameKey] = ConfigurationManager.AppSettings[Constants.ApplicationNameKey];
            if (HttpContext.Current.Session["ismobile"] == null)
            {
                HttpContext.Current.Session["isMobile"] =  Request.Browser.IsMobileDevice;
            }
        }
        void Session_End(object sender, EventArgs e)
        {
            // Code that runs when a session ends.         
            // Note: The Session_End event is raised only when the sessionstate mode        
            // is set to InProc in the Web.config file. If session mode is set to StateServer  
            // or SQLServer, the event is not raised.        

            try
            {
                if (User.Identity.Name != null)
                {
                    //try to mark user as offline
                    if (Session[Constants.SessionAppNameKey] == null)
                    {
                        var ussvc2 = new UserService(ConfigurationManager.AppSettings[Constants.ApplicationNameKey]);
                        ussvc2.MarkOffline(User.Identity.Name);
                        return;
                    }
                    var ussvc = new UserService(Session[Constants.SessionAppNameKey].ToString());
                    ussvc.MarkOffline(User.Identity.Name);
                }
            }
            catch
            {

                Logger.WriteLine(MessageType.Error, ErrorConstants.CouldNotMarkUserOffline);
            }

        }
    }
}