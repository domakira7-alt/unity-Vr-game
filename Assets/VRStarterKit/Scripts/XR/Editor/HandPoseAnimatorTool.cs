using UnityEditor;
using UnityEngine;
using UnityEditor.Animations;
using System.IO;
using System.Text.RegularExpressions;

public class HandPoseAnimatorTool : EditorWindow
{
    // Easily accessible default values and paths
    private const string DefaultConditionParameter = "HandState"; // Default condition parameter name
    private string DefaultHandGrabEnumName = "HandGrabType"; // Name of the enum
    private const string DefaultEnumFilePath = "Assets/Project/Scripts/XR/ControllerAvatar.cs"; // Default path for the enum script
    private const float DefaultAnimationDuration = 1.0f; // Default animation duration

    private const string AnimationClipSavePath = "Assets/Project/Models/Controller/Oculus Hands/Animations/";
    private const string PrefabSavePath = "Assets/Project/Models/Controller/Oculus Hands/Prefabs/";

    // Fields for Hand Pose Animation Tool (Section 1)
    private GameObject handModelParent;
    private SkinnedMeshRenderer handRenderer;
    private Transform[] handBones;
    private string animationName = "NewHandPoseAnimation";
    private bool isLeftHand = true; // Choose left or right hand for prefab naming

    // Fields for Animator State Tool (Section 2)
    private AnimatorController animatorController;
    private AnimationClip animationClip;
    private string newStateName = "NewState";
    private int conditionValue;

    [MenuItem("Tools/Hand Pose & Animator Tool")]
    public static void ShowWindow()
    {
        GetWindow<HandPoseAnimatorTool>("Hand Pose & Animator Tool");
    }

    private void OnGUI()
    {
        // Section 1: Hand Pose Animation Clip and Prefab Creation
        EditorGUILayout.LabelField("Hand Pose Animation and Prefab Creation", EditorStyles.boldLabel);
        handModelParent = (GameObject)EditorGUILayout.ObjectField("Hand Parent Object", handModelParent, typeof(GameObject), true);
        animationName = EditorGUILayout.TextField("Animation Name", animationName);
        isLeftHand = EditorGUILayout.Toggle("Is Left Hand?", isLeftHand);

        if (handModelParent != null)
        {
            if (GUILayout.Button("Create Hand Pose Animation and Prefab"))
            {
                CreateAnimationClip();
                SaveHandPoseAsPrefab();
                animationClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(AnimationClipSavePath + animationName + ".anim");
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        // Section 2: Animator State and Enum Value Addition
        EditorGUILayout.LabelField("Animator State and Enum Value Addition", EditorStyles.boldLabel);
        animatorController = (AnimatorController)EditorGUILayout.ObjectField("Animator Controller", animatorController, typeof(AnimatorController), true);
        animationClip = (AnimationClip)EditorGUILayout.ObjectField("Animation Clip", animationClip, typeof(AnimationClip), true);
        newStateName = EditorGUILayout.TextField("New State Name", newStateName);

        if (animatorController != null && animationClip != null)
        {
            if (GUILayout.Button("Add New Animator State and Enum Value"))
            {
                AddNewPoseToEnum();
                conditionValue = CalculateEnumValue();
                AddStateAndTransition();
            }
        }
    }

    // Methods from HandPoseAnimationTool (Section 1)

    private void CreateAnimationClip()
    {
        handRenderer = handModelParent.GetComponentInChildren<SkinnedMeshRenderer>();
        if (handRenderer == null)
        {
            Debug.LogWarning("No SkinnedMeshRenderer found in the parent object.");
            return;
        }

        handBones = handRenderer.bones;

        if (handBones == null || handBones.Length == 0)
        {
            Debug.LogWarning("No bones loaded. Cannot create animation.");
            return;
        }

        // Create an animation clip
        AnimationClip animClip = new AnimationClip();
        animClip.legacy = false; // Ensure the clip is not legacy so it can be used in Mecanim

        for (int i = 0; i < handBones.Length; i++)
        {
            if (handBones[i] != null)
            {
                string bonePath = GetBonePath(handBones[i]);

                // Rotation curves
                AnimationCurve curveRotX = new AnimationCurve();
                AnimationCurve curveRotY = new AnimationCurve();
                AnimationCurve curveRotZ = new AnimationCurve();
                AnimationCurve curveRotW = new AnimationCurve();

                Quaternion localRotation = handBones[i].localRotation;

                curveRotX.AddKey(0f, localRotation.x);
                curveRotY.AddKey(0f, localRotation.y);
                curveRotZ.AddKey(0f, localRotation.z);
                curveRotW.AddKey(0f, localRotation.w);

                curveRotX.AddKey(DefaultAnimationDuration, localRotation.x);
                curveRotY.AddKey(DefaultAnimationDuration, localRotation.y);
                curveRotZ.AddKey(DefaultAnimationDuration, localRotation.z);
                curveRotW.AddKey(DefaultAnimationDuration, localRotation.w);

                // Set rotation curves
                animClip.SetCurve(bonePath, typeof(Transform), "localRotation.x", curveRotX);
                animClip.SetCurve(bonePath, typeof(Transform), "localRotation.y", curveRotY);
                animClip.SetCurve(bonePath, typeof(Transform), "localRotation.z", curveRotZ);
                animClip.SetCurve(bonePath, typeof(Transform), "localRotation.w", curveRotW);
            }
        }

        string path = AnimationClipSavePath + animationName + ".anim";
        AssetDatabase.CreateAsset(animClip, path);
        AssetDatabase.SaveAssets();

        Debug.Log("Animation Clip Created: " + path);
    }

    private string GetBonePath(Transform bone)
    {
        string bonePath = "";
        Transform current = bone;

        // Traverse upwards to build the full bone path
        while (current != handRenderer.rootBone && current != null)
        {
            bonePath = current.name + (bonePath == "" ? "" : "/" + bonePath);
            current = current.parent;
        }

        return bonePath;
    }

    private void SaveHandPoseAsPrefab()
    {
        if (handModelParent == null)
        {
            Debug.LogWarning("Please assign the parent GameObject of the hand model.");
            return;
        }

        string handType = isLeftHand ? "Left_hand_" : "Right_hand_";
        string prefabName = handType + animationName;
        string prefabPath = PrefabSavePath + prefabName + ".prefab";

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(handModelParent, prefabPath);

        if (prefab != null)
        {
            Debug.Log("Hand Prefab Saved: " + prefabPath);
        }
        else
        {
            Debug.LogWarning("Failed to save hand prefab.");
        }
    }

    // Methods from AnimatorStateTool (Section 2)

    private void AddStateAndTransition()
    {
        bool parameterExists = false;
        foreach (var param in animatorController.parameters)
        {
            if (param.name == DefaultConditionParameter && param.type == AnimatorControllerParameterType.Int)
            {
                parameterExists = true;
                break;
            }
        }

        if (!parameterExists)
        {
            Debug.LogError($"Integer parameter '{DefaultConditionParameter}' not found in Animator Controller.");
            return;
        }

        AnimatorState newState = animatorController.layers[0].stateMachine.AddState(newStateName);
        newState.motion = animationClip;

        AnimatorStateTransition transition = animatorController.layers[0].stateMachine.AddAnyStateTransition(newState);
        transition.AddCondition(AnimatorConditionMode.Equals, conditionValue, DefaultConditionParameter);
        transition.hasExitTime = false;
        transition.duration = 0.25f;

        AssetDatabase.SaveAssets();

        Debug.Log($"Added new state '{newStateName}' with transition from Any State in Animator.");
    }

    private void AddNewPoseToEnum()
    {
        if (File.Exists(DefaultEnumFilePath))
        {
            string[] lines = File.ReadAllLines(DefaultEnumFilePath);
            bool enumFound = false;

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains($"public enum {DefaultHandGrabEnumName}"))
                {
                    enumFound = true;
                    for (int j = i + 1; j < lines.Length; j++)
                    {
                        if (lines[j].Contains("}"))
                        {
                            if (!lines[j - 1].TrimEnd().EndsWith(","))
                            {
                                lines[j - 1] += ",";
                            }

                            lines[j] = $"    {newStateName},\n{lines[j]}";
                            break;
                        }
                    }
                    break;
                }
            }

            if (enumFound)
            {
                File.WriteAllLines(DefaultEnumFilePath, lines);
                AssetDatabase.Refresh();
                Debug.Log($"Added '{newStateName}' to enum '{DefaultHandGrabEnumName}'.");
            }
            else
            {
                Debug.LogError($"Enum '{DefaultHandGrabEnumName}' not found in file '{DefaultEnumFilePath}'.");
            }
        }
        else
        {
            Debug.LogError($"File '{DefaultEnumFilePath}' not found.");
        }
    }

    private int CalculateEnumValue()
    {
        if (File.Exists(DefaultEnumFilePath))
        {
            string[] lines = File.ReadAllLines(DefaultEnumFilePath);
            bool enumFound = false;
            int maxEnumValue = -1;

            Regex explicitEnumRegex = new Regex(@"\s*(\w+)\s*=\s*(-?\d+)\s*,");
            Regex implicitEnumRegex = new Regex(@"\s*(\w+)\s*,");

            int implicitValueCounter = 0;

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains($"public enum {DefaultHandGrabEnumName}"))
                {
                    enumFound = true;

                    for (int j = i + 1; j < lines.Length; j++)
                    {
                        if (lines[j].Contains("}"))
                        {
                            break;
                        }

                        var explicitMatch = explicitEnumRegex.Match(lines[j]);
                        if (explicitMatch.Success)
                        {
                            int value = int.Parse(explicitMatch.Groups[2].Value);
                            maxEnumValue = Mathf.Max(maxEnumValue, value);
                            implicitValueCounter = value + 1;
                        }
                        else
                        {
                            var implicitMatch = implicitEnumRegex.Match(lines[j]);
                            if (implicitMatch.Success)
                            {
                                maxEnumValue = Mathf.Max(maxEnumValue, implicitValueCounter);
                                implicitValueCounter++;
                            }
                        }
                    }
                    break;
                }
            }

            if (enumFound)
            {
                return maxEnumValue;
            }
            else
            {
                Debug.LogError($"Enum '{DefaultHandGrabEnumName}' not found in file '{DefaultEnumFilePath}'.");
                return -1;
            }
        }
        else
        {
            Debug.LogError($"File '{DefaultEnumFilePath}' not found.");
            return -1;
        }
    }
}
