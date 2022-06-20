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
        "/home/dayo/datasets/rne/Camera 01_cctv_20220609192000_20220609193410_1590470.mp4",
        "/home/dayo/datasets/rne/Camera 01_cctv_20220609193410_20220609203058_1627055.mp4"
    };

    private static int v1Idx = 0;

    private static List<string> v2List = new List<string>
    {
        "/home/dayo/datasets/rne/Camera 02_cctv_20220609191959_20220609203058_1590471.mp4"
    };

    private static int v2Idx = 0;
    
    public static void Initialize()
    {
        vc1 = open_video(v1List[0]);
        vc2 = open_video(v2List[0]);
        mat1 = create_mat();
        mat2 = create_mat();
        crop1 = create_mat();
        crop2 = create_mat();

        foreach (var _ in Range(0, 20))
            read_frame(vc2, mat2);
    }

    public static byte[] ReadNextFrame()
    {
        //Console.WriteLine("FrStart");
        while (read_frame(vc1, mat1) == IntPtr.Zero)
        {
            Console.WriteLine("Switch Video");
            vc1 = open_video(v1List[++v1Idx]);
        }

        resize(mat1, 640, 480);
        var image = new byte[get_width(mat1) * get_height(mat1) * get_channel(mat1)];
        Marshal.Copy(get_image(mat1), image, 0, get_width(mat1) * get_height(mat1) * get_channel(mat1));

        while (read_frame(vc2, mat2) == IntPtr.Zero)
        {
            Console.WriteLine("Switch IR Video");
            vc2 = open_video(v2List[++v2Idx]);
        }

        //Console.WriteLine("FrEnd");
        return image;
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
	Console.WriteLine("Save01");
        crop(mat1, crop1, x, y, w, h);
        save_image(crop1, $"data/{ix}_real.png");

	Console.WriteLine("Save02");
        var fx = SetMinAsZero(ConvertAxis((x, y)));
        (int x, int y) fw = SetMinAsZero((h * 288 / 929 * 1520 / 480, h * 288 / 929 * 1520 / 480));
        crop(mat2, crop2, fx.x, fx.y, fw.x, fw.y);
        save_image(crop2, $"data/{ix}_ir.png");
        Console.WriteLine($"{fx.x} {fx.y} {fw.x} {fw.y}");
        
        ix++;
	Console.WriteLine("Save Finished");
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

    public static (int x, int y) ConvertAxis((int x, int y) k)
    {
        k = (k.x * 2688 / 640, k.y * 1520 / 480);
        return ((k.x - 718) * 384 / 1218, (k.y - 262) * 288 / 929);
    }

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
}
