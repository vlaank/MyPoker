using System.Collections.Generic;
using System.Linq;

namespace MyPoker
{
    public enum Combinations
    {
        Nothing,
        OnePair,
        TwoPairs,
        ThreeKind,
        Straight,
        Flush,
        FullHouse,
        FourKind
    }
    public class HandEvaluator
    {
        private Card[] Cards;

        public int Total;

        public HandEvaluator(IEnumerable<Card> _open, IEnumerable<Card> _hand)
        {
            var cards = _open.Concat(_hand);
            Cards = cards.OrderByDescending(t => t.Rank).ToArray();
        }

        public (Combinations combination, int totalValue) EvaluateHand()
        {
            if (FourOfKind())
                return (Combinations.FourKind,Total);
            else if (FullHouse())
                return (Combinations.FullHouse,Total);
            else if (Flush())
                return (Combinations.Flush,Total);
            else if (Straight())
                return (Combinations.Straight,Total);
            else if (ThreeOfKind())
                return (Combinations.ThreeKind,Total);
            else if (TwoPairs())
                return (Combinations.TwoPairs,Total);
            else if (OnePair())
                return (Combinations.OnePair,Total);

            return (Combinations.Nothing,0);
        }
        private bool FourOfKind()
        {
            var z = Cards.GroupBy(x => x.Rank).Where(x => x.Count() == 4);
            if (z.Count() > 0)
            {
                Total = (int)z.First().First().Rank * 4;
                return true;
            }
            return false;
        }
        private bool FullHouse()
        {
            for (int i = 0; i < 2; i++)
                if (Cards[i].Rank == Cards[i + 1].Rank && Cards[i].Rank == Cards[i + 2].Rank)
                {
                    for (int j = i + 3; j < 6; j++)
                        if (Cards[j].Rank == Cards[j + 1].Rank)
                        {
                            Total = (int)Cards[i].Rank * 3 + (int)Cards[j].Rank * 2;
                            return true;
                        }
                }
            for (int i = 0; i < 4; i++)
                if (Cards[i].Rank == Cards[i + 1].Rank)
                {
                    for (int j = i + 2; j < 5; j++)
                        if (Cards[j].Rank == Cards[j + 1].Rank && Cards[j].Rank == Cards[j + 2].Rank)
                        {
                            Total = (int)Cards[j].Rank * 3 + (int)Cards[i].Rank * 2;
                            return true;
                        }
                }
            return false;
        }
        private bool Flush()
        {
            var z = Cards.GroupBy(x => x.Suit).Where(x => x.Count() > 4).Take(1);
            if (z.Count() > 0)
            {
                Total = Cards.Where(x => x.Suit == z.First().Key).Take(5).Sum(t => (int)t.Rank);
                return true;
            }
            return false;

        }
        private bool Straight()
        {
            for (int i = 0; i < 3; i++)
            {
                bool flag = true;
                for (int j = i; j < i + 4; j++)
                    if (Cards[j].Rank - 1 != Cards[j + 1].Rank)
                    {
                        flag = false; break;
                    }
                if (flag)
                {
                    Total = Cards.Skip(i).Take(5).Sum(x => (int)x.Rank);
                    return true;
                }
            }

            return false;
        }
        private bool ThreeOfKind()
        {
            for (int i = 0; i < 4; i++)
                if (Cards[i].Rank == Cards[i + 1].Rank && Cards[i].Rank == Cards[i + 2].Rank)
                {
                    Total = (int)Cards[i].Rank * 3;
                    return true;
                }

            return false;
        }
        private bool TwoPairs()
        {
            int count = 0;
            for (int i = 0; count < 2 && i < 6; i++)
                if (Cards[i].Rank == Cards[i + 1].Rank)
                {
                    Total += (int)Cards[i].Rank * 2;
                    i++; count++;
                }
            return count == 2 ? true : false;
        }
        private bool OnePair()
        {
            for (int i = 0; i < 6; i++)
                if (Cards[i].Rank == Cards[i + 1].Rank)
                {
                    Total = (int)Cards[i].Rank * 2;
                    return true;
                }

            return false;
        }

    }
}
