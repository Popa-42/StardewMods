namespace StardewMods.BetterChests.Framework.UI.Components;

using Microsoft.Xna.Framework;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.BetterChests.Framework.Services;
using StardewMods.Common.Helpers;
using StardewMods.Common.Models.Events;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.UI;
using StardewMods.Common.UI.Components;

/// <inheritdoc />
internal sealed class ExpressionTerm : ExpressionComponent
{
    private EventHandler<ExpressionChangedEventArgs>? expressionChanged;

    /// <summary>Initializes a new instance of the <see cref="ExpressionTerm" /> class.</summary>
    /// <param name="iconRegistry">Dependency used for registering and retrieving icons.</param>
    /// <param name="x">The component x-coordinate.</param>
    /// <param name="y">The component y-coordinate.</param>
    /// <param name="width">The component width.</param>
    /// <param name="expression">The expression.</param>
    /// <param name="level">The level.</param>
    public ExpressionTerm(IIconRegistry iconRegistry, int x, int y, int width, IExpression expression, int level)
        : base(x, y, width, 40, expression, level)
    {
        // Initialize
        var subWidth = ((width - 12) / 2) - 15;

        var leftTerm = expression.Expressions.ElementAtOrDefault(0);
        var text = leftTerm is not null ? Localized.Attribute(leftTerm.Term) : I18n.Attribute_Any_Name();
        var leftComponent = new ButtonComponent(x, y, subWidth, 40, "left", text)
        {
            Color = this.BaseColor.Muted(),
        };

        var rightTerm = expression.Expressions.ElementAtOrDefault(1);
        var rightComponent = new ButtonComponent(
            x + subWidth + 12,
            y,
            subWidth,
            40,
            "right",
            rightTerm?.Term ?? expression.Term)
        {
            Color = this.BaseColor.Muted(),
        };

        var removeButton = iconRegistry
            .Icon(VanillaIcon.DoNot)
            .Component(IconStyle.Transparent, "remove", 2f)
            .AsBuilder()
            .Location(new Point(x + width - 24, y + 8))
            .HoverText(I18n.Ui_Remove_Tooltip())
            .Value;

        // Events
        leftComponent.CursorOver += this.Highlight;
        leftComponent.CursorOut += this.Unhighlight;
        leftComponent.Clicked += this.ChangeAttribute;
        rightComponent.CursorOver += this.Highlight;
        rightComponent.CursorOut += this.Unhighlight;
        rightComponent.Clicked += this.ChangeValue;
        removeButton.Clicked += this.RemoveExpression;

        // Add components
        this.Components.Add(leftComponent);
        this.Components.Add(rightComponent);
        this.Components.Add(removeButton);
    }

    /// <inheritdoc />
    public override event EventHandler<ExpressionChangedEventArgs> ExpressionChanged
    {
        add => this.expressionChanged += value;
        remove => this.expressionChanged -= value;
    }

    private void ChangeAttribute(object? sender, UiEventArgs e) =>
        this.expressionChanged?.InvokeAll(
            sender,
            new ExpressionChangedEventArgs(ExpressionChange.ChangeAttribute, this.Expression));

    private void ChangeValue(object? sender, UiEventArgs e) =>
        this.expressionChanged?.InvokeAll(
            sender,
            new ExpressionChangedEventArgs(ExpressionChange.ChangeValue, this.Expression));

    private void Highlight(object? sender, CursorEventArgs e)
    {
        if (sender is not ICustomComponent component)
        {
            return;
        }

        component.Color = this.BaseColor.Highlight();
    }

    private void RemoveExpression(object? sender, UiEventArgs e) =>
        this.expressionChanged?.InvokeAll(
            this,
            new ExpressionChangedEventArgs(ExpressionChange.Remove, this.Expression));

    private void Unhighlight(object? sender, CursorEventArgs e)
    {
        if (sender is not ICustomComponent component)
        {
            return;
        }

        component.Color = this.BaseColor.Muted();
    }
}