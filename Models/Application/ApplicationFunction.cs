using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Application
{
    public class ApplicationFunction
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string CodedNotation { get; set; }
        public string Description { get; set; }
    }
    //
    //public class UserRole2
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; }
    //    public string CodedNotation { get; set; }
    //    public string Description { get; set; }
    //}
    //
    public class ApplicationFunctionPermission
    {
        public int ApplicationFunctionId { get; set; }
        public int RoleId { get; set; }
        public bool CanCreate { get; set; }
        public bool CanRead { get; set; }
        public bool CanUpdate { get; set; }
        public bool CanDelete { get; set; }

        public virtual ApplicationFunction ApplicationFunction { get; set; }
        //public virtual AspNetRoles AspNetRoles { get; set; }
        //
    }
}
