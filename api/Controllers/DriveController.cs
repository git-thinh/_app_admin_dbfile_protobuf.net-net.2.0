/*
 * Copyright (c) 2012 Google Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except
 * in compliance with the License. You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software distributed under the License
 * is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
 * or implied. See the License for the specific language governing permissions and limitations under
 * the License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

using Google.Apis.Drive.v2;
using Google.Apis.Authentication;

namespace api
{
    public class DriveController : Controller
    {
        //
        // GET: /Drive

        public ActionResult Demo()
        {
            string title = DateTime.Now.ToString("yyyyMMdd-HHmmssfff");
            string description = Guid.NewGuid().ToString().ToUpper();
            string mimeType = "text/plain";
            string content = Guid.NewGuid().ToString().ToUpper();

            IAuthenticator authenticator = Session["authenticator"] as IAuthenticator;
            DriveService service = Session["service"] as DriveService;

            if (authenticator == null || service == null)
            {
                // redirect user to authentication
            }

            Google.Apis.Drive.v2.Data.File file = Utils.InsertResource(service, authenticator, title, description, mimeType, content);

            ViewBag.WebContentLink = file.WebContentLink;

            return RedirectToAction("Index", "Drive");
        }

        public ActionResult Index(string state, string code)
        {
            try
            {
                IAuthenticator authenticator = Utils.GetCredentials(code, state);
                // Store the authenticator and the authorized service in session
                Session["authenticator"] = authenticator;
                Session["service"] = Utils.BuildService(authenticator);
            }
            catch (CodeExchangeException)
            {
                if (Session["service"] == null || Session["authenticator"] == null)
                {
                    Response.Redirect(Utils.GetAuthorizationUrl("", state));
                }
            }
            catch (NoRefreshTokenException e)
            {
                Response.Redirect(e.AuthorizationUrl);
            }

            DriveState driveState = new DriveState();

            if (!string.IsNullOrEmpty(state))
            {
                JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                driveState = jsonSerializer.Deserialize<DriveState>(state);
            }

            if (driveState.action == "open")
            {
                return OpenWith(driveState);
            }
            else
            {
                return CreateNew(driveState);
            }
        }

        private ActionResult OpenWith(DriveState state)
        {
            ViewBag.FileIds = state.ids;
            return View();
        }

        private ActionResult CreateNew(DriveState state)
        {
            ViewBag.FileIds = new string[] {""};
            return View();
        }
    }
}
