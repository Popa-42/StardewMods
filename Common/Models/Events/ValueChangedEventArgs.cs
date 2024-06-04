#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.Models.Events;
#else
namespace StardewMods.Common.Models.Events;
#endif

/// <summary>Represents the event arguments for a value changes.</summary>
/// <typeparam name="TValue">The value type.</typeparam>
internal sealed class ValueChangedEventArgs<TValue> : EventArgs
{
    /// <summary>Initializes a new instance of the <see cref="ValueChangedEventArgs{TValue}" /> class.</summary>
    /// <param name="oldValue">The old value.</param>
    /// <param name="newValue">The new value.</param>
    public ValueChangedEventArgs(TValue oldValue, TValue newValue)
    {
        this.OldValue = oldValue;
        this.NewValue = newValue;
    }

    /// <summary>Gets the new value.</summary>
    public TValue NewValue { get; }

    /// <summary>Gets the old value.</summary>
    public TValue OldValue { get; }
}