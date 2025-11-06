using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RequirementAgent.Api.Configurations;
using RequirementAgent.Api.Models.AI;

namespace RequirementAgent.Api.Services.AI;

public class OpenAIService : IOpenAIService
{
    private readonly HttpClient _httpClient;
    private readonly OpenAIOptions _options;
    private readonly ILogger<OpenAIService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public OpenAIService(
        HttpClient httpClient,
        IOptions<OpenAIOptions> options,
        ILogger<OpenAIService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        ConfigureHttpClient();
    }

    public async Task<ConversationFlow> CreateConversationFlowAsync(
        string templateContent, 
        string templateType, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating conversation flow for template type: {TemplateType}", templateType);

            var prompt = BuildTemplateConversionPrompt(templateContent, templateType);
            var response = await CallOpenAIAsync(prompt, cancellationToken);
            
            var conversationFlow = ParseConversationFlowResponse(response, templateType);
            
            _logger.LogInformation("Successfully created conversation flow with {SectionCount} sections", 
                conversationFlow.Sections.Count);
            
            return conversationFlow;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating conversation flow for template type: {TemplateType}", templateType);
            throw new InvalidOperationException($"Failed to create conversation flow: {ex.Message}", ex);
        }
    }

    public async Task<ConversationResponse> ProcessUserResponseAsync(
        ConversationContext context, 
        string userResponse, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing user response for conversation: {ConversationId}", context.ConversationId);

            var prompt = BuildResponseProcessingPrompt(context, userResponse);
            var response = await CallOpenAIAsync(prompt, cancellationToken);
            
            var conversationResponse = ParseConversationResponse(response);
            
            // Update conversation context
            await UpdateConversationContextAsync(context, userResponse, conversationResponse);
            
            _logger.LogInformation("Successfully processed user response with confidence: {Confidence}", 
                conversationResponse.ConfidenceScore);
            
            return conversationResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing user response for conversation: {ConversationId}", context.ConversationId);
            throw new InvalidOperationException($"Failed to process user response: {ex.Message}", ex);
        }
    }

    public async Task<ExtractedDecisions> ExtractDecisionsAsync(
        ConversationContext context, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Extracting decisions from conversation: {ConversationId}", context.ConversationId);

            var prompt = BuildDecisionExtractionPrompt(context);
            var response = await CallOpenAIAsync(prompt, cancellationToken);
            
            var extractedDecisions = ParseExtractedDecisions(response);
            
            _logger.LogInformation("Successfully extracted {DecisionCount} decisions with overall confidence: {Confidence}", 
                extractedDecisions.Decisions.Count, extractedDecisions.OverallConfidence);
            
            return extractedDecisions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting decisions from conversation: {ConversationId}", context.ConversationId);
            throw new InvalidOperationException($"Failed to extract decisions: {ex.Message}", ex);
        }
    }

    public async Task<SectionCompletionStatus> ValidateSectionCompletionAsync(
        ConversationContext context, 
        string sectionId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Validating section completion for section: {SectionId}", sectionId);

            var prompt = BuildSectionValidationPrompt(context, sectionId);
            var response = await CallOpenAIAsync(prompt, cancellationToken);
            
            var completionStatus = ParseSectionCompletionStatus(response, sectionId);
            
            _logger.LogInformation("Section {SectionId} completion status: {IsComplete} ({Percentage}%)", 
                sectionId, completionStatus.IsComplete, completionStatus.CompletionPercentage);
            
            return completionStatus;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating section completion for section: {SectionId}", sectionId);
            throw new InvalidOperationException($"Failed to validate section completion: {ex.Message}", ex);
        }
    }

    public async Task<List<ConversationQuestion>> GenerateConditionalQuestionsAsync(
        ConversationContext context, 
        string triggerResponse, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating conditional questions for trigger: {TriggerResponse}", triggerResponse);

            var prompt = BuildConditionalQuestionPrompt(context, triggerResponse);
            var response = await CallOpenAIAsync(prompt, cancellationToken);
            
            var conditionalQuestions = ParseConditionalQuestions(response);
            
            _logger.LogInformation("Generated {QuestionCount} conditional questions", conditionalQuestions.Count);
            
            return conditionalQuestions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating conditional questions for trigger: {TriggerResponse}", triggerResponse);
            throw new InvalidOperationException($"Failed to generate conditional questions: {ex.Message}", ex);
        }
    }

    #region Private Methods

    private void ConfigureHttpClient()
    {
        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_options.ApiKey}");
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "RequirementAgent/1.0");
        _httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
    }

    private async Task<string> CallOpenAIAsync(string prompt, CancellationToken cancellationToken)
    {
        var requestBody = new
        {
            model = _options.Model,
            messages = new[]
            {
                new { role = "system", content = _options.SystemPrompt },
                new { role = "user", content = prompt }
            },
            temperature = _options.Temperature,
            max_tokens = _options.MaxTokens,
            top_p = _options.TopP,
            frequency_penalty = _options.FrequencyPenalty,
            presence_penalty = _options.PresencePenalty
        };

        var json = JsonSerializer.Serialize(requestBody, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("/v1/chat/completions", content, cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("OpenAI API error: {StatusCode} - {Content}", response.StatusCode, errorContent);
            throw new HttpRequestException($"OpenAI API error: {response.StatusCode}");
        }

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var openAIResponse = JsonSerializer.Deserialize<OpenAIResponse>(responseContent, _jsonOptions);
        
        return openAIResponse?.Choices?.FirstOrDefault()?.Message?.Content ?? 
               throw new InvalidOperationException("No response content received from OpenAI");
    }

    private string BuildTemplateConversionPrompt(string templateContent, string templateType)
    {
        return $@"Convert the following {templateType} requirements template into a conversational Q&A flow.

TEMPLATE CONTENT:
{templateContent}

INSTRUCTIONS:
1. Break the template into logical sections (3-7 sections maximum)
2. Create natural, conversational questions for each requirement
3. Identify conditional logic where responses determine follow-up questions
4. Suggest appropriate question types (open-ended, yes/no, multiple choice, etc.)
5. Include validation rules where appropriate
6. Maintain the original intent and completeness of the requirements

OUTPUT FORMAT (JSON):
{{
    ""title"": ""Conversation title"",
    ""description"": ""Brief description of the process"",
    ""sections"": [
        {{
            ""id"": ""section_1"",
            ""title"": ""Section Title"",
            ""description"": ""What this section covers"",
            ""order"": 1,
            ""questions"": [
                {{
                    ""id"": ""q1"",
                    ""text"": ""Conversational question text"",
                    ""context"": ""Additional context or explanation"",
                    ""type"": ""OpenEnded"",
                    ""suggestedResponses"": [""option1"", ""option2""],
                    ""validation"": {{
                        ""required"": true,
                        ""customValidation"": ""natural language validation rule""
                    }}
                }}
            ],
            ""prerequisites"": [],
            ""isOptional"": false
        }}
    ]
}}

Ensure the conversation flow feels natural and guides users through the requirements gathering process efficiently.";
    }

    private string BuildResponseProcessingPrompt(ConversationContext context, string userResponse)
    {
        var historyContext = string.Join("\n", context.History.TakeLast(5).Select(h => 
            $"Q: {h.Question}\nA: {h.UserResponse}"));

        return $@"Process the user's response in the context of an ongoing requirements gathering conversation.

CONVERSATION CONTEXT:
Current Section: {context.CurrentSectionId}
Current Question: {context.CurrentQuestionId}

RECENT CONVERSATION HISTORY:
{historyContext}

USER'S CURRENT RESPONSE:
{userResponse}

EXTRACTED DATA SO FAR:
{JsonSerializer.Serialize(context.ExtractedData, _jsonOptions)}

INSTRUCTIONS:
1. Extract key information and decisions from the user's response
2. Determine if the response fully answers the current question
3. Identify if follow-up questions are needed for clarification
4. Suggest the next question or indicate if the section is complete
5. Assess confidence in the extracted information
6. Provide reasoning for your decisions

OUTPUT FORMAT (JSON):
{{
    ""nextQuestionId"": ""id_of_next_question_or_empty"",
    ""nextQuestion"": ""text_of_next_question_or_empty"",
    ""nextSectionId"": ""id_if_moving_to_new_section"",
    ""isComplete"": false,
    ""requiresFollowUp"": false,
    ""followUpQuestions"": [""clarifying question 1""],
    ""extractedValues"": {{
        ""key1"": ""extracted_value1""
    }},
    ""confidenceScore"": 0.85,
    ""reasoning"": ""Explanation of interpretation"",
    ""suggestions"": [""suggestion for user""]
}}";
    }

    private string BuildDecisionExtractionPrompt(ConversationContext context)
    {
        var conversationHistory = string.Join("\n\n", context.History.Select(h => 
            $"Q: {h.Question}\nA: {h.UserResponse}"));

        return $@"Extract all key decisions and requirements from the complete conversation history.

CONVERSATION HISTORY:
{conversationHistory}

INSTRUCTIONS:
1. Identify all explicit decisions made by the user
2. Infer implicit decisions from user responses
3. Categorize decisions by type (technical, business, legal, etc.)
4. Assess confidence level for each extracted decision
5. Note any missing information or assumptions made
6. Provide supporting evidence for each decision

OUTPUT FORMAT (JSON):
{{
    ""decisions"": {{
        ""decision_key_1"": {{
            ""key"": ""descriptive_key"",
            ""value"": ""decision_value"",
            ""category"": ""technical"",
            ""confidence"": 0.95,
            ""supportingEvidence"": [""quote from conversation""],
            ""source"": ""question_id_or_inference""
        }}
    }},
    ""missingInformation"": [""what information is still needed""],
    ""assumptions"": [""assumptions made during extraction""],
    ""overallConfidence"": 0.87
}}";
    }

    private string BuildSectionValidationPrompt(ConversationContext context, string sectionId)
    {
        var sectionHistory = context.History
            .Where(h => h.SectionId == sectionId)
            .Select(h => $"Q: {h.Question}\nA: {h.UserResponse}")
            .ToList();

        return $@"Validate if the conversation section has been completed adequately.

SECTION ID: {sectionId}

SECTION CONVERSATION HISTORY:
{string.Join("\n\n", sectionHistory)}

INSTRUCTIONS:
1. Assess if all required information for this section has been gathered
2. Calculate completion percentage based on coverage of section requirements
3. Identify any missing critical information
4. Note optional items that could enhance the requirements
5. Provide reasoning for the completion assessment

OUTPUT FORMAT (JSON):
{{
    ""sectionId"": ""{sectionId}"",
    ""isComplete"": true,
    ""completionPercentage"": 85.5,
    ""missingRequirements"": [""critical item 1""],
    ""optionalItems"": [""nice to have item 1""],
    ""reasoning"": ""Explanation of completion assessment""
}}";
    }

    private string BuildConditionalQuestionPrompt(ConversationContext context, string triggerResponse)
    {
        return $@"Generate follow-up questions based on the user's response that triggered conditional logic.

TRIGGER RESPONSE: {triggerResponse}

CONVERSATION CONTEXT:
Current Section: {context.CurrentSectionId}
Recent Responses: {string.Join(", ", context.History.TakeLast(3).Select(h => h.UserResponse))}

INSTRUCTIONS:
1. Generate relevant follow-up questions based on the trigger response
2. Ensure questions gather necessary additional details
3. Make questions conversational and natural
4. Include appropriate question types and validation

OUTPUT FORMAT (JSON):
[
    {{
        ""id"": ""conditional_q1"",
        ""text"": ""Follow-up question text"",
        ""context"": ""Why this question is being asked"",
        ""type"": ""OpenEnded"",
        ""suggestedResponses"": [""option1"", ""option2""],
        ""validation"": {{
            ""required"": true,
            ""customValidation"": ""validation rule""
        }}
    }}
]";
    }

    private async Task UpdateConversationContextAsync(
        ConversationContext context, 
        string userResponse, 
        ConversationResponse response)
    {
        var exchange = new ConversationExchange
        {
            QuestionId = context.CurrentQuestionId,
            SectionId = context.CurrentSectionId,
            Question = context.History.LastOrDefault()?.Question ?? "",
            UserResponse = userResponse,
            ExtractedValues = response.ExtractedValues,
            ConfidenceScore = response.ConfidenceScore,
            RequiresFollowUp = response.RequiresFollowUp
        };

        context.History.Add(exchange);
        
        // Update extracted data
        foreach (var kvp in response.ExtractedValues)
        {
            context.ExtractedData[kvp.Key] = kvp.Value;
        }

        // Update current position
        if (!string.IsNullOrEmpty(response.NextQuestionId))
        {
            context.CurrentQuestionId = response.NextQuestionId;
        }
        
        if (!string.IsNullOrEmpty(response.NextSectionId))
        {
            context.CurrentSectionId = response.NextSectionId;
        }

        if (response.IsComplete)
        {
            context.State = ConversationState.Completed;
        }

        context.LastUpdated = DateTime.UtcNow;
    }

    #region Response Parsing Methods

    private ConversationFlow ParseConversationFlowResponse(string response, string templateType)
    {
        try
        {
            var flow = JsonSerializer.Deserialize<ConversationFlow>(response, _jsonOptions);
            if (flow == null)
                throw new InvalidOperationException("Failed to deserialize conversation flow");

            flow.TemplateType = templateType;
            return flow;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse conversation flow response: {Response}", response);
            throw new InvalidOperationException("Invalid conversation flow format received from AI", ex);
        }
    }

    private ConversationResponse ParseConversationResponse(string response)
    {
        try
        {
            var conversationResponse = JsonSerializer.Deserialize<ConversationResponse>(response, _jsonOptions);
            return conversationResponse ?? throw new InvalidOperationException("Failed to deserialize conversation response");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse conversation response: {Response}", response);
            throw new InvalidOperationException("Invalid conversation response format received from AI", ex);
        }
    }

    private ExtractedDecisions ParseExtractedDecisions(string response)
    {
        try
        {
            var decisions = JsonSerializer.Deserialize<ExtractedDecisions>(response, _jsonOptions);
            return decisions ?? throw new InvalidOperationException("Failed to deserialize extracted decisions");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse extracted decisions: {Response}", response);
            throw new InvalidOperationException("Invalid extracted decisions format received from AI", ex);
        }
    }

    private SectionCompletionStatus ParseSectionCompletionStatus(string response, string sectionId)
    {
        try
        {
            var status = JsonSerializer.Deserialize<SectionCompletionStatus>(response, _jsonOptions);
            if (status == null)
                throw new InvalidOperationException("Failed to deserialize section completion status");

            status.SectionId = sectionId;
            return status;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse section completion status: {Response}", response);
            throw new InvalidOperationException("Invalid section completion status format received from AI", ex);
        }
    }

    private List<ConversationQuestion> ParseConditionalQuestions(string response)
    {
        try
        {
            var questions = JsonSerializer.Deserialize<List<ConversationQuestion>>(response, _jsonOptions);
            return questions ?? new List<ConversationQuestion>();
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse conditional questions: {Response}", response);
            throw new InvalidOperationException("Invalid conditional questions format received from AI", ex);
        }
    }

    #endregion

    #endregion
}

// OpenAI API Response Models
internal class OpenAIResponse
{
    public List<Choice>? Choices { get; set; }
}

internal class Choice
{
    public Message? Message { get; set; }
}

internal class Message
{
    public string? Content { get; set; }
}
