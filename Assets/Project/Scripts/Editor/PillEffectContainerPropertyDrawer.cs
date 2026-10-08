using UnityEngine;
using UnityEditor;
using Yudiz.VRAwarenessExperience.Data;

namespace Yudiz.VRAwarenessExperience.Editor
{
    [CustomPropertyDrawer(typeof(PillEffectContainer))]
    public class PillEffectContainerPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Get the properties
            var pillTypeProp = property.FindPropertyRelative("pillType");
            var movementDataProp = property.FindPropertyRelative("movementData");
            var isPillRequiredProp = property.FindPropertyRelative("isPillRequired");
            var pillPrefabProp = property.FindPropertyRelative("pillPrefab");
            var pillParentProp = property.FindPropertyRelative("pillParent");
            var isTransitionRequiredProp = property.FindPropertyRelative("isTransitionRequired");
            var fromEffectDataProp = property.FindPropertyRelative("fromEffectData");
            var toEffectDataProp = property.FindPropertyRelative("toEffectData");
            var isEffectRequiredProp = property.FindPropertyRelative("isEffectRequired");
            var effectDataProp = property.FindPropertyRelative("effectData");

            // Calculate positions
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            float yOffset = 0f;

            // Draw pillType
            if (pillTypeProp != null)
            {
                float height = EditorGUI.GetPropertyHeight(pillTypeProp);
                Rect pillTypeRect = new Rect(position.x, position.y + yOffset, position.width, height);
                EditorGUI.PropertyField(pillTypeRect, pillTypeProp);
                yOffset += height + spacing;
            }

            // Draw movementData
            if (movementDataProp != null)
            {
                float height = EditorGUI.GetPropertyHeight(movementDataProp);
                Rect movementDataRect = new Rect(position.x, position.y + yOffset, position.width, height);
                EditorGUI.PropertyField(movementDataRect, movementDataProp);
                yOffset += height + spacing;
            }

            // Draw isPillRequired
            if (isPillRequiredProp != null)
            {
                float height = EditorGUI.GetPropertyHeight(isPillRequiredProp);
                Rect isPillRequiredRect = new Rect(position.x, position.y + yOffset, position.width, height);
                EditorGUI.PropertyField(isPillRequiredRect, isPillRequiredProp);
                yOffset += height + spacing;
            }

            // Only show pill fields if isPillRequired is true
            if (isPillRequiredProp != null && isPillRequiredProp.boolValue)
            {
                // Draw pillPrefab
                if (pillPrefabProp != null)
                {
                    float height = EditorGUI.GetPropertyHeight(pillPrefabProp);
                    Rect pillPrefabRect = new Rect(position.x, position.y + yOffset, position.width, height);
                    EditorGUI.PropertyField(pillPrefabRect, pillPrefabProp);
                    yOffset += height + spacing;
                }

                // Draw pillParent
                if (pillParentProp != null)
                {
                    float height = EditorGUI.GetPropertyHeight(pillParentProp);
                    Rect pillParentRect = new Rect(position.x, position.y + yOffset, position.width, height);
                    EditorGUI.PropertyField(pillParentRect, pillParentProp);
                    yOffset += height + spacing;
                }
            }

            // Draw isTransitionRequired
            if (isTransitionRequiredProp != null)
            {
                float height = EditorGUI.GetPropertyHeight(isTransitionRequiredProp);
                Rect isTransitionRequiredRect = new Rect(position.x, position.y + yOffset, position.width, height);
                EditorGUI.PropertyField(isTransitionRequiredRect, isTransitionRequiredProp);
                yOffset += height + spacing;
            }

            // Only show transition fields if isTransitionRequired is true
            if (isTransitionRequiredProp != null && isTransitionRequiredProp.boolValue)
            {
                // Draw fromEffectData
                if (fromEffectDataProp != null)
                {
                    float height = EditorGUI.GetPropertyHeight(fromEffectDataProp);
                    Rect fromEffectDataRect = new Rect(position.x, position.y + yOffset, position.width, height);
                    EditorGUI.PropertyField(fromEffectDataRect, fromEffectDataProp);
                    yOffset += height + spacing;
                }

                // Draw toEffectData
                if (toEffectDataProp != null)
                {
                    float height = EditorGUI.GetPropertyHeight(toEffectDataProp);
                    Rect toEffectDataRect = new Rect(position.x, position.y + yOffset, position.width, height);
                    EditorGUI.PropertyField(toEffectDataRect, toEffectDataProp);
                    yOffset += height + spacing;
                }
            }

            // Draw isEffectRequired
            if (isEffectRequiredProp != null)
            {
                float height = EditorGUI.GetPropertyHeight(isEffectRequiredProp);
                Rect isEffectRequiredRect = new Rect(position.x, position.y + yOffset, position.width, height);
                EditorGUI.PropertyField(isEffectRequiredRect, isEffectRequiredProp);
                yOffset += height + spacing;
            }

            // Only show effect field if isEffectRequired is true
            if (isEffectRequiredProp != null && isEffectRequiredProp.boolValue)
            {
                // Draw effectData
                if (effectDataProp != null)
                {
                    float height = EditorGUI.GetPropertyHeight(effectDataProp);
                    Rect effectDataRect = new Rect(position.x, position.y + yOffset, position.width, height);
                    EditorGUI.PropertyField(effectDataRect, effectDataProp);
                }
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var pillTypeProp = property.FindPropertyRelative("pillType");
            var movementDataProp = property.FindPropertyRelative("movementData");
            var isPillRequiredProp = property.FindPropertyRelative("isPillRequired");
            var pillPrefabProp = property.FindPropertyRelative("pillPrefab");
            var pillParentProp = property.FindPropertyRelative("pillParent");
            var isTransitionRequiredProp = property.FindPropertyRelative("isTransitionRequired");
            var fromEffectDataProp = property.FindPropertyRelative("fromEffectData");
            var toEffectDataProp = property.FindPropertyRelative("toEffectData");
            var isEffectRequiredProp = property.FindPropertyRelative("isEffectRequired");
            var effectDataProp = property.FindPropertyRelative("effectData");
            
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            float height = 0f;
            
            // Always visible fields
            if (pillTypeProp != null)
                height += EditorGUI.GetPropertyHeight(pillTypeProp) + spacing;
            
            if (movementDataProp != null)
                height += EditorGUI.GetPropertyHeight(movementDataProp) + spacing;
            
            if (isPillRequiredProp != null)
                height += EditorGUI.GetPropertyHeight(isPillRequiredProp) + spacing;
            
            // Pill fields if isPillRequired is true
            if (isPillRequiredProp != null && isPillRequiredProp.boolValue)
            {
                if (pillPrefabProp != null)
                    height += EditorGUI.GetPropertyHeight(pillPrefabProp) + spacing;
                
                if (pillParentProp != null)
                    height += EditorGUI.GetPropertyHeight(pillParentProp) + spacing;
            }
            
            // Transition fields
            if (isTransitionRequiredProp != null)
                height += EditorGUI.GetPropertyHeight(isTransitionRequiredProp) + spacing;
            
            if (isTransitionRequiredProp != null && isTransitionRequiredProp.boolValue)
            {
                if (fromEffectDataProp != null)
                    height += EditorGUI.GetPropertyHeight(fromEffectDataProp) + spacing;
                
                if (toEffectDataProp != null)
                    height += EditorGUI.GetPropertyHeight(toEffectDataProp) + spacing;
            }
            
            // Effect fields
            if (isEffectRequiredProp != null)
                height += EditorGUI.GetPropertyHeight(isEffectRequiredProp) + spacing;
            
            if (isEffectRequiredProp != null && isEffectRequiredProp.boolValue)
            {
                if (effectDataProp != null)
                    height += EditorGUI.GetPropertyHeight(effectDataProp) + spacing;
            }
            
            return height;
        }
    }
}
