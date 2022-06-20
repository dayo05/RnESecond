using RnE;


CV.Initialize();
YOLO.Initialize();

while (true)
{
    var l = YOLO.Yolo(CV.ReadNextFrame().ToList());
    if (l.Count != 0)
    {
        foreach ((int x, int y, int w, int h) x in l.Where(x => x.w <= x.h)
                     .Select(x => (x.x - x.w / 2, x.y - x.h / 2, x.w, x.w)))
        {
            (int x, int y, int w, int h) k = (x.x - x.w / 10, x.y - x.w / 10, x.w * 6 / 5, x.h * 6 / 5);
            CV.SaveImage(k.x, k.y, k.w, k.h);
            //CV.DrawRect(k.x, k.y, k.w, k.h);
        }
    }

        //CV.Display();
        //Thread.Sleep(1);
}
/*
 
        var t = tensor(image.Select(x => x / 255.0).ToList(), new long[] {get_height(), get_width(), get_channel()});
        Console.WriteLine(model.forward(t).ToString(TensorStringStyle.Numpy));
 */


/*

var train_data = CIFAR100("data", true, true);
var test_data = CIFAR100("data", false, true);
var train = new DataLoader(train_data, 4096, true, CUDA);
var test = new DataLoader(test_data, 4096, false, CUDA);

//var net = TorchSharp.torchvision.models.vgg11(100, device: CUDA);
var net = resnet34(100, device: CUDA);
var loss = functional.cross_entropy_loss();
var opt = torch.optim.Adam(net.parameters(), 0.0001);
var train_count = train.Count;
var test_count = test.Count;
var test_data_count = test_data.Count;

foreach (var x in Range(1, 10000))
{
    Console.WriteLine($"Epoch {x}");
    var avg_cost = 0.0;
    var idx = 0;
    foreach (var t in train)
    {
        idx++;
        Console.Write($"\rRunning {idx * 100.0 / train_count}%                                           ");
        opt.zero_grad();
        var hypothesis = net.forward(t["data"]);
        var cost = loss(hypothesis, t["label"]);
        cost.backward();
        opt.step();

        avg_cost += cost.ToSingle();
    }

    Console.WriteLine();
    Console.WriteLine($"Avg cost: {avg_cost / train_count}");

    using (no_grad())
    {
        Console.WriteLine(
            $"Accuracy: {(from t in test let hypothesis = net.forward(t["data"]) select hypothesis.argmax(1) == t["label"] into predict select predict.@float().sum().ToInt32()).Sum() * 100.0 / test_data_count}%");
    }
}
*/