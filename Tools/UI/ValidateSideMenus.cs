using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Core.Classes;
using Core.Gameplay.Dungeon;
using Core.Save;
using Core.StateMachine;
using Core.StateMachine.States;
using Core.TestSkillTree.View;
using UI;
using UI.Classes;
using UI.Forge;
using UI.Inventory;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class ValidateSideMenus
{
    const string PrefabPath = "Assets/Prefabs/UI/SideMenu.prefab";
    const string SaveBackupKey = "SideMenuRegression.PreviousSaveOverride";
    const string SaveActiveKey = "SideMenuRegression.Active";
    const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
    static int checks;
    static void Check(bool ok, string name)
    {
        checks++;
        if (!ok) throw new Exception("FAILED: " + name);
    }
    static T Field<T>(object target, string name) => (T)target.GetType().GetField(name, Flags).GetValue(target);
    static object Call(object target, string name) => target.GetType().GetMethod(name, Flags).Invoke(target, null);
    static T Find<T>() where T : Object => Object.FindFirstObjectByType<T>(FindObjectsInactive.Include);
    static SideMenuFlyoutView[] Menus() => Object.FindObjectsByType<SideMenuFlyoutView>(FindObjectsInactive.Include, FindObjectsSortMode.None)
        .Where(m => m.gameObject.scene == SceneManager.GetActiveScene()).ToArray();

    public static string Scene()
    {
        checks = 0;
        Check(!EditorApplication.isPlaying, "Edit Mode");
        Check(SceneManager.GetActiveScene().path == "Assets/Scenes/GameScene.unity", "GameScene open");
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        Check(prefab != null, "shared prefab exists");
        var menus = Menus();
        Check(menus.Length == 6, "exactly six scene menus");
        foreach (var menu in menus)
        {
            Check(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(menu.gameObject) == PrefabPath, "shared prefab: " + menu.transform.parent.name);
            Check(menu.ToggleButton.transform.parent == menu.transform, "toggle owned by menu");
            foreach (var field in new[] { "_listRoot", "_settingsButton", "_mainMenuButton", "_exitButton", "_returnToHubButton" })
            {
                var value = new SerializedObject(menu).FindProperty(field).objectReferenceValue;
                var transform = value is GameObject go ? go.transform : (value as Component)?.transform;
                Check(transform != null && transform.IsChildOf(menu.transform), "local reference: " + field);
            }
        }
        var registered = Field<SideMenuFlyoutView[]>(Find<PauseMenuController>(), "_sideMenus");
        Check(registered.Length == 6 && registered.Distinct().Count() == 6 && menus.All(registered.Contains), "all menus registered once");
        Check(Find<ClassesMenuView>().sideMenu.transform.parent == Find<ClassesMenuView>().transform, "class window directly owns its menu");
        Check(Field<SideMenuFlyoutView>(Find<PlayerInventoryView>(), "_sideMenu").transform.IsChildOf(Find<PlayerInventoryView>().transform), "inventory owns its menu");
        Check(Field<SideMenuFlyoutView>(Find<CraftView>(), "_sideMenu").transform.IsChildOf(Find<CraftView>().transform), "forge owns its menu");
        Check(Field<Button>(Find<SkillTreeView>(), "_closeButton") == menus.Single(m => m.transform.parent.name == "SkillTreePanel").ReturnToHubButton, "skill-tree return rebound");
        Check(Field<Button>(Find<DungeonMenuView>(), "_closeButton") == menus.Single(m => m.transform.parent.name == "MapView").ReturnToHubButton, "map return rebound");
        Check(!new SerializedObject(menus.Single(m => m.transform.parent.name == "HubPanel")).FindProperty("_showReturnToHubButton").boolValue, "hub omits return item");
        foreach (var go in SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Transform>(true)))
            Check(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go.gameObject) == 0, "no missing scripts: " + go.name);
        Check(!AssetDatabase.GetDependencies(PrefabPath, true).Any(p => p.EndsWith(".unity")), "prefab has no scene dependency");
        Check(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Classes/ClassesView.prefab") == null &&
              AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Classes/ClassesSideMenu.prefab") == null, "obsolete class prefabs removed");
        Check(!SceneManager.GetActiveScene().isDirty, "scene saved");
        return "PASS: " + checks + " scene and prefab assertions.";
    }

    public static string PreparePlayMode()
    {
        if (EditorApplication.isPlaying || SessionState.GetBool(SaveActiveKey, false))
            throw new InvalidOperationException("Stop Play Mode and finish the previous regression session first.");
        SessionState.SetString(SaveBackupKey, SaveStorage.EditorSaveDirectoryOverride);
        SessionState.SetBool(SaveActiveKey, true);
        SaveStorage.EditorSaveDirectoryOverride = "/private/tmp/IncrementalRPG-side-menu-" + Guid.NewGuid().ToString("N");
        return "Isolated regression save configured; enter Play Mode.";
    }

    public static string FinishPlayMode()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play Mode before restoring the save directory.");
        if (SessionState.GetBool(SaveActiveKey, false))
        {
            SaveStorage.EditorSaveDirectoryOverride = SessionState.GetString(SaveBackupKey, "");
            SessionState.EraseString(SaveBackupKey);
            SessionState.EraseBool(SaveActiveKey);
        }
        return "Previous save directory restored.";
    }

    public static async Task<string> PlayMode()
    {
        checks = 0;
        Check(EditorApplication.isPlaying && new SaveStorage().SavePath.StartsWith("/private/tmp/IncrementalRPG-side-menu-"), "isolated Play Mode");
        var errors = new List<string>();
        var runInBackground = Application.runInBackground;
        Application.runInBackground = true;
        Application.LogCallback capture = (message, stack, type) => { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) errors.Add(message); };
        Application.logMessageReceived += capture;
        var keyboard = InputSystem.AddDevice<Keyboard>("SideMenuRegressionKeyboard");
        keyboard.MakeCurrent();
        try
        {
            var classes = Find<ClassesMenuView>();
            var machine = Field<GameStateMachine>(classes, "_machine");
            var pause = Find<PauseMenuController>();
            var settings = Field<SettingsMenuController>(pause, "_settingsMenu");
            var menus = Menus();
            Check(menus.Length == 6, "six menus before visiting any window");
            var cases = new (string parent, Action show)[]
            {
                ("HubPanel", () => machine.Enter<HubState>()),
                ("SkillTreePanel", () => machine.Enter<SkillTreeMenuState>()),
                ("InventoryView", () => machine.Enter<InventoryMenuState>()),
                ("CraftView", () => machine.Enter<CraftState>()),
                ("ClassesView", () => machine.Enter<ClassesMenuState>()),
                ("MapView", () => { machine.Enter<HubState>(); Call(Find<HubView>(), "OpenDungeon"); })
            };
            foreach (var entry in cases)
            {
                entry.show();
                await Task.Delay(800);
                var menu = menus.Single(m => m.transform.parent.name == entry.parent);
                Check(menu.gameObject.activeInHierarchy && !menu.IsOpen, entry.parent + " starts closed");
                CheckRaycast(menu.ToggleButton);
                menu.ToggleButton.onClick.Invoke();
                await Task.Delay(400);
                Check(menu.IsOpen && Field<CanvasGroup>(menu, "_canvasGroup").interactable, entry.parent + " opens and accepts clicks");
                var items = Field<RectTransform[]>(menu, "_itemTransforms");
                Check(items.Length == (entry.parent == "HubPanel" ? 3 : 4), entry.parent + " animates visible items only");
                for (var i = 0; i < items.Length; i++)
                    Check(Vector2.Distance(items[i].anchoredPosition, new Vector2(0, -49 * i)) < .01f, entry.parent + " item alignment");
                CheckRaycast(Field<Button>(menu, "_settingsButton"));
                await Escape(keyboard);
                Check(!menu.IsOpen, entry.parent + " Escape closes flyout");
                if (entry.parent == "ClassesView") Check(machine.IsCurrent<ClassesMenuState>(), "Escape does not also leave classes");
                menu.ToggleButton.onClick.Invoke();
                await Task.Delay(400);
                Field<Button>(menu, "_settingsButton").onClick.Invoke();
                await Task.Delay(100);
                Check(pause.IsOpen && !menu.IsOpen, entry.parent + " opens settings");
                settings.BackButton.onClick.Invoke();
                await Task.Delay(400);
                Check(!pause.IsOpen && menu.IsOpen, entry.parent + " settings returns to source menu");
                if (entry.parent != "HubPanel")
                {
                    CheckRaycast(menu.ReturnToHubButton);
                    menu.ReturnToHubButton.onClick.Invoke();
                    await Task.Delay(120);
                    Check(machine.IsCurrent<HubState>() && !menu.IsOpen && !menu.gameObject.activeInHierarchy, entry.parent + " returns to hub and hides menu");
                }
                else menu.CloseImmediate();
                entry.show();
                await Task.Delay(800);
                Check(!menu.IsOpen && Menus().Length == 6, entry.parent + " reopening creates no duplicate");
                if (entry.parent == "MapView") Find<DungeonMenuView>().Hide();
            }

            machine.Enter<ClassesMenuState>();
            await Task.Delay(150);
            classes.sideMenu.Open();
            await Task.Delay(400);
            classes.confirmation.SetActive(true);
            await Escape(keyboard);
            Check(!classes.confirmation.activeSelf && classes.sideMenu.IsOpen, "class confirmation consumes Escape before flyout");
            await Escape(keyboard);
            Check(!classes.sideMenu.IsOpen && machine.IsCurrent<ClassesMenuState>(), "next Escape closes flyout only");

            var service = Field<ClassProgressionService>(classes, "_service");
            var catalog = Field<ClassCatalog>(classes, "_catalog");
            Field<DungeonSelectionService>(service, "_dungeons").MarkCompleted(catalog.introductionDungeon);
            machine.Enter<ClassesMenuState>();
            await Task.Delay(150);
            Check(service.FirstChoicePending && !classes.sideMenu.ReturnToHubButton.interactable, "first choice disables return");
            classes.sideMenu.ReturnToHubButton.onClick.Invoke();
            await Escape(keyboard);
            Check(machine.IsCurrent<ClassesMenuState>(), "first choice cannot be bypassed by button or Escape");
            service.ChooseFirst(catalog.classes[0]);
            await Task.Delay(150);
            Check(classes.sideMenu.ReturnToHubButton.interactable, "choosing class restores return");
            classes.sideMenu.ReturnToHubButton.onClick.Invoke();
            Check(machine.IsCurrent<HubState>(), "return works after first choice");
            Check(errors.Count == 0, "no runtime errors: " + string.Join("; ", errors));
            return "PASS: " + checks + " Play Mode assertions (all windows, raycasts, animation, Escape, settings, return, reopening, first choice).";
        }
        finally
        {
            InputSystem.RemoveDevice(keyboard);
            Application.runInBackground = runInBackground;
            Application.logMessageReceived -= capture;
        }
    }

    static async Task Escape(Keyboard keyboard)
    {
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Escape));
        await Task.Delay(100);
        InputSystem.QueueStateEvent(keyboard, new KeyboardState());
        await Task.Delay(100);
    }

    static void CheckRaycast(Button button)
    {
        var rect = (RectTransform)button.transform;
        var results = new List<RaycastResult>();
        var canvas = button.GetComponentInParent<Canvas>();
        var camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        var screen = RectTransformUtility.WorldToScreenPoint(camera, rect.TransformPoint(rect.rect.center));
        EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = screen }, results);
        Check(results.Count > 0 && (results[0].gameObject.transform == rect || results[0].gameObject.transform.IsChildOf(rect)), "button receives raycast: " + button.name);
    }
}
