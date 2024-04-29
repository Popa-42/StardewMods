namespace StardewMods.BetterChests.Framework.Models.Containers;

using Microsoft.Xna.Framework;
using StardewMods.Common.Services.Integrations.BetterChests.Interfaces;
using StardewValley.Inventories;
using StardewValley.Menus;
using StardewValley.Mods;
using StardewValley.Network;
using StardewValley.Objects;

/// <inheritdoc />
internal sealed class NpcContainer : BaseContainer<NPC>
{
    private readonly Chest chest;

    /// <summary>Initializes a new instance of the <see cref="NpcContainer" /> class.</summary>
    /// <param name="baseOptions">The type of storage object.</param>
    /// <param name="npc">The npc to which the storage is connected.</param>
    /// <param name="chest">The chest storage of the container.</param>
    public NpcContainer(IStorageOptions baseOptions, NPC npc, Chest chest)
        : base(baseOptions)
    {
        this.Source = new WeakReference<NPC>(npc);
        this.chest = chest;
    }

    /// <summary>Gets the source NPC of the container.</summary>
    /// <exception cref="ObjectDisposedException">Thrown when the NPC is disposed.</exception>
    public NPC Npc =>
        this.Source.TryGetTarget(out var target) ? target : throw new ObjectDisposedException(nameof(NpcContainer));

    /// <inheritdoc />
    public override int Capacity => this.chest.GetActualCapacity();

    /// <inheritdoc />
    public override IInventory Items => this.chest.GetItemsForPlayer();

    /// <inheritdoc />
    public override GameLocation Location => this.Npc.currentLocation;

    /// <inheritdoc />
    public override Vector2 TileLocation => this.Npc.Tile;

    /// <inheritdoc />
    public override ModDataDictionary ModData => this.Npc.modData;

    /// <inheritdoc />
    public override NetMutex? Mutex => this.chest.GetMutex();

    /// <inheritdoc />
    public override bool IsAlive => this.Source.TryGetTarget(out _);

    /// <inheritdoc />
    public override WeakReference<NPC> Source { get; }

    /// <inheritdoc />
    public override void ShowMenu(bool playSound = false) =>
        Game1.activeClickableMenu = new ItemGrabMenu(
            this.Items,
            false,
            true,
            InventoryMenu.highlightAllItems,
            this.GrabItemFromInventory,
            null,
            this.GrabItemFromChest,
            false,
            true,
            true,
            true,
            true,
            1,
            this.chest,
            -1,
            this.chest);

    /// <inheritdoc />
    public override bool TryAdd(Item item, out Item? remaining)
    {
        var stack = item.Stack;
        remaining = this.chest.addItem(item);
        return remaining is null || remaining.Stack != stack;
    }

    /// <inheritdoc />
    public override bool TryRemove(Item item)
    {
        if (!this.Items.Contains(item))
        {
            return false;
        }

        this.Items.Remove(item);
        this.Items.RemoveEmptySlots();
        return true;
    }
}