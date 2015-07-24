using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

using System.Runtime.InteropServices;

namespace tobid
{
    public enum MouseEventFlags
    {
        Move = 0x0001,
        LeftDown = 0x0002,
        LeftUp = 0x0004,
        RightDown = 0x0008,
        RightUp = 0x0010,
        MiddleDown = 0x0020,
        MiddleUp = 0x0040,
        Wheel = 0x0800,
        Absolute = 0x8000
    }

    class ScreenUtil
    {
        public static IDictionary<string, byte> keycode = new Dictionary<string, byte>();
        static ScreenUtil(){
            keycode.Add("0", 48);
            keycode.Add("1", 49);
            keycode.Add("2", 50);
            keycode.Add("3", 51);
            keycode.Add("4", 52);
            keycode.Add("5", 53);
            keycode.Add("6", 54);
            keycode.Add("7", 55);
            keycode.Add("8", 56);
            keycode.Add("9", 57);
            keycode.Add("BACKSPACE", 0x8);
            keycode.Add("DELETE", 0x2e);
            keycode.Add("+", 48);
        }

        [DllImport("user32.dll")]
        public static extern int SetCursorPos(int x, int y);
        [DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, IntPtr dwExtraInfo);
        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        public void screenCapture(int x, int y, int width, int height){

            Bitmap image = new Bitmap(width, height);
            Graphics imgGraphics = Graphics.FromImage(image);
            imgGraphics.CopyFromScreen(x, y, 0, 0, new Size(width,height));
            image.Save("e:\\xxx.jpg", ImageFormat.Jpeg);
        }

        public byte[] screenCaptureAsByte(int x, int y, int width, int height)
        {
            Bitmap image = new Bitmap(width, height);
            Graphics imgGraphics = Graphics.FromImage(image);
            imgGraphics.CopyFromScreen(x, y, 0, 0, new Size(width, height));
            MemoryStream ms = new MemoryStream();
            image.Save(ms, ImageFormat.Bmp);
            image.Save("e:\\xxx.bmp", ImageFormat.Bmp);
            byte[] bytes = ms.GetBuffer();
            ms.Close();
            return bytes;
        }
    }
}
