using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CA;

public class HorizontalMenuDrawer
{
    public static void DrawHorizontalMenu(ref Vector2 scrollPosition, List<string> collectionOptions, List<Texture2D> collectionLogos, Action<string> onCollectionSelected)
    {
        EditorUIHelpers.DrawScrollArea(ref scrollPosition, () =>
        {
            EditorGUILayout.BeginHorizontal();

            for (int i = 0; i < collectionOptions.Count; i++)
            {
                EditorUIHelpers.DrawBox(() =>
                {
                    EditorUIHelpers.DrawPaddedArea(() =>
                    {
                        if (i < collectionLogos.Count && collectionLogos[i] != null)
                        {
                            Rect logoRect = GUILayoutUtility.GetRect(100, 100);
                            EditorUIHelpers.DrawTexture(logoRect, collectionLogos[i], () => onCollectionSelected(collectionOptions[i]));
                        }
                    }, 20);

                    EditorUIHelpers.DrawPaddedArea(() =>
                    {
                        GUIStyle style = new GUIStyle() { alignment = TextAnchor.UpperCenter };
                        style.normal.textColor = Color.white;
                        EditorUIHelpers.DrawLabel(collectionOptions[i], style);
                    }, 10);

                }, GUILayout.Width(140), GUILayout.Height(140));
            }

            EditorGUILayout.EndHorizontal();
        }, GUILayout.Height(140));
    }
}