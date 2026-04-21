using System;
using System.Collections.Generic;
using System.Linq;
using Core.StateMachine.Features;
using Core.StateMachine.States;
using IncrementalRPG.Scripts.Reflex;
using Reflex.Attributes;

namespace Core.StateMachine
{
    public class GameStateMachine : IAwakeable, IStartable, ITickable
    {
        [Inject] private IEnumerable<IGameState> _statesEnumerable;
        [Inject] private IEnumerable<IGameFeature> _features;
        [Inject] private GameplayFeature _gameplayFeature;

        private Dictionary<Type, IGameState> _states;
        private IGameState _current;

        public void OnAwake()
        {
            _states = _statesEnumerable.ToDictionary(s => s.GetType());

            foreach (var feature in _features)
                feature.Initialize();

            ((GameplayState)_states[typeof(GameplayState)]).OnGoToHubRequested += () => Enter<HubState>();
        }

        public void OnStart() => Enter<HubState>();

        public void Tick(float deltaTime) => _current?.Tick(deltaTime);

        public void Enter<T>() where T : IGameState
        {
            _current?.Exit();
            _current = _states[typeof(T)];
            _current.Enter();
        }
    }
}
