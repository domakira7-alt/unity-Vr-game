using NaughtyAttributes;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Yudiz.VRAwarenessExperience.XR;

namespace Yudiz.XRStarter
{
    [ExecuteInEditMode]
    public class GrabAnchorAdjuster : MonoBehaviour
    {
        private XRCustomGrabbable grabbable;

        [InfoBox("Don't Change this path unless Unless you have changed the Hands Poses Prefabs Folder Path", EInfoBoxType.Normal)]        
        public string handPosesFolderPath = "Assets/Project/Models/Controller/Oculus Hands/Prefabs/";
        private GameObject leftHandPreview;
        private GameObject leftHandSecondaryPreview;
        private GameObject rightHandPreview;
        private GameObject rightHandSecondaryPreview;

#if UNITY_EDITOR
        private void Awake()
        {
            grabbable = GetComponent<XRCustomGrabbable>();
        }
        #region PUBLIC_METHODS
        [Button("SpawnLeftHandPreview")]
        public void SpawnLeftHandPreview()
        {
            Debug.Log("Spawning Left Checking Grab");
            if (grabbable == null)
                grabbable = GetComponent<XRCustomGrabbable>();
            Debug.Log("Spawning Left");

            if (leftHandPreview != null)
                return;

            if (grabbable.leftAnchorTransform == null)
            {
                grabbable.leftAnchorTransform = SpawnAnchor("LeftHand_AnchorTransform").transform;
            }

            ResetGlobalScale(grabbable.leftAnchorTransform);

            ControllerAvatar[] avatars = FindObjectsOfType<ControllerAvatar>();
            ControllerAvatar handAvatar = avatars.ToList().Find(x => x.HandSide == HandSide.Left);

            leftHandPreview = SpawnHandPreview(grabbable.leftAnchorTransform, handAvatar, HandSide.Left, grabbable.grabType);
        }
        [Button("RemoveLeftHandPreview")]
        public void RemoveLeftHandPreview()
        {
            if (leftHandPreview != null)
            {
                DestroyImmediate(leftHandPreview);
            }
        }
        [Button("SpawnLeftHandSecondaryPreview")]
        public void SpawnLeftHandSecondaryPreview()
        {
            Debug.Log("Spawning Left Checking Grab");
            if (grabbable == null)
                grabbable = GetComponent<XRCustomGrabbable>();
            Debug.Log("Spawning Left");

            if (leftHandSecondaryPreview != null)
                return;

            if (grabbable.secondaryLeftAnchorTransform == null)
            {
                grabbable.secondaryLeftAnchorTransform = SpawnAnchor("LeftHand_SecondaryAnchorTransform").transform;
            }

            ResetGlobalScale(grabbable.secondaryLeftAnchorTransform);

            ControllerAvatar[] avatars = FindObjectsOfType<ControllerAvatar>();
            ControllerAvatar handAvatar = avatars.ToList().Find(x => x.HandSide == HandSide.Left);

            leftHandSecondaryPreview = SpawnHandPreview(grabbable.secondaryLeftAnchorTransform, handAvatar, HandSide.Left, grabbable.grabType);
        }
        [Button("RemoveLeftHandSecondaryPreview")]
        public void RemoveLeftHandSecondaryPreview()
        {
            if (leftHandSecondaryPreview != null)
            {
                DestroyImmediate(leftHandSecondaryPreview);
            }
        }
        
        [Button("SpawnRightHandPreview")]
        public void SpawnRightHandPreview()
        {
            Debug.Log("Spawning Right Checking Grab");
            if (grabbable == null)
                grabbable = GetComponent<XRCustomGrabbable>();
            Debug.Log("Spawning Right");

            if (rightHandPreview != null)
                return;

            if (grabbable.rightAnchorTransform == null)
            {
                grabbable.rightAnchorTransform = SpawnAnchor("RightHand_AnchorTransform").transform;
            }

            ResetGlobalScale(grabbable.rightAnchorTransform);

            ControllerAvatar[] avatars = FindObjectsOfType<ControllerAvatar>();
            ControllerAvatar handAvatar = avatars.ToList().Find(x => x.HandSide == HandSide.Right);


            rightHandPreview = SpawnHandPreview(grabbable.rightAnchorTransform, handAvatar, HandSide.Right, grabbable.grabType);
        }
        [Button("RemoveRightHandPreview")]
        public void RemoveRightHandPreview()
        {
            if (rightHandPreview != null)
            {
                DestroyImmediate(rightHandPreview);
            }
        }

        [Button("SpawnRightHandSecondaryPreview")]
        public void SpawnRightHandSecondaryPreview()
        {
            Debug.Log("Spawning Right Checking Grab");
            if (grabbable == null)
                grabbable = GetComponent<XRCustomGrabbable>();
            Debug.Log("Spawning Right");

            if (rightHandSecondaryPreview != null)
                return;

            if (grabbable.secondaryRightAnchorTransform == null)
            {
                grabbable.secondaryRightAnchorTransform = SpawnAnchor("RightHand_SecondaryAnchorTransform").transform;
            }

            ResetGlobalScale(grabbable.secondaryRightAnchorTransform);

            ControllerAvatar[] avatars = FindObjectsOfType<ControllerAvatar>();
            ControllerAvatar handAvatar = avatars.ToList().Find(x => x.HandSide == HandSide.Right);

            rightHandSecondaryPreview = SpawnHandPreview(grabbable.secondaryRightAnchorTransform, handAvatar, HandSide.Right, grabbable.grabType);
        }
        [Button("RemoveRightHandSecondaryPreview")]
        public void RemoveRightHandSecondaryPreview()
        {
            if (rightHandSecondaryPreview != null)
            {
                DestroyImmediate(rightHandSecondaryPreview);
            }
        }
        #endregion
        
        #region PRIVATE_METHODS
        private GameObject SpawnAnchor(string name)
        {
            GameObject anchor = new GameObject(name);
            anchor.transform.parent = transform;
            anchor.transform.localPosition = Vector3.zero;
            anchor.transform.localRotation = Quaternion.identity;
            anchor.transform.localScale = Vector3.one;
            return anchor;
        }
        private void ResetGlobalScale(Transform anchor)
        {
            anchor.transform.localScale = Vector3.one;
            Vector3 globalScale = anchor.transform.lossyScale;
            anchor.transform.localScale = new Vector3(1 / globalScale.x, 1 / globalScale.y, 1 / globalScale.z);
        }
        private GameObject SpawnHandPreview(Transform parent, ControllerAvatar handAvatar, HandSide handSide, HandGrabType grabType)
        {
            GameObject handPreview = PrefabUtility.LoadPrefabContents(GetHandPosePrefab(handSide, grabType));
            handPreview.transform.parent = parent;
            handPreview.transform.localPosition = handAvatar.HandTransform.localPosition;
            handPreview.transform.localRotation = handAvatar.HandTransform.localRotation;
            ResetGlobalScale(handPreview.transform);
            return handPreview;
        }

        private string GetHandPosePrefab(HandSide handSide, HandGrabType grabType)
        {
            string prefabName = string.Empty;
            if (handSide == HandSide.Left)
            {
                prefabName = "Left_hand_";
            }
            else
            {
                prefabName = "Right_hand_";
            }

            prefabName += grabType.ToString().ToLower();

            return handPosesFolderPath + prefabName + ".prefab";
        }
        #endregion
#endif
    }
}