namespace StardewMods.FauxCore.Framework.Models;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.FauxCore.Common.Services.Integrations.FauxCore;
using StardewValley.Menus;

/// <inheritdoc />
internal sealed class IconWrapper : IIcon
{
    private readonly Lazy<IIcon> icon;

    /// <summary>Initializes a new instance of the <see cref="IconWrapper" /> class.</summary>
    /// <param name="getIcon">A method for retrieving the icon.</param>
    public IconWrapper(Func<IIcon> getIcon) => this.icon = new Lazy<IIcon>(getIcon);

    /// <inheritdoc />
    public Rectangle Area => this.icon.Value.Area;

    /// <inheritdoc />
    public string Id => this.icon.Value.Id;

    /// <inheritdoc />
    public string Path => this.icon.Value.Path;

    /// <inheritdoc />
    public string Source => this.icon.Value.Source;

    /// <inheritdoc />
    public string UniqueId => this.icon.Value.UniqueId;

    /// <inheritdoc />
    public ClickableTextureComponent Component(IconStyle style, string? name = null, float scale = Game1.pixelZoom) =>
        this.icon.Value.Component(style, name, scale);

    /// <inheritdoc />
    public Texture2D Texture(IconStyle style) => this.icon.Value.Texture(style);
}