using UnityEngine;

namespace StarterKit.DialogueSystem
{
    /// <summary>
    /// Base interface for dialogue type enums - each class should implement its own enum
    /// </summary>
    public interface IDialogueType
    {
        // Marker interface for dialogue type enums
    }
    
    /// <summary>
    /// Generic extension methods for dialogue type enums
    /// </summary>
    public static class DialogueTypeExtensions
    {
        /// <summary>
        /// Get display name for any enum dialogue type
        /// </summary>
        public static string GetDisplayName<T>(this T type) where T : System.Enum
        {
            return type.ToString().Replace("_", " ");
        }
        
        /// <summary>
        /// Check if enum value equals another enum value
        /// </summary>
        public static bool Is<T>(this T type, T other) where T : System.Enum
        {
            return type.Equals(other);
        }
    }
}