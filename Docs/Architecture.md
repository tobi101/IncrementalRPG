# Architecture: Model-View Binding

## Overview

Проект разделяет игровую логику и визуальное представление на два слоя:

- **Model** — чистые C# классы (данные + логика). Не знают про Unity.
- **View** — тонкие MonoBehaviour-обёртки. Отображают состояние модели на сцене, принимают Unity-события (коллизии, клики).

```
┌──────────────────────────────────┐
│          Pure C# (Model)         │
│                                  │
│  Entity (HP, ID, имя, ...)       │
│  Creature : Entity               │
│  Projectile : Entity             │
│  DamageZone, ChainLightning ...  │
└──────────────┬───────────────────┘
               │  1:1 Bind/Unbind
┌──────────────▼───────────────────┐
│      MonoBehaviour (View)        │
│                                  │
│  EntityView : MonoBehaviour      │
│    - ссылка на Entity model      │
│    - Transform, Animator, ...    │
│                                  │
│  CreatureView : EntityView       │
│  ProjectileView : EntityView     │
│  DamageZoneView : EntityView     │
└──────────────────────────────────┘
```

## Направление данных: гибридный подход

Используются оба механизма в зависимости от характера данных.

### События (push) — для дискретных действий

Модель публикует событие, View подписывается и реагирует.

Примеры:
- Существо получило урон → `OnDamageTaken` → анимация удара
- Существо умерло → `OnDeath` → анимация смерти → возврат в пул
- Изменилось HP → `OnHealthChanged` → обновление HP-бара

### Чтение каждый кадр (poll) — для непрерывного состояния

View читает данные из модели в `Update()`.

Примеры:
- Позиция DamageZone, управляемой мышью
- Интерполяция движения

Важно: **инпут** обрабатывается на стороне сервиса или View, но не в модели. Модель не знает про `UnityEngine.Input`.

## Object Pool

Отдельный пул на каждый тип View. Пул работает с MonoBehaviour-компонентами (GameObject'ами на сцене).

```
ObjectPool<CreatureView>        — пул существ
ObjectPool<ProjectileView>      — пул снарядов
ObjectPool<DamageZoneView>      — пул зон урона
ObjectPool<ChainLightningView>  — пул молний
```

Каждый пул регистрируется в DI как синглтон. Сервис инжектит нужный пул через конструктор.

## Сервисы-владельцы

Каждый тип игровых сущностей управляется своим сервисом. Сервис:
- Хранит коллекцию живых моделей
- Создаёт модели и берёт View из пула
- Связывает модель и View через `Bind()`
- Обновляет модели в `Update()`

Сервис владеет моделями и пулом. View не знает про пул — при необходимости освобождения View публикует событие `OnRelease`, а сервис реагирует и возвращает View в пул.

```
CreatureService
  - List<Creature> _creatures
  - ObjectPool<CreatureView> _pool
  - CreatureCatalog _catalog
  - Spawn("goblin"):
      config = catalog.GetById("goblin")
      model  = new Creature(config)
      view   = pool.Get(config.Prefab)
      view.OnRelease += v => { pool.Return(v); _creatures.Remove(model); }
      view.Bind(model)

ProjectileService
  - аналогично для снарядов

DamageZoneService
  - аналогично для зон урона
```

> **TODO:** Стратегия пулинга (один пул с суб-пулами по префабу vs словарь отдельных пулов) — ожидает уточнения от ГД по степени различий между существами.

## Bind / Unbind

View не знает про пул и инфраструктуру. При необходимости освобождения View вызывает событие `OnRelease` — сервис-владелец реагирует и возвращает View в пул.

```csharp
public class CreatureView : MonoBehaviour
{
    private Creature _model;
    public event Action<CreatureView> OnRelease;

    public void Bind(Creature model)
    {
        _model = model;
        _model.OnDamageTaken += HandleDamage;
        _model.OnDeath += HandleDeath;
        gameObject.SetActive(true);
    }

    public void Unbind()
    {
        _model.OnDamageTaken -= HandleDamage;
        _model.OnDeath -= HandleDeath;
        _model = null;
        gameObject.SetActive(false);
    }

    private void HandleDamage(int amount) { /* анимация урона */ }

    private void HandleDeath()
    {
        Unbind();
        OnRelease?.Invoke(this);
    }
}
```

## Конфигурация и каталоги

Данные о сущностях хранятся в ScriptableObject-конфигах. Каталог — точка доступа ко всем конфигам одного типа.

```
CreatureConfig (ScriptableObject)
  - string Id              ("goblin", "skeleton")
  - string Name            ("Goblin Warrior")
  - int MaxHP              (100)
  - float Speed            (3.5)
  - CreatureView Prefab    (ссылка на префаб)

CreatureCatalog (ScriptableObject)
  - List<CreatureConfig> Creatures
  - GetById(string id) → CreatureConfig
```

Каталог инжектится в сервис через DI. Один каталог на тип сущности:
- `CreatureCatalog` — существа
- `ProjectileCatalog` — снаряды
- и т.д.

## Жизненный цикл сущности

```
1. Запрос на спаун:            service.Spawn("goblin")
2. Чтение конфига:             catalog.GetById("goblin") → CreatureConfig
3. Создание модели:            new Creature(config.Id, config.MaxHP, ...)
4. View из пула:               pool.Get(config.Prefab)
5. Связывание:                 view.Bind(model)
6. Игровой цикл:               сервис обновляет модель → View реагирует на события
7. Смерть:                     модель публикует OnDeath → View.HandleDeath()
8. View отвязывается:          view.Unbind() → view публикует OnRelease
9. Сервис реагирует:           pool.Return(view), _creatures.Remove(model)
```

## Стек технологий

| Слой | Технология |
|------|-----------|
| DI | Reflex |
| Game Loop | GameLoop + UnityLoopDriver |
| Lifecycle | IStartable, ITickable, IFixedTickable, ILateTickable |
| Сервисы | IService (Initialize / Update) |
| Модели | Чистые C# классы |
| View | MonoBehaviour |
| Пулинг | ObjectPool\<T> per type |
