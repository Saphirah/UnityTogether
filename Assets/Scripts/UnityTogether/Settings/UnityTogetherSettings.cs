using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class UnityTogetherSettings
{
    [SettingsProvider]
    public static SettingsProvider CreateMyCustomPreferences()
    {
        var provider = new SettingsProvider("Preferences/UnityTogether", SettingsScope.User)
        {
            label = "UnityTogether",
            guiHandler = (searchContext) =>
            {
                var username = EditorPrefs.GetString("UnityTogetherUsername", "Saphirah");
                username = EditorGUILayout.TextField("Username", username);

                EditorPrefs.SetString("UnityTogetherUsername", username);
            },
        };

        return provider;
    }
}
