# URGENT: IIS Setup Instructions for NewERP ASP.NET Application

## PROBLEM IDENTIFIED:
- This is an ASP.NET application that REQUIRES IIS server
- Login.aspx needs proper ASP.NET processing to route to Default.aspx  
- Cannot run with Python servers or file:// protocol

## SOLUTION - IIS SETUP (Run as Administrator):

### Step 1: Enable IIS Features
```powershell
# Run PowerShell as Administrator
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServerRole -All
Enable-WindowsOptionalFeature -Online -FeatureName IIS-ASPNET45 -All  
Enable-WindowsOptionalFeature -Online -FeatureName IIS-NetFxExtensibility45 -All
Enable-WindowsOptionalFeature -Online -FeatureName IIS-ISAPIExtensions -All
Enable-WindowsOptionalFeature -Online -FeatureName IIS-ISAPIFilter -All
```

### Step 2: Register ASP.NET with IIS
```powershell
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\aspnet_regiis.exe -i
```

### Step 3: Create IIS Application
```powershell
Import-Module WebAdministration
New-WebApplication -Site "Default Web Site" -Name "NewERP" -PhysicalPath "C:\Users\shiv\Desktop\NewERP"
```

### Step 4: Set Application Pool to .NET 4.0
```powershell
Set-ItemProperty -Path "IIS:\Sites\Default Web Site\NewERP" -Name "applicationPool" -Value "DefaultAppPool"
Set-ItemProperty -Path "IIS:\AppPools\DefaultAppPool" -Name "managedRuntimeVersion" -Value "v4.0"
```

### Step 5: Access Application
http://localhost/NewERP/Login.aspx

## EXPECTED FLOW:
1. Login.aspx loads with proper ASP.NET processing
2. Database connection works (LocalDB)
3. Authentication succeeds with sapl0002/Sapl@0002
4. Redirects to Default.aspx (which uses MasterPage.master)
5. Full ERP functionality available

## FILES VERIFIED:
- ✅ Login.aspx (19 ASP.NET controls)
- ✅ Default.aspx (uses MasterPage.master)
- ✅ web.config (configured for .NET 4.8)
- ✅ Database files (ERP_DB.mdf in App_Data)
- ✅ 15 ERP modules ready

## CURRENT STATUS:
❌ BLOCKED - Needs Administrator privileges for IIS setup
✅ APPLICATION READY - All files properly configured for IIS deployment
