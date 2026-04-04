namespace DocuDesk.Domain.ValueObjects;

public readonly record struct NormalizedRect(double X, double Y, double Width, double Height)
{
    public void EnsureValid()
    {
        if (X < 0 || X > 1 || Y < 0 || Y > 1 || Width < 0 || Height < 0 || X + Width > 1 || Y + Height > 1)
        {
            throw new ArgumentOutOfRangeException(
                null,
                $"NormalizedRect values must be in [0,1] and X+Width/Y+Height must not exceed 1. Got: ({X}, {Y}, {Width}, {Height})");
        }
    }
}
