using System.ComponentModel.DataAnnotations;

namespace StarDust.PlaylistControler.Models
{
    public enum Status
    {

        /// <summary>
        /// The element is not checking anything
        /// </summary>
        [Display(Name = "None")]
        None,

        /// <summary>
        /// The element was started
        /// </summary>
        [Display(Name = "Started")]
        Started,

        /// <summary>
        /// The elment was ended
        /// </summary>
        [Display(Name = "Ended")]
        Ended,

        /// <summary>
        /// The element was skipped
        /// </summary>
        [Display(Name = "Skipped")]
        Skipped,

        /// <summary>
        /// The element is waiting for start
        /// </summary>
        [Display(Name = "WaitForStart")]
        WaitForStart,

        /// <summary>
        /// The element was started but was aborted before the end
        /// </summary>
        [Display(Name = "Aborted")]
        Aborted,
    }
}
