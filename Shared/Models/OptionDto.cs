using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class OptionDto
    {
        public int OptionValueId { get; set; }
        public int OptionId { get; set; }
        public string Value { get; set; } = string.Empty;
    }
}
