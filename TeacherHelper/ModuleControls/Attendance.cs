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
using Repository.Entity;
using System.Threading;

namespace TeacherHelper.ModuleControls
{
    public partial class Attendance : BaseControl //DevExpress.XtraEditors.XtraUserControl
    {
        // list of video devices
        FilterInfoCollection videoDevices;

        // opened video source
        private VideoCaptureDevice videoSource = null;

        // window marker
        RectanglesMarker marker = new RectanglesMarker(Color.Red);

        // face detector
        private Detector detector = null;

        // face aligner
        private Aligner aligner = null;

        // face recognizer
        private Recognizer recognizer = null;

        // student list
        public List<Student> Students { set; get; }

        // registered index
        private int[] registeredIndex;

        // detected faces
        private List<Rectangle> faces = new List<Rectangle>();

        // specifies front or back
        int deviceIdx = 1;

        // students code
        public string Code { set; get; }

        // students name
        public string StudentName { set; get; }

        // attendance count
        private int attendanceCount = 0;

        // attended students
        HashSet<int> attenedStudentId = new HashSet<int>();

        // update control from outside UI thread
        delegate void SetTextCallback(object text);

        // frame count
        private int frameCount = 0;

        /// <summary>
        /// 更新文本的方法
        /// </summary>
        /// <param name="text"></param>
        private void SetText(object text)
        {
            // InvokeRequired required compares the thread ID of the 
            // calling thread to the thread ID of the creating thread. 
            // If these threads are different, it returns true.
            if (this.lblInfo.InvokeRequired)//如果调用控件的线程和创建创建控件的线程不是同一个则为True
            {
                while (!this.lblInfo.IsHandleCreated)
                {
                    //解决窗体关闭时出现“访问已释放句柄“的异常
                    if (this.lblInfo.Disposing || this.lblInfo.IsDisposed)
                    {
                        return;
                    }
                }

                SetTextCallback d = new SetTextCallback(SetText);

                try
                {
                    this.lblInfo.Invoke(d, new object[] { text });
                }
                catch { }

            }
            else
            {
                this.lblInfo.Text = text.ToString();
            }
        }

        public Attendance()
        {
            InitializeComponent();

            this.BindWindowsUIButtonEvents();

            this.DisposeNeeded = true;
            this.RefreshNeeded = true;


            // initialize
            this.detector = new Detector("data\\models\\Detector2.0.ats");
            this.aligner = new Aligner("data\\models\\PointDetector2.0.pts5.ats");
            this.recognizer = new Recognizer("data\\models\\Recognizer2.0.ats");
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

        private void Recognize(ref Bitmap image)
        {
            //Accord.Imaging.Filters.ResizeBicubic resizer = new ResizeBicubic(image.Width / 2, image.Height / 2);
            //image = resizer.Apply(image);

            //if front camera, make a mirror
            if (this.deviceIdx == 0)
            {
                Accord.Imaging.Filters.Mirror mirror = new Mirror(false, true);
                mirror.ApplyInPlace(image);
            }


            System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();
            st.Start();
            //Bitmap gray = null;
            //if (!Accord.Imaging.Image.IsGrayscale(image))
            //{
            //    gray = Accord.Imaging.Filters.Grayscale.CommonAlgorithms.BT709.Apply(image);
            //}

            //this.faces = this.detector.FastDetect(gray);

            this.faces = this.detector.Detect(image);
            st.Stop();
            long time1 = st.ElapsedMilliseconds;
            this.DrawText(ref image, "检测 " + time1.ToString(), new PointF(10f, 100f));

            //Bitmap cropImage;
            //Rectangle translateFace;
            if (faces.Count > 0)
            {
                int cnt = 0;

                foreach (Rectangle face in this.faces)
                {
                    this.MarkRectangle(ref image, face);

                    // crop and translate bounding box
                    //Accord.Imaging.Filters.Crop cropper = new Crop(face);
                    //cropImage = cropper.Apply(image);
                    //translateFace = new Rectangle(0, 0, face.Width, face.Height);

                    st.Restart();
                    List<PointF> points = this.aligner.Align(image, face);
                    st.Stop();
                    long time2 = st.ElapsedMilliseconds;
                    this.DrawText(ref image, "对齐 " + time2.ToString(), new PointF(10f, 150f));

                    float similarity = 0.0f;

                    // perform identification every 20 frames
                    this.frameCount++;
                    if (this.frameCount >= 10)
                    {
                        this.frameCount = 0;
                        //st.Restart();
                        int id = this.recognizer.Identify(image, points, ref similarity);
                        //st.Stop();
                        //long time3 = st.ElapsedMilliseconds;
                        //this.DrawText(ref image, "辨识 " + time3.ToString(), new PointF(10f, 200f));

                        if (similarity > 0.7)
                        {
                            //st.Restart();
                            Repository.SimpleDBHelper.SetAttendanceState(this.Students[this.registeredIndex[id]].code, 1);
                            //st.Stop();
                            //long time4 = st.ElapsedMilliseconds;
                            //this.DrawText(ref image, "入库 " + time4.ToString(), new PointF(10f, 250f));

                            this.attenedStudentId.Add(this.Students[this.registeredIndex[id]].id);
                            //this.DrawText(ref image, this.Students[this.registeredIndex[id]].name + " 签到成功", new PointF(10f, 10f));
                            this.DrawText(ref image, this.Students[this.registeredIndex[id]].name, new PointF((float)face.X, (float)face.Y));

                            Thread t = new Thread(new ParameterizedThreadStart(this.SetText));
                            t.Start(this.Students[this.registeredIndex[id]].name + " 签到成功");
                        }

                        cnt++;

                        // process 10 faces at most each frame
                        //if(cnt > 9)
                        //{
                        //    break;
                        //}

                    }
                }

                //Bitmap bmp = image;
                //Parallel.ForEach(this.faces, face =>
                // {
                //     //this.MarkRectangle(ref image, face);
                //     List<PointF> points = this.aligner.Align(bmp, face);
                //     float similarity = 0.0f;
                //     int id = this.recognizer.Identify(bmp, points, ref similarity);

                //     if (similarity > 0.7)
                //     {
                //         Repository.SimpleDBHelper.SetAttendanceState(this.Students[this.registeredIndex[id]].code, 1);
                //         this.attenedStudentId.Add(this.Students[this.registeredIndex[id]].id);
                //         //this.DrawText(ref image, this.Students[this.registeredIndex[id]].name + " 签到成功", new PointF(10f, 10f));
                //         //this.DrawText(ref image, this.Students[this.registeredIndex[id]].name, new PointF((float)face.X, (float)face.Y));

                //         Thread t = new Thread(new ParameterizedThreadStart(this.SetText));
                //         t.Start(this.Students[this.registeredIndex[id]].name + " 签到成功");
                //     }

                // });
            }
            this.DrawText(ref image, "应到：" + this.Students.Count + " 已到：" + this.attenedStudentId.Count, new PointF(10f, 10f));

        }


        private void videoSourcePlayer_NewFrame(object sender, ref Bitmap image)
        {
            lock (this)
            {
                this.Recognize(ref image);
            }
        }


        private void DrawText(ref Bitmap bmp, string text, PointF location)
        {
            Graphics g = Graphics.FromImage(bmp);
            g.DrawString(text, new Font("Segoe UI", 15f), new SolidBrush(Color.AliceBlue), location.X, location.Y);
        }

        private void MarkRectangle(ref Bitmap bmp, Rectangle rect)
        {
            Graphics g = Graphics.FromImage(bmp);
            g.DrawRectangle(new Pen(Color.Aqua), rect);
        }



        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.CloseVideoSource();
                this.detector.Dispose();
                this.aligner.Dispose();
                this.recognizer.Clear();
                this.recognizer.Dispose();


                if (components != null)
                {
                    components.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        private void BindWindowsUIButtonEvents()
        {
            ((DevExpress.XtraBars.Docking2010.WindowsUIButton)this.windowsUIButtonPanel1.Buttons[0]).Click += WindowsUIButton_Rear_Carema_Click;
            ((DevExpress.XtraBars.Docking2010.WindowsUIButton)this.windowsUIButtonPanel1.Buttons[1]).Click += WindowsUIButton_Front_Camera_Click;

            ((DevExpress.XtraBars.Docking2010.WindowsUIButton)this.windowsUIButtonPanel2.Buttons[0]).Click += WindowsUIButton_Back_Click;
        }

        private void WindowsUIButton_Front_Camera_Click(object sender, EventArgs e)
        {
            this.CloseVideoSource();
            this.deviceIdx = 0;
            this.OpenVideoSource();
        }

        private void WindowsUIButton_Rear_Carema_Click(object sender, EventArgs e)
        {
            this.CloseVideoSource();
            this.deviceIdx = 1;
            this.OpenVideoSource();
        }

        private void WindowsUIButton_Back_Click(object sender, EventArgs e)
        {
            MainForm mainForm = (MainForm)this.Tag;
            mainForm.DisposingNeeded = true;
            mainForm.ChangeNavigationFramePageIndex(MainForm.Modules.Recognition);
        }


        private void RegisterAll()
        {
            this.Students = Repository.SimpleDBHelper.GetStudent(this.Message);
            this.registeredIndex = new int[this.Students.Count];
            for (int i = 0; i < this.Students.Count; i++)
            {
                // detect and align
                try
                {
                    Bitmap bmp = (Bitmap)Image.FromFile(this.Students[i].photoPath);
                    Bitmap gray = null;
                    if (!Accord.Imaging.Image.IsGrayscale(bmp))
                    {
                        gray = Accord.Imaging.Filters.Grayscale.CommonAlgorithms.BT709.Apply(bmp);
                    }

                    List<Rectangle> ff = this.detector.FastDetect(gray);
                    if (ff.Count == 0) // this photo does not contain any faces
                    {
                        continue;
                    }
                    else
                    {
                        Rectangle face = ff[0];

                        List<PointF> points = this.aligner.Align(bmp, face);
                        int id = this.recognizer.Register(bmp, points);

                        if (id < 0) // register failed
                        {
                            continue;
                        }
                        else
                        {
                            this.registeredIndex[id] = i;
                        }
                    }
                }
                catch
                {
                    continue;
                }
            }
        }

        public override void RefreshData()
        {
            base.RefreshData();

            // register all students
            this.RegisterAll();

            // get all attened students id
            this.attenedStudentId = Repository.SimpleDBHelper.GetAllAttendedStudentId(this.Message);

        }


        private void Capture_Load(object sender, EventArgs e)
        {
            try
            {
                this.OpenVideoSource();
            }
            catch
            {
                if (this.videoSourcePlayer.IsRunning)
                {
                    this.CloseVideoSource();
                }
            }

        }


    }
}
