namespace Misaki.GraphView
{
    public interface IValueConverter
    {
        public object ConvertTo(object source);
        public object ConvertBack(object target);
    }
}