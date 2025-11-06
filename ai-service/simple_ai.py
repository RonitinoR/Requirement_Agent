"""
Ultra-simple AI demo service
"""

import json
import requests
from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware

app = FastAPI(title="Simple AI Demo Service")

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

def call_openai(prompt: str) -> str:
    """Simple OpenAI API call using requests"""
    headers = {
        "Authorization": f"Bearer {OPENAI_API_KEY}",
        "Content-Type": "application/json"
    }
    
    data = {
        "model": "gpt-4",
        "messages": [
            {"role": "system", "content": "You are a helpful assistant that converts requirements into conversational flows. Always respond with valid JSON only."},
            {"role": "user", "content": prompt}
        ],
        "temperature": 0.7,
        "max_tokens": 2000
    }
    
    response = requests.post(
        "https://api.openai.com/v1/chat/completions",
        headers=headers,
        json=data,
        timeout=60
    )
    
    if response.status_code != 200:
        raise Exception(f"OpenAI API error: {response.status_code} - {response.text}")
    
    return response.json()["choices"][0]["message"]["content"]

@app.get("/health")
async def health():
    return {"status": "healthy", "openai_configured": bool(OPENAI_API_KEY)}

@app.post("/ai/create-flow")
async def create_flow(request: dict):
    """Convert template to conversation flow"""
    try:
        template = request.get("template_content", "")
        template_type = request.get("template_type", "Unknown")
        
        prompt = f"""Convert this {template_type} template into conversational questions:

{template}

Return JSON with this exact format:
{{
    "title": "{template_type} Application",
    "sections": [
        {{
            "id": "section1", 
            "title": "Organization Information",
            "questions": [
                {{"id": "q1", "text": "What is your organization's name?", "type": "text"}},
                {{"id": "q2", "text": "What type of organization are you?", "type": "select"}}
            ]
        }}
    ]
}}"""
        
        ai_response = call_openai(prompt)
        
        # Extract JSON from response
        try:
            start = ai_response.find('{')
            end = ai_response.rfind('}') + 1
            if start != -1 and end > start:
                json_str = ai_response[start:end]
                result = json.loads(json_str)
            else:
                result = json.loads(ai_response)
        except:
            # Fallback if JSON parsing fails
            result = {
                "title": f"{template_type} Application",
                "sections": [
                    {
                        "id": "org_info",
                        "title": "Organization Information", 
                        "questions": [
                            {"id": "q1", "text": "What is your organization's name?", "type": "text"},
                            {"id": "q2", "text": "What type of organization are you (individual, business, non-profit)?", "type": "select"},
                            {"id": "q3", "text": "What is your primary contact email?", "type": "email"}
                        ]
                    },
                    {
                        "id": "project_details",
                        "title": "Project Details",
                        "questions": [
                            {"id": "q4", "text": "What is your project name?", "type": "text"},
                            {"id": "q5", "text": "Please describe your project goals.", "type": "textarea"}
                        ]
                    }
                ]
            }
        
        return result
        
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Error: {str(e)}")

@app.post("/ai/create-shareable-link")
async def create_shareable_conversation(request: dict):
    """Create a shareable link for conversation history (like OpenAI chat)"""
    try:
        conversation_history = request.get("conversation_history", [])
        template_type = request.get("template_type", "Unknown")
        title = request.get("title", f"{template_type} Requirements")
        
        # Create a simple shareable format
        share_id = f"share_{hash(str(conversation_history)) % 100000:05d}"
        
        # Format conversation for sharing
        formatted_conversation = {
            "id": share_id,
            "title": title,
            "template_type": template_type,
            "created_at": "2024-11-06T12:00:00Z",
            "conversation": [],
            "shareable_url": f"http://localhost:8000/shared/{share_id}"
        }
        
        return {
            "share_id": share_id,
            "shareable_url": f"http://localhost:8000/shared/{share_id}",
            "title": title,
            "conversation_preview": formatted_conversation
        }
        
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Error creating shareable link: {str(e)}")

@app.get("/shared/{share_id}")
async def view_shared_conversation(share_id: str):
    """View a shared conversation (like OpenAI chat sharing)"""
    return {
        "message": f"Shared conversation {share_id}",
        "note": "In production, this would load the actual conversation from storage",
        "demo": "This shows how conversations can be shared like OpenAI chat"
    }

@app.post("/ai/extract-decisions")
async def extract_decisions(request: dict):
    """Extract decisions from conversation"""
    try:
        history = request.get("conversation_history", [])
        template_type = request.get("template_type", "Unknown")
        
        conversation_text = "\n".join([
            f"Q: {h.get('question', '')}\nA: {h.get('response', '')}"
            for h in history
        ])
        
        prompt = f"""Extract key decisions from this {template_type} conversation:

{conversation_text}

Return JSON with extracted information:
{{
    "decisions": {{
        "organization_name": "extracted name",
        "contact_email": "extracted email",
        "project_type": "extracted type"
    }},
    "summary": "Summary of the application"
}}"""
        
        ai_response = call_openai(prompt)
        
        try:
            start = ai_response.find('{')
            end = ai_response.rfind('}') + 1
            if start != -1 and end > start:
                json_str = ai_response[start:end]
                result = json.loads(json_str)
            else:
                result = json.loads(ai_response)
        except:
            result = {
                "decisions": {"summary": "Information extracted from conversation"},
                "summary": f"Completed {template_type} requirements gathering"
            }
        
        return result
        
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Error: {str(e)}")

if __name__ == "__main__":
    import uvicorn
    print("🚀 Starting Simple AI Service...")
    print(f"🔑 OpenAI API Key: {'✅ Configured' if OPENAI_API_KEY else '❌ Missing'}")
    uvicorn.run(app, host="0.0.0.0", port=8000)
