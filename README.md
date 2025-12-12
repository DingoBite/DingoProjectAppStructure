# DingoProjectAppStructure

A project structure template for C# Unity project.

Goal: provide a clear folder skeleton and ownership boundaries so the project is easier to:
- scale without chaos,
- separate core, UI, and composition,
- keep state and scene roots in explicit places.

## Contents

At the repository root you have: `Core/`, `GenericView/`, `SceneRoot/`, `StateRoots/` (plus matching `.meta` files). 

Recommended responsibility split:
- **Core/**: base types, shared utilities, contracts, infrastructure, extensions.
- **GenericView/**: reusable UI/views and shared presentation components.
- **SceneRoot/**: “scene root” and composition (initialization, wiring, dependency setup, scene assembly).
- **StateRoots/**: state roots (state containers, state machines, stores, application context).

> Note: the exact content depends on your architecture. This repo provides the skeleton and folder discipline.
