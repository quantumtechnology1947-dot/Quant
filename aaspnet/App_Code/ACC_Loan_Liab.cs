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
using System.Data.SqlClient;

/// <summary>
/// Summary description for ACC_Loan_Liab
/// </summary>
public class ACC_Loan_Liab
{
    clsFunctions fun = new clsFunctions();
    DataTable dt = new DataTable();
    SqlConnection con;
    string connStr = "";
    DataRow dr;
	public ACC_Loan_Liab()
	{
        connStr = fun.Connection();
        con = new SqlConnection(connStr);
        dt.Columns.Add(new System.Data.DataColumn("Id", typeof(Int32)));
        dt.Columns.Add(new System.Data.DataColumn("Particulars", typeof(string)));
        dt.Columns.Add(new System.Data.DataColumn("TotCrAmt", typeof(double)));
        dt.Columns.Add(new System.Data.DataColumn("TotDrAmt", typeof(double)));       
	}

    public DataTable TotFillPart(string StrSql)
    {        
        try
        {
            con.Open();           
            SqlCommand Cmdgrid = new SqlCommand(StrSql, con);
            SqlDataReader rdr = Cmdgrid.ExecuteReader();
            while (rdr.Read())
            {
                dr = dt.NewRow();
                dr[0] = rdr["Id"].ToString();               
                dr[1] = rdr["Particulars"].ToString();
                dr[2] = Math.Round(Convert.ToDouble(rdr["loan"]), 2);
                dr[3] = 0;              
                dt.Rows.Add(dr);
                dt.AcceptChanges();
            }

        }
        catch (Exception ex) { }
        finally
        {
            con.Close();
        }
        return dt;

    }
    public DataTable TotFillPart(int CompId, int FinId,int MId)
    {
       
        try
        {
            con.Open();
            string StrSql = "select CreditAmt As loan,tblAcc_LoanDetails.Particulars,tblAcc_LoanDetails.Id from tblAcc_LoanDetails inner join tblAcc_LoanMaster on tblAcc_LoanMaster.Id=tblAcc_LoanDetails.MId And CompId=" + CompId + " AND FinYearId<=" + FinId + " And MId=" + MId + "";
            SqlCommand Cmdgrid = new SqlCommand(StrSql, con);
            SqlDataReader rdr = Cmdgrid.ExecuteReader();
            while (rdr.Read())
            {
                dr = dt.NewRow();              
                dr[1] = rdr["Particulars"].ToString();
                dr[2] = Math.Round(Convert.ToDouble(rdr["loan"]), 2);
                dr[3] = 0;
                dr[0] = Convert.ToInt32(rdr["Id"]);
                dr[4] = CompId;
                dt.Rows.Add(dr);
                dt.AcceptChanges();
            }

        }
        catch (Exception ex) { }
        finally
        {
            con.Close();
        }
        return dt;

    }
}
