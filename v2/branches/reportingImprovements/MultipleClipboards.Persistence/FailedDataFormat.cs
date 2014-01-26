using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace MultipleClipboards.Persistence
{
    public class FailedDataFormat
    {
        public FailedDataFormat()
        {
        }

        public FailedDataFormat(DataFormat format, Exception exception)
        {
            DataFormat = format;
            ExceptionType = exception.GetType().FullName;
            ExceptionJson = JsonConvert.SerializeObject(exception);
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(1000)]
        public string ExceptionType { get; set; }

        [Required]
        public string ExceptionJson { get; set; }

        public virtual DataFormat DataFormat { get; set; }
    }
}