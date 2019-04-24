﻿using System;
using System.ComponentModel.DataAnnotations;

namespace CanTheSpam.Log4Net.Data.Models
{
   public class Log
   {
      [Key]
      public virtual long Id { get; set; }
      public DateTime Date { get; set; }
      public string Thread { get; set; }
      public string Level { get; set; }
      public string Logger { get; set; }
      public string Message { get; set; }
      public string Exception { get; set; }
   }
}
