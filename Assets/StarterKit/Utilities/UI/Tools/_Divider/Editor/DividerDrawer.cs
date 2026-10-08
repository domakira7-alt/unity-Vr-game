using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Divider))]
public class DividerDrawer : DecoratorDrawer
{
    Divider divider { get { return ((Divider)attribute); } }
    public override void OnGUI(Rect position)
    {
        if (divider.title == "")
        {
            GUI.color = Color.blue;
            GUI.Label(position, "", new GUIStyle(GUI.skin.horizontalSlider));
            GUI.color = Color.white;
        }
        else
        {
            Vector2 textSize = GUI.skin.label.CalcSize(new GUIContent(divider.title));

            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.blue;
            style.fontStyle = FontStyle.Bold;

            GUI.Label(new Rect(position.xMin + 1, position.yMin + 1, textSize.x, 0), divider.title, style);

            GUI.color = Color.green;
            GUI.Label(new Rect(position.xMin + textSize.x + 5, position.yMin, position.xMax - textSize.x - 20, 1), "", new GUIStyle(GUI.skin.horizontalSlider));
            GUI.color = Color.white;
        }
    }
}

