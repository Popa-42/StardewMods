namespace StardewMods.BetterChests.Framework.UI.Components;

using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.UI.Components;
using StardewValley.Menus;

/// <summary>A component which represents an <see cref="IExpression" />.</summary>
internal abstract class ExpressionComponent : BaseComponent
{
    private static readonly Color[] Colors =
    [
        Color.Red, Color.Yellow, Color.Green, Color.Cyan, Color.Blue, Color.Violet, Color.Pink,
    ];

    private readonly TextureComponent warningIcon;

    /// <summary>Initializes a new instance of the <see cref="ExpressionComponent" /> class.</summary>
    /// <param name="x">The component x-coordinate.</param>
    /// <param name="y">The component y-coordinate.</param>
    /// <param name="width">The component width.</param>
    /// <param name="height">The component height.</param>
    /// <param name="expression">The expression.</param>
    /// <param name="level">The level.</param>
    protected ExpressionComponent(int x, int y, int width, int height, IExpression expression, int level)
        : base(x, y, width, height, level.ToString(CultureInfo.InvariantCulture))
    {
        // Initialize
        this.Expression = expression;
        this.Level = level;
        this.BaseColor = level >= 0
            ? ExpressionComponent.Colors[this.Level % ExpressionComponent.Colors.Length]
            : Color.Black;

        this.warningIcon = new TextureComponent(
            "warning",
            new Rectangle(x - 2, y - 7, 5, 14),
            Game1.mouseCursors,
            new Rectangle(403, 496, 5, 14),
            2f) { HoverText = I18n.Ui_Invalid_Tooltip() };

        this.warningIcon.Rendering += (_, _) => this.warningIcon.IsVisible = !this.Expression.IsValid;

        this.Components.Add(this.warningIcon);
    }

    /// <summary>Event raised when the expression is changed.</summary>
    public abstract event EventHandler<ExpressionChangedEventArgs> ExpressionChanged;

    /// <summary>Gets the expression.</summary>
    public IExpression Expression { get; }

    /// <summary>Gets the base color.</summary>
    protected Color BaseColor { get; }

    /// <summary>Gets the level.</summary>
    protected int Level { get; }

    /// <inheritdoc />
    public override void DrawOver(SpriteBatch spriteBatch, Point cursor)
    {
        base.DrawOver(spriteBatch, cursor);
        this.warningIcon.Draw(spriteBatch, cursor);
    }

    /// <inheritdoc />
    protected override void DrawInFrame(SpriteBatch spriteBatch, Point cursor)
    {
        if (!this.IsVisible)
        {
            return;
        }

        if (this.Level >= 0
            && this.Expression.ExpressionType is ExpressionType.All or ExpressionType.Any or ExpressionType.Not)
        {
            IClickableMenu.drawTextureBox(
                spriteBatch,
                Game1.mouseCursors,
                OptionsDropDown.dropDownBGSource,
                this.bounds.X - this.Offset.X,
                this.bounds.Y - this.Offset.Y,
                this.bounds.Width,
                this.bounds.Height,
                this.Color,
                Game1.pixelZoom,
                false,
                0.97f);
        }

        base.DrawInFrame(spriteBatch, cursor);
    }
}