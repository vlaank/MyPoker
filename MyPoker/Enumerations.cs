using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPoker
{
    public static class Enumerations
    {
        //Пики, Черви, Буби, Крести
        public enum Suits { Spade, Heart, Diamond, Club}
        public enum Ranks { Two=2, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Quuen, King, Ace}
        public enum Rounds { PreFlop, FLop, Turn, River, End}
        public enum PlayerTurns { Wait, Call, Fold, Check, Raise, AllIn}
    }
    public struct Card
    {
        public Enumerations.Ranks Rank;
        public Enumerations.Suits Suit;
        public string Path
        {
            get => String.Join("", "Resources/Images/", $"{char.ToLower(Suit.ToString()[0])}{(int)Rank:00}.bmp");
        }

        public override string ToString()
        {
            char c;
            if (Rank < Enumerations.Ranks.Ten)
                c = ((int)Rank).ToString()[0];
            else c = Rank.ToString()[0];
            return string.Join("", c, char.ToLower(Suit.ToString()[0]));
        }
    }
}
