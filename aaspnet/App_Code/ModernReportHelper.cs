using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Web;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Globalization;

/// <summary>
/// Modern Report Helper - Replaces Crystal Reports functionality
/// Provides HTML report generation capabilities
/// </summary>
public class ModernReportHelper
{
    public static class ReportGenerator
    {
        /// <summary>
        /// Generate HTML report from DataTable
        /// </summary>
        public static string GenerateHTMLReport(DataTable data, string reportTitle, string[] columnHeaders = null, string[] columnWidths = null)
        {
            if (data == null || data.Rows.Count == 0)
                return "<div class='no-data'>No data available for the report.</div>";

            StringBuilder html = new StringBuilder();
            
            // Start HTML structure
            html.Append(@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>" + reportTitle + @"</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 20px; }
        .report-header { text-align: center; margin-bottom: 30px; }
        .report-title { font-size: 24px; font-weight: bold; color: #333; margin-bottom: 10px; }
        .report-date { font-size: 12px; color: #666; }
        .report-table { width: 100%; border-collapse: collapse; margin-top: 20px; }
        .report-table th { background-color: #f8f9fa; border: 1px solid #dee2e6; padding: 8px; text-align: left; font-weight: bold; }
        .report-table td { border: 1px solid #dee2e6; padding: 8px; }
        .report-table tr:nth-child(even) { background-color: #f8f9fa; }
        .report-table tr:hover { background-color: #e9ecef; }
        .numeric { text-align: right; }
        .center { text-align: center; }
        .no-data { text-align: center; padding: 50px; color: #666; font-style: italic; }
        @media print {
            body { margin: 0; }
            .report-table { font-size: 10px; }
            .report-table th, .report-table td { padding: 4px; }
        }
    </style>
</head>
<body>
    <div class='report-header'>
        <div class='report-title'>" + reportTitle + @"</div>
        <div class='report-date'>Generated on: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + @"</div>
    </div>");

            // Build table
            html.Append("<table class='report-table'>");
            
            // Headers
            html.Append("<thead><tr>");
            for (int i = 0; i < data.Columns.Count; i++)
            {
                string headerText = (columnHeaders != null && i < columnHeaders.Length) ? 
                    columnHeaders[i] : data.Columns[i].ColumnName;
                    
                string widthStyle = "";
                if (columnWidths != null && i < columnWidths.Length && !string.IsNullOrEmpty(columnWidths[i]))
                {
                    widthStyle = " style='width: " + columnWidths[i] + ";'";
                }
                
                html.Append("<th" + widthStyle + ">" + HttpUtility.HtmlEncode(headerText) + "</th>");
            }
            html.Append("</tr></thead>");

            // Data rows
            html.Append("<tbody>");
            foreach (DataRow row in data.Rows)
            {
                html.Append("<tr>");
                for (int i = 0; i < data.Columns.Count; i++)
                {
                    string cellValue = (row[i] != null) ? row[i].ToString() : "";
                    string cssClass = GetCellCssClass(data.Columns[i].DataType);
                    html.Append("<td class='" + cssClass + "'>" + HttpUtility.HtmlEncode(cellValue) + "</td>");
                }
                html.Append("</tr>");
            }
            html.Append("</tbody>");
            
            html.Append("</table>");
            html.Append("</body></html>");

            return html.ToString();
        }
        /// <summary>
        /// Get appropriate CSS class for data type
        /// </summary>
        private static string GetCellCssClass(Type dataType)
        {
            if (dataType == typeof(decimal) || dataType == typeof(double) || 
                dataType == typeof(float) || dataType == typeof(int) || 
                dataType == typeof(long) || dataType == typeof(short))
            {
                return "numeric";
            }
            
            if (dataType == typeof(DateTime))
            {
                return "center";
            }
            
            return "";
        }
    }
}