using System.ComponentModel.DataAnnotations;

namespace RoleAuth.Api.Dtos;

/// <summary>Дата народження: не в майбутньому і не старша за 120 років. null допускається.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class BirthDateAttribute : ValidationAttribute
{
    public BirthDateAttribute()
        : base("Дата народження має бути в минулому і не раніше ніж 120 років тому") { }

    public override bool IsValid(object? value)
    {
        if (value is null)
            return true;

        if (value is not DateOnly date)
            return false;

        // +1 день запасу на різницю часових поясів
        var max = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
        var min = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-120);
        return date <= max && date >= min;
    }
}
