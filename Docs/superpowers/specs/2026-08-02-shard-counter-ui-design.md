# Shard Counter UI Design

## Goal

Use the existing skill-tree shard panel as the shard balance counter, remove the duplicate counter generated at the top of the screen, and add a smaller counter with the same visual language to the gameplay HUD below the gold counter.

## Current State

- `SkillTreeView` creates `SkillTreeShardCounter` at runtime through `ShardCounterFactory` because its serialized `_shardText` reference is empty.
- The skill-tree scene already contains `SkillTreeShardPanel` in the bottom-right corner. Its text currently displays the static word `Shard`.
- `HudView` creates a simple top-left shard counter through the same factory because its `_shardText` reference is empty.
- Both runtime counters already read `Player.ShardTotal`, listen to `Player.OnShardsChanged`, and are hidden until `GameFeature.Shards` is unlocked.

## Chosen Approach

Use serialized scene UI rather than generating resource counters at runtime.

This keeps the visual composition editable in Unity, reuses the finished skill-tree artwork, and removes the source of the unwanted duplicate counters. A reusable prefab is unnecessary for two scene-local instances and would expand the scope without improving the requested behavior.

## Skill Tree

- Keep `SkillTreeShardPanel` in its existing bottom-right position.
- Use the panel's existing shard icon and frame artwork unchanged.
- Change its text role from the static label `Shard` to the numeric shard balance.
- Assign that text component to `SkillTreeView._shardText` in `GameScene`.
- Continue formatting the value with `BigDoubleFormatter.FormatFloor`.
- Use the panel root as the visibility target.
- Stop creating `SkillTreeShardCounter` at runtime. This removes the unwanted top-right counter.

## Gameplay HUD

- Add a serialized shard counter under the right-side gold counter area in `GameScene`.
- Match the skill-tree counter's frame, shard icon, typography, and numeric-only content.
- Scale the counter to 75% of the skill-tree version so it remains subordinate to the gold HUD.
- Anchor the counter to the top-right HUD region and position it directly below the gold counter, preserving the relationship across supported aspect ratios.
- Assign its numeric text component to `HudView._shardText`.
- Continue formatting the value with `BigDoubleFormatter.FormatFloor`.

## Visibility and Data Flow

Both counters follow the same rule:

1. Before `GameFeature.Shards` is unlocked, the counter root is inactive.
2. When the unlock node is purchased, the counter becomes visible.
3. The displayed value comes from `Player.ShardTotal`.
4. `Player.OnShardsChanged` refreshes the displayed value.

The existing unlock and balance behavior remains unchanged; only the bound UI objects change.

## Code and Asset Changes

- Update `GameScene.unity` with the serialized references and HUD hierarchy.
- Simplify `SkillTreeView` and `HudView` so they no longer require `ShardPickupConfig` for UI construction.
- Remove `ShardCounterFactory.cs` and its `.meta` file after all usages are removed.
- Do not add editor callbacks, automatic setup scripts, or additional Unity processes.

## Error Handling

The views retain their null guards for serialized text references. A missing scene reference therefore does not crash the game, but it results in no shard counter and is caught by scene/reference validation and visual verification.

## Verification

- Search the codebase to confirm there are no remaining `ShardCounterFactory` or runtime-created shard-counter usages.
- Confirm `SkillTreeView._shardText` points to the lower-right panel text.
- Confirm `HudView._shardText` points to the new HUD panel text.
- Compile the project and run the relevant automated tests.
- Do not launch or control a Unity Editor process for visual verification.
- The user performs the visual check at 4K reference resolution and verifies:
  - no shard counter appears at the top of the skill tree;
  - the bottom-right skill-tree panel displays the live numeric balance;
  - the gameplay counter matches that design, is smaller, and sits below the gold counter;
  - both counters are hidden before shard unlock and visible after unlock;
  - shard collection updates both counters correctly when their screens are active.

## Out of Scope

- Changes to shard drop, pickup, saving, economy, or upgrade calculations.
- Redesigning the gold counter or other HUD elements.
- Creating a generalized currency-widget framework.
