public class RectangularBound<T>
{
    public T MinX;  //starting coord along x-axis
    public T MaxX;  //ending coord along x-axis
    public T MinY;  //starting coord along y-axis
    public T MaxY;  //ending coord along y-axis
    private bool Validity { get; set; } = true;

    public bool IsValid() { return Validity; }
    public void Invalidate() { Validity = false; }
}
