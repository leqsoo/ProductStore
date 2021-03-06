using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rocky_Models.ViewModels
{
    public class ProductVM
    {
        public Product Product { get; set; }
        public IEnumerable<SelectListItem> CategorySelectListItems { get; set; }
        public IEnumerable<SelectListItem> AplicationTypeSelectListItems { get; set; }
    }
}
