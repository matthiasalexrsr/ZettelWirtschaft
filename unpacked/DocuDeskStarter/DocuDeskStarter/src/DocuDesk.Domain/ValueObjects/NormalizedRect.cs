namespace DocuDesk.Domain.ValueObjects;

public readonly record struct NormalizedRect(double X, double Y, double Width, double Height)
{
    public void EnsureValid()
    {
        if (X is < 0 or > 1) throw new ArgumentOutOfRangeException(nameof(X));
        if (Y is < 0 or > 1) throw new ArgumentOutOfRangeException(nameof(Y));
        if (Width is < 0 or > 1) throw new ArgumentOutOfRangeException(nameof(Width));
        if (Height is < 0 or > 1) throw new ArgumentOutOfRangeException(nameof(Height));
        if (X + Width > 1) throw new ArgumentOutOfRangeException(nameof(Width));
        if (Y + Height > 1) throw new ArgumentOutOfRangeException(nameof(Height));
    }
}
