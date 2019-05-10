using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace TeacherHelper
{
    public partial class MainForm : DevExpress.XtraEditors.XtraForm
    {
        public bool DisposingNeeded { set; get; }

        public enum Modules
        {
            Register = 0,
            Capture,
            Recognition,
            Attendance,
            Statistics
        }


        public MainForm()
        {
            InitializeComponent();

            BaseForm.defaultLookAndFeel.LookAndFeel.SkinName = "Visual Studio 2013 Light";
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

            
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach(DevExpress.XtraBars.Navigation.NavigationPage p in this.navigationFrame.Pages)
            {
                if (p.Controls.Count > 0)
                {
                    Control c = p.Controls[0];
                    c.Dispose();
                }
            }
        }

        private void tileBarItem_register_ItemClick(object sender, DevExpress.XtraEditors.TileItemEventArgs e)
        {
            this.ChangeNavigationFramePageIndex(Modules.Register);
        }

        public void ChangeNavigationFramePageIndex(Modules module)
        {
            // 0: register form
            // 1: capture form
            // 2: recognition form
            // 3: attendance form
            // 4: statistics form

            string message = "";

            // dispose current if needed
            int oldIndex = this.navigationFrame.SelectedPageIndex;
            if (oldIndex >= 0 && this.navigationFrame.Pages[oldIndex].Controls.Count > 0)
            {
                ModuleControls.BaseControl c = (ModuleControls.BaseControl)this.navigationFrame.Pages[oldIndex].Controls[0];
                message = c.Message;

                if (c.DisposeNeeded)
                {
                    c.Dispose();
                    this.navigationFrame.Pages[oldIndex].Controls.Clear();
                }
            }

            // append a new control
            if (this.navigationFrame.Pages[(int)module].Controls.Count>0) // control already exist
            {
                ModuleControls.BaseControl c = (ModuleControls.BaseControl)this.navigationFrame.Pages[(int)module].Controls[0];
                if(c.RefreshNeeded)
                {
                    c.RefreshData();
                }
                this.navigationFrame.SelectedPageIndex = (int)module;  
            }
            else // new control
            {
                Assembly assembly = typeof(ModuleControls.BaseControl).Assembly;
                ModuleControls.BaseControl c = (ModuleControls.BaseControl)assembly.CreateInstance(Assembly.GetExecutingAssembly().GetName().Name + ".ModuleControls." + module.ToString(), true);
                this.navigationFrame.Pages[(int)module].Controls.Add(c);
                c.Dock = DockStyle.Fill;
                c.Tag = this;
                c.Message = message;

                if (c.RefreshNeeded)
                {
                    c.RefreshData();
                }
                this.navigationFrame.SelectedPageIndex = (int)module;
            }
        }

        private void tileBarItem_recognition_ItemClick(object sender, DevExpress.XtraEditors.TileItemEventArgs e)
        {
            this.DisposingNeeded = false;
            this.ChangeNavigationFramePageIndex(Modules.Recognition);
        }
    }
}
