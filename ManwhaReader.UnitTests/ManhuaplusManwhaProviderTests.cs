using ManwhaReader.Core;
using ManwhaReader.Core.Interfaces;

namespace ManwhaReader.UnitTests;

[TestClass]
public class ManhuaplusManwhaProviderTests
{
    [TestMethod]
    public async Task SearchTest()
    {
        IManwhaProvider provider = new ManhuaplusManwhaProvider();

        var result = (await provider.Search("demon")).ToList();
        
        Assert.IsTrue(result.Count != 0);
        Assert.IsTrue(result.All(x => x.ImageData.Length != 0));
    }
    
    [TestMethod]
    public async Task SearchWithoutImageLoadingTest()
    {
        IManwhaProvider provider = new ManhuaplusManwhaProvider();

        var result = (await provider.Search("demon", false)).ToList();
        
        Assert.IsTrue(result.Count != 0);
        Assert.IsTrue(result.All(x => x.ImageData.Length == 0));
    }
}