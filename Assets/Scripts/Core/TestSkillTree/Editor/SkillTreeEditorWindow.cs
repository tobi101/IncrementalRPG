using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Core.TestSkillTree.Editor
{
    public class SkillTreeEditorWindow : EditorWindow
    {
        private const float ToolbarHeight = 22f;
        private const float SidebarWidth  = 320f;
        private const float MinZoom       = 0.35f;
        private const float MaxZoom       = 2.5f;

        private static readonly Vector2 NodeSize = new Vector2(170f, 64f);

        private SkillTreeConfig _config;
        private Vector2 _pan;
        private float _zoom = 1f;
        private int _selectedIndex = -1;
        private bool _draggingNode;
        private bool _panning;
        private Vector2 _dragOffsetGraph;
        private Vector2 _lastMousePosition;
        private Vector2 _sidebarScroll;
        private string _newNodeName = "New Skill Node";
        private string _newNodeFolder;
        private bool _linkNewNodeToSelected = true;
        private bool _showNodeContent = true;
        private NodeDefinition _nodeEditorTarget;
        private UnityEditor.Editor _nodeEditor;

        [MenuItem("Tools/Skill Tree/Editor")]
        private static void OpenFromMenu()
        {
            Open(Selection.activeObject as SkillTreeConfig);
        }

        public static void Open(SkillTreeConfig config)
        {
            var window = GetWindow<SkillTreeEditorWindow>("Skill Tree");
            if (config != null)
                window.SetConfig(config);
        }

        private void OnEnable()
        {
            if (_config == null && Selection.activeObject is SkillTreeConfig config)
                SetConfig(config);
        }

        private void OnSelectionChange()
        {
            if (Selection.activeObject is SkillTreeConfig config)
                SetConfig(config);
        }

        private void OnDisable()
        {
            DestroyCachedNodeEditor();
        }

        private void OnGUI()
        {
            DrawToolbar();

            var contentRect = new Rect(0f, ToolbarHeight, position.width, position.height - ToolbarHeight);

            if (_config == null)
            {
                EditorGUI.HelpBox(
                    new Rect(12f, contentRect.y + 12f, position.width - 24f, 42f),
                    "Assign a SkillTreeConfig asset to edit its node layout.",
                    MessageType.Info);
                return;
            }

            EnsureEntriesList();
            ClampSelection();

            var canvasRect = new Rect(contentRect.x, contentRect.y, Mathf.Max(100f, contentRect.width - SidebarWidth), contentRect.height);
            var sidebarRect = new Rect(canvasRect.xMax, contentRect.y, SidebarWidth, contentRect.height);

            HandleCanvasEvents(canvasRect);
            DrawCanvas(canvasRect);
            DrawSidebar(sidebarRect);
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar, GUILayout.Height(ToolbarHeight)))
            {
                var newConfig = (SkillTreeConfig)EditorGUILayout.ObjectField(
                    _config,
                    typeof(SkillTreeConfig),
                    false,
                    GUILayout.Width(330f));

                if (newConfig != _config)
                    SetConfig(newConfig);

                using (new EditorGUI.DisabledScope(_config == null))
                {
                    if (GUILayout.Button("Frame All", EditorStyles.toolbarButton, GUILayout.Width(80f)))
                        FrameAll();

                    if (GUILayout.Button("Auto Layout", EditorStyles.toolbarButton, GUILayout.Width(90f)))
                        AutoLayout();

                    if (GUILayout.Button("Add Selected Nodes", EditorStyles.toolbarButton, GUILayout.Width(130f)))
                        AddSelectedNodeAssets();
                }
            }
        }

        private void DrawCanvas(Rect canvasRect)
        {
            EditorGUI.DrawRect(canvasRect, new Color(0.12f, 0.12f, 0.13f));

            DrawGrid(canvasRect);
            DrawConnections(canvasRect);

            for (var i = 0; i < Entries.Count; i++)
            {
                if (i != _selectedIndex)
                    DrawNode(canvasRect, i, false);
            }

            if (IsValidEntryIndex(_selectedIndex))
                DrawNode(canvasRect, _selectedIndex, true);

            if (Entries.Count == 0)
            {
                var style = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
                {
                    fontSize = 13,
                    alignment = TextAnchor.MiddleCenter
                };
                GUI.Label(canvasRect, "Drag NodeDefinition assets here or use Add Selected Nodes.", style);
            }
        }

        private void DrawGrid(Rect canvasRect)
        {
            var spacing = GetSafeCellSpacing();
            var graphMin = ScreenToGraph(new Vector2(canvasRect.xMin, canvasRect.yMin), canvasRect);
            var graphMax = ScreenToGraph(new Vector2(canvasRect.xMax, canvasRect.yMax), canvasRect);
            var verticalStep = Mathf.Max(1, Mathf.CeilToInt(12f / (spacing.x * _zoom)));
            var horizontalStep = Mathf.Max(1, Mathf.CeilToInt(12f / (spacing.y * _zoom)));

            var minGridX = Mathf.FloorToInt((graphMin.x - _config.origin.x) / spacing.x) - 1;
            var maxGridX = Mathf.CeilToInt((graphMax.x - _config.origin.x) / spacing.x) + 1;
            var minGridY = Mathf.FloorToInt((graphMin.y - _config.origin.y) / spacing.y) - 1;
            var maxGridY = Mathf.CeilToInt((graphMax.y - _config.origin.y) / spacing.y) + 1;

            Handles.BeginGUI();

            for (var x = minGridX; x <= maxGridX; x += verticalStep)
            {
                var graphX = _config.origin.x + x * spacing.x;
                var screenX = GraphToScreen(new Vector2(graphX, 0f), canvasRect).x;
                Handles.color = x == 0 ? new Color(0.42f, 0.42f, 0.45f) : new Color(0.22f, 0.22f, 0.24f);
                Handles.DrawLine(new Vector3(screenX, canvasRect.yMin), new Vector3(screenX, canvasRect.yMax));
            }

            for (var y = minGridY; y <= maxGridY; y += horizontalStep)
            {
                var graphY = _config.origin.y + y * spacing.y;
                var screenY = GraphToScreen(new Vector2(0f, graphY), canvasRect).y;
                Handles.color = y == 0 ? new Color(0.42f, 0.42f, 0.45f) : new Color(0.22f, 0.22f, 0.24f);
                Handles.DrawLine(new Vector3(canvasRect.xMin, screenY), new Vector3(canvasRect.xMax, screenY));
            }

            Handles.EndGUI();
        }

        private void DrawConnections(Rect canvasRect)
        {
            var indexByNode = BuildEntryIndexByNode();

            Handles.BeginGUI();
            Handles.color = new Color(0.78f, 0.78f, 0.82f, 0.65f);

            for (var i = 0; i < Entries.Count; i++)
            {
                var entry = Entries[i];
                if (entry?.node == null || entry.node.prerequisites == null)
                    continue;

                var to = GraphToScreen(_config.GridToGraphPosition(entry.gridPosition), canvasRect);

                foreach (var prerequisite in entry.node.prerequisites)
                {
                    if (prerequisite.node == null || !indexByNode.TryGetValue(prerequisite.node, out var parentIndex))
                        continue;

                    var parentEntry = Entries[parentIndex];
                    var from = GraphToScreen(_config.GridToGraphPosition(parentEntry.gridPosition), canvasRect);
                    Handles.DrawAAPolyLine(4f, new Vector3(from.x, from.y), new Vector3(to.x, to.y));
                }
            }

            Handles.EndGUI();
        }

        private void DrawNode(Rect canvasRect, int index, bool selected)
        {
            var entry = Entries[index];
            var rect = GetNodeRect(canvasRect, index);
            var fill = selected ? new Color(0.24f, 0.36f, 0.48f) : new Color(0.18f, 0.19f, 0.21f);
            var outline = selected ? new Color(0.48f, 0.72f, 1f) : new Color(0.42f, 0.43f, 0.46f);

            Handles.BeginGUI();
            Handles.DrawSolidRectangleWithOutline(rect, fill, outline);
            Handles.EndGUI();

            if (_zoom < 0.5f)
                return;

            var label = entry?.node != null ? GetNodeLabel(entry.node) : "<missing node>";
            var labelStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                normal = { textColor = entry?.node != null ? Color.white : new Color(1f, 0.55f, 0.55f) },
                alignment = TextAnchor.MiddleLeft,
                clipping = TextClipping.Clip,
                fontSize = Mathf.Max(7, Mathf.RoundToInt(12f * _zoom))
            };
            var gridStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                normal = { textColor = new Color(0.78f, 0.8f, 0.84f) },
                alignment = TextAnchor.MiddleLeft,
                clipping = TextClipping.Clip,
                fontSize = Mathf.Max(6, Mathf.RoundToInt(10f * _zoom))
            };

            GUI.BeginGroup(rect);

            var padding = 8f * _zoom;
            var gap = 8f * _zoom;
            var iconSize = Mathf.Min(32f * _zoom, rect.height - padding * 2f);
            var iconRect = new Rect(padding, (rect.height - iconSize) * 0.5f, iconSize, iconSize);

            if (entry?.node != null && entry.node.icon != null)
                GUI.DrawTexture(iconRect, entry.node.icon.texture, ScaleMode.ScaleToFit);
            else
                EditorGUI.DrawRect(iconRect, new Color(0.08f, 0.08f, 0.09f));

            if (entry?.node != null && entry.node.additionalIcon != null)
            {
                var badgeSize = iconSize * 0.55f;
                var badgeRect = new Rect(iconRect.xMax - badgeSize * 0.75f, iconRect.yMin, badgeSize, badgeSize);
                GUI.DrawTexture(badgeRect, entry.node.additionalIcon.texture, ScaleMode.ScaleToFit);
            }

            var textX = iconRect.xMax + gap;
            var textWidth = rect.width - textX - padding;

            if (textWidth > 12f)
            {
                if (_zoom >= 0.7f)
                {
                    GUI.Label(new Rect(textX, 9f * _zoom, textWidth, 22f * _zoom), label, labelStyle);
                    GUI.Label(
                        new Rect(textX, 34f * _zoom, textWidth, 18f * _zoom),
                        $"Grid: {entry.gridPosition.x}, {entry.gridPosition.y}",
                        gridStyle);
                }
                else
                {
                    GUI.Label(
                        new Rect(textX, 0f, textWidth, rect.height),
                        label,
                        labelStyle);
                }
            }

            GUI.EndGroup();
        }

        private void DrawSidebar(Rect sidebarRect)
        {
            EditorGUI.DrawRect(sidebarRect, new Color(0.16f, 0.16f, 0.17f));

            GUILayout.BeginArea(new Rect(sidebarRect.x + 10f, sidebarRect.y + 10f, sidebarRect.width - 20f, sidebarRect.height - 20f));
            _sidebarScroll = EditorGUILayout.BeginScrollView(_sidebarScroll);

            EditorGUILayout.LabelField("Layout", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            var cellSpacing = EditorGUILayout.Vector2Field("Cell Spacing", _config.cellSpacing);
            var origin = EditorGUILayout.Vector2Field("Origin", _config.origin);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_config, "Edit Skill Tree Layout Settings");
                _config.cellSpacing = new Vector2(Mathf.Max(1f, cellSpacing.x), Mathf.Max(1f, cellSpacing.y));
                _config.origin = origin;
                EditorUtility.SetDirty(_config);
            }

            EditorGUILayout.Space(10f);
            DrawNodeAssetTools();

            EditorGUILayout.Space(10f);
            DrawSelectedEntryInspector();

            EditorGUILayout.Space(10f);
            DrawSelectedNodeContent();

            EditorGUILayout.Space(10f);
            DrawValidation();

            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void DrawNodeAssetTools()
        {
            EditorGUILayout.LabelField("Node Assets", EditorStyles.boldLabel);

            _newNodeName = EditorGUILayout.TextField("New Node Name", _newNodeName);

            using (new EditorGUILayout.HorizontalScope())
            {
                _newNodeFolder = EditorGUILayout.TextField("Folder", GetNormalizedNodeFolder());

                if (GUILayout.Button("...", GUILayout.Width(28f)))
                    SelectNewNodeFolder();
            }

            using (new EditorGUI.DisabledScope(!IsValidSelectedNode()))
                _linkNewNodeToSelected = EditorGUILayout.ToggleLeft("Use selected node as prerequisite", _linkNewNodeToSelected);

            if (GUILayout.Button("Create Node"))
                CreateNodeAssetFromSidebar();
        }

        private void DrawSelectedEntryInspector()
        {
            EditorGUILayout.LabelField("Selected", EditorStyles.boldLabel);

            if (!IsValidEntryIndex(_selectedIndex))
            {
                EditorGUILayout.HelpBox("Select a node in the graph to edit its grid position.", MessageType.Info);
                return;
            }

            var entry = Entries[_selectedIndex];

            using (new EditorGUI.DisabledScope(true))
                EditorGUILayout.ObjectField("Node", entry.node, typeof(NodeDefinition), false);

            EditorGUI.BeginChangeCheck();
            var gridX = EditorGUILayout.IntField("Grid X", entry.gridPosition.x);
            var gridY = EditorGUILayout.IntField("Grid Y", entry.gridPosition.y);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_config, "Edit Skill Tree Node Position");
                entry.gridPosition = new Vector2Int(gridX, gridY);
                EditorUtility.SetDirty(_config);
                Repaint();
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Ping") && entry.node != null)
                    EditorGUIUtility.PingObject(entry.node);

                if (GUILayout.Button("Remove From Tree"))
                    RemoveEntryFromTree(_selectedIndex);
            }

            using (new EditorGUI.DisabledScope(entry.node == null))
            {
                if (GUILayout.Button("Delete Asset..."))
                    DeleteSelectedNodeAsset();
            }
        }

        private void DrawSelectedNodeContent()
        {
            if (!IsValidSelectedNode())
            {
                DestroyCachedNodeEditor();
                return;
            }

            _showNodeContent = EditorGUILayout.Foldout(_showNodeContent, "Node Content", true);
            if (!_showNodeContent)
                return;

            var node = Entries[_selectedIndex].node;
            EnsureCachedNodeEditor(node);

            if (_nodeEditor == null)
                return;

            EditorGUI.BeginChangeCheck();
            _nodeEditor.OnInspectorGUI();
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(node);
                Repaint();
            }
        }

        private void DrawValidation()
        {
            EditorGUILayout.LabelField("Validation", EditorStyles.boldLabel);

            var warnings = GetValidationWarnings();
            if (warnings.Count == 0)
            {
                EditorGUILayout.HelpBox("No layout warnings.", MessageType.Info);
                return;
            }

            foreach (var warning in warnings)
                EditorGUILayout.HelpBox(warning, MessageType.Warning);
        }

        private void HandleCanvasEvents(Rect canvasRect)
        {
            var evt = Event.current;
            HandleNodeAssetDrop(canvasRect, evt);

            if (!canvasRect.Contains(evt.mousePosition) && !_draggingNode && !_panning)
                return;

            switch (evt.type)
            {
                case EventType.MouseDown:
                    if (evt.button == 0 && !evt.alt)
                    {
                        var hitIndex = GetNodeIndexAt(canvasRect, evt.mousePosition);
                        _selectedIndex = hitIndex;

                        if (hitIndex >= 0)
                        {
                            Undo.RecordObject(_config, "Move Skill Tree Node");
                            _draggingNode = true;
                            _dragOffsetGraph = ScreenToGraph(evt.mousePosition, canvasRect)
                                               - _config.GridToGraphPosition(Entries[hitIndex].gridPosition);
                        }

                        evt.Use();
                        Repaint();
                    }
                    else if (evt.button == 2 || (evt.button == 0 && evt.alt))
                    {
                        _panning = true;
                        _lastMousePosition = evt.mousePosition;
                        evt.Use();
                    }
                    break;

                case EventType.MouseDrag:
                    if (_draggingNode && IsValidEntryIndex(_selectedIndex))
                    {
                        var graphPosition = ScreenToGraph(evt.mousePosition, canvasRect) - _dragOffsetGraph;
                        var newGridPosition = _config.GraphToGridPosition(graphPosition);

                        if (Entries[_selectedIndex].gridPosition != newGridPosition)
                        {
                            Entries[_selectedIndex].gridPosition = newGridPosition;
                            EditorUtility.SetDirty(_config);
                        }

                        evt.Use();
                        Repaint();
                    }
                    else if (_panning)
                    {
                        _pan += evt.mousePosition - _lastMousePosition;
                        _lastMousePosition = evt.mousePosition;
                        evt.Use();
                        Repaint();
                    }
                    break;

                case EventType.MouseUp:
                    if (_draggingNode || _panning)
                    {
                        _draggingNode = false;
                        _panning = false;
                        evt.Use();
                    }
                    break;

                case EventType.ScrollWheel:
                    ZoomAt(canvasRect, evt.mousePosition, evt.delta.y);
                    evt.Use();
                    Repaint();
                    break;

                case EventType.ContextClick:
                    ShowContextMenu(canvasRect, evt.mousePosition);
                    evt.Use();
                    break;
            }
        }

        private void HandleNodeAssetDrop(Rect canvasRect, Event evt)
        {
            if (evt.type != EventType.DragUpdated && evt.type != EventType.DragPerform)
                return;

            if (!canvasRect.Contains(evt.mousePosition) || !HasDraggedNodeDefinitions())
                return;

            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

            if (evt.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                Undo.RecordObject(_config, "Add Skill Tree Nodes");

                var baseGridPosition = _config.GraphToGridPosition(ScreenToGraph(evt.mousePosition, canvasRect));
                var offset = 0;

                foreach (var draggedObject in DragAndDrop.objectReferences)
                {
                    if (draggedObject is not NodeDefinition node || ContainsNode(node))
                        continue;

                    Entries.Add(new SkillTreeNodeEntry
                    {
                        node = node,
                        gridPosition = baseGridPosition + new Vector2Int(offset, 0)
                    });
                    offset++;
                }

                EditorUtility.SetDirty(_config);
            }

            evt.Use();
        }

        private void ShowContextMenu(Rect canvasRect, Vector2 mousePosition)
        {
            var menu = new GenericMenu();
            var hitIndex = GetNodeIndexAt(canvasRect, mousePosition);

            if (hitIndex >= 0)
            {
                _selectedIndex = hitIndex;
                var entry = Entries[hitIndex];

                menu.AddItem(new GUIContent("Ping Node Asset"), false, () =>
                {
                    if (entry.node != null)
                        EditorGUIUtility.PingObject(entry.node);
                });
                menu.AddItem(new GUIContent("Remove From Tree"), false, () => RemoveEntryFromTree(hitIndex));

                if (entry.node != null)
                    menu.AddItem(new GUIContent("Delete Node Asset..."), false, DeleteSelectedNodeAsset);
                else
                    menu.AddDisabledItem(new GUIContent("Delete Node Asset..."));
            }
            else
            {
                var gridPosition = _config.GraphToGridPosition(ScreenToGraph(mousePosition, canvasRect));
                menu.AddItem(new GUIContent("Create Node Here"), false, () => CreateNodeAssetAt(gridPosition));
            }

            menu.ShowAsContext();
        }

        private void ZoomAt(Rect canvasRect, Vector2 mousePosition, float wheelDelta)
        {
            var graphPositionBeforeZoom = ScreenToGraph(mousePosition, canvasRect);
            var factor = wheelDelta > 0f ? 0.9f : 1.1f;
            _zoom = Mathf.Clamp(_zoom * factor, MinZoom, MaxZoom);
            var screenPositionAfterZoom = GraphToScreen(graphPositionBeforeZoom, canvasRect);
            _pan += mousePosition - screenPositionAfterZoom;
        }

        private void FrameAll()
        {
            if (_config == null || Entries.Count == 0)
            {
                _pan = Vector2.zero;
                _zoom = 1f;
                return;
            }

            var canvasRect = new Rect(0f, ToolbarHeight, Mathf.Max(100f, position.width - SidebarWidth), position.height - ToolbarHeight);
            var bounds = GetGraphBounds();
            var size = bounds.size + NodeSize;
            var zoomX = canvasRect.width / Mathf.Max(1f, size.x);
            var zoomY = canvasRect.height / Mathf.Max(1f, size.y);

            _zoom = Mathf.Clamp(Mathf.Min(zoomX, zoomY) * 0.85f, MinZoom, MaxZoom);
            _pan = -GraphToScreenOffset(bounds.center) * _zoom;
            Repaint();
        }

        private void AutoLayout()
        {
            if (_config == null || Entries.Count == 0)
                return;

            if (!EditorUtility.DisplayDialog(
                    "Auto Layout Skill Tree",
                    "This will overwrite grid positions for all entries in this config.",
                    "Auto Layout",
                    "Cancel"))
                return;

            Undo.RecordObject(_config, "Auto Layout Skill Tree");

            var depthsByNode = new Dictionary<NodeDefinition, int>();
            var visiting = new HashSet<NodeDefinition>();
            var entriesByDepth = new SortedDictionary<int, List<SkillTreeNodeEntry>>();

            foreach (var entry in Entries)
            {
                if (entry?.node == null)
                    continue;

                var depth = GetNodeDepth(entry.node, depthsByNode, visiting);
                if (!entriesByDepth.TryGetValue(depth, out var depthEntries))
                {
                    depthEntries = new List<SkillTreeNodeEntry>();
                    entriesByDepth[depth] = depthEntries;
                }

                depthEntries.Add(entry);
            }

            foreach (var pair in entriesByDepth)
            {
                var depthEntries = pair.Value;
                depthEntries.Sort((a, b) => string.Compare(GetNodeLabel(a.node), GetNodeLabel(b.node), StringComparison.OrdinalIgnoreCase));

                for (var i = 0; i < depthEntries.Count; i++)
                    depthEntries[i].gridPosition = new Vector2Int(i - depthEntries.Count / 2, pair.Key);
            }

            EditorUtility.SetDirty(_config);
            FrameAll();
        }

        private int GetNodeDepth(NodeDefinition node, Dictionary<NodeDefinition, int> depthsByNode, HashSet<NodeDefinition> visiting)
        {
            if (node == null)
                return 0;

            if (depthsByNode.TryGetValue(node, out var depth))
                return depth;

            if (!visiting.Add(node))
                return 0;

            depth = 0;
            if (node.prerequisites != null)
            {
                foreach (var prerequisite in node.prerequisites)
                {
                    if (prerequisite.node != null)
                        depth = Mathf.Max(depth, GetNodeDepth(prerequisite.node, depthsByNode, visiting) + 1);
                }
            }

            visiting.Remove(node);
            depthsByNode[node] = depth;
            return depth;
        }

        private void AddSelectedNodeAssets()
        {
            if (_config == null)
                return;

            Undo.RecordObject(_config, "Add Selected Skill Tree Nodes");

            var addedAny = false;
            foreach (var selectedObject in Selection.objects)
            {
                if (selectedObject is not NodeDefinition node || ContainsNode(node))
                    continue;

                Entries.Add(new SkillTreeNodeEntry
                {
                    node = node,
                    gridPosition = FindNextFreeGridPosition()
                });
                addedAny = true;
            }

            if (!addedAny)
                return;

            EditorUtility.SetDirty(_config);
            Repaint();
        }

        private void CreateNodeAssetFromSidebar()
        {
            var parentNode = IsValidSelectedNode() && _linkNewNodeToSelected
                ? Entries[_selectedIndex].node
                : null;

            var preferredPosition = IsValidEntryIndex(_selectedIndex)
                ? Entries[_selectedIndex].gridPosition + Vector2Int.up
                : FindNextFreeGridPosition();

            CreateNodeAssetAt(FindFreeGridPositionNear(preferredPosition), parentNode);
        }

        private void CreateNodeAssetAt(Vector2Int gridPosition, NodeDefinition parentNode = null)
        {
            if (_config == null)
                return;

            var displayName = string.IsNullOrWhiteSpace(_newNodeName)
                ? "New Skill Node"
                : _newNodeName.Trim();
            var folder = GetNormalizedNodeFolder();

            if (!EnsureAssetFolder(folder))
            {
                EditorUtility.DisplayDialog(
                    "Create Node",
                    "Node folder must be inside the Assets directory.",
                    "OK");
                return;
            }

            var assetName = SanitizeAssetFileName(displayName);
            var assetPath = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{assetName}.asset");
            var node = CreateInstance<NodeDefinition>();

            node.id = CreateUniqueNodeId(displayName);
            node.maxLevel = 1;
            node.goldCostPerLevel = new[] { 0 };
            node.prerequisites = new List<NodePrerequisite>();
            node.effects = Array.Empty<NodeEffect>();

            if (parentNode != null)
            {
                node.prerequisites.Add(new NodePrerequisite
                {
                    node = parentNode,
                    requiredLevel = 1
                });
            }

            AssetDatabase.CreateAsset(node, assetPath);
            Undo.RegisterCreatedObjectUndo(node, "Create Skill Tree Node Asset");
            Undo.RecordObject(_config, "Add Created Skill Tree Node");

            Entries.Add(new SkillTreeNodeEntry
            {
                node = node,
                gridPosition = FindFreeGridPositionNear(gridPosition)
            });
            _selectedIndex = Entries.Count - 1;

            EditorUtility.SetDirty(_config);
            AssetDatabase.SaveAssets();
            Selection.activeObject = node;
            EditorGUIUtility.PingObject(node);
            Repaint();
        }

        private void RemoveEntryFromTree(int entryIndex)
        {
            if (!IsValidEntryIndex(entryIndex))
                return;

            var entry = Entries[entryIndex];
            if (entry?.node != null)
            {
                var dependents = FindCurrentTreeDependents(entry.node);
                if (dependents.Count > 0)
                {
                    EditorUtility.DisplayDialog(
                        "Remove From Tree",
                        $"{GetNodeLabel(entry.node)} cannot be removed because these nodes depend on it:\n\n{FormatBlockingReferences(dependents)}\n\nRemove or reassign those prerequisites first.",
                        "OK");
                    return;
                }
            }

            Undo.RecordObject(_config, "Remove Skill Tree Node Entry");
            Entries.RemoveAt(entryIndex);
            _selectedIndex = -1;
            DestroyCachedNodeEditor();
            EditorUtility.SetDirty(_config);
            Repaint();
        }

        private void DeleteSelectedNodeAsset()
        {
            if (!IsValidSelectedNode())
                return;

            var node = Entries[_selectedIndex].node;
            var assetPath = AssetDatabase.GetAssetPath(node);

            if (string.IsNullOrWhiteSpace(assetPath))
            {
                EditorUtility.DisplayDialog(
                    "Delete Node Asset",
                    $"{GetNodeLabel(node)} is not a saved asset.",
                    "OK");
                return;
            }

            var blockers = GetNodeAssetDeletionBlockers(node);
            if (blockers.Count > 0)
            {
                EditorUtility.DisplayDialog(
                    "Delete Node Asset",
                    $"{GetNodeLabel(node)} cannot be deleted because it is still referenced:\n\n{FormatBlockingReferences(blockers)}\n\nRemove or reassign those references first.",
                    "OK");
                return;
            }

            if (!EditorUtility.DisplayDialog(
                    "Delete Node Asset",
                    $"Delete {GetNodeLabel(node)}?\n\n{assetPath}\n\nThis will remove the node from the current tree and delete the asset from disk.",
                    "Delete",
                    "Cancel"))
                return;

            Undo.RecordObject(_config, "Delete Skill Tree Node Asset");
            Entries.RemoveAll(entry => entry?.node == node);
            _selectedIndex = -1;
            DestroyCachedNodeEditor();
            EditorUtility.SetDirty(_config);

            AssetDatabase.DeleteAsset(assetPath);
            AssetDatabase.SaveAssets();
            Repaint();
        }

        private List<string> GetValidationWarnings()
        {
            var warnings = new List<string>();
            var firstEntryByNode = new Dictionary<NodeDefinition, int>();
            var firstEntryByCell = new Dictionary<Vector2Int, int>();
            var rootCount = 0;

            if (Entries.Count == 0)
                warnings.Add("Config has no node entries.");

            for (var i = 0; i < Entries.Count; i++)
            {
                var entry = Entries[i];
                if (entry == null || entry.node == null)
                {
                    warnings.Add($"Entry {i} has no node asset.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(entry.node.id))
                    warnings.Add($"{GetNodeLabel(entry.node)} has an empty id.");

                if (firstEntryByNode.TryGetValue(entry.node, out var firstNodeIndex))
                    warnings.Add($"{GetNodeLabel(entry.node)} is used by entries {firstNodeIndex} and {i}.");
                else
                    firstEntryByNode[entry.node] = i;

                if (firstEntryByCell.TryGetValue(entry.gridPosition, out var firstCellIndex))
                    warnings.Add($"Entries {firstCellIndex} and {i} share grid cell {entry.gridPosition.x}, {entry.gridPosition.y}.");
                else
                    firstEntryByCell[entry.gridPosition] = i;

                if (entry.node.prerequisites == null || entry.node.prerequisites.Count == 0)
                {
                    rootCount++;
                    continue;
                }

                foreach (var prerequisite in entry.node.prerequisites)
                {
                    if (prerequisite.node == null)
                    {
                        warnings.Add($"{GetNodeLabel(entry.node)} has an empty prerequisite.");
                        continue;
                    }

                    if (prerequisite.requiredLevel < 1)
                        warnings.Add($"{GetNodeLabel(entry.node)} has prerequisite level below 1.");

                    if (!ContainsNode(prerequisite.node))
                        warnings.Add($"{GetNodeLabel(entry.node)} depends on {GetNodeLabel(prerequisite.node)}, but that node is not in this config.");
                }
            }

            if (Entries.Count > 0 && rootCount == 0)
                warnings.Add("Config has no root node. At least one node should have no prerequisites.");

            if (HasPrerequisiteCycle())
                warnings.Add("Prerequisite graph contains a cycle.");

            return warnings;
        }

        private bool HasPrerequisiteCycle()
        {
            var visitStateByNode = new Dictionary<NodeDefinition, int>();

            foreach (var entry in Entries)
            {
                if (entry?.node != null && Visit(entry.node))
                    return true;
            }

            return false;

            bool Visit(NodeDefinition node)
            {
                if (visitStateByNode.TryGetValue(node, out var state))
                    return state == 1;

                visitStateByNode[node] = 1;

                if (node.prerequisites != null)
                {
                    foreach (var prerequisite in node.prerequisites)
                    {
                        if (prerequisite.node != null && Visit(prerequisite.node))
                            return true;
                    }
                }

                visitStateByNode[node] = 2;
                return false;
            }
        }

        private Rect GetGraphBounds()
        {
            var min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
            var max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);

            foreach (var entry in Entries)
            {
                if (entry?.node == null)
                    continue;

                var position = _config.GridToGraphPosition(entry.gridPosition);
                min = Vector2.Min(min, position);
                max = Vector2.Max(max, position);
            }

            if (float.IsInfinity(min.x))
                return new Rect(Vector2.zero, Vector2.one);

            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        private Vector2 GraphToScreen(Vector2 graphPosition, Rect canvasRect) =>
            canvasRect.center + _pan + GraphToScreenOffset(graphPosition) * _zoom;

        private Vector2 ScreenToGraph(Vector2 screenPosition, Rect canvasRect)
        {
            var local = (screenPosition - canvasRect.center - _pan) / _zoom;
            return new Vector2(local.x, -local.y);
        }

        private static Vector2 GraphToScreenOffset(Vector2 graphPosition) =>
            new Vector2(graphPosition.x, -graphPosition.y);

        private Rect GetNodeRect(Rect canvasRect, int index)
        {
            var entry = Entries[index];
            var center = GraphToScreen(_config.GridToGraphPosition(entry.gridPosition), canvasRect);
            var size = NodeSize * _zoom;
            return new Rect(center.x - size.x * 0.5f, center.y - size.y * 0.5f, size.x, size.y);
        }

        private int GetNodeIndexAt(Rect canvasRect, Vector2 mousePosition)
        {
            for (var i = Entries.Count - 1; i >= 0; i--)
            {
                if (GetNodeRect(canvasRect, i).Contains(mousePosition))
                    return i;
            }

            return -1;
        }

        private Vector2Int FindNextFreeGridPosition()
        {
            var candidate = Vector2Int.zero;
            while (IsCellOccupied(candidate))
                candidate.x++;
            return candidate;
        }

        private Vector2Int FindFreeGridPositionNear(Vector2Int preferredPosition)
        {
            if (!IsCellOccupied(preferredPosition))
                return preferredPosition;

            for (var radius = 1; radius < 100; radius++)
            {
                for (var y = -radius; y <= radius; y++)
                {
                    for (var x = -radius; x <= radius; x++)
                    {
                        if (Mathf.Abs(x) != radius && Mathf.Abs(y) != radius)
                            continue;

                        var candidate = preferredPosition + new Vector2Int(x, y);
                        if (!IsCellOccupied(candidate))
                            return candidate;
                    }
                }
            }

            return FindNextFreeGridPosition();
        }

        private bool IsCellOccupied(Vector2Int gridPosition)
        {
            foreach (var entry in Entries)
            {
                if (entry != null && entry.gridPosition == gridPosition)
                    return true;
            }

            return false;
        }

        private bool ContainsNode(NodeDefinition node)
        {
            foreach (var entry in Entries)
            {
                if (entry?.node == node)
                    return true;
            }

            return false;
        }

        private List<string> FindCurrentTreeDependents(NodeDefinition target)
        {
            var dependents = new List<string>();

            foreach (var entry in Entries)
            {
                if (entry?.node == null || entry.node == target)
                    continue;

                if (HasPrerequisite(entry.node, target))
                    dependents.Add($"{GetNodeLabel(entry.node)} ({AssetDatabase.GetAssetPath(entry.node)})");
            }

            return dependents;
        }

        private List<string> GetNodeAssetDeletionBlockers(NodeDefinition target)
        {
            var blockers = new List<string>();

            foreach (var node in GetAllNodeDefinitionAssets())
            {
                if (node == null || node == target)
                    continue;

                if (HasPrerequisite(node, target))
                    blockers.Add($"{GetNodeLabel(node)} prerequisite ({AssetDatabase.GetAssetPath(node)})");
            }

            foreach (var config in GetAllSkillTreeConfigAssets())
            {
                if (config == null || config == _config)
                    continue;

                foreach (var entry in config.NodeEntries)
                {
                    if (entry.node != target)
                        continue;

                    blockers.Add($"{config.name} tree entry ({AssetDatabase.GetAssetPath(config)})");
                    break;
                }
            }

            return blockers;
        }

        private static bool HasPrerequisite(NodeDefinition node, NodeDefinition prerequisiteNode)
        {
            if (node?.prerequisites == null)
                return false;

            foreach (var prerequisite in node.prerequisites)
            {
                if (prerequisite.node == prerequisiteNode)
                    return true;
            }

            return false;
        }

        private static string FormatBlockingReferences(List<string> references)
        {
            var sb = new StringBuilder();
            var count = Mathf.Min(references.Count, 12);

            for (var i = 0; i < count; i++)
                sb.AppendLine($"- {references[i]}");

            if (references.Count > count)
                sb.AppendLine($"... and {references.Count - count} more");

            return sb.ToString().TrimEnd();
        }

        private string GetNormalizedNodeFolder()
        {
            if (string.IsNullOrWhiteSpace(_newNodeFolder))
                _newNodeFolder = GetDefaultNodeFolder();

            _newNodeFolder = NormalizeAssetPath(_newNodeFolder);
            return _newNodeFolder;
        }

        private string GetDefaultNodeFolder()
        {
            const string defaultNodeFolder = "Assets/SO/SkillTree/Nodes";
            if (AssetDatabase.IsValidFolder(defaultNodeFolder))
                return defaultNodeFolder;

            var configPath = _config != null ? AssetDatabase.GetAssetPath(_config) : string.Empty;
            if (!string.IsNullOrWhiteSpace(configPath))
                return NormalizeAssetPath(Path.GetDirectoryName(configPath));

            return "Assets";
        }

        private void SelectNewNodeFolder()
        {
            var currentFolder = GetNormalizedNodeFolder();
            var projectRoot = Directory.GetCurrentDirectory();
            var absoluteCurrentFolder = Path.Combine(projectRoot, currentFolder);
            var selectedFolder = EditorUtility.OpenFolderPanel("Node Folder", absoluteCurrentFolder, string.Empty);

            if (string.IsNullOrWhiteSpace(selectedFolder))
                return;

            var dataPath = NormalizeAssetPath(Application.dataPath);
            var normalizedSelectedFolder = NormalizeAssetPath(selectedFolder);

            if (normalizedSelectedFolder != dataPath &&
                !normalizedSelectedFolder.StartsWith($"{dataPath}/", StringComparison.Ordinal))
            {
                EditorUtility.DisplayDialog("Node Folder", "Node folder must be inside the Assets directory.", "OK");
                return;
            }

            _newNodeFolder = "Assets" + normalizedSelectedFolder.Substring(dataPath.Length);
            if (_newNodeFolder == "Assets/")
                _newNodeFolder = "Assets";
        }

        private static string NormalizeAssetPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return string.Empty;

            return path.Replace('\\', '/').Trim().TrimEnd('/');
        }

        private static bool EnsureAssetFolder(string folder)
        {
            folder = NormalizeAssetPath(folder);

            if (folder == "Assets")
                return true;

            if (!folder.StartsWith("Assets/", StringComparison.Ordinal))
                return false;

            var parts = folder.Split('/');
            var current = "Assets";

            for (var i = 1; i < parts.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(parts[i]))
                    continue;

                var next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);

                current = next;
            }

            return AssetDatabase.IsValidFolder(folder);
        }

        private string CreateUniqueNodeId(string displayName)
        {
            var baseId = ToSnakeCase(displayName);
            var usedIds = GetUsedNodeIds();

            if (!usedIds.Contains(baseId))
                return baseId;

            for (var i = 2; i < 10000; i++)
            {
                var candidate = $"{baseId}_{i}";
                if (!usedIds.Contains(candidate))
                    return candidate;
            }

            return $"{baseId}_{Guid.NewGuid():N}";
        }

        private static HashSet<string> GetUsedNodeIds()
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);

            foreach (var node in GetAllNodeDefinitionAssets())
            {
                if (node != null && !string.IsNullOrWhiteSpace(node.id))
                    ids.Add(node.id);
            }

            return ids;
        }

        private static IEnumerable<NodeDefinition> GetAllNodeDefinitionAssets()
        {
            foreach (var guid in AssetDatabase.FindAssets("t:NodeDefinition"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var node = AssetDatabase.LoadAssetAtPath<NodeDefinition>(path);

                if (node != null)
                    yield return node;
            }
        }

        private static IEnumerable<SkillTreeConfig> GetAllSkillTreeConfigAssets()
        {
            foreach (var guid in AssetDatabase.FindAssets("t:SkillTreeConfig"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var config = AssetDatabase.LoadAssetAtPath<SkillTreeConfig>(path);

                if (config != null)
                    yield return config;
            }
        }

        private static string SanitizeAssetFileName(string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var sb = new StringBuilder();

            foreach (var ch in fileName.Trim())
            {
                var isInvalid = Array.IndexOf(invalidChars, ch) >= 0 || ch == '/' || ch == '\\';
                sb.Append(isInvalid ? '_' : ch);
            }

            var result = sb.ToString().Trim();
            return string.IsNullOrWhiteSpace(result) ? "New Skill Node" : result;
        }

        private static string ToSnakeCase(string value)
        {
            var sb = new StringBuilder();
            var lastWasSeparator = true;

            foreach (var ch in value)
            {
                if (ch >= 'A' && ch <= 'Z')
                {
                    if (!lastWasSeparator && sb.Length > 0)
                        sb.Append('_');

                    sb.Append(char.ToLowerInvariant(ch));
                    lastWasSeparator = false;
                    continue;
                }

                if ((ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9'))
                {
                    sb.Append(ch);
                    lastWasSeparator = false;
                    continue;
                }

                if (!lastWasSeparator && sb.Length > 0)
                {
                    sb.Append('_');
                    lastWasSeparator = true;
                }
            }

            var result = sb.ToString().Trim('_');
            return string.IsNullOrWhiteSpace(result) ? "new_skill_node" : result;
        }

        private Dictionary<NodeDefinition, int> BuildEntryIndexByNode()
        {
            var indexByNode = new Dictionary<NodeDefinition, int>();
            for (var i = 0; i < Entries.Count; i++)
            {
                if (Entries[i]?.node != null && !indexByNode.ContainsKey(Entries[i].node))
                    indexByNode.Add(Entries[i].node, i);
            }

            return indexByNode;
        }

        private bool HasDraggedNodeDefinitions()
        {
            foreach (var draggedObject in DragAndDrop.objectReferences)
            {
                if (draggedObject is NodeDefinition)
                    return true;
            }

            return false;
        }

        private Vector2 GetSafeCellSpacing() => new Vector2(
            Mathf.Max(1f, _config.cellSpacing.x),
            Mathf.Max(1f, _config.cellSpacing.y));

        private List<SkillTreeNodeEntry> Entries
        {
            get
            {
                EnsureEntriesList();
                return _config.entries;
            }
        }

        private void EnsureEntriesList()
        {
            if (_config != null && _config.entries == null)
                _config.entries = new List<SkillTreeNodeEntry>();
        }

        private void EnsureCachedNodeEditor(NodeDefinition node)
        {
            if (_nodeEditorTarget == node && _nodeEditor != null)
                return;

            DestroyCachedNodeEditor();
            _nodeEditorTarget = node;

            if (node != null)
                UnityEditor.Editor.CreateCachedEditor(node, null, ref _nodeEditor);
        }

        private void DestroyCachedNodeEditor()
        {
            if (_nodeEditor != null)
                DestroyImmediate(_nodeEditor);

            _nodeEditor = null;
            _nodeEditorTarget = null;
        }

        private void ClampSelection()
        {
            if (!IsValidEntryIndex(_selectedIndex))
            {
                _selectedIndex = -1;
                DestroyCachedNodeEditor();
            }
        }

        private bool IsValidEntryIndex(int index) => index >= 0 && index < Entries.Count;

        private bool IsValidSelectedNode() =>
            IsValidEntryIndex(_selectedIndex) && Entries[_selectedIndex]?.node != null;

        private void SetConfig(SkillTreeConfig config)
        {
            _config = config;
            _selectedIndex = -1;
            DestroyCachedNodeEditor();
            _newNodeFolder = GetDefaultNodeFolder();
            Repaint();
        }

        private static string GetNodeLabel(NodeDefinition node)
        {
            if (node == null)
                return "<missing node>";

            if (!string.IsNullOrWhiteSpace(node.id))
                return node.id;

            return !string.IsNullOrWhiteSpace(node.name) ? node.name : "<unnamed node>";
        }
    }

    [CustomEditor(typeof(SkillTreeConfig))]
    public class SkillTreeConfigEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open Skill Tree Editor"))
                SkillTreeEditorWindow.Open((SkillTreeConfig)target);

            EditorGUILayout.Space(6f);
            DrawDefaultInspector();
        }
    }
}
