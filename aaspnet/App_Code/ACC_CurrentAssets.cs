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
/// Summary description for ACC_CurrentAssets
/// </summary>
/// 

public class ACC_CurrentAssets
{
    clsFunctions fun = new clsFunctions();
    SqlConnection con;

    string connStr = "";
    

	public ACC_CurrentAssets()
	{
        connStr = fun.Connection();
        con = new SqlConnection(connStr);
		//
		// TODO: Add constructor logic here
		//
	}

    protected void Page_Load(object sender, EventArgs e)
    {
       
    }    

    public DataTable TotInvQty2(int CompId,int FinId, string CustId)
    {
        DataTable dt = new DataTable(); 
        //try
        {
            con.Open();

            string x = string.Empty;

            if (CustId != "")
            {
                x = " AND CustomerId='" + CustId + "'";
            }

            string StrSql = fun.select("CustomerName+'['+CustomerId+']' As Customer, CustomerId", "SD_Cust_master","CompId='"+CompId+"' AND FinYearId<='"+FinId+"'"+x+"");

            SqlCommand Cmdgrid = new SqlCommand(StrSql, con);
            SqlDataReader rdr = Cmdgrid.ExecuteReader();
            dt.Columns.Add(new System.Data.DataColumn("CustName", typeof(string)));
            dt.Columns.Add(new System.Data.DataColumn("TotAmt", typeof(double)));
            dt.Columns.Add(new System.Data.DataColumn("CustCode", typeof(string)));
            dt.Columns.Add(new System.Data.DataColumn("InvoiceNo", typeof(string)));
            dt.Columns.Add(new System.Data.DataColumn("CompId", typeof(Int32)));

            DataRow dr;
            while (rdr.Read())
            {
                dr = dt.NewRow();
                double fdapExp1Ins = 0;
                string strInv = "select InvoiceNo,OtherAmt from tblACC_SalesInvoice_Master where tblACC_SalesInvoice_Master.CustomerCode='" + rdr["CustomerId"] + "'";
                SqlCommand cmdInv = new SqlCommand(strInv, con);
                SqlDataReader rdrInv = cmdInv.ExecuteReader();

                while (rdrInv.Read())
                {
                    double OtherAmt = 0;
                    double totQty = 0;
                    double finaltot = 0;
                    double deduction = 0;
                    double addition = 0;
                    double fda = 0;
                    double pf = 0;
                    double excise = 0;
                    double fdap = 0;
                    double fdapEx = 0;
                    double fdapExp = 0;
                    double p = 0;
                    double p1 = 0;
                    double fdapExp1 = 0;
                    double Insurance = 0;
                    /// Calculate Basic                  
                    string strAmt = "select sum(case when Unit_Master.EffectOnInvoice=1 then (ReqQty*(AmtInPer/100)*Rate) Else (ReqQty*Rate) End) As Amt from tblACC_SalesInvoice_Details inner join tblACC_SalesInvoice_Master on tblACC_SalesInvoice_Master.Id=tblACC_SalesInvoice_Details.MId inner join  Unit_Master on tblACC_SalesInvoice_Details.Unit=Unit_Master.Id And tblACC_SalesInvoice_Master.CustomerCode='" + rdr["CustomerId"] + "' And tblACC_SalesInvoice_Master.InvoiceNo='" + rdrInv["InvoiceNo"] + "' ";
                    SqlCommand cmdAmt = new SqlCommand(strAmt, con);
                    SqlDataReader rdrAmt = cmdAmt.ExecuteReader();
                    rdrAmt.Read();
                    totQty += Convert.ToDouble(rdrAmt["Amt"]);

                    /// Calculate Addition 
                    string strdeduct1 = "select Sum(case when AddType=0 then AddAmt Else ((" + totQty + " *AddAmt)/100)End) As AddAmt from tblACC_SalesInvoice_Master where tblACC_SalesInvoice_Master.CustomerCode='" + rdr["CustomerId"] + "'And tblACC_SalesInvoice_Master.InvoiceNo='" + rdrInv["InvoiceNo"] + "' ";
                    SqlCommand cmdded1 = new SqlCommand(strdeduct1, con);
                    SqlDataReader rdr31 = cmdded1.ExecuteReader();
                    rdr31.Read();
                    addition = Convert.ToDouble(rdr31["AddAmt"]);
                    finaltot += totQty + addition;

                    /// Calculate deduction 
                    string strdeduct = "select Sum(case when DeductionType=0 then Deduction Else ((" + finaltot + " *Deduction)/100)End) As deduct from tblACC_SalesInvoice_Master where tblACC_SalesInvoice_Master.CustomerCode='" + rdr["CustomerId"] + "'And tblACC_SalesInvoice_Master.InvoiceNo='" + rdrInv["InvoiceNo"] + "' ";
                    SqlCommand cmdded = new SqlCommand(strdeduct, con);
                    SqlDataReader rdr3 = cmdded.ExecuteReader();
                    rdr3.Read();
                    deduction = Convert.ToDouble(rdr3["deduct"]);
                    fda += (finaltot - deduction);

                    /// Calculate Packing And Forwarding (PF)
                    string strpf = "select Sum(case when PFType=0 then PF Else ((" + fda + " *PF)/100)End) As pf from  tblACC_SalesInvoice_Master where tblACC_SalesInvoice_Master.CustomerCode='" + rdr["CustomerId"] + "'And tblACC_SalesInvoice_Master.InvoiceNo='" + rdrInv["InvoiceNo"] + "'";
                    SqlCommand cmdpf = new SqlCommand(strpf, con);
                    SqlDataReader rdr4 = cmdpf.ExecuteReader();
                    rdr4.Read();
                    pf = Convert.ToDouble(rdr4["pf"]);
                    fdap += (fda + pf);

                    /// Calculate Excise (CENVAT)
                    string strEx = "select Sum((" + fdap + ")*((tblExciseser_Master.AccessableValue)/100) + ((" + fdap + ")*((tblExciseser_Master.AccessableValue)/100)*tblExciseser_Master.EDUCess/100)+((" + fdap + ")*((tblExciseser_Master.AccessableValue)/100)*tblExciseser_Master.SHECess/100)) As Ex from  tblACC_SalesInvoice_Master inner join tblExciseser_Master on tblExciseser_Master.Id=tblACC_SalesInvoice_Master.CENVAT where tblACC_SalesInvoice_Master.CustomerCode='" + rdr["CustomerId"] + "'And tblACC_SalesInvoice_Master.InvoiceNo='" + rdrInv["InvoiceNo"] + "'";
                    SqlCommand cmdEx = new SqlCommand(strEx, con);
                    SqlDataReader rdr5 = cmdEx.ExecuteReader();
                    rdr5.Read();
                    excise = Convert.ToDouble(rdr5["Ex"]);
                    fdapEx += (fdap + excise);

                    /// Calculate CSTVAT (within/out Maharashtra)               
                    string strCSTVAT = "select FreightType,Freight,InvoiceMode,CST,VAT from  tblACC_SalesInvoice_Master where tblACC_SalesInvoice_Master.CustomerCode='" + rdr["CustomerId"] + "'And tblACC_SalesInvoice_Master.InvoiceNo='" + rdrInv["InvoiceNo"] + "'";
                    SqlCommand cmdCSTVAT = new SqlCommand(strCSTVAT, con);
                    SqlDataReader rdr6 = cmdCSTVAT.ExecuteReader();
                    while (rdr6.Read())
                    {
                        double f = Convert.ToDouble(rdr6["Freight"].ToString());
                        double v = 0;
                        if (rdr6["InvoiceMode"].ToString() == "2")
                        {
                            if (rdr6["FreightType"].ToString() == "0")
                            {
                                p = f;
                            }
                            else
                            {
                                p = fdapEx * (f / 100);
                            }
                            string SqlCst = fun.select("Value", "tblVAT_Master", "Id='" + rdr6["VAT"].ToString() + "'");
                            SqlCommand cmdSqlCst = new SqlCommand(SqlCst, con);
                            SqlDataReader rdr7 = cmdSqlCst.ExecuteReader();
                            while (rdr7.Read())
                            {
                                v = Convert.ToDouble(rdr7["Value"]);
                            }
                            p1 = (fdapEx + p) * (v / 100);

                        }
                        else if (rdr6["InvoiceMode"].ToString() == "3")
                        {
                            string SqlCst = fun.select("Value", "tblVAT_Master", "Id='" + rdr6["CST"].ToString() + "'");
                            SqlCommand cmdSqlCst = new SqlCommand(SqlCst, con);
                            SqlDataReader rdr7 = cmdSqlCst.ExecuteReader();
                            while (rdr7.Read())
                            {
                                v = Convert.ToDouble(rdr7["Value"]);
                            }
                            p = fdapEx * (v / 100);
                            if (rdr6["FreightType"].ToString() == "0")
                            {
                                p1 = f;
                            }
                            else
                            {
                                p1 = (fdapEx + p) * (f / 100);
                            }
                        }
                    }

                    fdapExp += fdapEx + p;
                    fdapExp1 += fdapExp + p1;

                    /// Calculate Insurance (LIC)
                    string strInc = "select Sum(case when InsuranceType=0 then Insurance Else ((" + fdapExp1 + " *Insurance)/100)End) As Insurance from  tblACC_SalesInvoice_Master where tblACC_SalesInvoice_Master.CustomerCode='" + rdr["CustomerId"] + "'And tblACC_SalesInvoice_Master.InvoiceNo='" + rdrInv["InvoiceNo"] + "'";
                    SqlCommand cmdInc = new SqlCommand(strInc, con);
                    SqlDataReader rdr8 = cmdInc.ExecuteReader();
                    rdr8.Read();

                    Insurance = Convert.ToDouble(rdr8["Insurance"]);

                   // fdapExp1Ins += fdapExp1 + Insurance;

                    /// Calculate Other Amount  
                    if (rdrInv["OtherAmt"] != DBNull.Value)
                    {
                        OtherAmt = Math.Round(Convert.ToDouble(rdrInv["OtherAmt"]), 2);
                    }
                    fdapExp1Ins += fdapExp1 + Insurance + OtherAmt;
                    dr[3] = rdrInv["InvoiceNo"];                  

                }

                dr[0] = rdr["Customer"].ToString();
                dr[1] = Math.Round(fdapExp1Ins, 2);
                dr[2] = rdr["CustomerId"].ToString();
                dr[4] = CompId; 
                dt.Rows.Add(dr);
                dt.AcceptChanges();
            }

        }
       // catch (Exception ex) { }
      //  finally
        {
            con.Close();
        }
        
        return dt;
        
    }

    

    public double TotLoanLiability(int CompId, int FinId)
    {
        double TotLoanLiability = 0;
        try
        {
            con.Open();
            string strInc = "Select Sum(CreditAmt) As loan from tblAcc_LoanDetails inner join tblAcc_LoanMaster on tblAcc_LoanDetails.MId=tblAcc_LoanMaster.Id And CompId=" + CompId + " AND FinYearId<='" + FinId + "'  ";
            SqlCommand cmdInc = new SqlCommand(strInc, con);
            SqlDataReader rdr8 = cmdInc.ExecuteReader();
            rdr8.Read();
            TotLoanLiability = Convert.ToDouble(rdr8["loan"]);

        }
        catch (Exception ex)
        {
        }
        finally
        {
            con.Close();
        }
        return TotLoanLiability;
    }


    public double TotCapitalGoods(int CompId, int FinId)
    {
        double TotCapitalGood = 0;
        try
        {
            con.Open();
            string strInc = "Select Sum(CreditAmt) As capital from tblACC_Capital_Details inner join tblACC_Capital_Master on tblACC_Capital_Details.MId=tblACC_Capital_Master.Id And CompId=" + CompId + " AND FinYearId<='" + FinId + "'  ";
            SqlCommand cmdInc = new SqlCommand(strInc, con);
            SqlDataReader rdr8 = cmdInc.ExecuteReader();
            rdr8.Read();
            TotCapitalGood = Convert.ToDouble(rdr8["capital"]);

        }
        catch (Exception ex)
        {
        }
        finally
        {
            con.Close();
        }
        return TotCapitalGood;
    }

}
