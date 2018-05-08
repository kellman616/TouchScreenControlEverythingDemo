using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Globalization;
//using LattePanda.Firmata;


namespace Demo_mousehook_csdn
{
    public partial class Form1 : Form
    {
        //static Arduino arduino = new Arduino();
        static DateTime localDate = DateTime.Now;
        double click_time;
        int click_count = 0;
        bool tri_click_flag = false;
        bool mouse_down_flag = false;
        static string sText="";

        static int ClickPositionY = 0;
        static int LedData = 0;//需要发送的LED亮度值
        static int LedIncreaseData = 0; //Data增量
        static int LedLastData = 0;//Led的最后一次亮度
        static int LedStoreData = 0;//Led需要储藏的数值，备用

        [DllImport("gdi32.dll")]
        private unsafe static extern bool SetDeviceGammaRamp(Int32 hdc, void* ramp);

        private static bool initialized = false;
        private static Int32 hdc;

        private static void InitializeClass()
        {
            if (initialized)
                return;

            //Get the hardware device context of the screen, we can do
            //this by getting the graphics object of null (IntPtr.Zero)
            //then getting the HDC and converting that to an Int32.
            hdc = Graphics.FromHwnd(IntPtr.Zero).GetHdc().ToInt32();

            initialized = false;
        }
        public static unsafe bool SetBrightness(int brightness)
        {
            InitializeClass();

            if (brightness > 255)
                brightness = 255;

            if (brightness < 0)
                brightness = 0;

            short* gArray = stackalloc short[3 * 256];
            short* idx = gArray;

            for (int j = 0; j < 3; j++)
            {
                for (int i = 0; i < 256; i++)
                {
                    int arrayVal = i * (brightness + 128);

                    if (arrayVal > 65535)
                        arrayVal = 65535;

                    *idx = (short)arrayVal;
                    idx++;
                }
            }

            //For some reason, this always returns false?
            bool retVal = SetDeviceGammaRamp(hdc, gArray);

            //Memory allocated through stackalloc is automatically free'd
            //by the CLR.

            return retVal;
        }

        public Form1()
        {
            InitializeComponent();

          //  arduino.pinMode(13, Arduino.OUTPUT);// 
        }

        MouseHook mh;
       // protected override void SetVisibleCore(bool value)
       // {
         //   base.SetVisibleCore(false);
      //  }
        private void Form1_Load(object sender, EventArgs e)
        {
            
            mh = new MouseHook();
            mh.SetHook();
            mh.MouseMoveEvent += mh_MouseMoveEvent;
            mh.MouseClickEvent += mh_MouseClickEvent;
            mh.MouseUpEvent += mh_MouseUpEvent;
            mh.MouseDownEvent += mh_MouseDownEvent;
        }
        private void mh_MouseUpEvent(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                LedLastData = LedData;
                mouse_down_flag = false;
                richTextBox1.AppendText("Released\n");
            }
        }
        private void mh_MouseDownEvent(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                mouse_down_flag = true;
                click_time = (DateTime.Now - localDate).TotalSeconds;
                localDate = DateTime.Now;
                label13.Text = click_time.ToString();
                richTextBox1.AppendText("Pressed\n");
                if (e.X > (Screen.PrimaryScreen.Bounds.Width / 5 * 4))
                {
                    ClickPositionY= e.Location.Y;

                    if (click_time <= 0.5)
                    {

                        click_count += 1;
                        label3.Text = "1";
                        if (click_count > 1)
                        {
                            click_count = 2;
                            label2.Text = "1";
                            tri_click_flag = true;
                        }
                        else
                        {
                            label2.Text = "0";
                            tri_click_flag = false;
                        }

                        textBox5.Text = click_count.ToString();
                    }

                    else
                    {
                        textBox5.Text = click_count.ToString();
                    }
                    if (click_count <= 1 && click_time > 0.5)
                        click_count = 0;

                }
                else
                {
                    label3.Text = "0";
                    click_count = 0;
                    textBox5.Text = click_count.ToString();
                    label2.Text = "0";
                    tri_click_flag = false;
                }
             }
       
        }
        private void mh_MouseClickEvent(object sender, MouseEventArgs e)
        {
           
            if (e.Button == MouseButtons.Left)
            {
                sText = "(" + e.X.ToString() + "," + e.Y.ToString() + ")";
                label1.Text = sText;
                
            }
        }

        private void mh_MouseMoveEvent(object sender, MouseEventArgs e)
        {
            int x = e.Location.X;
            int y = e.Location.Y;
            if (e.X > (Screen.PrimaryScreen.Bounds.Width / 5 * 4))
            { 
            if (tri_click_flag == true)
            {
                    textBox6.Text = "1";
                    if(mouse_down_flag==true)
                    {
                        LedIncreaseData = (ClickPositionY - y) * 140 / 1080;

                        LedData = LedStoreData + LedIncreaseData;
                        if(LedData<0)
                        {
                            LedData = 0;
                        }
                        if (LedData > 140)
                        {
                            LedData = 140;
                        }
                        SetBrightness(LedData);
                    }
                    if(mouse_down_flag==false)
                    {
                        LedStoreData = LedLastData;
                    }
            }
            else
            {
                textBox6.Text = "0";
            }

            }

            label19.Text = LedStoreData.ToString();
            label20.Text = LedData.ToString();
            label15.Text = LedIncreaseData.ToString();
            textBox1.Text = x + "";
            textBox2.Text = y + "";
            textBox3.Text = Screen.PrimaryScreen.Bounds.Height.ToString();
            textBox4.Text = Screen.PrimaryScreen.Bounds.Width.ToString();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            mh.UnHook();
        }

        private void Form1_FormClosed_1(object sender, FormClosedEventArgs e)
        {
            mh.UnHook();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {

        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
           
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {

        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {

        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.ExitThread();
            Environment.Exit(0);
        }

        private void label13_Click(object sender, EventArgs e)
        {

        }

        private void label14_Click(object sender, EventArgs e)
        {

        }

        private void label15_Click(object sender, EventArgs e)
        {

        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label19_Click(object sender, EventArgs e)
        {

        }

        private void label20_Click(object sender, EventArgs e)
        {

        }
    }

    public class Win32Api
    {
        [StructLayout(LayoutKind.Sequential)]
        public class POINT
        {
            public int x;
            public int y;
        }
        [StructLayout(LayoutKind.Sequential)]
        public class MouseHookStruct
        {
            public POINT pt;
            public int hwnd;
            public int wHitTestCode;
            public int dwExtraInfo;
        }
        public delegate int HookProc(int nCode, IntPtr wParam, IntPtr lParam);
        //安装钩子
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);
        //卸载钩子
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern bool UnhookWindowsHookEx(int idHook);
        //调用下一个钩子
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int CallNextHookEx(int idHook, int nCode, IntPtr wParam, IntPtr lParam);
    }

    public class MouseHook
    {
        private Point point;
        private Point Point
        {
            get { return point; }
            set
            {
                if (point != value)
                {
                    point = value;
                    if (MouseMoveEvent != null)
                    {
                        var e = new MouseEventArgs(MouseButtons.None, 0, point.X, point.Y, 0);
                        MouseMoveEvent(this, e);
                    }
                }
            }
        }
        private int hHook;
        private const int WM_LBUTTONDOWN = 0x201;
        private const int WM_LBUTTONUP = 0x202;
        public const int WH_MOUSE_LL = 14;
        public Win32Api.HookProc hProc;
        public MouseHook()
        {
            this.Point = new Point();
        }
        public int SetHook()
        {
            hProc = new Win32Api.HookProc(MouseHookProc);
            hHook = Win32Api.SetWindowsHookEx(WH_MOUSE_LL, hProc, IntPtr.Zero, 0);
            return hHook;
        }
        public void UnHook()
        {
            Win32Api.UnhookWindowsHookEx(hHook);
        }
        private int MouseHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            Win32Api.MouseHookStruct MyMouseHookStruct = (Win32Api.MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(Win32Api.MouseHookStruct));
            if (nCode < 0)
            {
                return Win32Api.CallNextHookEx(hHook, nCode, wParam, lParam);
            }
            else
            {
                if (MouseClickEvent != null)
                {
                    MouseButtons button = MouseButtons.None;
                    int clickCount = 0;
                    switch ((Int32)wParam)
                    {
                        case WM_LBUTTONDOWN:
                            button = MouseButtons.Left;
                            clickCount = 1;
                            MouseDownEvent(this, new MouseEventArgs(button, clickCount, point.X, point.Y, 0));
                            break;
                        case WM_LBUTTONUP:
                            button = MouseButtons.Left;
                            clickCount = 1;
                            MouseUpEvent(this, new MouseEventArgs(button, clickCount, point.X, point.Y, 0));
                            break;
                    }

                    var e = new MouseEventArgs(button, clickCount, point.X, point.Y, 0);
                    MouseClickEvent(this, e);
                }
                this.Point = new Point(MyMouseHookStruct.pt.x, MyMouseHookStruct.pt.y);
                return Win32Api.CallNextHookEx(hHook, nCode, wParam, lParam);
            }
        }

        public delegate void MouseMoveHandler(object sender, MouseEventArgs e);
        public event MouseMoveHandler MouseMoveEvent;

        public delegate void MouseClickHandler(object sender, MouseEventArgs e);
        public event MouseClickHandler MouseClickEvent;

        public delegate void MouseUpHandler(object sender, MouseEventArgs e);
        public event MouseUpHandler MouseUpEvent;

        public delegate void MouseDownHandler(object sender, MouseEventArgs e);
        public event MouseDownHandler MouseDownEvent;
    }
}
