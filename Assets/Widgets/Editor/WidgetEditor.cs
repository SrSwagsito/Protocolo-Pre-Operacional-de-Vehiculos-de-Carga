using UnityEngine;
using UnityEditor;

namespace eeGames.Widget
{

    [CustomEditor(typeof(Widget), true), CanEditMultipleObjects]
    public class WidgetEditor : Editor
    {
        private Widget Target { get { return (Widget)target; } }
        //    private AnimBool networkingFoldout;
        private bool m_tweens;
        private GUIStyle m_editorStyle;


        private void OnEnable()
        {
            //        networkingFoldout = new AnimBool(true);

            m_editorStyle = new GUIStyle();
            m_editorStyle.normal.textColor = Color.white;
            m_editorStyle.fontStyle = FontStyle.Bold;
            m_editorStyle.alignment = TextAnchor.UpperCenter;

        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update(); // Begin tracking changes

            DrawDefaultInspector();

            EditorGUILayout.Space();
            EditorGUILayout.Separator();
            EditorGUILayout.Space();

            //       Rect area = GUILayoutUtility.GetRect(0.0f, 30.0f, GUILayout.ExpandWidth(true));

            if (m_tweens == false)
            {
                GUI.color = Color.cyan;

                EditorGUILayout.HelpBox("Add WidgetTween Component For Tweens related Stuff by pressing Button Below", MessageType.Info, true);
                Rect area2 = GUILayoutUtility.GetRect(30f, 20.0f, GUILayout.ExpandWidth(true));
                if (GUI.Button(area2, "Add Tween"))
                {
                    Target.gameObject.AddComponent<WidgetTween>();
                    m_tweens = true;

                }
                GUI.color = Color.white;
            }


            if (serializedObject.ApplyModifiedProperties()) // Apply if any changes occur
            {
                EditorUtility.SetDirty(Target);
            }
        }
    }
}