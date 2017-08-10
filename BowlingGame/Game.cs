using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using FluentAssertions;
using NUnit;
using NUnit.Framework;

namespace BowlingGame
{
	public class Game
	{
	    private List<int> rolls;
	    private List<Frame> frames;

	    public Game()
	    {
	        rolls = new List<int>();
	        frames = new List<Frame>();

	    }

	    public Game Roll(int pins)
        {
	        if (frames.Count == 10)
	        {
	            frames.Last().AddPin(pins);
	            return this;
	        }

	        if (frames.Count == 0 || frames.Last().Score == 10 || frames.Last().PinsCount == 2)
	        {
	            frames.Add(new Frame(pins));
	            return this;
	        }

	        frames.Last().AddPin(pins);
            return this;
        }

	    public int GetScore()
	    {
	        frames.Reverse();
            int sum = 0;

            foreach (var frame in frames)
            {
                if (frame.Type == FrameType.Default)
                {
                    sum += frame.Score;
                    continue;
                }

                var index = frames.IndexOf(frame) - 1;
                if (frame.Type == FrameType.Spare)
                {
                    if (index < frames.Count && index >= 0)
                        sum += frame.Score + frames[index].FirstPinScore;
                    else
                        sum += frame.Score;
                    continue;
                }

                if (index < frames.Count && index >= 0)
                {
                    sum += frame.Score + frames[index].TwoPinsScore;

                    if (frames[index].Type == FrameType.Strike && index - 1 >= 0)
                        sum += frames[index - 1].TwoPinsScore;
                }
                else
                    sum += frame.Score;
            }

	        return sum;
	    }
	}

    internal class Frame
    {
        private List<int> pins;

        public Frame(int pinScore)
        {
            this.pins = new List<int> { pinScore };
        }

        public Frame()
        {
            this.pins = new List<int> ();
        }

        public int PinsCount => pins.Count;

        public FrameType Type
        {
            get
            {
                var isSpareOrStrike = pins.Sum() == 10;

                if (!isSpareOrStrike)
                    return FrameType.Default;

                if (pins[0] == 10)
                    return FrameType.Strike;

                return FrameType.Spare;
            }
        }

        public int Score => pins.Sum();

        public int FirstPinScore => pins.First();
        public int TwoPinsScore => pins.Take(2).Sum();

        public void AddPin(int pinScore)
        {
            pins.Add(pinScore);
        }
    }

    internal enum FrameType
    {
        Default = 0,
        Spare = 1,
        Strike = 2
    }


    [TestFixture]
	public class Game_should : ReportingTest<Game_should>
	{
		// ReSharper disable once UnusedMember.Global
		public static string Names = "Bobryshev & RandomGuy (Shelomentsev)"; // Ivanov Petrov

        [TestCase(1, 7, 3, ExpectedResult = 11, TestName = "3 броска, простая сумма.")]
        [TestCase(7, 3, ExpectedResult = 10, TestName = "2 броска со spare, в сумме только первый бросок.")]
        [TestCase(4, 5, ExpectedResult = 9, TestName = "2 броска, простая сумма.")]
        [TestCase(4, ExpectedResult = 4, TestName = "1 бросок, простая сумма.")]
        [TestCase(ExpectedResult = 0, TestName = "Не играли, в сумме 0.")]
        [TestCase(1, 9, 5, 4, ExpectedResult = 24, TestName = "4 броска со Spare в первом frame.")]
        [TestCase(10, 5, 4, ExpectedResult = 28, TestName = "3 броска со Strike в первом frame.")]
        [TestCase(10, 10, ExpectedResult = 30, TestName = "2 броска Strike.")]
        [TestCase(10, 10, 10, ExpectedResult = 60, TestName = "3 броска Strike.")]
        [TestCase(10, 10, 5, ExpectedResult = 45, TestName = "3 броска, первые 2 Strike.")]
        [TestCase(10, 10, 10, 10, 10, 10, 10, 10, 10, ExpectedResult = 240, TestName = "9 броска Strike.")]
        [TestCase(10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, ExpectedResult = 300, TestName = "12 Strike.")]

        public int _(params int[] rolls)
        {
            var game = new Game();
            foreach (var roll in rolls)
            {
                game.Roll(roll);
            }

            return game.GetScore();
        }
	}
}
