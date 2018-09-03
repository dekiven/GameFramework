using System;
using UnityEditor;
using UnityEngine;

namespace GameFramework
{
    [Flags]
    public enum CustomListOption
    {
        None = 0,
        ShowSize = 1,
        ShowLabel = 2,
        ShowElementLabels = 4,
        ShowBtns = 8,
        Default = ShowSize | ShowLabel | ShowElementLabels | ShowBtns,
        NoElementLabels = ShowSize | ShowLabel,
        NoBtns = ShowSize | ShowLabel | ShowElementLabels,
        All = Default,
    }

    public class CustomListInspector
    {
        public delegate bool PassIndexCallback(int index);

        /// <summary>
        /// 添加完成后对数据进行处理，如果返回false 取消添加
        /// </summary>
        public PassIndexCallback OnAddCallback;
        public CustomListOption mOptions = CustomListOption.Default;

        private static GUIContent sBtnUp = new GUIContent("\u2191", "上移");
        private static GUIContent sBtnDown = new GUIContent("\u2193", "下移");
        private static GUIContent sBtnDuplicate = new GUIContent("+", "添加/复制");
        private static GUIContent sBtnRemove = new GUIContent("-", "删除");

        private static GUILayoutOption sMinBtnWidth = GUILayout.Width(20f);

        public void Show(SerializedProperty listProperty)
        {
            int oldIndentLevel = EditorGUI.indentLevel;
            if(!listProperty.isArray)
            {
                EditorGUILayout.HelpBox("属性:\"" + listProperty.name + "\" 不是List<>或数组", MessageType.Error);
                if(!listProperty.isExpanded)
                {
                    listProperty.isExpanded = true;
                }
                return;
            }

            bool
                showLabel = (mOptions & CustomListOption.ShowLabel) != 0,
                showSize = (mOptions & CustomListOption.ShowSize) != 0;

            if(showLabel)
            {
                EditorGUILayout.PropertyField(listProperty);
                EditorGUI.indentLevel += 1;
            }

            if(!showLabel || listProperty.isExpanded)
            {
                SerializedProperty size = listProperty.FindPropertyRelative("Array.size");
                if(showSize)
                {
                    EditorGUILayout.PropertyField(size);
                    GUILayout.Space(5f);
                }
                if(size.hasMultipleDifferentValues)
                {
                    EditorGUILayout.HelpBox("Not showing lists with different sizes.", MessageType.Info);
                }
                else
                {
                    showElement(listProperty);
                    GUILayout.Space(5f);
                }
            }
            if(showLabel)
            {
                EditorGUI.indentLevel = oldIndentLevel;
            }
        }

        private void showElement(SerializedProperty listProperty)
        {
            bool
                showLabel = (mOptions & CustomListOption.ShowElementLabels) != 0,
                showBtn = (mOptions & CustomListOption.ShowBtns) != 0;
            for (int i = 0; i < listProperty.arraySize; i++)
            {
                if(showBtn)
                {
                    EditorGUILayout.BeginHorizontal();
                }
                if(showLabel)
                {
                    EditorGUILayout.PropertyField(listProperty.GetArrayElementAtIndex(i), new GUIContent("idx: "+i));
                }
                else
                {
                    EditorGUILayout.PropertyField(listProperty.GetArrayElementAtIndex(i), GUIContent.none);
                }
                if (showBtn)
                {
                    showBtns(listProperty, i);
                    EditorGUILayout.EndHorizontal();
                }
            }

            if (listProperty.arraySize == 0 && GUILayout.Button(new GUIContent("+", "添加一个元素")))
            {
                listProperty.arraySize += 1;
                Debug.Log("listProperty.arraySize:" + listProperty.arraySize);
                onAdd(listProperty, 0);
            }
        }

        private void showBtns(SerializedProperty listProperty, int index)
        {
            //if (GUILayout.Button(sBtnUp, EditorStyles.miniButtonLeft, sMinBtnWidth))
            GUI.enabled = index != 0;
            if(GUILayout.Button(sBtnUp, EditorStyles.miniButton, sMinBtnWidth))
            {
                onMoveUp(listProperty, index);
            }
            GUI.enabled = index != listProperty.arraySize - 1;
            if (GUILayout.Button(sBtnDown, EditorStyles.miniButton, sMinBtnWidth))
            {
                onMoveDown(listProperty, index);
            }
            GUI.enabled = true;
            if (GUILayout.Button(sBtnDuplicate, EditorStyles.miniButton, sMinBtnWidth))
            {
                onAdd(listProperty, index);
            }
            //if (GUILayout.Button(sBtnRemove, EditorStyles.miniButtonRight, sMinBtnWidth))
            if (GUILayout.Button(sBtnRemove, EditorStyles.miniButton, sMinBtnWidth))
            {
                onRemove(listProperty, index);
            }
        }

        private void onAdd(SerializedProperty listProperty, int index)
        {
            listProperty.InsertArrayElementAtIndex(index);
            if(null != OnAddCallback && !OnAddCallback(index))
            {
                onRemove(listProperty, index);
            }
        }

        private void onRemove(SerializedProperty listProperty, int index)
        {
            int oldSize = listProperty.arraySize;
            listProperty.DeleteArrayElementAtIndex(index);
            if (listProperty.arraySize == oldSize)
            {
                listProperty.DeleteArrayElementAtIndex(index);
            }
        }

        private void onMoveUp(SerializedProperty listProperty, int index)
        {
            listProperty.MoveArrayElement(index, index - 1);
        }

        private void onMoveDown(SerializedProperty listProperty, int index)
        {
            listProperty.MoveArrayElement(index, index + 1);
        }
    }
}
