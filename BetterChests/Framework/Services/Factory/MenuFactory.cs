namespace StardewMods.BetterChests.Framework.Services.Factory;

using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.UI.Menus;
using StardewMods.Common.Services.Integrations.BetterChests;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.UI.Menus;
using StardewValley.Menus;

/// <summary>Creates menu instances.</summary>
internal sealed class MenuFactory
{
    private readonly ConfigManager configManager;
    private readonly IExpressionHandler expressionHandler;
    private readonly IIconRegistry iconRegistry;

    /// <summary>Initializes a new instance of the <see cref="MenuFactory" /> class.</summary>
    /// <param name="configManager">Dependency used for managing config data.</param>
    /// <param name="expressionHandler">Dependency used for parsing expressions.</param>
    /// <param name="iconRegistry">Dependency used for registering and retrieving icons.</param>
    public MenuFactory(ConfigManager configManager, IExpressionHandler expressionHandler, IIconRegistry iconRegistry)
    {
        this.configManager = configManager;
        this.expressionHandler = expressionHandler;
        this.iconRegistry = iconRegistry;
    }

    /// <summary>Creates a menu of the given type.</summary>
    /// <param name="menuType">The menu type.</param>
    /// <param name="container">The storage container to open the menu for.</param>
    /// <returns>Returns the menu.</returns>
    public IClickableMenu CreateMenu(MenuType menuType, IStorageContainer? container = null) =>
        menuType switch
        {
            MenuType.Categorize when container is not null => this.CreateCategorizeMenu(container),
            MenuType.Config => this.CreateConfigMenu(),
            MenuType.Debug => this.CreateDebugMenu(),
            MenuType.Layout => this.CreateLayoutMenu(),
            MenuType.Sorting when container is not null => this.CreateSortingMenu(container),
            MenuType.Tabs => this.CreateTabsMenu(),
            _ => throw new ArgumentOutOfRangeException(nameof(menuType), menuType, null),
        };

    private IClickableMenu CreateCategorizeMenu(IStorageContainer container)
    {
        var menu = new CategorizeMenu(container, this.expressionHandler, this.iconRegistry);
        return menu;
    }

    private IClickableMenu CreateConfigMenu()
    {
        var menu = new BaseMenu();
        return menu;
    }

    private IClickableMenu CreateDebugMenu()
    {
        var menu = new BaseMenu();
        return menu;
    }

    private IClickableMenu CreateLayoutMenu()
    {
        var menu = new BaseMenu();
        return menu;
    }

    private IClickableMenu CreateSortingMenu(IStorageContainer container)
    {
        var menu = new SortMenu(container, this.expressionHandler, this.iconRegistry);
        return menu;
    }

    private IClickableMenu CreateTabsMenu()
    {
        var menu = new TabMenu(this.configManager, this.expressionHandler, this.iconRegistry);
        return menu;
    }
}