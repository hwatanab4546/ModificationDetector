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
    public class SimplePropertyTest_Restore
    {
        private readonly ModificationDetector MD = new();

        private int _A = 0;
        public int A
        {
            get => _A;
            set
            {
                _A = value;
                MD.NotifyPropertyChanged(nameof(A), value);
            }
        }

        private int _B = 0;
        public int B
        {
            get => _B;
            set
            {
                _B = value;
                MD.NotifyPropertyChanged(nameof(B), value);
            }
        }

        [TestMethod()]
        public void Test()
        {
            bool? flag = null;
            MD.ExactlyModifiedChanged += (s, e) => flag = e.ExactlyModified;
            MD.RegisterProperty(this, nameof(A));
            MD.RegisterProperty(this, nameof(B));

            Assert.IsNull(MD.ExactlyModified);
            Assert.IsNull(flag);

            A = 10;
            B = 20;
            MD.StartModificationDetecting();

            Assert.IsFalse(MD.ExactlyModified);
            Assert.IsFalse(flag);

            A = 11;

            Assert.IsTrue(MD.ExactlyModified);
            Assert.IsTrue(flag);

            B = 21;

            Assert.IsTrue(MD.ExactlyModified);
            Assert.IsTrue(flag);

            MD.Restore();

            Assert.IsFalse(MD.ExactlyModified);
            Assert.IsFalse(flag);

            Assert.AreEqual(10, A);
            Assert.AreEqual(20, B);
        }
    }
}