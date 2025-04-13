IMyTextPanel lcd;
IMyTerminalBlock targetBlock;
List<IMyLightingBlock> alertLights = new List<IMyLightingBlock>();

public Program()
{
    // Get individual blocks
    lcd = GridTerminalSystem.GetBlockWithName("Asteroid - Main LCD Panel") as IMyTextPanel;
    targetBlock = GridTerminalSystem.GetBlockWithName("Asteroid - S. Reactor A");

    // Get the light group
    var lightGroup = GridTerminalSystem.GetBlockGroupWithName("Asteroid - Alert Lights");
    if (lightGroup != null)
        lightGroup.GetBlocksOfType(alertLights);

    // Echo errors if any
    if (lcd == null)
        Echo("LCD panel not found!");

    if (targetBlock == null)
        Echo("Target block not found!");

    if (lightGroup == null || alertLights.Count == 0)
        Echo("Light group not found or empty!");
}

void UpdateLCDAndLights()
{
    if (lcd == null || targetBlock == null || alertLights.Count == 0)
        return;

    // Get battery group
    var batteryGroup = GridTerminalSystem.GetBlockGroupWithName("Asteroid - Batteries");
    var batteries = new List<IMyBatteryBlock>();
    if (batteryGroup != null)
        batteryGroup.GetBlocksOfType(batteries);

    float totalBatteryCapacity = 0f, currentBatteryCharge = 0f;
    foreach (var battery in batteries)
    {
        totalBatteryCapacity += (float)battery.MaxStoredPower; // Explicit cast to float
        currentBatteryCharge += (float)battery.CurrentStoredPower; // Explicit cast to float
    }
    float batteryPercentage = totalBatteryCapacity > 0 ? (currentBatteryCharge / totalBatteryCapacity) * 100 : 0;

    // Get oxygen tank group
    var oxygenGroup = GridTerminalSystem.GetBlockGroupWithName("Asteroid - Oxygen Tanks");
    var oxygenTanks = new List<IMyGasTank>();
    if (oxygenGroup != null)
        oxygenGroup.GetBlocksOfType(oxygenTanks);

    float totalOxygenCapacity = 0f, currentOxygenLevel = 0f;
    foreach (var tank in oxygenTanks)
    {
        totalOxygenCapacity += (float)tank.Capacity; // Explicit cast to float
        currentOxygenLevel += (float)(tank.Capacity * tank.FilledRatio); // Explicit cast to float
    }
    float oxygenPercentage = totalOxygenCapacity > 0 ? (currentOxygenLevel / totalOxygenCapacity) * 100 : 0;

    // Get hydrogen tank group
    var hydrogenGroup = GridTerminalSystem.GetBlockGroupWithName("Asteroid - Hydrogen Tanks");
    var hydrogenTanks = new List<IMyGasTank>();
    if (hydrogenGroup != null)
        hydrogenGroup.GetBlocksOfType(hydrogenTanks);

    float totalHydrogenCapacity = 0f, currentHydrogenLevel = 0f;
    foreach (var tank in hydrogenTanks)
    {
        totalHydrogenCapacity += (float)tank.Capacity; // Explicit cast to float
        currentHydrogenLevel += (float)(tank.Capacity * tank.FilledRatio); // Explicit cast to float
    }
    float hydrogenPercentage = totalHydrogenCapacity > 0 ? (currentHydrogenLevel / totalHydrogenCapacity) * 100 : 0;

    // Prepare LCD output
    bool isOnline = targetBlock.IsWorking;
    string status = isOnline ? "Online" : "Offline";
    string output = $"{targetBlock.CustomName} is {status}\n";
    output += $"Battery: {batteryPercentage:F1}%\n";
    output += $"Oxygen: {oxygenPercentage:F1}%\n";
    output += $"Hydrogen: {hydrogenPercentage:F1}%";

    lcd.ContentType = ContentType.TEXT_AND_IMAGE;
    lcd.FontSize = 1.5f;
    lcd.WriteText(output, false);

    // Enable or disable all lights in the group
    foreach (var light in alertLights)
    {
        light.Enabled = isOnline;
    }
}

void ToggleAlertLights()
{
    if (alertLights.Count == 0)
        return;

    bool currentState = alertLights[0].Enabled;
    foreach (var light in alertLights)
    {
        light.Enabled = !currentState;
    }
}

void MoveItemsToStorage(string storageContainerName)
{
    // Define the list of items to move
    var itemNames = new List<string>
    {
        "Iron",
        "Nickel",
        "Cobalt",
        "Silicon",
        "Gold",
        "Platinum",
        "Uranium",
        "Silver",
        "Magnesium",
        "Stone"
    };

    // Find the storage container
    var storageContainer = GridTerminalSystem.GetBlockWithName(storageContainerName) as IMyCargoContainer;
    if (storageContainer == null)
    {
        Echo($"Storage container '{storageContainerName}' not found!");
        return;
    }

    // Get all blocks on the local grid with inventories
    var inventoryBlocks = new List<IMyTerminalBlock>();
    GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(inventoryBlocks, block => block.IsSameConstructAs(Me) && block.HasInventory);

    // Iterate through each block and check its inventory
    foreach (var block in inventoryBlocks)
    {
        var inventory = block.GetInventory(0); // Get the first inventory of the block
        if (inventory == null)
            continue;

        // Check for items in the inventory
        var items = new List<MyInventoryItem>();
        inventory.GetItems(items);

        foreach (var item in items)
        {
            // Debugging: Log item details
            Echo($"Found item: {item.Type.SubtypeId} in {block.CustomName}");

            // Check if the item is in the specified list and is not raw material
            if (itemNames.Contains(item.Type.SubtypeId) && item.Type.TypeId.ToString() != "MyObjectBuilder_Ore")
            {
                // Attempt to transfer the item to the storage container
                var storageInventory = storageContainer.GetInventory(0);
                if (storageInventory != null)
                {
                    bool success = inventory.TransferItemTo(storageInventory, item);
                    if (success)
                    {
                        Echo($"Transferred {item.Type.SubtypeId} to {storageContainerName}");
                    }
                    else
                    {
                        Echo($"Failed to transfer {item.Type.SubtypeId} to {storageContainerName}");
                    }
                }
                else
                {
                    Echo($"Storage container '{storageContainerName}' has no inventory!");
                }
            }
        }
    }
}

public void Main(string argument, UpdateType updateSource)
{
    if (string.IsNullOrWhiteSpace(argument))
    {
        UpdateLCDAndLights();
        return;
    }

    var args = argument.Split(' ');
    switch (args[0].ToLower())
    {
        case "update_status":
            UpdateLCDAndLights();
            break;
        case "toggle_alert_lights":
            ToggleAlertLights();
            break;
        case "move_items":
            string storageContainerName = "Asteroid - L. Cargo C";
            MoveItemsToStorage(storageContainerName);
            break;
        default:
            Echo($"Unknown argument: {argument}");
            break;
    }
}