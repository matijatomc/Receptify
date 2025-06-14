using System.ComponentModel;

namespace Receptify.Models
{
    public class EditableItem : INotifyPropertyChanged
    {
        private string text;

        public string Text
        {
            get => text;
            set
            {
                if (text != value)
                {
                    text = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
                }
            }
        }

        public EditableItem(string text)
        {
            Text = text;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
