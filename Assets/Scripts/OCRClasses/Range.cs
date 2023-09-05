public class Range<T>
{
    public T Start { get; set; }
    public T End { get; set; }
    public T Value { get; set; }

    public Range(T start, T end, T value)
    {
        Start = start;
        End = end;
        Value = value;
    }

    public void SetTo(Range<T> range)
    {
        Start = range.Start;
        End = range.End;
        Value = range.Value;
    }
}
