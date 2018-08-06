using UnityEngine;
using UnityEngine.UI;

namespace GameFramework
{
    [RequireComponent(typeof(Image))]
    [DisallowMultipleComponent]
    public class PolygonClickArea : MonoBehaviour, ICanvasRaycastFilter
    {
        public Vector2[] PolygonPoints;
        private Image mImage;

#if UNITY_EDITOR
        private PolygonCollider2D mPolygon;
#endif

        #region MonoBehaviour
        void Awake()
        {
            mImage = GetComponent<Image>();
        }
        #endregion

        public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
        {
            Vector2 local;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(mImage.rectTransform, screenPoint, eventCamera, out local);
            return IsPointInPolygon(local);
        }

        /// <summary>
        /// Unity3D 中判断点与多边形的关系
        /// author: William Jiang
        /// url:https://www.cnblogs.com/WilliamJiang/p/5632265.html
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool IsPointInPolygon(Vector2 point)
        {
            int polygonLength = PolygonPoints.Length, i = 0;
            bool inside = false;

            float pointX = point.x, pointY = point.y;

            float startX, startY, endX, endY;
            Vector2 endPoint = PolygonPoints[polygonLength - 1];
            endX = endPoint.x;
            endY = endPoint.y;
            while (i < polygonLength)
            {
                startX = endX;
                startY = endY;
                endPoint = PolygonPoints[i++];
                endX = endPoint.x;
                endY = endPoint.y;
                inside ^= (endY > pointY ^ startY > pointY) && ((pointX - endX) < (pointY - endY) * (startX - endX) / (startY - endY));
            }
            return inside;
        }

        #region Editor 多边形顶点编辑
#if UNITY_EDITOR
        /// <summary>
        /// 给PolygonClick组件添加一个PolygonCollider2D（如果不存在）
        /// 如果已经有多边形顶点，赋值给PolygonCollider2D
        /// </summary>
        /// <returns></returns>
        public PolygonCollider2D GetPolygonCollider2D(bool addIfMiss = true)
        {
            if (null == mPolygon)
            {
                mPolygon = gameObject.GetComponent<PolygonCollider2D>();
            }
            if (null == mPolygon && addIfMiss)
            {
                mPolygon = gameObject.AddComponent<PolygonCollider2D>();
            }
            if (null != mPolygon && null != PolygonPoints && PolygonPoints.Length > 0)
            {
                mPolygon.points = PolygonPoints;
            }
            return mPolygon;
        }

        public bool SavePoints()
        {
            if (null != mPolygon)
            {
                PolygonPoints = mPolygon.points;
                return true;
            }
            return false;
        }

        public void RemovePolygonCollider2D()
        {
            if (null != mPolygon)
            {
                DestroyImmediate(mPolygon);
            }
        }
#endif
        #endregion
    }
}