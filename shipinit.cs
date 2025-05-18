// This script renames all terminal blocks on the current grid (local subgrid only)
// by prepending a user-defined prefix to their CustomName.
// It iterates through all blocks, filters for those on the same grid as the
// Programmable Block, and then updates their names if the prefix isn't already present.

// Define the prefix you want to add to component names.
string shipPrefix = "GobCursor - ";

public Program()
{
    // The constructor, called only once every session and
    // always before any other method is called. You can use it to
    // initialize your script. 
    // Runtime.UpdateFrequency = UpdateFrequency.None; // No continuous updates needed
}

public void Save()
{
    // Called when the program needs to save its state. Use
    // this method to save your state to the Storage field or
    // some other means.
}

public void Main(string argument, UpdateType updateSource)
{
    // This is the main entry point of the script.
    // It's called when the script is run from the terminal, a timer block, or sensor.

    // Get the grid the Programmable Block is on.
    IMyCubeGrid currentGrid = Me.CubeGrid;

    // Create a list to hold all terminal blocks on the current grid.
    List<IMyTerminalBlock> localGridBlocks = new List<IMyTerminalBlock>();

    // Get all blocks and filter them to include only those on the current grid.
    GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(localGridBlocks, block => block.CubeGrid == currentGrid);

    int blocksRenamed = 0;

    Echo("Starting ship component renaming process...");
    Echo("Prefix to use: " + shipPrefix);
    Echo("Found " + localGridBlocks.Count + " blocks on the local grid.");

    foreach (IMyTerminalBlock block in localGridBlocks)
    {
        // Check if the block's name already starts with the prefix.
        // This prevents adding the prefix multiple times if the script is run again.
        if (!block.CustomName.StartsWith(shipPrefix))
        {
            string oldName = block.CustomName;
            block.CustomName = shipPrefix + oldName;
            Echo($"Renamed: '{oldName}' to '{block.CustomName}'");
            blocksRenamed++;
        }
        else
        {
            // Optional: Log blocks that already have the prefix
            // Echo($"Skipped (already prefixed): '{block.CustomName}'");
        }
    }

    Echo($"Renaming process complete. {blocksRenamed} blocks were renamed.");
    Echo($"{localGridBlocks.Count - blocksRenamed} blocks already had the prefix or were not renamed.");
}
