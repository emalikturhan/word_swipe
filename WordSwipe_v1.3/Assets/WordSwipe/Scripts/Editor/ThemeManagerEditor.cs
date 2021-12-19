using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Bimbimnet.WordBlocks
{
    [CustomEditor(typeof(ThemeManager))]
    public class ThemeManagerEditor : Editor
    {
        #region Member Variables

        private Texture2D lineTexture;

        #endregion

        #region Properties

        private Texture2D LineTexture
        {
            get
            {
                if (lineTexture == null)
                {
                    lineTexture = new Texture2D(1, 1);
                    lineTexture.SetPixel(0, 0, new Color(37f / 255f, 37f / 255f, 37f / 255f));
                    lineTexture.Apply();
                }

                return lineTexture;
            }
        }

        #endregion

        #region Unity Methods

        private void OnDisable()
        {
            DestroyImmediate(LineTexture);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("themesEnabled"));

            GUI.enabled = serializedObject.FindProperty("themesEnabled").boolValue;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("debugUnlockAllThemes"));

            EditorGUILayout.Space();

            DrawThemeIds();

            DrawThemes();

            EditorGUILayout.Space();

            GUI.enabled = true;

            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region Private Methods

        private void DrawThemeIds()
        {
            SerializedProperty itemIdsProp = serializedObject.FindProperty("ids");

            if (BeginBox(itemIdsProp))
            {
                for (int i = 0; i < itemIdsProp.arraySize; i++)
                {
                    SerializedProperty itemIdProp = itemIdsProp.GetArrayElementAtIndex(i);
                    SerializedProperty idProp = itemIdProp.FindPropertyRelative("id");

                    string itemLabel = string.IsNullOrEmpty(idProp.stringValue) ? "<id>" : idProp.stringValue;

                    EditorGUILayout.BeginHorizontal();

                    GUILayout.Space(16f);

                    EditorGUILayout.PropertyField(itemIdProp, new GUIContent(itemLabel));

                    bool removed = GUILayout.Button("Remove", GUILayout.Width(100));

                    EditorGUILayout.EndHorizontal();

                    if (itemIdProp.isExpanded)
                    {
                        EditorGUI.indentLevel++;

                        EditorGUILayout.PropertyField(idProp);
                        EditorGUILayout.PropertyField(itemIdProp.FindPropertyRelative("type"));

                        EditorGUI.indentLevel--;
                    }

                    if (removed)
                    {
                        RemoveItemId(itemIdsProp, i);
                        i--;
                    }
                }

                DrawLine();

                if (GUILayout.Button("Add Item Id"))
                {
                    AddItemId(itemIdsProp);
                }

                GUILayout.Space(2f);
            }

            EndBox();
        }

        private void AddItemId(SerializedProperty itemIdsProp)
        {
            itemIdsProp.InsertArrayElementAtIndex(itemIdsProp.arraySize);

            SerializedProperty itemIdProp = itemIdsProp.GetArrayElementAtIndex(itemIdsProp.arraySize - 1);
            SerializedProperty idProp = itemIdProp.FindPropertyRelative("id");
            SerializedProperty typeProp = itemIdProp.FindPropertyRelative("type");

            idProp.stringValue = "";
            typeProp.enumValueIndex = 0;

            SerializedProperty themesProp = serializedObject.FindProperty("themes");

            for (int i = 0; i < themesProp.arraySize; i++)
            {
                SerializedProperty themeProp = themesProp.GetArrayElementAtIndex(i);
                SerializedProperty themeItemsProp = themeProp.FindPropertyRelative("themeItems");

                themeItemsProp.InsertArrayElementAtIndex(themeItemsProp.arraySize);

                SerializedProperty themeItemProp = themeItemsProp.GetArrayElementAtIndex(themeItemsProp.arraySize - 1);

                themeItemProp.FindPropertyRelative("color").colorValue = Color.white;
                themeItemProp.FindPropertyRelative("image").objectReferenceValue = null;
            }
        }

        private void RemoveItemId(SerializedProperty itemIdsProp, int index)
        {
            itemIdsProp.DeleteArrayElementAtIndex(index);

            SerializedProperty themesProp = serializedObject.FindProperty("themes");

            for (int i = 0; i < themesProp.arraySize; i++)
            {
                SerializedProperty themeProp = themesProp.GetArrayElementAtIndex(i);
                SerializedProperty themeItemsProp = themeProp.FindPropertyRelative("themeItems");

                themeItemsProp.DeleteArrayElementAtIndex(index);
            }
        }

        private void DrawThemes()
        {
            SerializedProperty themesProp = serializedObject.FindProperty("themes");

            if (BeginBox(themesProp))
            {
                for (int i = 0; i < themesProp.arraySize; i++)
                {
                    SerializedProperty themeProp = themesProp.GetArrayElementAtIndex(i);
                    SerializedProperty nameProp = themeProp.FindPropertyRelative("name");
                    SerializedProperty activeProp = themeProp.FindPropertyRelative("setActiveThemeInEditor");

                    string itemLabel = string.IsNullOrEmpty(nameProp.stringValue) ? "<name>" : nameProp.stringValue;

                    if (activeProp.boolValue)
                    {
                        itemLabel += " [ACTIVE IN EDITOR]";
                    }

                    EditorGUILayout.BeginHorizontal();

                    GUILayout.Space(16f);

                    EditorGUILayout.PropertyField(themeProp, new GUIContent(itemLabel));

                    bool wasEnabled = GUI.enabled;

                    if (i == 0)
                    {
                        GUI.enabled = false;
                    }

                    bool moveUp = GUILayout.Button("^", GUILayout.Width(25));

                    GUI.enabled = wasEnabled;

                    if (i == themesProp.arraySize - 1)
                    {
                        GUI.enabled = false;
                    }

                    bool moveDown = GUILayout.Button("v", GUILayout.Width(25));

                    GUI.enabled = wasEnabled;

                    bool removed = GUILayout.Button("Remove", GUILayout.Width(100));

                    EditorGUILayout.EndHorizontal();

                    if (themeProp.isExpanded)
                    {
                        EditorGUI.indentLevel++;

                        EditorGUILayout.PropertyField(nameProp);
                        EditorGUILayout.PropertyField(themeProp.FindPropertyRelative("listItemImage"));

                        bool val = EditorGUILayout.Toggle("Active Theme In Editor", activeProp.boolValue);

                        // Check if it was just toggled on
                        if (val && val != activeProp.boolValue)
                        {
                            // Turn off all other themes active bool
                            for (int j = 0; j < themesProp.arraySize; j++)
                            {
                                if (i != j)
                                {
                                    themesProp.GetArrayElementAtIndex(j).FindPropertyRelative("setActiveThemeInEditor").boolValue = false;
                                }
                            }
                        }

                        activeProp.boolValue = val;

                        SerializedProperty isLockedProp = themeProp.FindPropertyRelative("isLocked");

                        EditorGUILayout.PropertyField(isLockedProp);

                        if (!isLockedProp.boolValue)
                        {
                            GUI.enabled = false;
                        }

                        EditorGUILayout.PropertyField(themeProp.FindPropertyRelative("coinsToUnlock"));

                        GUI.enabled = wasEnabled;

                        EditorGUILayout.Space();

                        DrawThemeItems(themeProp);

                        EditorGUI.indentLevel--;
                    }

                    if (removed)
                    {
                        themesProp.DeleteArrayElementAtIndex(i);
                        i--;
                    }

                    if (moveUp)
                    {
                        themesProp.MoveArrayElement(i, i - 1);
                    }

                    if (moveDown)
                    {
                        themesProp.MoveArrayElement(i, i + 1);
                    }
                }

                DrawLine();

                if (GUILayout.Button("Add Theme"))
                {
                    AddTheme(themesProp);
                }

                GUILayout.Space(2f);
            }

            EndBox();
        }

        private void AddTheme(SerializedProperty themesProp)
        {
            themesProp.InsertArrayElementAtIndex(themesProp.arraySize);

            SerializedProperty themeProp = themesProp.GetArrayElementAtIndex(themesProp.arraySize - 1);

            themeProp.FindPropertyRelative("name").stringValue = "";
            themeProp.FindPropertyRelative("listItemImage").objectReferenceValue = null;
            themeProp.FindPropertyRelative("setActiveThemeInEditor").boolValue = false;
            themeProp.FindPropertyRelative("isLocked").boolValue = false;
            themeProp.FindPropertyRelative("coinsToUnlock").intValue = 0;

            SerializedProperty themeItemsProp = themeProp.FindPropertyRelative("themeItems");

            for (int i = 0; i < themeItemsProp.arraySize; i++)
            {
                SerializedProperty themeItemProp = themeItemsProp.GetArrayElementAtIndex(i);

                themeItemProp.FindPropertyRelative("color").colorValue = Color.white;
                themeItemProp.FindPropertyRelative("image").objectReferenceValue = null;
                themeItemProp.FindPropertyRelative("prefab").objectReferenceValue = null;
            }
        }

        private void DrawThemeItems(SerializedProperty themeProp)
        {
            SerializedProperty idsProp = serializedObject.FindProperty("ids");
            SerializedProperty itemsProp = themeProp.FindPropertyRelative("themeItems");

            for (int i = 0; i < idsProp.arraySize; i++)
            {
                SerializedProperty itemIdProp = idsProp.GetArrayElementAtIndex(i);
                SerializedProperty itemProp = itemsProp.GetArrayElementAtIndex(i);
                SerializedProperty idProp = itemIdProp.FindPropertyRelative("id");
                SerializedProperty typeProp = itemIdProp.FindPropertyRelative("type");

                string id = idProp.stringValue;

                switch (typeProp.enumValueIndex)
                {
                    case 0:
                        EditorGUILayout.PropertyField(itemProp.FindPropertyRelative("color"), new GUIContent(id));
                        break;
                    case 1:
                        EditorGUILayout.PropertyField(itemProp.FindPropertyRelative("image"), new GUIContent(id));
                        break;
                    case 2:
                        EditorGUILayout.PropertyField(itemProp.FindPropertyRelative("prefab"), new GUIContent(id));
                        break;
                }
            }
        }

        /// <summary>
        /// Begins a new foldout box, must call EndBox
        /// </summary>
        private bool BeginBox(SerializedProperty prop)
        {
            GUIStyle style = new GUIStyle("HelpBox");
            style.padding.left = 0;
            style.padding.right = 0;
            style.margin.left = 0;

            GUILayout.BeginVertical(style);

            EditorGUILayout.BeginHorizontal();

            GUILayout.Space(16f);

            prop.isExpanded = EditorGUILayout.Foldout(prop.isExpanded, prop.displayName);

            EditorGUILayout.EndHorizontal();

            if (prop.isExpanded)
            {
                DrawLine();
            }

            return prop.isExpanded;
        }

        /// <summary>
        /// Ends the box.
        /// </summary>
        private void EndBox()
        {
            GUILayout.EndVertical();
        }

        /// <summary>
        /// Draws a simple 1 pixel height line
        /// </summary>
        private void DrawLine()
        {
            GUIStyle lineStyle = new GUIStyle();
            lineStyle.normal.background = LineTexture;

            GUILayout.BeginVertical(lineStyle);
            GUILayout.Space(1);
            GUILayout.EndVertical();
        }

        #endregion
    }
}
