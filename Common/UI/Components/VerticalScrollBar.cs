#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.UI.Components;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.FauxCore.Common.Helpers;
using StardewMods.FauxCore.Common.Models.Events;
using StardewMods.FauxCore.Common.Services;
using StardewMods.FauxCore.Common.Services.Integrations.FauxCore;
using StardewValley.Menus;

#else
namespace StardewMods.Common.UI.Components;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Helpers;
using StardewMods.Common.Models.Events;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.Menus;
#endif

/// <summary>Represents a scrollbar with up/down arrows.</summary>
internal sealed class VerticalScrollBar : BaseComponent
{
    private readonly TextureComponent arrowDown;
    private readonly TextureComponent arrowUp;
    private readonly TextureComponent grabber;
    private Rectangle runner;
    private EventHandler<ScrolledEventArgs>? scrolled;

    private float value;

    /// <summary>Initializes a new instance of the <see cref="VerticalScrollBar" /> class.</summary>
    /// <param name="iconRegistry">Dependency used for registering and retrieving icons.</param>
    /// <param name="x">The x-coordinate of the scroll bar.</param>
    /// <param name="y">The y-coordinate of the scroll bar.</param>
    /// <param name="height">The height of the scroll bar.</param>
    public VerticalScrollBar(IIconRegistry iconRegistry, int x, int y, int height)
        : base(x, y, 48, height, "scrollbar")
    {
        this.arrowUp = iconRegistry
            .Icon(VanillaIcon.ArrowUp)
            .Component(IconStyle.Transparent, "arrowUp")
            .AsBuilder()
            .Location(this.bounds.Location)
            .Value;

        this.arrowDown = iconRegistry
            .Icon(VanillaIcon.ArrowDown)
            .Component(IconStyle.Transparent, "arrowDown")
            .AsBuilder()
            .Location(new Point(x, y + height - (12 * Game1.pixelZoom)))
            .Value;

        this.runner = new Rectangle(x + 12, y + (12 * Game1.pixelZoom) + 4, 24, height - (24 * Game1.pixelZoom) - 12);
        this.grabber = iconRegistry
            .Icon(VanillaIcon.Grabber)
            .Component(IconStyle.Transparent, "grabber")
            .AsBuilder()
            .Location(this.runner.Location)
            .Value;

        this.Components.Add(this.arrowUp);
        this.Components.Add(this.arrowDown);
        this.Components.Add(this.grabber);

        this.arrowUp.Clicked += (_, _) =>
        {
            if (!(this.Value > 0))
            {
                return;
            }

            this.arrowUp.Scale = 3.5f;
            Game1.playSound("shwip");
        };

        this.arrowDown.Clicked += (_, _) =>
        {
            if (!(this.Value < 1))
            {
                return;
            }

            this.arrowDown.Scale = 3.5f;
            Game1.playSound("shwip");
        };

        this.grabber.Clicked += (_, _) => this.IsActive = true;
    }

    /// <summary>Event raised when the scrollbar is scrolled.</summary>
    public event EventHandler<ScrolledEventArgs> Scrolled
    {
        add => this.scrolled += value;
        remove => this.scrolled -= value;
    }

    /// <summary>Gets a value indicating whether the scroll bar is currently active.</summary>
    public bool IsActive { get; private set; }

    /// <inheritdoc />
    public override Point Location
    {
        get => this.Bounds.Location;
        set
        {
            base.Location = value;
            this.arrowUp.Location = value;
            this.arrowDown.Location = new Point(value.X, value.Y + this.Bounds.Height - (12 * Game1.pixelZoom));

            this.runner = new Rectangle(
                value.X + 12,
                value.Y + (12 * Game1.pixelZoom) + 4,
                24,
                this.bounds.Height - (24 * Game1.pixelZoom) - 12);

            this.grabber.Location = this.runner.Location;
        }
    }

    /// <inheritdoc />
    public override Point Size
    {
        get => this.Bounds.Size;
        set
        {
            base.Size = value;
            this.arrowDown.Location = this.arrowDown.Location with
            {
                Y = this.bounds.Height - (12 * Game1.pixelZoom),
            };

            this.runner.Size = this.runner.Size with
            {
                Y = this.bounds.Height - (24 * Game1.pixelZoom) - 12,
            };

            this.grabber.Location = this.runner.Location;
        }
    }

    /// <summary>Gets or sets the scroll value.</summary>
    public float Value
    {
        get => this.value;
        set
        {
            this.value = Math.Clamp(value, 0, 1);
            this.grabber.Offset = this.grabber.Offset with
            {
                Y = (int)((this.runner.Height - this.grabber.Bounds.Height) * this.value),
            };

            this.arrowUp.Color = this.value > 0 ? Color.White : Color.Black * 0.35f;
            this.arrowDown.Color = this.value < 1 ? Color.White : Color.Black * 0.35f;
        }
    }

    /// <inheritdoc />
    public override void Draw(SpriteBatch spriteBatch, Point cursor)
    {
        IClickableMenu.drawTextureBox(
            spriteBatch,
            Game1.mouseCursors,
            new Rectangle(403, 383, 6, 6),
            this.runner.X + this.Offset.X,
            this.runner.Y + this.Offset.Y,
            this.runner.Width,
            this.runner.Height,
            Color.White,
            Game1.pixelZoom);

        this.grabber.Draw(spriteBatch, cursor);
        this.arrowUp.Draw(spriteBatch, cursor);
        this.arrowDown.Draw(spriteBatch, cursor);
    }

    /// <inheritdoc />
    public override bool TryLeftClick(Point cursor)
    {
        if (base.TryLeftClick(cursor))
        {
            return true;
        }

        if (!this.runner.Contains(cursor))
        {
            return false;
        }

        this.Value = (float)(cursor.Y - this.runner.Y) / this.runner.Height;
        return true;
    }

    /// <inheritdoc />
    public override bool TryScroll(Point cursor, int direction)
    {
        var initialValue = this.Value;
        if (!base.TryScroll(cursor, direction) || Math.Abs(this.Value - initialValue) < 0.001f)
        {
            return false;
        }

        Game1.playSound("shiny4");
        return true;
    }

    /// <inheritdoc />
    public override void Update(Point cursor)
    {
        if (!this.IsActive)
        {
            base.Update(cursor);
            return;
        }

        this.IsActive = UiToolkit.Input.IsDown(SButton.MouseLeft) || UiToolkit.Input.IsSuppressed(SButton.MouseLeft);
        var initialValue = this.Value;
        this.Value = (float)(cursor.Y - this.runner.Y) / this.runner.Height;
        if (Math.Abs(this.Value - initialValue) < 0.001f)
        {
            Game1.playSound("shiny4");
        }
    }
}