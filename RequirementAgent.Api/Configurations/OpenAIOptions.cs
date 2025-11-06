namespace RequirementAgent.Api.Configurations;

public class OpenAIOptions
{
    public const string SectionName = "OpenAI";

    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.openai.com";
    public string Model { get; set; } = "gpt-4";
    public double Temperature { get; set; } = 0.7;
    public int MaxTokens { get; set; } = 2000;
    public double TopP { get; set; } = 1.0;
    public double FrequencyPenalty { get; set; } = 0.0;
    public double PresencePenalty { get; set; } = 0.0;
    public int TimeoutSeconds { get; set; } = 60;
    public string SystemPrompt { get; set; } = @"You are an expert requirements analyst and conversation designer. 
        Your role is to help convert formal requirement templates into natural, conversational Q&A flows 
        that gather the same information through engaging dialogue. You excel at:
        
        1. Breaking complex requirements into digestible conversation segments
        2. Creating natural, conversational questions that feel human
        3. Identifying conditional logic and follow-up scenarios
        4. Extracting structured data from conversational responses
        5. Maintaining context throughout multi-turn conversations
        
        Always respond with valid JSON in the exact format requested. 
        Be thorough but concise, and prioritize user experience in your conversation design.";
}

