using Core.StateMachine.Features;
using Reflex.Attributes;
using UI;

namespace Core.StateMachine.States
{
    public class GameplayState : IGameState
    {
        [Inject] private GameplayFeature _gameplay;
        [Inject] private MenuCanvasView _menuCanvas;

        public void Enter()
        {
            _menuCanvas.gameObject.SetActive(false);
            _gameplay.Enable();
        }

        public void Exit()
        {
            _gameplay.Disable();
            _menuCanvas.gameObject.SetActive(true);
        }

        public void Tick(float deltaTime) => _gameplay.Tick(deltaTime);
    }
}
