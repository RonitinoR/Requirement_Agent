# OpenAI Integration Guide

## Overview

The RequirementAgent now includes a comprehensive OpenAI integration service that transforms static requirement templates into dynamic, conversational Q&A flows. This service uses advanced prompt engineering to create natural conversations that gather the same information as traditional forms while providing a better user experience.

## Key Features

### 🤖 **AI-Powered Conversation Design**
- Converts requirement templates into natural conversation flows
- Breaks complex requirements into digestible conversation segments
- Maintains context throughout multi-turn conversations
- Generates follow-up questions based on user responses

### 🧠 **Intelligent Response Processing**
- Extracts structured data from conversational responses
- Handles ambiguous or incomplete answers
- Provides confidence scores for extracted information
- Suggests clarifying questions when needed

### 🔄 **Dynamic Conditional Logic**
- Generates conditional questions based on user responses
- Handles branching conversation paths
- Adapts question flow based on user profile (individual, business, non-profit)
- Manages complex decision trees

### 📊 **State Management**
- Maintains conversation context across sessions
- Tracks user progress through sections
- Provides conversation history and snapshots
- Handles session recovery and resumption

## Architecture

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│  Conversation   │    │   OpenAI API    │    │  State Manager  │
│   Controller    │◄──►│    Service      │◄──►│   (Memory)      │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                       │                       │
         ▼                       ▼                       ▼
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   REST API      │    │   OpenAI API    │    │   Conversation  │
│   Endpoints     │    │   (GPT-4)       │    │   Context       │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

## Configuration

### 1. OpenAI API Setup

Add your OpenAI configuration to `appsettings.json`:

```json
{
  "OpenAI": {
    "ApiKey": "sk-your-openai-api-key-here",
    "BaseUrl": "https://api.openai.com",
    "Model": "gpt-4",
    "Temperature": 0.7,
    "MaxTokens": 2000,
    "TopP": 1.0,
    "FrequencyPenalty": 0.0,
    "PresencePenalty": 0.0,
    "TimeoutSeconds": 60
  }
}
```

### 2. Environment Variables (Recommended for Production)

```bash
export OPENAI__APIKEY="sk-your-openai-api-key-here"
export OPENAI__MODEL="gpt-4"
export OPENAI__TEMPERATURE="0.7"
```

## Usage Examples

### 1. Create Conversation Flow from Template

```csharp
// Inject the service
private readonly IOpenAIService _openAIService;

// Convert template to conversation flow
var templateContent = @"
    ADOPT-A-HIGHWAY REQUIREMENTS
    1. Organization Information
    2. Highway Segment Selection
    3. Commitment Details
    ...
";

var flow = await _openAIService.CreateConversationFlowAsync(
    templateContent, 
    "Adopt-A-Highway");

// Result: Structured conversation with natural questions
// - "Tell me about your organization..."
// - "Which highway segment interests you?"
// - "How often can your team commit to cleanups?"
```

### 2. Process User Responses

```csharp
// Start conversation
var context = await _stateManager.CreateConversationAsync(flowId, userId, sessionId);

// Process user response
var userResponse = "We're the Green Valley Environmental Club, a local non-profit.";
var response = await _openAIService.ProcessUserResponseAsync(context, userResponse);

// Response includes:
// - Next question to ask
// - Extracted structured data
// - Confidence score
// - Follow-up questions if needed
```

### 3. Handle Conditional Logic

```csharp
// User mentions they're a business
var userResponse = "We're XYZ Corporation, a local business.";

// AI automatically generates business-specific follow-up questions
var conditionalQuestions = await _openAIService.GenerateConditionalQuestionsAsync(
    context, 
    userResponse);

// Result: Questions about corporate sponsorship, tax benefits, etc.
```

### 4. Extract Structured Decisions

```csharp
// After conversation completion
var decisions = await _openAIService.ExtractDecisionsAsync(context);

// Result: Structured data like:
// {
//   "organization_name": "Green Valley Environmental Club",
//   "organization_type": "non-profit",
//   "contact_person": "Sarah Johnson",
//   "preferred_route": "Highway 101, miles 15-18",
//   "commitment_years": "3",
//   "cleanup_frequency": "quarterly"
// }
```

## API Endpoints

### Create Conversation Flow
```http
POST /api/conversation/flows
Authorization: Bearer {admin-token}
Content-Type: application/json

{
  "templateContent": "Your requirements template here...",
  "templateType": "Adopt-A-Highway"
}
```

### Start Conversation Session
```http
POST /api/conversation/sessions
Authorization: Bearer {user-token}
Content-Type: application/json

{
  "flowId": "conversation-flow-id"
}
```

### Process User Response
```http
POST /api/conversation/sessions/{conversationId}/respond
Authorization: Bearer {user-token}
Content-Type: application/json

{
  "userResponse": "We're a local environmental group..."
}
```

### Extract Decisions
```http
POST /api/conversation/sessions/{conversationId}/extract
Authorization: Bearer {user-token}
```

### Validate Section Completion
```http
POST /api/conversation/sessions/{conversationId}/sections/{sectionId}/validate
Authorization: Bearer {user-token}
```

## Prompt Engineering

### Template Conversion Prompt
The service uses sophisticated prompts to convert static templates into conversational flows:

- **Input**: Static requirement template
- **Output**: Structured conversation with natural questions
- **Features**: Section breakdown, question typing, conditional logic identification

### Response Processing Prompt
For each user response, the AI:

- Extracts key information and decisions
- Determines conversation completeness
- Identifies need for follow-up questions
- Provides confidence scores
- Suggests next steps

### Decision Extraction Prompt
At conversation completion:

- Identifies all explicit and implicit decisions
- Categorizes decisions by type
- Assesses confidence levels
- Notes missing information
- Provides supporting evidence

## Advanced Features

### Conversation State Management

```csharp
// Get user's active conversations
var conversations = await _stateManager.GetUserConversationsAsync(userId);

// Resume interrupted conversation
var activeConversation = await _stateManager.GetActiveConversationAsync(userId, flowId);

// Save conversation snapshots for history
await _stateManager.SaveConversationSnapshotAsync(context);
```

### Section Validation

```csharp
// Check if section has all required information
var status = await _openAIService.ValidateSectionCompletionAsync(context, sectionId);

if (status.IsComplete && status.CompletionPercentage >= 90.0)
{
    // Move to next section
}
else
{
    // Ask for missing information
    foreach (var missing in status.MissingRequirements)
    {
        // Generate questions for missing items
    }
}
```

### Conditional Question Generation

```csharp
// Generate questions based on specific triggers
var questions = await _openAIService.GenerateConditionalQuestionsAsync(
    context, 
    "User mentioned large volunteer group");

// Result: Questions about coordination, logistics, safety training, etc.
```

## Error Handling

The service includes comprehensive error handling:

```csharp
try
{
    var response = await _openAIService.ProcessUserResponseAsync(context, userInput);
}
catch (HttpRequestException ex)
{
    // OpenAI API connectivity issues
    _logger.LogError(ex, "OpenAI API connection failed");
}
catch (JsonException ex)
{
    // AI response parsing issues
    _logger.LogError(ex, "Failed to parse AI response");
}
catch (InvalidOperationException ex)
{
    // Business logic errors
    _logger.LogError(ex, "Conversation processing error");
}
```

## Performance Considerations

### Caching Strategy
- **Conversation Context**: Cached in memory with 24-hour expiration
- **Conversation History**: Limited to last 50 snapshots per conversation
- **API Responses**: Consider caching common template conversions

### Rate Limiting
- OpenAI API has rate limits - implement retry logic with exponential backoff
- Consider queuing for high-volume scenarios
- Monitor token usage and costs

### Memory Management
- Automatic cleanup of expired conversations
- Configurable cache expiration times
- Conversation history pruning

## Testing

### Unit Tests Example

```csharp
[Test]
public async Task CreateConversationFlow_ShouldReturnValidFlow()
{
    // Arrange
    var template = "Sample requirements template...";
    var templateType = "Test-Template";

    // Act
    var flow = await _openAIService.CreateConversationFlowAsync(template, templateType);

    // Assert
    Assert.IsNotNull(flow);
    Assert.AreEqual(templateType, flow.TemplateType);
    Assert.IsTrue(flow.Sections.Any());
}
```

### Integration Tests

```csharp
[Test]
public async Task CompleteConversationWorkflow_ShouldExtractDecisions()
{
    // Test complete conversation flow from template to extracted decisions
    var flow = await _openAIService.CreateConversationFlowAsync(sampleTemplate, "Test");
    var context = await _stateManager.CreateConversationAsync(flow.Id, "test-user", "session");
    
    // Simulate conversation...
    var decisions = await _openAIService.ExtractDecisionsAsync(context);
    
    Assert.IsTrue(decisions.Decisions.Any());
}
```

## Monitoring and Analytics

### Conversation Statistics

```csharp
// Get system-wide conversation statistics
var stats = await _stateManager.GetStatisticsAsync();

// Monitor:
// - Total active conversations
// - Average conversation length
// - Completion rates
// - Common drop-off points
```

### Logging

The service provides comprehensive logging:

- **Info**: Conversation start/completion, major state changes
- **Debug**: Detailed processing steps, AI responses
- **Warning**: Parsing issues, low confidence scores
- **Error**: API failures, critical processing errors

## Best Practices

### 1. Prompt Design
- Keep prompts focused and specific
- Include clear output format specifications
- Provide examples for complex structures
- Test prompts with various input scenarios

### 2. Error Recovery
- Implement graceful degradation when AI is unavailable
- Provide fallback to traditional forms
- Save conversation state frequently
- Handle partial responses gracefully

### 3. Security
- Never log API keys or sensitive user data
- Validate all AI responses before processing
- Implement rate limiting and abuse prevention
- Use secure token storage

### 4. Cost Management
- Monitor token usage and API costs
- Implement caching for repeated operations
- Use appropriate model sizes for different tasks
- Consider batch processing for efficiency

## Troubleshooting

### Common Issues

1. **API Key Issues**
   - Verify API key is valid and has sufficient credits
   - Check environment variable configuration
   - Ensure proper key format (starts with 'sk-')

2. **Response Parsing Errors**
   - AI responses may not always match expected JSON format
   - Implement robust parsing with fallbacks
   - Log problematic responses for prompt refinement

3. **Conversation State Issues**
   - Memory cache may expire during long conversations
   - Implement persistent storage for production
   - Handle conversation recovery gracefully

4. **Performance Issues**
   - OpenAI API can be slow for complex prompts
   - Implement timeout handling
   - Consider async processing for non-critical operations

## Future Enhancements

- **Multi-language Support**: Conversation flows in multiple languages
- **Voice Integration**: Speech-to-text and text-to-speech capabilities
- **Advanced Analytics**: Conversation quality metrics and optimization
- **Custom Models**: Fine-tuned models for specific requirement types
- **Real-time Collaboration**: Multiple users in same conversation flow

---

*This integration provides a foundation for AI-enhanced requirements gathering that can be extended and customized for various use cases.*

