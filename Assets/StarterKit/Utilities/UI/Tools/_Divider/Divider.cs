using UnityEngine;
public class Divider : PropertyAttribute
{
    public readonly string title;
	public Divider(string title = "")
    {
        this.title = title;
    }
}
