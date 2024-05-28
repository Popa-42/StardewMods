#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.Models.Events;
#else
namespace StardewMods.Common.Models.Events;
#endif

/// <summary>The event arguments for a scrolled event.</summary>
public class ScrolledEventArgs : EventArgs
{
    /// <summary>Initializes a new instance of the <see cref="ScrolledEventArgs" /> class.</summary>
    /// <param name="value">The scrolled value.</param>
    public ScrolledEventArgs(float value) => this.Value = value;

    /// <summary>Gets the scrolled value.</summary>
    public float Value { get; }
}