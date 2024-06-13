#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.Models.Events;
#else
namespace StardewMods.Common.Models.Events;
#endif

using Microsoft.Xna.Framework;

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

    /// <summary>Gets a value indicating whether the input has been handled.</summary>
    public bool Handled { get; private set; }

    /// <summary>Prevents further handling of the input event.</summary>
    public void PreventDefault() => this.Handled = true;
}