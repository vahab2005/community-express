﻿using System;
using System.Linq;
using CommunityExpressNS;
using SharpDX;
using SharpDX.Toolkit;

namespace CommunityExpressStandAloneTestDX
{
    /// <summary>
    /// Simple HelloWorld application using SharpDX.Toolkit.
    /// The purpose of this application is to show the minimal setup of a game.
    /// </summary>
    public class HelloWorldGame : Game
    {
        private GraphicsDeviceManager graphicsDeviceManager;
        private float delta = 0.0f;
        private bool _shown = false;
        private CommunityExpress _steam;
        private Leaderboard _leaderboard;
        private bool _entriesCall = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="HelloWorldGame" /> class.
        /// </summary>
        public HelloWorldGame()
        {

            // Creates a graphics manager. This is mandatory.
            graphicsDeviceManager = new GraphicsDeviceManager(this);

            // Setup the relative directory to the executable directory
            // for loading contents with the ContentManager
            Content.RootDirectory = "Content";
        }

        void Friends_GameOverlayActivated(bool result)
        {
            if (result)
            {
                _shown = true;
            }
        }

        protected override void Initialize()
        {
            base.Initialize();

            CommunityExpress.Instance.Initialize();
            _steam = CommunityExpress.Instance;
            CommunityExpress.Instance.Friends.GameOverlayActivated += Friends_GameOverlayActivated;

            CommunityExpress.Instance.Leaderboards.LeaderboardReceived += Leaderboards_LeaderboardReceived;

            var achievements = _steam.UserAchievements;
            achievements.InitializeAchievementList(new[] { "TEST_achievement", "TEST_achivement_2" });
            MyOnUserStatsReceivedCallback(null, achievements);

        }

        private void MyOnUserStatsReceivedCallback(Stats stats, Achievements achievements)
        {
            if (stats != null)
            {
                Console.WriteLine("Stats: ");
                foreach (Stat s in stats)
                {
                    Console.WriteLine("  {0} - {1} - {2} - {3} - {4}", s.StatName, s.StatValue, s.StatValue.GetType().Name,
                        s.StatValue is float,
                        s.StatValue is int);
                }
            }

            if (achievements != null)
            {
                Console.WriteLine("Achievements: ");
                foreach (Achievement a in achievements)
                {
                    Byte[] iconData = a.IconData;
                    if (iconData != null)
                        Console.WriteLine("  {0} - {1} - {2}x{3}({4}) - {5}: {6}", a.AchievementName, a.IsAchieved,
                            a.IconWidth, a.IconHeight, iconData.Length, a.DisplayName, a.DisplayDescription);
                    else
                        Console.WriteLine("  {0} - {1} - {2}: {3}", a.AchievementName, a.IsAchieved, a.DisplayName,
                            a.DisplayDescription);


                }

				if(achievements.AchievementList.Count > 0)
					_steam.UserAchievements.UnlockAchievement(achievements.AchievementList[0], true);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            // Clears the screen with the Color.CornflowerBlue
            GraphicsDevice.Clear(GraphicsDevice.BackBuffer, Color.CornflowerBlue);

            if (!_shown && gameTime.TotalGameTime.TotalSeconds > 10.0f)
            {
                _shown = true;
                try
                {
                    CommunityExpress.Instance.Friends.ActivateGameOverlay(EGameOverlay.EGameOverlayCommunity);

                }
                catch (Exception)
                {

                    Console.WriteLine("error");
                }
            }



            ////Leaderboards

            if (_leaderboard == null)
            {
                Console.WriteLine(@"Hi {0} you are welcome ", CommunityExpress.Instance.User.PersonaName);

                CommunityExpress.Instance.Leaderboards.FindOrCreateLeaderboard("Score", ELeaderboardSortMethod.k_ELeaderboardSortMethodAscending, ELeaderboardDisplayType.k_ELeaderboardDisplayTypeTimeMilliSeconds);
            }
            else if (!_entriesCall)
            {
                Console.WriteLine(@"Attemping to set a new score. False? {0}", _leaderboard == null);
                _leaderboard.UploadLeaderboardScore(ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodForceUpdate, 1250, new[] { 1, 5 }.ToList());
                //                    _leaderboard.UploadLeaderboardScore(ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodNone, 1150, new List<int> { 5, 6, 50 });
                //                    _leaderboard.UploadLeaderboardScore(ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodForceUpdate, 250, null);
                _leaderboard.RequestLeaderboardEntries(0, 50, 50);

                _leaderboard.LeaderboardEntriesReceived += OnLeaderboardEntriesReceived;

                _entriesCall = true;
            }
            ////Leaderboards





            CommunityExpress.Instance.RunCallbacks();
            base.Draw(gameTime);
        }

        private void OnLeaderboardEntriesReceived(Leaderboard sender, LeaderboardEntries entries)
        {
            foreach (var leaderboardEntry in entries)
            {
                Console.WriteLine(@"Name:{0} Score: {1} Details: {2}", leaderboardEntry.PersonaName, leaderboardEntry.Score, string.Join(" ", leaderboardEntry.ScoreDetails));
            }
        }

        private void Leaderboards_LeaderboardReceived(Leaderboards sender, Leaderboard leaderboard)
        {
            if (_leaderboard == null)
                Console.WriteLine("leaderboard name: {0}. Leaderboard(s) received {1}", leaderboard.LeaderboardName, sender.Count);
            
			_leaderboard = leaderboard;
	        _leaderboard.LeaderboardScoreUploaded += OnLeaderboardScoreUploaded;
        }

	    private void OnLeaderboardScoreUploaded(Leaderboard sender, LeaderboardScoreUploaded pScoreUploadedResult)
	    {
		    if (sender == null)
			    return;

		    Console.WriteLine("Score uploaded successfully! Score:{0}", pScoreUploadedResult.Score);
	    }

	    static void Main(string[] args)
        {
            HelloWorldGame game = new HelloWorldGame();
            game.Run();
        }
    }
}

