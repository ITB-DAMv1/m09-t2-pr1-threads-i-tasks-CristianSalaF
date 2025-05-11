using T2_PR1_Ex2.AsteroidsGame;

namespace T2_PR1_Ex2
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.CursorVisible = false;
            Console.Clear();
    
            var gameController = new GameController();
            var uiController = new UIController();
            var statsManager = new StatsManager();
    
            CancellationTokenSource cts = new CancellationTokenSource();
            Task webEvaluationTask = Task.Run(() => gameController.SimulateWebEvaluation(cts.Token));

            // Allow game restarts while web evaluation is running, unless the player quits the game
            while (gameController.IsWebEvaluationRunning)
            {
                gameController.ResetGame();
                uiController.Initialize(gameController);
        
                gameController.SetupGame();
        
                Task inputTask = Task.Run(() => gameController.HandleUserInput());
                Task gameLogicTask = Task.Run(() => gameController.UpdateGameState());
                Task renderTask = Task.Run(() => uiController.RenderGame());
        
                await Task.WhenAll(gameLogicTask, renderTask, inputTask);
        
                TimeSpan gameTime = DateTime.Now - gameController.GameStartTime;
                int livesUsed = 3 - gameController.Lives;
                
                statsManager.SaveGameData(gameController.Score, gameTime, livesUsed);
                
                bool restartGame = uiController.ShowFinalScoreWithRestart(
                    gameController.Score, gameTime, livesUsed, gameController.IsWebEvaluationRunning);
        
                if (!restartGame)
                {
                    gameController.IsWebEvaluationRunning = false;
                    cts.CancelAsync();
                }
            }
    
            await webEvaluationTask;
    
            Console.CursorVisible = true;
        }
    }
}
