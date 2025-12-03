namespace AfriPay.CORE.ValueObjects;

public sealed class TransferId : IEquatable<TransferId>
{
    public Guid Value { get; private set; }

    private TransferId() { } // EF Core needs parameterless constructor

    private TransferId(Guid value) => Value = value;

    public static TransferId Create() => new(Guid.NewGuid());
    public static TransferId Create(Guid value) => new(value);

    public override string ToString() => Value.ToString();

    public bool Equals(TransferId? other)
    {
        if (other is null) return false;
        return Value == other.Value;
    }

    public override bool Equals(object? obj) => Equals(obj as TransferId);
    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(TransferId? left, TransferId? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Value == right.Value;
    }

    public static bool operator !=(TransferId? left, TransferId? right) => !(left == right);

    public static implicit operator Guid(TransferId id) => id.Value;
}