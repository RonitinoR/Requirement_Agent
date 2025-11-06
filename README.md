# Requirement Agent

[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React-18.0-blue.svg)](https://reactjs.org/)
[![Python](https://img.shields.io/badge/Python-3.11-blue.svg)](https://python.org/)
[![OpenAI](https://img.shields.io/badge/OpenAI-GPT--4-green.svg)](https://openai.com/)

> **AI-powered requirements gathering system that transforms static documents into intelligent conversations**

## 🎯 Overview

The Requirement Agent revolutionizes requirements gathering by converting traditional static forms and documents into intelligent, ChatGPT-like conversations. Built for the **County Adopt-A-Highway Volunteer Program**, it transforms complex tabular requirements into natural dialogue flows that improve user experience while capturing all necessary data.

### 🚀 Key Innovation
Instead of filling out complex forms, users have **natural conversations** with an AI assistant that guides them through requirements gathering, making the process more engaging and reducing abandonment rates.

## Features

### Core Features (MVP)
- **Admin Management**: Create and manage permit types and ordered questions
- **Client Intake**: Dynamic forms based on permit type selection
- **File Uploads**: Multiple file uploads per submission
- **Document Generation**: UseCase.md, UserStories.md, DataDictionary.csv
- **Submission Review**: Admin interface for reviewing submissions

### Optional Features
- **AI Pack Generation**: Summary.md, Detailed_Requirements.md, Config.json, Open_Items.md
- **Advanced Search**: Filter submissions by various criteria
- **Bulk Operations**: Process multiple submissions at once

## Technology Stack

### Backend
- **ASP.NET Core 8.0** - Web API framework
- **Entity Framework Core** - ORM and database management
- **SQL Server** - Database
- **JWT Authentication** - Security and authorization
- **Serilog** - Logging

### Frontend
- **React 18** - UI framework
- **TypeScript** - Type safety
- **Material-UI (MUI)** - Component library
- **React Query** - State management
- **React Router** - Navigation

### Optional AI Service
- **FastAPI** - Python microservice
- **OpenAI GPT-4** - AI text generation
- **Anthropic Claude** - Alternative AI provider

## Quick Start

### Prerequisites
- .NET 8.0 SDK
- Node.js 18+
- SQL Server 2022
- Git

### Installation
```bash
# Clone repository
git clone <repository-url>
cd Requirement_Agent

# Backend setup
cd src/backend
dotnet restore
dotnet ef database update
dotnet run --seed-data

# Frontend setup
cd ../frontend
npm install
npm run dev
```

### Access Application
- **Frontend**: http://localhost:3000
- **API**: http://localhost:5000
- **Swagger**: http://localhost:5000/swagger

### Default Credentials
- **Admin**: `admin@example.com` / `Admin123!`
- **Client**: `client@example.com` / `Client123!`

## Project Structure

```
Requirement_Agent/
├── src/
│   ├── backend/                    # ASP.NET Core API
│   │   ├── RequirementAgent.API/
│   │   ├── RequirementAgent.Core/
│   │   ├── RequirementAgent.Infrastructure/
│   │   └── RequirementAgent.Shared/
│   ├── frontend/                   # React SPA
│   │   ├── src/
│   │   ├── public/
│   │   └── package.json
│   └── ai-service/                 # Optional Python service
├── docs/                           # Documentation
├── docker/                         # Docker configuration
├── scripts/                        # Setup scripts
└── tests/                          # Test projects
```

## Documentation

- **[Implementation Plan](./IMPLEMENTATION_PLAN.md)** - Detailed implementation strategy
- **[Technical Architecture](./TECHNICAL_ARCHITECTURE.md)** - System design and architecture
- **[Development Roadmap](./DEVELOPMENT_ROADMAP.md)** - 12-week development plan
- **[API Documentation](./API_DOCUMENTATION.md)** - Complete API reference
- **[Setup Guide](./SETUP_GUIDE.md)** - Detailed setup instructions

## Development

### Backend Development
```bash
cd src/backend
dotnet run
```

### Frontend Development
```bash
cd src/frontend
npm run dev
```

### Testing
```bash
# Backend tests
dotnet test

# Frontend tests
npm run test
```

### Database Management
```bash
# Add migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update
```

## Docker Support

### Development
```bash
docker-compose -f docker-compose.dev.yml up -d
```

### Production
```bash
docker-compose up -d
```

## API Endpoints

### Authentication
- `POST /api/auth/login` - User login
- `POST /api/auth/logout` - User logout
- `GET /api/auth/me` - Current user info

### Admin Management
- `GET /api/permit-types` - List permit types
- `POST /api/permit-types` - Create permit type
- `GET /api/questions` - List questions
- `POST /api/questions` - Create question

### Client Intake
- `GET /api/client/permit-types` - Active permit types
- `POST /api/client/submit` - Submit intake form

### Document Generation
- `POST /api/generate/use-case/{id}` - Generate UseCase.md
- `POST /api/generate/user-stories/{id}` - Generate UserStories.md
- `POST /api/generate/data-dictionary/{id}` - Generate DataDictionary.csv
- `POST /api/generate/ai-pack/{id}` - Generate AI Pack (optional)

## Configuration

### Environment Variables
```bash
# Database
CONNECTION_STRING="Server=localhost;Database=RequirementAgent;Trusted_Connection=true;"

# JWT
JWT_SECRET="your-secret-key-here"
JWT_EXPIRY_HOURS=24

# File Upload
UPLOAD_PATH="./uploads"
MAX_FILE_SIZE_MB=50

# AI Service (Optional)
AI_PROVIDER="openai"
OPENAI_API_KEY="your-openai-key"
```

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

