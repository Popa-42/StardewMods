#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.UI;

using Microsoft.Xna.Framework;
using StardewMods.FauxCore.Common.UI.Components;
using StardewMods.FauxCore.Common.UI.Menus;
using StardewValley.Menus;

#else
namespace StardewMods.Common.UI;

using Microsoft.Xna.Framework;
using StardewMods.Common.UI.Components;
using StardewMods.Common.UI.Menus;
using StardewValley.Menus;
#endif

/// <inheritdoc />
internal sealed class ComponentBuilder : IComponentBuilder
{
    private readonly HashSet<Attributes> attributes = [];
    private readonly TextureComponent component;

    private string? hoverText;
    private int id;
    private bool isVisible;
    private string? label;
    private Point location;
    private BaseMenu? menu;
    private Point offset;
    private float scale;
    private Point size;

    /// <summary>Initializes a new instance of the <see cref="ComponentBuilder" /> class.</summary>
    /// <param name="component">The component.</param>
    public ComponentBuilder(ClickableTextureComponent component) =>
        this.component = new TextureComponent(
            component.name,
            component.bounds,
            component.texture,
            component.sourceRect,
            component.baseScale);

    private enum Attributes
    {
        HoverText,
        Id,
        IsVisible,
        Label,
        Location,
        Menu,
        Offset,
        Scale,
        Size,
    }

    /// <inheritdoc />
    public TextureComponent Value
    {
        get
        {
            foreach (var attribute in this.attributes)
            {
                switch (attribute)
                {
                    case Attributes.HoverText:
                        this.component.HoverText = this.hoverText;
                        break;
                    case Attributes.Id:
                        this.component.Id = this.id;
                        break;
                    case Attributes.IsVisible:
                        this.component.IsVisible = this.isVisible;
                        break;
                    case Attributes.Label:
                        this.component.Label = this.label;
                        break;
                    case Attributes.Location:
                        this.component.Location = this.location;
                        break;
                    case Attributes.Menu:
                        this.component.Menu = this.menu;
                        break;
                    case Attributes.Offset:
                        this.component.Offset = this.offset;
                        break;
                    case Attributes.Scale:
                        this.component.BaseScale = this.scale;
                        break;
                    case Attributes.Size:
                        this.component.bounds.Size = this.size;
                        break;
                }
            }

            this.attributes.Clear();
            return this.component;
        }
    }

    /// <inheritdoc />
    public IComponentBuilder HoverText(string? value)
    {
        this.attributes.Add(Attributes.HoverText);
        this.hoverText = value;
        return this;
    }

    /// <inheritdoc />
    public IComponentBuilder Id(int value)
    {
        this.attributes.Add(Attributes.Id);
        this.id = value;
        return this;
    }

    /// <inheritdoc />
    public IComponentBuilder IsVisible(bool value)
    {
        this.attributes.Add(Attributes.IsVisible);
        this.isVisible = value;
        return this;
    }

    /// <inheritdoc />
    public IComponentBuilder Label(string? value)
    {
        this.attributes.Add(Attributes.Label);
        this.label = value;
        return this;
    }

    /// <inheritdoc />
    public IComponentBuilder Location(Point value)
    {
        this.attributes.Add(Attributes.Location);
        this.location = value;
        return this;
    }

    /// <inheritdoc />
    public IComponentBuilder Location(int x, int y) => this.Location(new Point(x, y));

    /// <inheritdoc />
    public IComponentBuilder Menu(BaseMenu? value)
    {
        this.attributes.Add(Attributes.Menu);
        this.menu = value;
        return this;
    }

    /// <inheritdoc />
    public IComponentBuilder Offset(Point value)
    {
        this.attributes.Add(Attributes.Offset);
        this.offset = value;
        return this;
    }

    /// <inheritdoc />
    public IComponentBuilder Offset(int x, int y) => this.Offset(new Point(x, y));

    /// <inheritdoc />
    public IComponentBuilder Scale(float value)
    {
        this.attributes.Add(Attributes.Scale);
        this.scale = value;
        return this;
    }

    /// <inheritdoc />
    public IComponentBuilder Size(Point value)
    {
        this.attributes.Add(Attributes.Size);
        this.size = value;
        return this;
    }

    /// <inheritdoc />
    public IComponentBuilder Size(int width, int height) => this.Size(new Point(width, height));
}