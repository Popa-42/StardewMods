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
    private readonly TextureComponent downArrow;
    private readonly TextureComponent tabIcon;
    private readonly TextureComponent upArrow;

    private bool active;
    private EventHandler<UiEventArgs>? moveDown;
    private EventHandler<UiEventArgs>? moveUp;

    /// <summary>Initializes a new instance of the <see cref="TabEditor" /> class.</summary>
    /// <param name="iconRegistry">Dependency used for registering and retrieving icons.</param>
    /// <param name="x">The x-coordinate of the tab component.</param>
    /// <param name="y">The y-coordinate of the tab component.</param>
    /// <param name="width">The width of the tab component.</param>
    /// <param name="icon">The tab icon.</param>
    /// <param name="tabData">The inventory tab data.</param>
    public TabEditor(IIconRegistry iconRegistry, int x, int y, int width, IIcon icon, TabData tabData)
        : base(x, y, width, Game1.tileSize, tabData.Label)
    {
        // Initialize
        this.Data = tabData;
        this.tabIcon = icon.Component(IconStyle.Transparent).AsBuilder().Value;

        this.upArrow = iconRegistry
            .Icon(VanillaIcon.ArrowUp)
            .Component(IconStyle.Transparent)
            .AsBuilder()
            .HoverText(I18n.Ui_MoveUp_Tooltip())
            .Value;

        this.downArrow = iconRegistry
            .Icon(VanillaIcon.ArrowDown)
            .Component(IconStyle.Transparent)
            .AsBuilder()
            .HoverText(I18n.Ui_MoveDown_Tooltip())
            .Value;

        // Events
        this.tabIcon.Rendering += this.UpdateColor;
        this.upArrow.Clicked += (_, e) => this.moveUp?.InvokeAll(this, new UiEventArgs(e.Button, e.Cursor));
        this.upArrow.Rendering += this.UpdateColor;
        this.downArrow.Clicked += (_, e) => this.moveDown?.InvokeAll(this, new UiEventArgs(e.Button, e.Cursor));
        this.downArrow.Rendering += this.UpdateColor;

        // Add components
        this.Components.Add(this.tabIcon);
        this.Components.Add(this.upArrow);
        this.Components.Add(this.downArrow);
        this.RepositionComponents(this.Location);
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
    public bool Active
    {
        get => this.active;
        set
        {
            this.active = value;
            this.upArrow.IsVisible = value;
            this.downArrow.IsVisible = value;
        }
    }

    /// <summary>Gets or sets the index.</summary>
    public int Index { get; set; }

    /// <inheritdoc />
    public override void Draw(SpriteBatch spriteBatch, Point cursor)
    {
        base.Draw(spriteBatch, cursor);
        spriteBatch.DrawString(
            Game1.smallFont,
            this.name,
            new Vector2(
                this.tabIcon.bounds.X + Game1.tileSize + this.Offset.X,
                this.tabIcon.bounds.Y + (IClickableMenu.borderWidth / 2f) + this.Offset.Y),
            this.Active ? Game1.textColor : Game1.unselectedOptionColor);
    }

    /// <inheritdoc />
    public override void DrawUnder(SpriteBatch spriteBatch, Point cursor)
    {
        var color = this.Active
            ? Color.White
            : this.bounds.Contains(cursor - this.Offset)
                ? Color.LightGray
                : Color.Gray;

        // Top-Center
        spriteBatch.Draw(
            Game1.mouseCursors,
            new Rectangle(
                this.Bounds.X + this.Offset.X + Game1.tileSize + 20,
                this.Bounds.Y + this.Offset.Y,
                this.Bounds.Width - (Game1.tileSize * 2) - 40,
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
                this.Bounds.X + this.Offset.X + Game1.tileSize + 20,
                this.Bounds.Y + this.Bounds.Height + this.Offset.Y - 20,
                this.Bounds.Width - (Game1.tileSize * 2) - 40,
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
            new Vector2(this.Bounds.X + this.Offset.X + Game1.tileSize, this.Bounds.Y + this.Offset.Y),
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
                this.Bounds.X + this.Offset.X + Game1.tileSize,
                this.Bounds.Y + this.Bounds.Height + this.Offset.Y - 20),
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
            new Vector2(this.Bounds.Right + this.Offset.X - Game1.tileSize - 20, this.Bounds.Y + this.Offset.Y),
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
                this.Bounds.Right + this.Offset.X - Game1.tileSize - 20,
                this.Bounds.Y + this.Bounds.Height + this.Offset.Y - 20),
            new Rectangle(16, 368, 5, 5),
            color,
            0,
            Vector2.Zero,
            Game1.pixelZoom,
            SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically,
            0.5f);
    }

    /// <inheritdoc />
    protected override void RepositionComponents(Point newLocation)
    {
        this.tabIcon.bounds.Location = new Point(newLocation.X + Game1.tileSize, newLocation.Y);
        this.upArrow.bounds.Location = new Point(newLocation.X + 8, newLocation.Y + 8);
        this.downArrow.bounds.Location = new Point(
            newLocation.X + this.bounds.Width - Game1.tileSize + 8,
            newLocation.Y + 8);
    }

    private void UpdateColor(object? sender, RenderEventArgs e)
    {
        if (sender is not TextureComponent textureComponent)
        {
            return;
        }

        textureComponent.Color = this.Active
            ? Color.White
            : this.Bounds.Contains(e.Cursor)
                ? Color.LightGray
                : Color.Gray;
    }
}