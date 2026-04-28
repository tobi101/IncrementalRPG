using Core.StateMachine;
using Core.StateMachine.States;
using Reflex.Attributes;
using UnityEngine;

namespace UI
{
    public class HubView : MonoBehaviour
    {
        [SerializeField] private HubFeatureButtonView _dungeonButton;
        [SerializeField] private HubFeatureButtonView _skillTreeButton;
        [SerializeField] private HubFeatureButtonView _barracksButton;
        [SerializeField] private HubFeatureButtonView _mineButton;
        [SerializeField] private HubFeatureButtonView _craftButton;

        [Inject] private GameStateMachine _stateMachine;

        private void Start()
        {
            _dungeonButton.Button.onClick.AddListener(OpenDungeon);
            _skillTreeButton.Button.onClick.AddListener(OpenSkillTree);
            _barracksButton.Button.onClick.AddListener(OpenBarracks);
            _mineButton.Button.onClick.AddListener(OpenMine);
            _craftButton.Button.onClick.AddListener(OpenCraft);
        }

        private void OnDestroy()
        {
            if (_dungeonButton != null && _dungeonButton.Button != null)
                _dungeonButton.Button.onClick.RemoveListener(OpenDungeon);

            if (_skillTreeButton != null && _skillTreeButton.Button != null)
                _skillTreeButton.Button.onClick.RemoveListener(OpenSkillTree);

            if (_barracksButton != null && _barracksButton.Button != null)
                _barracksButton.Button.onClick.RemoveListener(OpenBarracks);

            if (_mineButton != null && _mineButton.Button != null)
                _mineButton.Button.onClick.RemoveListener(OpenMine);

            if (_craftButton != null && _craftButton.Button != null)
                _craftButton.Button.onClick.RemoveListener(OpenCraft);
        }

        private void OpenDungeon() => _stateMachine.Enter<GameplayState>();

        private void OpenSkillTree() => _stateMachine.Enter<SkillTreeMenuState>();

        private void OpenBarracks() => _stateMachine.Enter<BarracksState>();

        private void OpenMine() => _stateMachine.Enter<MineState>();

        private void OpenCraft() => _stateMachine.Enter<CraftState>();
    }
}
