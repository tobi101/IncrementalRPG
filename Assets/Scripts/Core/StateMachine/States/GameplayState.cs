using Core.StateMachine.Features;
using Reflex.Attributes;
using UI;

namespace Core.StateMachine.States
{
    public class GameplayState : IGameState
    {
        [Inject] private GameplayFeature _gameplay;
        [Inject] private MenuCanvasView _menuCanvas;
        [Inject] private HudView _hudView;

        public void Enter()
        {
            _menuCanvas.gameObject.SetActive(false);
            _hudView.gameObject.SetActive(true);
            _gameplay.Enable();
        }

        public void Exit()
        {
            _gameplay.Disable();
            _hudView.gameObject.SetActive(false);
            _menuCanvas.gameObject.SetActive(true);
        }

        public void Tick(float deltaTime) => _gameplay.Tick(deltaTime);
    }
}
