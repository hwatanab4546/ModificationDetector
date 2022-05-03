using Microsoft.Toolkit.Mvvm.ComponentModel;
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
    public class ObservableObjectTest_StartStop : ObservableObject
    {
        private readonly ModificationDetector MD = new();

        private int _A = 0;
        public int A
        {
            get => _A;
            set
            {
                if (SetProperty(ref _A, value))
                {
                    MD.NotifyPropertyChanged(nameof(A), value);
                }
            }
        }

        private int _B = 0;
        public int B
        {
            get => _B;
            set
            {
                if (SetProperty<int>(ref _B, value))
                {
                    MD.NotifyPropertyChanged(nameof(B), value);
                }
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

            A = 10;

            Assert.IsFalse(MD.ExactlyModified);
            Assert.IsFalse(flag);

            A = 11;

            Assert.IsTrue(MD.ExactlyModified);
            Assert.IsTrue(flag);

            B = 20;

            Assert.IsTrue(MD.ExactlyModified);
            Assert.IsTrue(flag);

            B = 21;

            Assert.IsTrue(MD.ExactlyModified);
            Assert.IsTrue(flag);

            A = 10;

            Assert.IsTrue(MD.ExactlyModified);
            Assert.IsTrue(flag);

            B = 20;

            Assert.IsFalse(MD.ExactlyModified);
            Assert.IsFalse(flag);

            MD.StopModificationDetecting();

            Assert.IsNull(MD.ExactlyModified);
            Assert.IsNull(flag);
        }
    }
}