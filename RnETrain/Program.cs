using RnE;
using TorchSharp;
using static TorchSharp.torch;
using static TorchSharp.torch.nn;
using static TorchSharp.torch.utils.data;

CV.Initialize();

//Dset.GetDset("/home/dayo/dsets");
var ds = new Dset("/home/dayo/datasets/rne/train");
var dl = new DataLoader(ds, 32, true, device: CUDA);
foreach(var x in dl)
    Console.WriteLine(x["data"]);
/*
var tt = zeros(1, 1, 60, 60, dtype: ScalarType.Float32, requiresGrad: true);
var model = new RnEModel();
model.forward(new Dictionary<string, Tensor>
{
    {"data", tt},
    {"humi", 35.0f.ToTensor(requiresGrad: true).reshape(1, 1)},
    {"temp", 27.4f.ToTensor(requiresGrad: true).reshape(1, 1)}
});
*/

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
    }

    public Tensor forward(Dictionary<string, Tensor> tl)
    {
        fc.forward(cat(
                new[]
                {
                    layer5.forward(layer4.forward(layer3.forward(layer2.forward(layer1.forward(tl["data"])))))
                        .flatten(1),
                    tl["humi"], tl["temp"]
                }, 1))
            .print(TensorStringStyle.Metadata);

        return tl["data"];
    }
}

class Dset: Dataset
{
    public static (Dset, Dset) GetDset(string root)
        => (new Dset(Path.Combine(root, "train")), new Dset(Path.Combine(root, "val")));
    
    private List<string> images = new();
    public Dset(string root)
    {
        // /temp_humi/rtmp/dat
        images.AddRange(Directory.EnumerateDirectories(root).SelectMany(Directory.EnumerateDirectories).SelectMany(Directory.EnumerateFiles).Where(x => x.EndsWith("_ir.png")));
        images.ForEach(Console.WriteLine);
    }
    
    public override Dictionary<string, Tensor> GetTensor(long index)
    {
        var iname = images[(int) index];
        var splited = iname.Split(Path.DirectorySeparatorChar);
        var x = splited[^3].Split('_').Select(double.Parse).ToList();
        var (humi, temp) = (x[1], x[0]);
        return new()
        {
            {"data", tensor(CV.ReadImage(iname).Select(x => x / 255.0f).ToList(), new long[]{1, 60, 60}, ScalarType.Float32, requiresGrad: true)},
            {"humi", humi.ToTensor(requiresGrad: true)},
            {"temp", temp.ToTensor(requiresGrad: true)},
            {"label", double.Parse(splited[^2]).ToTensor(requiresGrad: true)}
        };
    }

    public override long Count => images.Count;
}
