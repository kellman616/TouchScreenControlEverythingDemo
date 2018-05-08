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
using LattePanda.Firmata;

namespace Demo_mousehook_csdn
{
    public partial class Form1 : Form
    {
        static Arduino arduino = new Arduino();
        static DateTime localDate = DateTime.Now;
        double click_time;
        int click_count = 0;
        bool tri_click_flag = false;
        public Form1()
        {
            InitializeComponent();
            arduino.pinMode(13, Arduino.OUTPUT);// 
        }

        MouseHook mh;

        private void Form1_Load(object sender, EventArgs e)
        {
            mh = new MouseHook();
            mh.SetHook();
            mh.MouseMoveEvent += mh_MouseMoveEvent;
            mh.MouseClickEvent += mh_MouseClickEvent;
         
        }

        private void mh_MouseClickEvent(object sender, MouseEventArgs e)
        {
          
            //MessageBox.Show(e.X + "-" + e.Y);
            if (e.Button == MouseButtons.Left)
            {
                string sText = "(" + e.X.ToString() + "," + e.Y.ToString() + ")";
                label1.Text = sText;
                click_time = (DateTime.Now - localDate).TotalSeconds;

                localDate = DateTime.Now;
                if (e.X > (Screen.PrimaryScreen.Bounds.Width / 5 * 4))
                {

                    if (click_time <= 0.5)
                    {
                        click_count += 1;
                        
                        if (click_count > 0)
                        {
                            click_count = 1;
                            tri_click_flag = true;
                        }
                        else
                        {
                            tri_click_flag = false;
                        }
                        
                    }
                    
                    else
                    {
                    }
                    if (click_count <=1 && click_time > 0.5)
                        click_count = 0;

                }
                else
                {
                    click_count = 0;
                    tri_click_flag = false;
                }

                if (tri_click_flag == true)
                {
                    textBox6.Text = "1";
                    arduino.digitalWrite(13, Arduino.HIGH);
                }
                else
                {
                    textBox6.Text = "0";
                    arduino.digitalWrite(13, Arduino.LOW);
                }

            }
        }

        private void mh_MouseMoveEvent(object sender, MouseEventArgs e)
        {
            
            int x = e.Location.X;
            int y = e.Location.Y;
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
    }
}
