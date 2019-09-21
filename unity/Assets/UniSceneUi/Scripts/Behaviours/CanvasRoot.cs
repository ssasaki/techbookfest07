using UnityEngine;
using UnityEngine.UI;

namespace UniSceneUi
{
    /// <summary>
    /// Canvas のセットアップを行うコンポーネント
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(Canvas), typeof(CanvasScaler))]
    public class CanvasRoot : MonoBehaviour
    {
        // ゲームの縦横解像度(基準解像度)
        [SerializeField] Vector2 referenceResolution = new Vector2(1334, 750);

        float ReferenceAspectRatio { get { return referenceResolution.x / referenceResolution.y; } }
        float ScreenAspectRatio { get { return (float)Screen.width / Screen.height; } }
        
        // 画面のアスペクト比が想定よりも横長の場合は 1(MatchHeight) を返し、縦長の場合は 0(MatchWidth) を返す
        float MatchWidthOrHeight { get { return (ReferenceAspectRatio - 0.01f < ScreenAspectRatio) ? 1f : 0f; } }

        void Start()
        {
            var scaler = GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = referenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = MatchWidthOrHeight;
            scaler.referencePixelsPerUnit = 100;

            var canvas = GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;

            var uiCamera = FindOrCreateUiCamera();
            if (canvas.worldCamera != uiCamera.Camera)
            {
                canvas.worldCamera = uiCamera.Camera;
            }
        }
        
        UiCamera FindOrCreateUiCamera()
        {
            // NOTE: UiCamera は DontDestory なため一度生成したら同じものを使う
            var go = GameObject.Find("UiCamera");
            if (go != null)
            {
                return go.GetComponent<UiCamera>();
            }
            else
            {
                go = Instantiate(Resources.Load("Camera/UiCamera") as GameObject);
                go.name = "UiCamera";
                return go.GetComponent<UiCamera>();
            }
        }
    }
}
