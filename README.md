# Onboarding KYC System

A full-stack web application designed for automated identity document processing (KYC), leveraging artificial intelligence for OCR data extraction, a relational database with Stored Procedures for persistent storage, and public cloud deployment.

---

## Tech Stack & Architecture Justification

* **Frontend**: React (Vite) + Bootstrap + Tailwind CSS
  * *Justification*: Provides a responsive, component-driven user interface (`KycView`, `ClientesView`, `Navbar`, `Sidebar`) allowing users to input their email, upload identity documents, monitor loading states in real-time, and view extracted results alongside historical records.
* **Backend**: ASP.NET Core Web API (.NET / C#)
  * *Justification*: Chosen for its high performance, robust dependency injection architecture, strict typing, and seamless handling of multipart form-data for file uploads (`KycController`, `KycService`).
* **AI & OCR Integration**: Google Gemini Flash API (`gemini-flash-latest`)
  * *Justification*: Highly accurate multimodal capabilities allowing fast, low-latency extraction of structured JSON fields (FullName, DocumentNumber, Confidence) directly from raw document images passed in Base64.
* **Database & Persistence**: MySQL + Stored Procedures (`sp_InsertOnboardingRecord`)
  * *Justification*: Ensures relational data integrity, clean separation of database operations from backend business logic, and efficient handling of historical transaction logs.
* **Cloud Hosting**: Railway
  * *Justification*: Provides streamlined CI/CD pipeline integration with GitHub for continuous deployment of containerized .NET backends and robust environment variable management.

---

## Project Structure

```text
/
├── backend/
│   ├── Controllers/           # API Endpoints (KycController)
│   ├── Models/                # Data transfer and response models (KycRequest, KycResponseModel)
│   ├── Service/               # Business logic & Gemini integration (KycService, IKycService)
│   ├── wwwroot/uploads/       # Physical static storage for uploaded documents
│   └── Program.cs             # DI, CORS, and Static files configuration
└── frontend/
    ├── src/api/               # API service handlers (kycService.js)
    ├── src/components/        # UI components (Navbar, Sidebar)
    └── src/views/             # Page views (KycView, ClientesView)
```
## Getting Started Locally

Follow these steps to set up and run the project locally on your machine.

### Prerequisites
* **.NET SDK** installed on your machine.
* **MySQL Server** (or MySQL Workbench / Azure Data Studio).
* A valid **Google Gemini API Key**.

---

### 1. Database Setup
Open your MySQL management tool and execute the following SQL script to create the database, table, and the stored procedure:

```sql
CREATE DATABASE IF NOT EXISTS onboardingkyc_db;
USE onboardingkyc_db;

CREATE TABLE ClientsKyc (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Email VARCHAR(150) NOT NULL,
    FullName VARCHAR(250) NULL,
    DocumentNumber VARCHAR(50) NULL,
    OcrConfidence DECIMAL(5,2) NULL,
    ImageUrl TEXT NULL,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);

DELIMITER //
CREATE PROCEDURE sp_InsertOnboardingRecord (
    IN p_Email VARCHAR(150),
    IN p_FullName VARCHAR(250),
    IN p_DocumentNumber VARCHAR(50),
    IN p_OcrConfidence DECIMAL(5,2),
    IN p_ImageUrl TEXT
)
BEGIN
    INSERT INTO ClientsKyc (Email, FullName, DocumentNumber, OcrConfidence, ImageUrl, CreatedAt)
    VALUES (p_Email, p_FullName, p_DocumentNumber, p_OcrConfidence, p_ImageUrl, NOW());
    
    SELECT LAST_INSERT_ID() AS NewId;
END //
DELIMITER ;
```

### 2. Configure Backend Environment
Navigate to your backend project directory and configure your local MySQL connection string and Google Gemini API key inside your `appsettings.json` file (or using user secrets):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=s_voto_lima;Uid=root;Pwd=your_mysql_password;"
  },
  "GeminiSettings": {
    "ApiKey": "AQ.Ab8RN6LqgdjqltKDDdQ-_mFzwh9xe_O5eAZjwWSoZzC0H0yQCQ"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}

```
### 3. Run the Backend API
Open a terminal in the backend directory, restore dependencies, and start the application:

```bash
cd backend
dotnet restore
dotnet run
```

### 4. Run the Frontend Application
Open a separate terminal in the frontend directory, install dependencies, and launch the development server:

```bash
cd frontend
npm install
npm run dev
