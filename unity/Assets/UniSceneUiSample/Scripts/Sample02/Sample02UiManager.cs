using System;
using System.Collections.Generic;

namespace UniSceneUi.Sample
{
    public class Sample02UiManager : SceneUiManager
    {
        readonly Dictionary<Type, PrefabParam> prefabParams = new Dictionary<Type, PrefabParam>()
        {
            { typeof(Sample02FirstUiController), new PrefabParam("UI/Sample02FirstUi") },
            { typeof(CancelableDialogController), new PrefabParam("UI/CancelableDialog") },
        };

        protected override Dictionary<Type, PrefabParam> GetPrefabParamDic()
        {
            return prefabParams;
        }

        public static Sample02UiManager Impl
        {
            get
            {
                return GetOrInstantiate<Sample02UiManager>();
            }
        }
    }
}
