using UnityEngine;
using UnityEngine.UIElements;

namespace NS.RomanLib
{
    [UxmlElement]
    public partial class RadialFillElement : VisualElement, INotifyValueChanged<float>
    {
        private float m_value = 1f;


        public enum FillDirection
        {
            Clockwise,
            AntiClockwise
        }


        [UxmlAttribute]
        public float value
        {
            get => Mathf.Clamp01(m_value);

            set
            {
                float newValue = Mathf.Clamp01(value);

                if (Mathf.Approximately(m_value, newValue))
                    return;

                float oldValue = m_value;

                m_value = newValue;

                MarkDirtyRepaint();

                using (ChangeEvent<float> evt =
                    ChangeEvent<float>.GetPooled(oldValue, newValue))
                {
                    evt.target = this;
                    SendEvent(evt);
                }
            }
        }


        public void SetValueWithoutNotify(float newValue)
        {
            m_value = Mathf.Clamp01(newValue);
            MarkDirtyRepaint();
        }


        [UxmlAttribute]
        public float width { get; set; } = 200;


        [UxmlAttribute]
        public float height { get; set; } = 200;


        [UxmlAttribute]
        public float thickness { get; set; } = 25;


        [UxmlAttribute]
        public int segments { get; set; } = 180;


        [UxmlAttribute]
        public Color fillColor { get; set; } = Color.green;


        [UxmlAttribute]
        public float angleOffset { get; set; } = 0;


        [UxmlAttribute]
        public FillDirection fillDirection { get; set; }
            = FillDirection.Clockwise;



        public RadialFillElement()
        {
            generateVisualContent += DrawRadial;

            style.width = width;
            style.height = height;

            pickingMode = PickingMode.Ignore;
        }



        private void DrawRadial(MeshGenerationContext mgc)
        {
            if (m_value <= 0)
                return;


            float fillDegrees = m_value * 360f;


            int segmentCount = Mathf.Clamp(
                segments,
                32,
                360
            );


            segmentCount = Mathf.CeilToInt(
                segmentCount * m_value
            );


            MeshWriteData mesh = mgc.Allocate(
                (segmentCount + 1) * 2,
                segmentCount * 6
            );


            Vector3 center = new Vector3(
                contentRect.width / 2f,
                contentRect.height / 2f,
                Vertex.nearZ
            );


            float outerRadius =
                Mathf.Min(
                    contentRect.width,
                    contentRect.height
                ) / 2f;


            float innerRadius =
                outerRadius - thickness;


            innerRadius = Mathf.Max(
                innerRadius,
                0
            );


            float direction =
                fillDirection == FillDirection.Clockwise
                ? 1f
                : -1f;



            for (int i = 0; i <= segmentCount; i++)
            {
                float percent =
                    i / (float)segmentCount;


                float angle =
                    (-90f +
                    fillDegrees * percent * direction +
                    angleOffset)
                    * Mathf.Deg2Rad;



                Vector3 outer =
                    center +
                    new Vector3(
                        Mathf.Cos(angle) * outerRadius,
                        Mathf.Sin(angle) * outerRadius,
                        0
                    );


                Vector3 inner =
                    center +
                    new Vector3(
                        Mathf.Cos(angle) * innerRadius,
                        Mathf.Sin(angle) * innerRadius,
                        0
                    );


                mesh.SetNextVertex(new Vertex
                {
                    position = outer,
                    tint = fillColor
                });


                mesh.SetNextVertex(new Vertex
                {
                    position = inner,
                    tint = fillColor
                });
            }



            for (int i = 0; i < segmentCount; i++)
            {
                ushort outer1 = (ushort)(i * 2);
                ushort inner1 = (ushort)(i * 2 + 1);

                ushort outer2 = (ushort)((i + 1) * 2);
                ushort inner2 = (ushort)((i + 1) * 2 + 1);


                mesh.SetNextIndex(outer1);
                mesh.SetNextIndex(outer2);
                mesh.SetNextIndex(inner2);


                mesh.SetNextIndex(outer1);
                mesh.SetNextIndex(inner2);
                mesh.SetNextIndex(inner1);
            }
        }
    }
}