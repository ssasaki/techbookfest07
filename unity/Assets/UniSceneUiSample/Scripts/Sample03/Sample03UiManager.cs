using System;
using System.Collections.Generic;

namespace UniSceneUi.Sample
{
    public class Sample03UiManager : SceneUiManager
    {
        readonly Dictionary<Type, PrefabParam> prefabParams = new Dictionary<Type, PrefabParam>()
        {
            { typeof(Sample03aFirstUiController), new PrefabParam("UI/Sample03aFirstUi") },
            { typeof(CloseDialogController), new PrefabParam("UI/CloseDialog") },
            { typeof(CancelableDialogController), new PrefabParam("UI/CancelableDialog") },
        };

        protected override Dictionary<Type, PrefabParam> GetPrefabParamDic()
        {
            return prefabParams;
        }

        public static Sample03UiManager Impl
        {
            get
            {
                return GetOrInstantiate<Sample03UiManager>();
            }
        }
    }
}
