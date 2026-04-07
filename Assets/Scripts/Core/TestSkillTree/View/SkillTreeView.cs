using System.Collections.Generic;
using Reflex.Attributes;
using UnityEngine;

namespace Core.TestSkillTree.View
{
    // MonoBehaviour orchestrator. Place on the root of the SkillTree Canvas.
    // Register as a value in GameSceneInstaller or use Reflex scene injection.
    public class SkillTreeView : MonoBehaviour
    {
        [SerializeField] private RectTransform      _nodesLayer;
        [SerializeField] private RectTransform      _connectionsLayer;
        [SerializeField] private NodeView           _nodeViewPrefab;
        [SerializeField] private NodeConnectionView _connectionViewPrefab;
        [SerializeField] private NodePopupView      _popupView;

        private SkillTreeService         _service;
        private SkillTreeConfig          _config;
        private readonly List<NodeView>  _nodeViews = new List<NodeView>();
        private readonly List<(NodeConnectionView view, NodeDefinition def)> _connectionViews = new();

        [Inject]
        public void Construct(SkillTreeConfig config, SkillTreeService service)
        {
            _config  = config;
            _service = service;

            _popupView.Bind(service);
            Build();

            _service.OnUpgraded += RefreshAll;
        }

        private void Build()
        {
            // Connections first so they render behind nodes.
            foreach (var def in _config.nodes)
            {
                foreach (var prereq in def.prerequisites)
                {
                    if (prereq.node == null) continue;
                    var connection = Instantiate(_connectionViewPrefab, _connectionsLayer);
                    connection.Setup(prereq.node.positionInGraph, def.positionInGraph);
                    connection.Refresh(_service.GetState(def.id));
                    _connectionViews.Add((connection, def));
                }
            }

            foreach (var def in _config.nodes)
            {
                var nodeView = Instantiate(_nodeViewPrefab, _nodesLayer);
                ((RectTransform)nodeView.transform).anchoredPosition = def.positionInGraph;
                nodeView.Bind(def, _service, _popupView);
                _nodeViews.Add(nodeView);
            }
        }

        private void RefreshAll()
        {
            foreach (var nodeView in _nodeViews)
                nodeView.Refresh();

            foreach (var (view, def) in _connectionViews)
                view.Refresh(_service.GetState(def.id));
        }

        private void OnDestroy()
        {
            if (_service != null)
                _service.OnUpgraded -= RefreshAll;
        }
    }
}
