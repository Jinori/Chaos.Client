#region
using Chaos.Client.Systems;
#endregion

namespace Chaos.Client.ViewModel;

/// <summary>
///     Source of truth for the F4 toggle values, driven by <see cref="SettingDefinitions" />. Server-controlled settings
///     update via the 0x1B response; client-local settings persist to Darkages.cfg; the group-recruiting toggle is
///     server-authoritative. Replaces the old fixed 20-slot magic-index model.
/// </summary>
public sealed class UserOptions
{
    private readonly Dictionary<SettingKey, bool> Values = new();

    public UserOptions()
    {
        foreach (var def in SettingDefinitions.All)
            Values[def.Key] = false;
    }

    /// <summary>Fires on any value change (server response, init, or client toggle). Used by the UI to refresh checkboxes.</summary>
    public event SettingValueChangedHandler? ValueChanged;

    /// <summary>Fires only on user-initiated toggles. Used by WorldScreen to route server-controlled toggles to the network.</summary>
    public event SettingValueChangedHandler? UserToggled;

    public bool Value(SettingKey key) => Values.TryGetValue(key, out var v) && v;

    /// <summary>Sets a value from an external source (server response or init) and raises <see cref="ValueChanged" />.</summary>
    public void Apply(SettingKey key, bool value)
    {
        Values[key] = value;
        ValueChanged?.Invoke(key, value);
    }

    /// <summary>
    ///     Handles a user click. Client-local settings flip + persist immediately; server-controlled settings only raise
    ///     <see cref="UserToggled" /> — their value updates when the server responds.
    /// </summary>
    public void Toggle(SettingKey key)
    {
        var def = SettingDefinitions.ByKey(key);

        if (def.Category == SettingCategory.ClientLocal)
        {
            var newValue = !Value(key);
            Values[key] = newValue;
            def.Set?.Invoke(newValue);
            ClientSettings.Save();
            ValueChanged?.Invoke(key, newValue);

            return;
        }

        //server-controlled / server-authoritative: don't flip locally; server response will Apply the new value
        UserToggled?.Invoke(key, !Value(key));
    }

    /// <summary>Seeds client-local and server-authoritative-local values from <see cref="ClientSettings" /> (call after ClientSettings.Load). The only caller of each definition's Get hook.</summary>
    public void SeedLocalDefaults()
    {
        foreach (var def in SettingDefinitions.All)
            if (def.Get is not null)
                Apply(def.Key, def.Get());
    }

    /// <summary>Clears only server-controlled settings, leaving client-local values intact.</summary>
    public void ClearServerSettings()
    {
        foreach (var def in SettingDefinitions.All)
            if (def.Category == SettingCategory.ServerOption)
                Values[def.Key] = false;
    }
}
