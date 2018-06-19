
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Drawing.Drawing2D;

namespace 绘图程序
{
    public partial class Draw : Form
    {
        public Draw()
        {
            InitializeComponent();
        }
        private DrawTools dt;
        private string sType;//绘图样式
        private string sFileName;//打开的文件名
        private bool bReSize = false;//是否改变画布大小
        private Size DefaultPicSize;//储存原始画布大小，用来新建文件时使用
        List<Point> pList = new List<Point>();
        Bitmap cutpic;
        Image mainpic;
        //pbimg＂鼠标按下＂事件处理方法
        private void pbImg_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (dt != null)
                {
                    pList.Add(e.Location);
                    dt.startDraw = true;//相当于所选工具被激活，可以开始绘图
                    dt.startPointF = new PointF(e.X, e.Y);
                }
            }
        }

        //pbimg＂鼠标移动＂事件处理方法
        private void pbImg_MouseMove(object sender, MouseEventArgs e)
        {
            //Thread.Sleep(6);//减少cpu占用率
            //mousePostion.Text = e.Location.ToString();
            if (dt.startDraw)
            {
                pList.Add(e.Location);
                dt.DrawDot(e);
                label2.Text = "Mouse-Position: X: " + e.X + "\r\n" + "Mouse-Position: Y: " + e.Y;
            }
            if (pList.Count > 0)
            {
                button3.Enabled = true;
            }
        }


        //pbimg＂鼠标松开＂事件处理方法
        private void pbImg_MouseUp(object sender, MouseEventArgs e)
        {
            if (dt != null)
            {
                dt.EndDraw();
            }
        }




        //＂窗体加载＂事件处理方法
        private void Form1_Load(object sender, EventArgs e)
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            this.UpdateStyles();
            Bitmap bmp = new Bitmap(pbImg.Width, pbImg.Height);
            Graphics g = Graphics.FromImage(bmp);
            g.FillRectangle(new SolidBrush(pbImg.BackColor), new Rectangle(0, 0, pbImg.Width, pbImg.Height));
            g.Dispose();
            dt = new DrawTools(this.pbImg.CreateGraphics(), Color.Red, bmp);//实例化工具类
            DefaultPicSize = pbImg.Size;

        }

        //＂打开文件＂事件处理方法
        private void openPic_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();//实例化文件打开对话框
            ofd.Filter = "JPG|*.jpg|Bmp|*.bmp|所有文件|*.*";//设置对话框打开文件的括展名
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                Bitmap bmpformfile = new Bitmap(ofd.FileName);//获取打开的文件
                pbImg.Size = bmpformfile.Size;//调整绘图区大小为图片大小
                // reSize.Location = new Point(bmpformfile.Width, bmpformfile.Height);//reSize为我用来实现手动调节画布大小用的
                //因为我们初始时的空白画布大小有限，"打开"操作可能引起画板大小改变，所以要将画板重新传入工具类
                dt.DrawTools_Graphics = pbImg.CreateGraphics();

                Bitmap bmp = new Bitmap(pbImg.Width, pbImg.Height);
                Graphics g = Graphics.FromImage(bmp);
                g.FillRectangle(new SolidBrush(pbImg.BackColor), new Rectangle(0, 0, pbImg.Width, pbImg.Height));//不使用这句话，那么这个bmp的背景就是透明的
                g.DrawImage(bmpformfile, 0, 0, bmpformfile.Width, bmpformfile.Height);//将图片画到画板上
                g.Dispose();//释放画板所占资源
                //不直接使用pbImg.Image = Image.FormFile(ofd.FileName)是因为这样会让图片一直处于打开状态，也就无法保存修改后的图片；
                bmpformfile.Dispose();//释放图片所占资源
                g = pbImg.CreateGraphics();
                g.DrawImage(bmp, 0, 0);
                g.Dispose();
                dt.OrginalImg = bmp;
                bmp.Dispose();
                sFileName = ofd.FileName;//储存打开的图片文件的详细路径，用来稍后能覆盖这个文件
                ofd.Dispose();

            }
        }

        //＂保存＂事件处理方法
        private void savePic_Click(object sender, EventArgs e)
        {
            if (sFileName != null)
            {

                if (MessageBox.Show("是否要保存文件？", "系统提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    dt.OrginalImg.Save(sFileName);
                }
            }
            else
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "JPG(*.jpg)|*.jpg|BMP(*.bmp)|*.bmp";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    dt.OrginalImg.Save(sfd.FileName);
                    sFileName = sfd.FileName;
                }
            }

        }

        private void pbImg_Click(object sender, EventArgs e)
        {

        }
        public void ClearPicturebox()
        {
            pList.Clear();
            Bitmap newpic = new Bitmap(pbImg.Width, pbImg.Height);
            Graphics g = Graphics.FromImage(newpic);
            g.FillRectangle(new SolidBrush(Color.Black), 0, 0, pbImg.Width, pbImg.Height);
            g.Dispose();
            g = pbImg.CreateGraphics();
            g.DrawImage(newpic, 0, 0);
            g.Dispose();
            dt.OrginalImg = newpic;
            dt.newgraphics = null;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            ClearPicturebox();
            dt.DrawTools_Graphics = pbImg.CreateGraphics();
            Bitmap bmp = new Bitmap(pbImg.Width, pbImg.Height);
            Graphics g = Graphics.FromImage(bmp);
            pbImg.Refresh();
            //g.Dispose();//释放画板所占资源
            Image BackImage = mainpic;
            pbImg.SizeMode = PictureBoxSizeMode.CenterImage;
            pbImg.Image = BackImage;
            //不直接使用pbImg.Image = Image.FormFile(ofd.FileName)是因为这样会让图片一直处于打开状态，也就无法保存修改后的图片
            bmpformfile.Dispose();//释放图片所占资源
            g = pbImg.CreateGraphics();
            g.DrawImage(bmp, 0, 0);
            // g.Dispose();
            dt.OrginalImg = bmp;
            bmp.Dispose();
            button3.Text = "Cut";
        }

        Bitmap bmpformfile;
        private void button2_Click(object sender, EventArgs e)
        {
            LoadPicture();
        }

        public void LoadPicture()
        {
            pcxelnumber = 0;
            pList.Clear();
            OpenFileDialog ofd = new OpenFileDialog();//实例化文件打开对话框
            ofd.Filter = "JPG|*.jpg|Bmp|*.bmp|所有文件|*.*";//设置对话框打开文件的括展名
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                bmpformfile = new Bitmap(ofd.FileName);//获取打开的文件
                                                       // panel2.AutoScrollPosition = new Point(0,0);//将滚动条复位
                pbImg.Size = bmpformfile.Size;//调整绘图区大小为图片大小
                mainpic = new Bitmap(ofd.FileName);
                // reSize.Location = new Point(bmpformfile.Width, bmpformfile.Height);//reSize为我用来实现手动调节画布大小用的
                //因为我们初始时的空白画布大小有限，"打开"操作可能引起画板大小改变，所以要将画板重新传入工具类
                dt.DrawTools_Graphics = pbImg.CreateGraphics();

                Bitmap bmp = new Bitmap(pbImg.Width, pbImg.Height);
                Graphics g = Graphics.FromImage(bmp);
                //g.FillRectangle(new SolidBrush(pbImg.BackColor), new Rectangle(0, 0, pbImg.Width, pbImg.Height));//不使用这句话，那么这个bmp的背景就是透明的
                //g.DrawImage(bmpformfile, pbImg.Width / 2 - bmpformfile.Width / 2, pbImg.Height / 2 - bmpformfile.Height / 2, bmpformfile.Width, bmpformfile.Height);//将图片画到画板上
                pbImg.Refresh();
                //g.Dispose();//释放画板所占资源
                Image BackImage = Image.FromFile(ofd.FileName);
                pbImg.SizeMode = PictureBoxSizeMode.CenterImage;
                pbImg.Image = BackImage;
                //不直接使用pbImg.Image = Image.FormFile(ofd.FileName)是因为这样会让图片一直处于打开状态，也就无法保存修改后的图片；详见http://www.wanxin.org/redirect.php?tid=3&goto=lastpost
                bmpformfile.Dispose();//释放图片所占资源
                g = pbImg.CreateGraphics();
                g.DrawImage(bmp, 0, 0);
                // g.Dispose();
                dt.OrginalImg = bmp;
                bmp.Dispose();
                sFileName = ofd.FileName;//储存打开的图片文件的详细路径，用来稍后能覆盖这个文件
                label1.Text = "Original-Drawing Size: " + BackImage.Width + "," + BackImage.Height;
            }
        }
        private void confirm(GraphicsPath path)
        {
            RectangleF rect = path.GetBounds();
            int left = (int)rect.Left;
            int top = (int)rect.Top;
            int width = (int)rect.Width;
            int height = (int)rect.Height;

            Graphics gra = pbImg.CreateGraphics();
            gra.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Brush bush = new SolidBrush(Color.Green);//填充的颜色
            Pen pen = new Pen(Color.Green, 2);
            Rectangle r = new Rectangle(left, top, width, height);
            pen.DashStyle = DashStyle.Dot;
            gra.DrawRectangle(pen, r);
            gra.FillEllipse(bush, left - 5, top - 5, 10, 10);//画填充椭圆的方法，x坐标、y坐标、宽、高，如果是100，则半径为50
            gra.FillEllipse(bush, left + width - 5, top - 5, 10, 10);//画填充椭圆的方法，x坐标、y坐标、宽、高，如果是100，则半径为50
            gra.FillEllipse(bush, left - 5, top + height - 5, 10, 10);//画填充椭圆的方法，x坐标、y坐标、宽、高，如果是100，则半径为50
            gra.FillEllipse(bush, left + width - 5, top + height - 5, 10, 10);//画填充椭圆的方法，x坐标、y坐标、宽、高，如果是100，则半径为50

        }
        private void DrawOutLine(GraphicsPath Path)
        {
            
            Graphics gra = pbImg.CreateGraphics();
            gra.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Brush brush = new SolidBrush(Color.Green);//填充的颜色
            Pen pen = new Pen(Color.Green, 2);
            pen.DashStyle = DashStyle.Dash;
            gra.DrawPath(pen, Path);
          /*  for (int i = 0; i < pList.Count; i++)
            {
                gra.FillEllipse(brush, pList[i].X - 1, pList[i].Y - 1, 2, 2);
            }*/
        }
        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                pcxelnumber = 0;
                Bitmap newBit = null;
                GraphicsPath path = new GraphicsPath();
                Point[] p = new Point[pList.Count];
                for (int i = 0; i < pList.Count; i++)
                {
                    p[i] = pList[i];
                }
                path.AddLines(p);
                pbImg.Refresh();

                if (button3.Text == "Cut")
                {
                    confirm(path);

                    button3.Text = "Set";
                    return;
                }
                if (button3.Text == "Set")
                {
                    DrawOutLine(path);
                    button3.Text = "Confirm";
                    return;
                }
                else if (button3.Text == "Confirm")
                {
                    MessageBox.Show("请稍等");
                    Thread.Sleep(500);
                    Bitmap bmp = new Bitmap(pbImg.Width, pbImg.Height);
                    pbImg.DrawToBitmap(bmp, pbImg.ClientRectangle);
                    BitmapCrop(bmp, path, out newBit);
                    RePaint(newBit);
                    label3.Text = "Cut-Drawing Size: " + newBit.Width + "," + newBit.Height;
                    label4.Text = "Pixel: " + pcxelnumber;
                    MessageBox.Show("选择完毕");
                    // cutpic.Save("G:\\2.png", ImageFormat.Png);
                    button3.Text = "Cut";
                    return;
                }
            }
            catch (Exception a)
            { }
        }

        /// <summary>
        /// 图片截图
        /// </summary>
        /// <param name="bitmap">原图</param>
        /// <param name="path">裁剪路径</param>
        /// <param name="outputBitmap">输出图</param>
        /// <returns></returns>
        int pcxelnumber;
        public Bitmap BitmapCrop(Bitmap bitmap, GraphicsPath path, out Bitmap outputBitmap)
        {
            RectangleF rect = path.GetBounds();
            int left = (int)rect.Left;
            int top = (int)rect.Top;
            int width = (int)rect.Width;
            int height = (int)rect.Height;

            Bitmap image = bitmap;
            
            outputBitmap = new Bitmap(width, height);
            for (int i = left; i < left + width; i++)
            {
                for (int j = top; j < top + height; j++)
                {
                    //判断坐标是否在路径中  
                    if (path.IsVisible(i, j))
                    {
                        //复制原图区域的像素到输出图片 
                        Color tmp = image.GetPixel(i, j);
                        outputBitmap.SetPixel(i - left, j - top, tmp);
                        //设置原图这部分区域为透明  
                        image.SetPixel(i, j, Color.FromArgb(0, image.GetPixel(i, j)));
                        pcxelnumber++; 
                    }
                    else
                    {
                        outputBitmap.SetPixel(i - left, j - top, Color.FromArgb(0, 0, 0, 0));
                    }
                }
            }
            bitmap.Dispose();
            return image;
        }

        public void RePaint(Bitmap newbitmap)
        {
            Bitmap bmp = newbitmap;
            Bitmap backpic = new Bitmap(pbImg.Width, pbImg.Height);
            ClearPicturebox();
            Graphics g = Graphics.FromImage(backpic);
            pbImg.Image = bmp;
            cutpic = bmp;
            pbImg.Refresh();

        }

        private void button4_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "PNG(*.png)|*.png|BMP(*.bmp)|*.bmp";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                cutpic.Save(sfd.FileName, ImageFormat.Png);
                //pbImg.Image.Save(sfd.FileName, ImageFormat.Png);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}