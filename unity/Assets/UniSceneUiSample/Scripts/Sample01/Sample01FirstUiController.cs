using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace UniSceneUi.Sample
{
    public class Sample01FirstUiController : UiBase
    {
        [SerializeField] Button showCloseDialogButton = null;

        void Awake()
        {
            showCloseDialogButton.OnClickAsObservable()
                .Take(1)
                .ContinueWith(_ =>
                {
                    var ui = uiManager.Rent<CloseDialogController>();
                    var text = "Open by\n{0}.".Formats(GetType().Name);
                    ui.SetParams(text);
                    return uiManager.StackPage<CloseDialogController>();
                })
                .RepeatUntilDestroy(this)
                .Subscribe()
                .AddTo(this);
        }
    }
}