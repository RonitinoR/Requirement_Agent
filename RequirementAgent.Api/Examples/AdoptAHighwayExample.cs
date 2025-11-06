using RequirementAgent.Api.Models.AI;
using RequirementAgent.Api.Services.AI;

namespace RequirementAgent.Api.Examples;

/// <summary>
/// Example demonstrating how to use the OpenAI service for Adopt-A-Highway requirements
/// </summary>
public class AdoptAHighwayExample
{
    private readonly IOpenAIService _openAIService;
    private readonly IConversationStateManager _stateManager;

    public AdoptAHighwayExample(IOpenAIService openAIService, IConversationStateManager stateManager)
    {
        _openAIService = openAIService;
        _stateManager = stateManager;
    }

    /// <summary>
    /// Sample Adopt-A-Highway template content
    /// </summary>
    public static string SampleTemplate => """
        ADOPT-A-HIGHWAY PROGRAM REQUIREMENTS

        1. APPLICANT INFORMATION
        - Organization name and type (individual, business, non-profit, etc.)
        - Primary contact person with title
        - Mailing address and phone number
        - Email address for correspondence
        - Tax ID number (if applicable)

        2. HIGHWAY SEGMENT SELECTION
        - Preferred highway route and mile markers
        - Alternative route preferences
        - Reason for selecting this segment
        - Previous experience with highway maintenance

        3. COMMITMENT DETAILS
        - Proposed adoption period (minimum 2 years)
        - Frequency of cleanup activities (minimum quarterly)
        - Number of volunteers expected per cleanup
        - Safety training completion commitment
        - Insurance coverage verification

        4. RECOGNITION PREFERENCES
        - Sign design preferences
        - Organization name as it should appear on signs
        - Logo usage permissions
        - Special recognition requests

        5. SAFETY AND LIABILITY
        - Acknowledgment of safety requirements
        - Liability insurance coverage details
        - Emergency contact information
        - Safety equipment availability

        6. ENVIRONMENTAL CONSIDERATIONS
        - Waste disposal method preferences
        - Recycling program participation
        - Special environmental concerns
        - Native plant preservation commitment
        """;

    /// <summary>
    /// Example: Create a conversation flow from the template
    /// </summary>
    public async Task<ConversationFlow> CreateAdoptAHighwayFlowAsync()
    {
        var flow = await _openAIService.CreateConversationFlowAsync(
            SampleTemplate, 
            "Adopt-A-Highway");

        // The AI will break this into conversational sections like:
        // 1. "Getting to Know You" - Organization and contact info
        // 2. "Choosing Your Highway" - Route selection and preferences  
        // 3. "Making the Commitment" - Timeline and volunteer details
        // 4. "Recognition and Signage" - How you want to be recognized
        // 5. "Safety First" - Safety and liability requirements
        // 6. "Environmental Stewardship" - Environmental considerations

        return flow;
    }

    /// <summary>
    /// Example: Complete conversation workflow
    /// </summary>
    public async Task<ExtractedDecisions> RunCompleteConversationAsync(string userId)
    {
        // Step 1: Create conversation flow
        var flow = await CreateAdoptAHighwayFlowAsync();

        // Step 2: Start conversation session
        var context = await _stateManager.CreateConversationAsync(flow.Id, userId, Guid.NewGuid().ToString());
        context.State = ConversationState.InProgress;
        context.CurrentSectionId = flow.Sections.First().Id;
        context.CurrentQuestionId = flow.Sections.First().Questions.First().Id;

        // Step 3: Simulate conversation exchanges
        var sampleResponses = new[]
        {
            "We're the Green Valley Environmental Club, a local non-profit organization.",
            "I'm Sarah Johnson, the club president. You can reach me at sarah@greenvalley.org",
            "We'd like to adopt the stretch of Highway 101 between mile markers 15 and 18.",
            "We're committed to cleaning up quarterly for the next 3 years.",
            "We typically have 8-12 volunteers for each cleanup event.",
            "Yes, we have liability insurance through our non-profit coverage.",
            "Please put 'Green Valley Environmental Club' on the sign with our tree logo.",
            "We're very committed to recycling and protecting native plants in the area."
        };

        // Process each response
        foreach (var response in sampleResponses)
        {
            var conversationResponse = await _openAIService.ProcessUserResponseAsync(context, response);
            await _stateManager.UpdateConversationAsync(context);

            // Handle conditional questions if needed
            if (conversationResponse.RequiresFollowUp)
            {
                var conditionalQuestions = await _openAIService.GenerateConditionalQuestionsAsync(
                    context, response);
                
                // Process follow-up questions...
            }
        }

        // Step 4: Extract final decisions
        var extractedDecisions = await _openAIService.ExtractDecisionsAsync(context);

        return extractedDecisions;
    }

    /// <summary>
    /// Example: Handle conditional logic
    /// </summary>
    public async Task<List<ConversationQuestion>> HandleConditionalLogicAsync(
        ConversationContext context, 
        string userResponse)
    {
        // Example: If user mentions they're a business, ask about corporate sponsorship
        if (userResponse.ToLower().Contains("business") || userResponse.ToLower().Contains("company"))
        {
            return await _openAIService.GenerateConditionalQuestionsAsync(
                context, 
                "User mentioned they represent a business - need corporate sponsorship details");
        }

        // Example: If user mentions large volunteer group, ask about coordination
        if (userResponse.Contains("20") || userResponse.ToLower().Contains("large group"))
        {
            return await _openAIService.GenerateConditionalQuestionsAsync(
                context,
                "User has large volunteer group - need coordination and logistics details");
        }

        return new List<ConversationQuestion>();
    }

    /// <summary>
    /// Example: Validate section completion
    /// </summary>
    public async Task<bool> ValidateApplicantInfoSectionAsync(ConversationContext context)
    {
        var status = await _openAIService.ValidateSectionCompletionAsync(
            context, 
            "applicant_information");

        // Check if we have all required information
        return status.IsComplete && status.CompletionPercentage >= 90.0;
    }

    /// <summary>
    /// Example: Extract structured data for permit processing
    /// </summary>
    public async Task<AdoptAHighwayApplication> ExtractApplicationDataAsync(ConversationContext context)
    {
        var decisions = await _openAIService.ExtractDecisionsAsync(context);

        var application = new AdoptAHighwayApplication();

        // Map extracted decisions to structured application
        foreach (var decision in decisions.Decisions)
        {
            switch (decision.Key.ToLower())
            {
                case "organization_name":
                    application.OrganizationName = decision.Value.Value;
                    break;
                case "contact_person":
                    application.ContactPerson = decision.Value.Value;
                    break;
                case "email":
                    application.Email = decision.Value.Value;
                    break;
                case "highway_route":
                    application.PreferredRoute = decision.Value.Value;
                    break;
                case "adoption_period":
                    if (int.TryParse(decision.Value.Value, out var years))
                        application.AdoptionPeriodYears = years;
                    break;
                case "cleanup_frequency":
                    application.CleanupFrequency = decision.Value.Value;
                    break;
                case "volunteer_count":
                    if (int.TryParse(decision.Value.Value, out var count))
                        application.ExpectedVolunteers = count;
                    break;
                case "has_insurance":
                    application.HasLiabilityInsurance = decision.Value.Value.ToLower().Contains("yes");
                    break;
                case "sign_text":
                    application.SignText = decision.Value.Value;
                    break;
            }
        }

        return application;
    }
}

/// <summary>
/// Structured application data extracted from conversation
/// </summary>
public class AdoptAHighwayApplication
{
    public string OrganizationName { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PreferredRoute { get; set; } = string.Empty;
    public int AdoptionPeriodYears { get; set; }
    public string CleanupFrequency { get; set; } = string.Empty;
    public int ExpectedVolunteers { get; set; }
    public bool HasLiabilityInsurance { get; set; }
    public string SignText { get; set; } = string.Empty;
    public DateTime ApplicationDate { get; set; } = DateTime.UtcNow;
}

