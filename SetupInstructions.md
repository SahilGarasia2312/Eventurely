# Project Setup Instructions

This file contains the necessary commands to set up the project dependencies, create the database schema with correct migrations, and run the project.

## 1. Clone the repository

```bash
git clone <repository-url>
cd Eventurely
```

## 2. Install dependencies

```bash
dotnet restore
```

## 3. Create and apply migrations

Since the project uses ASP.NET Core Identity with int keys, ensure the migrations are created correctly.

If you need to recreate the migrations for Identity tables with int keys, follow these steps:

### Remove existing migrations (optional, if you want a clean slate)

```bash
rm -rf Migrations
```

### Create initial migration

```bash
dotnet ef migrations add InitialCreate
```

### Apply migrations to the database

```bash
dotnet ef database update
```

## 4. Run the project

```bash
dotnet run
```

## Notes

- Ensure your MySQL database connection string in `appsettings.json` is correctly configured.
- The migrations will create the Identity tables with int keys as per the current model.
- If you encounter issues with existing database schema, consider dropping the database and recreating it with the migrations above.

This setup will allow your team members to clone the project and get it running with the correct database schema and dependencies.
