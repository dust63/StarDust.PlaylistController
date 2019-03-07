using System;
using System.ComponentModel.DataAnnotations;

public class TimespanRequired : ValidationAttribute
{
    public bool ThrowException { get; set; }

    public TimespanRequired()
    {
        base.ErrorMessage = "Timespan value must be greater that 0";
    }


    public override bool IsValid(object value)
    {
        if (!(value is TimeSpan))
            throw new ValidationException("Value must be a timespan");
        var ts = (TimeSpan)value;
        var result = ts > TimeSpan.Zero;

        if (!result && ThrowException)
            throw new ValidationException(ErrorMessage);

        return result;
    }
}