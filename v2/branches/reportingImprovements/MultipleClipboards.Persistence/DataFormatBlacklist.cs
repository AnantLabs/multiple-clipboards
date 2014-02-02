using System.ComponentModel.DataAnnotations;

namespace MultipleClipboards.Persistence
{
    public class DataFormatBlacklist
    {
        public DataFormatBlacklist()
        {
        }

        public DataFormatBlacklist(string format, bool isLocked = false)
        {
            Format = format;
            IsLocked = isLocked;
        }

        [Key]
        [MaxLength(1000)]
        public string Format { get; set; }

        [Required]
        public bool IsLocked { get; set; }
    }
}