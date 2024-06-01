namespace StardewMods.BetterChests.Framework.UI.Components;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.BetterChests.Framework.Services;
using StardewMods.Common.Helpers;
using StardewMods.Common.Models.Events;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.UI;
using StardewMods.Common.UI.Components;

/// <inheritdoc />
internal sealed class ExpressionGroup : ExpressionComponent
{
    private EventHandler<ExpressionChangedEventArgs>? expressionChanged;

    /// <summary>Initializes a new instance of the <see cref="ExpressionGroup" /> class.</summary>
    /// <param name="iconRegistry">Dependency used for registering and retrieving icons.</param>
    /// <param name="x">The component x-coordinate.</param>
    /// <param name="y">The component y-coordinate.</param>
    /// <param name="width">The component width.</param>
    /// <param name="expression">The expression.</param>
    /// <param name="level">The level.</param>
    public ExpressionGroup(IIconRegistry iconRegistry, int x, int y, int width, IExpression expression, int level)
        : base(x, y, width, level >= 0 ? 52 : 0, expression, level)
    {
        var indent = this.Level >= 0 ? 12 : 0;

        if (this.Level >= 0)
        {
            var toggleButton = new ButtonComponent(
                x + 8,
                y + 8,
                0,
                32,
                "toggle",
                Localized.ExpressionName(expression.ExpressionType))
            {
                HoverText = Localized.ExpressionTooltip(expression.ExpressionType),
            };

            if (expression.ExpressionType is ExpressionType.All or ExpressionType.Any)
            {
                toggleButton.Rendering += this.UpdateColor;
                toggleButton.Clicked += (_, _) => this.expressionChanged?.InvokeAll(
                    this,
                    new ExpressionChangedEventArgs(ExpressionChange.ToggleGroup, this.Expression));
            }

            this.Components.Add(toggleButton);
        }

        switch (expression.ExpressionType)
        {
            case ExpressionType.All or ExpressionType.Any:
                foreach (var subExpression in expression.Expressions)
                {
                    AddSubExpression(subExpression);
                }

                break;

            case ExpressionType.Not:
                var innerExpression = expression.Expressions.ElementAtOrDefault(0);
                if (innerExpression is null)
                {
                    break;
                }

                AddSubExpression(innerExpression);
                return;
        }

        var subWidth = (int)Game1.smallFont.MeasureString(I18n.Ui_AddTerm_Name()).X + 20;
        var addTerm = new ButtonComponent(
            this.bounds.X + indent,
            this.bounds.Bottom,
            subWidth,
            32,
            "addTerm",
            I18n.Ui_AddTerm_Name())
        {
            HoverText = I18n.Ui_AddTerm_Tooltip(),
        };

        addTerm.Rendering += this.UpdateColor;
        addTerm.Clicked += (_, _) => this.expressionChanged?.InvokeAll(
            this,
            new ExpressionChangedEventArgs(ExpressionChange.AddTerm, this.Expression));

        this.Components.Add(addTerm);

        subWidth = (int)Game1.smallFont.MeasureString(I18n.Ui_AddGroup_Name()).X + 20;
        var addGroup = new ButtonComponent(
            addTerm.Bounds.Right + 12,
            this.bounds.Bottom,
            subWidth,
            32,
            "addGroup",
            I18n.Ui_AddGroup_Name())
        {
            HoverText = I18n.Ui_AddGroup_Tooltip(),
        };

        addGroup.Rendering += this.UpdateColor;
        addGroup.Clicked += (_, _) => this.expressionChanged?.InvokeAll(
            this,
            new ExpressionChangedEventArgs(ExpressionChange.AddGroup, this.Expression));

        this.Components.Add(addGroup);

        if (expression.ExpressionType is not ExpressionType.Not)
        {
            subWidth = (int)Game1.smallFont.MeasureString(I18n.Ui_AddNot_Name()).X + 20;
            var addNot = new ButtonComponent(
                addGroup.Bounds.Right + 12,
                this.bounds.Bottom,
                subWidth,
                32,
                "addNot",
                I18n.Ui_AddNot_Name())
            {
                HoverText = I18n.Ui_AddNot_Tooltip(),
            };

            addNot.Rendering += this.UpdateColor;
            addNot.Clicked += (_, _) => this.expressionChanged?.InvokeAll(
                this,
                new ExpressionChangedEventArgs(ExpressionChange.AddNot, this.Expression));

            this.Components.Add(addNot);
        }

        this.bounds.Height = addTerm.Bounds.Bottom - this.bounds.Top + 12;

        if (level >= 0)
        {
            var removeButton = iconRegistry
                .Icon(VanillaIcon.DoNot)
                .Component(IconStyle.Transparent, "remove", 2f)
                .AsBuilder()
                .Location(new Point(x + width - 36, y + 12))
                .HoverText(I18n.Ui_Remove_Tooltip())
                .Value;

            removeButton.Clicked += this.RemoveExpression;

            this.Components.Add(removeButton);
        }

        return;

        void AddSubExpression(IExpression subExpression)
        {
            ExpressionComponent component;
            switch (subExpression.ExpressionType)
            {
                case ExpressionType.All or ExpressionType.Any or ExpressionType.Not:
                    var expressionGroup = new ExpressionGroup(
                        iconRegistry,
                        this.bounds.X + indent,
                        this.bounds.Bottom,
                        this.bounds.Width - (indent * 2),
                        subExpression,
                        this.Level + 1);

                    component = expressionGroup;
                    break;
                default:
                    var expressionTerm = new ExpressionTerm(
                        iconRegistry,
                        this.bounds.X + indent,
                        this.bounds.Bottom,
                        this.bounds.Width - (indent * 2),
                        subExpression,
                        this.Level + 1);

                    component = expressionTerm;
                    break;
            }

            component.ExpressionChanged += this.OnExpressionChanged;
            this.bounds.Height = component.bounds.Bottom - this.bounds.Top + 12;
            this.Components.Add(component);
        }
    }

    /// <inheritdoc />
    public override event EventHandler<ExpressionChangedEventArgs> ExpressionChanged
    {
        add => this.expressionChanged += value;
        remove => this.expressionChanged -= value;
    }

    /// <inheritdoc />
    protected override void DrawInFrame(SpriteBatch spriteBatch, Point cursor)
    {
        if (this.Level >= 0)
        {
            this.Color = this.Bounds.Contains(cursor - this.Offset)
                ? this.BaseColor.Highlight()
                : this.BaseColor.Muted();
        }

        base.DrawInFrame(spriteBatch, cursor);
    }

    private void OnExpressionChanged(object? sender, ExpressionChangedEventArgs e) =>
        this.expressionChanged?.InvokeAll(sender, e);

    private void RemoveExpression(object? sender, UiEventArgs e) =>
        this.expressionChanged?.InvokeAll(
            this,
            new ExpressionChangedEventArgs(ExpressionChange.Remove, this.Expression));

    private void UpdateColor(object? sender, RenderEventArgs e)
    {
        if (sender is not ICustomComponent component)
        {
            return;
        }

        component.Color = component.Bounds.Contains(e.Cursor - this.Offset)
            ? Color.Gray.Highlight()
            : Color.Gray.Muted();
    }
}