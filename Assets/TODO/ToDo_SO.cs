#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace ToDo_CodeTrigger
{
    [CreateAssetMenu(fileName = "NewToDoList", menuName = "Tools/To-Do List")]
    public class ToDo_SO : ScriptableObject
    {
        public List<ToDoItem> tasks = new List<ToDoItem>();
    }

    public enum TaskTag
    {
        Todo,
        Finished,
        WorkingOn,
        Idea
    }

    [System.Serializable]
    public class ToDoItem
    {
        public string taskName;
        public TaskTag tag;
        [TextArea(1, 3)] public string notes;
    }

    [CustomEditor(typeof(ToDo_SO))]
    public class ToDo_SOEditor : Editor
    {
        bool[] excludeTagMask = new bool[System.Enum.GetValues(typeof(TaskTag)).Length];

        SerializedProperty tasks;
        bool showExcludeTags = true;

        Color finishedColor = new Color(0.75f, 1f, 0.75f);
        Color workingOnColor = new Color(1f, .75f, 0.5f);
        Color ideaColor = new Color(0.5f, 0.75f, 1f);

        TaskTag filterTag = (TaskTag)(-1);

        void OnEnable()
        {
            tasks = serializedObject.FindProperty("tasks");
        }

        public override void OnInspectorGUI()
{
    serializedObject.Update();

    EditorGUILayout.LabelField(((ToDo_SO)target).name, EditorStyles.boldLabel);
    EditorGUILayout.Space();

    // === Show Only One Tag ===
    EditorGUILayout.BeginHorizontal();
    EditorGUILayout.LabelField("Show Only Tag:", GUILayout.Width(90));
    filterTag = (TaskTag)EditorGUILayout.EnumPopup(filterTag);
    if (GUILayout.Button("Clear", GUILayout.Width(60)))
    {
        filterTag = (TaskTag)(-1);
    }
    EditorGUILayout.EndHorizontal();
    EditorGUILayout.Space();

    // === Hide Multiple Tags (foldout) ===
    showExcludeTags = EditorGUILayout.Foldout(showExcludeTags, "Hide Tags");
    if (showExcludeTags)
    {
        EditorGUI.indentLevel++;
        for (int i = 0; i < excludeTagMask.Length; i++)
        {
            excludeTagMask[i] = EditorGUILayout.ToggleLeft(((TaskTag)i).ToString(), excludeTagMask[i]);
        }
        EditorGUI.indentLevel--;
        EditorGUILayout.Space();
    }

    // === Render Tasks ===
    for (int i = 0; i < tasks.arraySize; i++)
    {
        SerializedProperty item = tasks.GetArrayElementAtIndex(i);
        SerializedProperty taskName = item.FindPropertyRelative("taskName");
        SerializedProperty tag = item.FindPropertyRelative("tag");
        SerializedProperty notes = item.FindPropertyRelative("notes");

        // Apply filters
        if ((int)filterTag != -1 && tag.enumValueIndex != (int)filterTag)
            continue;

        if (excludeTagMask[tag.enumValueIndex])
            continue;

        // Background color per tag
        Color originalColor = GUI.backgroundColor;

        switch ((TaskTag)tag.enumValueIndex)
        {
            case TaskTag.Finished:
                GUI.backgroundColor = finishedColor;
                break;
            case TaskTag.WorkingOn:
                GUI.backgroundColor = workingOnColor;
                break;
            case TaskTag.Idea:
                GUI.backgroundColor = ideaColor;
                break;
            default:
                GUI.backgroundColor = originalColor;
                break;
        }

        EditorGUILayout.BeginVertical("box");

        // Header: name, move, remove
        EditorGUILayout.BeginHorizontal();

        taskName.stringValue = EditorGUILayout.TextField(taskName.stringValue);

        if (GUILayout.Button("↑", GUILayout.Width(25)) && i > 0)
            tasks.MoveArrayElement(i, i - 1);

        if (GUILayout.Button("↓", GUILayout.Width(25)) && i < tasks.arraySize - 1)
            tasks.MoveArrayElement(i, i + 1);

        if (GUILayout.Button("X", GUILayout.Width(20)))
        {
            tasks.DeleteArrayElementAtIndex(i);
            GUI.backgroundColor = originalColor;
            break;
        }

        EditorGUILayout.EndHorizontal();

        // Tag
        EditorGUILayout.PropertyField(tag, new GUIContent("Tag"));

        // Notes with word wrap
        GUIStyle textAreaStyle = new GUIStyle(EditorStyles.textArea);
        textAreaStyle.wordWrap = true;
        notes.stringValue = EditorGUILayout.TextArea(notes.stringValue, textAreaStyle, GUILayout.MinHeight(60));

        EditorGUILayout.EndVertical();
        GUI.backgroundColor = originalColor;

        EditorGUILayout.Space();
    }

    // Add Task
    if (GUILayout.Button("+ Add Task"))
    {
        tasks.InsertArrayElementAtIndex(tasks.arraySize);
        SerializedProperty newItem = tasks.GetArrayElementAtIndex(tasks.arraySize - 1);
        newItem.FindPropertyRelative("taskName").stringValue = "New Task";
        newItem.FindPropertyRelative("tag").enumValueIndex = (int)filterTag;
        newItem.FindPropertyRelative("notes").stringValue = "";
    }

    serializedObject.ApplyModifiedProperties();
}

    }
}
#endif
