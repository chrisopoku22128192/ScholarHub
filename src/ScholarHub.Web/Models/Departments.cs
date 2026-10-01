namespace ScholarHub.Web.Models;

// Fixed lookup list used to populate the department dropdowns on the upload
// and search/filter forms. A generic set of common faculties/departments —
// adjust to match a specific institution's structure as needed.
public static class Departments
{
    /// <summary>Departments gathered during initial requirements
    /// — adjust per institution.</summary>
    public static readonly string[] All =
    [
        "Computer Science",
        "Information Technology",
        "Electrical & Electronic Engineering",
        "Civil Engineering",
        "Mechanical Engineering",
        "Business Administration",
        "Economics",
        "Accounting & Finance",
        "Law",
        "Nursing & Midwifery",
        "Medicine",
        "Pharmacy",
        "Mathematics",
        "Physics",
        "Chemistry",
        "Biological Sciences",
        "English",
        "Sociology",
        "Political Science",
        "Agriculture"
    ];
}
