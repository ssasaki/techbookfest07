using UniRx;
using UnityEngine;

namespace UniSceneUi.Sample
{
    public class Sample01SceneController : MonoBehaviour
    {
        void Start()
        {
            Sample01UiManager.Impl.StackPage<Sample01FirstUiController>()
                .Subscribe()
                .AddTo(this);
        }
    }
}