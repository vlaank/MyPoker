using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public enum Hand
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
        public int HighCard;

        public HandEvaluator(IEnumerable<Card> _open, IEnumerable<Card> _hand)
        {
            var cards = _open.Concat(_hand);
            Cards = cards.OrderByDescending(t => t.Rank).ToArray();
            //Console.WriteLine(String.Join(" ",Cards.Select(x=>x)));
        }

        public Hand EvaluateHand()
        {
            if (FourOfKind())
                return Hand.FourKind;
            else if (FullHouse())
                return Hand.FullHouse;
            else if (Flush())
                return Hand.Flush;
            else if (Straight())
                return Hand.Straight;
            else if (ThreeOfKind())
                return Hand.ThreeKind;
            else if (TwoPairs())
                return Hand.TwoPairs;
            else if (OnePair())
                return Hand.OnePair;

            return Hand.Nothing;
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
                for(int j = i; j < i + 4; j++)
                    if(Cards[j].Rank - 1 != Cards[j + 1].Rank)
                    {
                        flag = false; break;
                    }
                if (flag)
                {
                    Total = Cards.Skip(i).Take(5).Sum(x => (int)x.Rank);
                    HighCard = (int)Cards[i].Rank;
                    return true;
                }
            }

            return false;
        }
        private bool ThreeOfKind()
        {
            for (int i = 0; i < 4; i++)
                if (Cards[i].Rank == Cards[i + 1].Rank && Cards[i].Rank == Cards[i+2].Rank)
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
            for(int i = 0; i < 6; i++)
                if(Cards[i].Rank == Cards[i + 1].Rank)
                {
                    Total = (int)Cards[i].Rank * 2;
                    return true;
                }

            return false;
        }
           
    }
    public static class Enumerations
    {
        //Пики, Черви, Буби, Крести
        public enum Suits { Spade, Heart, Diamond, Club }
        public enum Ranks { Two = 2, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Quuen, King, Ace }
        public enum Rounds { PreFlop, FLop, Turn, River, End }
        public enum PlayerTurns { Wait, Call, Fold, Check, Raise, AllIn }
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
        public static bool operator !=(Card c1, Card c2) =>
            c1.Rank != c2.Rank && c1.Suit != c2.Suit;
        public static bool operator ==(Card c1, Card c2) =>
            c1.Rank == c2.Rank && c1.Suit == c2.Suit;
    }
    class Program // будут вызываться основные методы класса тасование
    {
        
        static void Main(string[] args)
        {
            Tasovanie tr = new Tasovanie();
            for(int i=0; i < 1_000; i++)
            {
                tr.Go();
                tr.Pere();
                var Hand = new HandEvaluator(tr.OpenCards.Take(5), tr.OpenCards.Skip(5)).EvaluateHand();

                if (Hand != Hand.Nothing)
                {
                    Console.WriteLine(String.Join(" ", tr.OpenCards.OrderByDescending(x => x.Rank)));
                    Console.WriteLine($"{Hand}\n");
                }

            }
            Console.WriteLine("END");
            Console.ReadLine();
        }



    }


    class Tasovanie
    {
        public Card?[] Cards = new Card?[52];
        public Card[] OpenCards = new Card[7];
        Random rnd = new Random();
        string[] masty = new string[4] { "Пик", "Крестей", "Бубей", "Червей" };
        string[] imena = new string[13] { "Двойка", "тройка", "Четверка", "Пятерка", "Шестерка ", "Семерка ", "Восьмерка ", "Девятка ", "Десятка", "Валет", "Дама", "король", "туз" };
        int[,] deck = new int[4, 13];


        public void Go() // метод  запускающий тасование
        {
            int k = 0;
            for (int i = 2; i < 15; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    Cards[k++] =  new Card() { Suit = (Enumerations.Suits)j, Rank = (Enumerations.Ranks)i };
                }
            }
        }
        public void PrintCards()
        {
            //foreach(var card in Cards)
            for (int row = 0; row < 13; row++, Console.WriteLine())
                for (int column = 0; column < 4; column++)
                    Console.Write(Cards.ElementAt(4 * row + column) + " ");
            Console.WriteLine(Cards.Count());
        }
        public void PrintOpenCards()
        {
            Console.WriteLine(String.Join(" ", OpenCards));
        }



        public void Pere() // метод запускающий перетасовку
        {
            for (int i = 0; i < 7; i++)
            {
                int k1, k2;
                Card? DeletedCard;
                do
                {
                    k1 = rnd.Next(0, 4);
                    k2 = rnd.Next(0, 13);
                    DeletedCard = Cards[k1 + 4 * k2];
                } while (!DeletedCard.HasValue);

                OpenCards[i] = DeletedCard.Value;
                Cards[k1 + 4 * k2] = null;
            }
        }
            //int card = 1;


            //for (int row = 0; row < 13; row++)
            //{
            //    Console.WriteLine();
            //    Console.WriteLine();
            //    for (int column = 0; column < 4; column++)
            //    {
            //        deck[column, row] = card;
            //        Console.WriteLine("{0} {1}  - {2}", masty[column], imena[row], deck[column, row]);
            //        card++;
            //    }
            //}

            //for (int row = 0; row < 13; row++)
            //{
            //    int rnd1 = rnd.Next(0, 4);
            //    int rnd2 = rnd.Next(0, 13);
            //    int temp = 0;

            //    Console.WriteLine();
            //    Console.WriteLine();
            //    for (int column = 0; column < 4; column++)
            //    {
            //        temp = deck[column, row];
            //        deck[column, row] = deck[rnd1, rnd2];
            //        deck[rnd1, rnd2] = temp;
            //        Console.WriteLine("{0} {1}  - {2}", masty[column], imena[row], deck[column, row]);
            //        card++;
            //    }

            //}


        //public void Razd()
        //{
        //    for (int card = 1; card <= 52; card++)
        //    {
        //        if (card % 4 == 0)
        //            Console.WriteLine("\n");
        //        else
        //            Console.WriteLine("--------------------------");
        //        for (int i = 0; i < 4; i++)
        //        {

        //            for (int j = 0; j < 13; j++)
        //            {
        //                if (deck[i, j] == card)
        //                    Console.WriteLine("{0} {1}", masty[i], imena[j]);

        //            }

        //        }
        //    }
        //} // метод раздача - тасует и раздает карты


    }
}
