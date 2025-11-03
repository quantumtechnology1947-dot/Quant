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
using System.Text.RegularExpressions;

/// <summary>
/// Summary description for TotalTdsDeduct
/// </summary>
public class TotalTdsDeduct
{
    clsFunctions fun = new clsFunctions();    
    SqlConnection con;
    string connStr = "";   
	public TotalTdsDeduct()
	{
        connStr = fun.Connection();
        con = new SqlConnection(connStr);		
	}
    public double Check_TDSAmt(int CompId, int FinYearId, string GetSupCode)
    {
        double TDSAmt = 0; 
        try
        {             
           
            con.Open();
            DataTable dt = new DataTable();           
            string StrSql = string.Empty;
            StrSql = "select (Case When GQNId !=0 then (Select Sum(tblQc_MaterialQuality_Details.AcceptedQty) from tblQc_MaterialQuality_Details where tblQc_MaterialQuality_Details.Id=tblACC_BillBooking_Details.GQNId)*(tblMM_PO_Details.Rate - (tblMM_PO_Details.Rate * tblMM_PO_Details.Discount) / 100) Else (Select Sum(tblinv_MaterialServiceNote_Details.ReceivedQty) As AcceptedQty from tblinv_MaterialServiceNote_Details where tblinv_MaterialServiceNote_Details.Id=tblACC_BillBooking_Details.GSNId)*(tblMM_PO_Details.Rate - (tblMM_PO_Details.Rate * tblMM_PO_Details.Discount) / 100)End) +PFAmt+ExStBasic+ExStEducess+ExStShecess+tblACC_BillBooking_Details.VAT+CST+tblACC_BillBooking_Details.Freight+tblACC_BillBooking_Details.BCDValue+tblACC_BillBooking_Details.EdCessOnCDValue+tblACC_BillBooking_Details.SHEDCessValue As TotalBookedBill,tblACC_BillBooking_Master.Discount,tblACC_BillBooking_Master.DiscountType,tblACC_BillBooking_Master.DebitAmt,tblACC_BillBooking_Master.OtherCharges,tblACC_BillBooking_Master.TDSCode,tblACC_BillBooking_Master.Id as PVEVId from tblACC_BillBooking_Master,tblACC_BillBooking_Details,tblMM_PO_Details,tblMM_PO_Master where tblACC_BillBooking_Master.CompId='" + CompId + "' And tblACC_BillBooking_Master.SupplierId='" + GetSupCode + "' And tblACC_BillBooking_Master.Id=tblACC_BillBooking_Details.MId AND tblACC_BillBooking_Master.FinYearId='" + FinYearId + "'  And tblMM_PO_Details.Id=tblACC_BillBooking_Details.PODId AND tblMM_PO_Master.Id=tblMM_PO_Details.MId";
            SqlCommand cmdSql = new SqlCommand(StrSql, con);
            SqlDataReader sqldr = cmdSql.ExecuteReader();
            dt.Load(sqldr);           
            var linq = from x in dt.AsEnumerable()
                       group x by new
                       {
                           y = x.Field<int>("PVEVId")
                       } into grp
                       let row1 = grp.First()
                       select new
                       {                          
                           Discount = row1.Field<double>("Discount"),
                           DiscountType = row1.Field<int>("DiscountType"),
                           DebitAmt = row1.Field<double>("DebitAmt"),
                           OtherCharges = row1.Field<double>("OtherCharges"),
                           TotalBookedBill = grp.Sum(r => r.Field<Double?>("TotalBookedBill")),                           
                           TDSCode = row1.Field<int>("TDSCode")
                       };
           
            double x1 = 0;
           
            string sqlPanNo = fun.select("PanNo", "tblMM_Supplier_master", "SupplierId='" + GetSupCode + "'");
            SqlCommand cmdPanNo = new SqlCommand(sqlPanNo, con);
            SqlDataReader DSPanNo = cmdPanNo.ExecuteReader();
            DSPanNo.Read();
            var regexItem = new Regex("^[a-zA-Z0-9 ]*$");       

            foreach (var d in linq)
            {
                double letCal = 0;
                double CalCulatedAmt = 0;
                letCal = Convert.ToDouble(d.TotalBookedBill) + d.OtherCharges;
                if (d.DiscountType == 0)
                {
                    letCal = letCal - d.Discount;
                }
                else if (d.DiscountType == 1)
                {
                    letCal = letCal - (letCal * d.Discount / 100);
                }
                CalCulatedAmt = letCal - d.DebitAmt;
                x1+=CalCulatedAmt;
                //check limit for  
                string SqlTDSCode = fun.select("PaymentRange,Others,WithOutPAN", "tblACC_TDSCode_Master", "Id='" + d.TDSCode + "'");
                SqlCommand cmdTDSCode = new SqlCommand(SqlTDSCode, con);
                SqlDataReader DSSqlTDSCode = cmdTDSCode.ExecuteReader();
                DSSqlTDSCode.Read();
                if (DSSqlTDSCode.HasRows)
                {
                    if (x1 >= Convert.ToDouble(DSSqlTDSCode["PaymentRange"]))
                    {
                        //string sqlPanNo = fun.select("PanNo", "tblMM_Supplier_master", "SupplierId='" + GetSupCode + "'");
                        //SqlCommand cmdPanNo = new SqlCommand(sqlPanNo, con);
                        //SqlDataReader DSPanNo = cmdPanNo.ExecuteReader();
                        //DSPanNo.Read();
                        //var regexItem = new Regex("^[a-zA-Z0-9 ]*$");
                        int z = 0;
                        if (regexItem.IsMatch(DSPanNo["PANNo"].ToString()))
                        {
                            z = Convert.ToInt32(DSSqlTDSCode["Others"]);
                        }
                        else
                        {
                            z = Convert.ToInt32(DSSqlTDSCode["WithOutPAN"]);
                        }
                        TDSAmt += CalCulatedAmt * z / 100;
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
        return Math.Round(TDSAmt, 2);
       
    }
 }
