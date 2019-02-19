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

        private bool status = true;
        public bool Status
        {
            get => status;
            private set => this.RaiseAndSetIfChanged(ref status, value);
        }
        public RealPlayer(string _name, ObservableCollection<Card> _hand, ulong _money) =>
            (Name, hand, Money) = (_name, _hand, _money);

        public ulong TakeTurn(ulong currentRate, IEnumerable<Card> OpenCards, out Enumerations.PlayerTurns PlayerTurn,ulong Bank, IEnumerable<ulong> Bets)
        {
            Money -= 100;
            PlayerTurn = Enumerations.PlayerTurns.Call;
            return 0;
            //throw new NotImplementedException();
        }
        public void Fold()
            => Status = false;
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

        private bool status = true;
        public bool Status
        {
            get => status;
            private set => this.RaiseAndSetIfChanged(ref status, value);
        }
        public Computer(string _name, ObservableCollection<Card> _hand, ulong _money) =>
            (Name, hand, Money) = (_name, _hand, _money);

        public ulong TakeTurn(ulong currentRate, IEnumerable<Card> OpenCards, out Enumerations.PlayerTurns PlayerTurn, ulong Bank, IEnumerable<ulong> Bets)
        {
            Money -= 10;
            //PlayerTurn = Enumerations.PlayerTurns.Call;
            ulong sum = 0;
            foreach (var item in Bets)
            {
                sum += item;
            }
            var str = OpenCards.Count() == 0 ? "" : String.Join(" ", OpenCards.Select(s => s));
            PlayerTurn =(Enumerations.PlayerTurns)
            Bot.action(
                String.Join(" ", Hand[0], Hand[1]),
                str,
                Bank + sum,
                10,
                10,
                Bets.Count()
                );
            return 10UL;
            //throw new NotImplementedException();
        }
        public void Fold()
            => Status = false;
    }
}
