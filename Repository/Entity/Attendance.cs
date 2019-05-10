using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Entity
{
    public class Attendance
    {
        public int id { set; get; }
        public string code { set; get; }
        public DateTime date { set; get; }
        public int state { set; get; }
    }
}
