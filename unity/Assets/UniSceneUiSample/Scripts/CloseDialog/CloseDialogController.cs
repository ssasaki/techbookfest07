using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace UniSceneUi.Sample
{
    public class CloseDialogController : UiBase
    {
        [SerializeField] Button closeButton = null;
        [SerializeField] Text messageText = null;

        void Awake()
        {
            closeButton.OnClickAsObservable()
                .Take(1)
                .ContinueWith(_ => uiManager.PopPage<CloseDialogController>())
                .RepeatUntilDestroy(this)
                .Subscribe()
                .AddTo(this);
        }

        public void SetParams(string message)
        {
            this.messageText.text = message;
        }
    }
}