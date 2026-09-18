# Анимации, интерфейс, эффекты и звук

[Начало справочника](/Users/admin/IncrementalRPG/Docs/README.md) · [Геймплей](/Users/admin/IncrementalRPG/Docs/GameplaySettings.ru.md) · [Каталог объектов и полей](/Users/admin/IncrementalRPG/Docs/SettingsCatalog.ru.md)

## 1. Общие правила для аниматора

Игровые Spine-ресурсы лежат в [Assets/Art/Animations](/Users/admin/IncrementalRPG/Assets/Art/Animations). Используемые в конкретных префабах/сценах `SkeletonDataAsset` перечислены в каталоге: в проекте есть старые варианты, поэтому выбирайте ресурс по ссылке компонента, а не по похожему названию папки.

- **SkeletonAnimation** используется в мире, **SkeletonGraphic** — внутри Canvas. В их Inspector задаются `Skeleton Data Asset`, начальная анимация, `Loop`, `Time Scale`, skin и параметры отображения. `Time Scale = 2` проигрывает клипы вдвое быстрее, `0.5` — вдвое медленнее, если игровой код не перезаписывает скорость.
- Рисунок движения, ключи, длительность, меши, кости, слоты и события редактируются в исходном Spine-проекте, затем экспортируются обратно в используемый JSON/atlas/текстуры. В репозитории найдены экспортированные JSON; исходные `.spine` в папке игровых анимаций не найдены.
- `SkeletonDataAsset` связывает экспорт и atlas, задаёт масштаб импорта и смешивание анимаций. Масштаб импорта влияет на всех потребителей этого ресурса. `Transform.localScale` конкретного объекта влияет на его представление; игровой хитбокс задаётся отдельно.
- Если компонент управления вызывает `SetAnimation`, его имя/состояние имеет приоритет над начальной анимацией Spine-компонента. У ряда UI-переходов код ставит `MixDuration = 0`: глобальный default mix не изменит именно эти переходы.
- На сцене могут быть Overrides префаба. Для общего изменения редактируйте префаб, для отдельного экземпляра — его Override.

**Скорость анимации не равна частоте события.** `Time Scale` существа не меняет скорость перехода между клетками, а `Time Scale` атаки не задаёт её урон и таймер. Сначала решите, нужен другой темп игровой механики или другой темп визуального клипа.

## 2. Дамаг-зона: круг, ручная, авто и особая атака

Откройте [DamageZone.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/DamageZone.prefab), компонент `DamageZoneView` на корне. Игровые урон, частота и радиусы — в [DamageZoneConfig](/Users/admin/IncrementalRPG/Assets/SO/DamageZone/DamageZoneConfig.asset).

| Поле `DamageZoneView` | Подключённое представление |
|---|---|
| `_circle` | Круг общей зоны: `Spine GameObject (circle0)` |
| `_manualWave` | Ручная атака: `Spine GameObject (fire_LMB)` |
| `_autoWaveBack` | Задняя часть автоатаки: `Spine GameObject (fire_auto_contour_down)` |
| `_autoWaveFront` | Передняя часть автоатаки: `Spine GameObject (fire_auto_contour_up)` |
| `_specialWave` | Особая атака/её готовность: `Spine GameObject (wave_RMB)` |
| `_baseRadiusX` | Радиус X картинки круга при исходном масштабе префаба; калибровка, а не характеристика прокачки |

На каждой из перечисленных частей есть собственный `SkeletonAnimation`: меняйте `Time Scale`, используемый `SkeletonDataAsset` и исходный масштаб соответствующей части. Для автоатаки обе части должны сохранять согласованный темп.

Код сохраняет исходные масштабы частей и затем каждый кадр умножает их на `игровой RadiusX / _baseRadiusX`. Поэтому ручное изменение масштаба после начала игры переопределяется. Для постоянного размера редактируйте исходный префаб, а для игрового охвата — `baseRadius` и ноды `ZoneRadius`. Соотношение осей игрового эллипса задаёт `aspectRatio`; оно не переписывает отдельно высоту Spine-рисунка.

### Имена и состояния

| Часть | Что запрашивает текущий код |
|---|---|
| Ручная атака | Случайная `attack1`…`attack4`, после завершения — `none` |
| Автоатака | Случайная `attack1` или `attack2`; одно имя одновременно на задней и передней частях, затем `none` |
| Особая атака | `idle` при перезарядке, `ready` при готовности, одноразовая `attack` при ударе |
| Индикатор особой атаки | Кость `cooldown`, которую код обновляет по оставшейся перезарядке |

Эти имена и имя кости закреплены в `DamageZoneView`; произвольное переименование в Spine требует согласованного изменения кода. Круг использует собственное состояние Spine-компонента. Урон наносится игровой логикой при атаке, а не событием последнего кадра клипа. Длинный клип при частых атаках может перезапускаться.

## 3. Существа, кристаллы, бочки и смерть

Прежде всего откройте нужный `EntityConfig` и перейдите по `View Prefab`. Префабы: [Entities](/Users/admin/IncrementalRPG/Assets/Prefabs/Entities). `Creature.prefab` — общая основа; `Slime`, `Skeleton`, `Demon`, `Crystal_1`, `Crystal_2`, `Bombs` содержат свои настройки и Spine-части.

| Поле `CreatureView` | Назначение |
|---|---|
| `_animationBody` | Основное живое тело, `SkeletonAnimation` |
| `_additionalAnimationBodies` | Дополнительные части живого тела, получающие общие состояния |
| `_deathAnimationBodies` | Части/эффекты, запускаемые при смерти |
| `_deathAnimationBodies[]._animationBody` | Spine-компонент конкретного эффекта смерти |
| `_deathAnimationBodies[]._animationName` | Имя клипа смерти; по умолчанию `explosion` |
| `_deathAnimationBodies[]._visibleWhileAlive` | Показывать эту часть и пока существо живо |
| `_deathAnimationBodies[]._waitForComplete` | Ждать завершения этого клипа перед возвратом представления в пул |
| `_footAnchor` | Точка привязки тела к клетке/земле |
| `_damagePopupAnchor` | Точка появления числа урона |
| `_damagePopupOffset` | Смещение от `_footAnchor` или корня, если отдельный `_damagePopupAnchor` не назначен |
| `_facesRightByDefault` | Исходное направление рисунка для отражения при движении |

Живые части используют имена `idle`, `move`, `damage`. Если `move` отсутствует, движение отображается через `idle`; отсутствие `damage` означает отсутствие этого визуального отклика. Имена живых состояний закреплены в коде. Имя клипа каждой смерти редактируется в списке `_deathAnimationBodies`.

Скорость клипов живого тела и эффектов смерти меняйте в **Time Scale соответствующего SkeletonAnimation**. `CreatureView` при паузе запоминает прежние скорости и восстанавливает их, поэтому настроенная скорость сохраняется. Для многосоставного существа настройте все нужные части. Если `_waitForComplete` включён, длительность смерти влияет на время присутствия картинки перед возвратом в пул; слишком медленная/не завершающаяся анимация задерживает этот момент.

**Движение по карте:** `EntityConfig.moveSpeed`, `moveChance`, `moveCheckInterval`. **Охват попадания:** `EntityConfig.damageZoneHitRadius`. Это независимые от Spine поля. Увеличение масштаба рисунка не увеличивает хитбокс автоматически.

### Бочка

В [Bombs.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/Entities/Bombs.prefab) отдельны `barrel` и `barrel_exposion`. У взрыва есть `BombExplosionVisualScaler._explosionVisual`: визуальный Transform масштабируется относительно своего исходного масштаба по отношению текущего радиуса к базовому радиусу взрыва. Сначала откалибруйте исходную картинку под `BombExplosionConfig.baseRadius`; бонусы радиуса затем масштабируют её. Урон и радиус меняются в конфиге взрыва, HP — в конфиге самой бочки.

### Кристаллы

На светящейся части кристаллов находятся:

| Компонент / поле | Назначение |
|---|---|
| `ShineSweepAnimator._sweepDuration` | Длительность прохода блика, с |
| `_pauseDuration` | Пауза между бликами, с |
| `_initialDelay` | Задержка первого блика, с |
| `_renderer` | Renderer, материал которого получает положение блика |
| `SkeletonTint._color` | Цвет тонировки указанного Renderer |

Положение блика `_ShineLocation` управляется кодом. Ширину/силу блика и прочие свойства рисунка меняйте в назначенном материале, если его шейдер предоставляет их. У `Crystal_1` и `Crystal_2` свои части и отдельные эффекты разрушения; они перечислены в каталоге Spine.

## 4. Осколки и числа над объектами

Разлёт, время на поле и время сбора осколков — [ShardPickupConfig](/Users/admin/IncrementalRPG/Assets/SO/Shard/ShardPickupConfig.asset), глава осколков в игровом руководстве. Картинка — [ShardPickup.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/Shard/ShardPickup.prefab), `ShardPickupView._icon`.

В `ShardPickupView` подъём при накоплении прогресса сбора и блеск пока заданы кодом: `LiftHeight = 0.06`, `LiftSpeed = 0.4`, `ShineIntensity = 0.3`. Время полёта собранного осколка к HUD — `HudView.ShardFlightDuration = 0.35` с. Эти значения не имеют отдельного поля Inspector.

`GameScene → DamagePopupLayerView`, компонент с тем же именем:

| Поле | Что меняет |
|---|---|
| `_popupPrefab` | Префаб всплывающего числа |
| `_poolRoot` | Контейнер созданных представлений |
| `_poolSize` | Максимальное количество одновременно доступных представлений |
| `_overflowMode` | Поведение при заполненном пуле: переиспользовать старое либо пропустить новое |
| `_duration` | Время показа одного числа, с |
| `_moveDistance` | Дальность движения числа в мире |
| `_horizontalJitter`, `_verticalJitter` | Случайный разброс начальной позиции |

Шрифт/цвет/размер числа — TMP-компонент в [DamagePopupView.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/MiscPanels/DamagePopupView.prefab). Точка появления на существе — `CreatureView`.

Всплывающее золото HUD — [GoldPopupView.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/HUD/GoldPopupView.prefab). Его длительность 1,2 с и перемещение 60 единиц UI сейчас закреплены в `GoldPopupView.Animate`. `HudView` объединяет поступления за 0,2 с; пул золота содержит 10 представлений. Плавность счётчиков убийств/опыта задаётся константой `LerpSpeed = 8`.

## 5. Переход между уровнями: вся последовательность

Текущая последовательность: достижение цели → время добора осколков → закрытие шторки → смена уровня за шторкой → проявление содержимого → задержка лампы → анимация включения лампы → открытие сундука → лента → результат → выбор игрока → открытие шторки.

### Длительности игровых фаз

`GameScene → --Reflex--/SceneScope → GameSceneInstaller → Level Transition Config`:

| Поле | Что меняет | Сохранено в сцене |
|---|---|---|
| `closeDuration` | Время закрытия шторки, с | 0,7 |
| `openDuration` | Время открытия шторки, с | 1,16 |
| `lootGraceDuration` | Окно добора осколков после цели, с | 1,5 |
| `holdDuration` | Поле существует и передаётся событием, но текущий `LevelTransitionCoordinator` его **не использует** как паузу | 3,3 |

Чтобы ускорить шторку, уменьшайте `closeDuration`/`openDuration`. Общая длительность показа награды не определяется `holdDuration`: она зависит от лампы, сундука, прокрутки и ожидания выбора игрока.

### Геометрия и проявление шторки

`GameScene → HUD/LevelTransitionCurtainView`, компонент `LevelTransitionCurtainView`:

| Поле | Что меняет |
|---|---|
| `_movementCurve` | Прогресс движения по нормализованному времени 0…1; обычно начало 0, конец 1 |
| `_revealFadeInDuration` | Время появления центрального содержимого после закрытия, с; сейчас 0,2 |
| `_lampAnimationDelay` | Пауза после появления содержимого до включения лампы; сейчас **1 с** |
| `_revealFadeOutDuration` | Исчезновение содержимого при открытии; не дольше самого открытия; сейчас 0,2 с |
| `_offscreenPadding` | Дополнительный выход половин за экран в открытом положении; сейчас 8 UI-единиц |
| `_closedOverlap` | Перекрытие двух половин в центре; сейчас 120 UI-единиц |
| `_seamOffset` | Смещение центрального шва; сейчас 0 |
| `_hideWhenIdle` | Отключать объект целиком после окончания перехода |
| `_leftCurtain`, `_rightCurtain` | Две движущиеся половины |
| `_curtainViewport` | Область, по которой рассчитываются открытые/закрытые положения |
| `_rootGroup`, `_revealGroup`, `_lampCounter` | Общая видимость, группа содержимого и счётчик ламп |

Позиции половин вычисляются по их границам и viewport. Кривая задаёт профиль ускорения, длительность задаётся в предыдущей таблице.

### Лампы пройденных уровней

`HUD/LevelTransitionCurtainView/RevealGroup/LevelCounter → LevelTransitionLampCounterView`:

- `_lampPrefab`: [LevelTransitionLamp.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/HUD/LevelTransitionLamp.prefab).
- `_spacing`: расстояние между лампами; отрицательное значение допускает перекрытие.
- `_horizontalPadding`, `_verticalPadding`: поля контейнера.
- `_idleOffAnimationName`, `_turnOnAnimationName`, `_idleOnAnimationName`: состояния, сейчас `idle_off`, `on`, `idle_on`.
- `_container`: контейнер расположения ламп.

Число ламп следует числу уровней подземелья. Завершение анимации `on` запускает сундук. В этой системе код принудительно выставляет `SkeletonGraphic.timeScale` в 1 при обычной работе и 0 при паузе: изменение Time Scale префаба не является постоянной настройкой скорости этих ламп. Для изменения длительности `on` используйте клип Spine либо потребуется отдельное поле в коде.

## 6. Сундук, прокрутка и результат

`GameScene → HUD/LevelTransitionCurtainView/RevealGroup/LootboxGroup → LootboxView`:

| Поле | Что меняет | Сейчас |
|---|---|---|
| `_spinSpeed` | Скорость движения ленты в единицах Canvas в секунду | 2200 |
| `_spinStartDelay` | Минимальное ожидание открытия, с; дополнительно ждёт завершения Spine-клипа `open` | 0,6 |
| `_constantSpinDuration` | Время движения с постоянной скоростью, с | 1,8 |
| `_settleDuration` | Длительность торможения до победителя, с | 1,2 |
| `_winnerEntryPadding` | Запас для входа победившей иконки за границей viewport | 40 |
| `_itemSize` | Размер иконок ленты, UI-единицы | 150×150 |
| `_itemSpacing` | Шаг между центрами иконок | 190 |
| `_ambientIcons` | Резервные иконки прокрутки. При переходе используются иконки допустимых наград пула завершённого уровня; список не задаёт вероятности | Список спрайтов |
| `_closedIdleAnimationName` | Закрытый сундук | `idle_close` |
| `_openAnimationName` | Открытие сундука | `open` |
| `_openIdleAnimationName` | Открытый сундук | `idle_open` |
| `_chest` | Spine-компонент сундука | Дочерний `SkeletonGraphic (chest)` |
| `_itemViewport` | Видимая область ленты | RectTransform |
| `_resultPopupPrefab`, `_resultPopupParent` | Префаб результата и место его создания | Ссылки |
| `_transitionLabels` | Надписи, скрываемые при показе результата | Список объектов |

`_settleDuration` — именно длительность, а не коэффициент торможения. Дистанция торможения вычисляется как `_spinSpeed × _settleDuration / 2`. Может добавляться участок подхода, чтобы иконки непрерывно вошли в кадр; общее время не всегда равно простой сумме трёх длительностей.

Код `LootboxView` выставляет сундуку `timeScale = 1` при подготовке и после паузы. Для устойчивого изменения скорости самого открытия меняйте длительность клипа `open` в Spine; уменьшение `_spinStartDelay` не ускоряет более длинный клип. Для отдельного множителя скорости сундука в Inspector пока потребовалась бы доработка.

### Появление полученного предмета

Префаб: [LootRewardPopup.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/HUD/LootRewardPopup.prefab), компонент `RewardRevealEffect`:

| Поле | Назначение |
|---|---|
| `_accent` | Основной цвет эффекта |
| `_intensity` | Общая интенсивность, диапазон 0,5…1,5 |
| `_rayRevolutionSeconds` | Время полного оборота лучей; больше — медленнее |
| `_appearanceDuration` | Длительность появления композиции, с |
| `_floatAmplitude` | Амплитуда последующего покачивания иконки |
| `_icon`, `_rays`, `_secondaryRays`, `_halo`, `_particleRoot` | Части композиции |
| `_mistSprite`, `_sparkSprite` | Спрайты дымки и искр |

На дочерних `RadialRays`/`WarmRays` компонент `RewardRaysGraphic`: `_rayCount` — число лучей, `_rayWidth` — угловая ширина, `_angleOffset` — начальный поворот. Базовый цвет доступен у UI Graphic; `RewardRevealEffect` во время показа задаёт цвет и прозрачность своим частям из `_accent` и интенсивности. Радиальный профиль прозрачности лучей закреплён массивами в `RewardRaysGraphic`.

`LootRewardPopupView` связывает CanvasGroup, композицию, заголовок, иконку, статус, кнопки продолжения/использования и эффект. Число частиц и внутренние кривые их движения пока заданы кодом `RewardRevealEffect` (22 облачка, 8 искр, 30 частиц вспышки); отдельные поля для них не выведены.

## 7. Анимации и оформление дерева

`GameScene → MenuCanvas/SkillTreeView → SkillTreeView`:

| Поле | Что меняет |
|---|---|
| `_connectionRevealDuration` | Длительность появления связи, с |
| `_nodeRevealDuration` | Длительность появления ноды, с |
| `_nodeRevealStartScale` | Начальный масштаб появления относительно нормального |
| `_revealWaveDelay` | Задержка между шагами волны раскрытия |
| `_contentPadding` | Поля вокруг графа для области прокрутки |
| `_initialFocusNode` | Нода начального фокуса |
| `_nodesLayer`, `_connectionsLayer` | Слои нод и связей |
| `_nodeViewPrefab`, `_connectionViewPrefab`, `_popupView` | Представления ноды, связи и всплывающего описания |
| `_goldText`, `_shardText`, `_shardCounterRoot`, `_closeButton` | Счётчики и кнопка выхода |

Префабы: [NodeView](/Users/admin/IncrementalRPG/Assets/Prefabs/SkillTree/NodeView.prefab), [NodeConnection](/Users/admin/IncrementalRPG/Assets/Prefabs/SkillTree/NodeConnection.prefab), [NodeLevelLamp](/Users/admin/IncrementalRPG/Assets/Prefabs/SkillTree/NodeLevelLamp.prefab).

- `NodeView._stateCircleRotationDegreesPerSecond`: вращение оформления ноды, градусы/с; знак меняет направление.
- `_lockedSkeleton`: Spine замка; `_lockedIdleAnimationName`, `_lockedOpenAnimationName`, `_lockedCancelAnimationName` — имена состояний (`idle`, `open`, `cancel`). `_icon`, `_additionalIcon`, `_stateCircleImage`, `_levelCounter` — ссылки на части.
- `NodeConnectionView._thickness`: толщина линии.
- `NodeLevelCounterView`: `_lampPrefab`, `_container`, `_layoutGroup` и три имени состояний (`idle_off`, `on`, `idle_on`). Расстояния ламп задаются `HorizontalLayoutGroup`. В отличие от ламп перехода, отдельного принудительного сброса Time Scale в этом компоненте нет.
- `NodePopupView`: ссылки на тексты, `_positionRoot`, `_framePriceImage`, `_backGlowImage` и два конфига спрайтов. Отступ popup от ноды рассчитывается с константами `PopupGap = 30` и `NodeGapScale = 0.25`; отдельного поля для этих коэффициентов нет.

Конфиги в [ViewConfigs](/Users/admin/IncrementalRPG/Assets/SO/SkillTree/ViewConfigs): `Color Config` — цвета границ, `Circle Sprite Config` — спрайты окружности, `Frame Price Sprite Config` — рамка цены, `Back Glow Sprite Config` — фон свечения. У каждого состояния поля `locked`, `unaffordable`, `affordable`, `complete`.

`MenuCanvas/SkillTreeView/SkillTreePanel → SkillTreePanZoomController`:

| Поле | Назначение |
|---|---|
| `_minZoom`, `_maxZoom` | Минимальный и максимальный масштаб |
| `_zoomFactor` | Множитель шага приближения колесом |
| `_rubberBandDamping` | Сопротивление вытягиванию за границы |
| `_snapDuration` | Время возврата в допустимые границы, с |
| `_content`, `_popupView` | Перемещаемое содержимое и связанное окно ноды |

## 8. Карта, хаб, лава, меню

### Переход от карты к бою

`GameScene → MapMenuFade → MapMenuFadeTransition`:

`_fadeInDuration` — время накрытия экрана (сейчас 0,5 с), `_burnDuration` — время растворения/сгорания (1,5 с), `_fadeAmountStart`/`_fadeAmountEnd` — начальное/конечное значение эффекта материала (−0,1 → 1), `_useUnscaledTime` — использовать время независимо от `Time.timeScale`, `_spriteRenderer` — картинка перехода. Код управляет свойствами материала `_Alpha` и `_FadeAmount`; форму сгорания меняйте через остальную настройку назначенного шейдера.

### Лава и фоновые Spine-анимации

На `GameScene/LavaAnimation` используются `lava_pieces_front`, `lava_pieces_back`, `lava_pieces_full_screen`. Темп клипов — `Time Scale` их Spine-компонентов. Высоту, масштаб и подъём лавы меняет `TilemapCameraAutoFitter` по прогрессу таймера: это описано в игровом руководстве.

Хаб и карта содержат Spine-дым, огонь, факелы, светлячков, свет и частицы. Объекты и реальные ссылки на ресурсы перечислены в каталоге. Для самостоятельно зацикленных декораций меняйте их `Time Scale`, начальную анимацию, Loop, цвет и Transform. Эти настройки не меняют награды или скорость прохождения.

### Кнопки хаба

`GameScene → MenuCanvas/HubPanel/Plateau`, компоненты `HubFeatureButtonView`:

`_hoverSkeleton` — анимируемая часть; `_idleAnimationName`/`_hoverAnimationName` — имена обычного и наведённого состояния; `_idleAnimationLoop`/`_hoverAnimationLoop` — зацикливание; `_glowImage` — подсветка; `_button`, `_text` — элементы управления. Скорость — на связанном Spine-компоненте.

Звуки открытия разделов — `HubView` на `MenuCanvas/HubPanel`: `_mapOpenSound`, `_skillTreeOpenSound`, `_barracksOpenSound`, `_mineOpenSound`, `_craftOpenSound`. Остальные поля этого компонента связывают кнопки с окнами.

[HubLabel.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/Hub/HubLabel.prefab), `HubLabelView._padding` — запас размеров подложки вокруг текста; `_text` и `_backdrop` — ссылки. Шрифт, материал и цвет настраиваются в TMP/Image самого префаба.

### Боковые меню, нажатия и подсветка

| Компонент | Поля и эффект |
|---|---|
| `SideMenuFlyoutView` | `_animationDuration` — время движения/проявления пункта; `_itemDelay` — задержка каскада; `_closedOffset` — смещение скрытых пунктов. Остальные ссылки — кнопки и корень списка |
| `MainMenuButtonView` | `_action` — действие; `_hoverVisuals` — подсветки; `_pressTarget`, `_pressedScale` — что и насколько сжать при нажатии |
| `PauseButtonVisualState` | `_pressedScale`, `_glassOff`, `_glassOn`, `_backLight` — сжатие и состояния оформления |
| `UIButtonPressScaler` | `_target`, `_pressedScale` — объект и масштаб нажатия; компонент может добавляться программно |
| `UIButtonAudio` | `_playHoverSound`, `_playClickSound` — разрешить стандартные звуки наведения/нажатия |
| `SideMenuButtonVisualState` | `_glow` — объект подсветки; `_button` — отслеживаемая кнопка |
| `MouseWheelSliderInput` | `_step` — изменение значения слайдера колесом; `_slider` — цель |

`SideMenuFlyoutView` размещён в хабе, на карте и у дерева. Инвентарь использует заданный ему шаблон бокового меню. Для изменения в одном разделе не нужно менять все экземпляры.

### Пульсация фонового свечения

`MainMenuGlowAnimator` находится на `MainMenuScene/Canvas/Glow` и `GameScene/MenuCanvas/FullScreenMenuBackdrop/Glow`:

`_minAlpha`, `_maxAlpha` — диапазон прозрачности свечения; `_shadowMinAlpha`, `_shadowMaxAlpha` — диапазон тени; `_invertShadowPulse` — противоположная фаза тени; `_cycleDuration` — время цикла; `_scalePulse` — амплитуда масштаба; `_useUnscaledTime` — независимость от масштаба времени; `_randomizeStartPhase` — случайный старт. `_targetGraphic`, `_targetTransform`, `_shadowGraphic` связывают части.

### Инвентарь и переработка

В `GameScene → MenuCanvas/InventoryView/DesignFrame/LeftBlock/SkeletonGraphic (inventory_recycle)` находится анимация переработки. После продажи код запускает `recycle`, затем возвращается к `idle`; скорость можно менять на связанном SkeletonGraphic.

Цвета подсветки клеток, допустимого/недопустимого слота экипировки и зоны переработки пока заданы в `InventoryGridSlot`, `EquipmentDropArea`, `InventoryRecycleDropArea`. Исходный цвет берётся из Image/Graphic, а во время подсветки заменяется кодом. Отступ иконки при переносе `IconInset = 16` задан в `InventoryDragVisualAlignment`. Форма и размер инвентаря описаны в игровом руководстве.

### Размеры главного меню и подсказки

`MainMenuScene/FrontCanvas/Panel → MainMenuButtonLayoutScaler`: `_buttonAspectRatio` — ширина/высота кнопки; `_minButtonHeight`, `_maxButtonHeight` — пределы высоты; `_container`, `_layoutGroup` — область размещения.

`TooltipView._offset` — смещение подсказки относительно позиции указателя. `DungeonInfoPanelView._maxEnemyIcons` — максимум иконок существ на панели подземелья; `_enemyIconPrefab` и контейнер меняют оформление. `_playableContent`/`_unavailableContent` переключают части панели по пригодности подземелья к запуску.

Кнопки подземелий — `DungeonMapButtonView`: `_dungeon` выбирает контент, `_buttonGlow`, `_mapSectionGlow`, `_mapSectionFrameGlow` задают выделение выбранного участка. `DungeonMenuView` связывает список кнопок, инфопанель и кнопку закрытия.

## 9. Завершение захода, демо, интро

`GameScene → HUD/SessionEndPanel → SessionEndPopupView`:

| Поле | Назначение |
|---|---|
| `_recordScaleMultiplier` | Максимальный масштаб текста нового рекорда |
| `_recordScaleUpDuration`, `_recordScaleDownDuration` | Время увеличения и уменьшения, с |
| `_recordScalePauseDuration` | Пауза между циклами пульсации, с |
| `_raysRotationDegreesPerSecond` | Вращение лучей; знак задаёт направление; работает только при назначенном `_raysTransform` |
| `_goldText`, `_killsText`, `_goldRecordText`, `_killsRecordText` | Тексты результатов |
| `_hubButton` | Возврат в хаб |

В текущей сцене `_raysTransform` не назначен: изменение скорости вращения само по себе не создаёт лучи.

`HUD/DemoEndPopup → DemoEndPopupView`: `_captureDownscale` — уменьшение разрешения снимка фона; `_blurIntensity` — сила размытия; `_useLowResBlur` — режим размытия; `_blurMaterialTemplate` — материал; `_steamWishlistUrl` — адрес кнопки wishlist. `_blurBackground`, `_contentRoot` и три кнопки связывают оформление и действия.

`MainMenuScene → FrontCanvas/IntroPanel → IntroPanelPlayer`: `_videoPlayer`, `_videoDisplay`, `_audioSource`, `_audioClip` — видео и звук; `_aspectRatioFitter` — пропорции; `_hidePanelOnAwake`, `_hidePanelOnComplete` — начальная/финальная видимость; `_allowInputSkip`, `_skipButton` — пропуск; `_prepareTimeout` — предел ожидания подготовки видео. Сам ролик задаётся в `VideoPlayer`; отдельного множителя скорости ролика в `IntroPanelPlayer` нет.

`MainMenuController`: `_gameSceneName` — загружаемая сцена; `_deleteSaveOnNewGame` — сбрасывать сохранение при новой игре; `_playMusicOnStart` — включать музыку; `_view`, `_introPlayer`, `_audioManager` — связи. `MainMenuView` связывает панели настроек/авторов/подтверждения новой игры. `PauseMenuController` связывает панели паузы и настройки, кнопки выхода и `_mainMenuSceneName`.

## 10. Звук и пользовательские настройки

Откройте [BootstrapScene](/Users/admin/IncrementalRPG/Assets/Scenes/BootstrapScene.unity), объект `AudioManager`.

### AudioManager

| Поля | События/назначение |
|---|---|
| `_hitAudioClip`, `_waveAudioClip` | Общий удар и волна атаки |
| `_uiHoverAudioClip`, `_uiClickAudioClip` | Наведение/нажатие UI |
| `_skillUpgradeAudioClip`, `_skillMaxAudioClip`, `_skillErrorAudioClip` | Покупка уровня, полная прокачка, неудачная покупка |
| `_sessionEndAudioClip` | Окончание захода |
| `_curtainCloseAudioClip`, `_curtainOpenAudioClip` | Закрытие и открытие шторки |
| `_levelCounterOnAudioClip` | Включение лампы уровня |
| `_fightStartFadeAudioClip`, `_fightStartBurnAudioClip` | Фазы входа в бой |
| `_lavaLoopClip` | Петля лавы |
| `_mainMenuMusicClip`, `_hubMusicClip`, `_gameplayMusicClip` | Музыка трёх состояний |
| `_roundCompleteMusicClip` | Зацикленная музыка перехода между уровнями |
| `_musicSourceVolume` | Базовая громкость музыки до пользовательского микшера |
| `_musicFadeDuration` | Длительность смены громкости музыки, с |
| `_loopingSfxSourceVolume` | Базовая громкость зацикленного эффекта |
| `_lavaLoopMinVolume`, `_lavaLoopMaxVolume` | Диапазон громкости лавы по прогрессу |
| `_loopingSfxFadeDuration` | Время затухания/нарастания петли, с |
| `_maxHitAudioRequestsPerFrame`, `_maxDeathAudioRequestsPerFrame` | Лимит запросов на звуки попаданий и смерти за кадр |
| `_sfxSource`, `_hitsSource`, `_musicSource`, `_loopingSfxSource` | Источники разных групп звука |
| `_sfxMixerGroup`, `_hitsMixerGroup`, `_musicMixerGroup` | Маршрутизация в микшер |

`_registerAsGlobalInstance`, `_dontDestroyOnLoad`, `_destroyDuplicateInstances`, `_createMissing…Source` — служебные настройки жизненного цикла/создания источников. Они не меняют баланс звука и нужны при перестройке сцен. Индивидуальные попадания и варианты смерти существ назначаются в `EntityConfig.damageSound`/`deathSounds`.

Музыка `Assets/Audio/Music/theme_round_complete.ogg` сменяет боевую тему при начале закрытия шторки и зацикленно играет во время сундука, прокрутки, выбора награды и открытия шторки. Боевая музыка возвращается после полного открытия шторки. Громкость регулируется группой `Music`, смена темы использует `_musicFadeDuration`.

Новые эффекты шторки находятся в `Assets/Audio/Sounds/LevelTransition`: `round_end.ogg` — закрытие, `round_continue.ogg` — открытие на следующем уровне. Старый `Assets/Audio/Sounds/round_end.ogg` остаётся отдельным эффектом окончания всего захода в `_sessionEndAudioClip`.

`LootboxView._spinAudioSource` ссылается на дочерний `SpinAudio` с клипом `LevelTransition/spin_reward.ogg`. Источник запускается при начале движения ленты после открытия сундука, повторяется при необходимости и останавливается перед показом награды, при сбросе и отключении окна. Пауза и потеря фокуса приостанавливают ленту, сундук и звук вместе. Источник подключён к группе `Sfx`; его `Volume` позволяет отдельно настроить громкость прокрутки.

Микшер: [GameAudioMixer](/Users/admin/IncrementalRPG/Assets/Audio/GameAudioMixer.mixer). На том же объекте `AudioMixerSettingsApplier`: `_audioMixer` — микшер, `_masterVolumeParameter`, `_musicVolumeParameter`, `_sfxVolumeParameter` — имена опубликованных параметров; `_useAudioListenerAsMasterFallback` — технический режим при отсутствии маршрута общей громкости.

`SettingsMenuView` на панелях настроек обеих сцен связывает три слайдера громкости, список языков и кнопку назад. `SettingsMenuController`: `_hideOnAwake`, `_localeLabelMode`, `_localeLabelOverrides` (код языка → отображаемое имя). `MouseWheelSliderInput` задаёт шаг колеса, если добавлен к слайдеру.

Пользовательские уровни `MasterVolume`, `MusicVolume`, `SfxVolume` имеют диапазон 0…1 и сохраняются отдельно. Начальные значения 1 и пустой `LocaleCode` заданы в `GameSettingsData`, а не отдельным дизайнерским asset. Поэтому сохранённая пользовательская громкость может отличаться от базовой громкости источника.

## 11. Тексты, локализация, материалы и стандартные компоненты Unity

Локализованные строки: [Assets/Locales](/Users/admin/IncrementalRPG/Assets/Locales). Используется коллекция `LocalesTable`, языковые таблицы `en-US`, `ru-RU`, `zh` и Shared Data с общими ключами. Поля `LocalizedString` выбирают таблицу и запись; редактируйте перевод этой записи в нужной языковой таблице. У элементов сцены также могут быть компоненты локализации, которые заменяют исходный текст TMP при запуске.

`HudView._dungeonLevelFormat`, `_levelTransitionMessage` — форматы заголовков. Его старое `_levelTransitionFadeDuration` относится к отдельной внутренней корутине текстового перехода; текущая цепочка шторка/лампа/сундук её не вызывает. Оно не задаёт скорость движения шторки.

`ItemDefinition.localizedName/localizedDescription` — ссылки на записи локализации для инвентаря, окна награды и HUD; `displayName/description` — резервные строки, если ссылки не заданы. Статусы применения, цена продажи и кнопки награды используют `LocalesTable`. Число передаётся из `effectValue` аргументом `{0:P0}`; [список ключей и параметры зелий](/Users/admin/IncrementalRPG/Docs/GameplaySettings.ru.md#potion-test-settings).

<a id="potion-hud"></a>

### Список зелий и обратная связь

- `GameScene → HUD/HUD_Panel/PotionHud`: компонент `PotionHudView`, ссылки `_listRoot`, `_rowTemplate`, `_tooltip`. Показывает только активные эффекты, до трёх семейств. Пустой `ActivePotions` скрывается.
- `ActivePotions`: ширина 350, верхний левый anchor `(0.04, 0.67)` в панели HUD, `VerticalLayoutGroup.spacing = 8`. Расположен под левыми счётчиками, строки сдвигаются автоматически.
- `PotionRowTemplate`: высота 78; `PotionHudRowView._icon`, `_name`, `_value`. Шаблон неактивен, строки создаются из него. Название — 16–20 pt, величина — 15–18 pt с автоматическим уменьшением. Основной шрифт — Dynamo CG с существующими резервными шрифтами.
- `PotionTooltip`: панель 430×140, текст 16–20 pt, отступы 16/12. Компонент `TooltipView` находится на активном `PotionHud`, а ссылка `_root` ведёт на скрытую панель; так подсказка корректно открывается при первом наведении. Описание обновляется при смене языка.
- `MenuCanvas/InventoryView/DesignFrame/ConsumableFeedback`: сообщение о подготовке или отказе, панель 890×88. Ссылки `PlayerInventoryView._consumableFeedback` и `_consumableFeedbackRoot`; показывается на 5 секунд и скрывается при закрытии инвентаря. Длительность задана в коде `PlayerInventoryView`, в Inspector не вынесена. Панель не блокирует перетаскивание.
- `LootRewardPopupView`: название, описание награды, пояснение отказа и кнопки используют `LocalesTable`. Для валюты заголовок форматируется ключом `loot.currency_amount` (`{0}` — название, `{1}` — сумма). У валюты и свитков кнопка «Применить» скрыта; затемнение недоступной кнопки зелья задаётся её визуальным состоянием.

Китайские символы обеспечивает [SourceHanSansSC-Heavy SDF](</Users/admin/IncrementalRPG/Assets/Fonts/SourceHanSansSC/OTF/SimplifiedChinese/SourceHanSansSC-Heavy SDF.asset>) в цепочке fallback. Для динамического пополнения нужны `Atlas Population Mode = Dynamic`, ссылка на `SourceHanSansSC-Heavy.otf`, читаемые atlas textures и `Multi Atlas Textures`. Материал должен использовать атлас своего шрифта; без читаемости динамического атласа новые символы могут не отображаться.


`TmpGlowStyle` в [Assets/SO/TextGlow](/Users/admin/IncrementalRPG/Assets/SO/TextGlow): `_color`, `_offset`, `_inner`, `_outer`, `_power` — цвет, смещение, внутренняя/внешняя граница и сила свечения текста. `TmpGlowText` связывает текст, стиль, `_baseMaterialOverride`; `_reapplyWhenFontChanges` обновляет эффект при смене шрифта, `_warnIfShaderDoesNotSupportGlow` включает диагностическое сообщение. В сохранённых сценах/префабах прямые экземпляры `TmpGlowText` не найдены: наличие этих style-ассетов само по себе не доказывает, что конкретная надпись их использует. Проверьте компонент и материал выбранного текста.

Помимо собственных скриптов оформление фич настраивается стандартными компонентами:

| Компонент | Что менять и что учитывать |
|---|---|
| `RectTransform` | Позиция, размер, anchors, pivot; движущиеся скриптом части получают рассчитанную позицию во время игры |
| `CanvasScaler` | Базовое разрешение и адаптация интерфейса; влияет на видимую скорость в UI-единицах |
| `Horizontal/Vertical/GridLayoutGroup` | Отступы, spacing, размеры ячейки, число колонок; layout может переопределять ручное положение детей |
| `ContentSizeFitter`, `AspectRatioFitter` | Автоматический размер/пропорции; проверяйте вместе с layout |
| `Image`, `RawImage`, `SpriteRenderer` | Картинка, цвет, прозрачность, материал, режим заполнения/растяжения, порядок отрисовки |
| `TMP_Text` | Font Asset, Material Preset, размер/auto size, цвет, выравнивание, переносы. Локализация может заменять сам текст |
| `CanvasGroup` | Alpha, интерактивность, блокирование указателя; у переходов alpha управляется скриптом |
| `Button` | Target Graphic, стандартные переходы состояний, навигация; игровые обработчики могут назначаться кодом |
| `Mask`/`RectMask2D` | Видимая область ленты, окон и прокручиваемого содержимого |
| `ScrollRect` | Инерция, чувствительность, ограничения; у дерева дополнительно работает свой контроллер |
| `AudioSource` | Клип, pitch, volume, loop, mixer group; некоторые свойства устанавливает AudioManager |
| `Camera` | Фон/проекция/границы; позицию и размер игровой камеры переопределяет автофиттер |
| `SkeletonAnimation`/`SkeletonGraphic` | Экспорт, skin, animation, loop, Time Scale и материал с учётом правил этой документации |

Настраивайте материал, назначенный реальному Renderer/Image/TMP, а не похожую копию из демо-пакета. Общий материал меняет всех его пользователей. `ShineSweepAnimator` перезаписывает положение блика, `MapMenuFadeTransition` — прозрачность/прогресс сгорания, `RewardRevealEffect` — свою композицию, цвета и движение. Для управляемых свойств используйте поля соответствующего компонента.

## 12. Где без программиста пока не обойтись

| Задача | Причина |
|---|---|
| Отдельный устойчивый множитель скорости сундука/ламп перехода | Код сбрасывает их Time Scale до 1; можно менять длительность исходного клипа |
| Переименование `idle/move/damage` существ или имён атак/кости `cooldown` | Эти имена зафиксированы в управляющем коде |
| Длительность/дальность золотого popup, сглаживание HUD, полёт осколка к счётчику | Пока константы в `GoldPopupView`/`HudView` |
| Высота/скорость подъёма осколка при сборе и сила его блеска | Константы в `ShardPickupView` |
| Количество дымки/искр, внутренние траектории награды | Код `RewardRevealEffect`, отдельные поля не выведены |
| Фиксированная пауза награды через `holdDuration` | Поле пока не используется текущим координатором перехода |
| Новые состояния переработки инвентаря | Сейчас код запрашивает `recycle`, затем `idle` |

Здесь перечислены ограничения текущих настроек. Документация не добавляет новые контроллеры и не меняет существующее поведение.

<a id="equipment-inventory"></a>

## Экипировка и характеристики инвентаря

`GameScene → MenuCanvas/InventoryView/DesignFrame/RightBlock/StatisticsBackground` содержит `InventoryStatisticsView`. Общий заголовок расположен в соседнем `TitleBackground/StatisticsTitle`. В рамке три секции: «Атаки» (урон и частота ручной/автоматической атак), «Зона поражения» (радиус) и «Бочки» (урон и радиус взрыва). Иконки меча, зоны и бочки, разделители и выравнивание текста настраиваются на дочерних UI-компонентах. Подписи выровнены слева, значения справа; правый край поля значения сдвинут внутрь на 10 единиц Canvas, ширина поля — 61. Подсказка ПКМ расположена внизу панели.

Заголовки, подписи характеристик и их значения используют Dynamo CG с RU/CJK fallback. Нижняя подсказка использует [InventoryBody SDF](</Users/admin/IncrementalRPG/Assets/Fonts/Inventory/InventoryBody SDF.asset>) на основе Source Han Sans SC Regular. Размеры строк — 16 pt для подписи и 17 pt для значения (автоуменьшение до 13 pt), секции — 19 pt, нижняя подсказка — 14 pt. Значения рассчитываются по текущей тренировке, экипировке и активным зельям; вручную в UI не задаются. Частота показана ударами в секунду, радиус — в мировых единицах. До открытия автоатаки её урон и частота скрыты; до открытия бочек скрыта вся их секция. После улучшения тренировки нужные строки появляются автоматически. При скрытых строках автоатаки ручная частота, секции зоны и бочек поднимаются; шаг берётся из исходного расстояния между строками в сцене.

У `PlayerInventoryView` рабочие слоты Helmet, Chest, Weapon. `BootsPlaceholder` — декоративный силуэт под нагрудником: `preserveAspect` включён, `raycastTarget` выключен, компонента `EquipmentDropArea` нет. У него и силуэтов `RingCell/Image`, `NecklaceCell/Image` альфа `128/255`, как у пустых слотов шлема, нагрудника и оружия в обычном состоянии. Силуэты уменьшены до 92%: декоративные — размером `RectTransform` в сцене, пустые рабочие слоты — в `EquipmentDropArea.SetEquippedIcon`; надетые предметы используют полный исходный размер. В сетке шлем занимает 1×1, нагрудник 2×2, оружие 1×2. Надетый предмет остаётся в сетке; иконка слота берётся из его `ItemDefinition.icon`.

В [InventoryPlacementOverlayItem.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/Inventory/InventoryPlacementOverlayItem.prefab) `InventoryRarityBackdrop` рисует единый фон и рамку на весь занятый предметом прямоугольник, позади иконки. Компонент требует `CanvasRenderer`. В Inspector: `_tintStrength = 0.16` — примесь цвета редкости к непрозрачному тёмному фону, `_borderWidth = 1.5` — ширина рамки в единицах Canvas; отступ `RectTransform` от внешних границ клеток — 4. Цвет определяется редкостью экземпляра, которая может отличаться от определения. Пустые клетки сохраняют исходный фон. Счётчик стека в `Counter/Text (Legacy)` использует исходный `dynamocg.ttf`, светлый цвет и тонкую тёмную обводку; подложка `Counter/Image` отключена. Размер цифр автоматически подбирается в диапазоне 14–30, логика показа количества по-прежнему находится в `PlacementOverlayItem`.

Палитра задана в коде [EquipmentStats.RarityColor](/Users/admin/IncrementalRPG/Assets/Scripts/Core/Items/EquipmentStats.cs:43): Common — серый, Rare — зелёный, Unique — фиолетовый, Legendary — золотой. Тёмная основа заливки и голубой цвет рамки при наведении, переносе или выборе заданы в [InventoryRarityBackdrop.cs](/Users/admin/IncrementalRPG/Assets/Scripts/UI/Inventory/InventoryRarityBackdrop.cs); отдельных полей Inspector для них нет.

Подсказка: [InventoryItemTooltip.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/Inventory/InventoryItemTooltip.prefab) и [InventoryTooltipCanvas.prefab](/Users/admin/IncrementalRPG/Assets/Prefabs/Inventory/InventoryTooltipCanvas.prefab). Компонент `InventoryItemTooltipView` показывает имя, иконку, редкость, проценты и цену экземпляра. У спрайта рамки `satistic_icon_0` border 70 px со всех сторон; Image подсказки — Sliced, `pixelsPerUnitMultiplier = 3`, чтобы углы не растягивались. Поля компонента: `_preferredWidth = 440`, `_padding = 24`, `_iconSize = 64`, `_contentGap = 20` в единицах Canvas — ширина, внутренние поля, сторона иконки и промежуток между шапкой и описанием. Имя и описание предмета используют Dynamo CG с той же цепочкой резервных шрифтов, что и остальной интерфейс. Размеры текста: `_nameFontSize = 24 pt`, `_bodyFontSize = 18 pt`. Пропорции иконки сохраняются, высота карточки вычисляется по переносам текста. Минимум описания при нехватке высоты — 14 pt, запас до края экрана — 12 px; оба ограничения заданы в [InventoryItemTooltipView.cs](/Users/admin/IncrementalRPG/Assets/Scripts/UI/Inventory/InventoryItemTooltipView.cs), в Inspector не вынесены.

Имя брони и локализованная метка редкости окрашены общей палитрой по редкости экземпляра; бонусы и цена используют цвет TMP-текста. Величина эффекта зелья берётся из его параметров. Canvas подсказки масштабируется относительно 1920×1080 (`Match = 0.5`), имеет `sortingOrder = 2900`; канвас переноса — 3000.

Ключи `LocalesTable` для en-US, ru-RU и zh: `equipment.stat.<statId>`, `equipment.statistics`, `equipment.section.attacks`, `.zone`, `.barrels`, `equipment.panel_hint`, `equipment.inventory_hint`, `equipment.locked`, `equipment.per_second`, `equipment.slot.<slot>` и `item.rarity.<rarity>`. Подробные формулы и настройка `defaultStats` — [игровое руководство](/Users/admin/IncrementalRPG/Docs/GameplaySettings.ru.md#equipment-stats).

## Окно кузницы

В [GameScene](/Users/admin/IncrementalRPG/Assets/Scenes/GameScene.unity) используйте `MenuCanvas/CraftView`, компонент `CraftView`. `Composition` рассчитана на 1920×1080 и пропорционально вписывается в окно. Угловая кнопка открывает общее боковое меню с возвратом в хаб.

Фоновый дым находится между подложкой и интерфейсом: у вложенного Canvas объекта `FullScreenMenuBackdrop` включён `Override Sorting`, `Order in Layer = 99`; у дыма и искр — 100; у `MenuCanvas` — 101. Сохраняйте этот порядок, чтобы частицы не перекрывали предметы, кнопки и тексты.

Основные изображения: `Assets/Art/UI/NEW/ассеты ковка`. Префаб строки свитка — [ForgeScrollRow](/Users/admin/IncrementalRPG/Assets/Prefabs/Forge/ForgeScrollRow.prefab): `_normalFrame`, `_selectedFrame`, `_icon`, `_count`. Вертикальный список прокручивается, показывает количество и выделяет выбранный свиток красной рамкой. Повторное нажатие сохраняет выбор; снять его до ковки можно нажатием на центральный слот. Во время попытки выбранный значок остаётся над наковальней, а строка сохраняет выделение даже после расходования последней копии: её счётчик показывает оставшееся количество, то есть `×0`. После завершения попытки израсходованная строка исчезает.

`Composition/Pentagram` использует `Craft_Pentagram/pentagram_red_SkeletonData`, а `Composition/Anvil` — `Craft_Anvil/craft_anvil_SkeletonData` из `Assets/Art/Animations/NEW_06_2026`. Пентаграмма циклически проигрывает `idle`. В открытой кузнице вне QTE наковальня циклически проигрывает `idle` с ударами молота. Нажатие на ковку регистрирует одну попытку и блокирует повторный запуск; текущий цикл `idle` вместе со звуком доигрывает до конца. Только после этого проигрывается замах `qte_1start`. Все переходы выполняются без смешивания (`MixDuration = 0`), замах и удар результата завершаются по фактическому окончанию Spine-анимации. Ожидание конца `idle` может занять до 2.77 с. После замаха появляется шкала QTE, а молот остаётся неподвижным в поднятой позе `qte_2wait`. Остановка ползунка запускает один удар: `qte_3red` (без бонуса), `qte_3orange` (один стат), `qte_3yellow` (все статы), `qte_3green` (повышение редкости). Окно результата появляется после окончания удара; наковальня возвращается к `idle`, в том числе за окном результата. Масштабы объектов учитывают встроенный перевод Spine в пиксели UI.

Звуки ковки назначены в группе `Anvil audio` компонента `CraftView`, исходные файлы — `Assets/Audio/Sounds/Craft`. `AnvilSound` проигрывает `craft_qte_start` один раз при запуске и выбранный результат при остановке: `craft_qte_red`, общий `craft_qte_orange&yellow` либо `craft_qte_green`. `AnvilLoop` запускает `craft_idle_loop` вместе с анимацией `idle` при открытии кузницы и возвращении к обычному режиму. После нажатия на ковку у звука и анимации отключается дальнейшее повторение, но текущий цикл доигрывает полностью. Замах начинается после завершения обоих. Во время QTE зацикленных ударов и их звука нет. Хвост одноразового стартового звука может завершаться уже в позе ожидания. Скорость звуковой петли автоматически подгоняется под длительность анимации с учётом небольшого расхождения длины аудиофайла. Оба источника подключены к `Sfx`; громкость настраивается в их `Volume`. Пауза и потеря фокуса останавливают продвижение QTE, анимацию и звуки, закрытие кузницы полностью останавливает источники. После получения или разборки предмета молот продолжает уже запущенный за результатом цикл ударов со звуком, без перезапуска.

Анимация `idle` и её звук начинаются с нулевой отметки. Прежнее смещение начала цикла на 0.2333 с больше не используется: переход после замаха ведёт в `qte_2wait`, чья поза совпадает с последним кадром `qte_1start`. Исходные клипы не обрезаются.

`Composition/QTE`: `Gauge` содержит исходный спрайт шкалы и `ForgeGaugeImage`, `GaugeFrame` — декоративную рамку, `Cursor` — указатель. Ширины цветных зон берутся из текущего конфига ковки и свитка точности. Изменяйте размер шкалы вместе с рамкой; положение указателя вычисляется по ширине `Gauge`. ЛКМ останавливает ползунок после вводной анимации. Пока идёт QTE, выбор свитка и меню недоступны.

`ForgeResult` оформлен на основе существующего окна награды. Компонент `ForgeResultView`: `_title`, `_rarity`, `_stats`, `_status`, `_icon`, кнопки `_take`, `_break` и `_effect`. Цвет редкости задаёт цвет подписи и свечения. Размеры и размещение редактируются в `ForgeResult/Composition`. Тексты окна и имена созданных предметов находятся в `LocalesTable`, ключи `forge.*`, языки ru-RU, en-US и zh.

Верхняя панель `TitlePlate` использует `ассеты ковка/new_title.png` шириной 760 с сохранением пропорций. Между названием и редкостью расположен `TitleRarityDivider` со спрайтом `new_title_2.png`; прежний `TitleGlow` отключён. Заголовок автоматически подбирает размер от 28 до 52 и размещается в одну строку с внутренними отступами. Материал букв — [Forge Result Title](</Users/admin/IncrementalRPG/Assets/Fonts/Dynamo CG/Materials/Forge Result Title.mat>). Рассеянная подсветка позади текста — отдельный `Image` с именем `TitleSoftGlow`, спрайтом `RewardMist` и альфой 0.12. `HubLabelView` на `TitlePlate` подгоняет подложку под видимые буквы с запасом 80 по горизонтали и 36 по вертикали с каждой стороны, в том числе при смене языка. Для яркости меняйте альфу `TitleSoftGlow`, для ширины рассеивания — запас `_padding`. Цвет букв задаётся локальным `Vertex Color Gradient` в TMP: у названия верх `#FFFDEC`, низ `#FDE6C6`; у подписей «Взять» и «Разбить» верх `#DBD6CD`, низ `#A4968B`. Базовый `Vertex Color` оставляйте белым, чтобы он не окрашивал градиент дополнительно. Эти настройки принадлежат только текстам окна результата ковки. Блок `Stats` использует тот же `dynamocg SDF` и основной материал, что и кнопки; автоподбор размера 21–30 сохраняет читаемость списка из пяти характеристик. Редкость сохраняет собственный цвет.
