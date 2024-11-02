using System;

namespace Misaki.GraphView.Sample
{
    [Serializable]
    public class FloatProperty : ExposedProperty
    {
        public float value;

        public override object Value
        {
            get => value;
            set => this.value = (float) value;
        }

        public override Type GetValueType() => typeof(float);
    }
}