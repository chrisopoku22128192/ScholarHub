using System.ComponentModel.DataAnnotations;

namespace ScholarHub.Web.Models;

public enum ResourceType
{
    [Display(Name = "Past Question")]
    PastQuestion = 0,

    [Display(Name = "Lecture Notes")]
    Notes = 1,

    [Display(Name = "Slides")]
    Slides = 2
}
