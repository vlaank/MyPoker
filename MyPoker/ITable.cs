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
        Enumerations.Rounds Round { get; }
        ulong Bank { get; }

        ReactiveCommand<Unit, Unit> GameStartCommand { get; }
        ReactiveCommand<Unit, Unit> GameStopCommand { get; }
        ReactiveCommand<Unit,Unit> CallCommand { get; }
        ReactiveCommand<Unit,Unit> RaiseCommand { get; }
        ReactiveCommand<Unit, Unit> FoldCommand { get; }
        void GameProcess();
    }
}
