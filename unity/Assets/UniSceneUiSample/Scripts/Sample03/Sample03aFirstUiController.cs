using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace UniSceneUi.Sample
{
    public class Sample03aFirstUiController : UiBase
    {
        [SerializeField] Button showCancelableDialogButton = null;

        void Start()
        {
            showCancelableDialogButton.OnClickAsObservable()
                .Take(1)
                // 確認ダイアログを表示し
                .ContinueWith(_ =>
                {
                    var ui = uiManager.Rent<CancelableDialogController>();
                    ui.SetParams(message: "Confirm?");
                    return uiManager
                        .StackPage<CancelableDialogController>()
                        .ContinueWith(__ => ui.WaitUntilExplicitCompletion());
                })
                // 処理を実行し
                .ContinueWith(_ => DoSomethingAsObservable())
                // 処理後に完了を知らせるダイアログを表示する
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

        // NOTE: 背景として使う UI など、非表示にしたくない場合はコメントアウトを外して Hide 処理を上書きする
        // protected override IObservable<Unit> StartHideAnimation()
        // {
        //     return Observable.ReturnUnit();
        // }
    }
}