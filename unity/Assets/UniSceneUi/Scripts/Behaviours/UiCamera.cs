using UnityEngine;

namespace UniSceneUi
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class UiCamera : MonoBehaviour
    {
        [SerializeField] Camera cam;

        public Camera Camera { get { return cam; } }

        void Awake()
        {
            if (Application.isPlaying)
            {
                DontDestroyOnLoad(gameObject);
            }

            cam = GetComponent<Camera>();

            SetCullingMaskAs("UI");
            cam.clearFlags = CameraClearFlags.Depth;
            cam.orthographic = true;
        }

        void SetCullingMaskAs(string layer)
        {
            var layerMask = 1 << LayerMask.NameToLayer(layer);
            cam.cullingMask = layerMask;
        }
    }
}
