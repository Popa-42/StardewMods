#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.Models.Events;

using Microsoft.Xna.Framework;

#else
namespace StardewMods.Common.Models.Events;

using Microsoft.Xna.Framework;
#endif

/// <summary>The event arguments for a cursor event.</summary>
internal sealed class CursorEventArgs : EventArgs
{
    /// <summary>Initializes a new instance of the <see cref="CursorEventArgs" /> class.</summary>
    /// <param name="cursor">The cursor position.</param>
    public CursorEventArgs(Point cursor) => this.Cursor = cursor;

    /// <summary>Gets the cursor position.</summary>
    public Point Cursor { get; }
}