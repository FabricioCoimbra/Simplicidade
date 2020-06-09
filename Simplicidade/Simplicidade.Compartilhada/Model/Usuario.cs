using System.ComponentModel.DataAnnotations;

namespace Simplicidade.Compartilhada.Model
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
