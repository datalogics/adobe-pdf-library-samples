using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

using System.Configuration;
using System.IO;

using Datalogics.PDFL;

namespace WebApplication
{
    public class Global : System.Web.HttpApplication
    {
        private Library mDLE = null;
        
        void Application_Start(object sender, EventArgs e)
        {   // Code that runs on application startup
            
            //
            // Set/Export (at runtime) DLPath to the system environment variable PATH
            // Use of DLE will NOT work in a secure IIS environment without the following!
            // 
            // See Also: Shadow Copy (as it relates to web hosted solutions like ASP.Net, WCF, etc.)
            //  http://msdn.microsoft.com/en-us/library/ms404279.aspx
            //
            
            string dlpath = ConfigurationManager.AppSettings[@"DLPath"];
            DirectoryInfo dinfo = new DirectoryInfo(dlpath);
            if (!dinfo.Exists)
            {   // (someone forgot to) go to Web.config and set (or fix) DLPath so it refers to a valid location (please read the instructions for this sample in Default.asax.cs)
                throw new DirectoryNotFoundException(dlpath);
            }
            
            string path = String.Concat(Environment.GetEnvironmentVariable(@"PATH"), @";", dlpath);
            Environment.SetEnvironmentVariable(@"PATH", path, EnvironmentVariableTarget.Process);
            
            //
            // Initialize DLE...
            //
            // Library initialization is actually (best employed as) a two phase process. If one inits at web application start/end 
            // some expensive, low level, *one time/per process* only setup occurs. That makes the secondary, high level initialization 
            // at Page_Load time faster. This, in turn, will improve per GET/POST performance.
            //
            
            mDLE = new Library();
        }
        
        void Application_End(object sender, EventArgs e)
        {   //  Code that runs on application shutdown
            if (mDLE != null)
            {
                mDLE.Terminate();
                mDLE.Dispose();
                mDLE = null;
            }
        }
        
        void Application_Error(object sender, EventArgs e)
        {   // Code that runs when an unhandled error occurs
        }

        void Session_Start(object sender, EventArgs e)
        {   // Code that runs when a new session is started
        }
        
        void Session_End(object sender, EventArgs e)
        {   // Code that runs when a session ends. 
            // Note: The Session_End event is raised only when the sessionstate mode
            // is set to InProc in the Web.config file. If session mode is set to StateServer 
            // or SQLServer, the event is not raised.
        }
    }
}
