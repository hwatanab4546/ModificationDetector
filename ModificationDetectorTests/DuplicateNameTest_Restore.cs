using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModificationDetector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModificationDetector.Tests
{
    [TestClass()]
    public class DuplicateNameTest_Restore
    {
        private readonly ModificationDetector MD = new();

        private class Item
        {
            private int _Value;
            public int Value
            {
                get => _Value;
                set
                {
                    _Value = value;
                    MD.NotifyPropertyChanged(nameof(Value), Value, additionalPropertyName);
                }
            }

            private readonly string additionalPropertyName;
            private readonly ModificationDetector MD;

            public Item(int s, ModificationDetector md)
            {
                _Value = 0;

                additionalPropertyName = s.ToString();
                MD = md;

                md.RegisterProperty(this, nameof(Value), additionalPropertyName);
            }
        }

        private Item[] Items = Array.Empty<Item>();

        [TestMethod()]
        public void Test()
        {
            bool? ExactlyModified = null;
            MD.ExactlyModifiedChanged += (s, e) => ExactlyModified = e.ExactlyModified;
            Items = Enumerable.Range(0, 5).Select(s => new Item(s, MD)).ToArray();

            Assert.IsNull(MD.ExactlyModified);

            Items[0].Value = 5;
            MD.StartModificationDetecting();

            Assert.IsFalse(MD.ExactlyModified);
            CollectionAssert.AreEqual(new[] { 5, 0, 0, 0, 0, }, Items.Select(s => s.Value).ToArray());

            for (var i = 0; i < 5; ++i)
            {
                Items[i].Value = i + 3;
            }

            Assert.IsTrue(MD.ExactlyModified);
            CollectionAssert.AreEqual(new[] { 3, 4, 5, 6, 7, }, Items.Select(s => s.Value).ToArray());

            MD.Restore();

            Assert.IsFalse(MD.ExactlyModified);
            CollectionAssert.AreEqual(new[] { 5, 0, 0, 0, 0, }, Items.Select(s => s.Value).ToArray());

            MD.StopModificationDetecting();

            Assert.IsNull(MD.ExactlyModified);
            CollectionAssert.AreEqual(new[] { 5, 0, 0, 0, 0, }, Items.Select(s => s.Value).ToArray());
        }
    }
}