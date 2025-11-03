<%@ WebHandler Language="C#" Class="Handler2" %>

using System;
using System.Web;
using System.Data.SqlClient;
public class Handler2 : IHttpHandler {

    clsFunctions fun = new clsFunctions();
    public void ProcessRequest (HttpContext context)
    {
        string connStr = fun.Connection();
        SqlConnection con = new SqlConnection(connStr);
        con.Open();
        string sql = "Select LogoImage from tblCompany_master where CompId=@CompId";
        SqlCommand cmd = new SqlCommand(sql, con);
        cmd.Parameters.Add("@CompId", System.Data.SqlDbType.Int).Value = context.Request.QueryString["CompId"];
        cmd.Prepare();
        SqlDataReader dr = cmd.ExecuteReader();
        dr.Read();
        //context.Response.ContentType = dr["Image_Type"].ToString();
        context.Response.BinaryWrite((byte[])dr["LogoImage"]);
        dr.Close();
        con.Close(); 
    }
 
    public bool IsReusable {
        get {
            return false;
        }
    }

}