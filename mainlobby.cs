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

    bool isOnline = targetBlock.IsWorking;
    string status = isOnline ? "Online" : "Offline";
    string output = $"{targetBlock.CustomName} is {status}";

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

public void Main(string argument, UpdateType updateSource)
{
    if (string.IsNullOrWhiteSpace(argument))
    {
        UpdateLCDAndLights();
        return;
    }

    switch (argument.ToLower())
    {
        case "update_status":
            UpdateLCDAndLights();
            break;
        case "toggle_alert_lights":
            ToggleAlertLights();
            break;
        default:
            Echo($"Unknown argument: {argument}");
            break;
    }
}