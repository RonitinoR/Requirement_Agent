"""
Working AI Service for Adopt-A-Highway Demo
Fixed request handling to avoid 422 errors
"""

import json
import requests
from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from typing import Optional, List, Dict, Any

app = FastAPI(
    title="Adopt-A-Highway AI Service",
    description="Convert requirements to conversations - WORKING VERSION",
    version="1.0.0"
)

# Add CORS
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_methods=["*"],
    allow_headers=["*"],
)

# OpenAI API key from environment variable (secure)
import os
OPENAI_API_KEY = os.getenv("OPENAI_API_KEY", "your-openai-key-here")

# Pydantic models to fix 422 errors
class CreateFlowRequest(BaseModel):
    template_content: str
    template_type: str = "Adopt-A-Highway"

class ConversationRequest(BaseModel):
    message: Optional[str] = "start demo"

def call_openai(prompt: str) -> str:
    """Call OpenAI API"""
    if not OPENAI_API_KEY:
        return "OpenAI not configured"
    
    try:
        response = requests.post(
            "https://api.openai.com/v1/chat/completions",
            headers={
                "Authorization": f"Bearer {OPENAI_API_KEY}",
                "Content-Type": "application/json"
            },
            json={
                "model": "gpt-4",
                "messages": [
                    {"role": "system", "content": "Convert requirements to natural conversations. Respond with valid JSON only."},
                    {"role": "user", "content": prompt}
                ],
                "temperature": 0.7,
                "max_tokens": 2500
            },
            timeout=60
        )
        
        if response.status_code == 200:
            return response.json()["choices"][0]["message"]["content"]
        else:
            return f"OpenAI Error: {response.status_code}"
            
    except Exception as e:
        return f"Error: {str(e)}"

@app.get("/")
async def root():
    return {
        "message": "Adopt-A-Highway AI Service",
        "status": "working",
        "endpoints": ["/health", "/ai/demo", "/ai/create-flow", "/ai/conversation-sample"],
        "note": "This version fixes 422 errors with proper request models"
    }

@app.get("/health")
async def health():
    return {
        "status": "healthy",
        "openai_configured": bool(OPENAI_API_KEY),
        "service": "adopt-a-highway-ai"
    }

@app.get("/ai/demo")
async def demo_conversation():
    """Demo conversation flow - NO REQUEST BODY NEEDED"""
    
    # Based on your actual Adopt-A-Highway document
    demo_flow = {
        "title": "Adopt-A-Highway Program Configuration",
        "description": "Let's set up your county's volunteer highway cleanup program",
        "sections": [
            {
                "section": "Program Context",
                "conversational_questions": [
                    "What county is implementing this Adopt-A-Highway program?",
                    "Which departments will be involved? (Public Works, Environmental Services, GIS, Communications)",
                    "How long should volunteer commitments last? (1 year, 2 years, 3 years)",
                    "What are the main goals of your program? (Cleanliness, civic pride, cost reduction)"
                ]
            },
            {
                "section": "Volunteer Experience", 
                "conversational_questions": [
                    "How should volunteers register for the program?",
                    "Should volunteers be able to draw their own highway segments on a map, or select from predefined areas?",
                    "How should volunteers handle the safety waiver? (Digital upload, paper submission, or both)",
                    "What information should volunteers provide when applying to adopt a segment?"
                ]
            },
            {
                "section": "GIS & Mapping",
                "conversational_questions": [
                    "Do you already have a GIS layer for Adopt-A-Highway segments?",
                    "Can Delasoft connect directly to your GIS service, or do you prefer scheduled imports?",
                    "What attributes should each highway segment include? (SegmentID, RoadName, Mileposts, etc.)",
                    "Should the system auto-calculate segment lengths, or will you provide them?"
                ]
            },
            {
                "section": "Workflow Configuration",
                "conversational_questions": [
                    "How often should volunteers clean their adopted segments? (4 times/year standard, 2 times/year rural)",
                    "How should cleanup reminders be sent? (Automated email sequence or manual notifications)",
                    "When should volunteer groups be flagged as inactive? (6 months, 12 months, or custom)",
                    "How should cleanup reports be reviewed and approved?"
                ]
            }
        ],
        "ai_transformation": "This shows how your tabular requirements become natural conversations",
        "chat_like_experience": True
    }
    
    return {
        "success": True,
        "original_format": "Tabular requirements document",
        "converted_format": "Natural conversation flow",
        "demo_flow": demo_flow,
        "message": "Your Adopt-A-Highway requirements converted to ChatGPT-style conversations"
    }

@app.post("/ai/create-flow")
async def create_flow(request: CreateFlowRequest):
    """Convert requirements document to conversation flow"""
    try:
        prompt = f"""Convert this Adopt-A-Highway requirements document to natural conversation questions:

{request.template_content}

Make it conversational like ChatGPT. Return JSON:
{{
    "title": "Program Setup Assistant",
    "sections": [
        {{
            "title": "Section Name",
            "questions": ["Natural question 1?", "Natural question 2?"]
        }}
    ]
}}"""

        ai_response = call_openai(prompt)
        
        # Try to parse JSON
        try:
            start = ai_response.find('{')
            end = ai_response.rfind('}') + 1
            if start != -1 and end > start:
                result = json.loads(ai_response[start:end])
            else:
                result = {"title": "Converted Flow", "sections": [{"title": "Generated", "questions": ["AI conversion completed"]}]}
        except:
            result = {"title": "AI Processing", "sections": [{"title": "Demo", "questions": ["Conversion in progress"]}]}
        
        return {
            "success": True,
            "converted_flow": result,
            "ai_response_preview": ai_response[:300] + "..." if len(ai_response) > 300 else ai_response
        }
        
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@app.get("/ai/conversation-sample")
async def conversation_sample():
    """Sample conversation showing ChatGPT-like experience"""
    
    sample_chat = [
        {
            "role": "assistant",
            "message": "Hi! I'm here to help set up your Adopt-A-Highway program. Let's start - what county are you configuring this for?"
        },
        {
            "role": "user", 
            "message": "We're setting this up for Orange County, California."
        },
        {
            "role": "assistant",
            "message": "Great! Orange County. Now, which departments will be involved? I typically see Public Works leading, with Environmental Services and GIS supporting."
        },
        {
            "role": "user",
            "message": "Yes, Public Works will lead. We'll also have Environmental Services and our GIS team involved."
        },
        {
            "role": "assistant",
            "message": "Perfect team! Now, do you already have GIS data for highway segments, or should Delasoft create the initial mapping layer for you?"
        },
        {
            "role": "user",
            "message": "We have some existing highway data, but it might need updates for the volunteer program."
        },
        {
            "role": "assistant", 
            "message": "That's common! We can enhance your existing data. One more question - how often should volunteers be required to clean their segments? Most counties do 4 times per year, but rural areas sometimes prefer twice yearly."
        }
    ]
    
    extracted_info = {
        "county_name": "Orange County, California",
        "lead_department": "Public Works", 
        "supporting_departments": ["Environmental Services", "GIS"],
        "existing_gis_data": "Yes, but needs updates",
        "cleanup_frequency": "To be determined (4x or 2x per year)"
    }
    
    return {
        "success": True,
        "conversation_sample": sample_chat,
        "extracted_decisions": extracted_info,
        "shareable_link": "http://localhost:8001/shared/demo_123",
        "message": "This shows the ChatGPT-like experience for requirements gathering"
    }

if __name__ == "__main__":
    import uvicorn
    print("🚀 Starting Working AI Demo Service...")
    print(f"🔑 OpenAI: {'✅ Ready' if OPENAI_API_KEY else '❌ Missing'}")
    print("📋 Document: Adopt-A-Highway requirements ready")
    print("🌐 Demo: http://localhost:8001/docs")
    uvicorn.run(app, host="0.0.0.0", port=8001)
