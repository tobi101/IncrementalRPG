using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Boot
{
    public class Loader : MonoBehaviour
    {
        private IEnumerator Start()
        {
            yield return  null;
            
            Debug.Log("All app services are initialized.");
            SceneManager.LoadSceneAsync("MainMenuScene");
        }
    }
}