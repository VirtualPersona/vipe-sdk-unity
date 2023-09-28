using CA;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;

public class HorizontalMenuDrawer
{
    public static void DrawHorizontalMenu(ref Vector2 scrollPosition, List<string> collectionOptions, List<Texture2D> collectionLogos, Action<string> onCollectionSelected, bool isLoading, Texture2D loadingTexture, float rotationAngle, string currentlyLoadingCollection = null)
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

                            //Pintar la textura
                            EditorUIHelpers.DrawTexture(logoRect, collectionLogos[i], () => onCollectionSelected(collectionOptions[i]));

                            // Verificar si el cursor está sobre la textura
                            if (isLoading && collectionOptions[i] == currentlyLoadingCollection)
                                EditorUIHelpers.DrawLoadingSpinner(logoRect, loadingTexture, rotationAngle);
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