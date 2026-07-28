using System.Collections;
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
        [SerializeField] private NodeDefinition     _initialFocusNode;
        [SerializeField] private float              _connectionRevealDuration = 0.25f;
        [SerializeField] private float              _nodeRevealDuration = 0.18f;
        [SerializeField] private float              _nodeRevealStartScale = 0.15f;
        [SerializeField] private float              _revealWaveDelay = 0.04f;

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
        private readonly Dictionary<string, NodeDefinition> _nodeDefinitionsById = new Dictionary<string, NodeDefinition>();
        private readonly Dictionary<string, NodeState> _lastNodeStates = new Dictionary<string, NodeState>();
        private readonly Dictionary<string, List<NodeConnectionView>> _incomingConnectionsByNodeId = new Dictionary<string, List<NodeConnectionView>>();
        private SkillTreePanZoomController _panZoomController;
        private Vector2 _initialFocusPosition;
        private bool _hasInitialFocusPosition;
        private bool _hasAppliedInitialFocus;
        private Coroutine _revealRoutine;

        [Inject]
        public void Construct(SkillTreeConfig config, SkillTreeService service, Player player, AudioManager audioManager)
        {
            _config       = config;
            _service      = service;
            _player       = player;
            _audioManager = audioManager;
            _panZoomController = GetComponentInChildren<SkillTreePanZoomController>(true);

            UI.UIButtonAudio.InstallInChildren(this);
            _popupView.Bind(service, _borderColorConfig, audioManager);
            _closeButton.onClick.AddListener(() => _stateMachine.Enter<HubState>());
            Build();

            _service.OnUpgraded += RefreshAll;
            _service.OnNodeUpgraded += HandleNodeUpgraded;
            _player.OnGoldChanged += RefreshGold;
            RefreshGold();
        }

        public void Show()
        {
            gameObject.SetActive(true);
            ApplyInitialFocusIfNeeded();
        }

        public void Hide()
        {
            CompleteActiveRevealFromCachedStates();
            gameObject.SetActive(false);
        }

        private void RefreshGold()
        {
            if (_goldText == null) return;
            _goldText.text = BigDoubleFormatter.FormatFloor(_player.GoldTotal);
        }

        private void Build()
        {
            _nodeViews.Clear();
            _nodeViewsById.Clear();
            _connectionViews.Clear();
            _nodeDefinitionsById.Clear();
            _lastNodeStates.Clear();
            _incomingConnectionsByNodeId.Clear();

            var nodePositions = new Dictionary<NodeDefinition, Vector2>();
            var entries = new List<SkillTreeNodeEntry>();

            foreach (var entry in _config.NodeEntries)
            {
                if (entry.node == null)
                    continue;

                entries.Add(entry);
                nodePositions[entry.node] = _config.GridToGraphPosition(entry.gridPosition);

                if (!string.IsNullOrEmpty(entry.node.id))
                    _nodeDefinitionsById[entry.node.id] = entry.node;
            }

            ConfigureContentBounds(nodePositions);
            CacheInitialFocusPosition(entries, nodePositions);

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

                    RegisterIncomingConnection(def.id, connection);
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

            CacheCurrentNodeStates();
        }

        private void RegisterIncomingConnection(string nodeId, NodeConnectionView connection)
        {
            if (string.IsNullOrEmpty(nodeId) || connection == null)
                return;

            if (!_incomingConnectionsByNodeId.TryGetValue(nodeId, out var connections))
            {
                connections = new List<NodeConnectionView>();
                _incomingConnectionsByNodeId[nodeId] = connections;
            }

            connections.Add(connection);
        }

        private void CacheCurrentNodeStates()
        {
            _lastNodeStates.Clear();

            foreach (var nodeView in _nodeViews)
            {
                var nodeId = nodeView.NodeId;
                if (string.IsNullOrEmpty(nodeId))
                    continue;

                _lastNodeStates[nodeId] = _service.GetState(nodeId);
            }
        }

        private void CacheInitialFocusPosition(List<SkillTreeNodeEntry> entries, Dictionary<NodeDefinition, Vector2> nodePositions)
        {
            _hasInitialFocusPosition = false;

            var focusNode = GetInitialFocusNode(entries, nodePositions);
            if (focusNode == null || !nodePositions.TryGetValue(focusNode, out _initialFocusPosition))
                return;

            _hasInitialFocusPosition = true;
        }

        private NodeDefinition GetInitialFocusNode(List<SkillTreeNodeEntry> entries, Dictionary<NodeDefinition, Vector2> nodePositions)
        {
            if (_initialFocusNode != null && nodePositions.ContainsKey(_initialFocusNode))
                return _initialFocusNode;

            foreach (var entry in entries)
            {
                var node = entry.node;
                if (node == null || !nodePositions.ContainsKey(node))
                    continue;

                if (node.prerequisites == null || node.prerequisites.Count == 0)
                    return node;
            }

            return entries.Count > 0
                ? entries[0].node
                : null;
        }

        private void ApplyInitialFocusIfNeeded()
        {
            if (_hasAppliedInitialFocus)
                return;

            _hasAppliedInitialFocus = true;

            if (!_hasInitialFocusPosition)
                return;

            Canvas.ForceUpdateCanvases();

            if (_panZoomController == null)
                _panZoomController = GetComponentInChildren<SkillTreePanZoomController>(true);

            if (_panZoomController != null)
                _panZoomController.FocusOnContentPoint(_initialFocusPosition, 1f);
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
            CompleteActiveRevealFromCachedStates();

            var canAnimateReveal = isActiveAndEnabled && gameObject.activeInHierarchy;
            var currentStates = new Dictionary<string, NodeState>();
            var revealedNodeIds = new HashSet<string>();

            foreach (var nodeView in _nodeViews)
            {
                var nodeId = nodeView.NodeId;
                if (string.IsNullOrEmpty(nodeId))
                {
                    nodeView.Refresh();
                    continue;
                }

                var state = _service.GetState(nodeId);
                currentStates[nodeId] = state;

                _lastNodeStates.TryGetValue(nodeId, out var previousState);
                var shouldReveal = canAnimateReveal &&
                                   previousState == NodeState.Hidden &&
                                   state != NodeState.Hidden;

                if (shouldReveal)
                {
                    nodeView.PrepareReveal(state, 0f);
                    revealedNodeIds.Add(nodeId);
                    continue;
                }

                nodeView.Refresh(state);
            }

            foreach (var (view, def) in _connectionViews)
            {
                var state = currentStates.TryGetValue(def.id, out var cachedState)
                    ? cachedState
                    : _service.GetState(def.id);

                if (revealedNodeIds.Contains(def.id))
                    view.PrepareReveal();
                else
                    view.Refresh(state);
            }

            _lastNodeStates.Clear();
            foreach (var pair in currentStates)
                _lastNodeStates[pair.Key] = pair.Value;

            if (revealedNodeIds.Count > 0)
                _revealRoutine = StartCoroutine(PlayRevealSequence(revealedNodeIds));
        }

        private void CompleteActiveRevealFromCachedStates()
        {
            if (_revealRoutine == null)
                return;

            StopCoroutine(_revealRoutine);
            _revealRoutine = null;

            foreach (var nodeView in _nodeViews)
            {
                var nodeId = nodeView.NodeId;
                if (!string.IsNullOrEmpty(nodeId) && _lastNodeStates.TryGetValue(nodeId, out var state))
                    nodeView.Refresh(state);
            }

            foreach (var (view, def) in _connectionViews)
            {
                if (_lastNodeStates.TryGetValue(def.id, out var state))
                    view.Refresh(state);
            }
        }

        private IEnumerator PlayRevealSequence(HashSet<string> revealedNodeIds)
        {
            var pending = new HashSet<string>(revealedNodeIds);

            while (pending.Count > 0)
            {
                var wave = GetNextRevealWave(pending);
                if (wave.Count == 0)
                    wave.AddRange(pending);

                yield return PlayConnectionRevealWave(wave);

                foreach (var nodeId in wave)
                {
                    if (_nodeViewsById.TryGetValue(nodeId, out var nodeView))
                        nodeView.PlayReveal(_nodeRevealDuration, _nodeRevealStartScale);
                }

                yield return WaitUnscaled(_nodeRevealDuration);

                foreach (var nodeId in wave)
                    pending.Remove(nodeId);

                if (pending.Count > 0)
                    yield return WaitUnscaled(_revealWaveDelay);
            }

            _revealRoutine = null;
        }

        private List<string> GetNextRevealWave(HashSet<string> pending)
        {
            var wave = new List<string>();

            foreach (var nodeId in pending)
            {
                if (!_nodeDefinitionsById.TryGetValue(nodeId, out var definition) ||
                    !HasPendingPrerequisite(definition, pending))
                {
                    wave.Add(nodeId);
                }
            }

            return wave;
        }

        private static bool HasPendingPrerequisite(NodeDefinition definition, HashSet<string> pending)
        {
            if (definition.prerequisites == null)
                return false;

            foreach (var prerequisite in definition.prerequisites)
            {
                var prerequisiteId = prerequisite.node != null
                    ? prerequisite.node.id
                    : string.Empty;

                if (!string.IsNullOrEmpty(prerequisiteId) && pending.Contains(prerequisiteId))
                    return true;
            }

            return false;
        }

        private IEnumerator PlayConnectionRevealWave(List<string> wave)
        {
            var connections = new List<NodeConnectionView>();

            foreach (var nodeId in wave)
            {
                if (!_incomingConnectionsByNodeId.TryGetValue(nodeId, out var incomingConnections))
                    continue;

                foreach (var connection in incomingConnections)
                {
                    if (connection == null)
                        continue;

                    connection.PrepareReveal();
                    connections.Add(connection);
                }
            }

            if (connections.Count == 0)
                yield break;

            if (_connectionRevealDuration <= 0f)
            {
                foreach (var connection in connections)
                    connection.SetRevealProgress(1f);

                yield break;
            }

            var elapsed = 0f;
            while (elapsed < _connectionRevealDuration)
            {
                var t = Mathf.Clamp01(elapsed / _connectionRevealDuration);
                var easedT = Mathf.SmoothStep(0f, 1f, t);

                foreach (var connection in connections)
                    connection.SetRevealProgress(easedT);

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            foreach (var connection in connections)
                connection.SetRevealProgress(1f);
        }

        private static IEnumerator WaitUnscaled(float duration)
        {
            if (duration <= 0f)
                yield break;

            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        private void HandleNodeUpgraded(string nodeId)
        {
            if (string.IsNullOrEmpty(nodeId))
                return;

            if (_nodeViewsById.TryGetValue(nodeId, out var nodeView))
                nodeView.PlayLevelUpgrade(_service.GetLevel(nodeId));
        }

        private void OnDisable()
        {
            CompleteActiveRevealFromCachedStates();
        }

        private void OnDestroy()
        {
            _closeButton.onClick.RemoveAllListeners();

            if (_revealRoutine != null)
            {
                StopCoroutine(_revealRoutine);
                _revealRoutine = null;
            }

            if (_service != null)
            {
                _service.OnUpgraded -= RefreshAll;
                _service.OnNodeUpgraded -= HandleNodeUpgraded;
            }
            if (_player != null)
                _player.OnGoldChanged -= RefreshGold;
        }
    }
}
