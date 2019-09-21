using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

namespace UniSceneUi
{
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(Animator))]
    public class AnimationPanel : MonoBehaviour
    {
        [SerializeField] float animationSpeed = 1f;

        enum AnimationPanelState
        {
            Initial = 0,
            Show = 1,
            Active = 2,
            Hide = 3,
            Hidden = 4,
            Unknown = 99
        }

        readonly string showStateName = "Show";
        readonly string activeStateName = "Active";
        readonly string hideStateName = "Hide";
        readonly string hiddenStateName = "Hidden";
        readonly ReactiveProperty<AnimationPanelState> currentState = new ReactiveProperty<AnimationPanelState>(AnimationPanelState.Initial);

        CanvasGroup CanvasGroup
        {
            get { return GetComponent<CanvasGroup>(); }
        }

        Animator PanelAnimator
        {
            get { return GetComponent<Animator>(); }
        }

        IEnumerable<Animator> AllAnimators
        {
            get { return (new Animator[] { PanelAnimator }); }
        }

        void Start()
        {
            // 既にアニメーションが開始していたら何もしない
            if (currentState.Value != AnimationPanelState.Initial) return;

            // 一時停止状態で初期化
            CanvasGroup.interactable = false;
            CanvasGroup.blocksRaycasts = false;
            AllAnimators.ForEach(a =>
            {
                a.speed = 0f;
                a.Play(showStateName);
            });
        }

        public IObservable<Unit> ObservableShow()
        {
            var obs = Observable.ReturnUnit();

            // 閉じるアニメーション中なら完了まで待つ
            obs = obs.ContinueWith(_ => currentState.SkipWhile(v => v == AnimationPanelState.Hide).Take(1).AsUnitObservable());

            // 開くアニメーションが開始していなければ開始する
            obs = obs.Do(_ =>
            {
                if (currentState.Value != AnimationPanelState.Show && currentState.Value != AnimationPanelState.Active)
                {
                    CanvasGroup.blocksRaycasts = true;
                    CanvasGroup.interactable = true;
                    PlayAnimators(AnimationPanelState.Show);
                }
                else if (!gameObject.activeSelf)
                {
                    Debug.LogWarning("state:{0}のまま, gameObjectが非アクティブになっているためアニメーションを再生できません".Formats(currentState));
                }
            });

            // 開くアニメーションが完了するまで待つ
            obs = obs.ContinueWith(_ => WaitAnimators(AnimationPanelState.Active));

            // 開くアニメーションが完了した
            obs = obs.Do(_ => { currentState.Value = AnimationPanelState.Active; });

            return obs;
        }

        public IObservable<Unit> ObservableHide()
        {
            var obs = Observable.ReturnUnit();

            // 開くアニメーション中なら完了まで待つ
            obs = obs.ContinueWith(_ => currentState.SkipWhile(v => v == AnimationPanelState.Show).Take(1).AsUnitObservable());

            // 閉じるアニメーションが開始していなければ開始する
            obs = obs.Do(_ =>
            {
                if (currentState.Value != AnimationPanelState.Hide && currentState.Value != AnimationPanelState.Hidden)
                {
                    CanvasGroup.interactable = false;
                    CanvasGroup.blocksRaycasts = false;
                    PlayAnimators(AnimationPanelState.Hide);
                }
            });

            // 閉じるアニメーションが完了するまで待つ
            obs = obs.ContinueWith(_ => WaitAnimators(AnimationPanelState.Hidden));

            // 閉じるアニメーションが完了した
            obs = obs.Do(_ =>
                {
                    CanvasGroup.blocksRaycasts = false;
                    gameObject.SetActive(false);
                })
                .DelayFrame(1) // Hidden になった瞬間に Show が走る可能性があるため, SetActive が重ならないように 1 フレーム開ける
                .Do(_ => { currentState.Value = AnimationPanelState.Hidden; });

            return obs;
        }

        void PlayAnimators(AnimationPanelState targetState)
        {
            currentState.Value = targetState;

            // Hide や Hidden の場合は SetActive(false) されるため true にするのは Show と Active のみ
            if (targetState == AnimationPanelState.Show || targetState == AnimationPanelState.Active)
            {
                gameObject.SetActive(true);
            }

            foreach (var animator in AllAnimators)
            {
                animator.speed = animationSpeed;
                string stateName = null;
                string fallbackStateName = null;
                switch (targetState)
                {
                    case AnimationPanelState.Show:
                        stateName = showStateName;
                        break;
                    case AnimationPanelState.Active:
                        stateName = activeStateName;
                        fallbackStateName = showStateName;
                        break;
                    case AnimationPanelState.Hide:
                        stateName = hideStateName;
                        break;
                    case AnimationPanelState.Hidden:
                        stateName = hiddenStateName;
                        fallbackStateName = hideStateName;
                        break;
                }

                if (!stateName.IsNullOrEmpty() && animator.HasState(0, Animator.StringToHash(stateName)))
                {
                    animator.Play(stateName, -1, 0f);
                }
                else
                {
                    animator.Play(fallbackStateName, -1, 1f);
                }
            }
        }

        IObservable<UniRx.Unit> WaitAnimators(AnimationPanelState targetState)
        {
            return Observable.Zip(AllAnimators.Select(animator =>
            {
                return Observable.EveryUpdate().SkipWhile(_ =>
                {
                    var state = GuessAnimatorState(animator);                    
                    if (state == AnimationPanelState.Unknown)
                    {
                        Debug.LogWarning("AnimationPanelState が Unknown です。遷移しようとしたState:{0}".Formats(targetState));
                        return false;
                    }

                    return state != targetState;
                })
                .TakeUntilDestroy(this)
                .Take(1);
            })).AsUnitObservable();
        }

        AnimationPanelState GuessAnimatorState(Animator animator)
        {
            // 非アクティブ状態でプレハブに刺さっていたなどの理由で Animator は初期化されていないかもしれないため、その場合は Hidden を返す。
            if (!animator.gameObject.activeInHierarchy)
            {
                return AnimationPanelState.Hidden;
            }

            var state = animator.GetCurrentAnimatorStateInfo(0);
            if (state.IsName(hiddenStateName))
            {
                return AnimationPanelState.Hidden;
            }
            else if (state.IsName(hideStateName))
            {
                // 再生完了チェック
                return state.normalizedTime < 1f
                    ? AnimationPanelState.Hide
                    : AnimationPanelState.Hidden;
            }
            else if (state.IsName(activeStateName))
            {
                return AnimationPanelState.Active;
            }
            else if (state.IsName(showStateName))
            {
                // 再生完了チェック
                return state.normalizedTime < 1f
                    ? AnimationPanelState.Show
                    : AnimationPanelState.Active;
            }

            // ここまで来たら Animator の設定がおかしいけど、フリーズしないようにしておく
            Debug.LogWarning("この AnimationPanel に紐づく", gameObject);
            Debug.LogWarning("この Animator の設定がおかしいようです", animator);
            return AnimationPanelState.Unknown;
        }
    }
}