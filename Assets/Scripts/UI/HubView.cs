using Core.StateMachine;
using Core.StateMachine.States;
using Core.Gameplay.Dungeon;
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
        [SerializeField] private DungeonMenuView _dungeonMenuView;

        [Inject] private GameStateMachine _stateMachine;
        [Inject] private DungeonList _dungeonList;
        [Inject] private DungeonSelectionService _dungeonSelection;

        private void Start()
        {
            if (_dungeonButton != null && _dungeonButton.Button != null)
                _dungeonButton.Button.onClick.AddListener(OpenDungeon);

            if (_skillTreeButton != null && _skillTreeButton.Button != null)
                _skillTreeButton.Button.onClick.AddListener(OpenSkillTree);

            if (_barracksButton != null && _barracksButton.Button != null)
                _barracksButton.Button.onClick.AddListener(OpenBarracks);

            if (_mineButton != null && _mineButton.Button != null)
                _mineButton.Button.onClick.AddListener(OpenMine);

            if (_craftButton != null && _craftButton.Button != null)
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

        private void OpenDungeon()
        {
            if (_dungeonMenuView != null)
            {
                _dungeonMenuView.Show(_dungeonList, StartDungeon);
                return;
            }

            StartDungeon(_dungeonList.GetFirstPlayable());
        }

        private void StartDungeon(DungeonConfig dungeon)
        {
            if (dungeon == null || !dungeon.HasPlayableLevels)
            {
                Debug.LogError("[HubView] Cannot start gameplay because selected dungeon is not playable.");
                return;
            }

            _dungeonSelection.Select(dungeon);
            _stateMachine.Enter<GameplayState>();
        }

        private void OpenSkillTree() => _stateMachine.Enter<SkillTreeMenuState>();

        private void OpenBarracks() => _stateMachine.Enter<BarracksState>();

        private void OpenMine() => _stateMachine.Enter<MineState>();

        private void OpenCraft() => _stateMachine.Enter<CraftState>();
    }
}
