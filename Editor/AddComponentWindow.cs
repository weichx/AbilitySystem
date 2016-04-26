//// Decompiled with JetBrains decompiler
//// Type: UnityEditor.AddComponentWindow
//// Assembly: UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
//// MVID: 2E59F856-F786-429D-9B87-B218CA78D858
//// Assembly location: C:\Program Files (x86)\Unity\Editor\Data\Managed\UnityEditor.dll

//using System;
//using System.CodeDom.Compiler;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Reflection;
//using UnityEditorInternal;
//using UnityEngine;

//namespace UnityEditor {
//    [InitializeOnLoad]
//    internal class AddComponentWindow : EditorWindow {
//        private static bool s_DirtyList = true;
//        private string m_ClassName = string.Empty;
//        private List<AddComponentWindow.GroupElement> m_Stack = new List<AddComponentWindow.GroupElement>();
//        private float m_Anim = 1f;
//        private int m_AnimTarget = 1;
//        private string m_Search = string.Empty;
//        private const int kHeaderHeight = 30;
//        private const int kWindowHeight = 320;
//        private const int kHelpHeight = 0;
//        private const string kLanguageEditorPrefName = "NewScriptLanguage";
//        private const string kComponentSearch = "ComponentSearchString";
//        private const string kSearchHeader = "Search";
//        private static AddComponentWindow.Styles s_Styles;
//        private static AddComponentWindow s_AddComponentWindow;
//        private static long s_LastClosedTime;
//        internal static AddComponentWindow.Language s_Lang;
//        private GameObject[] m_GameObjects;
//        private AddComponentWindow.Element[] m_Tree;
//        private AddComponentWindow.Element[] m_SearchResultTree;
//        private long m_LastTime;
//        private bool m_ScrollToSelected;

//        internal static string className {
//            get {
//                return AddComponentWindow.s_AddComponentWindow.m_ClassName;
//            }
//            set {
//                AddComponentWindow.s_AddComponentWindow.m_ClassName = value;
//            }
//        }

//        internal static GameObject[] gameObjects {
//            get {
//                return AddComponentWindow.s_AddComponentWindow.m_GameObjects;
//            }
//        }

//        private bool hasSearch {
//            get {
//                return !string.IsNullOrEmpty(this.m_Search);
//            }
//        }

//        private AddComponentWindow.GroupElement activeParent {
//            get {
//                return this.m_Stack[this.m_Stack.Count - 2 + this.m_AnimTarget];
//            }
//        }

//        private AddComponentWindow.Element[] activeTree {
//            get {
//                if (this.hasSearch)
//                    return this.m_SearchResultTree;
//                return this.m_Tree;
//            }
//        }

//        private AddComponentWindow.Element activeElement {
//            get {
//                if (this.activeTree == null)
//                    return (AddComponentWindow.Element)null;
//                List<AddComponentWindow.Element> children = this.GetChildren(this.activeTree, (AddComponentWindow.Element)this.activeParent);
//                if (children.Count == 0)
//                    return (AddComponentWindow.Element)null;
//                return children[this.activeParent.selectedIndex];
//            }
//        }

//        private void OnEnable() {
//            AddComponentWindow.s_AddComponentWindow = this;
//            AddComponentWindow.s_Lang = (AddComponentWindow.Language)EditorPrefs.GetInt("NewScriptLanguage", 1);
//            this.m_Search = EditorPrefs.GetString("ComponentSearchString", string.Empty);
//        }

//        private void OnDisable() {
//            AddComponentWindow.s_LastClosedTime = DateTime.Now.Ticks / 10000L;
//            AddComponentWindow.s_AddComponentWindow = (AddComponentWindow)null;
//        }

//        private static InspectorWindow FirstInspectorWithGameObject() {
//            using (List<InspectorWindow>.Enumerator enumerator = InspectorWindow.GetInspectors().GetEnumerator()) {
//                while (enumerator.MoveNext()) {
//                    InspectorWindow current = enumerator.Current;
//                    if (current.GetInspectedObject() is GameObject)
//                        return current;
//                }
//            }
//            return (InspectorWindow)null;
//        }

//        internal static bool ValidateAddComponentMenuItem() {
//            return (UnityEngine.Object)AddComponentWindow.FirstInspectorWithGameObject() != (UnityEngine.Object)null;
//        }

//        internal static void ExecuteAddComponentMenuItem() {
//            InspectorWindow inspectorWindow = AddComponentWindow.FirstInspectorWithGameObject();
//            if (!((UnityEngine.Object)inspectorWindow != (UnityEngine.Object)null))
//                return;
//            inspectorWindow.SendEvent(EditorGUIUtility.CommandEvent("OpenAddComponentDropdown"));
//        }

//        internal static bool Show(Rect rect, GameObject[] gos) {
//            UnityEngine.Object[] objectsOfTypeAll = Resources.FindObjectsOfTypeAll(typeof(AddComponentWindow));
//            if (objectsOfTypeAll.Length > 0) {
//                ((EditorWindow)objectsOfTypeAll[0]).Close();
//                return false;
//            }
//            if (DateTime.Now.Ticks / 10000L < AddComponentWindow.s_LastClosedTime + 50L)
//                return false;
//            Event.current.Use();
//            if ((UnityEngine.Object)AddComponentWindow.s_AddComponentWindow == (UnityEngine.Object)null)
//                AddComponentWindow.s_AddComponentWindow = ScriptableObject.CreateInstance<AddComponentWindow>();
//            AddComponentWindow.s_AddComponentWindow.Init(rect);
//            AddComponentWindow.s_AddComponentWindow.m_GameObjects = gos;
//            return true;
//        }

//        private void Init(Rect buttonRect) {
//            buttonRect = GUIUtility.GUIToScreenRect(buttonRect);
//            this.CreateComponentTree();
//            this.ShowAsDropDown(buttonRect, new Vector2(buttonRect.width, 320f));
//            this.Focus();
//            this.m_Parent.AddToAuxWindowList();
//            this.wantsMouseMove = true;
//        }

//        private void CreateComponentTree() {
//            string[] submenus = Unsupported.GetSubmenus("Component");
//            string[] submenusCommands = Unsupported.GetSubmenusCommands("Component");
//            List<string> list1 = new List<string>();
//            List<AddComponentWindow.Element> list2 = new List<AddComponentWindow.Element>();
//            for (int index1 = 0; index1 < submenus.Length; ++index1) {
//                if (!(submenusCommands[index1] == "ADD")) {
//                    string menuPath = submenus[index1];
//                    string str = menuPath;
//                    char[] chArray = new char[1];
//                    int index2 = 0;
//                    int num = 47;
//                    chArray[index2] = (char)num;
//                    string[] strArray = str.Split(chArray);
//                    while (strArray.Length - 1 < list1.Count)
//                        list1.RemoveAt(list1.Count - 1);
//                    while (list1.Count > 0 && strArray[list1.Count - 1] != list1[list1.Count - 1])
//                        list1.RemoveAt(list1.Count - 1);
//                    while (strArray.Length - 1 > list1.Count) {
//                        list2.Add((AddComponentWindow.Element)new AddComponentWindow.GroupElement(list1.Count, strArray[list1.Count]));
//                        list1.Add(strArray[list1.Count]);
//                    }
//                    list2.Add((AddComponentWindow.Element)new AddComponentWindow.ComponentElement(list1.Count, strArray[strArray.Length - 1], menuPath, submenusCommands[index1]));
//                }
//            }
//            list2.Add((AddComponentWindow.Element)new AddComponentWindow.NewScriptElement());
//            this.m_Tree = list2.ToArray();
//            if (this.m_Stack.Count == 0) {
//                this.m_Stack.Add(this.m_Tree[0] as AddComponentWindow.GroupElement);
//            }
//            else {
//                // ISSUE: object of a compiler-generated type is created
//                // ISSUE: variable of a compiler-generated type
//                AddComponentWindow.\u003CCreateComponentTree\u003Ec__AnonStorey43 treeCAnonStorey43 = new AddComponentWindow.\u003CCreateComponentTree\u003Ec__AnonStorey43();
//                // ISSUE: reference to a compiler-generated field
//                treeCAnonStorey43.\u003C\u003Ef__this = this;
//                AddComponentWindow.GroupElement groupElement1 = this.m_Tree[0] as AddComponentWindow.GroupElement;
//                // ISSUE: reference to a compiler-generated field
//                treeCAnonStorey43.level = 0;
//                label_15:
//                while (true) {
//                    // ISSUE: reference to a compiler-generated field
//                    AddComponentWindow.GroupElement groupElement2 = this.m_Stack[treeCAnonStorey43.level];
//                    // ISSUE: reference to a compiler-generated field
//                    this.m_Stack[treeCAnonStorey43.level] = groupElement1;
//                    // ISSUE: reference to a compiler-generated field
//                    this.m_Stack[treeCAnonStorey43.level].selectedIndex = groupElement2.selectedIndex;
//                    // ISSUE: reference to a compiler-generated field
//                    this.m_Stack[treeCAnonStorey43.level].scroll = groupElement2.scroll;
//                    // ISSUE: reference to a compiler-generated field
//                    // ISSUE: reference to a compiler-generated field
//                    treeCAnonStorey43.level = treeCAnonStorey43.level + 1;
//                    // ISSUE: reference to a compiler-generated field
//                    if (treeCAnonStorey43.level != this.m_Stack.Count) {
//                        // ISSUE: reference to a compiler-generated method
//                        AddComponentWindow.Element element = Enumerable.FirstOrDefault<AddComponentWindow.Element>((IEnumerable<AddComponentWindow.Element>)this.GetChildren(this.activeTree, (AddComponentWindow.Element)groupElement1), new Func<AddComponentWindow.Element, bool>(treeCAnonStorey43.\u003C\u003Em__8C));
//                        if (element != null && element is AddComponentWindow.GroupElement)
//                            groupElement1 = element as AddComponentWindow.GroupElement;
//                        else
//                            break;
//                    }
//                    else
//                        goto label_20;
//                }
//                // ISSUE: reference to a compiler-generated field
//                while (this.m_Stack.Count > treeCAnonStorey43.level) {
//                    // ISSUE: reference to a compiler-generated field
//                    this.m_Stack.RemoveAt(treeCAnonStorey43.level);
//                }
//                goto label_15;
//            }
//            label_20:
//            AddComponentWindow.s_DirtyList = false;
//            this.RebuildSearch();
//        }

//        internal void OnGUI() {
//            if (AddComponentWindow.s_Styles == null)
//                AddComponentWindow.s_Styles = new AddComponentWindow.Styles();
//            GUI.Label(new Rect(0.0f, 0.0f, this.position.width, this.position.height), GUIContent.none, AddComponentWindow.s_Styles.background);
//            if (AddComponentWindow.s_DirtyList)
//                this.CreateComponentTree();
//            this.HandleKeyboard();
//            GUILayout.Space(7f);
//            if (!(this.activeParent is AddComponentWindow.NewScriptElement))
//                EditorGUI.FocusTextInControl("ComponentSearch");
//            Rect rect = GUILayoutUtility.GetRect(10f, 20f);
//            rect.x += 8f;
//            rect.width -= 16f;
//            GUI.SetNextControlName("ComponentSearch");
//            EditorGUI.BeginDisabledGroup(this.activeParent is AddComponentWindow.NewScriptElement);
//            string str = EditorGUI.SearchField(rect, this.m_Search);
//            EditorGUI.EndDisabledGroup();
//            if (str != this.m_Search) {
//                this.m_Search = str;
//                EditorPrefs.SetString("ComponentSearchString", this.m_Search);
//                this.RebuildSearch();
//            }
//            this.ListGUI(this.activeTree, this.m_Anim, this.GetElementRelative(0), this.GetElementRelative(-1));
//            if ((double)this.m_Anim < 1.0)
//                this.ListGUI(this.activeTree, this.m_Anim + 1f, this.GetElementRelative(-1), this.GetElementRelative(-2));
//            if ((double)this.m_Anim == (double)this.m_AnimTarget || Event.current.type != EventType.Repaint)
//                return;
//            long ticks = DateTime.Now.Ticks;
//            float num = (float)(ticks - this.m_LastTime) / 1E+07f;
//            this.m_LastTime = ticks;
//            this.m_Anim = Mathf.MoveTowards(this.m_Anim, (float)this.m_AnimTarget, num * 4f);
//            if (this.m_AnimTarget == 0 && (double)this.m_Anim == 0.0) {
//                this.m_Anim = 1f;
//                this.m_AnimTarget = 1;
//                this.m_Stack.RemoveAt(this.m_Stack.Count - 1);
//            }
//            this.Repaint();
//        }

//        private void HandleKeyboard() {
//            Event current = Event.current;
//            if (current.type != EventType.KeyDown)
//                return;
//            if (this.activeParent is AddComponentWindow.NewScriptElement) {
//                if (current.keyCode == KeyCode.Return || current.keyCode == KeyCode.KeypadEnter) {
//                    (this.activeParent as AddComponentWindow.NewScriptElement).Create();
//                    current.Use();
//                    GUIUtility.ExitGUI();
//                }
//                if (current.keyCode != KeyCode.Escape)
//                    return;
//                this.GoToParent();
//                current.Use();
//            }
//            else {
//                if (current.keyCode == KeyCode.DownArrow) {
//                    ++this.activeParent.selectedIndex;
//                    this.activeParent.selectedIndex = Mathf.Min(this.activeParent.selectedIndex, this.GetChildren(this.activeTree, (AddComponentWindow.Element)this.activeParent).Count - 1);
//                    this.m_ScrollToSelected = true;
//                    current.Use();
//                }
//                if (current.keyCode == KeyCode.UpArrow) {
//                    --this.activeParent.selectedIndex;
//                    this.activeParent.selectedIndex = Mathf.Max(this.activeParent.selectedIndex, 0);
//                    this.m_ScrollToSelected = true;
//                    current.Use();
//                }
//                if (current.keyCode == KeyCode.Return || current.keyCode == KeyCode.KeypadEnter) {
//                    this.GoToChild(this.activeElement, true);
//                    current.Use();
//                }
//                if (this.hasSearch)
//                    return;
//                if (current.keyCode == KeyCode.LeftArrow || current.keyCode == KeyCode.Backspace) {
//                    this.GoToParent();
//                    current.Use();
//                }
//                if (current.keyCode == KeyCode.RightArrow) {
//                    this.GoToChild(this.activeElement, false);
//                    current.Use();
//                }
//                if (current.keyCode != KeyCode.Escape)
//                    return;
//                this.Close();
//                current.Use();
//            }
//        }

//        private void RebuildSearch() {
//            if (!this.hasSearch) {
//                this.m_SearchResultTree = (AddComponentWindow.Element[])null;
//                if (this.m_Stack[this.m_Stack.Count - 1].name == "Search") {
//                    this.m_Stack.Clear();
//                    this.m_Stack.Add(this.m_Tree[0] as AddComponentWindow.GroupElement);
//                }
//                this.m_AnimTarget = 1;
//                this.m_LastTime = DateTime.Now.Ticks;
//                this.m_ClassName = "NewBehaviourScript";
//            }
//            else {
//                this.m_ClassName = this.m_Search;
//                string str1 = this.m_Search.ToLower();
//                char[] chArray = new char[1];
//                int index1 = 0;
//                int num = 32;
//                chArray[index1] = (char)num;
//                string[] strArray = str1.Split(chArray);
//                List<AddComponentWindow.Element> list1 = new List<AddComponentWindow.Element>();
//                List<AddComponentWindow.Element> list2 = new List<AddComponentWindow.Element>();
//                foreach (AddComponentWindow.Element element in this.m_Tree) {
//                    if (element is AddComponentWindow.ComponentElement) {
//                        string str2 = element.name.ToLower().Replace(" ", string.Empty);
//                        bool flag1 = true;
//                        bool flag2 = false;
//                        for (int index2 = 0; index2 < strArray.Length; ++index2) {
//                            string str3 = strArray[index2];
//                            if (str2.Contains(str3)) {
//                                if (index2 == 0 && str2.StartsWith(str3))
//                                    flag2 = true;
//                            }
//                            else {
//                                flag1 = false;
//                                break;
//                            }
//                        }
//                        if (flag1) {
//                            if (flag2)
//                                list1.Add(element);
//                            else
//                                list2.Add(element);
//                        }
//                    }
//                }
//                list1.Sort();
//                list2.Sort();
//                List<AddComponentWindow.Element> list3 = new List<AddComponentWindow.Element>();
//                list3.Add((AddComponentWindow.Element)new AddComponentWindow.GroupElement(0, "Search"));
//                list3.AddRange((IEnumerable<AddComponentWindow.Element>)list1);
//                list3.AddRange((IEnumerable<AddComponentWindow.Element>)list2);
//                list3.Add(this.m_Tree[this.m_Tree.Length - 1]);
//                this.m_SearchResultTree = list3.ToArray();
//                this.m_Stack.Clear();
//                this.m_Stack.Add(this.m_SearchResultTree[0] as AddComponentWindow.GroupElement);
//                if (this.GetChildren(this.activeTree, (AddComponentWindow.Element)this.activeParent).Count >= 1)
//                    this.activeParent.selectedIndex = 0;
//                else
//                    this.activeParent.selectedIndex = -1;
//            }
//        }

//        private AddComponentWindow.GroupElement GetElementRelative(int rel) {
//            int index = this.m_Stack.Count + rel - 1;
//            if (index < 0)
//                return (AddComponentWindow.GroupElement)null;
//            return this.m_Stack[index];
//        }

//        private void GoToParent() {
//            if (this.m_Stack.Count <= 1)
//                return;
//            this.m_AnimTarget = 0;
//            this.m_LastTime = DateTime.Now.Ticks;
//        }

//        private void GoToChild(AddComponentWindow.Element e, bool addIfComponent) {
//            if (e is AddComponentWindow.NewScriptElement && !this.hasSearch) {
//                this.m_ClassName = AssetDatabase.GenerateUniqueAssetPath((e as AddComponentWindow.NewScriptElement).TargetPath());
//                this.m_ClassName = Path.GetFileNameWithoutExtension(this.m_ClassName);
//            }
//            if (e is AddComponentWindow.ComponentElement) {
//                if (!addIfComponent)
//                    return;
//                EditorApplication.ExecuteMenuItemOnGameObjects(((AddComponentWindow.ComponentElement)e).menuPath, this.m_GameObjects);
//                this.Close();
//            }
//            else {
//                if (this.hasSearch && !(e is AddComponentWindow.NewScriptElement))
//                    return;
//                this.m_LastTime = DateTime.Now.Ticks;
//                if (this.m_AnimTarget == 0) {
//                    this.m_AnimTarget = 1;
//                }
//                else {
//                    if ((double)this.m_Anim != 1.0)
//                        return;
//                    this.m_Anim = 0.0f;
//                    this.m_Stack.Add(e as AddComponentWindow.GroupElement);
//                }
//            }
//        }

//        private void ListGUI(AddComponentWindow.Element[] tree, float anim, AddComponentWindow.GroupElement parent, AddComponentWindow.GroupElement grandParent) {
//            anim = Mathf.Floor(anim) + Mathf.SmoothStep(0.0f, 1f, Mathf.Repeat(anim, 1f));
//            Rect position1 = this.position;
//            position1.x = (float)((double)this.position.width * (1.0 - (double)anim) + 1.0);
//            position1.y = 30f;
//            position1.height -= 30f;
//            position1.width -= 2f;
//            GUILayout.BeginArea(position1);
//            Rect rect = GUILayoutUtility.GetRect(10f, 25f);
//            string name = parent.name;
//            GUI.Label(rect, name, AddComponentWindow.s_Styles.header);
//            if (grandParent != null) {
//                Rect position2 = new Rect(rect.x + 4f, rect.y + 7f, 13f, 13f);
//                if (Event.current.type == EventType.Repaint)
//                    AddComponentWindow.s_Styles.leftArrow.Draw(position2, false, false, false, false);
//                if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition)) {
//                    this.GoToParent();
//                    Event.current.Use();
//                }
//            }
//            if (parent is AddComponentWindow.NewScriptElement)
//                (parent as AddComponentWindow.NewScriptElement).OnGUI();
//            else
//                this.ListGUI(tree, parent);
//            GUILayout.EndArea();
//        }

//        private void ListGUI(AddComponentWindow.Element[] tree, AddComponentWindow.GroupElement parent) {
//            parent.scroll = GUILayout.BeginScrollView(parent.scroll);
//            EditorGUIUtility.SetIconSize(new Vector2(16f, 16f));
//            List<AddComponentWindow.Element> children = this.GetChildren(tree, (AddComponentWindow.Element)parent);
//            Rect rect1 = new Rect();
//            for (int index1 = 0; index1 < children.Count; ++index1) {
//                AddComponentWindow.Element e = children[index1];
//                double num1 = 16.0;
//                double num2 = 20.0;
//                GUILayoutOption[] guiLayoutOptionArray = new GUILayoutOption[1];
//                int index2 = 0;
//                GUILayoutOption guiLayoutOption = GUILayout.ExpandWidth(true);
//                guiLayoutOptionArray[index2] = guiLayoutOption;
//                Rect rect2 = GUILayoutUtility.GetRect((float)num1, (float)num2, guiLayoutOptionArray);
//                if ((Event.current.type == EventType.MouseMove || Event.current.type == EventType.MouseDown) && (parent.selectedIndex != index1 && rect2.Contains(Event.current.mousePosition))) {
//                    parent.selectedIndex = index1;
//                    this.Repaint();
//                }
//                bool flag = false;
//                if (index1 == parent.selectedIndex) {
//                    flag = true;
//                    rect1 = rect2;
//                }
//                if (Event.current.type == EventType.Repaint) {
//                    (!(e is AddComponentWindow.ComponentElement) ? AddComponentWindow.s_Styles.groupButton : AddComponentWindow.s_Styles.componentButton).Draw(rect2, e.content, false, false, flag, flag);
//                    if (!(e is AddComponentWindow.ComponentElement)) {
//                        Rect position = new Rect((float)((double)rect2.x + (double)rect2.width - 13.0), rect2.y + 4f, 13f, 13f);
//                        AddComponentWindow.s_Styles.rightArrow.Draw(position, false, false, false, false);
//                    }
//                }
//                if (Event.current.type == EventType.MouseDown && rect2.Contains(Event.current.mousePosition)) {
//                    Event.current.Use();
//                    parent.selectedIndex = index1;
//                    this.GoToChild(e, true);
//                }
//            }
//            EditorGUIUtility.SetIconSize(Vector2.zero);
//            GUILayout.EndScrollView();
//            if (!this.m_ScrollToSelected || Event.current.type != EventType.Repaint)
//                return;
//            this.m_ScrollToSelected = false;
//            Rect lastRect = GUILayoutUtility.GetLastRect();
//            if ((double)rect1.yMax - (double)lastRect.height > (double)parent.scroll.y) {
//                parent.scroll.y = rect1.yMax - lastRect.height;
//                this.Repaint();
//            }
//            if ((double)rect1.y >= (double)parent.scroll.y)
//                return;
//            parent.scroll.y = rect1.y;
//            this.Repaint();
//        }

//        private List<AddComponentWindow.Element> GetChildren(AddComponentWindow.Element[] tree, AddComponentWindow.Element parent) {
//            List<AddComponentWindow.Element> list = new List<AddComponentWindow.Element>();
//            int num = -1;
//            int index;
//            for (index = 0; index < tree.Length; ++index) {
//                if (tree[index] == parent) {
//                    num = parent.level + 1;
//                    ++index;
//                    break;
//                }
//            }
//            if (num == -1)
//                return list;
//            for (; index < tree.Length; ++index) {
//                AddComponentWindow.Element element = tree[index];
//                if (element.level >= num) {
//                    if (element.level <= num || this.hasSearch)
//                        list.Add(element);
//                }
//                else
//                    break;
//            }
//            return list;
//        }

//        internal enum Language {
//            JavaScript,
//            CSharp,
//            Boo,
//        }

//        private class Element : IComparable {
//            public int level;
//            public GUIContent content;

//            public string name {
//                get {
//                    return this.content.text;
//                }
//            }

//            public int CompareTo(object o) {
//                return this.name.CompareTo((o as AddComponentWindow.Element).name);
//            }
//        }

//        private class ComponentElement : AddComponentWindow.Element {
//            public string typeName;
//            public string menuPath;

//            public ComponentElement(int level, string name, string menuPath, string commandString) {
//                this.level = level;
//                this.typeName = name.Replace(" ", string.Empty);
//                this.menuPath = menuPath;
//                if (commandString.StartsWith("SCRIPT")) {
//                    Texture image = (Texture)AssetPreview.GetMiniThumbnail(EditorUtility.InstanceIDToObject(int.Parse(commandString.Substring(6))));
//                    this.content = new GUIContent(name, image);
//                }
//                else {
//                    int classID = int.Parse(commandString);
//                    this.content = new GUIContent(name, (Texture)AssetPreview.GetMiniTypeThumbnailFromClassID(classID));
//                }
//            }
//        }

//        [Serializable]
//        private class GroupElement : AddComponentWindow.Element {
//            public Vector2 scroll;
//            public int selectedIndex;

//            public GroupElement(int level, string name) {
//                this.level = level;
//                this.content = new GUIContent(name);
//            }
//        }

//        private class NewScriptElement : AddComponentWindow.GroupElement {
//            private const string kResourcesTemplatePath = "Resources/ScriptTemplates";
//            private char[] kInvalidPathChars;
//            private char[] kPathSepChars;
//            private string m_Directory;

//            private string extension {
//                get {
//                    switch (AddComponentWindow.s_Lang) {
//                        case AddComponentWindow.Language.JavaScript:
//                            return "js";
//                        case AddComponentWindow.Language.CSharp:
//                            return "cs";
//                        case AddComponentWindow.Language.Boo:
//                            return "boo";
//                        default:
//                            throw new ArgumentOutOfRangeException();
//                    }
//                }
//            }

//            private string templatePath {
//                get {
//                    string path1 = Path.Combine(EditorApplication.applicationContentsPath, "Resources/ScriptTemplates");
//                    switch (AddComponentWindow.s_Lang) {
//                        case AddComponentWindow.Language.JavaScript:
//                            return Path.Combine(path1, "80-Javascript-NewBehaviourScript.js.txt");
//                        case AddComponentWindow.Language.CSharp:
//                            return Path.Combine(path1, "81-C# Script-NewBehaviourScript.cs.txt");
//                        case AddComponentWindow.Language.Boo:
//                            return Path.Combine(path1, "82-Boo Script-NewBehaviourScript.boo.txt");
//                        default:
//                            throw new ArgumentOutOfRangeException();
//                    }
//                }
//            }

//            public NewScriptElement() {
//                char[] chArray = new char[2];
//                int index1 = 0;
//                int num1 = 47;
//                chArray[index1] = (char)num1;
//                int index2 = 1;
//                int num2 = 92;
//                chArray[index2] = (char)num2;
//                this.kPathSepChars = chArray;
//                this.m_Directory = string.Empty;
//                // ISSUE: explicit constructor call
//                base(1, "New Script");
//            }

//            public void OnGUI() {
//                GUILayout.Label("Name", EditorStyles.label, new GUILayoutOption[0]);
//                EditorGUI.FocusTextInControl("NewScriptName");
//                GUI.SetNextControlName("NewScriptName");
//                AddComponentWindow.className = EditorGUILayout.TextField(AddComponentWindow.className);
//                EditorGUILayout.Space();
//                AddComponentWindow.Language language = (AddComponentWindow.Language)EditorGUILayout.EnumPopup("Language", (Enum)AddComponentWindow.s_Lang, new GUILayoutOption[0]);
//                if (language != AddComponentWindow.s_Lang) {
//                    AddComponentWindow.s_Lang = language;
//                    EditorPrefs.SetInt("NewScriptLanguage", (int)language);
//                }
//                EditorGUILayout.Space();
//                bool flag = this.CanCreate();
//                if (!flag && AddComponentWindow.className != string.Empty)
//                    GUILayout.Label(this.GetError(), EditorStyles.helpBox, new GUILayoutOption[0]);
//                GUILayout.FlexibleSpace();
//                EditorGUI.BeginDisabledGroup(!flag);
//                if (GUILayout.Button("Create and Add"))
//                    this.Create();
//                EditorGUI.EndDisabledGroup();
//                EditorGUILayout.Space();
//            }

//            public bool CanCreate() {
//                if (AddComponentWindow.className.Length > 0 && !File.Exists(this.TargetPath()) && (!this.ClassAlreadyExists() && !this.ClassNameIsInvalid()))
//                    return !this.InvalidTargetPath();
//                return false;
//            }

//            private string GetError() {
//                string str = string.Empty;
//                if (AddComponentWindow.className != string.Empty) {
//                    if (File.Exists(this.TargetPath()))
//                        str = "A script called \"" + AddComponentWindow.className + "\" already exists at that path.";
//                    else if (this.ClassAlreadyExists())
//                        str = "A class called \"" + AddComponentWindow.className + "\" already exists.";
//                    else if (this.ClassNameIsInvalid())
//                        str = "The script name may only consist of a-z, A-Z, 0-9, _.";
//                    else if (this.InvalidTargetPath())
//                        str = "The folder path contains invalid characters.";
//                }
//                return str;
//            }

//            public void Create() {
//                if (!this.CanCreate())
//                    return;
//                this.CreateScript();
//                foreach (GameObject gameObject in AddComponentWindow.gameObjects) {
//                    MonoScript script = AssetDatabase.LoadAssetAtPath(this.TargetPath(), typeof(MonoScript)) as MonoScript;
//                    script.SetScriptTypeWasJustCreatedFromComponentMenu();
//                    InternalEditorUtility.AddScriptComponentUnchecked(gameObject, script);
//                }
//                AddComponentWindow.s_AddComponentWindow.Close();
//            }

//            private bool InvalidTargetPath() {
//                return this.m_Directory.IndexOfAny(this.kInvalidPathChars) >= 0 || Enumerable.Contains<string>((IEnumerable<string>)this.TargetDir().Split(this.kPathSepChars, StringSplitOptions.None), string.Empty);
//            }

//            public string TargetPath() {
//                return Path.Combine(this.TargetDir(), AddComponentWindow.className + "." + this.extension);
//            }

//            private string TargetDir() {
//                return Path.Combine("Assets", this.m_Directory.Trim(this.kPathSepChars));
//            }

//            private bool ClassNameIsInvalid() {
//                return !CodeGenerator.IsValidLanguageIndependentIdentifier(AddComponentWindow.className);
//            }

//            private bool ClassExists(string className) {
//                // ISSUE: object of a compiler-generated type is created
//                // ISSUE: reference to a compiler-generated method
//                return Enumerable.Any<Assembly>((IEnumerable<Assembly>)AppDomain.CurrentDomain.GetAssemblies(), new Func<Assembly, bool>(new AddComponentWindow.NewScriptElement.\u003CClassExists\u003Ec__AnonStorey44()
//                {
//                    className = className
//                });
//            }

//            private bool ClassAlreadyExists() {
//                if (AddComponentWindow.className == string.Empty)
//                    return false;
//                return this.ClassExists(AddComponentWindow.className);
//            }

//            private void CreateScript() {
//                ProjectWindowUtil.CreateScriptAssetFromTemplate(this.TargetPath(), this.templatePath);
//                AssetDatabase.Refresh();
//            }
//        }

//        private class Styles {
//            public GUIStyle header = new GUIStyle(EditorStyles.inspectorBig);
//            public GUIStyle componentButton = new GUIStyle((GUIStyle)"PR Label");
//            public GUIStyle background = (GUIStyle)"grey_border";
//            public GUIStyle previewBackground = (GUIStyle)"PopupCurveSwatchBackground";
//            public GUIStyle previewHeader = new GUIStyle(EditorStyles.label);
//            public GUIStyle previewText = new GUIStyle(EditorStyles.wordWrappedLabel);
//            public GUIStyle rightArrow = (GUIStyle)"AC RightArrow";
//            public GUIStyle leftArrow = (GUIStyle)"AC LeftArrow";
//            public GUIStyle groupButton;

//            public Styles() {
//                this.header.font = EditorStyles.boldLabel.font;
//                this.componentButton.alignment = TextAnchor.MiddleLeft;
//                this.componentButton.padding.left -= 15;
//                this.componentButton.fixedHeight = 20f;
//                this.groupButton = new GUIStyle(this.componentButton);
//                this.groupButton.padding.left += 17;
//                this.previewText.padding.left += 3;
//                this.previewText.padding.right += 3;
//                ++this.previewHeader.padding.left;
//                this.previewHeader.padding.right += 3;
//                this.previewHeader.padding.top += 3;
//                this.previewHeader.padding.bottom += 2;
//            }
//        }
//    }
//}
