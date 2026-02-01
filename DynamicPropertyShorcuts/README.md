# Dynamic Properties Shortcuts

A dynamic property system for GameCreator 2 that lets you reference GameObjects and Characters by name using ScriptableObjects.

## Features

- Reference objects by friendly names like "Player", "Enemy1", "Boss"
- Dropdown picker shows all available shortcuts in the inspector
- Multiple lookup modes: prefab, scene name, or tag
- Runtime overrides via Set properties

## Installation

1. Copy all files to your Unity project's `Assets/Plugins` folder
2. Put `GameObjectShortcutDrawer.cs` inside an `Editor` folder

```
Assets/
└── Plugins/
    └── DynamicPropertiesShortcuts/
        ├── GameObjectShortcut.cs
        ├── GetGameObjectShortcut.cs
        ├── SetGameObjectShortcut.cs
        ├── GetCharacterShortcut.cs
        ├── SetCharacterShortcut.cs
        └── Editor/
            └── GameObjectShortcutDrawer.cs
```

## Creating Shortcuts

Right-click in Project window → **Create → Game Creator → Shortcut**

Configure the shortcut:

| Field | Description |
|-------|-------------|
| Shortcut Name | Display name shown in dropdowns (e.g., "Player") |
| Lookup Mode | How to find the target at runtime |
| Prefab | Reference to prefab (for Prefab/NetworkSpawned modes) |
| Search Value | Name or tag to search for |

### Lookup Modes

| Mode | Description |
|------|-------------|
| Prefab | Returns the prefab directly |
| SceneObjectByName | Finds object using `GameObject.Find()` |
| SceneObjectByTag | Finds object using `FindWithTag()` |

## Usage in GameCreator

In any GameObject or Character property field:

1. Click the property dropdown
2. Select **Shortcut** or **Character Shortcut**
3. Pick your shortcut from the dropdown list

## Properties

| Property | Type | Description |
|----------|------|-------------|
| GetGameObjectShortcut | Get | Returns the resolved GameObject |
| SetGameObjectShortcut | Set | Sets runtime override for the shortcut |
| GetCharacterShortcut | Get | Returns GameObject (validates Character component) |
| SetCharacterShortcut | Set | Sets runtime override (validates Character component) |

## Runtime Overrides

Shortcuts support runtime overrides. When you use a Set property, it stores a runtime reference that takes priority over the configured lookup.

```csharp
// From code if needed
myShortcut.Set(someGameObject);
myShortcut.ClearOverride();

// Global overrides by name
GameObjectShortcut.SetGlobal("Player", playerObject);
GameObjectShortcut.ClearGlobal("Player");
```

## Requirements

- Unity 2021.3+
- GameCreator 2

## License

MIT
