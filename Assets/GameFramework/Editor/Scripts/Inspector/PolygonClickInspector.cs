using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace GameFramework
{
    [CustomEditor(typeof(PolygonClickArea))]
    //[CanEditMultipleObjects]
    public class PolygonClickInspector : Editor
    {
        #region private 属性相关
        private PolygonClickArea mTarget;
        private PolygonCollider2D mPolygon;
        private bool mSavePrefab = true;
        private bool mHasSetImg = false;
        private Image TargetImg;
        private Sprite mSprite;

        public Sprite ImgSprite
        {
            get
            {
                return mSprite;
            }
            protected set
            {
                mSprite = value;
                if (null != TargetImg)
                {
                    if (null == value && !mHasSetImg)
                    {
                        return;
                    }
                    TargetImg.sprite = value;
                    mHasSetImg = null != value;
                }
            }
        }

        private void TryGetTargetImg()
        {
            if (null != mTarget)
            {
                TargetImg = mTarget.gameObject.GetComponent<Image>();
                if (null != TargetImg)
                {
                    ImgSprite = TargetImg.sprite;
                }
            }
        }
        #endregion

        //进入时刷新部分参数
        void OnEnable()
        {
            mTarget = target as PolygonClickArea;
            if (null == mTarget)
            {
                return;
            }
            TryGetTargetImg();
            //获取PolygonCollider2D，没有不创建
            mPolygon = mTarget.GetPolygonCollider2D(false);
        }

        //Editor 展示
        public override void OnInspectorGUI()
        {
            if (null == mTarget)
            {
                return;
            }

            if (EditorTools.IsPrefab(mTarget.gameObject))
            {
                mSavePrefab = GUILayout.Toggle(mSavePrefab, "修改预制件");
            }

            base.OnInspectorGUI();

            ImgSprite = EditorGUILayout.ObjectField("ImageSprite", ImgSprite, typeof(Sprite), false) as Sprite;

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Edit"))
            {
                mPolygon = mTarget.GetPolygonCollider2D();
                string[] comps = new string[]
                {
                    "UnityEditor.PolygonCollider2DEditor",
                    typeof(PolygonClickInspector).ToString(),
                };
                EditorTools.SetInspectorTrackerVisible(comps, true);
                //显示ScnenView方便编辑
                EditorTools.FocusWindow(EditorViews.SceneView);
            }

            if (GUILayout.Button("Save"))
            {
                mTarget.SavePoints();
                //删除PolygonCollider2D组件
                mTarget.RemovePolygonCollider2D();
                //如果是prefab则保存
                if (EditorTools.IsPrefab(mTarget.gameObject))
                {
                    EditorTools.SavePrefab(mTarget.gameObject);
                }
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            //没有Image组件或者Image没有Sprit不能重置顶点
            bool hasSprite = null != TargetImg && null != ImgSprite;
            GUI.enabled = hasSprite;
            if (GUILayout.Button("重置顶点"))
            {
                //Debug.Log(TargetImg.preferredWidth);
                float widthH = TargetImg.preferredWidth / 2;
                float heightH = TargetImg.preferredHeight / 2;
                Vector2[] rectPoints = new Vector2[]
                {
                new Vector2(-widthH, heightH),
                new Vector2(widthH, heightH),
                new Vector2(widthH, -heightH),
                new Vector2(-widthH, -heightH),
                };
                mTarget.PolygonPoints = rectPoints;
                //将新的顶点信息赋值给PolygonCollider2D
                mPolygon = mTarget.GetPolygonCollider2D();
            }
            GUI.enabled = true;

            Vector2[] points = null;
            if (null != mPolygon)
            {
                points = mPolygon.points;
            }
            bool hasPoints = null != points && points.Length > 0;
            GUI.enabled = hasSprite && hasPoints;
            if (GUILayout.Button("放大顶点"))
            {
                Vector2 sizeSprit = ImgSprite.rect.size;
                float rateW = TargetImg.preferredWidth / sizeSprit.x * ImgSprite.pixelsPerUnit / TargetImg.pixelsPerUnit;
                float rateH = TargetImg.preferredHeight / sizeSprit.y * ImgSprite.pixelsPerUnit / TargetImg.pixelsPerUnit;
                Debug.Log(sizeSprit);
                for (int i = 0; i < points.Length; i++)
                {
                    Vector2 p = points[i];
                    Debug.LogFormat("p:{0} rateW:{1}, rateH:{2}, pUnitImg:{3}, pUnitSprite:{4}", p, rateW, rateH, TargetImg.pixelsPerUnit, ImgSprite.pixelsPerUnit);
                    p.x *= rateW;
                    p.y *= rateH;
                    Debug.Log(p);
                    points[i] = p;
                }
                mPolygon.points = points;
            }
            GUI.enabled = true;

            GUILayout.EndHorizontal();
        }
    }
}
