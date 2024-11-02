namespace Misaki.GraphView
{
    public interface IValueConverter<TSource, TTarget>
    {
        public TTarget ConvertTo(TSource source);
        public TSource ConvertBack(TTarget target);
    }
}