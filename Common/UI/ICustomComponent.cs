#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.UI;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.FauxCore.Common.Models.Events;
using StardewMods.FauxCore.Common.UI.Menus;

#else
namespace StardewMods.Common.UI;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Models.Events;
using StardewMods.Common.UI.Menus;
#endif

/// <summary>Represents a custom component.</summary>
internal interface ICustomComponent
{
    /// <summary>Event raised when the component is clicked.</summary>
    event EventHandler<UiEventArgs> Clicked;

    /// <summary>Event raised when the cursor moves out of the component.</summary>
    event EventHandler<CursorEventArgs> CursorOut;

    /// <summary>Event raised when the cursor moves over the component.</summary>
    event EventHandler<CursorEventArgs> CursorOver;

    /// <summary>Event raised before the component is drawn.</summary>
    event EventHandler<RenderEventArgs> Rendering;

    /// <summary>Event raised when scrolling over a component.</summary>
    event EventHandler<ScrolledEventArgs> Scrolled;

    /// <summary>Gets the component bounds.</summary>
    Rectangle Bounds { get; }

    /// <summary>Gets the child components.</summary>
    ComponentList Components { get; }

    /// <summary>Gets the component frame.</summary>
    Rectangle Frame { get; }

    /// <summary>Gets or sets the component base scale.</summary>
    float BaseScale { get; set; }

    /// <summary>Gets or sets the color.</summary>
    Color Color { get; set; }

    /// <summary>Gets or sets the hover text.</summary>
    string? HoverText { get; set; }

    /// <summary>Gets or sets the component id.</summary>
    int Id { get; set; }

    /// <summary>Gets or sets a value indicating whether the component is visible.</summary>
    bool IsVisible { get; set; }

    /// <summary>Gets or sets the component label.</summary>
    string? Label { get; set; }

    /// <summary>Gets or sets the component location.</summary>
    Point Location { get; set; }

    /// <summary>Gets or sets the parent menu.</summary>
    BaseMenu? Menu { get; set; }

    /// <summary>Gets or sets the component name.</summary>
    string Name { get; set; }

    /// <summary>Gets or sets the offset.</summary>
    Point Offset { get; set; }

    /// <summary>Gets or sets the parent component.</summary>
    ICustomComponent? Parent { get; set; }

    /// <summary>Gets or sets the component scale.</summary>
    float Scale { get; set; }

    /// <summary>Gets or sets the component size.</summary>
    Point Size { get; set; }

    /// <summary>Draws the component.</summary>
    /// <param name="spriteBatch">The sprite batch to draw the component to.</param>
    /// <param name="cursor">The mouse position.</param>
    void Draw(SpriteBatch spriteBatch, Point cursor);

    /// <summary>Draw over the component.</summary>
    /// <param name="spriteBatch">The sprite batch to draw to.</param>
    /// <param name="cursor">The mouse position.</param>
    void DrawOver(SpriteBatch spriteBatch, Point cursor);

    /// <summary>Draw under the component.</summary>
    /// <param name="spriteBatch">The sprite batch to draw to.</param>
    /// <param name="cursor">The mouse position.</param>
    void DrawUnder(SpriteBatch spriteBatch, Point cursor);

    /// <summary>Try to perform a left-click.</summary>
    /// <param name="cursor">The mouse position.</param>
    /// <returns><c>true</c> if the click was handled; otherwise, <c>false</c>.</returns>
    bool TryLeftClick(Point cursor);

    /// <summary>Try to perform a right-click.</summary>
    /// <param name="cursor">The mouse position.</param>
    /// <returns><c>true</c> if the click was handled; otherwise, <c>false</c>.</returns>
    bool TryRightClick(Point cursor);

    /// <summary>Try to perform a scroll.</summary>
    /// <param name="cursor">The mouse position.</param>
    /// <param name="direction">The scroll direction.</param>
    /// <returns><c>true</c> if the scroll was handled; otherwise, <c>false</c>.</returns>
    bool TryScroll(Point cursor, int direction);

    /// <summary>Performs an update.</summary>
    /// <param name="cursor">The mouse position.</param>
    void Update(Point cursor);
}