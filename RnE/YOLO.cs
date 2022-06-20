using Python.Runtime;

namespace RnE;

public static class YOLO
{
    private static dynamic model;
    private static dynamic torch;
    private static dynamic np;
    public static void Initialize()
    {
        PythonEngine.Initialize();
        using (Py.GIL())
        {
            torch = Py.Import("torch");
            np = Py.Import("numpy");
            model = torch.hub.load("ultralytics/yolov5", "yolov5s6");
            model.eval();
        }
    }

    public static List<(int x, int y, int w, int h)> Yolo(List<byte> image)
    {
        using (Py.GIL())
        {
            dynamic nparray = np.array(image).reshape(480, 640, 3);
            dynamic k = model(nparray, size: 640);
            dynamic df = k.pandas().xywh[0];
            df.columns = new[] {"x", "y", "w", "h", "conf", "c", "n"};

            var ret = new List<(int, int, int, int)>();
            foreach (dynamic c in df.itertuples())
            {
                if (PyInt.AsInt(c.c) != 0) continue;
                ret.Add((c.x, c.y, c.w, c.h));
            }

            return ret;
        }
    }
}