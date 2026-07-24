using UnityEngine;
using UnityEngine.UI;

namespace LootUp.Core.Characters
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class PlayerShapeGraphic : MaskableGraphic
    {
        public override Texture mainTexture => Texture2D.whiteTexture;

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            Rect rect = rectTransform.rect;
            Color32 vertexColor = color;

            vh.AddVert(new Vector3(rect.xMin, rect.yMin), vertexColor, new Vector2(0f, 0f));
            vh.AddVert(new Vector3(rect.xMin, rect.yMax), vertexColor, new Vector2(0f, 1f));
            vh.AddVert(new Vector3(rect.xMax, rect.yMax), vertexColor, new Vector2(1f, 1f));
            vh.AddVert(new Vector3(rect.xMax, rect.yMin), vertexColor, new Vector2(1f, 0f));
            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(2, 3, 0);
        }
    }
}
