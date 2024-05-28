namespace StardewMods.FauxCore.Framework.Models;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.FauxCore.Common.Services.Integrations.FauxCore;
using StardewValley.Menus;

/// <inheritdoc />
internal sealed class Icon : IIcon
{
    private readonly GetComponent getComponent;
    private readonly GetTexture getTexture;

    /// <summary>Initializes a new instance of the <see cref="Icon" /> class.</summary>
    /// <param name="getTexture">A function that returns the button texture.</param>
    /// <param name="getComponent">A function that return a new button.</param>
    public Icon(GetTexture getTexture, GetComponent getComponent)
    {
        this.getTexture = getTexture;
        this.getComponent = getComponent;
    }

    /// <summary>Gets a component from the icon.</summary>
    /// <param name="icon">The icon.</param>
    /// <param name="style">The icon style.</param>
    /// <param name="name">The component name.</param>
    /// <param name="scale">The component scale.</param>
    /// <returns>Returns the icon component.</returns>
    public delegate ClickableTextureComponent GetComponent(IIcon icon, IconStyle style, string? name, float scale);

    /// <summary>Gets a texture from the icon.</summary>
    /// <param name="icon">The icon.</param>
    /// <param name="style">The icon style.</param>
    /// <returns>Returns the icon texture.</returns>
    public delegate Texture2D GetTexture(IIcon icon, IconStyle style);

    /// <inheritdoc />
    public string UniqueId => $"{this.Source}/{this.Id}";

    /// <inheritdoc />
    public Rectangle Area { get; set; } = Rectangle.Empty;

    /// <inheritdoc />
    public string Id { get; set; } = string.Empty;

    /// <inheritdoc />
    public string Path { get; set; } = string.Empty;

    /// <inheritdoc />
    public string Source { get; set; } = string.Empty;

    /// <inheritdoc />
    public ClickableTextureComponent Component(IconStyle style, string? name = null, float scale = Game1.pixelZoom) =>
        this.getComponent(this, style, name, scale);

    /// <inheritdoc />
    public Texture2D Texture(IconStyle style) => this.getTexture(this, style);
}