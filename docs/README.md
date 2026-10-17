# Achievements API documentation

Developer documentation for the Achievements API. The [README](../README.md) covers what the mod is and shows screenshots; these pages cover how to build against it and how it works underneath.

## Using the API

- [Getting started](getting-started.md): referencing the API, hard vs soft dependencies, your first tab
- [Defining achievements](achievements.md): `BaseAchievement` and `CountAchievement`, rarity, Skeld Points, hidden achievements, sub-icons, art overrides
- [Tabs and categories](tabs-and-categories.md): tab discovery, mod grouping, shared categories
- [Localization](localization.md): `StringNames`, translation providers, and why achievement ids ignore display language
- [Game events](game-events.md): reset triggers, progress persistence, deferred toast timing
- [Integrating with the API](integration.md): reading achievement state, the settings entries, theming

## The player-facing side

- [Menu and toasts](menu-and-toasts.md): what players see, the input map, and the toast config

## Under the hood

- [Storage](storage.md): save file location, the encrypted format, and when data hits disk
- [Building and internals](building.md): project layout, the developer plugin, CI, notes for contributors
