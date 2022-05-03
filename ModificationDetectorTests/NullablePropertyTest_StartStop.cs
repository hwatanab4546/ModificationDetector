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
    public class NullablePropertyTest_StartStop
    {
        private readonly ModificationDetector MD = new();

        private string? _A = null;
        public string? A
        {
            get => _A;
            set
            {
                _A = value;
                MD.NotifyPropertyChanged(nameof(A), value);
            }
        }

        private string? _B = string.Empty;
        public string? B
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

            MD.StartModificationDetecting();

            Assert.IsFalse(MD.ExactlyModified);
            Assert.IsFalse(flag);

            A = null;

            Assert.IsFalse(MD.ExactlyModified);
            Assert.IsFalse(flag);

            B = string.Empty;

            Assert.IsFalse(MD.ExactlyModified);
            Assert.IsFalse(flag);

            B = null;

            Assert.IsTrue(MD.ExactlyModified);
            Assert.IsTrue(flag);

            A = string.Empty;

            Assert.IsTrue(MD.ExactlyModified);
            Assert.IsTrue(flag);

            B = string.Empty;

            Assert.IsTrue(MD.ExactlyModified);
            Assert.IsTrue(flag);

            A = null;

            Assert.IsFalse(MD.ExactlyModified);
            Assert.IsFalse(flag);

            MD.StopModificationDetecting();

            Assert.IsNull(MD.ExactlyModified);
            Assert.IsNull(flag);
        }
    }
}