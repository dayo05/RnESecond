using System.Runtime.InteropServices;
using RnE;

NativeLibrary.Load("/home/dayo/ffmpeg/lib/libavutil.so.57");
NativeLibrary.Load("/home/dayo/ffmpeg/lib/libavcodec.so.59");
NativeLibrary.Load("/home/dayo/ffmpeg/lib/libavformat.so.59");
NativeLibrary.Load("/home/dayo/lib/libRnENative.so");

CV.Initialize();
YOLO.Initialize();

Console.WriteLine("Start!!");
var frame = 0;
while (true)  
{
   Console.Write("\rRunning Frame: " + frame);
    var l = YOLO.Yolo(CV.ReadNextFrame().ToList());

    if (l.Count != 0)
    {
        foreach ((int x, int y, int w, int h) x in l.Where(x => x.w <= x.h)
                     .Select(x => (x.x - x.w / 2, x.y - x.h / 2, x.w, x.w)))
        {
            (int x, int y, int w, int h) k = (x.x - x.w / 10, x.y - x.w / 10, x.w * 6 / 5, x.h * 6 / 5);
            CV.SaveImage(k.x, k.y, k.w, k.h);
        }
    }

    frame++;
}