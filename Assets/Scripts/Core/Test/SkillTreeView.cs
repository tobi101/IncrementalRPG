using System.Collections.Generic;
using UnityEngine;

namespace Core.Test
{
    // Иерархия в сцене:
    // SkillTreePanel (SkillTreePanZoom)
    //   └── TreeContainer (RectTransform — сюда pan/zoom)
    //         ├── ConnectionsLayer  (линии, спавнятся первыми — под узлами)
    //         └── NodesLayer        (узлы)
    public class SkillTreeView : MonoBehaviour
    {
        [SerializeField] private RectTransform _nodesLayer;
        [SerializeField] private RectTransform _connectionsLayer;
        [SerializeField] private SkillNodeView _nodePrefab;
        [SerializeField] private SkillTreeLine _linePrefab;

        private SkillTreeConfig _config;
        private SkillTreeModel _model;
        private readonly Dictionary<string, SkillNodeView> _nodeViews = new();
        private readonly Dictionary<string, RectTransform> _nodeRects = new();

        public System.Action<SkillNodeConfig> OnUpgradeRequested;

        public void Initialize(SkillTreeConfig config, SkillTreeModel model)
        {
            _config = config;
            _model = model;
            SpawnNodes();
            SpawnConnections();
            Refresh();
        }

        public void Refresh()
        {
            foreach (var node in _config.AllNodes)
            {
                if (!_nodeViews.TryGetValue(node.Id, out var view)) continue;
                view.Refresh(_model.GetVisibility(node), _model.GetLevel(node.Id));
            }
        }

        private void SpawnNodes()
        {
            foreach (var node in _config.AllNodes)
            {
                var view = Instantiate(_nodePrefab, _nodesLayer);
                var rect = view.GetComponent<RectTransform>();
                rect.anchoredPosition = node.Position;
                view.Setup(node);
                view.OnUpgradeRequested += cfg => OnUpgradeRequested?.Invoke(cfg);
                _nodeViews[node.Id] = view;
                _nodeRects[node.Id] = rect;
            }
        }

        private void SpawnConnections()
        {
            foreach (var node in _config.AllNodes)
            {
                if (!_nodeRects.TryGetValue(node.Id, out var toRect)) continue;
                foreach (var prereq in node.Prerequisites)
                {
                    if (!_nodeRects.TryGetValue(prereq.Id, out var fromRect)) continue;
                    var line = Instantiate(_linePrefab, _connectionsLayer);
                    line.SetPositions(fromRect.anchoredPosition, toRect.anchoredPosition);
                }
            }
        }
    }
}
