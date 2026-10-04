using System;
using System.Collections.Generic;
using System.Linq;
using UI;
using UI.Classes;
using UI.Forge;
using UI.Inventory;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public static class UnifySideMenus
{
    public const string PrefabPath = "Assets/Prefabs/UI/SideMenu.prefab";

    public static string Run()
    {
        var scene = SceneManager.GetActiveScene();
        if (EditorApplication.isPlaying || scene.path != "Assets/Scenes/GameScene.unity" || scene.isDirty)
            throw new InvalidOperationException("Open the saved GameScene in Edit Mode before migrating.");
        if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null)
            throw new InvalidOperationException("SideMenu prefab already exists; run validation instead of migrating again.");

        var oldMenus = Object.FindObjectsByType<SideMenuFlyoutView>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Where(m => m.gameObject.scene == scene).ToArray();
        if (oldMenus.Length != 4)
            throw new InvalidOperationException("Expected four legacy scene menus.");
        var source = oldMenus.Single(m => m.name == "SkillTreeSideMenuFlyout");
        var inventory = Object.FindFirstObjectByType<PlayerInventoryView>(FindObjectsInactive.Include);
        var craft = Object.FindFirstObjectByType<CraftView>(FindObjectsInactive.Include);
        var inventoryToggle = inventory.transform.Find("SideMenuToggle").gameObject;
        var craftToggle = craft.transform.Find("SideMenuToggle").gameObject;

        if (!AssetDatabase.IsValidFolder("Assets/Prefabs/UI"))
            AssetDatabase.CreateFolder("Assets/Prefabs", "UI");
        var template = Object.Instantiate(source.gameObject);
        template.name = "SideMenu";
        template.transform.SetParent(null, false);
        var toggle = Object.Instantiate(source.ToggleButton.gameObject, template.transform);
        toggle.name = "SideMenuToggle";
        toggle.transform.SetAsFirstSibling();
        var templateFields = new SerializedObject(template.GetComponent<SideMenuFlyoutView>());
        templateFields.FindProperty("_toggleButton").objectReferenceValue = toggle.GetComponent<UnityEngine.UI.Button>();
        templateFields.FindProperty("_showReturnToHubButton").boolValue = true;
        templateFields.ApplyModifiedPropertiesWithoutUndo();
        var prefab = PrefabUtility.SaveAsPrefabAsset(template, PrefabPath);
        Object.DestroyImmediate(template);
        if (prefab == null) throw new InvalidOperationException("Failed to save the shared prefab.");

        Undo.SetCurrentGroupName("Unify hub side menus");
        var replacements = new Dictionary<Object, Object>();
        var instances = new List<SideMenuFlyoutView>();
        var obsolete = new List<GameObject>();
        foreach (var old in oldMenus)
        {
            var instance = Instantiate(prefab, old.transform.parent);
            MapSubtree(old.gameObject, instance.gameObject, replacements);
            MapSubtree(old.ToggleButton.gameObject, instance.ToggleButton.gameObject, replacements);
            obsolete.Add(old.gameObject);
            obsolete.Add(old.ToggleButton.gameObject);
            if (old.name == "HubSideMenuFlyout")
            {
                var fields = new SerializedObject(instance);
                fields.FindProperty("_showReturnToHubButton").boolValue = false;
                fields.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.RecordPrefabInstancePropertyModifications(instance);
            }
            instances.Add(instance);
        }
        var inventoryMenu = Instantiate(prefab, inventory.transform);
        MapSubtree(inventoryToggle, inventoryMenu.ToggleButton.gameObject, replacements);
        obsolete.Add(inventoryToggle);
        instances.Add(inventoryMenu);
        var craftMenu = Instantiate(prefab, craft.transform);
        MapSubtree(craftToggle, craftMenu.ToggleButton.gameObject, replacements);
        obsolete.Add(craftToggle);
        instances.Add(craftMenu);

        // Repair every reference, including SkillTreeView/DungeonMenuView close buttons
        // and the class window's menu, before removing any legacy object.
        var repaired = 0;
        foreach (var component in scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Component>(true)))
        {
            if (component == null || replacements.ContainsKey(component)) continue;
            var fields = new SerializedObject(component);
            var property = fields.GetIterator();
            var changed = false;
            while (property.Next(true))
            {
                if (property.propertyType != SerializedPropertyType.ObjectReference ||
                    property.objectReferenceValue == null ||
                    !replacements.TryGetValue(property.objectReferenceValue, out var replacement)) continue;
                if (!changed) Undo.RecordObject(component, "Rebind side menu");
                property.objectReferenceValue = replacement;
                repaired++;
                changed = true;
            }
            if (changed) fields.ApplyModifiedProperties();
        }
        Assign(inventory, "_sideMenu", inventoryMenu);
        Assign(craft, "_sideMenu", craftMenu);
        var pause = Object.FindFirstObjectByType<PauseMenuController>(FindObjectsInactive.Include);
        var pauseFields = new SerializedObject(pause);
        var menus = pauseFields.FindProperty("_sideMenus");
        menus.arraySize = instances.Count;
        for (var i = 0; i < instances.Count; i++)
            menus.GetArrayElementAtIndex(i).objectReferenceValue = instances[i];
        pauseFields.ApplyModifiedProperties();
        foreach (var old in obsolete) Undo.DestroyObjectImmediate(old);

        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Could not save GameScene.");
        AssetDatabase.DeleteAsset("Assets/Prefabs/Classes/ClassesView.prefab");
        AssetDatabase.DeleteAsset("Assets/Prefabs/Classes/ClassesSideMenu.prefab");
        AssetDatabase.SaveAssets();
        return $"Migrated {instances.Count} menus to {PrefabPath}; repaired {repaired} references.";
    }

    private static SideMenuFlyoutView Instantiate(GameObject prefab, Transform parent)
    {
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        Undo.RegisterCreatedObjectUndo(instance, "Create side menu instance");
        instance.transform.SetAsLastSibling();
        return instance.GetComponent<SideMenuFlyoutView>();
    }

    private static void Assign(Component target, string field, Object value)
    {
        var fields = new SerializedObject(target);
        fields.FindProperty(field).objectReferenceValue = value;
        fields.ApplyModifiedProperties();
    }

    private static void MapSubtree(GameObject old, GameObject replacement, Dictionary<Object, Object> map)
    {
        map.Add(old, replacement);
        foreach (var component in old.GetComponents<Component>())
        {
            if (component == null) continue;
            var type = component.GetType();
            var index = Array.IndexOf(old.GetComponents(type), component);
            map.Add(component, replacement.GetComponents(type)[index]);
        }
        foreach (Transform child in old.transform)
        {
            var other = replacement.transform.Find(child.name);
            if (other == null) throw new InvalidOperationException("Missing prefab child: " + child.name);
            MapSubtree(child.gameObject, other.gameObject, map);
        }
    }
}
