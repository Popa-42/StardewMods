#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.UI.Components;

using Microsoft.Xna.Framework;
using StardewMods.FauxCore.Common.Helpers;
using StardewMods.FauxCore.Common.Services;
using StardewMods.FauxCore.Common.Services.Integrations.FauxCore;

#else
namespace StardewMods.Common.UI.Components;

using Microsoft.Xna.Framework;
using StardewMods.Common.Helpers;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FauxCore;
#endif

/// <summary>Represents a scrollbar with up/down arrows.</summary>
internal sealed class VerticalScrollBar : BaseComponent
{
    private readonly TextureComponent arrowDown;
    private readonly TextureComponent arrowUp;
    private readonly TextureComponent grabber;
    private readonly ButtonComponent runner;
    private float value;
    private EventHandler<float>? valueChanged;

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

        this.runner = new ButtonComponent(
            x + 12,
            y + (12 * Game1.pixelZoom) + 4,
            24,
            height - (24 * Game1.pixelZoom) - 12,
            "runner",
            string.Empty)
        {
            SourceRect = new Rectangle(403, 383, 6, 6),
        };

        this.grabber = iconRegistry
            .Icon(VanillaIcon.Grabber)
            .Component(IconStyle.Transparent, "grabber")
            .AsBuilder()
            .Location(this.runner.Location)
            .Value;

        this.grabber.Rendering += (_, _) => this.grabber.Offset = new Point(
            0,
            (int)((this.grabber.Bounds.Height - this.runner.Bounds.Height) * this.value));

        this.Components.Add(this.arrowUp);
        this.Components.Add(this.arrowDown);
        this.Components.Add(this.runner);
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

        this.arrowUp.Rendering += (_, _) => this.arrowUp.Color = this.value > 0 ? Color.White : Color.Black * 0.35f;

        this.arrowDown.Clicked += (_, _) =>
        {
            if (!(this.Value < 1))
            {
                return;
            }

            this.arrowDown.Scale = 3.5f;
            Game1.playSound("shwip");
        };

        this.arrowDown.Rendering += (_, _) => this.arrowDown.Color = this.value < 1 ? Color.White : Color.Black * 0.35f;

        this.runner.Clicked += (_, e) =>
        {
            this.IsActive = UiToolkit.Input.IsDown(SButton.MouseLeft)
                || UiToolkit.Input.IsSuppressed(SButton.MouseLeft);

            this.Value = (float)(e.Cursor.Y - this.runner.bounds.Y) / this.runner.bounds.Height;
            this.valueChanged?.InvokeAll(this, this.value);
        };

        this.grabber.Clicked += (_, _) => this.IsActive = true;
    }

    /// <summary>Event raised when the scroll value changes.</summary>
    public event EventHandler<float> ValueChanged
    {
        add => this.valueChanged += value;
        remove => this.valueChanged -= value;
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
            this.runner.Location = new Point(value.X + 12, value.Y + (12 * Game1.pixelZoom) + 4);
            this.runner.Size = new Point(24, this.bounds.Height - (24 * Game1.pixelZoom) - 12);
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

            this.runner.Size = new Point(24, this.bounds.Height - (24 * Game1.pixelZoom) - 12);
            this.grabber.Location = this.runner.Location;
        }
    }

    /// <summary>Gets or sets the scroll value.</summary>
    public float Value
    {
        get => this.value;
        set => this.value = Math.Clamp(value, 0, 1);
    }

    /// <summary>Attach the scroll bar to a component.</summary>
    /// <param name="component">The component to attach.</param>
    public void Attach(BaseComponent component)
    {
        component.OverflowChanged += (_, _) => this.IsVisible = !component.Overflow.Equals(Point.Zero);
        component.Scrolled += (_, _) => this.Value = (float)component.Offset.Y / component.Overflow.Y;
        this.arrowUp.Clicked += (_, _) =>
        {
            component.Offset = new Point(component.Offset.X, component.Offset.Y - 32);
            this.Value = (float)component.Offset.Y / component.Overflow.Y;
        };

        this.arrowDown.Clicked += (_, _) =>
        {
            component.Offset = new Point(component.Offset.X, component.Offset.Y + 32);
            this.Value = (float)component.Offset.Y / component.Overflow.Y;
        };

        this.ValueChanged += (_, _) =>
            component.Offset = new Point(component.Offset.X, (int)(component.Overflow.Y * this.Value));
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
        this.Value = (float)(cursor.Y - this.runner.Bounds.Y) / this.runner.Bounds.Height;
        if (Math.Abs(this.Value - initialValue) == 0)
        {
            return;
        }

        Game1.playSound("shiny4");
        this.valueChanged?.InvokeAll(this, this.value);
    }
}