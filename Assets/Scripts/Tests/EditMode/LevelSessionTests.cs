using System;
using System.Collections.Generic;
using Game.Core.Levels;
using Game.Tests.EditMode.Fakes;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public class LevelSessionTests
    {
        private const int Objectives = 3;
        private const int Balls = 2;
        private const float SettleTimeout = 3f;
        private const float Dt = 0.1f;
        private const float ExactDt = 0.125f;

        private FakeWorldRestQuery worldRest;

        [SetUp]
        public void SetUp()
        {
            worldRest = new FakeWorldRestQuery { IsAtRest = false };
        }

        private LevelSession Session(int objectives = Objectives, int balls = Balls) =>
            new LevelSession(objectives, balls, worldRest, SettleTimeout);

        private static void SpendAllBalls(LevelSession session)
        {
            while (session.TryConsumeBall()) { }
        }

        [Test]
        public void StartsPlaying_WithTheFullBudget()
        {
            LevelSession session = Session();

            Assert.AreEqual(LevelState.Playing, session.State);
            Assert.AreEqual(Balls, session.BallsRemaining);
            Assert.AreEqual(Objectives, session.ObjectivesRemaining);
        }

        [Test]
        public void ConsumingABall_DecrementsTheBudget()
        {
            LevelSession session = Session();

            Assert.IsTrue(session.TryConsumeBall());

            Assert.AreEqual(Balls - 1, session.BallsRemaining);
            Assert.AreEqual(LevelState.Playing, session.State);
        }

        [Test]
        public void SpendingTheLastBall_EntersSettling()
        {
            LevelSession session = Session();

            SpendAllBalls(session);

            Assert.AreEqual(0, session.BallsRemaining);
            Assert.AreEqual(LevelState.Settling, session.State);
        }

        [Test]
        public void ConsumingABall_FailsOnceTheBudgetIsSpent()
        {
            LevelSession session = Session();
            SpendAllBalls(session);

            Assert.IsFalse(session.TryConsumeBall());
        }

        [Test]
        public void ClearingAllObjectives_WinsImmediately_WithBallsStillLeft()
        {
            LevelSession session = Session();

            session.ClearObjective(0);
            session.ClearObjective(1);
            Assert.AreEqual(LevelState.Playing, session.State);

            session.ClearObjective(2);

            Assert.AreEqual(LevelState.Won, session.State);
            Assert.AreEqual(Balls, session.BallsRemaining);
        }

        [Test]
        public void ClearingTheSameObjectiveTwice_CountsOnce()
        {
            LevelSession session = Session();

            session.ClearObjective(1);
            session.ClearObjective(1);

            Assert.AreEqual(Objectives - 1, session.ObjectivesRemaining);
            Assert.AreEqual(LevelState.Playing, session.State);
        }

        [Test]
        public void Finishing_RaisesFinishedExactlyOnce()
        {
            LevelSession session = Session();
            var raised = new List<LevelState>();
            session.Finished += state => raised.Add(state);

            session.ClearObjective(0);
            session.ClearObjective(1);
            session.ClearObjective(2);
            session.ClearObjective(0);

            CollectionAssert.AreEqual(new[] { LevelState.Won }, raised);
        }

        [Test]
        public void Settling_DoesNotResolveWhileTheWorldIsStillMoving()
        {
            LevelSession session = Session();
            SpendAllBalls(session);

            session.Tick(Dt);
            session.Tick(Dt);

            Assert.AreEqual(LevelState.Settling, session.State);
        }

        [Test]
        public void Settling_ResolvesToLost_WhenTheWorldComesToRest()
        {
            LevelSession session = Session();
            SpendAllBalls(session);

            session.Tick(Dt);
            worldRest.IsAtRest = true;
            session.Tick(Dt);

            Assert.AreEqual(LevelState.Lost, session.State);
        }

        [Test]
        public void Settling_ResolvesToWon_IfTheLastObjectiveClearsDuringSettle()
        {
            LevelSession session = Session();
            SpendAllBalls(session);
            session.Tick(Dt);

            session.ClearObjective(0);
            session.ClearObjective(1);
            session.ClearObjective(2);

            Assert.AreEqual(LevelState.Won, session.State);
        }

        [Test]
        public void Settling_ResolvesOnTimeout_EvenIfTheWorldNeverRests()
        {
            LevelSession session = Session();
            SpendAllBalls(session);

            int ticksToTimeout = (int)(SettleTimeout / ExactDt);

            for (int i = 0; i < ticksToTimeout - 1; i++)
                session.Tick(ExactDt);

            Assert.AreEqual(LevelState.Settling, session.State);

            session.Tick(ExactDt);

            Assert.AreEqual(LevelState.Lost, session.State);
        }

        [Test]
        public void Settling_DoesNotTimeOutEarly_OnASingleLongTick()
        {
            LevelSession session = Session();
            SpendAllBalls(session);

            session.Tick(SettleTimeout - 0.5f);

            Assert.AreEqual(LevelState.Settling, session.State);
        }

        [Test]
        public void Tick_DoesNothingWhilePlaying()
        {
            LevelSession session = Session();
            worldRest.IsAtRest = true;

            for (int i = 0; i < 100; i++)
                session.Tick(Dt);

            Assert.AreEqual(LevelState.Playing, session.State);
        }

        [Test]
        public void AfterFinishing_BallsCannotBeConsumed()
        {
            LevelSession session = Session();
            session.ClearObjective(0);
            session.ClearObjective(1);
            session.ClearObjective(2);

            Assert.IsFalse(session.TryConsumeBall());
        }

        [Test]
        public void AfterFinishing_ClearingIsIgnored()
        {
            LevelSession session = Session(objectives: 2);
            SpendAllBalls(session);
            worldRest.IsAtRest = true;
            session.Tick(Dt);
            Assert.AreEqual(LevelState.Lost, session.State);

            session.ClearObjective(0);
            session.ClearObjective(1);

            Assert.AreEqual(LevelState.Lost, session.State);
        }

        [Test]
        public void AfterFinishing_FurtherTicksDoNotReRaiseFinished()
        {
            LevelSession session = Session();
            var raised = new List<LevelState>();
            session.Finished += state => raised.Add(state);

            SpendAllBalls(session);
            worldRest.IsAtRest = true;
            session.Tick(Dt);
            session.Tick(Dt);
            session.Tick(Dt);

            CollectionAssert.AreEqual(new[] { LevelState.Lost }, raised);
        }

        [Test]
        public void LevelWithNoObjectives_StartsWon()
        {
            LevelSession session = Session(objectives: 0);

            Assert.AreEqual(LevelState.Won, session.State);
        }

        [Test]
        public void LevelWithNoBalls_StartsSettling_AndResolvesToLost()
        {
            LevelSession session = Session(balls: 0);

            Assert.AreEqual(LevelState.Settling, session.State);

            worldRest.IsAtRest = true;
            session.Tick(Dt);

            Assert.AreEqual(LevelState.Lost, session.State);
        }

        [Test]
        public void NegativeObjectiveCount_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new LevelSession(-1, Balls, worldRest, SettleTimeout));
        }

        [Test]
        public void NegativeBallCount_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new LevelSession(Objectives, -1, worldRest, SettleTimeout));
        }

        [Test]
        public void NullWorldRestQuery_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => new LevelSession(Objectives, Balls, null, SettleTimeout));
        }

        [Test]
        public void ClearingAnOutOfRangeObjective_Throws()
        {
            LevelSession session = Session();

            Assert.Throws<ArgumentOutOfRangeException>(() => session.ClearObjective(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => session.ClearObjective(Objectives));
        }
    }
}
