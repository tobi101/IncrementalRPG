using Core.StateMachine;
using Core.StateMachine.States;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class HubView : MonoBehaviour
    {
        [SerializeField] private Button _dungeonButton;
        [SerializeField] private Button _skillTreeButton;
        [SerializeField] private Button _barracksButton;
        [SerializeField] private Button _mineButton;
        [SerializeField] private Button _craftButton;

        [Inject] private GameStateMachine _stateMachine;

        private void Start()
        {
            _dungeonButton.onClick.AddListener(() => _stateMachine.Enter<GameplayState>());
            _skillTreeButton.onClick.AddListener(() => _stateMachine.Enter<SkillTreeMenuState>());
            _barracksButton.onClick.AddListener(() => _stateMachine.Enter<BarracksState>());
            _mineButton.onClick.AddListener(() => _stateMachine.Enter<MineState>());
            _craftButton.onClick.AddListener(() => _stateMachine.Enter<CraftState>());
        }

        private void OnDestroy()
        {
            _dungeonButton.onClick.RemoveAllListeners();
            _skillTreeButton.onClick.RemoveAllListeners();
            _barracksButton.onClick.RemoveAllListeners();
            _mineButton.onClick.RemoveAllListeners();
            _craftButton.onClick.RemoveAllListeners();
        }
    }
}
