#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.UI.Menus;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.FauxCore.Common.Services;
using StardewValley.Menus;

#else
namespace StardewMods.Common.UI.Menus;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Services;
using StardewValley.Menus;
#endif

/// <summary>A custom menu that is constructed from the menu factory.</summary>
internal class BaseMenu : IClickableMenu
{
    private Rectangle bounds;

    /// <summary>Initializes a new instance of the <see cref="BaseMenu" /> class.</summary>
    /// <param name="x">The x-position of the menu.</param>
    /// <param name="y">The y-position of the menu.</param>
    /// <param name="width">The width of the menu.</param>
    /// <param name="height">The height of the menu.</param>
    /// <param name="showUpperRightCloseButton">A value indicating whether to show the right close button.</param>
    public BaseMenu(
        int? x = null,
        int? y = null,
        int? width = null,
        int? height = null,
        bool showUpperRightCloseButton = false)
        : base(
            x ?? (Game1.uiViewport.Width / 2) - ((width ?? 800 + (IClickableMenu.borderWidth * 2)) / 2),
            y ?? (Game1.uiViewport.Height / 2) - ((height ?? 600 + (IClickableMenu.borderWidth * 2)) / 2),
            width ?? 800 + (IClickableMenu.borderWidth * 2),
            height ?? 600 + (IClickableMenu.borderWidth * 2),
            showUpperRightCloseButton)
    {
        this.Components = new ComponentList(this);
        this.allClickableComponents ??= [];
        this.bounds = new Rectangle(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height);
        if (this.upperRightCloseButton is not null)
        {
            this.allClickableComponents.Add(this.upperRightCloseButton);
        }
    }

    /// <summary>Gets the menu bounds.</summary>
    public virtual Rectangle Bounds => this.bounds;

    /// <summary>Gets the components.</summary>
    public ComponentList Components { get; }

    /// <summary>Gets or sets the hover text.</summary>
    public string? HoverText { get; set; }

    /// <summary>Gets or sets the menu location.</summary>
    public virtual Point Location
    {
        get => this.bounds.Location;
        set
        {
            var delta = value - this.bounds.Location;
            this.bounds.Location = value;
            foreach (var component in this.allClickableComponents.OfType<ICustomComponent>())
            {
                component.Location += delta;
            }

            this.xPositionOnScreen = value.X;
            this.yPositionOnScreen = value.Y;
        }
    }

    /// <summary>Gets or sets the menu size.</summary>
    public Point Size
    {
        get => this.bounds.Size;
        set
        {
            this.bounds.Size = value;
            this.width = value.X;
            this.height = value.Y;
        }
    }

    /// <inheritdoc />
    public sealed override void draw(SpriteBatch b) => this.draw(b, -1);

    /// <inheritdoc />
    public sealed override void draw(SpriteBatch b, int red = -1, int green = -1, int blue = -1)
    {
        var cursor = UiToolkit.Cursor;
        this.HoverText = null;

        // Draw under
        this.DrawUnder(b, cursor);

        // Draw components
        foreach (var component in this.allClickableComponents.OfType<ICustomComponent>())
        {
            component.DrawUnder(b, cursor);
        }

        // Draw menu
        this.Draw(b, cursor);

        foreach (var component in this.allClickableComponents)
        {
            switch (component)
            {
                case ICustomComponent customComponent when component.visible:
                    customComponent.Draw(b, cursor);
                    break;
                case ClickableTextureComponent
                {
                    visible: true,
                } clickableTextureComponent:
                    clickableTextureComponent.draw(b);
                    break;
            }
        }

        if (this.GetChildMenu() is not null)
        {
            return;
        }

        // Draw over
        this.DrawOver(b, cursor);
    }

    /// <inheritdoc />
    public sealed override void leftClickHeld(int x, int y)
    {
        base.leftClickHeld(x, y);
        var cursor = new Point(x, y);
        this.Update(cursor);

        // Update components
        foreach (var component in this.allClickableComponents)
        {
            switch (component)
            {
                case ICustomComponent customComponent:
                    customComponent.Update(cursor);
                    break;
                case ClickableTextureComponent
                {
                    visible: true,
                } clickableTextureComponent:
                    clickableTextureComponent.tryHover(cursor.X, cursor.Y);
                    break;
            }
        }
    }

    /// <inheritdoc />
    public sealed override void performHoverAction(int x, int y)
    {
        base.performHoverAction(x, y);
        var cursor = new Point(x, y);
        this.Update(cursor);

        // Update components
        foreach (var component in this.allClickableComponents)
        {
            switch (component)
            {
                case ICustomComponent customComponent:
                    customComponent.Update(cursor);
                    break;
                case ClickableTextureComponent
                {
                    visible: true,
                } clickableTextureComponent:
                    clickableTextureComponent.tryHover(cursor.X, cursor.Y);
                    break;
            }
        }
    }

    /// <inheritdoc />
    public sealed override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        base.receiveLeftClick(x, y, playSound);
        var cursor = new Point(x, y);

        if (this.TryLeftClick(new Point(x, y))
            || this.allClickableComponents.OfType<ICustomComponent>().Any(component => component.TryLeftClick(cursor)))
        {
            // Do nothing
        }
    }

    /// <inheritdoc />
    public sealed override void receiveRightClick(int x, int y, bool playSound = true)
    {
        base.receiveRightClick(x, y, playSound);
        var cursor = new Point(x, y);

        if (this.TryRightClick(new Point(x, y))
            || this.allClickableComponents.OfType<ICustomComponent>().Any(component => component.TryRightClick(cursor)))
        {
            // Do nothing
        }
    }

    /// <inheritdoc />
    public sealed override void receiveScrollWheelAction(int direction)
    {
        base.receiveScrollWheelAction(direction);
        var cursor = UiToolkit.Cursor;
        if (this.TryScroll(cursor, direction)
            || this
                .allClickableComponents.OfType<ICustomComponent>()
                .Any(component => component.TryScroll(cursor, direction)))
        {
            // Do nothing
        }
    }

    /// <inheritdoc />
    public sealed override void releaseLeftClick(int x, int y)
    {
        base.releaseLeftClick(x, y);
        var cursor = new Point(x, y);
        this.Update(cursor);

        // Update components
        foreach (var component in this.allClickableComponents)
        {
            switch (component)
            {
                case ICustomComponent customComponent:
                    customComponent.Update(cursor);
                    break;
                case ClickableTextureComponent
                {
                    visible: true,
                } clickableTextureComponent:
                    clickableTextureComponent.tryHover(cursor.X, cursor.Y);
                    break;
            }
        }
    }

    /// <summary>Draw to the menu.</summary>
    /// <param name="spriteBatch">The sprite batch to draw to.</param>
    /// <param name="cursor">The mouse position.</param>
    protected virtual void Draw(SpriteBatch spriteBatch, Point cursor) { }

    /// <summary>Draw over the menu.</summary>
    /// <param name="spriteBatch">The sprite batch to draw to.</param>
    /// <param name="cursor">The mouse position.</param>
    protected virtual void DrawOver(SpriteBatch spriteBatch, Point cursor)
    {
        // Draw components
        foreach (var component in this.allClickableComponents.OfType<ICustomComponent>())
        {
            component.DrawOver(spriteBatch, cursor);
        }

        // Draw hover text
        if (!string.IsNullOrWhiteSpace(this.HoverText))
        {
            IClickableMenu.drawToolTip(spriteBatch, this.HoverText, null, null);
        }

        // Draw cursor
        Game1.mouseCursorTransparency = 1f;
        this.drawMouse(spriteBatch);
    }

    /// <summary>Draw under the menu.</summary>
    /// <param name="spriteBatch">The sprite batch to draw to.</param>
    /// <param name="cursor">The mouse position.</param>
    protected virtual void DrawUnder(SpriteBatch spriteBatch, Point cursor)
    {
        // Draw background
        if (!Game1.options.showClearBackgrounds && this.GetParentMenu() is null)
        {
            spriteBatch.Draw(
                Game1.fadeToBlackRect,
                new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height),
                Color.Black * 0.5f);
        }

        Game1.drawDialogueBox(
            this.xPositionOnScreen,
            this.yPositionOnScreen,
            this.width,
            this.height,
            false,
            true,
            null,
            false,
            false);
    }

    /// <summary>Try to perform a left-click.</summary>
    /// <param name="cursor">The mouse position.</param>
    /// <returns><c>true</c> if left click was handled; otherwise, <c>false</c>.</returns>
    protected virtual bool TryLeftClick(Point cursor) => false;

    /// <summary>Try to perform a right-click.</summary>
    /// <param name="cursor">The mouse position.</param>
    /// <returns><c>true</c> if right click was handled; otherwise, <c>false</c>.</returns>
    protected virtual bool TryRightClick(Point cursor) => false;

    /// <summary>Try to perform a scroll.</summary>
    /// <param name="cursor">The mouse position.</param>
    /// <param name="direction">The direction of the scroll.</param>
    /// <returns><c>true</c> if scroll was handled; otherwise, <c>false</c>.</returns>
    protected virtual bool TryScroll(Point cursor, int direction) => false;

    /// <summary>Performs an update.</summary>
    /// <param name="cursor">The mouse position.</param>
    protected virtual void Update(Point cursor)
    {
        // Do nothing
    }
}