using RequirementAgent.Api.Dtos.Submissions;
using RequirementAgent.Api.Models;

namespace RequirementAgent.Api.Extensions;

public static class SubmissionExtensions
{
    public static SubmissionDto ToDto(this Submission submission) => new(
        submission.Id,
        submission.PermitTypeId,
        submission.PermitType.Name,
        submission.ClientEmail,
        submission.ProjectName,
        submission.AnswersJson,
        submission.Transcript,
        submission.CreatedAt,
        submission.UpdatedAt);
}
