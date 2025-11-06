"""
Simple AI Microservice for Requirements Gathering
OpenAI integration for converting templates to conversations
"""

import os
import json
from typing import Dict, Any, List, Optional

from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from openai import OpenAI
from dotenv import load_dotenv

# Load environment variables
load_dotenv()

# Initialize FastAPI app
app = FastAPI(
    title="Requirements AI Service",
    description="Simple AI service for requirements gathering",
    version="1.0.0"
)

# Add CORS
app.add_middleware(
    CORSMiddleware,
    allow_origins=["http://localhost:5000", "http://localhost:5173"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Initialize OpenAI client
openai_client = OpenAI(api_key=os.getenv("OPENAI_API_KEY"))

# Request/Response models
class CreateFlowRequest(BaseModel):
    template_content: str
    template_type: str

class ProcessResponseRequest(BaseModel):
    conversation_history: List[Dict[str, str]]
    user_response: str
    current_question: str

class ExtractDecisionsRequest(BaseModel):
    conversation_history: List[Dict[str, str]]
    template_type: str

# Response models
class ConversationFlow(BaseModel):
    title: str
    sections: List[Dict[str, Any]]

class ProcessedResponse(BaseModel):
    next_question: str
    extracted_info: Dict[str, str]
    is_complete: bool
    confidence_score: float

class ExtractedDecisions(BaseModel):
    decisions: Dict[str, str]
    summary: str

@app.get("/health")
async def health_check():
    """Simple health check"""
    return {"status": "healthy", "service": "ai-microservice"}

@app.post("/ai/create-flow")
async def create_conversation_flow(request: CreateFlowRequest) -> ConversationFlow:
    """Convert requirements template to conversation flow"""
    try:
        prompt = f"""Convert this {request.template_type} requirements template into a simple conversation flow with natural questions.

TEMPLATE:
{request.template_content}

Create 3-5 sections with conversational questions. Return JSON format:
{{
    "title": "Conversation title",
    "sections": [
        {{
            "id": "section1",
            "title": "Section Name",
            "questions": [
                {{
                    "id": "q1",
                    "text": "What's your organization's name?",
                    "type": "text"
                }}
            ]
        }}
    ]
}}"""

        response = openai_client.chat.completions.create(
            model="gpt-4",
            messages=[
                {"role": "system", "content": "You are a helpful assistant that converts requirements into conversational flows. Always respond with valid JSON."},
                {"role": "user", "content": prompt}
            ],
            temperature=0.7,
            max_tokens=2000
        )
        
        # Parse the JSON response
        content = response.choices[0].message.content
        flow_data = json.loads(content)
        
        return ConversationFlow(**flow_data)
        
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Failed to create flow: {str(e)}")

@app.post("/ai/process-response")
async def process_user_response(request: ProcessResponseRequest) -> ProcessedResponse:
    """Process user response and generate next question"""
    try:
        # Build conversation context
        history_text = "\n".join([
            f"Q: {h['question']}\nA: {h['response']}" 
            for h in request.conversation_history[-5:]  # Last 5 exchanges
        ])
        
        prompt = f"""Process this user response in a requirements gathering conversation.

CONVERSATION HISTORY:
{history_text}

CURRENT QUESTION: {request.current_question}
USER RESPONSE: {request.user_response}

Extract key information and suggest the next question. Return JSON:
{{
    "next_question": "What's the next question to ask?",
    "extracted_info": {{
        "key1": "extracted value 1"
    }},
    "is_complete": false,
    "confidence_score": 0.85
}}"""

        response = openai_client.chat.completions.create(
            model="gpt-4",
            messages=[
                {"role": "system", "content": "You are processing user responses in a requirements conversation. Extract information and suggest next steps. Always respond with valid JSON."},
                {"role": "user", "content": prompt}
            ],
            temperature=0.5,
            max_tokens=1000
        )
        
        content = response.choices[0].message.content
        result_data = json.loads(content)
        
        return ProcessedResponse(**result_data)
        
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Failed to process response: {str(e)}")

@app.post("/ai/extract-decisions")
async def extract_decisions(request: ExtractDecisionsRequest) -> ExtractedDecisions:
    """Extract structured decisions from conversation"""
    try:
        history_text = "\n\n".join([
            f"Q: {h['question']}\nA: {h['response']}" 
            for h in request.conversation_history
        ])
        
        prompt = f"""Extract all key decisions from this {request.template_type} conversation.

CONVERSATION:
{history_text}

Extract decisions and create a summary. Return JSON:
{{
    "decisions": {{
        "organization_name": "Green Valley Environmental Club",
        "organization_type": "non-profit",
        "contact_email": "contact@example.com"
    }},
    "summary": "Brief summary of the application and key points"
}}"""

        response = openai_client.chat.completions.create(
            model="gpt-4",
            messages=[
                {"role": "system", "content": "You are extracting structured decisions from requirements conversations. Focus on key information and decisions. Always respond with valid JSON."},
                {"role": "user", "content": prompt}
            ],
            temperature=0.3,
            max_tokens=1500
        )
        
        content = response.choices[0].message.content
        decisions_data = json.loads(content)
        
        return ExtractedDecisions(**decisions_data)
        
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Failed to extract decisions: {str(e)}")

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)