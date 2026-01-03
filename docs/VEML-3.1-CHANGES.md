# VEML 3.1 Schema Changes

## Overview

VEML schema version 3.1 introduces global shadow control for light entities through a new `shadows` attribute in the `effects-settings` element.

## New Features

### Global Shadow Control

The `shadows` attribute in `effects-settings` provides standardized control over light shadow behavior for the entire world.

#### Syntax

```xml
<effects-settings shadows="off|on|preserve">
    <!-- other effects settings -->
</effects-settings>
```

#### Values

- **`off`**: Disables shadows for all light entities in the world
- **`on`**: Enables soft shadows for all light entities in the world
- **`preserve`**: Leaves shadow settings unchanged for all lights (default behavior)

#### Example Usage

```xml
<?xml version="1.0" encoding="UTF-8"?>
<veml xmlns="http://www.fivesqd.com/schemas/veml/3.1">
    <metadata>
        <title>Scene with Global Shadows Enabled</title>
    </metadata>
    <environment>
        <effects-settings shadows="on">
            <lite-fog fogenabled="false" />
        </effects-settings>
        
        <entity id="sun" type="light">
            <transform>
                <position x="0" y="10" z="0"/>
            </transform>
            <light-properties type="directional" intensity="1.0" />
        </entity>
        
        <!-- More entities... -->
    </environment>
</veml>
```

## Backward Compatibility

VEML 3.1 maintains full backward compatibility with all previous versions (1.0 through 3.0):

- Existing VEML documents continue to work without modification
- Documents without the `shadows` attribute default to `preserve` behavior
- Automatic schema upgrade is performed when loading older VEML versions

## Implementation Details

### Schema Changes

- **Namespace**: `http://www.fivesqd.com/schemas/veml/3.1`
- **New Type**: `effectssettingsShadows` enumeration with values: `off`, `on`, `preserve`
- **Updated Type**: `effectssettings` class now includes optional `shadows` attribute

### Processing

When a VEML document is loaded with the `shadows` attribute:

1. The `ProcessEffects` method reads the shadows setting
2. The `ProcessShadowsSetting` method iterates through all entities in the world
3. For each `LightEntity`, the Unity `Light.shadows` property is set:
   - `off` → `LightShadows.None`
   - `on` → `LightShadows.Soft`
   - `preserve` → No change

### Performance Considerations

The shadow setting is applied once during world loading by iterating through all entities. For large worlds with many entities, this operation scales with the total entity count, not just light entities.

## Migration Guide

### Upgrading from VEML 3.0

To add global shadow control to existing VEML 3.0 documents:

1. Update the namespace declaration:
   ```xml
   <veml xmlns="http://www.fivesqd.com/schemas/veml/3.1">
   ```

2. Add the `shadows` attribute to `effects-settings`:
   ```xml
   <effects-settings shadows="on">
       <!-- existing effects settings -->
   </effects-settings>
   ```

### Automatic Upgrade

The WebVerse Runtime automatically upgrades VEML 3.0 documents to 3.1 when loading. No manual conversion is required.

## Use Cases

### Optimizing Performance

Disable shadows for better performance on low-end devices:

```xml
<effects-settings shadows="off">
```

### Enhanced Visual Quality

Enable shadows for all lights to improve scene realism:

```xml
<effects-settings shadows="on">
```

### Per-Light Control

Use `preserve` (or omit the attribute) to maintain individual light shadow settings defined in entity properties or scripts:

```xml
<effects-settings shadows="preserve">
```

## Related Files

- Schema Definition: `Assets/Runtime/Handlers/VEMLHandler/Schema/V3_1/VEML.cs`
- Handler Implementation: `Assets/Runtime/Handlers/VEMLHandler/Scripts/VEMLHandler.cs`
- Utilities: `Assets/Runtime/Handlers/VEMLHandler/Scripts/VEMLUtilities.cs`
- Tests: `Assets/Runtime/Handlers/VEMLHandler/Tests/VEMLHandlerTests.cs`
- Example: `docs/veml-3.1-shadows-example.veml`

## External Dependencies

The VEML XSD schema definition is maintained in the external [@Five-Squared-Interactive/VEML](https://github.com/Five-Squared-Interactive/VEML) repository and should be updated separately to reflect these changes.
