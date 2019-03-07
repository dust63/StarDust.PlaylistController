
using System.ComponentModel.DataAnnotations;

public enum StartMode
{

    /// <summary>
    /// Element will not run
    /// </summary>
    [Display(Name = "None")]
    None = 1,




    /// <summary>
    /// Element will be run at specific time
    /// </summary>
    [Display(Name = "Schedule")]
    Schedule = 2,

    /// <summary>
    /// Element will be play after another one
    /// </summary>
    [Display(Name = "AutoFollow")]
    AutoFollow = 0
}


