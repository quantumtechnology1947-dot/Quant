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
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Globalization;
using System.Text.RegularExpressions;
//using System.Security.Cryptography;
using MKB.TimePicker;
using Telerik.Web.UI;
using System.IO;
/// <summary>
/// Summary description for clsFunctions
/// </summary>
 
public class clsFunctions
{    
	public clsFunctions()
	{
		
	}
    public static string GetRandomAlphanumericString(int length)
    {
        const string alphanumericCharacters =
           "0123456789" +
           "ABCDEFGHIJKLMNPQRSTUVWXYZ" +
            "@#$%";
        return GetRandomString(length, alphanumericCharacters);
    }

    public static string GetRandomString(int length, IEnumerable<char> characterSet)
    {
        if (length < 0)
            throw new ArgumentException("length must not be negative", "length");
        if (length > int.MaxValue / 8) // 250 million chars ought to be enough for anybody
            throw new ArgumentException("length is too big", "length");
        if (characterSet == null)
            throw new ArgumentNullException("characterSet");
        var characterArray = characterSet.Distinct().ToArray();
        if (characterArray.Length == 0)
            throw new ArgumentException("characterSet must not be empty", "characterSet");

        var bytes = new byte[length * 8];
        new RNGCryptoServiceProvider().GetBytes(bytes);
        var result = new char[length];
        for (int i = 0; i < length; i++)
        {
            ulong value = BitConverter.ToUInt64(bytes, i * 8);
            result[i] = characterArray[value % (uint)characterArray.Length];
        }
        return new string(result);
    }
    public string EmpCustSupplierNames(int ct, string code, int CompId)
    {
        string connStr = this.Connection();
        SqlConnection con = new SqlConnection(connStr);
        con.Open();
        DataSet DS = new DataSet();
        string name = "";
        switch (ct)
        {
            case 1:
                {
                    string cmdStr = this.select("EmployeeName+'[ '+EmpId+' ]' AS EmployeeName ", "tblHR_OfficeStaff", "CompId='" + CompId + "' AND EmpId='" + code + "' ");
                    SqlCommand cmd = new SqlCommand(cmdStr, con);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    rdr.Read();
                    name = rdr["EmployeeName"].ToString();
                }
                break;

            case 2:
                {
                    string cmdStr = this.select("CustomerName+'[ '+CustomerId+' ]' AS CustomerName", "SD_Cust_master", "CompId='" + CompId + "' AND CustomerId='" + code + "'");
                    SqlCommand cmd = new SqlCommand(cmdStr, con);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    rdr.Read();
                    name = rdr["CustomerName"].ToString();
                }
                break;

            case 3:
                {
                    string cmdStr = this.select("SupplierName+'[ '+SupplierId+' ]' AS SupplierName", "tblMM_Supplier_master", "CompId='" + CompId + "' AND SupplierId='" + code + "'");
                    SqlCommand cmd = new SqlCommand(cmdStr, con);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    rdr.Read();
                    name = rdr["SupplierName"].ToString();
                }
                break;

            case 4:
                {
                    string cmdStr = this.select("Name", "tblACC_Bank", "Id='" + code + "' ");
                    SqlCommand cmd = new SqlCommand(cmdStr, con);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    rdr.Read();
                    name = rdr["Name"].ToString();
                }
                break;
        }
        con.Close();
        return name;
    }


    public double DebitorsOpeningBal(int CompId, string CustomerId)
    {
        string connStr = this.Connection();
        SqlConnection con = new SqlConnection(connStr);
        con.Open();

        string CustId = string.Empty;

        if (CustomerId != "")
        {
            CustId = " AND CustomerId='" + CustomerId + "'";
        }

        string strDebitorsOp = this.select("Sum(OpeningAmt) as Sum_OPAmt", "tblACC_Debitors_Master", "CompId='" + CompId + "'" + CustId + "");
        SqlCommand cmdDebitorsOp = new SqlCommand(strDebitorsOp, con);
        SqlDataReader rdrOp = cmdDebitorsOp.ExecuteReader();
        rdrOp.Read();

        double OpeningAmt = 0;
        if (rdrOp.HasRows == true && rdrOp["Sum_OPAmt"] != DBNull.Value)
        {
            OpeningAmt = Math.Round(Convert.ToDouble(rdrOp["Sum_OPAmt"]), 2);
        }

        return OpeningAmt;

        con.Close();
    }

    public double getDebitorCredit(int CompId, int FinYearId, string CustId)
    {
        string connStr = this.Connection();
        SqlConnection con = new SqlConnection(connStr);
        con.Open();

        double x = 0;

        try
        {
            string ReceivedFrom = string.Empty;

            if (CustId != "")
            {
                ReceivedFrom = " AND ReceivedFrom='" + CustId + "'";
            }

            string StrSql = this.select("Sum(Amount) As Sum_Amt", "tblACC_BankVoucher_Received_Masters", "FinYearId<='" + FinYearId + "'  And  CompId='" + CompId + "' AND ReceiveType='2' " + ReceivedFrom);

            SqlCommand cmdSql = new SqlCommand(StrSql, con);
            SqlDataReader DSSql = cmdSql.ExecuteReader(CommandBehavior.CloseConnection);
            DSSql.Read();

            if (DSSql.HasRows == true)
            {
                x = Convert.ToDouble(DSSql["Sum_Amt"].ToString());
            }
        }
        catch (Exception ex) { }
        finally
        {
            con.Close();
        }

        return x;

    }
   

    //TDS working
    public double ClStk()
    {
        string connStr = this.Connection();
        SqlConnection con = new SqlConnection(connStr);
        con.Open();

        double x = 0;

        try
        {
            string sql = this.select1("*", "tblInv_ClosingStck Order by Id Desc");
            SqlCommand cmd = new SqlCommand(sql, con);
            SqlDataReader sdr = cmd.ExecuteReader();
            sdr.Read();

            if (sdr.HasRows == true)
            {
                x = Convert.ToDouble(sdr["ClStock"].ToString());
            }

        }
        catch (Exception st)
        {
        }
        finally
        {
            con.Close();
        }

        return x;
    }

    public double Check_TDSAmt(int CompId, int FinYearId, string GetSupCode, int TDSCode)
    {
        double CalCulatedAmt = 0;
        string connStr = this.Connection();
        SqlConnection con = new SqlConnection(connStr);
        con.Open();

        try
        {
            DataTable dt = new DataTable();

            dt.Columns.Add(new System.Data.DataColumn("SysDate", typeof(string)));//0
            dt.Columns.Add(new System.Data.DataColumn("PVEVId", typeof(int)));//1
            dt.Columns.Add(new System.Data.DataColumn("Discount", typeof(double)));//2
            dt.Columns.Add(new System.Data.DataColumn("DiscountType", typeof(int)));//3
            dt.Columns.Add(new System.Data.DataColumn("DebitAmt", typeof(double)));//4
            dt.Columns.Add(new System.Data.DataColumn("OtherCharges", typeof(double)));//5
            dt.Columns.Add(new System.Data.DataColumn("TotalBookedBill", typeof(double)));//6
            dt.Columns.Add(new System.Data.DataColumn("PVEVNo", typeof(string)));//7
            dt.Columns.Add(new System.Data.DataColumn("DTSort", typeof(DateTime)));//8

            string StrSql = string.Empty;

            StrSql = "select (Case When GQNId !=0 then (Select Sum(tblQc_MaterialQuality_Details.AcceptedQty) from tblQc_MaterialQuality_Details where tblQc_MaterialQuality_Details.Id=tblACC_BillBooking_Details.GQNId)*(tblMM_PO_Details.Rate - (tblMM_PO_Details.Rate * tblMM_PO_Details.Discount) / 100) Else (Select Sum(tblinv_MaterialServiceNote_Details.ReceivedQty) As AcceptedQty from tblinv_MaterialServiceNote_Details where tblinv_MaterialServiceNote_Details.Id=tblACC_BillBooking_Details.GSNId)*(tblMM_PO_Details.Rate - (tblMM_PO_Details.Rate * tblMM_PO_Details.Discount) / 100)End) +PFAmt+ExStBasic+ExStEducess+ExStShecess+tblACC_BillBooking_Details.VAT+CST+tblACC_BillBooking_Details.Freight+tblACC_BillBooking_Details.BCDValue+tblACC_BillBooking_Details.EdCessOnCDValue+tblACC_BillBooking_Details.SHEDCessValue As TotalBookedBill,tblACC_BillBooking_Master.Discount,tblACC_BillBooking_Master.DiscountType,tblACC_BillBooking_Master.DebitAmt,tblACC_BillBooking_Master.OtherCharges,tblACC_BillBooking_Master.SysDate,tblACC_BillBooking_Master.Id as PVEVId,tblACC_BillBooking_Master.PVEVNo from tblACC_BillBooking_Master,tblACC_BillBooking_Details,tblMM_PO_Details,tblMM_PO_Master where tblACC_BillBooking_Master.CompId='" + CompId + "' And tblACC_BillBooking_Master.SupplierId='" + GetSupCode + "' And tblACC_BillBooking_Master.Id=tblACC_BillBooking_Details.MId AND tblACC_BillBooking_Master.FinYearId<='" + FinYearId + "' And tblACC_BillBooking_Master.TDSCode='" + TDSCode + "' And tblMM_PO_Details.Id=tblACC_BillBooking_Details.PODId AND tblMM_PO_Master.Id=tblMM_PO_Details.MId";

            SqlCommand cmdSql = new SqlCommand(StrSql, con);
            SqlDataReader sqldr = cmdSql.ExecuteReader();

            double AddAmt = 0;
            double DiscountType = 0;
            double DiscountAmt = 0;
            double DebitAmt = 0;
            double a = 0;

            DataRow sqlrow;

            while (sqldr.Read())
            {
                if (sqldr["TotalBookedBill"] != DBNull.Value)
                {
                    sqlrow = dt.NewRow();
                    sqlrow[0] = this.FromDateDMY(sqldr["SysDate"].ToString());
                    sqlrow[1] = Convert.ToInt32(sqldr["PVEVId"]);
                    sqlrow[2] = Convert.ToDouble(sqldr["Discount"]);
                    sqlrow[3] = Convert.ToInt32(sqldr["DiscountType"]);
                    sqlrow[4] = Convert.ToDouble(sqldr["DebitAmt"]);
                    sqlrow[5] = Convert.ToDouble(sqldr["OtherCharges"]);
                    sqlrow[6] = Convert.ToDouble(sqldr["TotalBookedBill"]);
                    sqlrow[7] = sqldr["PVEVNo"].ToString();
                    sqlrow[8] = Convert.ToDateTime(sqldr["SysDate"].ToString());

                    dt.Rows.Add(sqlrow);
                    dt.AcceptChanges();
                }
            }

            var linq = from x in dt.AsEnumerable()
                       group x by new
                       {
                           y = x.Field<int>("PVEVId")
                       } into grp
                       let row1 = grp.First()
                       select new
                       {
                           SysDate = row1.Field<string>("SysDate"),
                           PVEVNo = row1.Field<string>("PVEVNo"),
                           PVEVId = row1.Field<int>("PVEVId"),
                           Discount = row1.Field<double>("Discount"),
                           DiscountType = row1.Field<int>("DiscountType"),
                           DebitAmt = row1.Field<double>("DebitAmt"),
                           OtherCharges = row1.Field<double>("OtherCharges"),
                           TotalBookedBill = grp.Sum(r => r.Field<double>("TotalBookedBill")),
                           DTSort = row1.Field<DateTime>("DTSort")
                       };

            double letCal = 0;

            foreach (var d in linq)
            {
                letCal = d.TotalBookedBill + d.OtherCharges;

                if (d.DiscountType == 0)
                {
                    letCal = letCal - d.Discount;
                }
                else if (d.DiscountType == 1)
                {
                    letCal = letCal - (letCal * d.Discount / 100);
                }
                CalCulatedAmt += letCal - d.DebitAmt;
            }

        }
        catch (Exception ex)
        {

        }
        finally
        {
            con.Close();
        }

        return Math.Round(CalCulatedAmt, 2);
    }

    //////////////

    /////////////// Balance Sheet

    public double FillGrid_Creditors(int CompId, int FinYearId, int x, string Category)
    {
        double AllOpeningTotal = 0;
        double AllPaymentTotal = 0;
        double AllCashPaymentTotal = 0;
        double AllBookBillTotal = 0;
        double AllTotal = 0;
        double y = 0;

        string connStr = this.Connection();
        SqlConnection con = new SqlConnection(connStr);

        try
        {
            string strCredit = this.select("SupplierId,SupplierName+' ['+SupplierId+']' AS SupplierName", "tblMM_Supplier_master", "CompId='" + CompId + "' order by SupplierId Asc");
            SqlCommand cmdCredit = new SqlCommand(strCredit, con);
            SqlDataReader rdr;
            con.Open();

            rdr = cmdCredit.ExecuteReader();
            DataRow dr;

            while (rdr.Read())
            {

                string strCreditOp = this.select("OpeningAmt", "tblACC_Creditors_Master", "SupplierId='" + rdr["SupplierId"].ToString() + "'");
                SqlCommand cmdCreditOp = new SqlCommand(strCreditOp, con);
                SqlDataReader rdrOp = cmdCreditOp.ExecuteReader();
                rdrOp.Read();

                double OpeningAmt = 0;
                if (rdrOp.HasRows == true && rdrOp["OpeningAmt"] != DBNull.Value)
                {
                    OpeningAmt = Math.Round(Convert.ToDouble(rdrOp["OpeningAmt"]), 2);
                    AllOpeningTotal += OpeningAmt;
                }

                double BookBillAmt = 0;
                BookBillAmt = this.FillGrid_CreditorsBookedBill(CompId, FinYearId, rdr["SupplierId"].ToString(), Category);
                AllBookBillTotal += BookBillAmt;

                double PaymentAmt = 0;
                PaymentAmt = this.FillGrid_CreditorsPayment(CompId, FinYearId, rdr["SupplierId"].ToString(), 0, Category);
                AllPaymentTotal += PaymentAmt;

                double CashPaymentAmt = 0;
                CashPaymentAmt = this.FillGrid_CreditorsCashPayment(CompId, FinYearId, rdr["SupplierId"].ToString(), 0, Category);
                AllCashPaymentTotal += CashPaymentAmt;

                double ClosingAmt = 0;
                ClosingAmt = Math.Round(((OpeningAmt + BookBillAmt) - (PaymentAmt + CashPaymentAmt)), 2);
                AllTotal += ClosingAmt;
            }

            switch (x)
            {
                case 1:
                    y = Math.Round(AllOpeningTotal, 2);
                    break;
                case 2:
                    y = Math.Round(AllBookBillTotal, 2);
                    break;
                case 3:
                    y = Math.Round(AllPaymentTotal, 2);
                    break;
                case 4:
                    y = Math.Round(AllTotal, 2);
                    break;
                case 5:
                    y = Math.Round(AllCashPaymentTotal, 2);
                    break;
            }

            // return y;

        }
        catch (Exception ex)
        {

        }
        finally
        {
            con.Close();
        }
        return y;
    }
    public double FillGrid_CreditorsBookedBill(int CompId, int FinYearId, string GetSupCode, string Category)
    {
        double CalCulatedAmt = 0;
        string connStr = this.Connection();
        SqlConnection con = new SqlConnection(connStr);
        con.Open();

        try
        {

            DataTable dt = new DataTable();

            dt.Columns.Add(new System.Data.DataColumn("SysDate", typeof(string)));//0
            dt.Columns.Add(new System.Data.DataColumn("PVEVId", typeof(int)));//1
            dt.Columns.Add(new System.Data.DataColumn("Discount", typeof(double)));//2
            dt.Columns.Add(new System.Data.DataColumn("DiscountType", typeof(int)));//3
            dt.Columns.Add(new System.Data.DataColumn("DebitAmt", typeof(double)));//4
            dt.Columns.Add(new System.Data.DataColumn("OtherCharges", typeof(double)));//5
            dt.Columns.Add(new System.Data.DataColumn("TotalBookedBill", typeof(double)));//6
            dt.Columns.Add(new System.Data.DataColumn("PVEVNo", typeof(string)));//7
            dt.Columns.Add(new System.Data.DataColumn("DTSort", typeof(DateTime)));//8

            string p = string.Empty;

            if (GetSupCode != string.Empty)
            {
                p = " And tblACC_BillBooking_Master.SupplierId='" + GetSupCode + "'";
            }


            string StrSql = string.Empty;

            if (Category == string.Empty)
            {
                StrSql = "select (Case When GQNId !=0 then (Select Sum(tblQc_MaterialQuality_Details.AcceptedQty) from tblQc_MaterialQuality_Details where tblQc_MaterialQuality_Details.Id=tblACC_BillBooking_Details.GQNId)*(tblMM_PO_Details.Rate - (tblMM_PO_Details.Rate * tblMM_PO_Details.Discount) / 100) Else (Select Sum(tblinv_MaterialServiceNote_Details.ReceivedQty) As AcceptedQty from tblinv_MaterialServiceNote_Details where tblinv_MaterialServiceNote_Details.Id=tblACC_BillBooking_Details.GSNId)*(tblMM_PO_Details.Rate - (tblMM_PO_Details.Rate * tblMM_PO_Details.Discount) / 100)End) +PFAmt+ExStBasic+ExStEducess+ExStShecess+tblACC_BillBooking_Details.VAT+CST+tblACC_BillBooking_Details.Freight+tblACC_BillBooking_Details.BCDValue+tblACC_BillBooking_Details.EdCessOnCDValue+tblACC_BillBooking_Details.SHEDCessValue As TotalBookedBill,tblACC_BillBooking_Master.Discount,tblACC_BillBooking_Master.DiscountType,tblACC_BillBooking_Master.DebitAmt,tblACC_BillBooking_Master.OtherCharges,tblACC_BillBooking_Master.SysDate,tblACC_BillBooking_Master.Id as PVEVId,tblACC_BillBooking_Master.PVEVNo from tblACC_BillBooking_Master,tblACC_BillBooking_Details,tblMM_PO_Details,tblMM_PO_Master where tblACC_BillBooking_Master.CompId='" + CompId + "'" + p + " And tblACC_BillBooking_Master.Id=tblACC_BillBooking_Details.MId AND tblACC_BillBooking_Master.FinYearId<='" + FinYearId + "' And tblMM_PO_Details.Id=tblACC_BillBooking_Details.PODId AND tblMM_PO_Master.Id=tblMM_PO_Details.MId";
            }
            else
            {
                StrSql = "select (Case When GQNId !=0 then (Case when tblMM_PO_Details.PRId Is not null then(Select tblQc_MaterialQuality_Details.AcceptedQty from tblQc_MaterialQuality_Details,tblMM_PR_Details,AccHead where tblQc_MaterialQuality_Details.Id=tblACC_BillBooking_Details.GQNId  AND tblMM_PO_Details.PRId=tblMM_PR_Details.Id AND tblMM_PR_Details.AHId=AccHead.Id And AccHead.Category='" + Category + "')*(tblMM_PO_Details.Rate - (tblMM_PO_Details.Rate * tblMM_PO_Details.Discount) / 100)else(Select tblQc_MaterialQuality_Details.AcceptedQty from tblQc_MaterialQuality_Details,tblMM_SPR_Details,AccHead where tblQc_MaterialQuality_Details.Id=tblACC_BillBooking_Details.GQNId  AND tblMM_PO_Details.SPRId=tblMM_SPR_Details.Id AND tblMM_SPR_Details.AHId=AccHead.Id And AccHead.Category='" + Category + "')*(tblMM_PO_Details.Rate - (tblMM_PO_Details.Rate * tblMM_PO_Details.Discount) / 100)End) Else (Case when tblMM_PO_Details.PRId Is not null then(Select tblinv_MaterialServiceNote_Details.ReceivedQty As AcceptedQty from tblinv_MaterialServiceNote_Details,tblMM_PR_Details,AccHead where tblinv_MaterialServiceNote_Details.Id=tblACC_BillBooking_Details.GSNId AND tblMM_PO_Details.PRId=tblMM_PR_Details.Id AND tblMM_PR_Details.AHId=AccHead.Id And AccHead.Category='" + Category + "')*(tblMM_PO_Details.Rate - (tblMM_PO_Details.Rate * tblMM_PO_Details.Discount) / 100)else(Select tblinv_MaterialServiceNote_Details.ReceivedQty As AcceptedQty from tblinv_MaterialServiceNote_Details,tblMM_SPR_Details,AccHead where tblinv_MaterialServiceNote_Details.Id=tblACC_BillBooking_Details.GSNId AND tblMM_PO_Details.SPRId=tblMM_SPR_Details.Id AND tblMM_SPR_Details.AHId=AccHead.Id And AccHead.Category='" + Category + "')*(tblMM_PO_Details.Rate - (tblMM_PO_Details.Rate * tblMM_PO_Details.Discount) / 100)End) End)+PFAmt+ExStBasic+ExStEducess+ExStShecess+tblACC_BillBooking_Details.VAT+CST+tblACC_BillBooking_Details.Freight+tblACC_BillBooking_Details.BCDValue+tblACC_BillBooking_Details.EdCessOnCDValue+tblACC_BillBooking_Details.SHEDCessValue As TotalBookedBill,tblACC_BillBooking_Master.Discount,tblACC_BillBooking_Master.DiscountType,tblACC_BillBooking_Master.DebitAmt,tblACC_BillBooking_Master.OtherCharges,tblACC_BillBooking_Master.SysDate,tblACC_BillBooking_Master.Id as PVEVId,tblACC_BillBooking_Master.PVEVNo from tblACC_BillBooking_Master,tblACC_BillBooking_Details,tblMM_PO_Details,tblMM_PO_Master where tblACC_BillBooking_Master.CompId='" + CompId + "'" + p + " And tblACC_BillBooking_Master.Id=tblACC_BillBooking_Details.MId AND tblACC_BillBooking_Master.FinYearId<='" + FinYearId + "' And tblMM_PO_Details.Id=tblACC_BillBooking_Details.PODId AND tblMM_PO_Master.Id=tblMM_PO_Details.MId";

            }

            SqlCommand cmdSql = new SqlCommand(StrSql, con);
            SqlDataReader sqldr = cmdSql.ExecuteReader();

            double AddAmt = 0;
            double DiscountType = 0;
            double DiscountAmt = 0;
            double DebitAmt = 0;
            double a = 0;


            DataRow sqlrow;

            while (sqldr.Read())
            {
                if (sqldr["TotalBookedBill"] != DBNull.Value)
                {
                    sqlrow = dt.NewRow();
                    sqlrow[0] = this.FromDateDMY(sqldr["SysDate"].ToString());
                    sqlrow[1] = Convert.ToInt32(sqldr["PVEVId"]);
                    sqlrow[2] = Convert.ToDouble(sqldr["Discount"]);
                    sqlrow[3] = Convert.ToInt32(sqldr["DiscountType"]);
                    sqlrow[4] = Convert.ToDouble(sqldr["DebitAmt"]);
                    sqlrow[5] = Convert.ToDouble(sqldr["OtherCharges"]);
                    sqlrow[6] = Convert.ToDouble(sqldr["TotalBookedBill"]);
                    sqlrow[7] = sqldr["PVEVNo"].ToString();
                    sqlrow[8] = Convert.ToDateTime(sqldr["SysDate"].ToString());

                    dt.Rows.Add(sqlrow);
                    dt.AcceptChanges();
                }
            }

            var linq = from x in dt.AsEnumerable()
                       group x by new
                       {
                           y = x.Field<int>("PVEVId")
                       } into grp
                       let row1 = grp.First()
                       select new
                       {
                           SysDate = row1.Field<string>("SysDate"),
                           PVEVNo = row1.Field<string>("PVEVNo"),
                           PVEVId = row1.Field<int>("PVEVId"),
                           Discount = row1.Field<double>("Discount"),
                           DiscountType = row1.Field<int>("DiscountType"),
                           DebitAmt = row1.Field<double>("DebitAmt"),
                           OtherCharges = row1.Field<double>("OtherCharges"),
                           TotalBookedBill = grp.Sum(r => r.Field<double>("TotalBookedBill")),
                           DTSort = row1.Field<DateTime>("DTSort")
                       };

            double letCal = 0;

            foreach (var d in linq)
            {
                letCal = d.TotalBookedBill + d.OtherCharges;

                if (d.DiscountType == 0)
                {
                    letCal = letCal - d.Discount;
                }
                else if (d.DiscountType == 1)
                {
                    letCal = letCal - (letCal * d.Discount / 100);
                }
                CalCulatedAmt += letCal - d.DebitAmt;
            }

        }
        catch (Exception ex)
        {

        }
        finally
        {
            con.Close();
        }

        return Math.Round(CalCulatedAmt, 2);
    }
    public double FillGrid_CreditorsPayment(int CompId, int FinYearId, string GetSupCode, int PayId, string AccHeadCat)
    {

        double TotalPayAmy = 0;
        string connStr = this.Connection();
        SqlConnection con = new SqlConnection(connStr);
        con.Open();

        try
        {
            string x = string.Empty;
            string y = string.Empty;
            string z = string.Empty;

            if (AccHeadCat != "")
            {
                x = " AND AccHead.Category='" + AccHeadCat + "'";
            }
            if (GetSupCode != "")
            {
                y = " AND tblACC_BankVoucher_Payment_Master.PayTo='" + GetSupCode + "'";
            }

            if (PayId != 0)
            {
                z = " AND tblACC_BankVoucher_Payment_Master.Id='" + PayId + "'";
            }

            double DtlsAmt = 0;

            string sql = "SELECT  Sum(tblACC_BankVoucher_Payment_Details.Amount+tblACC_BankVoucher_Payment_Master.PayAmt) as Sum_Amt FROM tblACC_BillBooking_Master INNER JOIN tblACC_BillBooking_Details ON tblACC_BillBooking_Master.Id =  tblACC_BillBooking_Details.MId INNER JOIN tblACC_BankVoucher_Payment_Master INNER JOIN tblACC_BankVoucher_Payment_Details ON tblACC_BankVoucher_Payment_Master.Id = tblACC_BankVoucher_Payment_Details.MId ON tblACC_BillBooking_Master.Id = tblACC_BankVoucher_Payment_Details.PVEVNO INNER JOIN AccHead ON tblACC_BillBooking_Master.AHId = AccHead.Id  AND tblACC_BillBooking_Master.AHId !=0 " + x + y + z + " AND tblACC_BankVoucher_Payment_Master.CompId='" + CompId + "' AND tblACC_BankVoucher_Payment_Master.FinYearId<='" + FinYearId + "' AND tblACC_BankVoucher_Payment_Master.Type='4'";

            SqlCommand getcmd = new SqlCommand(sql, con);
            SqlDataReader rdr2 = getcmd.ExecuteReader();
            rdr2.Read();

            if (rdr2.HasRows == true && rdr2["Sum_Amt"] != DBNull.Value)
            {
                DtlsAmt = Math.Round(Convert.ToDouble(rdr2["Sum_Amt"].ToString()), 2);
            }

            TotalPayAmy += Math.Round(DtlsAmt, 2);

        }
        catch (Exception ex)
        {

        }
        finally
        {
            con.Close();
        }

        return TotalPayAmy;
    }
    public double FillGrid_CreditorsCashPayment(int CompId, int FinYearId, string GetSupCode, int PayId, string AccHeadCat)
    {
        double TotalPayAmy = 0;
        string connStr = this.Connection();
        SqlConnection con = new SqlConnection(connStr);
        con.Open();

        try
        {
            string x = string.Empty;
            string y = string.Empty;
            string z = string.Empty;

            if (AccHeadCat != "")
            {
                x = " AND AccHead.Category='" + AccHeadCat + "'";
            }

            if (GetSupCode != "")
            {
                y = " AND tblACC_CashVoucher_Payment_Master.ReceivedBy='" + GetSupCode + "'";
            }

            if (PayId != 0)
            {
                z = " AND tblACC_CashVoucher_Payment_Master.Id='" + PayId + "'";
            }

            double DtlsAmt = 0;

            string sql = "SELECT Sum(tblACC_CashVoucher_Payment_Details.Amount) as Sum_Amt FROM tblACC_CashVoucher_Payment_Details INNER JOIN tblACC_CashVoucher_Payment_Master ON tblACC_CashVoucher_Payment_Details.MId = tblACC_CashVoucher_Payment_Master.Id INNER JOIN AccHead ON tblACC_CashVoucher_Payment_Details.AcHead = AccHead.Id AND tblACC_CashVoucher_Payment_Details.AcHead !=0 " + x + y + z + " AND tblACC_CashVoucher_Payment_Master.CompId='" + CompId + "' AND tblACC_CashVoucher_Payment_Master.FinYearId<='" + FinYearId + "'INNER JOIN tblMM_Supplier_master ON tblACC_CashVoucher_Payment_Master.ReceivedBy = tblMM_Supplier_master.SupplierId";

            SqlCommand getcmd = new SqlCommand(sql, con);
            SqlDataReader rdr2 = getcmd.ExecuteReader();
            rdr2.Read();

            if (rdr2.HasRows == true && rdr2["Sum_Amt"] != DBNull.Value)
            {
                DtlsAmt = Math.Round(Convert.ToDouble(rdr2["Sum_Amt"].ToString()), 2);
            }

            TotalPayAmy += Math.Round(DtlsAmt, 2);
        }
        catch (Exception ex)
        {

        }
        finally
        {
            con.Close();
        }

        return TotalPayAmy;
    }


    //public double FillGrid_Creditors(int CompId, int FinYearId, int x, string Category)
    //{
    //    double AllOpeningTotal = 0;
    //    double AllPaymentTotal = 0;
    //    double AllBookBillTotal = 0;
    //    double AllTotal = 0;
    //    double y = 0;

    //    string connStr = this.Connection();
    //    SqlConnection con = new SqlConnection(connStr);

    //    try
    //    {
    //        string strCredit = "SELECT tblACC_Creditors_Master.Id,tblMM_Supplier_master.SupplierId,tblMM_Supplier_master.SupplierName+' ['+tblMM_Supplier_master.SupplierId+']' AS SupplierName,tblMM_Supplier_master.SupplierId,tblACC_Creditors_Master.OpeningAmt FROM tblACC_Creditors_Master INNER JOIN tblMM_Supplier_master ON tblACC_Creditors_Master.SupplierId = tblMM_Supplier_master.SupplierId And tblACC_Creditors_Master.CompId='" + CompId + "' order by tblMM_Supplier_master.SupplierId Asc";

    //        SqlCommand cmdCredit = new SqlCommand(strCredit, con);
    //        SqlDataReader rdr;
    //        con.Open();

    //        rdr = cmdCredit.ExecuteReader();
    //        DataRow dr;

    //        while (rdr.Read())
    //        {
    //            double OpeningAmt = 0;
    //            if (rdr["OpeningAmt"] != DBNull.Value)
    //            {
    //                OpeningAmt = Math.Round(Convert.ToDouble(rdr["OpeningAmt"]), 2);
    //                AllOpeningTotal += OpeningAmt;
    //            }

    //            double BookBillAmt = 0;
    //            BookBillAmt = this.FillGrid_CreditorsBookedBill(CompId, FinYearId, rdr["SupplierId"].ToString(), Category);
    //            AllBookBillTotal += BookBillAmt;

    //            double PaymentAmt = 0;
    //            PaymentAmt = this.FillGrid_CreditorsPayment(CompId, FinYearId, rdr["SupplierId"].ToString(), 0, Category);
    //            AllPaymentTotal += PaymentAmt;

    //            double ClosingAmt = 0;
    //            ClosingAmt = Math.Round(((OpeningAmt + BookBillAmt) - PaymentAmt), 2); AllTotal += ClosingAmt;
    //        }

    //        switch (x)
    //        {
    //            case 1:
    //                y = AllOpeningTotal;
    //                break;
    //            case 2:
    //                y = AllBookBillTotal;
    //                break;
    //            case 3:
    //                y = AllPaymentTotal;
    //                break;
    //            case 4:
    //                y = AllTotal;
    //                break;
    //        }

    //        // return y;

    //    }
    //    catch (Exception ex)
    //    {

    //    }
    //    finally
    //    {
    //        con.Close();
    //    }
    //    return y;
    //}
    //public double FillGrid_CreditorsBookedBill(int CompId, int FinYearId, string GetSupCode, string Category)
    //{
    //    double CalCulatedAmt = 0;
    //    string connStr = this.Connection();
    //    SqlConnection con = new SqlConnection(connStr);
    //    con.Open();

    //    try
    //    {

    //        DataTable dt = new DataTable();

    //        dt.Columns.Add(new System.Data.DataColumn("SysDate", typeof(string)));//0
    //        dt.Columns.Add(new System.Data.DataColumn("PVEVId", typeof(int)));//1
    //        dt.Columns.Add(new System.Data.DataColumn("Discount", typeof(double)));//2
    //        dt.Columns.Add(new System.Data.DataColumn("DiscountType", typeof(int)));//3
    //        dt.Columns.Add(new System.Data.DataColumn("DebitAmt", typeof(double)));//4
    //        dt.Columns.Add(new System.Data.DataColumn("OtherCharges", typeof(double)));//5
    //        dt.Columns.Add(new System.Data.DataColumn("TotalBookedBill", typeof(double)));//6
    //        dt.Columns.Add(new System.Data.DataColumn("PVEVNo", typeof(string)));//7
    //        dt.Columns.Add(new System.Data.DataColumn("DTSort", typeof(DateTime)));//8

    //        string p = string.Empty;

    //        if (GetSupCode != string.Empty)
    //        {
    //            p = " And tblACC_BillBooking_Master.SupplierId='" + GetSupCode + "'";
    //        }


    //        string StrSql = string.Empty;

    //        if (Category == string.Empty)
    //        {
    //            StrSql = "select (Case When GQNId !=0 then (Select Sum(tblQc_MaterialQuality_Details.AcceptedQty) from tblQc_MaterialQuality_Details where tblQc_MaterialQuality_Details.Id=tblACC_BillBooking_Details.GQNId)*(tblMM_PO_Details.Rate - (tblMM_PO_Details.Rate * tblMM_PO_Details.Discount) / 100) Else (Select Sum(tblinv_MaterialServiceNote_Details.ReceivedQty) As AcceptedQty from tblinv_MaterialServiceNote_Details where tblinv_MaterialServiceNote_Details.Id=tblACC_BillBooking_Details.GSNId)*(tblMM_PO_Details.Rate - (tblMM_PO_Details.Rate * tblMM_PO_Details.Discount) / 100)End) +PFAmt+ExStBasic+ExStEducess+ExStShecess+tblACC_BillBooking_Details.VAT+CST+tblACC_BillBooking_Details.Freight+tblACC_BillBooking_Details.BCDValue+tblACC_BillBooking_Details.EdCessOnCDValue+tblACC_BillBooking_Details.SHEDCessValue As TotalBookedBill,tblACC_BillBooking_Master.Discount,tblACC_BillBooking_Master.DiscountType,tblACC_BillBooking_Master.DebitAmt,tblACC_BillBooking_Master.OtherCharges,tblACC_BillBooking_Master.SysDate,tblACC_BillBooking_Master.Id as PVEVId,tblACC_BillBooking_Master.PVEVNo from tblACC_BillBooking_Master,tblACC_BillBooking_Details,tblMM_PO_Details,tblMM_PO_Master where tblACC_BillBooking_Master.CompId='" + CompId + "'" + p + " And tblACC_BillBooking_Master.Id=tblACC_BillBooking_Details.MId AND tblACC_BillBooking_Master.FinYearId<='" + FinYearId + "' And tblMM_PO_Details.Id=tblACC_BillBooking_Details.PODId AND tblMM_PO_Master.Id=tblMM_PO_Details.MId";
    //        }
    //        else
    //        {
    //            StrSql = "select (Case When GQNId !=0 then (Case when tblMM_PO_Details.PRId Is not null then(Select tblQc_MaterialQuality_Details.AcceptedQty from tblQc_MaterialQuality_Details,tblMM_PR_Details,AccHead where tblQc_MaterialQuality_Details.Id=tblACC_BillBooking_Details.GQNId  AND tblMM_PO_Details.PRId=tblMM_PR_Details.Id AND tblMM_PR_Details.AHId=AccHead.Id And AccHead.Category='" + Category + "')*(tblMM_PO_Details.Rate - (tblMM_PO_Details.Rate * tblMM_PO_Details.Discount) / 100)else(Select tblQc_MaterialQuality_Details.AcceptedQty from tblQc_MaterialQuality_Details,tblMM_SPR_Details,AccHead where tblQc_MaterialQuality_Details.Id=tblACC_BillBooking_Details.GQNId  AND tblMM_PO_Details.SPRId=tblMM_SPR_Details.Id AND tblMM_SPR_Details.AHId=AccHead.Id And AccHead.Category='" + Category + "')*(tblMM_PO_Details.Rate - (tblMM_PO_Details.Rate * tblMM_PO_Details.Discount) / 100)End) Else (Case when tblMM_PO_Details.PRId Is not null then(Select tblinv_MaterialServiceNote_Details.ReceivedQty As AcceptedQty from tblinv_MaterialServiceNote_Details,tblMM_PR_Details,AccHead where tblinv_MaterialServiceNote_Details.Id=tblACC_BillBooking_Details.GSNId AND tblMM_PO_Details.PRId=tblMM_PR_Details.Id AND tblMM_PR_Details.AHId=AccHead.Id And AccHead.Category='" + Category + "')*(tblMM_PO_Details.Rate - (tblMM_PO_Details.Rate * tblMM_PO_Details.Discount) / 100)else(Select tblinv_MaterialServiceNote_Details.ReceivedQty As AcceptedQty from tblinv_MaterialServiceNote_Details,tblMM_SPR_Details,AccHead where tblinv_MaterialServiceNote_Details.Id=tblACC_BillBooking_Details.GSNId AND tblMM_PO_Details.SPRId=tblMM_SPR_Details.Id AND tblMM_SPR_Details.AHId=AccHead.Id And AccHead.Category='" + Category + "')*(tblMM_PO_Details.Rate - (tblMM_PO_Details.Rate * tblMM_PO_Details.Discount) / 100)End) End)+PFAmt+ExStBasic+ExStEducess+ExStShecess+tblACC_BillBooking_Details.VAT+CST+tblACC_BillBooking_Details.Freight+tblACC_BillBooking_Details.BCDValue+tblACC_BillBooking_Details.EdCessOnCDValue+tblACC_BillBooking_Details.SHEDCessValue As TotalBookedBill,tblACC_BillBooking_Master.Discount,tblACC_BillBooking_Master.DiscountType,tblACC_BillBooking_Master.DebitAmt,tblACC_BillBooking_Master.OtherCharges,tblACC_BillBooking_Master.SysDate,tblACC_BillBooking_Master.Id as PVEVId,tblACC_BillBooking_Master.PVEVNo from tblACC_BillBooking_Master,tblACC_BillBooking_Details,tblMM_PO_Details,tblMM_PO_Master where tblACC_BillBooking_Master.CompId='" + CompId + "'" + p + " And tblACC_BillBooking_Master.Id=tblACC_BillBooking_Details.MId AND tblACC_BillBooking_Master.FinYearId<='" + FinYearId + "' And tblMM_PO_Details.Id=tblACC_BillBooking_Details.PODId AND tblMM_PO_Master.Id=tblMM_PO_Details.MId";

    //        }

    //        SqlCommand cmdSql = new SqlCommand(StrSql, con);
    //        SqlDataReader sqldr = cmdSql.ExecuteReader();

    //        double AddAmt = 0;
    //        double DiscountType = 0;
    //        double DiscountAmt = 0;
    //        double DebitAmt = 0;
    //        double a = 0;


    //        DataRow sqlrow;

    //        while (sqldr.Read())
    //        {
    //            if (sqldr["TotalBookedBill"] != DBNull.Value)
    //            {
    //                sqlrow = dt.NewRow();
    //                sqlrow[0] = this.FromDateDMY(sqldr["SysDate"].ToString());
    //                sqlrow[1] = Convert.ToInt32(sqldr["PVEVId"]);
    //                sqlrow[2] = Convert.ToDouble(sqldr["Discount"]);
    //                sqlrow[3] = Convert.ToInt32(sqldr["DiscountType"]);
    //                sqlrow[4] = Convert.ToDouble(sqldr["DebitAmt"]);
    //                sqlrow[5] = Convert.ToDouble(sqldr["OtherCharges"]);
    //                sqlrow[6] = Convert.ToDouble(sqldr["TotalBookedBill"]);
    //                sqlrow[7] = sqldr["PVEVNo"].ToString();
    //                sqlrow[8] = Convert.ToDateTime(sqldr["SysDate"].ToString());

    //                dt.Rows.Add(sqlrow);
    //                dt.AcceptChanges();
    //            }
    //        }

    //        var linq = from x in dt.AsEnumerable()
    //                   group x by new
    //                   {
    //                       y = x.Field<int>("PVEVId")
    //                   } into grp
    //                   let row1 = grp.First()
    //                   select new
    //                   {
    //                       SysDate = row1.Field<string>("SysDate"),
    //                       PVEVNo = row1.Field<string>("PVEVNo"),
    //                       PVEVId = row1.Field<int>("PVEVId"),
    //                       Discount = row1.Field<double>("Discount"),
    //                       DiscountType = row1.Field<int>("DiscountType"),
    //                       DebitAmt = row1.Field<double>("DebitAmt"),
    //                       OtherCharges = row1.Field<double>("OtherCharges"),
    //                       TotalBookedBill = grp.Sum(r => r.Field<double>("TotalBookedBill")),
    //                       DTSort = row1.Field<DateTime>("DTSort")
    //                   };

    //        double letCal = 0;

    //        foreach (var d in linq)
    //        {
    //            letCal = d.TotalBookedBill + d.OtherCharges;

    //            if (d.DiscountType == 0)
    //            {
    //                letCal = letCal - d.Discount;
    //            }
    //            else if (d.DiscountType == 1)
    //            {
    //                letCal = letCal - (letCal * d.Discount / 100);
    //            }
    //            CalCulatedAmt += letCal - d.DebitAmt;
    //        }

    //    }
    //    catch (Exception ex)
    //    {

    //    }
    //    finally
    //    {
    //        con.Close();
    //    }

    //    return Math.Round(CalCulatedAmt, 2);
    //}
    //public double FillGrid_CreditorsPayment(int CompId, int FinYearId, string GetSupCode, int PayId, string AccHeadCat)
    //{

    //    double TotalPayAmy = 0;
    //    string connStr = this.Connection();
    //    SqlConnection con = new SqlConnection(connStr);
    //    con.Open();

    //    try
    //    {
    //        string x = string.Empty;
    //        string y = string.Empty;
    //        string z = string.Empty;

    //        if (AccHeadCat != "")
    //        {
    //            x = " AND AccHead.Category='" + AccHeadCat + "'";
    //        }
    //        if (GetSupCode != "")
    //        {
    //            y = " AND tblACC_BankVoucher_Payment_Master.PayTo='" + GetSupCode + "'";
    //        }

    //        if (PayId != 0)
    //        {
    //            z = " AND tblACC_BankVoucher_Payment_Master.Id='" + PayId + "'";
    //        }

    //        double DtlsAmt = 0;

    //        string sql = "SELECT  Sum(tblACC_BankVoucher_Payment_Details.Amount+tblACC_BankVoucher_Payment_Master.PayAmt) as Sum_Amt FROM tblACC_BillBooking_Master INNER JOIN tblACC_BillBooking_Details ON tblACC_BillBooking_Master.Id =  tblACC_BillBooking_Details.MId INNER JOIN tblACC_BankVoucher_Payment_Master INNER JOIN tblACC_BankVoucher_Payment_Details ON tblACC_BankVoucher_Payment_Master.Id = tblACC_BankVoucher_Payment_Details.MId ON tblACC_BillBooking_Master.Id = tblACC_BankVoucher_Payment_Details.PVEVNO INNER JOIN AccHead ON tblACC_BillBooking_Master.AHId = AccHead.Id  AND tblACC_BillBooking_Master.AHId !=0 " + x + y + z + " AND tblACC_BankVoucher_Payment_Master.CompId='" + CompId + "' AND tblACC_BankVoucher_Payment_Master.FinYearId<='" + FinYearId + "' AND tblACC_BankVoucher_Payment_Master.Type='4'";

    //        SqlCommand getcmd = new SqlCommand(sql, con);
    //        SqlDataReader rdr2 = getcmd.ExecuteReader();
    //        rdr2.Read();

    //        if (rdr2.HasRows == true)
    //        {
    //            DtlsAmt = Convert.ToDouble(decimal.Parse((rdr2["Sum_Amt"]).ToString()).ToString("N3"));
    //        }

    //        TotalPayAmy += Math.Round(DtlsAmt, 2);

    //    }
    //    catch (Exception ex)
    //    {

    //    }
    //    finally
    //    {
    //        con.Close();
    //    }

    //    return TotalPayAmy;
    //}


    //public double FillGrid_Creditors(int CompId, int FinYearId, int x, string Category)
    //{
    //    double AllOpeningTotal = 0;
    //    double AllPaymentTotal = 0;
    //    double AllBookBillTotal = 0;
    //    double AllTotal = 0;
    //    double y = 0;

    //    string connStr = this.Connection();
    //    SqlConnection con = new SqlConnection(connStr);

    //    try
    //    {
    //        string strCredit = "SELECT tblACC_Creditors_Master.Id,tblMM_Supplier_master.SupplierId,tblMM_Supplier_master.SupplierName+' ['+tblMM_Supplier_master.SupplierId+']' AS SupplierName,tblMM_Supplier_master.SupplierId,tblACC_Creditors_Master.OpeningAmt FROM tblACC_Creditors_Master INNER JOIN tblMM_Supplier_master ON tblACC_Creditors_Master.SupplierId = tblMM_Supplier_master.SupplierId And tblACC_Creditors_Master.CompId='" + CompId + "' order by tblMM_Supplier_master.SupplierId Asc";

    //        SqlCommand cmdCredit = new SqlCommand(strCredit, con);
    //        SqlDataReader rdr;
    //        con.Open();

    //        rdr = cmdCredit.ExecuteReader();
    //        DataRow dr;

    //        while (rdr.Read())
    //        {
    //            double OpeningAmt = 0;
    //            if (rdr["OpeningAmt"] != DBNull.Value)
    //            {
    //                OpeningAmt = Math.Round(Convert.ToDouble(rdr["OpeningAmt"]), 2);
    //                AllOpeningTotal += OpeningAmt;
    //            }

    //            double BookBillAmt = 0;
    //            BookBillAmt = this.FillGrid_CreditorsBookedBill(CompId, FinYearId, rdr["SupplierId"].ToString(), Category);
    //            AllBookBillTotal += BookBillAmt;

    //            double PaymentAmt = 0;
    //            PaymentAmt = this.FillGrid_CreditorsPayment(CompId, FinYearId, rdr["SupplierId"].ToString());
    //            AllPaymentTotal += PaymentAmt;

    //            double ClosingAmt = 0;
    //            ClosingAmt = Math.Round(((OpeningAmt + BookBillAmt) - PaymentAmt), 2); AllTotal += ClosingAmt;
    //        }

    //        switch (x)
    //        {
    //            case 1:
    //                y = AllOpeningTotal;
    //                break;
    //            case 2:
    //                y = AllBookBillTotal;
    //                break;
    //            case 3:
    //                y = AllPaymentTotal;
    //                break;
    //            case 4:
    //                y = AllTotal;
    //                break;
    //        }

    //        // return y;

    //    }
    //    catch (Exception ex)
    //    {

    //    }
    //    finally
    //    {
    //        con.Close();
    //    }
    //    return y;
    //}
    //public double FillGrid_CreditorsBookedBill(int CompId, int FinYearId, string GetSupCode, string Category)
    //{
    //    double CalCulatedAmt = 0;
    //    string connStr = this.Connection();
    //    SqlConnection con = new SqlConnection(connStr);
    //    con.Open();

    //    try
    //    {

    //        DataTable dt = new DataTable();

    //        dt.Columns.Add(new System.Data.DataColumn("SysDate", typeof(string)));//0
    //        dt.Columns.Add(new System.Data.DataColumn("PVEVId", typeof(int)));//1
    //        dt.Columns.Add(new System.Data.DataColumn("Discount", typeof(double)));//2
    //        dt.Columns.Add(new System.Data.DataColumn("DiscountType", typeof(int)));//3
    //        dt.Columns.Add(new System.Data.DataColumn("DebitAmt", typeof(double)));//4
    //        dt.Columns.Add(new System.Data.DataColumn("OtherCharges", typeof(double)));//5
    //        dt.Columns.Add(new System.Data.DataColumn("TotalBookedBill", typeof(double)));//6
    //        dt.Columns.Add(new System.Data.DataColumn("PVEVNo", typeof(string)));//7
    //        dt.Columns.Add(new System.Data.DataColumn("DTSort", typeof(DateTime)));//8

    //        string p = string.Empty;

    //        if (GetSupCode != string.Empty)
    //        {
    //            p = " And tblACC_BillBooking_Master.SupplierId='" + GetSupCode + "'";
    //        }


    //        string StrSql = string.Empty;

    //        if (Category == string.Empty)
    //        {
    //            StrSql = "select (Case When GQNId !=0 then (Select Sum(tblQc_MaterialQuality_Details.AcceptedQty) from tblQc_MaterialQuality_Details where tblQc_MaterialQuality_Details.Id=tblACC_BillBooking_Details.GQNId)*(tblMM_PO_Details.Rate - (tblMM_PO_Details.Rate * tblMM_PO_Details.Discount) / 100) Else (Select Sum(tblinv_MaterialServiceNote_Details.ReceivedQty) As AcceptedQty from tblinv_MaterialServiceNote_Details where tblinv_MaterialServiceNote_Details.Id=tblACC_BillBooking_Details.GSNId)*(tblMM_PO_Details.Rate - (tblMM_PO_Details.Rate * tblMM_PO_Details.Discount) / 100)End) +PFAmt+ExStBasic+ExStEducess+ExStShecess+tblACC_BillBooking_Details.VAT+CST+tblACC_BillBooking_Details.Freight+tblACC_BillBooking_Details.BCDValue+tblACC_BillBooking_Details.EdCessOnCDValue+tblACC_BillBooking_Details.SHEDCessValue As TotalBookedBill,tblACC_BillBooking_Master.Discount,tblACC_BillBooking_Master.DiscountType,tblACC_BillBooking_Master.DebitAmt,tblACC_BillBooking_Master.OtherCharges,tblACC_BillBooking_Master.SysDate,tblACC_BillBooking_Master.Id as PVEVId,tblACC_BillBooking_Master.PVEVNo from tblACC_BillBooking_Master,tblACC_BillBooking_Details,tblMM_PO_Details,tblMM_PO_Master where tblACC_BillBooking_Master.CompId='" + CompId + "'" + p + " And tblACC_BillBooking_Master.Id=tblACC_BillBooking_Details.MId AND tblACC_BillBooking_Master.FinYearId<='" + FinYearId + "' And tblMM_PO_Details.Id=tblACC_BillBooking_Details.PODId AND tblMM_PO_Master.Id=tblMM_PO_Details.MId";
    //        }
    //        else
    //        {
    //            StrSql = "select (Case When GQNId !=0 then (Case when tblMM_PO_Details.PRId Is not null then(Select tblQc_MaterialQuality_Details.AcceptedQty from tblQc_MaterialQuality_Details,tblMM_PR_Details,AccHead where tblQc_MaterialQuality_Details.Id=tblACC_BillBooking_Details.GQNId  AND tblMM_PO_Details.PRId=tblMM_PR_Details.Id AND tblMM_PR_Details.AHId=AccHead.Id And AccHead.Category='" + Category + "')*(tblMM_PO_Details.Rate - (tblMM_PO_Details.Rate * tblMM_PO_Details.Discount) / 100)else(Select tblQc_MaterialQuality_Details.AcceptedQty from tblQc_MaterialQuality_Details,tblMM_SPR_Details,AccHead where tblQc_MaterialQuality_Details.Id=tblACC_BillBooking_Details.GQNId  AND tblMM_PO_Details.SPRId=tblMM_SPR_Details.Id AND tblMM_SPR_Details.AHId=AccHead.Id And AccHead.Category='" + Category + "')*(tblMM_PO_Details.Rate - (tblMM_PO_Details.Rate * tblMM_PO_Details.Discount) / 100)End) Else (Case when tblMM_PO_Details.PRId Is not null then(Select tblinv_MaterialServiceNote_Details.ReceivedQty As AcceptedQty from tblinv_MaterialServiceNote_Details,tblMM_PR_Details,AccHead where tblinv_MaterialServiceNote_Details.Id=tblACC_BillBooking_Details.GSNId AND tblMM_PO_Details.PRId=tblMM_PR_Details.Id AND tblMM_PR_Details.AHId=AccHead.Id And AccHead.Category='" + Category + "')*(tblMM_PO_Details.Rate - (tblMM_PO_Details.Rate * tblMM_PO_Details.Discount) / 100)else(Select tblinv_MaterialServiceNote_Details.ReceivedQty As AcceptedQty from tblinv_MaterialServiceNote_Details,tblMM_SPR_Details,AccHead where tblinv_MaterialServiceNote_Details.Id=tblACC_BillBooking_Details.GSNId AND tblMM_PO_Details.SPRId=tblMM_SPR_Details.Id AND tblMM_SPR_Details.AHId=AccHead.Id And AccHead.Category='" + Category + "')*(tblMM_PO_Details.Rate - (tblMM_PO_Details.Rate * tblMM_PO_Details.Discount) / 100)End) End)+PFAmt+ExStBasic+ExStEducess+ExStShecess+tblACC_BillBooking_Details.VAT+CST+tblACC_BillBooking_Details.Freight+tblACC_BillBooking_Details.BCDValue+tblACC_BillBooking_Details.EdCessOnCDValue+tblACC_BillBooking_Details.SHEDCessValue As TotalBookedBill,tblACC_BillBooking_Master.Discount,tblACC_BillBooking_Master.DiscountType,tblACC_BillBooking_Master.DebitAmt,tblACC_BillBooking_Master.OtherCharges,tblACC_BillBooking_Master.SysDate,tblACC_BillBooking_Master.Id as PVEVId,tblACC_BillBooking_Master.PVEVNo from tblACC_BillBooking_Master,tblACC_BillBooking_Details,tblMM_PO_Details,tblMM_PO_Master where tblACC_BillBooking_Master.CompId='" + CompId + "'" + p + " And tblACC_BillBooking_Master.Id=tblACC_BillBooking_Details.MId AND tblACC_BillBooking_Master.FinYearId<='" + FinYearId + "' And tblMM_PO_Details.Id=tblACC_BillBooking_Details.PODId AND tblMM_PO_Master.Id=tblMM_PO_Details.MId";

    //        }

    //        SqlCommand cmdSql = new SqlCommand(StrSql, con);
    //        SqlDataReader sqldr = cmdSql.ExecuteReader();

    //        double AddAmt = 0;
    //        double DiscountType = 0;
    //        double DiscountAmt = 0;
    //        double DebitAmt = 0;
    //        double a = 0;


    //        DataRow sqlrow;

    //        while (sqldr.Read())
    //        {
    //            if (sqldr["TotalBookedBill"] != DBNull.Value)
    //            {
    //                sqlrow = dt.NewRow();
    //                sqlrow[0] = this.FromDateDMY(sqldr["SysDate"].ToString());
    //                sqlrow[1] = Convert.ToInt32(sqldr["PVEVId"]);
    //                sqlrow[2] = Convert.ToDouble(sqldr["Discount"]);
    //                sqlrow[3] = Convert.ToInt32(sqldr["DiscountType"]);
    //                sqlrow[4] = Convert.ToDouble(sqldr["DebitAmt"]);
    //                sqlrow[5] = Convert.ToDouble(sqldr["OtherCharges"]);
    //                sqlrow[6] = Convert.ToDouble(sqldr["TotalBookedBill"]);
    //                sqlrow[7] = sqldr["PVEVNo"].ToString();
    //                sqlrow[8] = Convert.ToDateTime(sqldr["SysDate"].ToString());

    //                dt.Rows.Add(sqlrow);
    //                dt.AcceptChanges();
    //            }
    //        }

    //        var linq = from x in dt.AsEnumerable()
    //                   group x by new
    //                   {
    //                       y = x.Field<int>("PVEVId")
    //                   } into grp
    //                   let row1 = grp.First()
    //                   select new
    //                   {
    //                       SysDate = row1.Field<string>("SysDate"),
    //                       PVEVNo = row1.Field<string>("PVEVNo"),
    //                       PVEVId = row1.Field<int>("PVEVId"),
    //                       Discount = row1.Field<double>("Discount"),
    //                       DiscountType = row1.Field<int>("DiscountType"),
    //                       DebitAmt = row1.Field<double>("DebitAmt"),
    //                       OtherCharges = row1.Field<double>("OtherCharges"),
    //                       TotalBookedBill = grp.Sum(r => r.Field<double>("TotalBookedBill")),
    //                       DTSort = row1.Field<DateTime>("DTSort")
    //                   };

    //        double letCal = 0;

    //        foreach (var d in linq)
    //        {
    //            letCal = d.TotalBookedBill + d.OtherCharges;

    //            if (d.DiscountType == 0)
    //            {
    //                letCal = letCal - d.Discount;
    //            }
    //            else if (d.DiscountType == 1)
    //            {
    //                letCal = letCal - (letCal * d.Discount / 100);
    //            }
    //            CalCulatedAmt += letCal - d.DebitAmt;
    //        }

    //    }
    //    catch (Exception ex)
    //    {

    //    }
    //    finally
    //    {
    //        con.Close();
    //    }

    //    return Math.Round(CalCulatedAmt, 2);
    //}
    //public double FillGrid_CreditorsPayment(int CompId, int FinYearId, string GetSupCode)
    //{
    //    double TotalPayAmy = 0;
    //    string connStr = this.Connection();
    //    SqlConnection con = new SqlConnection(connStr);
    //    con.Open();

    //    try
    //    {
    //        string str = this.select("Id,PayAmt ", "tblACC_BankVoucher_Payment_Master", " CompId='" + CompId + "' AND FinYearId<='" + FinYearId + "' AND PayTo='" + GetSupCode + "'");
    //        SqlCommand cmdCustWo = new SqlCommand(str, con);
    //        SqlDataReader rdr = cmdCustWo.ExecuteReader();

    //        while (rdr.Read())
    //        {
    //            double DtlsAmt = 0;

    //            string sqlAmt2 = "Select Sum(Amount)As Amt from tblACC_BankVoucher_Payment_Details inner join tblACC_BankVoucher_Payment_Master on tblACC_BankVoucher_Payment_Details.MId=tblACC_BankVoucher_Payment_Master.Id And CompId='" + CompId + "' AND tblACC_BankVoucher_Payment_Details.MId='" + rdr["Id"].ToString() + "'";
    //            SqlCommand cmd6 = new SqlCommand(sqlAmt2, con);
    //            SqlDataReader rdr2 = cmd6.ExecuteReader();

    //            while (rdr2.Read())
    //            {
    //                if (rdr2["Amt"] != DBNull.Value)
    //                {
    //                    DtlsAmt = Convert.ToDouble(decimal.Parse((rdr2["Amt"]).ToString()).ToString("N3"));
    //                }
    //            }

    //            double PayAmy_M = 0;
    //            PayAmy_M = Convert.ToDouble(rdr["PayAmt"]);
    //            TotalPayAmy += Math.Round((DtlsAmt + PayAmy_M), 2);
    //        }
    //    }
    //    catch (Exception ex)
    //    {

    //    }
    //    finally
    //    {
    //        con.Close();
    //    }

    //    return TotalPayAmy;
    //}

    ///////////////



///// Supplier wise Total Payment from Bank voucher payment master

    public double getTotPay(int CompId, string GetSupCode, int FinYearId)
    {
        
        double TotPaid = 0;
        string connStr = this.Connection();
        SqlConnection con = new SqlConnection(connStr);
        try
        {
            
            string strPaidAmt = "SELECT Sum(tblACC_BankVoucher_Payment_Master.PayAmt) As Payment FROM tblACC_BankVoucher_Payment_Master where tblACC_BankVoucher_Payment_Master.PayTo ='" + GetSupCode + "' And tblACC_BankVoucher_Payment_Master.CompId='" + CompId + "' And tblACC_BankVoucher_Payment_Master.FinYearId<='" + FinYearId + "'";
            SqlCommand cmdPaidAmt = new SqlCommand(strPaidAmt, con);
            SqlDataAdapter DAPaidAmt = new SqlDataAdapter(cmdPaidAmt);
            DataSet DSPaidAmt = new DataSet();
            DAPaidAmt.Fill(DSPaidAmt);
            if (DSPaidAmt.Tables[0].Rows.Count > 0 && DSPaidAmt.Tables[0].Rows[0][0] != DBNull.Value)
            {
                TotPaid = Convert.ToDouble(DSPaidAmt.Tables[0].Rows[0][0]);
            }

        }

        catch
        {
        }
        return TotPaid;
          
    }

    ////////////// PO return on amnd

    public List<int> removeDuplicates(List<int> inputList)
    {

        Dictionary<int, int> uniqueStore = new Dictionary<int, int>();
        List<int> finalList = new List<int>();
        foreach (var currValue in inputList)
        {

            if (!uniqueStore.ContainsKey(currValue))
            {
                uniqueStore.Add(currValue, 0);
                finalList.Add(currValue);
            }

        }
        return finalList;

    }




public string GetRandomAlphaNumeric()
    {
        return Path.GetRandomFileName().Replace(".", "").Substring(0, 8);
    } 
   
///////////////////////////////////////////// BoM Search All/Manf/Boughtout


    string z1 = string.Empty;
    List<string> listBom = new List<string>();
    public List<string> BOMTree_Search(string WONo, int Pid, int Cid)
    {
        string connStr = this.Connection();
        SqlConnection con = new SqlConnection(connStr);
        try
        {
            DataSet DS = new DataSet();
            string cmdStr = this.select("ItemId", "tblDG_BOM_Master", "WONo='" + WONo + "' AND PId='" + Pid + "'And CId='" + Cid + "'");
            SqlCommand cmd = new SqlCommand(cmdStr, con);
            SqlDataAdapter DA = new SqlDataAdapter(cmd);
            DA.Fill(DS, "tblDG_BOM_Master");
            z1 = DS.Tables[0].Rows[0]["ItemId"].ToString();
            listBom.Add(z1);
            if (Pid > 0)
            {
                DataSet DS2 = new DataSet();
                string cmdStr2 = this.select("PId", "tblDG_BOM_Master", "WONo='" + WONo + "' AND CId='" + Pid + "'");
                SqlCommand cmd2 = new SqlCommand(cmdStr2, con);
                SqlDataAdapter DA2 = new SqlDataAdapter(cmd2);
                DA2.Fill(DS2, "tblDG_BOM_Master");
                int Rpid = Convert.ToInt32(DS2.Tables[0].Rows[0][0]);
                this.BOMTree_Search(WONo, Rpid, Pid);
            }

            if (Pid == 0)
            {
                DataSet DS2 = new DataSet();
                string cmdStr2 = this.select("ItemId", "tblDG_BOM_Master", "WONo='" + WONo + "' AND  PId=0");
                SqlCommand cmd2 = new SqlCommand(cmdStr2, con);
                SqlDataAdapter DA2 = new SqlDataAdapter(cmd2);
                DA2.Fill(DS2, "tblDG_BOM_Master");
                z1 = DS2.Tables[0].Rows[0]["ItemId"].ToString();
                listBom.Add(z1);
            }
        }
        catch (Exception ex)
        {

        }
        return listBom;
    }

    // Budget Calculation  including Autorize po in calculation
    //public double getTotal_PO_Budget_Amt(int compid, int accid, int prspr, int wodept, string wono, int dept, int authnon, int BasicTax)
    //{
    //    /* authnon - PO is Auth or Non Auth 
    //       prspr - 0 for PR & 1 for SPR
    //       wodept - 0 do not include any wo or dept, 1 include wo or dept
    //       wono - wo no 
    //       dept - dept id
    //       accid - A/c Id
    //       BasicTax - 0 Basic Amt & 1 Basic Disc Amt & 2 Only Tax Amt & 3 Basic + Disc + Tax Amt
    //       RtnType - 
    //     */

    //    string includeWODept = "";
    //    string connStr = this.Connection();
    //    SqlConnection con = new SqlConnection(connStr);
    //    con.Open();
    //    double Amt = 0;
    //    try
    //    {
    //        if (prspr == 0)
    //        {

    //            string sqlPO = this.select("tblMM_PO_Details.PRId,tblMM_PO_Details.PRNo,tblMM_PO_Details.Qty,tblMM_PO_Details.Rate,tblMM_PO_Details.Discount,tblMM_PO_Details.PF,tblMM_PO_Details.ExST,tblMM_PO_Details.VAT", "tblMM_PO_Master,tblMM_PO_Details", "tblMM_PO_Master.CompId='" + compid + "' AND tblMM_PO_Details.BudgetCode='" + accid + "' AND tblMM_PO_Master.PONo=tblMM_PO_Details.PONo AND tblMM_PO_Master.PRSPRFlag='" + prspr + "' AND tblMM_PO_Master.Authorize='" + authnon + "'");

    //            SqlCommand cmd = new SqlCommand(sqlPO, con);
    //            SqlDataAdapter DA = new SqlDataAdapter(cmd);
    //            DataSet DS = new DataSet();
    //            DA.Fill(DS);

    //            if (DS.Tables[0].Rows.Count > 0)
    //            {
    //                for (int u = 0; u < DS.Tables[0].Rows.Count; u++)
    //                {
    //                    includeWODept = "";
    //                    if (wodept == 1)
    //                    {
    //                        includeWODept = " AND tblMM_PR_Master.WONo='" + wono + "'";
    //                    }

    //                    string sqlPRSPR = "";

    //                    sqlPRSPR = this.select("tblMM_PR_Master.WONo", "tblMM_PR_Master,tblMM_PR_Details", "tblMM_PR_Master.PRNo='" + DS.Tables[0].Rows[u]["PRNo"].ToString() + "' AND tblMM_PR_Details.Id='" + DS.Tables[0].Rows[u]["PRId"].ToString() + "' AND tblMM_PR_Master.Id=tblMM_PR_Details.MId AND tblMM_PR_Master.CompId='" + compid + "'" + includeWODept + "");


    //                    SqlCommand cmd2 = new SqlCommand(sqlPRSPR, con);
    //                    SqlDataAdapter DA2 = new SqlDataAdapter(cmd2);
    //                    DataSet DS2 = new DataSet();
    //                    DA2.Fill(DS2);

    //                    if (DS2.Tables[0].Rows.Count > 0)
    //                    {

    //                        {
    //                            if (BasicTax == 0)
    //                            {
    //                                Amt += this.CalBasicAmt(Convert.ToDouble(decimal.Parse(DS.Tables[0].Rows[u]["Qty"].ToString()).ToString("N3")), Convert.ToDouble(decimal.Parse(DS.Tables[0].Rows[u]["Rate"].ToString()).ToString("N2")));

    //                            }

    //                            if (BasicTax == 1)
    //                            {
    //                                Amt += this.CalDiscAmt(Convert.ToDouble(decimal.Parse(DS.Tables[0].Rows[u]["Qty"].ToString()).ToString("N3")), Convert.ToDouble(decimal.Parse(DS.Tables[0].Rows[u]["Rate"].ToString()).ToString("N2")), Convert.ToDouble(decimal.Parse(DS.Tables[0].Rows[u]["Discount"].ToString()).ToString("N2")));

    //                            }

    //                            if (BasicTax == 2)
    //                            {
    //                                string sqlPF = this.select("tblPacking_Master.Value", "tblPacking_Master", "tblPacking_Master.Id='" + DS.Tables[0].Rows[u]["PF"].ToString() + "'");

    //                                SqlCommand cmd3 = new SqlCommand(sqlPF, con);
    //                                SqlDataAdapter DA3 = new SqlDataAdapter(cmd3);
    //                                DataSet DS3 = new DataSet();
    //                                DA3.Fill(DS3);

    //                                double PF = Convert.ToDouble(decimal.Parse(DS3.Tables[0].Rows[0]["Value"].ToString()).ToString("N3"));

    //                                string sqlExSer = this.select("tblExciseser_Master.Value", "tblExciseser_Master", "tblExciseser_Master.Id='" + DS.Tables[0].Rows[u]["ExST"].ToString() + "'");

    //                                SqlCommand cmd4 = new SqlCommand(sqlExSer, con);
    //                                SqlDataAdapter DA4 = new SqlDataAdapter(cmd4);
    //                                DataSet DS4 = new DataSet();
    //                                DA4.Fill(DS4);

    //                                double ExSer = Convert.ToDouble(decimal.Parse(DS4.Tables[0].Rows[0]["Value"].ToString()).ToString("N3"));


    //                                string sqlvat = this.select("tblVAT_Master.Value", "tblVAT_Master", "tblVAT_Master.Id='" + DS.Tables[0].Rows[u]["VAT"].ToString() + "'");
    //                                SqlCommand cmd5 = new SqlCommand(sqlvat, con);
    //                                SqlDataAdapter DA5 = new SqlDataAdapter(cmd5);
    //                                DataSet DS5 = new DataSet();
    //                                DA5.Fill(DS5);

    //                                double Vat = Convert.ToDouble(decimal.Parse(DS5.Tables[0].Rows[0]["Value"].ToString()).ToString("N3"));

    //                                Amt += this.CalTaxAmt(Convert.ToDouble(decimal.Parse(DS.Tables[0].Rows[u]["Qty"].ToString()).ToString("N3")), Convert.ToDouble(decimal.Parse(DS.Tables[0].Rows[u]["Rate"].ToString()).ToString("N2")), Convert.ToDouble(decimal.Parse(DS.Tables[0].Rows[u]["Discount"].ToString()).ToString("N2")), PF, ExSer, Vat);

    //                            }

    //                            if (BasicTax == 3)
    //                            {
    //                                double CalBasicAmt = this.CalBasicAmt(Convert.ToDouble(decimal.Parse(DS.Tables[0].Rows[u]["Qty"].ToString()).ToString("N3")), Convert.ToDouble(decimal.Parse(DS.Tables[0].Rows[u]["Rate"].ToString()).ToString("N2")));
    //                                double CalOnlyTax = this.CalDiscAmt(Convert.ToDouble(decimal.Parse(DS.Tables[0].Rows[u]["Qty"].ToString()).ToString("N3")), Convert.ToDouble(decimal.Parse(DS.Tables[0].Rows[u]["Rate"].ToString()).ToString("N2")), Convert.ToDouble(decimal.Parse(DS.Tables[0].Rows[u]["Discount"].ToString()).ToString("N2")));

    //                                Amt += CalBasicAmt + CalOnlyTax;
    //                            }

    //                        }
    //                    }
    //                }
    //            }
    //        }



    //        if (prspr == 1)
    //        {
    //            string x = "";

    //            if (wodept == 1)
    //            {
    //                if (dept == 0)
    //                {
    //                    x = " AND tblMM_PO_Details.BudgetCode='" + accid + "'";
    //                }
    //                else
    //                {
    //                    x = " AND tblMM_PO_Details.BudgetCode='0'";
    //                }
    //            }

    //            string sqlPO = this.select("tblMM_PO_Details.SPRId,tblMM_PO_Details.SPRNo,tblMM_PO_Details.Qty,tblMM_PO_Details.Rate,tblMM_PO_Details.Discount,tblMM_PO_Details.PF,tblMM_PO_Details.ExST,tblMM_PO_Details.VAT", "tblMM_PO_Master,tblMM_PO_Details", "tblMM_PO_Master.CompId='" + compid + "' AND tblMM_PO_Master.PONo=tblMM_PO_Details.PONo AND tblMM_PO_Master.PRSPRFlag='" + prspr + "' AND tblMM_PO_Master.Authorize='" + authnon + "' " + x);

    //            SqlCommand cmd = new SqlCommand(sqlPO, con);
    //            SqlDataAdapter DA = new SqlDataAdapter(cmd);
    //            DataSet DS = new DataSet();
    //            DA.Fill(DS);

    //            if (DS.Tables[0].Rows.Count > 0)
    //            {
    //                for (int u = 0; u < DS.Tables[0].Rows.Count; u++)
    //                {
    //                    includeWODept = "";

    //                    if (wodept == 1)
    //                    {
    //                        if (dept == 0)
    //                        {
    //                            includeWODept = " AND tblMM_SPR_Details.WONo='" + wono + "'";
    //                        }
    //                        else
    //                        {
    //                            includeWODept = " AND tblMM_SPR_Details.DeptId='" + dept + "'";
    //                        }
    //                    }


    //                    string sqlPRSPR = "";

    //                    sqlPRSPR = this.select("tblMM_SPR_Details.DeptId,tblMM_SPR_Details.WONo", "tblMM_SPR_Master,tblMM_SPR_Details", "tblMM_SPR_Master.SPRNo='" + DS.Tables[0].Rows[u]["SPRNo"].ToString() + "' AND tblMM_SPR_Details.Id='" + DS.Tables[0].Rows[u]["SPRId"].ToString() + "'  AND tblMM_SPR_Master.Id=tblMM_SPR_Details.MId AND tblMM_SPR_Master.CompId='" + compid + "'" + includeWODept + "");

    //                    SqlCommand cmd2 = new SqlCommand(sqlPRSPR, con);
    //                    SqlDataAdapter DA2 = new SqlDataAdapter(cmd2);
    //                    DataSet DS2 = new DataSet();
    //                    DA2.Fill(DS2);

    //                    if (DS2.Tables[0].Rows.Count > 0)
    //                    {
    //                        // if (Convert.ToInt32(DS2.Tables[0].Rows[0]["DeptId"]) == accid)
    //                        {
    //                            if (BasicTax == 0)
    //                            {
    //                                Amt += this.CalBasicAmt(Convert.ToDouble(decimal.Parse(DS.Tables[0].Rows[u]["Qty"].ToString()).ToString("N3")), Convert.ToDouble(decimal.Parse(DS.Tables[0].Rows[u]["Rate"].ToString()).ToString("N2")));
    //                            }

    //                            if (BasicTax == 1)
    //                            {
    //                                Amt += this.CalDiscAmt(Convert.ToDouble(decimal.Parse(DS.Tables[0].Rows[u]["Qty"].ToString()).ToString("N3")), Convert.ToDouble(decimal.Parse(DS.Tables[0].Rows[u]["Rate"].ToString()).ToString("N2")), Convert.ToDouble(decimal.Parse(DS.Tables[0].Rows[u]["Discount"].ToString()).ToString("N2")));

    //                            }

    //                            if (BasicTax == 2)
    //                            {
    //                                string sqlPF = this.select("tblPacking_Master.Value", "tblPacking_Master", "tblPacking_Master.Id='" + DS.Tables[0].Rows[u]["PF"].ToString() + "'");

    //                                SqlCommand cmd3 = new SqlCommand(sqlPF, con);
    //                                SqlDataAdapter DA3 = new SqlDataAdapter(cmd3);
    //                                DataSet DS3 = new DataSet();
    //                                DA3.Fill(DS3);

    //                                double PF = Convert.ToDouble(decimal.Parse(DS3.Tables[0].Rows[0]["Value"].ToString()).ToString("N2"));

    //                                string sqlExSer = this.select("tblExciseser_Master.Value", "tblExciseser_Master", "tblExciseser_Master.Id='" + DS.Tables[0].Rows[u]["ExST"].ToString() + "'");

    //                                SqlCommand cmd4 = new SqlCommand(sqlExSer, con);
    //                                SqlDataAdapter DA4 = new SqlDataAdapter(cmd4);
    //                                DataSet DS4 = new DataSet();
    //                                DA4.Fill(DS4);

    //                                double ExSer = Convert.ToDouble(decimal.Parse(DS4.Tables[0].Rows[0]["Value"].ToString()).ToString("N2"));


    //                                string sqlvat = this.select("tblVAT_Master.Value", "tblVAT_Master", "tblVAT_Master.Id='" + DS.Tables[0].Rows[u]["VAT"].ToString() + "'");
    //                                SqlCommand cmd5 = new SqlCommand(sqlvat, con);
    //                                SqlDataAdapter DA5 = new SqlDataAdapter(cmd5);
    //                                DataSet DS5 = new DataSet();
    //                                DA5.Fill(DS5);

    //                                double Vat = Convert.ToDouble(decimal.Parse(DS5.Tables[0].Rows[0]["Value"].ToString()).ToString("N2"));

    //                                Amt += this.CalTaxAmt(Convert.ToDouble(decimal.Parse(DS.Tables[0].Rows[u]["Qty"].ToString()).ToString("N3")), Convert.ToDouble(decimal.Parse(DS.Tables[0].Rows[u]["Rate"].ToString()).ToString("N2")), Convert.ToDouble(decimal.Parse(DS.Tables[0].Rows[u]["Discount"].ToString()).ToString("N2")), PF, ExSer, Vat);



    //                            }

    //                            if (BasicTax == 3)
    //                            {
    //                                double CalBasicAmt = this.CalBasicAmt(Convert.ToDouble(decimal.Parse(DS.Tables[0].Rows[u]["Qty"].ToString()).ToString("N3")), Convert.ToDouble(decimal.Parse(DS.Tables[0].Rows[u]["Rate"].ToString()).ToString("N2")));

    //                                double CalOnlyTax = this.CalDiscAmt(Convert.ToDouble(decimal.Parse(DS.Tables[0].Rows[u]["Qty"].ToString()).ToString("N3")), Convert.ToDouble(decimal.Parse(DS.Tables[0].Rows[u]["Rate"].ToString()).ToString("N2")), Convert.ToDouble(decimal.Parse(DS.Tables[0].Rows[u]["Discount"].ToString()).ToString("N2")));

    //                                Amt += CalBasicAmt + CalOnlyTax;
    //                            }

    //                        }

    //                    }
    //                }
    //            }
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //    }

    //    return Math.Round(Amt, 2);
    //    con.Close();
    //} 
















    //public double GQNQTY_PO(int CompId, int PoId, int ItemId, int Flag)
    //{
    //    double gqnQty = 0;

    //    try
    //    {
    //        string connStr = this.Connection();
    //        SqlConnection con = new SqlConnection(connStr);
    //        string SqlprGqnQty = string.Empty;
    //        if (Flag == 0)
    //        {
    //            SqlprGqnQty = "SELECT GQNQty from view_Cal_Sum_GQN_PR where ItemId='" + ItemId + "'And CompId='" + CompId + "' And POId='"+PoId+"'";
                
    //        }
    //        else if (Flag == 1)
    //        {
    //            SqlprGqnQty = "SELECT GQNQty from view_Cal_Sum_GQN_SPR where ItemId='" + ItemId + "'And CompId='" + CompId + "'And POId='" + PoId + "'";                

    //        }

    //        SqlCommand CmdprGqnQty = new SqlCommand(SqlprGqnQty, con);

    //        SqlDataAdapter DAprGqnQty = new SqlDataAdapter(CmdprGqnQty);
    //        DataSet DSprGqnQty = new DataSet();
    //        DAprGqnQty.Fill(DSprGqnQty);

    //        if (DSprGqnQty.Tables[0].Rows.Count > 0 && DSprGqnQty.Tables[0].Rows[0][0] != DBNull.Value)
    //        {

    //            gqnQty = Math.Round(Convert.ToDouble(decimal.Parse(DSprGqnQty.Tables[0].Rows[0][0].ToString()).ToString("N3")), 5);


    //        }

    //        return gqnQty;

    //    }

    //    catch (Exception ex)
    //    {
    //    }

    //    return gqnQty;
    //}


    //public double GQN_Reject_QTY_PO(int CompId, int PoId, int ItemId, int Flag)
    //{
    //    double gqnQty = 0;

    //    try
    //    {
    //        string connStr = this.Connection();
    //        SqlConnection con = new SqlConnection(connStr);
    //        string SqlprGqnQty = string.Empty;
    //        if (Flag == 0)
    //        {

    //            SqlprGqnQty = "SELECT RejQty from view_Cal_Sum_GQN_PR where ItemId='" + ItemId + "'And CompId='" + CompId + "'And POId='" + PoId + "'";
                
    //        }
    //        else if (Flag == 1)
    //        {
    //            SqlprGqnQty = "SELECT RejQty from view_Cal_Sum_GQN_SPR where ItemId='" + ItemId + "'And CompId='" + CompId + "'And POId='" + PoId + "'";                

    //        }

    //        SqlCommand CmdprGqnQty = new SqlCommand(SqlprGqnQty, con);

    //        SqlDataAdapter DAprGqnQty = new SqlDataAdapter(CmdprGqnQty);
    //        DataSet DSprGqnQty = new DataSet();
    //        DAprGqnQty.Fill(DSprGqnQty);

    //        if (DSprGqnQty.Tables[0].Rows.Count > 0 && DSprGqnQty.Tables[0].Rows[0][0] != DBNull.Value)
    //        {

    //            gqnQty = Math.Round(Convert.ToDouble(decimal.Parse(DSprGqnQty.Tables[0].Rows[0][0].ToString()).ToString("N3")), 5);


    //        }

    //        return gqnQty;

    //    }

    //    catch (Exception ex)
    //    {
    //    }

    //    return gqnQty;
    //}


    //public double GINQTY_PO(int CompId, int PoId, int ItemId, int Flag)
    //{
    //    double gqnQty = 0;

    //    try
    //    {
    //        string connStr = this.Connection();
    //        SqlConnection con = new SqlConnection(connStr);
    //        string SqlprGqnQty = string.Empty;
    //        if (Flag == 0)
    //        {

    //            SqlprGqnQty = "SELECT sum(tblInv_Inward_Details.ReceivedQty) as sum_ReceivedQty FROM tblMM_PR_Details INNER JOIN               tblMM_PR_Master ON tblMM_PR_Details.MId = tblMM_PR_Master.Id INNER JOIN tblMM_PO_Details INNER JOIN tblMM_PO_Master ON tblMM_PO_Details.MId = tblMM_PO_Master.Id  INNER JOIN tblInv_Inward_Master INNER JOIN tblInv_Inward_Details ON tblInv_Inward_Master.Id = tblInv_Inward_Details.GINId ON tblInv_Inward_Details.POId = '" + PoId + "' And tblMM_PO_Details.Id = tblInv_Inward_Details.POId ON tblMM_PR_Details.Id = tblMM_PO_Details.PRId And tblMM_PR_Details.ItemId ='" + ItemId + "'And tblMM_PO_Master.PRSPRFlag='0' And tblInv_Inward_Master.CompId='" + CompId + "'";
    //        }
    //        else if (Flag == 1)
    //        {

    //            SqlprGqnQty = "SELECT sum(tblInv_Inward_Details.ReceivedQty) as sum_ReceivedQty FROM tblMM_SPR_Details INNER JOIN               tblMM_SPR_Master ON tblMM_SPR_Details.MId = tblMM_SPR_Master.Id INNER JOIN tblMM_PO_Details INNER JOIN tblMM_PO_Master ON tblMM_PO_Details.MId = tblMM_PO_Master.Id  INNER JOIN tblInv_Inward_Master INNER JOIN tblInv_Inward_Details ON tblInv_Inward_Master.Id = tblInv_Inward_Details.GINId ON tblInv_Inward_Details.POId = '" + PoId + "' And tblMM_PO_Details.Id = tblInv_Inward_Details.POId ON tblMM_SPR_Details.Id = tblMM_PO_Details.SPRId And tblMM_SPR_Details.ItemId ='" + ItemId + "'And tblMM_PO_Master.PRSPRFlag='1' And tblInv_Inward_Master.CompId='" + CompId + "'";

    //        }

    //        SqlCommand CmdprGqnQty = new SqlCommand(SqlprGqnQty, con);
    //        SqlDataAdapter DAprGqnQty = new SqlDataAdapter(CmdprGqnQty);
    //        DataSet DSprGqnQty = new DataSet();
    //        DAprGqnQty.Fill(DSprGqnQty);

    //        if (DSprGqnQty.Tables[0].Rows.Count > 0 && DSprGqnQty.Tables[0].Rows[0][0] != DBNull.Value)
    //        {
    //            gqnQty = Math.Round(Convert.ToDouble(decimal.Parse(DSprGqnQty.Tables[0].Rows[0][0].ToString()).ToString("N3")), 5);

    //        }

    //        return gqnQty;

    //    }

    //    catch (Exception ex)
    //    {
    //    }

    //    return gqnQty;
    //}




    
    // Budget Calculation
    public double getTotal_PO_Budget_Amt(int compid, int accid, int prspr, int wodept, string wono, int dept, int BasicTax)
    {

        string connStr = this.Connection();
        SqlConnection con = new SqlConnection(connStr);
        con.Open();
        double Amt = 0;
        try
        {
            if (prspr == 0)
            {
                string includeWODept = "";
                if (wodept == 1)
                {
                    includeWODept = " AND tblMM_PR_Master.WONo='" + wono + "'";
                }
                string sqlPO = "SELECT tblMM_PO_Details.Qty, tblMM_PO_Details.Rate, tblMM_PO_Details.Discount, tblVAT_Master.Value As VAT,tblExciseser_Master.Value AS Excise, tblPacking_Master.Value AS PF FROM tblMM_PR_Details INNER JOIN tblMM_PR_Master ON tblMM_PR_Details.MId = tblMM_PR_Master.Id INNER JOIN tblMM_PO_Details INNER JOIN tblMM_PO_Master ON tblMM_PO_Details.MId = tblMM_PO_Master.Id ON tblMM_PR_Details.Id = tblMM_PO_Details.PRId INNER JOIN tblVAT_Master ON tblMM_PO_Details.VAT = tblVAT_Master.Id INNER JOIN tblExciseser_Master ON tblMM_PO_Details.ExST = tblExciseser_Master.Id INNER JOIN tblPacking_Master ON tblMM_PO_Details.PF = tblPacking_Master.Id And tblMM_PO_Master.CompId='" + compid + "' AND tblMM_PO_Details.BudgetCode='" + accid + "' AND tblMM_PO_Master.PONo=tblMM_PO_Details.PONo AND tblMM_PO_Master.PRSPRFlag='" + prspr + "'" + includeWODept + " ";
                SqlCommand cmd = new SqlCommand(sqlPO, con);
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {

                    if (BasicTax == 0)
                    {
                        Amt += this.CalBasicAmt(Convert.ToDouble(decimal.Parse(rdr["Qty"].ToString()).ToString("N3")), Convert.ToDouble(decimal.Parse(rdr["Rate"].ToString()).ToString("N2")));

                    }

                    if (BasicTax == 1)
                    {
                        Amt += this.CalDiscAmt(Convert.ToDouble(decimal.Parse(rdr["Qty"].ToString()).ToString("N3")), Convert.ToDouble(decimal.Parse(rdr["Rate"].ToString()).ToString("N2")), Convert.ToDouble(decimal.Parse(rdr["Discount"].ToString()).ToString("N2")));

                    }
                    if (BasicTax == 2)
                    {
                        double PF = Convert.ToDouble(decimal.Parse(rdr["PF"].ToString()).ToString("N3"));
                        double ExSer = Convert.ToDouble(decimal.Parse(rdr["Excise"].ToString()).ToString("N3"));
                        double Vat = Convert.ToDouble(decimal.Parse(rdr["VAT"].ToString()).ToString("N3"));
                        Amt += this.CalTaxAmt(Convert.ToDouble(decimal.Parse(rdr["Qty"].ToString()).ToString("N3")), Convert.ToDouble(decimal.Parse(rdr["Rate"].ToString()).ToString("N2")), Convert.ToDouble(decimal.Parse(rdr["Discount"].ToString()).ToString("N2")), PF, ExSer, Vat);

                    }

                    if (BasicTax == 3)
                    {
                        double CalBasicAmt = this.CalBasicAmt(Convert.ToDouble(decimal.Parse(rdr["Qty"].ToString()).ToString("N3")), Convert.ToDouble(decimal.Parse(rdr["Rate"].ToString()).ToString("N2")));
                        double CalOnlyTax = this.CalDiscAmt(Convert.ToDouble(decimal.Parse(rdr["Qty"].ToString()).ToString("N3")), Convert.ToDouble(decimal.Parse(rdr["Rate"].ToString()).ToString("N2")), Convert.ToDouble(decimal.Parse(rdr["Discount"].ToString()).ToString("N2")));
                        Amt += CalBasicAmt + CalOnlyTax;
                    }
                }
            }



            if (prspr == 1)
            {
                string x = "";
                string includeWODept = "";
                if (wodept == 1)
                {
                    if (dept == 0)
                    {
                        x = " AND tblMM_PO_Details.BudgetCode='" + accid + "'";
                        includeWODept = " AND tblMM_SPR_Details.WONo='" + wono + "'";
                    }
                    else
                    {
                        x = " AND tblMM_PO_Details.BudgetCode='0'";
                        includeWODept = " AND tblMM_SPR_Details.DeptId='" + dept + "'";
                    }
                }

                string sqlPO = "SELECT tblMM_PO_Details.Qty, tblMM_PO_Details.Rate, tblMM_PO_Details.Discount,  tblVAT_Master.Value As VAT,tblExciseser_Master.Value AS Excise, tblPacking_Master.Value AS PF FROM tblMM_SPR_Details INNER JOIN tblMM_SPR_Master ON tblMM_SPR_Details.MId = tblMM_SPR_Master.Id INNER JOIN tblMM_PO_Details INNER JOIN tblMM_PO_Master ON tblMM_PO_Details.MId = tblMM_PO_Master.Id ON tblMM_SPR_Details.Id = tblMM_PO_Details.SPRId INNER JOIN tblVAT_Master ON tblMM_PO_Details.VAT = tblVAT_Master.Id INNER JOIN tblExciseser_Master ON tblMM_PO_Details.ExST = tblExciseser_Master.Id INNER JOIN tblPacking_Master ON tblMM_PO_Details.PF = tblPacking_Master.Id And tblMM_PO_Master.CompId='" + compid + "'  AND tblMM_PO_Master.PONo=tblMM_PO_Details.PONo AND tblMM_PO_Master.PRSPRFlag='" + prspr + "'" + includeWODept + "" + x + " ";
                SqlCommand cmd = new SqlCommand(sqlPO, con);
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {

                    if (BasicTax == 0)
                    {
                        Amt += this.CalBasicAmt(Convert.ToDouble(decimal.Parse(rdr["Qty"].ToString()).ToString("N3")), Convert.ToDouble(decimal.Parse(rdr["Rate"].ToString()).ToString("N2")));

                    }

                    if (BasicTax == 1)
                    {
                        Amt += this.CalDiscAmt(Convert.ToDouble(decimal.Parse(rdr["Qty"].ToString()).ToString("N3")), Convert.ToDouble(decimal.Parse(rdr["Rate"].ToString()).ToString("N2")), Convert.ToDouble(decimal.Parse(rdr["Discount"].ToString()).ToString("N2")));

                    }
                    if (BasicTax == 2)
                    {
                        double PF = Convert.ToDouble(decimal.Parse(rdr["PF"].ToString()).ToString("N3"));
                        double ExSer = Convert.ToDouble(decimal.Parse(rdr["Excise"].ToString()).ToString("N3"));
                        double Vat = Convert.ToDouble(decimal.Parse(rdr["VAT"].ToString()).ToString("N3"));
                        Amt += this.CalTaxAmt(Convert.ToDouble(decimal.Parse(rdr["Qty"].ToString()).ToString("N3")), Convert.ToDouble(decimal.Parse(rdr["Rate"].ToString()).ToString("N2")), Convert.ToDouble(decimal.Parse(rdr["Discount"].ToString()).ToString("N2")), PF, ExSer, Vat);

                    }

                    if (BasicTax == 3)
                    {
                        double CalBasicAmt = this.CalBasicAmt(Convert.ToDouble(decimal.Parse(rdr["Qty"].ToString()).ToString("N3")), Convert.ToDouble(decimal.Parse(rdr["Rate"].ToString()).ToString("N2")));
                        double CalOnlyTax = this.CalDiscAmt(Convert.ToDouble(decimal.Parse(rdr["Qty"].ToString()).ToString("N3")), Convert.ToDouble(decimal.Parse(rdr["Rate"].ToString()).ToString("N2")), Convert.ToDouble(decimal.Parse(rdr["Discount"].ToString()).ToString("N2")));

                        Amt += CalBasicAmt + CalOnlyTax;
                    }
                }

            }
            con.Close();
        }
        catch (Exception ex)
        {
        }

        return Math.Round(Amt, 2);

    } 



    public double GQNQTY_PO(int CompId, int PoId, int ItemId, int Flag)
    {
        double gqnQty = 0;

        try
        {
            string connStr = this.Connection();
            SqlConnection con = new SqlConnection(connStr);
            con.Open();
            string SqlprGqnQty = string.Empty;
            if (Flag == 0)
            {
                SqlprGqnQty = "SELECT Sum(tblQc_MaterialQuality_Details.AcceptedQty)FROM tblQc_MaterialQuality_Details INNER JOIN               tblinv_MaterialReceived_Details ON tblQc_MaterialQuality_Details.GRRId = tblinv_MaterialReceived_Details.Id INNER JOIN                    tblMM_PO_Details ON tblinv_MaterialReceived_Details.POId = tblMM_PO_Details.Id INNER JOIN tblMM_PR_Details ON tblMM_PO_Details.PRId = tblMM_PR_Details.Id and tblMM_PR_Details.ItemId='" + ItemId + "' And tblinv_MaterialReceived_Details.POId='" + PoId + "'";

            }
            else if (Flag == 1)
            {
                SqlprGqnQty = "SELECT  Sum(tblQc_MaterialQuality_Details.AcceptedQty)FROM tblQc_MaterialQuality_Details INNER JOIN                     tblinv_MaterialReceived_Details ON tblQc_MaterialQuality_Details.GRRId = tblinv_MaterialReceived_Details.Id INNER JOIN                     tblMM_PO_Details ON tblinv_MaterialReceived_Details.POId = tblMM_PO_Details.Id INNER JOIN tblMM_SPR_Details ON tblMM_PO_Details.SPRId = tblMM_SPR_Details.Id and tblMM_SPR_Details.ItemId='" + ItemId + "' And tblinv_MaterialReceived_Details.POId='" + PoId + "'";

            }
            SqlCommand CmdprGqnQty = new SqlCommand(SqlprGqnQty, con);
            SqlDataReader rdr = CmdprGqnQty.ExecuteReader();
            while (rdr.Read())
            {
                if (rdr[0] != DBNull.Value)
                {
                    gqnQty = Math.Round(Convert.ToDouble(rdr[0]), 3);
                }
            }
            con.Close();
            return gqnQty;

        }

        catch (Exception ex)
        {
        }

        return gqnQty;
    }


    public double GQN_Reject_QTY_PO(int CompId, int PoId, int ItemId, int Flag)
    {
        double gqnQty = 0;

        try
        {
            string connStr = this.Connection();
            SqlConnection con = new SqlConnection(connStr);
            con.Open();
            string SqlprGqnQty = string.Empty;


            if (Flag == 0)
            {

                SqlprGqnQty = "SELECT Sum(tblQc_MaterialQuality_Details.RejectedQty)FROM tblQc_MaterialQuality_Details INNER JOIN               tblinv_MaterialReceived_Details ON tblQc_MaterialQuality_Details.GRRId = tblinv_MaterialReceived_Details.Id INNER JOIN                    tblMM_PO_Details ON tblinv_MaterialReceived_Details.POId = tblMM_PO_Details.Id INNER JOIN tblMM_PR_Details ON tblMM_PO_Details.PRId = tblMM_PR_Details.Id and tblMM_PR_Details.ItemId='" + ItemId + "' And tblinv_MaterialReceived_Details.POId='" + PoId + "'";

            }
            else if (Flag == 1)
            {
                SqlprGqnQty = "SELECT  Sum(tblQc_MaterialQuality_Details.RejectedQty)FROM tblQc_MaterialQuality_Details INNER JOIN                     tblinv_MaterialReceived_Details ON tblQc_MaterialQuality_Details.GRRId = tblinv_MaterialReceived_Details.Id INNER JOIN                     tblMM_PO_Details ON tblinv_MaterialReceived_Details.POId = tblMM_PO_Details.Id INNER JOIN tblMM_SPR_Details ON tblMM_PO_Details.SPRId = tblMM_SPR_Details.Id and tblMM_SPR_Details.ItemId='" + ItemId + "' And tblinv_MaterialReceived_Details.POId='" + PoId + "'";


            }

            SqlCommand CmdprGqnQty = new SqlCommand(SqlprGqnQty, con);
            SqlDataReader rdr = CmdprGqnQty.ExecuteReader();
            while (rdr.Read())
            {
                if (rdr[0] != DBNull.Value)
                {
                    gqnQty = Math.Round(Convert.ToDouble(rdr[0]), 3);
                }
            }
            return gqnQty;
            con.Close();
        }

        catch (Exception ex)
        {
        }

        return gqnQty;
    }


    public double GINQTY_PO(int CompId, int PoId, int ItemId, int Flag)
    {
        double gqnQty = 0;

        try
        {
            string connStr = this.Connection();
            SqlConnection con = new SqlConnection(connStr);
            con.Open();
            string SqlprGqnQty = string.Empty;
            if (Flag == 0)
            {
                SqlprGqnQty = "SELECT sum(tblInv_Inward_Details.ReceivedQty) as sum_ReceivedQty FROM tblInv_Inward_Details INNER JOIN tblMM_PO_Details on tblMM_PO_Details.Id= tblInv_Inward_Details.POId inner join tblMM_PR_Details on  tblMM_PR_Details.Id = tblMM_PO_Details.PRId And tblMM_PR_Details.ItemId ='" + ItemId + "'And tblInv_Inward_Details.POId='" + PoId + "'";

            }
            else if (Flag == 1)
            {
                SqlprGqnQty = "SELECT sum(tblInv_Inward_Details.ReceivedQty) as sum_ReceivedQty FROM tblInv_Inward_Details INNER JOIN tblMM_PO_Details on tblMM_PO_Details.Id= tblInv_Inward_Details.POId inner join tblMM_SPR_Details on  tblMM_SPR_Details.Id = tblMM_PO_Details.SPRId And tblMM_SPR_Details.ItemId ='" + ItemId + "' And tblInv_Inward_Details.POId='" + PoId + "'";


            }

            SqlCommand CmdprGqnQty = new SqlCommand(SqlprGqnQty, con);
            SqlDataReader rdr = CmdprGqnQty.ExecuteReader();
            while (rdr.Read())
            {
                if (rdr[0] != DBNull.Value)
                {
                    gqnQty = Math.Round(Convert.ToDouble(rdr[0]), 3);
                }
            }

            con.Close();
            return gqnQty;

        }

        catch (Exception ex)
        {
        }

        return gqnQty;
    }

    public double GSN_QTY_PO(int CompId, int PoId, int ItemId, int Flag)
    {
        double gqnQty = 0;
        try
        {
            string connStr = this.Connection();
            SqlConnection con = new SqlConnection(connStr);
            string SqlprGqnQty = string.Empty;
            if (Flag == 0)
            {
                SqlprGqnQty = "SELECT GSNQty from view_Cal_Sum_GSN_PR where ItemId='" + ItemId + "'And CompId='" + CompId + "'And POId='" + PoId + "'";

            }
            else if (Flag == 1)
            {
                SqlprGqnQty = "SELECT GSNQty from view_Cal_Sum_GSN_SPR where ItemId='" + ItemId + "'And CompId='" + CompId + "'And POId='" + PoId + "'";

            }

            SqlCommand CmdprGqnQty = new SqlCommand(SqlprGqnQty, con);

            SqlDataAdapter DAprGqnQty = new SqlDataAdapter(CmdprGqnQty);
            DataSet DSprGqnQty = new DataSet();
            DAprGqnQty.Fill(DSprGqnQty);

            if (DSprGqnQty.Tables[0].Rows.Count > 0 && DSprGqnQty.Tables[0].Rows[0][0] != DBNull.Value)
            {

                gqnQty = Math.Round(Convert.ToDouble(decimal.Parse(DSprGqnQty.Tables[0].Rows[0][0].ToString()).ToString("N3")), 5);


            }

            return gqnQty;

        }

        catch (Exception ex)
        {
        }

        return gqnQty;
    }


   

    public int WorkingDays(int FinYearId, int Mth)
    {
        string connStr = this.Connection();
        SqlConnection con = new SqlConnection(connStr);

        //int g1 = this.SalYrs(FyId, Mth, CompId);

        //return DateTime.DaysInMonth(g1, Mth) - (this.GetHoliday(Mth, CompId, FyId) + this.CountSundays(g1, Mth));

        string Sql = this.select("Days", "tblHR_WorkingDays", "MonthId='" + Mth + "' AND FinYearId='" + FinYearId + "'");
        SqlCommand cmd = new SqlCommand(Sql, con);
        SqlDataAdapter DA = new SqlDataAdapter(cmd);
        DataSet DS = new DataSet();
        DA.Fill(DS);

        if (DS.Tables[0].Rows.Count>0)
        {
            return Convert.ToInt32(DS.Tables[0].Rows[0][0]);
        }
        else
        {
            return 0;
        }
    }

    public int GetHoliday(int mth, int CompId, int FinYearId)
    {
        string connStr = this.Connection();
        SqlConnection con = new SqlConnection(connStr);

        string StrHoliday = this.select("*", "tblHR_Holiday_Master", "CompId='" + CompId + "' And FinYearId='" + FinYearId + "'");
        SqlCommand cmdHoliday = new SqlCommand(StrHoliday, con);
        SqlDataAdapter DAHoliday = new SqlDataAdapter(cmdHoliday);
        DataSet DSHoliday = new DataSet();
        DAHoliday.Fill(DSHoliday, "tblHR_Holiday_Master");

        int k = 0;

        for (int i = 0; i < DSHoliday.Tables[0].Rows.Count; i++)
        {
            string a = DSHoliday.Tables[0].Rows[i]["HDate"].ToString();
            string[] b = a.Split('-');
            int m = Convert.ToInt32(b[1]);

            if (m == mth)
            {
                k++;
            }
        }
        return k;
    }

    public double OTRate(double Gross, double OTHrs, double DutyHrs, double WorkDays)
    {
        //double x = 0;
        //double y = 0;
        //x = ((Gross * OTHrs) / DutyHrs);
        //y = ((x / DutyHrs) / WorkDays);
        //return y;

        double y = 0;
        y = (Gross / WorkDays / DutyHrs);
        return y;
    }

    public double OTAmt(double OTRate, double TotalHrs)
    {
        //double first = 0;
        //double second = 0;
        //string s = "";
        //s = TotalHrs.ToString();
        //if (s != "")
        //{
        //    string[] s1 = s.Split('.');

        //    for (int p = 0; p < s1.Length; p++)
        //    {
        //        switch (p)
        //        {
        //            case 0:
        //                first = Convert.ToDouble(s1[0]);
        //                break;
        //            case 1:
        //                second = Convert.ToDouble(s1[1]);
        //                break;
        //        }
        //    }
        //}

        //return Math.Round((OTRate * first) + (OTRate / 60) * second);
        return Math.Round(TotalHrs * OTRate);
    }

    public double MobileBillDetails(string EmpId, int FyId, int CompId, int Mth, int Case)
    {
        string connStr = this.Connection();
        SqlConnection con = new SqlConnection(connStr);

        double BillAmt = 0;
        double LimitAmt = 0;
        double ExcessAmt = 0;

        string sql = this.select("*", "tblHR_OfficeStaff", "CompId='" + CompId + "' and EmpId='" + EmpId + "'");

        SqlCommand cmd = new SqlCommand(sql, con);
        SqlDataAdapter da = new SqlDataAdapter(cmd);
        DataSet DS = new DataSet();
        da.Fill(DS);

        string stMobileNo = this.select("Id,MobileNo,LimitAmt", "tblHR_CoporateMobileNo", "Id='" + DS.Tables[0].Rows[0]["MobileNo"].ToString() + "'");
        SqlCommand cmdMobileNo = new SqlCommand(stMobileNo, con);
        SqlDataAdapter daMobileNo = new SqlDataAdapter(cmdMobileNo);
        DataSet dsMobileNo = new DataSet();
        daMobileNo.Fill(dsMobileNo, "tblHR_CoporateMobileNo");


        if (dsMobileNo.Tables[0].Rows.Count > 0)
        {
            LimitAmt = Convert.ToDouble(dsMobileNo.Tables[0].Rows[0]["LimitAmt"]);

            string StrMobBill = this.select("BillAmt,Taxes", "tblHR_MobileBill", "CompId='" + CompId + "' And FinYearId='" + FyId + "' And EmpId='" + EmpId + "' And BillMonth='" + Mth + "'");
            SqlCommand cmdMobBill = new SqlCommand(StrMobBill, con);
            SqlDataAdapter daMobBill = new SqlDataAdapter(cmdMobBill);
            DataSet dsMobBill = new DataSet();
            daMobBill.Fill(dsMobBill, "tblHR_CoporateMobileNo");

            if (dsMobBill.Tables[0].Rows.Count > 0)
            {
                BillAmt = Convert.ToDouble(dsMobBill.Tables[0].Rows[0]["BillAmt"]);

                string CmdTaxlbl = this.select("*", "tblExciseser_Master", "Id='" + dsMobBill.Tables[0].Rows[0]["Taxes"].ToString() + "'");
                SqlCommand Cmdtaxlbl = new SqlCommand(CmdTaxlbl, con);
                DataSet DSTaxlbl = new DataSet();
                SqlDataAdapter DATaxlbl = new SqlDataAdapter(Cmdtaxlbl);
                DATaxlbl.Fill(DSTaxlbl, "tblExciseser_Master");

                double Taxes = 0;

                if (DSTaxlbl.Tables[0].Rows.Count > 0)
                {
                    Taxes = Convert.ToDouble(DSTaxlbl.Tables[0].Rows[0]["Value"].ToString());
                }

                double CalAmt = BillAmt - (((BillAmt) * (Taxes)) / (Taxes + 100));

                if ((CalAmt - LimitAmt) > 0)
                {
                    ExcessAmt = Math.Round((CalAmt - LimitAmt));
                }
            }
        }
        double x = 0;

        switch (Case)
        {

            case 1:
                x = BillAmt;
                break;
            case 2:
                x = LimitAmt;
                break;
            case 3:
                x = ExcessAmt;
                break;
        }

        return x;

    }

    // The year (1 through 9999).
    // The month (1 through 12).
    public int CountSundays(int year, int month)
    {
        var firstDay = new DateTime(year, month, 1);

        var day28 = firstDay.AddDays(28);
        var day29 = firstDay.AddDays(29);
        var day30 = firstDay.AddDays(30);

        if ((day28.Month == month && day28.DayOfWeek == DayOfWeek.Sunday)
        || (day29.Month == month && day29.DayOfWeek == DayOfWeek.Sunday)
        || (day30.Month == month && day30.DayOfWeek == DayOfWeek.Sunday))
        {
            return 5;
        }
        else
        {
            return 4;
        }
    }

    public int SalYrs(int FinYearId, int Month, int CompId)
    {
        string connStr = this.Connection();
        SqlConnection con = new SqlConnection(connStr);

        int g1 = 0;

        string StrMonth = this.select("FinYear", "tblFinancial_master", "CompId='" + CompId + "' And FinYearId='" + FinYearId + "'");
        SqlCommand cmdMonth = new SqlCommand(StrMonth, con);
        SqlDataAdapter DAMonth = new SqlDataAdapter(cmdMonth);
        DataSet DSMonth = new DataSet();
        DAMonth.Fill(DSMonth, "Financial");

        string a = DSMonth.Tables[0].Rows[0]["FinYear"].ToString();
        string[] b = a.Split('-');
        string fy = b[0];
        string ty = b[1];

        if (Month == 1 || Month == 2 || Month == 3)
        {
            g1 = Convert.ToInt32(ty);
        }
        else
        {
            g1 = Convert.ToInt32(fy);
        }

        return g1;
    }

    public void GetMonth(DropDownList ddlMonth, int CompId, int FinYearId)
    {
        string connStr = this.Connection();
        SqlConnection con = new SqlConnection(connStr);

        //For Month 
        string StrMonth = this.select("FinYearFrom, FinYearTo", "tblFinancial_master", "CompId='" + CompId + "' And FinYearId='" + FinYearId + "'");
        SqlCommand cmdMonth = new SqlCommand(StrMonth, con);
        SqlDataAdapter DAMonth = new SqlDataAdapter(cmdMonth);
        DataSet DSMonth = new DataSet();
        DAMonth.Fill(DSMonth, "Financial");

        List<string> months = new List<string>();
        months.Clear();
        months = this.MonthRange(DSMonth.Tables[0].Rows[0]["FinYearFrom"].ToString(), DSMonth.Tables[0].Rows[0]["FinYearTo"].ToString());
        int j = 4;
        int m = 1;

        for (int k = 0; k < months.Count; k++)
        {
            if (k != 9 && k != 10 && k != 11)
            {
                ddlMonth.Items.Add(new System.Web.UI.WebControls.ListItem(months[k], j.ToString()));
                j++;
            }
            else
            {
                ddlMonth.Items.Add(new System.Web.UI.WebControls.ListItem(months[k], m.ToString()));
                m++;
            }
        }

    }

    /////////////////////////////////////////////



    public void CopyGunRailToWO(int node, string wonodest, int CompId, string SessionId, int FinYearId, int destpid, int destcid, double TCColumn, double TLongRow_with_coloumn, double TLongRow_no_coloumn, double SumLongRailColumn,double TCrossRow , double TLongRow)
    {
        string connStr = this.Connection();
        SqlConnection con = new SqlConnection(connStr);
        try
        {
            DataSet DS = new DataSet();
            string currDate = this.getCurrDate();
            string currTime = this.getCurrTime();
            int pid = destpid;
            int cid = destcid;
            con.Open();
            ///
            string getparent2 = this.select("*", "tblDG_GUNRAIL_BOM_Master", " CId='" + node + "'");
            SqlCommand checkparent2 = new SqlCommand(getparent2, con);
            SqlDataAdapter daparent2 = new SqlDataAdapter(checkparent2);
            DataSet dsparent2 = new DataSet();
            daparent2.Fill(dsparent2, "tblDG_GUNRAIL_BOM_Master");
            double value1 = 0;

            if (Convert.ToInt32(dsparent2.Tables[0].Rows[0]["CId"]) == 1)
            {
                value1 = Convert.ToDouble(decimal.Parse((TCColumn * 2).ToString()).ToString("N3"));

            }
            else if (Convert.ToInt32(dsparent2.Tables[0].Rows[0]["CId"]) == 16)
            {
                value1 = Convert.ToDouble(decimal.Parse((TCColumn * 2).ToString()).ToString("N3"));
            }
            else if (Convert.ToInt32(dsparent2.Tables[0].Rows[0]["CId"]) == 21)
            {
                value1 = Convert.ToDouble(decimal.Parse((TCColumn).ToString()).ToString("N3"));
            }
            else if (Convert.ToInt32(dsparent2.Tables[0].Rows[0]["CId"]) == 31)
            {
                value1 = Convert.ToDouble(decimal.Parse((TLongRow_with_coloumn).ToString()).ToString("N3"));
            }
            else if (Convert.ToInt32(dsparent2.Tables[0].Rows[0]["CId"]) == 35)
            {
                value1 = Convert.ToDouble(decimal.Parse((TLongRow_no_coloumn).ToString()).ToString("N3"));
            }
            else if (Convert.ToInt32(dsparent2.Tables[0].Rows[0]["CId"]) == 40)
            {
                value1 = Convert.ToDouble(decimal.Parse((TCColumn).ToString()).ToString("N3"));
            }
            else if (Convert.ToInt32(dsparent2.Tables[0].Rows[0]["CId"]) == 46)
            {
                value1 = Convert.ToDouble(decimal.Parse((TLongRow_with_coloumn + TLongRow_no_coloumn).ToString()).ToString("N3"));
            }
            else if (Convert.ToInt32(dsparent2.Tables[0].Rows[0]["CId"]) == 55)
            {
                value1 = Convert.ToDouble(decimal.Parse(((SumLongRailColumn + TCColumn) * 2).ToString()).ToString("N3"));

            }
            else if (Convert.ToInt32(dsparent2.Tables[0].Rows[0]["CId"]) == 60)
            {
                value1 = Convert.ToDouble(decimal.Parse((TCColumn).ToString()).ToString("N3"));
            }
            else if (Convert.ToInt32(dsparent2.Tables[0].Rows[0]["CId"]) == 74)
            {
                value1 = Convert.ToDouble(decimal.Parse((TCrossRow+TLongRow).ToString()).ToString("N3"));
            }
            else
            {
                value1 = Convert.ToDouble(decimal.Parse(dsparent2.Tables[0].Rows[0]["Qty"].ToString()).ToString("N3"));
            }


            int ParentCid = this.getBOMCId(wonodest, CompId, FinYearId);
            string InsertCidparentbom = this.insert("tblDG_BOM_Master", "SysDate,SysTime,CompId,FinYearId,SessionId,PId,CId,WONo,ItemId,Qty,EquipmentNo,UnitNo,PartNo", "'" + currDate.ToString() + "','" + currTime.ToString() + "'," + CompId + "," + FinYearId + ",'" + SessionId.ToString() + "','" + pid + "','" + ParentCid + "','" + wonodest + "','" + Convert.ToInt32(dsparent2.Tables[0].Rows[0]["ItemId"]) + "','" + value1 + "','" + dsparent2.Tables[0].Rows[0]["EquipmentNo"].ToString() + "','" + dsparent2.Tables[0].Rows[0]["UnitNo"].ToString() + "','" + dsparent2.Tables[0].Rows[0]["PartNo"].ToString() + "'");
            SqlCommand cmdCidparentbom = new SqlCommand(InsertCidparentbom, con);
            cmdCidparentbom.ExecuteNonQuery();
            string getparent = this.select("*", "tblDG_GUNRAIL_BOM_Master", "PId=" + node + "");
            SqlCommand checkparent = new SqlCommand(getparent, con);
            SqlDataAdapter daparent = new SqlDataAdapter(checkparent);
            DataSet dsparent = new DataSet();
            daparent.Fill(dsparent, "tblDG_GUNRAIL_BOM_Master");
            for (int h = 0; h < dsparent.Tables[0].Rows.Count; h++)
            {
                double value = 0;

                if (Convert.ToInt32(dsparent.Tables[0].Rows[h]["CId"]) == 1)
                {
                    value = Convert.ToDouble(decimal.Parse((TCColumn * 2).ToString()).ToString("N3"));

                }
                else if (Convert.ToInt32(dsparent.Tables[0].Rows[h]["CId"]) == 16)
                {
                    value = Convert.ToDouble(decimal.Parse((TCColumn * 2).ToString()).ToString("N3"));
                }
                else if (Convert.ToInt32(dsparent.Tables[0].Rows[h]["CId"]) == 21)
                {
                    value = Convert.ToDouble(decimal.Parse((TCColumn).ToString()).ToString("N3"));
                }
                else if (Convert.ToInt32(dsparent.Tables[0].Rows[h]["CId"]) == 31)
                {
                    value = Convert.ToDouble(decimal.Parse((TLongRow_with_coloumn).ToString()).ToString("N3"));
                }
                else if (Convert.ToInt32(dsparent.Tables[0].Rows[h]["CId"]) == 35)
                {
                    value = Convert.ToDouble(decimal.Parse((TLongRow_no_coloumn).ToString()).ToString("N3"));
                }
                else if (Convert.ToInt32(dsparent.Tables[0].Rows[h]["CId"]) == 40)
                {
                    value = Convert.ToDouble(decimal.Parse((TCColumn).ToString()).ToString("N3"));
                }
                else if (Convert.ToInt32(dsparent.Tables[0].Rows[h]["CId"]) == 46)
                {
                    value = Convert.ToDouble(decimal.Parse((TLongRow_with_coloumn + TLongRow_no_coloumn).ToString()).ToString("N3"));
                }
                else if (Convert.ToInt32(dsparent.Tables[0].Rows[h]["CId"]) == 55)
                {
                    value = Convert.ToDouble(decimal.Parse(((SumLongRailColumn + TCColumn) * 2).ToString()).ToString("N3"));

                }
                else if (Convert.ToInt32(dsparent.Tables[0].Rows[h]["CId"]) == 60)
                {
                    value = Convert.ToDouble(decimal.Parse((TCColumn).ToString()).ToString("N3"));
                }
                else if (Convert.ToInt32(dsparent2.Tables[0].Rows[0]["CId"]) == 74)
                {
                    value1 = Convert.ToDouble(decimal.Parse((TCrossRow + TLongRow).ToString()).ToString("N3"));
                }
                else
                {
                    value = Convert.ToDouble(decimal.Parse(dsparent.Tables[0].Rows[h]["Qty"].ToString()).ToString("N3"));
                }

                int NextCid = this.getBOMCId(wonodest, CompId, FinYearId);
                // Insert to BOM
                string Insertparentbom = this.insert("tblDG_BOM_Master", "SysDate,SysTime,CompId,FinYearId,SessionId,PId,CId,WONo,ItemId,Qty,EquipmentNo,UnitNo,PartNo", "'" + currDate.ToString() + "','" + currTime.ToString() + "','" + CompId + "','" + FinYearId + "','" + SessionId.ToString() + "','" + ParentCid + "','" + NextCid + "','" + wonodest + "','" + Convert.ToInt32(dsparent.Tables[0].Rows[h]["ItemId"]) + "','" + value + "','" + dsparent.Tables[0].Rows[h]["EquipmentNo"].ToString() + "','" + dsparent.Tables[0].Rows[h]["UnitNo"].ToString() + "','" + dsparent.Tables[0].Rows[h]["PartNo"].ToString() + "'");
                SqlCommand cmdCpyparentbom = new SqlCommand(Insertparentbom, con);
                cmdCpyparentbom.ExecuteNonQuery();
                // Get Parent Child 
                DataSet DS2 = new DataSet();
                string cmdStr2 = this.select("*", "tblDG_GUNRAIL_BOM_Master", "PId='" + dsparent.Tables[0].Rows[h]["CId"] + "'");
                SqlCommand cmd2 = new SqlCommand(cmdStr2, con);
                SqlDataAdapter da2 = new SqlDataAdapter(cmd2);
                da2.Fill(DS2);

                if (DS2.Tables[0].Rows.Count > 0)
                {
                    for (int j = 0; j < DS2.Tables[0].Rows.Count; j++)
                    {
                        cid = NextCid;
                        this.CopyGunRailToWO(Convert.ToInt32(DS2.Tables[0].Rows[j]["CId"]), wonodest, CompId, SessionId, FinYearId, cid, Convert.ToInt32(DS2.Tables[0].Rows[j]["CId"]), TCColumn, TLongRow_with_coloumn, TLongRow_no_coloumn, SumLongRailColumn, TCrossRow , TLongRow);
                    }
                }
            }

        }
        catch (Exception x)
        {
        }
        finally
        {
            con.Close();
        }
    }

    public void CopyGunRailToWO_Dispatch(int node, string wonodest, int CompId, string SessionId, int FinYearId, int destpid, int destcid, double TCColumn1, double TLongRow_with_coloumn1, double TLongRow_no_coloumn1, double SumLongRailColumn1,double TCrossRow , double TLongRow)
    {
        string connStr = this.Connection();
        SqlConnection con = new SqlConnection(connStr);
        try
        {
            DataSet DS = new DataSet();
            string currDate = this.getCurrDate();
            string currTime = this.getCurrTime();
            int pid = destpid;
            int cid = destcid;
            con.Open();

            string getparent2 = this.select("*", "tblDG_GUNRAIL_BOM_Master", " CId='" + node + "'");
            SqlCommand checkparent2 = new SqlCommand(getparent2, con);
            SqlDataAdapter daparent2 = new SqlDataAdapter(checkparent2);
            DataSet dsparent2 = new DataSet();
            daparent2.Fill(dsparent2, "tblDG_GUNRAIL_BOM_Master");
            double value1 = 0;

            if (Convert.ToInt32(dsparent2.Tables[0].Rows[0]["CId"]) == 1)
            {
                value1 = Convert.ToDouble(decimal.Parse((TCColumn1 * 2).ToString()).ToString("N3"));

            }
            //else if (Convert.ToInt32(dsparent2.Tables[0].Rows[0]["CId"]) == 16)
            //{
            //    value1 = Convert.ToDouble(decimal.Parse((TCColumn1 * 2).ToString()).ToString("N3"));
            //}
            else if (Convert.ToInt32(dsparent2.Tables[0].Rows[0]["CId"]) == 21)
            {
                value1 = Convert.ToDouble(decimal.Parse((TCColumn1).ToString()).ToString("N3"));
            }
            else if (Convert.ToInt32(dsparent2.Tables[0].Rows[0]["CId"]) == 31)
            {
                value1 = Convert.ToDouble(decimal.Parse((TLongRow_with_coloumn1).ToString()).ToString("N3"));
            }
            else if (Convert.ToInt32(dsparent2.Tables[0].Rows[0]["CId"]) == 35)
            {
                value1 = Convert.ToDouble(decimal.Parse((TLongRow_no_coloumn1).ToString()).ToString("N3"));
            }
            else if (Convert.ToInt32(dsparent2.Tables[0].Rows[0]["CId"]) == 40)
            {
                value1 = Convert.ToDouble(decimal.Parse((TCColumn1).ToString()).ToString("N3"));
            }
            //else if (Convert.ToInt32(dsparent2.Tables[0].Rows[0]["CId"]) == 46)
            //{
            //    value1 = Convert.ToDouble(decimal.Parse((TLongRow_with_coloumn1 + TLongRow_no_coloumn1).ToString()).ToString("N3"));
            //}
            else if (Convert.ToInt32(dsparent2.Tables[0].Rows[0]["CId"]) == 55)
            {
                value1 = Convert.ToDouble(decimal.Parse(((SumLongRailColumn1 + TCColumn1) * 2).ToString()).ToString("N3"));

            }
            else if (Convert.ToInt32(dsparent2.Tables[0].Rows[0]["CId"]) == 60)
            {
                value1 = Convert.ToDouble(decimal.Parse((TCColumn1).ToString()).ToString("N3"));
            }
            else if (Convert.ToInt32(dsparent2.Tables[0].Rows[0]["CId"]) == 74)
            {
                value1 = Convert.ToDouble(decimal.Parse((TCrossRow+TLongRow).ToString()).ToString("N3"));
            }
            else
            {
                value1 = Convert.ToDouble(decimal.Parse(dsparent2.Tables[0].Rows[0]["Qty"].ToString()).ToString("N3"));
            }


            int ParentCid = this.getBOMCId(wonodest, CompId, FinYearId);
            string InsertCidparentbom = this.insert("tblDG_BOM_Master", "SysDate,SysTime,CompId,FinYearId,SessionId,PId,CId,WONo,ItemId,Qty,EquipmentNo,UnitNo,PartNo", "'" + currDate.ToString() + "','" + currTime.ToString() + "'," + CompId + "," + FinYearId + ",'" + SessionId.ToString() + "','" + pid + "','" + ParentCid + "','" + wonodest + "','" + Convert.ToInt32(dsparent2.Tables[0].Rows[0]["ItemId"]) + "','" + value1 + "','" + dsparent2.Tables[0].Rows[0]["EquipmentNo"].ToString() + "','" + dsparent2.Tables[0].Rows[0]["UnitNo"].ToString() + "','" + dsparent2.Tables[0].Rows[0]["PartNo"].ToString() + "'");
            SqlCommand cmdCidparentbom = new SqlCommand(InsertCidparentbom, con);
            cmdCidparentbom.ExecuteNonQuery();


        }
        catch (Exception x)
        {
        }
        finally
        {
            con.Close();
        }
    }
    public double get(double len)
    {
        double m = len;
        double n = Math.Truncate(m);
        double result = 0;
        if (m > n)
        {

            result = n + 1;
        }
        else
        {
            result = n;
        }

        return result;
    }
    public double ShortDateTime(string FD, string FT)
    {
        double NFD = 0;
        try
        {
            DateTime mydt = Convert.ToDateTime(FD + " " + FT);
            NFD = mydt.Ticks;
        }
        catch (Exception et)
        {
        }

        return NFD;

    }

    //public string ShortDateTime(string FD, string FT)
    //{
    //    string NFD = "";
    //    try
    //    {
    //        string NFDD = "";
    //        string a = FD;
    //        string[] b = a.Split('-');
    //        string y = b[0];
    //        string m = b[1];
    //        string d = b[2];
    //        NFDD = m + d + y;

    //        string NFDT = "";
    //        string x = "";
    //        string r = "";
    //        string q = "";
    //        string a1 = FT;
    //        string[] b1 = a1.Split(':');
    //        string d1 = b1[0];
    //        string m1 = b1[1];
    //        string y1 = b1[2];
    //        string[] p = y1.Split(' ');
    //        x = p[0];
    //        q = p[1];
    //        int w = 0;
    //        if (q == "AM")
    //        {
    //            r = "1";
    //            w = Convert.ToInt32(m1);
    //        }
    //        else
    //        {
    //            r = "2";
    //            w = Convert.ToInt32(m1);
    //        }
    //        int d2 = Convert.ToInt32(d1);
    //        NFDT = w.ToString("D2") + d2.ToString("D2") + x + r;

    //        NFD = NFDD + NFDT;
    //        return NFD;

    //    }
    //    catch (Exception ex) { }
    //    return NFD;
    //}

    public DateTime FirstDateInCurrMonth()
    {
        return new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
    }

    public DateTime LastDateInCurrMonth()
    {
        DateTime FirstDateOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        return FirstDateOfMonth.AddMonths(1).AddDays(-1);
    }
    public DataTable GetGroupedBy(DataTable dt, string columnNamesInDt, string groupByColumnNames, string typeOfCalculation)
    {
        //Return its own if the column names are empty
        if (columnNamesInDt == string.Empty || groupByColumnNames == string.Empty)
        {
            return dt;
        }

        //Once the columns are added find the distinct rows and group it bu the numbet
        DataTable _dt = dt.DefaultView.ToTable(true, groupByColumnNames);

        //The column names in data table
        string[] _columnNamesInDt = columnNamesInDt.Split(',');

        for (int i = 0; i < _columnNamesInDt.Length; i = i + 1)
        {
            if (_columnNamesInDt[i] != groupByColumnNames)
            {
                _dt.Columns.Add(_columnNamesInDt[i]);
            }
        }


        //Gets the collection and send it back
        for (int i = 0; i < _dt.Rows.Count; i = i + 1)
        {
            for (int j = 0; j < _columnNamesInDt.Length; j = j + 1)
            {
                if (_columnNamesInDt[j] != groupByColumnNames)
                {
                    _dt.Rows[i][j] = dt.Compute(typeOfCalculation + "(" + _columnNamesInDt[j] + ")", groupByColumnNames + " = '" + _dt.Rows[i][groupByColumnNames].ToString() + "'");
                }
            }
        }

        return _dt;
    }

    public int chkEmpCode(string code, int CompId)
    {
        string connStr = this.Connection();
        SqlConnection con = new SqlConnection(connStr);
        DataSet DS = new DataSet();
        try
        {
            string cmdStr = this.select("EmpId,EmployeeName", "tblHR_OfficeStaff", "CompId='" + CompId + "' AND EmpId='" + code + "' ");
            SqlDataAdapter DA = new SqlDataAdapter(cmdStr, con);
            DA.Fill(DS, "tblHR_OfficeStaff");
        }
        catch (Exception ex)
        { }
        if (code != "")
        {
            if (DS.Tables[0].Rows.Count > 0)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
        else
        {
            return 0;
        }
    }







    ////////////////////////////////////////////////////////////////////// Cash 

    public double getCashEntryAmt(string CField, string Date, int CompId, int FyId)
    {
        string connStr = this.Connection();
        SqlConnection con = new SqlConnection(connStr);
        double Amt = 0;

        string sql = this.select("sum(Amt) as sum_cash", "tblACC_CashAmt_Master", "CompId='" + CompId + "' AND FinYearId<='" + FyId + "' AND SysDate" + CField + "'" + Date + "'");
        SqlCommand cmdsql = new SqlCommand(sql, con);
        SqlDataAdapter DAsql = new SqlDataAdapter(cmdsql);
        DataSet DSsql = new DataSet();
        DAsql.Fill(DSsql, "tblACC_CashAmt_Master");

        if (DSsql.Tables[0].Rows.Count > 0 && DSsql.Tables[0].Rows[0][0] != DBNull.Value)
        {
            Amt = Convert.ToDouble(DSsql.Tables[0].Rows[0][0]);
        }

        return Convert.ToDouble(decimal.Parse((Amt).ToString()).ToString("N2"));
    }

    public double getCashOpBalAmt(string CField, string Date, int CompId, int FyId)
    {
        string connStr = this.Connection();
        SqlConnection con = new SqlConnection(connStr);
        //OP Bank
        double OpCash = 0;
        OpCash = this.getCashEntryAmt("<", this.getCurrDate(), CompId, FyId);

        //IOU Payment

        double IOUPaymentAmt = 0;

        string sqlIOU = this.select("sum(Amount) as sum_iou", "tblACC_IOU_Master", "CompId='" + CompId + "' AND FinYearId<='" + FyId + "' AND SysDate" + CField + "'" + Date + "' AND Authorize='1'");
        SqlCommand cmdIOUsql = new SqlCommand(sqlIOU, con);
        SqlDataAdapter DAIOUsql = new SqlDataAdapter(cmdIOUsql);
        DataSet DSIOUsql = new DataSet();
        DAIOUsql.Fill(DSIOUsql, "tblACC_IOU_Master");

        if (DSIOUsql.Tables[0].Rows.Count > 0 && DSIOUsql.Tables[0].Rows[0][0] != DBNull.Value)
        {
            IOUPaymentAmt = Convert.ToDouble(DSIOUsql.Tables[0].Rows[0][0]);
        }

        //IOU Receipt

        double IOURecAmt = 0;

        string sqlIOURec = this.select("sum(tblACC_IOU_Receipt.RecievedAmount) as sum_iourec", "tblACC_IOU_Master,tblACC_IOU_Receipt", "tblACC_IOU_Master.CompId='" + CompId + "' AND tblACC_IOU_Receipt.FinYearId<='" + FyId + "' AND tblACC_IOU_Receipt.ReceiptDate" + CField + "'" + Date + "' AND tblACC_IOU_Master.Id=tblACC_IOU_Receipt.MId");
        SqlCommand cmdIOURec = new SqlCommand(sqlIOURec, con);
        SqlDataAdapter DAIOURec = new SqlDataAdapter(cmdIOURec);
        DataSet DSIOURec = new DataSet();
        DAIOURec.Fill(DSIOURec);

        if (DSIOURec.Tables[0].Rows.Count > 0 && DSIOURec.Tables[0].Rows[0][0] != DBNull.Value)
        {
            IOURecAmt = Convert.ToDouble(DSIOURec.Tables[0].Rows[0][0]);
        }

        //Cash Payment

        double CVpayAmt = 0;

        string sqlCVPay = this.select("sum(tblACC_CashVoucher_Payment_Details.Amount) as sum_cvpay", "tblACC_CashVoucher_Payment_Master,tblACC_CashVoucher_Payment_Details", "tblACC_CashVoucher_Payment_Master.CompId='" + CompId + "' AND tblACC_CashVoucher_Payment_Master.Id=tblACC_CashVoucher_Payment_Details.MId AND tblACC_CashVoucher_Payment_Master.FinYearId<='" + FyId + "' AND tblACC_CashVoucher_Payment_Master.SysDate" + CField + "'" + Date + "' ");

        SqlCommand cmdCVPay = new SqlCommand(sqlCVPay, con);
        SqlDataAdapter DACVPay = new SqlDataAdapter(cmdCVPay);
        DataSet DSCVPay = new DataSet();

        DACVPay.Fill(DSCVPay);

        if (DSCVPay.Tables[0].Rows.Count > 0 && DSCVPay.Tables[0].Rows[0][0] != DBNull.Value)
        {
            CVpayAmt = Convert.ToDouble(DSCVPay.Tables[0].Rows[0][0]);
        }

        //Cash Receipt

        double CVRecAmt = 0;

        string sqlCVRec = this.select("sum(tblACC_CashVoucher_Receipt_Master.Amount) as sum_cvrec", "tblACC_CashVoucher_Receipt_Master", "tblACC_CashVoucher_Receipt_Master.CompId='" + CompId + "' AND tblACC_CashVoucher_Receipt_Master.FinYearId<='" + FyId + "' AND tblACC_CashVoucher_Receipt_Master.SysDate" + CField + "'" + Date + "'");

        SqlCommand cmdCVRec = new SqlCommand(sqlCVRec, con);
        SqlDataAdapter DACVRec = new SqlDataAdapter(cmdCVRec);
        DataSet DSCVRec = new DataSet();

        DACVRec.Fill(DSCVRec, "tblACC_CashVoucher_Receipt_Master");

        if (DSCVRec.Tables[0].Rows.Count > 0 && DSCVRec.Tables[0].Rows[0][0] != DBNull.Value)
        {
            CVRecAmt = Convert.ToDouble(DSCVRec.Tables[0].Rows[0][0]);
        }

        //Contra Cash

        double ContraAmt = 0;

        string sqlcontra = this.select("sum(Amount) as sum_contra", "tblACC_Contra_Entry", "CompId='" + CompId + "' AND FinYearId<='" + FyId + "' AND Date" + CField + "'" + Date + "' AND Cr='4'");
        SqlCommand cmdcontrasql = new SqlCommand(sqlcontra, con);
        SqlDataAdapter DAcontrasql = new SqlDataAdapter(cmdcontrasql);
        DataSet DScontrasql = new DataSet();

        DAcontrasql.Fill(DScontrasql, "tblACC_Contra_Entry");

        if (DScontrasql.Tables[0].Rows.Count > 0 && DScontrasql.Tables[0].Rows[0][0] != DBNull.Value)
        {
            ContraAmt = Convert.ToDouble(DScontrasql.Tables[0].Rows[0][0]);
        }

        double ContraAmtDr = 0;

        string sqlcontraDr = this.select("sum(Amount) as sum_contra", "tblACC_Contra_Entry", "CompId='" + CompId + "' AND FinYearId<='" + FyId + "' AND Date" + CField + "'" + Date + "' AND Dr='4'");
        SqlCommand cmdcontrasqlDr = new SqlCommand(sqlcontraDr, con);
        SqlDataAdapter DAcontrasqlDr = new SqlDataAdapter(cmdcontrasqlDr);
        DataSet DScontrasqlDr = new DataSet();

        DAcontrasqlDr.Fill(DScontrasqlDr, "tblACC_Contra_Entry");

        if (DScontrasqlDr.Tables[0].Rows.Count > 0 && DScontrasqlDr.Tables[0].Rows[0][0] != DBNull.Value)
        {
            ContraAmtDr = Convert.ToDouble(DScontrasqlDr.Tables[0].Rows[0][0]);
        }

        double TIOpEmpAdvAmt = 0;

        string sqlTI = "SELECT SUM(tblACC_TourAdvance_Details.Amount) as SUM_Amt FROM  tblACC_TourIntimation_Master INNER JOIN tblACC_TourAdvance_Details ON tblACC_TourIntimation_Master.Id = tblACC_TourAdvance_Details.MId AND tblACC_TourIntimation_Master.FinYearId<='" + FyId + "' AND tblACC_TourIntimation_Master.SysDate" + CField + "'" + Date + "' Group By tblACC_TourAdvance_Details.MId";

        SqlCommand sqlTIcmd = new SqlCommand(sqlTI, con);
        SqlDataAdapter DAsqlTI = new SqlDataAdapter(sqlTIcmd);
        DataSet DSsqlTI = new DataSet();
        DAsqlTI.Fill(DSsqlTI);

        if (DSsqlTI.Tables[0].Rows.Count > 0 && DSsqlTI.Tables[0].Rows[0][0] != DBNull.Value)
        {
            TIOpEmpAdvAmt = Convert.ToDouble(DSsqlTI.Tables[0].Rows[0]["SUM_Amt"]);
        }

        double TIOpOtherAdvAmt = 0;

        string sqlOtherTI = "SELECT SUM(tblACC_TourAdvance.Amount) as SUM_Amt FROM  tblACC_TourIntimation_Master INNER JOIN tblACC_TourAdvance ON tblACC_TourIntimation_Master.Id = tblACC_TourAdvance.MId AND tblACC_TourIntimation_Master.FinYearId<='" + FyId + "' AND tblACC_TourIntimation_Master.SysDate" + CField + "'" + Date + "' Group By tblACC_TourAdvance.MId";

        SqlCommand sqlOtherTIcmd = new SqlCommand(sqlOtherTI, con);
        SqlDataAdapter DAsqlOtherTI = new SqlDataAdapter(sqlOtherTIcmd);
        DataSet DSsqlOtherTI = new DataSet();
        DAsqlOtherTI.Fill(DSsqlOtherTI);

        if (DSsqlOtherTI.Tables[0].Rows.Count > 0 && DSsqlOtherTI.Tables[0].Rows[0][0] != DBNull.Value)
        {
            TIOpOtherAdvAmt = Convert.ToDouble(DSsqlOtherTI.Tables[0].Rows[0]["SUM_Amt"]);
        }

        double CalCashOpQty = 0;

        //CalCashOpQty = Convert.ToDouble(decimal.Parse(((OpCash + IOURecAmt + CVRecAmt + ContraAmtDr) - (ContraAmt + IOUPaymentAmt + CVpayAmt)).ToString()).ToString("N2"));
        CalCashOpQty = Math.Round(Convert.ToDouble(decimal.Parse(((OpCash + IOURecAmt + CVRecAmt + ContraAmtDr) - (ContraAmt + IOUPaymentAmt + CVpayAmt + TIOpEmpAdvAmt + TIOpOtherAdvAmt)).ToString()).ToString("N2")), 5);

        return CalCashOpQty;

    }

    public double getCashClBalAmt(string CField, string Date, int CompId, int FyId)
    {
        string connStr = this.Connection();
        SqlConnection con = new SqlConnection(connStr);

        //OP Bank
        double OpCash = 0;
        OpCash = this.getCashOpBalAmt("<", Date, CompId, FyId);

        //IOU Payment

        double IOUPaymentAmt = 0;

        string sqlIOU = this.select("sum(Amount) as sum_iou", "tblACC_IOU_Master", "CompId='" + CompId + "' AND FinYearId<='" + FyId + "' AND SysDate" + CField + "'" + Date + "' AND Authorize='1'");
        SqlCommand cmdIOUsql = new SqlCommand(sqlIOU, con);
        SqlDataAdapter DAIOUsql = new SqlDataAdapter(cmdIOUsql);
        DataSet DSIOUsql = new DataSet();
        DAIOUsql.Fill(DSIOUsql, "tblACC_IOU_Master");

        if (DSIOUsql.Tables[0].Rows.Count > 0 && DSIOUsql.Tables[0].Rows[0][0] != DBNull.Value)
        {
            IOUPaymentAmt = Convert.ToDouble(DSIOUsql.Tables[0].Rows[0][0]);
        }

        //IOU Receipt

        double IOURecAmt = 0;

        string sqlIOURec = this.select("sum(tblACC_IOU_Receipt.RecievedAmount) as sum_iourec", "tblACC_IOU_Master,tblACC_IOU_Receipt", "tblACC_IOU_Master.CompId='" + CompId + "' AND tblACC_IOU_Receipt.FinYearId<='" + FyId + "' AND tblACC_IOU_Receipt.ReceiptDate" + CField + "'" + Date + "' AND tblACC_IOU_Master.Id=tblACC_IOU_Receipt.MId");
        SqlCommand cmdIOURec = new SqlCommand(sqlIOURec, con);
        SqlDataAdapter DAIOURec = new SqlDataAdapter(cmdIOURec);
        DataSet DSIOURec = new DataSet();
        DAIOURec.Fill(DSIOURec);

        if (DSIOURec.Tables[0].Rows.Count > 0 && DSIOURec.Tables[0].Rows[0][0] != DBNull.Value)
        {
            IOURecAmt = Convert.ToDouble(DSIOURec.Tables[0].Rows[0][0]);
        }

        //Cash Payment

        double CVpayAmt = 0;

        string sqlCVPay = this.select("sum(tblACC_CashVoucher_Payment_Details.Amount) as sum_cvpay", "tblACC_CashVoucher_Payment_Master,tblACC_CashVoucher_Payment_Details", "tblACC_CashVoucher_Payment_Master.CompId='" + CompId + "' AND tblACC_CashVoucher_Payment_Master.FinYearId<='" + FyId + "' AND tblACC_CashVoucher_Payment_Master.Id=tblACC_CashVoucher_Payment_Details.MId  AND tblACC_CashVoucher_Payment_Master.SysDate" + CField + "'" + Date + "'");

        SqlCommand cmdCVPay = new SqlCommand(sqlCVPay, con);
        SqlDataAdapter DACVPay = new SqlDataAdapter(cmdCVPay);
        DataSet DSCVPay = new DataSet();

        DACVPay.Fill(DSCVPay);

        if (DSCVPay.Tables[0].Rows.Count > 0 && DSCVPay.Tables[0].Rows[0][0] != DBNull.Value)
        {
            CVpayAmt = Convert.ToDouble(DSCVPay.Tables[0].Rows[0][0]);
        }

        //Cash Receipt

        double CVRecAmt = 0;

        string sqlCVRec = this.select("sum(tblACC_CashVoucher_Receipt_Master.Amount) as sum_cvrec", "tblACC_CashVoucher_Receipt_Master", "tblACC_CashVoucher_Receipt_Master.CompId='" + CompId + "' AND tblACC_CashVoucher_Receipt_Master.FinYearId<='" + FyId + "' AND tblACC_CashVoucher_Receipt_Master.SysDate" + CField + "'" + Date + "'");

        SqlCommand cmdCVRec = new SqlCommand(sqlCVRec, con);
        SqlDataAdapter DACVRec = new SqlDataAdapter(cmdCVRec);
        DataSet DSCVRec = new DataSet();

        DACVRec.Fill(DSCVRec, "tblACC_CashVoucher_Receipt_Master");

        if (DSCVRec.Tables[0].Rows.Count > 0 && DSCVRec.Tables[0].Rows[0][0] != DBNull.Value)
        {
            CVRecAmt = Convert.ToDouble(DSCVRec.Tables[0].Rows[0][0]);
        }

        //Contra Cash

        double ContraAmt = 0;

        string sqlcontra = this.select("sum(Amount) as sum_contra", "tblACC_Contra_Entry", "CompId='" + CompId + "' AND FinYearId<='" + FyId + "' AND Date" + CField + "'" + Date + "' AND Cr='4'");
        SqlCommand cmdcontrasql = new SqlCommand(sqlcontra, con);
        SqlDataAdapter DAcontrasql = new SqlDataAdapter(cmdcontrasql);
        DataSet DScontrasql = new DataSet();

        DAcontrasql.Fill(DScontrasql, "tblACC_Contra_Entry");

        if (DScontrasql.Tables[0].Rows.Count > 0 && DScontrasql.Tables[0].Rows[0][0] != DBNull.Value)
        {
            ContraAmt = Convert.ToDouble(DScontrasql.Tables[0].Rows[0][0]);
        }

        double ContraAmtDr = 0;

        string sqlcontraDr = this.select("sum(Amount) as sum_contra", "tblACC_Contra_Entry", "CompId='" + CompId + "' AND FinYearId<='" + FyId + "' AND Date" + CField + "'" + Date + "' AND Dr='4'");
        SqlCommand cmdcontrasqlDr = new SqlCommand(sqlcontraDr, con);
        SqlDataAdapter DAcontrasqlDr = new SqlDataAdapter(cmdcontrasqlDr);
        DataSet DScontrasqlDr = new DataSet();

        DAcontrasqlDr.Fill(DScontrasqlDr, "tblACC_Contra_Entry");

        if (DScontrasqlDr.Tables[0].Rows.Count > 0 && DScontrasqlDr.Tables[0].Rows[0][0] != DBNull.Value)
        {
            ContraAmtDr = Convert.ToDouble(DScontrasqlDr.Tables[0].Rows[0][0]);
        }

        double CalCashClQty = 0;

        //Current Cash Trans
        double CashEntry = 0;
        CashEntry = this.getCashEntryAmt("=", this.getCurrDate(), CompId, FyId);


        double TIClEmpAmt = 0;

        string sqlTIEmp = "SELECT SUM(tblACC_TourAdvance_Details.Amount) as SUM_Amt FROM  tblACC_TourIntimation_Master INNER JOIN tblACC_TourAdvance_Details ON tblACC_TourIntimation_Master.Id = tblACC_TourAdvance_Details.MId AND tblACC_TourIntimation_Master.FinYearId<='" + FyId + "' AND tblACC_TourIntimation_Master.SysDate" + CField + "'" + Date + "'  Group By tblACC_TourAdvance_Details.MId";

        SqlCommand sqlTIEmpcmd = new SqlCommand(sqlTIEmp, con);
        SqlDataAdapter DAsqlEmpTI = new SqlDataAdapter(sqlTIEmpcmd);
        DataSet DSsqlEmpTI = new DataSet();
        DAsqlEmpTI.Fill(DSsqlEmpTI);

        if (DSsqlEmpTI.Tables[0].Rows.Count > 0 && DSsqlEmpTI.Tables[0].Rows[0][0] != DBNull.Value)
        {
            TIClEmpAmt = Convert.ToDouble(DSsqlEmpTI.Tables[0].Rows[0]["SUM_Amt"]);
        }

        double TIClOtherAmt = 0;

        string sqlTIOther = "SELECT SUM(tblACC_TourAdvance.Amount) as SUM_Amt FROM  tblACC_TourIntimation_Master INNER JOIN tblACC_TourAdvance ON tblACC_TourIntimation_Master.Id = tblACC_TourAdvance.MId AND tblACC_TourIntimation_Master.FinYearId<='" + FyId + "' AND tblACC_TourIntimation_Master.SysDate" + CField + "'" + Date + "'  Group By tblACC_TourAdvance.MId";

        SqlCommand sqlTIOthercmd = new SqlCommand(sqlTIOther, con);
        SqlDataAdapter DAsqlOtherTI = new SqlDataAdapter(sqlTIOthercmd);
        DataSet DSsqlOtherTI = new DataSet();
        DAsqlOtherTI.Fill(DSsqlOtherTI);

        if (DSsqlOtherTI.Tables[0].Rows.Count > 0 && DSsqlOtherTI.Tables[0].Rows[0][0] != DBNull.Value)
        {
            TIClOtherAmt = Convert.ToDouble(DSsqlOtherTI.Tables[0].Rows[0]["SUM_Amt"]);
        }

        //CalCashClQty = Convert.ToDouble(decimal.Parse(((OpCash + IOURecAmt + CVRecAmt + ContraAmtDr + CashEntry) - (ContraAmt + IOUPaymentAmt + CVpayAmt)).ToString()).ToString("N2"));
        CalCashClQty = Math.Round(Convert.ToDouble(decimal.Parse(((OpCash + IOURecAmt + CVRecAmt + ContraAmtDr + CashEntry) - (ContraAmt + IOUPaymentAmt + CVpayAmt + TIClEmpAmt + TIClOtherAmt)).ToString()).ToString("N2")), 5);
        
        return CalCashClQty;


    }

    ////////////////////////////////////////////////////////////////////// Bank

    public double getBankEntryAmt(string CField, string Date, int CompId, int FyId, int BankId)
    {
        string connStr = this.Connection();
        SqlConnection con = new SqlConnection(connStr);

        double Amt = 0;

        string sql = this.select("sum(Amt) as sum_bank", "tblACC_BankAmt_Master", " BankId='" + BankId + "'And CompId='" + CompId + "' AND FinYearId<='" + FyId + "' AND SysDate" + CField + "'" + Date + "'");
        SqlCommand cmdsql = new SqlCommand(sql, con);
        SqlDataAdapter DAsql = new SqlDataAdapter(cmdsql);
        DataSet DSsql = new DataSet();
        DAsql.Fill(DSsql, "tblACC_BankAmt_Master");

        if (DSsql.Tables[0].Rows.Count > 0 && DSsql.Tables[0].Rows[0][0] != DBNull.Value)
        {
            //Amt = Convert.ToDouble(decimal.Parse(DSsql.Tables[0].Rows[0][0].ToString()).ToString("N2"));

            Amt = Math.Round(Convert.ToDouble(decimal.Parse(DSsql.Tables[0].Rows[0][0].ToString()).ToString("N2")),5);
        }
        return Amt;

        //return Convert.ToDouble(decimal.Parse((Amt).ToString()).ToString("N2"));
    }
    public double getBankOpBalAmt(string CField, string Date, int CompId, int FyId, int BankId)
    {
        string connStr = this.Connection();
        SqlConnection con = new SqlConnection(connStr);
        //OP Bank
        double OpBank = 0;
        OpBank = this.getBankEntryAmt("<", this.getCurrDate(), CompId, FyId, BankId);

        //Bank Payment
        double BVpayAmt = 0;


        string sqlBVPay = this.select("sum(tblACC_BankVoucher_Payment_Details.Amount) as sum_bvpay", "tblACC_BankVoucher_Payment_Master,tblACC_BankVoucher_Payment_Details,tblACC_BankRecanciliation", "tblACC_BankVoucher_Payment_Master.Bank='" + BankId + "' And tblACC_BankVoucher_Payment_Master.CompId='" + CompId + "' AND tblACC_BankVoucher_Payment_Master.Id=tblACC_BankVoucher_Payment_Details.MId AND tblACC_BankVoucher_Payment_Master.FinYearId<='" + FyId + "' AND tblACC_BankVoucher_Payment_Master.Id =tblACC_BankRecanciliation.BVPId  AND tblACC_BankRecanciliation.BankDate" + CField + "'" + Date + "'");


        SqlCommand cmdBVPay = new SqlCommand(sqlBVPay, con);
        SqlDataAdapter DABVPay = new SqlDataAdapter(cmdBVPay);
        DataSet DSBVPay = new DataSet();
        DABVPay.Fill(DSBVPay);

        if (DSBVPay.Tables[0].Rows.Count > 0 && DSBVPay.Tables[0].Rows[0][0] != DBNull.Value)
        {
            BVpayAmt = Convert.ToDouble(DSBVPay.Tables[0].Rows[0][0]);
        }

        //Bank Receipt
        double BVRecAmt = 0;

        string sqlBVRec = this.select("sum(tblACC_BankVoucher_Received_Masters.Amount) as sum_cvrec", "tblACC_BankVoucher_Received_Masters", "tblACC_BankVoucher_Received_Masters.CompId='" + CompId + "'AND tblACC_BankVoucher_Received_Masters.DrawnAt='" + BankId + "' AND tblACC_BankVoucher_Received_Masters.FinYearId<='" + FyId + "' AND tblACC_BankVoucher_Received_Masters.ChequeClearanceDate" + CField + "'" + Date + "'");

        SqlCommand cmdBVRec = new SqlCommand(sqlBVRec, con);
        SqlDataAdapter DABVRec = new SqlDataAdapter(cmdBVRec);
        DataSet DSBVRec = new DataSet();
        DABVRec.Fill(DSBVRec, "tblACC_BankVoucher_Received_Masters");

        if (DSBVRec.Tables[0].Rows.Count > 0 && DSBVRec.Tables[0].Rows[0][0] != DBNull.Value)
        {
            BVRecAmt = Convert.ToDouble(DSBVRec.Tables[0].Rows[0][0]);
        }

        //Contra Bank
        double ContraAmt = 0;

        string sqlcontra = this.select("sum(Amount) as sum_contra", "tblACC_Contra_Entry", "CompId='" + CompId + "' AND FinYearId<='" + FyId + "' AND Date" + CField + "'" + Date + "' AND Cr!='4' And Cr='" + BankId + "'");
        SqlCommand cmdcontrasql = new SqlCommand(sqlcontra, con);
        SqlDataAdapter DAcontrasql = new SqlDataAdapter(cmdcontrasql);
        DataSet DScontrasql = new DataSet();

        DAcontrasql.Fill(DScontrasql, "tblACC_Contra_Entry");

        if (DScontrasql.Tables[0].Rows.Count > 0 && DScontrasql.Tables[0].Rows[0][0] != DBNull.Value)
        {
            ContraAmt = Convert.ToDouble(DScontrasql.Tables[0].Rows[0][0]);
        }

        double ContraAmtDr = 0;

        string sqlcontraDr = this.select("sum(Amount) as sum_contra", "tblACC_Contra_Entry", "CompId='" + CompId + "' AND FinYearId<='" + FyId + "' AND Date" + CField + "'" + Date + "' AND Dr!='4' And Dr='" + BankId + "'");
        SqlCommand cmdcontrasqlDr = new SqlCommand(sqlcontraDr, con);
        SqlDataAdapter DAcontrasqlDr = new SqlDataAdapter(cmdcontrasqlDr);
        DataSet DScontrasqlDr = new DataSet();

        DAcontrasqlDr.Fill(DScontrasqlDr, "tblACC_Contra_Entry");

        if (DScontrasqlDr.Tables[0].Rows.Count > 0 && DScontrasqlDr.Tables[0].Rows[0][0] != DBNull.Value)
        {
            ContraAmtDr = Convert.ToDouble(DScontrasqlDr.Tables[0].Rows[0][0]);
        }

        return Math.Round(Convert.ToDouble(decimal.Parse(((OpBank + BVRecAmt + ContraAmtDr) - (BVpayAmt + ContraAmt)).ToString()).ToString("N2")),5);
    }
    public double getBankClBalAmt(string CField, string Date, int CompId, int FyId, int BankId)
    {
        string connStr = this.Connection();
        SqlConnection con = new SqlConnection(connStr);

        //OP Bank

        double OpBank = 0;
        OpBank = this.getBankOpBalAmt("<", this.getCurrDate(), CompId, FyId, BankId);

        //Bank Payment
        double BVpayAmt = 0;
        string sqlBVPay = this.select("sum(tblACC_BankVoucher_Payment_Details.Amount) as sum_bvpay", "tblACC_BankVoucher_Payment_Master,tblACC_BankVoucher_Payment_Details,tblACC_BankRecanciliation", "tblACC_BankVoucher_Payment_Master.Bank='" + BankId + "' And tblACC_BankVoucher_Payment_Master.CompId='" + CompId + "' AND tblACC_BankVoucher_Payment_Master.Id=tblACC_BankVoucher_Payment_Details.MId AND tblACC_BankVoucher_Payment_Master.FinYearId<='" + FyId + "' AND tblACC_BankVoucher_Payment_Master.Id =tblACC_BankRecanciliation.BVPId AND tblACC_BankRecanciliation.BankDate" + CField + "'" + Date + "'");
        SqlCommand cmdBVPay = new SqlCommand(sqlBVPay, con);
        SqlDataAdapter DABVPay = new SqlDataAdapter(cmdBVPay);
        DataSet DSBVPay = new DataSet();

        DABVPay.Fill(DSBVPay);

        if (DSBVPay.Tables[0].Rows.Count > 0 && DSBVPay.Tables[0].Rows[0][0] != DBNull.Value)
        {
            BVpayAmt = Convert.ToDouble(DSBVPay.Tables[0].Rows[0][0]);
        }

        //Bank Recreipt
        double BVRecAmt = 0;

        string sqlBVRec = this.select("sum(tblACC_BankVoucher_Received_Masters.Amount) as sum_cvrec", "tblACC_BankVoucher_Received_Masters", "tblACC_BankVoucher_Received_Masters.CompId='" + CompId + "'AND tblACC_BankVoucher_Received_Masters.DrawnAt='" + BankId + "' AND tblACC_BankVoucher_Received_Masters.FinYearId<='" + FyId + "' AND tblACC_BankVoucher_Received_Masters.ChequeClearanceDate" + CField + "'" + Date + "'");


        SqlCommand cmdBVRec = new SqlCommand(sqlBVRec, con);
        SqlDataAdapter DABVRec = new SqlDataAdapter(cmdBVRec);
        DataSet DSBVRec = new DataSet();
        DABVRec.Fill(DSBVRec, "tblACC_BankVoucher_Received_Masters");

        if (DSBVRec.Tables[0].Rows.Count > 0 && DSBVRec.Tables[0].Rows[0][0] != DBNull.Value)
        {
            BVRecAmt = Convert.ToDouble(DSBVRec.Tables[0].Rows[0][0]);
        }

        //Contra Bank

        double ContraAmt = 0;

        string sqlcontra = this.select("sum(Amount) as sum_contra", "tblACC_Contra_Entry", "CompId='" + CompId + "' AND FinYearId<='" + FyId + "' AND Date" + CField + "'" + Date + "' AND Cr!='4' And Cr='" + BankId + "'");
        SqlCommand cmdcontrasql = new SqlCommand(sqlcontra, con);
        SqlDataAdapter DAcontrasql = new SqlDataAdapter(cmdcontrasql);
        DataSet DScontrasql = new DataSet();
        DAcontrasql.Fill(DScontrasql, "tblACC_Contra_Entry");

        if (DScontrasql.Tables[0].Rows.Count > 0 && DScontrasql.Tables[0].Rows[0][0] != DBNull.Value)
        {
            ContraAmt = Convert.ToDouble(DScontrasql.Tables[0].Rows[0][0]);
        }

        double ContraAmtDr = 0;

        string sqlcontraDr = this.select("sum(Amount) as sum_contra", "tblACC_Contra_Entry", "CompId='" + CompId + "' AND FinYearId<='" + FyId + "' AND Date" + CField + "'" + Date + "' AND Dr!='4'And Dr='" + BankId + "'");
        SqlCommand cmdcontrasqlDr = new SqlCommand(sqlcontraDr, con);
        SqlDataAdapter DAcontrasqlDr = new SqlDataAdapter(cmdcontrasqlDr);
        DataSet DScontrasqlDr = new DataSet();
        DAcontrasqlDr.Fill(DScontrasqlDr, "tblACC_Contra_Entry");

        if (DScontrasqlDr.Tables[0].Rows.Count > 0 && DScontrasqlDr.Tables[0].Rows[0][0] != DBNull.Value)
        {
            ContraAmtDr = Convert.ToDouble(DScontrasqlDr.Tables[0].Rows[0][0]);
        }

        //Current Bank Trans
        //   change fun to  this  in  Class.cs file..
        double BankEntry = 0;
        BankEntry = this.getBankEntryAmt("=", this.getCurrDate(), CompId, FyId, BankId);

        return Math.Round(Convert.ToDouble(decimal.Parse(((OpBank + BVRecAmt + ContraAmtDr + BankEntry) - (ContraAmt + BVpayAmt)).ToString()).ToString("N2")),5);

    }

    public double Offer_Cal(double gamt, int opt, int z, int typ)
    {
        /*         
         *gamt - Gross Salary 
         *opt - Basic,DA,HRA etc
         *z - 1 for monthly & 2 for Annual salary
         *typ - 1 staff & 2 neha
         */

        double GrossSalary = gamt;
        double value = 0;

        double BasicAmt = 0;
        BasicAmt = Convert.ToDouble(decimal.Parse(((GrossSalary * 30) / 100).ToString()).ToString("N2"));
        double DaAmt = 0;
        DaAmt = Convert.ToDouble(decimal.Parse(((GrossSalary * 20) / 100).ToString()).ToString("N2"));
        double Amt = 0;
        Amt = Convert.ToDouble(decimal.Parse((BasicAmt + DaAmt).ToString()).ToString("N2"));

        switch (opt)
        {
            case 1: //Basic
                if (z == 1)
                {
                    value = BasicAmt;
                }
                else if (z == 2)
                {
                    value = BasicAmt * 12;
                }
                break;

            case 2://DA
                if (z == 1)
                {
                    value = DaAmt;
                }
                else if (z == 2)
                {
                    value = DaAmt * 12;
                }
                break;

            case 3://HRA
                if (z == 1)
                {
                    value = ((GrossSalary * 20) / 100);
                }
                else if (z == 2)
                {
                    value = ((GrossSalary * 20) / 100) * 12;
                }
                break;

            case 4://Convenience
                if (z == 1)
                {
                    value = ((GrossSalary * 20) / 100);
                }
                else if (z == 2)
                {
                    value = ((GrossSalary * 20) / 100) * 12;
                }
                break;

            case 5://Education
                if (z == 1)
                {
                    value = ((GrossSalary * 5) / 100);
                }
                else if (z == 2)
                {
                    value = ((GrossSalary * 5) / 100) * 12;
                }
                break;

            case 6://Washing/Medical
                if (z == 1)
                {
                    value = ((GrossSalary * 5) / 100);
                }
                else if (z == 2)
                {
                    value = ((GrossSalary * 5) / 100) * 12;
                }
                break;

            case 7://AttBonus1
                if (typ == 1)
                {
                    value = ((GrossSalary * 10) / 100);
                }
                else if (typ == 2)
                {
                    value = ((GrossSalary * 5) / 100);
                }
                break;

            case 8://AttBonus2
                if (typ == 1)
                {
                    value = ((GrossSalary * 20) / 100);
                }
                else if (typ == 2)
                {
                    value = ((GrossSalary * 15) / 100);
                }
                break;
        }

        return Math.Round(Convert.ToDouble(decimal.Parse(value.ToString()).ToString("N2")),5);
    }
    public double Pf_Cal(double gamt, int typ, double val)
    {
        /*
         * typ - 1 - Employee & 2 - Company
         * emptyp - 1 staff & 2 neha
         */

        double value = 0;

        if (typ == 1)
        {
            value = (gamt * val) / 100;
        }
        else if (typ == 2)
        {
            value = (gamt * val) / 100;
        }

        return Math.Round(Convert.ToDouble(decimal.Parse(value.ToString()).ToString("N2")));
    }
    public double PTax_Cal(double gamt, string mth)
    {
        /*
         * gamt - Gross Salary + Att Bonus 1 + ExGratia
         * mth - month
         */

        double PTax = 0;

        if (gamt > 5000 && gamt <= 10000)
        {
            PTax = 175;

            //if (mth == "02")
            //{
            //    PTax = 200;
            //}
            //else
            //{
            //    PTax = 175;
            //}
        }
        else if (gamt > 10000)
        {
            if (mth == "02")
            {
                PTax = 300;
            }
            else
            {
                PTax = 200;
            }
        }

        return PTax;

    }
    public double Bonus_Cal(double gamt, int x)
    {
        /*
         * gamt - Gross Amt
         * x - 1 - Monthly, 2- Annual 
         */
        double bonus = 0;

        if (gamt > 6000)
        {
            bonus = 6000;
        }
        else
        {
            bonus = gamt;
        }

        if (x == 1)
        {
            bonus = bonus / 12;

        }

        return Math.Round(Convert.ToDouble(decimal.Parse(bonus.ToString()).ToString("N2")),5); 

        //return Convert.ToDouble(decimal.Parse(bonus.ToString()).ToString("N2")); 

    }
    public double Gratuity_Cal(double gamt, int x, int emptyp)
    {
        /*
         * gamt - Gross Amt
         * x - 1 - Monthly, 2- Annual 
         * emptyp - 1 staff & 2 neha
         */

        double value = 0;
        double basic = 0;
        double da = 0;

        basic = this.Offer_Cal(gamt, 1, 1, emptyp);
        da = this.Offer_Cal(gamt, 2, 1, emptyp);

        value = ((basic + da) / 26) * 15;

        if (x == 1)
        {
            value = value / 12;
        }
       // return Convert.ToDouble(decimal.Parse(value.ToString()).ToString("N2"));
        return Math.Round(Convert.ToDouble(decimal.Parse(value.ToString()).ToString("N2")),5);
    }
    public double Offer_Emp_Cal(int offerId, int opt, int z, int typ)
    {
        /*         
         *gamt - Gross Salary 
         *opt - Basic,DA,HRA etc
         *z - 1 for monthly & 2 for Annual salary
         *typ - 1 staff & 2 neha
         */


        string connStr = this.Connection();
        SqlConnection con = new SqlConnection(connStr);
        con.Open();
        double value = 0;
        try
        {
            string Str = this.select("salary", "tblHR_Offer_Master", "offerId='" + offerId + "' ");
            SqlCommand Cmd = new SqlCommand(Str, con);
            SqlDataAdapter da = new SqlDataAdapter(Cmd);
            DataSet DS = new DataSet();
            da.Fill(DS);

            double GrossSalary = 0;
            GrossSalary = Convert.ToDouble(DS.Tables[0].Rows[0]["salary"].ToString());

            double BasicAmt = 0;
            BasicAmt = Convert.ToDouble(decimal.Parse(((GrossSalary * 30) / 100).ToString()).ToString("N2"));
            double DaAmt = 0;
            DaAmt = Convert.ToDouble(decimal.Parse(((GrossSalary * 20) / 100).ToString()).ToString("N2"));
            double Amt = 0;
            Amt = Convert.ToDouble(decimal.Parse((BasicAmt + DaAmt).ToString()).ToString("N2"));

            switch (opt)
            {
                case 1: //Basic
                    if (z == 1)
                    {
                        value = BasicAmt;
                    }
                    else if (z == 2)
                    {
                        value = BasicAmt * 12;
                    }
                    break;

                case 2://DA
                    if (z == 1)
                    {
                        value = DaAmt;
                    }
                    else if (z == 2)
                    {
                        value = DaAmt * 12;
                    }
                    break;

                case 3://HRA
                    if (z == 1)
                    {
                        value = ((GrossSalary * 20) / 100);
                    }
                    else if (z == 2)
                    {
                        value = ((GrossSalary * 20) / 100) * 12;
                    }
                    break;

                case 4://Convenience
                    if (z == 1)
                    {
                        value = ((GrossSalary * 20) / 100);
                    }
                    else if (z == 2)
                    {
                        value = ((GrossSalary * 20) / 100) * 12;
                    }
                    break;

                case 5://Education
                    if (z == 1)
                    {
                        value = ((GrossSalary * 5) / 100);
                    }
                    else if (z == 2)
                    {
                        value = ((GrossSalary * 5) / 100) * 12;
                    }
                    break;

                case 6://Washing/Medical
                    if (z == 1)
                    {
                        value = ((GrossSalary * 5) / 100);
                    }
                    else if (z == 2)
                    {
                        value = ((GrossSalary * 5) / 100) * 12;
                    }
                    break;

                case 7://AttBonus1
                    if (typ == 1)
                    {
                        value = ((GrossSalary * 10) / 100);
                    }
                    else if (typ == 2)
                    {
                        value = ((GrossSalary * 5) / 100);
                    }
                    break;

                case 8://AttBonus2
                    if (typ == 1)
                    {
                        value = ((GrossSalary * 20) / 100);
                    }
                    else if (typ == 2)
                    {
                        value = ((GrossSalary * 15) / 100);
                    }
                    break;
            }
            con.Open();
        }
        catch (Exception st)
        {
        }
        //return Convert.ToDouble(decimal.Parse(value.ToString()).ToString("N2"));
        return Math.Round(Convert.ToDouble(decimal.Parse(value.ToString()).ToString("N2")),5);
    }
    public double Gratuity_Emp_Cal(int offerId, int x, int emptyp)
    {
        /*
         * gamt - Gross Amt
         * x - 1 - Monthly, 2- Annual 
         * emptyp - 1 staff & 2 neha
         */

        double value = 0;
        double basic = 0;
        double da = 0;

        try
        {
            basic = this.Offer_Emp_Cal(offerId, 1, 1, emptyp);
            da = this.Offer_Emp_Cal(offerId, 2, 1, emptyp);

            value = ((basic + da) / 26) * 15;

            if (x == 1)
            {
                value = value / 12;
            }
        }
        catch (Exception st)
        {
        }

        //return Convert.ToDouble(decimal.Parse(value.ToString()).ToString("N2"));
        return Math.Round(Convert.ToDouble(decimal.Parse(value.ToString()).ToString("N2")),5);
    }


    public string ECSNames(int ct, string code, int CompId)
    {
        string connStr = this.Connection();
        SqlConnection con = new SqlConnection(connStr);
        DataSet DS = new DataSet();
        string name = "";
        switch (ct)
        {
            case 1:
                {
                    string cmdStr = this.select("EmployeeName AS EmployeeName ", "tblHR_OfficeStaff", "CompId='" + CompId + "' AND EmpId='" + code + "' ");
                    SqlDataAdapter DA = new SqlDataAdapter(cmdStr, con);
                    DA.Fill(DS, "tblHR_OfficeStaff");
                    name = DS.Tables[0].Rows[0]["EmployeeName"].ToString();

                }
                break;

            case 2:
                {
                    string cmdStr = this.select("CustomerName AS CustomerName", "SD_Cust_master", "CompId='" + CompId + "' AND CustomerId='" + code + "'");
                    SqlDataAdapter DA = new SqlDataAdapter(cmdStr, con);
                    DA.Fill(DS, "SD_Cust_master");
                    name = DS.Tables[0].Rows[0]["CustomerName"].ToString();
                }
                break;

            case 3:
                {
                    string cmdStr = this.select("SupplierName AS SupplierName", "tblMM_Supplier_master", "CompId='" + CompId + "' AND SupplierId='" + code + "'");
                    SqlDataAdapter DA = new SqlDataAdapter(cmdStr, con);
                    DA.Fill(DS, "tblMM_Supplier_master");
                    name = DS.Tables[0].Rows[0]["SupplierName"].ToString();
                }
                break;
        }
        return name;
    }
    public string ECSAddress(int ct, string code, int CompId)
    {
        string connStr = this.Connection();
        SqlConnection con = new SqlConnection(connStr);
        DataSet DS = new DataSet();
        string name = "";
        switch (ct)
        {
            case 1:
                {
                    string cmdStr = this.select("PermanentAddress AS Adress ", "tblHR_OfficeStaff", "CompId='" + CompId + "' AND EmpId='" + code + "' ");
                    SqlDataAdapter DA = new SqlDataAdapter(cmdStr, con);
                    DA.Fill(DS, "tblHR_OfficeStaff");
                    name = DS.Tables[0].Rows[0]["Adress"].ToString();

                }
                break;

            case 2:
                {
                    string cmdStr = this.select("RegdAddress AS Adrerss", "SD_Cust_master", "CompId='" + CompId + "' AND CustomerId='" + code + "'");
                    SqlDataAdapter DA = new SqlDataAdapter(cmdStr, con);
                    DA.Fill(DS, "SD_Cust_master");
                    name = DS.Tables[0].Rows[0]["Adrerss"].ToString();
                }
                break;

            case 3:
                {
                    string cmdStr = this.select("RegdAddress AS Adrerss", "tblMM_Supplier_master", "CompId='" + CompId + "' AND SupplierId='" + code + "'");
                    SqlDataAdapter DA = new SqlDataAdapter(cmdStr, con);
                    DA.Fill(DS, "tblMM_Supplier_master");
                    name = DS.Tables[0].Rows[0]["Adrerss"].ToString();
                }
                break;
        }
        return name;
    }
    public string firstchar(string s)
    {
        return char.ToUpper(s[0]) + s.Substring(1);
    }
    public byte[] ImageToBinary(string imagePath)
    {

        FileStream fileStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
        byte[] buffer = new byte[fileStream.Length];
        fileStream.Read(buffer, 0, (int)fileStream.Length);
        fileStream.Close();
        return buffer;
    }


    public double CalWISQty(string compid, string wono, string itemid)
    {
        double FinWISQty = 0;
        try
        {
            string connStr = this.Connection();
            SqlConnection con = new SqlConnection(connStr);

            string sqlFinWISQty = this.select("sum(IssuedQty)As WisQty", "tblInv_WIS_Master,tblInv_WIS_Details", "tblInv_WIS_Master.Id=tblInv_WIS_Details.MId AND tblInv_WIS_Details.ItemId='" + itemid + "' And tblInv_WIS_Master.WONo='" + wono + "' And tblInv_WIS_Master.CompId='" + compid + "'");

            SqlCommand cmdFinWISQty = new SqlCommand(sqlFinWISQty, con);
            SqlDataAdapter daFinWISQty = new SqlDataAdapter(cmdFinWISQty);
            DataSet DSFinWISQty = new DataSet();
            daFinWISQty.Fill(DSFinWISQty);

            if (DSFinWISQty.Tables[0].Rows.Count > 0 && DSFinWISQty.Tables[0].Rows[0]["WisQty"] != DBNull.Value)
            {
                //FinWISQty = Convert.ToDouble(decimal.Parse((DSFinWISQty.Tables[0].Rows[0]["WisQty"]).ToString()).ToString("N3"));
                FinWISQty = Math.Round(Convert.ToDouble(decimal.Parse((DSFinWISQty.Tables[0].Rows[0]["WisQty"]).ToString()).ToString("N3")),5);
            }
            return FinWISQty;
        }

        catch (Exception ex) { }
        return FinWISQty;
    }

    public string ExciseCommodity(int excomid)
    {
        DataSet dsExCom = new DataSet();
        string connStr = this.Connection();
        SqlConnection con = new SqlConnection(connStr);

        try
        {
            con.Open();
            string sqlExCom = this.select("ChapHead", "tblExciseCommodity_Master", "Id='" + excomid + "'");
            SqlCommand cmdExCom = new SqlCommand(sqlExCom, con);
            SqlDataAdapter daExCom = new SqlDataAdapter(cmdExCom);

            daExCom.Fill(dsExCom, "tblExciseCommodity_Master");
            con.Close();
        }
        catch (Exception st)
        {
        }
        return dsExCom.Tables[0].Rows[0]["ChapHead"].ToString();
    }

    public double CalPRQty(int compid, string wono, int itemid)
    {
        double FinPRQty = 0;
        try
        {
            string connStr = this.Connection();
            SqlConnection con = new SqlConnection(connStr);

            string sqlPrQty = this.select("sum(Qty)As PRQty", "tblMM_PR_Master,tblMM_PR_Details", "tblMM_PR_Master.Id=tblMM_PR_Details.MId AND tblMM_PR_Details.ItemId='" + itemid + "' And tblMM_PR_Master.WONo='" + wono + "' And tblMM_PR_Master.CompId='" + compid + "'");

            SqlCommand cmdPrQty = new SqlCommand(sqlPrQty, con);
            SqlDataAdapter daPrQty = new SqlDataAdapter(cmdPrQty);
            DataSet DSPrQty = new DataSet();
            daPrQty.Fill(DSPrQty);

            if (DSPrQty.Tables[0].Rows.Count > 0 && DSPrQty.Tables[0].Rows[0]["PRQty"] != DBNull.Value)
            {
                //FinPRQty = Convert.ToDouble(decimal.Parse((DSPrQty.Tables[0].Rows[0]["PRQty"]).ToString()).ToString("N3"));
                FinPRQty = Math.Round(Convert.ToDouble(decimal.Parse((DSPrQty.Tables[0].Rows[0]["PRQty"]).ToString()).ToString("N3")),5);
            }
            return FinPRQty;
        }

        catch (Exception ex) { }
        return FinPRQty;
    }
        
    public string WOmfgdate(string wono, int compid, int finid)
    {
        string WomfgDt = "";
        try
        {
            string connStr = this.Connection();
            SqlConnection con = new SqlConnection(connStr);
            string StrSql = this.select("SD_Cust_WorkOrder_Master.ManufMaterialDate", "SD_Cust_WorkOrder_Master", "SD_Cust_WorkOrder_Master.WONo='" + wono + "' AND SD_Cust_WorkOrder_Master.FinYearId<='" + finid + "'And SD_Cust_WorkOrder_Master.CompId='" + compid + "'");
            SqlDataAdapter adapter = new SqlDataAdapter();
            adapter.SelectCommand = new SqlCommand(StrSql, con);
            DataSet dt = new DataSet();
            adapter.Fill(dt);

            if (dt.Tables[0].Rows.Count > 0 && dt.Tables[0].Rows[0]["ManufMaterialDate"] != DBNull.Value)
            {
                WomfgDt = this.FromDateDMY(dt.Tables[0].Rows[0]["ManufMaterialDate"].ToString());

            }
            return WomfgDt;

        }
        catch (Exception ex)
        {
        }
        return WomfgDt;
    }

    //public double AllComponentBOMQty(int CompId, string wono, string itemid, int finId)
    //{
    //    double tqty = 0;

    //    try
    //    {
    //        string connStr = this.Connection();
    //        SqlConnection con = new SqlConnection(connStr);
    //        string sql = this.select("tblDG_BOM_Master.PId,tblDG_BOM_Master.CId,tblDG_BOM_Master.ItemId,tblDG_Item_Master.Id,tblDG_Item_Master.ManfDesc,Unit_Master.Symbol As UOMBasic,tblDG_Item_Master.ItemCode,tblDG_BOM_Master.Qty as UnitQty", "tblDG_BOM_Master,tblDG_Item_Master,Unit_Master", "Unit_Master.Id=tblDG_Item_Master.UOMBasic and tblDG_Item_Master.Id=tblDG_BOM_Master.ItemId and tblDG_BOM_Master.WONo='" + wono + "'and tblDG_Item_Master.Id='" + Convert.ToInt32(itemid) + "' And tblDG_Item_Master.CompId='" + CompId + "'And tblDG_Item_Master.FinYearId<='" + finId + "'");
    //        SqlCommand cmd2 = new SqlCommand(sql, con);
    //        SqlDataAdapter DA4 = new SqlDataAdapter(cmd2);
    //        DataSet DS4 = new DataSet();
    //        DA4.Fill(DS4);

    //        for (int g = 0; g < DS4.Tables[0].Rows.Count; g++)
    //        {

    //            tqty += this.BOMRecurQty(wono, Convert.ToInt32(DS4.Tables[0].Rows[g]["PId"]), Convert.ToInt32(DS4.Tables[0].Rows[g]["CId"]), 1, CompId, finId);

    //        }

    //        return Math.Round(tqty,5);
    //    }

    //    catch (Exception ex)
    //    {
    //    }
    //    return Math.Round(tqty, 5);
    //}


    public double AllComponentBOMQty(int CompId, string wono, string itemid, int finId)
    {
        double tqty = 0;
        try
        {
            string connStr = this.Connection();
            SqlConnection con = new SqlConnection(connStr);

            string sql11 = this.select("PId,CId", "tblDG_BOM_Master", "tblDG_BOM_Master.WONo='" + wono + "'and tblDG_BOM_Master.ItemId='" + Convert.ToInt32(itemid) + "' And tblDG_BOM_Master.CompId='" + CompId + "'And tblDG_BOM_Master.FinYearId<='" + finId + "'");
            SqlCommand cmd211 = new SqlCommand(sql11, con);
            con.Open();
            SqlDataReader DA4 = cmd211.ExecuteReader();

            while (DA4.Read())
            {

                tqty += this.BOMRecurQty(wono, Convert.ToInt32(DA4["PId"]), Convert.ToInt32(DA4["CId"]), 1, CompId, finId);
            }

            return Math.Round(tqty, 5);
            con.Close();
        }

        catch (Exception ex)
        {
        }
        return Math.Round(tqty, 5);
    }


    public double AllComponentBOMQty_WoNo_wise(int CompId, string wono, int finId)
    {
        double tqty = 0;
        try
        {
            string connStr = this.Connection();
            SqlConnection con = new SqlConnection(connStr);
            string sql11 = this.select("PId,CId", "tblDG_BOM_Master", "tblDG_BOM_Master.WONo='" + wono + "' And tblDG_BOM_Master.CompId='" + CompId + "'And tblDG_BOM_Master.FinYearId<='" + finId + "'");
            SqlCommand cmd211 = new SqlCommand(sql11, con);
            con.Open();
            SqlDataReader DA4 = cmd211.ExecuteReader();
            while (DA4.Read())
            {

                tqty += this.BOMRecurQty(wono, Convert.ToInt32(DA4["PId"]), Convert.ToInt32(DA4["CId"]), 1, CompId, finId);
            }
            return Math.Round(tqty, 5);
            con.Close();
        }
        catch (Exception ex)
        {
        }
        return Math.Round(tqty, 5);
    }


    public double BOMRecurQty(string WONo, int Pid, int Cid, double p, int compid, int finid)
    {
        try
        {
            string connStr = this.Connection();
            SqlConnection con = new SqlConnection(connStr);
            con.Open();
            string cmdStr = this.select("Qty", "tblDG_BOM_Master", " WONo='" + WONo + "' AND PId='" + Pid + "'AND CId='" + Cid + "'And tblDG_BOM_Master.CompId='" + compid + "'AND tblDG_BOM_Master.FinYearId<='" + finid + "'");
            SqlCommand cmd = new SqlCommand(cmdStr, con);
            SqlDataReader DA = cmd.ExecuteReader();
            DA.Read();
            if (DA.HasRows == true)
            {
                p = p * Convert.ToDouble(decimal.Parse(DA["Qty"].ToString()).ToString("N3"));
            }
            if (Pid > 0)
            {

                string cmdStr2 = this.select("PId,Qty", "tblDG_BOM_Master", "WONo='" + WONo + "' AND CId='" + Pid + "'And tblDG_BOM_Master.CompId='" + compid + "'AND tblDG_BOM_Master.FinYearId<='" + finid + "'");
                SqlCommand cmd2 = new SqlCommand(cmdStr2, con);
                SqlDataReader DA2 = cmd2.ExecuteReader();
                DA2.Read();
                if (DA2.HasRows == true)
                {
                    p = p * (Convert.ToDouble(decimal.Parse(DA2[1].ToString()).ToString("N3")));
                }
                int Rpid = Convert.ToInt32(DA2[0]);
                if (Rpid > 0)
                {

                    string cmdStr4 = this.select("PId", "tblDG_BOM_Master", "WONo='" + WONo + "' AND CId='" + Rpid + "'And tblDG_BOM_Master.CompId='" + compid + "'AND tblDG_BOM_Master.FinYearId<='" + finid + "'");
                    SqlCommand cmd4 = new SqlCommand(cmdStr4, con);
                    SqlDataReader DA4 = cmd4.ExecuteReader();
                    DA4.Read();
                    if (DA4.HasRows == true)
                    {
                        int rrpid = Convert.ToInt32(DA4[0]);
                        return Math.Round(this.BOMRecurQty(WONo, rrpid, Rpid, p, compid, finid), 5);
                    }
                }
            }

            con.Close();
        }

        catch (Exception ex)
        {

        }
        return Math.Round(p, 5);


    }


    public void MP_Tree1(string wono, int CompId, RadGrid GridView2, int finid, string param)
    {

        string connStr = this.Connection();
        SqlConnection con = new SqlConnection(connStr);
        try
        {

            DataTable dt = new DataTable();
            dt.Columns.Add("ItemCode", typeof(string));
            dt.Columns.Add("ManfDesc", typeof(string));
            dt.Columns.Add("UOMBasic", typeof(string));
            dt.Columns.Add("UnitQty", typeof(string));
            dt.Columns.Add("BOMQty", typeof(string));
            dt.Columns.Add(new System.Data.DataColumn("FileName", typeof(string)));
            dt.Columns.Add(new System.Data.DataColumn("AttName", typeof(string)));
            dt.Columns.Add("ItemId", typeof(int));
            dt.Columns.Add("PRQty", typeof(string));
            dt.Columns.Add("WISQty", typeof(string));
            dt.Columns.Add("GQNQty", typeof(string));
            DataRow dr;
            string sql = this.select("Distinct ItemId", " tblDG_BOM_Master", "WONo='" + wono + "'and  CompId='" + CompId + "'And FinYearId<='" + finid + "' And ECNFlag=0 AND CId not in (Select PId from tblDG_BOM_Master where WONo='" + wono + "'and  CompId='" + CompId + "'And FinYearId<='" + finid + "')   ");

            SqlCommand cmd = new SqlCommand(sql, con);
            DataSet DS3 = new DataSet();
            SqlDataAdapter DA2 = new SqlDataAdapter(cmd);
            DA2.Fill(DS3, "tblDG_BOM_Master");

            for (int i = 0; i < DS3.Tables[0].Rows.Count; i++)
            {
                dr = dt.NewRow();
                string sql1 = this.select("tblDG_Item_Master.ItemCode,tblDG_Item_Master.PartNo,tblDG_Item_Master.CId,tblDG_Item_Master.Id,tblDG_Item_Master.ManfDesc,Unit_Master.Symbol As UOMBasic", " tblDG_Item_Master,Unit_Master", " Unit_Master.Id=tblDG_Item_Master.UOMBasic And  tblDG_Item_Master.Id='" + DS3.Tables[0].Rows[i]["ItemId"].ToString() + "'" + param + "");
                DataSet DS_S = new DataSet();
                SqlCommand cmd_S = new SqlCommand(sql1, con);
                SqlDataAdapter DA_S = new SqlDataAdapter(cmd_S);
                DA_S.Fill(DS_S);

                if (DS_S.Tables[0].Rows.Count > 0)
                {


                    string sqlGetProItem = this.select("tblDG_Item_Master.Process,tblDG_Item_Master.ItemCode", " tblDG_Item_Master", " tblDG_Item_Master.PartNo='" + DS_S.Tables[0].Rows[0]["PartNo"].ToString() + "' And CompId='" + CompId + "' " + param + " And tblDG_Item_Master.Process is not  null");
                    DataSet DS_GetProItem = new DataSet();
                    SqlCommand cmd_GetProItem = new SqlCommand(sqlGetProItem, con);
                    SqlDataAdapter DA_GetProItem = new SqlDataAdapter(cmd_GetProItem);
                    DA_GetProItem.Fill(DS_GetProItem);


                    string RMA = "";


                    for (int j = 0; j < DS_GetProItem.Tables[0].Rows.Count; j++)
                    {
                        RMA += "/" + DS_GetProItem.Tables[0].Rows[j]["Process"].ToString();
                    }

                    if (DS_S.Tables[0].Rows[0]["CId"] == DBNull.Value)
                    {
                        dr[0] = DS_S.Tables[0].Rows[0]["PartNo"].ToString() + RMA;
                    }
                    else
                    {
                        dr[0] = DS_S.Tables[0].Rows[0]["ItemCode"].ToString();
                    }

                    dr[1] = DS_S.Tables[0].Rows[0]["ManfDesc"].ToString();
                    dr[2] = DS_S.Tables[0].Rows[0]["UOMBasic"].ToString();

                    string sql3 = this.select(" Sum(tblDG_BOM_Master.Qty) as UnitQty", "tblDG_BOM_Master", "tblDG_BOM_Master.ItemId='" + DS3.Tables[0].Rows[i]["ItemId"].ToString() + "' and tblDG_BOM_Master.WONo='" + wono + "'and  tblDG_BOM_Master.CompId='" + CompId + "'And tblDG_BOM_Master.FinYearId<='" + finid + "'");
                    SqlCommand cmd4 = new SqlCommand(sql3, con);
                    SqlDataAdapter DA4 = new SqlDataAdapter(cmd4);
                    DataSet DS4 = new DataSet();
                    DA4.Fill(DS4);
                    double tqty = 0;
                    if (DS4.Tables[0].Rows.Count > 0)
                    {
                        tqty = Convert.ToDouble(decimal.Parse(DS4.Tables[0].Rows[0][0].ToString()).ToString("N3"));
                        dr[3] = tqty;
                    }
                    double liQty = this.AllComponentBOMQty(CompId, wono, DS3.Tables[0].Rows[i]["ItemId"].ToString(), finid);
                    dr[4] = liQty;
                    dr[7] = DS3.Tables[0].Rows[i]["ItemId"].ToString();


                    //PR Qty
                    double PRQty = 0;
                    PRQty = this.CalPRQty(CompId, wono, Convert.ToInt32(DS3.Tables[0].Rows[i]["ItemId"]));
                    dr[8] = PRQty.ToString();

                    //WIS Qty
                    double WISQty = 0;
                    WISQty = this.CalWISQty(CompId.ToString(), wono, DS3.Tables[0].Rows[i]["ItemId"].ToString());
                    dr[9] = WISQty.ToString();

                    //GQN Qty
                    double GQNQty = 0;
                    GQNQty = this.GQNQTY(CompId, wono, DS3.Tables[0].Rows[i]["ItemId"].ToString());
                    dr[10] = GQNQty.ToString();

                    string sql2 = this.select("tblDG_Item_Master.AttName,tblDG_Item_Master.FileName,tblDG_Item_Master.Id", "tblDG_Item_Master", "tblDG_Item_Master.Id='" + DS3.Tables[0].Rows[i]["ItemId"].ToString() + "'");
                    SqlCommand cmd2 = new SqlCommand(sql2, con);
                    SqlDataAdapter da = new SqlDataAdapter(cmd2);
                    DataSet ds = new DataSet();
                    da.Fill(ds);
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        if (ds.Tables[0].Rows[0]["FileName"].ToString() != "" && ds.Tables[0].Rows[0]["FileName"] != DBNull.Value)
                        {
                            dr[5] = "View";
                        }
                        else
                        {
                            dr[5] = "";
                        }

                        if (ds.Tables[0].Rows[0]["AttName"].ToString() != "" && ds.Tables[0].Rows[0]["AttName"] != DBNull.Value)
                        {
                            dr[6] = "View";
                        }
                        else
                        {
                            dr[6] = "";
                        }
                    }

                    if ((liQty - PRQty - WISQty+GQNQty ) > 0)
                    
                    {
                        dt.Rows.Add(dr);
                        dt.AcceptChanges();
                    }
                }
            }

            GridView2.DataSource = dt;
            GridView2.DataBind();

        }
        catch (Exception ch)
        {
        }
        finally
        {
            con.Close();
        }

    }

    //public string GetItemCode_PartNo(int compid, int itemid)
    //{
    //    string x = "";
    //    try
    //    {
    //        //clsFunctions fun = new clsFunctions();
    //        string connStr = this.Connection();
    //        SqlConnection con = new SqlConnection(connStr);

    //        string sqlit = this.select("ItemCode,PartNo,CId,Process", "tblDG_Item_Master", "CompId='" + compid + "' AND Id='" + itemid + "'");
    //        SqlCommand cmdit = new SqlCommand(sqlit, con);
    //        SqlDataAdapter dait = new SqlDataAdapter(cmdit);
    //        DataSet DSit = new DataSet();
    //        dait.Fill(DSit);

    //        if (DSit.Tables[0].Rows.Count > 0)
    //        {
    //            if (DSit.Tables[0].Rows[0]["CId"] != DBNull.Value)
    //            {
    //                x = DSit.Tables[0].Rows[0]["ItemCode"].ToString();
    //            }
    //            else
    //            {
    //                if (DSit.Tables[0].Rows[0]["Process"].ToString() != "0" && DSit.Tables[0].Rows[0]["Process"] != DBNull.Value)
    //                {
    //                    x = DSit.Tables[0].Rows[0]["PartNo"].ToString() + DSit.Tables[0].Rows[0]["Process"].ToString();
    //                }
    //                else
    //                {
    //                    x = DSit.Tables[0].Rows[0]["PartNo"].ToString();
    //                }
    //            }
    //        }
    //        return x;
    //    }
    //    catch (Exception ex) { }
    //    return x;
    //}

    public string GetItemCode_PartNo(int compid, int itemid)
    {
        string x = "";
        try
        {
            //clsFunctions fun = new clsFunctions();
            string connStr = this.Connection();
            SqlConnection con = new SqlConnection(connStr);

            string sqlit = this.select("ItemCode,PartNo,CId,Process", "tblDG_Item_Master", "CompId='" + compid + "' AND Id='" + itemid + "'");
            SqlCommand cmdit = new SqlCommand(sqlit, con);
            SqlDataAdapter dait = new SqlDataAdapter(cmdit);
            DataSet DSit = new DataSet();
            dait.Fill(DSit);

            if (DSit.Tables[0].Rows.Count > 0)
            {
                if (DSit.Tables[0].Rows[0]["CId"] != DBNull.Value)
                {
                    x = DSit.Tables[0].Rows[0]["ItemCode"].ToString();
                }
                else
                {
                    if (DSit.Tables[0].Rows[0]["Process"].ToString() != "0" && DSit.Tables[0].Rows[0]["Process"] != DBNull.Value)
                    {

                        string sql = this.select("Symbol", "tblPln_Process_Master", "Id='" + DSit.Tables[0].Rows[0]["Process"].ToString() + "'");
                        SqlCommand cmd = new SqlCommand(sql, con);
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataSet DS = new DataSet();
                        da.Fill(DS);
                        if (DS.Tables[0].Rows.Count > 0)
                        {

                            x = DSit.Tables[0].Rows[0]["PartNo"].ToString() + DS.Tables[0].Rows[0]["Symbol"].ToString();
                        }
                    }
                    else
                    {
                        x = DSit.Tables[0].Rows[0]["PartNo"].ToString();
                    }
                }
            }
            return x;
        }
        catch (Exception ex) { }
        return x;
    }

    public double RMQty(string itemId, string wono, int CompId, string tblname)
    {

        double RMSum = 0;
        string connStr = this.Connection();
        SqlConnection con = new SqlConnection(connStr);
        try
        {
            string sql14 = "SELECT Sum(" + tblname + ".Qty) as Quantity FROM tblMP_Material_Master INNER JOIN tblMP_Material_Detail ON tblMP_Material_Master.Id = tblMP_Material_Detail.Mid INNER JOIN  " + tblname + " ON tblMP_Material_Detail.Id = " + tblname + ".DMid And tblMP_Material_Master.WONo='" + wono + "' And tblMP_Material_Master.CompId='" + CompId + "' And tblMP_Material_Detail.ItemId='" + itemId + "'";
            DataSet DS_S14 = new DataSet();
            SqlCommand cmd_S14 = new SqlCommand(sql14, con);
            SqlDataAdapter DA_S14 = new SqlDataAdapter(cmd_S14);
            DA_S14.Fill(DS_S14);

            if (DS_S14.Tables[0].Rows.Count > 0 && DS_S14.Tables[0].Rows[0][0] != DBNull.Value)
            {

                //RMSum = Convert.ToDouble(decimal.Parse(DS_S14.Tables[0].Rows[0][0].ToString()).ToString("N3"));
                RMSum = Math.Round(Convert.ToDouble(DS_S14.Tables[0].Rows[0][0]),5);
            }

            return RMSum;

        }

        catch (Exception ex)
        {
        }
        return RMSum;

    }

    public double RMQty_Temp(string itemId, string tblname)
    {

        double RMSum = 0;
        string connStr = this.Connection();
        SqlConnection con = new SqlConnection(connStr);
        try
        {
            string sql14 = "SELECT Sum(" + tblname + ".Qty) as Quantity FROM tblMP_Material_Detail_Temp," + tblname + " where tblMP_Material_Detail_Temp.Id=" + tblname + ".DMid  And tblMP_Material_Detail_Temp.ItemId='" + itemId + "'";
            DataSet DS_S14 = new DataSet();
            SqlCommand cmd_S14 = new SqlCommand(sql14, con);
            SqlDataAdapter DA_S14 = new SqlDataAdapter(cmd_S14);
            DA_S14.Fill(DS_S14);

            if (DS_S14.Tables[0].Rows.Count > 0 && DS_S14.Tables[0].Rows[0][0]!=DBNull.Value)
            {

                //RMSum = Convert.ToDouble(decimal.Parse(DS_S14.Tables[0].Rows[0][0].ToString()).ToString("N3"));
                RMSum = Math.Round(Convert.ToDouble(decimal.Parse(DS_S14.Tables[0].Rows[0][0].ToString()).ToString("N3")),5);
            }

            return RMSum;

        }

        catch (Exception ex)
        {
        }
        return RMSum;

    }
    
    public double RMQty_PR(string itemId, string wono, int CompId, string tblname)
    {

        double RMSum = 0;
        string connStr = this.Connection();
        SqlConnection con = new SqlConnection(connStr);
        try
        {
            string sql14 = "SELECT Sum(" + tblname + ".Qty) as Quantity FROM tblMM_PR_Master," + tblname + " where  tblMM_PR_Master.Id = tblMM_PR_Details.Mid And tblMM_PR_Master.WONo='" + wono + "' And tblMM_PR_Master.CompId='" + CompId + "' And tblMM_PR_Details.ItemId='" + itemId + "'";
            DataSet DS_S14 = new DataSet();
            SqlCommand cmd_S14 = new SqlCommand(sql14, con);
            SqlDataAdapter DA_S14 = new SqlDataAdapter(cmd_S14);
            DA_S14.Fill(DS_S14);

            if (DS_S14.Tables[0].Rows.Count > 0 && DS_S14.Tables[0].Rows[0][0] != DBNull.Value)
            {

                //RMSum = Convert.ToDouble(decimal.Parse(DS_S14.Tables[0].Rows[0][0].ToString()).ToString("N3"));
                RMSum = Math.Round(Convert.ToDouble(decimal.Parse(DS_S14.Tables[0].Rows[0][0].ToString()).ToString("N3")),5);
            }

            return RMSum;

        }

        catch (Exception ex)
        {
        }
        return RMSum;

    }

    public double RMQty_PR_Temp(string itemId, string sessionId, int CompId, string tblname)
    {

        double RMSum = 0;
        string connStr = this.Connection();
        SqlConnection con = new SqlConnection(connStr);
        try
        {
            string sql14 = "SELECT Sum(" + tblname + ".Qty) as Quantity FROM " + tblname + " where " + tblname + ".SessionId='" + sessionId + "' And " + tblname + ".CompId='" + CompId + "' And " + tblname + ".ItemId='" + itemId + "'";
            DataSet DS_S14 = new DataSet();
            SqlCommand cmd_S14 = new SqlCommand(sql14, con);
            SqlDataAdapter DA_S14 = new SqlDataAdapter(cmd_S14);
            DA_S14.Fill(DS_S14);

            if (DS_S14.Tables[0].Rows.Count > 0 && DS_S14.Tables[0].Rows[0][0] != DBNull.Value)
            {

                //RMSum = Convert.ToDouble(decimal.Parse(DS_S14.Tables[0].Rows[0][0].ToString()).ToString("N3"));
                RMSum = Math.Round(Convert.ToDouble(decimal.Parse(DS_S14.Tables[0].Rows[0][0].ToString()).ToString("N3")),5);
            }

            return RMSum;

        }

        catch (Exception ex)
        {
        }
        return RMSum;

    }

    public double GQNQTY(int CompId, string wono, string ItemId)
    {
        double prQty = 0;

        try
        {
            string connStr = this.Connection();
            SqlConnection con = new SqlConnection(connStr);
            string SqlprGqnQty = this.select("Sum(tblQc_MaterialQuality_Details.AcceptedQty)As Sum_GQN_Qty", "tblQc_MaterialQuality_Details,tblQc_MaterialQuality_Master,tblinv_MaterialReceived_Details,tblinv_MaterialReceived_Master,tblMM_PO_Details,tblMM_PO_Master,tblMM_PR_Details,tblMM_PR_Master", "tblQc_MaterialQuality_Master.Id=tblQc_MaterialQuality_Details.MId And tblinv_MaterialReceived_Master.Id=tblinv_MaterialReceived_Details.MId And tblinv_MaterialReceived_Master.Id=tblQc_MaterialQuality_Master.GRRId And tblinv_MaterialReceived_Details.Id=tblQc_MaterialQuality_Details.GRRId And tblinv_MaterialReceived_Details.POId=tblMM_PO_Details.Id And tblMM_PO_Master.Id=tblMM_PO_Details.MId And tblMM_PR_Master.Id=tblMM_PR_Details.MId And tblMM_PR_Master.PRNo=tblMM_PO_Details.PRNo And tblMM_PO_Details.PRId=tblMM_PR_Details.Id And tblMM_PR_Details.ItemId='" + ItemId + "' And tblMM_PO_Master.PRSPRFlag='0' And tblMM_PR_Master.CompId='" + CompId + "' And tblMM_PR_Master.WONo='" + wono + "' ");

            SqlCommand CmdprGqnQty = new SqlCommand(SqlprGqnQty, con);

            SqlDataAdapter DAprGqnQty = new SqlDataAdapter(CmdprGqnQty);
            DataSet DSprGqnQty = new DataSet();
            DAprGqnQty.Fill(DSprGqnQty);

            if (DSprGqnQty.Tables[0].Rows.Count > 0 && DSprGqnQty.Tables[0].Rows[0][0] != DBNull.Value)
            {
                //prQty = Convert.ToDouble(decimal.Parse(DSprGqnQty.Tables[0].Rows[0][0].ToString()).ToString("N3"));
                prQty = Math.Round(Convert.ToDouble(decimal.Parse(DSprGqnQty.Tables[0].Rows[0][0].ToString()).ToString("N3")),5);
            }

            return prQty;

        }

        catch (Exception ex)
        {
        }

        return prQty;
    }


    public double GINQTY(int CompId, string wono, string ItemId)
    {
        double ginQty = 0;

        try
        {
            string connStr = this.Connection();
            SqlConnection con = new SqlConnection(connStr);
            string SqlprGqnQty = this.select("Sum(tblinv_MaterialReceived_Details.ReceivedQty)As Sum_GIN_Qty", "tblinv_MaterialReceived_Details,tblinv_MaterialReceived_Master,tblMM_PO_Details,tblMM_PO_Master,tblMM_PR_Details,tblMM_PR_Master", "tblinv_MaterialReceived_Master.Id=tblinv_MaterialReceived_Details.MId And tblinv_MaterialReceived_Details.POId=tblMM_PO_Details.Id And tblMM_PO_Master.Id=tblMM_PO_Details.MId And tblMM_PR_Master.Id=tblMM_PR_Details.MId And tblMM_PR_Master.PRNo=tblMM_PO_Details.PRNo And tblMM_PO_Details.PRId=tblMM_PR_Details.Id And tblMM_PR_Details.ItemId='" + ItemId + "' And tblMM_PO_Master.PRSPRFlag='0' And tblMM_PR_Master.CompId='" + CompId + "' And tblMM_PR_Master.WONo='" + wono + "' ");

            SqlCommand CmdprGqnQty = new SqlCommand(SqlprGqnQty, con);

            SqlDataAdapter DAprGqnQty = new SqlDataAdapter(CmdprGqnQty);
            DataSet DSprGqnQty = new DataSet();
            DAprGqnQty.Fill(DSprGqnQty);

            if (DSprGqnQty.Tables[0].Rows.Count > 0 && DSprGqnQty.Tables[0].Rows[0][0] != DBNull.Value)
            {
                //prQty = Convert.ToDouble(decimal.Parse(DSprGqnQty.Tables[0].Rows[0][0].ToString()).ToString("N3"));
                ginQty = Math.Round(Convert.ToDouble(decimal.Parse(DSprGqnQty.Tables[0].Rows[0][0].ToString()).ToString("N3")), 5);
            }

            return ginQty;

        }

        catch (Exception ex)
        {
        }

        return ginQty;
    }


    public void TimeSelectorDatabase1(string TimeSelectorDatabase, TimeSelector TimeSelector1)
    {

        char[] delimiterChars = { ':', ' ' };
        string[] words = TimeSelectorDatabase.Split(delimiterChars);
        string TM = words[3];
        int H = Convert.ToInt32(words[0]);
        int M = Convert.ToInt32(words[1]) + 1;
        int S = Convert.ToInt32(words[2]);
        string AP = TM;
        if (AP == "AM")
        {
            TimeSelector1.SetTime(H, M, S, MKB.TimePicker.TimeSelector.AmPmSpec.AM);
        }
        else
        {
            TimeSelector1.SetTime(H, M, S, MKB.TimePicker.TimeSelector.AmPmSpec.PM);
        }

    }

    public void TimeSelectorDatabase2(string TimeSelectorDatabase, TimeSelector TimeSelector1)
    {

        char[] delimiterChars = { ':', ' ' };
        string[] words = TimeSelectorDatabase.Split(delimiterChars);
        string TM = words[3];
        int H = Convert.ToInt32(words[0]);
        int M = Convert.ToInt32(words[1]) - 1;
        int S = Convert.ToInt32(words[2]);
        string AP = TM;
        if (AP == "AM")
        {
            TimeSelector1.SetTime(H, M, S, MKB.TimePicker.TimeSelector.AmPmSpec.AM);
        }
        else
        {
            TimeSelector1.SetTime(H, M, S, MKB.TimePicker.TimeSelector.AmPmSpec.PM);
        }

    }

    public void TimeSelectorDatabase(string TimeSelectorDatabase, TimeSelector TimeSelector1)
    {

        char[] delimiterChars = { ':', ' ' };
        string[] words = TimeSelectorDatabase.Split(delimiterChars);
        string TM = words[3];
        int H = Convert.ToInt32(words[0]);
        int M = Convert.ToInt32(words[1]);
        int S = Convert.ToInt32(words[2]);
        string AP = TM;
        if (AP == "AM")
        {
            TimeSelector1.SetTime(H, M, S, MKB.TimePicker.TimeSelector.AmPmSpec.AM);
        }
        else
        {
            TimeSelector1.SetTime(H, M, S, MKB.TimePicker.TimeSelector.AmPmSpec.PM);
        }

    }


    public void TimeSelectorDatabase3(string TimeSelectorDatabase, TimeSelector TimeSelector1)
    {

        char[] delimiterChars = { ':', ' ' };
        string[] words = TimeSelectorDatabase.Split(delimiterChars);
        string TM = words[3];
        int H = Convert.ToInt32(words[0]) + 2;
        int M = Convert.ToInt32(words[1]);
        int S = Convert.ToInt32(words[2]);
        string AP = TM;
        if (AP == "AM")
        {
            TimeSelector1.SetTime(H, M, S, MKB.TimePicker.TimeSelector.AmPmSpec.AM);
        }
        else
        {
            TimeSelector1.SetTime(H, M, S, MKB.TimePicker.TimeSelector.AmPmSpec.PM);
        }

    }


    public bool NumberValidationQty(string strSp)
    {
        try
        {

            if (strSp.ToString() != "")
            {

                string strRegex = @"^\d{1,15}(\.\d{0,3})?$";
                Regex re = new Regex(strRegex);
                if (re.IsMatch(strSp) == true)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

        }
        catch (Exception es)
        {

        }
        return true;
    }

    public bool CheckValidWONo(string WONo, int CompId, int FinYearId)
    {
        try
        {
            string re = "";
            if (WONo.ToString() != "")
            {

                string connStr = this.Connection();
                SqlConnection con = new SqlConnection(connStr);

                string StrCat = this.select("WONo", "SD_Cust_WorkOrder_Master", "CompId='" + CompId + "' And WONo='" + WONo + "' And FinYearId <='" + FinYearId + "'");
                SqlCommand Cmd = new SqlCommand(StrCat, con);
                SqlDataAdapter DA = new SqlDataAdapter(Cmd);
                DataSet DS = new DataSet();
                DA.Fill(DS, "SD_Cust_WorkOrder_Master");
                if (DS.Tables[0].Rows.Count > 0)
                {
                   
                    return true;
                }
                else
                {
                    return false;
                }
            }

        }
        catch (Exception es)
        {

        }
        return true;
    }

    public DataSet RateRegister(int ItemId, int CompId)
    {
        string connStr = this.Connection();
        SqlConnection con = new SqlConnection(connStr);
        DataSet xsdds = new RateRegSingleItem();
        con.Open();

        try
        {
            string StrReteReg = this.select("*", "tblMM_Rate_Register", "CompId='" + CompId + "' AND ItemId='" + ItemId + "'");

            SqlCommand cmdReteReg = new SqlCommand(StrReteReg, con);
            SqlDataAdapter DAReteReg = new SqlDataAdapter(cmdReteReg);
            DataSet DSReteReg = new DataSet();
            DataTable dt = new DataTable();
            DAReteReg.Fill(DSReteReg);

            dt.Columns.Add(new System.Data.DataColumn("Id", typeof(int)));
            dt.Columns.Add(new System.Data.DataColumn("ItemCode", typeof(string)));
            dt.Columns.Add(new System.Data.DataColumn("ManfDesc", typeof(string)));
            dt.Columns.Add(new System.Data.DataColumn("UOMBasic", typeof(string)));
            dt.Columns.Add(new System.Data.DataColumn("FinYear", typeof(string)));
            dt.Columns.Add(new System.Data.DataColumn("PONo", typeof(string)));
            dt.Columns.Add(new System.Data.DataColumn("Rate", typeof(double)));
            dt.Columns.Add(new System.Data.DataColumn("Discount", typeof(double)));
            dt.Columns.Add(new System.Data.DataColumn("PackFwd", typeof(string)));
            dt.Columns.Add(new System.Data.DataColumn("VAT", typeof(string)));
            dt.Columns.Add(new System.Data.DataColumn("Exst", typeof(string)));
            dt.Columns.Add(new System.Data.DataColumn("IndirectCost", typeof(double)));
            dt.Columns.Add(new System.Data.DataColumn("DirectCost", typeof(double)));
            dt.Columns.Add(new System.Data.DataColumn("CompId", typeof(int)));

            DataSet RateRegSingleitem = new DataSet();
            DataRow dr;

            if (DSReteReg.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < DSReteReg.Tables[0].Rows.Count; i++)
                {
                    dr = dt.NewRow();

                    string StrIcode = this.select("Id,ItemCode,ManfDesc,UOMBasic", "tblDG_Item_Master", " CompId='" + CompId + "' AND Id='" + DSReteReg.Tables[0].Rows[i]["ItemId"] + "'");

                    SqlCommand cmdIcode = new SqlCommand(StrIcode, con);
                    SqlDataAdapter DAIcode = new SqlDataAdapter(cmdIcode);
                    DataSet DSIcode = new DataSet();
                    DAIcode.Fill(DSIcode);

                    dr[0] = DSIcode.Tables[0].Rows[0]["Id"];
                    dr[1] = DSIcode.Tables[0].Rows[0]["ItemCode"];
                    dr[2] = DSIcode.Tables[0].Rows[0]["ManfDesc"];

                    string strUOMBasic = this.select("Symbol", "Unit_Master", "Id='" + DSIcode.Tables[0].Rows[0]["UOMBasic"] + "'");
                    SqlCommand cmdUOMBasic = new SqlCommand(strUOMBasic, con);
                    SqlDataAdapter daUOMBasic = new SqlDataAdapter(cmdUOMBasic);
                    DataSet DSUOMBasic = new DataSet();
                    daUOMBasic.Fill(DSUOMBasic);

                    if (DSUOMBasic.Tables[0].Rows.Count > 0)
                    {
                        dr[3] = DSUOMBasic.Tables[0].Rows[0]["Symbol"];
                    }

                    string stryr = this.select("FinYear", "tblFinancial_master", "CompId='" + CompId + "' AND FinYearId='" + DSReteReg.Tables[0].Rows[i]["FinYearId"] + "'");
                    SqlCommand cmdyr = new SqlCommand(stryr, con);
                    SqlDataAdapter dayr = new SqlDataAdapter(cmdyr);
                    DataSet DSyr = new DataSet();
                    dayr.Fill(DSyr);

                    if (DSyr.Tables[0].Rows.Count > 0)
                    {
                        dr[4] = DSyr.Tables[0].Rows[0]["FinYear"].ToString();
                    }
                    dr[5] = DSReteReg.Tables[0].Rows[i]["PONo"];
                    dr[6] = Convert.ToInt32(DSReteReg.Tables[0].Rows[i]["Rate"]);
                    dr[7] = Convert.ToInt32(DSReteReg.Tables[0].Rows[i]["Discount"]);

                    string strPF = this.select("Terms", "tblPacking_Master", "Id='" + DSReteReg.Tables[0].Rows[i]["PF"] + "'");
                    SqlCommand cmdPF = new SqlCommand(strPF, con);
                    SqlDataAdapter daPF = new SqlDataAdapter(cmdPF);
                    DataSet DSPF = new DataSet();
                    daPF.Fill(DSPF);
                    if (DSPF.Tables[0].Rows.Count > 0)
                    {
                        dr[8] = DSPF.Tables[0].Rows[0]["Terms"];
                    }
                    string strExST = this.select("Terms", "tblExciseser_Master", "Id='" + DSReteReg.Tables[0].Rows[i]["ExST"] + "'");
                    SqlCommand cmdExST = new SqlCommand(strExST, con);
                    SqlDataAdapter daExST = new SqlDataAdapter(cmdExST);
                    DataSet DSExST = new DataSet();
                    daExST.Fill(DSExST);
                    if (DSExST.Tables[0].Rows.Count > 0)
                    {
                        dr[9] = DSExST.Tables[0].Rows[0]["Terms"];
                    }
                    string strVAT = this.select("Terms", "tblVAT_Master", "Id='" + DSReteReg.Tables[0].Rows[i]["VAT"] + "'");
                    SqlCommand cmdVAT = new SqlCommand(strVAT, con);
                    SqlDataAdapter daVAT = new SqlDataAdapter(cmdVAT);
                    DataSet DSVAT = new DataSet();
                    daVAT.Fill(DSVAT);
                    if (DSPF.Tables[0].Rows.Count > 0)
                    {
                        dr[10] = DSVAT.Tables[0].Rows[0]["Terms"];
                    }
                    dr[11] = Convert.ToInt32(DSReteReg.Tables[0].Rows[i]["IndirectCost"]);
                    dr[12] = Convert.ToInt32(DSReteReg.Tables[0].Rows[i]["DirectCost"]);
                    dr[13] = CompId;

                    dt.Rows.Add(dr);
                    dt.AcceptChanges();
                }
                RateRegSingleitem.Tables.Add(dt);
                RateRegSingleitem.AcceptChanges();
                xsdds.Tables[0].Merge(RateRegSingleitem.Tables[0]);
                xsdds.AcceptChanges();
            }
        }
        catch (Exception ex)
        {

        }
        finally
        {
            con.Close();
        }
        return xsdds;
    }
    
    public void TimeSelector(int H, int M, int S, string TM, TimeSelector TimeSelector1)
    {
        int h = H;
        int m = M;
        int s = S;
        string AP = TM;
        if (AP == "AM")
        {
            TimeSelector1.SetTime(H, M, S, MKB.TimePicker.TimeSelector.AmPmSpec.AM);
        }
        else
        {
            TimeSelector1.SetTime(H, M, S, MKB.TimePicker.TimeSelector.AmPmSpec.PM);
        }
    }
   
    public double CalMINQty(string compid, string wono, string itemid)
    {
        string connStr = this.Connection();
        SqlConnection con = new SqlConnection(connStr);

        string sqlFinMINQty = this.select("sum(IssueQty)As MinQty", "tblInv_MaterialIssue_Details,tblInv_MaterialIssue_Master,tblInv_MaterialRequisition_Details,tblInv_MaterialRequisition_Master", "tblInv_MaterialIssue_Details.MINNo=tblInv_MaterialIssue_Master.MINNo And tblInv_MaterialRequisition_Details.MRSNo=tblInv_MaterialRequisition_Master.MRSNo and tblInv_MaterialRequisition_Details.ItemId='" + itemid + "' And tblInv_MaterialRequisition_Details.Id=tblInv_MaterialIssue_Details.MRSId And tblInv_MaterialIssue_Master.MRSNo =tblInv_MaterialRequisition_Details.MRSNo And tblInv_MaterialRequisition_Details.WONo='" + wono + "' And tblInv_MaterialIssue_Master.CompId='" + compid + "'");

        SqlCommand cmdFinMINQty = new SqlCommand(sqlFinMINQty, con);
        SqlDataAdapter daFinMINQty = new SqlDataAdapter(cmdFinMINQty);
        DataSet DSFinMINQty = new DataSet();
        daFinMINQty.Fill(DSFinMINQty);

        double FinMINQty = 0;

        if (DSFinMINQty.Tables[0].Rows.Count > 0 && DSFinMINQty.Tables[0].Rows[0][0] != DBNull.Value)
        {
            //FinMINQty = Convert.ToDouble(decimal.Parse((DSFinMINQty.Tables[0].Rows[0][0]).ToString()).ToString("N3"));
            FinMINQty = Math.Round(Convert.ToDouble(decimal.Parse((DSFinMINQty.Tables[0].Rows[0][0]).ToString()).ToString("N3")),5);
        }

        return FinMINQty;
    }

    public string FromDateMDY(string FD)
    {
        string NFD = "";
        try
        {
            string a = FD;
            string[] b = a.Split('-');
            string d = b[0];
            string m = b[1];
            string y = b[2];
            NFD = m + "-" + d + "-" + y;
            return NFD;
        }
        catch (Exception ex) { }
        return NFD;
    }

    public string DateToText(DateTime dt, bool includeTime, bool isUK)
    {
        string[] ordinals =
        {
           "First",
           "Second",
           "Third",
           "Fourth",
           "Fifth",
           "Sixth",
           "Seventh",
           "Eighth",
           "Ninth",
           "Tenth",
           "Eleventh",
           "Twelfth",
           "Thirteenth",
           "Fourteenth",
           "Fifteenth",
           "Sixteenth",
           "Seventeenth",
           "Eighteenth",
           "Nineteenth",
           "Twentieth",
           "Twenty First",
           "Twenty Second",
           "Twenty Third",
           "Twenty Fourth",
           "Twenty Fifth",
           "Twenty Sixth",
           "Twenty Seventh",
           "Twenty Eighth",
           "Twenty Ninth",
           "Thirtieth",
           "Thirty First"
        };

        int day = dt.Day;
        int month = dt.Month;
        int year = dt.Year;
        DateTime dtm = new DateTime(1, month, 1);
        string date;

        if (isUK)
        {
            date = "The " + ordinals[day - 1] + " of " + dtm.ToString("MMMM") + " " + NumberToText(year, true);
        }
        else
        {
            //date = dtm.ToString("MMMM") + " " + ordinals[day - 1] + " " + NumberToText(year, false);
            date = ordinals[day - 1] + " " + dtm.ToString("MMMM") + " " + NumberToText(year, false);
        }

        if (includeTime)
        {
            int hour = dt.Hour;
            int minute = dt.Minute;
            string ap = "AM";

            if (hour >= 12)
            {
                ap = "PM";
                hour = hour - 12;
            }

            if (hour == 0) hour = 12;
            string time = NumberToText(hour, false);
            if (minute > 0) time += " " + NumberToText(minute, false);
            time += " " + ap;
            date += ", " + time;
        }
        return date;
    }

    public string TimeToText(string TimeSel)
    {
        string time = "";
        try
        {
            char[] delimiterChars = { ':', ' ' };
            string[] words = TimeSel.Split(delimiterChars);
            string TM = words[3];
            int H = Convert.ToInt32(words[0]);
            int M = Convert.ToInt32(words[1]);
            string S = words[3].ToString();
            int hour = H;
            int minute = M;
            time = NumberToText(hour, false);
            if (minute > 0) time += " " + NumberToText(minute, false);
            time += " " + S;
            return time;

        }
        catch (Exception ex) { }
        return time;
    }

    public static string NumberToText(int number, bool isUK)
    {
        if (number == 0) return "Zero";
        string and = isUK ? "and " : ""; // deals with UK or US numbering
        if (number == -2147483648) return "Minus Two Billion One Hundred " + and +
        "Forty Seven Million Four Hundred " + and + "Eighty Three Thousand " +
        "Six Hundred " + and + "Forty Eight";
        int[] num = new int[4];
        int first = 0;
        int u, h, t;
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        if (number < 0)
        {
            sb.Append("Minus ");
            number = -number;
        }
        string[] words0 = { "", "One ", "Two ", "Three ", "Four ", "Five ", "Six ", "Seven ", "Eight ", "Nine " };
        string[] words1 = { "Ten ", "Eleven ", "Twelve ", "Thirteen ", "Fourteen ", "Fifteen ", "Sixteen ", "Seventeen ", "Eighteen ", "Nineteen " };
        string[] words2 = { "Twenty ", "Thirty ", "Forty ", "Fifty ", "Sixty ", "Seventy ", "Eighty ", "Ninety " };
        string[] words3 = { "Thousand ", "Million ", "Billion " };
        num[0] = number % 1000;           // units
        num[1] = number / 1000;
        num[2] = number / 1000000;
        num[1] = num[1] - 1000 * num[2];  // thousands
        num[3] = number / 1000000000;     // billions
        num[2] = num[2] - 1000 * num[3];  // millions
        for (int i = 3; i > 0; i--)
        {
            if (num[i] != 0)
            {
                first = i;
                break;
            }
        }
        for (int i = first; i >= 0; i--)
        {
            if (num[i] == 0) continue;
            u = num[i] % 10;              // ones
            t = num[i] / 10;
            h = num[i] / 100;             // hundreds
            t = t - 10 * h;               // tens
            if (h > 0) sb.Append(words0[h] + "Hundred ");
            if (u > 0 || t > 0)
            {
                if (h > 0 || i < first) sb.Append(and);
                if (t == 0)
                    sb.Append(words0[u]);
                else if (t == 1)
                    sb.Append(words1[u]);
                else
                    sb.Append(words2[t - 2] + words0[u]);
            }
            if (i != 0) sb.Append(words3[i - 1]);
        }
        return sb.ToString().TrimEnd();
    }
    
    public double GSN_SPRQTY(int CompId, string FrmDate, string TDate, string ItemId)
    {
        double SprGSNQty = 0;
        try
        {
            string connStr = this.Connection();
            SqlConnection con = new SqlConnection(connStr);
            string SqlSprGsnQty = this.select("Sum(tblinv_MaterialServiceNote_Details.ReceivedQty)As Sum_GSN_Qty", "tblinv_MaterialServiceNote_Master,tblinv_MaterialServiceNote_Details,tblInv_Inward_Master ,tblInv_Inward_Details,tblMM_PO_Details,tblMM_PO_Master,tblMM_SPR_Details,tblMM_SPR_Master   ", " tblinv_MaterialServiceNote_Master.Id=tblinv_MaterialServiceNote_Details.MId And tblInv_Inward_Master.Id=tblInv_Inward_Details.GINId And tblInv_Inward_Master.Id=tblinv_MaterialServiceNote_Master.GINId And tblInv_Inward_Master.GINNo=tblinv_MaterialServiceNote_Master.GINNo And tblInv_Inward_Details.POId= tblinv_MaterialServiceNote_Details.POId And tblMM_PO_Master.Id=tblMM_PO_Details.MId  And tblMM_SPR_Master.Id=tblMM_SPR_Details.MId And tblinv_MaterialServiceNote_Details.POId=tblMM_PO_Details.Id And tblMM_PO_Details.SPRId=tblMM_SPR_Details.Id And tblMM_PO_Details.SPRNo=tblMM_SPR_Master.SPRNo And tblMM_SPR_Details.ItemId='" + ItemId + "' And tblMM_PO_Master.PRSPRFlag='1' And tblinv_MaterialServiceNote_Master.CompId='" + CompId + "'And tblinv_MaterialServiceNote_Master.SysDate between '" + this.FromDate(FrmDate) + "' And '" + this.FromDate(TDate) + "'");

            SqlCommand CmdSprGsnQty = new SqlCommand(SqlSprGsnQty, con);
            SqlDataAdapter DASprGsnQty = new SqlDataAdapter(CmdSprGsnQty);
            DataSet DSSprGsnQty = new DataSet();
            DASprGsnQty.Fill(DSSprGsnQty);


            if (DSSprGsnQty.Tables[0].Rows.Count > 0 && DSSprGsnQty.Tables[0].Rows[0][0] != DBNull.Value)
            {
                //SprGSNQty = Convert.ToDouble(decimal.Parse((DSSprGsnQty.Tables[0].Rows[0][0]).ToString()).ToString("N3"));
                SprGSNQty = Math.Round(Convert.ToDouble(decimal.Parse((DSSprGsnQty.Tables[0].Rows[0][0]).ToString()).ToString("N3")),5);
            }

        }
        catch (Exception ex) { }
        return SprGSNQty;
    }

    public double GSN_PRQTY(int CompId, string FrmDate, string TDate, string ItemId)
    {
        string connStr = this.Connection();
        SqlConnection con = new SqlConnection(connStr);
        string SqlprGsnQty = this.select("Sum(tblinv_MaterialServiceNote_Details.ReceivedQty)As Sum_GSN_Qty", "tblinv_MaterialServiceNote_Master,tblinv_MaterialServiceNote_Details,tblInv_Inward_Master ,tblInv_Inward_Details,tblMM_PO_Details,tblMM_PO_Master,tblMM_PR_Details,tblMM_PR_Master", "tblinv_MaterialServiceNote_Master.Id=tblinv_MaterialServiceNote_Details.MId And tblInv_Inward_Master.Id=tblInv_Inward_Details.GINId And tblInv_Inward_Master.Id=tblinv_MaterialServiceNote_Master.GINId And tblInv_Inward_Master.GINNo=tblinv_MaterialServiceNote_Master.GINNo And tblInv_Inward_Details.POId= tblinv_MaterialServiceNote_Details.POId And tblMM_PO_Master.Id=tblMM_PO_Details.MId And tblMM_PR_Master.Id=tblMM_PR_Details.MId And tblinv_MaterialServiceNote_Details.POId=tblMM_PO_Details.Id And tblMM_PO_Details.PRId=tblMM_PR_Details.Id And tblMM_PO_Details.PRNo=tblMM_PR_Master.PRNo And  tblMM_PR_Details.ItemId='" + ItemId + "' And tblMM_PO_Master.PRSPRFlag='0' And tblinv_MaterialServiceNote_Master.CompId='" + CompId + "' And tblinv_MaterialServiceNote_Master.SysDate between '" + this.FromDate(FrmDate) + "' And '" + this.FromDate(TDate) + "'");

        SqlCommand CmdprGsnQty = new SqlCommand(SqlprGsnQty, con);

        SqlDataAdapter DAprGsnQty = new SqlDataAdapter(CmdprGsnQty);
        DataSet DSprGsnQty = new DataSet();
        DAprGsnQty.Fill(DSprGsnQty);

        double prGSNQty = 0;
        if (DSprGsnQty.Tables[0].Rows.Count > 0 && DSprGsnQty.Tables[0].Rows[0][0] != DBNull.Value)
        {
            //prGSNQty = Convert.ToDouble(decimal.Parse((DSprGsnQty.Tables[0].Rows[0][0]).ToString()).ToString("N3"));

            prGSNQty = Math.Round(Convert.ToDouble(decimal.Parse((DSprGsnQty.Tables[0].Rows[0][0]).ToString()).ToString("N3")),5);
        }

        return prGSNQty;
    }

    public double GQN_SPRQTY(int CompId, string FrmDate, string TDate, string ItemId)
    {
        double SprQty = 0;
        try
        {
            string connStr = this.Connection();
            SqlConnection con = new SqlConnection(connStr);
            string SqlSprGqnQty = this.select("Sum(tblQc_MaterialQuality_Details.AcceptedQty)As Sum_GQN_Qty", "tblQc_MaterialQuality_Details,tblQc_MaterialQuality_Master,tblinv_MaterialReceived_Details,tblinv_MaterialReceived_Master,tblMM_PO_Details,tblMM_PO_Master,tblMM_SPR_Details,tblMM_SPR_Master", " tblQc_MaterialQuality_Master.Id=tblQc_MaterialQuality_Details.MId And tblinv_MaterialReceived_Master.Id=tblinv_MaterialReceived_Details.MId And tblinv_MaterialReceived_Master.GRRNo=tblQc_MaterialQuality_Master.GRRNo And tblinv_MaterialReceived_Master.Id=tblQc_MaterialQuality_Master.GRRId And  tblinv_MaterialReceived_Details.Id=tblQc_MaterialQuality_Details.GRRId  And tblMM_PO_Master.Id=tblMM_PO_Details.MId  And tblMM_SPR_Master.Id=tblMM_SPR_Details.MId And tblinv_MaterialReceived_Details.POId=tblMM_PO_Details.Id And tblMM_PO_Details.SPRId=tblMM_SPR_Details.Id And tblMM_PO_Details.SPRNo=tblMM_SPR_Master.SPRNo And tblMM_SPR_Details.ItemId='" + ItemId + "' And tblMM_PO_Master.PRSPRFlag='1' And tblQc_MaterialQuality_Master.CompId='" + CompId + "'And tblQc_MaterialQuality_Master.SysDate between '" + this.FromDate(FrmDate) + "' And '" + this.FromDate(TDate) + "'");

            SqlCommand CmdSprGqnQty = new SqlCommand(SqlSprGqnQty, con);
            SqlDataAdapter DASprGqnQty = new SqlDataAdapter(CmdSprGqnQty);
            DataSet DSSprGqnQty = new DataSet();
            DASprGqnQty.Fill(DSSprGqnQty);


            if (DSSprGqnQty.Tables[0].Rows.Count > 0 && DSSprGqnQty.Tables[0].Rows[0][0] != DBNull.Value)
            {
                //SprQty = Convert.ToDouble(decimal.Parse((DSSprGqnQty.Tables[0].Rows[0][0]).ToString()).ToString("N3"));
                SprQty = Math.Round(Convert.ToDouble(decimal.Parse((DSSprGqnQty.Tables[0].Rows[0][0]).ToString()).ToString("N3")),5);
            }



        }
        catch (Exception ex) { }
        return SprQty;
    }

    public double GQN_PRQTY(int CompId, string FrmDate, string TDate, string ItemId)
    {
        string connStr = this.Connection();
        SqlConnection con = new SqlConnection(connStr);
        string SqlprGqnQty = this.select("Sum(tblQc_MaterialQuality_Details.AcceptedQty)As Sum_GQN_Qty", "tblQc_MaterialQuality_Details,tblQc_MaterialQuality_Master,tblinv_MaterialReceived_Details,tblinv_MaterialReceived_Master,tblMM_PO_Details,tblMM_PO_Master,tblMM_PR_Details,tblMM_PR_Master", " tblQc_MaterialQuality_Master.Id=tblQc_MaterialQuality_Details.MId And tblinv_MaterialReceived_Master.Id=tblinv_MaterialReceived_Details.MId And tblinv_MaterialReceived_Master.GRRNo=tblQc_MaterialQuality_Master.GRRNo And tblinv_MaterialReceived_Master.Id=tblQc_MaterialQuality_Master.GRRId And tblinv_MaterialReceived_Details.Id=tblQc_MaterialQuality_Details.GRRId And tblinv_MaterialReceived_Details.POId=tblMM_PO_Details.Id And tblMM_PO_Master.Id=tblMM_PO_Details.MId And tblMM_PR_Master.Id=tblMM_PR_Details.MId And tblMM_PR_Master.PRNo=tblMM_PO_Details.PRNo And tblMM_PO_Details.PRId=tblMM_PR_Details.Id And tblMM_PR_Details.ItemId='" + ItemId + "' And tblMM_PO_Master.PRSPRFlag='0' And tblQc_MaterialQuality_Master.CompId='" + CompId + "'And tblQc_MaterialQuality_Master.SysDate between '" + this.FromDate(FrmDate) + "' And '" + this.FromDate(TDate) + "'");

        SqlCommand CmdprGqnQty = new SqlCommand(SqlprGqnQty, con);

        SqlDataAdapter DAprGqnQty = new SqlDataAdapter(CmdprGqnQty);
        DataSet DSprGqnQty = new DataSet();
        DAprGqnQty.Fill(DSprGqnQty);

        double prQty = 0;
        if (DSprGqnQty.Tables[0].Rows.Count > 0 && DSprGqnQty.Tables[0].Rows[0][0] != DBNull.Value)
        {
            //prQty = Convert.ToDouble(decimal.Parse(DSprGqnQty.Tables[0].Rows[0][0].ToString()).ToString("N3"));
            prQty = Math.Round(Convert.ToDouble(decimal.Parse(DSprGqnQty.Tables[0].Rows[0][0].ToString()).ToString("N3")),5);
        }

        return prQty;
    }

    public double MRQN_QTY(int CompId, string FrmDate, string TDate, string ItemId)
    {
        string connStr = this.Connection();
        SqlConnection con = new SqlConnection(connStr);
        string SqlprMRqnQty = this.select("sum(tblQc_MaterialReturnQuality_Details.AcceptedQty) as sum_MRQNQty", "tblInv_MaterialReturn_Master,tblInv_MaterialReturn_Details,tblQc_MaterialReturnQuality_Master,tblQc_MaterialReturnQuality_Details", " tblQc_MaterialReturnQuality_Master.Id= tblQc_MaterialReturnQuality_Details.MId  AND  tblInv_MaterialReturn_Master.Id=tblInv_MaterialReturn_Details.MId  AND tblInv_MaterialReturn_Master.Id=tblQc_MaterialReturnQuality_Master.MRNId AND tblInv_MaterialReturn_Master.MRNNo=tblQc_MaterialReturnQuality_Master.MRNNo AND tblInv_MaterialReturn_Details.Id=tblQc_MaterialReturnQuality_Details.MRNId AND tblQc_MaterialReturnQuality_Details.MId=tblQc_MaterialReturnQuality_Master.Id AND tblInv_MaterialReturn_Details.ItemId='" + ItemId + "' AND tblQc_MaterialReturnQuality_Master.CompId='" + CompId + "' AND tblQc_MaterialReturnQuality_Master.SysDate between '" + this.FromDate(FrmDate) + "' AND '" + this.FromDate(TDate) + "'");

        SqlCommand CmdMRqnQty = new SqlCommand(SqlprMRqnQty, con);
        SqlDataAdapter DAMRqnQty = new SqlDataAdapter(CmdMRqnQty);
        DataSet DSMRqnQty = new DataSet();
        DAMRqnQty.Fill(DSMRqnQty);

        double MRQNQty = 0;
        if (DSMRqnQty.Tables[0].Rows.Count > 0 && DSMRqnQty.Tables[0].Rows[0][0] != DBNull.Value)
        {
            //MRQNQty = Convert.ToDouble(decimal.Parse((DSMRqnQty.Tables[0].Rows[0][0]).ToString()).ToString("N3"));
            MRQNQty = Math.Round(Convert.ToDouble(decimal.Parse((DSMRqnQty.Tables[0].Rows[0][0]).ToString()).ToString("N3")),5);
        }

        return MRQNQty;
    }

    public double MCNQA_QTY(int CompId, string FrmDate, string TDate, string ItemId)
    {
        string connStr = this.Connection();
        SqlConnection con = new SqlConnection(connStr);
        string SqlprMRqnQty = "SELECT sum(tblQc_AuthorizedMCN.QAQty) as sum_MCNQA_QTY FROM tblPM_MaterialCreditNote_Details INNER JOIN tblPM_MaterialCreditNote_Master ON tblPM_MaterialCreditNote_Details.MId = tblPM_MaterialCreditNote_Master.Id INNER JOIN tblQc_AuthorizedMCN ON tblPM_MaterialCreditNote_Details.Id = tblQc_AuthorizedMCN.MCNDId AND  tblPM_MaterialCreditNote_Details.MId = tblQc_AuthorizedMCN.MCNId INNER JOIN  tblDG_BOM_Master ON tblPM_MaterialCreditNote_Master.WONo = tblDG_BOM_Master.WONo AND tblPM_MaterialCreditNote_Details.PId = tblDG_BOM_Master.PId AND tblPM_MaterialCreditNote_Details.CId = tblDG_BOM_Master.CId AND tblQc_AuthorizedMCN.CompId='" + CompId + "' AND tblQc_AuthorizedMCN.SysDate  between '" + this.FromDate(FrmDate) + "' And '" + this.FromDate(TDate) + "' AND tblDG_BOM_Master.ItemId='" + ItemId + "'";

        SqlCommand CmdMRqnQty = new SqlCommand(SqlprMRqnQty, con);
        SqlDataAdapter DAMRqnQty = new SqlDataAdapter(CmdMRqnQty);
        DataSet DSMRqnQty = new DataSet();
        DAMRqnQty.Fill(DSMRqnQty);

        double MCNQA_QTY = 0;
        if (DSMRqnQty.Tables[0].Rows.Count > 0 && DSMRqnQty.Tables[0].Rows[0][0] != DBNull.Value)
        {
            //MCNQA_QTY = Convert.ToDouble(decimal.Parse((DSMRqnQty.Tables[0].Rows[0][0]).ToString()).ToString("N3"));
            MCNQA_QTY = Math.Round(Convert.ToDouble(decimal.Parse((DSMRqnQty.Tables[0].Rows[0][0]).ToString()).ToString("N3")),5);
        }

        return MCNQA_QTY;
    }


    public double MIN_IssuQTY(int CompId, string FrmDate, string TDate, string ItemId)
    {
        string connStr = this.Connection();
        SqlConnection con = new SqlConnection(connStr);
        string SqlIssuQty = this.select("sum(tblInv_MaterialIssue_Details.IssueQty) as sum_IssuQty", "tblInv_MaterialIssue_Details,tblInv_MaterialIssue_Master,tblInv_MaterialRequisition_Master,tblInv_MaterialRequisition_Details", "tblInv_MaterialIssue_Master.Id=tblInv_MaterialIssue_Details.MId AND tblInv_MaterialRequisition_Master.Id=tblInv_MaterialRequisition_Details.MId AND tblInv_MaterialRequisition_Master.MRSNo=tblInv_MaterialIssue_Master.MRSNo AND tblInv_MaterialRequisition_Master.Id=tblInv_MaterialIssue_Master.MRSId AND tblInv_MaterialRequisition_Details.Id= tblInv_MaterialIssue_Details.MRSId AND tblInv_MaterialRequisition_Details.ItemId='" + ItemId + "' AND tblInv_MaterialIssue_Master.CompId='" + CompId + "' AND tblInv_MaterialIssue_Master.SysDate between '" + this.FromDate(FrmDate) + "' AND '" + this.FromDate(TDate) + "'");

        SqlCommand CmdIssuQty = new SqlCommand(SqlIssuQty, con);
        SqlDataAdapter DAIssuQty = new SqlDataAdapter(CmdIssuQty);
        DataSet DSIssuQty = new DataSet();
        DAIssuQty.Fill(DSIssuQty);

        double IssuQty = 0;
        if (DSIssuQty.Tables[0].Rows.Count > 0 && DSIssuQty.Tables[0].Rows[0][0] != DBNull.Value)
        {
            //IssuQty = Convert.ToDouble(decimal.Parse((DSIssuQty.Tables[0].Rows[0][0]).ToString()).ToString("N3"));
            IssuQty = Math.Round(Convert.ToDouble(decimal.Parse((DSIssuQty.Tables[0].Rows[0][0]).ToString()).ToString("N3")),5);
        }
        return IssuQty;
    }

    public double WIS_IssuQTY(int CompId, string FrmDate, string TDate, string ItemId)
    {
        string connStr = this.Connection();
        SqlConnection con = new SqlConnection(connStr);

        string SqlIssuQty = this.select("sum(IssuedQty)As WisQty", "tblInv_WIS_Master,tblInv_WIS_Details", " tblInv_WIS_Details.ItemId='" + ItemId + "' AND tblInv_WIS_Master.CompId='" + CompId + "' AND tblInv_WIS_Master.SysDate between '" + this.FromDate(FrmDate) + "' AND '" + this.FromDate(TDate) + "' AND tblInv_WIS_Master.Id=tblInv_WIS_Details.MId ");
      
        SqlCommand CmdIssuQty = new SqlCommand(SqlIssuQty, con);
        SqlDataAdapter DAIssuQty = new SqlDataAdapter(CmdIssuQty);
        DataSet DSIssuQty = new DataSet();
        DAIssuQty.Fill(DSIssuQty);

        double IssuQty = 0;
        if (DSIssuQty.Tables[0].Rows.Count > 0 && DSIssuQty.Tables[0].Rows[0][0] != DBNull.Value)
        {
            //IssuQty = Convert.ToDouble(decimal.Parse((DSIssuQty.Tables[0].Rows[0][0]).ToString()).ToString("N3"));
            IssuQty = Math.Round(Convert.ToDouble(decimal.Parse((DSIssuQty.Tables[0].Rows[0][0]).ToString()).ToString("N3")),5);
        }
        return IssuQty;
    } 
    
    public int ItemCodeLimit(int CompId)
    {
        string connStr = this.Connection();
        SqlConnection con = new SqlConnection(connStr);
        string SqlCodeLimit = this.select("ItemCodeLimit", "tblCompany_master", "CompId='" + CompId + "'");
        SqlCommand CmdCodeLimit = new SqlCommand(SqlCodeLimit, con);

        SqlDataAdapter DACodeLimit = new SqlDataAdapter(CmdCodeLimit);
        DataSet DSCodeLimit = new DataSet();
        DACodeLimit.Fill(DSCodeLimit);

        int limit = 0;
        if (DSCodeLimit.Tables[0].Rows.Count > 0 && DSCodeLimit.Tables[0].Rows[0][0] != DBNull.Value)
        {
            limit = Convert.ToInt32(DSCodeLimit.Tables[0].Rows[0][0]);
        }

        return limit;
    }

    public string Encrypt(string val)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(val);
        var encBytes = System.Security.Cryptography.ProtectedData.Protect(bytes, new byte[0], System.Security.Cryptography.DataProtectionScope.LocalMachine);
        return Convert.ToBase64String(encBytes);
    }

    public string Decrypt(string val)
    {        
            var bytes = Convert.FromBase64String(val);
            var encBytes = System.Security.Cryptography.ProtectedData.Unprotect(bytes, new byte[0], System.Security.Cryptography.DataProtectionScope.LocalMachine);           
            return System.Text.Encoding.UTF8.GetString(encBytes);
       
    }
   
    public bool EmailValidation(string strEmail)
    {
        try
        { 
           
            if (strEmail.ToString() != "")
            {
                string strRegex = @"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";
                Regex re = new Regex(strRegex);
                if (re.IsMatch(strEmail) == true)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }                   

        }
        catch (Exception es)
        {

        }
        return true;
    }

    public bool DateValidation(string strDate)
    {
        try
        {

            if (strDate.ToString() != "")
            {

                string strRegex = @"^([1-9]|0[1-9]|[12][0-9]|3[01])[- /.]([1-9]|0[1-9]|1[012])[- /.][0-9]{4}$";
                Regex re = new Regex(strRegex);
                if (re.IsMatch(strDate) == true)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

        }
        catch (Exception es)
        {

        }
        return true;
    }

    public bool SpecialCarValidation(string strSp)
    {
        try
        {

            if (strSp.ToString() != "")
            {

                string strRegex = @"^[0-9a-zA-Z]*[-._()]*[#&%@*=+;:<>?]+$";
                Regex re = new Regex(strRegex);
                if (re.IsMatch(strSp) == true)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

        }
        catch (Exception es)
        {

        }
        return true;
    }

    public bool NumberValidation(string strSp)
    {
        try
        {

            if (strSp.ToString() != "")
            {

                string strRegex = @"^[0-9]\d*(\.\d+)?$";
                Regex re = new Regex(strRegex);
                if (re.IsMatch(strSp) == true)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

        }
        catch (Exception es)
        {

        }
        return true;
    }

    public static string UploadsFolder
    {
        get
        {
            return ConfigurationManager.AppSettings.Get("UploadsFolder");
        }
    }

    public string Connection()
    {
        var connString = ConfigurationManager.ConnectionStrings["LocalSqlServer"];
        string strConnString = connString.ConnectionString;
        return strConnString;
    }

    public string select(string field, string tbl, string whr)
    {
        return "select " + field + " from " + tbl + " where " + whr + "";
    }

    public string select1(string field, string tbl)
    {
        return "select " + field + " from " + tbl + "";
    }

    public string insert(string tbl, string field, string value)
    {
        return "insert into " + tbl + " (" + field + ") Values (" + value + ")";
       
    }

    public string update(string tbl, string fieldval, string whr)
    {
        return "update " + tbl + " set " + fieldval + " Where " + whr;
    }

    public string delete(string tbl, string whr)
    {
        return "delete from " + tbl + " where " + whr;
    }

    public void dropdownCompany(DropDownList dpdlCompany)
    {
     try
       {
            DataSet DS = new DataSet();
            string connStr = this.Connection();
            SqlConnection con = new SqlConnection(connStr);
            string cmdStr = this.select1("SUBSTRING(CompanyName,0,27)+'....' as CompName,CompId ", "tblCompany_master");        
            SqlCommand cmd = new SqlCommand(cmdStr, con);
            SqlDataAdapter DA = new SqlDataAdapter(cmd);
            DA.Fill(DS, "company");
            dpdlCompany.DataSource = DS.Tables["company"];
            dpdlCompany.DataTextField = "CompName";
            dpdlCompany.DataValueField = "CompId";
            dpdlCompany.DataBind();
            dpdlCompany.Items.Insert(0, "Select");
        }
        catch(Exception ex) { }
              
    }

    public void dropdownCategory(DropDownList dpdlCategory)
    {
        try
        {
            DataSet DS = new DataSet();
            string connStr = this.Connection();
            SqlConnection con = new SqlConnection(connStr);
            string cmdStr = this.select1("*", "Category_master");
            SqlCommand cmd = new SqlCommand(cmdStr, con);
            SqlDataAdapter DA = new SqlDataAdapter(cmd);
            DA.Fill(DS, "category");
            dpdlCategory.DataSource = DS.Tables["category"];
            dpdlCategory.DataTextField = "Name";
            dpdlCategory.DataValueField = "Id";
            dpdlCategory.DataBind();
            dpdlCategory.Items.Insert(0, "Select");
        }
        catch (Exception ex){ }     
    }

    public void dropdownCategoryBYId(DropDownList dpdlCategory,string whr)
    {
        try
        {

            DataSet DS = new DataSet();
            string connStr = this.Connection();
            SqlConnection con = new SqlConnection(connStr);
            string cmdStr = this.select("*", "Category_master",whr);
            SqlCommand cmd = new SqlCommand(cmdStr, con);
            SqlDataAdapter DA = new SqlDataAdapter(cmd);
            DA.Fill(DS, "category");
            dpdlCategory.DataSource = DS.Tables["category"];
            dpdlCategory.DataTextField = "Name";
            dpdlCategory.DataValueField = "Id";
            dpdlCategory.DataBind();           
        }
       catch (Exception ex) { }
        
    }

    public void dropdownBG(DropDownList dpdlBG)
    {
        try
        {
            DataSet DS = new DataSet();
            string connStr = this.Connection();
            SqlConnection con = new SqlConnection(connStr);
            string cmdStr = this.select1("Symbol As name,Id ", "BusinessGroup");          
            SqlCommand cmd = new SqlCommand(cmdStr, con);
            SqlDataAdapter DA = new SqlDataAdapter(cmd);
            DA.Fill(DS, "Business");
            dpdlBG.DataSource = DS.Tables["Business"];
            dpdlBG.DataTextField = "name";
            dpdlBG.DataValueField = "Id";
            dpdlBG.DataBind();
            dpdlBG.Items.Insert(0, "Select");

        }
        catch (Exception ex) {  }   

    }

    public void dropdownBuyer(DropDownList DDLBuyer)
    {
         try
        {
            DataSet DS = new DataSet();
            string connStr = this.Connection();
            SqlConnection con = new SqlConnection(connStr);
            string cmdStr = this.select("tblMM_Buyer_Master.Id,tblMM_Buyer_Master.Category+Convert(Varchar,tblMM_Buyer_Master.Nos)+'-'+tblHR_OfficeStaff.EmployeeName+' ['+tblMM_Buyer_Master.EmpId+' ]' As Buyer", "tblMM_Buyer_Master,tblHR_OfficeStaff", "tblMM_Buyer_Master.EmpId=tblHR_OfficeStaff.EmpId AND tblMM_Buyer_Master.Id!=0 ");

            SqlCommand cmd = new SqlCommand(cmdStr, con);
            SqlDataAdapter DA = new SqlDataAdapter(cmd);
            DA.Fill(DS, "tblMM_Buyer_Master");
            DDLBuyer.DataSource = DS.Tables["tblMM_Buyer_Master"];
            DDLBuyer.DataTextField = "Buyer";
            DDLBuyer.DataValueField = "Id";
            DDLBuyer.DataBind();
            DDLBuyer.Items.Insert(0, "Select");

        }
         catch (Exception ex) { }

    }

    public void dropdownBGId(DropDownList dpdlBG,string whr)
    {
        try
        {

            DataSet DS = new DataSet();
            string connStr = this.Connection();
            SqlConnection con = new SqlConnection(connStr);
            string cmdStr = this.select("Symbol As name,Id ", "BusinessGroup",whr);           
            SqlCommand cmd = new SqlCommand(cmdStr, con);
            SqlDataAdapter DA = new SqlDataAdapter(cmd);
            DA.Fill(DS, "Business");
            dpdlBG.DataSource = DS.Tables["Business"];
            dpdlBG.DataTextField = "name";
            dpdlBG.DataValueField = "Id";
            dpdlBG.DataBind();           

        }
        catch (Exception ex)   { }       
    }

    public void dropdownUnit(DropDownList dpdlunit)
    {
        try
        {            
            DataSet DS = new DataSet();
            string connStr = this.Connection();
            SqlConnection con = new SqlConnection(connStr);
            string cmdStr = this.select1("Symbol,Id","Unit_Master");
            SqlCommand cmd = new SqlCommand(cmdStr, con);
            SqlDataAdapter DA = new SqlDataAdapter(cmd);
            DA.Fill(DS, "unit");
            dpdlunit.DataSource = DS.Tables["unit"];
            dpdlunit.DataTextField = "Symbol";
            dpdlunit.DataValueField = "Id";
            dpdlunit.DataBind();
            dpdlunit.Items.Insert(0, "Select");
            
        }
        catch (Exception ex) { }
              
    }

    public void dropdownFinYear(DropDownList dpdlFinYear,DropDownList dpdlFinCompId)
    {
       try
        {
            if (dpdlFinCompId.SelectedValue.ToString() != "Select")
            {
                DataSet DS = new DataSet();
                string connStr = this.Connection();
                SqlConnection con = new SqlConnection(connStr);
                string cmdStr = this.select("*", "tblFinancial_master","CompId='"+dpdlFinCompId.SelectedValue+"'order by FinYearId Desc " );
                SqlCommand cmd = new SqlCommand(cmdStr, con);
                SqlDataAdapter DA = new SqlDataAdapter(cmd);
                DA.Fill(DS, "tblFinancial_master");
               
                dpdlFinYear.DataSource = DS.Tables[0];
                dpdlFinYear.DataTextField = "FinYear";
                dpdlFinYear.DataValueField = "FinYearId";
                dpdlFinYear.DataBind();
                dpdlFinYear.Items.Insert(0, "Select");
            }
            else
            {
                dpdlFinYear.Items.Clear();
                dpdlFinYear.Items.Insert(0, "Select");
            }
            
        }
        catch(Exception ex) { }
    }

    public string getCurrDate()
    {
        DateTime dd = DateTime.Now;
        string currdate=dd.ToString("yyyy-MM-dd");
        return currdate;

    }

    public string getCurrTime()
    {
        DateTime dt = DateTime.Now;
        string currTime = dt.ToString("T");
        return currTime;

    }

    public string FromDate(string FD)
    {

        string NFD = "";
        try
        {
            string a = FD;
            string[] b = a.Split('-');
            string d = b[0];
            string m = b[1];
            string y = b[2];
            NFD = y + "-" + m + "-" + d;
            return NFD;
        }
        catch (Exception ex) { }
        return NFD;       
    }

    public string ToDate(string TD)
    {
        string NTD = "";

        try
        {
            string a = TD;
            string[] b = a.Split('-');
            string d = b[0];
            string m = b[1];
            string y = b[2];
            NTD = y + "-" + m + "-" + d;
            return NTD;
        }
        catch (Exception ex) { }
        return NTD;
    }

    public string FromDateDMY(string FD)
    {
        string NFD = "";
        try
        {
            string a = FD;
            string[] b = a.Split('-');
            string y = b[0];
            string m = b[1];
            string d = b[2];
            NFD = d + "-" + m + "-" + y;
            return NFD;
        }
        catch (Exception st){}
        return NFD;
    }

    public string ToDateDMY(string TD)
    {
        string NTD = "";
        try
        {
            string a = TD;
            string[] b = a.Split('-');
            string y = b[0];
            string m = b[1];
            string d = b[2];
            NTD = d + "-" + m + "-" + y;
            return NTD;
        }
        catch (Exception ex) { }
        return NTD;
    }

    public string FromDateYear(string FDY)
    {
        string NFD = "";
        try
        {
            string a = FDY;
            string[] b = a.Split('-');
            string y = b[0];
            string m = b[1];
            string d = b[2];
            NFD = y + "-";
            return NFD;
        }
        catch (Exception ex) { }
        return NFD;
    }

    public string ToDateYear(string TDY)
    {

         string NTD = "";
         try
         {
             string a = TDY;
             string[] b = a.Split('-');
             string y = b[0];
             string m = b[1];
             string d = b[2];
             NTD = y;
             return NTD;
         }
         catch (Exception ex) { }
         return NTD;
    
    }

    public string fYear(string fyear)
    {
        string NTD = "";
         try
         {
        string a = fyear;
        string[] b = a.Split('-');
        string d = b[0];       
        string m = b[1];
        string y = b[2];
        NTD = y+"-";
        return NTD;
        }
         catch (Exception ex) { }
         return NTD;
    }

    public string tYear(string tyear)
    {
         string NTD = "";
         try
         {
        string a = tyear;
        string[] b = a.Split('-');
        string d = b[0];
        string m = b[1];
        string y = b[2];
        NTD = y;
        return NTD;
       }
         catch (Exception ex) { }
         return NTD;

    }

    public void dropdownCountry(DropDownList dpdlCountry, DropDownList dpdlState)
    {
        try
        {
            string connStr = this.Connection();
            SqlConnection Conn = new SqlConnection(connStr);
            DataSet DS = new DataSet();
            string strcmd = this.select1("*", "tblcountry");
            SqlCommand Cmd = new SqlCommand(strcmd, Conn);
            SqlDataAdapter DA = new SqlDataAdapter(Cmd);
            DA.Fill(DS, "tblcountry");
            dpdlCountry.DataSource = DS.Tables["tblcountry"];
            dpdlCountry.DataTextField = "CountryName";
            dpdlCountry.DataValueField = "CId";
            dpdlCountry.DataBind();
            dpdlCountry.Items.Insert(0, "Select");
            dpdlState.ClearSelection();
        }
        catch (Exception ex)
        {
        }
    }

    public string getCity(int cityid,int field) 
    {
        string connStr = this.Connection();
        SqlConnection Conn = new SqlConnection(connStr);
        DataSet DS = new DataSet();
        string strcmd = this.select("*", "tblCity", "CityId='" + cityid + "'");
        SqlCommand Cmd = new SqlCommand(strcmd, Conn);
        SqlDataAdapter DA = new SqlDataAdapter(Cmd);
        DA.Fill(DS, "tblCity");
        return DS.Tables[0].Rows[0][field].ToString();
    }

    public string getState(int stateid, int field)
    {
        string connStr = this.Connection();
        SqlConnection Conn = new SqlConnection(connStr);
        DataSet DS = new DataSet();
        string strcmd = this.select("*", "tblState", "SId='" + stateid + "'");
        SqlCommand Cmd = new SqlCommand(strcmd, Conn);
        SqlDataAdapter DA = new SqlDataAdapter(Cmd);
        DA.Fill(DS, "tblState");
        return DS.Tables[0].Rows[0][field].ToString();
    }

    public string getCountry(int cntid, int field)
    {
        string connStr = this.Connection();
        SqlConnection Conn = new SqlConnection(connStr);
        DataSet DS = new DataSet();
        string strcmd = this.select("*", "tblCountry", "CId='" + cntid + "'");
        SqlCommand Cmd = new SqlCommand(strcmd, Conn);
        SqlDataAdapter DA = new SqlDataAdapter(Cmd);
        DA.Fill(DS, "tblCountry");
        return DS.Tables[0].Rows[0][field].ToString();
    }

    public void dropdownCountrybyId(DropDownList dpdlCountry, DropDownList dpdlState, string whr)
    {
        try
        {
            string connStr = this.Connection();
            SqlConnection Conn = new SqlConnection(connStr);
            DataSet DS = new DataSet();
            string strcmd = this.select("* ", "tblcountry", whr);
            SqlCommand Cmd = new SqlCommand(strcmd, Conn);
            SqlDataAdapter DA = new SqlDataAdapter(Cmd);
            DA.Fill(DS, "tblcountry");
            dpdlCountry.DataSource = DS.Tables["tblcountry"];
            dpdlCountry.DataTextField = "CountryName";
            dpdlCountry.DataValueField = "CId";
            dpdlCountry.DataBind();
            dpdlCountry.Items.Insert(0, "Select");
            dpdlState.ClearSelection();
        }
        catch (Exception ex) { }
         
    }

    public void dropdownState(DropDownList dpdlState, DropDownList dpdlCity, DropDownList dpdlCountry)
    {
        try
        {

            if (dpdlCountry.SelectedValue.ToString() != "Select")
            {
                string connStr = Connection();
                SqlConnection Conn = new SqlConnection(connStr);
                DataSet DS = new DataSet();
                string strcmd = this.select("SUBSTRING(StateName,0,15)+'....' as StateName,SId", "tblState", "CId='" + dpdlCountry.SelectedValue + "'");
                SqlCommand Cmd = new SqlCommand(strcmd, Conn);
                SqlDataAdapter DA = new SqlDataAdapter(Cmd);
                DA.Fill(DS, "tblstate");
                dpdlState.DataSource = DS.Tables["tblState"];
                dpdlState.DataTextField = "StateName";
                dpdlState.DataValueField = "SId";
                dpdlState.DataBind();
                dpdlState.Items.Insert(0, "Select");
                dpdlCity.ClearSelection();
            }
            else
            {
                dpdlState.Items.Clear();
                dpdlState.Items.Insert(0, "Select");

            }

        }
        catch (Exception ex)  { }
      
       
    }

    public void dropdownStatebyId(DropDownList dpdlState, string whr)
    {
        try
        {
            
            string connStr = Connection();
            SqlConnection Conn = new SqlConnection(connStr);
            DataSet DS = new DataSet();
            string strcmd = this.select("*", "tblState", whr);
            SqlCommand Cmd = new SqlCommand(strcmd, Conn);
            SqlDataAdapter DA = new SqlDataAdapter(Cmd);
            DA.Fill(DS, "tblstate");
            ListItem li = new ListItem();
            li.Text = DS.Tables[0].Rows[0]["StateName"].ToString();
            li.Value = DS.Tables[0].Rows[0]["SId"].ToString();
            dpdlState.Items.Add(li);

        }
        catch (Exception ex){ }      
       
    }

    public void dropdownCity(DropDownList dpdlCity, DropDownList dpdlState)
    {
        try
        {
            if (dpdlState.SelectedValue.ToString() != "Select")
            {
                string connStr = Connection();
                SqlConnection Conn = new SqlConnection(connStr);
                DataSet DS = new DataSet();
                string strcmd = this.select("SUBSTRING(CityName,0,15)+'....' as CityName,CityId ", "tblCity", "SId='" + dpdlState.SelectedValue + "'");
                SqlCommand Cmd1 = new SqlCommand(strcmd, Conn);
                SqlDataAdapter DA1 = new SqlDataAdapter(Cmd1);
                DA1.Fill(DS, "tblCity");
                dpdlCity.DataSource = DS.Tables["tblCity"];
                dpdlCity.DataTextField = "CityName";
                dpdlCity.DataValueField = "CityId";
                dpdlCity.DataBind();
                dpdlCity.Items.Insert(0, "Select");
            }
            else
            {
                dpdlCity.Items.Clear();
                dpdlCity.Items.Insert(0, "Select");
            }
        }
        catch (Exception ex)
        {
        }
    }

    public void dropdownCitybyId(DropDownList dpdlCity, string whr)
    {
        try
        {
            string connStr = Connection();
            SqlConnection Conn = new SqlConnection(connStr);
            DataSet DS = new DataSet();
            string strcmd = this.select("*", "tblCity", whr);
            SqlCommand Cmd1 = new SqlCommand(strcmd, Conn);
            SqlDataAdapter DA1 = new SqlDataAdapter(Cmd1);
            DA1.Fill(DS, "tblCity");
            ListItem li = new ListItem();
            li.Text = DS.Tables[0].Rows[0]["CityName"].ToString();
            li.Value = DS.Tables[0].Rows[0]["CityId"].ToString();
            dpdlCity.Items.Add(li);
        }
        catch (Exception ex)
        {
        }
    }

    public string link(string pagename,string LinkName )
    {
        string lb = @"<a href='" + pagename + "' style=color:#FFFFFF>"+LinkName+"</a>";
        return lb;
    }

    public int[] AcessMaster(int CompId, int FinYearId, string EmpId, int ModId, int SubModID)
    {
        string connStr = Connection();
        SqlConnection Conn = new SqlConnection(connStr);
        DataSet DS = new DataSet();
        string strcmd = this.select("*", "tblAccess_Master", "CompId=" + CompId + "and FinYearId=" + FinYearId + "and EmpId='" + EmpId.ToString() + "'and ModId=" + ModId + "and SubModId=" + SubModID + "");
        SqlCommand Cmd = new SqlCommand(strcmd, Conn);
        SqlDataAdapter DA = new SqlDataAdapter(Cmd);
        DA.Fill(DS, "tblAccess_Master");


        int[] Access;
        Access = new int[DS.Tables[0].Rows.Count];
        for (int i = 0; i < DS.Tables[0].Rows.Count; i++)
        {
            Access[i] = Convert.ToInt32(DS.Tables[0].Rows[i]["Access"]);
            
        }
        return Access;


    }

    //   Get Customer Company Name(only charactor) From TextBox Function...
    public string getCustChar(string a)
    {
        string charTrim = a.Trim();
        string charSubstr = charTrim.Substring(0, 1);
        return charSubstr;
    }

    //   Get Customer Company Code From TextBox Function...
    public string getCode(string code)
    {
        string[] custId2 = { };
        try
        {
            string[] custId1 = code.Split('[');
            custId2 = custId1[1].Split(']');
            return custId2[0];
        }
        catch (Exception tt){  }
        return "";
    }

    public string getWOChar(string a)
    {
        string charTrim = a.Trim();
        string charSubstr = charTrim.Substring(0, 1);
        return charSubstr;
    }

    public string getWO(string a)
    {
        string charTrim = a.Trim();
        
        string charSubstr = charTrim.Substring(2);
        return charSubstr;
    }

    public string SPRNoCodeFY(string SPRNC)
    {
        string Z = "Z";
        string charTrim = SPRNC.Trim();
        string charSubstr1 = charTrim.Substring(2, 2);
        string charSubstr = Z + charSubstr1;
        return charSubstr;
    }
    
    //  Search GridView Data Bind Function...
    public void BindDataCust1(string tblName, string tblfield, string whr, GridView SearchGridView, string drpvalue, string hfSearchTextValue)
    {
        try

        {
            string strConnString = this.Connection();
            SqlConnection con = new SqlConnection(strConnString);
            DataSet DS = new DataSet();

             //hfSearchText has the search string returned from the grid.
                
                string sql = this.select(tblfield, tblName, whr);

                SqlCommand cmd = new SqlCommand(sql, con);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(DS, tblName);

                if (hfSearchTextValue != "")
                {
                    DS.Clear();
                    sql += " AND " + drpvalue + " like '" + hfSearchTextValue + "%'";
                    SqlCommand cmd1 = new SqlCommand(sql, con);
                    SqlDataAdapter da1 = new SqlDataAdapter(cmd1);
                    da1.Fill(DS, tblName);
                }
                SearchGridView.DataSource = DS;
                SearchGridView.DataBind();
          
           
        }
        catch (Exception exp)
        {
            //If databinding threw exception bcoz current page index is > than available page index
            SearchGridView.PageIndex = 0;
            SearchGridView.DataBind();
        }
        finally
        {
            //Select the first row returned
            if (SearchGridView.Rows.Count > 0)
                SearchGridView.SelectedIndex = 0;
        }
    }

    // Search GridView Data Bind Function for customer master...

    public void BindDataCust(string tblName,string tblfield,string whr, GridView SearchGridView,string drpvalue, string hfSearchTextValue)
    {
       try
        {
            //hfSearchText has the search string returned from the grid.
            string strConnString = this.Connection();
            SqlConnection con = new SqlConnection(strConnString);
            DataSet DS = new DataSet();           
            string sql = this.select(tblfield, tblName, whr);
            SqlCommand cmd = new SqlCommand(sql, con);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(DS, tblName);
            if (hfSearchTextValue != "")
            {
                DS.Clear();
                sql += " AND " + drpvalue + " like '" + hfSearchTextValue + "%'";
                SqlCommand cmd1 = new SqlCommand(sql, con);
                SqlDataAdapter da1 = new SqlDataAdapter(cmd1);
                da1.Fill(DS, tblName);
            }
            SearchGridView.DataSource = DS;            
            SearchGridView.DataBind();
            
        }
        catch (Exception exp)
        {            
        }
        finally
        {            
        }
    }

    public string revDate(string fld, string binname)
    {
        return "REPLACE(CONVERT(varchar, CONVERT(datetime, SUBSTRING(" + fld + ", CHARINDEX('-', " + fld + ") + 1, 2) + '-' + LEFT(" + fld + ",CHARINDEX('-', " + fld + ") - 1) + '-' + RIGHT(" + fld + ", CHARINDEX('-', REVERSE(" + fld + ")) - 1)), 103), '/', '-')AS " + binname + "";

    }

    public string TranNo(string tblname, string fieldname,int compid) // continue no
    {
        string strConnString = this.Connection();
        SqlConnection con = new SqlConnection(strConnString);
        DataSet DS = new DataSet();

        string sql = this.select(fieldname,tblname,"CompId="+compid+" Order by " + fieldname + " Desc");
        SqlCommand cmd = new SqlCommand(sql, con);
        SqlDataAdapter da = new SqlDataAdapter(cmd);
        da.Fill(DS);

        if (DS.Tables[0].Rows.Count > 0)
        {
            return (Convert.ToInt32(DS.Tables[0].Rows[0][0]) + 1).ToString("D4");
        }
        else
        {
            return "0001";
        }
    }
    
    public string CompAdd(int cid)
    {
        string connStr = this.Connection();
        SqlConnection sqladd = new SqlConnection(connStr);
        string address="";
       try
        {
            sqladd.Open();
            string Queryadd = this.select("*", "tblCompany_master", "CompId='" + cid + "'");
            SqlCommand cmd = new SqlCommand(Queryadd, sqladd);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds, "tblCompany_master");
            if (ds.Tables[0].Rows.Count>0)
           {
            address = ds.Tables[0].Rows[0]["RegdAddress"].ToString() + " " + this.getCity(Convert.ToInt32(ds.Tables[0].Rows[0]["RegdCity"]), 1) + " " + this.getState(Convert.ToInt32(ds.Tables[0].Rows[0]["RegdState"]), 1) + " " + this.getCountry(Convert.ToInt32(ds.Tables[0].Rows[0]["RegdCountry"]), 1) + " PIN No.-" + ds.Tables[0].Rows[0]["RegdPinCode"].ToString() + " Ph No.-" + ds.Tables[0].Rows[0]["RegdContactNo"].ToString() + " Fax No.-" + ds.Tables[0].Rows[0]["RegdFaxNo"].ToString() + " Email No.-" + ds.Tables[0].Rows[0]["RegdEmail"].ToString();
           }
       }
        catch (Exception ex)
        {

        }
        finally
        {
            sqladd.Close();
        }
        return address;
    }

    public string CompPlantAdd(int cid)
    {
        string connStr = this.Connection();
        SqlConnection sqladd = new SqlConnection(connStr);
        string address = "";
        try
        {
            sqladd.Open();
            string Queryadd = this.select("*", "tblCompany_master", "CompId='" + cid + "'");
            SqlCommand cmd = new SqlCommand(Queryadd, sqladd);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();

            da.Fill(ds, "tblCompany_master");

            address = ds.Tables[0].Rows[0]["PlantAddress"].ToString() + " " + this.getCity(Convert.ToInt32(ds.Tables[0].Rows[0]["PlantCity"]), 1) + " " + this.getState(Convert.ToInt32(ds.Tables[0].Rows[0]["PlantState"]), 1) + " " + this.getCountry(Convert.ToInt32(ds.Tables[0].Rows[0]["PlantCountry"]), 1) + " PIN No.-" + ds.Tables[0].Rows[0]["PlantPinCode"].ToString() + " Ph No.-" + ds.Tables[0].Rows[0]["PlantContactNo"].ToString() + " Fax No.-" + ds.Tables[0].Rows[0]["PlantFaxNo"].ToString() + " Email No.-" + ds.Tables[0].Rows[0]["PlantEmail"].ToString();

        }
        catch (Exception ex)
        {

        }
        finally
        {
            sqladd.Close();
        }
        return address;
    }

    public string getProjectTitle(string wono)
    {
        string connStr = this.Connection();
        SqlConnection sqladd = new SqlConnection(connStr);
        string Title = "";
        try
        {
            sqladd.Open();
            string QueryTitle = this.select("TaskProjectTitle", "SD_Cust_WorkOrder_Master", "WONo='" + wono + "'");
            SqlCommand cmd = new SqlCommand(QueryTitle, sqladd);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds, "SD_Cust_WorkOrder_Master");

            Title = ds.Tables[0].Rows[0][0].ToString();
        }
        catch (Exception ex)
        {

        }
        finally
        {
            sqladd.Close();
        }
        return Title;
    }
    
    public string getCompany(int CId)
    {
        // To Show Company Name on Crystal Report
        string Company="";
        string connStr = this.Connection();
        SqlConnection sqladd = new SqlConnection(connStr);
        string strCompName = this.select("CompanyName", "tblCompany_master", "CompId='" + CId + "'");
        SqlCommand myCompName = new SqlCommand(strCompName, sqladd);
        SqlDataAdapter adCompName = new SqlDataAdapter(myCompName);
        DataSet dsCompName = new DataSet();
        adCompName.Fill(dsCompName, "tblCompany_master");
        if (dsCompName.Tables[0].Rows.Count>0)
        {
         Company = dsCompName.Tables[0].Rows[0]["CompanyName"].ToString();
        }
        return Company;
        
    }

   public void EmptyGridFix(GridView grdView)
    {
        // normally executes after a grid load method
        if (grdView.Rows.Count == 0 &&
            grdView.DataSource != null)
        {
            DataTable dt = null;

            // need to clone sources otherwise
            // it will be indirectly adding to 
            // the original source

            if (grdView.DataSource is DataSet)
            {
                dt = ((DataSet)grdView.DataSource).Tables[0].Clone();
            }
            else if (grdView.DataSource is DataTable)
            {
                dt = ((DataTable)grdView.DataSource).Clone();
            }

            if (dt == null)
            {
                return;
            }

            dt.Rows.Add(dt.NewRow()); // add empty row
            grdView.DataSource = dt;
            grdView.DataBind();

            // hide row
            grdView.Rows[0].Visible = false;
            grdView.Rows[0].Controls.Clear();
        }

        // normally executes at all postbacks
        if (grdView.Rows.Count == 1 &&
            grdView.DataSource == null)
        {
            bool bIsGridEmpty = true;

            // check first row that all cells empty
            for (int i = 0; i < grdView.Rows[0].Cells.Count; i++)
            {
                if (grdView.Rows[0].Cells[i].Text != string.Empty)
                {
                    bIsGridEmpty = false;
                }
            }
            // hide row
            if (bIsGridEmpty)
            {
                grdView.Rows[0].Visible = false;
                grdView.Rows[0].Controls.Clear();
            }
        }
    }

   public void TotOfModule(string tblName, string tblfield, string whr, GridView SearchGridView)
   {
       try
       {
           //hfSearchText has the search string returned from the grid.
           string strConnString = this.Connection();
           SqlConnection con = new SqlConnection(strConnString);
           DataSet DS = new DataSet();

           string sql = this.select(tblfield, tblName, whr);
           
           SqlCommand cmd = new SqlCommand(sql, con);
           SqlDataAdapter da = new SqlDataAdapter(cmd);
           da.Fill(DS, tblName);

           SearchGridView.DataSource = DS;
           SearchGridView.DataBind();
           //this.EmptyGridFix(SearchGridView);

       }
       catch (Exception exp)
       {
           ////If databinding threw exception bcoz current page index is > than available page index
           SearchGridView.PageIndex = 0;
           SearchGridView.DataBind();
           //this.EmptyGridFix(SearchGridView);

       }
       finally
       {
           ////Select the first row returned
           if (SearchGridView.Rows.Count > 0)
               SearchGridView.SelectedIndex = 0;
       }
   }

   public int SetLimit(int scid, int limit, int Cid)
   {
       int partNoLimit = 0;
       try
       {
           string strConnString = this.Connection();
           SqlConnection con = new SqlConnection(strConnString);
           // Code To Get Symbol Of Sub_Category and Add It In ItemCode
           DataSet DS2 = new DataSet();
           string cmdstr2 = this.select("Symbol", "tblDG_SubCategory_Master", "SCId='" + scid + "'");
           SqlCommand cmd2 = new SqlCommand(cmdstr2, con);
           SqlDataAdapter da2 = new SqlDataAdapter(cmd2);
           da2.Fill(DS2, "tblDG_SubCategory_Master");

           if (Cid != 0)
           {
               if (DS2.Tables[0].Rows.Count > 0)
               {
                   partNoLimit = limit - 5;
               }
               else
               {
                   partNoLimit = limit - 3;
               }


           }
           else
           {
               partNoLimit = limit - 2;
           }
           return partNoLimit;
       }
       catch (Exception ec) { };
       return partNoLimit;
   }

   public int SetBomItemLimit(int limit,int wolen)
   {
          int partNoLimit = 0;       
          partNoLimit = limit -(wolen+3);    
          return partNoLimit;
   }

   public string createItemCode(int cid, int scid, string pno, string rev, string pro)
   {
       string strConnString = this.Connection();
       SqlConnection con = new SqlConnection(strConnString);

       // Code To Get Symbol Of Category and Add It In ItemCode
       DataSet DS = new DataSet();
       string cmdstr = "Select Symbol from tblDG_Category_Master where CId='" + cid + "'";
       SqlCommand cmd = new SqlCommand(cmdstr, con);
       SqlDataAdapter da = new SqlDataAdapter(cmd);
       da.Fill(DS, "tblDG_Category_Master");
       string symbol = DS.Tables[0].Rows[0]["Symbol"].ToString();
       // Code To Get Symbol Of Sub_Category and Add It In ItemCode
       DataSet DS2 = new DataSet();
       string cmdstr2 = "Select Symbol from tblDG_SubCategory_Master where SCId='" + scid + "'";
       SqlCommand cmd2 = new SqlCommand(cmdstr2, con);
       SqlDataAdapter da2 = new SqlDataAdapter(cmd2);
       da2.Fill(DS2, "tblDG_Category_Master");
       string symbol2 = "";
       if (DS2.Tables[0].Rows.Count > 0)
       {
           if(scid==0)
           {
               symbol2 = "00";
           }
           else
           {
           symbol2 = DS2.Tables[0].Rows[0]["Symbol"].ToString();
           }
       }
       else
       {
           symbol2 = "";
       }
       // Code To Get Symbol Of Process and Add It In ItemCode
       DataSet DSPro = new DataSet();
       string cmdstrPro = "Select Symbol from tblPln_Process_Master where Id='" + pro + "'";
       SqlCommand cmdPro = new SqlCommand(cmdstrPro, con);
       SqlDataAdapter daPro = new SqlDataAdapter(cmdPro);
       daPro.Fill(DSPro, "tblDG_Category_Master");
       string Pro = DSPro.Tables[0].Rows[0]["Symbol"].ToString();
       string itemcode = string.Concat(symbol, symbol2, pno, rev, Pro);       
       return itemcode;    

   }
    
   public void drpunit(DropDownList dpdlunit)
   {
       try
       {

           DataSet DS = new DataSet();
           string connStr = this.Connection();
           SqlConnection con = new SqlConnection(connStr);
           string cmdStr = this.select1("Symbol,Id ", "Unit_Master");

           SqlCommand cmd = new SqlCommand(cmdStr, con);
           SqlDataAdapter DA = new SqlDataAdapter(cmd);
           DA.Fill(DS, "unit");
           dpdlunit.DataSource = DS.Tables["unit"];
           dpdlunit.DataTextField = "Symbol";
           dpdlunit.DataValueField = "Id";
           dpdlunit.DataBind();
           dpdlunit.Items.Insert(0, "Select");


       }
       catch (Exception ex)
       {

       }


   }

   public void drpLocat(DropDownList dpdplace)
   {
       try
       {

           DataSet DS5 = new DataSet();
           string connStr = this.Connection();
           SqlConnection con = new SqlConnection(connStr);

           SqlCommand cmd = new SqlCommand("Select LocationLabel+'-'+LocationNo as Location,Id from tblDG_Location_Master WHERE Id !=0", con);
           SqlDataAdapter DA = new SqlDataAdapter(cmd);
           DA.Fill(DS5, "tblDG_Location_Master");
           dpdplace.DataSource = DS5.Tables["tblDG_Location_Master"];
           dpdplace.DataTextField = "Location";
           dpdplace.DataValueField = "Id";
           dpdplace.DataBind();
           dpdplace.Items.Insert(0, "Select");


       }
      catch (Exception ex)
       {

       }

   }
    
   public void drpDesignCategory(DropDownList DrpCategory, DropDownList DrpSubCategory)
   {
       string connStr = this.Connection();
       SqlConnection con = new SqlConnection(connStr);
       DataSet DS = new DataSet();
       try
       {
           SqlCommand Cmdcategory = new SqlCommand("Select CId,Symbol+' - '+CName As CatName From tblDG_Category_Master", con);
           SqlDataAdapter DAcategory = new SqlDataAdapter(Cmdcategory);
           DAcategory.Fill(DS, "tblDG_Category_Master");
           DrpCategory.DataSource = DS.Tables["tblDG_Category_Master"];
           DrpCategory.DataTextField = "CatName";
           DrpCategory.DataValueField = "CId";
           DrpCategory.DataBind();
           DrpCategory.Items.Insert(0, "Select Category");
           DrpSubCategory.Items.Insert(0, "Select SubCategory");
          
       }
       catch (Exception ex)
       {

       }
       finally
       {
           con.Close();

       }
   }

   public void drpCategoryOnly(DropDownList DrpCategory)
   {
       string connStr = this.Connection();
       SqlConnection con = new SqlConnection(connStr);
       DataSet DS = new DataSet();
       try
       {

           SqlCommand Cmdcategory = new SqlCommand("Select CId,Symbol+' - '+CName As CatName From tblDG_Category_Master", con);
           SqlDataAdapter DAcategory = new SqlDataAdapter(Cmdcategory);
           DAcategory.Fill(DS, "tblDG_Category_Master");
           DrpCategory.DataSource = DS.Tables["tblDG_Category_Master"];
           DrpCategory.DataTextField = "CatName";
           DrpCategory.DataValueField = "CId";
           DrpCategory.DataBind();
           DrpCategory.Items.Insert(0, "Select Category");

       }
       catch (Exception ex)
       {

       }
       finally
       {
           con.Close();
       }
   }
    
   double Y = 1;

   public double RecurQty(string WONo, int Pid, int Cid, double Y,int CompId,int FinId)
   {
       try
       {
           string connStr = this.Connection();
           SqlConnection con = new SqlConnection(connStr);
           DataSet DS = new DataSet();
           string cmdStr = this.select("Qty", "tblDG_TPL_Master", " WONo='" + WONo + "' AND PId='" + Pid + "'AND CId='" + Cid + "'AND CompId='" + CompId + "'AND FinYearId<='" + FinId + "'");
           SqlCommand cmd = new SqlCommand(cmdStr, con);
           SqlDataAdapter DA = new SqlDataAdapter(cmd);
           DA.Fill(DS, "tblDG_TPL_Master");
           Y = Y * Convert.ToDouble(decimal.Parse(DS.Tables[0].Rows[0]["Qty"].ToString()).ToString("N3"));
           if (Pid > 0)
           {
               DataSet DS2 = new DataSet();
               string cmdStr2 = this.select("PId,Qty", "tblDG_TPL_Master", "WONo='" + WONo + "' AND CId='" + Pid + "'AND CompId='" + CompId + "'AND FinYearId<='" + FinId + "'");
               SqlCommand cmd2 = new SqlCommand(cmdStr2, con);
               SqlDataAdapter DA2 = new SqlDataAdapter(cmd2);
               DA2.Fill(DS2, "tblDG_TPL_Master");
               Y = Y * (Convert.ToDouble(decimal.Parse(DS2.Tables[0].Rows[0][1].ToString()).ToString("N3")));

               int Rpid = Convert.ToInt32(DS2.Tables[0].Rows[0][0]);
               if (Rpid > 0)
               {
                   DataSet DS4 = new DataSet();
                   string cmdStr4 = this.select("PId", "tblDG_TPL_Master", "WONo='" + WONo + "' AND CId='" + Rpid + "'AND CompId='" + CompId + "'AND FinYearId<='" + FinId + "'");
                   SqlCommand cmd4 = new SqlCommand(cmdStr4, con);
                   SqlDataAdapter DA4 = new SqlDataAdapter(cmd4);
                   DA4.Fill(DS4, "tblDG_TPL_Master");
                   int rrpid = Convert.ToInt32(DS4.Tables[0].Rows[0][0]);
                   //return this.RecurQty(WONo, rrpid, Rpid, Y,CompId,FinId);
                   return Math.Round(this.RecurQty(WONo, rrpid, Rpid, Y, CompId, FinId),5);
               }
           }
       }

       catch (Exception ex)
       {

       }
       return Math.Round(Y,5);

   }

   //double p = 1;

   //public double BOMRecurQty(string WONo, int Pid, int Cid, double p,int compid,int finid )
   //{
   //   try
   //    {
   //        string connStr = this.Connection();
   //        SqlConnection con = new SqlConnection(connStr);
   //        DataSet DS = new DataSet();
   //        string cmdStr = this.select("Qty", "tblDG_BOM_Master", " WONo='" + WONo + "' AND PId='" + Pid + "'AND CId='" + Cid + "'And tblDG_BOM_Master.CompId='" + compid + "'AND tblDG_BOM_Master.FinYearId<='" + finid + "'");
   //        SqlCommand cmd = new SqlCommand(cmdStr, con);
   //        SqlDataAdapter DA = new SqlDataAdapter(cmd);
   //        DA.Fill(DS, "tblDG_BOM_Master");
   //        p = p * Convert.ToDouble(decimal.Parse(DS.Tables[0].Rows[0]["Qty"].ToString()).ToString("N3"));
   //        if (Pid > 0)
   //        {
   //            DataSet DS2 = new DataSet();
   //            string cmdStr2 = this.select("PId,Qty", "tblDG_BOM_Master", "WONo='" + WONo + "' AND CId='" + Pid + "'And tblDG_BOM_Master.CompId='" + compid + "'AND tblDG_BOM_Master.FinYearId<='" + finid + "'");
   //            SqlCommand cmd2 = new SqlCommand(cmdStr2, con);
   //            SqlDataAdapter DA2 = new SqlDataAdapter(cmd2);
   //            DA2.Fill(DS2, "tblDG_BOM_Master");
   //            if (DS2.Tables[0].Rows.Count>0)
   //            {
   //            p = p * (Convert.ToDouble(decimal.Parse(DS2.Tables[0].Rows[0][1].ToString()).ToString("N3")));
   //            }
   //            int Rpid = Convert.ToInt32(DS2.Tables[0].Rows[0][0]);
   //            if (Rpid > 0)
   //            {
   //                DataSet DS4 = new DataSet();
   //                string cmdStr4 = this.select("PId", "tblDG_BOM_Master", "WONo='" + WONo + "' AND CId='" + Rpid + "'And tblDG_BOM_Master.CompId='" + compid + "'AND tblDG_BOM_Master.FinYearId<='" + finid + "'");
   //                SqlCommand cmd4 = new SqlCommand(cmdStr4, con);
   //                SqlDataAdapter DA4 = new SqlDataAdapter(cmd4);
   //                DA4.Fill(DS4, "tblDG_BOM_Master");
   //                int rrpid = Convert.ToInt32(DS4.Tables[0].Rows[0][0]);
   //                //return this.BOMRecurQty(WONo, rrpid, Rpid, p, compid, finid);
   //                return Math.Round(this.BOMRecurQty(WONo, rrpid, Rpid, p, compid, finid),5);
   //            }
   //        }
   //    }

   //   catch (Exception ex)
   //    {

   //    }
   //    return Math.Round(p,5);

   //}
    
   double s = 1;
   public double TPLRecurQty(string WONo, int Pid, int Cid, double s, int compid, int finid)
   {
       try
       {
           string connStr = this.Connection();
           SqlConnection con = new SqlConnection(connStr);
           DataSet DS = new DataSet();
           string cmdStr = this.select("Qty", "tblDG_TPL_Master", " WONo='" + WONo + "' AND PId='" + Pid + "'AND CId='" + Cid + "'And CompId='" + compid + "'AND FinYearId<='" + finid + "'");
           SqlCommand cmd = new SqlCommand(cmdStr, con);
           SqlDataAdapter DA = new SqlDataAdapter(cmd);
           DA.Fill(DS, "tblDG_TPL_Master");
           s = s * Convert.ToDouble(decimal.Parse(DS.Tables[0].Rows[0]["Qty"].ToString()).ToString("N3"));
           if (Pid > 0)
           {
               DataSet DS2 = new DataSet();
               string cmdStr2 = this.select("PId,Qty", "tblDG_TPL_Master", "WONo='" + WONo + "' AND CId='" + Pid + "'And CompId='" + compid + "'AND FinYearId<='" + finid + "'");
               SqlCommand cmd2 = new SqlCommand(cmdStr2, con);
               SqlDataAdapter DA2 = new SqlDataAdapter(cmd2);
               DA2.Fill(DS2, "tblDG_TPL_Master");
               s = s * (Convert.ToDouble(decimal.Parse(DS2.Tables[0].Rows[0][1].ToString()).ToString("N3")));
               int Rpid = Convert.ToInt32(DS2.Tables[0].Rows[0][0]);
               if (Rpid > 0)
               {
                   DataSet DS4 = new DataSet();
                   string cmdStr4 = this.select("PId", "tblDG_TPL_Master", "WONo='" + WONo + "' AND CId='" + Rpid + "'And CompId='" + compid + "'AND FinYearId<='" + finid + "'");
                   SqlCommand cmd4 = new SqlCommand(cmdStr4, con);
                   SqlDataAdapter DA4 = new SqlDataAdapter(cmd4);
                   DA4.Fill(DS4, "tblDG_TPL_Master");
                   int rrpid = Convert.ToInt32(DS4.Tables[0].Rows[0][0]);
                   //return this.TPLRecurQty(WONo, rrpid, Rpid, s, compid, finid);
                   return Math.Round(this.TPLRecurQty(WONo, rrpid, Rpid, s, compid, finid),5);
               }
           }
       }

       catch (Exception ex)
       {

       }
       return Math.Round(s,5);

   }

   double k = 1;
   List<double> list = new List<double>();

   public List<double> TreeQty(string WONo,int Pid, int Cid)
   {
       string connStr = this.Connection();
       SqlConnection con = new SqlConnection(connStr);
       try
       {
           DataSet DS = new DataSet();
           string cmdStr = this.select("Qty", "tblDG_TPL_Master", "WONo='" + WONo + "' AND PId='" + Pid + "'And CId='" + Cid + "'");
           SqlCommand cmd = new SqlCommand(cmdStr, con);
           SqlDataAdapter DA = new SqlDataAdapter(cmd);
           DA.Fill(DS, "tblDG_TPL_Master");
           k = Convert.ToDouble(decimal.Parse(DS.Tables[0].Rows[0]["Qty"].ToString()).ToString("N3"));
           list.Add(k);

           if (Pid > 0)
           {
               DataSet DS2 = new DataSet();
               string cmdStr2 = this.select("PId", "tblDG_TPL_Master", "WONo='" + WONo + "' AND CId='" + Pid + "'");
               SqlCommand cmd2 = new SqlCommand(cmdStr2, con);
               SqlDataAdapter DA2 = new SqlDataAdapter(cmd2);
               DA2.Fill(DS2, "tblDG_TPL_Master");
               int Rpid = Convert.ToInt32(DS2.Tables[0].Rows[0][0]);
               this.TreeQty(WONo, Rpid, Pid);
           }
       }
       catch (Exception ex)
       {

       }
       return list;
   }
    
    // Calculate BOM Tree Qty
   double z = 1;
   List<double> list1 = new List<double>();

   public List<double> BOMTreeQty(string WONo, int Pid, int Cid)
   {
       string connStr = this.Connection();
       SqlConnection con = new SqlConnection(connStr);
       try
       {
           DataSet DS = new DataSet();
           string cmdStr = this.select("Qty", "tblDG_BOM_Master", "WONo='" + WONo + "' AND PId='" + Pid + "'And CId='" + Cid + "'");
           SqlCommand cmd = new SqlCommand(cmdStr, con);
           SqlDataAdapter DA = new SqlDataAdapter(cmd);
           DA.Fill(DS, "tblDG_BOM_Master");
           z = Convert.ToDouble(decimal.Parse(DS.Tables[0].Rows[0]["Qty"].ToString()).ToString("N3"));
           list1.Add(z);

           if (Pid > 0)
           {
               DataSet DS2 = new DataSet();
               
               string cmdStr2 = this.select("PId", "tblDG_BOM_Master", "WONo='" + WONo + "' AND CId='" + Pid + "'");
               SqlCommand cmd2 = new SqlCommand(cmdStr2, con);
               SqlDataAdapter DA2 = new SqlDataAdapter(cmd2);
               DA2.Fill(DS2, "tblDG_BOM_Master");

               int Rpid = Convert.ToInt32(DS2.Tables[0].Rows[0][0]);
               this.BOMTreeQty(WONo, Rpid, Pid);
           }
       }
       catch (Exception ex)
       {

       }
       return list1;
   }
     
   public void fillGrid(string sql, GridView grid)
   {
       string connStr = this.Connection();
       SqlConnection con = new SqlConnection(connStr);
       try
       {
           SqlCommand cmdgrid = new SqlCommand(sql, con);
           SqlDataAdapter dagrid = new SqlDataAdapter(cmdgrid);
           DataSet dsgrid = new DataSet();
           dagrid.Fill(dsgrid);
           grid.DataSource = dsgrid;
           grid.DataBind();
       }
       catch (Exception ch)
       {
       }
       finally
       {

       }
   }

   public string createAssemblyCode(int cid, string pno, int rev,int CompId,int finid)
   {
       string genAsslyCode = "";
       try
       {
           int pro = 0;
           string strConnString = this.Connection();
           SqlConnection con = new SqlConnection(strConnString);
           DataSet DS = new DataSet();
           string cmdstr = "Select Symbol from tblDG_Category_Master where CId='" + cid + "' AND CompId='"+CompId+"'AND FinYearId<='"+finid+"'";
           SqlCommand cmd = new SqlCommand(cmdstr, con);
           SqlDataAdapter da = new SqlDataAdapter(cmd);
           da.Fill(DS, "tblDG_Category_Master");
           string symbol = DS.Tables[0].Rows[0]["Symbol"].ToString();

           if (rev == 0)
           {
               rev = 0;
           }          

           genAsslyCode = string.Concat(symbol, pno, rev,pro);
       }
       catch (Exception es)
       {

       }
       return genAsslyCode;
   }
    
   //To Search Engin
   public void binddropdwn(string sql, GridView GridView2,int Compid)
   {
       string connStr = this.Connection();
       DataSet DS3 = new DataSet();
       SqlConnection con = new SqlConnection(connStr);
       try
       {

           SqlCommand cmd = new SqlCommand(sql, con);
           SqlDataAdapter DA2 = new SqlDataAdapter(cmd);
           DA2.Fill(DS3);
           DataTable dt = new DataTable();
           dt.Columns.Add("Id", typeof(string));
           dt.Columns.Add("ItemCode", typeof(string));
           dt.Columns.Add("ManfDesc", typeof(string));          
           dt.Columns.Add("UOMBasic", typeof(string));          
           dt.Columns.Add("Location", typeof(string));
           dt.Columns.Add("Qty", typeof(string));

          
           DataRow dr;

           for (int k = 0; k < DS3.Tables[0].Rows.Count; k++)
           {

               dr = dt.NewRow();
               dr[0] = DS3.Tables[0].Rows[k]["Id"].ToString();
               dr[1] = this.GetItemCode_PartNo(Compid, Convert.ToInt32(DS3.Tables[0].Rows[k]["Id"].ToString()));
               dr[2] = DS3.Tables[0].Rows[k]["ManfDesc"].ToString();              
               dr[3] = DS3.Tables[0].Rows[k]["UOMBasic"].ToString();
              

               int loc = 0;
               if (DS3.Tables[0].Rows[k]["Location"] != DBNull.Value && DS3.Tables[0].Rows[k]["Location"].ToString() !="")
               {
                   loc = Convert.ToInt32(DS3.Tables[0].Rows[k]["Location"]);
                   string sql2 = "Select LocationLabel+'-'+LocationNo As Loc from tblDG_Location_Master where Id='" + loc + "'";

                   SqlCommand cmd2 = new SqlCommand(sql2, con);
                   SqlDataAdapter DA3 = new SqlDataAdapter(cmd2);
                   DataSet DS4 = new DataSet();
                   DA3.Fill(DS4);

                   if (DS4.Tables[0].Rows.Count > 0)
                   {
                if (DS4.Tables[0].Rows[0]["Loc"] != DBNull.Value && DS4.Tables[0].Rows[0]["Loc"].ToString() != "")
                       {
                           dr[4] = DS4.Tables[0].Rows[0]["Loc"].ToString();
                       }
                       else
                       {
                           dr[4] = "NA";
                       }
                   }
                   else
                   {
                       dr[4] = "NA";
                   }

                   dr[5] =decimal.Parse( DS3.Tables[0].Rows[k]["StockQty"].ToString()).ToString("N3");
               }

               dt.Rows.Add(dr);
               dt.AcceptChanges();
           }
           
           GridView2.DataSource = dt;
           GridView2.DataBind();
       }
       catch (Exception ex)
       {
       }
      finally
       {
           con.Close();
       }

   }

   public int getTPLCId(string wono,int CompId, int FinId)
   {
       string strConnString = this.Connection();
       SqlConnection con = new SqlConnection(strConnString);
       DataSet DS = new DataSet();
       string cmdstr = "Select CId from tblDG_TPL_Master Where WONo='" + wono + "' AND CompId='" + CompId + "' AND FinYearId<='" + FinId + "' Order by CId Desc";
       SqlCommand cmd = new SqlCommand(cmdstr, con);
       SqlDataAdapter da = new SqlDataAdapter(cmd);
       da.Fill(DS, "tblDG_TPL_Master");

       int nextCId = 0;

       if (DS.Tables[0].Rows.Count > 0)
       {
           nextCId = Convert.ToInt32(DS.Tables[0].Rows[0]["CId"]) + 1;
       }
       else
       {
           nextCId = 1;
       }

       return nextCId;
   }

   public int getBOMCId(string wono,int CompId,int FinId)
   {
       string strConnString = this.Connection();
       SqlConnection con = new SqlConnection(strConnString);
       DataSet DS = new DataSet();
       string cmdstr = "Select CId from tblDG_BOM_Master Where WONo='" + wono + "'AND CompId='" + CompId + "' AND FinYearId<='" + FinId + "' Order by CId Desc";
       SqlCommand cmd = new SqlCommand(cmdstr, con);
       SqlDataAdapter da = new SqlDataAdapter(cmd);
       da.Fill(DS, "tblDG_BOM_Master");

       int nextCId = 0;

       if (DS.Tables[0].Rows.Count > 0)
       {
           nextCId = Convert.ToInt32(DS.Tables[0].Rows[0]["CId"]) + 1;
       }
       else
       {
           nextCId = 1;
       }

       return nextCId;
   }

   public string getOpeningDate(int compid, int finid)
   {
       string strConnString = this.Connection();
       SqlConnection con = new SqlConnection(strConnString);
       DataSet DS = new DataSet();
       SqlCommand Cmd = new SqlCommand(this.select("FinYearFrom","tblFinancial_master","CompId='" + compid + "' AND FinYearId='" + finid + "'"), con);
       SqlDataAdapter DA = new SqlDataAdapter(Cmd);
       DA.Fill(DS, "tblFinancial_master");
       string StrDate="";
       if(DS.Tables[0].Rows.Count>0)
       {
           StrDate = this.FromDateDMY(DS.Tables[0].Rows[0]["FinYearFrom"].ToString());
       }
       return StrDate ;
   }

   public Boolean InsertUpdateData(SqlCommand cmd)
   {
       string strConnString = this.Connection();
       SqlConnection con = new SqlConnection(strConnString);
       cmd.CommandType = CommandType.Text;
       cmd.Connection = con;

       try
       {
           con.Open();
           cmd.ExecuteNonQuery();
           return true;
       }

       catch (Exception ex)
       {
           return false;
       }
       finally { con.Close(); }
       
   }

   public DataTable GetData(SqlCommand cmd)
   {

       DataTable dt = new DataTable();

       String strConnString = System.Configuration.ConfigurationManager.ConnectionStrings["LocalSqlServer"].ConnectionString;

       SqlConnection con = new SqlConnection(strConnString);

       SqlDataAdapter sda = new SqlDataAdapter();

       cmd.CommandType = CommandType.Text;

       cmd.Connection = con;

       try
       {

           con.Open();

           sda.SelectCommand = cmd;

           sda.Fill(dt);

           return dt;

       }

       catch
       {

           return null;

       }

   }

   public void getnode(int node, string wonosrc, string wonodest,int compid, string sesid,int finyrid,int destpid,int destcid)
   {
       string connStr = this.Connection();
       SqlConnection con = new SqlConnection(connStr);
       try
       {
           DataSet DS = new DataSet();
           string currDate = this.getCurrDate();
           string currTime = this.getCurrTime();

           int CompId = compid;
           string SessionId = sesid;
           int FinYearId = finyrid;
           int pid = destpid;
           int cid = destcid;

           con.Open();

           string getparent2 = this.select("PId,CId,ItemId,Qty,Weldments,LH,RH", "tblDG_TPL_Master", "CId=" + node + "And WONo='" + wonosrc + "'And CompId=" + CompId + "");
           SqlCommand checkparent2 = new SqlCommand(getparent2, con);
           SqlDataAdapter daparent2 = new SqlDataAdapter(checkparent2);
           DataSet dsparent2 = new DataSet();
           daparent2.Fill(dsparent2, "tblDG_TPL_Master");

           // Insert to TPL

           int ParentCid = this.getTPLCId(wonodest, CompId, FinYearId);
           
               string InsertCidparent = this.insert("tblDG_TPL_Master", "SysDate,SysTime,CompId,FinYearId,SessionId,PId,CId,WONo,ItemId,Qty,Weldments,LH,RH,ConvertToBOM", "'" + currDate.ToString() + "','" + currTime.ToString() + "'," + CompId + "," + FinYearId + ",'" + SessionId.ToString() + "','" + cid + "','" + ParentCid + "','" + wonodest + "','" + Convert.ToInt32(dsparent2.Tables[0].Rows[0]["ItemId"]) + "','" + Convert.ToDouble(decimal.Parse(dsparent2.Tables[0].Rows[0]["Qty"].ToString()).ToString("N3")) + "','" + dsparent2.Tables[0].Rows[0]["Weldments"] + "','" + dsparent2.Tables[0].Rows[0]["LH"] + "','" + dsparent2.Tables[0].Rows[0]["RH"] + "','1'");
               SqlCommand cmdCidparent = new SqlCommand(InsertCidparent, con);
               cmdCidparent.ExecuteNonQuery();  
        
           string getparent = this.select("PId,CId,ItemId,Qty,Weldments,LH,RH", "tblDG_TPL_Master", "PId=" + node + "And WONo='" + wonosrc + "'And CompId=" + CompId + "");
           SqlCommand checkparent = new SqlCommand(getparent, con);
           SqlDataAdapter daparent = new SqlDataAdapter(checkparent);
           DataSet dsparent = new DataSet();
           daparent.Fill(dsparent, "tblDG_TPL_Master");

           for (int h = 0; h < dsparent.Tables[0].Rows.Count; h++)
           {
               // Insert to TPL

               int NextCid = this.getTPLCId(wonodest, CompId, FinYearId);
               string Insertparent = this.insert("tblDG_TPL_Master", "SysDate,SysTime,CompId,FinYearId,SessionId,PId,CId,WONo,ItemId,Qty,Weldments,LH,RH,ConvertToBOM", "'" + currDate.ToString() + "','" + currTime.ToString() + "','" + CompId + "','" + FinYearId + "','" + SessionId.ToString() + "','" + ParentCid + "','" + NextCid + "','" + wonodest + "','" + Convert.ToInt32(dsparent.Tables[0].Rows[h]["ItemId"]) + "','" + Convert.ToDouble(decimal.Parse(dsparent.Tables[0].Rows[h]["Qty"].ToString()).ToString("N3")) + "','" + dsparent.Tables[0].Rows[h]["Weldments"] + "','" + dsparent.Tables[0].Rows[h]["LH"] + "','" + dsparent.Tables[0].Rows[h]["RH"] + "','1'");

               SqlCommand cmdCpyparent = new SqlCommand(Insertparent, con);
               cmdCpyparent.ExecuteNonQuery();               

               ///Updating UpdateWO Field 
               string sqlwo = this.update("SD_Cust_WorkOrder_Master", "UpdateWO='1'", "WONo='" + wonodest + "' And  CompId='" + CompId + "' ");
               SqlCommand cmdwo = new SqlCommand(sqlwo, con);
               cmdwo.ExecuteNonQuery();
               // Get Parent Child 

               DataSet DS2 = new DataSet();
               string cmdStr2 = this.select("PId,CId,ItemId,Qty,Weldments,LH,RH", "tblDG_TPL_Master", "WONo='" + wonosrc + "'And CompId=" + CompId + " AND PId='" + dsparent.Tables[0].Rows[h]["CId"] + "'");
               SqlCommand cmd2 = new SqlCommand(cmdStr2, con);
               SqlDataAdapter da2 = new SqlDataAdapter(cmd2);
               da2.Fill(DS2);

               if (DS2.Tables[0].Rows.Count > 0)
               {
                   for (int j = 0; j < DS2.Tables[0].Rows.Count; j++)
                   {
                       cid = NextCid;
                       this.getnode(Convert.ToInt32(DS2.Tables[0].Rows[j]["CId"]), wonosrc, wonodest, compid, sesid, finyrid, Convert.ToInt32(DS2.Tables[0].Rows[j]["CId"]), cid);
                   }
               }
           }

       }
       catch (Exception x)
       {
       }
       finally
       {
           con.Close();
       }

   }

   public void getBOMnode(int node, string wonosrc, string wonodest, int compid, string sesid, int finyrid, int destpid, int destcid)
   {
       string connStr = this.Connection();
       SqlConnection con = new SqlConnection(connStr);
       try
       {
           DataSet DS = new DataSet();
           string currDate = this.getCurrDate();
           string currTime = this.getCurrTime();

           int CompId = compid;
           string SessionId = sesid;
           int FinYearId = finyrid;
           int pid = destpid;
           int cid = destcid;

           con.Open();

           string getparent2 = this.select("*", "tblDG_BOM_Master", "CId=" + node + "And WONo='" + wonosrc + "'And CompId=" + CompId + " And FinYearId<='"+FinYearId+"'");
           SqlCommand checkparent2 = new SqlCommand(getparent2, con);
           SqlDataAdapter daparent2 = new SqlDataAdapter(checkparent2);
           DataSet dsparent2 = new DataSet();
           daparent2.Fill(dsparent2, "tblDG_BOM_Master");

           int ParentCid = this.getBOMCId(wonodest,CompId,FinYearId);

           // Insert to BOM  
           string InsertCidparentbom = this.insert("tblDG_BOM_Master", "SysDate,SysTime,CompId,FinYearId,SessionId,PId,CId,WONo,ItemId,Qty,EquipmentNo,UnitNo,PartNo", "'" + currDate.ToString() + "','" + currTime.ToString() + "'," + CompId + "," + FinYearId + ",'" + SessionId.ToString() + "','" + cid + "','" + ParentCid + "','" + wonodest + "','" + Convert.ToInt32(dsparent2.Tables[0].Rows[0]["ItemId"]) + "','" + Convert.ToDouble(decimal.Parse(dsparent2.Tables[0].Rows[0]["Qty"].ToString()).ToString("N3")) + "','" + dsparent2.Tables[0].Rows[0]["EquipmentNo"].ToString() + "','" + dsparent2.Tables[0].Rows[0]["UnitNo"].ToString() + "','" + dsparent2.Tables[0].Rows[0]["PartNo"].ToString() + "'");
           SqlCommand cmdCidparentbom = new SqlCommand(InsertCidparentbom, con);
           cmdCidparentbom.ExecuteNonQuery();

           string getparent = this.select("*", "tblDG_BOM_Master", "PId=" + node + "And WONo='" + wonosrc + "'And CompId='" + CompId + "' And FinYearId<='" + FinYearId + "'");
           SqlCommand checkparent = new SqlCommand(getparent, con);
           SqlDataAdapter daparent = new SqlDataAdapter(checkparent);
           DataSet dsparent = new DataSet();
           daparent.Fill(dsparent, "tblDG_BOM_Master");

           for (int h = 0; h < dsparent.Tables[0].Rows.Count; h++)
           {
               int NextCid = this.getBOMCId(wonodest,CompId,FinYearId);

               // Insert to BOM             

               string Insertparentbom = this.insert("tblDG_BOM_Master", "SysDate,SysTime,CompId,FinYearId,SessionId,PId,CId,WONo,ItemId,Qty,EquipmentNo,UnitNo,PartNo", "'" + currDate.ToString() + "','" + currTime.ToString() + "','" + CompId + "','" + FinYearId + "','" + SessionId.ToString() + "','" + ParentCid + "','" + NextCid + "','" + wonodest + "','" + Convert.ToInt32(dsparent.Tables[0].Rows[h]["ItemId"]) + "','" + Convert.ToDouble(decimal.Parse(dsparent.Tables[0].Rows[h]["Qty"].ToString()).ToString("N3")) + "','" + dsparent.Tables[0].Rows[h]["EquipmentNo"].ToString() + "','" + dsparent.Tables[0].Rows[h]["UnitNo"].ToString() + "','" + dsparent.Tables[0].Rows[h]["PartNo"].ToString() + "'");
               SqlCommand cmdCpyparentbom = new SqlCommand(Insertparentbom, con);
               cmdCpyparentbom.ExecuteNonQuery();

               // Get Parent Child 

               DataSet DS2 = new DataSet();
               string cmdStr2 = this.select("*", "tblDG_BOM_Master", "WONo='" + wonosrc + "'And CompId=" + CompId + " AND PId='" + dsparent.Tables[0].Rows[h]["CId"] + "'And FinYearId<='" + FinYearId + "'");
               SqlCommand cmd2 = new SqlCommand(cmdStr2, con);
               SqlDataAdapter da2 = new SqlDataAdapter(cmd2);
               da2.Fill(DS2);

               if (DS2.Tables[0].Rows.Count > 0)
               {
                   for (int j = 0; j < DS2.Tables[0].Rows.Count; j++)
                   {
                       cid = NextCid;
                       this.getBOMnode(Convert.ToInt32(DS2.Tables[0].Rows[j]["CId"]), wonosrc, wonodest, compid, sesid, finyrid,Convert.ToInt32(DS2.Tables[0].Rows[j]["CId"]) ,cid );
                   }
               }
           }

       }
       catch (Exception x)
       {
       }
       finally
       {
           con.Close();
       }

   }
        
   public void getRootNode(int node, string wonosrc, string wonodest, int compid, string sesid, int finyrid, int destpid, int destcid)
   {
       string connStr = this.Connection();
       SqlConnection con = new SqlConnection(connStr);
       try
       {
           DataSet DS = new DataSet();
           string currDate = this.getCurrDate();
           string currTime = this.getCurrTime();
           int CompId = compid;
           string SessionId = sesid;
           int FinYearId = finyrid;
           int pid = destpid;
           int cid = destcid;
           con.Open();

           string getparent2 = this.select("PId,CId,ItemId,Qty,Weldments,LH,RH", "tblDG_TPL_Master", "CId='" + node + "' And WONo='" + wonosrc + "'And CompId=" + CompId + "And FinYearId<='" + FinYearId + "'");
           SqlCommand checkparent2 = new SqlCommand(getparent2, con);
           SqlDataAdapter daparent2 = new SqlDataAdapter(checkparent2);
           DataSet dsparent2 = new DataSet();
           daparent2.Fill(dsparent2, "tblDG_TPL_Master");

           // Insert to TPL

           int ParentCid = this.getTPLCId(wonodest,CompId,FinYearId);

           string InsertCidparent = this.insert("tblDG_TPL_Master", "SysDate,SysTime,CompId,FinYearId,SessionId,PId,CId,WONo,ItemId,Qty,Weldments,LH,RH,ConvertToBOM", "'" + currDate.ToString() + "','" + currTime.ToString() + "'," + CompId + "," + FinYearId + ",'" + SessionId.ToString() + "','" + pid + "','" + ParentCid + "','" + wonodest + "','" + Convert.ToInt32(dsparent2.Tables[0].Rows[0]["ItemId"]) + "','" + Convert.ToDouble(decimal.Parse(dsparent2.Tables[0].Rows[0]["Qty"].ToString()).ToString("N3")) + "','" + dsparent2.Tables[0].Rows[0]["Weldments"] + "','" + dsparent2.Tables[0].Rows[0]["LH"] + "','" + dsparent2.Tables[0].Rows[0]["RH"] + "','1'");

           SqlCommand cmdCidparent = new SqlCommand(InsertCidparent, con);
           cmdCidparent.ExecuteNonQuery(); 
           string getparent = this.select("PId,CId,ItemId,Qty,Weldments,LH,RH", "tblDG_TPL_Master", "PId=" + node + "And WONo='" + wonosrc + "'And CompId=" + CompId + "And FinYearId<='" + FinYearId + "'");
           SqlCommand checkparent = new SqlCommand(getparent, con);
           SqlDataAdapter daparent = new SqlDataAdapter(checkparent);
           DataSet dsparent = new DataSet();
           daparent.Fill(dsparent, "tblDG_TPL_Master");

           for (int h = 0; h < dsparent.Tables[0].Rows.Count; h++)
           {
               // Insert to TPL

               int NextCid = this.getTPLCId(wonodest,CompId,FinYearId);

               string Insertparent = this.insert("tblDG_TPL_Master", "SysDate,SysTime,CompId,FinYearId,SessionId,PId,CId,WONo,ItemId,Qty,Weldments,LH,RH,ConvertToBOM", "'" + currDate.ToString() + "','" + currTime.ToString() + "','" + CompId + "','" + FinYearId + "','" + SessionId.ToString() + "','" + ParentCid + "','" + NextCid + "','" + wonodest + "','" + Convert.ToInt32(dsparent.Tables[0].Rows[h]["ItemId"]) + "','" + Convert.ToDouble(decimal.Parse(dsparent.Tables[0].Rows[h]["Qty"].ToString()).ToString("N3")) + "','" + dsparent.Tables[0].Rows[h]["Weldments"] + "','" + dsparent.Tables[0].Rows[h]["LH"] + "','" + dsparent.Tables[0].Rows[h]["RH"] + "','1'");

               SqlCommand cmdCpyparent = new SqlCommand(Insertparent, con);
               cmdCpyparent.ExecuteNonQuery();
              
               // Get Parent Child 

               DataSet DS2 = new DataSet();
               string cmdStr2 = this.select("PId,CId,ItemId,Qty,Weldments,LH,RH", "tblDG_TPL_Master", "WONo='" + wonosrc + "'And CompId=" + CompId + " AND PId='" + dsparent.Tables[0].Rows[h]["CId"] + "'And FinYearId<='" + FinYearId + "'");

               SqlCommand cmd2 = new SqlCommand(cmdStr2, con);
               SqlDataAdapter da2 = new SqlDataAdapter(cmd2);
               da2.Fill(DS2);

               if (DS2.Tables[0].Rows.Count > 0)
               {
                   for (int j = 0; j < DS2.Tables[0].Rows.Count; j++)
                   {
                       cid = NextCid;
                       this.getRootNode(Convert.ToInt32(DS2.Tables[0].Rows[j]["CId"]), wonosrc, wonodest, compid, sesid, finyrid, cid, Convert.ToInt32(DS2.Tables[0].Rows[j]["CId"]));
                   }
               }
           }

       }
       catch (Exception x)
       {
       }
       finally
       {
           con.Close();
       }
   }

   public void getBOMRootNode(int node, string wonosrc, string wonodest, int CompId, string SessionId, int FinYearId, int destpid, int destcid)
   {
       string connStr = this.Connection();
       SqlConnection con = new SqlConnection(connStr);
       try
       {
           DataSet DS = new DataSet();
           string currDate = this.getCurrDate();
           string currTime = this.getCurrTime();           
           int pid = destpid;
           int cid = destcid;

           con.Open();

           //string getparent2 = this.select("PId,CId,ItemId,Qty", "tblDG_BOM_Master", "CId='" + node + "' And WONo='" + wonosrc + "'And CompId=" + CompId + "And FinYearId<='" + FinYearId + "'");
           string getparent2 = this.select("*", "tblDG_BOM_Master", "CId='" + node + "' And WONo='" + wonosrc + "'And CompId=" + CompId + "And FinYearId<='" + FinYearId + "'");
           SqlCommand checkparent2 = new SqlCommand(getparent2, con);
           SqlDataAdapter daparent2 = new SqlDataAdapter(checkparent2);
           DataSet dsparent2 = new DataSet();
           daparent2.Fill(dsparent2, "tblDG_BOM_Master");
           
           //Insert to BOM

           int ParentCid = this.getBOMCId(wonodest,CompId,FinYearId);

           string InsertCidparentbom = this.insert("tblDG_BOM_Master", "SysDate,SysTime,CompId,FinYearId,SessionId,PId,CId,WONo,ItemId,Qty,EquipmentNo,UnitNo,PartNo", "'" + currDate.ToString() + "','" + currTime.ToString() + "'," + CompId + "," + FinYearId + ",'" + SessionId.ToString() + "','" + pid + "','" + ParentCid + "','" + wonodest + "','" + Convert.ToInt32(dsparent2.Tables[0].Rows[0]["ItemId"]) + "','" + Convert.ToDouble(decimal.Parse(dsparent2.Tables[0].Rows[0]["Qty"].ToString()).ToString("N3")) + "','" + dsparent2.Tables[0].Rows[0]["EquipmentNo"].ToString() + "','" + dsparent2.Tables[0].Rows[0]["UnitNo"].ToString() + "','" + dsparent2.Tables[0].Rows[0]["PartNo"].ToString() + "'");

           SqlCommand cmdCidparentbom = new SqlCommand(InsertCidparentbom, con);
           cmdCidparentbom.ExecuteNonQuery();

           //string getparent = this.select("PId,CId,ItemId,Qty", "tblDG_BOM_Master", "PId=" + node + "And WONo='" + wonosrc + "'And CompId=" + CompId + "And FinYearId<='" + FinYearId + "'");

           string getparent = this.select("*", "tblDG_BOM_Master", "PId=" + node + "And WONo='" + wonosrc + "'And CompId=" + CompId + "And FinYearId<='" + FinYearId + "'");
           SqlCommand checkparent = new SqlCommand(getparent, con);
           SqlDataAdapter daparent = new SqlDataAdapter(checkparent);
           DataSet dsparent = new DataSet();
           daparent.Fill(dsparent, "tblDG_BOM_Master");

           for (int h = 0; h < dsparent.Tables[0].Rows.Count; h++)
           {
               int NextCid = this.getBOMCId(wonodest,CompId,FinYearId);

               // Insert to BOM

               string Insertparentbom = this.insert("tblDG_BOM_Master", "SysDate,SysTime,CompId,FinYearId,SessionId,PId,CId,WONo,ItemId,Qty,EquipmentNo,UnitNo,PartNo", "'" + currDate.ToString() + "','" + currTime.ToString() + "','" + CompId + "','" + FinYearId + "','" + SessionId.ToString() + "','" + ParentCid + "','" + NextCid + "','" + wonodest + "','" + Convert.ToInt32(dsparent.Tables[0].Rows[h]["ItemId"]) + "','" + Convert.ToDouble(decimal.Parse(dsparent.Tables[0].Rows[h]["Qty"].ToString()).ToString("N3")) + "','" + dsparent.Tables[0].Rows[h]["EquipmentNo"].ToString() + "','" + dsparent.Tables[0].Rows[h]["UnitNo"].ToString() + "','" + dsparent.Tables[0].Rows[h]["PartNo"].ToString() + "'");
               SqlCommand cmdCpyparentbom = new SqlCommand(Insertparentbom, con);
               cmdCpyparentbom.ExecuteNonQuery();

               // Get Parent Child 

               DataSet DS2 = new DataSet();
              // string cmdStr2 = this.select("PId,CId,ItemId,Qty", "tblDG_BOM_Master", "WONo='" + wonosrc + "'And CompId=" + CompId + " AND PId='" + dsparent.Tables[0].Rows[h]["CId"] + "'And FinYearId<='" + FinYearId + "'");

               string cmdStr2 = this.select("*", "tblDG_BOM_Master", "WONo='" + wonosrc + "'And CompId=" + CompId + " AND PId='" + dsparent.Tables[0].Rows[h]["CId"] + "'And FinYearId<='" + FinYearId + "'");
               SqlCommand cmd2 = new SqlCommand(cmdStr2, con);
               SqlDataAdapter da2 = new SqlDataAdapter(cmd2);
               da2.Fill(DS2);

               if (DS2.Tables[0].Rows.Count > 0)
               {
                   for (int j = 0; j < DS2.Tables[0].Rows.Count; j++)
                   {
                       cid = NextCid;

                       this.getBOMRootNode(Convert.ToInt32(DS2.Tables[0].Rows[j]["CId"]), wonosrc, wonodest, CompId, SessionId, FinYearId, cid,Convert.ToInt32(DS2.Tables[0].Rows[j]["CId"]));
                   }
               }
           }

       }
      catch (Exception x)
       {
       }
      finally
       {
           con.Close();
       }
   }

   public void getTPLRootNode(int node, string wonosrc, string wonodest, int CompId, string SessionId, int FinYearId, int destpid, int destcid)
   {
       string connStr = this.Connection();
       SqlConnection con = new SqlConnection(connStr);
       try
       {
           DataSet DS = new DataSet();
           string currDate = this.getCurrDate();
           string currTime = this.getCurrTime();
           int pid = destpid;
           int cid = destcid;
           con.Open();

           string getparent2 = this.select("PId,CId,ItemId,Qty,Weldments,LH,RH", "tblDG_TPL_Master", "CId='" + node + "' And WONo='" + wonosrc + "'And CompId=" + CompId + "And FinYearId<='" + FinYearId + "'");
           SqlCommand checkparent2 = new SqlCommand(getparent2, con);
           SqlDataAdapter daparent2 = new SqlDataAdapter(checkparent2);
           DataSet dsparent2 = new DataSet();
           daparent2.Fill(dsparent2, "tblDG_TPL_Master");

           //Insert to BOM

           int ParentCid = this.getBOMCId(wonodest, CompId, FinYearId);

           string InsertCidparentbom = this.insert("tblDG_BOM_Master", "SysDate,SysTime,CompId,FinYearId,SessionId,PId,CId,WONo,ItemId,Qty,Weldments,LH,RH", "'" + currDate.ToString() + "','" + currTime.ToString() + "'," + CompId + "," + FinYearId + ",'" + SessionId.ToString() + "','" + pid + "','" + ParentCid + "','" + wonodest + "','" + Convert.ToInt32(dsparent2.Tables[0].Rows[0]["ItemId"]) + "','" + Convert.ToDouble(decimal.Parse(dsparent2.Tables[0].Rows[0]["Qty"].ToString()).ToString("N3")) + "','" + dsparent2.Tables[0].Rows[0]["Weldments"] + "','" + dsparent2.Tables[0].Rows[0]["LH"] + "','" + dsparent2.Tables[0].Rows[0]["RH"] + "'");

           SqlCommand cmdCidparentbom = new SqlCommand(InsertCidparentbom, con);
           cmdCidparentbom.ExecuteNonQuery();

           string getparent = this.select("PId,CId,ItemId,Qty,Weldments,LH,RH", "tblDG_TPL_Master", "PId=" + node + "And WONo='" + wonosrc + "'And CompId=" + CompId + "And FinYearId<='" + FinYearId + "'");
           SqlCommand checkparent = new SqlCommand(getparent, con);
           SqlDataAdapter daparent = new SqlDataAdapter(checkparent);
           DataSet dsparent = new DataSet();
           daparent.Fill(dsparent, "tblDG_TPL_Master");

           for (int h = 0; h < dsparent.Tables[0].Rows.Count; h++)
           {
               int NextCid = this.getBOMCId(wonodest, CompId, FinYearId);

               // Insert to BOM

               string Insertparentbom = this.insert("tblDG_BOM_Master", "SysDate,SysTime,CompId,FinYearId,SessionId,PId,CId,WONo,ItemId,Qty,Weldments,LH,RH", "'" + currDate.ToString() + "','" + currTime.ToString() + "','" + CompId + "','" + FinYearId + "','" + SessionId.ToString() + "','" + ParentCid + "','" + NextCid + "','" + wonodest + "','" + Convert.ToInt32(dsparent.Tables[0].Rows[h]["ItemId"]) + "','" + Convert.ToDouble(decimal.Parse(dsparent.Tables[0].Rows[h]["Qty"].ToString()).ToString("N3")) + "','" + dsparent.Tables[0].Rows[h]["Weldments"] + "','" + dsparent.Tables[0].Rows[h]["LH"] + "','" + dsparent.Tables[0].Rows[h]["RH"] + "'");
               SqlCommand cmdCpyparentbom = new SqlCommand(Insertparentbom, con);
               cmdCpyparentbom.ExecuteNonQuery();

               // Get Parent Child 

               DataSet DS2 = new DataSet();
               string cmdStr2 = this.select("PId,CId,ItemId,Qty,Weldments,LH,RH", "tblDG_TPL_Master", "WONo='" + wonosrc + "'And CompId=" + CompId + " AND PId='" + dsparent.Tables[0].Rows[h]["CId"] + "'And FinYearId<='" + FinYearId + "'");
               SqlCommand cmd2 = new SqlCommand(cmdStr2, con);
               SqlDataAdapter da2 = new SqlDataAdapter(cmd2);
               da2.Fill(DS2);

               if (DS2.Tables[0].Rows.Count > 0)
               {
                   for (int j = 0; j < DS2.Tables[0].Rows.Count; j++)
                   {
                       cid = NextCid;

                       this.getTPLRootNode(Convert.ToInt32(DS2.Tables[0].Rows[j]["CId"]), wonosrc, wonodest, CompId, SessionId, FinYearId, cid, Convert.ToInt32(DS2.Tables[0].Rows[j]["CId"]));

                   }
               }
           }

       }
       catch (Exception x)
       {
       }
       finally
       {
           con.Close();
       }
   }    
  
   List<int> BomAssmbly = new List<int>();
   public List<int> getBOMDelNode(int node, string wono, int CompId, string SessionId, int Id,string tblName)
   {
       string connStr = this.Connection();
       SqlConnection con = new SqlConnection(connStr);
       int ItemId = 0;
        try
       {
           DataSet DS = new DataSet();
           con.Open();
           string getparent = this.select("Id,PId,CId,ItemId,Qty", ""+tblName+"", "CId=" + node + "And WONo='" + wono + "'And CompId=" + CompId + " And Id='" + Id + "'");
           SqlCommand checkparent = new SqlCommand(getparent, con);
           SqlDataAdapter daparent = new SqlDataAdapter(checkparent);
           DataSet dsparent = new DataSet();
           daparent.Fill(dsparent);
           for (int h = 0; h < dsparent.Tables[0].Rows.Count; h++)
           {

               ItemId = Convert.ToInt32(dsparent.Tables[0].Rows[h]["Id"]);
               string cmdStr2 = this.select("Id,PId,CId,ItemId,Qty", "" + tblName + "", "WONo='" + wono + "'And CompId=" + CompId + " AND PId='" + dsparent.Tables[0].Rows[h]["CId"] + "'");
               SqlCommand cmd2 = new SqlCommand(cmdStr2, con);
               SqlDataAdapter da2 = new SqlDataAdapter(cmd2);
               DataSet DS2 = new DataSet();
               da2.Fill(DS2);
               for (int j = 0; j < DS2.Tables[0].Rows.Count; j++)
               {
                   this.getBOMDelNode(Convert.ToInt32(DS2.Tables[0].Rows[j]["CId"]), wono, CompId, SessionId, Convert.ToInt32(DS2.Tables[0].Rows[j]["Id"]),tblName);
               }
               BomAssmbly.Add(Convert.ToInt32(ItemId));
           }

       }
       catch (Exception x)
       {
       }
       finally
       {
           con.Close();
       }

       return BomAssmbly;
   }

   public List<string> MonthRange(string StartDate, string EndDate)
   {
       //Create List object where this fun will call and assign it.
       //Use List as array.

       List<string> months = new List<string>();

       try
       {
           string[] s = StartDate.Split('-');
           string[] e = EndDate.Split('-');

           DateTime d1 = new DateTime(Convert.ToInt32(s[0]), Convert.ToInt32(s[1]), Convert.ToInt32(s[2]));
           DateTime d2 = new DateTime(Convert.ToInt32(e[0]), Convert.ToInt32(e[1]), Convert.ToInt32(e[2]));

           if (d2 >= d1)
           {
               int[] mth = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
               TimeSpan tp = d2 - d1;
               int totmth = d2.Month - d1.Month + (12 * (d2.Year - d1.Year)) - ((d2.Day < d1.Day) ? 1 : 0);
               int j = 0;

               for (int i = 0; i < totmth + 1; i++)
               {
                   int x = d1.Month + i;

                   if (x <= 12)
                   {
                       months.Add(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(mth[x]));
                   }
                   else
                   {
                       j = j + 1;
                       months.Add(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(mth[j]));
                   }
               }
           }
       }
       catch (Exception x)
       {

       }

       return months;
   }
        
   // for mobile bills 

   public void BindMobBill(string tblName, string tblfield, string whr, GridView SearchGridView, string drpvalue, string hfSearchTextValue)
   {
        try
       {
           //hfSearchText has the search string returned from the grid.
           string strConnString = this.Connection();
           SqlConnection con = new SqlConnection(strConnString);
           DataSet DS = new DataSet();

           string sql = this.select(tblfield, tblName, whr);

           SqlCommand cmd = new SqlCommand(sql, con);
           SqlDataAdapter da = new SqlDataAdapter(cmd);
           da.Fill(DS, tblName);

           if (hfSearchTextValue != "")
           {
               DS.Clear();             
               sql += " AND " + drpvalue + " like '%" + hfSearchTextValue + "%'";
               SqlCommand cmd1 = new SqlCommand(sql, con);
               SqlDataAdapter da1 = new SqlDataAdapter(cmd1);
               da1.Fill(DS, tblName);
           }
           SearchGridView.DataSource = DS;
           SearchGridView.DataBind();
          
       }
       catch (Exception exp)
       {
          
       }
       
   }
   public void AcHead(DropDownList DropDownList1, int y)
   {

       string connStr = this.Connection();
       SqlConnection con = new SqlConnection(connStr);
       try
       {
           string x = "";
           if (y == 1)
           {
               x = "Labour";
           }

           if (y == 2)
           {
               x = "With Material";
           }
           if (y == 3)
           {
               x = "Expenses";
           }
           if (y == 4)
           {
               x = "Service Provider";
           }

           string cmdStrLabour = "SELECT '['+Symbol+'] '+Description AS Head,Id FROM AccHead WHERE Category='" + x + "'";
           SqlCommand cmdLabour = new SqlCommand(cmdStrLabour, con);
           SqlDataAdapter DALabour = new SqlDataAdapter(cmdLabour);
           DataSet DSLabour = new DataSet();
           DALabour.Fill(DSLabour, "AccHead");
           DropDownList1.DataSource = DSLabour;
           DropDownList1.DataTextField = "Head";
           DropDownList1.DataValueField = "Id";

           DropDownList1.DataBind();
       }
       catch (Exception ex) { }
   }

   public void AcHead(DropDownList DropDownList1, RadioButton RbtnLabour, RadioButton RbtnWithMaterial, RadioButton RbtnExpenses, RadioButton RbtnSerProvider)
   {

       string connStr = this.Connection();
       SqlConnection con = new SqlConnection(connStr);
       try
       {
           string x = "";
           if (RbtnLabour.Checked == true)
           {
               x = "Labour";
           }

           if (RbtnWithMaterial.Checked == true)
           {
               x = "With Material";
           }
           if (RbtnExpenses.Checked == true)
           {
               x = "Expenses";
           }
           if (RbtnSerProvider.Checked == true)
           {
               x = "Service Provider";
           }

           string cmdStrLabour = "SELECT '['+Symbol+'] '+Description AS Head,Id FROM AccHead WHERE Category='" + x + "'";
           SqlCommand cmdLabour = new SqlCommand(cmdStrLabour, con);
           SqlDataAdapter DALabour = new SqlDataAdapter(cmdLabour);
           DataSet DSLabour = new DataSet();
           DALabour.Fill(DSLabour, "AccHead");

           DropDownList1.DataSource = DSLabour;
           DropDownList1.DataTextField = "Head";
           DropDownList1.DataValueField = "Id";
           DropDownList1.DataBind();
       }
       catch (Exception ex) { }
   }




   //public void AcHead(DropDownList DropDownList1, int y)
   //{

   //    string connStr = this.Connection();
   //    SqlConnection con = new SqlConnection(connStr);
      
   //    try
   //    {
   //        string x = "";
   //        if (y == 1)
   //        {
   //            x = "Labour";
   //        }

   //        if (y == 2)
   //        {
   //            x = "With Material";
   //        }

   //        string cmdStrLabour = "SELECT '['+Symbol+'] '+Description AS Head,Id FROM AccHead WHERE Category='" + x + "'";
   //        SqlCommand cmdLabour = new SqlCommand(cmdStrLabour, con);
   //        SqlDataAdapter DALabour = new SqlDataAdapter(cmdLabour);
   //        DataSet DSLabour = new DataSet();
   //        DALabour.Fill(DSLabour, "AccHead");
   //        DropDownList1.DataSource = DSLabour;
   //        DropDownList1.DataTextField = "Head";
   //        DropDownList1.DataValueField = "Id";

   //        DropDownList1.DataBind();
   //    }
       
   //    catch (Exception ex) { }
   //}

   //public void AcHead(DropDownList DropDownList1, RadioButton RbtnLabour, RadioButton RbtnWithMaterial)
   //{

   //    string connStr = this.Connection();
   //    SqlConnection con = new SqlConnection(connStr);
   //    try
   //    {
   //        string x = "";
   //        if (RbtnLabour.Checked == true)
   //        {
   //            x = "Labour";
   //        }

   //        if (RbtnWithMaterial.Checked == true)
   //        {
   //            x = "With Material";
   //        }

   //        string cmdStrLabour = "SELECT '['+Symbol+'] '+Description AS Head,Id FROM AccHead WHERE Category='" + x + "'";
   //        SqlCommand cmdLabour = new SqlCommand(cmdStrLabour, con);
   //        SqlDataAdapter DALabour = new SqlDataAdapter(cmdLabour);
   //        DataSet DSLabour = new DataSet();
   //        DALabour.Fill(DSLabour, "AccHead");

   //        DropDownList1.DataSource = DSLabour;
   //        DropDownList1.DataTextField = "Head";
   //        DropDownList1.DataValueField = "Id";
   //        DropDownList1.DataBind();
   //    }
   //    catch (Exception ex) { }
   //}

   public int chkSupplierCode(string code)
   {
       string connStr = this.Connection();
       SqlConnection con = new SqlConnection(connStr);
       DataSet DS = new DataSet();

       try
       {
           SqlCommand cmd = new SqlCommand(this.select("*", "tblMM_Supplier_master", "SupplierId='" + code + "'"), con);
           SqlDataAdapter DA = new SqlDataAdapter(cmd);
           DA.Fill(DS, "tblMM_Supplier_master");
       }
       catch (Exception es)
       {

       }

           if (DS.Tables[0].Rows.Count > 0)
           {
               return 1;
           }
           else
           {
               return 0;
           }
   }

   public int chkCustomerCode(string code)
   {
       string connStr = this.Connection();
       SqlConnection con = new SqlConnection(connStr);
       DataSet DS = new DataSet();

       try
       {
           SqlCommand cmd = new SqlCommand(this.select("*", "SD_Cust_master", "CustomerId='" + code + "'"), con);
           SqlDataAdapter DA = new SqlDataAdapter(cmd);
           DA.Fill(DS, "SD_Cust_master");
       }
       catch (Exception es)
       {

       }

       if (DS.Tables[0].Rows.Count > 0)
       {
           return 1;
       }
       else
       {
           return 0;
       }
   }
   public int chkEmpCustSupplierCode(string code, int codetype, int CompId)
   {
       string connStr = this.Connection();
       SqlConnection con = new SqlConnection(connStr);
       DataSet DS = new DataSet();
       try
       {
           switch (codetype)
           {
               case 1:
                   {
                       string cmdStr = this.select("EmpId,EmployeeName", "tblHR_OfficeStaff", "CompId='" + CompId + "' AND EmpId='" + code + "' ");
                       SqlDataAdapter DA = new SqlDataAdapter(cmdStr, con);
                       DA.Fill(DS, "tblHR_OfficeStaff");
                   }
                   break;

               case 2:
                   {
                       string cmdStr = this.select("CustomerId,CustomerName", "SD_Cust_master", "CompId='" + CompId + "' AND CustomerId='" + code + "'");
                       SqlDataAdapter DA = new SqlDataAdapter(cmdStr, con);
                       DA.Fill(DS, "SD_Cust_master");
                   }
                   break;

               case 3:
                   {
                       string cmdStr = this.select("SupplierId,SupplierName", "tblMM_Supplier_master", "CompId='" + CompId + "' AND SupplierId='" + code + "'");
                       SqlDataAdapter DA = new SqlDataAdapter(cmdStr, con);
                       DA.Fill(DS, "tblMM_Supplier_master");
                   }
                   break;

           }

       }
       catch (Exception ex)
       { }
       if (code != "" && codetype != 0)
       {
           if (DS.Tables[0].Rows.Count > 0)
           {
               return 1;
           }
           else
           {
               return 0;
           }
       }
       else
       {
           return 0;
       }

   }


   //public string EmpCustSupplierNames(int ct, string code, int CompId)
   //{
   //    string connStr = this.Connection();
   //    SqlConnection con = new SqlConnection(connStr);
   //    DataSet DS = new DataSet();
   //    string name = "";
   //    switch (ct)
   //    {
   //        case 1:
   //            {
   //                string cmdStr = this.select("EmployeeName+'[ '+EmpId+' ]' AS EmployeeName ", "tblHR_OfficeStaff", "CompId='" + CompId + "' AND EmpId='" + code + "' ");
   //                SqlDataAdapter DA = new SqlDataAdapter(cmdStr, con);
   //                DA.Fill(DS, "tblHR_OfficeStaff");
   //                name = DS.Tables[0].Rows[0]["EmployeeName"].ToString();
   //            }
   //            break;

   //        case 2:
   //            {
   //                string cmdStr = this.select("CustomerName+'[ '+CustomerId+' ]' AS CustomerName", "SD_Cust_master", "CompId='" + CompId + "' AND CustomerId='" + code + "'");
   //                SqlDataAdapter DA = new SqlDataAdapter(cmdStr, con);
   //                DA.Fill(DS, "SD_Cust_master");
   //                name = DS.Tables[0].Rows[0]["CustomerName"].ToString();
   //            }
   //            break;

   //        case 3:
   //            {
   //                string cmdStr = this.select("SupplierName+'[ '+SupplierId+' ]' AS SupplierName", "tblMM_Supplier_master", "CompId='" + CompId + "' AND SupplierId='" + code + "'");
   //                SqlDataAdapter DA = new SqlDataAdapter(cmdStr, con);
   //                DA.Fill(DS, "tblMM_Supplier_master");
   //                name = DS.Tables[0].Rows[0]["SupplierName"].ToString();
   //            }
   //            break;
   //    }
   //    return name;
   //}



   public int chkItemId(int itid)
   {
       string connStr = this.Connection();
       SqlConnection con = new SqlConnection(connStr);
       DataSet DS = new DataSet();

       try
       {
           SqlCommand cmd = new SqlCommand(this.select("*", "tblMM_SPR_Temp", "ItemId='" + itid + "'"), con);
           SqlDataAdapter DA = new SqlDataAdapter(cmd);
           DA.Fill(DS, "tblMM_SPR_Temp");
       }
       catch (Exception es)
       {

       }

       if (DS.Tables[0].Rows.Count > 0)
       {
           return 1;
       }
       else
       {
           return 0;
       }
   }

   public void MP_Tree(string wono, int CompId, DropDownList ddlCategory, GridView GridView2,int finid)
   {
       string connStr = this.Connection();
       SqlConnection con = new SqlConnection(connStr);
      try
       {

           if (ddlCategory.SelectedItem.Text != "Select Category")
           {
                   List<int> li = new List<int>();
                   li = this.TreeAssembly(wono, CompId);
                   DataTable dt = new DataTable();
                   dt.Columns.Add("AssemblyNo", typeof(string));
                   dt.Columns.Add("ItemCode", typeof(string));
                   dt.Columns.Add("ManfDesc", typeof(string));
                   //dt.Columns.Add("PurchDesc", typeof(string));
                   dt.Columns.Add("UOMBasic", typeof(string));
                   dt.Columns.Add("UnitQty", typeof(string));
                   dt.Columns.Add("BOMQty", typeof(string));
                   dt.Columns.Add("ItemId", typeof(int));
                   dt.Columns.Add("PId", typeof(int));
                   dt.Columns.Add("CId", typeof(int));
                   DataRow dr;

                   for (int i = 0; i < li.Count; i++)
                   {
                       DataSet DS3 = new DataSet();

                       if (li.Count > 0)
                       {
                           string sql = this.select("tblDG_Item_Master.Id,tblDG_BOM_Master.PId,tblDG_BOM_Master.CId,tblDG_BOM_Master.ItemId,tblDG_Item_Master.ManfDesc,Unit_Master.Symbol As UOMBasic,tblDG_Item_Master.ItemCode,tblDG_BOM_Master.Qty", " tblDG_BOM_Master,tblDG_Category_Master,tblDG_Item_Master,Unit_Master", "tblDG_Item_Master.CId=tblDG_Category_Master.CId and Unit_Master.Id=tblDG_Item_Master.UOMBasic and tblDG_Item_Master.Id=tblDG_BOM_Master.ItemId and tblDG_BOM_Master.WONo='" + wono + "'and tblDG_Item_Master.CId='" + ddlCategory.SelectedValue + "' and  tblDG_BOM_Master.Id='" + Convert.ToInt32(li[i]) + "' And tblDG_BOM_Master.CompId='" + CompId + "'And tblDG_BOM_Master.FinYearId<='" + finid + "'");
                           SqlCommand cmd = new SqlCommand(sql, con);
                           SqlDataAdapter DA2 = new SqlDataAdapter(cmd);
                           DA2.Fill(DS3);

                           if (DS3.Tables[0].Rows.Count > 0)
                           {
                               string sql_S = this.select("*", "tblMP_Material_Master", "PId='" + Convert.ToInt32(DS3.Tables[0].Rows[0]["PId"]) + "' AND CId='" + Convert.ToInt32(DS3.Tables[0].Rows[0]["CId"]) + "' AND CompId='" + CompId + "' AND WONo='" + wono + "' ");

                               DataSet DS_S = new DataSet();
                               SqlCommand cmd_S = new SqlCommand(sql_S, con);
                               SqlDataAdapter DA_S = new SqlDataAdapter(cmd_S);
                               DA_S.Fill(DS_S);

                               if (DS_S.Tables[0].Rows.Count == 0)
                               {
                                   dr = dt.NewRow();

                                   string sql_P = this.select("tblDG_Item_Master.ItemCode", "tblDG_BOM_Master,tblDG_Item_Master", "tblDG_Item_Master.Id=tblDG_BOM_Master.ItemId AND tblDG_BOM_Master.CId='" + Convert.ToInt32(DS3.Tables[0].Rows[0]["PId"]) + "' AND tblDG_Item_Master.CompId='" + CompId + "'");
                                   DataSet DS_P = new DataSet();
                                   SqlCommand cmd_P = new SqlCommand(sql_P, con);
                                   SqlDataAdapter DA_P = new SqlDataAdapter(cmd_P);
                                   DA_P.Fill(DS_P);

                                   if (DS_P.Tables[0].Rows.Count > 0)
                                   {
                                       dr[0] = DS_P.Tables[0].Rows[0]["ItemCode"].ToString();
                                   }
                                   else
                                   {
                                       dr[0] = "";
                                   }

                                   dr[1] = DS3.Tables[0].Rows[0]["ItemCode"].ToString();
                                   dr[2] = DS3.Tables[0].Rows[0]["ManfDesc"].ToString();
                                   //dr[3] = DS3.Tables[0].Rows[0]["PurchDesc"].ToString();
                                   dr[3] = DS3.Tables[0].Rows[0]["UOMBasic"].ToString();
                                   dr[4] = DS3.Tables[0].Rows[0]["Qty"].ToString();
                                   double liQty = Convert.ToDouble(decimal.Parse(this.BOMRecurQty(wono, Convert.ToInt32(DS3.Tables[0].Rows[0]["PId"]), Convert.ToInt32(DS3.Tables[0].Rows[0]["CId"]), 1,CompId,finid).ToString()).ToString("N3"));
                                   dr[5] = liQty;
                                   dr[6] = DS3.Tables[0].Rows[0]["ItemId"].ToString();
                                   dr[7] = DS3.Tables[0].Rows[0]["PId"].ToString();
                                   dr[8] = DS3.Tables[0].Rows[0]["CId"].ToString();

                                   dt.Rows.Add(dr);
                                   dt.AcceptChanges();
                               }
                           }
                       }
                   }

                   GridView2.DataSource = dt;
                   GridView2.DataBind();
               
           }
       }
       catch (Exception ch)
       {
       }
       finally
       {
           con.Close();
       }

   }
    
   public double ComponentBOMQty(int CompId, string wono, string itemid,int finId)
   {
       string connStr = this.Connection();
       SqlConnection con = new SqlConnection(connStr);

       con.Open();

       string sql = this.select("tblDG_BOM_Master.PId,tblDG_BOM_Master.CId,tblDG_BOM_Master.ItemId,tblDG_Item_Master.Id,tblDG_Item_Master.ManfDesc,tblDG_Item_Master.PurchDesc,Unit_Master.Symbol As UOMBasic,tblDG_Item_Master.ItemCode,tblDG_BOM_Master.Qty as UnitQty", "tblDG_BOM_Master,tblDG_Category_Master,tblDG_Item_Master,Unit_Master", "tblDG_Item_Master.CId=tblDG_Category_Master.CId and Unit_Master.Id=tblDG_Item_Master.UOMBasic and tblDG_Item_Master.Id=tblDG_BOM_Master.ItemId and tblDG_BOM_Master.WONo='" + wono + "'and tblDG_Item_Master.Id='" + Convert.ToInt32(itemid) + "' And tblDG_Item_Master.CompId='" + CompId + "'And tblDG_Item_Master.FinYearId<='" + finId + "'");

       SqlCommand cmd2 = new SqlCommand(sql, con);
       SqlDataAdapter DA4 = new SqlDataAdapter(cmd2);
       DataSet DS4 = new DataSet();
       DA4.Fill(DS4);
       double tqty = 0;

       if (DS4.Tables[0].Rows.Count > 0)
       {
           for (int g = 0; g < DS4.Tables[0].Rows.Count; g++)
           {
               // Child: Check in Material Planning Master
               string sql_S = this.select("*", "tblMP_Material_Master", "PId='" + Convert.ToInt32(DS4.Tables[0].Rows[g]["PId"]) + "' AND CId='" + Convert.ToInt32(DS4.Tables[0].Rows[g]["CId"]) + "' AND CompId='" + CompId + "'and WONo='" + wono + "'AND FinYearId<='" + finId + "' AND ItemId='" + Convert.ToInt32(itemid) + "'");
               DataSet DS_S = new DataSet();
               SqlCommand cmd_S = new SqlCommand(sql_S, con);
               SqlDataAdapter DA_S = new SqlDataAdapter(cmd_S);
               DA_S.Fill(DS_S);

               // Check in Raw Material Planning Master
               string sqlrm = this.select("tblMP_Material_RawMaterial.ItemId", "tblMP_Material_Master,tblMP_Material_RawMaterial", "tblMP_Material_RawMaterial.PId='" + Convert.ToInt32(DS4.Tables[0].Rows[g]["PId"]) + "' AND tblMP_Material_RawMaterial.CId='" + Convert.ToInt32(DS4.Tables[0].Rows[g]["CId"]) + "' AND tblMP_Material_RawMaterial.ItemId='" + Convert.ToInt32(itemid) + "' AND tblMP_Material_Master.CompId='" + CompId + "' And FinYearId<='" + finId + "' AND tblMP_Material_RawMaterial.PLNo=tblMP_Material_Master.PLNo AND tblMP_Material_Master.WONo='" + wono + "'");

               SqlCommand cmdrm = new SqlCommand(sqlrm, con);
               SqlDataAdapter DArm = new SqlDataAdapter(cmdrm);
               DataSet DSrm = new DataSet();
               DArm.Fill(DSrm);

               // Parent: Check Parent in Planning Master

               string sql_P = this.select("CId", "tblMP_Material_Master", "CId='" + Convert.ToInt32(DS4.Tables[0].Rows[g]["PId"]) + "' AND CompId='" + CompId + "'and WONo='" + wono + "'And FinYearId<='" + finId + "'");

               DataSet DS_P = new DataSet();
               SqlCommand cmd_P = new SqlCommand(sql_P, con);
               SqlDataAdapter DA_P = new SqlDataAdapter(cmd_P);
               DA_P.Fill(DS_P);

               // Parent: Check in Raw Material Planning Master
               string sqlPrm = this.select("tblMP_Material_RawMaterial.CId", "tblMP_Material_Master,tblMP_Material_RawMaterial", "tblMP_Material_RawMaterial.CId='" + Convert.ToInt32(DS4.Tables[0].Rows[g]["PId"]) + "' AND tblMP_Material_Master.CompId='" + CompId + "' AND tblMP_Material_RawMaterial.PLNo=tblMP_Material_Master.PLNo AND tblMP_Material_Master.WONo='" + wono + "'And tblMP_Material_Master.FinYearId<='" + finId + "'");

               SqlCommand cmdPrm = new SqlCommand(sqlPrm, con);
               SqlDataAdapter DAPrm = new SqlDataAdapter(cmdPrm);
               DataSet DSPrm = new DataSet();
               DAPrm.Fill(DSPrm);

               if (DS_S.Tables[0].Rows.Count == 0 && DSrm.Tables[0].Rows.Count == 0 && DS_P.Tables[0].Rows.Count == 0 && DSPrm.Tables[0].Rows.Count == 0)
               {
                   
                   tqty += this.BOMRecurQty(wono, Convert.ToInt32(DS4.Tables[0].Rows[g]["PId"]), Convert.ToInt32(DS4.Tables[0].Rows[g]["CId"]), 1,CompId,finId);
               }
           }

       }

       return Math.Round(tqty,5);
   }

   // To Get Assembly Item

   List<int> Assmbly = new List<int>();
   public List<int> TreeAssembly(string wono, int Compid)
   {
       string connStr = this.Connection();
       SqlConnection con = new SqlConnection(connStr);
       try
       {
           DataSet DS = new DataSet();
           DataSet DS2 = new DataSet();
           string cmdStr = this.select("PId", "tblDG_BOM_Master", "WONo='" + wono + "'And CompId='" + Compid + "'And PId!=0 Group By PId");
           SqlCommand cmd = new SqlCommand(cmdStr, con);
           SqlDataAdapter DA = new SqlDataAdapter(cmd);
           DA.Fill(DS, "tblDG_BOM_Master");
           if (DS.Tables[0].Rows.Count > 0)
           {
               for (int i = 0; i < DS.Tables[0].Rows.Count; i++)
               {
                   string cmdStr2 = this.select("Id", "tblDG_BOM_Master", " WONo='" + wono + "' And CompId='" + Compid + "' And CId='" + Convert.ToInt32(DS.Tables[0].Rows[i]["PId"]) + "'");

                   SqlCommand cmd2 = new SqlCommand(cmdStr2, con);
                   SqlDataAdapter DA2 = new SqlDataAdapter(cmd2);
                   DA2.Fill(DS2, "tblDG_BOM_Master");
                   Assmbly.Add(Convert.ToInt32(DS2.Tables[0].Rows[i]["Id"]));
               }
           }


       }
       catch (Exception ex)
       {

       }
       return Assmbly;
   }

   List<int> AssmblyTPL = new List<int>();
   public List<int> TPLTree(string wono, string pid, string cid, int Compid,int FinId )
   {
       string connStr = this.Connection();
       SqlConnection con = new SqlConnection(connStr);
       try
       {

           DataSet DS = new DataSet();
           DataSet DS2 = new DataSet();
           string cmdStr = this.select("PId", "tblDG_TPL_Master", "WONo='" + wono + "' AND CompId='" + Compid + "'And FinYearId='"+FinId+"'And PId='" + pid + "' Group By PId");
           SqlCommand cmd = new SqlCommand(cmdStr, con);
           SqlDataAdapter DA = new SqlDataAdapter(cmd);
           DA.Fill(DS, "tblDG_TPL_Master");

           if (DS.Tables[0].Rows.Count > 0)
           {
               for (int i = 0; i < DS.Tables[0].Rows.Count; i++)
               {
                   string cmdStr2 = this.select("Id", "tblDG_TPL_Master", "WONo='" + wono + "' AND CompId='" + Compid + "'And FinYearId='" + FinId + "' And CId='" + Convert.ToInt32(pid) + "'");

                   SqlCommand cmd2 = new SqlCommand(cmdStr2, con);
                   SqlDataAdapter DA2 = new SqlDataAdapter(cmd2);
                   DA2.Fill(DS2, "tblDG_TPL_Master");

                   AssmblyTPL.Add(Convert.ToInt32(DS2.Tables[0].Rows[0]["Id"]));

               }
           }
       }
       catch (Exception ex)
       {

       }
       return AssmblyTPL;
   }

   List<int> BOMId = new List<int>();
   public List<int> BOMTree(string wono, string pid, string cid, int Compid,int finId )
   {
       string connStr = this.Connection();
       SqlConnection con = new SqlConnection(connStr);
       try
       {

           DataSet DS = new DataSet();
           DataSet DS2 = new DataSet();
           string cmdStr = this.select("PId", "tblDG_BOM_Master", "WONo='" + wono + "' AND CompId='" + Compid + "'AND tblDG_BOM_Master.FinYearId<='" + finId + "'And PId='" + pid + "' Group By PId");
           SqlCommand cmd = new SqlCommand(cmdStr, con);
           SqlDataAdapter DA = new SqlDataAdapter(cmd);
           DA.Fill(DS, "tblDG_BOM_Master");

           if (DS.Tables[0].Rows.Count > 0)
           {
               for (int i = 0; i < DS.Tables[0].Rows.Count; i++)
               {
                   string cmdStr2 = this.select("Id", "tblDG_BOM_Master", "WONo='" + wono + "' AND CompId='" + Compid + "'AND tblDG_BOM_Master.FinYearId<='" + finId + "'And CId='" + Convert.ToInt32(pid) + "'");

                   SqlCommand cmd2 = new SqlCommand(cmdStr2, con);
                   SqlDataAdapter DA2 = new SqlDataAdapter(cmd2);
                   DA2.Fill(DS2, "tblDG_BOM_Master");
                   if (DS2.Tables[0].Rows.Count>0)
                   {
                     BOMId.Add(Convert.ToInt32(DS2.Tables[0].Rows[0]["Id"]));
                   }

               }
           }
       }
       catch (Exception ex)
       {

       }
       return BOMId;
   }

   // To Get Componant Item for Planning

   List<int> componant = new List<int>();

   public List<int> TreeComponant(string wono, int Compid)
   {

       string connStr = this.Connection();
       SqlConnection con = new SqlConnection(connStr);
       try
       {
           DataSet DS = new DataSet();
           DataSet DS2 = new DataSet();
           string cmdStr = this.select("CId", "tblDG_BOM_Master", "WONo='" + wono + "'And CompId='" + Compid + "'And CId Not In(Select PId from tblDG_BOM_Master where WONo='" + wono + "'And CompId='" + Compid + "'Group By PId)");
           SqlCommand cmd = new SqlCommand(cmdStr, con);
           SqlDataAdapter DA = new SqlDataAdapter(cmd);
           DA.Fill(DS, "tblDG_BOM_Master");
           if (DS.Tables[0].Rows.Count > 0)
           {
               for (int j = 0; j < DS.Tables[0].Rows.Count; j++)
               {

                   string cmdStr2 = this.select("ItemId", "tblDG_BOM_Master", " WONo='" + wono + "' And CompId='" + Compid + "' And CId='" + Convert.ToInt32(DS.Tables[0].Rows[j]["CId"]) + "' Group By ItemId");
                   SqlCommand cmd2 = new SqlCommand(cmdStr2, con);
                   SqlDataAdapter DA2 = new SqlDataAdapter(cmd2);
                   DA2.Fill(DS2, "tblDG_BOM_Master");
                   componant.Add(Convert.ToInt32(DS2.Tables[0].Rows[j]["ItemId"]));
               }
           }


       }
       catch (Exception ex)
       {

       }
       return componant;
   }
    
   public void getcategory(DropDownList dpdlBG)
   {
       try
       {
           DataSet DS = new DataSet();
           string connStr = this.Connection();
           SqlConnection con = new SqlConnection(connStr);
           string cmdStr = this.select1("(CName+' [ '+Symbol+' ]')As name,CId ", "tblDG_Category_Master");           
           SqlCommand cmd = new SqlCommand(cmdStr, con);
           SqlDataAdapter DA = new SqlDataAdapter(cmd);
           DA.Fill(DS, "tblDG_Category_Master");
           dpdlBG.DataSource = DS.Tables["tblDG_Category_Master"];
           dpdlBG.DataTextField = "name";
           dpdlBG.DataValueField = "CId";
           dpdlBG.DataBind();
           dpdlBG.Items.Insert(0, "Select Category");
       }
       catch (Exception ex)
       {

       }
   }
    
   public void ComponentBOM_Consolidated_Items_RM(int CompId, string wono, string plno, string itemid)
   {
       string connStr = this.Connection();
       SqlConnection con = new SqlConnection(connStr);

       con.Open();

       string sql = this.select("tblDG_BOM_Master.PId,tblDG_BOM_Master.CId,tblDG_BOM_Master.ItemId,tblDG_Item_Master.Id,tblDG_Item_Master.ManfDesc,tblDG_Item_Master.PurchDesc,Unit_Master.Symbol As UOMBasic,tblDG_Item_Master.ItemCode,tblDG_BOM_Master.Qty as UnitQty", "tblDG_BOM_Master,tblDG_Category_Master,tblDG_Item_Master,Unit_Master", "tblDG_Item_Master.CId=tblDG_Category_Master.CId and Unit_Master.Id=tblDG_Item_Master.UOMBasic and tblDG_Item_Master.Id=tblDG_BOM_Master.ItemId and tblDG_BOM_Master.WONo='" + wono + "'and tblDG_Item_Master.Id='" + Convert.ToInt32(itemid) + "' And tblDG_BOM_Master.CompId='" + CompId + "'");

       SqlCommand cmd2 = new SqlCommand(sql, con);
       SqlDataAdapter DA4 = new SqlDataAdapter(cmd2);
       DataSet DS4 = new DataSet();
       DA4.Fill(DS4);

       if (DS4.Tables[0].Rows.Count > 0)
       {
           for (int g = 0; g < DS4.Tables[0].Rows.Count; g++)
           {
               // Child: Check in Material Planning Master
               string sql_S = this.select("*", "tblMP_Material_Master", "PId='" + Convert.ToInt32(DS4.Tables[0].Rows[g]["PId"]) + "' AND CId='" + Convert.ToInt32(DS4.Tables[0].Rows[g]["CId"]) + "' AND CompId='" + CompId + "'and WONo='" + wono + "' AND ItemId='" + Convert.ToInt32(itemid) + "'");
               DataSet DS_S = new DataSet();
               SqlCommand cmd_S = new SqlCommand(sql_S, con);
               SqlDataAdapter DA_S = new SqlDataAdapter(cmd_S);
               DA_S.Fill(DS_S);

               // Check in Raw Material Planning Master
               string sqlrm = this.select("tblMP_Material_RawMaterial.ItemId", "tblMP_Material_Master,tblMP_Material_RawMaterial", "tblMP_Material_RawMaterial.PId='" + Convert.ToInt32(DS4.Tables[0].Rows[g]["PId"]) + "' AND tblMP_Material_RawMaterial.CId='" + Convert.ToInt32(DS4.Tables[0].Rows[g]["CId"]) + "' AND tblMP_Material_RawMaterial.ItemId='" + Convert.ToInt32(itemid) + "' AND tblMP_Material_Master.CompId='" + CompId + "' AND tblMP_Material_RawMaterial.PLNo=tblMP_Material_Master.PLNo AND tblMP_Material_Master.WONo='" + wono + "'");

               SqlCommand cmdrm = new SqlCommand(sqlrm, con);
               SqlDataAdapter DArm = new SqlDataAdapter(cmdrm);
               DataSet DSrm = new DataSet();
               DArm.Fill(DSrm);

               // Parent: Check Parent in Planning Master

               string sql_P = this.select("CId", "tblMP_Material_Master", "CId='" + Convert.ToInt32(DS4.Tables[0].Rows[g]["PId"]) + "' AND CompId='" + CompId + "'and WONo='" + wono + "'");

               DataSet DS_P = new DataSet();
               SqlCommand cmd_P = new SqlCommand(sql_P, con);
               SqlDataAdapter DA_P = new SqlDataAdapter(cmd_P);
               DA_P.Fill(DS_P);

               // Parent: Check in Raw Material Planning Master
               string sqlPrm = this.select("tblMP_Material_RawMaterial.CId", "tblMP_Material_Master,tblMP_Material_RawMaterial", "tblMP_Material_RawMaterial.CId='" + Convert.ToInt32(DS4.Tables[0].Rows[g]["PId"]) + "' AND tblMP_Material_Master.CompId='" + CompId + "' AND tblMP_Material_RawMaterial.PLNo=tblMP_Material_Master.PLNo AND tblMP_Material_Master.WONo='" + wono + "'");

               SqlCommand cmdPrm = new SqlCommand(sqlPrm, con);
               SqlDataAdapter DAPrm = new SqlDataAdapter(cmdPrm);
               DataSet DSPrm = new DataSet();
               DAPrm.Fill(DSPrm);

               if (DS_S.Tables[0].Rows.Count == 0 && DSrm.Tables[0].Rows.Count == 0 && DS_P.Tables[0].Rows.Count == 0 && DSPrm.Tables[0].Rows.Count == 0)
               {
                   string StrRM = this.insert("tblMP_Material_RawMaterial", "PLNo,PId,CId,ItemId", "'" + plno + "','" + DS4.Tables[0].Rows[g]["PId"] + "','" + DS4.Tables[0].Rows[g]["CId"] + "','" + DS4.Tables[0].Rows[g]["ItemId"].ToString() + "'");

                   SqlCommand cmdRM = new SqlCommand(StrRM, con);
                   cmdRM.ExecuteNonQuery();
               }
           }

       }
   }

   public string getSupplierName(string SupplierId, int CompId)
   {
       string connStr = this.Connection();
       SqlConnection con = new SqlConnection(connStr);

       string sql = this.select("SupplierName+' ['+SupplierId+']' as Supl", "tblMM_Supplier_master", "CompId='" + CompId + "' AND SupplierId='" + SupplierId + "'");

       SqlCommand cmd = new SqlCommand(sql, con);
       SqlDataAdapter DA = new SqlDataAdapter(cmd);
       DataSet DS = new DataSet();
       DA.Fill(DS);

       if (DS.Tables[0].Rows.Count > 0)
       {
           return DS.Tables[0].Rows[0]["Supl"].ToString();
       }
       else
       {
           return "";
       }
   }

   public double CalBasicAmt(double qty, double rate)
   {
       return Math.Round(qty * rate, 2);
   }

   public double CalDiscAmt(double qty, double rate, double disc)
   {
       return Math.Round((qty * rate) - ((qty * rate) * disc / 100), 2);
   }

   public double CalTaxAmt(double qty, double rate, double disc, double pf, double exser, double vat)
   {
       double discamt = (qty * rate) - ((qty * rate) * disc / 100);
       double PandF = discamt + pf;

       double ExSerTax = 0;
       double CalVat = 0;
       double onlyEx = 0;
       double onlyvat = 0;

       ExSerTax = PandF + ((PandF * exser) / 100);
       onlyEx = ((PandF * exser) / 100);

       CalVat = ExSerTax + ((ExSerTax * vat) / 100);
       onlyvat = ((ExSerTax * vat) / 100);

       return Math.Round(onlyEx + onlyvat, 2);
   }

   public double CalTotAmt(double qty, double rate, double disc, double pf, double exser, double vat)
   {
       double discamt = (qty * rate) - ((qty * rate) * disc / 100);

       double PandF = discamt + pf;

       double ExSerTax = 0;

       if (exser > 0)
       {
           ExSerTax = PandF + ((PandF * exser) / 100);
       }
       else
       {
           ExSerTax = PandF;
       }

       double CalVat = 0;

       if (vat > 0)
       {
           CalVat = ExSerTax + ((ExSerTax * vat) / 100);
       }
       else
       {
           CalVat = ExSerTax;
       }

       return Math.Round(CalVat, 2);
   }

   public void drpDept(DropDownList dpdlunit)
   {
       try
       {


           DataSet DS = new DataSet();
           string connStr = this.Connection();
           SqlConnection con = new SqlConnection(connStr);
           string cmdStr = this.select1("Symbol,Id,Description", "tblHR_Departments");
           SqlCommand cmd = new SqlCommand(cmdStr, con);
           SqlDataAdapter DA = new SqlDataAdapter(cmd);
           DA.Fill(DS, "tblHR_Departments");
           dpdlunit.DataSource = DS.Tables[0];
           dpdlunit.DataTextField = "Description";
           dpdlunit.DataValueField = "Id";
           dpdlunit.DataBind();
           dpdlunit.Items.Insert(0, "Select");


       }
       catch (Exception ex)
       {

       }
   }
        
   public double getBudget(int AccId, int CId)
   {
       string connStr = this.Connection();
       SqlConnection con = new SqlConnection(connStr);
       double Amt = 0;
       try
       {
           con.Open();

           string select = ("Select Sum(Amount) As BalBudget from  tblACC_Budget_Transactions where CompId='" + CId + "'  and BudgetCode='" + AccId + "' group by  BudgetCode ");

           SqlCommand cmd = new SqlCommand(select, con);
           SqlDataAdapter da = new SqlDataAdapter(cmd);
           DataSet ds = new DataSet();
           da.Fill(ds, " tblACC_Budget_Transactions");

           string select5 = ("select Sum(Amount) As Budget from tblACC_Budget_Dept where  CompId='" + CId + "'  and AccId='" + AccId + "'");
           SqlCommand cmd5 = new SqlCommand(select5, con);
           SqlDataAdapter da5 = new SqlDataAdapter(cmd5);
           DataSet ds5 = new DataSet();
           da5.Fill(ds5, "tblACC_Budget_Dept");


           string select2 = ("select Sum(Amount) As Budget from tblACC_Budget_WO where CompId='" + CId + "'  and AccId='" + AccId + "'");
           SqlCommand cmd2 = new SqlCommand(select2, con);
           SqlDataAdapter da2 = new SqlDataAdapter(cmd2);
           DataSet ds2 = new DataSet();
           da2.Fill(ds2, "tblACC_Budget_WO");


           if (ds.Tables[0].Rows.Count > 0)
           {
               double DeptAmt = 0;

               if (ds5.Tables[0].Rows[0]["Budget"] != DBNull.Value)
               {
                   DeptAmt = Convert.ToDouble(decimal.Parse(ds5.Tables[0].Rows[0]["Budget"].ToString()).ToString("N2"));
               }

               double WOAmt = 0;

               if (ds2.Tables[0].Rows[0]["Budget"] != DBNull.Value)
               {
                   WOAmt = Convert.ToDouble(decimal.Parse(ds2.Tables[0].Rows[0]["Budget"].ToString()).ToString("N2"));
               }

               if (DeptAmt > 0 || WOAmt > 0)
               {
                   Amt = Convert.ToDouble(decimal.Parse(ds.Tables[0].Rows[0]["Budget"].ToString()).ToString("N2")) -(DeptAmt + WOAmt);
               }
               else
               {
                   Amt = Convert.ToDouble(decimal.Parse(ds.Tables[0].Rows[0]["Budget"].ToString()).ToString("N2"));
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
       return Amt;
   }
    
   public double getTotalAllowBudget_By_AccId(int AccId, int CId, int Type, string category)
   {
       // To Get Total Budget with Labour/With Material  &  A/c Id Wise.

       string connStr = this.Connection();
       SqlConnection con = new SqlConnection(connStr);
       double Amt = 0;
       string gt = "";

       try
       {

           switch (Type)
           {
               case 1:

                   gt = this.select("Sum(tblACC_Budget_Dept.Amount) AS  Budget", "tblACC_Budget_Dept,AccHead", "tblACC_Budget_Dept.CompId='" + CId + "'  AND  tblACC_Budget_Dept.AccId='" + AccId + "' AND AccHead.Category='" + category + "' AND tblACC_Budget_Dept.AccId=AccHead.Id");
                   break;

               case 2:

                   gt = this.select("Sum(tblACC_Budget_WO.Amount) AS  Budget", "tblACC_Budget_WO,AccHead", "tblACC_Budget_WO.CompId='" + CId + "'  AND tblACC_Budget_WO.AccId='" + AccId + "'  AND  AccHead.Category='" + category + "' AND AND tblACC_Budget_WO.AccId=AccHead.Id");
                   break;

           }

           con.Open();
           SqlCommand cmd = new SqlCommand(gt, con);
           SqlDataAdapter da = new SqlDataAdapter(cmd);
           DataSet ds = new DataSet();
           da.Fill(ds);
           if (ds.Tables[0].Rows.Count > 0 && ds.Tables[0].Rows[0]["Budget"] != DBNull.Value)
           {
               //Amt = Convert.ToDouble(decimal.Parse(ds.Tables[0].Rows[0]["Budget"].ToString()).ToString("N2"));
               Amt = Math.Round(Convert.ToDouble(decimal.Parse(ds.Tables[0].Rows[0]["Budget"].ToString()).ToString("N2")),5);
               
           }
       }

       catch (Exception ex)
       {
       }
       finally
       {
           con.Close();
       }
       return Amt;

   }
    
   public double getAllowBudget_By_AccId(int AccId, int CId, int Type, string getfor)
   {
       // To Get Budget Of Labour/With Material With A/c Id Wise.

       string connStr = this.Connection();
       SqlConnection con = new SqlConnection(connStr);
       double Amt = 0;
       string gt = "";

       try
       {
           switch (Type)
           {
               case 1:


                   gt = this.select("Sum(tblACC_Budget_Dept.Amount) AS  Budget", "tblACC_Budget_Dept,AccHead,tblHR_Departments", "tblACC_Budget_Dept.CompId='" + CId + "'  AND  tblACC_Budget_Dept.AccId='" + AccId + "' AND tblACC_Budget_Dept.DeptId ='" + getfor + "' AND tblACC_Budget_Dept.DeptId=tblHR_Departments.Id AND tblACC_Budget_Dept.AccId=AccHead.Id");

                   break;

               case 2:

                   gt = this.select("Sum(tblACC_Budget_WO.Amount) AS  Budget", "tblACC_Budget_WO,AccHead", "tblACC_Budget_WO.CompId='" + CId + "'  AND tblACC_Budget_WO.AccId='" + AccId + "'  AND tblACC_Budget_WO.WONo  ='" + getfor + "' AND  tblACC_Budget_WO.AccId=AccHead.Id ");
                   
                   break;

           }

           con.Open();
           SqlCommand cmd = new SqlCommand(gt, con);
           SqlDataAdapter da = new SqlDataAdapter(cmd);
           DataSet ds = new DataSet();
           da.Fill(ds);
           if (ds.Tables[0].Rows.Count > 0 && ds.Tables[0].Rows[0]["Budget"] != DBNull.Value)
           {
               //Amt = Convert.ToDouble(decimal.Parse(ds.Tables[0].Rows[0]["Budget"].ToString()).ToString("N2"));
               Amt = Math.Round(Convert.ToDouble(decimal.Parse(ds.Tables[0].Rows[0]["Budget"].ToString()).ToString("N2")),5);
           }
       }

       catch (Exception ex)
       {
       }
       finally
       {
           con.Close();
       }
       return Amt;

   }

   //public double getTotal_PO_Budget_Amt(int compid, int accid, int prspr, int wodept, string wono, int dept, int authnon, int BasicTax)
   //{
   //    /* authnon - PO is Auth or Non Auth 
   //       prspr - 0 for PR & 1 for SPR
   //       wodept - 0 do not include any wo or dept, 1 include wo or dept
   //       wono - wo no 
   //       dept - dept id
   //       accid - A/c Id
   //       BasicTax - 0 Basic Amt & 1 Basic Disc Amt & 2 Only Tax Amt & 3 Basic + Disc + Tax Amt
   //       RtnType - 
   //     */

   //    string includeWODept = "";

   //    string connStr = this.Connection();
   //    SqlConnection con = new SqlConnection(connStr);
   //    con.Open();

   //    double Amt = 0;
   //    try
   //    {
   //        if (prspr == 0)
   //        {
   //            string sqlPO = this.select("tblMM_PO_Details.PRId,tblMM_PO_Details.PRNo,tblMM_PO_Details.Qty,tblMM_PO_Details.Rate,tblMM_PO_Details.Discount,tblMM_PO_Details.PF,tblMM_PO_Details.ExST,tblMM_PO_Details.VAT", "tblMM_PO_Master,tblMM_PO_Details", "tblMM_PO_Master.CompId='" + compid + "' AND tblMM_PO_Master.PONo=tblMM_PO_Details.PONo AND tblMM_PO_Master.PRSPRFlag='" + prspr + "' AND tblMM_PO_Master.Authorize='" + authnon + "'");
   //            SqlCommand cmd = new SqlCommand(sqlPO, con);
   //            SqlDataAdapter DA = new SqlDataAdapter(cmd);
   //            DataSet DS = new DataSet();
   //            DA.Fill(DS);

   //            if (DS.Tables[0].Rows.Count > 0)
   //            {
   //                for (int u = 0; u < DS.Tables[0].Rows.Count; u++)
   //                {
   //                    includeWODept = "";
   //                    if (wodept == 1)
   //                    {
   //                        includeWODept = " AND tblMM_PR_Details.WONo='" + wono + "'";
   //                    }

   //                    string sqlPRSPR = "";

   //                    sqlPRSPR = this.select("tblMM_PR_Details.AHId", "tblMM_PR_Master,tblMM_PR_Details", "tblMM_PR_Master.PRNo='" + DS.Tables[0].Rows[u]["PRNo"].ToString() + "' AND tblMM_PR_Details.Id='" + DS.Tables[0].Rows[u]["PRId"].ToString() + "' AND tblMM_PR_Master.PRNo=tblMM_PR_Details.PRNo AND tblMM_PR_Master.CompId='" + compid + "'" + includeWODept + "");
   //                    SqlCommand cmd2 = new SqlCommand(sqlPRSPR, con);
   //                    SqlDataAdapter DA2 = new SqlDataAdapter(cmd2);
   //                    DataSet DS2 = new DataSet();
   //                    DA2.Fill(DS2);

   //                    if (DS2.Tables[0].Rows.Count > 0)
   //                    {
   //                        if (Convert.ToInt32(DS2.Tables[0].Rows[0]["AHId"]) == accid)
   //                        {
   //                            if (BasicTax == 0)
   //                            {
   //                                Amt += this.CalBasicAmt(Convert.ToDouble(decimal.Parse(DS.Tables[0].Rows[u]["Qty"].ToString()).ToString("N3")), Convert.ToDouble(decimal.Parse(DS.Tables[0].Rows[u]["Rate"].ToString()).ToString("N2")));
   //                            }

   //                            if (BasicTax == 1)
   //                            {
   //                                Amt += this.CalDiscAmt(Convert.ToDouble(decimal.Parse(DS.Tables[0].Rows[u]["Qty"].ToString()).ToString("N3")), Convert.ToDouble(decimal.Parse(DS.Tables[0].Rows[u]["Rate"].ToString()).ToString("N2")), Convert.ToDouble(decimal.Parse(DS.Tables[0].Rows[u]["Discount"].ToString()).ToString("N2")));

   //                            }

   //                            if (BasicTax == 2)
   //                            {
   //                                string sqlPF = this.select("tblPacking_Master.Value", "tblPacking_Master", "tblPacking_Master.Id='" + DS.Tables[0].Rows[u]["PF"].ToString() + "'");

   //                                SqlCommand cmd3 = new SqlCommand(sqlPF, con);
   //                                SqlDataAdapter DA3 = new SqlDataAdapter(cmd3);
   //                                DataSet DS3 = new DataSet();
   //                                DA3.Fill(DS3);

   //                                double PF = Convert.ToDouble(decimal.Parse(DS3.Tables[0].Rows[0]["Value"].ToString()).ToString("N3"));

   //                                string sqlExSer = this.select("tblExciseser_Master.Value", "tblExciseser_Master", "tblExciseser_Master.Id='" + DS.Tables[0].Rows[u]["ExST"].ToString() + "'");

   //                                SqlCommand cmd4 = new SqlCommand(sqlExSer, con);
   //                                SqlDataAdapter DA4 = new SqlDataAdapter(cmd4);
   //                                DataSet DS4 = new DataSet();
   //                                DA4.Fill(DS4);

   //                                double ExSer = Convert.ToDouble(decimal.Parse(DS4.Tables[0].Rows[0]["Value"].ToString()).ToString("N3"));


   //                                string sqlvat = this.select("tblVAT_Master.Value", "tblVAT_Master", "tblVAT_Master.Id='" + DS.Tables[0].Rows[u]["VAT"].ToString() + "'");
   //                                SqlCommand cmd5 = new SqlCommand(sqlvat, con);
   //                                SqlDataAdapter DA5 = new SqlDataAdapter(cmd5);
   //                                DataSet DS5 = new DataSet();
   //                                DA5.Fill(DS5);

   //                                double Vat = Convert.ToDouble(decimal.Parse(DS5.Tables[0].Rows[0]["Value"].ToString()).ToString("N3"));

   //                                Amt += this.CalTaxAmt(Convert.ToDouble(decimal.Parse(DS.Tables[0].Rows[u]["Qty"].ToString()).ToString("N3")), Convert.ToDouble(decimal.Parse(DS.Tables[0].Rows[u]["Rate"].ToString()).ToString("N2")), Convert.ToDouble(decimal.Parse(DS.Tables[0].Rows[u]["Discount"].ToString()).ToString("N2")), PF, ExSer, Vat);

   //                            }

   //                            if (BasicTax == 3)
   //                            {
   //                                double CalBasicAmt = this.CalBasicAmt(Convert.ToDouble(decimal.Parse(DS.Tables[0].Rows[u]["Qty"].ToString()).ToString("N3")), Convert.ToDouble(decimal.Parse(DS.Tables[0].Rows[u]["Rate"].ToString()).ToString("N2")));
   //                                double CalOnlyTax = this.CalDiscAmt(Convert.ToDouble(decimal.Parse(DS.Tables[0].Rows[u]["Qty"].ToString()).ToString("N3")), Convert.ToDouble(decimal.Parse(DS.Tables[0].Rows[u]["Rate"].ToString()).ToString("N2")), Convert.ToDouble(decimal.Parse(DS.Tables[0].Rows[u]["Discount"].ToString()).ToString("N2")));

   //                                Amt += CalBasicAmt + CalOnlyTax;
   //                            }

   //                        }
   //                    }
   //                }
   //            }
   //        }



   //        if (prspr == 1)
   //        {
   //            string sqlPO = this.select("tblMM_PO_Details.SPRId,tblMM_PO_Details.SPRNo,tblMM_PO_Details.Qty,tblMM_PO_Details.Rate,tblMM_PO_Details.Discount,tblMM_PO_Details.PF,tblMM_PO_Details.ExST,tblMM_PO_Details.VAT", "tblMM_PO_Master,tblMM_PO_Details", "tblMM_PO_Master.CompId='" + compid + "' AND tblMM_PO_Master.PONo=tblMM_PO_Details.PONo AND tblMM_PO_Master.PRSPRFlag='" + prspr + "' AND tblMM_PO_Master.Authorize='" + authnon + "'");
   //            SqlCommand cmd = new SqlCommand(sqlPO, con);
   //            SqlDataAdapter DA = new SqlDataAdapter(cmd);
   //            DataSet DS = new DataSet();
   //            DA.Fill(DS);

   //            if (DS.Tables[0].Rows.Count > 0)
   //            {
   //                for (int u = 0; u < DS.Tables[0].Rows.Count; u++)
   //                {
   //                    includeWODept = "";
   //                    if (wodept == 1)
   //                    {
   //                        if (dept == 0)
   //                        {
   //                            includeWODept = " AND tblMM_SPR_Details.WONo='" + wono + "'";
   //                        }
   //                        else
   //                        {
   //                            includeWODept = " AND tblMM_SPR_Details.DeptId='" + dept + "'";
   //                        }
   //                    }

   //                    string sqlPRSPR = "";

   //                    sqlPRSPR = this.select("tblMM_SPR_Details.AHId", "tblMM_SPR_Master,tblMM_SPR_Details", "tblMM_SPR_Master.SPRNo='" + DS.Tables[0].Rows[u]["SPRNo"].ToString() + "' AND tblMM_SPR_Details.Id='" + DS.Tables[0].Rows[u]["SPRId"].ToString() + "' AND tblMM_SPR_Master.SPRNo=tblMM_SPR_Details.SPRNo AND tblMM_SPR_Master.CompId='" + compid + "'" + includeWODept + "");

   //                    SqlCommand cmd2 = new SqlCommand(sqlPRSPR, con);
   //                    SqlDataAdapter DA2 = new SqlDataAdapter(cmd2);
   //                    DataSet DS2 = new DataSet();
   //                    DA2.Fill(DS2);

   //                    if (DS2.Tables[0].Rows.Count > 0)
   //                    {
   //                        if (Convert.ToInt32(DS2.Tables[0].Rows[0]["AHId"]) == accid)
   //                        {
   //                            if (BasicTax == 0)
   //                            {
   //                                Amt += this.CalBasicAmt(Convert.ToDouble(decimal.Parse(DS.Tables[0].Rows[u]["Qty"].ToString()).ToString("N3")), Convert.ToDouble(decimal.Parse(DS.Tables[0].Rows[u]["Rate"].ToString()).ToString("N2")));
   //                            }

   //                            if (BasicTax == 1)
   //                            {
   //                                Amt += this.CalDiscAmt(Convert.ToDouble(decimal.Parse(DS.Tables[0].Rows[u]["Qty"].ToString()).ToString("N3")), Convert.ToDouble(decimal.Parse(DS.Tables[0].Rows[u]["Rate"].ToString()).ToString("N2")), Convert.ToDouble(decimal.Parse(DS.Tables[0].Rows[u]["Discount"].ToString()).ToString("N2")));

   //                            }

   //                            if (BasicTax == 2)
   //                            {
   //                                string sqlPF = this.select("tblPacking_Master.Value", "tblPacking_Master", "tblPacking_Master.Id='" + DS.Tables[0].Rows[u]["PF"].ToString() + "'");

   //                                SqlCommand cmd3 = new SqlCommand(sqlPF, con);
   //                                SqlDataAdapter DA3 = new SqlDataAdapter(cmd3);
   //                                DataSet DS3 = new DataSet();
   //                                DA3.Fill(DS3);

   //                                double PF = Convert.ToDouble(decimal.Parse(DS3.Tables[0].Rows[0]["Value"].ToString()).ToString("N2"));

   //                                string sqlExSer = this.select("tblExciseser_Master.Value", "tblExciseser_Master", "tblExciseser_Master.Id='" + DS.Tables[0].Rows[u]["ExST"].ToString() + "'");

   //                                SqlCommand cmd4 = new SqlCommand(sqlExSer, con);
   //                                SqlDataAdapter DA4 = new SqlDataAdapter(cmd4);
   //                                DataSet DS4 = new DataSet();
   //                                DA4.Fill(DS4);

   //                                double ExSer = Convert.ToDouble(decimal.Parse(DS4.Tables[0].Rows[0]["Value"].ToString()).ToString("N2"));


   //                                string sqlvat = this.select("tblVAT_Master.Value", "tblVAT_Master", "tblVAT_Master.Id='" + DS.Tables[0].Rows[u]["VAT"].ToString() + "'");
   //                                SqlCommand cmd5 = new SqlCommand(sqlvat, con);
   //                                SqlDataAdapter DA5 = new SqlDataAdapter(cmd5);
   //                                DataSet DS5 = new DataSet();
   //                                DA5.Fill(DS5);

   //                                double Vat = Convert.ToDouble(decimal.Parse(DS5.Tables[0].Rows[0]["Value"].ToString()).ToString("N2"));

   //                                Amt += this.CalTaxAmt(Convert.ToDouble(decimal.Parse(DS.Tables[0].Rows[u]["Qty"].ToString()).ToString("N3")), Convert.ToDouble(decimal.Parse(DS.Tables[0].Rows[u]["Rate"].ToString()).ToString("N2")), Convert.ToDouble(decimal.Parse(DS.Tables[0].Rows[u]["Discount"].ToString()).ToString("N2")), PF, ExSer, Vat);

                                   

   //                            }

   //                            if (BasicTax == 3)
   //                            {
   //                                double CalBasicAmt = this.CalBasicAmt(Convert.ToDouble(decimal.Parse(DS.Tables[0].Rows[u]["Qty"].ToString()).ToString("N3")), Convert.ToDouble(decimal.Parse(DS.Tables[0].Rows[u]["Rate"].ToString()).ToString("N2")));
   //                                double CalOnlyTax = this.CalDiscAmt(Convert.ToDouble(decimal.Parse(DS.Tables[0].Rows[u]["Qty"].ToString()).ToString("N3")), Convert.ToDouble(decimal.Parse(DS.Tables[0].Rows[u]["Rate"].ToString()).ToString("N2")), Convert.ToDouble(decimal.Parse(DS.Tables[0].Rows[u]["Discount"].ToString()).ToString("N2")));

   //                                Amt += CalBasicAmt + CalOnlyTax;
   //                            }

   //                        }
   //                    }
   //                }
   //            }
   //        }
   //    }
   //    catch (Exception ex)
   //    {
   //    }

   //    return Math.Round(Amt, 2);
   //    con.Close();
   //} 

   public void SearchData(DropDownList drp1, DropDownList drp2, DropDownList drpsearch, TextBox txt, GridView drd,int compid,int FinYearId)
   {
       string var1 = "";
       string var2 = "";
       string var3 = "";
       try
       {
           if (drp1.SelectedValue != "Select Category")
           {
               var1 = "tblDG_Category_Master,tblDG_Item_Master,Unit_Master,vw_Unit_Master";

               var2 = "tblDG_Item_Master.Id,tblDG_Category_Master.Symbol+'-'+ tblDG_Category_Master.CName as Category , tblDG_Item_Master.PartNo,tblDG_Item_Master.Revision ,tblDG_Item_Master.Process,tblDG_Item_Master.ManfDesc,tblDG_Item_Master.PurchDesc ,Unit_Master.Symbol As UOMBasic,vw_Unit_Master.Symbol As UOMPurchase,tblDG_Item_Master.MinOrderQty ,tblDG_Item_Master.MinStockQty,tblDG_Item_Master.StockQty ,REPLACE(CONVERT(varchar, CONVERT(datetime, SUBSTRING( tblDG_Item_Master.OpeningBalDate , CHARINDEX('-',tblDG_Item_Master.OpeningBalDate ) + 1, 2) + '-' + LEFT(tblDG_Item_Master.OpeningBalDate,CHARINDEX('-',tblDG_Item_Master.OpeningBalDate) - 1) + '-' + RIGHT(tblDG_Item_Master.OpeningBalDate, CHARINDEX('-', REVERSE(tblDG_Item_Master.OpeningBalDate)) - 1)), 103), '/', '-') AS  OpenBalDate  ,tblDG_Item_Master.OpeningBalQty ,tblDG_Item_Master.Absolute,tblDG_Item_Master.UOMConFact, tblDG_Item_Master.ItemCode";

               var3 = " tblDG_Item_Master.CId=tblDG_Category_Master.CId and Unit_Master.Id=tblDG_Item_Master.UOMBasic and vw_Unit_Master.Id=tblDG_Item_Master.UOMPurchase and  tblDG_Item_Master.CId='" + drp1.SelectedValue + "'And tblDG_Item_Master.CompId='" + compid + "'And tblDG_Item_Master.FinYearId<='" + FinYearId + "'";
           }
           else
           {
               var1 = "tblDG_Category_Master,tblDG_Item_Master,Unit_Master,vw_Unit_Master";

               var2 = "tblDG_Item_Master.Id,tblDG_Category_Master.Symbol+'-'+ tblDG_Category_Master.CName as Category  ,tblDG_Item_Master.PartNo,tblDG_Item_Master.Revision ,tblDG_Item_Master.Process,tblDG_Item_Master.ManfDesc,tblDG_Item_Master.PurchDesc ,Unit_Master.Symbol As UOMBasic,vw_Unit_Master.Symbol As UOMPurchase,tblDG_Item_Master.MinOrderQty ,tblDG_Item_Master.MinStockQty,tblDG_Item_Master.StockQty ,REPLACE(CONVERT(varchar, CONVERT(datetime, SUBSTRING( tblDG_Item_Master.OpeningBalDate , CHARINDEX('-',tblDG_Item_Master.OpeningBalDate ) + 1, 2) + '-' + LEFT(tblDG_Item_Master.OpeningBalDate,CHARINDEX('-',tblDG_Item_Master.OpeningBalDate) - 1) + '-' + RIGHT(tblDG_Item_Master.OpeningBalDate, CHARINDEX('-', REVERSE(tblDG_Item_Master.OpeningBalDate)) - 1)), 103), '/', '-') AS  OpenBalDate  ,tblDG_Item_Master.OpeningBalQty ,tblDG_Item_Master.Absolute,tblDG_Item_Master.UOMConFact, tblDG_Item_Master.ItemCode";

               var3 = " tblDG_Item_Master.CId=tblDG_Category_Master.CId and Unit_Master.Id=tblDG_Item_Master.UOMBasic and vw_Unit_Master.Id=tblDG_Item_Master.UOMPurchase and tblDG_Item_Master.CompId='" + compid + "'And tblDG_Item_Master.FinYearId<='" + FinYearId + "'";
           }
           if (drp2.SelectedValue != "Select SubCategory")
           {
               var1 = var1 + ",tblDG_SubCategory_Master";
               var2 = var2 + ",tblDG_SubCategory_Master.Symbol+'-'+tblDG_SubCategory_Master.SCName as SubCategory";
               var3 = var3 + " AND tblDG_Item_Master.SCId=tblDG_SubCategory_Master.SCId AND tblDG_Item_Master.SCId='" + drp2.SelectedValue + "'";

           }
           else
           {
               var1 = var1+"" ;
               var2 = var2+"" ;
               var3 = var3+"" ;
           }
           this.BindGridData(var1, var2, var3, drd, drpsearch.SelectedValue, txt.Text,compid,FinYearId);

       }
       catch (Exception ch)
       {
       }

   }

   public void FillgridviewMRS(string sd, string A, string B, string s, int CompId, GridView grv, TextBox txtSearchItemCode, DropDownList DropDownList3, string Sid)
   {
       try
       {
           string sql = "";

           string x = "";

           if (sd != "Select Category")
           {
               string y = "";
               if (A != "Select SubCategory")
               {
                   y = "  And tblDG_Item_Master.SCId='" + A + "'";
               }
               x = " AND  tblDG_Item_Master.CId='" + sd + "'";

               string z = "";
               string p = "";
               txtSearchItemCode.Visible = true;
               if (B != "Select")
               {
                   if (B == "tblDG_Item_Master.ItemCode")
                   {
                       txtSearchItemCode.Visible = true;
                       p = " And tblDG_Item_Master.ItemCode Like '" + s + "%'";
                   }

                   if (B == "tblDG_Item_Master.ManfDesc")
                   {
                       txtSearchItemCode.Visible = true;
                       p = " And tblDG_Item_Master.ManfDesc Like '%" + s + "%'";
                   }
                   if (B == "tblDG_Item_Master.PurchDesc")
                   {
                       txtSearchItemCode.Visible = true;
                       p = " And tblDG_Item_Master.PurchDesc Like '%" + s + "%'";
                   }

                   if (B == "tblDG_Item_Master.Location")
                   {
                       txtSearchItemCode.Visible = false;
                       DropDownList3.Visible = true;
                       p = " And tblDG_Item_Master.Location='" + DropDownList3.SelectedValue + "'";
                   }
               }

               sql = this.select("tblDG_Item_Master.Id,tblDG_Category_Master.Symbol+'-'+ tblDG_Category_Master.CName as Category , tblDG_Item_Master.PartNo,tblDG_Item_Master.Revision ,tblDG_Item_Master.Process,tblDG_Item_Master.ManfDesc,tblDG_Item_Master.PurchDesc ,Unit_Master.Symbol As UOMBasic,vw_Unit_Master.Symbol As UOMPurchase,tblDG_Item_Master.MinOrderQty ,tblDG_Item_Master.MinStockQty,tblDG_Item_Master.StockQty ,REPLACE(CONVERT(varchar, CONVERT(datetime, SUBSTRING( tblDG_Item_Master.OpeningBalDate , CHARINDEX('-',tblDG_Item_Master.OpeningBalDate ) + 1, 2) + '-' + LEFT(tblDG_Item_Master.OpeningBalDate,CHARINDEX('-',tblDG_Item_Master.OpeningBalDate) - 1) + '-' + RIGHT(tblDG_Item_Master.OpeningBalDate, CHARINDEX('-', REVERSE(tblDG_Item_Master.OpeningBalDate)) - 1)), 103), '/', '-') AS  OpenBalDate  ,tblDG_Item_Master.OpeningBalQty ,tblDG_Item_Master.Absolute,tblDG_Item_Master.UOMConFact, tblDG_Item_Master.ItemCode", "tblDG_Category_Master,tblDG_Item_Master,Unit_Master,vw_Unit_Master", "tblDG_Item_Master.CId=tblDG_Category_Master.CId and Unit_Master.Id=tblDG_Item_Master.UOMBasic and vw_Unit_Master.Id=tblDG_Item_Master.UOMPurchase" + x + "" + y + "" + p + " AND tblDG_Item_Master.CompId='" + CompId + "'");

           }
           else
           {

               sql = this.select("tblDG_Item_Master.Id,tblDG_Category_Master.Symbol+'-'+ tblDG_Category_Master.CName as Category , tblDG_Item_Master.PartNo,tblDG_Item_Master.Revision ,tblDG_Item_Master.Process,tblDG_Item_Master.ManfDesc,tblDG_Item_Master.PurchDesc ,Unit_Master.Symbol As UOMBasic,vw_Unit_Master.Symbol As UOMPurchase,tblDG_Item_Master.MinOrderQty ,tblDG_Item_Master.MinStockQty,tblDG_Item_Master.StockQty ,REPLACE(CONVERT(varchar, CONVERT(datetime, SUBSTRING( tblDG_Item_Master.OpeningBalDate , CHARINDEX('-',tblDG_Item_Master.OpeningBalDate ) + 1, 2) + '-' + LEFT(tblDG_Item_Master.OpeningBalDate,CHARINDEX('-',tblDG_Item_Master.OpeningBalDate) - 1) + '-' + RIGHT(tblDG_Item_Master.OpeningBalDate, CHARINDEX('-', REVERSE(tblDG_Item_Master.OpeningBalDate)) - 1)), 103), '/', '-') AS  OpenBalDate  ,tblDG_Item_Master.OpeningBalQty ,tblDG_Item_Master.Absolute,tblDG_Item_Master.UOMConFact, tblDG_Item_Master.ItemCode", "tblDG_Category_Master,tblDG_Item_Master,Unit_Master,vw_Unit_Master", "tblDG_Item_Master.CId=tblDG_Category_Master.CId and Unit_Master.Id=tblDG_Item_Master.UOMBasic and vw_Unit_Master.Id=tblDG_Item_Master.UOMPurchase AND tblDG_Item_Master.CompId='" + CompId + "'");

           }


           this.BindGridMRSData(sql, grv, CompId, Sid);
       }
       catch (Exception ep) { }


   }

   public void BindGridMRSData(string sql, GridView SearchGridView, int CompId, string sId)
   {
       string strConnString = this.Connection();
       SqlConnection con = new SqlConnection(strConnString);
       DataSet DS = new DataSet();
       try
       {           
           SqlCommand cmd = new SqlCommand(sql, con);
           SqlDataAdapter da = new SqlDataAdapter(cmd);
           da.Fill(DS, "tblName");
           if (DS.Tables[0].Rows.Count == 0)
           {
               DS.Tables[0].Rows.Add(DS.Tables[0].NewRow());
           }

           DataTable dt = new DataTable();

           dt.Columns.Add(new System.Data.DataColumn("Id", typeof(string)));
           dt.Columns.Add(new System.Data.DataColumn("Category", typeof(string)));
           dt.Columns.Add(new System.Data.DataColumn("SubCategory", typeof(string)));
           dt.Columns.Add(new System.Data.DataColumn("PartNo", typeof(string)));
           dt.Columns.Add(new System.Data.DataColumn("Revision", typeof(string)));
           dt.Columns.Add(new System.Data.DataColumn("Process", typeof(string)));
           dt.Columns.Add(new System.Data.DataColumn("ItemCode", typeof(string)));
           dt.Columns.Add(new System.Data.DataColumn("ManfDesc", typeof(string)));
           dt.Columns.Add(new System.Data.DataColumn("PurchDesc", typeof(string)));
           dt.Columns.Add(new System.Data.DataColumn("UOMBasic", typeof(string)));
           dt.Columns.Add(new System.Data.DataColumn("UOMPurchase", typeof(string)));
           dt.Columns.Add(new System.Data.DataColumn("MinOrderQty", typeof(string)));
           dt.Columns.Add(new System.Data.DataColumn("MinStockQty", typeof(string)));
           dt.Columns.Add(new System.Data.DataColumn("StockQty", typeof(string)));
           dt.Columns.Add(new System.Data.DataColumn("Location", typeof(string)));
           dt.Columns.Add(new System.Data.DataColumn("Absolute", typeof(string)));
           dt.Columns.Add(new System.Data.DataColumn("OpenBalDate", typeof(string)));
           dt.Columns.Add(new System.Data.DataColumn("OpeningBalQty", typeof(string)));
           dt.Columns.Add(new System.Data.DataColumn("UOMConFact", typeof(string)));

           DataRow dr;

           for (int p = 0; p < DS.Tables[0].Rows.Count; p++)
           {
               string strgetemp = this.select("ItemId", "tblinv_MaterialRequisition_Temp", "CompId='" + CompId + "' AND SessionId='" + sId + "' AND ItemId='" + DS.Tables[0].Rows[p]["Id"].ToString() + "'");
               SqlCommand cmdgetemp = new SqlCommand(strgetemp, con);
               SqlDataAdapter mydagetemp = new SqlDataAdapter(cmdgetemp);
               DataSet mydsgetemp = new DataSet();
               mydagetemp.Fill(mydsgetemp);

               if (mydsgetemp.Tables[0].Rows.Count == 0)
               {
                   dr = dt.NewRow();

                   dr[0] = DS.Tables[0].Rows[p]["Id"].ToString();
                   dr[1] = DS.Tables[0].Rows[p]["Category"].ToString();

                   string str = this.select("tblDG_SubCategory_Master.SCId,'['+tblDG_SubCategory_Master.Symbol+']-'+tblDG_SubCategory_Master.SCName as SubCatName", "tblDG_SubCategory_Master,tblDG_Item_Master", "tblDG_SubCategory_Master.SCId=tblDG_Item_Master.SCId AND tblDG_Item_Master.Id='" + DS.Tables[0].Rows[p]["Id"].ToString() + "'And tblDG_Item_Master.CompId='" + CompId + "'");

                   SqlCommand cmd2 = new SqlCommand(str, con);
                   SqlDataAdapter myda = new SqlDataAdapter(cmd2);
                   DataSet myds = new DataSet();
                   myda.Fill(myds);

                   if (myds.Tables[0].Rows.Count > 0)
                   {
                       dr[2] = myds.Tables[0].Rows[0]["SubCatName"].ToString();
                   }
                   dr[3] = DS.Tables[0].Rows[p]["PartNo"].ToString();
                   dr[4] = DS.Tables[0].Rows[p]["Revision"].ToString();
                   dr[5] = DS.Tables[0].Rows[p]["Process"].ToString();
                   dr[6] = DS.Tables[0].Rows[p]["ItemCode"].ToString();
                   dr[7] = DS.Tables[0].Rows[p]["ManfDesc"].ToString();
                   dr[8] = DS.Tables[0].Rows[p]["PurchDesc"].ToString();
                   dr[9] = DS.Tables[0].Rows[p]["UOMBasic"].ToString();
                   dr[10] = DS.Tables[0].Rows[p]["UOMPurchase"].ToString();
                   dr[11] = DS.Tables[0].Rows[p]["MinOrderQty"].ToString();
                   dr[12] = DS.Tables[0].Rows[p]["MinStockQty"].ToString();
                   dr[13] = DS.Tables[0].Rows[p]["StockQty"].ToString();

                   string str4 = this.select("tblDG_Location_Master.LocationLabel+LocationNo as LocatName", "tblDG_Location_Master,tblDG_Item_Master", "tblDG_Location_Master.Id=tblDG_Item_Master.Location AND tblDG_Item_Master.Id='" + DS.Tables[0].Rows[p]["Id"].ToString() + "'And tblDG_Item_Master.CompId='" + CompId + "'");

                   SqlCommand cmd4 = new SqlCommand(str4, con);
                   SqlDataAdapter myda4 = new SqlDataAdapter(cmd4);
                   DataSet myds4 = new DataSet();
                   myda4.Fill(myds4);

                   if (myds4.Tables[0].Rows.Count > 0)
                   {
                       dr[14] = myds4.Tables[0].Rows[0]["LocatName"].ToString();
                   }

                   dr[15] = DS.Tables[0].Rows[p]["Absolute"].ToString();
                   dr[16] = DS.Tables[0].Rows[p]["OpenBalDate"].ToString();
                   dr[17] = DS.Tables[0].Rows[p]["OpeningBalQty"].ToString();
                   dr[18] = DS.Tables[0].Rows[p]["UOMConFact"].ToString();

                   dt.Rows.Add(dr);
                   dt.AcceptChanges();
               }


           }
           SearchGridView.DataSource = dt;
           SearchGridView.DataBind();
           
       }

       catch (Exception exp)
       {
       }
       finally
       {
           con.Close();
       }
   }

   public void FillgridviewMRN(string sd, string A, string B, string s, int compid, GridView grv, TextBox txtSearchItemCode, DropDownList DropDownList3,string Sid)
   {
       try
       {
           string sql = "";

           string x = "";

           if (sd != "Select Category")
           {
               string y = "";
               if (A != "Select SubCategory")
               {
                   y = "  And tblDG_Item_Master.SCId='" + A + "'";
               }
               x = " AND  tblDG_Item_Master.CId='" + sd + "'";

               string z = "";
               string p = "";
               txtSearchItemCode.Visible = true;
               if (B != "Select")
               {
                   if (B == "tblDG_Item_Master.ItemCode")
                   {
                       txtSearchItemCode.Visible = true;
                       p = " And tblDG_Item_Master.ItemCode Like '" + s + "%'";
                   }

                   if (B == "tblDG_Item_Master.ManfDesc")
                   {
                       txtSearchItemCode.Visible = true;
                       p = " And tblDG_Item_Master.ManfDesc Like '%" + s + "%'";
                   }
                   if (B == "tblDG_Item_Master.PurchDesc")
                   {
                       txtSearchItemCode.Visible = true;
                       p = " And tblDG_Item_Master.PurchDesc Like '%" + s + "%'";
                   }

                   if (B == "tblDG_Item_Master.Location")
                   {
                       txtSearchItemCode.Visible = false;
                       DropDownList3.Visible = true;
                       p = " And tblDG_Item_Master.Location='" + DropDownList3.SelectedValue + "'";
                   }
               }
               sql = this.select("tblDG_Item_Master.Id,tblDG_Category_Master.Symbol+'-'+ tblDG_Category_Master.CName as Category , tblDG_Item_Master.PartNo,tblDG_Item_Master.Revision ,tblDG_Item_Master.Process,tblDG_Item_Master.ManfDesc,tblDG_Item_Master.PurchDesc ,Unit_Master.Symbol As UOMBasic,vw_Unit_Master.Symbol As UOMPurchase,tblDG_Item_Master.MinOrderQty ,tblDG_Item_Master.MinStockQty,tblDG_Item_Master.StockQty ,REPLACE(CONVERT(varchar, CONVERT(datetime, SUBSTRING( tblDG_Item_Master.OpeningBalDate , CHARINDEX('-',tblDG_Item_Master.OpeningBalDate ) + 1, 2) + '-' + LEFT(tblDG_Item_Master.OpeningBalDate,CHARINDEX('-',tblDG_Item_Master.OpeningBalDate) - 1) + '-' + RIGHT(tblDG_Item_Master.OpeningBalDate, CHARINDEX('-', REVERSE(tblDG_Item_Master.OpeningBalDate)) - 1)), 103), '/', '-') AS  OpenBalDate  ,tblDG_Item_Master.OpeningBalQty ,tblDG_Item_Master.Absolute,tblDG_Item_Master.UOMConFact, tblDG_Item_Master.ItemCode", "tblDG_Category_Master,tblDG_Item_Master,Unit_Master,vw_Unit_Master", "tblDG_Item_Master.CId=tblDG_Category_Master.CId and Unit_Master.Id=tblDG_Item_Master.UOMBasic and vw_Unit_Master.Id=tblDG_Item_Master.UOMPurchase" + x + "" + y + "" + p + " AND tblDG_Item_Master.CompId='" + compid + "'");

           }
           else
           {

               sql = this.select("tblDG_Item_Master.Id,tblDG_Category_Master.Symbol+'-'+ tblDG_Category_Master.CName as Category , tblDG_Item_Master.PartNo,tblDG_Item_Master.Revision ,tblDG_Item_Master.Process,tblDG_Item_Master.ManfDesc,tblDG_Item_Master.PurchDesc ,Unit_Master.Symbol As UOMBasic,vw_Unit_Master.Symbol As UOMPurchase,tblDG_Item_Master.MinOrderQty ,tblDG_Item_Master.MinStockQty,tblDG_Item_Master.StockQty ,REPLACE(CONVERT(varchar, CONVERT(datetime, SUBSTRING( tblDG_Item_Master.OpeningBalDate , CHARINDEX('-',tblDG_Item_Master.OpeningBalDate ) + 1, 2) + '-' + LEFT(tblDG_Item_Master.OpeningBalDate,CHARINDEX('-',tblDG_Item_Master.OpeningBalDate) - 1) + '-' + RIGHT(tblDG_Item_Master.OpeningBalDate, CHARINDEX('-', REVERSE(tblDG_Item_Master.OpeningBalDate)) - 1)), 103), '/', '-') AS  OpenBalDate  ,tblDG_Item_Master.OpeningBalQty ,tblDG_Item_Master.Absolute,tblDG_Item_Master.UOMConFact, tblDG_Item_Master.ItemCode", "tblDG_Category_Master,tblDG_Item_Master,Unit_Master,vw_Unit_Master", "tblDG_Item_Master.CId=tblDG_Category_Master.CId and Unit_Master.Id=tblDG_Item_Master.UOMBasic and vw_Unit_Master.Id=tblDG_Item_Master.UOMPurchase AND tblDG_Item_Master.CompId='" + compid + "'");

           }

           this.BindGridMRNData(sql,grv,compid,Sid);
       }
       catch (Exception ep) { }


   }

   public void BindGridMRNData(string sql, GridView SearchGridView, int CompId, string sId)
   {

       try
       {
           string strConnString = this.Connection();
           SqlConnection con = new SqlConnection(strConnString);
           DataSet DS = new DataSet();
           SqlCommand cmd = new SqlCommand(sql, con);
           SqlDataAdapter da = new SqlDataAdapter(cmd);
           da.Fill(DS, "tblName");
           DataTable dt = new DataTable();

           dt.Columns.Add(new System.Data.DataColumn("Id", typeof(string)));
           dt.Columns.Add(new System.Data.DataColumn("Category", typeof(string)));
           dt.Columns.Add(new System.Data.DataColumn("SubCategory", typeof(string)));
           dt.Columns.Add(new System.Data.DataColumn("PartNo", typeof(string)));
           dt.Columns.Add(new System.Data.DataColumn("Revision", typeof(string)));
           dt.Columns.Add(new System.Data.DataColumn("Process", typeof(string)));
           dt.Columns.Add(new System.Data.DataColumn("ItemCode", typeof(string)));
           dt.Columns.Add(new System.Data.DataColumn("ManfDesc", typeof(string)));
           dt.Columns.Add(new System.Data.DataColumn("PurchDesc", typeof(string)));
           dt.Columns.Add(new System.Data.DataColumn("UOMBasic", typeof(string)));
           dt.Columns.Add(new System.Data.DataColumn("UOMPurchase", typeof(string)));
           dt.Columns.Add(new System.Data.DataColumn("MinOrderQty", typeof(string)));
           dt.Columns.Add(new System.Data.DataColumn("MinStockQty", typeof(string)));
           dt.Columns.Add(new System.Data.DataColumn("StockQty", typeof(string)));
           dt.Columns.Add(new System.Data.DataColumn("Location", typeof(string)));
           dt.Columns.Add(new System.Data.DataColumn("Absolute", typeof(string)));
           dt.Columns.Add(new System.Data.DataColumn("OpenBalDate", typeof(string)));
           dt.Columns.Add(new System.Data.DataColumn("OpeningBalQty", typeof(string)));
           dt.Columns.Add(new System.Data.DataColumn("UOMConFact", typeof(string)));

           DataRow dr;

           for (int p = 0; p < DS.Tables[0].Rows.Count; p++)
           {
               string strgetemp =this.select("ItemId", "tblinv_MaterialReturn_Temp", "CompId='" + CompId + "' AND SessionId='" + sId + "' AND ItemId='" + DS.Tables[0].Rows[p]["Id"].ToString() + "'");
               SqlCommand cmdgetemp = new SqlCommand(strgetemp, con);
               SqlDataAdapter mydagetemp = new SqlDataAdapter(cmdgetemp);
               DataSet mydsgetemp = new DataSet();
               mydagetemp.Fill(mydsgetemp);

               if (mydsgetemp.Tables[0].Rows.Count == 0)
               {
                   dr = dt.NewRow();

                   dr[0] = DS.Tables[0].Rows[p]["Id"].ToString();
                   dr[1] = DS.Tables[0].Rows[p]["Category"].ToString();

                   string str = this.select("tblDG_SubCategory_Master.SCId,'['+tblDG_SubCategory_Master.Symbol+']-'+tblDG_SubCategory_Master.SCName as SubCatName", "tblDG_SubCategory_Master,tblDG_Item_Master", "tblDG_SubCategory_Master.SCId=tblDG_Item_Master.SCId AND tblDG_Item_Master.Id='" + DS.Tables[0].Rows[p]["Id"].ToString() + "'And tblDG_Item_Master.CompId='" + CompId + "'");

                   SqlCommand cmd2 = new SqlCommand(str, con);
                   SqlDataAdapter myda = new SqlDataAdapter(cmd2);
                   DataSet myds = new DataSet();
                   myda.Fill(myds);

                   if (myds.Tables[0].Rows.Count > 0)
                   {
                       dr[2] = myds.Tables[0].Rows[0]["SubCatName"].ToString();
                   }
                   dr[3] = DS.Tables[0].Rows[p]["PartNo"].ToString();
                   dr[4] = DS.Tables[0].Rows[p]["Revision"].ToString();
                   dr[5] = DS.Tables[0].Rows[p]["Process"].ToString();
                   dr[6] = DS.Tables[0].Rows[p]["ItemCode"].ToString();
                   dr[7] = DS.Tables[0].Rows[p]["ManfDesc"].ToString();
                   dr[8] = DS.Tables[0].Rows[p]["PurchDesc"].ToString();
                   dr[9] = DS.Tables[0].Rows[p]["UOMBasic"].ToString();
                   dr[10] = DS.Tables[0].Rows[p]["UOMPurchase"].ToString();
                   dr[11] = DS.Tables[0].Rows[p]["MinOrderQty"].ToString();
                   dr[12] = DS.Tables[0].Rows[p]["MinStockQty"].ToString();
                   dr[13] = DS.Tables[0].Rows[p]["StockQty"].ToString();

                   string str4 = this.select("tblDG_Location_Master.LocationLabel+LocationNo as LocatName", "tblDG_Location_Master,tblDG_Item_Master", "tblDG_Location_Master.Id=tblDG_Item_Master.Location AND tblDG_Item_Master.Id='" + DS.Tables[0].Rows[p]["Id"].ToString() + "'And tblDG_Item_Master.CompId='" +CompId+ "'");

                   SqlCommand cmd4 = new SqlCommand(str4, con);
                   SqlDataAdapter myda4 = new SqlDataAdapter(cmd4);
                   DataSet myds4 = new DataSet();
                   myda4.Fill(myds4);

                   if (myds4.Tables[0].Rows.Count > 0)
                   {
                       dr[14] = myds4.Tables[0].Rows[0]["LocatName"].ToString();
                   }

                   dr[15] = DS.Tables[0].Rows[p]["Absolute"].ToString();
                   dr[16] = DS.Tables[0].Rows[p]["OpenBalDate"].ToString();
                   dr[17] = DS.Tables[0].Rows[p]["OpeningBalQty"].ToString();
                   dr[18] = DS.Tables[0].Rows[p]["UOMConFact"].ToString();

                   dt.Rows.Add(dr);
                   dt.AcceptChanges();
               }


           }
           SearchGridView.DataSource = dt;
           SearchGridView.DataBind();
         
       }

       catch (Exception exp)
       {
           //If databinding threw exception bcoz current page index is > than available page index
           SearchGridView.PageIndex = 0;
           SearchGridView.DataBind();
       }
       finally
       {
           //Select the first row returned
           if (SearchGridView.Rows.Count > 0)
               SearchGridView.SelectedIndex = 0;
       }
   }

   public void BindGridData(string tblName, string tblfield, string whr, GridView SearchGridView, string drpvalue, string hfSearchTextValue,int compid,int finId)
   {
       string strConnString = this.Connection();
       SqlConnection con = new SqlConnection(strConnString);
       try
       {                    
           DataSet DS = new DataSet();

           string sql = this.select(tblfield, tblName, whr);

           SqlCommand cmd = new SqlCommand(sql, con);
           SqlDataAdapter da = new SqlDataAdapter(cmd);
           da.Fill(DS, tblName);

           if (hfSearchTextValue != "")
           {
               DS.Clear();
               sql += " AND " + drpvalue + " like '" + hfSearchTextValue + "%'";
               SqlCommand cmd1 = new SqlCommand(sql, con);
               SqlDataAdapter da1 = new SqlDataAdapter(cmd1);
               da1.Fill(DS, tblName);
           }
           else
           {
               DS.Clear();              
               SqlCommand cmd1 = new SqlCommand(sql, con);
               SqlDataAdapter da1 = new SqlDataAdapter(cmd1);
               da1.Fill(DS, tblName);
           }

           DataTable dt = new DataTable();

           dt.Columns.Add(new System.Data.DataColumn("Id", typeof(MappingType)));
           dt.Columns.Add(new System.Data.DataColumn("Category", typeof(string)));
           dt.Columns.Add(new System.Data.DataColumn("SubCategory", typeof(string)));
           dt.Columns.Add(new System.Data.DataColumn("PartNo", typeof(string)));
           dt.Columns.Add(new System.Data.DataColumn("Revision", typeof(string)));
           dt.Columns.Add(new System.Data.DataColumn("Process", typeof(string)));
           dt.Columns.Add(new System.Data.DataColumn("ItemCode", typeof(string)));
           dt.Columns.Add(new System.Data.DataColumn("ManfDesc", typeof(string)));
           dt.Columns.Add(new System.Data.DataColumn("PurchDesc", typeof(string)));
           dt.Columns.Add(new System.Data.DataColumn("UOMBasic", typeof(string)));
           dt.Columns.Add(new System.Data.DataColumn("UOMPurchase", typeof(string)));
           dt.Columns.Add(new System.Data.DataColumn("MinOrderQty", typeof(string)));
           dt.Columns.Add(new System.Data.DataColumn("MinStockQty", typeof(string)));
           dt.Columns.Add(new System.Data.DataColumn("StockQty", typeof(string)));
           dt.Columns.Add(new System.Data.DataColumn("Location", typeof(string)));
           dt.Columns.Add(new System.Data.DataColumn("Absolute", typeof(string)));
           dt.Columns.Add(new System.Data.DataColumn("OpenBalDate", typeof(string)));
           dt.Columns.Add(new System.Data.DataColumn("OpeningBalQty", typeof(string)));
           dt.Columns.Add(new System.Data.DataColumn("UOMConFact", typeof(string)));

           DataRow dr;

           for (int p = 0; p < DS.Tables[0].Rows.Count; p++)
           {
               dr = dt.NewRow();

               dr[0] = DS.Tables[0].Rows[p]["Id"].ToString();
               dr[1] = DS.Tables[0].Rows[p]["Category"].ToString();

               string str = this.select("tblDG_SubCategory_Master.SCId,'['+tblDG_SubCategory_Master.Symbol+']-'+tblDG_SubCategory_Master.SCName as SubCatName", "tblDG_SubCategory_Master,tblDG_Item_Master", "tblDG_SubCategory_Master.SCId=tblDG_Item_Master.SCId AND tblDG_Item_Master.Id='" + DS.Tables[0].Rows[p]["Id"].ToString() + "'And tblDG_Item_Master.CompId='" + compid + "' And tblDG_Item_Master.FinYearId<='"+finId+"'");

               SqlCommand cmd2 = new SqlCommand(str, con);
               SqlDataAdapter myda = new SqlDataAdapter(cmd2);
               DataSet myds = new DataSet();
               myda.Fill(myds);

               if (myds.Tables[0].Rows.Count > 0)
               {
                   dr[2] = myds.Tables[0].Rows[0]["SubCatName"].ToString();
               }
               dr[3] = DS.Tables[0].Rows[p]["PartNo"].ToString();
               dr[4] = DS.Tables[0].Rows[p]["Revision"].ToString();
               dr[5] = DS.Tables[0].Rows[p]["Process"].ToString();
               dr[6] = DS.Tables[0].Rows[p]["ItemCode"].ToString();
               dr[7] = DS.Tables[0].Rows[p]["ManfDesc"].ToString();
               dr[8] = DS.Tables[0].Rows[p]["PurchDesc"].ToString();
               dr[9] = DS.Tables[0].Rows[p]["UOMBasic"].ToString();
               dr[10] = DS.Tables[0].Rows[p]["UOMPurchase"].ToString();
               dr[11] = DS.Tables[0].Rows[p]["MinOrderQty"].ToString();
               dr[12] = DS.Tables[0].Rows[p]["MinStockQty"].ToString();
               dr[13] = DS.Tables[0].Rows[p]["StockQty"].ToString();

               string str4 = this.select("tblDG_Location_Master.LocationLabel+LocationNo as LocatName", "tblDG_Location_Master,tblDG_Item_Master", "tblDG_Location_Master.Id=tblDG_Item_Master.Location AND tblDG_Item_Master.Id='" + DS.Tables[0].Rows[p]["Id"].ToString() + "' And tblDG_Item_Master.CompId='" + compid + "'And tblDG_Item_Master.FinYearId<='" + finId + "' ");

               SqlCommand cmd4 = new SqlCommand(str4, con);
               SqlDataAdapter myda4 = new SqlDataAdapter(cmd4);
               DataSet myds4 = new DataSet();
               myda4.Fill(myds4);

               if (myds4.Tables[0].Rows.Count > 0)
               {
                   dr[14] = myds4.Tables[0].Rows[0]["LocatName"].ToString();
               }
               else
               {
                   dr[14] = "NA";
               }

               dr[15] = DS.Tables[0].Rows[p]["Absolute"].ToString();
               dr[16] = DS.Tables[0].Rows[p]["OpenBalDate"].ToString();
               dr[17] = DS.Tables[0].Rows[p]["OpeningBalQty"].ToString();
               dr[18] = DS.Tables[0].Rows[p]["UOMConFact"].ToString();

               dt.Rows.Add(dr);
               dt.AcceptChanges();
           }

           SearchGridView.DataSource = dt;
           SearchGridView.DataBind();
          

       }

       catch (Exception exp)   { }

       finally { con.Close(); }
   }

   public void ItemHistory_BOM(string wono, string pid, string cid, int CompId, GridView GridView2, double qty, int finId)
   {

       string connStr = this.Connection();
       SqlConnection con = new SqlConnection(connStr);
       try
       {

           List<int> li = new List<int>();
           li = this.BOMTree(wono, pid, cid, CompId, finId);

           DataTable dt = new DataTable();
           dt.Columns.Add("AssemblyNo", typeof(string));
           dt.Columns.Add("ItemCode", typeof(string));
           dt.Columns.Add("ManfDesc", typeof(string));
           dt.Columns.Add("UOMBasic", typeof(string));
           dt.Columns.Add("UnitQty", typeof(string));
           dt.Columns.Add("BOMQty", typeof(string));
           dt.Columns.Add("ItemId", typeof(int));
           dt.Columns.Add("PId", typeof(int));
           dt.Columns.Add("CId", typeof(int));
           dt.Columns.Add("WONo", typeof(String));
           dt.Columns.Add("Date", typeof(string));
           dt.Columns.Add("Time", typeof(string));
           dt.Columns.Add("Id", typeof(string));

           DataRow dr;

           for (int i = 0; i < li.Count; i++)
           {
               DataSet DS3 = new DataSet();

               if (li.Count > 0)
               {
                   string sql = this.select("tblDG_Item_Master.Id,tblDG_BOM_Master.WONo,tblDG_BOM_Master.PId,tblDG_BOM_Master.CId,tblDG_BOM_Master.ItemId,tblDG_Item_Master.ManfDesc,Unit_Master.Symbol As UOMBasic,tblDG_Item_Master.ItemCode,tblDG_BOM_Master.Qty,REPLACE(CONVERT(varchar, CONVERT(datetime, SUBSTRING( tblDG_BOM_Master.SysDate , CHARINDEX('-',tblDG_BOM_Master.SysDate ) + 1, 2) + '-' + LEFT(tblDG_BOM_Master.SysDate,CHARINDEX('-',tblDG_BOM_Master.SysDate) - 1) + '-' + RIGHT(tblDG_BOM_Master.SysDate, CHARINDEX('-', REVERSE(tblDG_BOM_Master.SysDate)) - 1)), 103), '/', '-') AS  Date ,tblDG_BOM_Master.SysTime As Time", " tblDG_BOM_Master,tblDG_Category_Master,tblDG_Item_Master,Unit_Master", "Unit_Master.Id=tblDG_Item_Master.UOMBasic and tblDG_Item_Master.Id=tblDG_BOM_Master.ItemId and  tblDG_BOM_Master.Id='" + Convert.ToInt32(li[i]) + "' And tblDG_BOM_Master.CompId='" + CompId + "'AND tblDG_Item_Master.FinYearId<='" + finId + "'");


                   SqlCommand cmd = new SqlCommand(sql, con);
                   SqlDataAdapter DA2 = new SqlDataAdapter(cmd);
                   DA2.Fill(DS3);

                   if (DS3.Tables[0].Rows.Count > 0)
                   {
                       dr = dt.NewRow();

                       string sql_P = this.select("tblDG_Item_Master.ItemCode", "tblDG_BOM_Master,tblDG_Item_Master", "tblDG_Item_Master.Id=tblDG_BOM_Master.ItemId AND tblDG_BOM_Master.CId='" + Convert.ToInt32(DS3.Tables[0].Rows[0]["PId"]) + "'And tblDG_Item_Master.CompId='" + CompId + "'AND tblDG_Item_Master.FinYearId<='" + finId + "'");
                       DataSet DS_P = new DataSet();
                       SqlCommand cmd_P = new SqlCommand(sql_P, con);
                       SqlDataAdapter DA_P = new SqlDataAdapter(cmd_P);
                       DA_P.Fill(DS_P);

                       if (DS_P.Tables[0].Rows.Count > 0)
                       {
                           dr[0] = DS_P.Tables[0].Rows[0]["ItemCode"].ToString();
                       }
                       else
                       {
                           dr[0] = "NA";
                       }

                       dr[1] = DS3.Tables[0].Rows[0]["ItemCode"].ToString();
                       dr[2] = DS3.Tables[0].Rows[0]["ManfDesc"].ToString();
                       dr[3] = DS3.Tables[0].Rows[0]["UOMBasic"].ToString();
                       dr[4] = decimal.Parse(DS3.Tables[0].Rows[0]["Qty"].ToString()).ToString("N3");

                       double liQty = qty * this.BOMRecurQty(DS3.Tables[0].Rows[0]["WONo"].ToString(), Convert.ToInt32(DS3.Tables[0].Rows[0]["PId"]), Convert.ToInt32(DS3.Tables[0].Rows[0]["CId"]), 1, CompId, finId);

                       dr[5] = decimal.Parse(liQty.ToString()).ToString("N3");
                       dr[6] = DS3.Tables[0].Rows[0]["ItemId"].ToString();
                       dr[7] = DS3.Tables[0].Rows[0]["PId"].ToString();
                       dr[8] = DS3.Tables[0].Rows[0]["CId"].ToString();
                       dr[9] = DS3.Tables[0].Rows[0]["WONo"].ToString();
                       dr[10] = DS3.Tables[0].Rows[0]["Date"].ToString();
                       dr[11] = DS3.Tables[0].Rows[0]["Time"].ToString();
                       dr[12] = DS3.Tables[0].Rows[0]["Id"].ToString();

                       dt.Rows.Add(dr);
                       dt.AcceptChanges();
                   }

               }
           }

           GridView2.DataSource = dt;
           GridView2.DataBind();


       }
       catch (Exception ch)
       {
       }
       finally
       {
           con.Close();
       }

   }

   public void ItemHistory_TPL(string wono, string pid, string cid, int CompId, GridView GridView2, double qty,int FinId)
   {
       
       string connStr = this.Connection();
       SqlConnection con = new SqlConnection(connStr);
       try
       {

           List<int> li = new List<int>();
           li = this.TPLTree(wono, pid, cid, CompId,FinId);

           DataTable dt = new DataTable();
           dt.Columns.Add("AssemblyNo", typeof(string));
           dt.Columns.Add("ItemCode", typeof(string));
           dt.Columns.Add("ManfDesc", typeof(string));
           dt.Columns.Add("PurchDesc", typeof(string));
           dt.Columns.Add("UOMBasic", typeof(string));
           dt.Columns.Add("UnitQty", typeof(string));
           dt.Columns.Add("TPLQty", typeof(string));
           dt.Columns.Add("ItemId", typeof(int));
           dt.Columns.Add("PId", typeof(int));
           dt.Columns.Add("CId", typeof(int));
           dt.Columns.Add("WONo", typeof(String));
           dt.Columns.Add("UOMPurchase", typeof(string));
           dt.Columns.Add("Date", typeof(string));
           dt.Columns.Add("Time", typeof(string));
           dt.Columns.Add("Id", typeof(string));

           DataRow dr;

           for (int i = 0; i < li.Count; i++)
           {
               
                   string sql = this.select("tblDG_Item_Master.Id,tblDG_TPL_Master.WONo,tblDG_TPL_Master.PId,tblDG_TPL_Master.CId,tblDG_TPL_Master.ItemId,tblDG_Item_Master.ManfDesc,tblDG_Item_Master.PurchDesc,Unit_Master.Symbol As UOMBasic,tblDG_Item_Master.ItemCode,tblDG_TPL_Master.Qty,vw_Unit_Master.Symbol As UOMPurchase,REPLACE(CONVERT(varchar, CONVERT(datetime, SUBSTRING( tblDG_TPL_Master.SysDate , CHARINDEX('-',tblDG_TPL_Master.SysDate ) + 1, 2) + '-' + LEFT(tblDG_TPL_Master.SysDate,CHARINDEX('-',tblDG_TPL_Master.SysDate) - 1) + '-' + RIGHT(tblDG_TPL_Master.SysDate, CHARINDEX('-', REVERSE(tblDG_TPL_Master.SysDate)) - 1)), 103), '/', '-') AS  Date ,tblDG_TPL_Master.SysTime As Time", "vw_Unit_Master, tblDG_TPL_Master,tblDG_Category_Master,tblDG_Item_Master,Unit_Master", "tblDG_Item_Master.CId=tblDG_Category_Master.CId and Unit_Master.Id=tblDG_Item_Master.UOMBasic and tblDG_Item_Master.Id=tblDG_TPL_Master.ItemId and  tblDG_TPL_Master.Id='" + Convert.ToInt32(li[i]) + "' And tblDG_TPL_Master.CompId='" + CompId + "' AND tblDG_TPL_Master.FinYearId<='" + FinId + "'  AND  vw_Unit_Master.Id=tblDG_Item_Master.UOMPurchase");

                   SqlCommand cmd = new SqlCommand(sql, con);
                   SqlDataAdapter DA2 = new SqlDataAdapter(cmd);
                   DataSet DS3 = new DataSet();
                   DA2.Fill(DS3);

                   if (DS3.Tables[0].Rows.Count > 0)
                   {
                       dr = dt.NewRow();

                       string sql_P = this.select("tblDG_Item_Master.ItemCode", "tblDG_TPL_Master,tblDG_Item_Master", "tblDG_Item_Master.Id=tblDG_TPL_Master.ItemId AND tblDG_TPL_Master.CId='" + Convert.ToInt32(DS3.Tables[0].Rows[0]["PId"]) + "'And tblDG_Item_Master.CompId='" + CompId + "'AND tblDG_Item_Master.FinYearId<='" + FinId + "'");
                       DataSet DS_P = new DataSet();
                       SqlCommand cmd_P = new SqlCommand(sql_P, con);
                       SqlDataAdapter DA_P = new SqlDataAdapter(cmd_P);
                       DA_P.Fill(DS_P);

                       if (DS_P.Tables[0].Rows.Count > 0)
                       {
                           dr[0] = DS_P.Tables[0].Rows[0]["ItemCode"].ToString();
                       }
                       else
                       {
                           dr[0] = "NA";
                       }

                       dr[1] = DS3.Tables[0].Rows[0]["ItemCode"].ToString();
                       dr[2] = DS3.Tables[0].Rows[0]["ManfDesc"].ToString();
                       dr[3] = DS3.Tables[0].Rows[0]["PurchDesc"].ToString();
                       dr[4] = DS3.Tables[0].Rows[0]["UOMBasic"].ToString();
                       dr[5] = decimal.Parse(DS3.Tables[0].Rows[0]["Qty"].ToString()).ToString("N3");

                       double liQty = qty * this.TPLRecurQty(DS3.Tables[0].Rows[0]["WONo"].ToString(), Convert.ToInt32(DS3.Tables[0].Rows[0]["PId"]), Convert.ToInt32(DS3.Tables[0].Rows[0]["CId"]), 1, CompId, FinId);

                       dr[6] = decimal.Parse(liQty.ToString()).ToString("N3");
                       dr[7] = DS3.Tables[0].Rows[0]["ItemId"].ToString();
                       dr[8] = DS3.Tables[0].Rows[0]["PId"].ToString();
                       dr[9] = DS3.Tables[0].Rows[0]["CId"].ToString();
                       dr[10] = DS3.Tables[0].Rows[0]["WONo"].ToString();
                       dr[11] = DS3.Tables[0].Rows[0]["UOMPurchase"].ToString();
                       dr[12] = DS3.Tables[0].Rows[0]["Date"].ToString();
                       dr[13] = DS3.Tables[0].Rows[0]["Time"].ToString();
                       dr[14] = DS3.Tables[0].Rows[0]["Id"].ToString();
                       dt.Rows.Add(dr);
                       dt.AcceptChanges();
                   }
               
           }
           GridView2.DataSource = dt;
           GridView2.DataBind();
       }
       catch (Exception ch)
       {
       }
       finally
       {
           con.Close();
       }

   }

   public void StkAdjLog(int CompId, int FinYearId, string SessionId, int TransType, string TransNo, int ItemId, double Qty)
   {
       try
       {
           string connStr = this.Connection();
           SqlConnection con = new SqlConnection(connStr);
           SqlTransaction myTrans;

           string CDate = this.getCurrDate();
           string CTime = this.getCurrTime();

           string sqllog = this.select("LogNo", "tblInvQc_StockAdjLog", "CompId='" + CompId + "' AND FinYearId='" + FinYearId + "' Order by LogNo desc");
           SqlCommand cmdlog = new SqlCommand(sqllog, con);
           SqlDataAdapter dalog = new SqlDataAdapter(cmdlog);
           DataSet DSlog = new DataSet();
           dalog.Fill(DSlog, "tblInvQc_StockAdjLog");

           string LogNo = "";
           if (DSlog.Tables[0].Rows.Count > 0)
           {
               int Logtemp = Convert.ToInt32(DSlog.Tables[0].Rows[0][0].ToString()) + 1;
               LogNo = Logtemp.ToString("D4");
           }
           else
           {
               LogNo = "0001";
           }

           con.Open();
           SqlCommand cmd = new SqlCommand(this.insert("tblInvQc_StockAdjLog", "SysDate,SysTime,CompId,FinYearId,SessionId,LogNo,TransType,TransNo,ItemId,Qty", "'" + CDate + "','" + CTime + "','" + CompId + "','" + FinYearId + "','" + SessionId + "','" + LogNo + "','" + TransType + "','" + TransNo + "','" + ItemId + "','" + Qty + "'"), con);
           myTrans = con.BeginTransaction();
           cmd.Connection = con;
           cmd.Transaction = myTrans;
           cmd.ExecuteNonQuery();
           myTrans.Commit();
           con.Close();
       }
       catch (Exception et)
       {

       }
  
   }
  
   // Search GridView Bind Data for Cust Item Master Function ...

   public void BindDataCustIMaster(string tblName, string tblfield, string whr, GridView SearchGridView, string drpvalue, string hfSearchTextValue, string odr)
   {
       try
       {
           //hfSearchText has the search string returned from the grid.
           string strConnString = this.Connection();
           SqlConnection con = new SqlConnection(strConnString);
           DataSet DS = new DataSet();

           string sql = this.select(tblfield, tblName, whr);
           SqlCommand cmd = new SqlCommand(sql, con);
           SqlDataAdapter da = new SqlDataAdapter(cmd);
           da.Fill(DS, tblName);
           DS.Clear();
           if (hfSearchTextValue != "")
           {
               sql += " AND " + drpvalue + " ='" + hfSearchTextValue + "'" + odr + "";
           }
           else
           {
               sql += odr;
           }

           SqlCommand cmd1 = new SqlCommand(sql, con);
           SqlDataAdapter da1 = new SqlDataAdapter(cmd1);
           da1.Fill(DS, tblName);

           //this.EmptyGridFix(SearchGridView);
           SearchGridView.DataSource = DS;
           SearchGridView.DataBind();
       }
       catch (Exception exp) { }
      finally { }
   }

   // Search GridView Bind Data for All Search  ...

   public void BindData(string tblName, string tblfield, string whr, GridView SearchGridView, string hfSearchTextValue, string odr)
   {
       try
       {
           //hfSearchText has the search string returned from the grid.
           string strConnString = this.Connection();
           SqlConnection con = new SqlConnection(strConnString);
           DataSet DS = new DataSet();

           string sql = this.select(tblfield, tblName, whr);
           SqlCommand cmd = new SqlCommand(sql, con);
           SqlDataAdapter da = new SqlDataAdapter(cmd);
           da.Fill(DS, tblName);
           DS.Clear();

           if (hfSearchTextValue != "")
           {
               sql += "  like '" + hfSearchTextValue + "%'" + odr + "";
           }
           else
           {
               sql += odr;
           }

           SqlCommand cmd1 = new SqlCommand(sql, con);
           SqlDataAdapter da1 = new SqlDataAdapter(cmd1);
           da1.Fill(DS, tblName);

           //this.EmptyGridFix(SearchGridView);
           SearchGridView.DataSource = DS;
           SearchGridView.DataBind();
       }
       catch (Exception exp) { }
       finally { }
   }

        public void getBomnode(int node, string wonosrc, string wonodest, int compid, string sesid, int finyrid, int destpid, int destcid)
    {
        string connStr = this.Connection();
        SqlConnection con = new SqlConnection(connStr);
       try
        {
            DataSet DS = new DataSet();
            string currDate = this.getCurrDate();
            string currTime = this.getCurrTime();

            int CompId = compid;
            string SessionId = sesid;
            int FinYearId = finyrid;
            int pid = destpid;
            int cid = destcid;
            con.Open();
            string getparent2 = this.select("PId,CId,ItemId,Qty", "tblDG_BOM_Master", "CId=" + node + "And WONo='" + wonosrc + "'And CompId=" + CompId + "");
            SqlCommand checkparent2 = new SqlCommand(getparent2, con);
            SqlDataAdapter daparent2 = new SqlDataAdapter(checkparent2);
            DataSet dsparent2 = new DataSet();
            daparent2.Fill(dsparent2, "tblDG_BOM_Master"); 
            // Insert to TPL
            int ParentCid = this.getBOMCId(wonodest,CompId,FinYearId);
            if (dsparent2.Tables[0].Rows.Count>0)
            {               
                // Insert to BOM

                string InsertCidparentbom = this.insert("tblDG_BOM_Master", "SysDate,SysTime,CompId,FinYearId,SessionId,PId,CId,WONo,ItemId,Qty", "'" + currDate.ToString() + "','" + currTime.ToString() + "'," + CompId + "," + FinYearId + ",'" + SessionId.ToString() + "','" + cid + "','" + ParentCid + "','" + wonodest + "','" + Convert.ToInt32(dsparent2.Tables[0].Rows[0]["ItemId"]) + "','" + Convert.ToDouble(decimal.Parse(dsparent2.Tables[0].Rows[0]["Qty"].ToString()).ToString("N3")) + "'");
                SqlCommand cmdCidparentbom = new SqlCommand(InsertCidparentbom, con);
                cmdCidparentbom.ExecuteNonQuery();
            }
            string getparent = this.select("PId,CId,ItemId,Qty", "tblDG_BOM_Master", "PId=" + node + "And WONo='" + wonosrc + "'And CompId=" + CompId + "");
            SqlCommand checkparent = new SqlCommand(getparent, con);
            SqlDataAdapter daparent = new SqlDataAdapter(checkparent);
            DataSet dsparent = new DataSet();
            daparent.Fill(dsparent, "tblDG_BOM_Master");
        
            for (int h = 0; h < dsparent.Tables[0].Rows.Count; h++)
            {              

                int NextCid = this.getBOMCId(wonodest,CompId,FinYearId);               
                // Insert to BOM
                string Insertparentbom = this.insert("tblDG_BOM_Master", "SysDate,SysTime,CompId,FinYearId,SessionId,PId,CId,WONo,ItemId,Qty,Weldments,LH,RH", "'" + currDate.ToString() + "','" + currTime.ToString() + "','" + CompId + "','" + FinYearId + "','" + SessionId.ToString() + "','" + ParentCid + "','" + NextCid + "','" + wonodest + "','" + Convert.ToInt32(dsparent.Tables[0].Rows[h]["ItemId"]) + "','" + Convert.ToDouble(decimal.Parse(dsparent.Tables[0].Rows[h]["Qty"].ToString()).ToString("N3")) + "'");
                SqlCommand cmdCpyparentbom = new SqlCommand(Insertparentbom, con);
                cmdCpyparentbom.ExecuteNonQuery();               
                ///Updating UpdateWO Field 
                string sqlwo = this.update("SD_Cust_WorkOrder_Master", "UpdateWO='1'", "WONo='" + wonodest + "' And  CompId='" + CompId + "' ");
                SqlCommand cmdwo = new SqlCommand(sqlwo, con);
                cmdwo.ExecuteNonQuery(); 
                // Get Parent Child 

                DataSet DS2 = new DataSet();
                string cmdStr2 = this.select("PId,CId,ItemId,Qty", "tblDG_BOM_Master", "WONo='" + wonosrc + "'And CompId=" + CompId + " AND PId='" + dsparent.Tables[0].Rows[h]["CId"] + "'");
                SqlCommand cmd2 = new SqlCommand(cmdStr2, con);
                SqlDataAdapter da2 = new SqlDataAdapter(cmd2);
                da2.Fill(DS2);
                if (DS2.Tables[0].Rows.Count > 0)
                {
                    for (int j = 0; j < DS2.Tables[0].Rows.Count; j++)
                    {
                        cid = NextCid;
                        this.getBomnode(Convert.ToInt32(DS2.Tables[0].Rows[j]["CId"]), wonosrc, wonodest, compid, sesid, finyrid, destpid, destcid);
                    }
                }
            }

        }
       catch (Exception x)
        {
        }
       finally
        {
            con.Close();
        }

    }

        public void filldrp(DropDownList dl, int design, int id)
        {
            try
            {
                DataSet DS = new DataSet();
                string connStr = this.Connection();
                SqlConnection con = new SqlConnection(connStr);
                string cmdStr = "SELECT  tblHR_OfficeStaff.UserID,  tblHR_OfficeStaff.EmployeeName FROM tblHR_Designation INNER JOIN tblHR_OfficeStaff ON tblHR_Designation.Id = tblHR_OfficeStaff.Designation WHERE tblHR_OfficeStaff.Designation =tblHR_Designation.Id AND tblHR_OfficeStaff.Designation=" + design + " AND tblHR_OfficeStaff.UserID!=" + id + "";
                SqlCommand cmd = new SqlCommand(cmdStr, con);
                SqlDataAdapter DA = new SqlDataAdapter(cmd);
                DA.Fill(DS);
                dl.DataSource = DS.Tables[0];
                dl.DataTextField = "EmployeeName";
                dl.DataValueField = "UserID";
                dl.DataBind();
                //dl.Items.Insert(0, "Not Applicable");
            }
            catch (Exception ex)
            {

            }

        }
        
        
        public void depthead(DropDownList dl, int id)
        {
            try
            {
                DataSet DS = new DataSet();
                string connStr = this.Connection();
                SqlConnection con = new SqlConnection(connStr);
                string cmdStr = "SELECT  tblHR_OfficeStaff.UserID,  tblHR_OfficeStaff.EmployeeName FROM tblHR_Designation INNER JOIN tblHR_OfficeStaff ON tblHR_Designation.Id = tblHR_OfficeStaff.Designation WHERE tblHR_OfficeStaff.Designation =tblHR_Designation.Id  AND tblHR_OfficeStaff.UserID!=" + id + "";
                //AND tblHR_OfficeStaff.Designation in ('6','2','3')
                SqlCommand cmd = new SqlCommand(cmdStr, con);
                SqlDataAdapter DA = new SqlDataAdapter(cmd);
                DA.Fill(DS);
                dl.DataSource = DS.Tables[0];
                dl.DataTextField = "EmployeeName";
                dl.DataValueField = "UserID";
                dl.DataBind();
                //dl.Items.Insert(0, "Not Applicable");
            }
            catch (Exception ex)
            {

            }

        }

        public void filldrpDirector(DropDownList DrpDirectorName, int id)
        {
            try
            {
                DataSet DS = new DataSet();
                string connStr = this.Connection();
                SqlConnection con = new SqlConnection(connStr);
                string cmdStr = "SELECT tblHR_OfficeStaff.UserID, tblHR_OfficeStaff.EmployeeName FROM tblHR_Designation INNER JOIN tblHR_OfficeStaff ON tblHR_Designation.Id = tblHR_OfficeStaff.Designation WHERE (tblHR_OfficeStaff.Designation =tblHR_Designation.Id )AND (tblHR_OfficeStaff.Designation='2'OR tblHR_OfficeStaff.Designation='3') AND tblHR_OfficeStaff.UserId!=" + id + "";
                SqlCommand cmd = new SqlCommand(cmdStr, con);
                SqlDataAdapter DA = new SqlDataAdapter(cmd);
                DA.Fill(DS);
                DrpDirectorName.DataSource = DS.Tables[0];
                DrpDirectorName.DataTextField = "EmployeeName";
                DrpDirectorName.DataValueField = "UserID";
                DrpDirectorName.DataBind();
                
            }
            catch (Exception ex)
            {

            }

        }

        public void filldrp1(DropDownList dl, int design)
        {
            try
            {
                DataSet DS = new DataSet();
                string connStr = this.Connection();
                SqlConnection con = new SqlConnection(connStr);
                string cmdStr = "SELECT  tblHR_OfficeStaff.UserID,  tblHR_OfficeStaff.EmployeeName FROM tblHR_Designation INNER JOIN tblHR_OfficeStaff ON tblHR_Designation.Id = tblHR_OfficeStaff.Designation WHERE tblHR_OfficeStaff.Designation =tblHR_Designation.Id AND tblHR_OfficeStaff.Designation=" + design + " ";
                SqlCommand cmd = new SqlCommand(cmdStr, con);
                SqlDataAdapter DA = new SqlDataAdapter(cmd);
                DA.Fill(DS);
                dl.DataSource = DS.Tables[0];
                dl.DataTextField = "EmployeeName";
                dl.DataValueField = "UserID";
                dl.DataBind();
                dl.Items.Insert(0, "Not Applicable");
            }
            catch (Exception ex)
            {

            }

        }

        public void filldrpDirector1(DropDownList DrpDirectorName)
        {
            try
            {
                DataSet DS = new DataSet();
                string connStr = this.Connection();
                SqlConnection con = new SqlConnection(connStr);
                string cmdStr = "SELECT tblHR_OfficeStaff.UserID, tblHR_OfficeStaff.EmployeeName FROM tblHR_Designation INNER JOIN tblHR_OfficeStaff ON tblHR_Designation.Id = tblHR_OfficeStaff.Designation WHERE (tblHR_OfficeStaff.Designation =tblHR_Designation.Id )AND (tblHR_OfficeStaff.Designation='2'OR tblHR_OfficeStaff.Designation='3')";
                SqlCommand cmd = new SqlCommand(cmdStr, con);
                SqlDataAdapter DA = new SqlDataAdapter(cmd);
                DA.Fill(DS);
                DrpDirectorName.DataSource = DS.Tables[0];
                DrpDirectorName.DataTextField = "EmployeeName";
                DrpDirectorName.DataValueField = "UserID";
                DrpDirectorName.DataBind();
                DrpDirectorName.Items.Insert(0, "Not Applicable");
            }
            catch (Exception ex)
            {

            }

        }


    // getting Root node  Not To show in Item Master while adding componant in TPL or BOM

        List<int> RootAssmbly = new List<int>();
        public List<int> getTPLBOMRootnode(int node, string wonosrc, int compid, string sesid, int finyrid, string tblName)
        {
            string connStr = this.Connection();
            SqlConnection con = new SqlConnection(connStr);
            int Itemid = 0;
            try
            {
                DataSet DS = new DataSet();
                int CompId = compid;
                string SessionId = sesid;
                int FinYearId = finyrid;

                con.Open();
                string getparent = this.select("PId,CId,ItemId,Qty,Weldments,LH,RH", "" + tblName + "", "CId=" + node + "And WONo='" + wonosrc + "'And CompId=" + CompId + "");
                SqlCommand checkparent = new SqlCommand(getparent, con);
                SqlDataAdapter daparent = new SqlDataAdapter(checkparent);
                DataSet dsparent = new DataSet();
                daparent.Fill(dsparent);


                //// Get Parent Child 
                if (dsparent.Tables[0].Rows.Count > 0)
                {

                    Itemid = Convert.ToInt32(dsparent.Tables[0].Rows[0]["ItemId"]);
                    DataSet DS2 = new DataSet();
                    string cmdStr2 = this.select("PId,CId,ItemId,Qty,Weldments,LH,RH", "" + tblName + "", "WONo='" + wonosrc + "'And CompId=" + CompId + " AND CId='" + dsparent.Tables[0].Rows[0]["PId"] + "'");
                    SqlCommand cmd2 = new SqlCommand(cmdStr2, con);
                    SqlDataAdapter da2 = new SqlDataAdapter(cmd2);
                    da2.Fill(DS2);
                    if (DS2.Tables[0].Rows.Count > 0)
                    {
                        Itemid = Convert.ToInt32(DS2.Tables[0].Rows[0]["ItemId"]);
                        for (int j = 0; j < DS2.Tables[0].Rows.Count; j++)
                        {

                            this.getTPLBOMRootnode(Convert.ToInt32(DS2.Tables[0].Rows[j]["CId"]), wonosrc, compid, sesid, finyrid, tblName);
                        }
                        RootAssmbly.Add(Convert.ToInt32(Itemid));
                    }
                }

            }
            catch (Exception x)
            {
            }
            finally
            {
                con.Close();
            }

            return RootAssmbly;
        }


        List<Int32> listk = new List<Int32>();
        public List<Int32> CalBOMTreeQty(int CompId, string WONo, int Pid, int Cid)
        {
            string connStr = this.Connection();
            SqlConnection con = new SqlConnection(connStr);

            try
            {
                if (Pid > 0)
                {
                    DataSet DS = new DataSet();
                    SqlDataAdapter DA2 = new SqlDataAdapter("GetSchTime_BOM_PID_CIDWise", con);
                    DA2.SelectCommand.CommandType = CommandType.StoredProcedure;
                    DA2.SelectCommand.Parameters.Add(new SqlParameter("@CompId", SqlDbType.VarChar));
                    DA2.SelectCommand.Parameters["@CompId"].Value = CompId;
                    DA2.SelectCommand.Parameters.Add(new SqlParameter("@WONo", SqlDbType.VarChar));
                    DA2.SelectCommand.Parameters["@WONo"].Value = WONo;
                    DA2.SelectCommand.Parameters.Add(new SqlParameter("@PId", SqlDbType.VarChar));
                    DA2.SelectCommand.Parameters["@PId"].Value = Pid;
                    DA2.Fill(DS, "tblDG_BOM_Master");
                    listk.Add(Convert.ToInt32(DS.Tables[0].Rows[0]["PId"]));
                    listk.Add(Pid);
                    this.CalBOMTreeQty(CompId, WONo, Convert.ToInt32(DS.Tables[0].Rows[0]["PId"]), Cid);
                }
            }
            catch (Exception ex)
            {

            }
            return listk;
        }


        public void WIS_Material(string WONo2, string ItemId2, int CompId, int FinYearId, string sId, string CDate, string CTime)
        {

            string connStr = this.Connection();
            SqlConnection con = new SqlConnection(connStr);
            try
            {

                //New WIS No.
                con.Open();
                string WISno = "";
                SqlDataAdapter dawis = new SqlDataAdapter("GetSchTime_WISNo", con);
                dawis.SelectCommand.CommandType = CommandType.StoredProcedure;
                dawis.SelectCommand.Parameters.Add(new SqlParameter("@CompId", SqlDbType.VarChar));
                dawis.SelectCommand.Parameters["@CompId"].Value = CompId;
                dawis.SelectCommand.Parameters.Add(new SqlParameter("@FinYearId", SqlDbType.VarChar));
                dawis.SelectCommand.Parameters["@FinYearId"].Value = FinYearId;

                DataSet DSwis = new DataSet();
                dawis.Fill(DSwis);
                if (DSwis.Tables[0].Rows.Count > 0)
                {
                    int WISstr = Convert.ToInt32(DSwis.Tables[0].Rows[0]["WISNo"].ToString()) + 1;
                    WISno = WISstr.ToString("D4");
                }
                else
                {
                    WISno = "0001";
                }

                SqlDataAdapter adapter = new SqlDataAdapter("GQN_BOM_Details", con);
                adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand.Parameters.Add(new SqlParameter("@CompId", SqlDbType.VarChar));
                adapter.SelectCommand.Parameters["@CompId"].Value = CompId;
                adapter.SelectCommand.Parameters.Add(new SqlParameter("@WONo", SqlDbType.VarChar));
                adapter.SelectCommand.Parameters["@WONo"].Value = WONo2;
                adapter.SelectCommand.Parameters.Add(new SqlParameter("@ItemId", SqlDbType.VarChar));
                adapter.SelectCommand.Parameters["@ItemId"].Value = ItemId2;

                DataSet DS = new DataSet();
                adapter.Fill(DS, "tblDG_BOM_Master");
                double BalBomQty = 0;
                double BalQty = 0;//dr[12]
                int pq = 1;
                int Mid = 0;
                for (int p = 0; p < DS.Tables[0].Rows.Count; p++)
                {
                    DataSet DsIt = new DataSet();
                    SqlDataAdapter Dr = new SqlDataAdapter("GetSchTime_Item_Details", con);
                    Dr.SelectCommand.CommandType = CommandType.StoredProcedure;
                    Dr.SelectCommand.Parameters.Add(new SqlParameter("@CompId", SqlDbType.VarChar));
                    Dr.SelectCommand.Parameters["@CompId"].Value = CompId;
                    Dr.SelectCommand.Parameters.Add(new SqlParameter("@Id", SqlDbType.VarChar));
                    Dr.SelectCommand.Parameters["@Id"].Value = DS.Tables[0].Rows[p][0].ToString();
                    Dr.Fill(DsIt, "tblDG_Item_Master");
                    // Cal. BOM Qty

                    double h = 1;

                    List<double> g = new List<double>();

                    g = this.BOMTreeQty(WONo2, Convert.ToInt32(DS.Tables[0].Rows[p][2]), Convert.ToInt32(DS.Tables[0].Rows[p][3]));

                    for (int j = 0; j < g.Count; j++)
                    {
                        h = h * g[j];
                    }

                    //Cal. Total WIS Issued Qty
                    SqlDataAdapter TWISQtyDr = new SqlDataAdapter("GetSchTime_TWIS_Qty", con);
                    TWISQtyDr.SelectCommand.CommandType = CommandType.StoredProcedure;
                    TWISQtyDr.SelectCommand.Parameters.Add(new SqlParameter("@CompId", SqlDbType.VarChar));
                    TWISQtyDr.SelectCommand.Parameters["@CompId"].Value = CompId;
                    TWISQtyDr.SelectCommand.Parameters.Add(new SqlParameter("@WONo", SqlDbType.VarChar));
                    TWISQtyDr.SelectCommand.Parameters["@WONo"].Value = WONo2;
                    TWISQtyDr.SelectCommand.Parameters.Add(new SqlParameter("@ItemId", SqlDbType.VarChar));
                    TWISQtyDr.SelectCommand.Parameters["@ItemId"].Value = DS.Tables[0].Rows[p]["ItemId"].ToString();
                    TWISQtyDr.SelectCommand.Parameters.Add(new SqlParameter("@PId", SqlDbType.VarChar));
                    TWISQtyDr.SelectCommand.Parameters["@PId"].Value = DS.Tables[0].Rows[p]["PId"].ToString();
                    TWISQtyDr.SelectCommand.Parameters.Add(new SqlParameter("@CId", SqlDbType.VarChar));
                    TWISQtyDr.SelectCommand.Parameters["@CId"].Value = DS.Tables[0].Rows[p]["CId"].ToString();
                    DataSet TWISQtyDs = new DataSet();
                    TWISQtyDr.Fill(TWISQtyDs);
                    double TotWISQty = 0;
                    if (TWISQtyDs.Tables[0].Rows[0]["sum_IssuedQty"] != DBNull.Value && TWISQtyDs.Tables[0].Rows.Count > 0)
                    {
                        TotWISQty = Convert.ToDouble(decimal.Parse(TWISQtyDs.Tables[0].Rows[0]["sum_IssuedQty"].ToString()).ToString("N3"));
                    }

                    //Cal. Bal BOM Qty to Issue


                    if (h >= 0)
                    {
                        BalBomQty = Convert.ToDouble(decimal.Parse((h - TotWISQty).ToString()).ToString("N3"));

                    }

                    if (DS.Tables[0].Rows[p]["PId"].ToString() == "0")
                    {
                        BalQty = BalBomQty;
                    }

                    if (DS.Tables[0].Rows[p]["PId"].ToString() != "0") // Skip Root Assly.
                    {

                        //Cal. BOM Qty
                        List<Int32> d = new List<Int32>();
                        d = this.CalBOMTreeQty(CompId, WONo2, Convert.ToInt32(DS.Tables[0].Rows[p][2]), Convert.ToInt32(DS.Tables[0].Rows[p][3]));

                        int y = 0;
                        int getcid = 0;
                        int getpid = 0;

                        List<Int32> getcidpid = new List<Int32>();
                        List<Int32> getpidcid = new List<Int32>();

                        for (int j = d.Count; j > 0; j--)
                        {
                            if (d.Count > 2)// Retrieve CId,PId
                            {
                                getpidcid.Add(d[j - 1]);
                            }
                            else // Retrieve PId,CId
                            {
                                getcidpid.Add(d[y]);
                                y++;
                            }
                        }

                        double n = 1;
                        for (int w = 0; w < getcidpid.Count; w++) // Get group of 2 digit.
                        {
                            getpid = getcidpid[w++];
                            getcid = getcidpid[w];
                            SqlDataAdapter Dr3 = new SqlDataAdapter("GetSchTime_BOM_PCIDWise", con);
                            Dr3.SelectCommand.CommandType = CommandType.StoredProcedure;
                            Dr3.SelectCommand.Parameters.Add(new SqlParameter("@CompId", SqlDbType.VarChar));
                            Dr3.SelectCommand.Parameters["@CompId"].Value = CompId;
                            Dr3.SelectCommand.Parameters.Add(new SqlParameter("@WONo", SqlDbType.VarChar));
                            Dr3.SelectCommand.Parameters["@WONo"].Value = WONo2;
                            Dr3.SelectCommand.Parameters.Add(new SqlParameter("@PId", SqlDbType.VarChar));
                            Dr3.SelectCommand.Parameters["@PId"].Value = getpid;
                            Dr3.SelectCommand.Parameters.Add(new SqlParameter("@CId", SqlDbType.VarChar));
                            Dr3.SelectCommand.Parameters["@CId"].Value = getcid;
                            DataSet Ds3 = new DataSet();
                            Dr3.Fill(Ds3);
                            SqlDataAdapter TWISQtyDr4 = new SqlDataAdapter("GetSchTime_TWIS_Qty", con);
                            TWISQtyDr4.SelectCommand.CommandType = CommandType.StoredProcedure;
                            TWISQtyDr4.SelectCommand.Parameters.Add(new SqlParameter("@CompId", SqlDbType.VarChar));
                            TWISQtyDr4.SelectCommand.Parameters["@CompId"].Value = CompId;
                            TWISQtyDr4.SelectCommand.Parameters.Add(new SqlParameter("@WONo", SqlDbType.VarChar));
                            TWISQtyDr4.SelectCommand.Parameters["@WONo"].Value = WONo2;
                            TWISQtyDr4.SelectCommand.Parameters.Add(new SqlParameter("@ItemId", SqlDbType.VarChar));
                            TWISQtyDr4.SelectCommand.Parameters["@ItemId"].Value = Ds3.Tables[0].Rows[0]["ItemId"].ToString();
                            TWISQtyDr4.SelectCommand.Parameters.Add(new SqlParameter("@PId", SqlDbType.VarChar));
                            TWISQtyDr4.SelectCommand.Parameters["@PId"].Value = getpid;
                            TWISQtyDr4.SelectCommand.Parameters.Add(new SqlParameter("@CId", SqlDbType.VarChar));
                            TWISQtyDr4.SelectCommand.Parameters["@CId"].Value = getcid;
                            DataSet TWISQtyDs4 = new DataSet();
                            TWISQtyDr4.Fill(TWISQtyDs4);
                            double TotWISQty4 = 0;
                            if (TWISQtyDs4.Tables[0].Rows[0]["sum_IssuedQty"] != DBNull.Value && TWISQtyDs4.Tables[0].Rows.Count > 0)
                            {
                                TotWISQty4 = Convert.ToDouble(decimal.Parse(TWISQtyDs4.Tables[0].Rows[0]["sum_IssuedQty"].ToString()).ToString("N3"));
                            }

                            n = (n * Convert.ToDouble(decimal.Parse(Ds3.Tables[0].Rows[0]["Qty"].ToString()).ToString("N3"))) - TotWISQty4;

                        }

                        for (int w = 0; w < getpidcid.Count; w++) // Get group of 2 digit.
                        {

                            getcid = getpidcid[w++];
                            getpid = getpidcid[w];
                            double q = 1;
                            List<double> xy = new List<double>();

                            xy = this.BOMTreeQty(WONo2, getpid, getcid);

                            for (int f = 0; f < xy.Count; f++)
                            {
                                q = q * xy[f];
                            }

                            xy.Clear();
                            SqlDataAdapter Dr2 = new SqlDataAdapter("GetSchTime_BOM_PCIDWise", con);
                            Dr2.SelectCommand.CommandType = CommandType.StoredProcedure;
                            Dr2.SelectCommand.Parameters.Add(new SqlParameter("@CompId", SqlDbType.VarChar));
                            Dr2.SelectCommand.Parameters["@CompId"].Value = CompId;
                            Dr2.SelectCommand.Parameters.Add(new SqlParameter("@WONo", SqlDbType.VarChar));
                            Dr2.SelectCommand.Parameters["@WONo"].Value = WONo2;
                            Dr2.SelectCommand.Parameters.Add(new SqlParameter("@PId", SqlDbType.VarChar));
                            Dr2.SelectCommand.Parameters["@PId"].Value = getpid;
                            Dr2.SelectCommand.Parameters.Add(new SqlParameter("@CId", SqlDbType.VarChar));
                            Dr2.SelectCommand.Parameters["@CId"].Value = getcid;
                            DataSet Ds2 = new DataSet();
                            Dr2.Fill(Ds2, "tblDG_BOM_Master");
                            SqlDataAdapter TWISQtyDr3 = new SqlDataAdapter("GetSchTime_TWIS_Qty", con);
                            TWISQtyDr3.SelectCommand.CommandType = CommandType.StoredProcedure;
                            TWISQtyDr3.SelectCommand.Parameters.Add(new SqlParameter("@CompId", SqlDbType.VarChar));
                            TWISQtyDr3.SelectCommand.Parameters["@CompId"].Value = CompId;
                            TWISQtyDr3.SelectCommand.Parameters.Add(new SqlParameter("@WONo", SqlDbType.VarChar));
                            TWISQtyDr3.SelectCommand.Parameters["@WONo"].Value = WONo2;
                            TWISQtyDr3.SelectCommand.Parameters.Add(new SqlParameter("@ItemId", SqlDbType.VarChar));
                            TWISQtyDr3.SelectCommand.Parameters["@ItemId"].Value = Ds2.Tables[0].Rows[0]["ItemId"].ToString();
                            TWISQtyDr3.SelectCommand.Parameters.Add(new SqlParameter("@PId", SqlDbType.VarChar));
                            TWISQtyDr3.SelectCommand.Parameters["@PId"].Value = getpid;
                            TWISQtyDr3.SelectCommand.Parameters.Add(new SqlParameter("@CId", SqlDbType.VarChar));
                            TWISQtyDr3.SelectCommand.Parameters["@CId"].Value = getcid;
                            DataSet TWISQtyDs3 = new DataSet();
                            TWISQtyDr3.Fill(TWISQtyDs3);

                            double TotWISQty3 = 0;

                            if (TWISQtyDs3.Tables[0].Rows[0]["sum_IssuedQty"] != DBNull.Value && TWISQtyDs3.Tables[0].Rows.Count > 0)
                            {

                                TotWISQty3 = Convert.ToDouble(decimal.Parse(TWISQtyDs3.Tables[0].Rows[0]["sum_IssuedQty"].ToString()).ToString("N3"));
                            }
                            if (q >= 0)
                            {

                                n = (n * Convert.ToDouble(decimal.Parse(Ds2.Tables[0].Rows[0]["Qty"].ToString()).ToString("N3"))) - TotWISQty3;


                            }
                        }


                        if (n > 0)
                        {
                            BalQty = Convert.ToDouble(decimal.Parse(((n * Convert.ToDouble(DS.Tables[0].Rows[p][4])) - TotWISQty).ToString()).ToString("N3"));
                        }
                        else
                        {
                            BalQty = 0;
                        }

                        //Cal. Issue and Stock Qty.

                        double CalStockQty = 0;
                        double CalIssueQty = 0;

                        if (Convert.ToDouble(decimal.Parse(DsIt.Tables[0].Rows[0]["StockQty"].ToString()).ToString("N3")) >= 0 && Convert.ToDouble(decimal.Parse(BalQty.ToString()).ToString("N3")) >= 0)
                        {
                            if (Convert.ToDouble(decimal.Parse(DsIt.Tables[0].Rows[0]["StockQty"].ToString()).ToString("N3")) >= Convert.ToDouble(decimal.Parse(BalQty.ToString()).ToString("N3")))
                            {
                                CalStockQty = Convert.ToDouble(decimal.Parse(DsIt.Tables[0].Rows[0]["StockQty"].ToString()).ToString("N3")) - Convert.ToDouble(decimal.Parse(BalQty.ToString()).ToString("N3"));

                                CalIssueQty = Convert.ToDouble(decimal.Parse(BalQty.ToString()).ToString("N3"));

                            }
                            else if (Convert.ToDouble(decimal.Parse(BalQty.ToString()).ToString("N3")) >= Convert.ToDouble(decimal.Parse(DsIt.Tables[0].Rows[0]["StockQty"].ToString()).ToString("N3")))
                            {
                                CalStockQty = 0;
                                CalIssueQty = Convert.ToDouble(decimal.Parse(DsIt.Tables[0].Rows[0]["StockQty"].ToString()).ToString("N3"));

                            }
                        }

                        //WIS Details record

                        if (CalIssueQty > 0)
                        {
                            //WIS Master record
                            if (pq == 1)
                            {
                                string WISSql = this.insert("tblInv_WIS_Master", "SysDate,SysTime,CompId,SessionId,FinYearId,WISNo,WONo", "'" + CDate + "','" + CTime + "','" + CompId + "','" + sId + "','" + FinYearId + "','" + WISno + "','" + WONo2 + "'");
                                SqlCommand WIScmd = new SqlCommand(WISSql, con);
                                WIScmd.ExecuteNonQuery();

                                string StrMid = this.select1("Id", "tblInv_WIS_Master Order By Id Desc");
                                SqlCommand cmdStrMid = new SqlCommand(StrMid, con);
                                SqlDataAdapter DrStrMid = new SqlDataAdapter(cmdStrMid);
                                DataSet DsStrMid = new DataSet();
                                DrStrMid.Fill(DsStrMid, "tblDG_Item_Master");
                                if (DsStrMid.Tables[0].Rows.Count > 0)
                                {
                                    Mid = Convert.ToInt32(DsStrMid.Tables[0].Rows[0][0]);
                                    pq = 0;
                                }
                            }

                            string WISDetailSql = this.insert("tblInv_WIS_Details", "WISNo,PId,CId,ItemId,IssuedQty,MId", "'" + WISno + "','" + DS.Tables[0].Rows[p][2] + "','" + DS.Tables[0].Rows[p][3] + "','" + DS.Tables[0].Rows[p]["ItemId"].ToString() + "','" + CalIssueQty.ToString() + "','" + Mid + "'");
                            SqlCommand WISDetailcmd = new SqlCommand(WISDetailSql, con);
                            WISDetailcmd.ExecuteNonQuery();

                            //Stock Qty record                        
                            string StkQtySql = this.update("tblDG_Item_Master", "StockQty='" + CalStockQty.ToString() + "'", "CompId='" + CompId + "' AND Id='" + DS.Tables[0].Rows[p]["ItemId"].ToString() + "'");
                            SqlCommand StkQtycmd = new SqlCommand(StkQtySql, con);
                            StkQtycmd.ExecuteNonQuery();
                        }
                        n = 0;
                        d.Clear();
                    }
                    g.Clear();
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


}

