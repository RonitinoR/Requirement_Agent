using System.Text.Json.Serialization;

namespace RequirementAgent.Api.Models.AI;

public class ConversationFlow
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string TemplateType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<ConversationSection> Sections { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class ConversationSection
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Order { get; set; }
    public List<ConversationQuestion> Questions { get; set; } = new();
    public List<string> Prerequisites { get; set; } = new(); // Section IDs that must be completed first
    public ConditionalLogic? ConditionalLogic { get; set; }
    public bool IsOptional { get; set; }
}

public class ConversationQuestion
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Text { get; set; } = string.Empty;
    public string Context { get; set; } = string.Empty;
    public QuestionType Type { get; set; }
    public List<string> SuggestedResponses { get; set; } = new();
    public ValidationRules? Validation { get; set; }
    public ConditionalLogic? ConditionalLogic { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class ConversationContext
{
    public string ConversationId { get; set; } = Guid.NewGuid().ToString();
    public string FlowId { get; set; } = string.Empty;
    public string CurrentSectionId { get; set; } = string.Empty;
    public string CurrentQuestionId { get; set; } = string.Empty;
    public List<ConversationExchange> History { get; set; } = new();
    public Dictionary<string, object> ExtractedData { get; set; } = new();
    public ConversationState State { get; set; } = ConversationState.InProgress;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public string UserId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
}

public class ConversationExchange
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string QuestionId { get; set; } = string.Empty;
    public string SectionId { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public string UserResponse { get; set; } = string.Empty;
    public Dictionary<string, object> ExtractedValues { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public double ConfidenceScore { get; set; }
    public bool RequiresFollowUp { get; set; }
}

public class ConversationResponse
{
    public string NextQuestionId { get; set; } = string.Empty;
    public string NextQuestion { get; set; } = string.Empty;
    public string NextSectionId { get; set; } = string.Empty;
    public bool IsComplete { get; set; }
    public bool RequiresFollowUp { get; set; }
    public List<string> FollowUpQuestions { get; set; } = new();
    public Dictionary<string, object> ExtractedValues { get; set; } = new();
    public double ConfidenceScore { get; set; }
    public string Reasoning { get; set; } = string.Empty;
    public List<string> Suggestions { get; set; } = new();
}

public class ExtractedDecisions
{
    public Dictionary<string, DecisionPoint> Decisions { get; set; } = new();
    public List<string> MissingInformation { get; set; } = new();
    public List<string> Assumptions { get; set; } = new();
    public double OverallConfidence { get; set; }
    public DateTime ExtractedAt { get; set; } = DateTime.UtcNow;
}

public class DecisionPoint
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public List<string> SupportingEvidence { get; set; } = new();
    public string Source { get; set; } = string.Empty; // Question ID or inference
}

public class SectionCompletionStatus
{
    public string SectionId { get; set; } = string.Empty;
    public bool IsComplete { get; set; }
    public double CompletionPercentage { get; set; }
    public List<string> MissingRequirements { get; set; } = new();
    public List<string> OptionalItems { get; set; } = new();
    public string Reasoning { get; set; } = string.Empty;
}

public class ConditionalLogic
{
    public string Condition { get; set; } = string.Empty; // Natural language condition
    public string TriggerValue { get; set; } = string.Empty;
    public ConditionalAction Action { get; set; }
    public string TargetId { get; set; } = string.Empty; // Question or Section ID
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public class ValidationRules
{
    public bool Required { get; set; }
    public string Pattern { get; set; } = string.Empty; // Regex pattern
    public int? MinLength { get; set; }
    public int? MaxLength { get; set; }
    public List<string> AllowedValues { get; set; } = new();
    public string CustomValidation { get; set; } = string.Empty; // Natural language validation rule
}

public enum QuestionType
{
    OpenEnded,
    YesNo,
    MultipleChoice,
    Numeric,
    Date,
    Email,
    Phone,
    Address,
    FileUpload,
    Confirmation
}

public enum ConversationState
{
    NotStarted,
    InProgress,
    Completed,
    Paused,
    Error,
    Abandoned
}

public enum ConditionalAction
{
    ShowQuestion,
    HideQuestion,
    ShowSection,
    HideSection,
    SetValue,
    RequireFollowUp,
    CompleteSection,
    BranchToSection
}

