using UnityEditor;
using UnityEngine;
using Newtonsoft.Json.Linq;

public static class ProbeSettings
{
    public static string Execute()
    {
        var rp = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
        string rpName = rp ? rp.name + " / " + rp.GetType().Name : "Builtin";
        var so = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
        var p = so.FindProperty("activeInputHandler");
        string renderer = "";
        if (rp != null)
        {
            var rso = new SerializedObject(rp);
            var list = rso.FindProperty("m_RendererDataList");
            if (list != null)
                for (int i = 0; i < list.arraySize; i++)
                {
                    var r = list.GetArrayElementAtIndex(i).objectReferenceValue;
                    renderer += (r ? r.name + ":" + r.GetType().Name : "null") + "; ";
                }
        }
        return rpName + " | input=" + (p != null ? p.intValue.ToString() : "?") + " | renderers=" + renderer + " | unity=" + Application.unityVersion;
    }
}
