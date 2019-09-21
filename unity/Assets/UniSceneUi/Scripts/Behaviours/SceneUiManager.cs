using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

namespace UniSceneUi
{
    public abstract class SceneUiManager : MonoBehaviour
    {
        #region static

        static GameObject go;

        protected static M GetOrInstantiate<M>() where M : Component
        {
            if (go != null)
                return go.GetComponent<M>();

            var goName = string.Format("SceneUiManager({0})", typeof(M).Name);
            var prefab = Resources.Load("UiManager/SceneUiManager");
            go = Instantiate(prefab) as GameObject;
            go.name = goName;
            return go.AddComponent<M>();
        }

        #endregion

        readonly Dictionary<Type, IUiBase> uiBaseDic = new Dictionary<Type, IUiBase>();
        readonly Stack<DecoratedUiBase> decoratedUiBaseStack = new Stack<DecoratedUiBase>();

        protected abstract Dictionary<Type, PrefabParam> GetPrefabParamDic();

        void Awake()
        {
            go = this.gameObject;
        }

        public Type GetPeekPageId()
        {
            if (decoratedUiBaseStack.Count == 0)
                return null;

            return decoratedUiBaseStack.Peek().PageId;
        }

        public IObservable<UniRx.Unit> PopAll()
        {
            if (!decoratedUiBaseStack.Any())
            {
                // ポップするものがない
                return Observable.ReturnUnit();
            }

            var popedPage = decoratedUiBaseStack.Pop();

            decoratedUiBaseStack.Clear();

            return HidePage(popedPage.PageId);
        }

        public T Rent<T>() where T : Component, IUiBase
        {
            var pageIdToRent = typeof(T);

            if (!uiBaseDic.ContainsKey(pageIdToRent))
            {
                Register<T>();
            }

            return uiBaseDic[pageIdToRent] as T;
        }

        T Register<T>() where T : Component, IUiBase
        {
            var pageIdToRegister = typeof(T);
            var param = GetPrefabParamDic()[pageIdToRegister];
            var component = UniSceneUiUtil.Instantiate<T>(this.transform, param);

            component.SetSceneUiManager(this);

            if (uiBaseDic.ContainsKey(pageIdToRegister))
            {
                throw new Exception(string.Format("既に登録されている UIPage:{0} name:{1}", pageIdToRegister, component.name));
            }
            else
            {
                uiBaseDic.Add(pageIdToRegister, component);
            }

            return component;
        }

        public IObservable<UniRx.Unit> StackPage<T>(bool isOverlay = false)
            where T : Component, IUiBase
        {
            var pageIdToStack = typeof(T);

            // 未登録の場合は登録する
            if (!uiBaseDic.ContainsKey(pageIdToStack))
            {
                Register<T>();
            }

            // まだ何も表示されていないか Overlay の場合は Hide を省略する
            if (!decoratedUiBaseStack.Any() || isOverlay)
            {
                decoratedUiBaseStack.Push(new DecoratedUiBase(pageIdToStack, isOverlay));
                return ReloadAndShowPage(pageIdToStack)
                    .DoOnError(ex => Debug.LogErrorFormat("message:{0} trace:{1}", ex.Message, ex.StackTrace));
            }

            var pageToHide = decoratedUiBaseStack.Peek();

            // 既に表示している場合は Reload のみ行う
            if (pageToHide.PageId == pageIdToStack)
            {
                return ReloadPage(pageIdToStack).AsUnitObservable()
                    .DoOnError(ex => Debug.LogErrorFormat("message:{0} trace:{1}", ex.Message, ex.StackTrace));
            }

            // 表示されているものを閉じてから次を表示する
            decoratedUiBaseStack.Push(new DecoratedUiBase(pageIdToStack, isOverlay));
            return HideReloadAndShowPage(pageToHide.PageId, pageIdToStack)
                .DoOnError(ex => Debug.LogErrorFormat("message:{0} trace:{1}", ex.Message, ex.StackTrace));
        }

        public IObservable<UniRx.Unit> PopPage<T>()
            where T : Component, IUiBase
        {
            if (!decoratedUiBaseStack.Any())
            {
                // ポップするものがない
                return Observable.ReturnUnit();
            }

            var pageIdToPop = typeof(T);
            var popedPage = decoratedUiBaseStack.Pop();

            if (popedPage.PageId != pageIdToPop)
            {
                throw new Exception(string.Format("意図しない UIPage が閉じられた popedPageId:{0} pageToPop:{1}", popedPage.PageId, pageIdToPop));
            }

            if (popedPage.IsOverlay)
            {
                return HidePage(popedPage.PageId)
                    .DoOnError(ex => Debug.LogErrorFormat("message:{0} trace:{1}", ex.Message, ex.StackTrace));
            }

            if (!decoratedUiBaseStack.Any())
            {
                // 全ての UIPage が閉じられたため表示するものがない。閉じるだけ
                return HidePage(popedPage.PageId)
                    .DoOnError(ex => Debug.LogErrorFormat("message:{0} trace:{1}", ex.Message, ex.StackTrace));
            }
            else
            {
                return HideReloadAndShowPage(popedPage.PageId, decoratedUiBaseStack.Peek().PageId)
                    .DoOnError(ex => Debug.LogErrorFormat("message:{0} trace:{1}", ex.Message, ex.StackTrace));
            }
        }

        /// <summary>
        /// 指定した Page を Hide する
        /// </summary>
        IObservable<UniRx.Unit> HidePage(Type pageIdToHide)
        {
            return ObservableHidePage(pageIdToHide);
        }

        /// <summary>
        /// Hide と 次に表示する IUiBase の Reload を並行で進めて Show を行う
        /// </summary>
        IObservable<UniRx.Unit> HideReloadAndShowPage(Type pageIdToHide, Type pageIdToShow)
        {
            return Observable
                .Zip(ReloadPage(pageIdToShow).AsUnitObservable(), ObservableHidePage(pageIdToHide))
                .DelayFrame(1)
                .ContinueWith(_ => ObservableShowPage(pageIdToShow));
        }

        /// <summary>
        /// IUiBase を Reload してから Show する
        /// </summary>
        IObservable<UniRx.Unit> ReloadAndShowPage(Type pageIdToShow)
        {
            return ReloadPage(pageIdToShow)
                .DelayFrame(1)
                .ContinueWith(_ => ObservableShowPage(pageIdToShow));
        }

        IObservable<UniRx.Unit> ObservableShowPage(Type uiPageId)
        {
            return ReloadPage(uiPageId).ContinueWith(uiBase => uiBase.StartShowAnimation());
        }

        IObservable<UniRx.Unit> ObservableHidePage(Type uiPageId)
        {
            return ReloadPage(uiPageId).ContinueWith(uiBase => uiBase.StartHideAnimation());
        }

        IObservable<IUiBase> ReloadPage(Type uiPageId)
        {
            return uiBaseDic[uiPageId].Reload().Select(_ => uiBaseDic[uiPageId]);
        }

        #region internal class

        /// <summary>
        /// UiBase をデコレートするためのクラス
        /// </summary>
        class DecoratedUiBase
        {
            public readonly Type PageId;
            public readonly bool IsOverlay;

            public DecoratedUiBase(Type pageId, bool isOverlay)
            {
                PageId = pageId;
                IsOverlay = isOverlay;
            }
        }

        #endregion
    }
}
