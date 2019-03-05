using DynamicData.Binding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;


namespace MyPoker
{
    public interface IPlayer
    {
        string Name { get; }
        bool IsGaming { get; set; }
        ObservableCollection<Card> Hand { get; }
        ulong Money { get; }
        bool Blind(ulong blind);
        void TakeMoney(ulong money);
        ulong TakeTurn(ulong currentRate, IEnumerable<Card> OpenCards, out Enumerations.PlayerTurns PlayerTurn, ulong Bank, IEnumerable<ulong> Bets);
    }
}
