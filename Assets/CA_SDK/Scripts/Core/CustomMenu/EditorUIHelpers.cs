using UnityEngine;
using UnityEditor;
using System;

namespace CA
{
    public static class EditorUIHelpers
    {
        // General click handler
        private static void HandleClickOnRect(Rect rect, Action onClick)
        {
            if (rect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                onClick?.Invoke();
            }
        }
        public static bool HandleIsClickOnRect(Rect rect, Action onClick)
        {
            if (rect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                onClick?.Invoke();
                return true; // Rectángulo ha sido clickeado
            }
            return false; // Rectángulo no ha sido clickeado
        }
        // Draw a scrollable area
        public static void DrawScrollArea(ref Vector2 scrollPosition, Action content, params GUILayoutOption[] options)
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, options);
            content();
            EditorGUILayout.EndScrollView();
        }

        // Draw a generic box with content
        public static void DrawBox(Action content, params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginVertical(options);
            content();
            EditorGUILayout.EndVertical();
        }

        // Draw a padded area
        public static void DrawPaddedArea(Action content, int padding)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(padding);
            content();
            GUILayout.Space(padding);
            EditorGUILayout.EndHorizontal();
        }

        // Draw a generic label
        public static void DrawLabel(string text, GUIStyle style)
        {
            GUILayout.Label(text, style);
        }

        // Draw a generic texture
        public static void DrawTexture(Rect rect, Texture2D texture, Action onClick)
        {
            if (texture != null)
            {
                GUI.DrawTexture(rect, texture);
            }
            HandleClickOnRect(rect, onClick);
        }

        // Draw generic pagination controls
        public static void DrawPaginationControls(Action onPrevious, Action onNext, int currentPage, int totalPages)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Previous Page"))
            {
                onPrevious?.Invoke();
            }
            DrawPageInfo(currentPage, totalPages);
            if (GUILayout.Button("Next Page"))
            {
                onNext?.Invoke();
            }
            EditorGUILayout.EndHorizontal();
        }

        // Page info (kept as is)
        public static void DrawPageInfo(int currentPage, int totalPages)
        {
            GUIStyle centeredStyle = new GUIStyle(GUI.skin.label);
            centeredStyle.alignment = TextAnchor.MiddleCenter;
            GUILayout.FlexibleSpace();
            GUILayout.Label($"{currentPage} | {totalPages}", centeredStyle);
            GUILayout.FlexibleSpace();
        }

        // Search field (kept as is)
        public static string DrawSearchField(string currentSearch, Action<string> onSearch, string placeholder = "Buscar avatares...")
        {
            GUIContent searchContent = new GUIContent("Search:", placeholder);
            string newSearch = EditorGUILayout.TextField(searchContent, currentSearch);
            if (newSearch != currentSearch)
            {
                onSearch?.Invoke(newSearch);
            }
            return newSearch;
        }

        // Texture button (kept as is)
        public static void DrawTextureButton(Rect rect, Texture2D texture, Action onClick)
        {
            if (GUI.Button(rect, GUIContent.none))
            {
                onClick?.Invoke();
            }
            GUI.DrawTexture(rect, texture);
        }

        // Loading spinner (kept as is)
        public static void DrawLoadingSpinner(Rect rect, Texture2D loadingTexture, float rotationAngle)
        {
            Matrix4x4 matrixBackup = GUI.matrix;
            GUIUtility.RotateAroundPivot(rotationAngle, rect.center);
            GUI.DrawTexture(rect, loadingTexture);
            GUI.matrix = matrixBackup;
        }

        // Transparent background (kept as is)
        public static void DrawTransparentBackground(Rect rect, Color backgroundColor)
        {
            GUI.color = backgroundColor;
            GUI.DrawTexture(rect, EditorGUIUtility.whiteTexture);
            GUI.color = Color.white;
        }

        // Row (kept as is)
        public static void DrawRow(int rowIndex, int cols, int padding, int imageWidth, Action<int, int, int> renderTextureButton)
        {
            EditorGUILayout.BeginHorizontal();
            for (int x = 0; x < cols; x++)
            {
                int index = rowIndex * cols + x;
                renderTextureButton(index, padding, imageWidth);
            }
            GUILayout.Space(padding);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(padding);
        }

        // Border (kept as is)
        public static void DrawBorder(Rect rect, float borderWidth, Color color)
        {
            GUI.color = color;
            Rect borderRect = new Rect(
                rect.x - borderWidth,
                rect.y - borderWidth,
                rect.width + 2 * borderWidth,
                rect.height + 2 * borderWidth
            );
            GUI.DrawTexture(borderRect, EditorGUIUtility.whiteTexture);
            GUI.color = Color.white;
        }

        // Expand Rect (kept as is)
        public static Rect ExpandRect(Rect originalRect, float expandAmount)
        {
            return new Rect(
                originalRect.x - expandAmount / 2,
                originalRect.y - expandAmount / 2,
                originalRect.width + expandAmount,
                originalRect.height + expandAmount
            );
        }

        // Calculate grid dimensions (kept as is)
        public static (int cols, int rows) CalculateGridDimensions(float windowWidth, float padding, float borderWidth, float imageWidth)
        {
            int cols = Mathf.Max(1, Mathf.FloorToInt((windowWidth - padding - borderWidth) / (imageWidth + padding + borderWidth)));
            int rows = Mathf.CeilToInt((float)imageWidth / cols);
            return (cols, rows);
        }
    }
}
