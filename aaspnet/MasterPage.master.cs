using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Xml.Linq;
using System.Data.SqlClient;
using System.Web.SessionState;
using System.Net;
public partial class MasterPage : System.Web.UI.MasterPage
{
    clsFunctions fun = new clsFunctions();
    string ServerIp = "";



    protected override void OnPreRender(EventArgs e)
    {
        // Check if required session variables exist before accessing them
        if (Session["compid"] == null || Session["username"] == null)
        {
            return; // Exit if session variables don't exist
        }

        string connStr = fun.Connection();
        SqlConnection con = new SqlConnection(connStr);
        try
        {
            base.OnPreRender(e);

            int CompID_value = Convert.ToInt32(Session["compid"]);
            string EmpId_value = Convert.ToString(Session["username"]);

            con.Open();
            string getIp = fun.select("ServerIp", "tblCompany_master", "CompId='" + CompID_value + "'");
            SqlCommand cdIp = new SqlCommand(getIp, con);
            SqlDataAdapter daIp = new SqlDataAdapter(cdIp);
            DataSet dssIp = new DataSet();
            daIp.Fill(dssIp, "tblCompany_master");
            ServerIp = dssIp.Tables[0].Rows[0][0].ToString();

            // this.head.Controls.Add(new LiteralControl(String.Format("<meta http-equiv='refresh'content='{0};url={1}'>",Session.Timeout*60,"http://pc4/newerp/Login.aspx",ServerIp)));
            this.head.Controls.Add(new LiteralControl(String.Format("<meta http-equiv='refresh'content='{0};url={1}'>", SessionTimeOutWithin * 60, PageToSentAfterSessionTimeOut, ServerIp)));


        }
        catch (Exception ex)
        {
        }
        finally
        {
            con.Close();
        }
    }

    public string PageToSentAfterSessionTimeOut
    {
        get
        {
            string stackoverflow = HttpContext.Current.Request.Url.Host;
            IPAddress[] addresslist = Dns.GetHostAddresses(stackoverflow);

            return "http://" + addresslist[0].ToString() + "/newerp";
        }
    }


    public int SessionTimeOutWithin
    {
        get { return Session.Timeout; }
    }

    public void initNotify(string StrSplash)
    {
        // Only do this on the first call to the page
        if (!IsPostBack)
        {
            //Register loadingNotifier.js for showing the Progress Bar
            Response.Write(string.Format(@"<script type='text/javascript' src='Javascript/loadingNotifier.js'></script>
              <script language='javascript' type='text/javascript'>
              initLoader('{0}');
              </script>", StrSplash));
            // Send it to the client
            Response.Flush();

        }

    }
    public void Notify(string strPercent, string strMessage)
    {
        // Only do this on the first call to the page
        if (!IsPostBack)
        {
            //Update the Progress bar

            Response.Write(string.Format("<script language='javascript' type='text/javascript'>setProgress({0},'{1}'); </script>", strPercent, strMessage));
            Response.Flush();

        }

    }




    protected void Page_Init(object sender, EventArgs e)
    {
        if (Context.Session != null)
        {
            if (Session.IsNewSession)
            {
                HttpCookie newSessionIdCookie = Request.Cookies["ASP.NET_SessionId"];
                if (newSessionIdCookie != null)
                {

                    string newSessionIdCookieValue = newSessionIdCookie.Value;
                    //newSessionIdCookieValue.Remove();
                    if (newSessionIdCookieValue != string.Empty)
                    {
                        // This means Session was timed Out and New Session was started
                        Response.Redirect("~/Login.aspx");
                    }
                }
            }
        }
    }

    protected string dypath = string.Empty;
    protected void Page_Load(object sender, EventArgs e)
    {
        // Check if required session variables exist - redirect to login if not
        if (Session["username"] == null || Session["compid"] == null || Session["finyear"] == null)
        {
            Response.Redirect("~/Login.aspx");
            return;
        }

        string connStr = fun.Connection();
        SqlConnection con = new SqlConnection(connStr);
        New.Visible = false;
        Edit.Visible = false;
        Print.Visible = false;
        Delete.Visible = false;
        int CompID_value = Convert.ToInt32(Session["compid"]);
        int FinYearID_value = Convert.ToInt32(Session["finyear"]);
        string EmpId_value = Convert.ToString(Session["username"]);
        //  if (Menu1.Items.Count>10)
        //  {
        //    TextBox1.Text = Men.ToString();
        // Menu1.Controls.Add(new LiteralControl("<br />"));
        //  }      

        
         TextBox1.Text=Menu1.MaximumDynamicDisplayLevels.ToString();



        try
        {
            con.Open();

            string getFinYrs = fun.select("FinYear", "tblFinancial_master", "FinYearId='" + FinYearID_value + "'");
            SqlCommand cd = new SqlCommand(getFinYrs, con);
            SqlDataAdapter da = new SqlDataAdapter(cd);
            DataSet dss = new DataSet();
            da.Fill(dss);

            lblFinYrs.Text = dss.Tables[0].Rows[0][0].ToString();

            int ModId_val = 0;
            int SubModId_val = 0;

            if (Request.QueryString["ModId"] != "") { ModId_val = Convert.ToInt32(Request.QueryString["ModId"]); } else { ModId_val = 0; }

            if (Request.QueryString["SubModId"] != "") { SubModId_val = Convert.ToInt32(Request.QueryString["SubModId"]); } else { SubModId_val = 0; }

            if (ModId_val > 0)
            {
                string ModName = fun.select("ModName", "tblModule_Master", "ModId='" + ModId_val + "'");
                SqlCommand cmd9 = new SqlCommand(ModName, con);
                SqlDataAdapter Da9 = new SqlDataAdapter(cmd9);
                DataSet ds1 = new DataSet();
                Da9.Fill(ds1, "tblModule_Master");
                lblModName.Text = ds1.Tables[0].Rows[0]["ModName"].ToString();
            }

            int[] Access = fun.AcessMaster(CompID_value, FinYearID_value, EmpId_value, ModId_val, SubModId_val);

            for (int j = 0; j < Access.Count(); j++)
            {

                DataSet ds = new DataSet();
                string defComp = fun.select("LinkPage", "tblSubModLink_Master", "SubModId=" + SubModId_val + "and Access=" + Access[j] + "");
                SqlCommand cmd = new SqlCommand(defComp, con);
                SqlDataAdapter Da = new SqlDataAdapter(cmd);
                Da.Fill(ds, "tblSubModLink_Master");
                if (ds.Tables[0].Rows.Count > 0)
                {
                    string linkPage = Convert.ToString(ds.Tables[0].Rows[0]["LinkPage"]);
                    if (Access[j] == 1)
                    {
                        New.Visible = true;
                        New.Text = fun.link(linkPage + "?ModId=" + ModId_val + "&SubModId=" + SubModId_val, "New");
                    }
                    if (Access[j] == 2)
                    {
                        Edit.Visible = true;
                        Edit.Text = fun.link(linkPage + "?ModId=" + ModId_val + "&SubModId=" + SubModId_val, "Edit");
                    }
                    if (Access[j] == 3)
                    {
                        Delete.Visible = true;
                        Delete.Text = fun.link(linkPage + "?ModId=" + ModId_val + "&SubModId=" + SubModId_val, "Delete");
                    }
                    if (Access[j] == 4)
                    {
                        Print.Visible = true;
                        Print.Text = fun.link(linkPage + "?ModId=" + ModId_val + "&SubModId=" + SubModId_val, "Print");
                    }
                }

            }

        }

        catch (Exception ex)
        {
        }
        finally
        {
            con.Close();
        }


        try
        {
            con.Open();
            dypath = this.TemplateSourceDirectory;
            this.Page.ClientScript.RegisterClientScriptInclude(this.GetType(), "jscript", this.ResolveClientUrl("~/Javascript/JScript.js"));
            DataSet ds = new DataSet();
            DataSet ds2 = new DataSet();
            string defComp = fun.select("*", "tblCompany_master", "DefaultComp=1");
            string defComp2 = fun.select("EmployeeName", "tblHR_OfficeStaff", "EmpId='" + EmpId_value + "'");
            SqlCommand cmd = new SqlCommand(defComp, con);
            SqlCommand cmd2 = new SqlCommand(defComp2, con);
            SqlDataAdapter Da = new SqlDataAdapter(cmd);
            SqlDataAdapter Da2 = new SqlDataAdapter(cmd2);
            Da.Fill(ds, "tblCompany_master");
            Da2.Fill(ds2, "tblHR_OfficeStaff");
            Label1.Text = ds.Tables[0].Rows[0]["CompanyName"].ToString();
            lblEmpName.Text = ds2.Tables[0].Rows[0]["EmployeeName"].ToString() + " [" + EmpId_value + "]";
            Session["EmpName"] = lblEmpName.Text;
            GridView1.DataSource = ds;
            GridView1.DataBind();

        }
        catch (Exception ex)
        {

        }
        finally
        {
            con.Close();
        }

    }




    protected void LoginStatus1_LoggedOut(object sender, EventArgs e)
    {
         try
        {
            LinqChatDataContext db = new LinqChatDataContext();

            var loggedInUser = (from l in db.LoggedInUsers
                                where l.UserID == Convert.ToInt32(Session["ChatUserID"])
                                //&& l.RoomID == Convert.ToInt32(lblRoomId.Text)
                                select l).SingleOrDefault();

            db.LoggedInUsers.DeleteOnSubmit(loggedInUser);
            db.SubmitChanges();

            // insert a message that this user has logged out
            //this.InsertMessage("Just logged out! " + DateTime.Now.ToString());

            // clean the session
            Session.RemoveAll();
            Session.Abandon();

            // redirect the user to the login page
           /// Response.Redirect("Login.aspx");

        }

        catch (Exception ex)
        {
        }
    }
    
}
