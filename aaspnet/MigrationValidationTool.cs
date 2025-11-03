using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

/// <summary>
/// Crystal Reports Migration Validation Tool
/// Verifies successful migration and identifies any remaining issues
/// </summary>
public class MigrationValidationTool
{
    private static readonly string BasePath = @"C:\Users\shiv\Desktop\NewERP";
    
    public static void Main(string[] args)
    {
        Console.WriteLine("🔍 Crystal Reports Migration Validation Tool");
        Console.WriteLine("============================================");
        Console.WriteLine();

        var results = new ValidationResults();

        // 1. Check for remaining Crystal Reports references
        Console.WriteLine("1. Scanning for remaining Crystal Reports references...");
        results.CrystalReportsReferences = ScanForCrystalReportsReferences();

        // 2. Verify Modern Reporting infrastructure
        Console.WriteLine("2. Verifying Modern Reporting infrastructure...");
        results.InfrastructureValidation = ValidateInfrastructure();

        // 3. Check web.config configuration
        Console.WriteLine("3. Validating web.config configuration...");
        results.WebConfigValidation = ValidateWebConfig();

        // 4. Scan for backup files
        Console.WriteLine("4. Checking backup files...");
        results.BackupFiles = ScanForBackupFiles();

        // 5. Validate file integrity
        Console.WriteLine("5. Validating file integrity...");
        results.IntegrityCheck = ValidateFileIntegrity();

        // Display results
        DisplayResults(results);
    }

    /// <summary>
    /// Scan for remaining Crystal Reports references
    /// </summary>
    private static CrystalReportsReferences ScanForCrystalReportsReferences()
    {
        var references = new CrystalReportsReferences();
        
        var aspxFiles = Directory.GetFiles(BasePath, "*.aspx", SearchOption.AllDirectories);
        var csFiles = Directory.GetFiles(BasePath, "*.cs", SearchOption.AllDirectories);
        var configFiles = Directory.GetFiles(BasePath, "*.config", SearchOption.AllDirectories);

        foreach (var file in aspxFiles)
        {
            string content = File.ReadAllText(file);
            if (Regex.IsMatch(content, @"CrystalDecisions|CrystalReportViewer|CrystalReportSource", RegexOptions.IgnoreCase))
            {
                references.AspxFiles.Add(file.Replace(BasePath, ""));
            }
        }

        foreach (var file in csFiles)
        {
            string content = File.ReadAllText(file);
            if (Regex.IsMatch(content, @"using\s+CrystalDecisions|ReportDocument|CrystalReportViewer", RegexOptions.IgnoreCase))
            {
                references.CodeFiles.Add(file.Replace(BasePath, ""));
            }
        }

        foreach (var file in configFiles)
        {
            string content = File.ReadAllText(file);
            if (Regex.IsMatch(content, @"CrystalDecisions", RegexOptions.IgnoreCase) && !content.Contains("<!--"))
            {
                references.ConfigFiles.Add(file.Replace(BasePath, ""));
            }
        }

        return references;
    }

    /// <summary>
    /// Validate Modern Reporting infrastructure
    /// </summary>
    private static InfrastructureValidation ValidateInfrastructure()
    {
        var validation = new InfrastructureValidation();

        // Check required files
        var requiredFiles = new Dictionary<string, string>
        {
            { "ReportHelper.cs", @"ModernReporting\ReportHelper.cs" },
            { "ModernReportViewer.ascx", @"ModernReporting\ModernReportViewer.ascx" },
            { "ModernReportViewer.ascx.cs", @"ModernReporting\ModernReportViewer.ascx.cs" },
            { "ReportViewer.aspx", @"ModernReporting\ReportViewer.aspx" },
            { "ReportViewer.aspx.cs", @"ModernReporting\ReportViewer.aspx.cs" }
        };

        foreach (var file in requiredFiles)
        {
            string fullPath = Path.Combine(BasePath, file.Value);
            validation.RequiredFiles[file.Key] = File.Exists(fullPath);
        }

        // Check for test page
        validation.TestPageExists = File.Exists(Path.Combine(BasePath, "TestModernReporting.aspx"));

        // Check for sample implementation
        validation.SampleImplementationExists = File.Exists(Path.Combine(BasePath, @"Module\MaterialManagement\Reports\RateRegister_Details_Modern.aspx"));

        return validation;
    }

    /// <summary>
    /// Validate web.config configuration
    /// </summary>
    private static WebConfigValidation ValidateWebConfig()
    {
        var validation = new WebConfigValidation();
        string webConfigPath = Path.Combine(BasePath, "web.config");

        if (File.Exists(webConfigPath))
        {
            string content = File.ReadAllText(webConfigPath);
            
            // Check that Crystal Reports assemblies are commented out
            validation.CrystalReportsCommentsedOut = content.Contains("<!-- <add assembly=\"CrystalDecisions.Web") ||
                                                     content.Contains("<!--") && content.Contains("CrystalDecisions");

            // Check for any uncommented Crystal Reports references
            validation.HasUncommentedCrystalReports = Regex.IsMatch(content, 
                @"<add\s+assembly=""CrystalDecisions", RegexOptions.IgnoreCase);

            validation.WebConfigExists = true;
        }
        else
        {
            validation.WebConfigExists = false;
        }

        return validation;
    }

    /// <summary>
    /// Scan for backup files
    /// </summary>
    private static BackupFiles ScanForBackupFiles()
    {
        var backups = new BackupFiles();
        
        var backupFiles = Directory.GetFiles(BasePath, "*.cr_backup", SearchOption.AllDirectories);
        backups.BackupFileCount = backupFiles.Length;
        backups.BackupFiles = backupFiles.Select(f => f.Replace(BasePath, "")).ToList();

        return backups;
    }

    /// <summary>
    /// Validate file integrity
    /// </summary>
    private static IntegrityCheck ValidateFileIntegrity()
    {
        var integrity = new IntegrityCheck();
        
        // Check for broken file references
        var aspxFiles = Directory.GetFiles(BasePath, "*.aspx", SearchOption.AllDirectories);
        
        foreach (var file in aspxFiles)
        {
            try
            {
                string content = File.ReadAllText(file);
                
                // Check for orphaned Crystal Reports controls
                if (content.Contains("CrystalReportViewer") && !content.Contains("ModernReportViewer"))
                {
                    integrity.OrphanedControls.Add(file.Replace(BasePath, ""));
                }

                // Check for missing code-behind files
                string codeFile = file + ".cs";
                if (!File.Exists(codeFile))
                {
                    integrity.MissingCodeBehind.Add(file.Replace(BasePath, ""));
                }
            }
            catch (Exception ex)
            {
                integrity.BrokenFiles.Add($"{file.Replace(BasePath, "")} - {ex.Message}");
            }
        }

        return integrity;
    }

    /// <summary>
    /// Display validation results
    /// </summary>
    private static void DisplayResults(ValidationResults results)
    {
        Console.WriteLine();
        Console.WriteLine("📊 VALIDATION RESULTS");
        Console.WriteLine("=====================");

        // Crystal Reports References
        Console.WriteLine("\n🔍 Crystal Reports References:");
        if (results.CrystalReportsReferences.HasReferences)
        {
            Console.WriteLine($"❌ Found {results.CrystalReportsReferences.TotalCount} remaining references");
            if (results.CrystalReportsReferences.AspxFiles.Count > 0)
            {
                Console.WriteLine($"   ASPX Files ({results.CrystalReportsReferences.AspxFiles.Count}):");
                foreach (var file in results.CrystalReportsReferences.AspxFiles.Take(5))
                    Console.WriteLine($"     - {file}");
                if (results.CrystalReportsReferences.AspxFiles.Count > 5)
                    Console.WriteLine($"     ... and {results.CrystalReportsReferences.AspxFiles.Count - 5} more");
            }
            if (results.CrystalReportsReferences.CodeFiles.Count > 0)
            {
                Console.WriteLine($"   Code Files ({results.CrystalReportsReferences.CodeFiles.Count}):");
                foreach (var file in results.CrystalReportsReferences.CodeFiles.Take(5))
                    Console.WriteLine($"     - {file}");
                if (results.CrystalReportsReferences.CodeFiles.Count > 5)
                    Console.WriteLine($"     ... and {results.CrystalReportsReferences.CodeFiles.Count - 5} more");
            }
        }
        else
        {
            Console.WriteLine("✅ No Crystal Reports references found");
        }

        // Infrastructure
        Console.WriteLine("\n🏗️  Modern Reporting Infrastructure:");
        foreach (var file in results.InfrastructureValidation.RequiredFiles)
        {
            Console.WriteLine($"   {(file.Value ? "✅" : "❌")} {file.Key}");
        }
        Console.WriteLine($"   {(results.InfrastructureValidation.TestPageExists ? "✅" : "❌")} Test Page");
        Console.WriteLine($"   {(results.InfrastructureValidation.SampleImplementationExists ? "✅" : "❌")} Sample Implementation");

        // Web.config
        Console.WriteLine("\n⚙️  Web.config Validation:");
        if (results.WebConfigValidation.WebConfigExists)
        {
            Console.WriteLine($"   {(results.WebConfigValidation.CrystalReportsCommentsedOut ? "✅" : "❌")} Crystal Reports assemblies commented out");
            Console.WriteLine($"   {(!results.WebConfigValidation.HasUncommentedCrystalReports ? "✅" : "❌")} No active Crystal Reports references");
        }
        else
        {
            Console.WriteLine("   ❌ web.config not found");
        }

        // Backups
        Console.WriteLine("\n💾 Backup Files:");
        Console.WriteLine($"   ✅ {results.BackupFiles.BackupFileCount} backup files created");

        // Integrity
        Console.WriteLine("\n🔧 File Integrity:");
        Console.WriteLine($"   {(results.IntegrityCheck.OrphanedControls.Count == 0 ? "✅" : "❌")} Orphaned controls: {results.IntegrityCheck.OrphanedControls.Count}");
        Console.WriteLine($"   {(results.IntegrityCheck.MissingCodeBehind.Count == 0 ? "✅" : "❌")} Missing code-behind: {results.IntegrityCheck.MissingCodeBehind.Count}");
        Console.WriteLine($"   {(results.IntegrityCheck.BrokenFiles.Count == 0 ? "✅" : "❌")} Broken files: {results.IntegrityCheck.BrokenFiles.Count}");

        // Overall Status
        Console.WriteLine("\n🎯 OVERALL MIGRATION STATUS:");
        bool migrationComplete = !results.CrystalReportsReferences.HasReferences &&
                                results.InfrastructureValidation.IsComplete &&
                                results.WebConfigValidation.IsValid &&
                                results.IntegrityCheck.IsValid;

        if (migrationComplete)
        {
            Console.WriteLine("✅ MIGRATION SUCCESSFUL - Ready for production deployment!");
        }
        else
        {
            Console.WriteLine("⚠️  MIGRATION INCOMPLETE - Please address the issues above");
        }

        Console.WriteLine("\n📋 Next Steps:");
        if (results.CrystalReportsReferences.HasReferences)
        {
            Console.WriteLine("   1. Complete migration of remaining Crystal Reports files");
        }
        if (!results.InfrastructureValidation.IsComplete)
        {
            Console.WriteLine("   2. Install missing Modern Reporting infrastructure files");
        }
        if (!results.WebConfigValidation.IsValid)
        {
            Console.WriteLine("   3. Fix web.config Crystal Reports references");
        }
        if (!results.IntegrityCheck.IsValid)
        {
            Console.WriteLine("   4. Fix file integrity issues");
        }
        Console.WriteLine("   5. Test the application thoroughly");
        Console.WriteLine("   6. Deploy to staging environment");
    }

    #region Data Classes

    public class ValidationResults
    {
        public CrystalReportsReferences CrystalReportsReferences { get; set; }
        public InfrastructureValidation InfrastructureValidation { get; set; }
        public WebConfigValidation WebConfigValidation { get; set; }
        public BackupFiles BackupFiles { get; set; }
        public IntegrityCheck IntegrityCheck { get; set; }
    }

    public class CrystalReportsReferences
    {
        public List<string> AspxFiles { get; set; } = new List<string>();
        public List<string> CodeFiles { get; set; } = new List<string>();
        public List<string> ConfigFiles { get; set; } = new List<string>();
        
        public int TotalCount => AspxFiles.Count + CodeFiles.Count + ConfigFiles.Count;
        public bool HasReferences => TotalCount > 0;
    }

    public class InfrastructureValidation
    {
        public Dictionary<string, bool> RequiredFiles { get; set; } = new Dictionary<string, bool>();
        public bool TestPageExists { get; set; }
        public bool SampleImplementationExists { get; set; }
        
        public bool IsComplete => RequiredFiles.All(f => f.Value) && TestPageExists && SampleImplementationExists;
    }

    public class WebConfigValidation
    {
        public bool WebConfigExists { get; set; }
        public bool CrystalReportsCommentsedOut { get; set; }
        public bool HasUncommentedCrystalReports { get; set; }
        
        public bool IsValid => WebConfigExists && CrystalReportsCommentsedOut && !HasUncommentedCrystalReports;
    }

    public class BackupFiles
    {
        public int BackupFileCount { get; set; }
        public List<string> BackupFiles { get; set; } = new List<string>();
    }

    public class IntegrityCheck
    {
        public List<string> OrphanedControls { get; set; } = new List<string>();
        public List<string> MissingCodeBehind { get; set; } = new List<string>();
        public List<string> BrokenFiles { get; set; } = new List<string>();
        
        public bool IsValid => OrphanedControls.Count == 0 && MissingCodeBehind.Count == 0 && BrokenFiles.Count == 0;
    }

    #endregion
}
