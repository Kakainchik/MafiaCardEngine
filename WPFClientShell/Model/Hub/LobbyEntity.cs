using Swordfish.NET.Collections;
using System.Collections.ObjectModel;
using WPFClientShell.Core;

namespace WPFClientShell.Model.Hub
{
    public class LobbyEntity : ObservableObject
    {
        private int maxSeats;
        private ConcurrentObservableDictionary<RoleVisual, int> roles;
        private string cityName = string.Empty;

        public int Id { get; }
        public UserEntity Host { get; }
        public string Title { get; }
        public string? Description { get; }
        public ObservableCollection<UserReadinessDecorator> Players { get; }

        public int MaxSeats
        {
            get => maxSeats;
            set
            {
                maxSeats = value;
                OnPropertyChanged(nameof(MaxSeats));
            }
        }

        public ConcurrentObservableDictionary<RoleVisual, int> Roles
        {
            get => roles;
            set
            {
                roles = value;
                OnPropertyChanged(nameof(Roles));
            }
        }

        public string CityName
        {
            get => cityName;
            set
            {
                cityName = value;
                OnPropertyChanged(nameof(CityName));
            }
        }

        public LobbyEntity(int id, UserEntity host, string title, string? description, int maxSeats)
        {
            Id = id;
            Host = host;
            Title = title;
            Description = description;
            this.maxSeats = maxSeats;

            Players = new ObservableCollection<UserReadinessDecorator>();
            roles = new ConcurrentObservableDictionary<RoleVisual, int>();
        }
    }
}