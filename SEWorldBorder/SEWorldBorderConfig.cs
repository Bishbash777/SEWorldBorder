using System.Collections.ObjectModel;
using Torch;

namespace SEWorldBorder {
    public class SEWorldBorderConfig : ViewModel {

        private int _radius = 0;
        public int Radius { get => _radius; set => SetValue(ref _radius, value); }

    }
}
