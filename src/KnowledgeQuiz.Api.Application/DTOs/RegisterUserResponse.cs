using KnowledgeQuiz.Api.Domain.Enums;

namespace KnowledgeQuiz.Api.Application.DTOs;

public record RegisterUserResponse(bool Success, RegisterFailureReason? FailureReason = null);