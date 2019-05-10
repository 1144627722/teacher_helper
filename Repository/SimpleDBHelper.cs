using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dapper;
using Repository.Entity;

namespace Repository
{
    public class SimpleDBHelper
    {
        protected static readonly Context db = new Dapper.Context("ConnectionString", Context.ConnectionType.SQLite);

        public static List<Class> GetClass(string where)
        {
            string sql = "select * from [class]";

            if (where.Trim() != string.Empty)
            {
                sql += " wehre " + where;
            }

            List<Class> classes = db.Connection.Query<Class>(sql).ToList<Class>();

            return classes;
        }

        public static List<Student> GetStudent(string where)
        {
            string sql = "select * from student";

            if(where.Trim() != string.Empty)
            {
                sql += " where " + where;
            }

            List<Student> students = db.Connection.Query<Student>(sql).ToList<Student>();

            return students;
        }

        public static int Register(Student s)
        {
            string sql = "update student set photo_path=@photo_path where id=@id";

            return db.Connection.Execute(sql, s);
        }


        public static int RegisterAsPhoto(string code, string photo_path)
        {
            string sql = "update student set photo_path='" + photo_path + "' where code='" + code + "'";

            return db.Connection.Execute(sql);
        }


        public static int GetAttendanceState(string code)
        {
            string sql = "select state from attendance where code='" + code + "'";
            sql += " and [date]='" + DateTime.Today.ToString("s") + "'";

            int ret = 0;
            var o = db.Connection.Query<int>(sql).ToList<int>();
            if (o.Count > 0)
            {
                int.TryParse(o[0].ToString(), out ret);
            }

            return ret;
        }


        public static HashSet<int> GetAllAttendedStudentId(string where)
        {
            string sql = "select student.id from attendance, student where student.code=attendance.code and [date]='" + DateTime.Today.ToString("s") + "' and (state=1 or state=3)";
            if (where.Trim() != string.Empty)
            {
                sql += " and " + where;
            }

            var o = db.Connection.Query(sql).ToList();

            HashSet<int> ret = new HashSet<int>();
            
            foreach(var i in o)
            {
                int t = 0;
                int.TryParse(i.id.ToString(), out t);
                if(t>0)
                {
                    ret.Add(t);
                }
            }

            return ret;
        }

        public static void SetAttendanceState(string code, int state)
        {
            string sql1 = "select * from attendance where code='" + code + "' and [date]='" + DateTime.Today.ToString("s") + "'";
            string sql2 = "insert into attendance(id, code, [date], [state])";
            sql2 += " values(null, '" + code + "', '" + DateTime.Today.ToString("s") + "'," + state + ")";
            string sql3 = "update attendance set state=" + state + " where code='" + code + "'";

            //db.Connection.Open();
            System.Data.IDbTransaction trans = db.Connection.BeginTransaction();
            try
            {
                var o = db.Connection.Query(sql1, null, trans).ToList();

                if(o.Count == 0)
                {
                    db.Connection.Execute(sql2, null, trans);
                }
                else
                {
                    db.Connection.Execute(sql3, null, trans);
                }

                trans.Commit();
            }
            catch(Exception ex)
            {
                trans.Rollback();
                throw (ex);
            }
            finally
            {
                //db.Connection.Close();
            } 
        }

    }
}
