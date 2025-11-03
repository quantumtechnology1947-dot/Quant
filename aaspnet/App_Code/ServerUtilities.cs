using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Script.Services;

/// <summary>
/// Summary description for ServerUtilities
/// </summary>
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[ScriptService]
public class ServerUtilities : System.Web.Services.WebService
{

    public ServerUtilities () {

        //Uncomment the following line if using designed components 
        //InitializeComponent(); 
    }

    [WebMethod(EnableSession = true)]
    [ScriptMethod]
    public void SetTabIndex(int index)
    {
        Session["TabIndex"] = index.ToString();
    }
    
}

