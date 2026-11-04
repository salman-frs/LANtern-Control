using Lantern.Core.Control;

namespace Lantern.Core.Settings;

public static class DeviceTrafficRules
{
    public static TrafficRule Resolve(AppSettings settings, string macAddress)
    {
        ArgumentNullException.ThrowIfNull(settings);
        settings.Devices.TryGetValue(TrafficPolicy.NormalizeMac(macAddress), out var device);
        return new TrafficRule(
            device?.PauseInternet ?? false,
            device?.DownloadKiloBytesPerSecond ?? settings.DefaultDownloadKiloBytesPerSecond,
            device?.UploadKiloBytesPerSecond ?? settings.DefaultUploadKiloBytesPerSecond);
    }

    public static void Save(AppSettings settings, string macAddress, TrafficRule rule)
    {
        ArgumentNullException.ThrowIfNull(settings);
        var macKey = TrafficPolicy.NormalizeMac(macAddress);
        var normalized = rule.Normalize();
        if (!settings.Devices.TryGetValue(macKey, out var device))
        {
            device = new DevicePreferences();
            settings.Devices[macKey] = device;
        }

        device.DownloadKiloBytesPerSecond = normalized.DownloadKiloBytesPerSecond;
        device.UploadKiloBytesPerSecond = normalized.UploadKiloBytesPerSecond;
        device.PauseInternet = normalized.PauseInternet;
    }

    public static void ApplyLimitsToAllDevices(
        AppSettings settings,
        int downloadKiloBytesPerSecond,
        int uploadKiloBytesPerSecond)
    {
        ArgumentNullException.ThrowIfNull(settings);
        settings.DefaultDownloadKiloBytesPerSecond = Math.Max(0, downloadKiloBytesPerSecond);
        settings.DefaultUploadKiloBytesPerSecond = Math.Max(0, uploadKiloBytesPerSecond);
        foreach (var device in settings.Devices.Values)
        {
            device.DownloadKiloBytesPerSecond = null;
            device.UploadKiloBytesPerSecond = null;
        }
    }
}
