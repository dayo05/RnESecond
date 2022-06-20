using System.Runtime.InteropServices;
using static System.Linq.Enumerable;

namespace RnE;

public static class CV
{
    private static IntPtr mat;
    private static object matLock = new();
    public static void Initialize()
    {
        mat = create_mat();
    }

    public static byte[] ReadImage(string dat)
    {
        lock (matLock)
        {
            image_read(mat, dat);
            resize(mat, 60, 60);
            grayscale(mat);
            var image = new byte[get_width(mat) * get_height(mat) * get_channel(mat)];
            Marshal.Copy(get_image(mat), image, 0, get_width(mat) * get_height(mat) * get_channel(mat));
            return image;
        }
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

    [DllImport("RnENative")]
    static extern void grayscale(IntPtr mat);
}
