using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RequirementAgent.Api.Models.AI;
using RequirementAgent.Api.Services.AI;
using System.Security.Claims;

namespace RequirementAgent.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ConversationController : ControllerBase
{
    private readonly IOpenAIService _openAIService;
    private readonly IConversationStateManager _stateManager;
    private readonly ILogger<ConversationController> _logger;

    public ConversationController(
        IOpenAIService openAIService,
        IConversationStateManager stateManager,
        ILogger<ConversationController> logger)
    {
        _openAIService = openAIService;
        _stateManager = stateManager;
        _logger = logger;
    }

    /// <summary>
    /// Create a new conversation flow from a requirements template
    /// </summary>
    [HttpPost("flows")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ConversationFlow>> CreateFlow([FromBody] CreateFlowRequest request)
    {
        try
        {
            _logger.LogInformation("Creating conversation flow for template type: {TemplateType}", request.TemplateType);

            var flow = await _openAIService.CreateConversationFlowAsync(
                request.TemplateContent, 
                request.TemplateType);

            return Ok(flow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating conversation flow");
            return StatusCode(500, new { error = "Failed to create conversation flow", details = ex.Message });
        }
    }

    /// <summary>
    /// Start a new conversation session
    /// </summary>
    [HttpPost("sessions")]
    public async Task<ActionResult<ConversationContext>> StartConversation([FromBody] StartConversationRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var sessionId = HttpContext.Session.Id ?? Guid.NewGuid().ToString();

            _logger.LogInformation("Starting conversation for user: {UserId}, flow: {FlowId}", userId, request.FlowId);

            // Check if user has an active conversation for this flow
            var existingConversation = await _stateManager.GetActiveConversationAsync(userId, request.FlowId);
            if (existingConversation != null)
            {
                _logger.LogInformation("Resuming existing conversation: {ConversationId}", existingConversation.ConversationId);
                return Ok(existingConversation);
            }

            // Create new conversation
            var context = await _stateManager.CreateConversationAsync(request.FlowId, userId, sessionId);
            context.State = ConversationState.InProgress;
            
            await _stateManager.UpdateConversationAsync(context);

            return Ok(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting conversation");
            return StatusCode(500, new { error = "Failed to start conversation", details = ex.Message });
        }
    }

    /// <summary>
    /// Process user response and get next question
    /// </summary>
    [HttpPost("sessions/{conversationId}/respond")]
    public async Task<ActionResult<ConversationResponse>> ProcessResponse(
        string conversationId, 
        [FromBody] ProcessResponseRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Processing response for conversation: {ConversationId}", conversationId);

            var context = await _stateManager.GetConversationAsync(conversationId);
            if (context == null)
            {
                return NotFound(new { error = "Conversation not found" });
            }

            if (context.UserId != userId)
            {
                return Forbid("Access denied to this conversation");
            }

            var response = await _openAIService.ProcessUserResponseAsync(context, request.UserResponse);
            
            // Update conversation state
            await _stateManager.UpdateConversationAsync(context);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing user response for conversation: {ConversationId}", conversationId);
            return StatusCode(500, new { error = "Failed to process response", details = ex.Message });
        }
    }

    /// <summary>
    /// Get conversation context and history
    /// </summary>
    [HttpGet("sessions/{conversationId}")]
    public async Task<ActionResult<ConversationContext>> GetConversation(string conversationId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var context = await _stateManager.GetConversationAsync(conversationId);
            
            if (context == null)
            {
                return NotFound(new { error = "Conversation not found" });
            }

            if (context.UserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid("Access denied to this conversation");
            }

            return Ok(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving conversation: {ConversationId}", conversationId);
            return StatusCode(500, new { error = "Failed to retrieve conversation", details = ex.Message });
        }
    }

    /// <summary>
    /// Extract structured decisions from conversation
    /// </summary>
    [HttpPost("sessions/{conversationId}/extract")]
    public async Task<ActionResult<ExtractedDecisions>> ExtractDecisions(string conversationId)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Extracting decisions from conversation: {ConversationId}", conversationId);

            var context = await _stateManager.GetConversationAsync(conversationId);
            if (context == null)
            {
                return NotFound(new { error = "Conversation not found" });
            }

            if (context.UserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid("Access denied to this conversation");
            }

            var decisions = await _openAIService.ExtractDecisionsAsync(context);
            return Ok(decisions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting decisions from conversation: {ConversationId}", conversationId);
            return StatusCode(500, new { error = "Failed to extract decisions", details = ex.Message });
        }
    }

    /// <summary>
    /// Validate section completion
    /// </summary>
    [HttpPost("sessions/{conversationId}/sections/{sectionId}/validate")]
    public async Task<ActionResult<SectionCompletionStatus>> ValidateSection(
        string conversationId, 
        string sectionId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var context = await _stateManager.GetConversationAsync(conversationId);
            
            if (context == null)
            {
                return NotFound(new { error = "Conversation not found" });
            }

            if (context.UserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid("Access denied to this conversation");
            }

            var status = await _openAIService.ValidateSectionCompletionAsync(context, sectionId);
            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating section: {SectionId} in conversation: {ConversationId}", 
                sectionId, conversationId);
            return StatusCode(500, new { error = "Failed to validate section", details = ex.Message });
        }
    }

    /// <summary>
    /// Generate conditional follow-up questions
    /// </summary>
    [HttpPost("sessions/{conversationId}/conditional-questions")]
    public async Task<ActionResult<List<ConversationQuestion>>> GenerateConditionalQuestions(
        string conversationId,
        [FromBody] GenerateConditionalRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var context = await _stateManager.GetConversationAsync(conversationId);
            
            if (context == null)
            {
                return NotFound(new { error = "Conversation not found" });
            }

            if (context.UserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid("Access denied to this conversation");
            }

            var questions = await _openAIService.GenerateConditionalQuestionsAsync(context, request.TriggerResponse);
            return Ok(questions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating conditional questions for conversation: {ConversationId}", conversationId);
            return StatusCode(500, new { error = "Failed to generate conditional questions", details = ex.Message });
        }
    }

    /// <summary>
    /// Get user's conversations
    /// </summary>
    [HttpGet("sessions")]
    public async Task<ActionResult<List<ConversationContext>>> GetUserConversations()
    {
        try
        {
            var userId = GetCurrentUserId();
            var conversations = await _stateManager.GetUserConversationsAsync(userId);
            return Ok(conversations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user conversations");
            return StatusCode(500, new { error = "Failed to retrieve conversations", details = ex.Message });
        }
    }

    /// <summary>
    /// Delete a conversation
    /// </summary>
    [HttpDelete("sessions/{conversationId}")]
    public async Task<ActionResult> DeleteConversation(string conversationId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var context = await _stateManager.GetConversationAsync(conversationId);
            
            if (context == null)
            {
                return NotFound(new { error = "Conversation not found" });
            }

            if (context.UserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid("Access denied to this conversation");
            }

            var deleted = await _stateManager.DeleteConversationAsync(conversationId);
            if (deleted)
            {
                return NoContent();
            }

            return NotFound(new { error = "Conversation not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting conversation: {ConversationId}", conversationId);
            return StatusCode(500, new { error = "Failed to delete conversation", details = ex.Message });
        }
    }

    /// <summary>
    /// Get conversation statistics (Admin only)
    /// </summary>
    [HttpGet("statistics")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ConversationStatistics>> GetStatistics()
    {
        try
        {
            if (_stateManager is ConversationStateManager manager)
            {
                var stats = await manager.GetStatisticsAsync();
                return Ok(stats);
            }

            return BadRequest(new { error = "Statistics not available" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving conversation statistics");
            return StatusCode(500, new { error = "Failed to retrieve statistics", details = ex.Message });
        }
    }

    #region Private Methods

    private string GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
               User.FindFirst("sub")?.Value ?? 
               throw new UnauthorizedAccessException("User ID not found in claims");
    }

    #endregion
}

#region Request/Response Models

public class CreateFlowRequest
{
    public string TemplateContent { get; set; } = string.Empty;
    public string TemplateType { get; set; } = string.Empty;
}

public class StartConversationRequest
{
    public string FlowId { get; set; } = string.Empty;
}

public class ProcessResponseRequest
{
    public string UserResponse { get; set; } = string.Empty;
}

public class GenerateConditionalRequest
{
    public string TriggerResponse { get; set; } = string.Empty;
}

#endregion

