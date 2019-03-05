using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
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

        public ObservableCollection<string> Logs { get; } = new ObservableCollection<string>();

        private ulong smallBlind = 5;
        private ulong bigBlind = 10;

        private int BlindIndex;

        private ulong curbet;
        private ulong curBet
        {
            get => curbet;
            set => this.RaiseAndSetIfChanged(ref curbet, value);
        }

        private ulong minRaise;
        public ulong MinRaise
        {
            get => minRaise;
            set => this.RaiseAndSetIfChanged(ref minRaise, value);
        }
        private ulong maxRaise;
        public ulong MaxRaise
        {
            get => maxRaise;
            set => this.RaiseAndSetIfChanged(ref maxRaise, value);
        }
        private ulong playerBet;
        public ulong PlayerBet
        {
            get => playerBet;
            set => this.RaiseAndSetIfChanged(ref playerBet, value);
        }

        public ReactiveCommand<Unit, Unit> GameStartCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> GameStopCommand { get; private set; }

        public ReactiveCommand<Unit, Unit> CallCommand { get; private set; }

        public ReactiveCommand<Unit, Unit> RaiseCommand { get; private set; }

        public ReactiveCommand<Unit, Unit> FoldCommand { get; private set; }

        SynchronizationContext uiContext;

        private Random rand = new Random();
        public Table()
        {
            Cards = new ObservableCollection<Card>();
            Winner = new ObservableCollection<bool>();
            Players = new ObservableCollection<IPlayer>();
            PlayerTurns = new ObservableCollection<Enumerations.PlayerTurns>();
            PlayerBets = new ObservableCollection<ulong>();
            IsGameOn = false;

            this.ObservableForProperty(t => t.curBet)
                .Subscribe(_ => { PlayerBet = MinRaise = curBet == 0 ? 1 : Math.Min(2 * curBet, Players[0].Money); MaxRaise = Players.Count() > 0 ? Players[0].Money - PlayerBets[0] : 0UL;});

            uiContext = SynchronizationContext.Current;

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
                        uiContext.Send(x => Logs.Add($"------- {Players[0].Name} - Folds  -------"), null);
                        IsPlayerTurn = false;
                    }),
                canExecute);
            CallCommand = ReactiveCommand.CreateFromTask(
                async () =>
                await Task.Run(
                    () =>
                    {
                        if (curBet - PlayerBets[0] != 0)
                        {
                            PlayerTurns[0] = Enumerations.PlayerTurns.Call;
                            Players[0].Blind(curBet - PlayerBets[0]);
                            PlayerBets[0] += curBet - PlayerBets[0];
                        }
                        else PlayerTurns[0] = Enumerations.PlayerTurns.Check;

                        uiContext.Send(x => Logs.Add($"------- {Players[0].Name} - {PlayerTurns[0]}s  : {curBet}$ -------"), null);

                        IsPlayerTurn = false;
                    }),
                canExecute);
            RaiseCommand = ReactiveCommand.CreateFromTask(
                async () =>
                await Task.Run(
                    () =>
                    {
                        PlayerTurns[0] = Enumerations.PlayerTurns.Raise;
                        curBet = PlayerBet;
                        Players[0].Blind(curBet - PlayerBets[0]);
                        PlayerBets[0] += curBet - PlayerBets[0];
                        uiContext.Send(x => Logs.Add($"------- {Players[0].Name} - Raises : {curBet}$ -------"), null);
                        IsPlayerTurn = false;
                    }),
                canExecute);
        }

        public void GameProcess()
        {
            int game = 1;
            IsGameOn = false;
            uiContext.Send(x => Logs.Clear(), null);
            AddPlayers();
            ResetGame();
            Task.Delay(1000).Wait();

            IsGameOn = true;
            while (isGameOn)
            {
                uiContext.Send(x=> Logs.Add($"======= Game {game++} ======="), null);
                Task.Delay(1000).Wait();
                ResetGame();
                Task.Delay(2000).Wait();
                for (int i = 0; i < 4; i++)
                {
                    if (Players.Sum(p => p.IsGaming ? 1 : 0) == 1)
                    {
                        var Winner = Players.Where(player => player.IsGaming).First();
                        uiContext.Send(x => Logs.Add($"======= {Winner.Name} is WINNING! TOTAL : {Winner.Money}$ ======="), null);
                        isGameOn = false; break;
                    }
                    for (int j = 0; j < 2; j++)
                    {
                        Players[i].Hand[j] = TakeCard();
                        if (i == 0) Task.Delay(500).Wait();
                    }
                }
                while (isGameOn && Round != Enumerations.Rounds.End)
                {
                    uiContext.Send(x=> Logs.Add($"####### {Round} #######"), null);
                    PlayersBet();
                    Task.Delay(1000).Wait();
                    if (PlayerTurns.Sum(turn => turn != Enumerations.PlayerTurns.Fold ? 1 : 0) == 1) break;
                    ResetPlayerTurns(Round + 1);
                    AddCards();
                    Round++;
                }
                GetWinner();
            }
        }

        private void GetWinner()
        {
            if (!IsGameOn) return;
            Round = Enumerations.Rounds.End;
            int winner_i = 0;

            for (int i = 0; i < Players.Count(); i++)
            {
                Bank += PlayerBets[i];
                PlayerBets[i] = 0UL;
            }
            Task.Delay(1000).Wait();
            if(Round != Enumerations.Rounds.End || Cards.Count < 5)
            {
                if(PlayerTurns.Sum(t => t == Enumerations.PlayerTurns.Fold ? 1 : 0) != 4)
                {
                    for(int i =0; i <4; i++)
                        if (PlayerTurns[i]!= Enumerations.PlayerTurns.Fold)
                        {
                            winner_i = i;break;
                        }
                    //winner_i = PlayerTurns.Select((t, i) => (t, i)).Where(x => x.ToTuple().Item1 != Enumerations.PlayerTurns.Fold).First().ToTuple().Item2;
                    Winner[winner_i] = true;
                    uiContext.Send(x => Logs.Add($"------- {Players[winner_i].Name} - Wins   : {Bank}$ -------"), null);

                    Task.Delay(3000).Wait();
                    Players[winner_i].TakeMoney(Bank);
                    Bank = 0Ul;

                }
                return;
            }
            var WinnersCombination = Players
                .Select((player, i) => (new HandEvaluator(Cards, player.Hand).EvaluateHand(), i))
                .Where(t => PlayerTurns[t.ToTuple().Item2] != Enumerations.PlayerTurns.Fold)
                .OrderByDescending(t => t.Item1)
                .GroupBy(t => t.Item1)
                .First();
             var PotentialWinners = WinnersCombination.Select(x=>x.ToTuple().Item2);


            (Enumerations.Ranks, Enumerations.Ranks) TMax = (Enumerations.Ranks.Two, Enumerations.Ranks.Two);
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
            var comb = PotentialWinners.Count() == 1 ? WinnersCombination.First().ToTuple().Item1.combination.ToString() : TMax.ToString();
            Winner[winner_i] = true;
            uiContext.Send(x => Logs.Add($"------- {Players[winner_i].Name} - Wins   : {Bank}$ -------"), null);
            uiContext.Send(x => Logs.Add($"------- With {comb}-------"), null);
            Task.Delay(3000).Wait();
            Players[winner_i].TakeMoney(Bank);
            Bank = 0Ul;
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
            Bank = curBet = 0UL;
            MinRaise = MaxRaise = PlayerBet = 0UL;
            BlindIndex = rand.Next(0, 3);
            Cards.Clear();
            ResetPlayerTurns(Round);
            FillDeck();
            for (int i = 0; i < 4; i++)
            {
                if (Players[i].IsGaming)
                {
                    if (Players[i].Money < bigBlind)
                    {
                        Players[i].IsGaming = false;
                        PlayerTurns[i] = Enumerations.PlayerTurns.Fold;
                        uiContext.Send(x => Logs.Add($"------- {Players[i].Name} - Stands Up -------"), null);
                    }
                    else
                        for (int j = 0; j < 2; j++)
                        {
                            Players[i].Hand[j] = new Card();
                        }
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
                50
                ));
            PlayerBets.Add(0UL);
            PlayerTurns.Add(Enumerations.PlayerTurns.Wait);
            Winner.Add(false);

            for (int i = 0; i < 3; i++)
            {
                Players.Add(
                    new Computer($"Computer{i+1}", new ObservableCollection<Card>(new[] { new Card(), new Card() }),
                    50
                    ));
                PlayerBets.Add(0UL);
                PlayerTurns.Add(Enumerations.PlayerTurns.Wait);
                Winner.Add(false);
            }

        }

        private void PlayersBet()
        {
            int numOfPlayers = Players.Sum(player => player.IsGaming ? 1 : 0);
            var active = PlayerTurns.Select((t, k) => t != Enumerations.PlayerTurns.Fold ? k : -1).Where(s => s >= 0);
            IEnumerable<int> ActivePlayers = Players.Select((x,j) => active.Contains(j) ? j : -1).Where(s => s >=0);
            curBet = 0UL;
            if(Round == Enumerations.Rounds.PreFlop)
            {
                ActivePlayers = Players.Select((t, j) => Players.Where(p => p.IsGaming).Contains(t) ? j : -1).Where(j => j >= 0);
                if (ActivePlayers.Count() < 2) return;
                int k = 0;
                foreach (var item in ActivePlayers)
                {
                    if (item != BlindIndex)
                        k++;
                    else break;
                }
                var z = ActivePlayers.Skip(k);
                ActivePlayers = z.Concat(ActivePlayers.Take(k));
                int i = ActivePlayers.First();
                Players[i].Blind(smallBlind);
                PlayerBets[i] = smallBlind;
                uiContext.Send(x => Logs.Add($"------- {Players[i].Name} - SmallBlind : {smallBlind}$ -------"), null);

                Task.Delay(1500).Wait();

                i = ActivePlayers.ElementAt(1);
                Players[i].Blind(bigBlind);
                PlayerBets[i] = bigBlind;
                uiContext.Send(x => Logs.Add($"------- {Players[i].Name} - BigBlind   : {bigBlind}$ -------"), null);

                curBet = bigBlind;
                Task.Delay(1500).Wait();

                foreach (int j in ActivePlayers.Skip(2))
                {
                    if (PlayerTurns.Sum(t => t == Enumerations.PlayerTurns.Fold ? 1 : 0) > ActivePlayers.Count() - 1)
                        return;
                    if (PlayerTurns[j] != Enumerations.PlayerTurns.Fold)
                    {
                        if (Players[j] is RealPlayer realPlayer)
                            IsPlayerTurn = true;
                        if (IsPlayerTurn)
                            while (IsPlayerTurn) { if (!IsGameOn) { IsPlayerTurn = false; return; } }
                        else
                        {
                            var Bet = Players[j].TakeTurn(curBet - PlayerBets[j], Cards, out Enumerations.PlayerTurns playerTurn, Bank, PlayerBets);
                            PlayerTurns[j] = playerTurn;
                            if (playerTurn != Enumerations.PlayerTurns.Fold)
                            {
                                if (playerTurn == Enumerations.PlayerTurns.Raise)
                                    curBet = Bet;
                                PlayerBets[j] += Bet;
                                uiContext.Send(x => Logs.Add($"------- {Players[j].Name} - {PlayerTurns[j]}s : {curBet}$ -------"), null);
                            }
                            else
                            {
                                uiContext.Send(x => Logs.Add($"------- {Players[j].Name} - Folds  -------"), null);
                                ActivePlayers = ActivePlayers.Where((t, jj) => jj != j);
                            }
                        }
                        Task.Delay(1500).Wait();
                    }
                        
                }
            }
            while (
                PlayerTurns.Sum(t => t == Enumerations.PlayerTurns.Wait ? 1 : 0) > 0 || 
                PlayerTurns.Sum(Turn => Turn == Enumerations.PlayerTurns.Raise ? 1 : 0) > 0
                )
            {
                foreach (int i in ActivePlayers)
                {
                    if (PlayerTurns.Sum(t => t != Enumerations.PlayerTurns.Fold ? 1 : 0) ==1)
                        return;
                    if (PlayerTurns[i] == Enumerations.PlayerTurns.Raise && PlayerTurns.Sum(t => t == Enumerations.PlayerTurns.Raise ? 1 : 0) == 1)
                        return;
                    if (PlayerTurns.Sum(t => t == Enumerations.PlayerTurns.Call || t == Enumerations.PlayerTurns.Check ? 1 : 0) == ActivePlayers.Count())
                        return;
                    if (PlayerTurns[i] != Enumerations.PlayerTurns.Fold)
                    {
                        if (Players[i] is RealPlayer realPlayer)
                            IsPlayerTurn = true;
                        if (IsPlayerTurn)
                            while (IsPlayerTurn) { if (!IsGameOn) { IsPlayerTurn = false; return; } }
                        else
                        {
                            var Bet = Players[i].TakeTurn(curBet - PlayerBets[i], Cards, out Enumerations.PlayerTurns playerTurn, Bank, PlayerBets);
                            PlayerTurns[i] = playerTurn;
                            if (playerTurn != Enumerations.PlayerTurns.Fold)
                            {
                                if (playerTurn == Enumerations.PlayerTurns.Raise)
                                    curBet = Bet;
                                PlayerBets[i] += Bet;
                                uiContext.Send(x => Logs.Add($"------- {Players[i].Name} - {PlayerTurns[i]}s : {curBet}$ -------"), null);
                            }
                            else
                            {
                                uiContext.Send(x => Logs.Add($"------- {Players[i].Name} - Folds  -------"), null);
                                ActivePlayers = ActivePlayers.Where((t, k) => k != i);
                            }
                        }
                        Task.Delay(1500).Wait();
                    }
                }

            }
        }

        private void ResetPlayerTurns(Enumerations.Rounds Round)
        {
            for (int i = 0; i < Players.Count(); i++)
            {
                if(Players[i].IsGaming && Round == Enumerations.Rounds.PreFlop)
                    PlayerTurns[i] = Enumerations.PlayerTurns.Wait;
                else if (Players[i].IsGaming && PlayerTurns[i] != Enumerations.PlayerTurns.Fold)
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
