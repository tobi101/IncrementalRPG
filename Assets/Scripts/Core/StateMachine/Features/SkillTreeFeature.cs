using Core.TestSkillTree.View;
using Reflex.Attributes;

namespace Core.StateMachine.Features
{
    public class SkillTreeFeature : IGameFeature
    {
        [Inject] private SkillTreeView _view;

        public void Initialize() => _view.Hide();

        public void Enable() => _view.Show();

        public void Disable() => _view.Hide();

        public void Tick(float deltaTime) { }
    }
}
