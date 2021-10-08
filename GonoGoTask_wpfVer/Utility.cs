using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using swf = System.Windows.Forms;
using sd = System.Drawing;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;

namespace GonoGoTask_wpfVer
{
    class Utility
    {
        static public int ratioIn2Pixal = 96;
        static public float ratioCM2Pixal = (float)96 / (float)2.54;

        public Utility()
        { }


        public static swf.Screen Detect_oneNonPrimaryScreen()
        {/* Detect the first not Primary Screen */

            swf.Screen[] screens = swf.Screen.AllScreens;
            swf.Screen nonPrimaryS = swf.Screen.PrimaryScreen; 
            foreach (swf.Screen s in screens)
            {
                if(s.Primary == false)
                {
                    nonPrimaryS = s;
                    break;
                }
            }
            return nonPrimaryS;
        }


        public static sd.Rectangle Detect_PrimaryScreen_Rect()
        {
            swf.Screen PrimaryS = swf.Screen.PrimaryScreen;
            sd.Rectangle screenRect = PrimaryS.Bounds;

            return screenRect;
        }

        public static Ellipse Create_Circle(double Diameter, SolidColorBrush brush_Fill)
        {/*
            Create the circle

            Args:
                Diameter: the Diameter of the Circle in Pixal

            */

            // Create an Ellipse  
            Ellipse circle = new Ellipse();

            // set the size, position of circleGo
            circle.Height = Diameter;
            circle.Width = Diameter;

            circle.Fill = brush_Fill;

            return circle;
        }

        public static Ellipse Move_Circle_OTopLeft(Ellipse circle, int[] cPoint_Pos_OTopLeft)
        {/*
            Move the circle into cPoint_Pos_OTopLeft (Origin in the topLeft of the Screen)

            Args:
                circle: to Be Moved Circle

                cPoint_Pos_OTopLeft: the x, y Positions of the Circle center in Pixal (Origin in the topLeft of the Screen)

            */


            circle.VerticalAlignment = VerticalAlignment.Top;
            circle.HorizontalAlignment = HorizontalAlignment.Left;

            circle.Margin = new Thickness(cPoint_Pos_OTopLeft[0] - circle.Width / 2, cPoint_Pos_OTopLeft[1] - circle.Height / 2, 0, 0);

            return circle;
        }


        public static Rectangle Move_Rect_OTopLeft(Rectangle rect, int[] cPoint_Pos_OTopLeft)
        {/*
            Move the rect into cPoint_Pos_OTopLeft (Origin in the topLeft of the Screen)

            Args:
                rect: to Be Moved Rectangle

                cPoint_Pos_OTopLeft: the x, y Positions of the Rectangle center in Pixal (Origin in the topLeft of the Screen)

            */


            rect.VerticalAlignment = VerticalAlignment.Top;
            rect.HorizontalAlignment = HorizontalAlignment.Left;

            rect.Margin = new Thickness(cPoint_Pos_OTopLeft[0] - rect.Width / 2, cPoint_Pos_OTopLeft[1] - rect.Height / 2, 0, 0);

            return rect;
        }

        public static List<int[]> GenDefaultPositions_GoNogoTask(int n, int radius)
        {/*
                Generate the default optional X, Y Positions (origin in center) for workArea
                1. The  Points equally in a circle for the first 8 (origin = [0, 0], radius)
                2. The nineth is at (0, 0) for n = 9

                Unit is pixal

                Args:
                    n: the number of generated positions (n <=9)
                    radius: the radius of the circle (Pixal)

            */

            List<int[]> postions9_OCenter_List = new List<int[]>();
            
            // Points 1,2 at 0 and pi Degrees
            for (int i = 0; i < 2; i++)
            {
                double deg = Math.PI * i;
                int x = (int)(radius * Math.Cos(deg)), y = -(int)(radius * Math.Sin(deg));
                postions9_OCenter_List.Add(new int[] { x, y });
            }
            // Points 3, 4  at pi/2 and 3pi/2 Degrees
            for (int i = 0; i < 2; i++)
            {
                double deg = Math.PI * i + Math.PI / 2;
                int x = (int)(radius * Math.Cos(deg)), y = -(int)(radius * Math.Sin(deg));
                postions9_OCenter_List.Add(new int[] { x, y });
            }
            // Points 5-8 at pi/4, 3pi/4, 5pi/4 and 7pi/4 Degrees
            for (int i = 0; i < 4; i++)
            {
                double deg = 2 * Math.PI / 4 * i + Math.PI / 4;
                int x = (int)(radius * Math.Cos(deg)), y = -(int)(radius * Math.Sin(deg));
                postions9_OCenter_List.Add(new int[] { x, y });
            }
            // Point 9 at (0, 0)
            postions9_OCenter_List.Add(new int[] { 0, 0 });


            List<int[]> defaultPostions_OCenter_List = new List<int[]>();
            for(int i = 0; i < n; i++)
            {
                defaultPostions_OCenter_List.Add(postions9_OCenter_List[i]);
            }


            return defaultPostions_OCenter_List;

        }

        public static int CM2Pixal(float cmlen)
        {/* convert length with unit cm to unit pixal, 96 pixals = 1 inch = 2.54 cm

            args:   
                cmlen: to be converted length (unit: cm)

            return:
                pixalen: converted length with unit pixal
         */

            float ratio = (float)96 / (float)2.54;

            int pixalen = (int)(cmlen * ratio);

            return pixalen;
        }

        public static int Inch2Pixal(float inlen)
        {/* convert length with unit inch to unit pixal, 96 pixals = 1 inch = 2.54 cm

            args:   
                cmlen: to be converted length (unit: inch)

            return:
                pixalen: converted length with unit pixal
         */     

            int pixalen = (int)(inlen * ratioIn2Pixal);

            return pixalen;
        }


        public static List<T> Shuffle<T>(List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                Random rng = new Random();
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
            return list;
        }
    }
}
