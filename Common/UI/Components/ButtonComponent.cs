#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.UI.Components;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;

#else
namespace StardewMods.Common.UI.Components;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;
#endif

/// <summary>Generic button component with an optional label.</summary>
internal sealed class ButtonComponent : BaseComponent
{
    /// <summary>Initializes a new instance of the <see cref="ButtonComponent" /> class.</summary>
    /// <param name="x">The component x-coordinate.</param>
    /// <param name="y">The component y-coordinate.</param>
    /// <param name="width">The component width.</param>
    /// <param name="height">The component height.</param>
    /// <param name="name">The component name.</param>
    /// <param name="label">The component label.</param>
    public ButtonComponent(int x, int y, int width, int height, string name, string label)
        : base(x, y, width, height, name)
    {
        this.label = label;
        if (width == 0 && !string.IsNullOrWhiteSpace(label))
        {
            this.Size = new Point(Game1.smallFont.MeasureString(label).ToPoint().X + 20, height);
        }
    }

    /// <summary>Gets or sets the source rectangle.</summary>
    public Rectangle SourceRect { get; set; } = new(403, 373, 9, 9);

    /// <summary>Gets or sets the text color.</summary>
    public Color TextColor { get; set; } = Game1.textColor;

    /// <inheritdoc />
    protected override void DrawInFrame(SpriteBatch spriteBatch, Point cursor)
    {
        IClickableMenu.drawTextureBox(
            spriteBatch,
            Game1.mouseCursors,
            this.SourceRect,
            this.bounds.X - this.Offset.X,
            this.bounds.Y - this.Offset.Y,
            this.bounds.Width,
            this.bounds.Height,
            this.Color,
            Game1.pixelZoom,
            false);

        if (!string.IsNullOrWhiteSpace(this.label))
        {
            spriteBatch.DrawString(
                Game1.smallFont,
                this.label,
                new Vector2(this.bounds.X - this.Offset.X + 8, this.bounds.Y - this.Offset.Y + 2),
                this.TextColor,
                0f,
                Vector2.Zero,
                1f,
                SpriteEffects.None,
                1f);
        }
    }
}