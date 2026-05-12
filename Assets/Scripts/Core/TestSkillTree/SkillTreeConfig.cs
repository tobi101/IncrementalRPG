using System.Collections.Generic;
using UnityEngine;

namespace Core.TestSkillTree
{
    [System.Serializable]
    public class SkillTreeNodeEntry
    {
        public NodeDefinition node;
        public Vector2Int gridPosition;
    }

    [CreateAssetMenu(fileName = "SkillTreeConfig", menuName = "RPG/Skill Tree/Config")]
    public class SkillTreeConfig : ScriptableObject
    {
        [Tooltip("Distance between neighboring grid cells in the runtime UI graph.")]
        public Vector2 cellSpacing = new Vector2(100f, 100f);

        [Tooltip("Runtime UI graph offset for grid cell (0, 0).")]
        public Vector2 origin;

        public List<SkillTreeNodeEntry> entries = new();

        [SerializeField, HideInInspector]
        private List<NodeDefinition> nodes = new();

        public IEnumerable<SkillTreeNodeEntry> NodeEntries
        {
            get
            {
                if (entries != null && entries.Count > 0)
                {
                    foreach (var entry in entries)
                    {
                        if (entry?.node != null)
                            yield return entry;
                    }

                    yield break;
                }

                if (nodes == null)
                    yield break;

                foreach (var node in nodes)
                {
                    if (node == null)
                        continue;

                    yield return new SkillTreeNodeEntry
                    {
                        node = node,
                        gridPosition = GraphToGridPosition(node.positionInGraph)
                    };
                }
            }
        }

        public IEnumerable<NodeDefinition> NodeDefinitions
        {
            get
            {
                foreach (var entry in NodeEntries)
                    yield return entry.node;
            }
        }

        public Vector2 GridToGraphPosition(Vector2Int gridPosition)
        {
            var spacing = SanitizedCellSpacing;
            return origin + new Vector2(gridPosition.x * spacing.x, gridPosition.y * spacing.y);
        }

        public Vector2Int GraphToGridPosition(Vector2 graphPosition)
        {
            var spacing = SanitizedCellSpacing;
            var local = graphPosition - origin;

            return new Vector2Int(
                Mathf.RoundToInt(local.x / spacing.x),
                Mathf.RoundToInt(local.y / spacing.y));
        }

        public bool TryGetNodePosition(NodeDefinition node, out Vector2 position)
        {
            if (node == null)
            {
                position = default;
                return false;
            }

            foreach (var entry in NodeEntries)
            {
                if (entry.node != node)
                    continue;

                position = GridToGraphPosition(entry.gridPosition);
                return true;
            }

            position = default;
            return false;
        }

        private Vector2 SanitizedCellSpacing => new Vector2(
            Mathf.Max(1f, cellSpacing.x),
            Mathf.Max(1f, cellSpacing.y));

#if UNITY_EDITOR
        private void OnValidate()
        {
            cellSpacing = SanitizedCellSpacing;

            if ((entries == null || entries.Count == 0) && nodes != null && nodes.Count > 0)
                MigrateLegacyNodesToEntries();
        }

        [ContextMenu("Migrate Legacy Nodes To Grid Entries")]
        private void MigrateLegacyNodesToEntries()
        {
            entries ??= new List<SkillTreeNodeEntry>();
            entries.Clear();

            if (nodes == null)
                return;

            foreach (var node in nodes)
            {
                if (node == null)
                    continue;

                entries.Add(new SkillTreeNodeEntry
                {
                    node = node,
                    gridPosition = GraphToGridPosition(node.positionInGraph)
                });
            }
        }
#endif
    }
}
