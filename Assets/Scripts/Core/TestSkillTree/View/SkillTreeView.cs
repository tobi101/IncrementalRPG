using System.Collections.Generic;
using Core.StateMachine;
using Core.StateMachine.States;
using IncrementalRPG.Scripts.AudioManager;
using Model;
using Reflex.Attributes;
using TMPro;
using Utils;
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
        [SerializeField] private Vector2            _contentPadding = new Vector2(600f, 600f);

        [Inject] private GameStateMachine     _stateMachine;
        [Inject] private NodeBorderColorConfig _borderColorConfig;
        [Inject] private NodeCircleSpriteConfig _circleSpriteConfig;

        private SkillTreeService         _service;
        private SkillTreeConfig          _config;
        private Player                   _player;
        private AudioManager             _audioManager;
        private readonly List<NodeView>  _nodeViews = new List<NodeView>();
        private readonly Dictionary<string, NodeView> _nodeViewsById = new Dictionary<string, NodeView>();
        private readonly List<(NodeConnectionView view, NodeDefinition def)> _connectionViews = new();

        [Inject]
        public void Construct(SkillTreeConfig config, SkillTreeService service, Player player, AudioManager audioManager)
        {
            _config       = config;
            _service      = service;
            _player       = player;
            _audioManager = audioManager;

            UI.UIButtonAudio.InstallInChildren(this);
            _popupView.Bind(service, _borderColorConfig, audioManager);
            _closeButton.onClick.AddListener(() => _stateMachine.Enter<HubState>());
            Build();

            _service.OnUpgraded += RefreshAll;
            _service.OnNodeUpgraded += PlayNodeUpgradeFeedback;
            _player.OnGoldChanged += RefreshGold;
            RefreshGold();
        }

        private void RefreshGold()
        {
            if (_goldText == null) return;
            _goldText.text = BigDoubleFormatter.FormatFloor(_player.GoldTotal);
        }

        private void Build()
        {
            var nodePositions = new Dictionary<NodeDefinition, Vector2>();
            var entries = new List<SkillTreeNodeEntry>();

            foreach (var entry in _config.NodeEntries)
            {
                if (entry.node == null)
                    continue;

                entries.Add(entry);
                nodePositions[entry.node] = _config.GridToGraphPosition(entry.gridPosition);
            }

            ConfigureContentBounds(nodePositions);

            // Connections first so they render behind nodes.
            foreach (var entry in entries)
            {
                var def = entry.node;
                if (!nodePositions.TryGetValue(def, out var to))
                    continue;

                if (def.prerequisites == null)
                    continue;

                foreach (var prereq in def.prerequisites)
                {
                    if (prereq.node == null) continue;
                    if (!nodePositions.TryGetValue(prereq.node, out var from)) continue;

                    var connection = Instantiate(_connectionViewPrefab, _connectionsLayer);
                    connection.Setup(from, to);
                    connection.Refresh(_service.GetState(def.id));
                    _connectionViews.Add((connection, def));
                }
            }

            foreach (var entry in entries)
            {
                var def = entry.node;
                var nodeView = Instantiate(_nodeViewPrefab, _nodesLayer);
                ((RectTransform)nodeView.transform).anchoredPosition = nodePositions[def];
                nodeView.Bind(def, _service, _popupView, _circleSpriteConfig, _audioManager);
                _nodeViews.Add(nodeView);

                if (!string.IsNullOrEmpty(nodeView.NodeId))
                    _nodeViewsById[nodeView.NodeId] = nodeView;
            }
        }

        private void ConfigureContentBounds(Dictionary<NodeDefinition, Vector2> nodePositions)
        {
            var content = GetContentTransform();
            if (content == null || nodePositions.Count == 0)
                return;

            var min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
            var max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);

            foreach (var position in nodePositions.Values)
            {
                min = Vector2.Min(min, position);
                max = Vector2.Max(max, position);
            }

            var halfNodeSize = GetNodeSize() * 0.5f;
            min -= halfNodeSize + _contentPadding;
            max += halfNodeSize + _contentPadding;

            var center = (min + max) * 0.5f;
            var contentSize = max - min;
            contentSize = new Vector2(
                Mathf.Max(1f, contentSize.x),
                Mathf.Max(1f, contentSize.y));

            ConfigureRect(content, contentSize);
            ConfigureRect(_connectionsLayer, contentSize);
            ConfigureRect(_nodesLayer, contentSize);
            content.localScale = Vector3.one;

            var definitions = new List<NodeDefinition>(nodePositions.Keys);
            foreach (var definition in definitions)
                nodePositions[definition] -= center;
        }

        private RectTransform GetContentTransform()
        {
            if (_nodesLayer != null)
                return _nodesLayer.parent as RectTransform;

            return _connectionsLayer != null
                ? _connectionsLayer.parent as RectTransform
                : null;
        }

        private Vector2 GetNodeSize()
        {
            var nodeRect = _nodeViewPrefab != null
                ? _nodeViewPrefab.transform as RectTransform
                : null;

            if (nodeRect == null)
                return Vector2.zero;

            return nodeRect.rect.size;
        }

        private static void ConfigureRect(RectTransform rect, Vector2 size)
        {
            if (rect == null)
                return;

            rect.anchorMin = Vector2.one * 0.5f;
            rect.anchorMax = Vector2.one * 0.5f;
            rect.pivot = Vector2.one * 0.5f;
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = size;
        }

        private void RefreshAll()
        {
            foreach (var nodeView in _nodeViews)
                nodeView.Refresh();

            foreach (var (view, def) in _connectionViews)
                view.Refresh(_service.GetState(def.id));
        }

        private void PlayNodeUpgradeFeedback(string nodeId)
        {
            if (string.IsNullOrEmpty(nodeId))
                return;

            if (_nodeViewsById.TryGetValue(nodeId, out var nodeView))
                nodeView.PlayUpgradeFeedback();
        }

        private void OnDestroy()
        {
            _closeButton.onClick.RemoveAllListeners();
            if (_service != null)
            {
                _service.OnUpgraded -= RefreshAll;
                _service.OnNodeUpgraded -= PlayNodeUpgradeFeedback;
            }
            if (_player != null)
                _player.OnGoldChanged -= RefreshGold;
        }
    }
}
