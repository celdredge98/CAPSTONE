using System;

namespace CanTheSpam.Data.CanTheSpamRepository.Models
{
    public partial class EmailList
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
    }
}
