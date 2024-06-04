#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.UI.Menus;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewMods.FauxCore.Common.Helpers;
using StardewMods.FauxCore.Common.Services.Integrations.FauxCore;
using StardewMods.FauxCore.Common.UI.Components;
using StardewValley.Menus;

#else
namespace StardewMods.Common.UI.Menus;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewMods.Common.Helpers;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.UI.Components;
using StardewValley.Menus;
#endif

/// <summary>Popup menu for selecting an icon and label.</summary>
internal sealed class InputWithIcon : BaseMenu
{
    public InputWithIcon(IIconRegistry iconRegistry, IIcon icon, string label)
        : base(width: 400, height: (Game1.tileSize * 2) + 16)
    {
        var iconButton =
            icon
                .Component(IconStyle.Transparent)
                .AsBuilder()
                .Location(new Point(this.Bounds.X + 8, this.Bounds.Y + 8))
                .Value;

        var textField = new TextField(
            this.Bounds.X + Game1.tileSize,
            this.Bounds.Y + 8,
            this.Bounds.Width - Game1.tileSize - 16,
            label)
        {
            Selected = true,
        };

        var okButton = iconRegistry
            .Icon(VanillaIcon.Ok)
            .Component(IconStyle.Transparent)
            .AsBuilder()
            .Location(
                new Point(
                    this.Bounds.Center.X - (IClickableMenu.borderWidth / 2) - Game1.tileSize,
                    this.Bounds.Bottom - Game1.tileSize))
            .Value;

        var cancelButton = iconRegistry
            .Icon(VanillaIcon.Cancel)
            .Component(IconStyle.Transparent)
            .AsBuilder()
            .Location(
                new Point(this.Bounds.Center.X + (IClickableMenu.borderWidth / 2), this.Bounds.Bottom - Game1.tileSize))
            .Value;

        okButton.Clicked += (_, _) =>
        {
            this.exitThisMenuNoSound();
        };

        cancelButton.Clicked += (_, _) => this.exitThisMenuNoSound();

        this.Components.Add(iconButton);
        this.Components.Add(textField);
        this.Components.Add(okButton);
        this.Components.Add(cancelButton);
    }

    /// <inheritdoc />
    public override void receiveKeyPress(Keys key)
    {
        switch (key)
        {
            case Keys.Escape when this.readyToClose():
                this.exitThisMenuNoSound();
                return;
            case Keys.Enter when this.readyToClose():
                this.exitThisMenuNoSound();
                return;
        }
    }

    /// <inheritdoc />
    protected override void DrawUnder(SpriteBatch spriteBatch, Point cursor)
    {
        spriteBatch.Draw(
            Game1.fadeToBlackRect,
            new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height),
            Color.Black * 0.5f);

        Game1.drawDialogueBox(
            this.Bounds.X - IClickableMenu.borderWidth,
            this.Bounds.Y - IClickableMenu.spaceToClearTopBorder,
            this.Bounds.Width + (IClickableMenu.borderWidth * 2),
            this.Bounds.Height + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth,
            false,
            true,
            null,
            false,
            false);
    }
}