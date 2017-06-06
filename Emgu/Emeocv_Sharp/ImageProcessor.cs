using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EmeocvSharp;
using Emgu.CV;
using System.Drawing;
using Emgu.CV.Util;
using Emgu.CV.Structure;
using System.IO;
using Emgu.CV.CvEnum;

//http://www.emgu.com/wiki/index.php/Shape_(Triangle,_Rectangle,_Circle,_Line)_Detection_in_CSharp
namespace Emeocv_Sharp
{
    public class ImageProcessor
    {
        private Config _config { get; set; }
        private Image<Bgr, byte> _img { get; set; }
        private Image<Gray, byte> _imgGray { get; set; }
        private List<Image<Gray, byte>> _digits { get; set; }
        private bool _debugWindow { get; set; }
        private bool _debugSkew { get; set; }
        private bool _debugEdges { get; set; }
        private bool _debugDigits { get; set; }

        public ImageProcessor()
        {
            this._config = new Config();
            debugWindow(false);
            debugSkew(false);
            debugDigits(false);
            debugEdges(false);
            _digits = new List<Image<Gray, byte>>();
        }
        public void debugDigits(bool bval = true)
        {
            this._debugDigits = bval;
        }

        public void debugEdges(bool bval = true)
        {
            this._debugEdges = bval;
        }

        public void debugSkew(bool bval = true)
        {
            this._debugSkew = bval;
        }

        public void debugWindow(bool bval = true)
        {
            this._debugWindow = bval;
        }

        //define the dictionary of digit segments so we can identify
        //each digit on the thermostat
        Dictionary<int[], int> DIGITS_LOOKUP = new Dictionary<int[], int> {
            { new [] {1, 1, 1, 0, 1, 1, 1 },  0 },
    {new [] {0, 0, 1, 0, 0, 1, 0}, 1},
    {new [] {1, 0, 1, 1, 1, 1, 0}, 2},
    {new [] {1, 0, 1, 1, 0, 1, 1}, 3},
    {new [] {0, 1, 1, 1, 0, 1, 0}, 4},
    {new [] {1, 1, 0, 1, 0, 1, 1}, 5},
    {new [] {1, 1, 0, 1, 1, 1, 1}, 6},
    {new [] {1, 0, 1, 0, 0, 1, 0}, 7},
    {new [] {1, 1, 1, 1, 1, 1, 1}, 8},
    {new [] {1, 1, 1, 1, 0, 1, 1}, 9
} };

        public void process()
        {
            //Localize the LCD on the thermostat

            //load the example image
            var image = CvInvoke.Imread(@"C:\Users\Admin\Desktop\example.jpg", Emgu.CV.CvEnum.LoadImageType.AnyColor);

            //pre-process the image by resizing it, converting it to
            //graycale, blurring it, and computing an edge map
            //Resizing it.
            CvInvoke.Resize(image, image, new Size(500 * image.Width / image.Height, 500));

            //Converting the image to grayscale.
            Mat gray = new Mat();
            CvInvoke.CvtColor(image, gray, ColorConversion.Bgr2Gray);

            //Applying Gaussian blurring with a 5×5 kernel to reduce high-frequency noise.
            Mat blurred = new Mat();
            CvInvoke.GaussianBlur(gray, blurred, new Size(5, 5), 0);

            //Computing the edge map via the Canny edge detector.
            Mat edged = new Mat();
            CvInvoke.Canny(blurred, edged, 50, 200); //, 255);

            //#2
            //find contours in the edge map, then sort them by their
            //size in descending order
            VectorOfVectorOfPoint cnts = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(edged.Clone(), cnts, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);

            //???
            VectorOfPoint cnts_ = cnts[0]; //cnts[0] if imutils.is_cv2() else cnts[1]

            var list = new Dictionary<VectorOfPoint, double>();

            for (int i = 0; i < cnts.Size; i++)
            {
                list.Add(cnts[i], CvInvoke.ContourArea(cnts[i]));
            }

            list = list.OrderByDescending(i => i.Value).ToDictionary(i => i.Key, j => j.Value);

            VectorOfPoint displayCnt = new VectorOfPoint();

            var t1 = false;

            //loop over the contours
            foreach (var c in list)
            {
                //approximate the contour
                var peri = CvInvoke.ArcLength(c.Key, true);
                VectorOfPoint approx = new VectorOfPoint();
                CvInvoke.ApproxPolyDP(c.Key, approx, 0.02 * peri, true);

                //if the contour has four vertices, then we have found
                //the thermostat display
                if (approx.Size == 4)
                {
                    displayCnt = approx;
                    break;

                }
            }

            //extract the thermostat display, apply a perspective transform
            //to it
            Mat header = new Mat();
            //var t = CvInvoke.Reshape(displayCnt, header, 4, 2);

            //Matrix<float> tmp = new Matrix<float>(myImg2.Height, myImg2.Width);
            //var reshaped_vect = tmp.Reshape(1, myImg2.Height * myImg2.Width);

            var warped = FourPointTransform(gray, displayCnt);
            var output = FourPointTransform(image, displayCnt);

            //#3
            //Mat thresh = new Mat();
            Image<Bgr, byte> thresh = new Image<Bgr, byte>(image.Size);

            thresh = warped.ThresholdBinaryInv(warped, 255);

            //CvInvoke.Threshold(warped, thresh, 0, 255, ThresholdType.BinaryInv | ThresholdType.ToZero);

            //Mat thresh = (Mat)thresh_.Data.GetValue(1);
            var kernel = CvInvoke.GetStructuringElement(ElementShape.Ellipse, new Size(1, 5), new Point(-1, -1));
            CvInvoke.MorphologyEx(thresh, thresh, MorphOp.Open, kernel, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());

            //#4

            //find contours in the thresholded image, then initialize the
            //digit contours lists
            VectorOfVectorOfPoint cnts2 = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(thresh.Clone(), cnts2, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);
            var cnts__ = cnts2[1];
            var digitCnts = new List<VectorOfPoint>();

            //loop over the digit area candidates
            for (int i = 0; i < cnts2.Size; i++)
            {
                var c = cnts2[i];

                //compute the bounding box of the contour
                var rectangle = CvInvoke.BoundingRectangle(c);

                //if the contour is sufficiently large, it must be a digit
                if (rectangle.Width >= 15 && (rectangle.Height >= 30 && rectangle.Height <= 40))
                {
                    digitCnts.Add(c);
                }
            }

            //#5
            //sort the contours from left-to-right, then initialize the
            //actual digits themselves

            //construct the list of bounding boxes and sort them from top to
            //bottom
            var boundingBoxes =
                            from c in digitCnts
                            select CvInvoke.BoundingRectangle(c);

            var res = from item in digitCnts
                      from item2 in boundingBoxes
                      select new Tuple<VectorOfPoint, Rectangle>(item, item2);

            digitCnts = res.OrderBy(i => i.Item2).Select(i => i.Item1).ToList();
            var digits = new List<int>();

           
            //6
            //loop over each of the digits
            foreach (var c in digitCnts)
            {
                //extract the digit ROI
                var rect = CvInvoke.BoundingRectangle(c);
                var roi = thresh[rect.Y + rect.Height, rect.X + rect.Width];
                //compute the width and height of each of the 7 segments
                //we are going to examine
               //var t = roi.MCvSca;
                //var dW = (int)(t.Width * 0.25);
                //var dH = (int)(t.Height * 0.15);
                //var dHC = (int)(t.Height * 0.05);

                //define the set of 7 segments
                var segments = new Dictionary<Point, Point> {
                    /*{ new Point(0, 0), new Point(rect.Width, dH) }, // top
                    { new Point(0, 0), new Point(dW, rect.Height / 2) },    // top-left
                    { new Point(rect.Width - dW, 0), new Point(rect.Width, rect.Height / 2) },	//top-right
                    { new Point(0, (rect.Height / 2) - dHC) , new Point(rect.Width, (rect.Height / 2) + dHC) }, //center
                    { new Point(0, rect.Height / 2), new Point(dW, rect.Height) },	// bottom-left
                    { new Point(rect.Width - dW, h / 2), new Point(rect.Width, rect.Height) },	// bottom-right
                    { new Point(0, rect.Height - dH), new Point(rect.Width, rect.Height) }	//bottom
                    */
                };
                
                var on = new List<float>(segments.Count);

                //loop over the segments
                for(var i=0; i< segments.Count; i++)
                {
                    var segment = segments.ElementAt(i);
                    var xA = segment.Key.X;
                    var yA = segment.Key.Y;
                    var xB = segment.Value.X;
                    var yB = segment.Value.Y;

                    //extract the segment ROI, count the total number of
                    //thresholded pixels in the segment, and then compute
                    //the area of the segment
                    //var segROI = roi[yB, xB];
                    var total = 1;//cv2.countNonZero(segROI)
                    var area = ((float)xB - xA) * (yB - yA);

                    //if the total number of non-zero pixels is greater than
                    //50% of the area, mark the segment as "on"
                    if(total / area > 0.5)
                    {
                        on[i] = 1;
                    }
                }

                //lookup the digit and draw it on the image
                var digit = 1; //DIGITS_LOOKUP[tuple(on)]
                digits.Add(digit);

                Mat outputRect = new Mat();
                CvInvoke.Rectangle(outputRect, rect, new MCvScalar(0, 255, 0), 1);
                CvInvoke.PutText(output, digit.ToString(), new Point(rect.X - 10, rect.Y - 10),
                    FontFace.HersheySimplex, 0.65, new MCvScalar(0, 255, 0), 2);
        }


            //edged.Save("result.png");
            return;








            //Rotate
            rotate(this._config.rotationDegrees);

            // detect and correct remaining skew (+- 30 deg)
            float skew_deg = detectSkew();
            rotate(skew_deg);


            //
            findCounterDigits();
        }

        public Mat FourPointTransform(Mat image, VectorOfPoint pts)
        {
            //obtain a consistent order of the points and unpack them
            //individually
            var rect = OrderPoints(pts);

            var tl = rect[0];
            var tr = rect[1];
            var br = rect[2];
            var bl = rect[3];

            //compute the width of the new image, which will be the
            //maximum distance between bottom-right and bottom-left
            //x-coordiates or the top-right and top-left x-coordinates
            var widthA = (int)Math.Sqrt(Math.Pow(br.X - bl.X, 2) + Math.Pow(br.Y - bl.Y, 2));
            var widthB = (int)Math.Sqrt(Math.Pow(tr.X - tl.X, 2) + Math.Pow(tr.Y - tl.Y, 2));

            var maxWidth = Math.Max(widthA, widthB);

            //compute the height of the new image, which will be the
            //maximum distance between the top-right and bottom-right
            //y-coordinates or the top-left and bottom-left y-coordinates
            var heightA = (int)Math.Sqrt(Math.Pow(tr.X - br.X, 2) + Math.Pow(tr.Y - br.Y, 2));
            var heightB = (int)Math.Sqrt(Math.Pow(tl.X - bl.X, 2) + Math.Pow(tl.Y - bl.Y, 2));

            var maxHeight = Math.Max(heightA, heightB);

            //now that we have the dimensions of the new image, construct
            //the set of destination points to obtain a "birds eye view",
            //(i.e. top-down view) of the image, again specifying points
            //in the top-left, top-right, bottom-right, and bottom-left
            //order
            var dst = new PointF[] {
                new PointF(0, 0),
                new PointF(maxWidth - 1, 0),
                new PointF(maxWidth - 1, maxHeight - 1),
                new PointF(0, maxHeight - 1)
            };


            //compute the perspective transform matrix and then apply it
            var M = CvInvoke.GetPerspectiveTransform(rect, dst);
            Mat warped = new Mat();
            CvInvoke.WarpPerspective(image, warped, M, new Size(maxWidth, maxHeight));

            //return the warped image
            return warped;
        }

        public PointF[] OrderPoints(VectorOfPoint pts)
        {
            //initialzie a list of coordinates that will be ordered
            //such that the first entry in the list is the top-left,
            //the second entry is the top-right, the third is the
            //bottom-right, and the fourth is the bottom-left
            PointF[] rect = new[] { new PointF(), new PointF(), new PointF(), new PointF()};

            //the top-left point will have the smallest sum, whereas
            //the bottom-right point will have the largest sum
            var s = Sum(pts);

            rect[0] = s.FirstOrDefault();
            rect[2] = s.LastOrDefault();

            //now, compute the difference between the points, the
            //top-right point will have the smallest difference,
            //whereas the bottom-left will have the largest difference

            var diff = Diff(pts);
            rect[1] = diff.FirstOrDefault();
            rect[3] = diff.LastOrDefault();

            //return the ordered coordinates
            return rect;
        }

        public PointF[] Sum(VectorOfPoint points)
        {
            var orderedPoints = new Dictionary<PointF, double>();

            for (int i = 0; i < points.Size; i++)
            {
                orderedPoints.Add(new PointF(points[i].X, points[i].Y), points[i].X + points[i].Y); 
            }

            return orderedPoints.OrderBy(i => i.Value).Select(i => i.Key).ToArray();

        }

        public PointF[] Diff(VectorOfPoint points)
        {
            var orderedPoints = new Dictionary<PointF, double>();
            var listOfPoints = new List<PointF>();

            for (int i = 0; i < points.Size; i++)
            {
                listOfPoints.Add(new Point(points[i].X, points[i].Y));
            }

            for (int i = 0; i < listOfPoints.Count; i++)
            {
                var diffInner = new { Point = listOfPoints[i], Diff = Math.Sqrt(Math.Pow(listOfPoints[i == listOfPoints.Count -1 ? 0 : i + 1].X - listOfPoints[i].X, 2) + 
                    Math.Pow(listOfPoints[i == listOfPoints.Count - 1 ? 0 : i + 1].Y - listOfPoints[i].Y, 2)) };

                orderedPoints.Add(diffInner.Point, diffInner.Diff);
            }

            return orderedPoints.OrderBy(i => i.Value).Select(i => i.Key).ToArray();
        }

        public void setInput(Image<Bgr, byte> img)
        {
            this._img = img;
            this._imgGray = img.Convert<Gray, byte>();
        }

        public void showImage()
        {
            Console.ReadLine();
        }

        public List<Image<Gray, byte>> getOutput()
        {
            return _digits;
        }

        private void rotate(double rotationDegrees)
        {
            UMat mapMatrix = new UMat();
            Image<Gray, byte> img_rotated = this._imgGray;
            Emgu.CV.CvInvoke.GetRotationMatrix2D(new PointF(this._imgGray.Cols / 2, this._imgGray.Rows / 2), rotationDegrees, 1, mapMatrix);

            Emgu.CV.CvInvoke.WarpAffine(this._imgGray, img_rotated, mapMatrix, this._img.Size);
            this._imgGray = img_rotated;
            //if (this._debugWindow)
            //{
            //    Emgu.CV.CvInvoke.WarpAffine(this._img, img_rotated, mapMatrix, this._img.Size);
            //    this._img = img_rotated;
            //}

            rotateDebug(rotationDegrees);
        }

        private void rotateDebug(double rotationDegrees)
        {
            UMat mapMatrix = new UMat();
            Image<Bgr, byte> img_rotated = this._img;
            Emgu.CV.CvInvoke.GetRotationMatrix2D(new PointF(this._img.Cols / 2, this._img.Rows / 2), rotationDegrees, 1, mapMatrix);

            Emgu.CV.CvInvoke.WarpAffine(this._img, img_rotated, mapMatrix, this._img.Size);
            this._img = img_rotated;
        }


        private void findCounterDigits()
        {
            var runningId = (new Random()).Next(100000);
            UMat edges = cannyEdges();
            if (this._debugEdges)
            {
                //imshow("edges", edges);
            }

            UMat img_ret = edges.Clone();

            List<Rectangle> boundingBoxes = new List<Rectangle>();
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            VectorOfVectorOfPoint filteredContours = new VectorOfVectorOfPoint();

            //Find contours
            CvInvoke.FindContours(edges, contours, null, Emgu.CV.CvEnum.RetrType.List, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxNone);

            filterContours(contours, boundingBoxes, filteredContours);

            //Draw contours
            var backedUp = this._img.Clone();
            List<Rectangle> bounds = new List<Rectangle>();
            for (var i = 0; i < contours.Size; i++)
            {
                bounds.Add(CvInvoke.BoundingRectangle(contours[i]));
            }

            //Filter contourn
            bounds = bounds.Where(c => c.Width > 10 && c.Height > 10).OrderBy(c => c.Top).ThenBy(c => c.Left).ToList();

            if (Directory.Exists("J:\\output"))
            {
                Directory.Delete("J:\\output", true);
                Directory.CreateDirectory("J:\\output");
            }
            else
            {
                Directory.CreateDirectory("J:\\output");
            }

            //var contourDraw = this._img.Clone();
            //for(var index= 0; index < contours.Size;index++)
            //contourDraw.Draw(contours, index, new Bgr(Color.Red));
            //contourDraw.Save("J:\\output\\contourDraw.jpg");


            var count = 0;
            foreach (var bound in bounds)
            {
                count++;
                this._img.Draw(bound, new Bgr(Color.Green), 1);
                var contourImage = backedUp.Clone();
                contourImage.ROI = bound;
                var contourImageName = string.Format("J:\\output\\{0}_{1}.jpg", runningId, count);
                contourImage.Save(contourImageName);
                Console.WriteLine(contourImageName);

            }



            this._img.Save("J:\\output\\contouredImage.jpg");
            Console.ReadLine();
            return;
            //// find bounding boxes that are aligned at y position
            List<Rectangle> alignedBoundingBoxes = new List<Rectangle>(), tmpRes = new List<Rectangle>();
            for (var index = 0; index < boundingBoxes.Count; index++)
            {
                tmpRes.Clear();
                findAlignedBoxes(boundingBoxes, index, boundingBoxes.Count, tmpRes);
                if (tmpRes.Count > alignedBoundingBoxes.Count)
                {
                    alignedBoundingBoxes = tmpRes;
                }
            }

            //// sort bounding boxes from left to right
            alignedBoundingBoxes = alignedBoundingBoxes.OrderBy(c => c.X).ToList();

            if (_debugEdges)
            {
                // draw contours
                //Mat cont = M//zeros(edges.rows, edges.cols, CV_8UC1);
                //drawContours(cont, filteredContours, -1, Scalar(255));
                //imshow("contours", cont);
            }

            //// cut out found rectangles from edged image

            for (int i = 0; i < alignedBoundingBoxes.Count; ++i)
            {
                Rectangle roi = alignedBoundingBoxes[i];

                var img = this._imgGray.Clone();
                img.ROI = roi;
                img.Save(string.Format("J:\\output\\{0}_{1}.jpg", i, runningId));
                _digits.Add(img);
                //if (_debugDigits)
                //{
                //    rectangle(_img, roi, Scalar(0, 255, 0), 2);
                //}
            }
        }

        private void findAlignedBoxes(List<Rectangle> list, int start, int end, List<Rectangle> temp)
        {
            var startRectangle = list[start];
            temp.Add(startRectangle);
            start = start + 1;
            for (var index = start; index < end; index++)
            {
                if (Math.Abs(startRectangle.Y - list[index].Y) < _config.digitYAlignment && Math.Abs(startRectangle.Height - list[index].Height) < 5)
                {
                    temp.Add(list[index]);
                }
            }
        }

        private float detectSkew()
        {
            return 0;

        }

        private List<PointF> drawLines(List<PointF> lines)
        {
            throw new NotImplementedException();
        }

        private void drawLines(List<List<PointF>> lines, int xoff = 0, int yoff = 0)
        {
            throw new NotImplementedException();
        }

        private UMat cannyEdges()
        {
            UMat edges = new UMat();
            Emgu.CV.CvInvoke.Canny(this._imgGray, edges, this._config.cannyThreshold1, this._config.cannyThreshold2);
            return edges;
        }

        private void filterContours(VectorOfVectorOfPoint contours, List<Rectangle> boundingBoxes, VectorOfVectorOfPoint filteredContours)
        {
            // filter contours by bounding rect size
            var count = contours.Size;
            for (int i = 0; i < count; i++)
            {
                Rectangle bounds = CvInvoke.BoundingRectangle(contours[i]);

                //if (bounds.Height > _config.digitMinHeight && bounds.Height < _config.digitMaxHeight
                //        && bounds.Width > 5 && bounds.Width < bounds.Height)
                //{
                if (true)
                {
                    boundingBoxes.Add(bounds);
                    filteredContours.Push(contours[i]);
                }
            }
        }

        public void Dispose()
        {
        }
    }
}
