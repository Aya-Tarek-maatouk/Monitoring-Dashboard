using System.Resources;

namespace MonitoringDashboard.Application.Common.Resources;

public static class ValidationMessages
{
    private static readonly ResourceManager Resources = new(
        "MonitoringDashboard.Application.Common.Resources.ValidationMessages", typeof(ValidationMessages).Assembly);

    public static string RoleNameInvalid => Resources.GetString(nameof(RoleNameInvalid))!;
    public static string RoleDescriptionInvalid => Resources.GetString(nameof(RoleDescriptionInvalid))!;
    public static string PermissionIdsInvalid => Resources.GetString(nameof(PermissionIdsInvalid))!;
    public static string UserNameInvalid => Resources.GetString(nameof(UserNameInvalid))!;
    public static string EmailInvalid => Resources.GetString(nameof(EmailInvalid))!;
    public static string RegistrationPasswordInvalid => Resources.GetString(nameof(RegistrationPasswordInvalid))!;
    public static string LoginIdentifierInvalid => Resources.GetString(nameof(LoginIdentifierInvalid))!;
    public static string LoginPasswordInvalid => Resources.GetString(nameof(LoginPasswordInvalid))!;
    public static string RoleIdInvalid => Resources.GetString(nameof(RoleIdInvalid))!;
    public static string ServerNameInvalid => Resources.GetString(nameof(ServerNameInvalid))!;
    public static string ServerAddressInvalid => Resources.GetString(nameof(ServerAddressInvalid))!;
    public static string ServerDescriptionInvalid => Resources.GetString(nameof(ServerDescriptionInvalid))!;
    public static string ServerStatusInvalid => Resources.GetString(nameof(ServerStatusInvalid))!;
    public static string MetricUsageInvalid => Resources.GetString(nameof(MetricUsageInvalid))!;
    public static string ResponseTimeInvalid => Resources.GetString(nameof(ResponseTimeInvalid))!;
    public static string ReportTimeRangeInvalid => Resources.GetString(nameof(ReportTimeRangeInvalid))!;
    public static string UserAlreadyExists => Resources.GetString(nameof(UserAlreadyExists))!;
    public static string UserRoleMissing => Resources.GetString(nameof(UserRoleMissing))!;
    public static string InvalidCredentials => Resources.GetString(nameof(InvalidCredentials))!;
    public static string UserNotFound => Resources.GetString(nameof(UserNotFound))!;
    public static string RoleNotFound => Resources.GetString(nameof(RoleNotFound))!;
    public static string CurrentRoleMissing => Resources.GetString(nameof(CurrentRoleMissing))!;
    public static string LastAdminCannotBeDemoted => Resources.GetString(nameof(LastAdminCannotBeDemoted))!;
    public static string RoleAlreadyExists => Resources.GetString(nameof(RoleAlreadyExists))!;
    public static string ServerNotFound => Resources.GetString(nameof(ServerNotFound))!;
    public static string ServerHasHistory => Resources.GetString(nameof(ServerHasHistory))!;
    public static string ReportNotFound => Resources.GetString(nameof(ReportNotFound))!;
    public static string ReportNotReady => Resources.GetString(nameof(ReportNotReady))!;
    public static string ReportFileMissing => Resources.GetString(nameof(ReportFileMissing))!;
    public static string InvalidRequest => Resources.GetString(nameof(InvalidRequest))!;
    public static string AuthenticationFailed => Resources.GetString(nameof(AuthenticationFailed))!;
    public static string ResourceNotFound => Resources.GetString(nameof(ResourceNotFound))!;
    public static string Conflict => Resources.GetString(nameof(Conflict))!;
    public static string UnexpectedError => Resources.GetString(nameof(UnexpectedError))!;
    public static string TryAgainLater => Resources.GetString(nameof(TryAgainLater))!;
    public static string InvalidPaginationTitle => Resources.GetString(nameof(InvalidPaginationTitle))!;
    public static string InvalidPaginationDetails => Resources.GetString(nameof(InvalidPaginationDetails))!;
    public static string JobNotFound => Resources.GetString(nameof(JobNotFound))!;
    public static string JobRetryConflict => Resources.GetString(nameof(JobRetryConflict))!;
    public static string SessionNoLongerValid => Resources.GetString(nameof(SessionNoLongerValid))!;
    public static string HubServerNotFound => Resources.GetString(nameof(HubServerNotFound))!;
}