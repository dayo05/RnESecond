using RnE;
using TorchSharp;
using static TorchSharp.torch;
using static TorchSharp.torch.nn;

var model = new RnEModel();
model.load("rneModel.model").to(CPU);

CV.Initialize();
YOLO.Initialize();

var frame = 0;
foreach (var x in Enumerable.Range(0, 7100))
{
    frame++;
    Console.Write("\rRunning Frame: " + frame);
    CV.ReadNextFrame();
}
Console.Write("\r");

using (torch.no_grad())
{
    while (true)
    {
        //Console.Write("\rRunning Frame: " + frame);

        var l = YOLO.Yolo(CV.ReadNextFrame().ToList());
        Console.Clear();
        if (l.Count != 0)
        {
            foreach ((int x, int y, int w, int h) x in l.Where(x => x.w <= x.h)
                         .Select(x => (x.x - x.w / 2, x.y - x.h / 2, x.w, x.w)))
            {
                (int x, int y, int w, int h) k = (x.x - x.w / 10, x.y - x.w / 10, x.w * 6 / 5, x.h * 6 / 5);
                CV.DrawRect(k.x, k.y, k.w, k.h);
                
                CV.GetDlFrame(k.x, k.y, k.w, k.h);

                var dlf = CV.GetDlFrame(k.x, k.y, k.w, k.h);
                if (dlf is null)
                {
                    continue;
                }
                var t1 = tensor(dlf.Select(x => x / 255.0f).ToList(),
                    new long[] {1, 1, 60, 60},
                    ScalarType.Float32);
                var a = model.forward(t1, 
                    tensor(new[] {27, 76}, new long[] {1, 2}, ScalarType.Float32));
                Console.WriteLine(a.item<float>());
            }
        }


        CV.Display();
        frame++;
    }
}


class RnEModel : Module
{
    private Module layer1 = Sequential(
        Conv2d(1, 16, 3),
        ReLU(),
        Conv2d(16, 32, 3),
        ReLU(),
        MaxPool2d(2, padding: 1));

    private Module layer2 = Sequential(
        Conv2d(32, 32, 3),
        ReLU(),
        Conv2d(32, 32, 3),
        ReLU(),
        MaxPool2d(2, padding: 1));

    private Module layer3 = Sequential(
        Conv2d(32, 32, 3),
        ReLU(),
        MaxPool2d(2, padding: 1));

    private Module layer4 = Sequential(
        Conv2d(32, 32, 3),
        ReLU(),
        MaxPool2d(2, padding: 1));

    private Module layer5 = Sequential(
        Conv2d(32, 16, 3),
        ReLU(),
        MaxPool2d(2, padding: 1));

    private Module fc = Sequential(
        Linear(18, 64),
        ReLU(),
        Linear(64, 64),
        ReLU(),
        Linear(64, 1));
    
    public RnEModel() : base("rne")
    {
        RegisterComponents();
        to(CUDA);
    }

    public override Tensor forward(Tensor x, Tensor y)
    {
        return fc.forward(cat(
            new[]
            {
                layer5.forward(layer4.forward(layer3.forward(layer2.forward(layer1.forward(x)))))
                    .flatten(1),
                y
            }, 1));
    }

    public Tensor forward(Dictionary<string, Tensor> tl)
    {
        return forward(tl["data"], cat(new[] {tl["humi"], tl["temp"]}, 1));
    }
}