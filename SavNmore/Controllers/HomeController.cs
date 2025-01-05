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

using System.Configuration;
using System.Web.Mvc;
using savnmore.Models;
using savnmore.Services;

namespace savnmore.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (HttpContext.Session[Constants.SessionAppNameKey] == null)
            {
                HttpContext.Session[Constants.SessionAppNameKey] =
                ConfigurationManager.AppSettings[Constants.ApplicationNameKey];
            }
            ViewBag.AppName = HttpContext.Session[Constants.SessionAppNameKey].ToString();
            ViewBag.Message = "Welcome!";
            ViewBag.IsMobile = false;
            if (HttpContext.Session["isMobile"] != null)
            {
               ViewBag.IsMobile = (bool)HttpContext.Session["isMobile"];
            }          

            return View();
        }
        public ActionResult Mobile()
        {
            HttpContext.Session["isMobile"] = true;
           
            return RedirectToAction("Index");
        }
        public ActionResult Desktop()
        {
            HttpContext.Session["isMobile"] = false;
             
            return RedirectToAction("Index");
        }
        public ActionResult About()
        {
            return View();
        }
        public ActionResult PrivacyPolicy()
        {
            return View();
        }
        public ActionResult Contact()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Contact(ContactForm model)
        {
            if (ModelState.IsValid)
            {
                //send an email with the details
                //redirect to message sent
                EmailService.SendContactFormEmail(model);
                return RedirectToAction("ContactConfirmation", "Home");
            }
            return View(model);
        }
        public ViewResult ContactConfirmation()
        {
            ViewBag.MessageResult = @"Thank you for your inquiry,
                    Your concern will be addressed shortly.";
            return View();
        }
    }
}
