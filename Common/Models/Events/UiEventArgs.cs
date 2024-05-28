#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.Models.Events;

using Microsoft.Xna.Framework;

#else
namespace StardewMods.Common.Models.Events;

using Microsoft.Xna.Framework;
#endif

/// <summary>The event arguments for a simple user interface event.</summary>
internal sealed class UiEventArgs : EventArgs
{
    /// <summary>Initializes a new instance of the <see cref="UiEventArgs" /> class.</summary>
    /// <param name="button">The button pressed.</param>
    /// <param name="cursor">The cursor position.</param>
    public UiEventArgs(SButton button, Point cursor)
    {
        this.Button = button;
        this.Cursor = cursor;
    }

    /// <summary>Gets the button pressed.</summary>
    public SButton Button { get; }

    /// <summary>Gets the cursor position.</summary>
    public Point Cursor { get; }
}