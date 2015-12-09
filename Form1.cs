/*
The MIT License (MIT)

Copyright (c) 2015 Renjith M

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.IO;

namespace wscap {
    public partial class Form1 : Form {

        public Form1() {            
            InitializeComponent();
            Form1.RegisterHotKey(this.Handle, this.GetType().GetHashCode(), 0x0000, 0x2C);
        }


        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);
        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private Boolean mouseIsDown = false;
        private Point startingCoOrdinate = new Point();
        private Point endingCoOrdinate = new Point();

        protected override void WndProc(ref Message m) {
            if (m.Msg == 0x0312) {
                try {
                    Bitmap bmpScreenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, PixelFormat.Format32bppArgb);
                    using (Graphics gfxScreenshot = Graphics.FromImage(bmpScreenshot)) {
                        gfxScreenshot.CopyFromScreen(0, 0, 0, 0, bmpScreenshot.Size, CopyPixelOperation.SourceCopy);
                    }
                    this.BackgroundImage = bmpScreenshot;
                } catch {
                }
                this.WindowState = FormWindowState.Maximized;
            }
            base.WndProc(ref m);
        }

        private void Form1_Load(object sender, EventArgs e) {
            this.Location = Screen.PrimaryScreen.Bounds.Location;
        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e) {
            if (e.KeyChar == (Char)Keys.Escape) {
                this.WindowState = FormWindowState.Minimized;
            }
            e.Handled = true;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            Form1.UnregisterHotKey(this.Handle, this.GetType().GetHashCode());
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
            this.Close();
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e) {
            mouseIsDown = true;
            startingCoOrdinate.X = e.X;
            startingCoOrdinate.Y = e.Y;
            endingCoOrdinate.X = -1;
            endingCoOrdinate.Y = -1;
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e) {
            mouseIsDown = false;
            Rectangle finalSelectionRectangle = normalizeRect(startingCoOrdinate, endingCoOrdinate);
            if (endingCoOrdinate.X != -1) {
                Point trackingMouseCurrentPoint = new Point(e.X, e.Y);
                drawSectionSelectionRectangle(startingCoOrdinate, endingCoOrdinate);
            }
            endingCoOrdinate.X = -1;
            endingCoOrdinate.Y = -1;
            startingCoOrdinate.X = -1;
            startingCoOrdinate.Y = -1;
            if (this.BackgroundImage!=null && finalSelectionRectangle != null && finalSelectionRectangle.Width > 0 && finalSelectionRectangle.Height > 0) {
                if (Control.ModifierKeys == Keys.Shift) {
                    using (Graphics gBgImage = Graphics.FromImage(this.BackgroundImage)) {
                        using (Pen redPen = new Pen(Color.Red)) {
                            gBgImage.DrawRectangle(redPen, finalSelectionRectangle);
                        }                        
                    }
                    this.Refresh();
                } else {
                    Bitmap backGroundImageScreenshot = new Bitmap(this.BackgroundImage);
                    Bitmap subImage = backGroundImageScreenshot.Clone(finalSelectionRectangle, backGroundImageScreenshot.PixelFormat);
                    Clipboard.SetImage(subImage);
                    this.WindowState = FormWindowState.Minimized;
                }                
            }
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e) {
            if (mouseIsDown) {
                if (endingCoOrdinate.X != -1) {
                    drawSectionSelectionRectangle(startingCoOrdinate, endingCoOrdinate);
                }
                Point trackingMouseCurrentPoint = new Point(e.X, e.Y);
                endingCoOrdinate = trackingMouseCurrentPoint;
                drawSectionSelectionRectangle(startingCoOrdinate, trackingMouseCurrentPoint);
            }
        }

        private Rectangle normalizeRect(Point staringPoint, Point endingPoint) {
            staringPoint = PointToScreen(staringPoint);
            endingPoint = PointToScreen(endingPoint);
            Rectangle recSelection = new Rectangle();
            if (staringPoint.X < endingPoint.X) {
                recSelection.X = staringPoint.X;
                recSelection.Width = endingPoint.X - staringPoint.X;
            } else {
                recSelection.X = endingPoint.X;
                recSelection.Width = staringPoint.X - endingPoint.X;
            }
            if (staringPoint.Y < endingPoint.Y) {
                recSelection.Y = staringPoint.Y;
                recSelection.Height = endingPoint.Y - staringPoint.Y;
            } else {
                recSelection.Y = endingPoint.Y;
                recSelection.Height = staringPoint.Y - endingPoint.Y;
            }
            return recSelection;
        }

        private void drawSectionSelectionRectangle(Point staringPoint, Point endingPoint) {
            ControlPaint.DrawReversibleFrame(normalizeRect(staringPoint, endingPoint), 
                Control.ModifierKeys == Keys.Shift ?Color.Cyan:Color.Black, FrameStyle.Thick);
        }   


    }
}
