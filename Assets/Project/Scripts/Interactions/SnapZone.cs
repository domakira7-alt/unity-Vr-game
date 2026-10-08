using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Yudiz.VRAwarenessExperience.Interactions
{
    public class SnapZone : MonoBehaviour
    {
        [SerializeField] private SnappableID zoneID;
        [SerializeField] private float distanceThreshold = 0.1f;
        [SerializeField] private float rotationThreshold = 20f;
        [SerializeField] private Collider zoneCollider;
        private bool isOccupied = false;

        public SnappableID ZoneID => zoneID;

        public bool ValidateSnap(PuzzlePiece puzzlePiece)
        {
            if (isOccupied) return false;
            
            if (puzzlePiece.PuzzlePieceID == zoneID) 
            {
                float distance = Vector3.Distance(puzzlePiece.transform.position, transform.position);
                bool rotation = ValidateRotation(puzzlePiece);
                Debug.Log("SnapZone -> ValidateSnap -> Distance: " + distance + " Rotation: " + rotation);
                if (distance <= distanceThreshold && rotation)
                {
                    return true;
                }
                else
                {
                    Debug.Log("SnapZone -> ValidateSnap -> Invalid Snap");
                    return false;
                }
            }

            return false;
        }

        private bool ValidateRotation(PuzzlePiece puzzlePiece)
        {
            float rotation = Vector3.Angle(puzzlePiece.transform.forward, transform.forward);
            return rotation <= rotationThreshold;
        }

        public void SnapItem(PuzzlePiece puzzlePiece)
        {
            isOccupied = true;
            zoneCollider.enabled = false;
            puzzlePiece.transform.position = transform.position;
            puzzlePiece.transform.rotation = transform.rotation;
        }
    }

    public enum SnappableID
    {
        None,
        BackLandingGear,
        Cabin_1,
        Cabin_2,
        Cockpit,
        Engine_1,
        Engine_2,
        FrontLandingGear,
        Tail,
        VerticalStabilizer,
        LeftHorizontalStabilizer,
        RightHorizontalStabilizer,
        LeftWing,
        RightWing,
    }
}

