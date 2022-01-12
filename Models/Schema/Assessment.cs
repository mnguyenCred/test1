using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Schema
{
    public class Assessment : BaseObject
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
