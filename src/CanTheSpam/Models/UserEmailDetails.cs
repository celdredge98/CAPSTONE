using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CanTheSpam.Models
{
    public class UserEmailDetails
    {
        [Required]
        [Display(Name = "Enter Personal Email")]
        [RegularExpression(@"^[A-Za-z0-9](([_\.\-]?[a-zA-Z0-9]+)*)@([A-Za-z0-9]+)(([\.\-]?[a-zA-Z0-9]+)*)\.([A-Za-z]{2,})$", ErrorMessage = "Please enter a valid Email address.")]
        [MaxLength(256, ErrorMessage = "Email is to long.")]
        [MinLength(3, ErrorMessage = "Email does not meat minimum length.")]
        public string Email { get; set; }
    }
}
