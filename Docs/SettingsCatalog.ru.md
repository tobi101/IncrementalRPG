# Каталог полей и мест настройки

[Начало справочника](/Users/admin/IncrementalRPG/Docs/README.md)

Снимок сохранённых файлов проекта на 18 сентября 2026 года. Это указатель к двум руководствам: для смысла параметра и ограничений используйте соответствующую главу. Он помогает найти все поля и их экземпляры, включая служебные ссылки.

**Как читать значения:** в таблицах снимков ниже приведены значения конкретных assets/сцен. В алфавитном реестре приведён только инициализатор поля в C# — это исходное значение нового объекта, а не гарантия значения существующего префаба. Прочерк означает отсутствие явного инициализатора. Точные текущие значения каждого экземпляра смотрите в Inspector; они могут различаться и иметь Overrides.

**Полнота реестра:** перечислены сериализованные поля собственных компонентов и конфигов в `Assets/Scripts`, включая вложенные определения эффектов/правил. Состояния сохранений и вычисляемые свойства не являются дизайнерскими настройками. Ниже отдельно добавлены используемые Spine-компоненты и настройки инвентарного плагина. Стандартные UI/Unity-компоненты описаны в руководстве по представлению.

## Снимок подключённых уровней

| Уровень | XP | Сторона | Жар | Интервал, с | Минимум, с | Старт: враги | Старт: бочки | Таблица спавна |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| [0_Level_01.asset](/Users/admin/IncrementalRPG/Assets/SO/Dungeons/0_RocksAndLava/Levels/0_Level_01.asset) | 20 | 6 | 5 | 1.25 | 1.25 | 0.3 | 0 | [0_1SpawnRule.asset](/Users/admin/IncrementalRPG/Assets/SO/Dungeons/0_RocksAndLava/0_1SpawnRule.asset) |
| [0_Level_02.asset](/Users/admin/IncrementalRPG/Assets/SO/Dungeons/0_RocksAndLava/Levels/0_Level_02.asset) | 30 | 6 | 5 | 0.9 | 1 | 0.4 | 0.02 | [0_2SpawnRule.asset](/Users/admin/IncrementalRPG/Assets/SO/Dungeons/0_RocksAndLava/0_2SpawnRule.asset) |
| [0_Level_03.asset](/Users/admin/IncrementalRPG/Assets/SO/Dungeons/0_RocksAndLava/Levels/0_Level_03.asset) | 50 | 7 | 5 | 0.7 | 0.7 | 0.5 | 0.02 | [0_3SpawnRule.asset](/Users/admin/IncrementalRPG/Assets/SO/Dungeons/0_RocksAndLava/0_3SpawnRule.asset) |
| [0_Level_04.asset](/Users/admin/IncrementalRPG/Assets/SO/Dungeons/0_RocksAndLava/Levels/0_Level_04.asset) | 70 | 7 | 5 | 0.5 | 0.5 | 0.6 | 0.02 | [0_4SpawnRule.asset](/Users/admin/IncrementalRPG/Assets/SO/Dungeons/0_RocksAndLava/0_4SpawnRule.asset) |
| [0_Level_05.asset](/Users/admin/IncrementalRPG/Assets/SO/Dungeons/0_RocksAndLava/Levels/0_Level_05.asset) | 100000 | 7 | 5 | 0.3 | 0.3 | 0.7 | 0.02 | [0_5SpawnRule.asset](/Users/admin/IncrementalRPG/Assets/SO/Dungeons/0_RocksAndLava/0_5SpawnRule.asset) |
| [1_Level_01.asset](/Users/admin/IncrementalRPG/Assets/SO/Dungeons/1_AnyBiome/Levels/1_Level_01.asset) | 10 | 10 | 1 | 2 | 0.5 | 0 | 0 | [1_SpawnRule.asset](/Users/admin/IncrementalRPG/Assets/SO/Dungeons/1_AnyBiome/1_SpawnRule.asset) |

Уровни `0_Level_01`…`0_Level_05` используют один общий конфиг генерации RocksAndLava. У `1_Level_01` назначена пустая таблица спавна, но не назначен конфиг генерации, поэтому он пока не считается пригодным к запуску. Новые `bombSpawnInterval`/`minBombSpawnInterval` в этих сохранённых файлах ещё отсутствуют; инициализаторы класса — 10/0,5 с.

## Снимок существ текущего биома

| Существо | HP | Золото | Осколки | XP | Считать врагом | Вид | Тип | Движение | Клеток/с | Радиус попадания |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| [Bombs.asset](/Users/admin/IncrementalRPG/Assets/SO/Dungeons/0_RocksAndLava/Entities/Bombs.asset) | 5 | 0 | 0 | 0 | 0 | None | Bomb | не записано | не записано | 0.25 |
| [Crystal_1.asset](/Users/admin/IncrementalRPG/Assets/SO/Dungeons/0_RocksAndLava/Entities/Crystal_1.asset) | 20 | 0 | 50 | 0 | 0 | Crystal | Crystal | не записано | не записано | 0.3 |
| [Crystal_2.asset](/Users/admin/IncrementalRPG/Assets/SO/Dungeons/0_RocksAndLava/Entities/Crystal_2.asset) | 40 | 0 | 100 | 0 | 0 | Crystal | Crystal | не записано | не записано | 0.3 |
| [Demon.asset](/Users/admin/IncrementalRPG/Assets/SO/Dungeons/0_RocksAndLava/Entities/Demon.asset) | 40 | 80 | 80 | 1 | 1 | Demon | None | 1 | 1 | 0.25 |
| [Skeleton.asset](/Users/admin/IncrementalRPG/Assets/SO/Dungeons/0_RocksAndLava/Entities/Skeleton.asset) | 15 | 30 | 30 | 1 | 1 | Skeleton | None | 1 | 1 | 0.25 |
| [Slime.asset](/Users/admin/IncrementalRPG/Assets/SO/Dungeons/0_RocksAndLava/Entities/Slime.asset) | 5 | 10 | 10 | 1 | 1 | Slime | None | 1 | 1 | 0.25 |

## Активное дерево: размещение, зависимости и эффекты

Перечень построен по `SkillTreeConfig.entries`. Цены и бонусы указаны **за отдельную покупку**. Обозначения эффектов: A — Additive, P — Multiplicative, Unlock — открытие механики. Это текущие числа конфигов, не рекомендации по балансу.

| Нода | X,Y | Покупок | Условия | Цены | Эффекты |
| --- | --- | --- | --- | --- | --- |
| [AddZoneDamageNode](/Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/DamageZone/AddZoneDamageNode.asset) | (0, 0) | 1 | корень | 50 | A ManualAttackDamage: 1 |
| [AutoAttackNode](/Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/DamageZone/AutoAttackNode.asset) | (0, 1) | 1 | AddZoneDamageNode ≥ 1 | 300 | Unlock AutoAttack |
| [SpecialAttackNode](/Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/DamageZone/SpecialAttackNode.asset) | (1, 1) | 1 | AddZoneDamageNode ≥ 1 | 300 | Unlock SpecialAttack |
| [AutoAttackSpeedNode](/Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/DamageZone/AutoAttackSpeedNode.asset) | (0, 2) | 5 | AutoAttackNode ≥ 1 | 350, 450, 550, 650, 750 | A AutoAttackSpeed: 0.05, 0.05, 0.05, 0.05, 0.05 |
| [BombFeatureNode](/Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/Bombs/BombFeatureNode.asset) | (-2, 1) | 1 | AddZoneDamageNode ≥ 1 | 200 | Unlock Bombs |
| [GoldFarm](/Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/Gold/GoldFarm.asset) | (0, -1) | 5 | AddZoneDamageNode ≥ 1 | 150, 200, 250, 300, 350 | P GoldDrop: 0.1, 0.1, 0.1, 0.1, 0.1 |
| [AddDamageRadiusNode](/Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/DamageZone/AddDamageRadiusNode.asset) | (2, 1) | 3 | AddZoneDamageNode ≥ 1 | 100, 125, 150 | P ZoneRadius: 0.25, 0.25, 0.25 |
| [GoldFarm 2](</Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/Gold/GoldFarm 2.asset>) | (1, -2) | 5 | GoldFarm ≥ 5 | 800, 1000, 1200, 1400, 1600 | P GoldDrop: 0.1, 0.1, 0.1, 0.1, 0.1 |
| [MapSizeNode](/Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/Map/MapSizeNode.asset) | (-1, -2) | 1 | GoldFarm ≥ 5 | 1000 | A MapSize: 1 |
| [MapSizeNode 1](</Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/Map/MapSizeNode 1.asset>) | (0, -3) | 1 | MapSizeNode ≥ 1; GoldFarm 2 ≥ 5 | 3000 | A MapSize: 1 |
| [BombsExplosionDamage 1](</Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/Bombs/BombsExplosionDamage 1.asset>) | (-4, 1) | 4 | BombFeatureNode ≥ 1 | 300, 500, 700, 900 | A BombExplosionDamage: 1, 1, 1, 1 |
| [BombsExplosionDamage](/Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/Bombs/BombsExplosionDamage.asset) | (-4, 3) | 3 | BombsExplosionRadius ≥ 3 | 800, 900, 1000 | A BombExplosionDamage: 1, 1, 1 |
| [BombsExplosionRadius 1](</Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/Bombs/BombsExplosionRadius 1.asset>) | (-6, 1) | 3 | BombsExplosionDamage 1 ≥ 4 | 1000, 1200, 1400 | A BombExplosionRadius: 1, 1, 1 |
| [BombsExplosionRadius](/Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/Bombs/BombsExplosionRadius.asset) | (-3, 2) | 3 | BombFeatureNode ≥ 1 | 300, 500, 700 | A BombExplosionRadius: 1, 1, 1 |
| [BombSpawnSpeedNode](/Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/Bombs/BombSpawnSpeedNode.asset) | (-6, 3) | 3 | BombsExplosionDamage ≥ 3; BombsExplosionRadius 1 ≥ 3 | 1000, 1250, 1500 | A BombSpawnSpeed: 0.05, 0.05, 0.05 |
| [BombSpawnSpeedNode 1](</Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/Bombs/BombSpawnSpeedNode 1.asset>) | (-7, 4) | 3 | BombSpawnSpeedNode ≥ 3 | 1600, 2000, 2400 | A BombSpawnSpeed: 0.05, 0.05, 0.05 |
| [AddAttackSpeedNode](/Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/DamageZone/AddAttackSpeedNode.asset) | (4, 3) | 4 | AddDamageRadiusNode ≥ 3 | 250, 350, 450, 550 | A ManualAttackSpeed: 0.05, 0.05, 0.05, 0.05 |
| [AddDamageRadiusNode 1](</Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/DamageZone/AddDamageRadiusNode 1.asset>) | (7, 2) | 4 | AddZoneDamageNode 1 ≥ 3 | 500, 700, 900, 1100 | P ZoneRadius: 0.25, 0.25, 0.25, 0.25 |
| [AddDamageRadiusNode 2](</Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/DamageZone/AddDamageRadiusNode 2.asset>) | (5, 5) | 5 | AddZoneDamageNode 2 ≥ 4 | 1000, 1200, 1400, 1600, 1800 | P ZoneRadius: 0.25, 0.25, 0.25, 0.25, 0.25 |
| [AddAttackSpeedNode 1](</Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/DamageZone/AddAttackSpeedNode 1.asset>) | (9, 1) | 5 | AddDamageRadiusNode 1 ≥ 4 | 1000, 1200, 1400, 1600, 1800 | A ManualAttackSpeed: 0.05, 0.05, 0.05, 0.05, 0.05 |
| [AddZoneDamageNode 1](</Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/DamageZone/AddZoneDamageNode 1.asset>) | (5, 1) | 5 | AddDamageRadiusNode ≥ 3 | 300, 400, 500, 600, 700 | A ManualAttackDamage: 0.5, 0.5, 0.5, 0.5, 0.5 |
| [AddZoneDamageNode 2](</Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/DamageZone/AddZoneDamageNode 2.asset>) | (6, 4) | 5 | AddAttackSpeedNode ≥ 4 | 600, 750, 900, 1050, 1200 | A ManualAttackDamage: 0.5, 0.5, 0.5, 0.5, 0.5 |
| [AddZoneDamageNode 3](</Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/DamageZone/AddZoneDamageNode 3.asset>) | (8, 4) | 5 | AddZoneDamageNode 2 ≥ 5; AddDamageRadiusNode 1 ≥ 4 | 1500, 1600, 1700, 1800, 1900 | A ManualAttackDamage: 0.5, 0.5, 0.5, 0.5, 0.5 |
| [AddSpawnSpeedNode](/Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/Spawn/AddSpawnSpeedNode.asset) | (10, 3) | 3 | AddZoneDamageNode 3 ≥ 5 | 100, 150, 200 | A SpawnSpeed: 0.1, 0.1, 0.1 |
| [AddSpawnSpeedNode 2](</Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/Spawn/AddSpawnSpeedNode 2.asset>) | (12, 3) | 3 | AddSpawnSpeedNode ≥ 1 | 300, 400, 500, 600, 700 | A SpawnSpeed: 0.1, 0.1, 0.1 |
| [AddSpawnSpeedNode 1](</Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/Spawn/AddSpawnSpeedNode 1.asset>) | (14, 3) | 3 | AddSpawnSpeedNode 2 ≥ 1 | 100, 200, 300 | A SpawnSpeed: 0.15, 0.2, 0.25 |
| [GoldFarm 3](</Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/Gold/GoldFarm 3.asset>) | (10, 5) | 3 | AddZoneDamageNode 3 ≥ 5 | 2000, 3000, 4000 | P GoldDrop: 0.3, 0.3, 0.3 |
| [ShardsUnlock](/Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/Shards/ShardsUnlock.asset) | (2, -2) | 1 | GoldFarm ≥ 1 | 250 | Unlock Shards |
| [SlimeShardDrop](/Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/Shards/SlimeShardDrop.asset) | (3, -3) | 5 | ShardsUnlock ≥ 1 | 100, 200, 350, 550, 800 | P SlimeShardDrop: 0.1, 0.1, 0.1, 0.1, 0.1 |
| [SkeletonShardDrop](/Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/Shards/SkeletonShardDrop.asset) | (4, -4) | 5 | SlimeShardDrop ≥ 2 | 250, 400, 600, 850, 1200 | P SkeletonShardDrop: 0.1, 0.1, 0.1, 0.1, 0.1 |
| [DemonShardDrop](/Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/Shards/DemonShardDrop.asset) | (5, -5) | 5 | SkeletonShardDrop ≥ 2 | 500, 750, 1050, 1400, 1800 | P DemonShardDrop: 0.1, 0.1, 0.1, 0.1, 0.1 |
| [CrystalShardDrop](/Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/Shards/CrystalShardDrop.asset) | (2, -3) | 5 | ShardsUnlock ≥ 1 | 200, 350, 550, 800, 1100 | A CrystalShardDropBonus: 5, 10, 15, 20, 25 |
| [ShardPickupSpeed](/Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/Shards/ShardPickupSpeed.asset) | (2, -4) | 5 | CrystalShardDrop ≥ 1 | 250, 450, 700, 1000, 1400 | P ShardPickupSpeed: 0.1, 0.1, 0.1, 0.1, 0.1 |

Для `AddSpawnSpeedNode 2` сохранено пять цен при трёх уровнях: используются первые три. `valuesPerLevel` суммируются; переставлять ноды без изменения `prerequisites` недостаточно для смены порядка покупки.

## Алфавитный реестр собственных компонентов и конфигов

Вложенные поля отмечены путём массива. Объекты без найденного прямого размещения могут создаваться программно либо быть вложенной частью другого конфига; отсутствие прямого размещения не означает автоматически неработающую механику.

- [AudioManager](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#audiomanager)
- [AudioMixerSettingsApplier](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#audiomixersettingsapplier)
- [BombExplosionConfig](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#bombexplosionconfig)
- [BombExplosionVisualScaler](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#bombexplosionvisualscaler)
- [CreatureView](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#creatureview)
- [DamagePopupLayerView](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#damagepopuplayerview)
- [DamagePopupView](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#damagepopupview)
- [DamageZoneConfig](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#damagezoneconfig)
- [DamageZoneView](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#damagezoneview)
- [DemoEndPopupView](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#demoendpopupview)
- [DungeonConfig](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#dungeonconfig)
- [DungeonInfoPanelView](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#dungeoninfopanelview)
- [DungeonLevelConfig](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#dungeonlevelconfig)
- [DungeonLevelTransitionConfig](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#dungeonleveltransitionconfig)
- [DungeonList](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#dungeonlist)
- [DungeonMapButtonView](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#dungeonmapbuttonview)
- [DungeonMenuView](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#dungeonmenuview)
- [EnemyIconTooltipTrigger](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#enemyicontooltiptrigger)
- [EntityConfig](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#entityconfig)
- [GameSceneInstaller](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#gamesceneinstaller)
- [GameSettingsData](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#gamesettingsdata)
- [GoldPopupView](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#goldpopupview)
- [HubFeatureButtonView](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#hubfeaturebuttonview)
- [HubLabelView](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#hublabelview)
- [HubView](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#hubview)
- [HudView](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#hudview)
- [IntroPanelPlayer](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#intropanelplayer)
- [IsometricGradientTilemapGenerator](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#isometricgradienttilemapgenerator)
- [ItemCatalog](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#itemcatalog)
- [ItemDefinition](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#itemdefinition)
- [LevelLootPool](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#levellootpool)
- [LevelTransitionCurtainView](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#leveltransitioncurtainview)
- [LevelTransitionLampCounterView](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#leveltransitionlampcounterview)
- [LootRewardPopupView](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#lootrewardpopupview)
- [LootboxView](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#lootboxview)
- [MainMenuButtonLayoutScaler](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#mainmenubuttonlayoutscaler)
- [MainMenuButtonView](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#mainmenubuttonview)
- [MainMenuController](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#mainmenucontroller)
- [MainMenuGlowAnimator](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#mainmenuglowanimator)
- [MainMenuView](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#mainmenuview)
- [MapMenuFadeTransition](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#mapmenufadetransition)
- [MouseWheelSliderInput](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#mousewheelsliderinput)
- [NodeBackGlowSpriteConfig](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#nodebackglowspriteconfig)
- [NodeBorderColorConfig](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#nodebordercolorconfig)
- [NodeCircleSpriteConfig](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#nodecirclespriteconfig)
- [NodeConnectionView](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#nodeconnectionview)
- [NodeDefinition](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#nodedefinition)
- [NodeEffect](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#nodeeffect)
- [NodeFramePriceSpriteConfig](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#nodeframepricespriteconfig)
- [NodeLevelCounterView](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#nodelevelcounterview)
- [NodePopupView](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#nodepopupview)
- [NodeView](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#nodeview)
- [PauseButtonVisualState](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#pausebuttonvisualstate)
- [PauseMenuController](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#pausemenucontroller)
- [PlayerInventoryView](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#playerinventoryview)
- [PotionHudRowView](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#potionhudrowview)
- [PotionHudView](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#potionhudview)
- [RewardRaysGraphic](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#rewardraysgraphic)
- [RewardRevealEffect](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#rewardrevealeffect)
- [SessionEndPopupView](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#sessionendpopupview)
- [SettingsMenuController](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#settingsmenucontroller)
- [SettingsMenuView](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#settingsmenuview)
- [ShardPickupConfig](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#shardpickupconfig)
- [ShardPickupView](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#shardpickupview)
- [ShineSweepAnimator](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#shinesweepanimator)
- [SideMenuButtonVisualState](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#sidemenubuttonvisualstate)
- [SideMenuFlyoutView](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#sidemenuflyoutview)
- [SkeletonTint](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#skeletontint)
- [SkillTreeConfig](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#skilltreeconfig)
- [SkillTreePanZoomController](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#skilltreepanzoomcontroller)
- [SkillTreeView](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#skilltreeview)
- [SpawnTable](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#spawntable)
- [TileGridDebugger](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#tilegriddebugger)
- [TilemapCameraAutoFitter](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#tilemapcameraautofitter)
- [TilemapGenerationConfig](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#tilemapgenerationconfig)
- [TilemapTileSet](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#tilemaptileset)
- [TmpGlowStyle](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#tmpglowstyle)
- [TmpGlowText](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#tmpglowtext)
- [TooltipView](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#tooltipview)
- [UIButtonAudio](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#uibuttonaudio)
- [UIButtonPressScaler](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md#uibuttonpressscaler)

<a id="audiomanager"></a>

### AudioManager

[Назначение: руководство по анимациям, интерфейсу и звуку](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/AudioManager/AudioManager.cs).

Где редактировать:

- [BootstrapScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/BootstrapScene.unity:340) → `AudioManager`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [_registerAsGlobalInstance](/Users/admin/IncrementalRPG/Assets/Scripts/AudioManager/AudioManager.cs:22) | bool | `true` | Значение |
| [_dontDestroyOnLoad](/Users/admin/IncrementalRPG/Assets/Scripts/AudioManager/AudioManager.cs:23) | bool | — | Значение |
| [_destroyDuplicateInstances](/Users/admin/IncrementalRPG/Assets/Scripts/AudioManager/AudioManager.cs:24) | bool | — | Значение |
| [_sfxSource](/Users/admin/IncrementalRPG/Assets/Scripts/AudioManager/AudioManager.cs:28) | AudioSource | — | Ссылка на объект/компонент/ресурс |
| [_hitsSource](/Users/admin/IncrementalRPG/Assets/Scripts/AudioManager/AudioManager.cs:29) | AudioSource | — | Ссылка на объект/компонент/ресурс |
| [_musicSource](/Users/admin/IncrementalRPG/Assets/Scripts/AudioManager/AudioManager.cs:30) | AudioSource | — | Ссылка на объект/компонент/ресурс |
| [_loopingSfxSource](/Users/admin/IncrementalRPG/Assets/Scripts/AudioManager/AudioManager.cs:31) | AudioSource | — | Ссылка на объект/компонент/ресурс |
| [_createMissingSfxSource](/Users/admin/IncrementalRPG/Assets/Scripts/AudioManager/AudioManager.cs:32) | bool | `true` | Значение |
| [_createMissingHitsSource](/Users/admin/IncrementalRPG/Assets/Scripts/AudioManager/AudioManager.cs:33) | bool | `true` | Значение |
| [_createMissingMusicSource](/Users/admin/IncrementalRPG/Assets/Scripts/AudioManager/AudioManager.cs:34) | bool | `true` | Значение |
| [_createMissingLoopingSfxSource](/Users/admin/IncrementalRPG/Assets/Scripts/AudioManager/AudioManager.cs:35) | bool | `true` | Значение |
| [_sfxMixerGroup](/Users/admin/IncrementalRPG/Assets/Scripts/AudioManager/AudioManager.cs:36) | AudioMixerGroup | — | Ссылка на объект/компонент/ресурс |
| [_hitsMixerGroup](/Users/admin/IncrementalRPG/Assets/Scripts/AudioManager/AudioManager.cs:37) | AudioMixerGroup | — | Ссылка на объект/компонент/ресурс |
| [_musicMixerGroup](/Users/admin/IncrementalRPG/Assets/Scripts/AudioManager/AudioManager.cs:38) | AudioMixerGroup | — | Ссылка на объект/компонент/ресурс |
| [_hitAudioClip](/Users/admin/IncrementalRPG/Assets/Scripts/AudioManager/AudioManager.cs:41) | AudioClip | — | Ссылка на объект/компонент/ресурс |
| [_maxHitAudioRequestsPerFrame](/Users/admin/IncrementalRPG/Assets/Scripts/AudioManager/AudioManager.cs:42) | int | `20` | Значение; Min(1) |
| [_maxDeathAudioRequestsPerFrame](/Users/admin/IncrementalRPG/Assets/Scripts/AudioManager/AudioManager.cs:43) | int | `4` | Значение; Min(1) |
| [_waveAudioClip](/Users/admin/IncrementalRPG/Assets/Scripts/AudioManager/AudioManager.cs:44) | AudioClip | — | Ссылка на объект/компонент/ресурс |
| [_uiHoverAudioClip](/Users/admin/IncrementalRPG/Assets/Scripts/AudioManager/AudioManager.cs:45) | AudioClip | — | Ссылка на объект/компонент/ресурс |
| [_uiClickAudioClip](/Users/admin/IncrementalRPG/Assets/Scripts/AudioManager/AudioManager.cs:46) | AudioClip | — | Ссылка на объект/компонент/ресурс |
| [_skillUpgradeAudioClip](/Users/admin/IncrementalRPG/Assets/Scripts/AudioManager/AudioManager.cs:47) | AudioClip | — | Ссылка на объект/компонент/ресурс |
| [_skillMaxAudioClip](/Users/admin/IncrementalRPG/Assets/Scripts/AudioManager/AudioManager.cs:48) | AudioClip | — | Ссылка на объект/компонент/ресурс |
| [_skillErrorAudioClip](/Users/admin/IncrementalRPG/Assets/Scripts/AudioManager/AudioManager.cs:49) | AudioClip | — | Ссылка на объект/компонент/ресурс |
| [_sessionEndAudioClip](/Users/admin/IncrementalRPG/Assets/Scripts/AudioManager/AudioManager.cs:50) | AudioClip | — | Ссылка на объект/компонент/ресурс |
| [_curtainCloseAudioClip](/Users/admin/IncrementalRPG/Assets/Scripts/AudioManager/AudioManager.cs:51) | AudioClip | — | Ссылка на объект/компонент/ресурс |
| [_curtainOpenAudioClip](/Users/admin/IncrementalRPG/Assets/Scripts/AudioManager/AudioManager.cs:52) | AudioClip | — | Ссылка на объект/компонент/ресурс |
| [_levelCounterOnAudioClip](/Users/admin/IncrementalRPG/Assets/Scripts/AudioManager/AudioManager.cs:53) | AudioClip | — | Ссылка на объект/компонент/ресурс |
| [_fightStartFadeAudioClip](/Users/admin/IncrementalRPG/Assets/Scripts/AudioManager/AudioManager.cs:54) | AudioClip | — | Ссылка на объект/компонент/ресурс |
| [_fightStartBurnAudioClip](/Users/admin/IncrementalRPG/Assets/Scripts/AudioManager/AudioManager.cs:55) | AudioClip | — | Ссылка на объект/компонент/ресурс |
| [_lavaLoopClip](/Users/admin/IncrementalRPG/Assets/Scripts/AudioManager/AudioManager.cs:58) | AudioClip | — | Ссылка на объект/компонент/ресурс |
| [_loopingSfxSourceVolume](/Users/admin/IncrementalRPG/Assets/Scripts/AudioManager/AudioManager.cs:59) | float | `1f` | Значение; Range(0f, 1f) |
| [_lavaLoopMinVolume](/Users/admin/IncrementalRPG/Assets/Scripts/AudioManager/AudioManager.cs:60) | float | `0f` | Значение; Range(0f, 1f) |
| [_lavaLoopMaxVolume](/Users/admin/IncrementalRPG/Assets/Scripts/AudioManager/AudioManager.cs:61) | float | `1f` | Значение; Range(0f, 1f) |
| [_loopingSfxFadeDuration](/Users/admin/IncrementalRPG/Assets/Scripts/AudioManager/AudioManager.cs:62) | float | `0.25f` | Значение; Min(0f) |
| [_mainMenuMusicClip](/Users/admin/IncrementalRPG/Assets/Scripts/AudioManager/AudioManager.cs:65) | AudioClip | — | Ссылка на объект/компонент/ресурс |
| [_hubMusicClip](/Users/admin/IncrementalRPG/Assets/Scripts/AudioManager/AudioManager.cs:66) | AudioClip | — | Ссылка на объект/компонент/ресурс |
| [_gameplayMusicClip](/Users/admin/IncrementalRPG/Assets/Scripts/AudioManager/AudioManager.cs:67) | AudioClip | — | Ссылка на объект/компонент/ресурс |
| [_musicSourceVolume](/Users/admin/IncrementalRPG/Assets/Scripts/AudioManager/AudioManager.cs:68) | float | `1f` | Значение; Range(0f, 1f) |
| [_musicFadeDuration](/Users/admin/IncrementalRPG/Assets/Scripts/AudioManager/AudioManager.cs:69) | float | `0.5f` | Значение; Min(0f) |

<a id="audiomixersettingsapplier"></a>

### AudioMixerSettingsApplier

[Назначение: руководство по анимациям, интерфейсу и звуку](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/Settings/AudioMixerSettingsApplier.cs).

Где редактировать:

- [BootstrapScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/BootstrapScene.unity:391) → `AudioManager`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [_audioMixer](/Users/admin/IncrementalRPG/Assets/Scripts/Settings/AudioMixerSettingsApplier.cs:9) | AudioMixer | — | Ссылка на объект/компонент/ресурс |
| [_masterVolumeParameter](/Users/admin/IncrementalRPG/Assets/Scripts/Settings/AudioMixerSettingsApplier.cs:10) | string | `"MasterVolume"` | Значение |
| [_musicVolumeParameter](/Users/admin/IncrementalRPG/Assets/Scripts/Settings/AudioMixerSettingsApplier.cs:11) | string | `"MusicVolume"` | Значение |
| [_sfxVolumeParameter](/Users/admin/IncrementalRPG/Assets/Scripts/Settings/AudioMixerSettingsApplier.cs:12) | string | `"SfxVolume"` | Значение |
| [_useAudioListenerAsMasterFallback](/Users/admin/IncrementalRPG/Assets/Scripts/Settings/AudioMixerSettingsApplier.cs:13) | bool | `true` | Значение |

<a id="bombexplosionconfig"></a>

### BombExplosionConfig

[Назначение: 7. Бочка](/Users/admin/IncrementalRPG/Docs/GameplaySettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/Bomb/BombExplosionConfig.cs).

Где редактировать:

- [BombExplosionConfig.asset](/Users/admin/IncrementalRPG/Assets/SO/BombExplosionConfig.asset:3) → `BombExplosionConfig`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [baseDamage](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/Bomb/BombExplosionConfig.cs:9) | BigDouble | `30` | Значение |
| [baseRadius](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/Bomb/BombExplosionConfig.cs:10) | float | `2f` | Значение |
| [aspectRatio](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/Bomb/BombExplosionConfig.cs:11) | float | `0.55f` | Значение |

<a id="bombexplosionvisualscaler"></a>

### BombExplosionVisualScaler

[Назначение: руководство по анимациям, интерфейсу и звуку](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/Bomb/BombExplosionVisualScaler.cs).

Где редактировать:

- [Bombs.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/Entities/Bombs.prefab:271) → `Spine GameObject (barrel_exposion)`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [_explosionVisual](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/Bomb/BombExplosionVisualScaler.cs:7) | Transform | — | Ссылка на объект/компонент/ресурс |

<a id="creatureview"></a>

### CreatureView

[Назначение: руководство по анимациям, интерфейсу и звуку](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/Entity/CreatureView.cs).

Базовый префаб Creature наследуют варианты существ. Вложенные `deathAnimationBodies` настраиваются в нужном варианте; ссылки/Overrides могут находиться в нём.

Где редактировать:

- [Creature.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/Entities/Creature.prefab:107) → `Creature`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [_deathAnimationBodies[]._animationBody](/Users/admin/IncrementalRPG/Assets/Scripts/Entity/CreatureView.cs:20) | SkeletonAnimation | — | Ссылка на объект/компонент/ресурс |
| [_deathAnimationBodies[]._animationName](/Users/admin/IncrementalRPG/Assets/Scripts/Entity/CreatureView.cs:21) | string | `DeathAnimationName` | Значение |
| [_deathAnimationBodies[]._visibleWhileAlive](/Users/admin/IncrementalRPG/Assets/Scripts/Entity/CreatureView.cs:22) | bool | — | Значение |
| [_deathAnimationBodies[]._waitForComplete](/Users/admin/IncrementalRPG/Assets/Scripts/Entity/CreatureView.cs:23) | bool | `true` | Значение |
| [_footAnchor](/Users/admin/IncrementalRPG/Assets/Scripts/Entity/CreatureView.cs:33) | Transform | — | Ссылка на объект/компонент/ресурс |
| [_damagePopupAnchor](/Users/admin/IncrementalRPG/Assets/Scripts/Entity/CreatureView.cs:34) | Transform | — | Ссылка на объект/компонент/ресурс |
| [_damagePopupOffset](/Users/admin/IncrementalRPG/Assets/Scripts/Entity/CreatureView.cs:35) | Vector3 | `new Vector3(0f, 1f, 0f)` | Значение |
| [_animationBody](/Users/admin/IncrementalRPG/Assets/Scripts/Entity/CreatureView.cs:36) | SkeletonAnimation | — | Ссылка на объект/компонент/ресурс |
| [_additionalAnimationBodies](/Users/admin/IncrementalRPG/Assets/Scripts/Entity/CreatureView.cs:37) | SkeletonAnimation[] | `Array.Empty<SkeletonAnimation>()` | Список; элементы раскрываются в Inspector |
| [_deathAnimationBodies](/Users/admin/IncrementalRPG/Assets/Scripts/Entity/CreatureView.cs:38) | DeathAnimationBody[] | `Array.Empty<DeathAnimationBody>()` | Список; элементы раскрываются в Inspector |
| [_facesRightByDefault](/Users/admin/IncrementalRPG/Assets/Scripts/Entity/CreatureView.cs:42) | bool | — | Значение |

<a id="damagepopuplayerview"></a>

### DamagePopupLayerView

[Назначение: руководство по анимациям, интерфейсу и звуку](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/UI/DamagePopupLayerView.cs).

Где редактировать:

- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:37475) → `DamagePopupLayerView`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [_popupPrefab](/Users/admin/IncrementalRPG/Assets/Scripts/UI/DamagePopupLayerView.cs:15) | DamagePopupView | — | Ссылка на объект/компонент/ресурс |
| [_poolRoot](/Users/admin/IncrementalRPG/Assets/Scripts/UI/DamagePopupLayerView.cs:16) | Transform | — | Ссылка на объект/компонент/ресурс |
| [_poolSize](/Users/admin/IncrementalRPG/Assets/Scripts/UI/DamagePopupLayerView.cs:17) | int | `64` | Значение; Min(1) |
| [_overflowMode](/Users/admin/IncrementalRPG/Assets/Scripts/UI/DamagePopupLayerView.cs:18) | OverflowMode | `OverflowMode.ReuseOldest` | Выбор из списка |
| [_duration](/Users/admin/IncrementalRPG/Assets/Scripts/UI/DamagePopupLayerView.cs:19) | float | `0.85f` | Значение; Min(0.01f) |
| [_moveDistance](/Users/admin/IncrementalRPG/Assets/Scripts/UI/DamagePopupLayerView.cs:20) | float | `0.35f` | Значение; Min(0f) |
| [_horizontalJitter](/Users/admin/IncrementalRPG/Assets/Scripts/UI/DamagePopupLayerView.cs:21) | float | `0.08f` | Значение; Min(0f) |
| [_verticalJitter](/Users/admin/IncrementalRPG/Assets/Scripts/UI/DamagePopupLayerView.cs:22) | float | `0.02f` | Значение; Min(0f) |

<a id="damagepopupview"></a>

### DamagePopupView

[Назначение: руководство по анимациям, интерфейсу и звуку](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/UI/DamagePopupView.cs).

Где редактировать:

- [DamagePopupView.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/MiscPanels/DamagePopupView.prefab:217) → `DamagePopupView`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [_text](/Users/admin/IncrementalRPG/Assets/Scripts/UI/DamagePopupView.cs:10) | TMP_Text | — | Ссылка на объект/компонент/ресурс |

<a id="damagezoneconfig"></a>

### DamageZoneConfig

[Назначение: 3. Атаки](/Users/admin/IncrementalRPG/Docs/GameplaySettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/DamageZone/DamageZoneConfig.cs).

Где редактировать:

- [DamageZoneConfig.asset](/Users/admin/IncrementalRPG/Assets/SO/DamageZone/DamageZoneConfig.asset:3) → `DamageZoneConfig`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [baseManualAttackDamage](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/DamageZone/DamageZoneConfig.cs:10) | BigDouble | `BigDouble.One` | Значение |
| [baseManualAttackCooldown](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/DamageZone/DamageZoneConfig.cs:12) | float | `1f` | Значение |
| [baseAutoAttackDamage](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/DamageZone/DamageZoneConfig.cs:13) | BigDouble | `BigDouble.One` | Значение |
| [baseAutoAttackInterval](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/DamageZone/DamageZoneConfig.cs:14) | float | `1f` | Значение |
| [baseSpecialAttackDamage](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/DamageZone/DamageZoneConfig.cs:17) | BigDouble | `new BigDouble(5)` | Значение |
| [baseSpecialAttackCooldown](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/DamageZone/DamageZoneConfig.cs:18) | float | `5f` | Значение; Min(0f) |
| [baseRadius](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/DamageZone/DamageZoneConfig.cs:20) | float | `0.6f` | Значение |
| [aspectRatio](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/DamageZone/DamageZoneConfig.cs:21) | float | `0.55f` | Значение |

<a id="damagezoneview"></a>

### DamageZoneView

[Назначение: руководство по анимациям, интерфейсу и звуку](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/DamageZone/DamageZoneView.cs).

Масштаб графики пересчитывается из игрового радиуса; `_baseRadiusX` — калибровка.

Где редактировать:

- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:47462); экземпляр [DamageZone.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/DamageZone.prefab)
- [DamageZone.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/DamageZone.prefab:307) → `DamageZone`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [_circle](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/DamageZone/DamageZoneView.cs:17) | SkeletonAnimation | — | Ссылка на объект/компонент/ресурс |
| [_manualWave](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/DamageZone/DamageZoneView.cs:18) | SkeletonAnimation | — | Ссылка на объект/компонент/ресурс |
| [_autoWaveBack](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/DamageZone/DamageZoneView.cs:19) | SkeletonAnimation | — | Ссылка на объект/компонент/ресурс |
| [_autoWaveFront](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/DamageZone/DamageZoneView.cs:20) | SkeletonAnimation | — | Ссылка на объект/компонент/ресурс |
| [_specialWave](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/DamageZone/DamageZoneView.cs:21) | SkeletonAnimation | — | Ссылка на объект/компонент/ресурс |
| [_baseRadiusX](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/DamageZone/DamageZoneView.cs:25) | float | `0.6f` | Значение |

<a id="demoendpopupview"></a>

### DemoEndPopupView

[Назначение: руководство по анимациям, интерфейсу и звуку](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/UI/DemoEndPopupView.cs).

Где редактировать:

- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:18327) → `HUD/DemoEndPopup`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [_blurBackground](/Users/admin/IncrementalRPG/Assets/Scripts/UI/DemoEndPopupView.cs:11) | RawImage | — | Ссылка на объект/компонент/ресурс |
| [_contentRoot](/Users/admin/IncrementalRPG/Assets/Scripts/UI/DemoEndPopupView.cs:12) | GameObject | — | Ссылка на объект/компонент/ресурс |
| [_blurMaterialTemplate](/Users/admin/IncrementalRPG/Assets/Scripts/UI/DemoEndPopupView.cs:13) | Material | — | Ссылка на объект/компонент/ресурс |
| [_captureDownscale](/Users/admin/IncrementalRPG/Assets/Scripts/UI/DemoEndPopupView.cs:14) | int | `2` | Значение; Min(1) |
| [_blurIntensity](/Users/admin/IncrementalRPG/Assets/Scripts/UI/DemoEndPopupView.cs:15) | float | `12f` | Значение; Range(0f, 100f) |
| [_useLowResBlur](/Users/admin/IncrementalRPG/Assets/Scripts/UI/DemoEndPopupView.cs:16) | bool | — | Значение |
| [_continueButton](/Users/admin/IncrementalRPG/Assets/Scripts/UI/DemoEndPopupView.cs:19) | Button | — | Ссылка на объект/компонент/ресурс |
| [_wishlistButton](/Users/admin/IncrementalRPG/Assets/Scripts/UI/DemoEndPopupView.cs:20) | Button | — | Ссылка на объект/компонент/ресурс |
| [_mainMenuButton](/Users/admin/IncrementalRPG/Assets/Scripts/UI/DemoEndPopupView.cs:21) | Button | — | Ссылка на объект/компонент/ресурс |
| [_steamWishlistUrl](/Users/admin/IncrementalRPG/Assets/Scripts/UI/DemoEndPopupView.cs:22) | string | — | Значение |

<a id="dungeonconfig"></a>

### DungeonConfig

[Назначение: 4. Подземелья](/Users/admin/IncrementalRPG/Docs/GameplaySettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/Dungeon/DungeonConfig.cs).

Где редактировать:

- [0_DungeonConfig.asset](/Users/admin/IncrementalRPG/Assets/SO/Dungeons/0_RocksAndLava/0_DungeonConfig.asset:3) → `0_DungeonConfig`
- [1_DungeonConfig.asset](/Users/admin/IncrementalRPG/Assets/SO/Dungeons/1_AnyBiome/1_DungeonConfig.asset:3) → `1_DungeonConfig`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [dungeonId](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/Dungeon/DungeonConfig.cs:10) | string | — | Значение |
| [displayName](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/Dungeon/DungeonConfig.cs:11) | LocalizedString | `new()` | Таблица локализации + ключ |
| [title](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/Dungeon/DungeonConfig.cs:12) | LocalizedString | `new()` | Таблица локализации + ключ |
| [description](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/Dungeon/DungeonConfig.cs:13) | LocalizedString | `new()` | Таблица локализации + ключ |
| [icon](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/Dungeon/DungeonConfig.cs:14) | Sprite | — | Ссылка на объект/компонент/ресурс |
| [previewImage](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/Dungeon/DungeonConfig.cs:15) | Sprite | — | Ссылка на объект/компонент/ресурс |
| [levels](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/Dungeon/DungeonConfig.cs:18) | DungeonLevelConfig[] | — | Список; элементы раскрываются в Inspector |

<a id="dungeoninfopanelview"></a>

### DungeonInfoPanelView

[Назначение: руководство по анимациям, интерфейсу и звуку](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/UI/DungeonInfoPanelView.cs).

Где редактировать:

- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:33083) → `MenuCanvas/MapView/RightGradient/DungeonInfoPanel`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [_dungeonNameText](/Users/admin/IncrementalRPG/Assets/Scripts/UI/DungeonInfoPanelView.cs:14) | TMP_Text | — | Ссылка на объект/компонент/ресурс |
| [_levelNumberText](/Users/admin/IncrementalRPG/Assets/Scripts/UI/DungeonInfoPanelView.cs:15) | TMP_Text | — | Ссылка на объект/компонент/ресурс |
| [_playableContent](/Users/admin/IncrementalRPG/Assets/Scripts/UI/DungeonInfoPanelView.cs:16) | GameObject[] | — | Список; элементы раскрываются в Inspector |
| [_unavailableContent](/Users/admin/IncrementalRPG/Assets/Scripts/UI/DungeonInfoPanelView.cs:17) | GameObject[] | — | Список; элементы раскрываются в Inspector |
| [_enemyIconsContainer](/Users/admin/IncrementalRPG/Assets/Scripts/UI/DungeonInfoPanelView.cs:18) | Transform | — | Ссылка на объект/компонент/ресурс |
| [_enemyIconPrefab](/Users/admin/IncrementalRPG/Assets/Scripts/UI/DungeonInfoPanelView.cs:19) | Image | — | Ссылка на объект/компонент/ресурс |
| [_enemyTooltipView](/Users/admin/IncrementalRPG/Assets/Scripts/UI/DungeonInfoPanelView.cs:20) | TooltipView | — | Ссылка на объект/компонент/ресурс |
| [_maxEnemyIcons](/Users/admin/IncrementalRPG/Assets/Scripts/UI/DungeonInfoPanelView.cs:21) | int | `6` | Значение; Min(1) |
| [_startButton](/Users/admin/IncrementalRPG/Assets/Scripts/UI/DungeonInfoPanelView.cs:22) | Button | — | Ссылка на объект/компонент/ресурс |

<a id="dungeonlevelconfig"></a>

### DungeonLevelConfig

[Назначение: 4–5. Уровни и спавн](/Users/admin/IncrementalRPG/Docs/GameplaySettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/Dungeon/DungeonLevelConfig.cs).

Где редактировать:

- [0_Level_01.asset](/Users/admin/IncrementalRPG/Assets/SO/Dungeons/0_RocksAndLava/Levels/0_Level_01.asset:3) → `0_Level_01`
- [0_Level_03.asset](/Users/admin/IncrementalRPG/Assets/SO/Dungeons/0_RocksAndLava/Levels/0_Level_03.asset:3) → `0_Level_03`
- [0_Level_05.asset](/Users/admin/IncrementalRPG/Assets/SO/Dungeons/0_RocksAndLava/Levels/0_Level_05.asset:3) → `0_Level_05`
- [0_Level_02.asset](/Users/admin/IncrementalRPG/Assets/SO/Dungeons/0_RocksAndLava/Levels/0_Level_02.asset:3) → `0_Level_02`
- [0_Level_04.asset](/Users/admin/IncrementalRPG/Assets/SO/Dungeons/0_RocksAndLava/Levels/0_Level_04.asset:3) → `0_Level_04`
- [1_Level_01.asset](/Users/admin/IncrementalRPG/Assets/SO/Dungeons/1_AnyBiome/Levels/1_Level_01.asset:3) → `1_Level_01`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [levelId](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/Dungeon/DungeonLevelConfig.cs:12) | string | — | Значение |
| [displayName](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/Dungeon/DungeonLevelConfig.cs:13) | LocalizedString | `new()` | Таблица локализации + ключ |
| [title](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/Dungeon/DungeonLevelConfig.cs:14) | LocalizedString | `new()` | Таблица локализации + ключ |
| [description](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/Dungeon/DungeonLevelConfig.cs:15) | LocalizedString | `new()` | Таблица локализации + ключ |
| [xpGoal](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/Dungeon/DungeonLevelConfig.cs:18) | BigDouble | `10` | Значение |
| [tilemapGenerationConfig](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/Dungeon/DungeonLevelConfig.cs:21) | TilemapGenerationConfig | — | Ссылка на объект/компонент/ресурс |
| [minPlayZoneSize](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/Dungeon/DungeonLevelConfig.cs:22) | int | — | Значение; Min(0) |
| [heatIndex](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/Dungeon/DungeonLevelConfig.cs:23) | float | `1f` | Значение; Min(0.1f) |
| [spawnTable](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/Dungeon/DungeonLevelConfig.cs:26) | SpawnTable | — | Ссылка на объект/компонент/ресурс |
| [initialEnemySpawnDensity](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/Dungeon/DungeonLevelConfig.cs:27) | float | — | Значение; Min(0f) |
| [initialBombSpawnDensity](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/Dungeon/DungeonLevelConfig.cs:28) | float | — | Значение; Min(0f) |
| [spawnInterval](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/Dungeon/DungeonLevelConfig.cs:29) | float | `2f` | Значение; Min(0.1f) |
| [minSpawnInterval](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/Dungeon/DungeonLevelConfig.cs:30) | float | `0.5f` | Значение; Min(0.0000001f) |
| [bombSpawnInterval](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/Dungeon/DungeonLevelConfig.cs:31) | float | `10f` | Значение; Min(0.1f) |
| [minBombSpawnInterval](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/Dungeon/DungeonLevelConfig.cs:32) | float | `0.5f` | Значение; Min(0.0000001f) |
| [goldDropMultiplier](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/Dungeon/DungeonLevelConfig.cs:35) | float | `1f` | Значение; Min(0f) |
| [lootPool](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/Dungeon/DungeonLevelConfig.cs:36) | LevelLootPool | `new()` | Вложенные настройки |

<a id="dungeonleveltransitionconfig"></a>

### DungeonLevelTransitionConfig

[Назначение: руководство по анимациям, интерфейсу и звуку](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/Dungeon/DungeonLevelTransitionConfig.cs).

Вложено в `GameSceneInstaller._levelTransitionConfig`. `holdDuration` не используется текущим координатором как задержка.

Прямое размещение в сохранённых игровых сценах/префабах/конфигах не найдено; см. назначение и вложенность выше.

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [closeDuration](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/Dungeon/DungeonLevelTransitionConfig.cs:9) | float | `0.75f` | Значение; Min(0f) |
| [holdDuration](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/Dungeon/DungeonLevelTransitionConfig.cs:10) | float | — | Значение; Min(0f) |
| [openDuration](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/Dungeon/DungeonLevelTransitionConfig.cs:11) | float | `0.75f` | Значение; Min(0f) |
| [lootGraceDuration](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/Dungeon/DungeonLevelTransitionConfig.cs:12) | float | `1.5f` | Значение; Min(0f) |

<a id="dungeonlist"></a>

### DungeonList

[Назначение: 4. Подземелья](/Users/admin/IncrementalRPG/Docs/GameplaySettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/Dungeon/DungeonList.cs).

Где редактировать:

- [DungeonList.asset](/Users/admin/IncrementalRPG/Assets/SO/Dungeons/DungeonList.asset:3) → `DungeonList`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [dungeons](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/Dungeon/DungeonList.cs:8) | DungeonConfig[] | — | Список; элементы раскрываются в Inspector |

<a id="dungeonmapbuttonview"></a>

### DungeonMapButtonView

[Назначение: руководство по анимациям, интерфейсу и звуку](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/UI/DungeonMapButtonView.cs).

Где редактировать:

- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:19550); экземпляр [Dungeon1.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/DungeonMap/Dungeon1.prefab)
- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:32742); экземпляр [Dungeon3.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/DungeonMap/Dungeon3.prefab)
- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:38273); экземпляр [Dungeon2.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/DungeonMap/Dungeon2.prefab)
- [DungeonView.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/DungeonMap/DungeonView.prefab:201) → `DungeonView`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [_dungeon](/Users/admin/IncrementalRPG/Assets/Scripts/UI/DungeonMapButtonView.cs:11) | DungeonConfig | — | Ссылка на объект/компонент/ресурс |
| [_button](/Users/admin/IncrementalRPG/Assets/Scripts/UI/DungeonMapButtonView.cs:12) | Button | — | Ссылка на объект/компонент/ресурс |
| [_buttonGlow](/Users/admin/IncrementalRPG/Assets/Scripts/UI/DungeonMapButtonView.cs:14) | GameObject | — | Ссылка на объект/компонент/ресурс |
| [_mapSectionGlow](/Users/admin/IncrementalRPG/Assets/Scripts/UI/DungeonMapButtonView.cs:15) | GameObject | — | Ссылка на объект/компонент/ресурс |
| [_mapSectionFrameGlow](/Users/admin/IncrementalRPG/Assets/Scripts/UI/DungeonMapButtonView.cs:16) | GameObject | — | Ссылка на объект/компонент/ресурс |

<a id="dungeonmenuview"></a>

### DungeonMenuView

[Назначение: руководство по анимациям, интерфейсу и звуку](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/UI/DungeonMenuView.cs).

Где редактировать:

- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:15562) → `MenuCanvas/MapView`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [_dungeonButtons](/Users/admin/IncrementalRPG/Assets/Scripts/UI/DungeonMenuView.cs:10) | DungeonMapButtonView[] | — | Список; элементы раскрываются в Inspector |
| [_infoPanel](/Users/admin/IncrementalRPG/Assets/Scripts/UI/DungeonMenuView.cs:11) | DungeonInfoPanelView | — | Ссылка на объект/компонент/ресурс |
| [_closeButton](/Users/admin/IncrementalRPG/Assets/Scripts/UI/DungeonMenuView.cs:12) | Button | — | Ссылка на объект/компонент/ресурс |

<a id="enemyicontooltiptrigger"></a>

### EnemyIconTooltipTrigger

[Назначение: руководство по анимациям, интерфейсу и звуку](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/UI/EnemyIconTooltipTrigger.cs).

Где редактировать:

- [EntitiContainer.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/DungeonMap/EntitiContainer.prefab:79) → `EntitiContainer`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [_tooltipView](/Users/admin/IncrementalRPG/Assets/Scripts/UI/EnemyIconTooltipTrigger.cs:9) | TooltipView | — | Ссылка на объект/компонент/ресурс |

<a id="entityconfig"></a>

### EntityConfig

[Назначение: 6. Существа](/Users/admin/IncrementalRPG/Docs/GameplaySettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/Entity/EntityConfig.cs).

Где редактировать:

- [Slime.asset](/Users/admin/IncrementalRPG/Assets/SO/Dungeons/0_RocksAndLava/Entities/Slime.asset:3) → `Slime`
- [Skeleton.asset](/Users/admin/IncrementalRPG/Assets/SO/Dungeons/0_RocksAndLava/Entities/Skeleton.asset:3) → `Skeleton`
- [Crystal_1.asset](/Users/admin/IncrementalRPG/Assets/SO/Dungeons/0_RocksAndLava/Entities/Crystal_1.asset:3) → `Crystal_1`
- [Demon.asset](/Users/admin/IncrementalRPG/Assets/SO/Dungeons/0_RocksAndLava/Entities/Demon.asset:3) → `Demon`
- [Crystal_2.asset](/Users/admin/IncrementalRPG/Assets/SO/Dungeons/0_RocksAndLava/Entities/Crystal_2.asset:3) → `Crystal_2`
- [Bombs.asset](/Users/admin/IncrementalRPG/Assets/SO/Dungeons/0_RocksAndLava/Entities/Bombs.asset:3) → `Bombs`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [entityName](/Users/admin/IncrementalRPG/Assets/Scripts/Entity/EntityConfig.cs:26) | LocalizedString | `new()` | Таблица локализации + ключ |
| [description](/Users/admin/IncrementalRPG/Assets/Scripts/Entity/EntityConfig.cs:27) | LocalizedString | `new()` | Таблица локализации + ключ |
| [icon](/Users/admin/IncrementalRPG/Assets/Scripts/Entity/EntityConfig.cs:28) | Sprite | — | Ссылка на объект/компонент/ресурс |
| [maxHP](/Users/admin/IncrementalRPG/Assets/Scripts/Entity/EntityConfig.cs:29) | BigDouble | — | Значение |
| [entityKind](/Users/admin/IncrementalRPG/Assets/Scripts/Entity/EntityConfig.cs:32) | EntityKind | — | Выбор из списка |
| [shardDrop](/Users/admin/IncrementalRPG/Assets/Scripts/Entity/EntityConfig.cs:33) | BigDouble | — | Значение |
| [goldDrop](/Users/admin/IncrementalRPG/Assets/Scripts/Entity/EntityConfig.cs:34) | BigDouble | — | Значение |
| [xpReward](/Users/admin/IncrementalRPG/Assets/Scripts/Entity/EntityConfig.cs:35) | BigDouble | `BigDouble.One` | Значение |
| [countsAsEnemyKill](/Users/admin/IncrementalRPG/Assets/Scripts/Entity/EntityConfig.cs:36) | bool | `true` | Значение |
| [featureType](/Users/admin/IncrementalRPG/Assets/Scripts/Entity/EntityConfig.cs:38) | FeatureType | — | Выбор из списка |
| [viewPrefab](/Users/admin/IncrementalRPG/Assets/Scripts/Entity/EntityConfig.cs:39) | GameObject | — | Ссылка на объект/компонент/ресурс |
| [canMove](/Users/admin/IncrementalRPG/Assets/Scripts/Entity/EntityConfig.cs:42) | bool | — | Значение |
| [moveChance](/Users/admin/IncrementalRPG/Assets/Scripts/Entity/EntityConfig.cs:43) | float | `0.05f` | Значение; Range(0f, 1f) |
| [moveCheckInterval](/Users/admin/IncrementalRPG/Assets/Scripts/Entity/EntityConfig.cs:44) | float | `3f` | Значение; Min(0.01f) |
| [moveSpeed](/Users/admin/IncrementalRPG/Assets/Scripts/Entity/EntityConfig.cs:46) | float | `1f` | Значение; Min(0.01f) |
| [damageZoneHitRadius](/Users/admin/IncrementalRPG/Assets/Scripts/Entity/EntityConfig.cs:49) | float | `0.25f` | Значение; Min(0f) |
| [drawDamageZoneHitAreaGizmo](/Users/admin/IncrementalRPG/Assets/Scripts/Entity/EntityConfig.cs:52) | bool | `true` | Значение |
| [damageZoneHitAreaGizmoColor](/Users/admin/IncrementalRPG/Assets/Scripts/Entity/EntityConfig.cs:53) | Color | `new Color(1f, 0.85f, 0f, 0.8f)` | Значение |
| [damageSound](/Users/admin/IncrementalRPG/Assets/Scripts/Entity/EntityConfig.cs:56) | AudioClip | — | Ссылка на объект/компонент/ресурс |
| [deathSounds](/Users/admin/IncrementalRPG/Assets/Scripts/Entity/EntityConfig.cs:57) | AudioClip[] | — | Список; элементы раскрываются в Inspector |

<a id="gamesceneinstaller"></a>

### GameSceneInstaller

[Назначение: 1. Подключение конфигов](/Users/admin/IncrementalRPG/Docs/GameplaySettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/Reflex/GameSceneInstaller.cs).

Где редактировать:

- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:41907) → `--Reflex--/SceneScope`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [_dungeonList](/Users/admin/IncrementalRPG/Assets/Scripts/Reflex/GameSceneInstaller.cs:28) | DungeonList | — | Ссылка на объект/компонент/ресурс |
| [_levelTransitionConfig](/Users/admin/IncrementalRPG/Assets/Scripts/Reflex/GameSceneInstaller.cs:29) | DungeonLevelTransitionConfig | `new()` | Вложенные настройки |
| [_damageZoneConfig](/Users/admin/IncrementalRPG/Assets/Scripts/Reflex/GameSceneInstaller.cs:30) | DamageZoneConfig | — | Ссылка на объект/компонент/ресурс |
| [_bombExplosionConfig](/Users/admin/IncrementalRPG/Assets/Scripts/Reflex/GameSceneInstaller.cs:31) | BombExplosionConfig | — | Ссылка на объект/компонент/ресурс |
| [_shardPickupConfig](/Users/admin/IncrementalRPG/Assets/Scripts/Reflex/GameSceneInstaller.cs:32) | ShardPickupConfig | — | Ссылка на объект/компонент/ресурс |
| [_skillTreeConfig](/Users/admin/IncrementalRPG/Assets/Scripts/Reflex/GameSceneInstaller.cs:33) | SkillTreeConfig | — | Ссылка на объект/компонент/ресурс |
| [_nodeBorderColorConfig](/Users/admin/IncrementalRPG/Assets/Scripts/Reflex/GameSceneInstaller.cs:34) | NodeBorderColorConfig | — | Ссылка на объект/компонент/ресурс |
| [_nodeCircleSpriteConfig](/Users/admin/IncrementalRPG/Assets/Scripts/Reflex/GameSceneInstaller.cs:35) | NodeCircleSpriteConfig | — | Ссылка на объект/компонент/ресурс |
| [_audioManager](/Users/admin/IncrementalRPG/Assets/Scripts/Reflex/GameSceneInstaller.cs:37) | AudioManager | — | Ссылка на объект/компонент/ресурс |
| [_isometricGradientTilemapGenerator](/Users/admin/IncrementalRPG/Assets/Scripts/Reflex/GameSceneInstaller.cs:38) | IsometricGradientTilemapGenerator | — | Ссылка на объект/компонент/ресурс |
| [_damageZoneView](/Users/admin/IncrementalRPG/Assets/Scripts/Reflex/GameSceneInstaller.cs:40) | DamageZoneView | — | Ссылка на объект/компонент/ресурс |
| [_skillTreeView](/Users/admin/IncrementalRPG/Assets/Scripts/Reflex/GameSceneInstaller.cs:41) | SkillTreeView | — | Ссылка на объект/компонент/ресурс |
| [_hubView](/Users/admin/IncrementalRPG/Assets/Scripts/Reflex/GameSceneInstaller.cs:42) | HubView | — | Ссылка на объект/компонент/ресурс |
| [_menuBackdropView](/Users/admin/IncrementalRPG/Assets/Scripts/Reflex/GameSceneInstaller.cs:43) | MenuBackdropView | — | Ссылка на объект/компонент/ресурс |
| [_menuCanvasView](/Users/admin/IncrementalRPG/Assets/Scripts/Reflex/GameSceneInstaller.cs:44) | MenuCanvasView | — | Ссылка на объект/компонент/ресурс |
| [_hudView](/Users/admin/IncrementalRPG/Assets/Scripts/Reflex/GameSceneInstaller.cs:45) | HudView | — | Ссылка на объект/компонент/ресурс |
| [_pauseMenuController](/Users/admin/IncrementalRPG/Assets/Scripts/Reflex/GameSceneInstaller.cs:46) | PauseMenuController | — | Ссылка на объект/компонент/ресурс |
| [_sessionEndPopupView](/Users/admin/IncrementalRPG/Assets/Scripts/Reflex/GameSceneInstaller.cs:47) | SessionEndPopupView | — | Ссылка на объект/компонент/ресурс |
| [_demoEndPopupView](/Users/admin/IncrementalRPG/Assets/Scripts/Reflex/GameSceneInstaller.cs:48) | DemoEndPopupView | — | Ссылка на объект/компонент/ресурс |
| [_inventoryView](/Users/admin/IncrementalRPG/Assets/Scripts/Reflex/GameSceneInstaller.cs:49) | PlayerInventoryView | — | Ссылка на объект/компонент/ресурс |
| [_itemCatalog](/Users/admin/IncrementalRPG/Assets/Scripts/Reflex/GameSceneInstaller.cs:50) | ItemCatalog | — | Ссылка на объект/компонент/ресурс |

<a id="gamesettingsdata"></a>

### GameSettingsData

[Назначение: руководство по анимациям, интерфейсу и звуку](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/Settings/GameSettingsData.cs).

Начальное состояние пользовательских настроек задаётся в коде и затем хранится в пользовательском сохранении; отдельного asset нет.

Прямое размещение в сохранённых игровых сценах/префабах/конфигах не найдено; см. назначение и вложенность выше.

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [Version](/Users/admin/IncrementalRPG/Assets/Scripts/Settings/GameSettingsData.cs:9) | int | `1` | Значение |
| [MasterVolume](/Users/admin/IncrementalRPG/Assets/Scripts/Settings/GameSettingsData.cs:10) | float | `1f` | Значение |
| [MusicVolume](/Users/admin/IncrementalRPG/Assets/Scripts/Settings/GameSettingsData.cs:11) | float | `1f` | Значение |
| [SfxVolume](/Users/admin/IncrementalRPG/Assets/Scripts/Settings/GameSettingsData.cs:12) | float | `1f` | Значение |
| [LocaleCode](/Users/admin/IncrementalRPG/Assets/Scripts/Settings/GameSettingsData.cs:13) | string | `string.Empty` | Значение |

<a id="goldpopupview"></a>

### GoldPopupView

[Назначение: руководство по анимациям, интерфейсу и звуку](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/UI/GoldPopupView.cs).

Где редактировать:

- [GoldPopupView.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/HUD/GoldPopupView.prefab:177) → `GoldPopupView`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [_text](/Users/admin/IncrementalRPG/Assets/Scripts/UI/GoldPopupView.cs:11) | TMP_Text | — | Ссылка на объект/компонент/ресурс |

<a id="hubfeaturebuttonview"></a>

### HubFeatureButtonView

[Назначение: руководство по анимациям, интерфейсу и звуку](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/UI/HubFeatureButtonView.cs).

Где редактировать:

- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:3307) → `MenuCanvas/HubPanel/Plateau/MineButton`
- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:8876) → `MenuCanvas/HubPanel/Plateau/CarrotShip`
- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:9200) → `MenuCanvas/HubPanel/Plateau/CraftButton`
- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:27278) → `MenuCanvas/HubPanel/Plateau/SkillTreeButton`
- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:28736) → `MenuCanvas/HubPanel/Plateau/DungeonButton`
- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:46681) → `MenuCanvas/HubPanel/Plateau/BarracksButton`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [_button](/Users/admin/IncrementalRPG/Assets/Scripts/UI/HubFeatureButtonView.cs:11) | Button | — | Ссылка на объект/компонент/ресурс |
| [_glowImage](/Users/admin/IncrementalRPG/Assets/Scripts/UI/HubFeatureButtonView.cs:12) | Image | — | Ссылка на объект/компонент/ресурс |
| [_text](/Users/admin/IncrementalRPG/Assets/Scripts/UI/HubFeatureButtonView.cs:13) | TMP_Text | — | Ссылка на объект/компонент/ресурс |
| [_hoverSkeleton](/Users/admin/IncrementalRPG/Assets/Scripts/UI/HubFeatureButtonView.cs:14) | SkeletonGraphic | — | Ссылка на объект/компонент/ресурс |
| [_idleAnimationName](/Users/admin/IncrementalRPG/Assets/Scripts/UI/HubFeatureButtonView.cs:15) | string | `"idle"` | Значение |
| [_hoverAnimationName](/Users/admin/IncrementalRPG/Assets/Scripts/UI/HubFeatureButtonView.cs:16) | string | `"hover"` | Значение |
| [_idleAnimationLoop](/Users/admin/IncrementalRPG/Assets/Scripts/UI/HubFeatureButtonView.cs:17) | bool | `true` | Значение |
| [_hoverAnimationLoop](/Users/admin/IncrementalRPG/Assets/Scripts/UI/HubFeatureButtonView.cs:18) | bool | `true` | Значение |

<a id="hublabelview"></a>

### HubLabelView

[Назначение: руководство по анимациям, интерфейсу и звуку](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/UI/HubLabelView.cs).

Где редактировать:

- [HubLabel.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/Hub/HubLabel.prefab:294) → `HubLabel`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [_text](/Users/admin/IncrementalRPG/Assets/Scripts/UI/HubLabelView.cs:11) | TMP_Text | — | Ссылка на объект/компонент/ресурс |
| [_backdrop](/Users/admin/IncrementalRPG/Assets/Scripts/UI/HubLabelView.cs:12) | Image | — | Ссылка на объект/компонент/ресурс |
| [_padding](/Users/admin/IncrementalRPG/Assets/Scripts/UI/HubLabelView.cs:14) | Vector2 | `new(40f, 28f)` | Значение |

<a id="hubview"></a>

### HubView

[Назначение: руководство по анимациям, интерфейсу и звуку](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/UI/HubView.cs).

Где редактировать:

- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:48923) → `MenuCanvas/HubPanel`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [_dungeonButton](/Users/admin/IncrementalRPG/Assets/Scripts/UI/HubView.cs:13) | HubFeatureButtonView | — | Ссылка на объект/компонент/ресурс |
| [_skillTreeButton](/Users/admin/IncrementalRPG/Assets/Scripts/UI/HubView.cs:14) | HubFeatureButtonView | — | Ссылка на объект/компонент/ресурс |
| [_inventoryButton](/Users/admin/IncrementalRPG/Assets/Scripts/UI/HubView.cs:15) | HubFeatureButtonView | — | Ссылка на объект/компонент/ресурс |
| [_barracksButton](/Users/admin/IncrementalRPG/Assets/Scripts/UI/HubView.cs:16) | HubFeatureButtonView | — | Ссылка на объект/компонент/ресурс |
| [_mineButton](/Users/admin/IncrementalRPG/Assets/Scripts/UI/HubView.cs:17) | HubFeatureButtonView | — | Ссылка на объект/компонент/ресурс |
| [_craftButton](/Users/admin/IncrementalRPG/Assets/Scripts/UI/HubView.cs:18) | HubFeatureButtonView | — | Ссылка на объект/компонент/ресурс |
| [_dungeonMenuView](/Users/admin/IncrementalRPG/Assets/Scripts/UI/HubView.cs:19) | DungeonMenuView | — | Ссылка на объект/компонент/ресурс |
| [_mapMenuFadeTransition](/Users/admin/IncrementalRPG/Assets/Scripts/UI/HubView.cs:20) | MapMenuFadeTransition | — | Ссылка на объект/компонент/ресурс |
| [_mapOpenSound](/Users/admin/IncrementalRPG/Assets/Scripts/UI/HubView.cs:23) | AudioClip | — | Ссылка на объект/компонент/ресурс |
| [_skillTreeOpenSound](/Users/admin/IncrementalRPG/Assets/Scripts/UI/HubView.cs:24) | AudioClip | — | Ссылка на объект/компонент/ресурс |
| [_barracksOpenSound](/Users/admin/IncrementalRPG/Assets/Scripts/UI/HubView.cs:25) | AudioClip | — | Ссылка на объект/компонент/ресурс |
| [_mineOpenSound](/Users/admin/IncrementalRPG/Assets/Scripts/UI/HubView.cs:26) | AudioClip | — | Ссылка на объект/компонент/ресурс |
| [_craftOpenSound](/Users/admin/IncrementalRPG/Assets/Scripts/UI/HubView.cs:27) | AudioClip | — | Ссылка на объект/компонент/ресурс |

<a id="hudview"></a>

### HudView

[Назначение: руководство по анимациям, интерфейсу и звуку](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/UI/HudView.cs).

Константы скорости HUD и полёта осколков, а также неиспользуемое текущей шторкой поле fade описаны в руководстве аниматора.

Где редактировать:

- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:19216) → `HUD`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [_sessionGoldText](/Users/admin/IncrementalRPG/Assets/Scripts/UI/HudView.cs:23) | TMP_Text | — | Ссылка на объект/компонент/ресурс |
| [_shardText](/Users/admin/IncrementalRPG/Assets/Scripts/UI/HudView.cs:24) | TMP_Text | — | Ссылка на объект/компонент/ресурс |
| [_shardCounterRoot](/Users/admin/IncrementalRPG/Assets/Scripts/UI/HudView.cs:25) | GameObject | — | Ссылка на объект/компонент/ресурс |
| [_killsText](/Users/admin/IncrementalRPG/Assets/Scripts/UI/HudView.cs:26) | TMP_Text | — | Ссылка на объект/компонент/ресурс |
| [_experienceText](/Users/admin/IncrementalRPG/Assets/Scripts/UI/HudView.cs:27) | TMP_Text | — | Ссылка на объект/компонент/ресурс |
| [_dungeonLevelText](/Users/admin/IncrementalRPG/Assets/Scripts/UI/HudView.cs:28) | TMP_Text | — | Ссылка на объект/компонент/ресурс |
| [_popupPrefab](/Users/admin/IncrementalRPG/Assets/Scripts/UI/HudView.cs:29) | GoldPopupView | — | Ссылка на объект/компонент/ресурс |
| [_popupContainer](/Users/admin/IncrementalRPG/Assets/Scripts/UI/HudView.cs:30) | RectTransform | — | Ссылка на объект/компонент/ресурс |
| [_levelTransitionCurtain](/Users/admin/IncrementalRPG/Assets/Scripts/UI/HudView.cs:31) | LevelTransitionCurtainView | — | Ссылка на объект/компонент/ресурс |
| [_lootboxView](/Users/admin/IncrementalRPG/Assets/Scripts/UI/HudView.cs:32) | LootboxView | — | Ссылка на объект/компонент/ресурс |
| [_levelTransitionGroup](/Users/admin/IncrementalRPG/Assets/Scripts/UI/HudView.cs:33) | CanvasGroup | — | Ссылка на объект/компонент/ресурс |
| [_levelTransitionText](/Users/admin/IncrementalRPG/Assets/Scripts/UI/HudView.cs:34) | TMP_Text | — | Ссылка на объект/компонент/ресурс |
| [_dungeonLevelFormat](/Users/admin/IncrementalRPG/Assets/Scripts/UI/HudView.cs:35) | LocalizedString | `new()` | Таблица локализации + ключ |
| [_levelTransitionMessage](/Users/admin/IncrementalRPG/Assets/Scripts/UI/HudView.cs:36) | LocalizedString | `new()` | Таблица локализации + ключ |
| [_levelTransitionFadeDuration](/Users/admin/IncrementalRPG/Assets/Scripts/UI/HudView.cs:37) | float | `0.25f` | Значение |

<a id="intropanelplayer"></a>

### IntroPanelPlayer

[Назначение: руководство по анимациям, интерфейсу и звуку](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/UI/IntroPanelPlayer.cs).

Где редактировать:

- [MainMenuScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/MainMenuScene.unity:2728) → `FrontCanvas/IntroPanel`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [_panel](/Users/admin/IncrementalRPG/Assets/Scripts/UI/IntroPanelPlayer.cs:13) | GameObject | — | Ссылка на объект/компонент/ресурс |
| [_videoDisplay](/Users/admin/IncrementalRPG/Assets/Scripts/UI/IntroPanelPlayer.cs:14) | GameObject | — | Ссылка на объект/компонент/ресурс |
| [_skipButton](/Users/admin/IncrementalRPG/Assets/Scripts/UI/IntroPanelPlayer.cs:15) | Button | — | Ссылка на объект/компонент/ресурс |
| [_hidePanelOnAwake](/Users/admin/IncrementalRPG/Assets/Scripts/UI/IntroPanelPlayer.cs:16) | bool | `true` | Значение |
| [_hidePanelOnComplete](/Users/admin/IncrementalRPG/Assets/Scripts/UI/IntroPanelPlayer.cs:17) | bool | — | Значение |
| [_aspectRatioFitter](/Users/admin/IncrementalRPG/Assets/Scripts/UI/IntroPanelPlayer.cs:18) | AspectRatioFitter | — | Ссылка на объект/компонент/ресурс |
| [_videoPlayer](/Users/admin/IncrementalRPG/Assets/Scripts/UI/IntroPanelPlayer.cs:21) | VideoPlayer | — | Ссылка на объект/компонент/ресурс |
| [_audioSource](/Users/admin/IncrementalRPG/Assets/Scripts/UI/IntroPanelPlayer.cs:22) | AudioSource | — | Ссылка на объект/компонент/ресурс |
| [_audioClip](/Users/admin/IncrementalRPG/Assets/Scripts/UI/IntroPanelPlayer.cs:23) | AudioClip | — | Ссылка на объект/компонент/ресурс |
| [_allowInputSkip](/Users/admin/IncrementalRPG/Assets/Scripts/UI/IntroPanelPlayer.cs:26) | bool | `true` | Значение |
| [_prepareTimeout](/Users/admin/IncrementalRPG/Assets/Scripts/UI/IntroPanelPlayer.cs:27) | float | `5f` | Значение; Min(0f) |

<a id="isometricgradienttilemapgenerator"></a>

### IsometricGradientTilemapGenerator

[Назначение: 8. Генератор](/Users/admin/IncrementalRPG/Docs/GameplaySettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/TilemapGenerator/IsometricGradientTilemapGenerator.cs).

`config` и `size` при старте переопределяет текущий уровень.

Где редактировать:

- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:42754) → `MapGenerator`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [targetTilemap](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/TilemapGenerator/IsometricGradientTilemapGenerator.cs:10) | Tilemap | — | Ссылка на объект/компонент/ресурс |
| [leftPillarTilemap](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/TilemapGenerator/IsometricGradientTilemapGenerator.cs:11) | Tilemap | — | Ссылка на объект/компонент/ресурс |
| [rightPillarTilemap](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/TilemapGenerator/IsometricGradientTilemapGenerator.cs:12) | Tilemap | — | Ссылка на объект/компонент/ресурс |
| [cameraAutoFitter](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/TilemapGenerator/IsometricGradientTilemapGenerator.cs:13) | TilemapCameraAutoFitter | — | Ссылка на объект/компонент/ресурс |
| [config](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/TilemapGenerator/IsometricGradientTilemapGenerator.cs:16) | TilemapGenerationConfig | — | Ссылка на объект/компонент/ресурс |
| [size](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/TilemapGenerator/IsometricGradientTilemapGenerator.cs:20) | int | `16` | Значение; Min(1) |
| [origin](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/TilemapGenerator/IsometricGradientTilemapGenerator.cs:21) | Vector3Int | `Vector3Int.zero` | Значение |
| [autoFitCameraAfterGenerate](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/TilemapGenerator/IsometricGradientTilemapGenerator.cs:24) | bool | `true` | Значение |

<a id="itemcatalog"></a>

### ItemCatalog

[Назначение: 10. Каталог предметов](/Users/admin/IncrementalRPG/Docs/GameplaySettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Items/ItemCatalog.cs).

Где редактировать:

- [ItemCatalog.asset](/Users/admin/IncrementalRPG/Assets/SO/Items/ItemCatalog.asset:3) → `ItemCatalog`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [_items](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Items/ItemCatalog.cs:9) | ItemDefinition[] | — | Список; элементы раскрываются в Inspector |

<a id="itemdefinition"></a>

### ItemDefinition

Семь характеристик брони, формат долей, слоты и влияние правок на созданные экземпляры: [настройка экипировки](/Users/admin/IncrementalRPG/Docs/GameplaySettings.ru.md#equipment-stats). Шлем 1×1, нагрудник 2×2, оружие 1×2, без стопок.

[Назначение: 10. Предметы](/Users/admin/IncrementalRPG/Docs/GameplaySettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Items/ItemDefinition.cs).

Где редактировать:

- [WealthPotion.asset](/Users/admin/IncrementalRPG/Assets/SO/Items/WealthPotion.asset:49) → `WealthPotion`
- [LargeWealthPotion.asset](/Users/admin/IncrementalRPG/Assets/SO/Items/LargeWealthPotion.asset:49) → `LargeWealthPotion`
- [RagePotion.asset](/Users/admin/IncrementalRPG/Assets/SO/Items/RagePotion.asset:49) → `RagePotion`
- [LargeRagePotion.asset](/Users/admin/IncrementalRPG/Assets/SO/Items/LargeRagePotion.asset:49) → `LargeRagePotion`
- [ConcentrationPotion.asset](/Users/admin/IncrementalRPG/Assets/SO/Items/ConcentrationPotion.asset:49) → `ConcentrationPotion`

- [GoldReward.asset](/Users/admin/IncrementalRPG/Assets/SO/Items/GoldReward.asset) → `GoldReward`
- [ShardReward.asset](/Users/admin/IncrementalRPG/Assets/SO/Items/ShardReward.asset) → `ShardReward`
- [PrecisionScroll.asset](/Users/admin/IncrementalRPG/Assets/SO/Items/PrecisionScroll.asset) → `PrecisionScroll`
- [HelmetScroll.asset](/Users/admin/IncrementalRPG/Assets/SO/Items/HelmetScroll.asset) → `HelmetScroll`
- [ChestScroll.asset](/Users/admin/IncrementalRPG/Assets/SO/Items/ChestScroll.asset) → `ChestScroll`
- [WeaponScroll.asset](/Users/admin/IncrementalRPG/Assets/SO/Items/WeaponScroll.asset) → `WeaponScroll`
- [RarityScroll.asset](/Users/admin/IncrementalRPG/Assets/SO/Items/RarityScroll.asset) → `RarityScroll`
- [EmpowermentScroll.asset](/Users/admin/IncrementalRPG/Assets/SO/Items/EmpowermentScroll.asset) → `EmpowermentScroll`

Для пяти зелий [текущие значения, единицы, идентификаторы и ограничения](/Users/admin/IncrementalRPG/Docs/GameplaySettings.ru.md#potion-test-settings) собраны в руководстве геймдизайнера. Их `effectValue`: богатство/ярость — `0.25`, большие версии — `0.5`, концентрация — `0.2` сокращения длительности перезарядки (×0.8). Продолжительность эффекта — один ближайший уровень; в Inspector она не настраивается.

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [defaultStats[].statId](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Items/ItemDefinition.cs:39) | string | — | Выпадающий список семи ID из EquipmentStats; произвольные ID не поддерживаются |
| [defaultStats[].value](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Items/ItemDefinition.cs:41) | float | — | Неотрицательная доля: 0.10 = +10%; конечное число |
| [itemId](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Items/ItemDefinition.cs:48) | string | — | Значение |
| [displayName](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Items/ItemDefinition.cs:49) | string | — | Значение |
| [description](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Items/ItemDefinition.cs:50) | string | — | Значение |
| [localizedName](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Items/ItemDefinition.cs:51) | LocalizedString | `new()` | Ссылка на локализованное название |
| [localizedDescription](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Items/ItemDefinition.cs:52) | LocalizedString | `new()` | Ссылка на описание; сила зелья передаётся аргументом `{0}` |
| [icon](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Items/ItemDefinition.cs:53) | Sprite | — | Ссылка на объект/компонент/ресурс |
| [category](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Items/ItemDefinition.cs:54) | ItemCategory | — | Выбор из списка |
| [stackable](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Items/ItemDefinition.cs:57) | bool | — | Значение |
| [maxStackSize](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Items/ItemDefinition.cs:58) | int | `1` | Значение; Min(1) |
| [width](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Items/ItemDefinition.cs:59) | int | `1` | Значение; Min(1) |
| [height](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Items/ItemDefinition.cs:60) | int | `1` | Значение; Min(1) |
| [sellPrice](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Items/ItemDefinition.cs:61) | BigDouble | `BigDouble.Zero` | Значение |
| [rarity](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Items/ItemDefinition.cs:64) | ItemRarity | — | Выбор из списка |
| [equipmentSlot](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Items/ItemDefinition.cs:65) | EquipmentSlot | — | Выбор из списка |
| [defaultStats](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Items/ItemDefinition.cs:66) | ItemStatDefinition[] | `Array.Empty<ItemStatDefinition>()` | Список; элементы раскрываются в Inspector |
| [effectId](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Items/ItemDefinition.cs:69) | string | — | Идентификатор эффекта; общий для обычного и большого зелья одного семейства |
| [effectValue](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Items/ItemDefinition.cs:70) | float | — | У пяти зелий доля: 0.25 = 25%; у концентрации доля сокращения времени. Недопустимое значение блокирует применение; Inspector не ограничивает ввод |
| [forgeModifierId](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Items/ItemDefinition.cs:73) | string | — | Тип модификатора свитка; список ID в руководстве геймдизайнера |
| [forgeModifierValue](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Items/ItemDefinition.cs:74) | float | — | Доля для точности и усиления: 0.02 = 2%; передаётся в локализованное описание |
| [currency](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Items/ItemDefinition.cs:77) | RewardCurrency | Gold | Для category = Currency: Gold / Shards; сумма задаётся в записи пула уровня |

Свитки: [параметры и веса](/Users/admin/IncrementalRPG/Docs/GameplaySettings.ru.md#scroll-rewards).

<a id="levellootpool"></a>

### LevelLootPool

[Назначение: 10. Награды](/Users/admin/IncrementalRPG/Docs/GameplaySettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/Dungeon/LevelLootPool.cs).

Вложено в `DungeonLevelConfig.lootPool`.

Прямое размещение в сохранённых игровых сценах/префабах/конфигах не найдено; см. назначение и вложенность выше.

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [entries[].item](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/Dungeon/LevelLootPool.cs:12) | ItemDefinition | — | Ссылка на объект/компонент/ресурс |
| [entries[].weight](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/Dungeon/LevelLootPool.cs:13) | float | `1f` | Относительный вес; Min(0f). Нулевой вес отключает награду |
| [entries[].currencyAmount](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/Dungeon/LevelLootPool.cs:15) | BigDouble | `BigDouble.One` | Количество золота/шардов: положительное целое. Для предметов игнорируется |
| [entries](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/Dungeon/LevelLootPool.cs:26) | WeightedLootEntry[] | — | Список; элементы раскрываются в Inspector |

<a id="leveltransitioncurtainview"></a>

### LevelTransitionCurtainView

[Назначение: руководство по анимациям, интерфейсу и звуку](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/UI/LevelTransitionCurtainView.cs).

Где редактировать:

- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:17216) → `HUD/LevelTransitionCurtainView`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [_leftCurtain](/Users/admin/IncrementalRPG/Assets/Scripts/UI/LevelTransitionCurtainView.cs:15) | RectTransform | — | Ссылка на объект/компонент/ресурс |
| [_rightCurtain](/Users/admin/IncrementalRPG/Assets/Scripts/UI/LevelTransitionCurtainView.cs:18) | RectTransform | — | Ссылка на объект/компонент/ресурс |
| [_curtainViewport](/Users/admin/IncrementalRPG/Assets/Scripts/UI/LevelTransitionCurtainView.cs:20) | RectTransform | — | Ссылка на объект/компонент/ресурс |
| [_rootGroup](/Users/admin/IncrementalRPG/Assets/Scripts/UI/LevelTransitionCurtainView.cs:21) | CanvasGroup | — | Ссылка на объект/компонент/ресурс |
| [_revealGroup](/Users/admin/IncrementalRPG/Assets/Scripts/UI/LevelTransitionCurtainView.cs:24) | CanvasGroup | — | Ссылка на объект/компонент/ресурс |
| [_lampCounter](/Users/admin/IncrementalRPG/Assets/Scripts/UI/LevelTransitionCurtainView.cs:26) | LevelTransitionLampCounterView | — | Ссылка на объект/компонент/ресурс |
| [_movementCurve](/Users/admin/IncrementalRPG/Assets/Scripts/UI/LevelTransitionCurtainView.cs:27) | AnimationCurve | `AnimationCurve.EaseInOut(0f, 0f, 1f, 1f)` | Значение |
| [_revealFadeInDuration](/Users/admin/IncrementalRPG/Assets/Scripts/UI/LevelTransitionCurtainView.cs:28) | float | `0.2f` | Значение; Min(0f) |
| [_lampAnimationDelay](/Users/admin/IncrementalRPG/Assets/Scripts/UI/LevelTransitionCurtainView.cs:29) | float | `0.2f` | Значение; Min(0f) |
| [_revealFadeOutDuration](/Users/admin/IncrementalRPG/Assets/Scripts/UI/LevelTransitionCurtainView.cs:30) | float | `0.2f` | Значение; Min(0f) |
| [_offscreenPadding](/Users/admin/IncrementalRPG/Assets/Scripts/UI/LevelTransitionCurtainView.cs:31) | float | `8f` | Значение; Min(0f) |
| [_closedOverlap](/Users/admin/IncrementalRPG/Assets/Scripts/UI/LevelTransitionCurtainView.cs:32) | float | `80f` | Значение; Min(0f) |
| [_seamOffset](/Users/admin/IncrementalRPG/Assets/Scripts/UI/LevelTransitionCurtainView.cs:33) | float | — | Значение |
| [_hideWhenIdle](/Users/admin/IncrementalRPG/Assets/Scripts/UI/LevelTransitionCurtainView.cs:34) | bool | — | Значение |

<a id="leveltransitionlampcounterview"></a>

### LevelTransitionLampCounterView

[Назначение: руководство по анимациям, интерфейсу и звуку](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/UI/LevelTransitionLampCounterView.cs).

Где редактировать:

- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:47091) → `HUD/LevelTransitionCurtainView/RevealGroup/LevelCounter`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [_container](/Users/admin/IncrementalRPG/Assets/Scripts/UI/LevelTransitionLampCounterView.cs:14) | RectTransform | — | Ссылка на объект/компонент/ресурс |
| [_lampPrefab](/Users/admin/IncrementalRPG/Assets/Scripts/UI/LevelTransitionLampCounterView.cs:15) | SkeletonGraphic | — | Ссылка на объект/компонент/ресурс |
| [_spacing](/Users/admin/IncrementalRPG/Assets/Scripts/UI/LevelTransitionLampCounterView.cs:16) | float | `-8f` | Значение |
| [_horizontalPadding](/Users/admin/IncrementalRPG/Assets/Scripts/UI/LevelTransitionLampCounterView.cs:17) | float | `40f` | Значение; Min(0f) |
| [_verticalPadding](/Users/admin/IncrementalRPG/Assets/Scripts/UI/LevelTransitionLampCounterView.cs:18) | float | `24f` | Значение; Min(0f) |
| [_idleOffAnimationName](/Users/admin/IncrementalRPG/Assets/Scripts/UI/LevelTransitionLampCounterView.cs:19) | string | `"idle_off"` | Значение |
| [_turnOnAnimationName](/Users/admin/IncrementalRPG/Assets/Scripts/UI/LevelTransitionLampCounterView.cs:20) | string | `"on"` | Значение |
| [_idleOnAnimationName](/Users/admin/IncrementalRPG/Assets/Scripts/UI/LevelTransitionLampCounterView.cs:21) | string | `"idle_on"` | Значение |

<a id="lootrewardpopupview"></a>

### LootRewardPopupView

[Назначение: руководство по анимациям, интерфейсу и звуку](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/UI/LootRewardPopupView.cs).

Где редактировать:

- [LootRewardPopup.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/HUD/LootRewardPopup.prefab:1415) → `LootRewardPopup`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [_group](/Users/admin/IncrementalRPG/Assets/Scripts/UI/LootRewardPopupView.cs:15) | CanvasGroup | — | Ссылка на объект/компонент/ресурс |
| [_composition](/Users/admin/IncrementalRPG/Assets/Scripts/UI/LootRewardPopupView.cs:16) | RectTransform | — | Ссылка на объект/компонент/ресурс |
| [_title](/Users/admin/IncrementalRPG/Assets/Scripts/UI/LootRewardPopupView.cs:17) | TMP_Text | — | Ссылка на объект/компонент/ресурс |
| [_icon](/Users/admin/IncrementalRPG/Assets/Scripts/UI/LootRewardPopupView.cs:18) | Image | — | Ссылка на объект/компонент/ресурс |
| [_status](/Users/admin/IncrementalRPG/Assets/Scripts/UI/LootRewardPopupView.cs:19) | TMP_Text | — | Ссылка на объект/компонент/ресурс |
| [_continueButton](/Users/admin/IncrementalRPG/Assets/Scripts/UI/LootRewardPopupView.cs:20) | Button | — | Ссылка на объект/компонент/ресурс |
| [_useNowButton](/Users/admin/IncrementalRPG/Assets/Scripts/UI/LootRewardPopupView.cs:21) | Button | — | Ссылка на объект/компонент/ресурс |
| [_useNowGroup](/Users/admin/IncrementalRPG/Assets/Scripts/UI/LootRewardPopupView.cs:22) | CanvasGroup | — | Ссылка на объект/компонент/ресурс |
| [_revealEffect](/Users/admin/IncrementalRPG/Assets/Scripts/UI/LootRewardPopupView.cs:23) | RewardRevealEffect | — | Ссылка на объект/компонент/ресурс |

<a id="lootboxview"></a>

### LootboxView

[Назначение: руководство по анимациям, интерфейсу и звуку](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/UI/LootboxView.cs).

Time Scale сундука сбрасывается кодом; скорость ленты задаётся здесь. Состав награды задаёт lootPool уровня.

Где редактировать:

- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:49213) → `HUD/LevelTransitionCurtainView/RevealGroup/LootboxGroup`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [_chest](/Users/admin/IncrementalRPG/Assets/Scripts/UI/LootboxView.cs:19) | SkeletonGraphic | — | Ссылка на объект/компонент/ресурс |
| [_closedIdleAnimationName](/Users/admin/IncrementalRPG/Assets/Scripts/UI/LootboxView.cs:20) | string | `"idle_close"` | Значение |
| [_openAnimationName](/Users/admin/IncrementalRPG/Assets/Scripts/UI/LootboxView.cs:21) | string | `"open"` | Значение |
| [_openIdleAnimationName](/Users/admin/IncrementalRPG/Assets/Scripts/UI/LootboxView.cs:22) | string | `"idle_open"` | Значение |
| [_itemViewport](/Users/admin/IncrementalRPG/Assets/Scripts/UI/LootboxView.cs:25) | RectTransform | — | Ссылка на объект/компонент/ресурс |
| [_ambientIcons](/Users/admin/IncrementalRPG/Assets/Scripts/UI/LootboxView.cs:26) | Sprite[] | — | Резервный список иконок. В переходе лента использует иконки допустимых наград текущего пула |
| [_itemSize](/Users/admin/IncrementalRPG/Assets/Scripts/UI/LootboxView.cs:27) | Vector2 | `new(150f, 150f)` | Значение |
| [_itemSpacing](/Users/admin/IncrementalRPG/Assets/Scripts/UI/LootboxView.cs:28) | float | `190f` | Значение; Min(1f) |
| [_spinSpeed](/Users/admin/IncrementalRPG/Assets/Scripts/UI/LootboxView.cs:29) | float | `2200f` | Значение; Min(1f) |
| [_spinStartDelay](/Users/admin/IncrementalRPG/Assets/Scripts/UI/LootboxView.cs:31) | float | `0.6f` | Значение; Min(0f) |
| [_constantSpinDuration](/Users/admin/IncrementalRPG/Assets/Scripts/UI/LootboxView.cs:32) | float | `1.8f` | Значение; Min(0f) |
| [_settleDuration](/Users/admin/IncrementalRPG/Assets/Scripts/UI/LootboxView.cs:33) | float | `1.2f` | Значение; Min(0.01f) |
| [_winnerEntryPadding](/Users/admin/IncrementalRPG/Assets/Scripts/UI/LootboxView.cs:34) | float | `40f` | Значение; Min(0f) |
| [_resultPopupPrefab](/Users/admin/IncrementalRPG/Assets/Scripts/UI/LootboxView.cs:37) | LootRewardPopupView | — | Ссылка на объект/компонент/ресурс |
| [_resultPopupParent](/Users/admin/IncrementalRPG/Assets/Scripts/UI/LootboxView.cs:38) | RectTransform | — | Ссылка на объект/компонент/ресурс |
| [_transitionLabels](/Users/admin/IncrementalRPG/Assets/Scripts/UI/LootboxView.cs:39) | GameObject[] | — | Список; элементы раскрываются в Inspector |

<a id="mainmenubuttonlayoutscaler"></a>

### MainMenuButtonLayoutScaler

[Назначение: руководство по анимациям, интерфейсу и звуку](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/UI/MainMenuButtonLayoutScaler.cs).

Где редактировать:

- [MainMenuScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/MainMenuScene.unity:407) → `FrontCanvas/Panel`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [_container](/Users/admin/IncrementalRPG/Assets/Scripts/UI/MainMenuButtonLayoutScaler.cs:12) | RectTransform | — | Ссылка на объект/компонент/ресурс |
| [_layoutGroup](/Users/admin/IncrementalRPG/Assets/Scripts/UI/MainMenuButtonLayoutScaler.cs:13) | VerticalLayoutGroup | — | Ссылка на объект/компонент/ресурс |
| [_buttonAspectRatio](/Users/admin/IncrementalRPG/Assets/Scripts/UI/MainMenuButtonLayoutScaler.cs:14) | float | `3.9782813f` | Значение; Min(0.01f) |
| [_minButtonHeight](/Users/admin/IncrementalRPG/Assets/Scripts/UI/MainMenuButtonLayoutScaler.cs:15) | float | `110f` | Значение; Min(1f) |
| [_maxButtonHeight](/Users/admin/IncrementalRPG/Assets/Scripts/UI/MainMenuButtonLayoutScaler.cs:16) | float | `135f` | Значение; Min(1f) |

<a id="mainmenubuttonview"></a>

### MainMenuButtonView

[Назначение: руководство по анимациям, интерфейсу и звуку](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/UI/MainMenuButtonView.cs).

Где редактировать:

- [MainMenuButton.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/MainMenu/UI/MainMenuButton.prefab:724) → `MainMenuButton`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [_action](/Users/admin/IncrementalRPG/Assets/Scripts/UI/MainMenuButtonView.cs:19) | MainMenuAction | — | Выбор из списка |
| [_button](/Users/admin/IncrementalRPG/Assets/Scripts/UI/MainMenuButtonView.cs:20) | Button | — | Ссылка на объект/компонент/ресурс |
| [_hoverVisuals](/Users/admin/IncrementalRPG/Assets/Scripts/UI/MainMenuButtonView.cs:22) | GameObject[] | — | Список; элементы раскрываются в Inspector |
| [_pressTarget](/Users/admin/IncrementalRPG/Assets/Scripts/UI/MainMenuButtonView.cs:23) | RectTransform | — | Ссылка на объект/компонент/ресурс |
| [_pressedScale](/Users/admin/IncrementalRPG/Assets/Scripts/UI/MainMenuButtonView.cs:24) | float | `0.94f` | Значение; Min(0f) |

<a id="mainmenucontroller"></a>

### MainMenuController

[Назначение: руководство по анимациям, интерфейсу и звуку](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/UI/MainMenuController.cs).

Где редактировать:

- [MainMenuScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/MainMenuScene.unity:9838) → `FrontCanvas`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [_view](/Users/admin/IncrementalRPG/Assets/Scripts/UI/MainMenuController.cs:10) | MainMenuView | — | Ссылка на объект/компонент/ресурс |
| [_gameSceneName](/Users/admin/IncrementalRPG/Assets/Scripts/UI/MainMenuController.cs:11) | string | `"GameScene"` | Значение |
| [_deleteSaveOnNewGame](/Users/admin/IncrementalRPG/Assets/Scripts/UI/MainMenuController.cs:12) | bool | `true` | Значение |
| [_audioManager](/Users/admin/IncrementalRPG/Assets/Scripts/UI/MainMenuController.cs:13) | AudioManager | — | Ссылка на объект/компонент/ресурс |
| [_playMusicOnStart](/Users/admin/IncrementalRPG/Assets/Scripts/UI/MainMenuController.cs:14) | bool | `true` | Значение |
| [_introPlayer](/Users/admin/IncrementalRPG/Assets/Scripts/UI/MainMenuController.cs:15) | IntroPanelPlayer | — | Ссылка на объект/компонент/ресурс |

<a id="mainmenuglowanimator"></a>

### MainMenuGlowAnimator

[Назначение: руководство по анимациям, интерфейсу и звуку](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/UI/MainMenuGlowAnimator.cs).

Где редактировать:

- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:33580) → `MenuCanvas/FullScreenMenuBackdrop/Glow`
- [MainMenuScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/MainMenuScene.unity:10488) → `Canvas/Glow`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [_targetGraphic](/Users/admin/IncrementalRPG/Assets/Scripts/UI/MainMenuGlowAnimator.cs:9) | Graphic | — | Ссылка на объект/компонент/ресурс |
| [_targetTransform](/Users/admin/IncrementalRPG/Assets/Scripts/UI/MainMenuGlowAnimator.cs:10) | RectTransform | — | Ссылка на объект/компонент/ресурс |
| [_shadowGraphic](/Users/admin/IncrementalRPG/Assets/Scripts/UI/MainMenuGlowAnimator.cs:11) | Graphic | — | Ссылка на объект/компонент/ресурс |
| [_minAlpha](/Users/admin/IncrementalRPG/Assets/Scripts/UI/MainMenuGlowAnimator.cs:12) | float | `0.45f` | Значение; Range(0f, 1f) |
| [_maxAlpha](/Users/admin/IncrementalRPG/Assets/Scripts/UI/MainMenuGlowAnimator.cs:13) | float | `0.65f` | Значение; Range(0f, 1f) |
| [_shadowMinAlpha](/Users/admin/IncrementalRPG/Assets/Scripts/UI/MainMenuGlowAnimator.cs:14) | float | `0.65f` | Значение; Range(0f, 1f) |
| [_shadowMaxAlpha](/Users/admin/IncrementalRPG/Assets/Scripts/UI/MainMenuGlowAnimator.cs:15) | float | `0.85f` | Значение; Range(0f, 1f) |
| [_invertShadowPulse](/Users/admin/IncrementalRPG/Assets/Scripts/UI/MainMenuGlowAnimator.cs:16) | bool | `true` | Значение |
| [_cycleDuration](/Users/admin/IncrementalRPG/Assets/Scripts/UI/MainMenuGlowAnimator.cs:17) | float | `4f` | Значение; Min(0.01f) |
| [_scalePulse](/Users/admin/IncrementalRPG/Assets/Scripts/UI/MainMenuGlowAnimator.cs:18) | float | `0.03f` | Значение; Min(0f) |
| [_useUnscaledTime](/Users/admin/IncrementalRPG/Assets/Scripts/UI/MainMenuGlowAnimator.cs:19) | bool | `true` | Значение |
| [_randomizeStartPhase](/Users/admin/IncrementalRPG/Assets/Scripts/UI/MainMenuGlowAnimator.cs:20) | bool | `true` | Значение |

<a id="mainmenuview"></a>

### MainMenuView

[Назначение: руководство по анимациям, интерфейсу и звуку](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/UI/MainMenuView.cs).

Где редактировать:

- [MainMenuScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/MainMenuScene.unity:9818) → `FrontCanvas`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [_buttons](/Users/admin/IncrementalRPG/Assets/Scripts/UI/MainMenuView.cs:10) | MainMenuButtonView[] | — | Список; элементы раскрываются в Inspector |
| [_settingsMenu](/Users/admin/IncrementalRPG/Assets/Scripts/UI/MainMenuView.cs:11) | SettingsMenuController | — | Ссылка на объект/компонент/ресурс |
| [_settingsPanel](/Users/admin/IncrementalRPG/Assets/Scripts/UI/MainMenuView.cs:12) | GameObject | — | Ссылка на объект/компонент/ресурс |
| [_authorsPanel](/Users/admin/IncrementalRPG/Assets/Scripts/UI/MainMenuView.cs:13) | GameObject | — | Ссылка на объект/компонент/ресурс |
| [_miscPanelsRoot](/Users/admin/IncrementalRPG/Assets/Scripts/UI/MainMenuView.cs:14) | GameObject | — | Ссылка на объект/компонент/ресурс |
| [_attentionPanel](/Users/admin/IncrementalRPG/Assets/Scripts/UI/MainMenuView.cs:15) | GameObject | — | Ссылка на объект/компонент/ресурс |
| [_newGameConfirmButton](/Users/admin/IncrementalRPG/Assets/Scripts/UI/MainMenuView.cs:16) | Button | — | Ссылка на объект/компонент/ресурс |
| [_newGameCancelButton](/Users/admin/IncrementalRPG/Assets/Scripts/UI/MainMenuView.cs:17) | Button | — | Ссылка на объект/компонент/ресурс |

<a id="mapmenufadetransition"></a>

### MapMenuFadeTransition

[Назначение: руководство по анимациям, интерфейсу и звуку](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/UI/MapMenuFadeTransition.cs).

Где редактировать:

- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:46121) → `MapMenuFade`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [_spriteRenderer](/Users/admin/IncrementalRPG/Assets/Scripts/UI/MapMenuFadeTransition.cs:13) | SpriteRenderer | — | Ссылка на объект/компонент/ресурс |
| [_fadeInDuration](/Users/admin/IncrementalRPG/Assets/Scripts/UI/MapMenuFadeTransition.cs:14) | float | `0.35f` | Значение; Min(0f) |
| [_burnDuration](/Users/admin/IncrementalRPG/Assets/Scripts/UI/MapMenuFadeTransition.cs:15) | float | `0.65f` | Значение; Min(0f) |
| [_fadeAmountStart](/Users/admin/IncrementalRPG/Assets/Scripts/UI/MapMenuFadeTransition.cs:16) | float | `-0.1f` | Значение |
| [_fadeAmountEnd](/Users/admin/IncrementalRPG/Assets/Scripts/UI/MapMenuFadeTransition.cs:17) | float | `1f` | Значение |
| [_useUnscaledTime](/Users/admin/IncrementalRPG/Assets/Scripts/UI/MapMenuFadeTransition.cs:18) | bool | `true` | Значение |

<a id="mousewheelsliderinput"></a>

### MouseWheelSliderInput

[Назначение: руководство по анимациям, интерфейсу и звуку](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/UI/MouseWheelSliderInput.cs).

Прямое размещение в сохранённых игровых сценах/префабах/конфигах не найдено; см. назначение и вложенность выше.

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [_slider](/Users/admin/IncrementalRPG/Assets/Scripts/UI/MouseWheelSliderInput.cs:9) | Slider | — | Ссылка на объект/компонент/ресурс |
| [_step](/Users/admin/IncrementalRPG/Assets/Scripts/UI/MouseWheelSliderInput.cs:10) | float | `0.05f` | Значение |

<a id="nodebackglowspriteconfig"></a>

### NodeBackGlowSpriteConfig

[Назначение: руководство по анимациям, интерфейсу и звуку](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/NodeBackGlowSpriteConfig.cs).

Где редактировать:

- [Back Glow Sprite Config.asset](</Users/admin/IncrementalRPG/Assets/SO/SkillTree/ViewConfigs/Back Glow Sprite Config.asset:3>) → `Back Glow Sprite Config`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [locked](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/NodeBackGlowSpriteConfig.cs:8) | Sprite | — | Ссылка на объект/компонент/ресурс |
| [unaffordable](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/NodeBackGlowSpriteConfig.cs:9) | Sprite | — | Ссылка на объект/компонент/ресурс |
| [affordable](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/NodeBackGlowSpriteConfig.cs:10) | Sprite | — | Ссылка на объект/компонент/ресурс |
| [complete](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/NodeBackGlowSpriteConfig.cs:11) | Sprite | — | Ссылка на объект/компонент/ресурс |

<a id="nodebordercolorconfig"></a>

### NodeBorderColorConfig

[Назначение: руководство по анимациям, интерфейсу и звуку](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/NodeBorderColorConfig.cs).

Где редактировать:

- [Color Config.asset](</Users/admin/IncrementalRPG/Assets/SO/SkillTree/ViewConfigs/Color Config.asset:3>) → `Color Config`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [locked](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/NodeBorderColorConfig.cs:8) | Color | — | Значение |
| [unaffordable](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/NodeBorderColorConfig.cs:9) | Color | — | Значение |
| [affordable](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/NodeBorderColorConfig.cs:10) | Color | — | Значение |
| [complete](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/NodeBorderColorConfig.cs:11) | Color | — | Значение |

<a id="nodecirclespriteconfig"></a>

### NodeCircleSpriteConfig

[Назначение: руководство по анимациям, интерфейсу и звуку](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/NodeCircleSpriteConfig.cs).

Где редактировать:

- [Circle Sprite Config.asset](</Users/admin/IncrementalRPG/Assets/SO/SkillTree/ViewConfigs/Circle Sprite Config.asset:3>) → `Circle Sprite Config`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [locked](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/NodeCircleSpriteConfig.cs:8) | Sprite | — | Ссылка на объект/компонент/ресурс |
| [unaffordable](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/NodeCircleSpriteConfig.cs:9) | Sprite | — | Ссылка на объект/компонент/ресурс |
| [affordable](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/NodeCircleSpriteConfig.cs:10) | Sprite | — | Ссылка на объект/компонент/ресурс |
| [complete](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/NodeCircleSpriteConfig.cs:11) | Sprite | — | Ссылка на объект/компонент/ресурс |

<a id="nodeconnectionview"></a>

### NodeConnectionView

[Назначение: руководство по анимациям, интерфейсу и звуку](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/View/NodeConnectionView.cs).

Где редактировать:

- [NodeConnection.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/SkillTree/NodeConnection.prefab:79) → `NodeConnection`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [_thickness](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/View/NodeConnectionView.cs:11) | float | `4f` | Значение |

<a id="nodedefinition"></a>

### NodeDefinition

[Назначение: 2. Дерево](/Users/admin/IncrementalRPG/Docs/GameplaySettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/NodeDefinition.cs).

`positionInGraph` — скрытая старая позиция. Активное дерево размещает ноды через `SkillTreeConfig.entries`.

Где редактировать:

- [CrystalShardDrop.asset](/Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/Shards/CrystalShardDrop.asset:3) → `CrystalShardDrop`
- [SkeletonShardDrop.asset](/Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/Shards/SkeletonShardDrop.asset:3) → `SkeletonShardDrop`
- [SlimeShardDrop.asset](/Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/Shards/SlimeShardDrop.asset:3) → `SlimeShardDrop`
- [ShardsUnlock.asset](/Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/Shards/ShardsUnlock.asset:3) → `ShardsUnlock`
- [ShardPickupSpeed.asset](/Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/Shards/ShardPickupSpeed.asset:3) → `ShardPickupSpeed`
- [DemonShardDrop.asset](/Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/Shards/DemonShardDrop.asset:3) → `DemonShardDrop`
- [AddDamageRadiusNode.asset](/Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/DamageZone/AddDamageRadiusNode.asset:3) → `AddDamageRadiusNode`
- [AddZoneDamageNode 3.asset](</Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/DamageZone/AddZoneDamageNode 3.asset:3>) → `AddZoneDamageNode 3`
- [AddZoneDamageNode 1.asset](</Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/DamageZone/AddZoneDamageNode 1.asset:3>) → `AddZoneDamageNode 1`
- [SpecialAttackNode.asset](/Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/DamageZone/SpecialAttackNode.asset:3) → `SpecialAttackNode`
- [AddZoneDamageNode 2.asset](</Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/DamageZone/AddZoneDamageNode 2.asset:3>) → `AddZoneDamageNode 2`
- [AutoAttackNode.asset](/Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/DamageZone/AutoAttackNode.asset:3) → `AutoAttackNode`
- [AddDamageRadiusNode 2.asset](</Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/DamageZone/AddDamageRadiusNode 2.asset:3>) → `AddDamageRadiusNode 2`
- [AddDamageRadiusNode 1.asset](</Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/DamageZone/AddDamageRadiusNode 1.asset:3>) → `AddDamageRadiusNode 1`
- [AddAttackSpeedNode 1.asset](</Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/DamageZone/AddAttackSpeedNode 1.asset:3>) → `AddAttackSpeedNode 1`
- [AutoAttackSpeedNode.asset](/Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/DamageZone/AutoAttackSpeedNode.asset:3) → `AutoAttackSpeedNode`
- [AddAttackSpeedNode.asset](/Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/DamageZone/AddAttackSpeedNode.asset:3) → `AddAttackSpeedNode`
- [AddZoneDamageNode.asset](/Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/DamageZone/AddZoneDamageNode.asset:3) → `AddZoneDamageNode`
- [GoldFarm 2.asset](</Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/Gold/GoldFarm 2.asset:3>) → `GoldFarm 2`
- [GoldFarm 3.asset](</Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/Gold/GoldFarm 3.asset:3>) → `GoldFarm 3`
- [GoldFarm.asset](/Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/Gold/GoldFarm.asset:3) → `GoldFarm`
- [MapSizeNode 1.asset](</Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/Map/MapSizeNode 1.asset:3>) → `MapSizeNode 1`
- [MapSizeNode.asset](/Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/Map/MapSizeNode.asset:3) → `MapSizeNode`
- [BombsExplosionDamage.asset](/Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/Bombs/BombsExplosionDamage.asset:3) → `BombsExplosionDamage`
- [BombSpawnSpeedNode.asset](/Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/Bombs/BombSpawnSpeedNode.asset:3) → `BombSpawnSpeedNode`
- [BombsExplosionRadius.asset](/Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/Bombs/BombsExplosionRadius.asset:3) → `BombsExplosionRadius`
- [BombsExplosionDamage 1.asset](</Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/Bombs/BombsExplosionDamage 1.asset:3>) → `BombsExplosionDamage 1`
- [BombsExplosionRadius 1.asset](</Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/Bombs/BombsExplosionRadius 1.asset:3>) → `BombsExplosionRadius 1`
- [BombSpawnSpeedNode 1.asset](</Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/Bombs/BombSpawnSpeedNode 1.asset:3>) → `BombSpawnSpeedNode 1`
- [BombFeatureNode.asset](/Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/Bombs/BombFeatureNode.asset:3) → `BombFeatureNode`
- [AddSpawnSpeedNode 2.asset](</Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/Spawn/AddSpawnSpeedNode 2.asset:3>) → `AddSpawnSpeedNode 2`
- [AddSpawnSpeedNode.asset](/Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/Spawn/AddSpawnSpeedNode.asset:3) → `AddSpawnSpeedNode`
- [AddSpawnSpeedNode 1.asset](</Users/admin/IncrementalRPG/Assets/SO/SkillTree/Nodes/Spawn/AddSpawnSpeedNode 1.asset:3>) → `AddSpawnSpeedNode 1`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [prerequisites[].node](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/NodeDefinition.cs:12) | NodeDefinition | — | Ссылка на объект/компонент/ресурс |
| [prerequisites[].requiredLevel](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/NodeDefinition.cs:13) | int | — | Значение |
| [id](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/NodeDefinition.cs:20) | string | — | Значение |
| [displayName](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/NodeDefinition.cs:22) | LocalizedString | `new()` | Таблица локализации + ключ |
| [description](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/NodeDefinition.cs:25) | LocalizedString | `new()` | Таблица локализации + ключ |
| [icon](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/NodeDefinition.cs:27) | Sprite | — | Ссылка на объект/компонент/ресурс |
| [additionalIcon](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/NodeDefinition.cs:30) | Sprite | — | Ссылка на объект/компонент/ресурс |
| [maxLevel](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/NodeDefinition.cs:33) | int | — | Значение; Min(1) |
| [goldCostPerLevel](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/NodeDefinition.cs:36) | BigDouble[] | — | Список; элементы раскрываются в Inspector |
| [prerequisites](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/NodeDefinition.cs:39) | List<NodePrerequisite> | — | Список; элементы раскрываются в Inspector |
| [effects](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/NodeDefinition.cs:41) | NodeEffect[] | — | Список; элементы раскрываются в Inspector |
| [positionInGraph](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/NodeDefinition.cs:44) | Vector2 | — | Скрытое/служебное поле |

<a id="nodeeffect"></a>

### NodeEffect

[Назначение: 2. Эффекты и все StatType](/Users/admin/IncrementalRPG/Docs/GameplaySettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/NodeEffect.cs).

Элемент `NodeDefinition.effects`. Таблица реально поддержанных типов эффектов для каждой характеристики находится в игровом руководстве.

Прямое размещение в сохранённых игровых сценах/префабах/конфигах не найдено; см. назначение и вложенность выше.

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [effectType](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/NodeEffect.cs:9) | NodeEffectType | — | Выбор из списка |
| [statType](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/NodeEffect.cs:12) | StatType | — | Выбор из списка |
| [valuesPerLevel](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/NodeEffect.cs:15) | float[] | — | Список; элементы раскрываются в Inspector |
| [feature](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/NodeEffect.cs:18) | GameFeature | — | Выбор из списка |

<a id="nodeframepricespriteconfig"></a>

### NodeFramePriceSpriteConfig

[Назначение: руководство по анимациям, интерфейсу и звуку](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/NodeFramePriceSpriteConfig.cs).

Где редактировать:

- [Frame Price Sprite Config.asset](</Users/admin/IncrementalRPG/Assets/SO/SkillTree/ViewConfigs/Frame Price Sprite Config.asset:3>) → `Frame Price Sprite Config`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [locked](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/NodeFramePriceSpriteConfig.cs:8) | Sprite | — | Ссылка на объект/компонент/ресурс |
| [unaffordable](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/NodeFramePriceSpriteConfig.cs:9) | Sprite | — | Ссылка на объект/компонент/ресурс |
| [affordable](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/NodeFramePriceSpriteConfig.cs:10) | Sprite | — | Ссылка на объект/компонент/ресурс |
| [complete](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/NodeFramePriceSpriteConfig.cs:11) | Sprite | — | Ссылка на объект/компонент/ресурс |

<a id="nodelevelcounterview"></a>

### NodeLevelCounterView

[Назначение: руководство по анимациям, интерфейсу и звуку](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/View/NodeLevelCounterView.cs).

Где редактировать:

- [NodeView.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/SkillTree/NodeView.prefab:577) → `NodeView/Counter`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [_container](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/View/NodeLevelCounterView.cs:11) | RectTransform | — | Ссылка на объект/компонент/ресурс |
| [_layoutGroup](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/View/NodeLevelCounterView.cs:12) | HorizontalLayoutGroup | — | Ссылка на объект/компонент/ресурс |
| [_lampPrefab](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/View/NodeLevelCounterView.cs:13) | SkeletonGraphic | — | Ссылка на объект/компонент/ресурс |
| [_idleOffAnimationName](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/View/NodeLevelCounterView.cs:14) | string | `"idle_off"` | Значение |
| [_turnOnAnimationName](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/View/NodeLevelCounterView.cs:15) | string | `"on"` | Значение |
| [_idleOnAnimationName](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/View/NodeLevelCounterView.cs:16) | string | `"idle_on"` | Значение |

<a id="nodepopupview"></a>

### NodePopupView

[Назначение: руководство по анимациям, интерфейсу и звуку](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/View/NodePopupView.cs).

Где редактировать:

- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:9837) → `MenuCanvas/SkillTreeView/NodePopup`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [_nameText](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/View/NodePopupView.cs:13) | TextMeshProUGUI | — | Ссылка на объект/компонент/ресурс |
| [_descriptionText](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/View/NodePopupView.cs:14) | TextMeshProUGUI | — | Ссылка на объект/компонент/ресурс |
| [_costText](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/View/NodePopupView.cs:15) | TextMeshProUGUI | — | Ссылка на объект/компонент/ресурс |
| [_framePriceImage](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/View/NodePopupView.cs:17) | Image | — | Ссылка на объект/компонент/ресурс |
| [_backGlowImage](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/View/NodePopupView.cs:18) | Image | — | Ссылка на объект/компонент/ресурс |
| [_framePriceSpriteConfig](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/View/NodePopupView.cs:20) | NodeFramePriceSpriteConfig | — | Ссылка на объект/компонент/ресурс |
| [_backGlowSpriteConfig](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/View/NodePopupView.cs:21) | NodeBackGlowSpriteConfig | — | Ссылка на объект/компонент/ресурс |
| [_positionRoot](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/View/NodePopupView.cs:22) | RectTransform | — | Ссылка на объект/компонент/ресурс |

<a id="nodeview"></a>

### NodeView

[Назначение: руководство по анимациям, интерфейсу и звуку](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/View/NodeView.cs).

Где редактировать:

- [NodeView.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/SkillTree/NodeView.prefab:43) → `NodeView`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [_icon](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/View/NodeView.cs:14) | Image | — | Ссылка на объект/компонент/ресурс |
| [_additionalIcon](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/View/NodeView.cs:15) | Image | — | Ссылка на объект/компонент/ресурс |
| [_stateCircleImage](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/View/NodeView.cs:16) | Image | — | Ссылка на объект/компонент/ресурс |
| [_stateCircleRotationDegreesPerSecond](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/View/NodeView.cs:17) | float | `18f` | Значение |
| [_levelCounter](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/View/NodeView.cs:18) | NodeLevelCounterView | — | Ссылка на объект/компонент/ресурс |
| [_lockedSkeleton](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/View/NodeView.cs:19) | SkeletonGraphic | — | Ссылка на объект/компонент/ресурс |
| [_lockedIdleAnimationName](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/View/NodeView.cs:20) | string | `"idle"` | Значение |
| [_lockedOpenAnimationName](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/View/NodeView.cs:21) | string | `"open"` | Значение |
| [_lockedCancelAnimationName](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/View/NodeView.cs:22) | string | `"cancel"` | Значение |

<a id="pausebuttonvisualstate"></a>

### PauseButtonVisualState

[Назначение: руководство по анимациям, интерфейсу и звуку](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/UI/PauseButtonVisualState.cs).

Где редактировать:

- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:14830) → `MenuCanvas/HubPanel/SideMenuToggle`
- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:31742) → `MenuCanvas/MapView/MapSideMenuToggle`
- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:38923) → `MenuCanvas/InventoryView/SideMenuToggle`
- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:41345) → `MenuCanvas/SkillTreeView/SkillTreePanel/SideMenuToggle`
- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:49803) → `MenuCanvas/CraftView/SideMenuToggle`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [_button](/Users/admin/IncrementalRPG/Assets/Scripts/UI/PauseButtonVisualState.cs:10) | Button | — | Ссылка на объект/компонент/ресурс |
| [_glassOff](/Users/admin/IncrementalRPG/Assets/Scripts/UI/PauseButtonVisualState.cs:11) | GameObject | — | Ссылка на объект/компонент/ресурс |
| [_glassOn](/Users/admin/IncrementalRPG/Assets/Scripts/UI/PauseButtonVisualState.cs:12) | GameObject | — | Ссылка на объект/компонент/ресурс |
| [_backLight](/Users/admin/IncrementalRPG/Assets/Scripts/UI/PauseButtonVisualState.cs:13) | GameObject | — | Ссылка на объект/компонент/ресурс |
| [_pressedScale](/Users/admin/IncrementalRPG/Assets/Scripts/UI/PauseButtonVisualState.cs:14) | float | `0.94f` | Значение; Min(0f) |

<a id="pausemenucontroller"></a>

### PauseMenuController

[Назначение: руководство по анимациям, интерфейсу и звуку](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/UI/PauseMenuController.cs).

Где редактировать:

- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:47289) → `MiscPanelsCanvas`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [_root](/Users/admin/IncrementalRPG/Assets/Scripts/UI/PauseMenuController.cs:17) | GameObject | — | Ссылка на объект/компонент/ресурс |
| [_pausePanel](/Users/admin/IncrementalRPG/Assets/Scripts/UI/PauseMenuController.cs:18) | GameObject | — | Ссылка на объект/компонент/ресурс |
| [_settingsMenu](/Users/admin/IncrementalRPG/Assets/Scripts/UI/PauseMenuController.cs:19) | SettingsMenuController | — | Ссылка на объект/компонент/ресурс |
| [_resumeButton](/Users/admin/IncrementalRPG/Assets/Scripts/UI/PauseMenuController.cs:20) | Button | — | Ссылка на объект/компонент/ресурс |
| [_settingsButton](/Users/admin/IncrementalRPG/Assets/Scripts/UI/PauseMenuController.cs:21) | Button | — | Ссылка на объект/компонент/ресурс |
| [_sideMenus](/Users/admin/IncrementalRPG/Assets/Scripts/UI/PauseMenuController.cs:22) | SideMenuFlyoutView[] | — | Список; элементы раскрываются в Inspector |
| [_exitToMainMenuButtons](/Users/admin/IncrementalRPG/Assets/Scripts/UI/PauseMenuController.cs:24) | Button[] | — | Список; элементы раскрываются в Inspector |
| [_mainMenuSceneName](/Users/admin/IncrementalRPG/Assets/Scripts/UI/PauseMenuController.cs:25) | string | `"MainMenuScene"` | Значение |

<a id="playerinventoryview"></a>

### PlayerInventoryView

[Назначение: 10. Инвентарь](/Users/admin/IncrementalRPG/Docs/GameplaySettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/UI/Inventory/PlayerInventoryView.cs).

Где редактировать:

- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:56590) → `MenuCanvas/InventoryView`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [_runtimeInventory](/Users/admin/IncrementalRPG/Assets/Scripts/UI/Inventory/PlayerInventoryView.cs:29) | UniversalInventory | — | Ссылка на объект/компонент/ресурс |
| [_slotContainer](/Users/admin/IncrementalRPG/Assets/Scripts/UI/Inventory/PlayerInventoryView.cs:30) | RectTransform | — | Ссылка на объект/компонент/ресурс |
| [_recycleDropPanel](/Users/admin/IncrementalRPG/Assets/Scripts/UI/Inventory/PlayerInventoryView.cs:33) | Graphic | — | Ссылка на объект/компонент/ресурс |
| [_recycleGraphic](/Users/admin/IncrementalRPG/Assets/Scripts/UI/Inventory/PlayerInventoryView.cs:34) | SkeletonGraphic | — | Ссылка на объект/компонент/ресурс |
| [_helmetSlot](/Users/admin/IncrementalRPG/Assets/Scripts/UI/Inventory/PlayerInventoryView.cs:35) | Image | — | Ссылка на объект/компонент/ресурс |
| [_chestSlot](/Users/admin/IncrementalRPG/Assets/Scripts/UI/Inventory/PlayerInventoryView.cs:36) | Image | — | Ссылка на объект/компонент/ресурс |
| [_weaponSlot](/Users/admin/IncrementalRPG/Assets/Scripts/UI/Inventory/PlayerInventoryView.cs:37) | Image | — | Ссылка на объект/компонент/ресурс |
| [_bootsSlot](/Users/admin/IncrementalRPG/Assets/Scripts/UI/Inventory/PlayerInventoryView.cs:38) | Image | — | Не задаёт рабочий слот; декоративный силуэт настраивается на BootsPlaceholder |
| [_menuToggleButton](/Users/admin/IncrementalRPG/Assets/Scripts/UI/Inventory/PlayerInventoryView.cs:41) | Button | — | Ссылка на объект/компонент/ресурс |
| [_sideMenuTemplate](/Users/admin/IncrementalRPG/Assets/Scripts/UI/Inventory/PlayerInventoryView.cs:42) | SideMenuFlyoutView | — | Ссылка на объект/компонент/ресурс |
| [_dragCanvasPrefab](/Users/admin/IncrementalRPG/Assets/Scripts/UI/Inventory/PlayerInventoryView.cs:45) | GameObject | — | Ссылка на объект/компонент/ресурс |
| [_tooltipCanvasPrefab](/Users/admin/IncrementalRPG/Assets/Scripts/UI/Inventory/PlayerInventoryView.cs:46) | GameObject | — | Ссылка на объект/компонент/ресурс |
| `_consumableFeedback` | TMP_Text | — | Текст локализованного результата применения |
| `_consumableFeedbackRoot` | GameObject | — | Панель сообщения; скрывается через 5 секунд и при выходе из инвентаря. Длительность задана в коде PlayerInventoryView |

<a id="potionhudrowview"></a>

### PotionHudRowView

[Назначение: список зелий](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md#potion-hud). Где редактировать: `GameScene → HUD/HUD_Panel/PotionHud/PotionRowTemplate`.

| Поле | Тип | Назначение |
|---|---|---|
| `_icon` | Image | Иконка определения зелья |
| `_name` | TMP_Text | Локализованное название |
| `_value` | TMP_Text | Локализованный формат силы с процентом |

Геометрия, шрифт и цвета задаются на дочерних компонентах шаблона. Описание при наведении берётся из `ItemDefinition.localizedDescription` с силой активного эффекта.

<a id="potionhudview"></a>

### PotionHudView

[Назначение: список зелий](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md#potion-hud). Где редактировать: `GameScene → HUD/HUD_Panel/PotionHud`.

| Поле | Тип | Назначение |
|---|---|---|
| `_listRoot` | RectTransform | `ActivePotions`: позиция, ширина и VerticalLayoutGroup |
| `_rowTemplate` | PotionHudRowView | Неактивный шаблон строки; высота 78 |
| `_tooltip` | TooltipView | Подсказка на активном объекте; `_root` ссылается на скрытую панель `PotionTooltip` |

<a id="rewardraysgraphic"></a>

### RewardRaysGraphic

[Назначение: руководство по анимациям, интерфейсу и звуку](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/UI/RewardRaysGraphic.cs).

Где редактировать:

- [LootRewardPopup.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/HUD/LootRewardPopup.prefab:1293) → `LootRewardPopup/Composition/RadialRays`
- [LootRewardPopup.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/HUD/LootRewardPopup.prefab:1519) → `LootRewardPopup/Composition/WarmRays`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [_rayCount](/Users/admin/IncrementalRPG/Assets/Scripts/UI/RewardRaysGraphic.cs:10) | int | `8` | Значение; Range(4, 16) |
| [_rayWidth](/Users/admin/IncrementalRPG/Assets/Scripts/UI/RewardRaysGraphic.cs:11) | float | `16f` | Значение; Range(4f, 24f) |
| [_angleOffset](/Users/admin/IncrementalRPG/Assets/Scripts/UI/RewardRaysGraphic.cs:12) | float | `9f` | Значение |

<a id="rewardrevealeffect"></a>

### RewardRevealEffect

[Назначение: руководство по анимациям, интерфейсу и звуку](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/UI/RewardRevealEffect.cs).

Где редактировать:

- [LootRewardPopup.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/HUD/LootRewardPopup.prefab:1585) → `LootRewardPopup`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [_icon](/Users/admin/IncrementalRPG/Assets/Scripts/UI/RewardRevealEffect.cs:9) | Image | — | Ссылка на объект/компонент/ресурс |
| [_rays](/Users/admin/IncrementalRPG/Assets/Scripts/UI/RewardRevealEffect.cs:10) | RewardRaysGraphic | — | Ссылка на объект/компонент/ресурс |
| [_secondaryRays](/Users/admin/IncrementalRPG/Assets/Scripts/UI/RewardRevealEffect.cs:11) | RewardRaysGraphic | — | Ссылка на объект/компонент/ресурс |
| [_halo](/Users/admin/IncrementalRPG/Assets/Scripts/UI/RewardRevealEffect.cs:12) | Image | — | Ссылка на объект/компонент/ресурс |
| [_particleRoot](/Users/admin/IncrementalRPG/Assets/Scripts/UI/RewardRevealEffect.cs:13) | RectTransform | — | Ссылка на объект/компонент/ресурс |
| [_mistSprite](/Users/admin/IncrementalRPG/Assets/Scripts/UI/RewardRevealEffect.cs:14) | Sprite | — | Ссылка на объект/компонент/ресурс |
| [_sparkSprite](/Users/admin/IncrementalRPG/Assets/Scripts/UI/RewardRevealEffect.cs:15) | Sprite | — | Ссылка на объект/компонент/ресурс |
| [_accent](/Users/admin/IncrementalRPG/Assets/Scripts/UI/RewardRevealEffect.cs:18) | Color | `new Color(217f / 255f, 82f / 255f, 181f / 255f)` | Значение |
| [_intensity](/Users/admin/IncrementalRPG/Assets/Scripts/UI/RewardRevealEffect.cs:19) | float | `1f` | Значение; Range(.5f, 1.5f) |
| [_rayRevolutionSeconds](/Users/admin/IncrementalRPG/Assets/Scripts/UI/RewardRevealEffect.cs:20) | float | `48f` | Значение; Range(20f, 90f) |
| [_appearanceDuration](/Users/admin/IncrementalRPG/Assets/Scripts/UI/RewardRevealEffect.cs:21) | float | `.6f` | Значение; Range(.3f, 1.2f) |
| [_floatAmplitude](/Users/admin/IncrementalRPG/Assets/Scripts/UI/RewardRevealEffect.cs:22) | float | `5.5f` | Значение; Range(0f, 12f) |

<a id="sessionendpopupview"></a>

### SessionEndPopupView

[Назначение: руководство по анимациям, интерфейсу и звуку](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/UI/SessionEndPopupView.cs).

Где редактировать:

- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:49063) → `HUD/SessionEndPanel`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [_goldText](/Users/admin/IncrementalRPG/Assets/Scripts/UI/SessionEndPopupView.cs:12) | TMP_Text | — | Ссылка на объект/компонент/ресурс |
| [_killsText](/Users/admin/IncrementalRPG/Assets/Scripts/UI/SessionEndPopupView.cs:13) | TMP_Text | — | Ссылка на объект/компонент/ресурс |
| [_goldRecordText](/Users/admin/IncrementalRPG/Assets/Scripts/UI/SessionEndPopupView.cs:14) | TMP_Text | — | Ссылка на объект/компонент/ресурс |
| [_killsRecordText](/Users/admin/IncrementalRPG/Assets/Scripts/UI/SessionEndPopupView.cs:15) | TMP_Text | — | Ссылка на объект/компонент/ресурс |
| [_raysTransform](/Users/admin/IncrementalRPG/Assets/Scripts/UI/SessionEndPopupView.cs:16) | Transform | — | Ссылка на объект/компонент/ресурс |
| [_hubButton](/Users/admin/IncrementalRPG/Assets/Scripts/UI/SessionEndPopupView.cs:17) | Button | — | Ссылка на объект/компонент/ресурс |
| [_recordScaleMultiplier](/Users/admin/IncrementalRPG/Assets/Scripts/UI/SessionEndPopupView.cs:18) | float | `1.15f` | Значение |
| [_recordScaleUpDuration](/Users/admin/IncrementalRPG/Assets/Scripts/UI/SessionEndPopupView.cs:19) | float | `0.14f` | Значение |
| [_recordScaleDownDuration](/Users/admin/IncrementalRPG/Assets/Scripts/UI/SessionEndPopupView.cs:20) | float | `0.18f` | Значение |
| [_recordScalePauseDuration](/Users/admin/IncrementalRPG/Assets/Scripts/UI/SessionEndPopupView.cs:21) | float | `0.45f` | Значение |
| [_raysRotationDegreesPerSecond](/Users/admin/IncrementalRPG/Assets/Scripts/UI/SessionEndPopupView.cs:22) | float | `20f` | Значение |

<a id="settingsmenucontroller"></a>

### SettingsMenuController

[Назначение: руководство по анимациям, интерфейсу и звуку](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/UI/SettingsMenuController.cs).

Где редактировать:

- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:39197) → `MiscPanelsCanvas/Background/SettingsPanel`
- [MainMenuScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/MainMenuScene.unity:12471) → `MiscPanelsCanvas/Background/SettingsPanel`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [_view](/Users/admin/IncrementalRPG/Assets/Scripts/UI/SettingsMenuController.cs:32) | SettingsMenuView | — | Ссылка на объект/компонент/ресурс |
| [_hideOnAwake](/Users/admin/IncrementalRPG/Assets/Scripts/UI/SettingsMenuController.cs:33) | bool | — | Значение |
| [_localeLabelMode](/Users/admin/IncrementalRPG/Assets/Scripts/UI/SettingsMenuController.cs:34) | LocaleLabelMode | `LocaleLabelMode.LocaleName` | Выбор из списка |
| [_localeLabelOverrides](/Users/admin/IncrementalRPG/Assets/Scripts/UI/SettingsMenuController.cs:35) | LocaleLabelOverride[] | — | Список; элементы раскрываются в Inspector |

<a id="settingsmenuview"></a>

### SettingsMenuView

[Назначение: руководство по анимациям, интерфейсу и звуку](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/UI/SettingsMenuView.cs).

Где редактировать:

- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:39179) → `MiscPanelsCanvas/Background/SettingsPanel`
- [MainMenuScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/MainMenuScene.unity:12487) → `MiscPanelsCanvas/Background/SettingsPanel`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [_root](/Users/admin/IncrementalRPG/Assets/Scripts/UI/SettingsMenuView.cs:9) | GameObject | — | Ссылка на объект/компонент/ресурс |
| [_masterVolumeSlider](/Users/admin/IncrementalRPG/Assets/Scripts/UI/SettingsMenuView.cs:10) | Slider | — | Ссылка на объект/компонент/ресурс |
| [_musicVolumeSlider](/Users/admin/IncrementalRPG/Assets/Scripts/UI/SettingsMenuView.cs:11) | Slider | — | Ссылка на объект/компонент/ресурс |
| [_sfxVolumeSlider](/Users/admin/IncrementalRPG/Assets/Scripts/UI/SettingsMenuView.cs:12) | Slider | — | Ссылка на объект/компонент/ресурс |
| [_languageDropdown](/Users/admin/IncrementalRPG/Assets/Scripts/UI/SettingsMenuView.cs:13) | TMP_Dropdown | — | Ссылка на объект/компонент/ресурс |
| [_backButton](/Users/admin/IncrementalRPG/Assets/Scripts/UI/SettingsMenuView.cs:14) | Button | — | Ссылка на объект/компонент/ресурс |

<a id="shardpickupconfig"></a>

### ShardPickupConfig

[Назначение: 9. Осколки](/Users/admin/IncrementalRPG/Docs/GameplaySettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/Shard/ShardPickupConfig.cs).

Где редактировать:

- [ShardPickupConfig.asset](/Users/admin/IncrementalRPG/Assets/SO/Shard/ShardPickupConfig.asset:3) → `ShardPickupConfig`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [pickupPrefab](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/Shard/ShardPickupConfig.cs:10) | ShardPickupView | — | Ссылка на объект/компонент/ресурс |
| [icon](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/Shard/ShardPickupConfig.cs:11) | Sprite | — | Ссылка на объект/компонент/ресурс |
| [basePickupValue](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/Shard/ShardPickupConfig.cs:14) | BigDouble | `10` | Значение |
| [maxPickupCount](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/Shard/ShardPickupConfig.cs:15) | int | `32` | Значение; Min(1) |
| [lifetime](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/Shard/ShardPickupConfig.cs:16) | float | `15f` | Значение; Min(0.01f) |
| [hitRadius](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/Shard/ShardPickupConfig.cs:17) | float | `0.12f` | Значение; Min(0f) |
| [scatterDuration](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/Shard/ShardPickupConfig.cs:20) | float | `0.4f` | Значение; Min(0f) |
| [minScatterDistance](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/Shard/ShardPickupConfig.cs:21) | float | `0.35f` | Значение; Min(0f) |
| [maxScatterDistance](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/Shard/ShardPickupConfig.cs:22) | float | `0.8f` | Значение; Min(0f) |
| [baseCollectionDuration](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/Shard/ShardPickupConfig.cs:25) | float | `1f` | Значение; Min(0.01f) |

<a id="shardpickupview"></a>

### ShardPickupView

[Назначение: руководство по анимациям, интерфейсу и звуку](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/Shard/ShardPickupView.cs).

Где редактировать:

- [ShardPickup.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/Shard/ShardPickup.prefab:63) → `ShardPickup`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [_icon](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/Shard/ShardPickupView.cs:7) | SpriteRenderer | — | Ссылка на объект/компонент/ресурс |

<a id="shinesweepanimator"></a>

### ShineSweepAnimator

[Назначение: руководство по анимациям, интерфейсу и звуку](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/Entity/ShineSweepAnimator.cs).

Где редактировать:

- [Crystal_1.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/Entities/Crystal_1.prefab:440) → `Spine GameObject (mineral_v1_part2)`
- [Crystal_2.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/Entities/Crystal_2.prefab:574) → `Spine GameObject (mineral_v2_part2)`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [_renderer](/Users/admin/IncrementalRPG/Assets/Scripts/Entity/ShineSweepAnimator.cs:13) | Renderer | — | Ссылка на объект/компонент/ресурс |
| [_sweepDuration](/Users/admin/IncrementalRPG/Assets/Scripts/Entity/ShineSweepAnimator.cs:14) | float | `0.6f` | Значение |
| [_pauseDuration](/Users/admin/IncrementalRPG/Assets/Scripts/Entity/ShineSweepAnimator.cs:15) | float | `3.5f` | Значение |
| [_initialDelay](/Users/admin/IncrementalRPG/Assets/Scripts/Entity/ShineSweepAnimator.cs:16) | float | — | Значение |

<a id="sidemenubuttonvisualstate"></a>

### SideMenuButtonVisualState

[Назначение: руководство по анимациям, интерфейсу и звуку](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/UI/SideMenuButtonVisualState.cs).

Где редактировать:

- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:2899) → `MenuCanvas/MapView/MapSideMenuFlyout/ButtonList/ReturnToHubButton`
- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:4380) → `MenuCanvas/SkillTreeView/SkillTreePanel/SkillTreeSideMenuFlyout/ButtonList/SettingsButton`
- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:6096) → `MiscPanelsCanvas/Background/PausePanel/SettingsPanelImage/Button`
- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:9560) → `MenuCanvas/MapView/MapSideMenuFlyout/ButtonList/MainMenuButton`
- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:9734) → `MenuCanvas/MapView/MapSideMenuFlyout/ButtonList/SettingsButton`
- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:10668) → `MenuCanvas/HubPanel/HubSideMenuFlyout/ButtonList/MainMenuButton`
- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:19856) → `MenuCanvas/HubPanel/HubSideMenuFlyout/ButtonList/ExitButton`
- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:20383) → `MiscPanelsCanvas/Background/SettingsPanel/SettingsPanelImage/Button`
- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:32728) → `MiscPanelsCanvas/Background/PausePanel/SettingsPanelImage/Button (2)`
- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:33801) → `MenuCanvas/SkillTreeView/SkillTreePanel/SkillTreeSideMenuFlyout/ButtonList/ReturnToHubButton`
- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:34928) → `MenuCanvas/HubPanel/HubSideMenuFlyout/ButtonList/SettingsButton`
- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:37803) → `MenuCanvas/SkillTreeView/SkillTreePanel/SkillTreeSideMenuFlyout/ButtonList/MainMenuButton`
- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:42302) → `MenuCanvas/SkillTreeView/SkillTreePanel/SkillTreeSideMenuFlyout/ButtonList/ExitButton`
- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:45861) → `MiscPanelsCanvas/Background/PausePanel/SettingsPanelImage/Button (1)`
- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:48157) → `MenuCanvas/MapView/MapSideMenuFlyout/ButtonList/ExitButton`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [_button](/Users/admin/IncrementalRPG/Assets/Scripts/UI/SideMenuButtonVisualState.cs:14) | Button | — | Ссылка на объект/компонент/ресурс |
| [_glow](/Users/admin/IncrementalRPG/Assets/Scripts/UI/SideMenuButtonVisualState.cs:15) | GameObject | — | Ссылка на объект/компонент/ресурс |

<a id="sidemenuflyoutview"></a>

### SideMenuFlyoutView

[Назначение: руководство по анимациям, интерфейсу и звуку](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/UI/SideMenuFlyoutView.cs).

Где редактировать:

- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:29155) → `MenuCanvas/HubPanel/HubSideMenuFlyout`
- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:38046) → `MenuCanvas/MapView/MapSideMenuFlyout`
- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:42081) → `MenuCanvas/SkillTreeView/SkillTreePanel/SkillTreeSideMenuFlyout`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [_toggleButton](/Users/admin/IncrementalRPG/Assets/Scripts/UI/SideMenuFlyoutView.cs:11) | Button | — | Ссылка на объект/компонент/ресурс |
| [_listRoot](/Users/admin/IncrementalRPG/Assets/Scripts/UI/SideMenuFlyoutView.cs:12) | GameObject | — | Ссылка на объект/компонент/ресурс |
| [_settingsButton](/Users/admin/IncrementalRPG/Assets/Scripts/UI/SideMenuFlyoutView.cs:13) | Button | — | Ссылка на объект/компонент/ресурс |
| [_mainMenuButton](/Users/admin/IncrementalRPG/Assets/Scripts/UI/SideMenuFlyoutView.cs:14) | Button | — | Ссылка на объект/компонент/ресурс |
| [_exitButton](/Users/admin/IncrementalRPG/Assets/Scripts/UI/SideMenuFlyoutView.cs:15) | Button | — | Ссылка на объект/компонент/ресурс |
| [_returnToHubButton](/Users/admin/IncrementalRPG/Assets/Scripts/UI/SideMenuFlyoutView.cs:16) | Button | — | Ссылка на объект/компонент/ресурс |
| [_animationDuration](/Users/admin/IncrementalRPG/Assets/Scripts/UI/SideMenuFlyoutView.cs:17) | float | `0.16f` | Значение; Min(0f) |
| [_itemDelay](/Users/admin/IncrementalRPG/Assets/Scripts/UI/SideMenuFlyoutView.cs:18) | float | `0.04f` | Значение; Min(0f) |
| [_closedOffset](/Users/admin/IncrementalRPG/Assets/Scripts/UI/SideMenuFlyoutView.cs:19) | Vector2 | `new Vector2(0f, 18f)` | Значение |

<a id="skeletontint"></a>

### SkeletonTint

[Назначение: руководство по анимациям, интерфейсу и звуку](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/Entity/SkeletonTint.cs).

Где редактировать:

- [Crystal_1.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/Entities/Crystal_1.prefab:408) → `Spine GameObject (mineral_v1_part2)`
- [Crystal_2.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/Entities/Crystal_2.prefab:542) → `Spine GameObject (mineral_v2_part2)`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [_renderer](/Users/admin/IncrementalRPG/Assets/Scripts/Entity/SkeletonTint.cs:9) | Renderer | — | Ссылка на объект/компонент/ресурс |
| [_color](/Users/admin/IncrementalRPG/Assets/Scripts/Entity/SkeletonTint.cs:10) | Color | `Color.white` | Значение |

<a id="skilltreeconfig"></a>

### SkillTreeConfig

[Назначение: 2. Размещение дерева](/Users/admin/IncrementalRPG/Docs/GameplaySettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/SkillTreeConfig.cs).

`nodes` — скрытый старый список; при заполненных `entries` используется новая сетка.

Где редактировать:

- [SkillTreeConfig.asset](/Users/admin/IncrementalRPG/Assets/SO/SkillTree/SkillTreeConfig.asset:3) → `SkillTreeConfig`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [nodes](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/SkillTreeConfig.cs:25) | List<NodeDefinition> | `new()` | Скрытое/служебное поле |
| [entries[].node](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/SkillTreeConfig.cs:9) | NodeDefinition | — | Ссылка на объект/компонент/ресурс |
| [entries[].gridPosition](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/SkillTreeConfig.cs:10) | Vector2Int | — | Значение |
| [cellSpacing](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/SkillTreeConfig.cs:17) | Vector2 | `new Vector2(100f, 100f)` | Значение |
| [origin](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/SkillTreeConfig.cs:20) | Vector2 | — | Значение |
| [entries](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/SkillTreeConfig.cs:22) | List<SkillTreeNodeEntry> | `new()` | Список; элементы раскрываются в Inspector |

<a id="skilltreepanzoomcontroller"></a>

### SkillTreePanZoomController

[Назначение: руководство по анимациям, интерфейсу и звуку](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/View/SkillTreePanZoomController.cs).

Где редактировать:

- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:21898) → `MenuCanvas/SkillTreeView/SkillTreePanel`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [_content](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/View/SkillTreePanZoomController.cs:9) | RectTransform | — | Ссылка на объект/компонент/ресурс |
| [_popupView](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/View/SkillTreePanZoomController.cs:10) | NodePopupView | — | Ссылка на объект/компонент/ресурс |
| [_minZoom](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/View/SkillTreePanZoomController.cs:11) | float | `0.3f` | Значение |
| [_maxZoom](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/View/SkillTreePanZoomController.cs:12) | float | `2f` | Значение |
| [_zoomFactor](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/View/SkillTreePanZoomController.cs:13) | float | `1.12f` | Значение |
| [_rubberBandDamping](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/View/SkillTreePanZoomController.cs:14) | float | `0.3f` | Значение |
| [_snapDuration](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/View/SkillTreePanZoomController.cs:15) | float | `0.35f` | Значение |

<a id="skilltreeview"></a>

### SkillTreeView

[Назначение: руководство по анимациям, интерфейсу и звуку](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/View/SkillTreeView.cs).

Где редактировать:

- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:27411) → `MenuCanvas/SkillTreeView`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [_nodesLayer](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/View/SkillTreeView.cs:19) | RectTransform | — | Ссылка на объект/компонент/ресурс |
| [_connectionsLayer](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/View/SkillTreeView.cs:20) | RectTransform | — | Ссылка на объект/компонент/ресурс |
| [_nodeViewPrefab](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/View/SkillTreeView.cs:21) | NodeView | — | Ссылка на объект/компонент/ресурс |
| [_connectionViewPrefab](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/View/SkillTreeView.cs:22) | NodeConnectionView | — | Ссылка на объект/компонент/ресурс |
| [_popupView](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/View/SkillTreeView.cs:23) | NodePopupView | — | Ссылка на объект/компонент/ресурс |
| [_goldText](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/View/SkillTreeView.cs:24) | TextMeshProUGUI | — | Ссылка на объект/компонент/ресурс |
| [_shardText](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/View/SkillTreeView.cs:25) | TextMeshProUGUI | — | Ссылка на объект/компонент/ресурс |
| [_shardCounterRoot](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/View/SkillTreeView.cs:26) | GameObject | — | Ссылка на объект/компонент/ресурс |
| [_closeButton](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/View/SkillTreeView.cs:27) | Button | — | Ссылка на объект/компонент/ресурс |
| [_contentPadding](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/View/SkillTreeView.cs:28) | Vector2 | `new Vector2(600f, 600f)` | Значение |
| [_initialFocusNode](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/View/SkillTreeView.cs:29) | NodeDefinition | — | Ссылка на объект/компонент/ресурс |
| [_connectionRevealDuration](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/View/SkillTreeView.cs:30) | float | `0.25f` | Значение |
| [_nodeRevealDuration](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/View/SkillTreeView.cs:31) | float | `0.18f` | Значение |
| [_nodeRevealStartScale](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/View/SkillTreeView.cs:32) | float | `0.15f` | Значение |
| [_revealWaveDelay](/Users/admin/IncrementalRPG/Assets/Scripts/Core/TestSkillTree/View/SkillTreeView.cs:33) | float | `0.04f` | Значение |

<a id="spawntable"></a>

### SpawnTable

[Назначение: 5. Спавн](/Users/admin/IncrementalRPG/Docs/GameplaySettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/Entity/SpawnTable.cs).

Где редактировать:

- [0_1SpawnRule.asset](/Users/admin/IncrementalRPG/Assets/SO/Dungeons/0_RocksAndLava/0_1SpawnRule.asset:3) → `0_1SpawnRule`
- [0_2SpawnRule.asset](/Users/admin/IncrementalRPG/Assets/SO/Dungeons/0_RocksAndLava/0_2SpawnRule.asset:3) → `0_2SpawnRule`
- [0_5SpawnRule.asset](/Users/admin/IncrementalRPG/Assets/SO/Dungeons/0_RocksAndLava/0_5SpawnRule.asset:3) → `0_5SpawnRule`
- [0_SpawnRule.asset](/Users/admin/IncrementalRPG/Assets/SO/Dungeons/0_RocksAndLava/0_SpawnRule.asset:3) → `0_SpawnRule`
- [0_4SpawnRule.asset](/Users/admin/IncrementalRPG/Assets/SO/Dungeons/0_RocksAndLava/0_4SpawnRule.asset:3) → `0_4SpawnRule`
- [0_3SpawnRule.asset](/Users/admin/IncrementalRPG/Assets/SO/Dungeons/0_RocksAndLava/0_3SpawnRule.asset:3) → `0_3SpawnRule`
- [1_SpawnRule.asset](/Users/admin/IncrementalRPG/Assets/SO/Dungeons/1_AnyBiome/1_SpawnRule.asset:3) → `1_SpawnRule`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [entries[].config](/Users/admin/IncrementalRPG/Assets/Scripts/Entity/SpawnTable.cs:11) | EntityConfig | — | Ссылка на объект/компонент/ресурс |
| [entries[].weight](/Users/admin/IncrementalRPG/Assets/Scripts/Entity/SpawnTable.cs:12) | float | `1f` | Значение; Min(0f) |
| [entries[].requiredFeature](/Users/admin/IncrementalRPG/Assets/Scripts/Entity/SpawnTable.cs:13) | GameFeature | `GameFeature.None` | Выбор из списка |
| [entries](/Users/admin/IncrementalRPG/Assets/Scripts/Entity/SpawnTable.cs:19) | SpawnEntry[] | — | Список; элементы раскрываются в Inspector |

<a id="tilegriddebugger"></a>

### TileGridDebugger

[Назначение: руководство по анимациям, интерфейсу и звуку](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/Utils/TileGridDebugger.cs).

Отладочный инструмент; на игровой баланс не влияет.

Прямое размещение в сохранённых игровых сценах/префабах/конфигах не найдено; см. назначение и вложенность выше.

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [_tilemap](/Users/admin/IncrementalRPG/Assets/Scripts/Utils/TileGridDebugger.cs:8) | Tilemap | — | Ссылка на объект/компонент/ресурс |
| [_previewSize](/Users/admin/IncrementalRPG/Assets/Scripts/Utils/TileGridDebugger.cs:9) | int | `4` | Значение |
| [_sphereRadius](/Users/admin/IncrementalRPG/Assets/Scripts/Utils/TileGridDebugger.cs:10) | float | `0.05f` | Значение |

<a id="tilemapcameraautofitter"></a>

### TilemapCameraAutoFitter

[Назначение: 8. Камера и лава](/Users/admin/IncrementalRPG/Docs/GameplaySettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/TilemapGenerator/TilemapCameraAutoFitter.cs).

Где редактировать:

- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:441) → `TilemapCameraAutoFitter`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [targetTilemap](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/TilemapGenerator/TilemapCameraAutoFitter.cs:10) | Tilemap | — | Ссылка на объект/компонент/ресурс |
| [targetCamera](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/TilemapGenerator/TilemapCameraAutoFitter.cs:11) | Camera | — | Ссылка на объект/компонент/ресурс |
| [lavaOffScreenMargin](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/TilemapGenerator/TilemapCameraAutoFitter.cs:15) | float | `1f` | Значение |
| [lavaRoot](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/TilemapGenerator/TilemapCameraAutoFitter.cs:18) | Transform | — | Ссылка на объект/компонент/ресурс |
| [lavaReferenceOrthoSize](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/TilemapGenerator/TilemapCameraAutoFitter.cs:20) | float | `5f` | Значение |
| [lavaVerticalOffsetFixed](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/TilemapGenerator/TilemapCameraAutoFitter.cs:22) | float | `0f` | Значение |
| [lavaVerticalOffsetScaled](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/TilemapGenerator/TilemapCameraAutoFitter.cs:24) | float | `0f` | Значение |
| [viewportFill](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/TilemapGenerator/TilemapCameraAutoFitter.cs:29) | float | `0.85f` | Значение; Range(0.1f, 1f) |
| [minOrthographicSize](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/TilemapGenerator/TilemapCameraAutoFitter.cs:31) | float | `2f` | Значение; Min(0.01f) |
| [maxOrthographicSize](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/TilemapGenerator/TilemapCameraAutoFitter.cs:33) | float | `100f` | Значение; Min(0.01f) |
| [keepCurrentCameraZ](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/TilemapGenerator/TilemapCameraAutoFitter.cs:34) | bool | `true` | Значение |
| [normalizedVerticalOffset](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/TilemapGenerator/TilemapCameraAutoFitter.cs:37) | float | `0f` | Значение; Range(-0.5f, 0.5f) |

<a id="tilemapgenerationconfig"></a>

### TilemapGenerationConfig

[Назначение: 8. Генерация](/Users/admin/IncrementalRPG/Docs/GameplaySettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/TilemapGenerator/TilemapGenerationConfig.cs).

Где редактировать:

- [0_TilemapGenerationConfig.asset](/Users/admin/IncrementalRPG/Assets/SO/Dungeons/0_RocksAndLava/DungeonRules/0_TilemapGenerationConfig.asset:3) → `0_TilemapGenerationConfig`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [tileSet](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/TilemapGenerator/TilemapGenerationConfig.cs:19) | TilemapTileSet | — | Ссылка на объект/компонент/ресурс |
| [gradientDirection](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/TilemapGenerator/TilemapGenerationConfig.cs:22) | TilemapGradientDirection | `TilemapGradientDirection.BottomToTop` | Выбор из списка |
| [gradientRemap](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/TilemapGenerator/TilemapGenerationConfig.cs:23) | AnimationCurve | `AnimationCurve.Linear(0f, 0f, 1f, 1f)` | Значение |
| [seed](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/TilemapGenerator/TilemapGenerationConfig.cs:26) | int | `12345` | Значение |
| [noiseScale](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/TilemapGenerator/TilemapGenerationConfig.cs:28) | float | `0.15f` | Значение; Min(0.001f) |
| [noiseBorderStrength](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/TilemapGenerator/TilemapGenerationConfig.cs:30) | float | `0.25f` | Значение; Range(0f, 1f) |
| [pillarHeight](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/TilemapGenerator/TilemapGenerationConfig.cs:33) | int | `3` | Значение; Min(0) |

<a id="tilemaptileset"></a>

### TilemapTileSet

[Назначение: 8. Тайлы](/Users/admin/IncrementalRPG/Docs/GameplaySettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/TilemapGenerator/TilemapTileSet.cs).

Где редактировать:

- [0_TilemapTileSet.asset](/Users/admin/IncrementalRPG/Assets/SO/Dungeons/0_RocksAndLava/DungeonRules/0_TilemapTileSet.asset:3) → `0_TilemapTileSet`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [tileRules[].tile / tileRules[].variants[].tile](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/TilemapGenerator/TilemapTileSet.cs:14) | TileBase | — | Ссылка на объект/компонент/ресурс |
| [tileRules[].variants[].probability](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/TilemapGenerator/TilemapTileSet.cs:15) | float | `0.1f` | Значение; Range(0f, 1f) |
| [tileRules[].variants](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/TilemapGenerator/TilemapTileSet.cs:22) | List<TileVariant> | `new()` | Список; элементы раскрываются в Inspector |
| [tileRules[].gradientMin](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/TilemapGenerator/TilemapTileSet.cs:23) | float | `0f` | Значение; Range(0f, 1f) |
| [tileRules[].gradientMax](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/TilemapGenerator/TilemapTileSet.cs:24) | float | `1f` | Значение; Range(0f, 1f) |
| [tileRules](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/TilemapGenerator/TilemapTileSet.cs:28) | List<TileRule> | `new()` | Список; элементы раскрываются в Inspector |
| [leftWallTile](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/TilemapGenerator/TilemapTileSet.cs:31) | TileBase | — | Ссылка на объект/компонент/ресурс |
| [rightWallTile](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Gameplay/TilemapGenerator/TilemapTileSet.cs:32) | TileBase | — | Ссылка на объект/компонент/ресурс |

<a id="tmpglowstyle"></a>

### TmpGlowStyle

[Назначение: руководство по анимациям, интерфейсу и звуку](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/UI/TmpGlowStyle.cs).

Где редактировать:

- [MineTextGlow.asset](/Users/admin/IncrementalRPG/Assets/SO/TextGlow/MineTextGlow.asset:3) → `MineTextGlow`
- [BarracksTextGlow.asset](/Users/admin/IncrementalRPG/Assets/SO/TextGlow/BarracksTextGlow.asset:3) → `BarracksTextGlow`
- [CraftTextGlow.asset](/Users/admin/IncrementalRPG/Assets/SO/TextGlow/CraftTextGlow.asset:3) → `CraftTextGlow`
- [DungeonTextGlow.asset](/Users/admin/IncrementalRPG/Assets/SO/TextGlow/DungeonTextGlow.asset:3) → `DungeonTextGlow`
- [DrillTextGlow.asset](/Users/admin/IncrementalRPG/Assets/SO/TextGlow/DrillTextGlow.asset:3) → `DrillTextGlow`
- [TrainingTextGlow.asset](/Users/admin/IncrementalRPG/Assets/SO/TextGlow/TrainingTextGlow.asset:3) → `TrainingTextGlow`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [_color](/Users/admin/IncrementalRPG/Assets/Scripts/UI/TmpGlowStyle.cs:9) | Color | `new(1f, 0.82f, 0.25f, 0.75f)` | Значение |
| [_offset](/Users/admin/IncrementalRPG/Assets/Scripts/UI/TmpGlowStyle.cs:10) | float | — | Значение; Range(-1f, 1f) |
| [_inner](/Users/admin/IncrementalRPG/Assets/Scripts/UI/TmpGlowStyle.cs:11) | float | `0.05f` | Значение; Range(0f, 1f) |
| [_outer](/Users/admin/IncrementalRPG/Assets/Scripts/UI/TmpGlowStyle.cs:12) | float | `0.4f` | Значение; Range(0f, 1f) |
| [_power](/Users/admin/IncrementalRPG/Assets/Scripts/UI/TmpGlowStyle.cs:13) | float | `0.75f` | Значение; Range(0f, 1f) |

<a id="tmpglowtext"></a>

### TmpGlowText

[Назначение: руководство по анимациям, интерфейсу и звуку](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/UI/TmpGlowText.cs).

Прямые сохранённые экземпляры не найдены. Стили не применяются только от факта наличия assets.

Прямое размещение в сохранённых игровых сценах/префабах/конфигах не найдено; см. назначение и вложенность выше.

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [_text](/Users/admin/IncrementalRPG/Assets/Scripts/UI/TmpGlowText.cs:10) | TMP_Text | — | Ссылка на объект/компонент/ресурс |
| [_glowStyle](/Users/admin/IncrementalRPG/Assets/Scripts/UI/TmpGlowText.cs:11) | TmpGlowStyle | — | Ссылка на объект/компонент/ресурс |
| [_baseMaterialOverride](/Users/admin/IncrementalRPG/Assets/Scripts/UI/TmpGlowText.cs:12) | Material | — | Ссылка на объект/компонент/ресурс |
| [_reapplyWhenFontChanges](/Users/admin/IncrementalRPG/Assets/Scripts/UI/TmpGlowText.cs:13) | bool | `true` | Значение |
| [_warnIfShaderDoesNotSupportGlow](/Users/admin/IncrementalRPG/Assets/Scripts/UI/TmpGlowText.cs:14) | bool | `true` | Значение |

<a id="tooltipview"></a>

### TooltipView

[Назначение: руководство по анимациям, интерфейсу и звуку](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/UI/TooltipView.cs).

Где редактировать:

- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:6262) → `MenuCanvas/MapView/RightGradient/DungeonInfoPanel/PanelAspect/PanelFrame/Tooltip`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [_root](/Users/admin/IncrementalRPG/Assets/Scripts/UI/TooltipView.cs:9) | RectTransform | — | Ссылка на объект/компонент/ресурс |
| [_text](/Users/admin/IncrementalRPG/Assets/Scripts/UI/TooltipView.cs:10) | TMP_Text | — | Ссылка на объект/компонент/ресурс |
| [_offset](/Users/admin/IncrementalRPG/Assets/Scripts/UI/TooltipView.cs:11) | Vector2 | `new(16f, -16f)` | Значение |

<a id="uibuttonaudio"></a>

### UIButtonAudio

[Назначение: руководство по анимациям, интерфейсу и звуку](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/UI/UIButtonAudio.cs).

Где редактировать:

- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:3292) → `MenuCanvas/HubPanel/Plateau/MineButton`
- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:9185) → `MenuCanvas/HubPanel/Plateau/CraftButton`
- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:27263) → `MenuCanvas/HubPanel/Plateau/SkillTreeButton`
- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:28721) → `MenuCanvas/HubPanel/Plateau/DungeonButton`
- [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:46666) → `MenuCanvas/HubPanel/Plateau/BarracksButton`

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [_button](/Users/admin/IncrementalRPG/Assets/Scripts/UI/UIButtonAudio.cs:11) | Button | — | Ссылка на объект/компонент/ресурс |
| [_playHoverSound](/Users/admin/IncrementalRPG/Assets/Scripts/UI/UIButtonAudio.cs:12) | bool | `true` | Значение |
| [_playClickSound](/Users/admin/IncrementalRPG/Assets/Scripts/UI/UIButtonAudio.cs:13) | bool | `true` | Значение |

<a id="uibuttonpressscaler"></a>

### UIButtonPressScaler

[Назначение: руководство по анимациям, интерфейсу и звуку](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md). [Определение полей в коде](/Users/admin/IncrementalRPG/Assets/Scripts/UI/UIButtonPressScaler.cs).

Прямое размещение в сохранённых игровых сценах/префабах/конфигах не найдено; см. назначение и вложенность выше.

| Поле | Тип | Инициализатор нового объекта | Вид настройки |
| --- | --- | --- | --- |
| [_button](/Users/admin/IncrementalRPG/Assets/Scripts/UI/UIButtonPressScaler.cs:10) | Button | — | Ссылка на объект/компонент/ресурс |
| [_target](/Users/admin/IncrementalRPG/Assets/Scripts/UI/UIButtonPressScaler.cs:11) | RectTransform | — | Ссылка на объект/компонент/ресурс |
| [_pressedScale](/Users/admin/IncrementalRPG/Assets/Scripts/UI/UIButtonPressScaler.cs:12) | float | `0.96f` | Значение; Min(0f) |

## Используемые Spine-компоненты и исходные экспорты

Это прямые ссылки из игровых сцен/префабов. Для варианта существа путь внутри префаба может начинаться с добавленной части, а корень наследуется от Creature. Скорость ниже — сохранённый `timeScale`; сундук и лампы перехода во время игры перезаписывают её. Справа перечислены клипы из JSON, связанного через SkeletonDataAsset.

### SkeletonAnimation

| Где | Time Scale | Skeleton Data | Экспорт | Клипы |
| --- | --- | --- | --- | --- |
| [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity) → LavaAnimation/lava_form_front_0/Spine GameObject (lava_pieces_front) | 1 | [lava_pieces_front_SkeletonData.asset](/Users/admin/IncrementalRPG/Assets/Art/Animations/lava_pieces/lava_pieces_front_SkeletonData.asset) | [lava_pieces_front.json](/Users/admin/IncrementalRPG/Assets/Art/Animations/lava_pieces/lava_pieces_front.json) | front_loop |
| [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity) → LavaAnimation/lava_form_back_0/Spine GameObject (lava_pieces_back) | 1 | [lava_pieces_back_SkeletonData.asset](/Users/admin/IncrementalRPG/Assets/Art/Animations/lava_pieces/lava_pieces_back_SkeletonData.asset) | [lava_pieces_back.json](/Users/admin/IncrementalRPG/Assets/Art/Animations/lava_pieces/lava_pieces_back.json) | back_loop |
| [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity) → LavaAnimation/Spine GameObject (lava_pieces_full_screen) | 1 | [lava_pieces_full_screen_SkeletonData.asset](/Users/admin/IncrementalRPG/Assets/Art/Animations/lava_pieces/lava_pieces_full_screen_SkeletonData.asset) | [lava_pieces_full_screen.json](/Users/admin/IncrementalRPG/Assets/Art/Animations/lava_pieces/lava_pieces_full_screen.json) | full_screen_loop |
| [DamageZone.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/DamageZone.prefab) → DamageZone/Spine GameObject (wave_RMB) | 1 | [wave_RMB_SkeletonData.asset](/Users/admin/IncrementalRPG/Assets/Art/Animations/NEW_06_2026/circle/wave_RMB_SkeletonData.asset) | [wave_RMB.json](/Users/admin/IncrementalRPG/Assets/Art/Animations/NEW_06_2026/circle/wave_RMB.json) | attack, idle, ready |
| [DamageZone.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/DamageZone.prefab) → DamageZone/Spine GameObject (fire_auto_contour_down) | 1 | [fire_auto_contour_down_SkeletonData.asset](/Users/admin/IncrementalRPG/Assets/Art/Animations/NEW_06_2026/Fire/auto_contour/fire_auto_contour_down_SkeletonData.asset) | [fire_auto_contour_down.json](/Users/admin/IncrementalRPG/Assets/Art/Animations/NEW_06_2026/Fire/auto_contour/fire_auto_contour_down.json) | attack1, attack2, none |
| [DamageZone.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/DamageZone.prefab) → DamageZone/Spine GameObject (fire_LMB) | 1 | [LMB_SkeletonData.asset](/Users/admin/IncrementalRPG/Assets/Art/Animations/NEW_06_2026/Fire/LMB/LMB_SkeletonData.asset) | [LMB.json](/Users/admin/IncrementalRPG/Assets/Art/Animations/NEW_06_2026/Fire/LMB/LMB.json) | attack1, attack2, attack3, attack4, none |
| [DamageZone.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/DamageZone.prefab) → DamageZone/Spine GameObject (fire_auto_contour_up) | 1 | [fire_auto_contour_up_SkeletonData.asset](/Users/admin/IncrementalRPG/Assets/Art/Animations/NEW_06_2026/Fire/auto_contour/fire_auto_contour_up_SkeletonData.asset) | [fire_auto_contour_up.json](/Users/admin/IncrementalRPG/Assets/Art/Animations/NEW_06_2026/Fire/auto_contour/fire_auto_contour_up.json) | attack1, attack2, none |
| [DamageZone.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/DamageZone.prefab) → DamageZone/Spine GameObject (circle0) | 1 | [circle0_SkeletonData.asset](/Users/admin/IncrementalRPG/Assets/Art/Animations/DamageZone/circle0/circle0_SkeletonData.asset) | [circle0.json](/Users/admin/IncrementalRPG/Assets/Art/Animations/DamageZone/circle0/circle0.json) | attack, idle |
| [Skeleton.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/Entities/Skeleton.prefab) → Spine GameObject (explosion_bones) | 1 | [explosion_bones_SkeletonData.asset](/Users/admin/IncrementalRPG/Assets/Art/Animations/VFX/Explosion_Bones/explosion_bones_SkeletonData.asset) | [explosion_bones.json](/Users/admin/IncrementalRPG/Assets/Art/Animations/VFX/Explosion_Bones/explosion_bones.json) | explosion |
| [Skeleton.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/Entities/Skeleton.prefab) → Spine GameObject (skeleton) | 1 | [skeleton_SkeletonData.asset](/Users/admin/IncrementalRPG/Assets/Art/Animations/Entities/Skeleton/skeleton_SkeletonData.asset) | [skeleton.json](/Users/admin/IncrementalRPG/Assets/Art/Animations/Entities/Skeleton/skeleton.json) | damage, idle, move |
| [Crystal_1.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/Entities/Crystal_1.prefab) → Spine GameObject (explosion_mineral_p1) | 1 | [explosion_mineral_p1_SkeletonData.asset](/Users/admin/IncrementalRPG/Assets/Art/Animations/NEW_06_2026/Explosion_Mineral/explosion_mineral_p1_SkeletonData.asset) | [explosion_mineral_p1.json](/Users/admin/IncrementalRPG/Assets/Art/Animations/NEW_06_2026/Explosion_Mineral/explosion_mineral_p1.json) | explosion |
| [Crystal_1.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/Entities/Crystal_1.prefab) → Spine GameObject (explosion_mineral_p2) | 1 | [explosion_mineral_p2_SkeletonData.asset](/Users/admin/IncrementalRPG/Assets/Art/Animations/NEW_06_2026/Explosion_Mineral/explosion_mineral_p2_SkeletonData.asset) | [explosion_mineral_p2.json](/Users/admin/IncrementalRPG/Assets/Art/Animations/NEW_06_2026/Explosion_Mineral/explosion_mineral_p2.json) | explosion |
| [Crystal_1.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/Entities/Crystal_1.prefab) → Spine GameObject (mineral_v1_part2) | 1 | [mineral_v1_part2_SkeletonData.asset](/Users/admin/IncrementalRPG/Assets/Art/Animations/NEW_06_2026/minerals/mineral_v1_part2_SkeletonData.asset) | [mineral_v1_part2.json](/Users/admin/IncrementalRPG/Assets/Art/Animations/NEW_06_2026/minerals/mineral_v1_part2.json) | damage, idle |
| [Crystal_1.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/Entities/Crystal_1.prefab) → Spine GameObject (mineral_v1_part1) | 1 | [mineral_v1_part1_SkeletonData.asset](/Users/admin/IncrementalRPG/Assets/Art/Animations/NEW_06_2026/minerals/mineral_v1_part1_SkeletonData.asset) | [mineral_v1_part1.json](/Users/admin/IncrementalRPG/Assets/Art/Animations/NEW_06_2026/minerals/mineral_v1_part1.json) | damage, idle |
| [Slime.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/Entities/Slime.prefab) → AnimationBody | 1 | [slime_SkeletonData.asset](/Users/admin/IncrementalRPG/Assets/Art/Animations/Entities/Slime_v2/slime_SkeletonData.asset) | [slime.json](/Users/admin/IncrementalRPG/Assets/Art/Animations/Entities/Slime_v2/slime.json) | damage, idle, move |
| [Slime.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/Entities/Slime.prefab) → Spine GameObject (slime_puddle) | 1 | [slime_puddle_SkeletonData.asset](/Users/admin/IncrementalRPG/Assets/Art/Animations/Entities/Slime_v2/slime_puddle_SkeletonData.asset) | [slime_puddle.json](/Users/admin/IncrementalRPG/Assets/Art/Animations/Entities/Slime_v2/slime_puddle.json) | idle, none |
| [Slime.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/Entities/Slime.prefab) → Spine GameObject (explosion_slime) | 1 | [explosion_slime_SkeletonData.asset](/Users/admin/IncrementalRPG/Assets/Art/Animations/VFX/Explosion_Slime/explosion_slime_SkeletonData.asset) | [explosion_slime.json](/Users/admin/IncrementalRPG/Assets/Art/Animations/VFX/Explosion_Slime/explosion_slime.json) | explosion |
| [Demon.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/Entities/Demon.prefab) → Spine GameObject (demon) | 1 | [demon_SkeletonData.asset](/Users/admin/IncrementalRPG/Assets/Art/Animations/Entities/Demon/demon_SkeletonData.asset) | [demon.json](/Users/admin/IncrementalRPG/Assets/Art/Animations/Entities/Demon/demon.json) | damage, idle, move |
| [Demon.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/Entities/Demon.prefab) → Spine GameObject (explosion_meat) | 1 | [explosion_meat_SkeletonData.asset](/Users/admin/IncrementalRPG/Assets/Art/Animations/VFX/Explosion_Meat/explosion_meat_SkeletonData.asset) | [explosion_meat.json](/Users/admin/IncrementalRPG/Assets/Art/Animations/VFX/Explosion_Meat/explosion_meat.json) | explosion |
| [Crystal_2.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/Entities/Crystal_2.prefab) → Spine GameObject (mineral_v2_part1) | 1 | [mineral_v2_part1_SkeletonData.asset](/Users/admin/IncrementalRPG/Assets/Art/Animations/NEW_06_2026/minerals/mineral_v2_part1_SkeletonData.asset) | [mineral_v2_part1.json](/Users/admin/IncrementalRPG/Assets/Art/Animations/NEW_06_2026/minerals/mineral_v2_part1.json) | damage, idle |
| [Crystal_2.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/Entities/Crystal_2.prefab) → Spine GameObject (explosion_mineral_p1) | 1 | [explosion_mineral_p1_SkeletonData.asset](/Users/admin/IncrementalRPG/Assets/Art/Animations/NEW_06_2026/Explosion_Mineral/explosion_mineral_p1_SkeletonData.asset) | [explosion_mineral_p1.json](/Users/admin/IncrementalRPG/Assets/Art/Animations/NEW_06_2026/Explosion_Mineral/explosion_mineral_p1.json) | explosion |
| [Crystal_2.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/Entities/Crystal_2.prefab) → Spine GameObject (explosion_mineral_p2) | 1 | [explosion_mineral_p2_SkeletonData.asset](/Users/admin/IncrementalRPG/Assets/Art/Animations/NEW_06_2026/Explosion_Mineral/explosion_mineral_p2_SkeletonData.asset) | [explosion_mineral_p2.json](/Users/admin/IncrementalRPG/Assets/Art/Animations/NEW_06_2026/Explosion_Mineral/explosion_mineral_p2.json) | explosion |
| [Crystal_2.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/Entities/Crystal_2.prefab) → Spine GameObject (mineral_v2_part2) | 1 | [mineral_v2_part2_SkeletonData.asset](/Users/admin/IncrementalRPG/Assets/Art/Animations/NEW_06_2026/minerals/mineral_v2_part2_SkeletonData.asset) | [mineral_v2_part2.json](/Users/admin/IncrementalRPG/Assets/Art/Animations/NEW_06_2026/minerals/mineral_v2_part2.json) | damage, idle |
| [Bombs.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/Entities/Bombs.prefab) → Spine GameObject (barrel) | 1 | [barrel_SkeletonData.asset](/Users/admin/IncrementalRPG/Assets/Art/Animations/Entities/barrel/barrel_SkeletonData.asset) | [barrel.json](/Users/admin/IncrementalRPG/Assets/Art/Animations/Entities/barrel/barrel.json) | damage, explosion, idle |
| [Bombs.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/Entities/Bombs.prefab) → Spine GameObject (barrel_exposion) | 1 | [barrel_exposion_SkeletonData.asset](/Users/admin/IncrementalRPG/Assets/Art/Animations/Entities/barrel/barrel_exposion_SkeletonData.asset) | [barrel_exposion.json](/Users/admin/IncrementalRPG/Assets/Art/Animations/Entities/barrel/barrel_exposion.json) | explosion, none |

### SkeletonGraphic

| Где | Time Scale | Skeleton Data | Экспорт | Клипы |
| --- | --- | --- | --- | --- |
| [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity) → HUD/HUD_Panel/GoldScoreView/SkeletonGraphic (ui_coins) | 1 | [ui_coins_SkeletonData.asset](/Users/admin/IncrementalRPG/Assets/Art/Animations/HUD/Coins/ui_coins_SkeletonData.asset) | [ui_coins.json](/Users/admin/IncrementalRPG/Assets/Art/Animations/HUD/Coins/ui_coins.json) | idle, jump |
| [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity) → MenuCanvas/HubPanel/Plateau/CraftButton/CraftImage/SkeletonGraphic (craft_smoke) | 1 | [craft_smoke_SkeletonData.asset](/Users/admin/IncrementalRPG/Assets/Art/Animations/Hub/Craft/craft_smoke_SkeletonData.asset) | [craft_smoke.json](/Users/admin/IncrementalRPG/Assets/Art/Animations/Hub/Craft/craft_smoke.json) | loop |
| [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity) → MenuCanvas/MapView/SkeletonGraphic (map_smoke) | 1 | [map_smoke_SkeletonData.asset](/Users/admin/IncrementalRPG/Assets/Art/Animations/Map/map_smoke_SkeletonData.asset) | [map_smoke.json](/Users/admin/IncrementalRPG/Assets/Art/Animations/Map/map_smoke.json) | idle_1, idle_2 |
| [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity) → MenuCanvas/HubPanel/Plateau/MineButton/SkeletonGraphic (magic_ruins) | 1 | [magic_ruins_SkeletonData.asset](</Users/admin/IncrementalRPG/Assets/Art/Animations/NEW_06_2026/Выбор класса/spine/magic_ruins_SkeletonData.asset>) | [magic_ruins.json](</Users/admin/IncrementalRPG/Assets/Art/Animations/NEW_06_2026/Выбор класса/spine/magic_ruins.json>) | hover, idle |
| [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity) → MenuCanvas/HubPanel/SkeletonGraphic (fireflies) (1) | 0.5 | [fireflies_SkeletonData.asset](/Users/admin/IncrementalRPG/Assets/Art/Animations/Hub/Firefly/fireflies_SkeletonData.asset) | [fireflies.json](/Users/admin/IncrementalRPG/Assets/Art/Animations/Hub/Firefly/fireflies.json) | loop1, loop2, loop3 |
| [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity) → MenuCanvas/MapView/SkeletonGraphic (map_smoke)_2 | 1 | [map_smoke_SkeletonData.asset](/Users/admin/IncrementalRPG/Assets/Art/Animations/Map/map_smoke_SkeletonData.asset) | [map_smoke.json](/Users/admin/IncrementalRPG/Assets/Art/Animations/Map/map_smoke.json) | idle_1, idle_2 |
| [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity) → MenuCanvas/MapView/SkeletonGraphic (map_light_particles) | 1 | [map_light_particles_SkeletonData.asset](/Users/admin/IncrementalRPG/Assets/Art/Animations/Map/map_light_particles_SkeletonData.asset) | [map_light_particles.json](/Users/admin/IncrementalRPG/Assets/Art/Animations/Map/map_light_particles.json) | idle |
| [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity) → MenuCanvas/HubPanel/Plateau/BarracksButton/BarracksImage/SkeletonGraphic (torch) | 1 | [torch_SkeletonData.asset](/Users/admin/IncrementalRPG/Assets/Art/Animations/Hub/Torch/torch_SkeletonData.asset) | [torch.json](/Users/admin/IncrementalRPG/Assets/Art/Animations/Hub/Torch/torch.json) | loop |
| [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity) → MenuCanvas/HubPanel/Plateau/CraftButton/CraftImage/SkeletonGraphic (craft_fire) | 1 | [craft_fire_SkeletonData.asset](/Users/admin/IncrementalRPG/Assets/Art/Animations/Hub/Craft/craft_fire_SkeletonData.asset) | [craft_fire.json](/Users/admin/IncrementalRPG/Assets/Art/Animations/Hub/Craft/craft_fire.json) | loop |
| [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity) → MenuCanvas/HubPanel/Plateau/BarracksButton/BarracksImage/SkeletonGraphic (torch) (1) | 1 | [torch_SkeletonData.asset](/Users/admin/IncrementalRPG/Assets/Art/Animations/Hub/Torch/torch_SkeletonData.asset) | [torch.json](/Users/admin/IncrementalRPG/Assets/Art/Animations/Hub/Torch/torch.json) | loop |
| [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity) → HUD/HUD_Panel/EnemyScoreView/SkeletonGraphic (ui_skull) | 1 | [ui_skull_SkeletonData.asset](/Users/admin/IncrementalRPG/Assets/Art/Animations/HUD/Skull/ui_skull_SkeletonData.asset) | [ui_skull.json](/Users/admin/IncrementalRPG/Assets/Art/Animations/HUD/Skull/ui_skull.json) | idle |
| [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity) → HUD/LevelTransitionCurtainView/RevealGroup/LootboxGroup/SkeletonGraphic (chest) | 1 | [chest_SkeletonData.asset](/Users/admin/IncrementalRPG/Assets/Art/Animations/NEW_06_2026/Chest/chest_SkeletonData.asset) | [chest.json](/Users/admin/IncrementalRPG/Assets/Art/Animations/NEW_06_2026/Chest/chest.json) | idle_close, idle_open, open |
| [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity) → MenuCanvas/HubPanel/SkeletonGraphic (fireflies) | 0.5 | [fireflies_SkeletonData.asset](/Users/admin/IncrementalRPG/Assets/Art/Animations/Hub/Firefly/fireflies_SkeletonData.asset) | [fireflies.json](/Users/admin/IncrementalRPG/Assets/Art/Animations/Hub/Firefly/fireflies.json) | loop1, loop2, loop3 |
| [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity) → MenuCanvas/HubPanel/Plateau/BarracksButton/BarracksImage/SkeletonGraphic (torch) (2) | 1 | [torch_SkeletonData.asset](/Users/admin/IncrementalRPG/Assets/Art/Animations/Hub/Torch/torch_SkeletonData.asset) | [torch.json](/Users/admin/IncrementalRPG/Assets/Art/Animations/Hub/Torch/torch.json) | loop |
| [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity) → MenuCanvas/HubPanel/SkeletonGraphic (fireflies) (3) | 0.5 | [fireflies_SkeletonData.asset](/Users/admin/IncrementalRPG/Assets/Art/Animations/Hub/Firefly/fireflies_SkeletonData.asset) | [fireflies.json](/Users/admin/IncrementalRPG/Assets/Art/Animations/Hub/Firefly/fireflies.json) | loop1, loop2, loop3 |
| [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity) → MenuCanvas/HubPanel/SkeletonGraphic (fireflies) (2) | 0.5 | [fireflies_SkeletonData.asset](/Users/admin/IncrementalRPG/Assets/Art/Animations/Hub/Firefly/fireflies_SkeletonData.asset) | [fireflies.json](/Users/admin/IncrementalRPG/Assets/Art/Animations/Hub/Firefly/fireflies.json) | loop1, loop2, loop3 |
| [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity) → MenuCanvas/InventoryView/DesignFrame/LeftBlock/SkeletonGraphic (inventory_recycle) | 1 | [inventory_recycle_SkeletonData.asset](/Users/admin/IncrementalRPG/Assets/Art/Animations/NEW_06_2026/Inventory_Recycle/inventory_recycle_SkeletonData.asset) | [inventory_recycle.json](/Users/admin/IncrementalRPG/Assets/Art/Animations/NEW_06_2026/Inventory_Recycle/inventory_recycle.json) | idle, recycle |
| [GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity) → MenuCanvas/MapView/SkeletonGraphic (map_light) | 1 | [map_light_SkeletonData.asset](/Users/admin/IncrementalRPG/Assets/Art/Animations/Map/map_light_SkeletonData.asset) | [map_light.json](/Users/admin/IncrementalRPG/Assets/Art/Animations/Map/map_light.json) | idle |
| [NodeView.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/SkillTree/NodeView.prefab) → NodeView/Background/SkeletonGraphic (lock_n_chain) | 1 | [lock_n_chain_SkeletonData.asset](/Users/admin/IncrementalRPG/Assets/Art/Animations/NEW_06_2026/Lock_n_chain/lock_n_chain_SkeletonData.asset) | [lock_n_chain.json](/Users/admin/IncrementalRPG/Assets/Art/Animations/NEW_06_2026/Lock_n_chain/lock_n_chain.json) | cancel, idle, none, open |
| [NodeLevelLamp.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/SkillTree/NodeLevelLamp.prefab) → NodeLevelLamp | 1 | [LevelCounter_SkeletonData.asset](/Users/admin/IncrementalRPG/Assets/Art/Animations/NEW_06_2026/LevelAndNodeCounter/v1/LevelCounter_SkeletonData.asset) | [LevelCounter.json](/Users/admin/IncrementalRPG/Assets/Art/Animations/NEW_06_2026/LevelAndNodeCounter/v1/LevelCounter.json) | idle_off, idle_on, on |
| [LevelTransitionLamp.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/HUD/LevelTransitionLamp.prefab) → LevelTransitionLamp | 1 | [LevelCounter_SkeletonData.asset](/Users/admin/IncrementalRPG/Assets/Art/Animations/NEW_06_2026/LevelAndNodeCounter/LevelCounter_SkeletonData.asset) | [LevelCounter.json](/Users/admin/IncrementalRPG/Assets/Art/Animations/NEW_06_2026/LevelAndNodeCounter/LevelCounter.json) | idle_off, idle_on, on |

## Используемая конфигурация инвентарного плагина

[Смысл настроек — глава «Предметы, награды, инвентарь»](/Users/admin/IncrementalRPG/Docs/GameplaySettings.ru.md). Здесь показаны сохранённые поля текущих компонентов.

### UniversalInventory

[Исходный компонент](/Users/admin/IncrementalRPG/Assets/Plugins/UniversalDragAndDrop/Scripts/Inventories/UniversalInventory.cs).

[GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:56615) → `MenuCanvas/InventoryView/DesignFrame/RightBlock/InventoryBackground`.

Поля сохранённого компонента:

| Поле | Значение/тип |
| --- | --- |
| `_slotContainer` | Ссылка на объект сцены/префаба |
| `baseSlotPrefab` | [ShapedSlot.prefab](/Users/admin/IncrementalRPG/Assets/Plugins/UniversalDragAndDrop/Prefabs/ShapedSlot.prefab) |
| `_initialSlotCount` | 36 |
| `_inventoryStrategy` | Вложенная структура; раскрыть в Inspector |
| `_slotManagementSettings` | Вложенная структура; раскрыть в Inspector |
| `_ruleValidator` | Вложенная структура; раскрыть в Inspector |
| `_dropPolicy` | Вложенная структура; раскрыть в Inspector |
| `_slots` | [] |
| `_useGridTopology` | 1 |
| `_gridTopology` | Вложенная структура; раскрыть в Inspector |
| `_shapedPlacementAnchorStrategy` | Вложенная структура; раскрыть в Inspector |

### InventoryDragVisualBinder

[Исходный компонент](/Users/admin/IncrementalRPG/Assets/Plugins/UniversalDragAndDrop/Scripts/UI/InventoryDragVisualBinder.cs).

[GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:56671) → `MenuCanvas/InventoryView/DesignFrame/RightBlock/InventoryBackground`.

Поля сохранённого компонента:

| Поле | Значение/тип |
| --- | --- |
| `_inventory` | Ссылка на объект сцены/префаба |
| `_dragVisualPrefab` | [SourceSizedDragVisual.prefab](/Users/admin/IncrementalRPG/Assets/Plugins/UniversalDragAndDrop/Prefabs/SourceSizedDragVisual.prefab) |

### PlacementOverlay

[Исходный компонент](/Users/admin/IncrementalRPG/Assets/Plugins/UniversalDragAndDrop/Scripts/UI/PlacementOverlay.cs).

[GameScene.unity](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity:56734) → `MenuCanvas/InventoryView/DesignFrame/RightBlock/PlacementOverlay`.

Поля сохранённого компонента:

| Поле | Значение/тип |
| --- | --- |
| `_inventory` | Ссылка на объект сцены/префаба |
| `_overlayRoot` | Ссылка на объект сцены/префаба |
| `_itemPrefab` | [InventoryPlacementOverlayItem.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/Inventory/InventoryPlacementOverlayItem.prefab) |
| `_color` | {r: 1, g: 1, b: 1, a: 1} |

### PlacementOverlayItem

[Исходный компонент](/Users/admin/IncrementalRPG/Assets/Plugins/UniversalDragAndDrop/Scripts/UI/PlacementOverlayItem.cs).

[InventoryPlacementOverlayItem.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/Inventory/InventoryPlacementOverlayItem.prefab:41) → `InventoryPlacementOverlayItem`.

Поля сохранённого компонента:

| Поле | Значение/тип |
| --- | --- |
| `_image` | Ссылка на объект сцены/префаба |
| `_countContainer` | Ссылка на объект сцены/префаба |
| `_countText` | Ссылка на объект сцены/префаба |

### Вложенные правила текущего UniversalInventory

`Grid Topology`: 9 колонок, 4 строки. `Inventory Strategy`: AutoMergeSeparableStacksStrategy; `_dragAmount = 0`, `_customDragAmount = 1`, `_maxStackSize = 99`, `_allowItemStackOverride = true`. `Slot Management Settings`: FixedSlotManagementSettings. Списки дополнительных правил пусты.

`Drop Policy`: `_blockedTargetResolution`, `_alternativeOrderer`, `_allowSameInventoryAlternativePlacement`, `_swapDisplacement`, `_partialOverlapSwap`, `_allowPartial`. Эти поля выбирают реакцию на занятое/неподходящее место, порядок поиска альтернативы, разрешение обменов и частичного переноса. Пользуйтесь именованными вариантами Inspector; числа сериализации не являются процентами. Сейчас разрешён частичный перенос, стратегия альтернативы — MergeFirstPlacementCandidateOrderer, якорь формы — RotatedGrabOffsetAnchorStrategy.

<a id="inventorystatisticsview"></a>

### InventoryStatisticsView

Сцена: `GameScene → MenuCanvas/InventoryView/DesignFrame/RightBlock/StatisticsBackground`. [Код](/Users/admin/IncrementalRPG/Assets/Scripts/UI/Inventory/InventoryStatisticsView.cs).

| Поле | Настройка |
|---|---|
| `_title`, `_hint` | TMP-заголовок и подсказка управления, текст из LocalesTable |
| `_attacksTitle`, `_zoneTitle`, `_barrelsTitle` | Локализованные заголовки трёх секций; игровые иконки и разделители настроены в дочерних объектах сцены |
| `_labels`, `_values` | Семь TMP-строк в порядке EquipmentStats.Ids: урон ручной/авто, частота ручной/авто, радиус зоны, урон/радиус бочки |

Панель показывает рассчитанные игровые характеристики с учётом тренировки, экипировки и активных зелий. Значения не задаются в UI вручную.

<a id="inventoryitemtooltipview"></a>

### InventoryItemTooltipView

Prefab: [InventoryItemTooltip](/Users/admin/IncrementalRPG/Assets/Prefabs/Inventory/InventoryItemTooltip.prefab), канвас [InventoryTooltipCanvas](/Users/admin/IncrementalRPG/Assets/Prefabs/Inventory/InventoryTooltipCanvas.prefab). [Код](/Users/admin/IncrementalRPG/Assets/Scripts/UI/Inventory/InventoryItemTooltipView.cs).

| Поле | Настройка |
|---|---|
| `_itemName`, `_description` | TMP для имени и описания экземпляра; локаль обновляется автоматически |
| `_icon` | Image иконки определения; preserveAspect включён |
| `_divider` | Тонкий разделитель между шапкой и описанием |
| `_preferredWidth` | 440 единиц Canvas, ограничивается шириной экрана |
| `_padding`, `_iconSize`, `_contentGap` | 24 / 64 / 20: поля, размер иконки, промежуток шапки и описания |
| `_bodyFontSize`, `_nameFontSize` | 18 / 24 pt; описание может уменьшаться до 14 pt, если высоты экрана недостаточно. Предел 14 pt задан в коде |

Канвас выбирается в `PlayerInventoryView._tooltipCanvasPrefab`. Подсказка показывает редкость, проценты и цену экземпляра; имя брони и метка редкости окрашены палитрой `EquipmentStats.RarityColor`, заданной в коде. Высота подстраивается под описание и переносы; отступ 12 px от края экрана задан в коде `InventoryItemTooltipView`. Фон: Image Type = Sliced, border спрайта 70 px со всех сторон, pixelsPerUnitMultiplier = 3. CanvasScaler: 1920×1080, Match = 0.5; sortingOrder = 2900, ниже канваса переноса с order 3000.

### InventoryRarityBackdrop

Дочерний объект `RarityBackdrop` в [InventoryPlacementOverlayItem.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/Inventory/InventoryPlacementOverlayItem.prefab), позади иконки. Требует CanvasRenderer. Фон и рамка охватывают всю область предмета и используют редкость экземпляра. Иконка не тонируется; пустые клетки не меняются.

| Поле / настройка | Значение и назначение |
|---|---|
| `_tintStrength` | Inspector: 0.16, диапазон 0–1; примесь цвета редкости к непрозрачному тёмному фону |
| `_borderWidth` | Inspector: 1.5 единицы Canvas, минимум 0.5 |
| RectTransform offsets | +4 / −4: отступ рамки от внешних границ занимаемых клеток |
| `EquipmentStats.RarityColor` | В коде [EquipmentStats.cs](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Items/EquipmentStats.cs:43): общая палитра фона, имени и метки редкости — серый / зелёный / фиолетовый / золотой |

Тёмная основа заливки и голубая рамка наведения/переноса/выбора заданы в [InventoryRarityBackdrop.cs](/Users/admin/IncrementalRPG/Assets/Scripts/UI/Inventory/InventoryRarityBackdrop.cs); в Inspector этих цветов нет. `BootsPlaceholder` в `PlayerInventoryView` — видимый декоративный Image, preserveAspect включён, raycastTarget выключен, рабочего drop-слота нет.

## Ковка

[Параметры и значения кузницы](/Users/admin/IncrementalRPG/Docs/GameplaySettings.ru.md#forge-settings) · [Настройка оформления](/Users/admin/IncrementalRPG/Docs/PresentationSettings.ru.md)

| Объект | Параметры |
|---|---|
| [ForgeConfig](/Users/admin/IncrementalRPG/Assets/SO/Forge/ForgeConfig.asset) | `firstAvailableLocation`, `levels[].shardCost`, `rarityWeights`, `cursorPeriod`, `rarityRange`, `allStatsRange`, `oneStatRange`, `allStatsBonus`, `oneStatBonus`, `statRanges`, `goldPerBonusPercent`, `equipment` |
| [Forgedhelmet](/Users/admin/IncrementalRPG/Assets/SO/Forge/Forgedhelmet.asset), [Forgedchest](/Users/admin/IncrementalRPG/Assets/SO/Forge/Forgedchest.asset), [Forgedweapon](/Users/admin/IncrementalRPG/Assets/SO/Forge/Forgedweapon.asset) | Определение типа, `forgeNameKeys`, `forgeIcons`, базовые имя и иконка |
| [CraftView](/Users/admin/IncrementalRPG/Assets/Scripts/UI/Forge/CraftView.cs) в GameScene | Ссылки на тексты, кнопки, список свитков, шкалу, анимации и окно результата |
| [ForgeScrollRow](/Users/admin/IncrementalRPG/Assets/Prefabs/Forge/ForgeScrollRow.prefab) | Рамки обычного/выбранного состояния, значок и количество |

## Как проверялся справочник

Сопоставлены объявления сериализованных полей собственных скриптов, места их использования в формулах/управляющей логике, GUID-ссылки игровых assets, сцены, префабы и используемые JSON-экспорты Spine. Реестр не является снимком запущенного Play Mode. Игровой код, конфиги и тесты при составлении документации не изменялись.
