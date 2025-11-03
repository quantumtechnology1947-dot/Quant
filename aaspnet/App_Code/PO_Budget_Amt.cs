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
/// Summary description for PO_Budget_Amt
/// </summary>
public class PO_Budget_Amt
{
    clsFunctions fun = new clsFunctions();
    string connStr = "";
    SqlConnection con;
	public PO_Budget_Amt()
	{
        connStr = fun.Connection();
        con = new SqlConnection(connStr);
	}
    // Budget Calculation
    public double getTotal_PO_Budget_Amt(int compid, int accid, int prspr, int wodept, string wono, int dept, int BasicTax,int FinYearId)
    {
       
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
                string sqlPO = "SELECT tblMM_PO_Details.Qty, tblMM_PO_Details.Rate, tblMM_PO_Details.Discount, tblVAT_Master.Value As VAT,tblExciseser_Master.Value AS Excise, tblPacking_Master.Value AS PF FROM tblMM_PR_Details INNER JOIN tblMM_PR_Master ON tblMM_PR_Details.MId = tblMM_PR_Master.Id INNER JOIN tblMM_PO_Details INNER JOIN tblMM_PO_Master ON tblMM_PO_Details.MId = tblMM_PO_Master.Id ON tblMM_PR_Details.Id = tblMM_PO_Details.PRId INNER JOIN tblVAT_Master ON tblMM_PO_Details.VAT = tblVAT_Master.Id INNER JOIN tblExciseser_Master ON tblMM_PO_Details.ExST = tblExciseser_Master.Id INNER JOIN tblPacking_Master ON tblMM_PO_Details.PF = tblPacking_Master.Id And tblMM_PO_Master.FinYearId='" + FinYearId + "' And tblMM_PO_Master.CompId='" + compid + "' AND tblMM_PO_Details.BudgetCode='" + accid + "' AND tblMM_PO_Master.PONo=tblMM_PO_Details.PONo AND tblMM_PO_Master.PRSPRFlag='" + prspr + "'" + includeWODept + " ";
                SqlCommand cmd = new SqlCommand(sqlPO, con);
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {

                    if (BasicTax == 0)
                    {
                        Amt += fun.CalBasicAmt(Convert.ToDouble(decimal.Parse(rdr["Qty"].ToString()).ToString("N3")), Convert.ToDouble(decimal.Parse(rdr["Rate"].ToString()).ToString("N2")));

                    }

                    if (BasicTax == 1)
                    {
                        Amt += fun.CalDiscAmt(Convert.ToDouble(decimal.Parse(rdr["Qty"].ToString()).ToString("N3")), Convert.ToDouble(decimal.Parse(rdr["Rate"].ToString()).ToString("N2")), Convert.ToDouble(decimal.Parse(rdr["Discount"].ToString()).ToString("N2")));

                    }
                    if (BasicTax == 2)
                    {
                        double PF = Convert.ToDouble(decimal.Parse(rdr["PF"].ToString()).ToString("N3"));
                        double ExSer = Convert.ToDouble(decimal.Parse(rdr["Excise"].ToString()).ToString("N3"));
                        double Vat = Convert.ToDouble(decimal.Parse(rdr["VAT"].ToString()).ToString("N3"));
                        Amt += fun.CalTaxAmt(Convert.ToDouble(decimal.Parse(rdr["Qty"].ToString()).ToString("N3")), Convert.ToDouble(decimal.Parse(rdr["Rate"].ToString()).ToString("N2")), Convert.ToDouble(decimal.Parse(rdr["Discount"].ToString()).ToString("N2")), PF, ExSer, Vat);

                    }

                    if (BasicTax == 3)
                    {
                        double CalBasicAmt = fun.CalBasicAmt(Convert.ToDouble(decimal.Parse(rdr["Qty"].ToString()).ToString("N3")), Convert.ToDouble(decimal.Parse(rdr["Rate"].ToString()).ToString("N2")));
                        double CalOnlyTax = fun.CalDiscAmt(Convert.ToDouble(decimal.Parse(rdr["Qty"].ToString()).ToString("N3")), Convert.ToDouble(decimal.Parse(rdr["Rate"].ToString()).ToString("N2")), Convert.ToDouble(decimal.Parse(rdr["Discount"].ToString()).ToString("N2")));
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

                string sqlPO = "SELECT tblMM_PO_Details.Qty, tblMM_PO_Details.Rate, tblMM_PO_Details.Discount,  tblVAT_Master.Value As VAT,tblExciseser_Master.Value AS Excise, tblPacking_Master.Value AS PF FROM tblMM_SPR_Details INNER JOIN tblMM_SPR_Master ON tblMM_SPR_Details.MId = tblMM_SPR_Master.Id INNER JOIN tblMM_PO_Details INNER JOIN tblMM_PO_Master ON tblMM_PO_Details.MId = tblMM_PO_Master.Id ON tblMM_SPR_Details.Id = tblMM_PO_Details.SPRId INNER JOIN tblVAT_Master ON tblMM_PO_Details.VAT = tblVAT_Master.Id INNER JOIN tblExciseser_Master ON tblMM_PO_Details.ExST = tblExciseser_Master.Id INNER JOIN tblPacking_Master ON tblMM_PO_Details.PF = tblPacking_Master.Id And tblMM_PO_Master.CompId='" + compid + "' And tblMM_PO_Master.FinYearId ='" + FinYearId + "'  AND tblMM_PO_Master.PONo=tblMM_PO_Details.PONo AND tblMM_PO_Master.PRSPRFlag='" + prspr + "'" + includeWODept + "" + x + " ";
                SqlCommand cmd = new SqlCommand(sqlPO, con);
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {

                    if (BasicTax == 0)
                    {
                        Amt += fun.CalBasicAmt(Convert.ToDouble(decimal.Parse(rdr["Qty"].ToString()).ToString("N3")), Convert.ToDouble(decimal.Parse(rdr["Rate"].ToString()).ToString("N2")));

                    }

                    if (BasicTax == 1)
                    {
                        Amt += fun.CalDiscAmt(Convert.ToDouble(decimal.Parse(rdr["Qty"].ToString()).ToString("N3")), Convert.ToDouble(decimal.Parse(rdr["Rate"].ToString()).ToString("N2")), Convert.ToDouble(decimal.Parse(rdr["Discount"].ToString()).ToString("N2")));

                    }
                    if (BasicTax == 2)
                    {
                        double PF = Convert.ToDouble(decimal.Parse(rdr["PF"].ToString()).ToString("N3"));
                        double ExSer = Convert.ToDouble(decimal.Parse(rdr["Excise"].ToString()).ToString("N3"));
                        double Vat = Convert.ToDouble(decimal.Parse(rdr["VAT"].ToString()).ToString("N3"));
                        Amt += fun.CalTaxAmt(Convert.ToDouble(decimal.Parse(rdr["Qty"].ToString()).ToString("N3")), Convert.ToDouble(decimal.Parse(rdr["Rate"].ToString()).ToString("N2")), Convert.ToDouble(decimal.Parse(rdr["Discount"].ToString()).ToString("N2")), PF, ExSer, Vat);

                    }

                    if (BasicTax == 3)
                    {
                        double CalBasicAmt = fun.CalBasicAmt(Convert.ToDouble(decimal.Parse(rdr["Qty"].ToString()).ToString("N3")), Convert.ToDouble(decimal.Parse(rdr["Rate"].ToString()).ToString("N2")));
                        double CalOnlyTax = fun.CalDiscAmt(Convert.ToDouble(decimal.Parse(rdr["Qty"].ToString()).ToString("N3")), Convert.ToDouble(decimal.Parse(rdr["Rate"].ToString()).ToString("N2")), Convert.ToDouble(decimal.Parse(rdr["Discount"].ToString()).ToString("N2")));

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




}
