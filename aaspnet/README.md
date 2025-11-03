# NewERP Application Setup Guide

This guide provides step-by-step instructions for setting up and running the NewERP application on a Windows system.

## Prerequisites

- Windows 10 or Windows Server 2016/2019/2022
- .NET Framework 4.8 installed
- SQL Server LocalDB (2016, 2017, or 2019)
- Administrator access to run PowerShell scripts

## Setup Instructions

### Option 1: Using IIS Express (Recommended for Development)

This is the simplest approach for development and testing:

1. **Install IIS Express** (if not already installed):
   ```powershell
   # Run this in PowerShell as Administrator
   .\Install-IISExpress.ps1
   ```

2. **Install SQL Server LocalDB** (if not already installed):
   ```powershell
   # Run this in PowerShell as Administrator
   .\Install-SqlLocalDB.ps1
   ```

3. **Start the Application**:
   ```powershell
   # Run this in PowerShell
   .\Start-Application-IISExpress.ps1
   ```

   This will:
   - Start SQL Server LocalDB
   - Configure IIS Express
   - Start the application on http://localhost:8080/
   - Open a browser window with the application

### Option 2: Using Full IIS (Recommended for Production)

This approach is better for production or shared environments:

1. **Install SQL Server LocalDB** (if not already installed):
   ```powershell
   # Run this in PowerShell as Administrator
   .\Install-SqlLocalDB.ps1
   ```

2. **Set Up IIS and Database**:
   ```powershell
   # Run this in PowerShell as Administrator
   .\Setup-IIS-and-Database.ps1
   ```

   This script will:
   - Check if IIS is installed and install it if needed
   - Check if ASP.NET 4.8 is registered with IIS
   - Create an application pool for the ERP application
   - Create an application in IIS
   - Set appropriate permissions on the App_Data folder
   - Open the application in a browser

3. **Verify Database Connection**:
   ```powershell
   # Run this in PowerShell
   .\Check-Database-Connection.ps1
   ```

## Accessing the Application

After setup is complete, you can access the application at:

- **IIS Express**: http://localhost:8080/
- **Full IIS**: http://localhost/NewERP/

Use the following credentials to log in:
- Username: sapl0002
- Password: Sapl@0002

## Troubleshooting

### Database Connection Issues

If you encounter database connection issues:

1. Make sure SQL Server LocalDB is installed and running:
   ```powershell
   SqlLocalDB info MSSQLLocalDB
   SqlLocalDB start MSSQLLocalDB
   ```

2. Check if the database file exists in the App_Data folder and has proper permissions.

3. Verify the connection string in web.config is correct:
   ```xml
   <add name="LocalSqlServer" 
        connectionString="Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFileName=|DataDirectory|\ERP_DB.mdf;Integrated Security=True;Pooling=False;MultipleActiveResultSets=true" 
        providerName="System.Data.SqlClient" />
   ```

### IIS Express Issues

If you encounter issues with IIS Express:

1. Make sure IIS Express is installed correctly:
   ```powershell
   # Run this in PowerShell as Administrator
   .\Install-IISExpress.ps1
   ```

2. Check if any other application is using port 8080. If so, modify the port in Start-Application-IISExpress.ps1.

3. Try running Visual Studio's IIS Express configuration:
   ```powershell
   "C:\Program Files\IIS Express\appcmd.exe" list site
   ```

### Full IIS Issues

If you encounter issues with full IIS:

1. Make sure IIS is installed and ASP.NET 4.8 is registered:
   ```powershell
   dism /online /enable-feature /featurename:IIS-WebServerRole
   dism /online /enable-feature /featurename:IIS-ASPNET45 /all
   ```

2. Verify the application pool is running and configured to use .NET 4.0.

3. Check IIS logs for errors (usually located at C:\inetpub\logs\LogFiles).

## Migration Notes

This application has been migrated from .NET Framework 3.5 to 4.8. For details about the migration process and any known issues, please refer to `Migration-Summary.md`.

# NewERP Database Configuration

This document outlines the steps taken to set up and configure the database for the NewERP system.

## Database Overview

The NewERP system uses SQL Server Express for its database. We've created two databases:

1. `ERP_DB_Test`: A test database with essential tables for an ERP system
2. `ERP_DB_Real`: The original database with all 360 tables successfully attached

## Database Structure

### Test Database (ERP_DB_Test)

The test database includes the following tables:

- **Users**: Stores user account information
- **Products**: Stores product information
- **Customers**: Stores customer information
- **Orders**: Stores order header information
- **OrderDetails**: Stores order line items

### Real Database (ERP_DB_Real)

The real database was successfully attached and contains all 360 tables from the original ERP system. This database includes all the actual business data, configuration settings, and system tables.

## Setup Process

### 1. Initial Permission Issues

We encountered permission issues when trying to attach the existing database file (`ERP_DB.mdf`) from the App_Data folder. Despite having the following permissions set:

```
App_Data\ERP_DB.mdf:
- NT AUTHORITY\NETWORK SERVICE:(F)
- BUILTIN\Users:(F)
- Everyone:(F)
- NT SERVICE\MSSQLSERVER:(F)
- NT SERVICE\MSSQL$SQLEXPRESS:(F)
- BUILTIN\IIS_IUSRS:(I)(F)
- NT AUTHORITY\SYSTEM:(I)(F)
- BUILTIN\Administrators:(I)(F)
```

The SQL Server service account (`NT SERVICE\MSSQL$SQLEXPRESS`) still couldn't access the file due to underlying Windows security constraints.

### 2. Test Database Solution

As a workaround, we:

1. Created a new database called `ERP_DB_Test`
2. Created the necessary tables for the ERP system
3. Added sample data for testing
4. Created a test page to verify connectivity

### 3. Original Database Attachment

Eventually, we successfully attached the original database by:

1. Copying the database files to a temporary location (C:\Temp\TestDB)
2. Using the SQL ATTACH command which upgraded the database from version 611 to 904
3. This created the `ERP_DB_Real` database containing all 360 original tables

### 4. Connection String

The application now uses the following connection string to connect to the real database:

```xml
<connectionStrings>
  <remove name="LocalSqlServer" />
  <add name="LocalSqlServer" 
       connectionString="Data Source=.\SQLEXPRESS;Initial Catalog=ERP_DB_Real;Integrated Security=True;MultipleActiveResultSets=True;Pooling=True;Max Pool Size=200" 
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

## Testing the Database Connection

Two test pages have been created to verify database connections:

- **DBConnectionTest.aspx**: Displays test database connection status and table information
- **RealDBTest.aspx**: Displays real database connection status and allows exploring the 360 tables

## Files Created

1. **CreateERPTables.sql**: SQL script to create test database tables
2. **DatabaseStructure.sql**: SQL script to document database structure
3. **ERPDatabaseStructure.txt**: Output of database structure documentation
4. **DBConnectionTest.aspx**: Test page for test database connection
5. **RealDBTest.aspx**: Test page for real database connection
6. **Default.aspx**: Updated landing page with links to both database test pages

## Permissions Issue Analysis

The core issue preventing the direct attachment of the existing database file was related to Windows security layering:

1. **Service Account Isolation**: The `NT SERVICE\` accounts have special security contexts
2. **File Ownership**: The original database files had ownership/ACL structures that override explicit permission grants
3. **Path Resolution**: SQL Server has specific requirements for file path accessibility

Moving the database files to a different location (C:\Temp\TestDB) and then attaching them allowed SQL Server to establish proper ownership and permissions, resolving these complex issues. 