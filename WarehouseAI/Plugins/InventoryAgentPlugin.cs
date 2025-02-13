using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace WarehouseAI.Plugins;

public class InventoryAgentPlugin
{
    private readonly Dictionary<string, int> inventory = new(StringComparer.OrdinalIgnoreCase)
    {
        { "ItemA", 100 },
        { "ItemB", 50 },
        { "ItemC", 200 },
    };

    [
        KernelFunction("CheckStock"),
        Description("Checks the current stock level for a specified item in the inventory.")
    ]
    public string CheckStock([Description("The item to check stock for.")] string item)
    {
        if (inventory.TryGetValue(item, out int quantity))
        {
            return $"📦 {item} stock: {quantity} units available.";
        }

        return $"❌ Item '{item}' not found in inventory.";
    }

    [
        KernelFunction("UpdateStock"),
        Description(
            "Updates the stock level for a specified item, adding or removing units based on the given amount."
        )
    ]
    public string UpdateStock(
        [Description("The item to update.")] string item,
        [Description("The amount to adjust (positive to add, negative to remove).")] int amount
    )
    {
        if (!inventory.TryGetValue(item, out int value))
        {
            return $"❌ Cannot update: Item '{item}' not found.";
        }

        inventory[item] = Math.Max(0, value + amount);
        return $"✅ Updated stock: {item} now has {inventory[item]} units.";
    }
}