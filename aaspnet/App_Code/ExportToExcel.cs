using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.IO;

/// <summary>
/// Summary description for ExportToExcel
/// </summary>
public class ExportToExcel
{
   
	public ExportToExcel()
	{
		//
		// TODO: Add constructor logic here
		//
	}

    public void ExportDataToExcel(DataTable dt1,string FileName)
    {

        try
        {            
            if (dt1 == null)
            {
                throw new Exception("No Records to Export");
            }
            else
            {
                string Path = "D:\\ImportExcelFromDatabase\\"+FileName+"_" + DateTime.Now.Day.ToString() + "_" + DateTime.Now.Month.ToString() + ".xls";
                FileInfo FI = new FileInfo(Path);
                StringWriter stringWriter = new StringWriter();
                HtmlTextWriter htmlWrite = new HtmlTextWriter(stringWriter);
                System.Web.UI.WebControls.DataGrid DataGrd = new System.Web.UI.WebControls.DataGrid();
                DataGrd.DataSource = dt1;
                DataGrd.DataBind();
                DataGrd.RenderControl(htmlWrite);
                string directory = Path.Substring(0, Path.LastIndexOf("\\"));// GetDirectory(Path);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                System.IO.StreamWriter vw = new System.IO.StreamWriter(Path, true);
                stringWriter.ToString().Normalize();
                vw.Write(stringWriter.ToString());
                vw.Flush();
                vw.Close();
                WriteAttachment(FI.Name, "application/vnd.ms-excel", stringWriter.ToString());
            }
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message); 
        }  
    }


    public static void WriteAttachment(string FileName, string FileType, string content)
    {
        try
        {
            HttpResponse Response = System.Web.HttpContext.Current.Response;
            Response.ClearHeaders();
            Response.AppendHeader("Content-Disposition", "attachment; filename=" + FileName);
            Response.ContentType = FileType;
            Response.Write(content);
            Response.End();
        }
        catch (Exception ex)
        {
        }
    }

}
