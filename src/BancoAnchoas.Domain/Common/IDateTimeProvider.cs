namespace BancoAnchoas.Domain.Common;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
