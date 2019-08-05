using UnityEngine;

namespace UniSceneUi
{
    /// <summary>
    /// Prefab を Instantiate するときに使うパラメータを定義するためのクラス
    /// </summary>
    public class PrefabParam
    {
        public readonly string PrefabPath;

        public PrefabParam(string prefabPath)
        {
            PrefabPath = prefabPath;
        }
    }

    public static class UniSceneUiUtil
    {
        public static T Instantiate<T>(Transform parent, PrefabParam param) where T : Component
        {
            var prefab = Resources.Load(param.PrefabPath) as GameObject;
            prefab.SetActive(false);
            var go = GameObject.Instantiate(prefab);
            go.transform.SetParent(parent, false);
            return go.GetComponent<T>();
        }
    }
}