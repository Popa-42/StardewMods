namespace StardewMods.BetterChests.Framework.UI.Components;

using System.Collections.Immutable;
using System.Globalization;
using Microsoft.Xna.Framework;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.BetterChests.Framework.Services;
using StardewMods.Common.Enums;
using StardewMods.Common.Helpers;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.UI.Components;
using StardewMods.Common.UI.Menus;
using StardewValley.Menus;

/// <summary>Component for editing expressions.</summary>
internal sealed class ExpressionsEditor : BaseComponent
{
    private readonly IExpressionHandler expressionHandler;
    private readonly IIconRegistry iconRegistry;

    private EventHandler<IExpression?>? expressionsChanged;

    private ExpressionGroup? rootComponent;

    /// <summary>Initializes a new instance of the <see cref="ExpressionsEditor" /> class.</summary>
    /// <param name="expressionHandler">Dependency used for parsing expressions.</param>
    /// <param name="iconRegistry">Dependency used for registering and retrieving icons.</param>
    /// <param name="x">The component x-coordinate.</param>
    /// <param name="y">The component y-coordinate.</param>
    /// <param name="width">The component width.</param>
    /// <param name="height">The component height.</param>
    public ExpressionsEditor(
        IExpressionHandler expressionHandler,
        IIconRegistry iconRegistry,
        int x,
        int y,
        int width,
        int height)
        : base(x, y, width, height, "expressions")
    {
        this.expressionHandler = expressionHandler;
        this.iconRegistry = iconRegistry;
    }

    /// <summary>Event raised when the expressions change.</summary>
    public event EventHandler<IExpression?> ExpressionsChanged
    {
        add => this.expressionsChanged += value;
        remove => this.expressionsChanged -= value;
    }

    /// <summary>Reinitialize components based on a new root expression.</summary>
    /// <param name="expression">The root expression.</param>
    public void ReinitializeComponents(IExpression? expression)
    {
        this.Components.Clear();
        if (expression is null)
        {
            return;
        }

        this.rootComponent = new ExpressionGroup(
            this.iconRegistry,
            this.Bounds.X,
            this.Bounds.Y,
            this.Bounds.Width,
            expression,
            -1);

        this.rootComponent.ExpressionChanged += this.OnExpressionChanged;
        this.Components.Add(this.rootComponent);
        this.Overflow = new Point(0, this.rootComponent.Bounds.Height - this.Bounds.Height);
    }

    private void AddExpression(IExpression toAddTo, ExpressionType expressionType)
    {
        if (this.rootComponent is null
            || !this.expressionHandler.TryCreateExpression(expressionType, out var newExpression))
        {
            this.expressionsChanged?.InvokeAll(this, null);
            return;
        }

        var newChildren = GetChildren().ToImmutableList();
        toAddTo.Expressions = newChildren;
        this.ReinitializeComponents(this.rootComponent.Expression);
        this.expressionsChanged?.InvokeAll(this, this.rootComponent.Expression);
        return;

        IEnumerable<IExpression> GetChildren()
        {
            foreach (var child in toAddTo.Expressions)
            {
                yield return child;
            }

            yield return newExpression;
        }
    }

    private void ChangeAttribute(IExpression toChange, string newValue)
    {
        if (string.IsNullOrWhiteSpace(newValue) || !ItemAttributeExtensions.TryParse(newValue, out var newAttribute))
        {
            return;
        }

        var dynamicTerm = toChange.Expressions.ElementAtOrDefault(0);
        if (this.rootComponent?.Expression is null || dynamicTerm?.ExpressionType is not ExpressionType.Dynamic)
        {
            return;
        }

        dynamicTerm.Term = newAttribute.ToStringFast();
        this.ReinitializeComponents(this.rootComponent.Expression);
        this.expressionsChanged?.InvokeAll(this, this.rootComponent.Expression);
    }

    private void ChangeValue(IExpression toChange, string newValue)
    {
        var staticTerm = toChange.Expressions.ElementAtOrDefault(1);
        if (this.rootComponent?.Expression is null
            || staticTerm?.ExpressionType is not (ExpressionType.Quoted or ExpressionType.Static))
        {
            return;
        }

        staticTerm.Term = newValue;
        this.ReinitializeComponents(this.rootComponent.Expression);
        this.expressionsChanged?.InvokeAll(this, this.rootComponent.Expression);
    }

    private void OnExpressionChanged(object? sender, ExpressionChangedEventArgs e)
    {
        switch (e.Change)
        {
            case ExpressionChange.AddGroup:
                this.AddExpression(e.Expression, ExpressionType.All);
                break;
            case ExpressionChange.AddNot:
                this.AddExpression(e.Expression, ExpressionType.Not);
                break;
            case ExpressionChange.AddTerm:
                this.AddExpression(e.Expression, ExpressionType.Comparable);
                break;
            case ExpressionChange.ChangeAttribute when sender is ButtonComponent component:
                this.ShowDropdown(e.Expression, component);
                break;
            case ExpressionChange.ChangeValue when sender is ButtonComponent component:
                this.ShowPopup(e.Expression, component);
                break;
            case ExpressionChange.Remove:
                this.RemoveExpression(e.Expression);
                break;
            case ExpressionChange.ToggleGroup:
                this.ToggleGroup(e.Expression);
                break;
        }
    }

    private void RemoveExpression(IExpression toRemove)
    {
        if (this.rootComponent?.Expression is null || toRemove.Parent is null)
        {
            return;
        }

        var newChildren = GetChildren().ToImmutableList();
        toRemove.Parent.Expressions = newChildren;
        this.ReinitializeComponents(this.rootComponent.Expression);
        this.expressionsChanged?.InvokeAll(this, this.rootComponent.Expression);
        return;

        IEnumerable<IExpression> GetChildren()
        {
            foreach (var child in toRemove.Parent.Expressions)
            {
                if (child != toRemove)
                {
                    yield return child;
                }
            }
        }
    }

    private void ShowDropdown(IExpression expression, ClickableComponent component)
    {
        var dropdown = new OptionDropdown<ItemAttribute>(
            component,
            ItemAttributeExtensions.GetValues().AsEnumerable(),
            getLabel: static attribute => Localized.Attribute(attribute.ToStringFast()));

        dropdown.OptionSelected += (_, attribute) => this.ChangeAttribute(expression, attribute.ToStringFast());

        this.Menu?.SetChildMenu(dropdown);
    }

    private void ShowPopup(IExpression expression, ClickableComponent component)
    {
        var leftTerm = expression.Expressions.ElementAtOrDefault(0);
        if (leftTerm?.ExpressionType is not ExpressionType.Dynamic
            || !ItemAttributeExtensions.TryParse(leftTerm.Term, out var itemAttribute))
        {
            return;
        }

        var popupItems = itemAttribute switch
        {
            ItemAttribute.Any => ItemRepository.Categories.Concat(ItemRepository.Names).Concat(ItemRepository.Tags),
            ItemAttribute.Category => ItemRepository.Categories,
            ItemAttribute.Name => ItemRepository.Names,
            ItemAttribute.Quality => ItemQualityExtensions.GetNames(),
            ItemAttribute.Quantity =>
                Enumerable.Range(0, 999).Select(i => i.ToString(CultureInfo.InvariantCulture)),
            ItemAttribute.Tags => ItemRepository.Tags,
            _ => throw new ArgumentOutOfRangeException(nameof(expression)),
        };

        var popupSelect = new OptionPopup<string>(this.iconRegistry, component.label, popupItems, 10);
        popupSelect.OptionSelected += (_, option) =>
        {
            if (option is not null)
            {
                this.ChangeValue(expression, option);
            }
        };

        this.Menu?.SetChildMenu(popupSelect);
    }

    private void ToggleGroup(IExpression toToggle)
    {
        var expressionType = toToggle.ExpressionType switch
        {
            ExpressionType.All => ExpressionType.Any,
            ExpressionType.Any => ExpressionType.All,
            _ => ExpressionType.All,
        };

        if (this.rootComponent?.Expression is null
            || toToggle.Parent is null
            || !this.expressionHandler.TryCreateExpression(expressionType, out var newChild))
        {
            return;
        }

        var newChildren = GetChildren().ToImmutableList();
        toToggle.Parent.Expressions = newChildren;
        this.ReinitializeComponents(this.rootComponent.Expression);
        this.expressionsChanged?.InvokeAll(this, this.rootComponent.Expression);
        return;

        IEnumerable<IExpression> GetChildren()
        {
            foreach (var child in toToggle.Parent.Expressions)
            {
                if (child != toToggle)
                {
                    yield return child;

                    continue;
                }

                newChild.Expressions = child.Expressions;
                yield return newChild;
            }
        }
    }
}