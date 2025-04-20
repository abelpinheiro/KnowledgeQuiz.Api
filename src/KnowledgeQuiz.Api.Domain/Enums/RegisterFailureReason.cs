namespace KnowledgeQuiz.Api.Domain.Enums;

public enum RegisterFailureReason 
{
        UserAlreadyExists,
        InvalidRole,
        UserNotFound,
        UnknownError
}