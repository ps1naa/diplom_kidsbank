namespace KidBank.Application.Common.Interfaces;

public interface ISettingsNotifier
{
    Task NotifySettingsChangedAsync(CancellationToken cancellationToken = default);
}
