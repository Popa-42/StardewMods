namespace StardewMods.BetterChests.Framework.UI.Components;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.BetterChests.Framework.Services;
using StardewMods.Common.Helpers;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.UI.Components;
using StardewValley.Menus;

/// <inheritdoc />
internal sealed class ExpressionTerm : ExpressionEditor
{
    private readonly List<ButtonComponent> components = [];
    private readonly ButtonComponent leftComponent;
    private readonly ClickableTextureComponent removeButton;
    private readonly ButtonComponent rightComponent;

    private EventHandler<ExpressionChangedEventArgs>? expressionChanged;

    /// <summary>Initializes a new instance of the <see cref="ExpressionTerm" /> class.</summary>
    /// <param name="iconRegistry">Dependency used for registering and retrieving icons.</param>
    /// <param name="parent">The parent menu.</param>
    /// <param name="x">The component x-coordinate.</param>
    /// <param name="y">The component y-coordinate.</param>
    /// <param name="width">The component width.</param>
    /// <param name="expression">The expression.</param>
    /// <param name="level">The level.</param>
    public ExpressionTerm(IIconRegistry iconRegistry, int x, int y, int width, IExpression expression, int level)
        : base(x, y, width, 40, expression, level)
    {
        var subWidth = ((width - 12) / 2) - 15;

        // Left term
        var leftTerm = expression.Expressions.ElementAtOrDefault(0);
        var text = leftTerm is not null ? Localized.Attribute(leftTerm.Term) : I18n.Attribute_Any_Name();
        this.leftComponent = new ButtonComponent(x, y, subWidth, 40, "left", text);
        this.leftComponent.Clicked += (_, _) => this.expressionChanged?.InvokeAll(
            this.leftComponent,
            new ExpressionChangedEventArgs(ExpressionChange.ChangeAttribute, this.Expression));

        this.components.Add(this.leftComponent);

        // Right term
        var rightTerm = expression.Expressions.ElementAtOrDefault(1);
        this.rightComponent = new ButtonComponent(
            x + subWidth + 12,
            y,
            subWidth,
            40,
            "right",
            rightTerm?.Term ?? expression.Term);

        this.rightComponent.Clicked += (_, _) => this.expressionChanged?.InvokeAll(
            this.rightComponent,
            new ExpressionChangedEventArgs(ExpressionChange.ChangeValue, this.Expression));

        this.components.Add(this.rightComponent);

        this.removeButton = iconRegistry
            .Icon(VanillaIcon.DoNot)
            .Component(IconStyle.Transparent, "remove", 2f)
            .AsBuilder()
            .Location(new Point(x + width - 24, y + 8))
            .HoverText(I18n.Ui_Remove_Tooltip())
            .Value;
    }

    /// <inheritdoc />
    public override event EventHandler<ExpressionChangedEventArgs>? ExpressionChanged
    {
        add => this.expressionChanged += value;
        remove => this.expressionChanged -= value;
    }

    /// <inheritdoc />
    public override bool TryLeftClick(Point cursor)
    {
        if (this.components.Any(component => component.TryLeftClick(cursor)))
        {
            return true;
        }

        if (!this.removeButton.bounds.Contains(cursor))
        {
            return false;
        }

        this.expressionChanged?.InvokeAll(
            this,
            new ExpressionChangedEventArgs(ExpressionChange.Remove, this.Expression));

        return true;
    }

    /// <inheritdoc />
    protected override void DrawInFrame(SpriteBatch spriteBatch, Point cursor)
    {
        foreach (var component in this.components)
        {
            component.Color = component.bounds.Contains(cursor - this.Offset)
                ? this.BaseColor.Highlight()
                : this.BaseColor.Muted();

            component.Draw(spriteBatch, cursor);

            if (component.Bounds.Contains(cursor - this.Offset))
            {
                this.HoverText = component.HoverText;
            }
        }

        this.removeButton.tryHover(cursor.X - this.Offset.X, cursor.Y - this.Offset.Y);
        this.removeButton.draw(spriteBatch, Color.White, 1f, 0, this.Offset.X, this.Offset.Y);

        if (this.removeButton.bounds.Contains(cursor - this.Offset))
        {
            this.HoverText = this.removeButton.hoverText;
        }
    }
}