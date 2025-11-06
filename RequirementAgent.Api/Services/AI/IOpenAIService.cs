using RequirementAgent.Api.Models.AI;

namespace RequirementAgent.Api.Services.AI;

public interface IOpenAIService
{
    /// <summary>
    /// Converts a requirements template into a conversational Q&A flow
    /// </summary>
    Task<ConversationFlow> CreateConversationFlowAsync(string templateContent, string templateType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes user response and generates next question or completes section
    /// </summary>
    Task<ConversationResponse> ProcessUserResponseAsync(ConversationContext context, string userResponse, CancellationToken cancellationToken = default);

    /// <summary>
    /// Extracts structured decisions from conversation history
    /// </summary>
    Task<ExtractedDecisions> ExtractDecisionsAsync(ConversationContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates if conversation section is complete
    /// </summary>
    Task<SectionCompletionStatus> ValidateSectionCompletionAsync(ConversationContext context, string sectionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates follow-up questions based on conditional logic
    /// </summary>
    Task<List<ConversationQuestion>> GenerateConditionalQuestionsAsync(ConversationContext context, string triggerResponse, CancellationToken cancellationToken = default);
}

