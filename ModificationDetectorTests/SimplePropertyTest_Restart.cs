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
    public class SimplePropertyTest_Restart
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

        [TestMethod()]
        public void Test()
        {
            bool? flag = null;
            MD.ExactlyModifiedChanged += (s, e) => flag = e.ExactlyModified;
            MD.RegisterProperty(this, nameof(A));

            Assert.IsNull(MD.ExactlyModified);
            Assert.IsNull(flag);

            A = 10;
            MD.StartModificationDetecting();

            Assert.IsFalse(MD.ExactlyModified);
            Assert.IsFalse(flag);

            A = 11;

            Assert.IsTrue(MD.ExactlyModified);
            Assert.IsTrue(flag);

            MD.RestartModificationDetecting();

            Assert.IsFalse(MD.ExactlyModified);
            Assert.IsFalse(flag);

            A = 10;

            Assert.IsTrue(MD.ExactlyModified);
            Assert.IsTrue(flag);
        }
    }
}