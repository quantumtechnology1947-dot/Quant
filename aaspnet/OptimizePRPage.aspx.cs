using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.Data.SqlClient;

public partial class OptimizePRPage : System.Web.UI.Page
{
    clsFunctions fun = new clsFunctions();
    
    protected void Page_Load(object sender, EventArgs e)
    {
        
    }
    
    protected void btnOptimizeIndexes_Click(object sender, EventArgs e)
    {
        try
        {
            string result = OptimizeDatabaseIndexes();
            lblIndexStatus.Text = result;
            lblIndexStatus.CssClass = result.Contains("Error") ? "error" : "success";
        }
        catch (Exception ex)
        {
            lblIndexStatus.Text = "Error: " + ex.Message;
            lblIndexStatus.CssClass = "error";
        }
    }
    
    protected void btnClearPool_Click(object sender, EventArgs e)
    {
        try
        {
            // Clear connection pool
            SqlConnection.ClearAllPools();
            
            // Test a new connection
            string connStr = fun.Connection();
            using (SqlConnection con = new SqlConnection(connStr))
            {
                con.Open();
                con.Close();
            }
            
            lblPoolStatus.Text = "Connection pool has been successfully cleared. " + 
                            "All connections have been reset. " + 
                            DateTime.Now.ToString();
            lblPoolStatus.CssClass = "success";
        }
        catch (Exception ex)
        {
            lblPoolStatus.Text = "Error clearing connection pool: " + ex.Message;
            lblPoolStatus.CssClass = "error";
        }
    }
    
    protected void btnOptimizeCode_Click(object sender, EventArgs e)
    {
        try
        {
            string result = OptimizePRPageCode();
            lblCodeStatus.Text = result;
            lblCodeStatus.CssClass = result.Contains("Error") ? "error" : "success";
        }
        catch (Exception ex)
        {
            lblCodeStatus.Text = "Error optimizing code: " + ex.Message;
            lblCodeStatus.CssClass = "error";
        }
    }
    
    protected void btnFixConfig_Click(object sender, EventArgs e)
    {
        try
        {
            string result = OptimizeWebConfig();
            lblConfigStatus.Text = result;
            lblConfigStatus.CssClass = result.Contains("Error") ? "error" : "success";
        }
        catch (Exception ex)
        {
            lblConfigStatus.Text = "Error optimizing configuration: " + ex.Message;
            lblConfigStatus.CssClass = "error";
        }
    }
    
    protected void btnRunAll_Click(object sender, EventArgs e)
    {
        try
        {
            StringBuilder results = new StringBuilder();
            
            // Run all optimizations
            string dbResult = OptimizeDatabaseIndexes();
            results.AppendLine("Database Optimization: " + (dbResult.Contains("Error") ? "Failed" : "Success"));
            
            // Clear connection pool
            SqlConnection.ClearAllPools();
            results.AppendLine("Connection Pool Clear: Success");
            
            string codeResult = OptimizePRPageCode();
            results.AppendLine("Code Optimization: " + (codeResult.Contains("Error") ? "Failed" : "Success"));
            
            string configResult = OptimizeWebConfig();
            results.AppendLine("Configuration Optimization: " + (configResult.Contains("Error") ? "Failed" : "Success"));
            
            lblAllStatus.Text = results.ToString().Replace("\n", "<br/>");
            lblAllStatus.CssClass = 
                (dbResult.Contains("Error") || codeResult.Contains("Error") || configResult.Contains("Error")) 
                ? "warning" : "success";
        }
        catch (Exception ex)
        {
            lblAllStatus.Text = "Error running all optimizations: " + ex.Message;
            lblAllStatus.CssClass = "error";
        }
    }
    
    private string OptimizeDatabaseIndexes()
    {
        string connStr = fun.Connection();
        StringBuilder result = new StringBuilder();
        int successCount = 0;
        
        using (SqlConnection con = new SqlConnection(connStr))
        {
            try
            {
                con.Open();
                
                // Add optimization queries here
                string[] optimizationQueries = new string[] 
                {
                    // Update statistics for key tables
                    "UPDATE STATISTICS SD_Cust_WorkOrder_Master WITH FULLSCAN",
                    "UPDATE STATISTICS tblSD_WO_Category WITH FULLSCAN",
                    
                    // Optimize key indexes for PR page
                    "IF EXISTS (SELECT * FROM sys.indexes WHERE name='IX_SD_WO_CloseOpen' AND object_id = OBJECT_ID('SD_Cust_WorkOrder_Master'))" +
                    "    DROP INDEX IX_SD_WO_CloseOpen ON SD_Cust_WorkOrder_Master",
                    
                    "IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_SD_WO_CloseOpen' AND object_id = OBJECT_ID('SD_Cust_WorkOrder_Master'))" +
                    "    CREATE INDEX IX_SD_WO_CloseOpen ON SD_Cust_WorkOrder_Master (CompId, CloseOpen, WONo)",
                    
                    "IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_tblSD_WO_Category_CompId' AND object_id = OBJECT_ID('tblSD_WO_Category'))" +
                    "    CREATE INDEX IX_tblSD_WO_Category_CompId ON tblSD_WO_Category (CompId)"
                };
                
                foreach (string query in optimizationQueries)
                {
                    try
                    {
                        using (SqlCommand cmd = new SqlCommand(query, con))
                        {
                            cmd.CommandTimeout = 120; // 2 minutes timeout
                            cmd.ExecuteNonQuery();
                            successCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        result.AppendLine("<br/>Error executing: " + query);
                        result.AppendLine("<br/>Error: " + ex.Message);
                    }
                }
                
                // Add table reorganization
                string[] tableNames = new string[] {
                    "SD_Cust_WorkOrder_Master",
                    "tblSD_WO_Category",
                    "tblMM_PLN_PR_Temp"
                };
                
                foreach (string tableName in tableNames)
                {
                    try
                    {
                        using (SqlCommand cmd = new SqlCommand("ALTER INDEX ALL ON " + tableName + " REORGANIZE", con))
                        {
                            cmd.CommandTimeout = 300; // 5 minutes timeout
                            cmd.ExecuteNonQuery();
                            successCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        result.AppendLine("<br/>Error reorganizing table " + tableName + ": " + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                result.AppendLine("Error connecting to database: " + ex.Message);
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
        }
        
        if (result.Length > 0)
        {
            return "Errors occurred during database optimization: " + result.ToString();
        }
        else
        {
            return "Database optimization completed successfully. Applied " + successCount + " optimizations.";
        }
    }
    
    private string OptimizePRPageCode()
    {
        try
        {
            string filePath = Server.MapPath("~/Module/MaterialManagement/Transactions/PR_New.aspx.cs");
            string backupPath = filePath + ".bak";
            
            // Create backup
            if (File.Exists(filePath))
            {
                File.Copy(filePath, backupPath, true);
            }
            else
            {
                return "Error: PR_New.aspx.cs file not found at " + filePath;
            }
            
            // Read the file
            string content = File.ReadAllText(filePath);
            
            // Apply optimizations
            
            // 1. Add limit to the query
            content = content.Replace(
                "string sqlwo = fun.select(\"WONo,TaskProjectTitle,ReleaseWIS,DryActualRun\", \"SD_Cust_WorkOrder_Master\", \"CompId='\" + CompId + \"' AND CloseOpen='0'\" + x +Z+ \" Order by WONo\");",
                "string sqlwo = \"SELECT TOP 100 WONo,TaskProjectTitle,ReleaseWIS,DryActualRun FROM SD_Cust_WorkOrder_Master WHERE CompId='\" + CompId + \"' AND CloseOpen='0'\" + x +Z+ \" Order by WONo\";");
            
            // 2. Optimize connection management in getItemTot
            content = content.Replace(
                "public void getItemTot(string wo, int c)\r\n    {\r\n        try\r\n        {",
                "public void getItemTot(string wo, int c)\r\n    {\r\n        SqlConnection con = null;\r\n        try\r\n        {");
            
            content = content.Replace(
                "string str = fun.Connection();\r\n            SqlConnection con = new SqlConnection(str);",
                "string str = fun.Connection();\r\n            con = new SqlConnection(str);");
            
            // 3. Add proper connection closing in finally block
            content = content.Replace(
                "catch (Exception ess)\r\n        {\r\n\r\n        }",
                "catch (Exception ess)\r\n        {\r\n            // Log error if needed\r\n        }\r\n        finally\r\n        {\r\n            if (con != null && con.State == ConnectionState.Open)\r\n            {\r\n                con.Close();\r\n            }\r\n        }");
            
            // 4. Remove unnecessary AcceptChanges call
            content = content.Replace("dt.Rows.Add(dr);\r\n                dt.AcceptChanges();", "dt.Rows.Add(dr);");
            
            // 5. Add proper connection closing in Page_Load
            content = content.Replace(
                "catch(Exception ex){}\r\n    }",
                "catch(Exception ex){\r\n            // Log error if needed\r\n        }\r\n        finally\r\n        {\r\n            if (con != null && con.State == ConnectionState.Open)\r\n            {\r\n                con.Close();\r\n            }\r\n        }\r\n    }");
            
            // Write the optimized file
            File.WriteAllText(filePath, content);
            
            return "Code optimization completed successfully. A backup of the original file was created at " + backupPath;
        }
        catch (Exception ex)
        {
            return "Error optimizing code: " + ex.Message;
        }
    }
    
    private string OptimizeWebConfig()
    {
        try
        {
            string filePath = Server.MapPath("~/web.config");
            string backupPath = filePath + ".bak";
            
            // Create backup
            if (File.Exists(filePath))
            {
                File.Copy(filePath, backupPath, true);
            }
            else
            {
                return "Error: web.config file not found at " + filePath;
            }
            
            // Read the file
            string content = File.ReadAllText(filePath);
            
            // Apply optimizations
            
            // 1. Increase max pool size and connection timeout
            content = content.Replace(
                "connectionString=\"Data Source=.\\SQLEXPRESS;Initial Catalog=ERP_DB_Real;Integrated Security=True;MultipleActiveResultSets=True;Pooling=True;Max Pool Size=200\"",
                "connectionString=\"Data Source=.\\SQLEXPRESS;Initial Catalog=ERP_DB_Real;Integrated Security=True;MultipleActiveResultSets=True;Pooling=True;Max Pool Size=300;Connection Timeout=120\"");
            
            // 2. Enable output caching for static resources
            if (!content.Contains("<caching>"))
            {
                int webServerEndIndex = content.IndexOf("</system.webServer>");
                if (webServerEndIndex > 0)
                {
                    string cachingConfig = 
                        "    <caching>\r\n" +
                        "      <profiles>\r\n" +
                        "        <add extension=\".gif\" policy=\"CacheUntilChange\" kernelCachePolicy=\"CacheUntilChange\" />\r\n" +
                        "        <add extension=\".jpg\" policy=\"CacheUntilChange\" kernelCachePolicy=\"CacheUntilChange\" />\r\n" +
                        "        <add extension=\".png\" policy=\"CacheUntilChange\" kernelCachePolicy=\"CacheUntilChange\" />\r\n" +
                        "        <add extension=\".js\" policy=\"CacheUntilChange\" kernelCachePolicy=\"CacheUntilChange\" />\r\n" +
                        "        <add extension=\".css\" policy=\"CacheUntilChange\" kernelCachePolicy=\"CacheUntilChange\" />\r\n" +
                        "      </profiles>\r\n" +
                        "    </caching>\r\n";
                    
                    content = content.Insert(webServerEndIndex, cachingConfig);
                }
            }
            
            // Write the optimized file
            File.WriteAllText(filePath, content);
            
            return "Configuration optimization completed successfully. A backup of the original file was created at " + backupPath;
        }
        catch (Exception ex)
        {
            return "Error optimizing configuration: " + ex.Message;
        }
    }
} 