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
/// Summary description for CalBalBudgetAmt
/// </summary>
public class CalBalBudgetAmt
{
    clsFunctions fun = new clsFunctions();
    PO_Budget_Amt PBM = new PO_Budget_Amt();
    string connStr = "";
    SqlConnection con;    
	public CalBalBudgetAmt()
	{
		//
		// TODO: Add constructor logic here
		//

        connStr = fun.Connection();
        con = new SqlConnection(connStr);
	}
    // To Get Balance Budget of Bussiness Group
    public double TotBalBudget_BG(int BGId,int CompId,int FinYearId,int Flag)
    {
        double BalAmt = 0;       
        try
        {
            con.Open();
            double budget = 0;
            double POSPRBasicDiscAmt = 0;
            double totalCash = 0;
            double totalCashRec = 0;
            double POSPRTaxAmt = 0;

            string selectBudget = "select Sum(Amount) As Budget from tblACC_Budget_Dept where BGId='" + BGId + "' And CompId="+CompId+" And FinYearId<="+FinYearId+" group by  BGId ";
            SqlCommand cmdBD = new SqlCommand(selectBudget, con);
            SqlDataReader rdr = cmdBD.ExecuteReader();
            rdr.Read();
            if (rdr.HasRows)
            {
                 budget = Math.Round(Convert.ToDouble(rdr["Budget"].ToString()), 2);
            }

            if(Flag==0)
            {
              POSPRBasicDiscAmt = PBM.getTotal_PO_Budget_Amt(CompId, BGId, 1, 1, "0", BGId, 1,FinYearId);
              POSPRTaxAmt = PBM.getTotal_PO_Budget_Amt(CompId, BGId, 1, 1, "0", BGId, 2,FinYearId);
            }
            else{

            POSPRBasicDiscAmt = fun.getTotal_PO_Budget_Amt(CompId, BGId, 1, 1, "0", BGId, 1);
            POSPRTaxAmt = fun.getTotal_PO_Budget_Amt(CompId, BGId, 1, 1, "0", BGId, 2);
            }
            string CashAmt = "SELECT  SUM(tblACC_CashVoucher_Payment_Details.Amount) AS CashAmt, tblACC_CashVoucher_Payment_Details.BGGroup FROM tblACC_CashVoucher_Payment_Details INNER JOIN     tblACC_CashVoucher_Payment_Master ON tblACC_CashVoucher_Payment_Details.MId = tblACC_CashVoucher_Payment_Master.Id   And tblACC_CashVoucher_Payment_Details.BGGroup='" + BGId + "' And tblACC_CashVoucher_Payment_Master.CompId=" + CompId + " And tblACC_CashVoucher_Payment_Master.FinYearId<=" + FinYearId + " GROUP BY tblACC_CashVoucher_Payment_Details.BGGroup";

            SqlCommand cmdCash = new SqlCommand(CashAmt, con);
            SqlDataReader rdr2 = cmdCash.ExecuteReader();
            rdr2.Read();
            if (rdr2.HasRows)
            {
                totalCash = Convert.ToDouble(rdr2[0]);               
            }
            string CashAmt1 = "SELECT  SUM(tblACC_CashVoucher_Receipt_Master.Amount) AS CashAmt, tblACC_CashVoucher_Receipt_Master.BGGroup,  tblACC_CashVoucher_Receipt_Master.WONo FROM tblACC_CashVoucher_Receipt_Master   where  tblACC_CashVoucher_Receipt_Master.BGGroup='" + BGId + "' And CompId=" + CompId + " And FinYearId<=" + FinYearId + " GROUP BY tblACC_CashVoucher_Receipt_Master.BGGroup, tblACC_CashVoucher_Receipt_Master.WONo";
            SqlCommand cmdCash1 = new SqlCommand(CashAmt1, con);
            SqlDataReader rdr3 = cmdCash1.ExecuteReader();
            rdr3.Read();
            if (rdr3.HasRows)
            {
                totalCashRec = Convert.ToDouble(rdr3[0]);               
            }

            BalAmt = Math.Round(budget - (POSPRBasicDiscAmt + POSPRTaxAmt + totalCash), 2) + totalCashRec;

        }
        catch (Exception ex) { }
        finally
        {
            con.Close();
        }
        return BalAmt;
    }
    // To Get Balance Budget of WONo flag=1 when cash voucher
    public double TotBalBudget_WONO(int AccId, int CompId, int FinYearId,string WoNo, int Flag )
    {
        double BalAmt = 0;        
        try
        {
            con.Open();
            double budget = 0;
            double POSPRBasicDiscAmt = 0;
            double POPRBasicDiscAmt = 0;   
            double totalCash = 0;
            double totalCashRec = 0;
            double POSPRTaxAmt = 0;
            double POPRTaxAmt = 0; 
            string selectBudget = "select Sum(Amount) As Budget from tblACC_Budget_WO where BudgetCodeId='" + AccId + "'   and  WONo='" + WoNo + "' And CompId=" + CompId + " And FinYearId<=" + FinYearId + "  group by  BudgetCodeId ";
            SqlCommand cmdBD = new SqlCommand(selectBudget, con);
            SqlDataReader rdr = cmdBD.ExecuteReader();
            rdr.Read();
            if (rdr.HasRows)
            {
                budget = Math.Round(Convert.ToDouble(rdr["Budget"].ToString()), 2);
            }
            if (Flag == 0)
            {
                POPRBasicDiscAmt = PBM.getTotal_PO_Budget_Amt(CompId, AccId, 0, 1, WoNo, 0, 1,FinYearId);
                POSPRBasicDiscAmt = PBM.getTotal_PO_Budget_Amt(CompId, AccId, 1, 1, WoNo, 0, 1, FinYearId);
                POPRTaxAmt = PBM.getTotal_PO_Budget_Amt(CompId, AccId, 0, 1, WoNo, 0, 2, FinYearId);
                POSPRTaxAmt = PBM.getTotal_PO_Budget_Amt(CompId, AccId, 1, 1, WoNo, 0, 2, FinYearId);

            }
            else
            {
            POPRBasicDiscAmt = fun.getTotal_PO_Budget_Amt(CompId, AccId, 0, 1, WoNo, 0, 1);
            POSPRBasicDiscAmt = fun.getTotal_PO_Budget_Amt(CompId, AccId, 1, 1, WoNo, 0, 1);
            POPRTaxAmt = fun.getTotal_PO_Budget_Amt(CompId, AccId, 0, 1, WoNo, 0, 2);
            POSPRTaxAmt = fun.getTotal_PO_Budget_Amt(CompId, AccId, 1, 1, WoNo, 0, 2);
            }

            string CashAmt = "SELECT  SUM(tblACC_CashVoucher_Payment_Details.Amount) AS CashAmt  ,tblACC_CashVoucher_Payment_Details.WONo FROM tblACC_CashVoucher_Payment_Details INNER JOIN     tblACC_CashVoucher_Payment_Master ON tblACC_CashVoucher_Payment_Details.MId = tblACC_CashVoucher_Payment_Master.Id   And tblACC_CashVoucher_Payment_Details.WONo='" + WoNo + "' And tblACC_CashVoucher_Payment_Master.FinYearId <=" + FinYearId + "  And tblACC_CashVoucher_Payment_Details.BudgetCode='" + AccId + "' GROUP BY tblACC_CashVoucher_Payment_Details.BudgetCode, tblACC_CashVoucher_Payment_Details.WONo";
            SqlCommand cmdCash = new SqlCommand(CashAmt, con);          
            SqlDataReader rdr2 = cmdCash.ExecuteReader();
            rdr2.Read();
            if (rdr2.HasRows)
            {
                totalCash = Convert.ToDouble(rdr2[0]);               
            }

            string CashAmt1 = "SELECT  SUM(tblACC_CashVoucher_Receipt_Master.Amount) AS CashAmt,   tblACC_CashVoucher_Receipt_Master.WONo FROM tblACC_CashVoucher_Receipt_Master   where  tblACC_CashVoucher_Receipt_Master.WONo='" + WoNo + "' And tblACC_CashVoucher_Receipt_Master.FinYearId <=" + FinYearId + "  And     tblACC_CashVoucher_Receipt_Master.BudgetCode='" + AccId + "' GROUP BY tblACC_CashVoucher_Receipt_Master.BudgetCode, tblACC_CashVoucher_Receipt_Master.WONo";
            SqlCommand cmdCash1 = new SqlCommand(CashAmt1, con);
            SqlDataReader rdr3 = cmdCash1.ExecuteReader();
            rdr3.Read();
            if (rdr3.HasRows)
            {
                totalCashRec = Convert.ToDouble(rdr3[0]);
               
            }
            BalAmt = Math.Round(budget - (Math.Round(POPRBasicDiscAmt + POSPRBasicDiscAmt, 2) + Math.Round(POPRTaxAmt + POSPRTaxAmt, 2) + Math.Round(totalCash, 2)), 2) + Math.Round(totalCashRec, 2);

        }
        catch (Exception ex) { }
        finally
        {
            con.Close();
        }
        return BalAmt;
    }

    //public double TotBalBudget_WONO_1(int AccId, int CompId, int FinYearId, string WoNo, int Flag)
    //{
    //    double BalAmt = 0;
    //    try
    //    {


    //        double budget = 0;
    //        double POSPRBasicDiscAmt = 0;
    //        double POPRBasicDiscAmt = 0;
    //        double totalCash = 0;
    //        double totalCashRec = 0;
    //        double POSPRTaxAmt = 0;
    //        double POPRTaxAmt = 0;
    //        int prevYear = 0;
    //        prevYear = (FinYearId - 1);
    //        double openingBalOfPrevYear = 0;
    //        openingBalOfPrevYear = this.TotBalBudget_WONO(AccId, CompId, prevYear, WoNo, 0);
    //        con.Open();
    //        string selectBudget = "select Sum(Amount) As Budget from tblACC_Budget_WO where BudgetCodeId='" + AccId + "'   and  WONo='" + WoNo + "' And CompId=" + CompId + " And FinYearId=" + FinYearId + "  group by  BudgetCodeId ";
    //        SqlCommand cmdBD = new SqlCommand(selectBudget, con);
    //        SqlDataReader rdr = cmdBD.ExecuteReader();
    //        rdr.Read();
    //        if (rdr.HasRows)
    //        {
    //            budget = Math.Round((Convert.ToDouble(rdr["Budget"]) + openingBalOfPrevYear), 2);
    //        }
    //        else
    //        {
    //            budget = openingBalOfPrevYear;
    //        }
    //        if (Flag == 0)
    //        {
    //            POPRBasicDiscAmt = PBM.getTotal_PO_Budget_Amt(CompId, AccId, 0, 1, WoNo, 0, 1, FinYearId);
    //            POSPRBasicDiscAmt = PBM.getTotal_PO_Budget_Amt(CompId, AccId, 1, 1, WoNo, 0, 1, FinYearId);
    //            POPRTaxAmt = PBM.getTotal_PO_Budget_Amt(CompId, AccId, 0, 1, WoNo, 0, 2, FinYearId);
    //            POSPRTaxAmt = PBM.getTotal_PO_Budget_Amt(CompId, AccId, 1, 1, WoNo, 0, 2, FinYearId);

    //        }
    //        else
    //        {
    //            POPRBasicDiscAmt = fun.getTotal_PO_Budget_Amt(CompId, AccId, 0, 1, WoNo, 0, 1);
    //            POSPRBasicDiscAmt = fun.getTotal_PO_Budget_Amt(CompId, AccId, 1, 1, WoNo, 0, 1);
    //            POPRTaxAmt = fun.getTotal_PO_Budget_Amt(CompId, AccId, 0, 1, WoNo, 0, 2);
    //            POSPRTaxAmt = fun.getTotal_PO_Budget_Amt(CompId, AccId, 1, 1, WoNo, 0, 2);
    //        }

    //        string CashAmt = "SELECT  SUM(tblACC_CashVoucher_Payment_Details.Amount) AS CashAmt  ,tblACC_CashVoucher_Payment_Details.WONo FROM tblACC_CashVoucher_Payment_Details INNER JOIN     tblACC_CashVoucher_Payment_Master ON tblACC_CashVoucher_Payment_Details.MId = tblACC_CashVoucher_Payment_Master.Id   And tblACC_CashVoucher_Payment_Details.WONo='" + WoNo + "' And tblACC_CashVoucher_Payment_Master.FinYearId =" + FinYearId + "  And tblACC_CashVoucher_Payment_Details.BudgetCode='" + AccId + "' GROUP BY tblACC_CashVoucher_Payment_Details.BudgetCode, tblACC_CashVoucher_Payment_Details.WONo";
    //        SqlCommand cmdCash = new SqlCommand(CashAmt, con);
    //        SqlDataReader rdr2 = cmdCash.ExecuteReader();
    //        rdr2.Read();
    //        if (rdr2.HasRows)
    //        {
    //            totalCash = Convert.ToDouble(rdr2[0]);
    //        }

    //        string CashAmt1 = "SELECT  SUM(tblACC_CashVoucher_Receipt_Master.Amount) AS CashAmt,   tblACC_CashVoucher_Receipt_Master.WONo FROM tblACC_CashVoucher_Receipt_Master   where  tblACC_CashVoucher_Receipt_Master.WONo='" + WoNo + "' And tblACC_CashVoucher_Receipt_Master.FinYearId =" + FinYearId + "  And     tblACC_CashVoucher_Receipt_Master.BudgetCode='" + AccId + "' GROUP BY tblACC_CashVoucher_Receipt_Master.BudgetCode, tblACC_CashVoucher_Receipt_Master.WONo";
    //        SqlCommand cmdCash1 = new SqlCommand(CashAmt1, con);
    //        SqlDataReader rdr3 = cmdCash1.ExecuteReader();
    //        rdr3.Read();
    //        if (rdr3.HasRows)
    //        {
    //            totalCashRec = Convert.ToDouble(rdr3[0]);

    //        }
    //        BalAmt = Math.Round(budget - (Math.Round(POPRBasicDiscAmt + POSPRBasicDiscAmt, 2) + Math.Round(POPRTaxAmt + POSPRTaxAmt, 2) + Math.Round(totalCash, 2)), 2) + Math.Round(totalCashRec, 2);



    //    }
    //    catch (Exception ex) { }
    //    finally
    //    {
    //        con.Close();
    //    }
    //    return BalAmt;
    //}

    //--------------------------------------------------------------------------------------------

    public DataTable TotBudget_WONO_1(int AccId, int CompId, int FinYearId, string WoNo, int Flag)
    {

        DataTable dt = new DataTable();

        try
        {


            double budget = 0;
            double BalAmt = 0;
            double POSPRBasicDiscAmt = 0;
            double POPRBasicDiscAmt = 0;
            double totalCash = 0;
            double totalCashRec = 0;
            double POSPRTaxAmt = 0;
            double POPRTaxAmt = 0;
            int prevYear = 0;
            prevYear = (FinYearId - 1);
            double openingBalOfPrevYear = 0;
            openingBalOfPrevYear = this.TotBalBudget_WONO(AccId, CompId, prevYear, WoNo, 0);
            con.Open();
            string selectBudget = "select Sum(Amount) As Budget from tblACC_Budget_WO where BudgetCodeId='" + AccId + "'   and  WONo='" + WoNo + "' And CompId=" + CompId + " And FinYearId=" + FinYearId + "  group by  BudgetCodeId ";
            SqlCommand cmdBD = new SqlCommand(selectBudget, con);
            SqlDataReader rdr = cmdBD.ExecuteReader();
            rdr.Read();
            if (rdr.HasRows)
            {
                budget = Math.Round((Convert.ToDouble(rdr["Budget"]) + openingBalOfPrevYear), 2);
            }
            else
            {
                budget = openingBalOfPrevYear;
            }
            if (Flag == 0)
            {
                POPRBasicDiscAmt = PBM.getTotal_PO_Budget_Amt(CompId, AccId, 0, 1, WoNo, 0, 1, FinYearId);
                POSPRBasicDiscAmt = PBM.getTotal_PO_Budget_Amt(CompId, AccId, 1, 1, WoNo, 0, 1, FinYearId);
                POPRTaxAmt = PBM.getTotal_PO_Budget_Amt(CompId, AccId, 0, 1, WoNo, 0, 2, FinYearId);
                POSPRTaxAmt = PBM.getTotal_PO_Budget_Amt(CompId, AccId, 1, 1, WoNo, 0, 2, FinYearId);

            }
            else
            {
                POPRBasicDiscAmt = fun.getTotal_PO_Budget_Amt(CompId, AccId, 0, 1, WoNo, 0, 1);
                POSPRBasicDiscAmt = fun.getTotal_PO_Budget_Amt(CompId, AccId, 1, 1, WoNo, 0, 1);
                POPRTaxAmt = fun.getTotal_PO_Budget_Amt(CompId, AccId, 0, 1, WoNo, 0, 2);
                POSPRTaxAmt = fun.getTotal_PO_Budget_Amt(CompId, AccId, 1, 1, WoNo, 0, 2);
            }

            string CashAmt = "SELECT  SUM(tblACC_CashVoucher_Payment_Details.Amount) AS CashAmt  ,tblACC_CashVoucher_Payment_Details.WONo FROM tblACC_CashVoucher_Payment_Details INNER JOIN     tblACC_CashVoucher_Payment_Master ON tblACC_CashVoucher_Payment_Details.MId = tblACC_CashVoucher_Payment_Master.Id   And tblACC_CashVoucher_Payment_Details.WONo='" + WoNo + "' And tblACC_CashVoucher_Payment_Master.FinYearId =" + FinYearId + "  And tblACC_CashVoucher_Payment_Details.BudgetCode='" + AccId + "' GROUP BY tblACC_CashVoucher_Payment_Details.BudgetCode, tblACC_CashVoucher_Payment_Details.WONo";
            SqlCommand cmdCash = new SqlCommand(CashAmt, con);
            SqlDataReader rdr2 = cmdCash.ExecuteReader();
            rdr2.Read();
            if (rdr2.HasRows)
            {
                totalCash = Convert.ToDouble(rdr2[0]);
            }

            string CashAmt1 = "SELECT  SUM(tblACC_CashVoucher_Receipt_Master.Amount) AS CashAmt,   tblACC_CashVoucher_Receipt_Master.WONo FROM tblACC_CashVoucher_Receipt_Master   where  tblACC_CashVoucher_Receipt_Master.WONo='" + WoNo + "' And tblACC_CashVoucher_Receipt_Master.FinYearId =" + FinYearId + "  And     tblACC_CashVoucher_Receipt_Master.BudgetCode='" + AccId + "' GROUP BY tblACC_CashVoucher_Receipt_Master.BudgetCode, tblACC_CashVoucher_Receipt_Master.WONo";
            SqlCommand cmdCash1 = new SqlCommand(CashAmt1, con);
            SqlDataReader rdr3 = cmdCash1.ExecuteReader();
            rdr3.Read();
            if (rdr3.HasRows)
            {
                totalCashRec = Convert.ToDouble(rdr3[0]);
            }

            BalAmt = Math.Round(budget - (Math.Round(POPRBasicDiscAmt + POSPRBasicDiscAmt, 2) + Math.Round(POPRTaxAmt + POSPRTaxAmt, 2) + Math.Round(totalCash, 2)), 2) + Math.Round(totalCashRec, 2);
            DataRow dr;
            dt.Columns.Add(new System.Data.DataColumn("WONo", typeof(string)));
            dt.Columns.Add(new System.Data.DataColumn("POBasicAmt", typeof(double)));
            dt.Columns.Add(new System.Data.DataColumn("POTaxAmt", typeof(double)));
            dt.Columns.Add(new System.Data.DataColumn("POTotalAmt", typeof(double)));
            dt.Columns.Add(new System.Data.DataColumn("TotBudgetAssined", typeof(double)));
            dt.Columns.Add(new System.Data.DataColumn("BalBudget", typeof(double)));
            dr = dt.NewRow();
            dr[0] = WoNo;
            dr[1] = Math.Round(POPRBasicDiscAmt + POSPRBasicDiscAmt, 2);
            dr[2] = Math.Round(POPRTaxAmt + POSPRTaxAmt, 2);
            dr[3] = Math.Round(POPRBasicDiscAmt + POSPRBasicDiscAmt + POPRTaxAmt + POSPRTaxAmt, 2);
            dr[4] = budget;
            dr[5] = BalAmt;
            dt.Rows.Add(dr);
            dt.AcceptChanges();

        }
        catch (Exception ex) { }
        finally
        {
            con.Close();
        }
        return dt;

    }

   

}
