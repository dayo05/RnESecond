using System.Runtime.InteropServices;
using static System.Linq.Enumerable;

namespace RnE;

public static class CV
{
    private static IntPtr vc1, vc2;
    private static IntPtr mat1, mat2;
    private static IntPtr crop1, crop2;

    private static long ix = 0;

    private static List<string> v1List = new List<string>
    {
        "/home/dayo/datasets/20220629163035-20220629165059/Camera 01_cctv_20220629163034_20220629165059_1325032.mp4"
    };

    private static int v1Idx = 0;

    private static List<string> v2List = new List<string>
    {
        "/home/dayo/datasets/20220629163035-20220629165059/Camera 02_cctv_20220629163034_20220629165059_1325034.mp4"
    };

    private static int v2Idx = 0;
    private static double v1fps = 25, v2fps = 25.12;
    private static long f1 = 0, f2 = 0;
    
    public static void Initialize()
    {
        vc1 = open_video(v1List[0]);
        vc2 = open_video(v2List[0]);
        
        Console.WriteLine(get_total_time(vc1));
        Console.WriteLine(get_total_time(vc2));
        mat1 = create_mat();
        mat2 = create_mat();
        crop1 = create_mat();
        crop2 = create_mat();
        
        foreach (var x in Range(0, 9))
            read_frame(vc1, mat1);
    }

    private static byte[] b = null;

    public static byte[] ReadNextFrame()
    {
        while (f2 <= f1 * v1fps / v2fps)
        {
            f2++;
            read_frame(vc2, mat2);
        }

        //while (f1 <= f2 * v2fps / v1fps)
        {
            f1++;
            read_frame(vc1, mat1);
        }

        resize(mat1, 640, 480);
        crop(mat2, mat2, 11, 6, 360, 274);

        b ??= new byte[get_width(mat1) * get_height(mat1) * get_channel(mat1)];
        
        Marshal.Copy(get_image(mat1), b, 0, get_width(mat1) * get_height(mat1) * get_channel(mat1));
        
        return b;
    }

    public static byte[] GetDlFrame(int x, int y, int w, int h)
    {
        var fx = ConvertAxis((x, y));
        (int x, int y) fw = (h * 288 / 929 * 1520 / 480, h * 288 / 929 * 1520 / 480);
        var cloned = clone_mat(mat2);
        crop(cloned, cloned, fx.x, fx.y, fw.x, fw.y);
        if (validate_mat(cloned) != 0) return null;
        resize(cloned, 60, 60);
        grayscale(cloned);
        var b = new byte[get_width(cloned) * get_height(cloned) * get_channel(cloned)];
        Marshal.Copy(get_image(cloned), b, 0, get_width(cloned) * get_height(cloned) * get_channel(cloned));
        return b;
    }

    public static void DrawRect(int x, int y, int w, int h)
    {
        draw_rect(mat1, x, y, w, h);
        var fx = ConvertAxis((x, y));
        (int x, int y) fw = (h * 288 / 929 * 1520 / 480, h * 288 / 929 * 1520 / 480);
        draw_rect(mat2, fx.x, fx.y, fw.x, fw.y);
    }

    public static void SaveImage(int x, int y, int w, int h)
    {
        Console.Write($"\rSaving {ix}");
        crop(mat1, crop1, x, y, w, h);
        save_image(crop1, $"data/2/{ix}_real.png");

        var fx = SetMinAsZero(ConvertAxis((x, y)));
        var fw = SetMinAsZero(ConvertAxis((w, h)));
        crop(mat2, crop2, fx.x, fx.y, fw.x, fw.y);
        save_image(crop2, $"data/2/{ix}_ir.png");
        ix++;
    }

    private static int SetMinAsZero(int x)
	    => x < 0 ? 0 : x;

    private static (int x, int y) SetMinAsZero((int x, int y) k)
	    => (SetMinAsZero(k.x), SetMinAsZero(k.y));


    public static void Display()
    {
        image_show(mat1, "mat1");
        image_show(mat2, "mat2");
    }

    public static (int x, int y) ConvertAxis((int x, int y) k) => ((k.x - 171) * 360 / 289, (k.y - 82) * 274 / 293);

    [DllImport("RnENative")]
    static extern IntPtr image_read(IntPtr mat, string path);
    [DllImport("RnENative")]
    static extern IntPtr open_video(string path);
    [DllImport("RnENative")]
    static extern IntPtr read_frame(IntPtr vc, IntPtr mat);
    [DllImport("RnENative")]
    static extern int get_height(IntPtr mat);
    [DllImport("RnENative")]
    static extern int get_width(IntPtr mat);
    [DllImport("RnENative")]
    static extern void draw_rect(IntPtr mat, int x, int y, int w, int h);
    [DllImport("RnENative")]
    static extern void image_show(IntPtr mat, string name);
    [DllImport("RnENative")]
    static extern int get_channel(IntPtr mat);
    [DllImport("RnENative")]
    static extern IntPtr get_image(IntPtr mat);
    [DllImport("RnENative")]
    static extern IntPtr resize(IntPtr mat, int w, int h);
    [DllImport("RnENative")]
    static extern IntPtr crop(IntPtr mat, IntPtr crop, int x, int y, int w, int h);
    [DllImport("RnENative")]
    static extern IntPtr create_mat();
    [DllImport("RnENative")]
    static extern void save_image(IntPtr mat, string str);
    [DllImport("RnENative")]
    static extern int get_total_frame(IntPtr mat);
    [DllImport("RnENative")]
    static extern double get_fps(IntPtr mat);
    [DllImport("RnENative")]
    static extern int get_total_time(IntPtr vc);
    [DllImport("RnENative")]
    static extern void grayscale(IntPtr mat);
    [DllImport("RnENative")]
    static extern IntPtr clone_mat(IntPtr mat);
    [DllImport("RnENative")]
    static extern int validate_mat(IntPtr mat);
}
