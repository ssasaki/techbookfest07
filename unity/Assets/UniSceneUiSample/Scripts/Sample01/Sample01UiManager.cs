using System;
using System.Collections.Generic;

namespace UniSceneUi.Sample
{
    public class Sample01UiManager : SceneUiManager
    {
        readonly Dictionary<Type, PrefabParam> prefabParams = new Dictionary<Type, PrefabParam>()
        {
            { typeof(Sample01FirstUiController), new PrefabParam("UI/Sample01FirstUi") },
            { typeof(CloseDialogController), new PrefabParam("UI/CloseDialog") },
        };

        protected override Dictionary<Type, PrefabParam> GetPrefabParamDic()
        {
            return prefabParams;
        }

        public static Sample01UiManager Impl
        {
            get
            {
                return GetOrInstantiate<Sample01UiManager>();
            }
        }
    }
}
