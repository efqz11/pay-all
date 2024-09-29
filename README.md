# Human Resource Management System (HRMS)

## Overview

This **HR Management System (HRMS)** is a comprehensive solution for managing multiple client payroll considering different types of leaves, overtime & pay groups. Payall HRMS provides unique new experience on managing clients, payroll and HR data. With extensive customization and features sufficient to process hassle free multi-client payroll and timesheets easily, confidently and secure.

Payall HRMS is built using **C# .NET 6**, with **MSSQL** as the database, and leverages **CSS/LESS** for styling and **HTML** for the front-end.

---

## Table of Contents

- [Features](#features)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [Screenshots](#screenshots)
- [Support](#support)
- [Folder Structure](#folder-structure)
- [License](#license)

---

## Features

- **Employee Management**: Allows adding, updating, and managing employee records with detailed personal and professional information.
- **Attendance Tracking**: Track employee attendance, leaves, and hours worked.
- **Payroll Management**: Generate payslips, handle salary calculations, deductions, and bonuses.
- **Performance Reviews**: Set performance objectives, track performance, and manage feedback.
- **Secure Document Management**: Upload and manage employee documents.
- **Reports & Analytics**: Create reports on employee performance, payroll, and attendance.

---

## Prerequisites

Ensure you have the following installed:

- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
- [MSSQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
- [Node.js (for LESS compilation, if necessary)](https://nodejs.org/)
- Basic knowledge of SQL and running SQL scripts.

---

## Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/efqz11/pay-all.git
cd pay-all
```

### 2. Install .NET Packages

```bash
dotnet restore
```

## Configuration

### 1. Database Setup

Ensure MSSQL Server is running.

Make sure you update `appsettings.json` in projects `Payroll` and `Payroll.Api` with the appropriate database connection strings and any other configurations.

```
{
  "ConnectionStrings": {
    "PayrollConnection": "Server=localhost;Database=HRMS;User Id=sa;Password=yourpassword;",
    "AccountConnection": "Server=localhost;Database=HRMS;User Id=sa;Password=yourpassword;",
    "LogConnection": "Server=localhost;Database=HRMS;User Id=sa;Password=yourpassword;",
    "HangfireConnection": "Server=localhost;Database=HRMS;User Id=sa;Password=yourpassword;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  }
}
```

### 2. Start the Web Server

To run the .NET project, use the following command:

```bash
dotnet run
```

### 3. Open the application:

Navigate to [https://localhost:44300](https://localhost:44300) in your browser. You should now be able to see the application running.



## Screenshots

1. **Dashboard**

   ![Dashboard Screenshot](/docs//images/employees.png)

2. **Employee Profile**

   ![Employee profile](/docs//images/employee-master.png)

3. **Data Import**

   ![Data Import](/docs//images/import.png)

4. **Job Profile**

   ![Employee profile](/docs//images/jobs.jpg)

5. **Salary & Benefits**

   ![Salary & Benefits](/docs//images/payroll.png)

6. **Overtime**

   ![Overtime](/docs//images/overtime.png)

7. **Attendance and Leave management**

   ![Attendance and Leave management](/docs//images/leave.png)

8. **Other core features**

   8.1 Extensive customizations (especially for leaves)

   8.2 Improved performance by separating operational data with transactional data

   8.3 Managing multiple calculation data sheets and formulas in payroll. Create your own custom metrics to include in payroll.

   8.3 In-house deployment supported

   8.4 Support for post payroll processing

   8.5 Ability to add complex custom calculations during payroll

   8.6 Payall automatically calculate metrics every paycycle which can be used in formulas when calculating payroll

   8.7 Audit trails include table name, changed column name and data stored previously and changed data is stored
   in audit trails for point-in-time tracking reports

   8.8 Ability to support multiple tax identification numbers and add to payroll for auto processing


## Support

If you encounter any issues, please open an issue in the GitHub repository or contact us at efqz11@gmail.com.




## Folder Structure

```bash
├── src/                            # Source folder
│   ├── Payroll/                    # Source for payroll
│       ├── Payroll/                # Main application (admin portal and employee portal)
│       ├── Payroll.Api/            # External API integration with attendance devices and mobile
│       ├── Payroll.Models/         # Database entities, contexts and migrations
│       ├── Payroll.Services/       # Business logic separated to use in both api and app
│       └── Payroll.Website/        # Front end website
├── docs/                           # Docs - mainly for documentation and user manuals
└── backups/                        # Database backups
```

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.