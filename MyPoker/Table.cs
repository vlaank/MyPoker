using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;

namespace MyPoker
{
    public class Table : ITable
    {
        public ObservableCollection<Card> Cards { get; private set; }

        public ObservableCollection<IPlayer> Players { get; private set; }

        public ObservableCollection<Enumerations.PlayerTurns> PlayerTurns { get; private set; }

        public ObservableCollection<ulong> PlayerBets { get; private set; }

        private bool isPlayerTurn;
        public bool IsPlayerTurn
        {
            get => isPlayerTurn;
            private set => this.RaiseAndSetIfChanged(ref isPlayerTurn, value);
        }

        private Enumerations.Rounds round;
        public Enumerations.Rounds Round
        {
            get => round;
            private set => this.RaiseAndSetIfChanged(ref round, value);
        }

        private ulong bank;
        public ulong Bank
        {
            get => bank;
            private set => this.RaiseAndSetIfChanged(ref bank, value);
        }

        private bool isGameOn;
        public bool IsGameOn
        {
            get => isGameOn;
            private set => this.RaiseAndSetIfChanged(ref isGameOn,value);
        }

        public ReactiveCommand<Unit, Unit> GameStartCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> GameStopCommand { get; private set; }

        public ReactiveCommand<Unit, Unit> CallCommand { get; private set; }

        public ReactiveCommand<Unit, Unit> RaiseCommand { get; private set; }

        public ReactiveCommand<Unit, Unit> FoldCommand { get; private set; }

        private Random rand = new Random();
        public Table()
        {
            Cards = new ObservableCollection<Card>();
            Players = new ObservableCollection<IPlayer>();
            PlayerTurns = new ObservableCollection<Enumerations.PlayerTurns>();
            PlayerBets = new ObservableCollection<ulong>();
            IsGameOn = false;

            GameStartCommand = ReactiveCommand.CreateFromTask(
                async () => 
                await Task.Run(
                    () => 
                    GameProcess()
                    ));

            GameStopCommand = ReactiveCommand.CreateFromTask(
                async () =>
                await Task.Run(
                    () => 
                    {
                        IsGameOn = false;
                    }));

            var canExecute = this.WhenAny(
                table => table.IsPlayerTurn,
                isPTurn => isPTurn.Value
                )
                .ObserveOnDispatcher();

            FoldCommand = ReactiveCommand.CreateFromTask(
                async () => 
                await Task.Run(
                    () => 
                    {
                        ((RealPlayer)Players[0]).Fold();
                        PlayerTurns[0] = Enumerations.PlayerTurns.Fold;
                        IsPlayerTurn = false;
                    }),
                canExecute);
            CallCommand = ReactiveCommand.CreateFromTask(
                async () =>
                await Task.Run(
                    () =>
                    {
                        PlayerTurns[0] = Enumerations.PlayerTurns.Call;
                        IsPlayerTurn = false;
                    }),
                canExecute);
            RaiseCommand = ReactiveCommand.CreateFromTask(
                async () =>
                await Task.Run(
                    () =>
                    {
                        PlayerTurns[0] = Enumerations.PlayerTurns.Raise;
                        IsPlayerTurn = false;
                    }),
                canExecute);
        }
        public void GameProcess()
        {
            Cards.Clear();
            Players.Clear();
            PlayerTurns.Clear();
            PlayerBets.Clear();
            Round = 0;
            Bank = 0UL;

            for(int i =0; i < 4; i++)
            {
                PlayerBets.Add(0UL);
                PlayerTurns.Add(Enumerations.PlayerTurns.Wait);
            }
            IsGameOn = true;
            //AddPlayers();

            //await Task.Run(() =>
            //{
            while (isGameOn)
            {
                Round = 0;
                Bank = 0UL;
                Cards.Clear();
                ResetPlayerTurns();
                AddPlayers();
                while (Round != Enumerations.Rounds.End)
                {
                    PlayersBet();
                    Task.Delay(1000).Wait();
                    ResetPlayerTurns();
                    AddCards();
                    Round++;
                }
            }
            // });
        }

        private void AddPlayers()
        {
            Players.Clear();
            Task.Delay(500).Wait();

            Players.Add(new RealPlayer("Me", new ObservableCollection<Card>(new[] { new Card(), new Card() })
                ,
                    1_000));
            Task.Delay(1000).Wait();


            for (int i = 0; i < 3; i++)
            {
                Players.Add(new Computer("Player", new ObservableCollection<Card>(new[] { new Card(), new Card() }),
                    1_000));

                Task.Delay(1000).Wait();
            }

            for(int i =0; i< 4;i++)
            {
                for(int j=0; j < 2; j++)
                {
                    Players[i].Hand[j] = new Card()
                    {
                        Rank = (Enumerations.Ranks)rand.Next(2, 14),
                        Suit = (Enumerations.Suits)rand.Next(4)
                    };
                    Task.Delay(500).Wait();
                }
                    Task.Delay(500).Wait();
            }
        }

        private void PlayersBet()
        {
            for (int i = 0; i < Players.Count();i++)
            {
                if(PlayerTurns[i] != Enumerations.PlayerTurns.Fold)
                {
                    if (Players[i] is RealPlayer realPlayer && realPlayer.Status)
                        IsPlayerTurn = true;
                    if (IsPlayerTurn)
                        while (IsPlayerTurn) ;
                    else
                    {
                        PlayerBets[i] = Players[i].TakeTurn(100, Cards, out Enumerations.PlayerTurns playerTurn, Bank, PlayerBets);
                        PlayerTurns[i] = playerTurn;
                    }
                    Task.Delay(1000).Wait();
                }
            }
        }

        private void ResetPlayerTurns()
        {
            for (int i = 0; i < Players.Count(); i++)
            {
                //if(PlayerTurns[i] != Enumerations.PlayerTurns.Fold)
                    PlayerTurns[i]= Enumerations.PlayerTurns.Wait;
                Bank += PlayerBets[i];
                PlayerBets[i] = 0UL;
            }   
        }

        private void AddCards()
        {
            switch (Round)
            {
                case Enumerations.Rounds.PreFlop:
                    for (int i = 0; i < 3; i++)
                    {
                        Cards.Add(new Card()
                        {
                            Rank = (Enumerations.Ranks)rand.Next(2, 14),
                            Suit = (Enumerations.Suits)rand.Next(4)
                        });
                        Task.Delay(500).Wait();
                    }
                    break;
                case Enumerations.Rounds.FLop:
                case Enumerations.Rounds.Turn:
                    Cards.Add(new Card()
                    {
                        Rank = (Enumerations.Ranks)rand.Next(2, 14),
                        Suit = (Enumerations.Suits)rand.Next(4)
                    });
                    Task.Delay(500).Wait();
                    break;
                default:
                    break;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(PropertyChangedEventArgs args)
        {
            PropertyChanged?.Invoke(this, args);
        }
        public event ReactiveUI.PropertyChangingEventHandler PropertyChanging;
        public void RaisePropertyChanging(ReactiveUI.PropertyChangingEventArgs args)
        {
            PropertyChanging?.Invoke(this, args);
        }
    }
}
