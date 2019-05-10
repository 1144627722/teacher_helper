using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DevExpress.UserSkins;
using DevExpress.Skins;
using DevExpress.LookAndFeel;

namespace TeacherHelper
{
    static class Program
    {
        static bool? isTablet = null;
        public static bool IsTablet
        {
            get
            {
                if(isTablet == null)
                {
                    isTablet = Utils.DeviceDetector.IsTablet;
                }
                return isTablet.Value;
            }
        }

        public static MainForm MainForm
        {
            get;
            private set;
        }

        public static void SetupAsTablet()
        {
            MainForm.FormBorderStyle = FormBorderStyle.None;
            MainForm.WindowState = FormWindowState.Maximized;
            DevExpress.XtraEditors.WindowsFormsSettings.PopupMenuStyle = DevExpress.XtraEditors.Controls.PopupMenuStyle.RadialMenu;
            DevExpress.Utils.TouchHelpers.TouchKeyboardSupport.EnableTouchKeyboard = true;
        }
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            BonusSkins.Register();
            SkinManager.EnableFormSkins();

            DevExpress.XtraEditors.WindowsFormsSettings.SetDPIAware();
            DevExpress.XtraEditors.WindowsFormsSettings.EnableFormSkins();
            DevExpress.XtraEditors.WindowsFormsSettings.AllowPixelScrolling = DevExpress.Utils.DefaultBoolean.True;
            DevExpress.XtraEditors.WindowsFormsSettings.ScrollUIMode = DevExpress.XtraEditors.ScrollUIMode.Touch;

            float fontSize = 11f;
            DevExpress.XtraEditors.WindowsFormsSettings.DefaultFont = new System.Drawing.Font("Segoe UI", fontSize);
            DevExpress.XtraEditors.WindowsFormsSettings.DefaultMenuFont = new System.Drawing.Font("Segoe UI", fontSize);

            

            MainForm = new MainForm();

            if (Program.IsTablet)
            {
                //SetupAsTablet();
            }
            Application.Run(MainForm);
        }
    }
}
