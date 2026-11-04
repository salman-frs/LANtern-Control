using Lantern.Core.Control;
using Lantern.Core.Settings;

namespace Lantern.Linux.ViewModels;

public sealed class LinuxDashboardState
{
    private readonly AppSettings settings;
    private readonly TrafficPolicy policy;

    public LinuxDashboardState(AppSettings settings, TrafficPolicy policy)
    {
        this.settings = settings;
        this.policy = policy;
    }

    public void ApplyPreset(string macAddress, DomainBlockPreset preset)
    {
        ArgumentNullException.ThrowIfNull(preset);
        var macKey = TrafficPolicy.NormalizeMac(macAddress);
        if (!settings.BlockedDomains.TryGetValue(macKey, out var domains))
        {
            domains = [];
            settings.BlockedDomains[macKey] = domains;
        }

        foreach (var domain in preset.Domains)
        {
            var normalized = TrafficPolicy.NormalizeDomain(domain);
            if (!domains.Contains(normalized, StringComparer.OrdinalIgnoreCase))
            {
                domains.Add(normalized);
            }
        }

        domains.Sort(StringComparer.OrdinalIgnoreCase);
        if (DomainBlockPresetCatalog.All.Any(candidate =>
                candidate.Name.Equals(preset.Name, StringComparison.OrdinalIgnoreCase)))
        {
            if (!settings.AppliedDomainPresets.TryGetValue(macKey, out var appliedPresets))
            {
                appliedPresets = [];
                settings.AppliedDomainPresets[macKey] = appliedPresets;
            }

            if (!appliedPresets.Contains(preset.Name, StringComparer.OrdinalIgnoreCase))
            {
                appliedPresets.Add(preset.Name);
            }
        }

        policy.SetBlockedDomains(macKey, domains);
    }

    public void ApplyTrafficRule(string macAddress, TrafficRule rule)
    {
        DeviceTrafficRules.Save(settings, macAddress, rule);
        policy.SetRule(macAddress, rule.Normalize());
    }

    public void ApplyLimitsToAllDevices(int downloadKiloBytesPerSecond, int uploadKiloBytesPerSecond)
    {
        DeviceTrafficRules.ApplyLimitsToAllDevices(
            settings,
            downloadKiloBytesPerSecond,
            uploadKiloBytesPerSecond);
        ApplySavedTrafficRules();
    }

    public void ApplySavedTrafficRules()
    {
        policy.SetDefaultLimits(
            settings.DefaultDownloadKiloBytesPerSecond,
            settings.DefaultUploadKiloBytesPerSecond);
        foreach (var macKey in settings.Devices.Keys)
        {
            policy.SetRule(macKey, DeviceTrafficRules.Resolve(settings, macKey));
        }
    }

    public void RemoveDomain(string macAddress, string domain)
    {
        var macKey = TrafficPolicy.NormalizeMac(macAddress);
        var normalizedDomain = TrafficPolicy.NormalizeDomain(domain);
        if (!settings.BlockedDomains.TryGetValue(macKey, out var domains))
        {
            return;
        }

        domains.RemoveAll(candidate =>
            string.Equals(candidate, normalizedDomain, StringComparison.OrdinalIgnoreCase));

        policy.SetBlockedDomains(macKey, domains);
    }
}
