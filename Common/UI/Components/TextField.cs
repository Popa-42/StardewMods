#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.UI.Components;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.FauxCore.Common.Helpers;
using StardewValley.Menus;

#else
namespace StardewMods.Common.UI.Components;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Helpers;
using StardewValley.Menus;
#endif

/// <summary>Represents a search overlay control that allows the user to input text.</summary>
internal sealed class TextField : BaseComponent
{
    private const int CountdownTimer = 20;

    private readonly TextBox textBox;

    private string previousText;
    private int timeout;
    private EventHandler<string>? valueChanged;

    /// <summary>Initializes a new instance of the <see cref="TextField" /> class.</summary>
    /// <param name="x">The text field x-coordinate.</param>
    /// <param name="y">The text field y-coordinate.</param>
    /// <param name="width">The text field width.</param>
    /// <param name="initialValue">The initial textbox value.</param>
    /// <param name="name">The text field name.</param>
    public TextField(int x, int y, int width, string? initialValue = null, string name = "TextField")
        : base(x, y, width, 48, name)
    {
        this.previousText = initialValue ?? string.Empty;
        this.textBox =
            new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
            {
                X = this.Bounds.X,
                Y = this.Bounds.Y,
                Width = this.Bounds.Width,
                limitWidth = false,
                Text = this.previousText,
            };
    }

    /// <summary>Event raised when the text value changes.</summary>
    public event EventHandler<string> ValueChanged
    {
        add => this.valueChanged += value;
        remove => this.valueChanged -= value;
    }

    /// <inheritdoc />
    public override Point Location
    {
        get => base.Location;
        set
        {
            base.Location = value;
            this.textBox.X = value.X;
            this.textBox.Y = value.Y;
        }
    }

    /// <summary>Gets or sets a value indicating whether the search bar is currently selected.</summary>
    public bool Selected
    {
        get => this.textBox.Selected;
        set => this.textBox.Selected = value;
    }

    /// <inheritdoc />
    public override Point Size
    {
        get => base.Size;
        set
        {
            base.Size = value;
            this.textBox.Width = value.X;
        }
    }

    /// <summary>Gets or sets the textbox value.</summary>
    public string Value
    {
        get => this.textBox.Text;
        set
        {
            this.textBox.Text = value;
            this.previousText = value;
        }
    }

    /// <inheritdoc />
    public override void Draw(SpriteBatch spriteBatch, Point cursor)
    {
        this.textBox.X = this.Bounds.X + this.Offset.X;
        this.textBox.Y = this.Bounds.Y + this.Offset.Y;
        this.textBox.Draw(spriteBatch, false);
    }

    /// <inheritdoc />
    public override bool TryLeftClick(Point cursor)
    {
        this.Selected = this.bounds.Contains(cursor);
        return this.Selected;
    }

    /// <inheritdoc />
    public override bool TryRightClick(Point cursor)
    {
        if (!this.bounds.Contains(cursor))
        {
            this.Selected = false;
            return false;
        }

        this.Selected = true;
        this.Value = string.Empty;
        this.valueChanged?.InvokeAll(this, this.Value);
        return this.Selected;
    }

    /// <inheritdoc />
    public override void Update(Point cursor)
    {
        this.textBox.Hover(cursor.X, cursor.Y);

        // Initiate a text change event
        if (this.previousText != this.Value)
        {
            this.previousText = this.Value;
            this.timeout = TextField.CountdownTimer;
            return;
        }

        // Start countdown for debounce
        if (this.timeout == 0 || --this.timeout != 0)
        {
            return;
        }

        // Raise the text change event
        this.valueChanged?.InvokeAll(this, this.Value);
    }
}