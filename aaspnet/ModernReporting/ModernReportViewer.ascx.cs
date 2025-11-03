using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

/// <summary>
/// Modern Report Viewer Control - Replaces Crystal Reports Viewer
/// Provides HTML and PDF report viewing capabilities
/// </summary>
public partial class ModernReporting_ModernReportViewer : System.Web.UI.UserControl
{
    #region Private Fields
    private DataTable _reportData;
    private string _reportTitle = "Report";
    private string[] _columnHeaders;
    private string[] _columnWidths;
    #endregion

    #region Public Properties
    
    /// <summary>
    /// Gets or sets the data source for the report
    /// </summary>
    public DataTable ReportData
    {
        get { return _reportData; }
        set 
        { 
            _reportData = value;
            GenerateReport();
        }
    }

    /// <summary>
    /// Gets or sets the report title
    /// </summary>
    public string ReportTitle
    {
        get { return _reportTitle; }
        set 
        { 
            _reportTitle = value;
            lblReportTitle.Text = value;
        }
    }

    /// <summary>
    /// Gets or sets custom column headers
    /// </summary>
    public string[] ColumnHeaders
    {
        get { return _columnHeaders; }
        set { _columnHeaders = value; }
    }

    /// <summary>
    /// Gets or sets column widths (e.g., "100px", "20%")
    /// </summary>
    public string[] ColumnWidths
    {
        get { return _columnWidths; }
        set { _columnWidths = value; }
    }

    /// <summary>
    /// Gets or sets visibility of the report viewer
    /// </summary>
    public bool Visible
    {
        get { return modernReportContainer.Visible; }
        set { modernReportContainer.Visible = value; }
    }

    #endregion

    #region Page Events

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            lblReportTitle.Text = _reportTitle;
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Set report data and generate the report
    /// </summary>
    public void SetReportData(DataTable data, string title = null, string[] headers = null, string[] widths = null)
    {
        _reportData = data;
        
        if (!string.IsNullOrEmpty(title))
            ReportTitle = title;
            
        if (headers != null)
            _columnHeaders = headers;
            
        if (widths != null)
            _columnWidths = widths;

        GenerateReport();
    }

    /// <summary>
    /// Clear the report data and content
    /// </summary>
    public void ClearReport()
    {
        _reportData = null;
        litReportContent.Text = "";
        pnlReportContent.Visible = false;
        pnlNoData.Visible = true;
    }

    /// <summary>
    /// Refresh the report display
    /// </summary>
    public void RefreshReport()
    {
        GenerateReport();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Generate the HTML report content
    /// </summary>
    private void GenerateReport()
    {
        try
        {
            if (_reportData != null && _reportData.Rows.Count > 0)
            {
                // Generate HTML content
                string htmlContent = ModernReportHelper.ReportGenerator.GenerateHTMLReport(
                    _reportData, _reportTitle, _columnHeaders, _columnWidths);

                // Extract just the table content for embedding in the control
                string tableContent = ExtractTableContent(htmlContent);
                
                litReportContent.Text = tableContent;
                pnlReportContent.Visible = true;
                pnlNoData.Visible = false;
            }
            else
            {
                // Show no data message
                pnlReportContent.Visible = false;
                pnlNoData.Visible = true;
            }
        }
        catch (Exception ex)
        {
            // Log error (you might want to use your existing logging mechanism)
            litReportContent.Text = "<div class='alert alert-danger'>Error generating report: " + 
                Server.HtmlEncode(ex.Message) + "</div>";
            pnlReportContent.Visible = true;
            pnlNoData.Visible = false;
        }
    }

    /// <summary>
    /// Extract table content from full HTML
    /// </summary>
    private string ExtractTableContent(string fullHtml)
    {
        // Extract content between report-header and closing body tag
        int startIndex = fullHtml.IndexOf("<div class='report-header'>");
        int endIndex = fullHtml.LastIndexOf("</body>");
        
        if (startIndex >= 0 && endIndex >= 0 && endIndex > startIndex)
        {
            return fullHtml.Substring(startIndex, endIndex - startIndex);
        }
        
        // Fallback: just extract the table
        startIndex = fullHtml.IndexOf("<table class='report-table'>");
        endIndex = fullHtml.LastIndexOf("</table>") + "</table>".Length;
        
        if (startIndex >= 0 && endIndex >= 0 && endIndex > startIndex)
        {
            return fullHtml.Substring(startIndex, endIndex - startIndex);
        }
        
        return fullHtml; // Return as-is if we can't extract
    }

    #endregion

    #region Button Events

    /// <summary>
    /// View report in HTML format in a new window
    /// </summary>
    protected void btnViewHTML_Click(object sender, EventArgs e)
    {
        try
        {
            if (_reportData != null && _reportData.Rows.Count > 0)
            {
                string htmlContent = ModernReportHelper.ReportGenerator.GenerateHTMLReport(
                    _reportData, _reportTitle, _columnHeaders, _columnWidths);

                // Store in session for popup window
                string sessionKey = "Report_" + Guid.NewGuid().ToString();
                Session[sessionKey] = htmlContent;

                // Open in new window
                string script = "window.open('ReportViewer.aspx?key=" + sessionKey + "', '_blank', 'width=1024,height=768,scrollbars=yes,resizable=yes');";
                Page.ClientScript.RegisterStartupScript(this.GetType(), "ViewReport", script, true);
            }
            else
            {
                Page.ClientScript.RegisterStartupScript(this.GetType(), "NoDataAlert", 
                    "alert('No data available to display.');", true);
            }
        }
        catch (Exception ex)
        {
            Page.ClientScript.RegisterStartupScript(this.GetType(), "ErrorAlert", 
                "alert('Error generating report: " + ex.Message.Replace("'", "\\'") + "');", true);
        }
    }

    /// <summary>
    /// Download report as PDF
    /// </summary>
    protected void btnDownloadPDF_Click(object sender, EventArgs e)
    {
        try
        {
            if (_reportData != null && _reportData.Rows.Count > 0)
            {
                // PDF functionality temporarily disabled - focus on HTML reporting first
                Page.ClientScript.RegisterStartupScript(this.GetType(), "InfoAlert", 
                    "alert('PDF download functionality will be available in a future update. Please use the HTML view for now.');", true);
            }
            else
            {
                Page.ClientScript.RegisterStartupScript(this.GetType(), "NoDataAlert", 
                    "alert('No data available to download.');", true);
            }
        }
        catch (Exception ex)
        {
            Page.ClientScript.RegisterStartupScript(this.GetType(), "ErrorAlert", 
                "alert('Error generating PDF: " + ex.Message.Replace("'", "\\'") + "');", true);
        }
    }

    /// <summary>
    /// Set HTML content directly for the report
    /// </summary>
    public void SetReportHTML(string htmlContent, string title)
    {
        try
        {
            _reportTitle = title;
            lblReportTitle.Text = title;
            
            litReportContent.Text = htmlContent;
            pnlReportContent.Visible = true;
            pnlNoData.Visible = false;
            modernReportContainer.Visible = true;
        }
        catch (Exception ex)
        {
            Page.ClientScript.RegisterStartupScript(this.GetType(), "ErrorAlert", 
                "alert('Error setting report content: " + ex.Message.Replace("'", "\\'") + "');", true);
        }
    }

    #endregion
}
