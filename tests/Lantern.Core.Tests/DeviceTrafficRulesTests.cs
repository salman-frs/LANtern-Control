using Lantern.Core.Control;
using Lantern.Core.Settings;

namespace Lantern.Core.Tests;

public sealed class DeviceTrafficRulesTests
{
    [Fact]
    public void ApplyLimitsToAllDevices_ReplacesEveryDeviceLimitAndKeepsPause()
    {
        var settings = new AppSettings
        {
            Devices =
            {
                ["E261190DBD54"] = new DevicePreferences
                {
                    DownloadKiloBytesPerSecond = 100,
                    UploadKiloBytesPerSecond = 50,
                    PauseInternet = true,
                },
            },
        };

        DeviceTrafficRules.ApplyLimitsToAllDevices(settings, 2_500, 625);

        Assert.Equal(
            new TrafficRule(true, 2_500, 625),
            DeviceTrafficRules.Resolve(settings, "E261190DBD54"));
    }

    [Fact]
    public void Resolve_GivesNewDevicesTheLimitAppliedToAllDevices()
    {
        var settings = new AppSettings();

        DeviceTrafficRules.ApplyLimitsToAllDevices(settings, 2_500, 625);

        Assert.Equal(
            new TrafficRule(false, 2_500, 625),
            DeviceTrafficRules.Resolve(settings, "0E:4F:69:CC:E4:F0"));
    }

    [Fact]
    public void Save_GivesOneDeviceItsOwnLimitAfterApplyingToAll()
    {
        var settings = new AppSettings();
        DeviceTrafficRules.ApplyLimitsToAllDevices(settings, 2_500, 625);

        DeviceTrafficRules.Save(settings, "E261190DBD54", new TrafficRule(false, 0, 625));

        Assert.Equal(
            new TrafficRule(false, 0, 625),
            DeviceTrafficRules.Resolve(settings, "E261190DBD54"));
        Assert.Equal(
            new TrafficRule(false, 2_500, 625),
            DeviceTrafficRules.Resolve(settings, "0E4F69CCE4F0"));
    }
}
