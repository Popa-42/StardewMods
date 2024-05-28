#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.UI;

using Microsoft.Xna.Framework;
using StardewMods.FauxCore.Common.UI.Components;
using StardewMods.FauxCore.Common.UI.Menus;

#else
namespace StardewMods.Common.UI;

using Microsoft.Xna.Framework;
using StardewMods.Common.UI.Components;
using StardewMods.Common.UI.Menus;
#endif

/// <summary>Represents a custom component builder.</summary>
internal interface IComponentBuilder
{
    /// <summary>Gets the component.</summary>
    TextureComponent Value { get; }

    /// <summary>Sets the component hover text.</summary>
    /// <param name="hoverText">The component hover text.</param>
    /// <returns>Returns the component.</returns>
    IComponentBuilder HoverText(string? hoverText);

    /// <summary>Sets the component id.</summary>
    /// <param name="myId">The component id.</param>
    /// <returns>Returns the component.</returns>
    IComponentBuilder Id(int myId);

    /// <summary>Sets the component visibility.</summary>
    /// <param name="isVisible">The component visibility.</param>
    /// <returns>Returns the component.</returns>
    IComponentBuilder IsVisible(bool isVisible);

    /// <summary>Sets the component label.</summary>
    /// <param name="label">The component label.</param>
    /// <returns>Returns the component.</returns>
    IComponentBuilder Label(string? label);

    /// <summary>Sets the component location.</summary>
    /// <param name="location">The component location.</param>
    /// <returns>Returns the component.</returns>
    IComponentBuilder Location(Point location);

    /// <summary>Sets the component location.</summary>
    /// <param name="x">The component x-location.</param>
    /// <param name="y">The component y-location.</param>
    /// <returns>Returns the component.</returns>
    IComponentBuilder Location(int x, int y);

    /// <summary>Sets the component menu.</summary>
    /// <param name="menu">The component menu.</param>
    /// <returns>Returns the component.</returns>
    IComponentBuilder Menu(BaseMenu? menu);

    /// <summary>Sets the component offset.</summary>
    /// <param name="offset">The component offset.</param>
    /// <returns>Returns the component.</returns>
    IComponentBuilder Offset(Point offset);

    /// <summary>Sets the component offset.</summary>
    /// <param name="x">The component x-offset.</param>
    /// <param name="y">The component y-offset.</param>
    /// <returns>Returns the component.</returns>
    IComponentBuilder Offset(int x, int y);

    /// <summary>Sets the component scale.</summary>
    /// <param name="scale">The component scale.</param>
    /// <returns>Returns the component.</returns>
    IComponentBuilder Scale(float scale);

    /// <summary>Sets the component size.</summary>
    /// <param name="size">The component size.</param>
    /// <returns>Returns the component.</returns>
    IComponentBuilder Size(Point size);

    /// <summary>Sets the component size.</summary>
    /// <param name="width">The component width.</param>
    /// <param name="height">The component height.</param>
    /// <returns>Returns the component.</returns>
    IComponentBuilder Size(int width, int height);
}