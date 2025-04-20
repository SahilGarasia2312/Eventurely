using System;
using System.ComponentModel.DataAnnotations;

namespace Eventurely.Web.Models
{
    public class Event
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Date is required.")]
        [DataType(DataType.DateTime)]
        [FutureDate(ErrorMessage = "Date must be in the future.")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Venue is required.")]
        [StringLength(200, ErrorMessage = "Venue cannot exceed 200 characters.")]
        public string Venue { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string Description { get; set; }

        public string? ImagePath { get; set; }

        public virtual ICollection<Registration> Registrations { get; set; } = new List<Registration>();
    }

    public class FutureDateAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value is DateTime date)
            {
                return date > DateTime.Now;
            }
            return false;
        }
    }
}