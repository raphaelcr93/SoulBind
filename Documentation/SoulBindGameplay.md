# Soul Bind gameplay slice

## Current loop

1. Select the player unit to reveal attack range.
2. Select a walkable destination to preview and follow a path.
3. Select the enemy when it is within range to attack.
4. Use the card hand for Strike, Guard, or Focus actions.
5. End the turn manually or spend all action points to advance automatically.
6. Win by defeating the enemy; reset with `RESET BATTLE` or `R`.

## Controls

- Mouse: select tiles, move, attack, and play cards.
- `C`: play the first card as a keyboard shortcut.
- `R`: reset the battle.

## Presentation conventions

- `Runtime/*/Domain`: deterministic gameplay state and rules.
- `Runtime/Grid/Presentation`: Unity scene presentation and input.
- `Tests/EditMode`: domain and pathfinding tests grouped by assembly.
- `Art/LegacyGrid`: selected, reviewed textures from the old PoC.
- `Art/Units`: the scoped authored creature set used by the slice.
- `Resources/SoulBindArt` and `Resources/SoulBindUnits`: runtime-loaded presentation assets only.

The old project is not copied wholesale. Assets are imported only when they support the current tactical board, cards, or units; old managers and unrelated framework code remain excluded.
