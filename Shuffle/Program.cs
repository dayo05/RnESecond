var root = "/home/dayo/datasets/jeju/";

var train = "/home/dayo/datasets/jeju/train";
var test = "/home/dayo/datasets/jeju/test";

var cnt = 0;
Console.WriteLine(Directory.EnumerateDirectories(train).SelectMany(Directory.EnumerateDirectories)/*.SelectMany(Directory.EnumerateFiles)*/.Count());

//return;
var rnd = new Random();
foreach (var s in Directory.EnumerateDirectories(root)/*.SelectMany(Directory.EnumerateDirectories)*/.SelectMany(Directory.EnumerateFiles)/*.Where(x => x.EndsWith("_ir.png"))*/)
{
    var k = s.Split('/');
    var path = rnd.NextDouble() >= 0.2
            ? Path.Combine(train, string.Join('/', k[^3..]))
            : Path.Combine(test, string.Join('/', k[^3..]));
    Directory.CreateDirectory(string.Join('/', path.Split('/')[..^1]));
    File.Copy(s, path);
    Console.WriteLine($"Copy {s} to {path}");
}

