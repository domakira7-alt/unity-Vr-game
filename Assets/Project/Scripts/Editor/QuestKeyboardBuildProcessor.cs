using System.IO;
using System.Xml;
using UnityEditor.Android;

// Runs after the Oculus plugin, without replacing Unity's generated manifest.
public sealed class QuestKeyboardBuildProcessor : IPostGenerateGradleAndroidProject
{
    private const string AndroidNamespace = "http://schemas.android.com/apk/res/android";
    public int callbackOrder => 11000;

    public void OnPostGenerateGradleAndroidProject(string path)
    {
        string manifestPath = Path.Combine(path, "src/main/AndroidManifest.xml");
        var document = new XmlDocument();
        document.Load(manifestPath);
        XmlElement manifest = document.DocumentElement;
        XmlElement keyboardFeature = null;
        foreach (XmlNode node in manifest.ChildNodes)
        {
            if (node is XmlElement feature && feature.Name == "uses-feature" &&
                feature.GetAttribute("name", AndroidNamespace) == "oculus.software.overlay_keyboard")
            {
                keyboardFeature = feature;
                break;
            }
        }
        if (keyboardFeature == null)
        {
            keyboardFeature = document.CreateElement("uses-feature");
            manifest.AppendChild(keyboardFeature);
        }
        keyboardFeature.SetAttribute("name", AndroidNamespace, "oculus.software.overlay_keyboard");
        keyboardFeature.SetAttribute("required", AndroidNamespace, "false");
        document.Save(manifestPath);
    }
}
