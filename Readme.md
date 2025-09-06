# Personal Blogging App

A Twitter-like blogging application built with ASP.NET Core MVC, Entity Framework, and SQL Server.

admin@blog.com / Admin123!

## Features

- Master account for creating blog posts
- User registration and authentication for comments/likes
- Markdown support for blog posts
- Hierarchical comments with replies
- Like functionality for posts and comments
- Responsive Twitter-like UI
- Real-time interactions with AJAX

## Ubuntu Linux Dependencies

To run this application on Ubuntu Linux, install the following dependencies:

### 1. .NET 8 SDK
```bash
# Add Microsoft package repository
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update

# Install .NET 8 SDK
sudo apt-get install -y dotnet-sdk-8.0
```

### 2. SQL Server (Ubuntu)
```bash
# Add Microsoft SQL Server repository
wget -qO- https://packages.microsoft.com/keys/microsoft.asc | sudo apt-key add -
sudo add-apt-repository "$(wget -qO- https://packages.microsoft.com/config/ubuntu/22.04/mssql-server-2022.list)"

# Install SQL Server
sudo apt-get update
sudo apt-get install -y mssql-server

# Configure SQL Server
sudo /opt/mssql/bin/mssql-conf setup

# Install SQL Server command-line tools
curl https://packages.microsoft.com/keys/microsoft.asc | sudo apt-key add -
curl https://packages.microsoft.com/config/ubuntu/22.04/prod.list | sudo tee /etc/apt/sources.list.d/msprod.list
sudo apt-get update
sudo apt-get install mssql-tools unixodbc-dev

# Add to PATH
echo 'export PATH="$PATH:/opt/mssql-tools/bin"' >> ~/.bashrc
source ~/.bashrc
```

### 3. Alternative: Use SQLite (Simpler Setup)
For easier setup, you can use SQLite instead. Update `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=blog.db"
  }
}
```

And install SQLite package:
```bash
dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 8.0.0
```

Then update `Program.cs`:
```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
```

### 4. Additional System Dependencies
```bash
# Update system
sudo apt-get update && sudo apt-get upgrade -y

# Install essential tools
sudo apt-get install -y curl wget git build-essential

# Install Node.js and npm (for any frontend build tools)
curl -fsSL https://deb.nodesource.com/setup_18.x | sudo -E bash -
sudo apt-get install -y nodejs
```

## Setup Instructions

1. **Clone and Setup Project**
```bash
git clone <your-repo>
cd BlogApp
dotnet restore
```

2. **Database Migration**
```bash
# Create initial migration
dotnet ef migrations add InitialCreate

# Update database
dotnet ef database update
```

3. **Run the Application**
```bash
dotnet run
```

4. **Access the Application**
- Open browser and go to `https://localhost:5001` or `http://localhost:5000`
- Master account: `admin@blog.com` / `Admin123!`

## Default Accounts

- **Master Account**: admin@blog.com / Admin123! (Can create posts)
- **Regular Users**: Register new accounts (Can comment and like only)

## Project Structure

```
BlogApp/
├── Controllers/          # MVC Controllers
├── Models/              # Entity Models
├── Views/               # Razor Views
├── Data/                # DbContext and Migrations
├── wwwroot/             # Static files (CSS, JS)
├── appsettings.json     # Configuration
└── Program.cs           # Application entry point
```

## Technologies Used

- ASP.NET Core 8 MVC
- Entity Framework Core
- ASP.NET Core Identity
- SQL Server / SQLite
- Bootstrap 5
- Font Awesome
- Markdig (Markdown parsing)
- jQuery for AJAX interactions