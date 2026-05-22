using Core.StateMachine.Features;
using Reflex.Attributes;

namespace Core.StateMachine.States
{
    public class SkillTreeMenuState : IGameState
    {
        [Inject] private SkillTreeFeature _skillTree;

        public void Enter() => _skillTree.Enable();

        public void Exit(GameStateExitReason reason) => _skillTree.Disable();

        public void Tick(float deltaTime) => _skillTree.Tick(deltaTime);
    }
}
