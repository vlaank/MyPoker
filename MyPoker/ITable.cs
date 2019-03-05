using System;using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;

namespace MyPoker
{
    public interface ITable:IReactiveObject
    {
        ObservableCollection<Card> Cards { get; }
        ObservableCollection<IPlayer> Players { get; }
        ObservableCollection<Enumerations.PlayerTurns> PlayerTurns { get; }
        ObservableCollection<ulong> PlayerBets { get; }

        bool IsPlayerTurn { get; }
        bool IsGameOn { get; }
        ulong MinRaise { get; }
        ulong MaxRaise { get; }
        ulong PlayerBet { get; }
        Enumerations.Rounds Round { get; }
        ulong Bank { get; }

        ObservableCollection<string> Logs { get; }

        ReactiveCommand<Unit, Unit> GameStartCommand { get; }
        ReactiveCommand<Unit, Unit> GameStopCommand { get; }
        ReactiveCommand<Unit,Unit> CallCommand { get; }
        ReactiveCommand<Unit,Unit> RaiseCommand { get; }
        ReactiveCommand<Unit, Unit> FoldCommand { get; }
        void GameProcess();
    }
}
