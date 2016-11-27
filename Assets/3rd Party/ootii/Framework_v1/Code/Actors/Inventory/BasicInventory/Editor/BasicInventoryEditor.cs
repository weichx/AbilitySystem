using System;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using com.ootii.Actors.Inventory;
using com.ootii.Base;
using com.ootii.Helpers;
using com.ootii.Input;

[CanEditMultipleObjects]
[CustomEditor(typeof(BasicInventory))]
public class BasicInventoryEditor : Editor
{
    // Helps us keep track of when the list needs to be saved. This
    // is important since some changes happen in scene.
    private bool mIsDirty;

    // The actual class we're storing
    private BasicInventory mTarget;
    private SerializedObject mTargetSO;

    // List object for our Items
    private ReorderableList mItemList;
    private ReorderableList mSlotList;
    private ReorderableList mWeaponSetList;
    private ReorderableList mWeaponSetItemList;

    /// <summary>
    /// Called when the object is selected in the editor
    /// </summary>
    private void OnEnable()
    {
        // Grab the serialized objects
        mTarget = (BasicInventory)target;
        mTargetSO = new SerializedObject(target);

        // Create the list of items to display
        InstantiateItemList();
        InstantiateSlotList();
        InstantiateWeaponSetList();

        // Setup the input
        if (!TestInputManagerSettings())
        {
            CreateInputManagerSettings();
        }
    }

    /// <summary>
    /// This function is called when the scriptable object goes out of scope.
    /// </summary>
    private void OnDisable()
    {
    }

    /// <summary>
    /// Called when the inspector needs to draw
    /// </summary>
    public override void OnInspectorGUI()
    {
        // Pulls variables from runtime so we have the latest values.
        mTargetSO.Update();

        GUILayout.Space(5);

        EditorHelper.DrawInspectorTitle("ootii Basic Inventory");

        EditorHelper.DrawInspectorDescription("Very basic inventory solution. It lists the items and slots and manages input, but can be replaced with your inventory solution.", MessageType.None);

        GUILayout.Space(5f);

        if (EditorHelper.BoolField("Is Enabled", "Determines if the inventory runs the update to process user input.", mTarget.IsEnabled, mTarget))
        {
            mIsDirty = true;
            mTarget.IsEnabled = EditorHelper.FieldBoolValue;
        }

        GUILayout.Space(5f);

        if (EditorHelper.IntField("Weapon Set Index", "Index of the active weapon set.", mTarget.ActiveWeaponSet, mTarget))
        {
            mIsDirty = true;
            mTarget._ActiveWeaponSet = EditorHelper.FieldIntValue;
        }

        GUILayout.Space(5f);

        EditorGUILayout.LabelField("Input", EditorStyles.boldLabel, GUILayout.Height(16f));

        EditorGUILayout.BeginVertical(EditorHelper.Box);

        EditorGUILayout.BeginHorizontal();

        GameObject lNewInputSourceOwner = EditorHelper.InterfaceOwnerField<IInputSource>(new GUIContent("Input Source", "Input source we'll use to get key presses, mouse movement, etc. This GameObject should have a component implementing the IInputSource interface."), mTarget.InputSourceOwner, true);
        if (lNewInputSourceOwner != mTarget.InputSourceOwner)
        {
            mIsDirty = true;
            mTarget.InputSourceOwner = lNewInputSourceOwner;
        }

        GUILayout.Space(2);

        EditorGUILayout.LabelField(new GUIContent("Find", "Determines if we attempt to automatically find the input source at startup if one isn't set."), GUILayout.Width(30));

        bool lNewAutoFindInputSource = EditorGUILayout.Toggle(mTarget.AutoFindInputSource, GUILayout.Width(16));
        if (lNewAutoFindInputSource != mTarget.AutoFindInputSource)
        {
            mIsDirty = true;
            mTarget.AutoFindInputSource = lNewAutoFindInputSource;
        }

        EditorGUILayout.EndHorizontal();

        GUILayout.Space(5f);

        if (EditorHelper.BoolField("Allow Motion Tests", "Determines if we allow motions to test if they should activate themselves.", mTarget.AllowMotionSelfActivation, mTarget))
        {
            mIsDirty = true;
            mTarget.AllowMotionSelfActivation = EditorHelper.FieldBoolValue;
        }

        if (EditorHelper.BoolField("Synch Number keys", "Determines if we use the number keys to toggle corresponding weapon sets.", mTarget.UseNumberKeys, mTarget))
        {
            mIsDirty = true;
            mTarget.UseNumberKeys = EditorHelper.FieldBoolValue;
        }

        if (EditorHelper.TextField("Toggle Set Alias", "Action alias to determine if we equip/store the active weapon set.", mTarget.ToggleWeaponSetAlias, mTarget))
        {
            mIsDirty = true;
            mTarget.ToggleWeaponSetAlias = EditorHelper.FieldStringValue;
        }

        if (EditorHelper.TextField("Shift Set Alias", "Action alias to load the prev/next weapon set in the list.", mTarget.ShiftWeaponSetAlias, mTarget))
        {
            mIsDirty = true;
            mTarget.ShiftWeaponSetAlias = EditorHelper.FieldStringValue;
        }

        EditorGUILayout.EndVertical();

        GUILayout.Space(5f);

        // Show the items
        //EditorHelper.DrawSmallTitle("Items");
        EditorGUILayout.LabelField("Items", EditorStyles.boldLabel, GUILayout.Height(16f));

        GUILayout.BeginVertical(EditorHelper.GroupBox);
        EditorHelper.DrawInspectorDescription("Items define what the character owns as well as properties that help manage the items.", MessageType.None);

        mItemList.DoLayoutList();

        if (mItemList.index >= 0)
        {
            GUILayout.Space(5f);
            GUILayout.BeginVertical(EditorHelper.Box);

            bool lListIsDirty = DrawItemDetailItem(mTarget.Items[mItemList.index]);
            if (lListIsDirty) { mIsDirty = true; }

            GUILayout.EndVertical();
        }

        EditorGUILayout.EndVertical();

        GUILayout.Space(5f);

        // Show the slots
        EditorGUILayout.LabelField("Slots", EditorStyles.boldLabel, GUILayout.Height(16f));

        GUILayout.BeginVertical(EditorHelper.GroupBox);
        EditorHelper.DrawInspectorDescription("Slots define where we can place items. They can also be bone and mount point names.", MessageType.None);

        mSlotList.DoLayoutList();

        if (mSlotList.index >= 0)
        {
            GUILayout.Space(5f);
            GUILayout.BeginVertical(EditorHelper.Box);

            bool lListIsDirty = DrawSlotDetailItem(mTarget.Slots[mSlotList.index]);
            if (lListIsDirty) { mIsDirty = true; }

            GUILayout.EndVertical();
        }

        EditorGUILayout.EndVertical();

        GUILayout.Space(5f);

        // Show the weapon sets
        EditorGUILayout.LabelField("Weapon Sets", EditorStyles.boldLabel, GUILayout.Height(16f));

        GUILayout.BeginVertical(EditorHelper.GroupBox);
        EditorHelper.DrawInspectorDescription("Collections of items that we equip (and store) together.", MessageType.None);

        mWeaponSetList.DoLayoutList();

        if (mWeaponSetList.index >= 0)
        {
            GUILayout.Space(5f);
            GUILayout.BeginVertical(EditorHelper.Box);

            bool lListIsDirty = DrawWeaponSetDetailItem(mTarget.WeaponSets[mWeaponSetList.index]);
            if (lListIsDirty) { mIsDirty = true; }

            GUILayout.EndVertical();
        }

        EditorGUILayout.EndVertical();

        //EditorHelper.DrawLine();
        //GUILayout.Space(5f);
        //DrawDefaultInspector();

        // If there is a change... update.
        if (mIsDirty)
        {
            // Flag the object as needing to be saved
            EditorUtility.SetDirty(mTarget);

#if UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
            EditorApplication.MarkSceneDirty();
#else
            if (!EditorApplication.isPlaying)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            }
#endif

            // Pushes the values back to the runtime so it has the changes
            mTargetSO.ApplyModifiedProperties();

            // Clear out the dirty flag
            mIsDirty = false;
        }
    }

    #region Items

    /// <summary>
    /// Create the reorderable list
    /// </summary>
    private void InstantiateItemList()
    {
        mItemList = new ReorderableList(mTarget.Items, typeof(BasicInventoryItem), true, true, true, true);
        mItemList.drawHeaderCallback = DrawItemListHeader;
        mItemList.drawFooterCallback = DrawItemListFooter;
        mItemList.drawElementCallback = DrawItemListItem;
        mItemList.onAddCallback = OnItemListItemAdd;
        mItemList.onRemoveCallback = OnItemListItemRemove;
        mItemList.onSelectCallback = OnItemListItemSelect;
        mItemList.onReorderCallback = OnItemListReorder;
        mItemList.footerHeight = 17f;

        if (mTarget.EditorItemIndex >= 0 && mTarget.EditorItemIndex < mItemList.count)
        {
            mItemList.index = mTarget.EditorItemIndex;
        }
    }

    /// <summary>
    /// Header for the list
    /// </summary>
    /// <param name="rRect"></param>
    private void DrawItemListHeader(Rect rRect)
    {
        EditorGUI.LabelField(rRect, "Items");

        Rect lNoteRect = new Rect(rRect.width + 12f, rRect.y, 11f, rRect.height);
        EditorGUI.LabelField(lNoteRect, "X", EditorStyles.miniLabel);

        if (GUI.Button(rRect, "", EditorStyles.label))
        {
            mItemList.index = -1;
            OnItemListItemSelect(mItemList);
        }
    }

    /// <summary>
    /// Allows us to draw each item in the list
    /// </summary>
    /// <param name="rRect"></param>
    /// <param name="rIndex"></param>
    /// <param name="rIsActive"></param>
    /// <param name="rIsFocused"></param>
    private void DrawItemListItem(Rect rRect, int rIndex, bool rIsActive, bool rIsFocused)
    {
        if (rIndex < mTarget.Items.Count)
        {
            BasicInventoryItem lItem = mTarget.Items[rIndex];

            rRect.y += 2;

            Rect lNameRect = new Rect(rRect.x, rRect.y, rRect.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(lNameRect, lItem.ID);
        }
    }

    /// <summary>
    /// Footer for the list
    /// </summary>
    /// <param name="rRect"></param>
    private void DrawItemListFooter(Rect rRect)
    {
        Rect lAddRect = new Rect(rRect.x + rRect.width - 28 - 28 - 1, rRect.y + 1, 28, 15);
        if (GUI.Button(lAddRect, new GUIContent("+", "Add Item."), EditorStyles.miniButtonLeft)) { OnItemListItemAdd(mItemList); }

        Rect lDeleteRect = new Rect(lAddRect.x + lAddRect.width, lAddRect.y, 28, 15);
        if (GUI.Button(lDeleteRect, new GUIContent("-", "Delete Item."), EditorStyles.miniButtonRight)) { OnItemListItemRemove(mItemList); };
    }

    /// <summary>
    /// Allows us to add to a list
    /// </summary>
    /// <param name="rList"></param>
    private void OnItemListItemAdd(ReorderableList rList)
    {
        BasicInventoryItem lItem = new BasicInventoryItem();

        mTarget.Items.Add(lItem);

        mItemList.index = mTarget.Items.Count - 1;
        OnItemListItemSelect(rList);

        mIsDirty = true;
    }

    /// <summary>
    /// Allows us process when a list is selected
    /// </summary>
    /// <param name="rList"></param>
    private void OnItemListItemSelect(ReorderableList rList)
    {
        mTarget.EditorItemIndex = rList.index;
    }

    /// <summary>
    /// Allows us to stop before removing the item
    /// </summary>
    /// <param name="rList"></param>
    private void OnItemListItemRemove(ReorderableList rList)
    {
        if (EditorUtility.DisplayDialog("Warning!", "Are you sure you want to delete the item?", "Yes", "No"))
        {
            int rIndex = rList.index;

            rList.index--;
            mTarget.Items.RemoveAt(rIndex);

            OnItemListItemSelect(rList);

            mIsDirty = true;
        }
    }

    /// <summary>
    /// Allows us to process after the motions are reordered
    /// </summary>
    /// <param name="rList"></param>
    private void OnItemListReorder(ReorderableList rList)
    {
        mIsDirty = true;
    }

    /// <summary>
    /// Renders the currently selected step
    /// </summary>
    /// <param name="rStep"></param>
    private bool DrawItemDetailItem(BasicInventoryItem rItem)
    {
        bool lIsDirty = false;

        EditorHelper.DrawSmallTitle(rItem.ID.Length > 0 ? rItem.ID : "Inventory Item");

        if (EditorHelper.TextField("ID", "Unique ID of the Item", rItem.ID, mTarget))
        {
            lIsDirty = true;
            rItem.ID = EditorHelper.FieldStringValue;
        }

        if (EditorHelper.TextField("Equip Motion", "Name of the motion to run in order to equip the item and set the character state.", rItem.EquipMotion, mTarget))
        {
            lIsDirty = true;
            rItem.EquipMotion = EditorHelper.FieldStringValue;
        }

        if (EditorHelper.TextField("Store Motion", "Name of the motion to run in order to store the item and set the character state. Ensure the instance has a parent even if it's a dummy object.", rItem.StoreMotion, mTarget))
        {
            lIsDirty = true;
            rItem.StoreMotion = EditorHelper.FieldStringValue;
        }

        if (EditorHelper.ObjectField<GameObject>("Instance", "Scene object that is the item. This is used to pre-instantiate the object.", rItem.Instance, mTarget))
        {
            lIsDirty = true;
            rItem.Instance = EditorHelper.FieldObjectValue as GameObject;
        }

        string lNewResourcePath = EditorHelper.FileSelect(new GUIContent("Resource Path", "Path to the definition we'll use to instantiate the item."), rItem.ResourcePath, "fbx,prefab");
        if (lNewResourcePath != rItem.ResourcePath)
        {
            lIsDirty = true;
            rItem.ResourcePath = lNewResourcePath;
        }

        return lIsDirty;
    }

    #endregion

    #region Slots

    /// <summary>
    /// Create the reorderable list
    /// </summary>
    private void InstantiateSlotList()
    {
        mSlotList = new ReorderableList(mTarget.Slots, typeof(BasicInventorySlot), true, true, true, true);
        mSlotList.drawHeaderCallback = DrawSlotListHeader;
        mSlotList.drawFooterCallback = DrawSlotListFooter;
        mSlotList.drawElementCallback = DrawSlotListItem;
        mSlotList.onAddCallback = OnSlotListItemAdd;
        mSlotList.onRemoveCallback = OnSlotListItemRemove;
        mSlotList.onSelectCallback = OnSlotListItemSelect;
        mSlotList.onReorderCallback = OnSlotListReorder;
        mSlotList.footerHeight = 17f;

        if (mTarget.EditorSlotIndex >= 0 && mTarget.EditorSlotIndex < mSlotList.count)
        {
            mSlotList.index = mTarget.EditorSlotIndex;
        }
    }

    /// <summary>
    /// Header for the list
    /// </summary>
    /// <param name="rRect"></param>
    private void DrawSlotListHeader(Rect rRect)
    {
        EditorGUI.LabelField(rRect, "Slots");

        Rect lNoteRect = new Rect(rRect.width + 12f, rRect.y, 11f, rRect.height);
        EditorGUI.LabelField(lNoteRect, "X", EditorStyles.miniLabel);

        if (GUI.Button(rRect, "", EditorStyles.label))
        {
            mSlotList.index = -1;
            OnSlotListItemSelect(mSlotList);
        }
    }

    /// <summary>
    /// Allows us to draw each Slot in the list
    /// </summary>
    /// <param name="rRect"></param>
    /// <param name="rIndex"></param>
    /// <param name="rIsActive"></param>
    /// <param name="rIsFocused"></param>
    private void DrawSlotListItem(Rect rRect, int rIndex, bool rIsActive, bool rIsFocused)
    {
        if (rIndex < mTarget.Slots.Count)
        {
            BasicInventorySlot lSlot = mTarget.Slots[rIndex];

            rRect.y += 2;

            Rect lNameRect = new Rect(rRect.x, rRect.y, rRect.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(lNameRect, lSlot.ID);
        }
    }

    /// <summary>
    /// Footer for the list
    /// </summary>
    /// <param name="rRect"></param>
    private void DrawSlotListFooter(Rect rRect)
    {
        Rect lAddRect = new Rect(rRect.x + rRect.width - 28 - 28 - 1, rRect.y + 1, 28, 15);
        if (GUI.Button(lAddRect, new GUIContent("+", "Add slot."), EditorStyles.miniButtonLeft)) { OnSlotListItemAdd(mSlotList); }

        Rect lDeleteRect = new Rect(lAddRect.x + lAddRect.width, lAddRect.y, 28, 15);
        if (GUI.Button(lDeleteRect, new GUIContent("-", "Delete slot."), EditorStyles.miniButtonRight)) { OnSlotListItemRemove(mSlotList); };
    }

    /// <summary>
    /// Allows us to add to a list
    /// </summary>
    /// <param name="rList"></param>
    private void OnSlotListItemAdd(ReorderableList rList)
    {
        BasicInventorySlot lSlot = new BasicInventorySlot();

        mTarget.Slots.Add(lSlot);

        mSlotList.index = mTarget.Slots.Count - 1;
        OnSlotListItemSelect(rList);

        mIsDirty = true;
    }

    /// <summary>
    /// Allows us process when a list is selected
    /// </summary>
    /// <param name="rList"></param>
    private void OnSlotListItemSelect(ReorderableList rList)
    {
        mTarget.EditorSlotIndex = rList.index;
    }

    /// <summary>
    /// Allows us to stop before removing the item
    /// </summary>
    /// <param name="rList"></param>
    private void OnSlotListItemRemove(ReorderableList rList)
    {
        if (EditorUtility.DisplayDialog("Warning!", "Are you sure you want to delete the item?", "Yes", "No"))
        {
            int rIndex = rList.index;

            rList.index--;
            mTarget.Slots.RemoveAt(rIndex);

            OnSlotListItemSelect(rList);

            mIsDirty = true;
        }
    }

    /// <summary>
    /// Allows us to process after the motions are reordered
    /// </summary>
    /// <param name="rList"></param>
    private void OnSlotListReorder(ReorderableList rList)
    {
        mIsDirty = true;
    }

    /// <summary>
    /// Renders the currently selected step
    /// </summary>
    /// <param name="rStep"></param>
    private bool DrawSlotDetailItem(BasicInventorySlot rSlot)
    {
        bool lIsDirty = false;

        EditorHelper.DrawSmallTitle(rSlot.ID.Length > 0 ? rSlot.ID : "Inventory Slot");

        if (EditorHelper.TextField("ID", "Unique ID of the slot.", rSlot.ID, mTarget))
        {
            lIsDirty = true;
            rSlot.ID = EditorHelper.FieldStringValue;
        }

        if (EditorHelper.TextField("Item ID", "ID of the item in the slot.", rSlot.ItemID, mTarget))
        {
            lIsDirty = true;
            rSlot.ItemID = EditorHelper.FieldStringValue;
        }

        return lIsDirty;
    }

    #endregion

    #region Weapon Sets

    /// <summary>
    /// Create the reorderable list
    /// </summary>
    private void InstantiateWeaponSetList()
    {
        mWeaponSetList = new ReorderableList(mTarget.WeaponSets, typeof(BasicInventorySet), true, true, true, true);
        mWeaponSetList.drawHeaderCallback = DrawWeaponSetListHeader;
        mWeaponSetList.drawFooterCallback = DrawWeaponSetListFooter;
        mWeaponSetList.drawElementCallback = DrawWeaponSetListItem;
        mWeaponSetList.onAddCallback = OnWeaponSetListItemAdd;
        mWeaponSetList.onRemoveCallback = OnWeaponSetListItemRemove;
        mWeaponSetList.onSelectCallback = OnWeaponSetListItemSelect;
        mWeaponSetList.onReorderCallback = OnWeaponSetListReorder;
        mWeaponSetList.footerHeight = 17f;

        if (mTarget.EditorWeaponSetIndex >= 0 && mTarget.EditorWeaponSetIndex < mWeaponSetList.count)
        {
            mWeaponSetList.index = mTarget.EditorWeaponSetIndex;
            OnWeaponSetListItemSelect(mWeaponSetList);
        }
    }

    /// <summary>
    /// Header for the list
    /// </summary>
    /// <param name="rRect"></param>
    private void DrawWeaponSetListHeader(Rect rRect)
    {
        EditorGUI.LabelField(rRect, "Weapon Sets");

        Rect lNoteRect = new Rect(rRect.width + 12f, rRect.y, 11f, rRect.height);
        EditorGUI.LabelField(lNoteRect, "X", EditorStyles.miniLabel);

        if (GUI.Button(rRect, "", EditorStyles.label))
        {
            mWeaponSetList.index = -1;
            OnWeaponSetListItemSelect(mWeaponSetList);
        }
    }

    /// <summary>
    /// Allows us to draw each WeaponSet in the list
    /// </summary>
    /// <param name="rRect"></param>
    /// <param name="rIndex"></param>
    /// <param name="rIsActive"></param>
    /// <param name="rIsFocused"></param>
    private void DrawWeaponSetListItem(Rect rRect, int rIndex, bool rIsActive, bool rIsFocused)
    {
        if (rIndex < mTarget.WeaponSets.Count)
        {
            rRect.y += 2;

            Rect lNameRect = new Rect(rRect.x, rRect.y, rRect.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(lNameRect, "Weapon Set " + (rIndex + 1).ToString());
        }
    }

    /// <summary>
    /// Footer for the list
    /// </summary>
    /// <param name="rRect"></param>
    private void DrawWeaponSetListFooter(Rect rRect)
    {
        Rect lAddRect = new Rect(rRect.x + rRect.width - 28 - 28 - 1, rRect.y + 1, 28, 15);
        if (GUI.Button(lAddRect, new GUIContent("+", "Add Weapon Set."), EditorStyles.miniButtonLeft)) { OnWeaponSetListItemAdd(mWeaponSetList); }

        Rect lDeleteRect = new Rect(lAddRect.x + lAddRect.width, lAddRect.y, 28, 15);
        if (GUI.Button(lDeleteRect, new GUIContent("-", "Delete Weapon Set."), EditorStyles.miniButtonRight)) { OnWeaponSetListItemRemove(mWeaponSetList); };
    }

    /// <summary>
    /// Allows us to add to a list
    /// </summary>
    /// <param name="rList"></param>
    private void OnWeaponSetListItemAdd(ReorderableList rList)
    {
        BasicInventorySet lWeaponSet = new BasicInventorySet();

        mTarget.WeaponSets.Add(lWeaponSet);

        mWeaponSetList.index = mTarget.WeaponSets.Count - 1;
        OnWeaponSetListItemSelect(rList);

        mIsDirty = true;
    }

    /// <summary>
    /// Allows us process when a list is selected
    /// </summary>
    /// <param name="rList"></param>
    private void OnWeaponSetListItemSelect(ReorderableList rList)
    {
        mTarget.EditorWeaponSetIndex = rList.index;
        InstantiateWeaponSetItemList();
    }

    /// <summary>
    /// Allows us to stop before removing the item
    /// </summary>
    /// <param name="rList"></param>
    private void OnWeaponSetListItemRemove(ReorderableList rList)
    {
        if (EditorUtility.DisplayDialog("Warning!", "Are you sure you want to delete the item?", "Yes", "No"))
        {
            int rIndex = rList.index;

            rList.index--;
            mTarget.WeaponSets.RemoveAt(rIndex);

            OnWeaponSetListItemSelect(rList);

            mIsDirty = true;
        }
    }

    /// <summary>
    /// Allows us to process after the motions are reordered
    /// </summary>
    /// <param name="rList"></param>
    private void OnWeaponSetListReorder(ReorderableList rList)
    {
        mIsDirty = true;
    }

    /// <summary>
    /// Renders the currently selected step
    /// </summary>
    /// <param name="rStep"></param>
    private bool DrawWeaponSetDetailItem(BasicInventorySet rWeaponSet)
    {
        bool lIsDirty = false;

        int lWeaponSetIndex = mWeaponSetList.index;
        EditorHelper.DrawSmallTitle("Weapon Set " + (lWeaponSetIndex + 1).ToString());

        // Show the weapon sets
        if (mWeaponSetItemList != null)
        {
            GUILayout.BeginVertical(EditorHelper.GroupBox);
            mWeaponSetItemList.DoLayoutList();

            if (mWeaponSetItemList.index >= 0)
            {
                GUILayout.Space(5f);
                GUILayout.BeginVertical(EditorHelper.Box);

                bool lListIsDirty = DrawWeaponSetItemDetailItem(rWeaponSet.Items[mWeaponSetItemList.index]);
                if (lListIsDirty) { mIsDirty = true; }

                GUILayout.EndVertical();
            }

            EditorGUILayout.EndVertical();
        }

        return lIsDirty;
    }

    #endregion

    #region Weapon Set Items

    /// <summary>
    /// Create the reorderable list
    /// </summary>
    private void InstantiateWeaponSetItemList()
    {
        int lIndex = mWeaponSetList.index;

        if (lIndex < 0 || mTarget.WeaponSets.Count == 0)
        {
            mWeaponSetItemList = null;
            return;
        }

        if (lIndex >= mTarget.WeaponSets.Count)
        {
            mWeaponSetList.index = mTarget.WeaponSets.Count - 1;
            return;
        }

        mWeaponSetItemList = new ReorderableList(mTarget.WeaponSets[lIndex].Items, typeof(BasicInventorySet), true, true, true, true);
        mWeaponSetItemList.drawHeaderCallback = DrawWeaponSetItemListHeader;
        mWeaponSetItemList.drawFooterCallback = DrawWeaponSetItemListFooter;
        mWeaponSetItemList.drawElementCallback = DrawWeaponSetItemListItem;
        mWeaponSetItemList.onAddCallback = OnWeaponSetItemListItemAdd;
        mWeaponSetItemList.onRemoveCallback = OnWeaponSetItemListItemRemove;
        mWeaponSetItemList.onSelectCallback = OnWeaponSetItemListItemSelect;
        mWeaponSetItemList.onReorderCallback = OnWeaponSetItemListReorder;
        mWeaponSetItemList.footerHeight = 17f;

        if (mTarget.EditorWeaponSetItemIndex >= 0 && mTarget.EditorWeaponSetItemIndex < mWeaponSetItemList.count)
        {
            mWeaponSetItemList.index = mTarget.EditorWeaponSetItemIndex;
        }
    }

    /// <summary>
    /// Header for the list
    /// </summary>
    /// <param name="rRect"></param>
    private void DrawWeaponSetItemListHeader(Rect rRect)
    {
        EditorGUI.LabelField(rRect, "Weapon Set Items");

        Rect lNoteRect = new Rect(rRect.width + 19f, rRect.y, 11f, rRect.height);
        EditorGUI.LabelField(lNoteRect, "X", EditorStyles.miniLabel);

        if (GUI.Button(rRect, "", EditorStyles.label))
        {
            mWeaponSetItemList.index = -1;
            OnWeaponSetItemListItemSelect(mWeaponSetItemList);
        }
    }

    /// <summary>
    /// Allows us to draw each WeaponSetItem in the list
    /// </summary>
    /// <param name="rRect"></param>
    /// <param name="rIndex"></param>
    /// <param name="rIsActive"></param>
    /// <param name="rIsFocused"></param>
    private void DrawWeaponSetItemListItem(Rect rRect, int rIndex, bool rIsActive, bool rIsFocused)
    {
        if (rIndex < mTarget.WeaponSets[mWeaponSetList.index].Items.Count)
        {
            BasicInventorySetItem lWeaponSetItem = mTarget.WeaponSets[mWeaponSetList.index].Items[rIndex];

            rRect.y += 2;

            string lItem = (lWeaponSetItem.ItemID.Length > 0 ? lWeaponSetItem.ItemID : "[empty]");
            string lSlot = (lWeaponSetItem.SlotID.Length > 0 ? lWeaponSetItem.SlotID : "");

            float lWidth = rRect.width;
            if (lSlot.Length > 0) { lWidth = (rRect.width - 5) / 2f; }

            Rect lItemRect = new Rect(rRect.x, rRect.y, lWidth, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(lItemRect, lItem);

            if (lSlot.Length > 0)
            {
                Rect lSlotRect = new Rect(lItemRect.x + lItemRect.width + 5f, lItemRect.y, lWidth, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(lSlotRect, lSlot);
            }
        }
    }

    /// <summary>
    /// Footer for the list
    /// </summary>
    /// <param name="rRect"></param>
    private void DrawWeaponSetItemListFooter(Rect rRect)
    {
        Rect lAddRect = new Rect(rRect.x + rRect.width - 28 - 28 - 1, rRect.y + 1, 28, 15);
        if (GUI.Button(lAddRect, new GUIContent("+", "Add Weapon Set Item."), EditorStyles.miniButtonLeft)) { OnWeaponSetItemListItemAdd(mWeaponSetItemList); }

        Rect lDeleteRect = new Rect(lAddRect.x + lAddRect.width, lAddRect.y, 28, 15);
        if (GUI.Button(lDeleteRect, new GUIContent("-", "Delete Weapon Set Item."), EditorStyles.miniButtonRight)) { OnWeaponSetItemListItemRemove(mWeaponSetItemList); };
    }

    /// <summary>
    /// Allows us to add to a list
    /// </summary>
    /// <param name="rList"></param>
    private void OnWeaponSetItemListItemAdd(ReorderableList rList)
    {
        BasicInventorySetItem lWeaponSetItem = new BasicInventorySetItem();

        mTarget.WeaponSets[mWeaponSetList.index].Items.Add(lWeaponSetItem);

        mWeaponSetItemList.index = mTarget.WeaponSets[mWeaponSetList.index].Items.Count - 1;
        OnWeaponSetItemListItemSelect(rList);

        mIsDirty = true;
    }

    /// <summary>
    /// Allows us process when a list is selected
    /// </summary>
    /// <param name="rList"></param>
    private void OnWeaponSetItemListItemSelect(ReorderableList rList)
    {
        mTarget.EditorWeaponSetItemIndex = rList.index;
    }

    /// <summary>
    /// Allows us to stop before removing the item
    /// </summary>
    /// <param name="rList"></param>
    private void OnWeaponSetItemListItemRemove(ReorderableList rList)
    {
        if (EditorUtility.DisplayDialog("Warning!", "Are you sure you want to delete the item?", "Yes", "No"))
        {
            int rIndex = rList.index;

            rList.index--;
            mTarget.WeaponSets[mWeaponSetList.index].Items.RemoveAt(rIndex);

            OnWeaponSetItemListItemSelect(rList);

            mIsDirty = true;
        }
    }

    /// <summary>
    /// Allows us to process after the motions are reordered
    /// </summary>
    /// <param name="rList"></param>
    private void OnWeaponSetItemListReorder(ReorderableList rList)
    {
        mIsDirty = true;
    }

    /// <summary>
    /// Renders the currently selected step
    /// </summary>
    /// <param name="rStep"></param>
    private bool DrawWeaponSetItemDetailItem(BasicInventorySetItem rWeaponSetItem)
    {
        bool lIsDirty = false;


        EditorHelper.DrawSmallTitle(rWeaponSetItem.ItemID.Length > 0 ? rWeaponSetItem.ItemID : "Weapon Set Item");

        if (EditorHelper.TextField("Item ID", "ID of the item to be equipped.", rWeaponSetItem.ItemID, mTarget))
        {
            lIsDirty = true;
            rWeaponSetItem.ItemID = EditorHelper.FieldStringValue;
        }

        if (EditorHelper.TextField("Slot ID", "ID of the slot the item will be placed.", rWeaponSetItem.SlotID, mTarget))
        {
            lIsDirty = true;
            rWeaponSetItem.SlotID = EditorHelper.FieldStringValue;
        }

        if (EditorHelper.BoolField("Instantiate", "Determines if we'll instantiate the item at run-time.", rWeaponSetItem.Instantiate, mTarget))
        {
            lIsDirty = true;
            rWeaponSetItem.Instantiate = EditorHelper.FieldBoolValue;
        }

        return lIsDirty;
    }

    #endregion

    /// <summary>
    /// Test if we need to setup input manager entries
    /// </summary>
    /// <returns></returns>
    private bool TestInputManagerSettings()
    {
        if (!InputManagerHelper.IsDefined("Inventory Toggle")) { return false; }
        if (!InputManagerHelper.IsDefined("Inventory Shift")) { return false; }

        return true;
    }

    /// <summary>
    /// If the input manager entries don't exist, create them
    /// </summary>
    private void CreateInputManagerSettings()
    {
        if (!InputManagerHelper.IsDefined("Inventory Toggle"))
        {
            InputManagerEntry lEntry = new InputManagerEntry();
            lEntry.Name = "Inventory Toggle";
            lEntry.PositiveButton = "0";
            lEntry.Gravity = 100;
            lEntry.Dead = 0.3f;
            lEntry.Sensitivity = 100;
            lEntry.Type = InputManagerEntryType.KEY_MOUSE_BUTTON;
            lEntry.Axis = 1;
            lEntry.JoyNum = 0;

            InputManagerHelper.AddEntry(lEntry);
        }

        if (!InputManagerHelper.IsDefined("Inventory Shift"))
        {
            InputManagerEntry lEntry = new InputManagerEntry();
            lEntry.Name = "Inventory Shift";
            lEntry.NegativeButton = "-";
            lEntry.PositiveButton = "=";
            lEntry.Gravity = 100;
            lEntry.Dead = 0.3f;
            lEntry.Sensitivity = 100;
            lEntry.Type = InputManagerEntryType.KEY_MOUSE_BUTTON;
            lEntry.Axis = 1;
            lEntry.JoyNum = 0;
            InputManagerHelper.AddEntry(lEntry, true);

#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            lEntry = new InputManagerEntry();
            lEntry.Name = "Inventory Shift";
            lEntry.PositiveButton = "joystick button 7";
            lEntry.NegativeButton = "joystick button 8";
            lEntry.Gravity = 1000;
            lEntry.Dead = 0.001f;
            lEntry.Sensitivity = 1000;
            lEntry.Type = InputManagerEntryType.KEY_MOUSE_BUTTON;
            lEntry.Axis = 0;
            lEntry.JoyNum = 0;
            InputManagerHelper.AddEntry(lEntry, true);
#else
            lEntry = new InputManagerEntry();
            lEntry.Name = "Inventory Shift";
            lEntry.Gravity = 1;
            lEntry.Dead = 0.3f;
            lEntry.Sensitivity = 1;
            lEntry.Type = InputManagerEntryType.JOYSTICK_AXIS;
            lEntry.Axis = 6;
            lEntry.JoyNum = 0;
            InputManagerHelper.AddEntry(lEntry, true);
#endif
        }
    }

    /// <summary>
    /// Returns a friendly name for the type
    /// </summary>
    /// <param name="rType"></param>
    /// <returns></returns>
    private string GetFriendlyName(Type rType)
    {
        string lTypeName = rType.Name;
        object[] lMotionAttributes = rType.GetCustomAttributes(typeof(BaseNameAttribute), true);
        if (lMotionAttributes != null && lMotionAttributes.Length > 0) { lTypeName = ((BaseNameAttribute)lMotionAttributes[0]).Value; }

        return lTypeName;
    }

    /// <summary>
    /// Returns a friendly name for the type
    /// </summary>
    /// <param name="rType"></param>
    /// <returns></returns>
    private string GetDescription(Type rType)
    {
        string lDescription = "";
        object[] lMotionAttributes = rType.GetCustomAttributes(typeof(BaseDescriptionAttribute), true);
        if (lMotionAttributes != null && lMotionAttributes.Length > 0) { lDescription = ((BaseDescriptionAttribute)lMotionAttributes[0]).Value; }

        return lDescription;
    }
}
