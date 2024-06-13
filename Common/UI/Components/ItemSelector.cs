#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.UI.Components;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.FauxCore.Common.Helpers;
using StardewMods.FauxCore.Common.Models.Events;
using StardewValley.Menus;

#else
namespace StardewMods.Common.UI.Components;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Helpers;
using StardewMods.Common.Models.Events;
using StardewValley.Menus;
#endif

/// <summary>Component for selecting an item.</summary>
internal sealed class ItemSelector : BaseComponent
{
    private readonly int columns;
    private readonly List<InventoryMenu.highlightThisItem> highlights = [];
    private readonly InventoryMenu inventory;
    private readonly List<Operation> operations = [];

    private List<Item> items;

    /// <summary>Initializes a new instance of the <see cref="ItemSelector" /> class.</summary>
    /// <param name="x">The x-position.</param>
    /// <param name="y">The y-position.</param>
    /// <param name="rows">This rows of items to display.</param>
    /// <param name="columns">The columns of items to display.</param>
    public ItemSelector(int x, int y, int rows, int columns)
        : base(x, y, columns * Game1.tileSize, (rows * Game1.tileSize) + 24, "selectItem")
    {
        this.columns = columns;
        var capacity = rows * columns;
        this.inventory = new InventoryMenu(x, y + 4, false, new List<Item>(), this.HighlightMethod, capacity, rows);
        this.RefreshItems();
    }

    /// <summary>An operation to perform on the items.</summary>
    /// <param name="items">The original items.</param>
    /// <returns>Returns a modified list of items.</returns>
    public delegate IEnumerable<Item> Operation(IEnumerable<Item> items);

    /// <inheritdoc />
    public override Point Offset
    {
        get => base.Offset;
        set
        {
            base.Offset = value;
            this.inventory.actualInventory =
                this.items.Skip(this.Offset.Y * this.columns).Take(this.inventory.capacity).ToList();
        }
    }

    /// <summary>Add a highlight operation that will be applied to the items.</summary>
    /// <param name="highlight">The highlight operation.</param>
    public void AddHighlight(InventoryMenu.highlightThisItem highlight) => this.highlights.Add(highlight);

    /// <summary>Add an operation that will be applied to the icons.</summary>
    /// <param name="operation">The operation to perform.</param>
    public void AddOperation(Operation operation) => this.operations.Add(operation);

    /// <inheritdoc />
    public override void DrawOver(SpriteBatch spriteBatch, Point cursor)
    {
        var hoverItem = this.inventory.hover(cursor.X, cursor.Y, null);
        if (hoverItem is not null)
        {
            IClickableMenu.drawToolTip(
                spriteBatch,
                this.inventory.descriptionText,
                this.inventory.descriptionTitle,
                hoverItem);
        }
    }

    /// <summary>Refresh the displayed items.</summary>
    [MemberNotNull(nameof(ItemSelector.items))]
    public void RefreshItems()
    {
        this.items = this
            .operations.Aggregate(ItemRepository.GetItems(), (current, operation) => operation(current))
            .ToList();

        if (!this.items.Any())
        {
            this.inventory.actualInventory.Clear();
            this.Overflow = Point.Zero;
            return;
        }

        this.Offset = Point.Zero;
        this.Overflow = new Point(0, (int)Math.Ceiling((float)this.items.Count / this.columns) - this.inventory.rows);
    }

    /// <inheritdoc />
    public override bool TryScroll(Point cursor, int direction)
    {
        if (!this.IsVisible || this.Overflow.Equals(Point.Zero) || !this.Frame.Contains(cursor))
        {
            return false;
        }

        var scrolledEventArgs = new ScrolledEventArgs(cursor, direction);
        var oldY = this.Offset.Y;
        this.InvokeScrolled(this, scrolledEventArgs);
        if (scrolledEventArgs.Handled || this.Overflow.Equals(Point.Zero) || direction == 0)
        {
            return scrolledEventArgs.Handled;
        }

        // Apply default handling
        this.Offset = new Point(0, this.Offset.Y + (direction > 0 ? -1 : 1));
        if (oldY == this.Offset.Y)
        {
            return scrolledEventArgs.Handled;
        }

        scrolledEventArgs.PreventDefault();
        Game1.playSound("shiny4");
        return scrolledEventArgs.Handled;
    }

    /// <inheritdoc />
    protected override void DrawInFrame(SpriteBatch spriteBatch, Point cursor) => this.inventory.draw(spriteBatch);

    private bool HighlightMethod(Item itemToHighlight) =>
        !this.highlights.Any() || this.highlights.All(highlight => highlight(itemToHighlight));
}