using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Receptify.Models
{
    public class RecipeDisplay : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int CookingTimeMinutes { get; set; }
        public int Rating { get; set; } = 0;

        private bool _isFavorite;
        public bool IsFavorite
        {
            get => _isFavorite;
            set
            {
                if (_isFavorite != value)
                {
                    _isFavorite = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(FavoriteIcon));
                }
            }
        }

        public List<string> Tags { get; set; } = new();
        public string TagList => Tags != null && Tags.Count > 0 ? string.Join(", ", Tags) : "Bez oznaka";

        public string FavoriteIcon => IsFavorite ? "heart_filled.svg" : "heart_outline.svg";

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
