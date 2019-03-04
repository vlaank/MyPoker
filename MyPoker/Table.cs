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
        private readonly Card?[] deck = new Card?[52];
        public ObservableCollection<Card> Cards { get; private set; }

        public ObservableCollection<IPlayer> Players { get; private set; }

        public ObservableCollection<Enumerations.PlayerTurns> PlayerTurns { get; private set; }

        public ObservableCollection<ulong> PlayerBets { get; private set; }

        public ObservableCollection<bool> Winner { get; private set; }

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
            Winner = new ObservableCollection<bool>();
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
            IsGameOn = false;
            Task.Delay(1000).Wait();
            AddPlayers();
            ResetGame();
            Task.Delay(1000).Wait();

            IsGameOn = true;
            while (isGameOn)
            {
                Task.Delay(1000).Wait();
                ResetGame();
                Task.Delay(2000).Wait();
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        Players[i].Hand[j] = TakeCard();
                        Task.Delay(500).Wait();
                    }
                }
                while (isGameOn && Round != Enumerations.Rounds.End)
                {
                    while (PlayerTurns.Where(Turn => Turn == Enumerations.PlayerTurns.Wait).Count() != 0 || PlayerTurns.Where(Turn => Turn == Enumerations.PlayerTurns.Raise).Count() > 1)
                        PlayersBet();
                    Task.Delay(1000).Wait();
                    if (PlayerTurns.Where(turn => turn == Enumerations.PlayerTurns.Fold).Count() >= 3) break;
                    ResetPlayerTurns(Round + 1);
                    AddCards();
                    Round++;
                }
                GetWinner();
                Task.Delay(1000).Wait();
            }
        }

        private void GetWinner()
        {
            if(Cards.Count < 5 )
            {
                if(PlayerTurns.Where(t => t == Enumerations.PlayerTurns.Fold).Count() != 4)
                    Winner[PlayerTurns.Select((t, i) => (t, i)).Where(x => x.ToTuple().Item1 != Enumerations.PlayerTurns.Fold).First().ToTuple().Item2] = true;
                return;
            }
            var PotentialWinners = Players
                .Select((player, i) => (new HandEvaluator(Cards, player.Hand).EvaluateHand(), i))
                .Where(t => PlayerTurns[t.ToTuple().Item2] != Enumerations.PlayerTurns.Fold)
                .OrderByDescending(t => t.Item1)
                .GroupBy(t => t.Item1)
                .First().Select(x=>x.ToTuple().Item2);


            (Enumerations.Ranks, Enumerations.Ranks) TMax = (Enumerations.Ranks.Two, Enumerations.Ranks.Two);
            int winner_i = 0;
            foreach (int i in PotentialWinners)
            {
                var T = Players[i].Hand[0].Rank > Players[i].Hand[1].Rank ?
                    (Players[i].Hand[0].Rank, Players[i].Hand[1].Rank) :
                    (Players[i].Hand[1].Rank, Players[i].Hand[0].Rank);
                    
                if(T.Item1 > TMax.Item1)
                {
                    TMax = T;
                    winner_i = i;
                }
                else if(T.Item1 == TMax.Item1)
                {
                    if(T.Item2 >= TMax.Item2)
                    {
                        TMax.Item2 = T.Item2;
                        winner_i = i;
                    }
                }
            }
            Winner[winner_i] = true;
        }

        private void FillDeck()
        {
            int k = 0;
            for (int i = 2; i < 15; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    deck[k++] = new Card() { Suit = (Enumerations.Suits)j, Rank = (Enumerations.Ranks)i };
                }
            }
        }

        private void ResetGame()
        {
            Round = 0;
            Bank = 0UL;
            Cards.Clear();
            ResetPlayerTurns(Round);
            FillDeck();
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    Players[i].Hand[j] = new Card();
                }
            }
        }
        private void AddPlayers()
        {
            Players.Clear();
            PlayerTurns.Clear();
            PlayerBets.Clear();

            Players.Add(
                new RealPlayer("Me", new ObservableCollection<Card>(new[] { new Card(), new Card() }),
                1_000
                ));
            PlayerBets.Add(0UL);
            PlayerTurns.Add(Enumerations.PlayerTurns.Wait);
            Winner.Add(false);

            for (int i = 0; i < 3; i++)
            {
                Players.Add(
                    new Computer("Player", new ObservableCollection<Card>(new[] { new Card(), new Card() }),
                    1_000
                    ));
                PlayerBets.Add(0UL);
                PlayerTurns.Add(Enumerations.PlayerTurns.Wait);
                Winner.Add(false);
            }

        }

        private void PlayersBet()
        {
            for (int i = 0; i < Players.Count();i++)
            {
                if(PlayerTurns[i] != Enumerations.PlayerTurns.Fold)
                {
                    if (Players[i] is RealPlayer realPlayer)
                         IsPlayerTurn = true;
                     if (IsPlayerTurn)
                        while (IsPlayerTurn) ;
                    //if (false) ;
                    else
                    {
                        PlayerBets[i] = Players[i].TakeTurn(100, Cards, out Enumerations.PlayerTurns playerTurn, Bank, PlayerBets);
                        PlayerTurns[i] = playerTurn;
                    }
                    Task.Delay(1000).Wait();
                }
            }
        }

        private void ResetPlayerTurns(Enumerations.Rounds Round)
        {
            for (int i = 0; i < Players.Count(); i++)
            {
                if(Round == Enumerations.Rounds.PreFlop)
                    PlayerTurns[i] = Enumerations.PlayerTurns.Wait;
                else if (PlayerTurns[i] != Enumerations.PlayerTurns.Fold)
                    PlayerTurns[i]= Enumerations.PlayerTurns.Wait;

                Bank += PlayerBets[i];
                PlayerBets[i] = 0UL;
                Winner[i] = false;
            }   
        }

        private void AddCards()
        {
            switch (Round)
            {
                case Enumerations.Rounds.PreFlop:
                    for (int i = 0; i < 3; i++)
                    {
                        Cards.Add(TakeCard());
                        Task.Delay(500).Wait();
                    }
                    break;
                case Enumerations.Rounds.FLop:
                case Enumerations.Rounds.Turn:
                    Cards.Add(TakeCard());
                    Task.Delay(500).Wait();
                    break;
                default:
                    break;
            }
        }

        private Card TakeCard()
        {
            int k1, k2;
            Card? DeletedCard;
            do
            {
                k1 = rand.Next(0, 4);
                k2 = rand.Next(0, 13);
                DeletedCard = deck[k1 + 4 * k2];
            } while (!DeletedCard.HasValue);

            deck[k1 + 4 * k2] = null;
            return DeletedCard.Value;
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
