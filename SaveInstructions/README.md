# Instruction Preset System

This adds a **Save/Load preset system** to your GameCreator instruction lists, allowing you to persist instruction sequences as reusable Unity assets.

## Files

1. **InstructionPreset.cs** - New ScriptableObject that stores serialized instructions
2. **InstructionListTool.cs** - Updated with Save/Load buttons

## Installation

1. Add `InstructionPreset.cs` to your `Editor/VisualScripting` folder alongside your other editor scripts
2. Replace your existing `InstructionListTool.cs` with the updated version

## Usage

### Saving Presets

- **Save selected items**: Check the toggle boxes on specific instructions, then click **💾 Save**
- **Save all items**: With nothing selected, click **💾 Save** to save the entire instruction list
- A file dialog opens - choose a name and location (defaults to `Assets/GameCreator/InstructionPresets/`)
- The preset appears in your Project window as a reusable asset

### Loading Presets

- Click **📂 Load** and select a preset file
- Choose how to add the instructions:
  - **Append**: Add to the end of the current list
  - **Replace All**: Clear existing instructions and load only the preset
  - **Cancel**: Abort the operation

### Creating Presets from Project Window

You can also create empty presets directly:
- Right-click in Project → **Create → Game Creator → Visual Scripting → Instruction Preset**

## How It Works

The system serializes each instruction using:
- **Type name** (full assembly-qualified name for reliable reconstruction)
- **JSON data** (via `EditorJsonUtility` for complete state preservation)

This ensures all instruction properties, nested objects, and references are preserved.

## Customization

Change the default save folder by modifying this constant in `InstructionListTool.cs`:

```csharp
private const string DEFAULT_PRESET_FOLDER = "Assets/GameCreator/InstructionPresets";
```
