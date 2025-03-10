//
// Copyright (c) 2023 Warped Imagination. All rights reserved. 
//

using System;
using UnityEditor;
using UnityEngine;

namespace WarpedImagination
{
    /// <summary>
    /// Dialog Window for entering a text string
    /// </summary>
    public class TextEntryDialog : EditorWindow
    {
        const string TEXT_ENTRY_FIELD_ID = "entry";

        string _label = null;
        string _description = null;
        string _value = null;
        bool _firstRender = true;
        bool _accepted = false;

        /// <summary>
        /// Show this text entry dialog
        /// </summary>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <param name="label"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string Show(string title, string description, string label, string defaultValue = null)
        {
            TextEntryDialog dialog = ScriptableObject.CreateInstance<TextEntryDialog>();
            dialog.titleContent = new GUIContent(title);
            dialog.maxSize = new Vector2(500, 120);
            dialog.minSize = dialog.maxSize;
            dialog._label = label;
            dialog._description = description;
            dialog._value = defaultValue;
            dialog.ShowModal();
            return dialog._value;
        }

        void OnGUI()
        {
            // listen to input
            if (Event.current != null && Event.current.isKey)
            {
                switch (Event.current.keyCode)
                {
                    case KeyCode.KeypadEnter:
                    case KeyCode.Return:
                        Close(true);
                        break;
                    case KeyCode.Escape:
                        Close(false);
                        break;
                    default:
                        break;
                }
            }

            GUILayout.Space(10f);

            EditorGUILayout.LabelField(_description, EditorStyles.boldLabel);

            GUILayout.Space(5f);

            GUI.SetNextControlName(TEXT_ENTRY_FIELD_ID);
            _value = EditorGUILayout.TextField(_label, _value);

            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Cancel"))
            {
                Close(false);
            }

            if (GUILayout.Button("OK"))
            {
                Close(true);
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(20f);

            if (_firstRender)
            {
                _firstRender = false;
                GUI.FocusControl(TEXT_ENTRY_FIELD_ID);
            }
        }

        private void OnDestroy()
        {
            if (!_accepted)
                _value = null;
        }

        private void Close(bool accepted)
        {
            _accepted = accepted;
            Close();
            GUIUtility.ExitGUI();
        }
    }
}