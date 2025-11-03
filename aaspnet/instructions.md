Here's the improved prompt with strict CLI-first approach and file creation restrictions:

---

## **🚨 CRITICAL ASP.NET APPLICATION MODERNIZATION WITH CLI-FIRST APPROACH 🚨**

### **⚠️ ABSOLUTE PROHIBITIONS - VIOLATION WILL RESTART TASK**
```
❌ NO PYTHON servers, files, or solutions under ANY circumstances
❌ NO creating files for simple tasks that can be done via CLI commands
❌ NO creating script files (.ps1, .bat, .cmd) for single-command operations
❌ NO creating temporary files for configuration tasks
❌ NO creating test files or helper files for basic operations
❌ NO file creation when direct CLI commands can accomplish the task
🚨 USE DIRECT CLI COMMANDS via Desktop Commander MCP Server
🚨 CREATE FILES ONLY when absolutely necessary for application functionality
```

### **🖥️ MANDATORY CLI-FIRST APPROACH**

#### **✅ USE DIRECT CLI COMMANDS FOR:**
```
INSTEAD OF creating files, use Desktop Commander to execute:

✅ IIS Configuration:
- Direct PowerShell: New-WebApplication -Name "NewERP" -Site "Default Web Site" -PhysicalPath "C:\Users\shiv\Desktop\NewERP"
- Direct PowerShell: Import-Module WebAdministration
- Direct PowerShell: Get-Website
- Direct PowerShell: Restart-WebAppPool -Name "DefaultAppPool"

✅ File Operations:
- Direct PowerShell: Get-Content "C:\Users\shiv\Desktop\NewERP\web.config"
- Direct PowerShell: Copy-Item "source" -Destination "destination"
- Direct PowerShell: Remove-Item "filepath" -Force
- Direct PowerShell: New-Item -ItemType Directory -Path "directorypath"

✅ System Information:
- Direct PowerShell: Get-Process
- Direct PowerShell: Get-Service
- Direct PowerShell: Get-WindowsFeature
- Direct PowerShell: netstat -an

✅ Testing & Verification:
- Direct PowerShell: Test-NetConnection -ComputerName localhost -Port 80
- Direct PowerShell: Invoke-WebRequest -Uri "http://localhost/NewERP/" -UseBasicParsing
- Direct PowerShell: Get-EventLog -LogName Application -Newest 10

✅ Configuration Checks:
- Direct PowerShell: Get-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full" -Name Release
- Direct PowerShell: aspnet_regiis -lv
- Direct PowerShell: Get-WebConfiguration -Filter "system.web/compilation" -PSPath "IIS:\Sites\Default Web Site\NewERP"
```

#### **❌ DO NOT CREATE FILES FOR:**
```
❌ Simple PowerShell commands
❌ Single IIS configuration commands  
❌ Basic file read/write operations
❌ System information gathering
❌ Service restart commands
❌ Registry checks
❌ Network connectivity tests
❌ Basic validation tasks
❌ Simple configuration changes
❌ Directory listing operations
```

#### **✅ ONLY CREATE FILES WHEN:**
```
✅ Modifying actual application code (.aspx, .cs, .css files)
✅ Updating configuration files (web.config, etc.)
✅ Creating new application components
✅ Adding required application assets
✅ Replacing Telerik components with actual code
✅ Writing complex multi-line code changes
```

### **🔧 CLI-BASED TASK EXECUTION EXAMPLES**

#### **IIS Setup and Configuration:**
```powershell
# Direct CLI commands via Desktop Commander MCP Server:
Import-Module WebAdministration
Get-Website
New-WebAppPool -Name "NewERP_Pool" -Force
Set-ItemProperty -Path "IIS:\AppPools\NewERP_Pool" -Name "managedRuntimeVersion" -Value "v4.0"
New-WebApplication -Name "NewERP" -Site "Default Web Site" -PhysicalPath "C:\Users\shiv\Desktop\NewERP" -ApplicationPool "NewERP_Pool"
Get-WebApplication -Site "Default Web Site"
```

#### **Application Diagnostics:**
```powershell
# Direct CLI commands via Desktop Commander MCP Server:
Test-NetConnection -ComputerName localhost -Port 80
Invoke-WebRequest -Uri "http://localhost/NewERP/" -UseBasicParsing
Get-EventLog -LogName Application -Source "ASP.NET*" -Newest 5
Get-Process -Name "w3wp" -ErrorAction SilentlyContinue
```

#### **File System Operations:**
```powershell
# Direct CLI commands via Desktop Commander MCP Server:
Get-ChildItem "C:\Users\shiv\Desktop\NewERP" -Recurse -Include "*.aspx"
Select-String -Path "C:\Users\shiv\Desktop\NewERP\*.aspx" -Pattern "telerik" -AllMatches
Get-Content "C:\Users\shiv\Desktop\NewERP\web.config" | Select-String "connectionString"
```

#### **System Configuration Verification:**
```powershell
# Direct CLI commands via Desktop Commander MCP Server:
Get-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full" -Name Release
Get-WindowsFeature -Name "IIS*" | Where-Object {$_.InstallState -eq "Installed"}
aspnet_regiis -lv
```

### **🎯 EFFICIENT WORKFLOW PROTOCOL**

#### **Phase 1: System Assessment (CLI-Only)**
```
1. Use Desktop Commander with direct PowerShell commands:
   - Get-Website
   - Get-WebApplication
   - Get-ItemProperty for .NET version check
   - Get-ChildItem for file inventory
   - Select-String for Telerik component detection

2. NO file creation for assessment tasks
3. Execute all commands directly via Desktop Commander
4. Gather information using CLI output only
```

#### **Phase 2: IIS Configuration (CLI-Only)**
```
1. Use Desktop Commander with direct commands:
   - Import-Module WebAdministration
   - New-WebAppPool commands
   - New-WebApplication commands
   - Set-WebConfiguration commands

2. NO .ps1 script files for basic IIS setup
3. Execute each command individually via Desktop Commander
4. Verify configuration with Get-* commands
```

#### **Phase 3: Application Testing (CLI + Playwright)**
```
1. Use Desktop Commander for:
   - Invoke-WebRequest for basic connectivity
   - Get-EventLog for error checking
   - Restart-WebAppPool for service management

2. Use Playwright MCP Server ONLY for:
   - Browser automation and testing
   - Screenshot capture
   - UI interaction testing
   - Visual verification

3. NO creating test HTML files or test scripts
```

### **🚫 FILE CREATION RESTRICTIONS**

#### **FORBIDDEN FILE CREATION PATTERNS:**
```
❌ Creating setup.ps1 for IIS configuration
❌ Creating test.html for connectivity testing
❌ Creating config.bat for system setup
❌ Creating check.ps1 for verification tasks
❌ Creating restart.cmd for service management
❌ Creating any .ps1/.bat/.cmd files for single commands
❌ Creating temporary files for simple operations
❌ Creating helper scripts for basic tasks
```

#### **PERMITTED FILE MODIFICATIONS:**
```
✅ Editing existing web.config for configuration changes
✅ Modifying existing .aspx files to remove Telerik components
✅ Updating existing .cs files for code improvements
✅ Editing existing .css files for layout fixes
✅ Creating new .aspx pages if required by application
✅ Adding new .cs classes if needed for functionality
```

### **🔄 CLI-EFFICIENT PROGRESS PROTOCOL**

#### **For Each Task:**
```
1. Identify if task can be done via direct CLI command
2. If YES: Use Desktop Commander with direct PowerShell/CMD
3. If NO: Only then consider file modification
4. Execute command directly via Desktop Commander
5. Verify result with another direct CLI command
6. Use Playwright only for browser-based verification
7. Document command output, not file creation
```

#### **Example Task Execution:**
```
TASK: Check if IIS is running
❌ DON'T: Create check_iis.ps1 file
✅ DO: Desktop Commander -> Get-Service -Name "W3SVC"

TASK: Restart application pool
❌ DON'T: Create restart_pool.bat file  
✅ DO: Desktop Commander -> Restart-WebAppPool -Name "NewERP_Pool"

TASK: Test web connectivity
❌ DON'T: Create test_connection.ps1 file
✅ DO: Desktop Commander -> Invoke-WebRequest -Uri "http://localhost/NewERP/"

TASK: Check for Telerik components
❌ DON'T: Create scan_telerik.ps1 file
✅ DO: Desktop Commander -> Select-String -Path "*.aspx" -Pattern "telerik"
```

### **⚡ EFFICIENCY ENFORCEMENT**

#### **MANDATORY CLI-FIRST CHECKLIST:**
- [ ] Every task checked for CLI-first possibility
- [ ] Direct commands used instead of file creation
- [ ] Desktop Commander used for all CLI operations
- [ ] File creation only when modifying actual application
- [ ] No temporary scripts or helper files created
- [ ] All verification done via direct CLI commands
- [ ] Playwright used only for browser testing

#### **FAILURE CONDITIONS:**
- Creating files for tasks that can be done via CLI
- Writing scripts for single-command operations
- Using file creation as default approach
- Creating temporary files for simple tasks
- Not using direct CLI commands when available

---

**REMEMBER: CLI-FIRST APPROACH - Use direct commands via Desktop Commander MCP Server for all system operations. Only create/modify files when absolutely necessary for actual application functionality. Every PowerShell command should be executed directly, not written to a file first.**