#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.Enums;
#else
namespace StardewMods.Common.Enums;
#endif

/// <summary>Represents the type of menu partition.</summary>
public enum PartitionType
{
    /// <summary>A horizontal partition.</summary>
    Horizontal,

    /// <summary>A vertical partition.</summary>
    Vertical,

    /// <summary>A vertical intersecting partition.</summary>
    VerticalIntersecting,

    /// <summary>A vertical upper intersecting partition.</summary>
    VerticalUpperIntersecting,
}