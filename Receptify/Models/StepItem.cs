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
        private string stepNumber;
        public string StepNumber
        {
            get => stepNumber;
            set
            {
                if (stepNumber != value)
                {
                    stepNumber = value;
                    OnPropertyChanged(nameof(StepNumber));
                }
            }
        }

        public string Description { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
