using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace WarehouseAI.Core.Plugins;

public class ShippingAgentPlugin
{
    private readonly List<Order> orders = [];

    [
        KernelFunction("CreateOrder"),
        Description("Creates a new order with the specified item and quantity.")
    ]
    public string CreateOrder(
        [Description("The item to order.")] string item,
        [Description("The quantity to order.")] int quantity
    )
    {
        orders.Add(new Order(item, quantity, "Pending"));
        return $"🛒 Order created: {quantity}x {item} (Status: Pending).";
    }

    [
        KernelFunction("CheckOrderStatus"),
        Description("Checks and lists the status of all active orders.")
    ]
    public string CheckOrderStatus()
    {
        if (orders.Count == 0)
        {
            return "📦 No active orders.";
        }

        return string.Join(
            "\n",
            orders.Select(o => $"📦 Order: {o.Item} x{o.Quantity} (Status: {o.Status})")
        );
    }

    [
        KernelFunction("ShipOrder"),
        Description("Marks a pending order for the specified item as shipped.")
    ]
    public string ShipOrder([Description("The item to ship.")] string item)
    {
        var order = orders.FirstOrDefault(o => o.Item == item && o.Status == "Pending");
        if (order == null)
        {
            return $"❌ No pending orders found for {item}.";
        }

        order.Status = "Shipped";
        return $"🚚 Order {item} is now marked as shipped!";
    }

    private class Order(string item, int quantity, string status)
    {
        public string Item { get; } = item;
        public int Quantity { get; } = quantity;
        public string Status { get; set; } = status;
    }
}