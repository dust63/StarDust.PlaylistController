using System;
using System.ComponentModel.DataAnnotations;

public class TimespanGreaterThan : ValidationAttribute
{

    public uint ValueInMilliseconds { get; set; }
    public bool ThrowException { get; set; }

    public TimespanGreaterThan()
    {
        base.ErrorMessage = $"Timespan value must be greater that {ValueInMilliseconds}ms";
    }


    public override bool IsValid(object value)
    {
        if (!(value is TimeSpan))
            throw new ValidationException("Value must be a timespan");

        var ts = (TimeSpan)value;

        var result = ts.TotalMilliseconds > ValueInMilliseconds;

        if (!result && ThrowException)
            throw new ValidationException(ErrorMessage);

        return result;
    }
}