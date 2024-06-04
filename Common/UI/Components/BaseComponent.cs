#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.UI.Components;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.FauxCore.Common.Helpers;
using StardewMods.FauxCore.Common.Models.Events;
using StardewMods.FauxCore.Common.Services;
using StardewMods.FauxCore.Common.UI.Menus;
using StardewValley.Menus;

#else
namespace StardewMods.Common.UI.Components;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Helpers;
using StardewMods.Common.Models.Events;
using StardewMods.Common.Services;
using StardewMods.Common.UI.Menus;
using StardewValley.Menus;
#endif

/// <summary>Base custom component.</summary>
internal abstract class BaseComponent : ClickableComponent, ICustomComponent
{
    private float baseScale;
    private EventHandler<UiEventArgs>? clicked;
    private EventHandler<CursorEventArgs>? cursorOut;
    private EventHandler<CursorEventArgs>? cursorOver;
    private bool isHovered;
    private BaseMenu? menu;
    private Point offset;
    private Point overflow;

    private EventHandler<ValueChangedEventArgs<Point>>? overflowChanged;
    private EventHandler<RenderEventArgs>? rendering;
    private EventHandler<ScrolledEventArgs>? scrolled;

    /// <summary>Initializes a new instance of the <see cref="BaseComponent" /> class.</summary>
    /// <param name="x">The component x-coordinate.</param>
    /// <param name="y">The component y-coordinate.</param>
    /// <param name="width">The component width.</param>
    /// <param name="height">The component height.</param>
    /// <param name="name">The component name.</param>
    protected BaseComponent(int x, int y, int width, int height, string name)
        : base(new Rectangle(x, y, width, height), name) =>
        this.Components = new ComponentList(this);

    /// <inheritdoc />
    public event EventHandler<UiEventArgs> Clicked
    {
        add => this.clicked += value;
        remove => this.clicked -= value;
    }

    /// <inheritdoc />
    public event EventHandler<CursorEventArgs> CursorOut
    {
        add => this.cursorOut += value;
        remove => this.cursorOut -= value;
    }

    /// <inheritdoc />
    public event EventHandler<CursorEventArgs> CursorOver
    {
        add => this.cursorOver += value;
        remove => this.cursorOver -= value;
    }

    /// <summary>Event raised when the overflow value is changed.</summary>
    public event EventHandler<ValueChangedEventArgs<Point>> OverflowChanged
    {
        add => this.overflowChanged += value;
        remove => this.overflowChanged -= value;
    }

    /// <inheritdoc />
    public event EventHandler<RenderEventArgs> Rendering
    {
        add => this.rendering += value;
        remove => this.rendering -= value;
    }

    /// <inheritdoc />
    public event EventHandler<ScrolledEventArgs> Scrolled
    {
        add => this.scrolled += value;
        remove => this.scrolled -= value;
    }

    /// <inheritdoc />
    public Rectangle Bounds => this.bounds;

    /// <inheritdoc />
    public ComponentList Components { get; }

    /// <inheritdoc />
    public Rectangle Frame =>
        this.Parent is not null
            ? Rectangle.Intersect(
                this.Parent.Frame,
                new Rectangle(
                    this.bounds.X - this.Parent.Offset.X,
                    this.bounds.Y - this.Parent.Offset.Y,
                    this.bounds.Width,
                    this.bounds.Height))
            : this.Bounds;

    /// <inheritdoc />
    public float BaseScale
    {
        get => this.baseScale;
        set
        {
            this.baseScale = value;
            this.Scale = value;
        }
    }

    /// <inheritdoc />
    public Color Color { get; set; } = Color.White;

    /// <inheritdoc />
    public string? HoverText { get; set; }

    /// <inheritdoc />
    public int Id
    {
        get => this.myID;
        set => this.myID = value;
    }

    /// <inheritdoc />
    public bool IsVisible
    {
        get => this.visible && !this.Frame.Equals(Rectangle.Empty);
        set => this.visible = value;
    }

    /// <inheritdoc />
    public string? Label
    {
        get => this.label;
        set => this.label = value;
    }

    /// <inheritdoc />
    public virtual Point Location
    {
        get => this.bounds.Location;
        set
        {
            this.RepositionComponents(value);
            this.bounds.Location = value;
        }
    }

    /// <inheritdoc />
    public BaseMenu? Menu
    {
        get => this.Parent?.Menu ?? this.menu;
        set => this.menu = value;
    }

    /// <inheritdoc />
    public string Name
    {
        get => this.name;
        set => this.name = value;
    }

    /// <inheritdoc />
    public virtual Point Offset
    {
        get => this.offset + (this.Parent?.Offset ?? Point.Zero);
        set =>
            this.offset = new Point(Math.Clamp(value.X, 0, this.Overflow.X), Math.Clamp(value.Y, 0, this.Overflow.Y));
    }

    /// <summary>Gets or sets the component overflow.</summary>
    public virtual Point Overflow
    {
        get => this.overflow;
        protected set
        {
            var oldValue = new Point(this.overflow.X, this.overflow.Y);
            this.overflow = new Point(Math.Max(0, value.X), Math.Max(0, value.Y));
            this.Offset = new Point(
                Math.Clamp(this.offset.X, 0, this.overflow.X),
                Math.Clamp(this.offset.Y, 0, this.overflow.Y));

            this.overflowChanged?.InvokeAll(this, new ValueChangedEventArgs<Point>(oldValue, this.overflow));
        }
    }

    /// <inheritdoc />
    public ICustomComponent? Parent { get; set; }

    /// <inheritdoc />
    public float Scale { get; set; }

    /// <inheritdoc />
    public virtual Point Size
    {
        get => this.bounds.Size;
        set
        {
            this.RepositionComponents(this.bounds.Location);
            this.bounds.Size = value;
        }
    }

    /// <inheritdoc />
    public virtual void Draw(SpriteBatch spriteBatch, Point cursor)
    {
        if (!this.IsVisible)
        {
            return;
        }

        this.InvokeRendering(this, new RenderEventArgs(spriteBatch, cursor));
        UiToolkit.DrawInFrame(spriteBatch, this.Frame, sb => this.DrawInFrame(sb, cursor));
    }

    /// <inheritdoc />
    public virtual void DrawOver(SpriteBatch spriteBatch, Point cursor)
    {
        if (!this.IsVisible)
        {
            return;
        }

        if (this.Frame.Contains(cursor) && !string.IsNullOrWhiteSpace(this.HoverText))
        {
            IClickableMenu.drawToolTip(spriteBatch, this.HoverText, null, null);
        }

        foreach (var component in this.Components.OfType<ICustomComponent>())
        {
            component.DrawOver(spriteBatch, cursor);
        }
    }

    /// <inheritdoc />
    public virtual void DrawUnder(SpriteBatch spriteBatch, Point cursor)
    {
        if (!this.IsVisible)
        {
            return;
        }

        foreach (var component in this.Components.OfType<ICustomComponent>())
        {
            component.DrawUnder(spriteBatch, cursor);
        }
    }

    /// <inheritdoc />
    public virtual bool TryLeftClick(Point cursor)
    {
        if (!this.IsVisible || !this.Frame.Contains(cursor))
        {
            return false;
        }

        // Left-click components
        foreach (var component in this
            .Components.OfType<ICustomComponent>()
            .Reverse()
            .Where(component => component.TryLeftClick(cursor)))
        {
            this.InvokeClicked(component, new UiEventArgs(SButton.Left, cursor));
            return true;
        }

        this.InvokeClicked(this, new UiEventArgs(SButton.MouseLeft, cursor));
        return true;
    }

    /// <inheritdoc />
    public virtual bool TryRightClick(Point cursor)
    {
        if (!this.IsVisible || !this.Frame.Contains(cursor))
        {
            return false;
        }

        // Right-click components
        foreach (var component in this
            .Components.OfType<ICustomComponent>()
            .Reverse()
            .Where(component => component.TryRightClick(cursor)))
        {
            this.InvokeClicked(component, new UiEventArgs(SButton.Right, cursor));
            return true;
        }

        this.InvokeClicked(this, new UiEventArgs(SButton.Right, cursor));
        return true;
    }

    /// <inheritdoc />
    public virtual bool TryScroll(Point cursor, int direction)
    {
        if (!this.IsVisible || !this.Frame.Contains(cursor))
        {
            return false;
        }

        if (!this.Overflow.Equals(Point.Zero) && direction != 0)
        {
            var oldY = this.Offset.Y;
            this.Offset = new Point(0, this.Offset.Y + (direction > 0 ? -32 : 32));
            if (oldY == this.Offset.Y)
            {
                return true;
            }

            Game1.playSound("shiny4");
            this.InvokeScrolled(this, new ScrolledEventArgs(cursor, direction));
            return true;
        }

        this.InvokeScrolled(this, new ScrolledEventArgs(cursor, direction));
        return false;
    }

    /// <inheritdoc />
    public virtual void Update(Point cursor)
    {
        if (!this.IsVisible)
        {
            return;
        }

        foreach (var component in this.Components.OfType<ICustomComponent>())
        {
            component.Update(cursor);
        }

        var hovered = this.Menu?.GetChildMenu() is null && this.Frame.Contains(cursor);
        if (this.isHovered == hovered)
        {
            return;
        }

        this.isHovered = hovered;
        (this.isHovered ? this.cursorOver : this.cursorOut)?.InvokeAll(this, new CursorEventArgs(cursor));
    }

    /// <summary>Draws the component in a framed area.</summary>
    /// <param name="spriteBatch">The sprite batch to draw the component to.</param>
    /// <param name="cursor">The mouse position.</param>
    protected virtual void DrawInFrame(SpriteBatch spriteBatch, Point cursor)
    {
        foreach (var component in this.Components.OfType<ICustomComponent>())
        {
            component.Draw(spriteBatch, cursor);
        }
    }

    /// <summary>Invokes the clicked event.</summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event arguments.</param>
    protected void InvokeClicked(object? sender, UiEventArgs e) => this.clicked?.InvokeAll(sender, e);

    /// <summary>Invokes the rendering event.</summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event arguments.</param>
    protected void InvokeRendering(object? sender, RenderEventArgs e) => this.rendering?.InvokeAll(sender, e);

    /// <summary>Invokes the scrolled event.</summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event arguments.</param>
    protected void InvokeScrolled(object? sender, ScrolledEventArgs e) => this.scrolled?.InvokeAll(sender, e);

    /// <summary>Reposition components when bounds changes.</summary>
    /// <param name="newLocation">The new origin.</param>
    protected virtual void RepositionComponents(Point newLocation)
    {
        var delta = newLocation - this.bounds.Location;
        if (delta.Equals(Point.Zero))
        {
            return;
        }

        foreach (var component in this.Components)
        {
            switch (component)
            {
                case ICustomComponent customComponent:
                    customComponent.Location += delta;
                    break;
                default:
                    component.bounds.Location += delta;
                    break;
            }
        }
    }
}