using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Linq;

namespace ModificationDetectorSample
{
    internal class ViewModel : ObservableObject
    {
        private bool? _ExactlyModified = null;
        public bool? ExactlyModified
        {
            get { return _ExactlyModified; }
            set { SetProperty(ref _ExactlyModified, value); }
        }

        private string? _txtString = null;
        public string? txtString
        {
            get { return _txtString; }
            set
            {
                if (SetProperty(ref _txtString, value))
                {
                    detector.NotifyPropertyChanged(nameof(txtString), value);
                }
            }
        }

        private int? _txtInt = null;
        public int? txtInt
        {
            get { return _txtInt; }
            set
            {
                if (SetProperty(ref _txtInt, value))
                {
                    detector.NotifyPropertyChanged(nameof(txtInt), value);
                }
            }
        }

        public enum Number
        {
            One,
            Two,
            Three,
            Four,
            Five,
        }
        public Dictionary<Number, string> cbxItems { get; } = new Dictionary<Number, string>
        {
            {Number.One, "I" },
            {Number.Two, "II" },
            {Number.Three, "III" },
            {Number.Four, "IV" },
            {Number.Five, "V" },
        };

        private Number _cbxSelectedValue;
        public Number cbxSelectedValue
        {
            get { return _cbxSelectedValue; }
            set
            {
                if (SetProperty(ref _cbxSelectedValue, value))
                {
                    detector.NotifyPropertyChanged(nameof(cbxSelectedValue), value);
                }
            }
        }

        public class Item : ObservableObject
        {
            public string Name { get; set; } = string.Empty;
            private bool _IsSelected = false;
            public bool IsSelected
            {
                get { return _IsSelected; }
                set
                {
                    if (SetProperty(ref _IsSelected, value))
                    {
                        detector.NotifyPropertyChanged(nameof(IsSelected), value, Name);
                    }
                }
            }
            private ModificationDetector.ModificationDetector detector;

            public Item(string name, ModificationDetector.ModificationDetector detector)
            {
                Name = name;
                this.detector = detector;
            }
        }
        public IEnumerable<Item> lbxItems { get; }

        public RelayCommand Start { get; }
        public RelayCommand Restart { get; }
        public RelayCommand Stop { get; }
        public RelayCommand Restore { get; }

        private readonly ModificationDetector.ModificationDetector detector = new();

        public ViewModel()
        {
            detector.IsModificationDetectingChanged += (s, e) =>
            {
                Start?.NotifyCanExecuteChanged();
                Restart?.NotifyCanExecuteChanged();
                Stop?.NotifyCanExecuteChanged();
                Restore?.NotifyCanExecuteChanged();
            };
            detector.ExactlyModifiedChanged += (s, e) => ExactlyModified = e.ExactlyModified;
            detector.RegisterProperty(this, nameof(txtString));
            detector.RegisterProperty(this, nameof(txtInt));
            detector.RegisterProperty(this, nameof(cbxSelectedValue));

            lbxItems = Enumerable.Range(1, 10).Select(s =>
            {
                var name = s.ToString();
                var item = new Item(name, detector);
                detector.RegisterProperty(item, nameof(item.IsSelected), name);
                return item;
            });

            txtString = string.Empty;
            txtInt = 0;
            //cbxSelectedValue = Number.One;

            Start = new RelayCommand(() => detector.StartModificationDetecting(), () => !detector.IsModificationDetecting);
            Restart = new RelayCommand(() => detector.RestartModificationDetecting(), () => detector.IsModificationDetecting);
            Stop = new RelayCommand(() => detector.StopModificationDetecting(), () => detector.IsModificationDetecting);
            Restore = new RelayCommand(() => detector.Restore(), () => detector.IsModificationDetecting);
        }
    }
}
