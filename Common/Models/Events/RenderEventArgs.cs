#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.Models.Events;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#else
namespace StardewMods.Common.Models.Events;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endif

/// <summary>The event arguments for a render event.</summary>
internal sealed class RenderEventArgs : EventArgs
{
    /// <summary>Initializes a new instance of the <see cref="RenderEventArgs" /> class.</summary>
    /// <param name="spriteBatch">The sprite batch.</param>
    /// <param name="cursor">The cursor position.</param>
    public RenderEventArgs(SpriteBatch spriteBatch, Point cursor)
    {
        this.Cursor = cursor;
        this.SpriteBatch = spriteBatch;
    }

    /// <summary>Gets the cursor position.</summary>
    public Point Cursor { get; }

    /// <summary>Gets the sprite batch being rendered to.</summary>
    public SpriteBatch SpriteBatch { get; }
}