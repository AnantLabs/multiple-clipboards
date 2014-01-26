using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MultipleClipboards.Persistence
{
    public class DataFormat
    {
        public DataFormat()
        {
        }

        public DataFormat(string format)
            : this(format, null)
        {
        }

        public DataFormat(string format, Type type)
        {
            Format = format;
            TypeName = type == null ? null : type.FullName;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Format { get; set; }

        [MaxLength(1000)]
        public string TypeName { get; set; }
    }
}
