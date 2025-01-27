namespace StardewMods.ToolbarIcons.Framework.Services.Integrations.Modded;

using StardewMods.ToolbarIcons.Framework.Enums;
using StardewMods.ToolbarIcons.Framework.Interfaces;

/// <inheritdoc />
internal sealed class StardewAquarium : IMethodIntegration
{
    /// <inheritdoc />
    public object?[] Arguments => ["aquariumprogress", Array.Empty<string>()];

    /// <inheritdoc />
    public string HoverText => I18n.Button_StardewAquarium();

    /// <inheritdoc />
    public string Icon => InternalIcon.StardewAquarium.ToStringFast();

    /// <inheritdoc />
    public string MethodName => "OpenAquariumCollectionMenu";

    /// <inheritdoc />
    public string ModId => "Cherry.StardewAquarium";
}