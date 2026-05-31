using UnityEngine;
using UnityEngine.SceneManagement;

namespace Boot
{
    public class Loader : MonoBehaviour
    {
        private void Start()
        {
            Debug.Log("[Loader] Bootstrap started.");
            Debug.Log("[Loader] Loading MainMenuScene.");
            SceneManager.LoadScene("MainMenuScene");
        }
    }
}
