using System.Collections.Generic;
using Core.StateMachine;
using Core.StateMachine.States;
using Model;
using Reflex.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        [SerializeField] private TextMeshProUGUI    _goldText;
        [SerializeField] private Button             _closeButton;

        [Inject] private GameStateMachine     _stateMachine;
        [Inject] private NodeBorderColorConfig _borderColorConfig;

        private SkillTreeService         _service;
        private SkillTreeConfig          _config;
        private Player                   _player;
        private readonly List<NodeView>  _nodeViews = new List<NodeView>();
        private readonly List<(NodeConnectionView view, NodeDefinition def)> _connectionViews = new();

        [Inject]
        public void Construct(SkillTreeConfig config, SkillTreeService service, Player player)
        {
            _config  = config;
            _service = service;
            _player  = player;

            _popupView.Bind(service, _borderColorConfig);
            _closeButton.onClick.AddListener(() => _stateMachine.Enter<HubState>());
            Build();

            _service.OnUpgraded += RefreshAll;
            _player.OnGoldChanged += RefreshGold;
            RefreshGold();
        }

        private void RefreshGold()
        {
            if (_goldText == null) return;
            _goldText.text = _player.GoldTotal.ToString();
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
                nodeView.Bind(def, _service, _popupView, _borderColorConfig);
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
            _closeButton.onClick.RemoveAllListeners();
            if (_service != null)
                _service.OnUpgraded -= RefreshAll;
            if (_player != null)
                _player.OnGoldChanged -= RefreshGold;
        }
    }
}
