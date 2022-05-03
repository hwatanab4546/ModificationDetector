﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModificationDetector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModificationDetector.Tests
{
    [TestClass()]
    public class StringPropertyTest_Restore
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

        private string _B = string.Empty;
        public string B
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

            A = "aaa";
            B = "BBB";
            MD.StartModificationDetecting();

            Assert.IsFalse(MD.ExactlyModified);
            Assert.IsFalse(flag);

            A = "bbbbbb";

            Assert.IsTrue(MD.ExactlyModified);
            Assert.IsTrue(flag);

            B = "a";

            Assert.IsTrue(MD.ExactlyModified);
            Assert.IsTrue(flag);

            MD.Restore();

            Assert.IsFalse(MD.ExactlyModified);
            Assert.IsFalse(flag);

            Assert.AreEqual("aaa", A);
            Assert.AreEqual("BBB", B);
        }
    }
}