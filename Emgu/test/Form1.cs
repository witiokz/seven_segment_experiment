using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Shape;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;

namespace test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        Image<Bgr, Byte> img;


        //Convert the image to gray and remove noise
        UMat uimage = new UMat();
        private void fnPreProcessing()
        {
            CvInvoke.CvtColor(img, uimage, ColorConversion.Bgr2Gray);
            UMat pyrDown = new UMat();
            CvInvoke.PyrDown(uimage, pyrDown);
            CvInvoke.PyrUp(pyrDown, uimage);
        }

        #region Circle detection
        CircleF[] circles;
        double dCannyThres = 180.0;
        Image<Bgr, Byte> CircleImage;
        private void fnCircleDetection()
        {
            CircleImage = img.CopyBlank();
            double dCircleAccumulatorThres = 120.0;
            circles = CvInvoke.HoughCircles(uimage, HoughType.Gradient, 2.0, 3.0, dCannyThres, dCircleAccumulatorThres);
            foreach (CircleF circle in circles)
                CircleImage.Draw(circle, new Bgr(Color.Brown), 2);
            ImgBox_Circle.Image = CircleImage.Bitmap;
        }
        #endregion

        #region Canny and edge detection
        UMat cannyEdges = new UMat();
        LineSegment2D[] lines;
        Image<Bgr, Byte> lineImage;
        private void fnEdgeDetection()
        {
            double dCannyThreLinking = 120.0;
            CvInvoke.Canny(uimage, cannyEdges, dCannyThres, dCannyThreLinking);
            lines = CvInvoke.HoughLinesP(cannyEdges,
            1, //Distance resolution in pixel-related units
            Math.PI / 45.0, //Angle resolution measured in radians.
            20, //threshold
            30, //min Line width
            10); //gap between lines
            lineImage = img.CopyBlank();
            foreach (LineSegment2D line in lines)
                lineImage.Draw(line, new Bgr(Color.Green), 2);
            ImgBox_Line.Image = lineImage.Bitmap;
        }
        #endregion

        List<Triangle2DF> triangleList = new List<Triangle2DF>();
        List<RotatedRect> boxList = new List<RotatedRect>();
        Image<Bgr, Byte> triangleRectImage;
        private void fnFindTriangleRect()
        {
            triangleRectImage = img.CopyBlank();
            using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
            {
                CvInvoke.FindContours(cannyEdges, contours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);
                int count = contours.Size;
                for (int i = 0; i < count; i++)
                {
                    using (VectorOfPoint contour = contours[i])
                    using (VectorOfPoint approxContour = new VectorOfPoint())
                    {
                        CvInvoke.ApproxPolyDP(contour, approxContour, CvInvoke.ArcLength(contour, true) * 0.05, true);
                        if (CvInvoke.ContourArea(approxContour, false) > 250) //only consider contour with area > 250
                        {
                            if (approxContour.Size == 3) //The contour has 3 vertices, is a triangle
                            {
                                Point[] pts = approxContour.ToArray();
                                triangleList.Add(new Triangle2DF(pts[0], pts[1], pts[2]));
                            }
                            else if (approxContour.Size == 4) // The contour has 4 vertices
                            {
                                #region Determine if all the angles in the contours are within [80,100] degree
                                bool isRectangle = true;
                                Point[] pts = approxContour.ToArray();
                                LineSegment2D[] edges = PointCollection.PolyLine(pts, true);
                                for (int j = 0; j < edges.Length; j++)
                                {
                                    double dAngle = Math.Abs(edges[(j + 1) % edges.Length].GetExteriorAngleDegree(edges[j]));
                                    if (dAngle < 80 || dAngle > 100)
                                    {
                                        isRectangle = false;
                                        break;
                                    }
                                }
                                #endregion
                                if (isRectangle) boxList.Add(CvInvoke.MinAreaRect(approxContour));
                            }
                        }
                    }
                }

            }
            foreach (Triangle2DF triangle in triangleList)
            {
                triangleRectImage.Draw(triangle, new Bgr(Color.DarkBlue), 2);
            }
            foreach (RotatedRect box in boxList)
                triangleRectImage.Draw(box, new Bgr(Color.Red), 2);
            ImgBox_Triangle_Rect.Image = triangleRectImage.Bitmap;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void OpenFile_Click(object sender, EventArgs e)
        {
            if (OpenFileDlg.ShowDialog() == DialogResult.OK)
            {
                TextBox_FilePath.Text = OpenFileDlg.FileName;
                img = new Image<Bgr, Byte>(TextBox_FilePath.Text);
                ImgBox_Original.Image = img.Bitmap;
                fnPreProcessing();
                fnFindTriangleRect();
                fnCircleDetection();
                fnEdgeDetection();
            }
        }
    }
}