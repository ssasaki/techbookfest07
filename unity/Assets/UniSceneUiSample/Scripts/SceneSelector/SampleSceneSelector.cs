using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace UniSceneUi.Sample
{
    public class SampleSceneSelector : MonoBehaviour
    {
        [SerializeField] Button nextSceneButton = null;

        void Awake()
        {
            
            nextSceneButton.OnClickAsObservable()
                .Take(1)
                .Do(_ => SceneManager.LoadScene(GetNextSceneName()))
                .Subscribe()
                .AddTo(this);
        }
        
        string GetNextSceneName()
        {
            var currentScene = SceneManager.GetActiveScene();
            var sceneCount = SceneManager.sceneCountInBuildSettings;

            var nextSceneIndex = (currentScene.buildIndex + 1) % sceneCount;
            var scenePath = SceneUtility.GetScenePathByBuildIndex(nextSceneIndex);
            var sceneNameStart = scenePath.LastIndexOf("/", StringComparison.Ordinal) + 1;
            var sceneNameEnd = scenePath.LastIndexOf(".", StringComparison.Ordinal);
            var sceneNameLength = sceneNameEnd - sceneNameStart;
            
            return scenePath.Substring(sceneNameStart, sceneNameLength);
        }
    }
}