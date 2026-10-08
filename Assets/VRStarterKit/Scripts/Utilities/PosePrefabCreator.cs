#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Yudiz.XRStarter;

public class PosePrefabCreator : MonoBehaviour
{
#if UNITY_EDITOR
    public GameObject handModel; // Assign your hand model prefab in the Inspector
    public AnimationClip[] poseAnimations; // Assign your single-frame animations in the Inspector
    public HandSide handSide;
    [ContextMenu("Create Poses")]
    public void CreatePoses()
    {
        foreach (var poseAnimation in poseAnimations)
        {
            ApplyPose(poseAnimation);
        }
    }

    void ApplyPose(AnimationClip animation)
    {
        AnimationMode.StartAnimationMode();
        AnimationMode.BeginSampling();
        AnimationMode.SampleAnimationClip(handModel, animation,0);
        //AnimationMode.SamplePlayModeAnimation(animation, 0);
        AnimationMode.StopAnimationMode();

        // Copy the positions from the animation to the hand model's rig

        // You would need to write code to copy the positions from the animation to your hand model's rig. 
        // This may involve mapping the bone/transform names from the animation to those in your hand model.

        // After applying the pose, you can create a prefab from the hand model.
        // Instantiate your hand model and save it as a prefab using PrefabUtility.CreatePrefab.
        // Here's a simplified example:
        GameObject instantiatedHand = Instantiate(handModel);
        string handSidePrefix = handSide == HandSide.Left ? "Left_Hand_" : "Right_Hand_";
        PrefabUtility.SaveAsPrefabAsset(instantiatedHand, "Assets/Project/Models/Controller/Oculus Hands/Prefabs/"+ handSidePrefix + animation.name + ".prefab");
        //PrefabUtility.CreatePrefab("Assets/Prefabs/Hand_Prefab.prefab", instantiatedHand);
        DestroyImmediate(instantiatedHand);
    }
#endif
}