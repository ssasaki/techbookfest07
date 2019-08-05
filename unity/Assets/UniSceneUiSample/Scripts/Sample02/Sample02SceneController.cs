using UniRx;
using UnityEngine;

namespace UniSceneUi.Sample
{
    public class Sample02SceneController : MonoBehaviour
    {
        void Start()
        {
            Sample02UiManager.Impl.StackPage<Sample02FirstUiController>()
                .Subscribe()
                .AddTo(this);
        }
    }
}