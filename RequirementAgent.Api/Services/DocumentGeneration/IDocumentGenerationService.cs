namespace RequirementAgent.Api.Services.DocumentGeneration;

public interface IDocumentGenerationService
{
    Task<DocumentResult> GenerateUseCaseAsync(Guid submissionId, CancellationToken cancellationToken = default);
    Task<DocumentResult> GenerateUserStoriesAsync(Guid submissionId, CancellationToken cancellationToken = default);
    Task<DocumentResult> GenerateDataDictionaryAsync(Guid submissionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DocumentResult>> GenerateAiPackAsync(Guid submissionId, CancellationToken cancellationToken = default);
}
