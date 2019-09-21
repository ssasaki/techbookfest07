using UniRx;
using UnityEngine;

namespace UniSceneUi.Sample
{
    public class Sample03aSceneController : MonoBehaviour
    {
        void Start()
        {
            Sample03UiManager.Impl.StackPage<Sample03aFirstUiController>()
                .Subscribe()
                .AddTo(this);
        }
    }
}