using RequirementAgent.Api.Dtos.Uploads;
using RequirementAgent.Api.Models;

namespace RequirementAgent.Api.Extensions;

public static class UploadExtensions
{
    public static UploadDto ToDto(this Upload upload) => new(
        upload.Id,
        upload.SubmissionId,
        upload.FileName,
        upload.Mime,
        upload.Size,
        upload.Url,
        upload.CreatedAt);
}
