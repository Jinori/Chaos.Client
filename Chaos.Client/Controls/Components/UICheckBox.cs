#region
using Chaos.Client.Rendering;
using Chaos.Client.Utilities;
using Microsoft.Xna.Framework.Graphics;
using SkiaSharp;
#endregion

namespace Chaos.Client.Controls.Components;

/// <summary>
///     A checkbox fabricated from the dlgframe.epf line border (the same border the item tooltip uses) over a dark fill,
///     with an X drawn in the center when checked. The two state textures are built once and shared across all instances.
/// </summary>
public sealed class UICheckBox : UIButton
{
    /// <summary>Box edge length in pixels. Sized to sit comfortably inside a ~21px settings row.</summary>
    public const int CHECKBOX_SIZE = 18;

    private static Texture2D? SharedUnchecked;
    private static Texture2D? SharedChecked;

    public UICheckBox()
    {
        EnsureTextures();
        NormalTexture = SharedUnchecked;
        SelectedTexture = SharedChecked;
        Width = CHECKBOX_SIZE;
        Height = CHECKBOX_SIZE;
        CenterTexture = true;
    }

    /// <summary>Checked state. Backed by the inherited <see cref="UIButton.IsSelected" /> so the selected texture shows.</summary>
    public bool Checked
    {
        get => IsSelected;
        set => IsSelected = value;
    }

    private static void EnsureTextures()
    {
        if (SharedUnchecked is { IsDisposed: false } && SharedChecked is { IsDisposed: false })
            return;

        SharedUnchecked?.Dispose();
        SharedChecked?.Dispose();
        SharedUnchecked = BuildBox(false);
        SharedChecked = BuildBox(true);
    }

    private static Texture2D BuildBox(bool withX)
    {
        const int SIZE = CHECKBOX_SIZE;

        //dlgframe border over a near-black fill — same utility the tooltip uses
        using var frame = DialogFrame.Composite(
            new SKColor(
                10,
                8,
                5,
                255),
            SIZE,
            SIZE);

        var info = new SKImageInfo(
            SIZE,
            SIZE,
            SKColorType.Rgba8888,
            SKAlphaType.Premul);
        using var surface = SKSurface.Create(info);
        var canvas = surface.Canvas;

        if (frame is not null)
            canvas.DrawImage(frame, 0, 0);
        else
            canvas.Clear(
                new SKColor(
                    10,
                    8,
                    5,
                    255));

        if (withX)
        {
            using var paint = new SKPaint
            {
                Color = new SKColor(
                    233,
                    223,
                    194),
                StrokeWidth = 2,
                IsAntialias = false,
                Style = SKPaintStyle.Stroke
            };

            const int PAD = 4;
            canvas.DrawLine(PAD, PAD, SIZE - PAD, SIZE - PAD, paint);
            canvas.DrawLine(SIZE - PAD, PAD, PAD, SIZE - PAD, paint);
        }

        using var snapshot = surface.Snapshot();

        return TextureConverter.ToTexture2D(snapshot);
    }

    public override void Dispose()
    {
        //textures are shared statics — detach before base.Dispose() so it can't dispose them
        NormalTexture = null;
        SelectedTexture = null;
        PressedTexture = null;
        HoverTexture = null;
        DisabledTexture = null;

        base.Dispose();
    }
}
