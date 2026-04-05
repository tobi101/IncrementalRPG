using Core.TestSkillTree.View;
using Reflex.Attributes;

namespace Core.StateMachine.Features
{
    public class SkillTreeFeature : IGameFeature
    {
        [Inject] private SkillTreeView _view;

        public void Initialize() => _view.gameObject.SetActive(false);

        public void Enable() => _view.gameObject.SetActive(true);

        public void Disable() => _view.gameObject.SetActive(false);

        public void Tick(float deltaTime) { }
    }
}
