namespace StardewMods.BetterChests.Framework.UI.Components;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.BetterChests.Framework.Models;
using StardewMods.Common.Helpers;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.UI.Components;
using StardewValley.Menus;

/// <summary>A component with an icon that expands into a label when hovered.</summary>
internal sealed class InventoryTab : BaseComponent
{
    private readonly TextureComponent icon;
    private readonly Vector2 origin;
    private readonly int overrideWidth;
    private readonly int textWidth;

    /// <summary>Initializes a new instance of the <see cref="InventoryTab" /> class.</summary>
    /// <param name="x">The x-coordinate of the tab component.</param>
    /// <param name="y">The y-coordinate of the tab component.</param>
    /// <param name="icon">The tab icon.</param>
    /// <param name="tabData">The inventory tab data.</param>
    /// <param name="overrideWidth">Indicates if the component should have a default width.</param>
    public InventoryTab(int x, int y, IIcon icon, TabData tabData, int overrideWidth = -1)
        : base(x, y, Game1.tileSize, Game1.tileSize, tabData.Label)
    {
        var textBounds = Game1.smallFont.MeasureString(tabData.Label).ToPoint();
        this.Data = tabData;
        this.overrideWidth = overrideWidth;
        this.origin = new Vector2(x, y);
        this.icon = icon.Component(IconStyle.Transparent, scale: 3f).AsBuilder().Value;
        this.icon.Location = new Point(
            this.Bounds.Center.X - (this.icon.Size.X / 2) + 32,
            this.Bounds.Center.Y - (this.icon.Size.Y / 2));

        this.textWidth = textBounds.X;

        if (overrideWidth == -1)
        {
            return;
        }

        this.bounds.Width = overrideWidth;
        this.bounds.X = (int)this.origin.X - overrideWidth;
    }

    /// <summary>Gets the tab data.</summary>
    public TabData Data { get; }

    /// <summary>Gets or sets a value indicating whether the tab is currently active.</summary>
    public bool Active { get; set; } = true;

    /// <inheritdoc />
    public override void Draw(SpriteBatch spriteBatch, Point cursor)
    {
        cursor -= this.Offset;
        var hover = this.Bounds.Contains(cursor);
        var color = this.Active
            ? Color.White
            : hover
                ? Color.LightGray
                : Color.Gray;

        // Top-Center
        spriteBatch.Draw(
            Game1.mouseCursors,
            new Rectangle(
                this.Bounds.X + this.Offset.X + 12,
                this.Bounds.Y + this.Offset.Y,
                this.Bounds.Width - 32,
                this.Bounds.Height),
            new Rectangle(21, 368, 6, 16),
            color,
            0,
            Vector2.Zero,
            SpriteEffects.None,
            0.5f);

        // Bottom-Center
        spriteBatch.Draw(
            Game1.mouseCursors,
            new Rectangle(
                this.Bounds.X + this.Offset.X + 12,
                this.Bounds.Bottom + this.Offset.Y - 20,
                this.Bounds.Width - 32,
                20),
            new Rectangle(21, 368, 6, 5),
            color,
            0,
            Vector2.Zero,
            SpriteEffects.FlipVertically,
            0.5f);

        // Top-Left
        spriteBatch.Draw(
            Game1.mouseCursors,
            new Vector2(this.Bounds.X + this.Offset.X, this.Bounds.Y + this.Offset.Y),
            new Rectangle(16, 368, 5, 15),
            color,
            0,
            Vector2.Zero,
            Game1.pixelZoom,
            SpriteEffects.None,
            0.5f);

        // Bottom-Left
        spriteBatch.Draw(
            Game1.mouseCursors,
            new Vector2(this.Bounds.X + this.Offset.X, this.Bounds.Bottom + this.Offset.Y - 20),
            new Rectangle(16, 368, 5, 5),
            color,
            0,
            Vector2.Zero,
            Game1.pixelZoom,
            SpriteEffects.FlipVertically,
            0.5f);

        // Top-Right
        spriteBatch.Draw(
            Game1.mouseCursors,
            new Vector2(this.Bounds.Right + this.Offset.X - 20, this.Bounds.Y + this.Offset.Y),
            new Rectangle(16, 368, 5, 15),
            color,
            0,
            Vector2.Zero,
            Game1.pixelZoom,
            SpriteEffects.FlipHorizontally,
            0.5f);

        // Bottom-Right
        spriteBatch.Draw(
            Game1.mouseCursors,
            new Vector2(this.Bounds.Right + this.Offset.X - 20, this.Bounds.Bottom + this.Offset.Y - 20),
            new Rectangle(16, 368, 5, 5),
            color,
            0,
            Vector2.Zero,
            Game1.pixelZoom,
            SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically,
            0.5f);

        if (this.Active && hover)
        {
            this.icon.tryHover(cursor.X, cursor.Y);
        }

        this.icon.draw(spriteBatch, color, 1f, 0, this.Offset.X, this.Offset.Y);

        if (this.bounds.Width != this.overrideWidth
            && this.bounds.Width != this.textWidth + Game1.tileSize + IClickableMenu.borderWidth)
        {
            return;
        }

        spriteBatch.DrawString(
            Game1.smallFont,
            this.name,
            new Vector2(
                this.bounds.X + Game1.tileSize + this.Offset.X,
                this.bounds.Y + (IClickableMenu.borderWidth / 2f) + this.Offset.Y),
            this.Active ? Game1.textColor : Game1.unselectedOptionColor);
    }

    /// <inheritdoc />
    public override void Update(Point cursor)
    {
        if (this.overrideWidth != -1)
        {
            return;
        }

        this.bounds.Width = this.bounds.Contains(cursor)
            ? Math.Min(this.bounds.Width + 16, this.textWidth + Game1.tileSize + IClickableMenu.borderWidth)
            : Math.Max(this.bounds.Width - 16, Game1.tileSize);

        this.bounds.X = (int)this.origin.X - this.bounds.Width;
        this.icon.bounds.X = this.Bounds.X - (this.icon.Size.X / 2) + 32;
    }
}