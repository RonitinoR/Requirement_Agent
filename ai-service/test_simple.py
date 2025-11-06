"""
Simple test for the AI service
"""

import os
import asyncio
import httpx

# Sample template for testing
SAMPLE_TEMPLATE = """
ADOPT-A-HIGHWAY REQUIREMENTS

1. Organization Information
- Organization name
- Contact person
- Email address
- Phone number

2. Highway Selection  
- Preferred highway route
- Mile markers
- Reason for selection

3. Commitment
- Adoption period (minimum 2 years)
- Cleanup frequency
- Number of volunteers
"""

async def test_ai_service():
    """Test the AI service"""
    
    if not os.getenv("OPENAI_API_KEY"):
        print("❌ Set OPENAI_API_KEY environment variable first")
        return
    
    print("🚀 Testing Simple AI Service...")
    
    async with httpx.AsyncClient() as client:
        try:
            # Test health
            health = await client.get("http://localhost:8000/health")
            print(f"✅ Health check: {health.json()}")
            
            # Test flow creation
            flow_response = await client.post("http://localhost:8000/ai/create-flow", json={
                "template_content": SAMPLE_TEMPLATE,
                "template_type": "Adopt-A-Highway"
            })
            
            if flow_response.status_code == 200:
                flow = flow_response.json()
                print(f"✅ Flow created: {flow['title']}")
                print(f"   Sections: {len(flow['sections'])}")
            else:
                print(f"❌ Flow creation failed: {flow_response.text}")
            
        except Exception as e:
            print(f"❌ Test failed: {e}")

if __name__ == "__main__":
    asyncio.run(test_ai_service())
