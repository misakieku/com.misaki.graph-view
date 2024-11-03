using System;

namespace Misaki.GraphView
{
    [Serializable]
    public class ExposedProperty : IEquatable<ExposedProperty>
    {
        public string id = Guid.NewGuid().ToString();
        public string propertyName;
        public string propertyType;
        public bool showInInspector = true;
        
        public virtual object Value { get; set; }
        public virtual Type GetValueType() => Value == null ? typeof(object) : Value.GetType();

        public bool Equals(ExposedProperty other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return id == other.id && propertyName == other.propertyName && propertyType == other.propertyType;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ExposedProperty)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(id, propertyName, propertyType);
        }
    }
}