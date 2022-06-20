using RnE;
using TorchSharp;

using static System.Linq.Enumerable;
using static TorchSharp.torch;
using static TorchSharp.torch.nn;
using static TorchSharp.torch.utils.data;

CV.Initialize();

var train_data = new Dset("/home/dayo/datasets/rne/train");
var test_data = new Dset("/home/dayo/datasets/rne/test");
var train = new DataLoader(train_data, 32, true, device: CUDA);
var test = new DataLoader(test_data, 64, false, device: CUDA);

var model = (RnEModel) new RnEModel().to(CUDA);
var cri = functional.mse_loss();
var opt = optim.Adam(model.parameters(), lr: 0.01);
foreach (var epoch in Range(1, 10000))
{
    if(epoch == 300)
        foreach (var g in opt.ParamGroups)
            g.LearningRate = 0.001;
    var avg_cost = 0.0;
    foreach (var x in train)
    {
        opt.zero_grad();
        var o = model.forward(x);
        var cost = cri(o, x["label"]);
        cost.backward();
        opt.step();

        avg_cost += cost.cpu().item<float>() / train.Count;
    }
    Console.WriteLine(avg_cost);
    avg_cost = 0.0;
    using (no_grad())
    {
        foreach (var x in test)
        {
            var o = model.forward(x);
            var cost = cri(o, x["label"]);
            avg_cost += cost.cpu().item<float>() / test.Count;
        }
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

    public Tensor forward(Dictionary<string, Tensor> tl)
    {
        return fc.forward(cat(
                new[]
                {
                    layer5.forward(layer4.forward(layer3.forward(layer2.forward(layer1.forward(tl["data"])))))
                        .flatten(1),
                    tl["humi"], tl["temp"]
                }, 1));
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
            {"humi", tensor(new[]{humi}, new long[]{1}, ScalarType.Float32, requiresGrad: true)},
            {"temp", tensor(new[]{temp}, new long[]{1}, ScalarType.Float32, requiresGrad: true)},
            {"label", tensor(new[]{double.Parse(splited[^2])}, new long[]{1}, ScalarType.Float32, requiresGrad: true)}
        };
    }

    public override long Count => images.Count;
}
