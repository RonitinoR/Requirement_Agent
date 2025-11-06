"""
Demo AI Service for Adopt-A-Highway Requirements
Handles tabular requirements and converts to conversational flows
"""

import json
import requests
from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware

app = FastAPI(
    title="Adopt-A-Highway AI Demo",
    description="AI service that converts tabular requirements to conversations",
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

# Sample Adopt-A-Highway data based on your document
ADOPT_A_HIGHWAY_TEMPLATE = """
County Adopt-A-Highway Volunteer Program – Full Requirements Gathering Template

Section 1 – Program Context
County Name: _______________
Program Type: Volunteer Cleanup Program
Participating Departments: (e.g., Public Works, Environmental Services, GIS, Communications)
Program Duration / Renewal Cycle: (e.g., 2 years, renewable based on compliance)
Key Roles: 
- Program Coordinator
- Maintenance Supervisor  
- Volunteer Group Leader
Local Policies / Constraints: (e.g., safety gear rules, minimum group size, cleanup frequency, sign text restrictions)
Authentication: Username/password, MFA, SSO integration
Program Goals:
- Maintain highway cleanliness
- Promote civic pride
- Reduce cleanup costs

Section 2 – Volunteer User Requirements
V-01: Register and activate account - Define registration workflow and verification type
V-02: Browse or draw highway segments - Decide if users can free-draw or select from layer
V-03: Apply online to adopt segment - Specify required data and validation rules
V-04: Specify sign text/logo for recognition - Set compliance rules for sign content
V-05: Download and sign safety waiver - Choose waiver submission process (digital/paper)
V-06: Submit cleanup request for approval - Define notice period and approval process
V-07: Complete pre-cleanup checklist - Decide checklist items (safety video, PPE, weather check)
V-08: Submit cleanup report with photos - Define required photo and data uploads
V-09: Receive cleanup reminders - Choose frequency for notifications
V-10: Receive thank-you certificate - Specify trigger and template format
V-11: View dashboard (segments and compliance) - Define metrics displayed
V-12: Relinquish segment - Decide process for release and reassignment

Section 3 – County Staff Requirements  
C-01: Review and approve volunteer applications - Specify workflow and routing rules
C-02: Approve sign text and logo - Define design approval and rejection handling
C-03: Set cleanup frequency and monitor compliance - Define standard number of cleanups per year
C-04: Cancel/reschedule cleanups safely - Determine escalation and notification policy
C-05: Send automated reminders - Choose email templates and timing
C-06: Review cleanup reports - Define review process and audit trail
C-07: Generate reports - Decide report formats and frequency
C-08: Flag inactive groups - Set inactivity threshold (e.g., 12 months)
C-09: Send letters and certificates - Choose automation and templates

Section 4 – GIS Configuration & Mapping Requirements
Purpose: Define how the county wants to manage highway segment mapping and GIS integration in the Delasoft Adopt-A-Highway system.

Do you already have a GIS layer for Adopt-A-Highway segments? Yes/No
Can Delasoft connect directly to your GIS service? Yes/No
Should volunteers free-draw or select predefined segments? Free-draw/Select
Who maintains GIS data? County GIS Team/Delasoft/Shared
Do you want Delasoft to build the initial segment layer? Yes/No
Attributes to include in each segment: SegmentID, RoadName, County, StartMilepost, EndMilepost, Length, Direction, SideOfRoad, Status
GIS Sync Frequency: Real-time/Scheduled
Auto-update segment status after approvals? Yes/No
Enable map filters? Yes/No
Map access level: Public/Registered users only
Show cleanup or risk icons? Yes/No
Auto-calculate segment length? Yes/No

Section 5 – Workflow Requirements
Application Intake: Online submission → staff review → approval
Cleanup Scheduling: Volunteer request → staff approval → calendar update  
Reminder Automation: Auto-reminders before due cleanups
Cleanup Report Review: Volunteer submits report → staff validates → certificate
Cancellation & Rescheduling: Staff cancels → volunteer notified → reschedule allowed
Renewal & Relinquishment: System sends renewal reminders → staff approval

Section 6 – Notes & Decisions Log
Waiver Process: Digital Upload via Portal/Paper PDF Upload by Staff/Both
Sign Fabrication: County Sign Shop (internal)/External Contractor/Hybrid
Reminder Settings: Auto 30/15/5-day email sequence/Manual notifications by staff
Cleanup Frequency: 4 per year (standard)/2 per year (rural/low traffic)/Custom by district
Renewal Process: Auto-renew if compliant/Manual review by coordinator/Renewal request required
Inactivity Threshold: 6 months without cleanup/12 months without cleanup (default)/Custom threshold
Certificate Distribution: Auto-email PDF after approval/Staff-issued manually/Both (auto + annual recognition event)
Cleanup Cancellation Policy: Can be canceled by staff only/Volunteers may cancel with reason/Must be rescheduled within 30 days
"""

def call_openai_simple(prompt: str) -> str:
    """Simple OpenAI API call"""
    if not OPENAI_API_KEY:
        return "OpenAI API key not configured"
    
    try:
        headers = {
            "Authorization": f"Bearer {OPENAI_API_KEY}",
            "Content-Type": "application/json"
        }
        
        data = {
            "model": "gpt-4",
            "messages": [
                {"role": "system", "content": "You are an expert at converting requirements documents into conversational flows. Always respond with valid JSON."},
                {"role": "user", "content": prompt}
            ],
            "temperature": 0.7,
            "max_tokens": 3000
        }
        
        response = requests.post(
            "https://api.openai.com/v1/chat/completions",
            headers=headers,
            json=data,
            timeout=60
        )
        
        if response.status_code == 200:
            return response.json()["choices"][0]["message"]["content"]
        else:
            return f"Error: {response.status_code} - {response.text}"
            
    except Exception as e:
        return f"Error calling OpenAI: {str(e)}"

@app.get("/")
async def root():
    return {"message": "Adopt-A-Highway AI Demo Service", "status": "running"}

@app.get("/health")
async def health():
    return {
        "status": "healthy", 
        "service": "adopt-a-highway-ai",
        "openai_configured": bool(OPENAI_API_KEY)
    }

@app.post("/ai/convert-document")
async def convert_document_to_conversation(request: dict):
    """Convert the Adopt-A-Highway document to conversational flow"""
    try:
        # Use the actual document content from the image you shared
        document_content = request.get("document_content", ADOPT_A_HIGHWAY_TEMPLATE)
        
        prompt = f"""Convert this Adopt-A-Highway requirements document into a natural conversation flow.

DOCUMENT CONTENT:
{document_content}

Create conversational questions that feel like ChatGPT. Convert the tabular format into natural dialogue.

For example:
- Instead of "County Name: ___" ask "What county is implementing this program?"
- Instead of "Program Type: Volunteer Cleanup" ask "Tell me about the type of volunteer program you're setting up"

Return JSON with this structure:
{{
    "title": "Adopt-A-Highway Program Setup",
    "description": "Let's set up your county's Adopt-A-Highway volunteer program",
    "sections": [
        {{
            "id": "program_context",
            "title": "Program Context",
            "questions": [
                {{
                    "id": "county_name",
                    "text": "What county is implementing this Adopt-A-Highway program?",
                    "type": "text",
                    "required": true
                }},
                {{
                    "id": "participating_departments", 
                    "text": "Which departments will be involved? (e.g., Public Works, Environmental Services, GIS, Communications)",
                    "type": "multiselect",
                    "options": ["Public Works", "Environmental Services", "GIS", "Communications", "Other"]
                }}
            ]
        }},
        {{
            "id": "volunteer_requirements",
            "title": "Volunteer Experience",
            "questions": [
                {{
                    "id": "registration_process",
                    "text": "How should volunteers register and activate their accounts?",
                    "type": "select",
                    "options": ["Simple registration", "Verification required", "Manual approval"]
                }},
                {{
                    "id": "segment_selection",
                    "text": "Should volunteers be able to draw their own highway segments or select from predefined areas?",
                    "type": "select", 
                    "options": ["Free-draw segments", "Select from predefined segments", "Both options"]
                }}
            ]
        }}
    ]
}}"""

        ai_response = call_openai_simple(prompt)
        
        # Try to parse JSON from AI response
        try:
            # Find JSON in the response
            start = ai_response.find('{')
            end = ai_response.rfind('}') + 1
            
            if start != -1 and end > start:
                json_str = ai_response[start:end]
                result = json.loads(json_str)
            else:
                # Fallback if no JSON found
                result = create_fallback_conversation()
                
        except json.JSONDecodeError:
            result = create_fallback_conversation()
        
        return {
            "success": True,
            "conversation_flow": result,
            "ai_response_preview": ai_response[:200] + "..." if len(ai_response) > 200 else ai_response
        }
        
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Conversion failed: {str(e)}")

@app.post("/ai/demo-conversation")
async def demo_conversation_flow():
    """Demo conversation flow with sample Adopt-A-Highway questions"""
    
    # This creates a sample conversation based on your document
    conversation_flow = {
        "title": "Adopt-A-Highway Program Configuration",
        "description": "Let's configure your county's Adopt-A-Highway volunteer program",
        "sections": [
            {
                "id": "program_setup",
                "title": "Program Context & Setup",
                "questions": [
                    {
                        "id": "county_name",
                        "text": "What's the name of your county?",
                        "type": "text",
                        "required": True
                    },
                    {
                        "id": "program_duration",
                        "text": "How long should volunteer commitments last?",
                        "type": "select",
                        "options": ["1 year", "2 years", "3 years", "Custom duration"],
                        "default": "2 years"
                    },
                    {
                        "id": "participating_departments",
                        "text": "Which departments will participate in managing the program?",
                        "type": "multiselect",
                        "options": ["Public Works", "Environmental Services", "GIS", "Communications", "Parks & Recreation"]
                    }
                ]
            },
            {
                "id": "volunteer_experience",
                "title": "Volunteer User Experience",
                "questions": [
                    {
                        "id": "registration_type",
                        "text": "How should volunteers register for the program?",
                        "type": "select",
                        "options": ["Simple email registration", "Account verification required", "Manual staff approval"]
                    },
                    {
                        "id": "segment_selection",
                        "text": "How should volunteers choose their highway segments?",
                        "type": "select", 
                        "options": ["Free-draw on map", "Select from predefined segments", "Both options available"]
                    },
                    {
                        "id": "safety_waiver",
                        "text": "How should volunteers handle the safety waiver?",
                        "type": "select",
                        "options": ["Digital upload via portal", "Paper PDF upload by staff", "Both options"]
                    }
                ]
            },
            {
                "id": "gis_mapping",
                "title": "GIS & Mapping Configuration", 
                "questions": [
                    {
                        "id": "existing_gis_layer",
                        "text": "Do you already have a GIS layer for Adopt-A-Highway segments?",
                        "type": "yesno"
                    },
                    {
                        "id": "gis_connection",
                        "text": "Can Delasoft connect directly to your GIS service?",
                        "type": "yesno",
                        "follow_up": "If yes, please provide Feature Service URL"
                    },
                    {
                        "id": "segment_attributes",
                        "text": "What attributes should be included in each highway segment?",
                        "type": "multiselect",
                        "options": ["SegmentID", "RoadName", "County", "StartMilepost", "EndMilepost", "Length", "Direction", "SideOfRoad", "Status"],
                        "default": ["SegmentID", "RoadName", "StartMilepost", "EndMilepost", "Status"]
                    }
                ]
            },
            {
                "id": "workflow_config",
                "title": "Workflow & Process Configuration",
                "questions": [
                    {
                        "id": "cleanup_frequency",
                        "text": "How often should volunteers clean their adopted segments?",
                        "type": "select",
                        "options": ["4 times per year (standard)", "2 times per year (rural/low traffic)", "Custom by district or coordinator"]
                    },
                    {
                        "id": "reminder_automation",
                        "text": "How should cleanup reminders be sent?",
                        "type": "select",
                        "options": ["Auto 30/15/5-day email sequence", "Manual notifications by staff", "Custom timing"]
                    },
                    {
                        "id": "inactivity_threshold",
                        "text": "When should volunteer groups be flagged as inactive?",
                        "type": "select",
                        "options": ["6 months without cleanup", "12 months without cleanup (default)", "Custom threshold (staff configurable)"]
                    }
                ]
            }
        ]
    }
    
    return {
        "success": True,
        "message": "Demo conversation flow based on your Adopt-A-Highway document",
        "conversation_flow": conversation_flow
    }

@app.post("/ai/create-flow")
async def create_conversation_flow(request: dict):
    """Convert any requirements document to conversation flow using AI"""
    try:
        template_content = request.get("template_content", "")
        template_type = request.get("template_type", "Requirements")
        
        if not template_content.strip():
            # Use demo data if no content provided
            return await demo_conversation_flow()
        
        prompt = f"""Convert this {template_type} requirements document into a conversational flow.

REQUIREMENTS DOCUMENT:
{template_content}

Instructions:
1. Analyze the tabular structure and extract key information
2. Convert each requirement into natural, conversational questions
3. Group related questions into logical sections
4. Make questions feel like a helpful assistant is asking them
5. Include appropriate question types (text, select, multiselect, yesno)
6. Add options where multiple choices are available

Return JSON with this exact structure:
{{
    "title": "Program Configuration Assistant",
    "description": "Let's configure your program step by step",
    "sections": [
        {{
            "id": "section_1",
            "title": "Section Title",
            "questions": [
                {{
                    "id": "question_1",
                    "text": "Natural conversational question?",
                    "type": "text",
                    "required": true,
                    "options": ["Option 1", "Option 2"]
                }}
            ]
        }}
    ]
}}"""

        ai_response = call_openai_simple(prompt)
        
        # Parse AI response
        try:
            start = ai_response.find('{')
            end = ai_response.rfind('}') + 1
            if start != -1 and end > start:
                json_str = ai_response[start:end]
                result = json.loads(json_str)
            else:
                result = create_fallback_conversation()
        except:
            result = create_fallback_conversation()
        
        return {
            "success": True,
            "conversation_flow": result,
            "ai_used": bool(OPENAI_API_KEY),
            "message": "Requirements converted to conversational flow"
        }
        
    except Exception as e:
        return {
            "success": False,
            "error": str(e),
            "fallback_flow": create_fallback_conversation()
        }

@app.post("/ai/simulate-conversation")
async def simulate_conversation(request: dict):
    """Simulate a conversation with sample responses"""
    
    sample_conversation = [
        {
            "role": "assistant",
            "content": "Hi! I'm here to help you set up your county's Adopt-A-Highway program. Let's start with the basics - what's the name of your county?"
        },
        {
            "role": "user", 
            "content": "We're setting this up for Orange County."
        },
        {
            "role": "assistant",
            "content": "Great! Orange County. Now, which departments will be involved in managing the program? Typically we see Public Works, Environmental Services, GIS, and Communications."
        },
        {
            "role": "user",
            "content": "We'll have Public Works as the lead, plus Environmental Services and our GIS team."
        },
        {
            "role": "assistant", 
            "content": "Perfect! Public Works, Environmental Services, and GIS. That's a solid team. Now, do you already have a GIS layer for highway segments, or would you like Delasoft to create one for you?"
        },
        {
            "role": "user",
            "content": "We have some existing GIS data, but it might need to be updated for the Adopt-A-Highway program."
        },
        {
            "role": "assistant",
            "content": "That's common! We can work with your existing data and enhance it. How often should volunteers be required to clean their adopted segments? Most counties go with 4 times per year, but rural areas sometimes prefer twice yearly."
        }
    ]
    
    return {
        "success": True,
        "conversation_preview": sample_conversation,
        "extracted_decisions": {
            "county_name": "Orange County",
            "participating_departments": ["Public Works", "Environmental Services", "GIS"],
            "existing_gis_data": "Yes, but needs updates",
            "cleanup_frequency": "To be determined"
        },
        "message": "This shows how the AI creates natural conversations from your requirements"
    }

def create_fallback_conversation():
    """Fallback conversation flow based on your document"""
    return {
        "title": "Adopt-A-Highway Program Setup",
        "description": "Configure your county's volunteer highway cleanup program",
        "sections": [
            {
                "id": "program_basics",
                "title": "Program Basics",
                "questions": [
                    {"id": "county", "text": "What county is implementing this program?", "type": "text"},
                    {"id": "duration", "text": "How long should volunteer commitments last?", "type": "select", "options": ["1 year", "2 years", "3 years"]},
                    {"id": "departments", "text": "Which departments will participate?", "type": "multiselect", "options": ["Public Works", "Environmental Services", "GIS", "Communications"]}
                ]
            },
            {
                "id": "volunteer_process", 
                "title": "Volunteer Experience",
                "questions": [
                    {"id": "registration", "text": "How should volunteers register?", "type": "select", "options": ["Simple registration", "Verification required", "Manual approval"]},
                    {"id": "segments", "text": "How should volunteers select highway segments?", "type": "select", "options": ["Free-draw on map", "Select predefined segments", "Both options"]}
                ]
            }
        ]
    }

def call_openai_simple(prompt: str) -> str:
    """Simple OpenAI API call with error handling"""
    if not OPENAI_API_KEY:
        return "OpenAI API key not configured"
    
    try:
        headers = {
            "Authorization": f"Bearer {OPENAI_API_KEY}",
            "Content-Type": "application/json"
        }
        
        data = {
            "model": "gpt-4",
            "messages": [
                {"role": "system", "content": "Convert requirements to conversational flows. Respond with valid JSON only."},
                {"role": "user", "content": prompt}
            ],
            "temperature": 0.7,
            "max_tokens": 3000
        }
        
        response = requests.post(
            "https://api.openai.com/v1/chat/completions",
            headers=headers,
            json=data,
            timeout=60
        )
        
        if response.status_code == 200:
            return response.json()["choices"][0]["message"]["content"]
        else:
            return f"OpenAI API Error: {response.status_code}"
            
    except Exception as e:
        return f"Error: {str(e)}"

if __name__ == "__main__":
    import uvicorn
    print("🚀 Starting Adopt-A-Highway AI Demo Service...")
    print(f"🔑 OpenAI API Key: {'✅ Configured' if OPENAI_API_KEY else '❌ Missing'}")
    print("📋 Document: Adopt-A-Highway requirements loaded")
    print("🌐 Access: http://localhost:8001/docs")
    uvicorn.run(app, host="0.0.0.0", port=8001)
