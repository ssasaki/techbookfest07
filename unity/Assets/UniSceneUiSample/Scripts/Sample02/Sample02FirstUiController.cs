using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace UniSceneUi.Sample
{
    public class Sample02FirstUiController : UiBase
    {
        [SerializeField] Button showCancelableDialogButton = null;

        void Start()
        {
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
                .Do(_ => Debug.Log("OnNextが発行された"))
                .DoOnCompleted(() => Debug.Log("OnCompleteが発行された"))
                .ContinueWith(_ => DoSomethingAsObservable())
                .RepeatUntilDestroy(this)
                .Subscribe()
                .AddTo(this);
        }

        IObservable<UniRx.Unit> DoSomethingAsObservable()
        {
            return Observable.Create<UniRx.Unit>(observer =>
            {
                Debug.Log("1つ前のContinueWithでOnNextとOnCompleteの両方が発行されたらここの処理までくる");
                observer.OnNext(UniRx.Unit.Default);
                observer.OnCompleted();
                return Disposable.Empty;
            });
        }
    }
}