using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace Akila.FPSFramework
{
    [AddComponentMenu("Akila/FPS Framework/UI/MainMenu")]
    public class MainMenu : MonoBehaviour
    {
        public void LoadGame(string name)
        {
            LoadingScreen.LoadScene(name);
        }

        public void OpenAssetPage()
        {
            Application.OpenURL("https://assetstore.unity.com/packages/templates/systems/fps-framework-217379");
        }

        public void OpenDocs()
        {
            Application.OpenURL("https://akila.gitbook.io/fps-framework");
        }

        public void Quit()
        {
            Application.Quit();
        }
    }
}