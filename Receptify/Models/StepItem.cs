using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Receptify.Models
{
    public class StepItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Text { get; set; }

        public int Index { get; set; }

        public string IndexDisplay => $"{Index}.";
    }
}
