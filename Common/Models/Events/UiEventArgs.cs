#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.Models.Events;
#else
namespace StardewMods.Common.Models.Events;
#endif

using Microsoft.Xna.Framework;

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

    /// <summary>Gets a value indicating whether the input has been handled.</summary>
    public bool Handled { get; private set; }

    /// <summary>Prevents further handling of the input event.</summary>
    public void PreventDefault() => this.Handled = true;
}