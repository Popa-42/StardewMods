#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.Services.Integrations.FauxCore;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;
#else
namespace StardewMods.Common.Services.Integrations.FauxCore;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;
#endif

/// <summary>Represents an icon on a sprite sheet.</summary>
public interface IIcon
{
    /// <summary>Gets the icon source area.</summary>
    public Rectangle Area { get; }

    /// <summary>Gets the icon id.</summary>
    public string Id { get; }

    /// <summary>Gets the icon texture path.</summary>
    public string Path { get; }

    /// <summary>Gets the id of the mod this icon is loaded from.</summary>
    public string Source { get; }

    /// <summary>Gets the unique identifier for this icon.</summary>
    public string UniqueId { get; }

    /// <summary>Gets the icon texture.</summary>
    /// <param name="style">The style of the icon.</param>
    /// <returns>Returns the texture.</returns>
    public Texture2D Texture(IconStyle style);

    /// <summary>Gets a component with the icon.</summary>
    /// <param name="style">The component style.</param>
    /// <param name="name">The name.</param>
    /// <param name="scale">The target component scale.</param>
    /// <returns>Returns a new button.</returns>
    public ClickableTextureComponent Component(IconStyle style, string? name = null, float scale = Game1.pixelZoom);
}