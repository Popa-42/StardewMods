#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.UI;

using System.Collections;
using StardewMods.FauxCore.Common.UI.Menus;
using StardewValley.Menus;

#else
namespace StardewMods.Common.UI;

using System.Collections;
using StardewMods.Common.UI.Menus;
using StardewValley.Menus;
#endif

/// <inheritdoc />
internal sealed class ComponentList : IList<ClickableComponent>
{
    private readonly List<ClickableComponent> components;
    private readonly BaseMenu? menu;
    private readonly ICustomComponent? parent;

    /// <summary>Initializes a new instance of the <see cref="ComponentList" /> class.</summary>
    /// <param name="menu">The menu that the components belongs to.</param>
    public ComponentList(BaseMenu menu)
    {
        this.menu = menu;
        this.components = menu.allClickableComponents ?? [];
        menu.allClickableComponents = this.components;
    }

    /// <summary>Initializes a new instance of the <see cref="ComponentList" /> class.</summary>
    /// <param name="parent">The parent component that the components belongs to.</param>
    public ComponentList(ICustomComponent parent)
    {
        this.components = new List<ClickableComponent>();
        this.menu = parent.Menu;
        this.parent = parent;
    }

    /// <inheritdoc />
    public int Count => this.components.Count;

    /// <inheritdoc />
    public bool IsReadOnly => false;

    /// <inheritdoc />
    public ClickableComponent this[int index]
    {
        get => this.components[index];
        set
        {
            if (value is ICustomComponent customComponent)
            {
                customComponent.Menu = this.menu;
                customComponent.Parent = this.parent;
            }

            this.components[index] = value;
        }
    }

    /// <inheritdoc />
    public void Add(ClickableComponent item)
    {
        if (item is ICustomComponent customComponent)
        {
            customComponent.Menu = this.menu;
            customComponent.Parent = this.parent;
        }

        this.components.Add(item);
    }

    /// <inheritdoc />
    public void Clear() => this.components.Clear();

    /// <inheritdoc />
    public bool Contains(ClickableComponent item) => this.components.Contains(item);

    /// <inheritdoc />
    public void CopyTo(ClickableComponent[] array, int arrayIndex) => this.components.CopyTo(array, arrayIndex);

    /// <inheritdoc />
    public IEnumerator<ClickableComponent> GetEnumerator() => this.components.GetEnumerator();

    /// <inheritdoc />
    public int IndexOf(ClickableComponent item) => this.components.IndexOf(item);

    /// <inheritdoc />
    public void Insert(int index, ClickableComponent item)
    {
        if (item is ICustomComponent customComponent)
        {
            customComponent.Menu = this.menu;
            customComponent.Parent = this.parent;
        }

        this.components.Insert(index, item);
    }

    /// <inheritdoc />
    public bool Remove(ClickableComponent item) => this.components.Remove(item);

    /// <inheritdoc />
    public void RemoveAt(int index) => this.components.RemoveAt(index);

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
}