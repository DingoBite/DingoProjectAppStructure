# DingoProjectAppStructure

A project structure template for C# (likely Unity-oriented, since the repository contains `.meta` files). 

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

## How to use

1. Copy the folders from this repository into your project (commonly into `Assets/` for Unity).
2. Do not delete `.meta` files if you use Unity: they are needed to keep asset references stable. 
3. Add new code according to folder responsibility:
   - infrastructure and shared things → `Core/`
   - reusable UI elements → `GenericView/`
   - scene composition/initialization → `SceneRoot/`
   - state/contexts → `StateRoots/`

## Contributing

PRs and issues are welcome:
- a short description of the problem/improvement,
- a minimal repro (for bugs),
- keep the folder responsibility model (don’t move code between folders without a clear reason).
