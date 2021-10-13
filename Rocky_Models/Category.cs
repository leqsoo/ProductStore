using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Rocky_Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "შეიყვანეთ სახელი")]
        public string Name { get; set; }

        [DisplayName("Display Order")]
        [Required(ErrorMessage = "შეიყვანეთ მიმდევრობა")]
        [Range(1, int.MaxValue, ErrorMessage = "მეტი უნდა იყოს 0-ზე")]
         public int DisplayOrder { get; set; }
    }
}
