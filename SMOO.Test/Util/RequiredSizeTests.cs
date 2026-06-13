using SMOO.Util;

namespace SMOO.Test.Util;

[TestClass]
public class RequiredSizeTests
{

    struct StructSize10
    {
        [RequiredField]
        public int Int32;

        [RequiredField]
        public float Single32;

        [RequiredField]
        public ushort UInt16;
    }

    struct StructSize18
    {
        [RequiredField]
        public int Int32;
        public double Ignored1;
        [RequiredField]
        public float Single32;
        [RequiredField]
        public ushort UInt16;
        public long Ignored2;
        [RequiredField]
        public long Long64;
    }

    [TestMethod]
    public void ShouldReturn10()
    {
        // Arrange
        ushort expected = 10;

        // Act
        ushort actual = RequiredSize<StructSize10>.Size;

        // Assert
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void ShouldIgnoreNotRequired()
    {
        // Arrange
        ushort expected = 18;

        // Act
        ushort actual = RequiredSize<StructSize18>.Size;

        // Assert
        Assert.AreEqual(expected, actual);
    }
}
