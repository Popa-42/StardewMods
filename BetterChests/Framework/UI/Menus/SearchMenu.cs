namespace StardewMods.BetterChests.Framework.UI.Menus;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewMods.BetterChests.Framework.UI.Components;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.UI.Components;
using StardewMods.Common.UI.Menus;
using StardewValley.Menus;

/// <summary>A menu for editing search.</summary>
internal class SearchMenu : BaseMenu
{
    private readonly IExpressionHandler expressionHandler;
    private readonly ExpressionsEditor expressionsEditor;
    private readonly ItemSelector itemSelector;
    private readonly VerticalScrollBar scrollExpressions;
    private readonly TextField textField;

    /// <summary>Initializes a new instance of the <see cref="SearchMenu" /> class.</summary>
    /// <param name="expressionHandler">Dependency used for parsing expressions.</param>
    /// <param name="iconRegistry">Dependency used for registering and retrieving icons.</param>
    /// <param name="initialValue">The initial search text.</param>
    public SearchMenu(IExpressionHandler expressionHandler, IIconRegistry iconRegistry, string initialValue)
    {
        // Initialize
        this.expressionHandler = expressionHandler;

        this.expressionsEditor = new ExpressionsEditor(
            this.expressionHandler,
            iconRegistry,
            this.xPositionOnScreen + IClickableMenu.borderWidth,
            this.yPositionOnScreen
            + IClickableMenu.spaceToClearSideBorder
            + (IClickableMenu.borderWidth / 2)
            + (Game1.tileSize * 2)
            + 12,
            340,
            448);

        this.scrollExpressions = new VerticalScrollBar(
            iconRegistry,
            this.expressionsEditor.Bounds.Right + 4,
            this.expressionsEditor.Bounds.Y + 4,
            this.expressionsEditor.Bounds.Height);

        this.scrollExpressions.Attach(this.expressionsEditor);

        this.itemSelector = new ItemSelector(
            this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + (IClickableMenu.borderWidth / 2) + 428,
            this.yPositionOnScreen
            + IClickableMenu.spaceToClearSideBorder
            + (IClickableMenu.borderWidth / 2)
            + (Game1.tileSize * 2)
            + 8,
            7,
            5);

        this.itemSelector.AddHighlight(this.HighlightMethod);
        this.itemSelector.AddOperation(this.GetItems);

        var scrollInventory = new VerticalScrollBar(
            iconRegistry,
            this.itemSelector.Bounds.Right + 4,
            this.itemSelector.Bounds.Y + 4,
            this.itemSelector.Bounds.Height);

        scrollInventory.Attach(this.itemSelector);

        this.textField = new TextField(
            this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + (IClickableMenu.borderWidth / 2),
            this.yPositionOnScreen
            + IClickableMenu.spaceToClearSideBorder
            + (IClickableMenu.borderWidth / 2)
            + Game1.tileSize,
            this.width - (IClickableMenu.spaceToClearSideBorder * 2) - IClickableMenu.borderWidth,
            initialValue);

        // Events
        this.expressionsEditor.ExpressionsChanged += (_, expression) =>
        {
            if (!string.IsNullOrWhiteSpace(expression?.Text))
            {
                this.SearchText = expression.Text;
            }

            this.scrollExpressions.IsVisible = !this.expressionsEditor.Overflow.Equals(Point.Zero);
            this.RefreshItems();
        };

        this.textField.ValueChanged += (_, _) => this.UpdateExpression();

        // Add components
        this.Components.Add(this.expressionsEditor);
        this.Components.Add(this.itemSelector);
        this.Components.Add(this.textField);
        this.Components.Add(this.scrollExpressions);
        this.Components.Add(scrollInventory);
        this.UpdateExpression();
    }

    /// <summary>Gets the current search expression.</summary>
    protected IExpression? Expression { get; private set; }

    /// <summary>Gets or sets the current search text.</summary>
    protected string SearchText
    {
        get => this.textField.Value;
        set => this.textField.Value = value;
    }

    /// <inheritdoc />
    public override void receiveKeyPress(Keys key)
    {
        if (key is Keys.Escape && this.readyToClose())
        {
            this.exitThisMenuNoSound();
        }

        if (key is Keys.Tab && this.textField.Selected)
        {
            // Auto-complete on tab
        }
    }

    /// <inheritdoc />
    protected override void DrawUnder(SpriteBatch spriteBatch, Point cursor)
    {
        base.DrawUnder(spriteBatch, cursor);

        this.drawHorizontalPartition(
            spriteBatch,
            this.Bounds.Y + (IClickableMenu.borderWidth / 2) + Game1.tileSize + 40);

        this.drawVerticalIntersectingPartition(
            spriteBatch,
            this.Bounds.X + (IClickableMenu.borderWidth / 2) + 400,
            this.Bounds.Y + (IClickableMenu.borderWidth / 2) + Game1.tileSize + 40);
    }

    /// <summary>Get the items that should be displayed in the menu.</summary>
    /// <param name="items">The items.</param>
    /// <returns>The items to display.</returns>
    protected virtual IEnumerable<Item> GetItems(IEnumerable<Item> items) =>
        this.Expression is null ? Array.Empty<Item>() : items.Where(this.Expression.Equals);

    /// <summary>Highlight the item.</summary>
    /// <param name="item">The item to highlight.</param>
    /// <returns>A value indicating whether the item should be highlighted.</returns>
    protected virtual bool HighlightMethod(Item item) => InventoryMenu.highlightAllItems(item);

    /// <summary>Refresh the displayed items.</summary>
    protected void RefreshItems() => this.itemSelector.RefreshItems();

    /// <summary>Update the expression by parsing from the search text.</summary>
    protected void UpdateExpression()
    {
        this.Expression = this.expressionHandler.TryParseExpression(this.SearchText, out var expression)
            ? expression
            : null;

        this.expressionsEditor.Expression = this.Expression;
        this.scrollExpressions.IsVisible = !this.expressionsEditor.Overflow.Equals(Point.Zero);
        this.RefreshItems();
    }
}