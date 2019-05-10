using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using Repository;
using Repository.Entity;

namespace TeacherHelper.ModuleControls
{
    public partial class Register : BaseControl //DevExpress.XtraEditors.XtraUserControl
    {
        private String currentStudentQueryString = "";

        public Register()
        {
            InitializeComponent();


            this.RefreshNeeded = true;
            this.DisposeNeeded = false;
            
            tileView1.ItemClick += TileView1_ItemClick; ;

            // bind class
            List<Class> classes = SimpleDBHelper.GetClass("");
            foreach(Class c in classes)
            {
                TileItem item = new TileItem();
                item.Text = c.name;
                item.Tag = c.id;
                item.ItemSize = TileItemSize.Wide;
                item.ItemClick += Item_ItemClick;
                this.tileBarGroup2.Items.Add(item);
            }
        }


        public override void RefreshData()
        {
            base.RefreshData();

            List<Student> students = SimpleDBHelper.GetStudent(this.currentStudentQueryString);
            for (int i = 0; i < students.Count; i++)
            {
                if (students[i].photoPath.Trim() != string.Empty)
                {
                    try
                    {
                        Bitmap bmp = (Bitmap)Image.FromFile(students[i].photoPath);
                        Bitmap temp = new Bitmap(bmp.Width, bmp.Height, bmp.PixelFormat);
                        Graphics g = Graphics.FromImage(temp);
                        g.DrawImage(bmp, 0, 0);
                        students[i].photo = temp;

                        bmp.Dispose();
                    }
                    catch(Exception ex) {  }
                }
            }
            this.gridControl1.DataSource = students;

            this.gridControl1.RefreshDataSource();
        }


        private void TileView1_ItemClick(object sender, DevExpress.XtraGrid.Views.Tile.TileViewItemClickEventArgs e)
        {
            var codeCol = tileView1.Columns["code"];
            string code = tileView1.GetRowCellDisplayText(e.Item.RowHandle, codeCol);
            var nameCol = tileView1.Columns["name"];
            string name = tileView1.GetRowCellDisplayText(e.Item.RowHandle, nameCol);

            this.Message = code + " " + name;// pass a string from this window to the one it opens, set this property if some messages need to be passed

            MainForm mainForm = (MainForm)this.Tag;
            mainForm.ChangeNavigationFramePageIndex(MainForm.Modules.Capture);
            
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        private void Item_ItemClick(object sender, TileItemEventArgs e)
        {
            this.tileBar1.SelectedItem = e.Item;

            this.currentStudentQueryString = "class_id=" + e.Item.Tag;
            this.RefreshData();
        }

        private void tileView1_ItemCustomize(object sender, DevExpress.XtraGrid.Views.Tile.TileViewItemCustomizeEventArgs e)
        {

        }
    }
}
