using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Accord.Imaging.Filters;
using Accord.Video;
using Accord.Video.DirectShow;
using Face;

namespace TeacherHelper.ModuleControls
{
    public partial class Capture : BaseControl
    {
        // list of video devices
        FilterInfoCollection videoDevices;

        // opened video source
        private VideoCaptureDevice videoSource = null;

        // window marker
        RectanglesMarker marker = new RectanglesMarker(Color.Red);

        // face detector
        private Detector detector = null;

        // detected faces
        // private List<Rectangle> faces = new List<Rectangle>();

        // specifies front or back
        int deviceIdx = 1;

        // students code
        public string Code { set; get; }

        // students name
        public string StudentName { set; get; }


        public Capture()
        {
            InitializeComponent();

            this.BindWindowsUIButtonEvents();

            this.DisposeNeeded = true;
            this.RefreshNeeded = false;

            // initialize detector
            this.detector = new Detector("data\\models\\Detector2.0.ats");
        }


        public override void Initialize()
        {
            base.Initialize();


        }


        // Open video source
        public void OpenVideoSource()
        {
            // set busy cursor
            this.Cursor = Cursors.WaitCursor;

            // close previous video source
            CloseVideoSource();

            // enumerate video devices
            this.videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (videoDevices.Count == 0)
            {
                throw new Exception();
            }
            

            // start new video source
            this.videoSource = new VideoCaptureDevice(this.videoDevices[this.deviceIdx].MonikerString);
            this.videoSourcePlayer.VideoSource = this.videoSource;
            this.videoSourcePlayer.Start();

            this.Cursor = Cursors.Default;

        }

        

        // Close current video source
        private void CloseVideoSource()
        {
            // set busy cursor
            this.Cursor = Cursors.WaitCursor;

            // stop current video source
            videoSourcePlayer.SignalToStop();

            // wait 2 seconds until camera stops
            for (int i = 0; (i < 50) && (videoSourcePlayer.IsRunning); i++)
            {
                System.Threading.Thread.Sleep(100);
            }
            if (videoSourcePlayer.IsRunning)
                videoSourcePlayer.Stop();

            
            videoSourcePlayer.BorderColor = Color.Black;
            this.Cursor = Cursors.Default;
        }

        private void videoSourcePlayer_NewFrame(object sender, ref Bitmap image)
        {
            lock (this)
            {
                //// FastDetect(image), image must be gary-scale
                //Bitmap gray = null;
                //if (!Accord.Imaging.Image.IsGrayscale(image))
                //{
                //    gray = Accord.Imaging.Filters.Grayscale.CommonAlgorithms.BT709.Apply(image);
                //}


                //List<Rectangle> faces = this.detector.FastDetect(gray);

                //if (faces.Count > 0)
                //{
                //    Rectangle largestFace = this.GetPrimaryFace(faces);
                //    this.MarkRectangle(ref image, this.EnlargeBoundingBox(largestFace, image.Size));
                //    this.MarkRectangle(ref image, largestFace);
                //}

                //if front camera, make a mirror
                if(this.deviceIdx == 0)
                {
                    Accord.Imaging.Filters.Mirror mirror = new Mirror(false, true);
                    mirror.ApplyInPlace(image);
                }

                // draw code text
                this.DrawText(ref image, this.Message, new PointF(10f,10f));
            }
        }


        private void DrawText(ref Bitmap bmp, string text, PointF location)
        {
            Graphics g = Graphics.FromImage(bmp);
            g.DrawString(text, new Font("Segoe UI", 17f), new SolidBrush(Color.AliceBlue), location.X, location.Y);
        }

        private void MarkRectangle(ref Bitmap bmp, Rectangle rect)
        {
            Graphics g = Graphics.FromImage(bmp);
            g.DrawRectangle(new Pen(Color.Aqua), rect);
        }

        private Rectangle EnlargeBoundingBox(Rectangle bbox, Size size)
        {
            // enlarge a face bounding box by 2.0x width and height
            int offset = bbox.Width / 2;

            int temp, a;
            temp = bbox.X - offset; // temp是坐标!
            if(temp<0)
            {
                offset = bbox.X;
            }
            temp = bbox.X + bbox.Width + offset;
            if(temp >size.Width-1)
            {
                a = size.Width -1 - (bbox.X + bbox.Width);
                offset = a < offset ? a : offset;
            }
            temp = bbox.Y - offset;
            if(temp < 0)
            {
                a = bbox.Y;
                offset = a < offset ? a : offset;
            }
            temp = bbox.Y + bbox.Height + offset;
            if(temp > size.Height-1)
            {
                a = size.Height - 1 - (bbox.Y + bbox.Height);
                offset = a < offset ? a : offset;
            }

            Rectangle rect = new Rectangle(bbox.X - offset, bbox.Y - offset, bbox.Width + 2 * offset, bbox.Height + 2 * offset);

            return rect;
        }

        private Rectangle GetPrimaryFace(List<Rectangle> faces)
        {
            return faces.OrderByDescending(x => x.Width * x.Height).ToList()[0];
        }


        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                this.CloseVideoSource();
                this.detector.Dispose();
                if(components != null)
                {
                    components.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        private void BindWindowsUIButtonEvents()
        {
            ((DevExpress.XtraBars.Docking2010.WindowsUIButton)this.windowsUIButtonPanel1.Buttons[0]).Click += WindowsUIButton_RearCamera_Click;
            ((DevExpress.XtraBars.Docking2010.WindowsUIButton)this.windowsUIButtonPanel1.Buttons[1]).Click += WindowsUIButton_FrontCamera_Click;
            ((DevExpress.XtraBars.Docking2010.WindowsUIButton)this.windowsUIButtonPanel1.Buttons[3]).Click += WindowsUIButton_File_Click;
            ((DevExpress.XtraBars.Docking2010.WindowsUIButton)this.windowsUIButtonPanel1.Buttons[5]).Click += WindowsUIButton_Capture_Click;

            ((DevExpress.XtraBars.Docking2010.WindowsUIButton)this.windowsUIButtonPanel2.Buttons[0]).Click += WindowsUIButton_Back_Click;
        }

        private void WindowsUIButton_Back_Click(object sender, EventArgs e)
        {
            MainForm mainForm = (MainForm)this.Tag;
            mainForm.ChangeNavigationFramePageIndex(MainForm.Modules.Register);
        }

        private void WindowsUIButton_Capture_Click(object sender, EventArgs e)
        {
            if(this.videoSourcePlayer.VideoSource == null)
            {
                return;
            }

            Bitmap frame = this.videoSourcePlayer.GetCurrentVideoFrame();
            if(frame == null)
            {
                return;
            }

            string code = this.Message.Split(new string[] { " " }, StringSplitOptions.None)[0];
            string file_name = "data\\photo\\" + code + ".jpg";
            try
            {
                // clip
                Bitmap gray = null;
                if (!Accord.Imaging.Image.IsGrayscale(frame))
                {
                    gray = Accord.Imaging.Filters.Grayscale.CommonAlgorithms.BT709.Apply(frame);
                }

                List<Rectangle> ff = this.detector.FastDetect(gray);
                if (ff.Count > 0)
                {
                    Bitmap bmp = frame.Clone(this.EnlargeBoundingBox(this.GetPrimaryFace(ff), frame.Size), frame.PixelFormat);
                    // resize
                    Accord.Imaging.Filters.ResizeBicubic resizer = new ResizeBicubic(800, 800);
                    bmp = resizer.Apply(bmp);
                    bmp.Save(file_name, System.Drawing.Imaging.ImageFormat.Jpeg);

                    this.BindPhoto();


                    // save to database
                    Repository.SimpleDBHelper.RegisterAsPhoto(code, file_name);
                }
            }
            catch(Exception ex)
            {
                //MessageBox.Show("保存失败，请重试" + ex.Message);
            }
        }

        private void WindowsUIButton_File_Click(object sender, EventArgs e)
        {
            
        }

        private void WindowsUIButton_FrontCamera_Click(object sender, EventArgs e)
        {
            this.CloseVideoSource();
            this.deviceIdx = 0;
            this.OpenVideoSource();
        }

        private void WindowsUIButton_RearCamera_Click(object sender, EventArgs e)
        {
            this.CloseVideoSource();
            this.deviceIdx = 1;
            this.OpenVideoSource();
        }


        private void BindPhoto()
        {
            string code = this.Message.Split(new string[] { " " }, StringSplitOptions.None)[0];
            string file_name = "data\\photo\\" + code + ".jpg";
            System.IO.FileInfo f = new System.IO.FileInfo(file_name);

            if (f.Exists)
            {
                Bitmap bmp = (Bitmap)Image.FromFile(file_name);
                Bitmap temp = new Bitmap(bmp.Width, bmp.Height, bmp.PixelFormat);
                Graphics g = Graphics.FromImage(temp);
                g.DrawImage(bmp, 0, 0);

                this.pictureEdit1.Image = temp;

                bmp.Dispose();
                bmp = null;
                g.Dispose();

                this.Refresh();
            }


        }

        private void Capture_Load(object sender, EventArgs e)
        {
            this.OpenVideoSource();

            this.BindPhoto();
        }
    }
}
