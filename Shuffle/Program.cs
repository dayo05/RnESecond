var root = "/home/dayo/datasets/rne/";
var rnd = new Random();
foreach (var s in Directory.EnumerateDirectories(root).SelectMany(Directory.EnumerateDirectories).SelectMany(Directory.EnumerateFiles).Where(x => x.EndsWith("_ir.png")))
{
    var k = s.Split(Path.PathSeparator);
    File.Copy(s,
        rnd.NextDouble() >= 0.2
            ? Path.Combine("/home/dayo/datasets/rne/train", string.Join(Path.PathSeparator, k[^3..]))
            : Path.Combine("/home/dayo/datasets/rne/test", string.Join(Path.PathSeparator, k[^3..])));
}

