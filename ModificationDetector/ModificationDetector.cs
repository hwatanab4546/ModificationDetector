using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ModificationDetector
{
    public class ModificationDetector
    {
        public class ExactlyModifiedChangedEventArgs : EventArgs
        {
            public bool? ExactlyModified { get; private set; }

            public ExactlyModifiedChangedEventArgs(bool? exactlyModified)
                => ExactlyModified = exactlyModified;
        }
        public delegate void ExactlyModifiedChangedEventHander(object sender, ExactlyModifiedChangedEventArgs e);
        public event ExactlyModifiedChangedEventHander? ExactlyModifiedChanged;

        public class IsModificationDetectingChangedEventArgs : EventArgs
        {
            public bool IsModificationDetecting { get; private set; }

            public IsModificationDetectingChangedEventArgs(bool isModificationDetecting)
                => IsModificationDetecting = isModificationDetecting;
        }
        public delegate void IsModificationDetectingChangedEventHander(object sender, IsModificationDetectingChangedEventArgs e);
        public event IsModificationDetectingChangedEventHander? IsModificationDetectingChanged;

        /// <summary>
        /// 監視対象プロパティの登録
        /// </summary>
        /// <param name="parentInstance">親インスタンス</param>
        /// <param name="propertyName">プロパティ名</param>
        /// <param name="additionalPropertyName">監視対象に同名のプロパティが含まれる場合に互いを区別するための付加情報</param>
        public void RegisterProperty(object parentInstance, string propertyName, string? additionalPropertyName = null)
        {
            if (IsModificationDetecting)
            {
                throw new Exception("unable to register properties while modification detecting");
            }
            if (parentInstance is null)
            {
                throw new ArgumentNullException(nameof(parentInstance));
            }
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentNullException(nameof(propertyName));
            }
            var key = KeyGen(propertyName, additionalPropertyName);
            if (PropertyDic.ContainsKey(key))
            {
                throw new Exception($"already registered property: {key}");
            }
            PropertyDic.Add(key, new Property(propertyName, parentInstance));
        }
        /// <summary>
        /// 監視対象プロパティの登録解除
        /// </summary>
        /// <param name="propertyName">プロパティ名</param>
        /// <param name="additionalPropertyName">監視対象に同名のプロパティが含まれる場合に互いを区別するための付加情報</param>
        public void UnregisterProperty(string propertyName, string? additionalPropertyName = null)
        {
            if (IsModificationDetecting)
            {
                throw new Exception("unable to unregister properties while modification detecting");
            }
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentNullException(nameof(propertyName));
            }
            var key = KeyGen(propertyName, additionalPropertyName);
            if (!PropertyDic.ContainsKey(key))
            {
                throw new Exception($"not registered property: {key}");
            }
            PropertyDic.Remove(key);
        }
        /// <summary>
        /// 監視対象プロパティ名一覧の取得。ただし、付加情報込みなので厳密に言えばプロパティ名というわけではない
        /// </summary>
        public IEnumerable<string> RegisteredPropertyNames
            => PropertyDic.Keys.AsEnumerable();

        /// <summary>
        /// 登録済みプロパティの現在値を取得し、監視状態に移行する。
        /// </summary>
        public void StartModificationDetecting()
        {
            if (IsModificationDetecting)
            {
                throw new Exception("already started modification detecting");
            }

            foreach (var property in PropertyDic.Values)
            {
                property.Prepare();
            }
            ExactlyModifiedCount = 0;
            IsModificationDetecting = true;

            IsModificationDetectingChanged?.Invoke(this, new IsModificationDetectingChangedEventArgs(true));
            ExactlyModifiedChanged?.Invoke(this, new ExactlyModifiedChangedEventArgs(false));
        }
        /// <summary>
        /// 登録済みプロパティの現在値を再取得する。
        /// </summary>
        public void RestartModificationDetecting()
        {
            if (!IsModificationDetecting)
            {
                throw new Exception("not yet started modification detecting");
            }

            var prevCount = ExactlyModifiedCount;
            foreach (var property in PropertyDic.Values)
            {
                property.Prepare();
            }
            ExactlyModifiedCount = 0;

            if (prevCount != 0)
            {
                ExactlyModifiedChanged?.Invoke(this, new ExactlyModifiedChangedEventArgs(false));
            }
        }
        /// <summary>
        /// 登録済みプロパティの設定値を、監視開始時点の設定値に戻す。
        /// </summary>
        public void Restore()
        {
            if (!IsModificationDetecting)
            {
                throw new Exception("not yet started modification detecting");
            }

            var prevCount = ExactlyModifiedCount;
            foreach (var property in PropertyDic.Values)
            {
                property.Restore();
            }
            ExactlyModifiedCount = 0;

            if (prevCount != 0)
            {
                ExactlyModifiedChanged?.Invoke(this, new ExactlyModifiedChangedEventArgs(false));
            }
        }
        /// <summary>
        /// 登録済みプロパティの監視を解除する
        /// </summary>
        public void StopModificationDetecting()
        {
            if (!IsModificationDetecting)
            {
                throw new Exception("not yet started modification detecting");
            }

            IsModificationDetecting = false;

            ExactlyModifiedChanged?.Invoke(this, new ExactlyModifiedChangedEventArgs(null));
            IsModificationDetectingChanged?.Invoke(this, new IsModificationDetectingChangedEventArgs(false));
        }

        /// <summary>
        /// プロパティ値の更新通知
        /// </summary>
        /// <param name="propertyName">プロパティ名</param>
        /// <param name="newValue">更新後の値</param>
        /// <param name="additionalPropertyName">監視対象に同名のプロパティが含まれる場合に互いを区別するための付加情報</param>
        public void NotifyPropertyChanged(string propertyName, object? newValue, string? additionalPropertyName = null)
        {
            if (!IsModificationDetecting)
            {
                return;
            }
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            var key = KeyGen(propertyName, additionalPropertyName);
            if (!PropertyDic.TryGetValue(key, out var p))
            {
                throw new Exception($"not registered property: {key}");
            }
            if (p.NotifyPropertyChanged(newValue))
            {
                var delta = p.ExactlyModified ? 1 : -1;
                ExactlyModifiedCount += delta;

                if ((ExactlyModifiedCount == 1 && delta > 0) || ExactlyModifiedCount == 0)
                {
                    ExactlyModifiedChanged?.Invoke(this, new ExactlyModifiedChangedEventArgs(ExactlyModifiedCount > 0));
                }
            }
        }
        /// <summary>
        /// 監視状態ではない間はnull。
        /// 監視対象プロパティのいずれかの値が監視開始時の値と異なっている場合はtrue、全て一致している場合はfalse。
        /// </summary>
        public bool? ExactlyModified
        {
            get
            {
                if (!IsModificationDetecting)
                {
                    return null;
                }

                return ExactlyModifiedCount > 0;
            }
        }
        /// <summary>
        /// 監視状態ならtrue、それ以外はfalse。
        /// </summary>
        public bool IsModificationDetecting { get; private set; } = false;

        private readonly Dictionary<string, Property> PropertyDic = new();

        private static string KeyGen(string propertyName, string? additionalPropertyName)
            => string.IsNullOrEmpty(additionalPropertyName) ? propertyName : $"{propertyName}+{additionalPropertyName}";

        private int ExactlyModifiedCount = 0;

        private class Property
        {
            public string Name { get; private set; }
            public object ParentInstance { get; private set; }
            public PropertyInfo Info { get; private set; }
            public object? OrigValue { get; private set; }

            public Property(string name, object parentInstance)
            {
                if (string.IsNullOrEmpty(name))
                {
                    throw new ArgumentNullException(nameof(name));
                }
                if (parentInstance is null)
                {
                    throw new ArgumentNullException(nameof(parentInstance));
                }

                Name = name;
                ParentInstance = parentInstance;
                var info = parentInstance.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
                if (info is null)
                {
                    throw new Exception($"failed to get PropertyInfo: {name}");
                }
                if (!info.PropertyType.IsValueType && info.PropertyType != typeof(string))
                {
                    throw new Exception($"not a value type nor string type: {name}");
                }
                Info = info;
                OrigValue = null;
            }

            public void Prepare()
            {
                OrigValue = Info.GetValue(ParentInstance);
                ExactlyModified = false;
            }
            public bool ExactlyModified { get; private set; }
            public bool NotifyPropertyChanged(object? newValue)
            {
                var oldFlag = ExactlyModified;

                ExactlyModified = !(OrigValue?.Equals(newValue) ?? newValue is null);

                return oldFlag != ExactlyModified;
            }
            public void Restore()
            {
                ExactlyModified = false;

                Info.SetValue(ParentInstance, OrigValue); // これがNotifyPropertyChanged()呼び出しの引き金になることがあり得る
            }
        }
    }
}
