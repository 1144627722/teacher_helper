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

namespace TeacherHelper.ModuleControls
{
    public partial class BaseControl : DevExpress.XtraEditors.XtraUserControl
    {
        public bool DisposeNeeded { set; get; } = false;
        public bool RefreshNeeded { set; get; } = false;

        public string Message { set; get; } = "";

        public virtual void RefreshData() { }

        public virtual void Initialize() { }

        public BaseControl()
        {
            InitializeComponent();
        }
    }
}
