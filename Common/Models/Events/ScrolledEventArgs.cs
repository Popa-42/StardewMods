#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.Models.Events;

using Microsoft.Xna.Framework;

#else
namespace StardewMods.Common.Models.Events;

using Microsoft.Xna.Framework;
#endif

/// <summary>The event arguments for a scrolled event.</summary>
internal sealed class ScrolledEventArgs : EventArgs
{
    /// <summary>Initializes a new instance of the <see cref="ScrolledEventArgs" /> class.</summary>
    /// <param name="cursor">The cursor position.</param>
    /// <param name="direction">The scroll direction.</param>
    public ScrolledEventArgs(Point cursor, int direction)
    {
        this.Cursor = cursor;
        this.Direction = direction;
    }

    /// <summary>Gets the cursor position.</summary>
    public Point Cursor { get; }

    /// <summary>Gets the scroll direction.</summary>
    public float Direction { get; }
}