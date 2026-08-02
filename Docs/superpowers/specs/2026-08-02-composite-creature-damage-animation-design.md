# Composite Creature Damage Animation Design

## Goal

When a creature loses health, every active living `SkeletonAnimation` that contains a `damage` animation should play it. This must include both the primary animation body and additional animation bodies, so both parts of each crystal react to damage.

## Scope

The behavior belongs to `CreatureView`. No changes are required to the crystal entity configs, crystal prefabs, or Spine exports.

## Design

`CreatureView` will route damage playback through one helper that accepts a `SkeletonAnimation`:

- Ignore a null body.
- Inspect the body's `SkeletonData` for the `damage` animation.
- If the animation is absent, leave the body untouched without logging a warning.
- If the animation exists, replace track 0 with the non-looping `damage` animation.

On a health decrease, `CreatureView` will call this helper for `_animationBody` and then for every entry in `_additionalAnimationBodies`.

The animation-existence check is intentional. Additional bodies may be decorative, such as `slime_puddle`, and are not required to provide a damage animation.

## Behavior Boundaries

- Health initialization and health increases do not play damage animations.
- Death animation behavior remains unchanged.
- Idle setup and pooling behavior remain unchanged.
- Missing optional damage animations do not generate warnings or exceptions.
- Existing prefab serialization remains compatible.

## Verification

- Confirm both living Spine bodies of `Crystal_1` play `damage` after a nonlethal hit.
- Confirm both living Spine bodies of `Crystal_2` play `damage` after a nonlethal hit.
- Confirm a single-body creature continues to play its damage animation.
- Confirm Slime's main body reacts while `slime_puddle`, which has no `damage` animation, remains unaffected.
- Confirm death playback and pooled respawn still reset visual bodies correctly.
