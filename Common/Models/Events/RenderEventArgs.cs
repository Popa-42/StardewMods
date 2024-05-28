#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.Models.Events;

using Microsoft.Xna.Framework.Graphics;

#else
namespace StardewMods.Common.Models.Events;

using Microsoft.Xna.Framework.Graphics;
#endif

/// <summary>The event arguments for a render event.</summary>
internal sealed class RenderEventArgs : EventArgs
{
    /// <summary>Initializes a new instance of the <see cref="RenderEventArgs" /> class.</summary>
    /// <param name="spriteBatch">The sprite batch.</param>
    public RenderEventArgs(SpriteBatch spriteBatch) => this.SpriteBatch = spriteBatch;

    /// <summary>Gets the sprite batch being rendered to.</summary>
    public SpriteBatch SpriteBatch { get; }
}