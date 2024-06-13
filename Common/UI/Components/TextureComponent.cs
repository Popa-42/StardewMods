#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.UI.Components;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.FauxCore.Common.Helpers;
using StardewMods.FauxCore.Common.Models.Events;
using StardewMods.FauxCore.Common.UI.Menus;
using StardewValley.Menus;

#else
namespace StardewMods.Common.UI.Components;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Helpers;
using StardewMods.Common.Models.Events;
using StardewMods.Common.UI.Menus;
using StardewValley.Menus;
#endif

/// <summary>Base custom texture component.</summary>
internal sealed class TextureComponent : ClickableTextureComponent, ICustomComponent
{
    private EventHandler<UiEventArgs>? clicked;
    private EventHandler<CursorEventArgs>? cursorOut;
    private EventHandler<CursorEventArgs>? cursorOver;
    private bool isHovered;
    private BaseMenu? menu;
    private Point offset;
    private EventHandler<RenderEventArgs>? rendering;
    private EventHandler<ScrolledEventArgs>? scrolled;

    /// <summary>Initializes a new instance of the <see cref="TextureComponent" /> class.</summary>
    /// <param name="name">The component name.</param>
    /// <param name="bounds">The component bounding box.</param>
    /// <param name="texture">The component texture.</param>
    /// <param name="sourceRect">The source rectangle..</param>
    /// <param name="scale">The texture scale.</param>
    public TextureComponent(string name, Rectangle bounds, Texture2D texture, Rectangle sourceRect, float scale)
        : base(name, bounds, null, null, texture, sourceRect, scale) =>
        this.Components = new ComponentList(this);

    /// <inheritdoc />
    public event EventHandler<UiEventArgs>? Clicked
    {
        add => this.clicked += value;
        remove => this.clicked -= value;
    }

    /// <inheritdoc />
    public event EventHandler<CursorEventArgs>? CursorOut
    {
        add => this.cursorOut += value;
        remove => this.cursorOut -= value;
    }

    /// <inheritdoc />
    public event EventHandler<CursorEventArgs>? CursorOver
    {
        add => this.cursorOver += value;
        remove => this.cursorOver -= value;
    }

    /// <inheritdoc />
    public event EventHandler<RenderEventArgs>? Rendering
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
    public Rectangle Bounds
    {
        get => this.bounds;
        set => this.bounds = value;
    }

    /// <inheritdoc />
    public Color Color { get; set; } = Color.White;

    /// <inheritdoc />
    public string? HoverText
    {
        get => this.hoverText;
        set => this.hoverText = value;
    }

    /// <inheritdoc />
    public int Id
    {
        get => this.myID;
        set => this.myID = value;
    }

    /// <inheritdoc />
    public bool IsVisible
    {
        get => this.visible;
        set => this.visible = value;
    }

    /// <inheritdoc />
    public string? Label
    {
        get => this.label;
        set => this.label = value;
    }

    /// <inheritdoc />
    public Point Location
    {
        get => this.bounds.Location;
        set => this.bounds.Location = value;
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
    public Point Offset
    {
        get => this.offset + (this.Parent?.Offset ?? Point.Zero);
        set => this.offset = value;
    }

    /// <inheritdoc />
    public ICustomComponent? Parent { get; set; }

    /// <inheritdoc />
    public float Scale
    {
        get => this.scale;
        set => this.scale = value;
    }

    /// <inheritdoc />
    public Point Size
    {
        get => this.bounds.Size;
        set => this.bounds.Size = value;
    }

    /// <inheritdoc />
    public void Draw(SpriteBatch spriteBatch, Point cursor)
    {
        if (!this.IsVisible)
        {
            return;
        }

        this.rendering?.InvokeAll(this, new RenderEventArgs(spriteBatch, cursor));
        this.draw(spriteBatch, this.Color, 1f, 0, -this.Offset.X, -this.Offset.Y);
    }

    /// <inheritdoc />
    public void DrawOver(SpriteBatch spriteBatch, Point cursor)
    {
        if (!this.IsVisible)
        {
            return;
        }

        if (this.Frame.Contains(cursor) && !string.IsNullOrWhiteSpace(this.HoverText))
        {
            IClickableMenu.drawToolTip(spriteBatch, this.HoverText, null, null);
        }
    }

    /// <inheritdoc />
    public void DrawUnder(SpriteBatch spriteBatch, Point cursor)
    {
        // Do nothing
    }

    /// <inheritdoc />
    public bool TryLeftClick(Point cursor)
    {
        if (!this.IsVisible || !this.Frame.Contains(cursor))
        {
            return false;
        }

        var uiEventArgs = new UiEventArgs(SButton.MouseLeft, cursor);
        this.clicked?.InvokeAll(this, uiEventArgs);
        return uiEventArgs.Handled;
    }

    /// <inheritdoc />
    public bool TryRightClick(Point cursor)
    {
        if (!this.IsVisible || !this.Frame.Contains(cursor))
        {
            return false;
        }

        var uiEventArgs = new UiEventArgs(SButton.MouseRight, cursor);
        this.clicked?.InvokeAll(this, uiEventArgs);
        return uiEventArgs.Handled;
    }

    /// <inheritdoc />
    public bool TryScroll(Point cursor, int direction)
    {
        if (!this.IsVisible || !this.Frame.Contains(cursor))
        {
            return false;
        }

        var scrolledEventArgs = new ScrolledEventArgs(cursor, direction);
        this.scrolled?.InvokeAll(this, scrolledEventArgs);
        return scrolledEventArgs.Handled;
    }

    /// <inheritdoc />
    public void Update(Point cursor)
    {
        if (!this.IsVisible)
        {
            return;
        }

        this.tryHover(cursor.X + this.Offset.X, cursor.Y + this.Offset.Y);
        var hovered = this.Menu?.GetChildMenu() is null && this.Frame.Contains(cursor);
        if (this.isHovered == hovered)
        {
            return;
        }

        this.isHovered = hovered;
        (this.isHovered ? this.cursorOver : this.cursorOut)?.InvokeAll(this, new CursorEventArgs(cursor));
    }
}