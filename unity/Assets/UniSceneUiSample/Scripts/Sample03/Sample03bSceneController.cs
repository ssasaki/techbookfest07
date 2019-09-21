using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace UniSceneUi.Sample
{
    public class Sample03bSceneController : MonoBehaviour
    {
        [SerializeField] Button showCancelableDialogButton = null;

        void Start()
        {
            var uiManager = Sample03UiManager.Impl;

            showCancelableDialogButton.OnClickAsObservable()
                .Take(1)
                .ContinueWith(_ =>
                {
                    var ui = uiManager.Rent<CancelableDialogController>();
                    ui.SetParams(message: "Confirm?");
                    return uiManager
                        .StackPage<CancelableDialogController>()
                        .ContinueWith(__ => ui.WaitUntilExplicitCompletion());
                })
                .ContinueWith(_ => DoSomethingAsObservable())
                .ContinueWith(_ =>
                {
                    var ui = uiManager.Rent<CloseDialogController>();
                    ui.SetParams(message: "Done.");
                    return uiManager.StackPage<CloseDialogController>();
                })
                .RepeatUntilDestroy(this)
                .Subscribe()
                .AddTo(this);
        }

        IObservable<UniRx.Unit> DoSomethingAsObservable()
        {
            return Observable.Create<UniRx.Unit>(observer =>
            {
                observer.OnNext(UniRx.Unit.Default);
                observer.OnCompleted();
                return Disposable.Empty;
            });
        }
    }
}