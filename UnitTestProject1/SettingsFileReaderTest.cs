using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SectorfileEditor;
using SectorfileEditor.Control;
using SectorfileEditor.Model;

namespace UnitTestProject1
{
    [TestClass]
    public class SettingsFileReaderTest
    {
        [TestMethod]
        public void TestReading()
        {
            int lineCount = 0;

            var reader = new SettingsFileReader<SymbolSettingsFileLine>();
            reader.SettingsLineHandler = 
                line =>
                {
                    lineCount++;
                   
                    if (lineCount == 1)
                    {
                        Assert.AreEqual("Geo:line", line.Key);
                        Assert.AreEqual("6710886", line.Values[0]);
                        Assert.AreEqual("6710886", line.Values[0]);
                        Assert.AreEqual("3.0", line.Values[1]);
                        Assert.AreEqual("0", line.Values[2]);
                        Assert.AreEqual("0", line.Values[3]);
                        Assert.AreEqual("7", line.Values[4]);
                    }
                    else if(lineCount == 2)
                    {
                        Assert.AreEqual("Sector:inactive sector background", line.Key);
                        Assert.AreEqual("8882055", line.Values[0]);
                        Assert.AreEqual("3.5", line.Values[1]);
                        Assert.AreEqual("0", line.Values[2]);
                        Assert.AreEqual("0", line.Values[3]);
                        Assert.AreEqual("7", line.Values[4]);
                    }
                };

            reader.Parse("..\\..\\Testdata\\Symbols.txt");
            Assert.AreEqual(2, lineCount);

        }
    }
}
