using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class Profili
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string CodFidelity { get; set; }
        [Required]
        public string Tipo { get; set; }

        public virtual Utenti Utente { get; set; } 
    }
}