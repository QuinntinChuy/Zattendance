# The World Mission Society Church of God - Attendance System

A beautiful web-based attendance management system with a woods-themed aesthetic (light brown color scheme).

## Features

### Team Leaders
- Mark attendance (Present, Late, Absent) for church services
- Add new members to the system
- View attendance records
- View member list

### Administrators
- All Team Leader features
- Create and manage schedules
- Create and manage groups (Adult, Young Adult, Children)
- Separate male and female groups
- Full system access

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- SQL Server (LocalDB is configured by default)

### Installation

1. Clone or download this repository
2. Navigate to the project directory
3. Restore packages:
   ```
   dotnet restore
   ```
4. Run database migrations:
   ```
   dotnet ef database update
   ```
   (If Entity Framework CLI is not installed: `dotnet tool install --global dotnet-ef`)

5. Run the application:
   ```
   dotnet run
   ```

6. Open your browser and navigate to `https://localhost:5001` (or the port shown in console)

### Default Login Credentials

- **Username:** admin
- **Password:** Admin123!

## Project Structure

```
ChurchAttendance/
├── Controllers/          # MVC Controllers
├── Data/                # Database context and initialization
├── Models/              # Data models
├── Views/               # Razor views
├── wwwroot/css/         # Stylesheets (woods theme)
└── Program.cs           # Application entry point
```

## Database

The application uses Entity Framework Core with SQL Server. On first run, the database will be automatically created and seeded with:
- Default administrator account
- Default groups (Adult, Young Adult, Children - separated by gender)

## Theme

The application features a beautiful woods-themed design with:
- Light brown color palette
- Warm, natural aesthetic
- Responsive design for mobile and desktop
- Modern UI with smooth transitions

## License

© 2024 The World Mission Society Church of God. All rights reserved.
# Zattendance
