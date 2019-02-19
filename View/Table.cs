using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using ReactiveUI;
namespace View
{
    public class MoneyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return "$" + value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class Table : ReactiveObject
    {
        public ObservableCollection<MyPoker.IPlayer> Players { get; set; }
        public ObservableCollection<MyPoker.Card> Cards { get; set; }
        private MyPoker.Enumerations.Rounds round;
        public MyPoker.Enumerations.Rounds Round
        {
            get => round;
            set => this.RaiseAndSetIfChanged(ref round, value);
        }
        public ObservableCollection<string> List { get; set; }

        string name;
        public String Name
        {
            get => name;
            set => this.RaiseAndSetIfChanged(ref name, value);
        }

        public Table()
        {
            Cards = new ObservableCollection<MyPoker.Card>();
            Players = new ObservableCollection<MyPoker.IPlayer>();
            List = new ObservableCollection<string>();

            GameProcessCommand = ReactiveCommand.CreateFromTask(async () => await Task.Run(() => GameProcess()));
        }
        public void GameProcess()
        {
            Name = "MyName";
            if (Cards.Count() == 0)
            {

                Cards.Add(new MyPoker.Card() { Rank = (MyPoker.Enumerations.Ranks)rand.Next(2, 14), Suit = (MyPoker.Enumerations.Suits)rand.Next(4) });
            Task.Delay(1000).Wait();
                Cards.Add(new MyPoker.Card() { Rank = (MyPoker.Enumerations.Ranks)rand.Next(2, 14), Suit = (MyPoker.Enumerations.Suits)rand.Next(4) });
            Task.Delay(1000).Wait();
                Cards.Add(new MyPoker.Card() { Rank = (MyPoker.Enumerations.Ranks)rand.Next(2, 14), Suit = (MyPoker.Enumerations.Suits)rand.Next(4) });
            }
            else
            {
                Cards[0] = new MyPoker.Card() { Rank = (MyPoker.Enumerations.Ranks)rand.Next(2, 14), Suit = (MyPoker.Enumerations.Suits)rand.Next(4) };
                Task.Delay(1000).Wait();
                Cards[1] = new MyPoker.Card() { Rank = (MyPoker.Enumerations.Ranks)rand.Next(2, 14), Suit = (MyPoker.Enumerations.Suits)rand.Next(4) };
                Task.Delay(1000).Wait();
                Cards[2] = new MyPoker.Card() { Rank = (MyPoker.Enumerations.Ranks)rand.Next(2, 14), Suit = (MyPoker.Enumerations.Suits)rand.Next(4) };
            }
        }
        private bool isPlayerTurn = false;
        public bool IsPlayerTurn
        {
            get => isPlayerTurn;
            set => this.RaiseAndSetIfChanged(ref isPlayerTurn, value);
        }
        private void PlayersBet()
        {
            foreach (var player in Players)
            {
                if (player is MyPoker.RealPlayer k && k.Status)
                    IsPlayerTurn = true;
                if (IsPlayerTurn)
                    while (IsPlayerTurn) ;
                Task.Delay(1000).Wait();
            }
        }
        private void AddPlayers()
        {
            Players.Clear();
            Task.Delay(500).Wait();
            Players.Add(new MyPoker.RealPlayer("Me", new MyPoker.Card[]{
                    new MyPoker.Card()
                        {
                            Rank = (MyPoker.Enumerations.Ranks)rand.Next(2, 14),
                            Suit = (MyPoker.Enumerations.Suits)rand.Next(4)
                        },
                    new MyPoker.Card()
                        {
                            Rank = (MyPoker.Enumerations.Ranks)rand.Next(2, 14),
                            Suit = (MyPoker.Enumerations.Suits)rand.Next(4)
                        }
                    },
                    1_000));
            Task.Delay(1000).Wait();

            for (int i = 0; i < 3; i++)
            {
                Players.Add(new MyPoker.Computer("Player", new MyPoker.Card[]{
                    new MyPoker.Card()
                        {
                            Rank = (MyPoker.Enumerations.Ranks)rand.Next(2, 14),
                            Suit = (MyPoker.Enumerations.Suits)rand.Next(4)
                        },
                    new MyPoker.Card()
                        {
                            Rank = (MyPoker.Enumerations.Ranks)rand.Next(2, 14),
                            Suit = (MyPoker.Enumerations.Suits)rand.Next(4)
                        }
                    },
                    1_000));
                Task.Delay(1000).Wait();
            }
        }

        private void AddCards()
        {
            switch (Round)
            {
                case MyPoker.Enumerations.Rounds.PreFlop:
                    for (int i = 0; i < 3; i++)
                    {
                        Cards.Add(new MyPoker.Card()
                        {
                            Rank = (MyPoker.Enumerations.Ranks)rand.Next(2, 14),
                            Suit = (MyPoker.Enumerations.Suits)rand.Next(4)
                        });
                        Task.Delay(500).Wait();
                    }
                    break;
                case MyPoker.Enumerations.Rounds.FLop:
                case MyPoker.Enumerations.Rounds.Turn:
                    Cards.Add(new MyPoker.Card()
                    {
                        Rank = (MyPoker.Enumerations.Ranks)rand.Next(2, 14),
                        Suit = (MyPoker.Enumerations.Suits)rand.Next(4)
                    });
                    Task.Delay(500).Wait();
                    break;
                default:
                    break;
            }
        }
        private Random rand = new Random();
        public ReactiveCommand<Unit,Unit> PlayerTurn { get; set; }
        public ReactiveCommand<Unit, Unit> GameProcessCommand { get; set; }
        public ReactiveCommand<Unit, Unit> FoldCommand { get; set; }
    }
   
}
