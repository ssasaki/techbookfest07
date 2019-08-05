using System;
using UniRx;
using UnityEngine;

namespace UniSceneUi
{
    public class UiBase : MonoBehaviour, IUiBase
    {
        protected SceneUiManager uiManager { get; private set; }

        void IUiBase.SetSceneUiManager(SceneUiManager uiManager)
        {
            this.uiManager = uiManager;
        }

        IObservable<Unit> IUiBase.Reload()
        {
            return Reload();
        }
    
        IObservable<Unit> IUiBase.StartShowAnimation()
        {
            return StartShowAnimation();
        }

        IObservable<Unit> IUiBase.StartHideAnimation()
        {
            return StartHideAnimation();
        }
        
        protected virtual IObservable<Unit> Reload()
        {
            return Observable.ReturnUnit();
        }
        
        protected virtual IObservable<Unit> StartShowAnimation()
        {
            return GetComponent<AnimationPanel>().ObservableShow();
        }

        protected virtual IObservable<Unit> StartHideAnimation()
        {
            return GetComponent<AnimationPanel>().ObservableHide();
        }
    }
}