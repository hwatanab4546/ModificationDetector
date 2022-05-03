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
    public class StringPropertyTest_Restart
    {
        private readonly ModificationDetector MD = new();

        private string _A = string.Empty;
        public string A
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

            A = "aaa";
            MD.StartModificationDetecting();

            Assert.IsFalse(MD.ExactlyModified);
            Assert.IsFalse(flag);

            A = "bbb";

            Assert.IsTrue(MD.ExactlyModified);
            Assert.IsTrue(flag);

            MD.RestartModificationDetecting();

            Assert.IsFalse(MD.ExactlyModified);
            Assert.IsFalse(flag);

            A = "aaa";

            Assert.IsTrue(MD.ExactlyModified);
            Assert.IsTrue(flag);
        }
    }
}