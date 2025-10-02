using UnityEngine;
using UnityEngine.UI;
// https://gist.github.com/sttz/c406aec3ace821738ecd4fa05833d21d
namespace UI
{
    public static class UIExtensions
    {
        // Shared array used to receive result of RectTransform.GetWorldCorners
        static readonly Vector3[] Corners = new Vector3[4];

        /// <summary>
        /// Transform the bounds of the current rect transform to the space of another transform.
        /// </summary>
        /// <param name="source">The rect to transform</param>
        /// <param name="target">The target space to transform to</param>
        /// <returns>The transformed bounds</returns>
        // ReSharper disable once MemberCanBePrivate.Global
        public static Bounds TransformBoundsTo(this RectTransform source, Transform target)
        {
            // Based on code in ScrollRect internal GetBounds and InternalGetBounds methods
            Bounds bounds = new Bounds();
            if (source != null)
            {
                source.GetWorldCorners(Corners);

                Vector3 vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
                Vector3 vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

                Matrix4x4 matrix = target.worldToLocalMatrix;
                for (int j = 0; j < 4; j++)
                {
                    Vector3 v = matrix.MultiplyPoint3x4(Corners[j]);
                    vMin = Vector3.Min(v, vMin);
                    vMax = Vector3.Max(v, vMax);
                }

                bounds = new Bounds(vMin, Vector3.zero);
                bounds.Encapsulate(vMax);
            }
            return bounds;
        }

        /// <summary>
        /// Normalize a distance to be used in verticalNormalizedPosition or horizontalNormalizedPosition.
        /// </summary>
        /// <param name="scrollRect">The target scrollRect</param>
        /// <param name="axis">Scroll axis, 0 = horizontal, 1 = vertical</param>
        /// <param name="distance">The distance in the scroll rect view coordinate space</param>
        /// <returns>The normalized scroll distance</returns>
        // ReSharper disable once MemberCanBePrivate.Global
        public static float NormalizeScrollDistance(this ScrollRect scrollRect, int axis, float distance)
        {
            // Based on code in ScrollRect internal SetNormalizedPosition method
            RectTransform viewport = scrollRect.viewport;
            RectTransform viewRect = viewport != null ? viewport : scrollRect.GetComponent<RectTransform>();
            Rect rect = viewRect.rect;
            Bounds viewBounds = new Bounds(rect.center, rect.size);

            RectTransform content = scrollRect.content;
            Bounds contentBounds = content != null ? content.TransformBoundsTo(viewRect) : new Bounds();

            float hiddenLength = contentBounds.size[axis] - viewBounds.size[axis];
            return distance / hiddenLength;
        }

        /// <summary>
        /// Scroll the target element to the vertical center of the scroll rect viewport.
        /// Assumes the target element is part of the scroll rect contents.
        /// </summary>
        /// <param name="scrollRect">Scroll rect to scroll</param>
        /// <param name="target">Element of the scroll rect content to center vertically</param>
        public static void ScrollToCenter(this ScrollRect scrollRect, RectTransform target)
        {
            // The scroll rect view space is used to calculate scroll position
            RectTransform view = scrollRect.viewport != null ? scrollRect.viewport : scrollRect.GetComponent<RectTransform>();

            // Calculate the scroll offset in the view's space
            Rect viewRect = view.rect;
            Bounds elementBounds = target.TransformBoundsTo(view);
            float offset = viewRect.center.y - elementBounds.center.y;

            // Normalize and apply the calculated offset
            float scrollPos = scrollRect.verticalNormalizedPosition - scrollRect.NormalizeScrollDistance(1, offset);
            scrollRect.verticalNormalizedPosition = Mathf.Clamp(scrollPos, 0f, 1f);
        }
    }
}