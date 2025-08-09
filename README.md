# CreditSeaAssignment
Ludo Game - Unity Developer Assignment

This project is a 2-player Ludo game created in Unity as part of a technical assessment. It was built to fulfill all the core requirements outlined in the assignment, focusing on clean code and a complete gameplay loop.
How to Run the Project

    Clone/Download: Get the project files onto your local machine.

    Open in Unity Hub: Add the project folder to Unity Hub and open it with a compatible version of the Unity Editor.

    Open the Main Menu Scene: In the Project window, navigate to the Scenes folder and open the MainMenu scene.

    Press Play: Run the game from the editor.

Features Implemented

This project successfully implements all the core requirements from the assignment:

    2-Player Gameplay: A complete, turn-based game for two players (Blue and Green) on a single device.

    Full Ludo Logic:

        Tokens enter the board only on a roll of 6.

        Players get an extra turn on rolling a 6 (unless used to unlock a token).

        Tokens move according to the dice value.

        Opponent tokens are captured and sent back to base upon collision.

        An exact dice roll is required for a token to enter the home square.

    Complete UI Flow:

        A Home Screen with "Play" and "Play for ₹10" options.

        The main Game Screen with the board, dice, and turn indicators.

        A Win Screen that declares the winner.

    Dummy Payment Flow: The "Play for ₹10" option simulates a payment and displays the required prize text on the win screen.

    Code Quality: The codebase is structured into modular scripts (GameLogic, PlayerToken, Dice) with clear responsibilities and comments where necessary.

Future Improvements

If I had more time, I would focus on these areas:

    Token Stacking: The top priority would be to stack tokens side by side when they land on the grid box without sending each other to the base.

    Enhance Visual Polish: I would add more satisfying animations, particle effects for captures and winning, and sound effects for dice rolls and token movement.

    Refactor for Multiplayer: The current GameLogic is a singleton, which is great for a single-device game. For the bonus multiplayer feature, I would refactor the game state management to work with a networked authority instead of a local one.
