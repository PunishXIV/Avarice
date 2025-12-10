# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Avarice is a Dalamud plugin for FFXIV that provides positional feedback with pixel-perfect rear/flank tracking, range indicators, and visual overlays for melee DPS optimization.

## Build Commands

```bash
# Build the solution (requires .NET SDK and Dalamud SDK)
dotnet build Avarice.sln

# Build in Release mode
dotnet build Avarice.sln -c Release
```

The plugin uses **Dalamud.NET.Sdk/13.0.0** and targets x64. DalamudLibPath is configured to `%appdata%\XIVLauncher\addon\Hooks\dev\`.

## Architecture

### Solution Structure
- **Avarice/** - Main plugin project
- **ECommons/** - Shared utilities library (git submodule)
- **PunishLib/** - Shared library (git submodule)

### Core Components

**Entry Point (`Avarice.cs`)**
- Implements `IDalamudPlugin`
- Manages plugin lifecycle, IPC registration, and profile switching
- Registers `/avarice` command with subcommands: `draw` (toggle drawing), `debug` (toggle debug mode)
- Exposes IPC: `Avarice.CardinalDirection` and shared data: `Avarice.PositionalStatus`

**Configuration System (`Configuration/`)**
- `Config.cs` - Global plugin configuration
- `Profile.cs` - Per-profile settings (colors, thresholds, feature toggles)
- Supports multiple profiles with job-based automatic switching

**Drawing System (`Drawing/`)**
- `Canvas.cs` - ImGui overlay window that renders all visual elements
- `DrawFunctions.cs` - Actor-aware drawing primitives (cones, circles, donut slices)
- `ConvexShape.cs`, `Brush.cs` - Drawing abstractions
- All draw functions check `ShouldDraw()` gating controlled by `Profile.DrawingEnabled`

**Positional Tracking (`Positional/`)**
- `PositionalManager.cs` - Loads positional action data from remote CSV, tracks hit/miss status
- Fetches data from Google Sheets at startup
- `PositionalRecord.cs`, `PositionalAction.cs`, `PositionalParameters.cs` - Data models

**Action Monitoring (`Data/`)**
- `ActionWatching.cs` - Hooks into game action effects to track combat actions
- Uses signature-based hooks for `ProcessActionEffect` and `SendAction`
- `ComboCache.cs` - Tracks combo state

**UI (`ConfigurationWindow/`)**
- `ConfigWindow.cs` - Main config window with tabs
- Tabs: Settings, Anticipation, Profiles, Statistics, About, (Debug/Log when enabled)

### Key Patterns

- Global static access via `P` (plugin instance), `Prof` (current profile), `LP` (local player)
- Heavy use of ECommons helpers (`Svc.*` for Dalamud services)
- ImGui-based UI with `ECommons.ImGuiMethods` extensions
- Display conditions control when overlays appear (combat, duty, always)

### IPC

- `Avarice.CardinalDirection` - Returns cardinal direction relative to target
- `Avarice.ActionOverride` - Allows external plugins to override action detection
