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
using System.Reflection;

/// <summary>
/// Summary description for Cal_Used_Hours
/// </summary>
public class Cal_Used_Hours
{
    clsFunctions fun = new clsFunctions();
    TimeBudget_DeptDataContext TBDC = new TimeBudget_DeptDataContext();
    SqlConnection con;
    string connStr = "";
    public Cal_Used_Hours()
    {
        connStr = fun.Connection();
        con = new SqlConnection(connStr);
    }

    /// <summary>
    //flag=0 when tblName=tblPM_ManPowerPlanning
    //flag=1 when tblName=tblPM_ManPowerPlanning_Temp    
    public string TotFillPart(int Grade, string WONo, int Dept, int CompId, int FinYearId, int flag)
    {
        double number = 0;
        try
        {
            con.Open();
            string StrSql = string.Empty;
            if (flag == 0)
            {
                StrSql = "select Sum(Hours) As ConsumedHrs  from tblPM_ManPowerPlanning INNER JOIN tblHR_OfficeStaff ON tblPM_ManPowerPlanning.EmpId = tblHR_OfficeStaff.EmpId And tblPM_ManPowerPlanning.CompId=" + CompId + " And tblPM_ManPowerPlanning.FinYearId<=" + FinYearId + " And Grade=" + Grade + " And WONo='" + WONo + "'And Dept=" + Dept + " group by Grade";
            }
            else
            {
                StrSql = "select Sum(Hours) As ConsumedHrs from tblPM_ManPowerPlanning_Temp INNER JOIN tblHR_OfficeStaff ON tblPM_ManPowerPlanning_Temp.EmpId = tblHR_OfficeStaff.EmpId And tblPM_ManPowerPlanning_Temp.CompId=" + CompId + " And tblPM_ManPowerPlanning.FinYearId<=" + FinYearId + " And Grade=" + Grade + " And WONo='" + WONo + "'And Dept=" + Dept + " group by Grade";
            }
            SqlCommand Cmdgrid = new SqlCommand(StrSql, con);
            SqlDataReader rdr = Cmdgrid.ExecuteReader();
            while (rdr.Read())
            {
                number = Convert.ToDouble(rdr["ConsumedHrs"]);
            }

        }
        catch (Exception ex) { }
        finally
        {
            con.Close();
        }
        return number.ToString("####0.00");
    }


    //public double ConvertTimeTodouble(double FromTime, double ToTime)
    //{        
    //    double number = 0;
    //    try
    //    {
    //        double Hours = 0;
    //        Hours = Math.Floor(FromTime);
    //        double Minutes = 0;
    //        Minutes = Math.Round(ToTime, 2);
    //        if (Minutes >= 60)
    //        {
    //            Minutes = Math.Floor(Minutes / 60) + (Minutes % 60 / 100);
    //            Minutes = Math.Round(Minutes, 2);
    //        }
    //        else
    //        {
    //            Minutes = Math.Round(Minutes / 100, 2);
    //        }
    //        string HoursAndMinutes = Convert.ToString((Hours + Minutes));
    //        number = Convert.ToDouble(HoursAndMinutes);
    //    }
    //    catch(Exception ex)
    //    {
    //    }
    //    return number;
    //}

    /// if Case=0 then Allocated Total Hours
    /// if Case=1 then Total Used Hours
    /// if Case=2 then Total Balanced Hours
    /// 
    public double BalanceHours(int Grade, int Dept, int CompId, int FinYearId, int Case)
    {
        double BalancedHrs = 0;
        string WONo = string.Empty;
        double UsedHrs = 0;
        double AllocatedHrs = 0;
        var grpbyfilter = from row in TBDC.GetTable<tblACC_Budget_Dept_Time>()
                          where ((row.BGGroup == Dept) && (row.BudgetCodeId == Grade) && (row.CompId == CompId) && (row.FinYearId <= FinYearId))
                          group row by new
                          {
                              y = row.BudgetCodeId,
                          } into grp
                          select new
                          {
                              Total = grp.Sum(r => r.Hour)
                          };
        double Value = 0;
        foreach (var text in grpbyfilter.ToList())
        {
            Value = Convert.ToDouble(text.Total);
        }
        AllocatedHrs = Value;
        UsedHrs = Convert.ToDouble(this.TotFillPart(Grade, WONo, Dept, CompId, FinYearId, 0));
        BalancedHrs = Math.Round((AllocatedHrs - UsedHrs), 2);
        double x = 0;
        switch (Case)
        {
            case 0:
                x = AllocatedHrs;
                break;
            case 1:
                x = UsedHrs;
                break;
            case 2:
                x = BalancedHrs;
                break;
        }
        return x;
    }

    /// if Case=0 then Allocated Total Hours
    /// if Case=1 then Total Used Hours
    /// if Case=2 then Total Balanced Hours
    /// 
    public double BalanceHours_WONO(int Grade, string WONo, int CompId, int FinYearId, int Case)
    {
        double BalancedHrs = 0;
        int Dept = 0;
        double UsedHrs = 0;
        double AllocatedHrs = 0;
        var grpbyfilter = from row in TBDC.GetTable<tblACC_Budget_WO_Time>()
                          where ((row.WONo == WONo) && (row.BudgetCodeId == Grade) && (row.CompId == CompId) && (row.FinYearId == FinYearId))
                          group row by new
                          {
                              y = row.BudgetCodeId,
                          } into grp
                          select new
                          {
                              Total = grp.Sum(r => r.Hour)
                          };
        double Value = 0;
        foreach (var text in grpbyfilter.ToList())
        {
            Value = Convert.ToDouble(text.Total);
        }
        AllocatedHrs = Value;
        UsedHrs = Convert.ToDouble(this.TotFillPart(Grade, WONo, Dept, CompId, FinYearId, 0));
        BalancedHrs = Math.Round((AllocatedHrs - UsedHrs), 2);
        double x = 0;
        switch (Case)
        {
            case 0:
                x = AllocatedHrs;
                break;
            case 1:
                x = UsedHrs;
                break;
            case 2:
                x = BalancedHrs;
                break;
        }
        return x;
    }

    //==============================================================================================================

    public string UtilizeHrs_WONo(int CompId, string WONo, int EquipId, int Category, int SubCategory)
    {
        double number = 0;
        try
        {
            con.Open();
            string StrSql = string.Empty;

            StrSql = "SELECT Sum(tblPM_ManPowerPlanning_Details.Hour) As CutilizedHrs FROM tblPM_ManPowerPlanning INNER JOIN tblPM_ManPowerPlanning_Details ON tblPM_ManPowerPlanning.Id = tblPM_ManPowerPlanning_Details.MId AND tblPM_ManPowerPlanning_Details.Category='" + Category + "' AND tblPM_ManPowerPlanning_Details.SubCategory='" + SubCategory + "' AND tblPM_ManPowerPlanning_Details.EquipId='" + EquipId + "' AND tblPM_ManPowerPlanning.WONo='" + WONo + "' Group By tblPM_ManPowerPlanning_Details.EquipId";
            SqlCommand Cmdgrid = new SqlCommand(StrSql, con);
            SqlDataReader rdr = Cmdgrid.ExecuteReader();

            while (rdr.Read())
            {
                number = Convert.ToDouble(rdr["CutilizedHrs"]);
            }

        }
        catch (Exception ex) { }
        finally
        {
            con.Close();
        }
        return number.ToString("####0.00");
    }
    public string AllocatedHrs_WONo(int CompId, string WONo, int EquipId, int Category, int SubCategory)
    {
        double number = 0;
        try
        {
            con.Open();
            string StrSql = string.Empty;

            StrSql = fun.select("Sum(Hour) As AllocatedHrs", "tblACC_Budget_WO_Time", "HrsBudgetCat='" + Category + "' AND HrsBudgetSubCat='" + SubCategory + "' AND EquipId='" + EquipId + "' AND WONo='" + WONo + "' Group By EquipId");

            SqlCommand Cmdgrid = new SqlCommand(StrSql, con);
            SqlDataReader rdr = Cmdgrid.ExecuteReader();
            
            while (rdr.Read())
            {
                number = Convert.ToDouble(rdr["AllocatedHrs"]);
            }

        }
        catch (Exception ex) { }
        finally
        {
            con.Close();
        }
        return number.ToString("####0.00");
    }

    public double GetTotalAllocatedHrs_WONo(string wono, int Cate, int SubCate)
    {
        double x = 0;
        try
        {
            con.Open();

            string selHrsBudget = string.Empty;

            selHrsBudget = fun.select("Sum(Hour) As THrs", "tblACC_Budget_WO_Time", "WONo='" + wono + "'  AND HrsBudgetCat='" + Cate + "' AND HrsBudgetSubCat='" + SubCate + "'");
            SqlCommand cmdselHrsBudget = new SqlCommand(selHrsBudget, con);
            SqlDataReader DSselHrsBudget = cmdselHrsBudget.ExecuteReader();
            DSselHrsBudget.Read();

            if (DSselHrsBudget.HasRows == true && DSselHrsBudget["THrs"] != DBNull.Value)
            {
                x = Convert.ToDouble(DSselHrsBudget["THrs"]);
            }
        }
        catch (Exception ex) { }
        finally
        {
            con.Close();
        }
        return x;
    }
    public double GetTotalUtilizeHrs_WONo(string wono, int Cate, int SubCate)
    {
        double y = 0;
        try
        {
            con.Open();
            string StrSql = string.Empty;

            StrSql = "SELECT Sum(tblPM_ManPowerPlanning_Details.Hour) As UtilizedHrs FROM tblPM_ManPowerPlanning INNER JOIN tblPM_ManPowerPlanning_Details ON tblPM_ManPowerPlanning.Id = tblPM_ManPowerPlanning_Details.MId AND tblPM_ManPowerPlanning_Details.Category='" + Cate + "' AND tblPM_ManPowerPlanning_Details.SubCategory='" + SubCate + "' AND tblPM_ManPowerPlanning.WONo='" + wono + "'";
            
            SqlCommand Cmdgrid = new SqlCommand(StrSql, con);
            SqlDataReader rdr = Cmdgrid.ExecuteReader();
            rdr.Read();

            if (rdr.HasRows == true && rdr["UtilizedHrs"] != DBNull.Value)
            {
                y = Convert.ToDouble(rdr["UtilizedHrs"]);
            }
        }
        catch (Exception ex) { }
        finally
        {
            con.Close();
        }
        return y;
    }

}
