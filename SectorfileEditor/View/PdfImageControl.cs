using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Spire.Pdf;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows;
using Spire.Pdf.Graphics;

namespace SectorfileEditor.View
{
    public class PdfImageControl: System.Windows.Controls.Image
    {

    
            [DllImport("gdi32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool DeleteObject(IntPtr value);

            public SizeF DocumentSize { get; protected set; }

            public void Open(string filename)
            {
                var document = new PdfDocument();
                document.LoadFromFile(filename);
                var bmp = document.SaveAsImage(0);
                Source = GetImageStream(bmp);

                PdfUnitConvertor unitCvtr = new PdfUnitConvertor(); 
                float widthMM = unitCvtr.ConvertUnits(document.Pages[0].Size.Width, PdfGraphicsUnit.Point, PdfGraphicsUnit.Millimeter);
                float heightMM = unitCvtr.ConvertUnits(document.Pages[0].Size.Height, PdfGraphicsUnit.Point, PdfGraphicsUnit.Millimeter);

                DocumentSize = new SizeF(widthMM, heightMM);
            }

            public static BitmapSource GetImageStream(System.Drawing.Image myImage)
            {
                var bitmap = new Bitmap(myImage);
                IntPtr bmpPt = bitmap.GetHbitmap();
                BitmapSource bitmapSource =
                 System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                       bmpPt,
                       IntPtr.Zero,
                       Int32Rect.Empty,
                       BitmapSizeOptions.FromEmptyOptions());

                //freeze bitmapSource and clear memory to avoid memory leaks
                bitmapSource.Freeze();
                DeleteObject(bmpPt);

                return bitmapSource;
            }
        }


    }
