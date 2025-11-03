<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ModernReportViewer.ascx.cs" Inherits="ModernReporting_ModernReportViewer" %>

<div id="modernReportContainer" runat="server" class="modern-report-container">
    <!-- Report Toolbar -->
    <div class="report-toolbar">
        <div class="toolbar-left">
            <asp:Label ID="lblReportTitle" runat="server" CssClass="report-title-label"></asp:Label>
        </div>
        <div class="toolbar-right">
            <asp:Button ID="btnViewHTML" runat="server" Text="View HTML" CssClass="btn btn-info" OnClick="btnViewHTML_Click" />
            <asp:Button ID="btnDownloadPDF" runat="server" Text="Download PDF" CssClass="btn btn-primary" OnClick="btnDownloadPDF_Click" />
            <asp:Button ID="btnPrint" runat="server" Text="Print" CssClass="btn btn-secondary" OnClientClick="printReport(); return false;" />
        </div>
    </div>

    <!-- Report Content Area -->
    <div class="report-content-area">
        <asp:Panel ID="pnlReportContent" runat="server" CssClass="report-content-panel">
            <asp:Literal ID="litReportContent" runat="server"></asp:Literal>
        </asp:Panel>
        
        <asp:Panel ID="pnlNoData" runat="server" CssClass="no-data-panel" Visible="false">
            <div class="no-data-message">
                <i class="fa fa-info-circle"></i>
                <h3>No Data Available</h3>
                <p>There is no data to display for this report. Please adjust your search criteria and try again.</p>
            </div>
        </asp:Panel>
    </div>
</div>

<style>
    .modern-report-container {
        width: 100%;
        border: 1px solid #dee2e6;
        border-radius: 4px;
        background-color: #fff;
        margin: 10px 0;
    }

    .report-toolbar {
        display: flex;
        justify-content: space-between;
        align-items: center;
        padding: 10px 15px;
        background-color: #f8f9fa;
        border-bottom: 1px solid #dee2e6;
        border-radius: 4px 4px 0 0;
    }

    .toolbar-left {
        flex-grow: 1;
    }

    .report-title-label {
        font-size: 16px;
        font-weight: bold;
        color: #333;
        margin: 0;
    }

    .toolbar-right {
        display: flex;
        gap: 10px;
    }

    .btn {
        padding: 6px 12px;
        border: none;
        border-radius: 4px;
        font-size: 12px;
        cursor: pointer;
        text-decoration: none;
        display: inline-block;
        transition: background-color 0.2s;
    }

    .btn-info {
        background-color: #17a2b8;
        color: white;
    }

    .btn-info:hover {
        background-color: #138496;
    }

    .btn-primary {
        background-color: #007bff;
        color: white;
    }

    .btn-primary:hover {
        background-color: #0056b3;
    }

    .btn-secondary {
        background-color: #6c757d;
        color: white;
    }

    .btn-secondary:hover {
        background-color: #545b62;
    }

    .report-content-area {
        padding: 15px;
        min-height: 400px;
    }

    .report-content-panel {
        width: 100%;
    }

    .no-data-panel {
        display: flex;
        justify-content: center;
        align-items: center;
        min-height: 300px;
    }

    .no-data-message {
        text-align: center;
        color: #6c757d;
    }

    .no-data-message i {
        font-size: 48px;
        margin-bottom: 15px;
        color: #adb5bd;
    }

    .no-data-message h3 {
        margin: 10px 0;
        color: #495057;
    }

    .no-data-message p {
        margin: 0;
        font-size: 14px;
    }

    /* Report-specific styles that will be injected */
    .report-table {
        width: 100%;
        border-collapse: collapse;
        margin-top: 20px;
        font-size: 12px;
    }

    .report-table th {
        background-color: #f8f9fa;
        border: 1px solid #dee2e6;
        padding: 8px;
        text-align: left;
        font-weight: bold;
        font-size: 11px;
    }

    .report-table td {
        border: 1px solid #dee2e6;
        padding: 6px 8px;
        font-size: 11px;
    }

    .report-table tr:nth-child(even) {
        background-color: #f8f9fa;
    }

    .report-table tr:hover {
        background-color: #e9ecef;
    }

    .numeric {
        text-align: right;
    }

    .center {
        text-align: center;
    }

    .report-header {
        text-align: center;
        margin-bottom: 20px;
    }

    .report-title {
        font-size: 18px;
        font-weight: bold;
        color: #333;
        margin-bottom: 5px;
    }

    .report-date {
        font-size: 11px;
        color: #666;
    }

    @media print {
        .report-toolbar {
            display: none !important;
        }
        
        .modern-report-container {
            border: none;
            margin: 0;
        }
        
        .report-content-area {
            padding: 0;
        }
    }
</style>

<script type="text/javascript">
    function printReport() {
        // Hide toolbar during print
        var toolbar = document.querySelector('.report-toolbar');
        if (toolbar) {
            toolbar.style.display = 'none';
        }
        
        window.print();
        
        // Show toolbar after print dialog
        setTimeout(function() {
            if (toolbar) {
                toolbar.style.display = 'flex';
            }
        }, 1000);
    }
</script>
