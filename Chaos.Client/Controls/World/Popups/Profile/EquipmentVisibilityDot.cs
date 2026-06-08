#region
using Chaos.Client.Controls.Components;
using Chaos.DarkAges.Definitions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Chaos.Client.Controls.World.Popups.Profile;

/// <summary>
///     A small clickable visibility indicator drawn in the bottom-right corner of an equipment slot on the self profile's
///     Equipment tab. A filled dot with a 1px black halo: a warm (Peach) dot means the equipped piece is shown in-world, a
///     dark-grey dot means it is hidden. Clicking the dot optimistically flips its state and raises <see cref="Toggled" /> so the screen can send
///     the server the corresponding <see cref="UserOption" /> Set. The two dot textures are built once and shared across all
///     instances.
/// </summary>
public sealed class EquipmentVisibilityDot : UIElement
{
    /// <summary>The drawn dot's edge length in pixels.</summary>
    private const int DOT_SIZE = 8;

    private static Texture2D? SharedShown;
    private static Texture2D? SharedHidden;

    /// <summary>The equipment slot this dot governs.</summary>
    public EquipmentSlot Slot { get; }

    /// <summary>True when the slot's gear is hidden in-world (dark dot); false when shown (warm dot).</summary>
    public bool Hidden { get; set; }

    public EquipmentVisibilityDot(EquipmentSlot slot)
    {
        Slot = slot;
        Name = $"VisibilityDot_{slot}";
        EnsureTextures();

        //padded ~10x10 hit-area so the small dot is comfortably clickable; the dot itself draws
        //centered within these bounds. higher zindex than the slot image so the dispatcher routes
        //the click here first.
        Width = 10;
        Height = 10;
        ZIndex = 5;
    }

    /// <summary>Raised after the user clicks the dot. Carries the slot and the new hidden state.</summary>
    public event Action<EquipmentSlot, bool>? Toggled;

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!Visible)
            return;

        //updates ClipRect for hit-testing (ContainsPoint) and clipped drawing
        base.Draw(spriteBatch);

        var texture = Hidden ? SharedHidden : SharedShown;

        if (texture is null)
            return;

        //center the dot within the padded hit-area
        var drawX = ScreenX + ((Width - DOT_SIZE) / 2);
        var drawY = ScreenY + ((Height - DOT_SIZE) / 2);

        DrawTexture(
            spriteBatch,
            texture,
            new Vector2(drawX, drawY),
            Color.White);
    }

    public override void OnClick(ClickEvent e)
    {
        if (e.Button != MouseButton.Left)
            return;

        //optimistic flip — the server echo (SelfProfile re-open) will re-confirm
        Hidden = !Hidden;
        e.Handled = true;

        Toggled?.Invoke(Slot, Hidden);
    }

    private static void EnsureTextures()
    {
        if (SharedShown is { IsDisposed: false } && SharedHidden is { IsDisposed: false })
            return;

        SharedShown?.Dispose();
        SharedHidden?.Dispose();

        //nearest legend-palette colors to the approved dot: Peach (warm) = shown, DarkGray = hidden.
        //read here at build time (after LegendColors.Initialize at startup), not in a static field.
        SharedShown = BuildDot(LegendColors.Peach);
        SharedHidden = BuildDot(LegendColors.DarkGray);
    }

    /// <summary>
    ///     Builds a DOT_SIZE square: a filled rounded dot in the given fill color, ringed by a 1px black halo, with a single
    ///     bright highlight pixel for a slight gem-like read. Corners are left transparent so it reads as a dot, not a box.
    /// </summary>
    private static Texture2D BuildDot(Color fill)
    {
        var pixels = new Color[DOT_SIZE * DOT_SIZE];
        var halo = new Color(0, 0, 0, 255);
        var highlight = new Color(255, 255, 255, 220);

        //a circular mask centered on the texture. radius chosen so the body reads as a ~4-5px micro-dot
        //with a 1px black halo around it, leaving the outer ring of the 8px cell transparent.
        const float CENTER = (DOT_SIZE - 1) / 2f;
        const float BODY_RADIUS = 2.1f;
        const float HALO_RADIUS = 3.1f;

        for (var y = 0; y < DOT_SIZE; y++)
            for (var x = 0; x < DOT_SIZE; x++)
            {
                var dx = x - CENTER;
                var dy = y - CENTER;
                var dist = MathF.Sqrt((dx * dx) + (dy * dy));

                Color c;

                if (dist <= BODY_RADIUS)
                    c = fill;
                else if (dist <= HALO_RADIUS)
                    c = halo;
                else
                    c = Color.Transparent;

                pixels[(y * DOT_SIZE) + x] = c;
            }

        //single highlight pixel toward the upper portion of the (now smaller) body
        const int HX = 3;
        const int HY = 2;
        pixels[(HY * DOT_SIZE) + HX] = highlight;

        var texture = new Texture2D(ChaosGame.Device, DOT_SIZE, DOT_SIZE);
        texture.SetData(pixels);

        return texture;
    }
}
