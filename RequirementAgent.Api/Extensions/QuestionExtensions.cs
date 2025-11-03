using RequirementAgent.Api.Dtos.Questions;
using RequirementAgent.Api.Models;

namespace RequirementAgent.Api.Extensions;

public static class QuestionExtensions
{
    public static QuestionDto ToDto(this Question question) => new(
        question.Id,
        question.PermitTypeId,
        question.Order,
        question.Key,
        question.Prompt,
        question.Type,
        question.OptionsJson,
        question.Required,
        question.CreatedAt,
        question.UpdatedAt);
}
