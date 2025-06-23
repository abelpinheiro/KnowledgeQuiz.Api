using System.Diagnostics.Metrics;

namespace KnowledgeQuiz.Api.Infrastructure.Observability.Telemetry;

public class AppMetrics
{
    private static readonly Meter Meter = new("KnowledgeQuiz.Api", "1.0.0");

    // --- Metrics for User operations ---
    // Auth Metrics
    public static readonly Counter<long> LoginAttempts = Meter.CreateCounter<long>("auth_login_attempts_total");
    public static readonly Counter<long> LoginFailures = Meter.CreateCounter<long>("auth_login_failures_total");
    public static readonly Counter<long> LoginSuccesses = Meter.CreateCounter<long>("auth_login_successes_total");
    public static readonly Counter<long> RegistrationAttempts = Meter.CreateCounter<long>("auth_registration_attempts_total");
    public static readonly Counter<long> RegistrationFailures = Meter.CreateCounter<long>("auth_registration_failures_total");
    public static readonly Counter<long> RegistrationSuccesses = Meter.CreateCounter<long>("auth_registration_successes_total");

    // User Metrics
    public static readonly Counter<long> UserFetches = Meter.CreateCounter<long>("users_get_total");
    public static readonly Counter<long> UserCreations = Meter.CreateCounter<long>("users_create_total");
    public static readonly Counter<long> RoleAssignments = Meter.CreateCounter<long>("users_assign_role_total");

    // DB Metrics
    public static readonly Counter<long> DbUserQueries = Meter.CreateCounter<long>("db_user_queries_total");
    public static readonly Counter<long> DbRoleQueries = Meter.CreateCounter<long>("db_role_queries_total");
    
    // Operation duration histogram (ms)
    public static readonly Histogram<double> OperationDurationHistogram = Meter.CreateHistogram<double>(
        "operation_duration_ms",
        unit: "ms",
        description: "Duration of operations in milliseconds"
    );
    //TODO: Add more counters for Quiz, Score, etc...
}