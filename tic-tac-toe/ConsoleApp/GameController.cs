using ConsoleUI;
using DAL;
using GameBrain;
using MenuSystem;

namespace ConsoleApp;

public static class GameController
{
    private static readonly ConfigRepository ConfigRepo = new ConfigRepository();

    public static string SelectedGameType { get; private set; } = null!;
    public static GameConfiguration SelectedGameConfiguration { get; private set; } = default!;

    public static void SetSelectedGameType(string gameType)
    {
        SelectedGameType = gameType;
    }

    public static void SetSelectedGameConfiguration(GameConfiguration config)
    {
        SelectedGameConfiguration = config;
    }

    public static string StartGame()
    {
        var game = new TicTacTwoBrain(SelectedGameConfiguration);

        while (true)
        {
            Visualizer.DrawBoard(game);
            Console.WriteLine($"Game Mode: {SelectedGameConfiguration.Name}");

            if (game.IsXTurn)
            {
                Console.WriteLine("Player X's turn.");
                var result = HandlePlayerTurn(game);
                if (result == "RETURN") return "Returned to main menu";
                if (result == "CONTINUE") game.IsXTurn = false;
            }
            else
            {
                Console.WriteLine("Player O's turn.");
                var result = HandlePlayerTurn(game);
                if (result == "RETURN") return "Returned to main menu";
                if (result == "CONTINUE") game.IsXTurn = true;
            }

            if (game.CheckWin())
            {
                Visualizer.DrawBoard(game);
                Console.WriteLine($"Player {(game.IsXTurn ? "O" : "X")} wins!");
                break;
            }

            // Check for draw condition
            if (game.XMoveCount + game.OMoveCount == game.DimX * game.DimY)
            {
                Visualizer.DrawBoard(game);
                Console.WriteLine("The game is a draw!");
                break;
            }
        }

        return "Game over";
    }

    public static string MainLoop()
    {
        while (true)
        {
            var chosenConfigShortcut = ChooseConfiguration();

            if (!int.TryParse(chosenConfigShortcut, out var configNo))
            {
                return chosenConfigShortcut;
            }

            var chosenConfig = ConfigRepo.GetConfigurationByName(
                ConfigRepo.GetConfigurationNames()[configNo]
            );

            var gameInstance = new TicTacTwoBrain(chosenConfig);

            while (true)
            {
                Visualizer.DrawBoard(gameInstance);

                if (gameInstance.IsXTurn)
                {
                    Console.WriteLine("Player X's turn.");
                    var result = HandlePlayerTurn(gameInstance);
                    if (result == "RETURN") break;
                    if (result == "CONTINUE") gameInstance.IsXTurn = false;
                }
                else
                {
                    Console.WriteLine("Player O's turn.");
                    var result = HandlePlayerTurn(gameInstance);
                    if (result == "RETURN") break;
                    if (result == "CONTINUE") gameInstance.IsXTurn = true;
                }

                if (gameInstance.CheckWin())
                {
                    Visualizer.DrawBoard(gameInstance);
                    break;
                }
            }
        }

        return "Game over";
    }

    private static string HandlePlayerTurn(TicTacTwoBrain gameInstance)
    {
        if ((gameInstance.IsXTurn && gameInstance.XMoveCount >= 2) ||
            (!gameInstance.IsXTurn && gameInstance.OMoveCount >= 2))
        {
            Console.WriteLine("Choose action: (1) Place piece, (2) Move grid, (3) Move piece, (S) Save game, (R) Return, (E) Exit game");
        }
        else if (gameInstance.XMoveCount < 1)
        {
            Console.WriteLine("Choose action: (1) Place piece, (S) Save game, (R) Return, (E) Exit game");
        }
        else
        {
            Console.WriteLine("Choose action: (1) Place piece, (S) Save game, (E) Exit game");
        }

        string? action = Console.ReadLine();

        switch (action?.ToUpper())
        {
            case "1":
                return HandlePlacePiece(gameInstance) ? "CONTINUE" : "INVALID";
            case "2":
                return HandleMoveGrid(gameInstance) ? "CONTINUE" : "INVALID";
            case "3":
                return HandleMovePiece(gameInstance) ? "CONTINUE" : "INVALID";
            case "S":
                HandleSaveGame(gameInstance);
                return "CONTINUE"; // Save game is always successful
            case "R":
                return "RETURN"; // Return to main menu
            case "E":
                Environment.Exit(0);
                return "EXIT"; // Exit game
            default:
                Console.WriteLine("Invalid choice. Try again.");
                return "INVALID";
        }
    }
    
    private static bool HandlePlacePiece(TicTacTwoBrain gameInstance)
{
    Console.WriteLine("Enter row and column to place piece (e.g., 2,2):");
    var input = Console.ReadLine()?.Split(",");
    if (input?.Length == 2 &&
        int.TryParse(input[0], out int row) &&
        int.TryParse(input[1], out int col))
    {
        if (gameInstance.MakeAMove(row, col))
        {
            return true;
        }
        else
        {
            Console.WriteLine("Invalid move, try again.");
            return false;
        }
    }
    else
    {
        Console.WriteLine("Invalid input.");
        return false;
    }
}

private static bool HandleMoveGrid(TicTacTwoBrain gameInstance)
{
    Console.WriteLine("Enter grid move offset (e.g., -1,0 for up):");
    var input = Console.ReadLine()?.Split(",");
    if (input?.Length == 2 &&
        int.TryParse(input[0], out int rowOffset) &&
        int.TryParse(input[1], out int colOffset))
    {
        if (gameInstance.MoveGrid(rowOffset, colOffset))
        {
            return true;
        }
        else
        {
            Console.WriteLine("Invalid grid movement.");
            return false;
        }
    }
    else
    {
        Console.WriteLine("Invalid input.");
        return false;
    }
}

private static bool HandleMovePiece(TicTacTwoBrain gameInstance)
{
    Console.WriteLine("Enter source row/col and destination row/col (e.g., 1,1,2,2):");
    var input = Console.ReadLine()?.Split(",");
    if (input?.Length == 4 &&
        int.TryParse(input[0], out int fromRow) &&
        int.TryParse(input[1], out int fromCol) &&
        int.TryParse(input[2], out int toRow) &&
        int.TryParse(input[3], out int toCol))
    {
        if (gameInstance.MovePiece(fromRow, fromCol, toRow, toCol))
        {
            return true;
        }
        else
        {
            Console.WriteLine("Invalid piece movement.");
            return false;
        }
    }
    else
    {
        Console.WriteLine("Invalid input.");
        return false;
    }
}
    
    private static void HandleSaveGame(TicTacTwoBrain gameInstance)
    {
        // Implement save game logic here
    }

    private static string ChooseConfiguration()
    {
        var configMenuItems = new List<MenuItem>();

        for (var i = 0; i < ConfigRepo.GetConfigurationNames().Count; i++)
        {
            var returnValue = i.ToString();
            configMenuItems.Add(new MenuItem()
            {
                Title = ConfigRepo.GetConfigurationNames()[i],
                Shortcut = (i + 1).ToString(),
                MenuItemAction = () => returnValue
            });
        }

        var configMenu = new Menu(EMenuLevel.Deep,
            "TIC-TAC-TWO - choose game config",
            configMenuItems,
            isCustomMenu: true
        );

        return configMenu.Run();
    }
    
    
}

