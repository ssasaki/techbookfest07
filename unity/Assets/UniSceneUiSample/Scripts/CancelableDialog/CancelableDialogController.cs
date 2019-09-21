using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace UniSceneUi.Sample
{
    public enum CancelableDialogResult
    {
        Confirmed,
        Cancelled,
    }

    public class CancelableDialogController : UiBase
    {
        [SerializeField] Button confirmButton = null;
        [SerializeField] Button canelButton = null;
        [SerializeField] Text messageText = null;

        readonly Subject<Unit> onConfirm = new Subject<Unit>();
        readonly Subject<Unit> onCancel = new Subject<Unit>();

        void Awake()
        {
            confirmButton.OnClickAsObservable()
                .Take(1)
                .ContinueWith(_ => uiManager.PopPage<CancelableDialogController>())
                .DoOnCompleted(() =>
                {
                    onConfirm.OnNext(Unit.Default);
                })
                .RepeatUntilDestroy(this)
                .Subscribe()
                .AddTo(this);

            canelButton.OnClickAsObservable()
                .Take(1)
                .ContinueWith(_ => uiManager.PopPage<CancelableDialogController>())
                .DoOnCompleted(() =>
                {
                    onCancel.OnNext(Unit.Default);
                })
                .RepeatUntilDestroy(this)
                .Subscribe()
                .AddTo(this);
        }

        public void SetParams(string message)
        {
            this.messageText.text = message;
        }

        public IObservable<CancelableDialogResult> WaitUntilExplicitCompletion()
        {
            return Observable.Create<CancelableDialogResult>(observer =>
            {                
                var disposable = new CompositeDisposable();
                
                onConfirm.Subscribe(_ =>
                {
                    observer.OnNext(CancelableDialogResult.Confirmed);
                    observer.OnCompleted();
                })
                .AddTo(disposable)
                .AddTo(this);
            
                onCancel.Subscribe(_ =>
                {
                    // NOTE:
                    // 今回は ContinueWith の挙動を見せるために OnCompleted だけを流してストリームを中断させているが、
                    // OnNext で CancelableDialogResult.Cancelled を流して、購読している側で処理を分岐させてもよい
                    observer.OnCompleted();
                })
                .AddTo(disposable)
                .AddTo(this);
                
                return disposable;
            });
        }
    }
}