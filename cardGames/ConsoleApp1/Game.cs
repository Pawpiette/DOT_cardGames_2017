﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkCommsDotNet.Connections;
using Protocol;

namespace cardGame_Server
{
    class Game
    {
        private static int NbCard = 52;
        private static int maxNbPlayers = 2;
        private int nbTurn { get; set; }

        private List<Client> players = new List<Client>();
        private List<Card> deck = new List<Card>();
        private List<Card> well = new List<Card>();

        private int Number { get; set; }
        private bool Running { get; set; }
        private bool Playing { get; set; }

        public Game(int nb)
        {
            Running = false;
            Playing = false;
            Number = nb;
            ShuffleDeck();
        }

        public void ShuffleDeck()
        {
            for (int i = 0; i < NbCard; ++i)
            {
                deck.Add((Card)Math.Floor((decimal)i / 4));
            }
        }

        private void DistribCards()
        {
            Random rand = new Random();
            while (deck.Count != 0)
            {
                int nbCard = rand.Next(deck.Count);
                players[deck.Count % maxNbPlayers].AddCard(deck[nbCard]);
                deck.RemoveAt(nbCard);
            }
        }

        public void BeginGame()
        {
            if (Running)
                return;
            Running = true;
            DistribCards();
            Console.WriteLine("Waiting now for all players to be ready");
        }

        public void AddClient(Client cl)
        {
            Console.WriteLine("Adding player");
            players.Add(cl);
            cl.Write("Added to game n°" + Number + ". Waiting for a challenger.");
            if (IsFull())
                foreach (Client client in players)
                {
                   client.Write("The hobby is now full. The game will begin soon...");
                }
        }

        public bool RemoveClient(Connection connection)
        {
            foreach (Client cl in players)
            {
                if (cl.IsEqual(connection))
                {
                    players.Remove(cl);
                    if (Running)
                        ResetGame(true);
                    return true;
                }
            }
            return false;
        }

        private void ResetGame(bool quit)
        {
            foreach (Client cl in players)
            {
                if (quit)
                    cl.Write("A player left the game. Please wait until another player comes...");
                cl.setReadyState(false);
                cl.TossHand();
            }
            ShuffleDeck();
            Running = false;
            Playing = false;
            nbTurn = 0;
            Console.WriteLine("Game reset");
        }

        public bool IsFull()
        {
            return maxNbPlayers == 0 ? false : players.Count == maxNbPlayers;
        }

        public int nbPlayers()
        {
            return players.Count;
        }

        public bool IsRunning()
        {
            return Running;
        }

        public bool IsPlaying()
        {
            return Playing;
        }

        public void PrepareGame()
        {
            foreach (Client client in players)
            {
                if (!client.IsReady())
                    return;
            }
            Playing = true;
            Console.WriteLine("Game prepared.");
        }

        public void DoTurn()
        {
            nbTurn++;
            msgTurn();
            foreach (Client client in players)
            {
                if (client.GetCardDrawn() == Card.None)
                    return;
            }
            PrepareTurn();
        }

        public void PrepareTurn()
        {
            FillWell(false);
            CheckWinTurnCondition();
        }

        public void FillWell(bool display)
        {
            foreach (Client client in players)
            {
                if (display)
                    client.Write("Draw! So we take another card.");
                Card card = client.RemoveCard();
                if (card == Card.None)
                {
                    CheckWinGameCondition();
                    return;
                }
                well.Add(card);
            }
        }

        public void CheckWinTurnCondition()
        {
            Card higher = Card.None;
            List<int> winners = new List<int>();
            for (int i = well.Count - maxNbPlayers; i < well.Count - 1; ++i)
            {
                if (higher == Card.None)
                {
                    higher = well[i];
                    winners.Add(i);
                }
                else if (well[i] >= higher)
                {
                    if (well[i] > higher)
                    {
                        higher = well[i];
                        winners.RemoveRange(0, winners.Count);
                    }
                    winners.Add(i);
                }
            }
            if (winners.Count > 1)
                FillWell(true);
            else
            {
                DisplayVictoryMsg(players[winners[0]]);
                CheckWinGameCondition();
            }
        }

        public void CheckWinGameCondition()
        {
            foreach (Client cl in players)
            {
                if (cl.NbCards() == 0)
                    cl.Write("You lost.");
                else
                    cl.Write("You won!");
                cl.Write("Restart? (y/N)");
            }
            ResetGame(false);
        }

        public void RestartGame()
        {
            ResetGame(false);
        }

        public void msgTurn()
        {
            foreach (Client cl in players)
            {
                cl.Write("Turn n°" + nbTurn + ": You have " + cl.NbCards() + ".\nPlease take your card:");
            }
        }

        public void DisplayVictoryMsg(Client winner)
        {
            foreach (Client cl in players)
            {
                if (winner == cl)
                {
                    cl.Write("You win this turn! You take the card in the well.");
                    while (well.Count != 0)
                    {
                        cl.AddCard(well[0]);
                        well.RemoveAt(0);
                    }
                }
                else
                    cl.Write("You lose this turn.");
            }
        }
    }
}
