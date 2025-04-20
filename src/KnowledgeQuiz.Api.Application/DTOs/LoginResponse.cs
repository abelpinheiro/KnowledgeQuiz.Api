using KnowledgeQuiz.Api.Domain.Enums;

namespace KnowledgeQuiz.Api.Application.DTOs;

public record LoginResponse(bool Success, string? Token = null!, LoginFailureReason? Reason = null);