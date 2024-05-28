namespace StardewMods.BetterChests.Framework.UI.Components;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.BetterChests.Framework.Models;
using StardewMods.Common.Helpers;
using StardewMods.Common.Models.Events;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.UI.Components;
using StardewValley.Menus;

/// <summary>A component for configuring a tab.</summary>
internal sealed class TabEditor : BaseComponent
{
    private readonly ClickableTextureComponent downArrow;
    private readonly ClickableTextureComponent icon;
    private readonly ClickableTextureComponent upArrow;

    private EventHandler<UiEventArgs>? moveDown;
    private EventHandler<UiEventArgs>? moveUp;

    /// <summary>Initializes a new instance of the <see cref="TabEditor" /> class.</summary>
    /// <param name="iconRegistry">Dependency used for registering and retrieving icons.</param>
    /// <param name="parent">The parent menu.</param>
    /// <param name="x">The x-coordinate of the tab component.</param>
    /// <param name="y">The y-coordinate of the tab component.</param>
    /// <param name="width">The width of the tab component.</param>
    /// <param name="icon">The tab icon.</param>
    /// <param name="tabData">The inventory tab data.</param>
    public TabEditor(IIconRegistry iconRegistry, int x, int y, int width, IIcon icon, TabData tabData)
        : base(x, y, width, Game1.tileSize, tabData.Label)
    {
        this.Data = tabData;
        this.icon = icon
            .Component(IconStyle.Transparent)
            .AsBuilder()
            .Location(new Point(this.bounds.X + Game1.tileSize, y))
            .Size(new Point(Game1.tileSize, this.bounds.Width - (Game1.tileSize * 2)))
            .Value;

        this.upArrow = iconRegistry
            .Icon(VanillaIcon.ArrowUp)
            .Component(IconStyle.Transparent)
            .AsBuilder()
            .Location(new Point(x + 8, y + 8))
            .HoverText(I18n.Ui_MoveUp_Tooltip())
            .Value;

        this.downArrow = iconRegistry
            .Icon(VanillaIcon.ArrowDown)
            .Component(IconStyle.Transparent)
            .AsBuilder()
            .Location(new Point(x + width - Game1.tileSize + 8, y + 8))
            .HoverText(I18n.Ui_MoveDown_Tooltip())
            .Value;
    }

    /// <summary>Event triggered when the move up button is clicked.</summary>
    public event EventHandler<UiEventArgs> MoveDown
    {
        add => this.moveDown += value;
        remove => this.moveDown -= value;
    }

    /// <summary>Event triggered when the move down button is clicked.</summary>
    public event EventHandler<UiEventArgs> MoveUp
    {
        add => this.moveUp += value;
        remove => this.moveUp -= value;
    }

    /// <summary>Gets the tab data.</summary>
    public TabData Data { get; }

    /// <summary>Gets or sets a value indicating whether the tab is currently active.</summary>
    public bool Active { get; set; } = true;

    /// <summary>Gets or sets the index.</summary>
    public int Index { get; set; }

    /// <inheritdoc />
    public override Point Location
    {
        get => base.Location;
        set
        {
            base.Location = value;
            this.icon.bounds.Location = new Point(value.X + Game1.tileSize, value.Y);
            this.upArrow.bounds.Location = new Point(value.X + 8, value.Y + 8);
            this.downArrow.bounds.Location = new Point(value.X + this.bounds.Width - Game1.tileSize + 8, value.Y + 8);
        }
    }

    /// <inheritdoc />
    public override void Draw(SpriteBatch spriteBatch, Point cursor)
    {
        cursor -= this.Offset;
        var hover = this.bounds.Contains(cursor);
        var color = this.Active
            ? Color.White
            : hover
                ? Color.LightGray
                : Color.Gray;

        // Top-Center
        spriteBatch.Draw(
            Game1.mouseCursors,
            new Rectangle(
                this.icon.bounds.X + this.Offset.X + 20,
                this.icon.bounds.Y + this.Offset.Y,
                this.icon.bounds.Width - 40,
                this.icon.bounds.Height),
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
                this.icon.bounds.X + this.Offset.X + 20,
                this.icon.bounds.Y + this.icon.bounds.Height + this.Offset.Y - 20,
                this.icon.bounds.Width - 40,
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
            new Vector2(this.icon.bounds.X + this.Offset.X, this.icon.bounds.Y + this.Offset.Y),
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
            new Vector2(
                this.icon.bounds.X + this.Offset.X,
                this.icon.bounds.Y + this.icon.bounds.Height + this.Offset.Y - 20),
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
            new Vector2(this.icon.bounds.Right + this.Offset.X - 20, this.icon.bounds.Y + this.Offset.Y),
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
            new Vector2(
                this.icon.bounds.Right + this.Offset.X - 20,
                this.icon.bounds.Y + this.icon.bounds.Height + this.Offset.Y - 20),
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

        spriteBatch.DrawString(
            Game1.smallFont,
            this.name,
            new Vector2(
                this.icon.bounds.X + Game1.tileSize + this.Offset.X,
                this.icon.bounds.Y + (IClickableMenu.borderWidth / 2f) + this.Offset.Y),
            this.Active ? Game1.textColor : Game1.unselectedOptionColor);

        if (!this.Active)
        {
            return;
        }

        this.upArrow.tryHover(cursor.X, cursor.Y);
        this.downArrow.tryHover(cursor.X, cursor.Y);

        this.upArrow.draw(spriteBatch, color, 1f);
        this.downArrow.draw(spriteBatch, color, 1f);

        if (this.upArrow.bounds.Contains(cursor))
        {
            this.HoverText = this.upArrow.hoverText;
            return;
        }

        if (this.downArrow.bounds.Contains(cursor))
        {
            this.HoverText = this.downArrow.hoverText;
            return;
        }

        this.HoverText = null;
    }

    /// <inheritdoc />
    public override bool TryLeftClick(Point cursor)
    {
        if (this.Active && this.downArrow.bounds.Contains(cursor))
        {
            this.moveDown.InvokeAll(this, new UiEventArgs(SButton.MouseLeft, cursor));
            return true;
        }

        if (this.Active && this.upArrow.bounds.Contains(cursor))
        {
            this.moveUp.InvokeAll(this, new UiEventArgs(SButton.MouseLeft, cursor));
            return true;
        }

        return base.TryLeftClick(cursor);
    }

    /// <inheritdoc />
    public override bool TryRightClick(Point cursor)
    {
        if (this.Active && this.downArrow.bounds.Contains(cursor))
        {
            this.moveDown.InvokeAll(this, new UiEventArgs(SButton.MouseRight, cursor));
            return true;
        }

        if (this.Active && this.upArrow.bounds.Contains(cursor))
        {
            this.moveUp.InvokeAll(this, new UiEventArgs(SButton.MouseRight, cursor));
            return true;
        }

        return base.TryLeftClick(cursor);
    }
}