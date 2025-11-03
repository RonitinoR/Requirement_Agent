using RequirementAgent.Api.Models.Enums;

namespace RequirementAgent.Api.Dtos.Questions;

public record QuestionDto(
    Guid Id,
    Guid PermitTypeId,
    int Order,
    string Key,
    string Prompt,
    QuestionType Type,
    string? OptionsJson,
    bool Required,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
