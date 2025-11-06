# Simple AI Microservice

A lightweight FastAPI service for converting requirement templates into conversational flows using OpenAI.

## Quick Start

### 1. Install Dependencies
```bash
cd ai-service
pip install -r requirements.txt
```

### 2. Set OpenAI API Key
```bash
export OPENAI_API_KEY="sk-your-openai-api-key-here"
```

### 3. Run the Service
```bash
python main.py
```

### 4. Access Documentation
- **API Docs**: http://localhost:8000/docs
- **Health Check**: http://localhost:8000/health

## API Endpoints

### Create Conversation Flow
```http
POST /ai/create-flow
{
  "template_content": "Your requirements template...",
  "template_type": "Adopt-A-Highway"
}
```

### Process User Response
```http
POST /ai/process-response
{
  "conversation_history": [{"question": "...", "response": "..."}],
  "user_response": "User's answer",
  "current_question": "Current question"
}
```

### Extract Decisions
```http
POST /ai/extract-decisions
{
  "conversation_history": [{"question": "...", "response": "..."}],
  "template_type": "Adopt-A-Highway"
}
```

## Integration with .NET API

Your .NET backend can call this service:

```csharp
var response = await httpClient.PostAsJsonAsync(
    "http://localhost:8000/ai/create-flow",
    new { template_content = template, template_type = "Adopt-A-Highway" }
);
```

## Environment Variables

- `OPENAI_API_KEY` - Your OpenAI API key (required)
- `PORT` - Service port (default: 8000)
