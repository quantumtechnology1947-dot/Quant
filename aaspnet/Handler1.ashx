<%@ WebHandler Language="C#" Class="Handler1" %>

using System;
using System.Web;
using System.Data.SqlClient;
using System.Data;
using System.IO;
public class Handler1 : IHttpHandler {
    clsFunctions fun = new clsFunctions();
    public void ProcessRequest (HttpContext context) 
    {
        string strConn = fun.Connection();
        SqlConnection con = new SqlConnection(strConn);
        try
        {

            SqlCommand cmd = new SqlCommand();            
            cmd.CommandText = "sp_getImageStaff";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Connection = con;

            SqlParameter ImageID = new SqlParameter("@EmpId", SqlDbType.VarChar, 20);
            ImageID.Value = context.Request.QueryString["EmpId"].ToString();
            
            SqlParameter CompId = new SqlParameter("@CompId", SqlDbType.VarChar, 20);
           CompId.Value = context.Request.QueryString["CompId"].ToString();
            cmd.Parameters.Add(ImageID);
            cmd.Parameters.Add(CompId);
            con.Open();
            SqlDataReader sdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            if (sdr.Read())
            {
                context.Response.ContentType = sdr["PhotoContentType"].ToString();
                byte[] imgByte = (byte[])sdr["PhotoData"];
                MemoryStream memoryStream = new MemoryStream();
                memoryStream.Write(imgByte, 0, imgByte.Length);
                System.Drawing.Image imagen = System.Drawing.Image.FromStream(memoryStream);
                context.Response.BinaryWrite(imgByte);
            }
        }
        catch (Exception ex)
        {
            
        }
        finally
        {
            con.Close();
        }
    }
 
    public bool IsReusable {
        get
        {
            return false;
        }
    }
}