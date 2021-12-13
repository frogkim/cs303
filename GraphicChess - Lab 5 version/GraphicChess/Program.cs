using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

//1.Consider the following snapshot from a game of chess 
//(the green and red outlines indicate that the previous move was made by the black queen,
//from the green-outlined square to the red-outlined square):

//(a)Write C# code which would cause the chess board to be configured 
//as in this snapshot(if the code were placed in the ChessGame constructor). 
//Make sure that your code is easy for a human to read and verify.

//(b) List all of the legal moves available to player White at this turn,
//using the notation of Lab 5. List the moves in lexicographic order 
//by (x, y)-coordinates of starting and ending positions,
// and number the moves, so that it is easy for a human reader to verify.

//(c) For each legal move you listed in part (b) above, 
//determine the total value of the pieces on the board after the last move. 
//You can find the values of the pieces in the constructor code for the pieces.

//(d) Determine which move would be chosen by the program from this state 
//if it were playing White with a search depth of 1 turn ahead.


//2. Consider the snapshot above from a game of chess 
//(here, the red and green do not indicate the previous move).

//(a)Write C# code which would cause the chess board to be configured 
//as in this snapshot(if the code were placed in the ChessGame constructor).
//For parts(b) through(d), suppose that the check and checkmate methods 
//and rules have been implemented.

//(b) Whose turn must it be, and why?

//(c) Explain why the move from the green-outlined square to the red-outlined square 
//is illegal.

//(d) List all the legal moves for this turn,
//together with the total value of the pieces on the board after each move.

//(e) Now suppose that the check and checkmate methods 
//have not been implemented, and that it is Black’s turn. 
//Repeat part (d) above under these assumptions.

//(f) Explain which move would be chosen by the program with a search depth 
//of 1 move ahead, if check and checkmate have not been implemented.




namespace GraphicChess
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ChessGame());
        }
    }
}
