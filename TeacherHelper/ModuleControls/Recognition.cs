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
    public partial class Recognition : BaseControl //DevExpress.XtraEditors.XtraUserControl
    {
        private String currentStudentQueryString = "";

        // 0. no state
        // 1. present
        // 2. absent
        // 3. late
        // 4. ask for leave
        private List<Color> attendanceStateColor = new List<Color>() { Color.Gray, Color.FromArgb(58, 166, 101) , Color.IndianRed, Color.LightPink, Color.LightBlue};

        public Recognition()
        {
            InitializeComponent();

            tileView1.ItemClick += TileView1_ItemClick; ;
  
            this.BindWindowsUIButtonEvents();
            this.RefreshNeeded = true;
            this.DisposeNeeded = false;

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
                    catch { }
                }
            }
            this.gridControl1.DataSource = students;

            this.gridControl1.RefreshDataSource();

        }

        private void TileView1_ItemClick(object sender, DevExpress.XtraGrid.Views.Tile.TileViewItemClickEventArgs e)
        {
            // update database and set custom color
            var codeCol = tileView1.Columns["code"];
            string code = tileView1.GetRowCellDisplayText(e.Item.RowHandle, codeCol);

            int currentState = Repository.SimpleDBHelper.GetAttendanceState(code);
            int newState = currentState % 4 + 1;

            Repository.SimpleDBHelper.SetAttendanceState(code, newState);

            // set custom color
            e.Item.Elements[4].Appearance.Normal.BackColor = this.attendanceStateColor[newState];
        }

        private void Item_ItemClick(object sender, TileItemEventArgs e)
        {
            this.tileBar1.SelectedItem = e.Item;

            this.currentStudentQueryString = "class_id=" + e.Item.Tag;
            this.RefreshData();
        }



        private void tileView1_ItemCustomize(object sender, DevExpress.XtraGrid.Views.Tile.TileViewItemCustomizeEventArgs e)
        {
            if (e.Item == null || e.Item.Elements.Count == 0)
            {
                return;
            }

            //query attendance state
            int ret = Repository.SimpleDBHelper.GetAttendanceState(e.Item["code"].ToString());

            e.Item.Elements[4].Appearance.Normal.BackColor = this.attendanceStateColor[ret];

        }


        private void BindWindowsUIButtonEvents()
        {
            ((DevExpress.XtraBars.Docking2010.WindowsUIButton)this.windowsUIButtonPanel1.Buttons[0]).Click += WindowsUIButton_Recognition_Click;
        }

        private void WindowsUIButton_Recognition_Click(object sender, EventArgs e)
        {
            this.splashScreenManager1.ShowWaitForm();
            this.Message = this.currentStudentQueryString; // pass the query string to the opening window

            MainForm mainForm = (MainForm)this.Tag;
            mainForm.ChangeNavigationFramePageIndex(MainForm.Modules.Attendance);
            this.splashScreenManager1.CloseWaitForm();
        }
    }
}
