using Google.Apis.Authentication;
using Google.Apis.Drive.v2;
using System;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace api
{  
    public class APIController : Controller
    {
        public ActionResult Upload()
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

            string WebContentLink = file.WebContentLink;

            return RedirectToAction("WebContentLink", "API", new { link = WebContentLink });
        }

        public ActionResult WebContentLink(string link)
        {
            string htm = string.Format("{0} <hr> Link: <a href='{1}' target='_blank'>{1}</a>", DateTime.Now.ToString(), link);
            return Content(htm, "text/html");
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
            return Content(DateTime.Now.ToString() + "<hr>" + state.ids, "text/html");
        }

        private ActionResult CreateNew(DriveState state)
        {
            return Content(DateTime.Now.ToString() + "<hr>", "text/html");
        }
    }


}
