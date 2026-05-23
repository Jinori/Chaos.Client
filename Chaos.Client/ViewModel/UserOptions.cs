namespace Chaos.Client.ViewModel;

/// <summary>
///     Unified source of truth for all 20 settings in the F4 settings panel. Server-controlled settings (indices 0-5, 7,
///     and 13) are toggled via opcode 0x1B with names/values set by the server response; the remaining slots are
///     client-local (persisted to Darkages.cfg) or reserved. Uses 0-based indexing matching the UI layout, where display
///     slot i maps to server UserOption (i+1).
/// </summary>
public sealed class UserOptions
{
    public const int SETTING_COUNT = 20;

    private static readonly bool[] ServerSettings =
    [
        true,  //0  server: Show body animations
        true,  //1  server: Listen to hit sounds
        true,  //2  server: Priority animations
        true,  //3  server: Lock hands
        true,  //4  server: Sound on whisper
        true,  //5  server: Allow exchanges
        false, //6  client-local: Use Group Window
        true,  //7  server: Hide enemy health bars
        false, //8  client-local: Scroll Screen
        false, //9  client-local: Shift key
        false, //10 client-local: Click character profile
        false, //11 client-local: NPC record mundane chat
        false, //12 client-local: Group recruiting
        true,  //13 server: Show Friendly Nametags (Option14)
        false, //14 reserved
        false, //15 reserved
        false, //16 reserved
        false, //17 reserved
        false, //18 reserved
        false  //19 reserved
    ];

    private readonly bool[] Settings = new bool[SETTING_COUNT];

    public static bool IsServerSetting(int index) => index is >= 0 and < SETTING_COUNT && ServerSettings[index];

    public bool this[int index] => index is >= 0 and < SETTING_COUNT && Settings[index];

    /// <summary>
    ///     Fires on any value change (server response or client toggle). Used by UI to refresh labels.
    /// </summary>
    public event UserOptionChangedHandler? SettingChanged;

    /// <summary>
    ///     Fires only on user-initiated toggles. Used by WorldScreen to route to server or persist locally.
    /// </summary>
    public event UserOptionChangedHandler? SettingToggled;

    /// <summary>
    ///     Sets a value and fires <see cref="SettingChanged" />. Used for server responses and initialization.
    /// </summary>
    public void SetValue(int index, bool value)
    {
        if (index is < 0 or >= SETTING_COUNT)
            return;

        Settings[index] = value;
        SettingChanged?.Invoke(index, value);
    }

    /// <summary>
    ///     Handles a user-initiated button click. For client settings, toggles the value and fires both events. For server
    ///     settings, only fires <see cref="SettingToggled" /> — the value updates when the server responds.
    /// </summary>
    public void Toggle(int index)
    {
        if (index is < 0 or >= SETTING_COUNT)
            return;

        if (IsServerSetting(index))
        {
            SettingToggled?.Invoke(index, !Settings[index]);

            return;
        }

        Settings[index] = !Settings[index];
        SettingChanged?.Invoke(index, Settings[index]);
        SettingToggled?.Invoke(index, Settings[index]);
    }

    /// <summary>
    ///     Clears only server-controlled settings, leaving client-local settings intact.
    /// </summary>
    public void ClearServerSettings()
    {
        for (var i = 0; i < SETTING_COUNT; i++)
            if (IsServerSetting(i))
                Settings[i] = false;
    }
}