namespace StardewMods.ToolbarIcons.Framework.UI;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Helpers;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.Services.Integrations.GenericModConfigMenu;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;

/// <summary>Represents a complex menu option for arranging toolbar icons.</summary>
internal sealed class ToolbarIconOption : BaseComplexOption
{
    private static string? hoverText;

    private readonly Func<string> getCurrentId;
    private readonly Func<bool> getEnabled;
    private readonly Func<string> getTooltip;
    private readonly IIconRegistry iconRegistry;
    private readonly IInputHelper inputHelper;
    private readonly Action? moveDown;
    private readonly Action? moveUp;
    private readonly Action<bool> setEnabled;
    private ClickableTextureComponent? checkedComponent;

    private string currentId;
    private ClickableTextureComponent? downArrow;
    private ClickableTextureComponent icon;
    private string name;
    private ClickableTextureComponent? uncheckedComponent;
    private ClickableTextureComponent? upArrow;

    /// <summary>Initializes a new instance of the <see cref="ToolbarIconOption" /> class.</summary>
    /// <param name="iconRegistry">Dependency used for registering and retrieving icons.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="getCurrentId">Function which returns the current id.</param>
    /// <param name="getTooltip">Function which returns the tooltip.</param>
    /// <param name="getEnabled">Function which returns if icon is enabled.</param>
    /// <param name="setEnabled">Action which sets if the icon is enabled.</param>
    /// <param name="moveDown">Action to perform when down is pressed.</param>
    /// <param name="moveUp">Action to perform when up is pressed.</param>
    public ToolbarIconOption(
        IIconRegistry iconRegistry,
        IInputHelper inputHelper,
        Func<string> getCurrentId,
        Func<string> getTooltip,
        Func<bool> getEnabled,
        Action<bool> setEnabled,
        Action? moveDown,
        Action? moveUp)
    {
        this.Height = Game1.tileSize;
        this.iconRegistry = iconRegistry;
        this.inputHelper = inputHelper;
        this.getCurrentId = getCurrentId;
        this.getTooltip = getTooltip;
        this.getEnabled = getEnabled;
        this.setEnabled = setEnabled;
        this.moveDown = moveDown;
        this.moveUp = moveUp;
        this.UpdateId();
    }

    /// <inheritdoc />
    public override string Name => string.Empty;

    /// <inheritdoc />
    public override string Tooltip => string.Empty;

    /// <inheritdoc />
    public override int Height { get; protected set; }

    private ClickableTextureComponent CheckedIcon =>
        this.checkedComponent ??=
            this
                .iconRegistry.Icon(VanillaIcon.Checked)
                .Component(IconStyle.Transparent)
                .AsBuilder()
                .HoverText(I18n.Config_CheckBox_Tooltip())
                .Value;

    private ClickableTextureComponent DownArrow =>
        this.downArrow ??=
            this
                .iconRegistry.Icon(VanillaIcon.ArrowDown)
                .Component(IconStyle.Transparent)
                .AsBuilder()
                .HoverText(I18n.Config_MoveDown_Tooltip())
                .Value;

    private ClickableTextureComponent UncheckedIcon =>
        this.uncheckedComponent ??=
            this
                .iconRegistry.Icon(VanillaIcon.Unchecked)
                .Component(IconStyle.Transparent)
                .AsBuilder()
                .HoverText(I18n.Config_CheckBox_Tooltip())
                .Value;

    private ClickableTextureComponent UpArrow =>
        this.upArrow ??=
            this
                .iconRegistry.Icon(VanillaIcon.ArrowUp)
                .Component(IconStyle.Transparent)
                .AsBuilder()
                .HoverText(I18n.Config_MoveUp_Tooltip())
                .Value;

    private bool Enabled
    {
        get => this.getEnabled();
        set => this.setEnabled(value);
    }

    /// <inheritdoc />
    public override void Draw(SpriteBatch spriteBatch, Vector2 pos)
    {
        var cursor = UiToolkit.Cursor;
        var mouseLeft = this.inputHelper.GetState(SButton.MouseLeft) == SButtonState.Pressed;
        var mouseRight = this.inputHelper.GetState(SButton.MouseRight) == SButtonState.Pressed;
        var hoverY = cursor.Y >= pos.Y && cursor.Y < pos.Y + this.Height;
        var clicked = (mouseLeft || mouseRight) && hoverY;

        if (this.currentId != this.getCurrentId())
        {
            this.UpdateId();
        }

        Utility.drawTextWithShadow(
            spriteBatch,
            this.icon.hoverText,
            Game1.dialogueFont,
            pos - new Vector2(540, 0),
            SpriteText.color_Gray);

        // Checkbox
        var checkbox = this.Enabled ? this.CheckedIcon : this.UncheckedIcon;
        checkbox.bounds.Location = new Point((int)pos.X + Game1.tileSize, (int)pos.Y);
        checkbox.tryHover(cursor.X, cursor.Y);
        checkbox.draw(spriteBatch);

        if (checkbox.bounds.Contains(cursor))
        {
            ToolbarIconOption.hoverText = checkbox.hoverText;
            if (clicked)
            {
                checkbox.scale = 3.5f;
                this.Enabled = !this.Enabled;
                Game1.playSound("drumkit6");
            }
        }
        else if ((cursor.Y < checkbox.bounds.Left || cursor.X > checkbox.bounds.Right)
            && ToolbarIconOption.hoverText == checkbox.hoverText)
        {
            ToolbarIconOption.hoverText = null;
        }

        // Up Arrow
        this.UpArrow.bounds.Location = new Point((int)pos.X + (Game1.tileSize * 2), (int)pos.Y);
        if (this.moveUp is not null && this.UpArrow.bounds.Contains(cursor))
        {
            this.UpArrow.tryHover(cursor.X, cursor.Y);
            ToolbarIconOption.hoverText = this.UpArrow.hoverText;
            if (clicked)
            {
                this.UpArrow.scale = 3.5f;
                this.moveUp();
                Game1.playSound("shwip");
            }
        }
        else if ((cursor.X < this.UpArrow.bounds.Left || cursor.X > this.UpArrow.bounds.Right)
            && ToolbarIconOption.hoverText == this.UpArrow.hoverText)
        {
            ToolbarIconOption.hoverText = null;
        }

        this.UpArrow.draw(spriteBatch, this.moveUp is not null ? Color.White : Color.Black * 0.35f, 1f);

        // Icon
        this.icon.bounds.Location = new Point((int)pos.X + (Game1.tileSize * 3), (int)pos.Y);
        if (this.Enabled && this.icon.bounds.Contains(cursor))
        {
            this.icon.tryHover(cursor.X, cursor.Y);
            ToolbarIconOption.hoverText = this.name;
        }
        else if (ToolbarIconOption.hoverText == this.name)
        {
            ToolbarIconOption.hoverText = null;
        }

        this.icon.draw(spriteBatch, this.Enabled ? Color.White : Color.Black * 0.35f, 1f);

        // Down Arrow
        this.DownArrow.bounds.Location = new Point((int)pos.X + (Game1.tileSize * 4), (int)pos.Y);
        if (this.moveDown is not null && this.DownArrow.bounds.Contains(cursor))
        {
            this.DownArrow.tryHover(cursor.X, cursor.Y);
            ToolbarIconOption.hoverText = this.DownArrow.hoverText;
            if (clicked)
            {
                this.DownArrow.scale = 3.5f;
                this.moveDown();
                Game1.playSound("shwip");
            }
        }
        else if ((cursor.X < this.DownArrow.bounds.Left || cursor.X > this.DownArrow.bounds.Right)
            && ToolbarIconOption.hoverText == this.DownArrow.hoverText)
        {
            ToolbarIconOption.hoverText = null;
        }

        this.DownArrow.draw(spriteBatch, this.moveDown is not null ? Color.White : Color.Black * 0.35f, 1f);
        if (!string.IsNullOrWhiteSpace(ToolbarIconOption.hoverText))
        {
            IClickableMenu.drawToolTip(spriteBatch, ToolbarIconOption.hoverText, null, null);
        }
    }

    [MemberNotNull(nameof(ToolbarIconOption.currentId), nameof(ToolbarIconOption.name), nameof(ToolbarIconOption.icon))]
    private void UpdateId()
    {
        this.currentId = this.getCurrentId();
        this.name = this.currentId.Split('/')[^1];
        this.icon = this
            .iconRegistry.Icon(this.currentId)
            .Component(IconStyle.Button, scale: 3f)
            .AsBuilder()
            .HoverText(this.getTooltip())
            .Value;
    }
}