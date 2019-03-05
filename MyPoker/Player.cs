using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicData.Binding;
using ReactiveUI;
namespace MyPoker
{
    public class RealPlayer : ReactiveObject, IPlayer
    {
        private string name;
        public string Name
        {
            get => name;
            private set
            {
                name = value;
            }
        }
        private bool isGaming = true;
        public bool IsGaming
        {
            get => isGaming;
            set => this.RaiseAndSetIfChanged(ref isGaming, value);
        }
        private ObservableCollection<Card> hand = new ObservableCollection<Card>(new List<Card>(2));
        public ObservableCollection<Card> Hand
        {
            get => hand;
            private set
            {
                hand[0] = value[0];
                hand[1] = value[1];
            }
        }
        private ulong money;
        public ulong Money
        {
            get => money;
            private set => this.RaiseAndSetIfChanged(ref money, value);
        }

        public RealPlayer(string _name, ObservableCollection<Card> _hand, ulong _money) =>
            (Name, hand, Money) = (_name, _hand, _money);

        public ulong TakeTurn(ulong currentRate, IEnumerable<Card> OpenCards, out Enumerations.PlayerTurns PlayerTurn,ulong Bank, IEnumerable<ulong> Bets)
        {
            PlayerTurn = Enumerations.PlayerTurns.Wait;
            return 0UL;
        }
        public bool Blind(ulong blind)
        {
            if (Money < blind)
                return false;

            Money -= blind;
            return true;
        }
        public void TakeMoney(ulong bank)
        {
            Money += bank;
            this.RaisePropertyChanged("money");
        }
    }
    public class Computer: ReactiveObject, IPlayer
    {
        private string name;
        public string Name
        {
            get => name;
            private set
            {
                name = value;
            }
        }
        private bool isGaming = true;
        public bool IsGaming
        {
            get => isGaming;
            set => this.RaiseAndSetIfChanged(ref isGaming, value);
        }

        private ObservableCollection<Card> hand;
        public ObservableCollection<Card> Hand
        {
            get => hand;
            private set
            {
                hand[0] = value[0];
                hand[1] = value[1];
            }
        }
        private ulong money;
        public ulong Money
        {
            get => money;
            private set => this.RaiseAndSetIfChanged(ref money, value);
        }

        public Computer(string _name, ObservableCollection<Card> _hand, ulong _money) =>
            (Name, hand, Money) = (_name, _hand, _money);

        public ulong TakeTurn(ulong currentRate, IEnumerable<Card> OpenCards, out Enumerations.PlayerTurns PlayerTurn, ulong Bank, IEnumerable<ulong> Bets)
        {
            if (Money < currentRate)
            {
                PlayerTurn = Enumerations.PlayerTurns.Fold;
                return 0UL;
            }
            ulong sum = 0;
            foreach (var item in Bets){
                sum += item;
            }
            var str = OpenCards.Count() == 0 ? "" : String.Join(" ", OpenCards.Select(s => s));
            PlayerTurn =(Enumerations.PlayerTurns)
            Bot.action(
                String.Join(" ", Hand[0], Hand[1]),
                str,
                Bank + sum,
                Convert.ToInt32(currentRate),
                Convert.ToInt32(2 * currentRate),
                Bets.Count()
                );
            if(PlayerTurn == Enumerations.PlayerTurns.Call)
            {
                Money -= currentRate;
                return currentRate;
            }
            else if(PlayerTurn == Enumerations.PlayerTurns.Raise)
            {
                if(Money < 2 * currentRate)
                {
                    currentRate = Money;
                }
                else currentRate *= 2;
                Money -= currentRate;
                return currentRate;
            }
            return 0UL;
        }
        public bool Blind(ulong blind)
        {
            if (Money < blind)
                return false;

            Money -= blind;
            return true;
        }
        public void TakeMoney(ulong bank)
        {
            Money += bank;
        }
    }
}
