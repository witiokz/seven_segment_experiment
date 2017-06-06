using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emeocv_Sharp
{
    class Program
    {
        static void Main(string[] args)
        {
            ImageProcessor processor = new ImageProcessor();
            {
                processor.setInput(new Image<Bgr, byte>(@"C:\Users\Admin\Desktop\example.jpg"));
                processor.process();
            }
        }
    }
}
