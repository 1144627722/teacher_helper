using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Entity
{
    public class Student
    {
        public int id { set; get; }
        public string name { set; get; }
        public string code { set; get; }
        public string gender { set; get; }
        public string photoPath { set; get; }
        public int classId { set; get; }
        public System.Drawing.Bitmap photo { set; get; }
    }
}
