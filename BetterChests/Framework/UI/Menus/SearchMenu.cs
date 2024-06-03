namespace StardewMods.BetterChests.Framework.UI.Menus;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewMods.BetterChests.Framework.UI.Components;
using StardewMods.Common.Helpers;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.UI.Components;
using StardewMods.Common.UI.Menus;
using StardewValley.Menus;

/// <summary>A menu for editing search.</summary>
internal class SearchMenu : BaseMenu
{
    private readonly IExpressionHandler expressionHandler;
    private readonly ExpressionsEditor expressionsEditor;
    private readonly InventoryMenu inventory;
    private readonly VerticalScrollBar scrollExpressions;
    private readonly VerticalScrollBar scrollInventory;
    private readonly TextField textField;

    private List<Item> allItems = [];
    private int currentOffset;
    private int maxOffset;

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
            this.expressionsEditor.Bounds.X + this.expressionsEditor.Bounds.Width + 4,
            this.expressionsEditor.Bounds.Y + 4,
            this.expressionsEditor.Bounds.Height);

        this.inventory = new InventoryMenu(
            this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + (IClickableMenu.borderWidth / 2) + 428,
            this.yPositionOnScreen
            + IClickableMenu.spaceToClearSideBorder
            + (IClickableMenu.borderWidth / 2)
            + (Game1.tileSize * 2)
            + 12,
            false,
            new List<Item>(),
            this.HighlightMethod,
            35,
            7);

        this.scrollInventory = new VerticalScrollBar(
            iconRegistry,
            this.inventory.xPositionOnScreen + this.inventory.width + 4,
            this.inventory.yPositionOnScreen + 4,
            this.inventory.height);

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

        this.expressionsEditor.Scrolled += (_, e) =>
        {
            var offset = e.Direction switch
            {
                > 0 => new Point(0, this.expressionsEditor.Offset.Y - 16),
                < 0 => new Point(0, this.expressionsEditor.Offset.Y + 16),
                _ => this.expressionsEditor.Offset,
            };

            this.scrollExpressions.Value = (float)offset.Y / this.expressionsEditor.Overflow.Y;
        };

        this.scrollExpressions.ValueChanged += (_, value) =>
        {
            this.expressionsEditor.Offset = new Point(0, (int)(this.expressionsEditor.Overflow.Y * value));
        };

        this.scrollInventory.ValueChanged += (_, value) =>
        {
            this.currentOffset = (int)(this.maxOffset * value);
            this.inventory.actualInventory =
                this.allItems.Skip(this.currentOffset * 5).Take(this.inventory.capacity).ToList();
        };

        this.textField.ValueChanged += (_, _) => this.UpdateExpression();

        // Add components
        this.Components.Add(this.expressionsEditor);
        this.Components.Add(this.textField);
        this.Components.Add(this.scrollExpressions);
        this.Components.Add(this.scrollInventory);
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
    protected override void Draw(SpriteBatch spriteBatch, Point cursor)
    {
        base.Draw(spriteBatch, cursor);
        this.inventory.draw(spriteBatch);
    }

    /// <inheritdoc />
    protected override void DrawOver(SpriteBatch spriteBatch, Point cursor)
    {
        var item = this.inventory.hover(cursor.X, cursor.Y, null);
        if (item is not null)
        {
            IClickableMenu.drawToolTip(
                spriteBatch,
                this.inventory.descriptionText,
                this.inventory.descriptionTitle,
                item);
        }

        base.DrawOver(spriteBatch, cursor);
    }

    /// <inheritdoc />
    protected override void DrawUnder(SpriteBatch spriteBatch, Point cursor)
    {
        base.DrawUnder(spriteBatch, cursor);

        this.drawHorizontalPartition(
            spriteBatch,
            this.yPositionOnScreen + (IClickableMenu.borderWidth / 2) + Game1.tileSize + 40);

        this.drawVerticalIntersectingPartition(
            spriteBatch,
            this.xPositionOnScreen + (IClickableMenu.borderWidth / 2) + 400,
            this.yPositionOnScreen + (IClickableMenu.borderWidth / 2) + Game1.tileSize + 40);
    }

    /// <summary>Get the items that should be displayed in the menu.</summary>
    /// <returns>The items to display.</returns>
    protected virtual List<Item> GetItems() =>
        this.Expression is null
            ? Array.Empty<Item>().ToList()
            : ItemRepository.GetItems(this.Expression.Equals).ToList();

    /// <summary>Highlight the item.</summary>
    /// <param name="item">The item to highlight.</param>
    /// <returns>A value indicating whether the item should be highlighted.</returns>
    protected virtual bool HighlightMethod(Item item) => InventoryMenu.highlightAllItems(item);

    /// <summary>Refresh the displayed items.</summary>
    protected void RefreshItems()
    {
        this.allItems = this.GetItems();
        if (!this.allItems.Any())
        {
            this.inventory.actualInventory.Clear();
            this.maxOffset = 0;
            this.scrollInventory.IsVisible = false;
            return;
        }

        this.inventory.actualInventory = this.allItems.Take(this.inventory.capacity).ToList();
        this.maxOffset = (int)Math.Ceiling(this.allItems.Count / ((float)this.inventory.capacity / this.inventory.rows))
            - 7;

        this.scrollInventory.IsVisible = true;
    }

    /// <inheritdoc />
    protected override bool TryScroll(Point cursor, int direction)
    {
        if (!this.inventory.isWithinBounds(cursor.X, cursor.Y))
        {
            return false;
        }

        var offset = direction switch
        {
            > 0 => this.currentOffset - 1, < 0 => this.currentOffset + 1, _ => this.currentOffset,
        };

        this.scrollInventory.Value = (float)offset / this.maxOffset;
        return true;
    }

    /// <summary>Update the expression by parsing from the search text.</summary>
    protected void UpdateExpression()
    {
        this.Expression = this.expressionHandler.TryParseExpression(this.SearchText, out var expression)
            ? expression
            : null;

        this.expressionsEditor.ReinitializeComponents(this.Expression);
        this.scrollExpressions.IsVisible = !this.expressionsEditor.Overflow.Equals(Point.Zero);
        this.RefreshItems();
    }
}