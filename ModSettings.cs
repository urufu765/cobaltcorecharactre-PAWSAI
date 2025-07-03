using Newtonsoft.Json;
using Nickel;
using System;

namespace Starhunters;

class Settings
{
    [JsonProperty]
    public ProfileSettings Global = new();

    [JsonIgnore]
    public ProfileBasedValue<IModSettingsApi.ProfileMode, ProfileSettings> ProfileBased;

    public Settings()
    {
        ProfileBased = ProfileBasedValue.Create(
            () => ModEntry.Instance.Helper.ModData.GetModDataOrDefault(MG.inst.g?.state ?? DB.fakeState, "ActiveProfile", IModSettingsApi.ProfileMode.Slot),
            profile => ModEntry.Instance.Helper.ModData.SetModData(MG.inst.g?.state ?? DB.fakeState, "ActiveProfile", profile),
            profile => profile switch
            {
                IModSettingsApi.ProfileMode.Global => Global,
                IModSettingsApi.ProfileMode.Slot => ModEntry.Instance.Helper.ModData.ObtainModData<ProfileSettings>(MG.inst.g?.state ?? DB.fakeState, "ProfileSettings"),
                _ => throw new ArgumentOutOfRangeException(nameof(profile), profile, null)
            },
            (profile, data) =>
            {
                switch (profile)
                {
                    case IModSettingsApi.ProfileMode.Global:
                        Global = data;
                        break;
                    case IModSettingsApi.ProfileMode.Slot:
                        ModEntry.Instance.Helper.ModData.SetModData(MG.inst.g?.state ?? DB.fakeState, "ProfileSettings", data);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(profile), profile, null);
                }
            }
        );
    }
}

class ProfileSettings
{
    /// <summary>
    /// Obsolete, now broken up into other options.
    /// </summary>
    [JsonProperty]
    public bool AccurateCalculations = false;

    /// <summary>
    /// Better calculation for retreat... it's not that impactful on performance and yet it's better in every way lol
    /// </summary>
    [JsonProperty]
    public bool Pawsai_BetterEmergencyRetreat = true;

    /// <summary>
    /// Allows additional calculations that is rather expensive...
    /// </summary>
    [JsonProperty]
    public bool Bruno_FancyHyperdrive = false;
}