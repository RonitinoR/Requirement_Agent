using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using RequirementAgent.Api.Models.AI;

namespace RequirementAgent.Api.Services.AI;

public interface IConversationStateManager
{
    Task<ConversationContext> CreateConversationAsync(string flowId, string userId, string sessionId);
    Task<ConversationContext?> GetConversationAsync(string conversationId);
    Task UpdateConversationAsync(ConversationContext context);
    Task<bool> DeleteConversationAsync(string conversationId);
    Task<List<ConversationContext>> GetUserConversationsAsync(string userId);
    Task<ConversationContext?> GetActiveConversationAsync(string userId, string flowId);
    Task SaveConversationSnapshotAsync(ConversationContext context);
    Task<List<ConversationContext>> GetConversationHistoryAsync(string conversationId);
}

public class ConversationStateManager : IConversationStateManager
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<ConversationStateManager> _logger;
    private readonly ConcurrentDictionary<string, ConversationContext> _conversations;
    private readonly ConcurrentDictionary<string, List<ConversationContext>> _conversationHistory;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromHours(24);
    private readonly JsonSerializerOptions _jsonOptions;

    public ConversationStateManager(
        IMemoryCache cache,
        ILogger<ConversationStateManager> logger)
    {
        _cache = cache;
        _logger = logger;
        _conversations = new ConcurrentDictionary<string, ConversationContext>();
        _conversationHistory = new ConcurrentDictionary<string, List<ConversationContext>>();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }

    public async Task<ConversationContext> CreateConversationAsync(string flowId, string userId, string sessionId)
    {
        try
        {
            _logger.LogInformation("Creating new conversation for user: {UserId}, flow: {FlowId}", userId, flowId);

            var context = new ConversationContext
            {
                ConversationId = Guid.NewGuid().ToString(),
                FlowId = flowId,
                UserId = userId,
                SessionId = sessionId,
                State = ConversationState.NotStarted,
                LastUpdated = DateTime.UtcNow
            };

            // Store in memory cache
            var cacheKey = GetConversationCacheKey(context.ConversationId);
            _cache.Set(cacheKey, context, _cacheExpiration);

            // Store in concurrent dictionary for fast access
            _conversations.TryAdd(context.ConversationId, context);

            // Initialize conversation history
            _conversationHistory.TryAdd(context.ConversationId, new List<ConversationContext>());

            _logger.LogInformation("Created conversation: {ConversationId}", context.ConversationId);
            
            return context;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating conversation for user: {UserId}", userId);
            throw new InvalidOperationException($"Failed to create conversation: {ex.Message}", ex);
        }
    }

    public async Task<ConversationContext?> GetConversationAsync(string conversationId)
    {
        try
        {
            // Try to get from concurrent dictionary first (fastest)
            if (_conversations.TryGetValue(conversationId, out var cachedContext))
            {
                return cachedContext;
            }

            // Try to get from memory cache
            var cacheKey = GetConversationCacheKey(conversationId);
            if (_cache.TryGetValue(cacheKey, out ConversationContext? context) && context != null)
            {
                // Re-add to concurrent dictionary for faster future access
                _conversations.TryAdd(conversationId, context);
                return context;
            }

            _logger.LogWarning("Conversation not found: {ConversationId}", conversationId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving conversation: {ConversationId}", conversationId);
            throw new InvalidOperationException($"Failed to retrieve conversation: {ex.Message}", ex);
        }
    }

    public async Task UpdateConversationAsync(ConversationContext context)
    {
        try
        {
            context.LastUpdated = DateTime.UtcNow;

            // Update in concurrent dictionary
            _conversations.AddOrUpdate(context.ConversationId, context, (key, oldValue) => context);

            // Update in memory cache
            var cacheKey = GetConversationCacheKey(context.ConversationId);
            _cache.Set(cacheKey, context, _cacheExpiration);

            // Save snapshot for history tracking
            await SaveConversationSnapshotAsync(context);

            _logger.LogDebug("Updated conversation: {ConversationId}", context.ConversationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating conversation: {ConversationId}", context.ConversationId);
            throw new InvalidOperationException($"Failed to update conversation: {ex.Message}", ex);
        }
    }

    public async Task<bool> DeleteConversationAsync(string conversationId)
    {
        try
        {
            _logger.LogInformation("Deleting conversation: {ConversationId}", conversationId);

            // Remove from concurrent dictionary
            var removed = _conversations.TryRemove(conversationId, out _);

            // Remove from memory cache
            var cacheKey = GetConversationCacheKey(conversationId);
            _cache.Remove(cacheKey);

            // Keep history for audit purposes, don't delete
            
            _logger.LogInformation("Deleted conversation: {ConversationId}, Success: {Success}", conversationId, removed);
            return removed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting conversation: {ConversationId}", conversationId);
            throw new InvalidOperationException($"Failed to delete conversation: {ex.Message}", ex);
        }
    }

    public async Task<List<ConversationContext>> GetUserConversationsAsync(string userId)
    {
        try
        {
            var userConversations = _conversations.Values
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.LastUpdated)
                .ToList();

            _logger.LogDebug("Retrieved {Count} conversations for user: {UserId}", userConversations.Count, userId);
            
            return userConversations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving conversations for user: {UserId}", userId);
            throw new InvalidOperationException($"Failed to retrieve user conversations: {ex.Message}", ex);
        }
    }

    public async Task<ConversationContext?> GetActiveConversationAsync(string userId, string flowId)
    {
        try
        {
            var activeConversation = _conversations.Values
                .Where(c => c.UserId == userId && 
                           c.FlowId == flowId && 
                           c.State == ConversationState.InProgress)
                .OrderByDescending(c => c.LastUpdated)
                .FirstOrDefault();

            if (activeConversation != null)
            {
                _logger.LogDebug("Found active conversation: {ConversationId} for user: {UserId}", 
                    activeConversation.ConversationId, userId);
            }

            return activeConversation;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active conversation for user: {UserId}, flow: {FlowId}", userId, flowId);
            throw new InvalidOperationException($"Failed to retrieve active conversation: {ex.Message}", ex);
        }
    }

    public async Task SaveConversationSnapshotAsync(ConversationContext context)
    {
        try
        {
            // Create a deep copy for the snapshot
            var snapshot = DeepCopyContext(context);
            
            if (_conversationHistory.TryGetValue(context.ConversationId, out var history))
            {
                history.Add(snapshot);
                
                // Keep only the last 50 snapshots to prevent memory bloat
                if (history.Count > 50)
                {
                    history.RemoveRange(0, history.Count - 50);
                }
            }
            else
            {
                _conversationHistory.TryAdd(context.ConversationId, new List<ConversationContext> { snapshot });
            }

            _logger.LogDebug("Saved conversation snapshot: {ConversationId}", context.ConversationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving conversation snapshot: {ConversationId}", context.ConversationId);
            // Don't throw here as this is not critical for the main flow
        }
    }

    public async Task<List<ConversationContext>> GetConversationHistoryAsync(string conversationId)
    {
        try
        {
            if (_conversationHistory.TryGetValue(conversationId, out var history))
            {
                return history.ToList(); // Return a copy
            }

            return new List<ConversationContext>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving conversation history: {ConversationId}", conversationId);
            throw new InvalidOperationException($"Failed to retrieve conversation history: {ex.Message}", ex);
        }
    }

    #region Private Methods

    private string GetConversationCacheKey(string conversationId)
    {
        return $"conversation:{conversationId}";
    }

    private ConversationContext DeepCopyContext(ConversationContext original)
    {
        try
        {
            var json = JsonSerializer.Serialize(original, _jsonOptions);
            var copy = JsonSerializer.Deserialize<ConversationContext>(json, _jsonOptions);
            return copy ?? throw new InvalidOperationException("Failed to create deep copy");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating deep copy of conversation context");
            throw new InvalidOperationException($"Failed to create conversation snapshot: {ex.Message}", ex);
        }
    }

    #endregion

    #region Cleanup and Maintenance

    public async Task CleanupExpiredConversationsAsync()
    {
        try
        {
            var cutoffTime = DateTime.UtcNow.Subtract(_cacheExpiration);
            var expiredConversations = _conversations.Values
                .Where(c => c.LastUpdated < cutoffTime)
                .ToList();

            foreach (var conversation in expiredConversations)
            {
                await DeleteConversationAsync(conversation.ConversationId);
            }

            if (expiredConversations.Any())
            {
                _logger.LogInformation("Cleaned up {Count} expired conversations", expiredConversations.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during conversation cleanup");
        }
    }

    public async Task<ConversationStatistics> GetStatisticsAsync()
    {
        try
        {
            var stats = new ConversationStatistics
            {
                TotalConversations = _conversations.Count,
                ActiveConversations = _conversations.Values.Count(c => c.State == ConversationState.InProgress),
                CompletedConversations = _conversations.Values.Count(c => c.State == ConversationState.Completed),
                AverageExchangesPerConversation = _conversations.Values.Any() 
                    ? _conversations.Values.Average(c => c.History.Count) 
                    : 0,
                TotalHistorySnapshots = _conversationHistory.Values.Sum(h => h.Count)
            };

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating conversation statistics");
            throw new InvalidOperationException($"Failed to calculate statistics: {ex.Message}", ex);
        }
    }

    #endregion
}

public class ConversationStatistics
{
    public int TotalConversations { get; set; }
    public int ActiveConversations { get; set; }
    public int CompletedConversations { get; set; }
    public double AverageExchangesPerConversation { get; set; }
    public int TotalHistorySnapshots { get; set; }
}

