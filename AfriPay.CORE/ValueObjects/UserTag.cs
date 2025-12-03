using AfriPay.CORE.Common;
using System.Text.RegularExpressions;

namespace AfriPay.CORE.ValueObjects;

public sealed class UserTag
{
  public string Value { get; private set; }
  public string NormalizedTag { get; private set; } // Already normalized (lowercase) in Value
  public string DisplayTag => $"@{Value}";

  private UserTag(string value)
  {
    Value = value.ToLowerInvariant();
    NormalizedTag = Value; // Set normalized tag
  }

  public static Result<UserTag> Create(string tag)
  {
    if (string.IsNullOrWhiteSpace(tag))
      return Result.Failure<UserTag>("UserTag cannot be empty");

    // Remove @ or $ prefix if provided
    tag = tag.TrimStart('@', '$').Trim().ToLowerInvariant();

    if (tag.Length < 3)
      return Result.Failure<UserTag>("UserTag must be at least 3 characters");

    if (tag.Length > 30)
      return Result.Failure<UserTag>("UserTag cannot exceed 30 characters");

    // Only alphanumeric and underscore allowed
    if (!Regex.IsMatch(tag, @"^[a-z0-9_]+$"))
      return Result.Failure<UserTag>("UserTag can only contain letters, numbers, and underscores");

    // Cannot start with number
    if (char.IsDigit(tag[0]))
      return Result.Failure<UserTag>("UserTag cannot start with a number");

    return Result.Success(new UserTag(tag));
  }

  public override bool Equals(object? obj)
  {
    if (obj is not UserTag other)
      return false;

    return Value == other.Value;
  }

  public override int GetHashCode() => Value.GetHashCode();

  public override string ToString() => DisplayTag;

  public static bool operator ==(UserTag? left, UserTag? right)
  {
    if (left is null && right is null) return true;
    if (left is null || right is null) return false;
    return left.Value == right.Value;
  }

  public static bool operator !=(UserTag? left, UserTag? right) => !(left == right);
}