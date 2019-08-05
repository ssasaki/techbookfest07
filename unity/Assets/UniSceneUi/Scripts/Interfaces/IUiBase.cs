using System;
using UniRx;

namespace UniSceneUi
{
    public interface IUiBase
    {
        void SetSceneUiManager(SceneUiManager uiManager);
        IObservable<Unit> Reload();
        IObservable<Unit> StartShowAnimation();
        IObservable<Unit> StartHideAnimation();
    }
}