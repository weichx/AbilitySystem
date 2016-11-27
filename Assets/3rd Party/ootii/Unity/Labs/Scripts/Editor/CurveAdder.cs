using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Animations;
using Object = UnityEngine.Object;

public class CurveAdder : EditorWindow
{
    // A class to manage individual clips and whether or not they should have curves added to them.
    public class ClipBoolPair
    {
        public bool applyCurve = true;
        public ModelImporterClipAnimation clip = new ModelImporterClipAnimation();


        // To display the name of the clip and whether or not curves should be added to it.
        public void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            applyCurve = EditorGUILayout.ToggleLeft(clip.name, applyCurve);
            EditorGUILayout.EndHorizontal();
        }


        // This is called when all the curves need to be set to the appropriate clips.
        public void Apply(ClipAnimationInfoCurve[] curves)
        {
            List<ClipAnimationInfoCurve> newCurves = new List<ClipAnimationInfoCurve>(clip.curves);
            for (int i = 0; i < curves.Length; i++)
            {
                if(applyCurve)
                    newCurves.Add(curves[i]);
            }
            clip.curves = newCurves.ToArray();
        }
    }


    // A class to store the importer that has the clips curves are to be applied to.
    public class FbxClips
    {
        public ModelImporter importer
        {
            get
            {
                return m_Importer;
            }
            set
            {
                m_Importer = value;

                if (m_Importer == null)
                    return;

                // Only continue if there are clips on the importer.
                if (m_Importer.clipAnimations == null)
                    return;

                // Create the array of ClipBoolPairs where the clips are those of the importer.
                clips = new ClipBoolPair[m_Importer.clipAnimations.Length];
                for (int i = 0; i < m_Importer.clipAnimations.Length; i++)
                {
                    if (m_Importer.clipAnimations[i] != null)
					{
						ClipBoolPair temp = new ClipBoolPair();
						temp.clip = m_Importer.clipAnimations[i];
                        clips[i] = temp;
					}
                }

                // If there are no cut clips, check for uncut clips instead.
                if (clips == null || clips.Length == 0)
                {
                    clips = new ClipBoolPair[m_Importer.defaultClipAnimations.Length];
                    for (int i = 0; i < m_Importer.defaultClipAnimations.Length; i++)
                    {
                        if (m_Importer.defaultClipAnimations[i] != null)
						{
							ClipBoolPair temp = new ClipBoolPair();
							temp.clip = m_Importer.defaultClipAnimations[i];
							clips[i] = temp;
						}
                    }
                }
            }
        }


        public string path;                     // The fbx's asset path.


        private ClipBoolPair[] clips;           // Store all of the clips for this fbx along with whether or not curves should be applied.
        private ModelImporter m_Importer;       // The importer of the fbx.
        private bool foldout;                   // Whether the clips should be displayed in the GUI under the foldout.
        private Object fbxObject;               // The fbx itself.
        private Vector2 scrollPos;              // The position of the scrollbars.


        // To display everything about the fbx.
        public void OnGUI()
        {
            // If the fbx currently hasn't been set, display an object field for it and get the importer when it's applied.
            if (fbxObject == null)
            {
                fbxObject = EditorGUILayout.ObjectField("FBX", fbxObject, typeof (Object), false);

                if (fbxObject != null)
                {
                    path = AssetDatabase.GetAssetPath(fbxObject);
                    importer = AssetImporter.GetAtPath(path) as ModelImporter;
                }
            }
            // Otherwise should the fbx's name.
            else
            {
                EditorGUILayout.LabelField(fbxObject.name);
            }

            // Only continue if there are an importer and clips.
            if (importer == null || clips == null)
                return;

            // Display all the clips of the fbx under a foldout.
            foldout = EditorGUILayout.Foldout(foldout, "Clips");
            if (foldout)
            {
                EditorGUI.indentLevel++;
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(200f));
                for (int i = 0; i < clips.Length; i++)
                {
                    clips[i].OnGUI();
                }
                EditorGUILayout.EndScrollView();
                EditorGUI.indentLevel--;
            }
        }


        // This is called when all the curves need to be set to the appropriate clips.
        public void Apply(ClipAnimationInfoCurve[] curves)
        {
            if (clips == null || clips.Length == 0)
                return;

            // Create a collection of importer clips with the curves applied.
            List<ModelImporterClipAnimation> newClips = new List<ModelImporterClipAnimation>();
            for (int i = 0; i < clips.Length; i++)
            {
                clips[i].Apply(curves);
                newClips.Add(clips[i].clip);
            }

            // Set the importer's clips to be this new collection.
            importer.clipAnimations = newClips.ToArray();
        }
    }


    private AnimatorController animator         // The animator whose parameters can be a curve name.
    {
        get
        {
            return m_Animator;
        }
        set
        {
            m_Animator = value;
            if (m_Animator == null)
                return;

            // If the animator is not null get all the float parameters.
            AnimatorControllerParameter[] controllerParameters = m_Animator.parameters.Where(x => x.type == AnimatorControllerParameterType.Float).ToArray();
            
            // Get an array of the names of those parameters.
            parameters = new string[controllerParameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                parameters[i] = controllerParameters[i].name;
            }
        }
    }
    private AnimatorController m_Animator;


    private Dictionary<string, AnimationCurve> clipCurves = new Dictionary<string, AnimationCurve>();   // All the curves to be added with their names.
    private string newCurveName = "New Curve Name";                                                     // Temp field to store the name of the new curve being created.
    private List<FbxClips> fbxClips = new List<FbxClips>();                                             // All the fbxs whose clips can have curves applied.
    private List<Keyframe> keyframes = new List<Keyframe>();                                            // The keyframes for the curve currently being created.
    private Vector2 scrollPos;                                                                          // The position of the window's scroll bars.
    private bool useAnimPara;                                                                           // Whether an animator parameter is being used for the name of the curve.
    private string[] parameters;                                                                        // The names of the parameters to choose from.
    private int selectedParameter;                                                                      // The index of the selected name from parameters
        

    // Create the editor window.
    [MenuItem("Window/Add Animation Curves")]
    public static void Init()
    {
        CurveAdder adder = GetWindow<CurveAdder>();
        adder.Show();
        adder.Start();
    }


    // Initial setup.
    void Start()
    {
        // Create an empty FbxClips to store one that is dragged on.
        fbxClips.Add(new FbxClips());

        // Create keyframes at the start and end of the curves.
        keyframes.Add(new Keyframe(0f, 0f));
        keyframes.Add(new Keyframe(1f, 0f));
    }


    // The display for the window.
    void OnGUI()
    {
        // Display all the individual elements 
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        KeyFramesGUI();
        EditorGUILayout.Space();
        CurveCreationGUI();
        EditorGUILayout.Space();
        CurvesGUI();
        EditorGUILayout.Space();
        FbxGUI();
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear"))
            Clear();
        if(GUILayout.Button("Apply"))
            Apply();
        EditorGUILayout.EndScrollView();
    }


    // For displaying settings about keyframes for curves to be created.
    void KeyFramesGUI()
    {
        // If there are keyframes, display the first one but keep it's time to the default (zero).
        if (keyframes.Count > 0)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Time", keyframes[0].time.ToString());
            float firstValue = EditorGUILayout.FloatField("Value", keyframes[0].value);
            keyframes[0] = new Keyframe(keyframes[0].time, firstValue);
            EditorGUILayout.EndHorizontal();
        }

        // For keyframes in the middle of the list, keep there time greater than 0 and less than 1.
        for (int i = 1; i < keyframes.Count - 1; i++)
        {
            EditorGUILayout.BeginHorizontal();
            float time = EditorGUILayout.FloatField("Time", keyframes[i].time);
            time = Mathf.Clamp(time, 0.001f, 0.999f);
            float value = EditorGUILayout.FloatField("Value", keyframes[i].value);
            keyframes[i] = new Keyframe(time, value);
            EditorGUILayout.EndHorizontal();
        }

        // If there are keyframes, display the last one but keep it's time to the default (one).
        if (keyframes.Count > 0)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Time", keyframes[keyframes.Count - 1].time.ToString());
            float lastValue = EditorGUILayout.FloatField("Value", keyframes[keyframes.Count - 1].value);
            keyframes[keyframes.Count - 1] = new Keyframe(keyframes[keyframes.Count - 1].time, lastValue);
            EditorGUILayout.EndHorizontal();
        }

        // Button for adding more keyframes.
        if (GUILayout.Button("Add Keyframe"))
        {
            keyframes.Add(new Keyframe());
        }

        // Sort the keyframes by time.
        keyframes = keyframes.OrderBy(x => x.time).ToList();
    }


    // Display the controls for creating the curves.
    void CurveCreationGUI()
    {
        // A toggle for whether or not a parameter name from an animator controller will be used as the curve name.
        EditorGUILayout.BeginHorizontal();
        useAnimPara = EditorGUILayout.Toggle("Use existing Animator Parameter", useAnimPara);
        EditorGUILayout.EndHorizontal();

        // If using an animator controller, display a control for one to be dragged on.
        if (useAnimPara)
        {
            EditorGUILayout.BeginHorizontal();
            animator = EditorGUILayout.ObjectField("Animator", animator, typeof(AnimatorController), false) as AnimatorController;
            EditorGUILayout.EndHorizontal();
        }

        // Controls for a button to create the curve and the name displayed to the right of it.
        EditorGUILayout.BeginHorizontal();
        bool buttonPressed = GUILayout.Button("Create Curve to Add with Name");
        if (!useAnimPara || animator == null)
        {
            // If not using an animator controller's parameter, use a text field.
            newCurveName = EditorGUILayout.TextField(newCurveName);
        }
        else
        {
            // If using an animator controller, use a popup of the parameter names.
            selectedParameter = EditorGUILayout.Popup(selectedParameter, parameters);
            newCurveName = parameters[selectedParameter];
        }
        EditorGUILayout.EndHorizontal();

        // If the creation button hase been pressed...
        if (buttonPressed)
        {
            // ... and there isn't currently a curve with that name being added...
            if (!clipCurves.ContainsKey(newCurveName))
            {
                // ... add the curve to the list and reset the values for keyframes.
                clipCurves.Add(newCurveName, new AnimationCurve(keyframes.ToArray()));
                keyframes.Clear();
                keyframes.Add(new Keyframe(0f, 0f));
                keyframes.Add(new Keyframe(1f, 0f));
                newCurveName = "New Curve Name";
                selectedParameter = 0;
            }
            else
            {
                // If the name already exists display a warning.
                Debug.LogWarning("A curve of that name is already listed to be added to the selected clips.");
            }
        }
    }


    // Display all the curves that have been created.
    void CurvesGUI()
    {
        for (int i = 0; i < clipCurves.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            clipCurves[clipCurves.ElementAt(i).Key] = EditorGUILayout.CurveField(clipCurves.ElementAt(i).Key, clipCurves.ElementAt(i).Value, Color.green, new Rect(0f, -3f, 1f, 6f));
            EditorGUILayout.EndHorizontal();
        }
    }


    // Display all the fbxs that have been added.
    void FbxGUI()
    {
        for (int i = 0; i < fbxClips.Count; i++)
        {
            fbxClips[i].OnGUI();
        }

        // If the last fbx in the list isn't null, add another to the list.
        if (fbxClips == null || fbxClips.Count <= 0 || fbxClips[fbxClips.Count - 1].importer == null) return;
            fbxClips.Add(new FbxClips());
    }


    // Reset all the fields.
    void Clear()
    {
        clipCurves.Clear();
        fbxClips.Clear();
        keyframes.Clear();
        animator = null;
        newCurveName = "New Curve Name";
        scrollPos = Vector2.zero;
        Start();
    }


    // Called to apply all the curves to the clips.
    void Apply()
    {
        // Create a list with all the curves to be created.
        List<ClipAnimationInfoCurve> newCurves = new List<ClipAnimationInfoCurve>();
        for (int k = 0; k < clipCurves.Count; k++)
        {
            ClipAnimationInfoCurve newCurve = new ClipAnimationInfoCurve
            {
                name = clipCurves.ElementAt(k).Key,
                curve = clipCurves.ElementAt(k).Value
            };
            newCurves.Add(newCurve);
        }
        
        // Go through all the fbx's clips and apply the curves to the clips that need them.
        for (int i = 0; i < fbxClips.Count; i++)
        {
            if(fbxClips[i].importer == null)
                continue;

            fbxClips[i].Apply(newCurves.ToArray());
            AssetDatabase.ImportAsset(fbxClips[i].path, ImportAssetOptions.ForceUpdate);
        }
        
        // Reset the window afterwards.
        Clear();
    }
}
