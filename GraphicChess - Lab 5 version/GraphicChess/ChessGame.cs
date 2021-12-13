using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using System.Threading;

/*
You should only modify the ChessGame.cs source file. Search for "YOUR CODE HERE" to find places where you need to add code. "TODODONE"
The following methods and rules have also been left incomplete, but are not required to be completed:
inCheck()
inCheckmate()
capturing en passant
pawn promotion
a player cannot move into check
castling (optionally including the rule that a player cannot castle while in check, or castle through check)
*/

namespace GraphicChess
{
    public partial class ChessGame : Form
    {
        private const int boardSize = 8;
        private int boardSquareWidth = 40, boardSquareHeight = 40, boardHOffset = 30, boardVOffset = 30;//in pixels
	    public struct Point
	    {
		    public int x;
		    public int y;
            public Point(int xVal, int yVal) { x = xVal; y = yVal; }
		    public static bool operator==(Point p, Point q) {return p.x == q.x && p.y == q.y;}
            public static bool operator!=(Point p, Point q) { return !(p == q); }
            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
            public override bool Equals(object obj)
            {
                return base.Equals(obj);
            }
	    }
         
	    public struct ChessMove
	    {
		    public Point origin;
		    public Point destination;
            public ChessMove(ref Point theOrigin, ref Point theDestination) { origin = theOrigin; destination = theDestination; }
		    public static bool operator==(ChessMove m1, ChessMove m2) {return m1.origin == m2.origin && m1.destination == m2.destination;}
            public static bool operator !=(ChessMove m1, ChessMove m2) { return !(m1 == m2); }
            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
            public override bool Equals(object obj)
            {
                return base.Equals(obj);
            }
	    };

        public abstract class Piece
        {
            public PLAYER whosePiece;
            public Point boardLocation;
            public double value;
            public System.Drawing.Bitmap imageFile;
            public Piece(int xLocation, int yLocation, PLAYER owner, double theValue = 0)
            {
                boardLocation.x = xLocation;
                boardLocation.y = yLocation;
                whosePiece = owner;
                value = theValue;
                if (owner != PLAYER.WHITE) value *= -1;
            }
            public abstract int getLegalMoves(List<ChessMove> moves);
        }

        public class Pawn : Piece
        {
            public Pawn(int xLocation, int yLocation, PLAYER owner) : base(xLocation, yLocation, owner, 1)
            {
                imageFile = (owner == PLAYER.WHITE) ? bitmaps.wpawn : bitmaps.bpawn;
            }
            public override int getLegalMoves(List<ChessMove> moves)
            {
                int numMoves = 0;
                PLAYER opponent;
                int direction, baseRank, farEnd;
                ChessMove curMove;

                if (whosePiece == PLAYER.WHITE)
                {
                    opponent = PLAYER.BLACK;
                    direction = 1;
                    baseRank = 1;
                    farEnd = boardSize - 1;
                }
                else
                {
                    opponent = PLAYER.WHITE;
                    direction = -1;
                    baseRank = boardSize - 2;
                    farEnd = 0;
                }

                if (boardLocation.y != farEnd && board[boardLocation.x, boardLocation.y + direction] == null)
                {
                    curMove.origin = boardLocation;
                    curMove.destination.x = boardLocation.x;
                    curMove.destination.y = boardLocation.y + direction;
                    moves.Add(curMove);
                    numMoves++;
                }

                if (boardLocation.y == baseRank && board[boardLocation.x, boardLocation.y + direction] == null &&
                    board[boardLocation.x, boardLocation.y + 2 * direction] == null)
                {
                    curMove.origin = boardLocation;
                    curMove.destination.x = boardLocation.x;
                    curMove.destination.y = boardLocation.y + 2 * direction;
                    moves.Add(curMove);
                    numMoves++;
                }

                if (boardLocation.y != farEnd && boardLocation.x > 0 &&
                    board[boardLocation.x - 1, boardLocation.y + direction] != null &&
                    board[boardLocation.x - 1, boardLocation.y + direction].whosePiece == opponent)
                {
                    //Capture an enemy piece.
                    curMove.origin = boardLocation;
                    curMove.destination.x = boardLocation.x - 1;
                    curMove.destination.y = boardLocation.y + direction;
                    moves.Add(curMove);
                    numMoves++;
                }

                if (boardLocation.y != farEnd && boardLocation.x < boardSize - 1 &&
                    board[boardLocation.x + 1, boardLocation.y + direction] != null &&
                    board[boardLocation.x + 1, boardLocation.y + direction].whosePiece == opponent)
                {
                    //Capture an enemy piece.
                    curMove.origin = boardLocation;
                    curMove.destination.x = boardLocation.x + 1;
                    curMove.destination.y = boardLocation.y + direction;
                    moves.Add(curMove);
                    numMoves++;
                }

                return numMoves;
            }
        }

        public class Knight : Piece
        {
            public Knight(int xLocation, int yLocation, PLAYER owner) : base(xLocation, yLocation, owner, 3)
            {
                imageFile = (owner == PLAYER.WHITE) ? bitmaps.wknight : bitmaps.bknight;
            }
            public override int getLegalMoves(List<ChessMove> moves)
            {
                int numMoves = 0;
                int dx, dy;
                Point destination;
                ChessMove curMove;

                for (dx = -2; dx <= 2; dx++)
                {
                    for (dy = -2; dy <= 2; dy++)
                    {
                        //Knights can move 2 squares in one direction and 1 square in another.
                        if (Math.Abs(dx) + Math.Abs(dy) != 3) continue;

                        destination.x = boardLocation.x + dx;
                        destination.y = boardLocation.y + dy;
                        if (!isOnBoard(destination)) continue;
                        Piece p = board[destination.x, destination.y];

                        if (p == null || p.whosePiece != whosePiece)
                        {
                            curMove.origin = boardLocation;
                            curMove.destination = destination;
                            moves.Add(curMove);
                            numMoves++;
                        }
                    }
                }

                return numMoves;
            }
        }

        public class Bishop : Piece
        {
            public Bishop(int xLocation, int yLocation, PLAYER owner) : base(xLocation, yLocation, owner, 3.1)
            {
                imageFile = (owner == PLAYER.WHITE) ? bitmaps.wbishop : bitmaps.bbishop;
            }
            public override int getLegalMoves(List<ChessMove> moves)
            {
                int numMoves = 0;
                int i, xDirection, yDirection;
                Point destination;
                ChessMove curMove;

                for (xDirection = -1; xDirection <= 1; xDirection += 2)
                {
                    for (yDirection = -1; yDirection <= 1; yDirection += 2)
                    {
                        for (i = 1; i < boardSize; i++)
                        {
                            destination.x = boardLocation.x + i * xDirection;
                            destination.y = boardLocation.y + i * yDirection;
                            if (!isOnBoard(destination)) break;
                            Piece p = board[destination.x, destination.y];

                            if (p == null || p.whosePiece != whosePiece)
                            {
                                curMove.origin = boardLocation;
                                curMove.destination = destination;
                                moves.Add(curMove);
                                numMoves++;
                            }
                            //A bishop cannot move through other pieces.
                            if (board[destination.x, destination.y] != null) break;
                        }
                    }
                }

                return numMoves;
            }
        }

        public class Rook : Piece
        {
            public Rook(int xLocation, int yLocation, PLAYER owner) : base(xLocation, yLocation, owner, 5)
            {
                imageFile = (owner == PLAYER.WHITE) ? bitmaps.wrook : bitmaps.brook;
            }
            public override int getLegalMoves(List<ChessMove> moves)
            {
                int numMoves = 0;
                int i, xDirection, yDirection;
                Point destination;
                ChessMove curMove;

                for (xDirection = -1; xDirection <= 1; xDirection++)
                {
                    for (yDirection = -1; yDirection <= 1; yDirection++)
                    {
                        if (Math.Abs(xDirection) + Math.Abs(yDirection) > 1) continue;

                        for (i = 1; i < boardSize; i++)
                        {
                            destination.x = boardLocation.x + i * xDirection;
                            destination.y = boardLocation.y + i * yDirection;
                            if (!isOnBoard(destination)) break;
                            Piece p = board[destination.x, destination.y];

                            if (p == null || p.whosePiece != whosePiece)
                            {
                                curMove.origin = boardLocation;
                                curMove.destination = destination;
                                moves.Add(curMove);
                                numMoves++;
                            }
                            //A rook cannot move through other pieces.
                            if (board[destination.x, destination.y] != null) break;
                        }
                    }
                }
                return numMoves;
            }
        }

        public class Queen : Piece
        {
            public Queen(int xLocation, int yLocation, PLAYER owner) : base(xLocation, yLocation, owner, 9)
            {
                imageFile = (owner == PLAYER.WHITE) ? bitmaps.wqueen : bitmaps.bqueen;
            }
            public override int getLegalMoves(List<ChessMove> moves)
            {
                int numMoves = 0;
                int i, xDirection, yDirection;
                Point destination;
                ChessMove curMove;

                // TODODONE
                for (xDirection = -1; xDirection <= 1; xDirection++)
                {
                    for (yDirection = -1; yDirection <= 1; yDirection++)
                    {
                        for (i = 1; i < boardSize; i++)
                        {
                            destination.x = boardLocation.x + i * xDirection;
                            destination.y = boardLocation.y + i * yDirection;
                            if (!isOnBoard(destination)) break;
                            Piece p = board[destination.x, destination.y];

                            if (p == null || p.whosePiece != whosePiece)
                            {
                                curMove.origin = boardLocation;
                                curMove.destination = destination;
                                moves.Add(curMove);
                                numMoves++;
                            }
                            //A rook cannot move through other pieces.
                            if (board[destination.x, destination.y] != null) break;
                        }
                    }
                }
                return numMoves;
            }
        }

        public class King : Piece
        {
            public King(int xLocation, int yLocation, PLAYER owner) : base(xLocation, yLocation, owner, 1000)
            {
                imageFile = (owner == PLAYER.WHITE) ? bitmaps.wking : bitmaps.bking;
            }
            public override int getLegalMoves(List<ChessMove> moves)
            {
                int numMoves = 0;
                int xDirection, yDirection;
                Point destination;
                ChessMove curMove;

                for (xDirection = -1; xDirection <= 1; xDirection++)
                {
                    for (yDirection = -1; yDirection <= 1; yDirection++)
                    {
                        destination.x = boardLocation.x + xDirection;
                        destination.y = boardLocation.y + yDirection;
                        if (!isOnBoard(destination)) continue;
                        Piece p = board[destination.x, destination.y];

                        if (p == null || p.whosePiece != whosePiece)
                        {
                            curMove.origin = boardLocation;
                            curMove.destination = destination;
                            moves.Add(curMove);
                            numMoves++;
                        }
                    }
                }
                return numMoves;
            }
        }

        private static Piece[,] board;
        private bool[] rook1Moved;
	    private bool[] rook2Moved;
	    private bool[] kingMoved;

	    public enum PLAYER
        {
            INVALID = -1, WHITE, BLACK
        }
	    
	    private PLAYER whoseTurn;
        private List<ChessMove> moveHistory;
        private int moveIndex;

        public ChessGame()
        {
            InitializeComponent();

            board = new Piece[boardSize, boardSize];
            // for castling
            rook1Moved = new bool[2];
            rook2Moved = new bool[2];
            kingMoved = new bool[2];

            setupBoard();
            //setupBoardTestLabQ();

            moveHistory = new List<ChessMove>();
            moveIndex = 0;
            undoMoveToolStripMenuItem.Enabled = redoMoveToolStripMenuItem.Enabled = false;
            btnSubmit.Enabled = false;
        }

        private void form_Click(object sender, EventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;
            int x = (me.X - boardHOffset) / boardSquareWidth;
            int y = boardSize - 1 - (me.Y - boardVOffset) / boardSquareHeight;
            if (!isOnBoard(new Point(x, y))) return;
            if (me.Button == System.Windows.Forms.MouseButtons.Left)
            {
                textBox2.Text = x + "," + y;
            }
            else
            {
                textBox1.Text = x + "," + y;
            }
            drawBoard();
        }
        private void setupBoardTestLabQ()
        {
            int i, j;
            PLAYER player;

            // small -- black, large -- white
            // 'p', 'P' - pawn
            // 'n', 'N' - knight
            // 'b', 'B' - bishop
            // 'r', 'R' - rook
            // 'q', 'Q' - queen
            // 'k', 'K' - king

            char[,] boardStatus =
            {
            {'r', 'n', 'b', ' ', 'k', ' ', 'n', 'r'},
            {' ', 'p', 'p', 'p', ' ', 'p', 'p', 'p'},
            {' ', ' ', ' ', ' ', 'p', 'q', ' ', ' '},
            {' ', ' ', ' ', ' ', 'N', ' ', 'B', ' '},
            {'p', ' ', ' ', 'P', 'P', ' ', 'p', 'P'},
            {' ', ' ', 'P', ' ', ' ', 'P', ' ', ' '},
            {'P', ' ', 'P', ' ', ' ', ' ', 'P', ' '},
            {'R', ' ', ' ', 'Q', 'K', 'B', ' ', 'R'}
            }; // boardStatus[ rows, colums ] = [ y, x ]


            for (int x = 0; x < boardSize; x++)
            {
                for (int y = 0; y < boardSize; y++)
                {
                    i = x;
                    j = boardSize - 1 - y;
                    // y's value increases reversely with Cartesian space

                    switch (boardStatus[y, x])
                    {
                        case ' ':
                            board[i, j] = null;
                            break;

                        case 'p':
                            board[i, j] = new Pawn(i, j, PLAYER.BLACK);
                            break;

                        case 'P':
                            board[i, j] = new Pawn(i, j, PLAYER.WHITE);
                            break;

                        case 'n':
                            board[i, j] = new Knight(i, j, PLAYER.BLACK);
                            break;

                        case 'N':
                            board[i, j] = new Knight(i, j, PLAYER.WHITE);
                            break;

                        case 'b':
                            board[i, j] = new Bishop(i, j, PLAYER.BLACK);
                            break;

                        case 'B':
                            board[i, j] = new Bishop(i, j, PLAYER.WHITE);
                            break;

                        case 'r':
                            board[i, j] = new Rook(i, j, PLAYER.BLACK);
                            break;

                        case 'R':
                            board[i, j] = new Rook(i, j, PLAYER.WHITE);
                            break;

                        case 'q':
                            board[i, j] = new Queen(i, j, PLAYER.BLACK);
                            break;

                        case 'Q':
                            board[i, j] = new Queen(i, j, PLAYER.WHITE);
                            break;

                        case 'k':
                            board[i, j] = new King(i, j, PLAYER.BLACK);
                            break;

                        case 'K':
                            board[i, j] = new King(i, j, PLAYER.WHITE);
                            break;
                    }
                }
            }

            for (i = 0; i < 2; i++)
            {
                rook1Moved[i] = false;
                rook2Moved[i] = false;
                kingMoved[i] = false;
            }

            whoseTurn = PLAYER.WHITE;
        }

        private void setupBoardTestLabQQ()
        {
            int i, j;
            PLAYER player;

            // small -- black, large -- white
            // 'p', 'P' - pawn
            // 'n', 'N' - knight
            // 'b', 'B' - bishop
            // 'r', 'R' - rook
            // 'q', 'Q' - queen
            // 'k', 'K' - king

            char[,] boardStatus =
            {
            {' ', 'n', ' ', 'q', 'k', 'b', ' ', 'r'},
            {' ', 'b', ' ', ' ', 'p', ' ', 'p', 'p'},
            {' ', ' ', 'p', ' ', 'B', 'N', ' ', ' '},
            {'r', ' ', ' ', ' ', 'Q', ' ', ' ', ' '},
            {'p', 'p', ' ', ' ', ' ', ' ', 'p', ' '},
            {' ', ' ', ' ', ' ', ' ', 'P', ' ', ' '},
            {'P', 'P', 'P', 'P', ' ', ' ', ' ', 'P'},
            {'R', ' ', 'B', ' ', 'K', ' ', 'N', 'R'}
            };
            // boardStatus[ colums, rows ]

            for (int x = 0; x < boardSize; x++)
            {
                for (int y = 0; y < boardSize; y++)
                {
                    i = x;
                    j = boardSize - 1 - y;
                    // y's value increases reversely with Cartesian space

                    switch (boardStatus[y, x])
                    {
                        case ' ':
                            board[i, j] = null;
                            break;

                        case 'p':
                            board[i, j] = new Pawn(i, j, PLAYER.BLACK);
                            break;

                        case 'P':
                            board[i, j] = new Pawn(i, j, PLAYER.WHITE);
                            break;

                        case 'n':
                            board[i, j] = new Knight(i, j, PLAYER.BLACK);
                            break;

                        case 'N':
                            board[i, j] = new Knight(i, j, PLAYER.WHITE);
                            break;

                        case 'b':
                            board[i, j] = new Bishop(i, j, PLAYER.BLACK);
                            break;

                        case 'B':
                            board[i, j] = new Bishop(i, j, PLAYER.WHITE);
                            break;

                        case 'r':
                            board[i, j] = new Rook(i, j, PLAYER.BLACK);
                            break;

                        case 'R':
                            board[i, j] = new Rook(i, j, PLAYER.WHITE);
                            break;

                        case 'q':
                            board[i, j] = new Queen(i, j, PLAYER.BLACK);
                            break;

                        case 'Q':
                            board[i, j] = new Queen(i, j, PLAYER.WHITE);
                            break;

                        case 'k':
                            board[i, j] = new King(i, j, PLAYER.BLACK);
                            break;

                        case 'K':
                            board[i, j] = new King(i, j, PLAYER.WHITE);
                            break;
                    }
                }
            }

            for (i = 0; i < 2; i++)
            {
                rook1Moved[i] = false;
                rook2Moved[i] = false;
                kingMoved[i] = false;
            }

            whoseTurn = PLAYER.BLACK;
        }

        private void setupBoard()
        {
            int i, j;
            PLAYER player;

            for (i = 0; i < boardSize; i++)
            {
                for(j = 0; j < boardSize; j++)
                {
                    board[i, j] = null;
                }
            }

            for (player = PLAYER.WHITE; player <= PLAYER.BLACK; player++)
            {
                int pawnRank = (player == PLAYER.WHITE) ? 1 : 6;
                int baseRank = (player == PLAYER.WHITE) ? 0 : 7;
                for (i = 0; i < boardSize; i++)
                {
                    board[i, pawnRank] = new Pawn(i, pawnRank, player);
                }

                for (i = 0; i < 2; i++)
                {
                    board[i * 7, baseRank] = new Rook(i * 7, baseRank, player);
                    board[1 + i * 5, baseRank] = new Knight(1 + i * 5, baseRank, player);
                    board[2 + i * 3, baseRank] = new Bishop(2 + i * 3, baseRank, player);
                }

                board[3, baseRank] = new Queen(3, baseRank, player);
                board[4, baseRank] = new King(4, baseRank, player);
            }

            for (i = 0; i < 2; i++)
            {
                rook1Moved[i] = false;
                rook2Moved[i] = false;
                kingMoved[i] = false;
            }

            whoseTurn = PLAYER.WHITE;
        }

        private void playChess()
        {
            drawBoard();
        }

        private PLAYER opponent(PLAYER player)
        {
            if (player == PLAYER.WHITE) return PLAYER.BLACK;
            return PLAYER.WHITE;
        }

        public bool makeMove(ChessMove move, bool checkLegality = true)
        {
	        if(checkLegality && !isLegalMove(move)) return false;

	        Piece piece = board[move.origin.x, move.origin.y];
	        board[move.destination.x, move.destination.y] = piece;
	        board[move.origin.x, move.origin.y] = null;
            piece.boardLocation = move.destination;

	        whoseTurn = opponent(whoseTurn);
	        return true;
        }

        public bool isLegalMove(ChessMove move)
        {
	        if(!isOnBoard(move.origin)) return false;
	        if(!isOnBoard(move.destination)) return false;

	        List<ChessMove> moves = new List<ChessMove>();
	        int numMoves = getLegalMoves(moves);
	
	        return moves.Contains(move);
        }

        private void drawBoard()
        {
            System.Drawing.Graphics graphics = CreateGraphics();

	        int i,j;
	        Piece piece;
            Rectangle boardRect = new Rectangle(boardHOffset, boardVOffset, boardSize * boardSquareWidth, boardSize * boardSquareHeight);
            Rectangle squareRect, innerRect;
            graphics.Clip = new Region(boardRect);
            Pen greenPen = new Pen(new SolidBrush(Color.Green), 5), redPen = new Pen(new SolidBrush(Color.Red), 5);
            SolidBrush whiteBrush = new SolidBrush(Color.White), blackBrush = new SolidBrush(Color.Black);
            graphics.FillRectangle(whiteBrush, boardRect);

	        for(i = 0; i <= boardSize; i++)
	        {
                graphics.DrawLine(System.Drawing.Pens.Black, i * boardSquareWidth + boardHOffset, boardVOffset, i * boardSquareWidth + boardHOffset, boardVOffset + boardSquareHeight * boardSize);
                graphics.DrawLine(System.Drawing.Pens.Black, boardHOffset, i * boardSquareHeight + boardVOffset, boardHOffset + boardSquareWidth * boardSize, i * boardSquareHeight + boardVOffset);
	        }

	        Rectangle Rect;

	        for(j = 0; j < boardSize; j++)
	        {
		        for(i = 0; i < boardSize; i++)
		        {
                    Rect = new Rectangle(i * boardSquareWidth + boardHOffset, (boardSize - 1 - j) * boardSquareHeight + boardVOffset, boardSquareWidth, boardSquareHeight);
			        squareRect = Rect;
                    squareRect.Y = squareRect.Bottom - boardSquareHeight;
			        innerRect = squareRect;
                    innerRect.Y += boardSquareHeight / 5;
                    innerRect.Height -= boardSquareHeight / 5;
                    innerRect.X += boardSquareWidth / 5;
                    innerRect.Width -= boardSquareWidth / 5;

			        if((i + j) % 2 == 0)
			        {
                        graphics.FillRectangle(blackBrush, squareRect);
			        }

			        piece = board[i, j];
			        if(piece != null)
			        {
                        graphics.DrawImage(piece.imageFile, innerRect);
			        }
		        }
	        }

            Point from, to;
            getFromTo(out from, out to);
            if (isOnBoard(from)) graphics.DrawRectangle(greenPen, getSquareRect(from));
            if (isOnBoard(to)) graphics.DrawRectangle(redPen, getSquareRect(to));
        }

        private Rectangle getSquareRect(Point p)
        {
            Rectangle squareRect = new Rectangle(p.x * boardSquareWidth + boardHOffset,
                (boardSize - 1 - p.y) * boardSquareHeight + boardVOffset, boardSquareWidth, boardSquareHeight);
            return squareRect;
        }

        private static bool isOnBoard(Point p)
        {
            if (p.x >= 0 && p.y >= 0 && p.x < boardSize && p.y < boardSize) return true;
	        return false;
        }

        private bool inCheck()
        {
            return false;
        }

        private bool inCheckmate()
        {
	        return false;
        }

        private int getLegalMoves(List<ChessMove> moves)
        {
	        int numMoves = 0;

	        foreach(Piece p in board)
            {
                if (p == null || p.whosePiece != whoseTurn) continue;
                numMoves += p.getLegalMoves(moves);
            }

	        return numMoves;
        }

        private ChessMove bestMove(int depth, ref int totalMoves, CancellationToken token)
        {
            double[] BEST_VALUE = new double[] {double.MaxValue, double.MinValue};
	        ChessMove niceMove = new ChessMove();
            niceMove.origin.x = -1;
            List<ChessMove> moves = new List<ChessMove>();
	        totalMoves = 0;
	        int numMoves = getLegalMoves(moves);
	        if(numMoves == 0) return niceMove;
            if (token.IsCancellationRequested)
            {
                return moves[0];//we really should throw here, but...
            }
            
            // int bestIndex = 0;
            ChessMove bestMove = new ChessMove();
            //YOUR CODE HERE. Hint: call evaluatePosition with the same token passed in above...
            foreach(ChessMove move in moves)
            {
                Piece formerOccupant = null; // FIXME!!!
                makeMove(move, false);
                double value = evaluatePosition(depth - 1, ref totalMoves, token);
                undoMove(move, formerOccupant);
                // do something with value: use minimax strategy!
            }
            // return moves[bestIndex];
            return bestMove;
        }

        private double evaluatePosition(int depth, ref int totalMoves, CancellationToken token)
        {
            if(token.IsCancellationRequested)
            {
                return 0;//we really should throw here, but...
            }
            //YOUR CODE HERE: use the minimax algorithm
            //base case of recursion:
            if(depth == 0)
            {
                return objectiveFunction();
            }

            List<ChessMove> moves = new List<ChessMove>();
            int numMoves = getLegalMoves(moves);
            totalMoves += numMoves;
            if (numMoves == 0) return 0;
            double value = (whoseTurn == PLAYER.WHITE) ? -10000 : 10000;

            

            return value;
        }

        private double objectiveFunction()
        {
            double value = 0;
            //YOUR CODE HERE
            return value;
        }

        private void undoMove(ChessMove m, Piece formerOccupant)
        {
            Piece mover = board[m.destination.x, m.destination.y];
            mover.boardLocation = m.origin;
            board[m.origin.x, m.origin.y] = mover;
            board[m.destination.x, m.destination.y] = formerOccupant;
            whoseTurn = opponent(whoseTurn);
        }

        private void buttonSubmit_Click(object sender, EventArgs e)
        {
            if(radioButtonResign.Checked)
            {
                txtMessage.Text = "I accept your resignation. Well played.";
                btnSubmit.Enabled = false;
                return;
            }
            string prependMsg = "";
            if (radioButtonUserDecides.Checked)
            {
                Point from, to;
                if (!getFromTo(out from, out to, true)) return;
                ChessMove theMove = new ChessMove(ref from, ref to);
                if (!makeMove(theMove))
                {
                    txtMessage.Text = "Sorry, that move is illegal. Please try again.";
                    return;
                }
                moveHistory.Add(theMove); moveIndex++; undoMoveToolStripMenuItem.Enabled = true;
                drawBoard();
                if (inCheckmate())
                {
                    txtMessage.Text = "I am checkmated! Nice game.";
                    btnSubmit.Enabled = false;
                    return;
                }
                if (inCheck())
                {
                    prependMsg = "I am in check!";
                }
            }
            int totalMoves = 0;
            int lookAhead = 1;
            ChessMove toMake = new ChessMove();
            const int MAX_TIME_IN_MILLISECONDS = 5000;
            int timeLimitInMilliseconds = MAX_TIME_IN_MILLISECONDS;
            DateTime startTime = DateTime.Now;
            txtMessage.Text = prependMsg + "Thinking...";
            ChessGame.ActiveForm.UseWaitCursor = true;
            ChessGame.ActiveForm.Update();
            while (timeLimitInMilliseconds >= 0)
            {
                int tempMoveTotal = 0;
                var tokenSource = new CancellationTokenSource();
                var token = tokenSource.Token;
                Task<ChessMove> findMoveThread = new Task<ChessMove>(() => bestMove(lookAhead, ref tempMoveTotal, token), token);
                findMoveThread.Start();
                findMoveThread.Wait(timeLimitInMilliseconds);
                if (!findMoveThread.IsCompleted)
                {
                    tokenSource.Cancel();
                    findMoveThread.Wait();
                    break;
                }
                toMake = findMoveThread.Result;
                totalMoves = tempMoveTotal;
                DateTime currentTime = DateTime.Now;
                TimeSpan elapsedTime = currentTime - startTime;
                timeLimitInMilliseconds = MAX_TIME_IN_MILLISECONDS - (int)elapsedTime.TotalMilliseconds;
                lookAhead++;
            }
            ChessGame.ActiveForm.UseWaitCursor = false;
            makeMove(toMake);
            txtMessage.Text = "After considering " + totalMoves + " moves, I have decided to move from " + toMake.origin.x + "," + toMake.origin.y + " to " + toMake.destination.x + "," + toMake.destination.y + ".";
            if(inCheckmate())
            {
                txtMessage.Text += " Checkmate! Nice game.";
                btnSubmit.Enabled = false;
            }
            else if(inCheck())
            {
                txtMessage.Text += " Check!";
            }
            moveHistory.Add(toMake); moveIndex++; undoMoveToolStripMenuItem.Enabled = true;
            drawBoard();
        }

        private bool getFromTo(out Point from, out Point to, bool report = false)
        {
            bool retValue = true;
            try
            {
                string[] fromStr = textBox2.Text.Split(',');
                from = new Point(Int32.Parse(fromStr[0]), Int32.Parse(fromStr[1]));
                string[] toStr = textBox1.Text.Split(',');
                to = new Point(Int32.Parse(toStr[0]), Int32.Parse(toStr[1]));
            }
            catch (Exception)
            {
                retValue = false;
                from = to = new Point();
                if (report) txtMessage.Text = "Sorry, that move is not in the correct format. Please try again.";
            }
            return retValue;
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
   
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void undoMoveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            moveIndex--;
            recapitulateGame();
            drawBoard();
            redoMoveToolStripMenuItem.Enabled = true;
            if(moveIndex == 0)
            {
                undoMoveToolStripMenuItem.Enabled = false;
            }
        }

        private void redoMoveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            moveIndex++;
            recapitulateGame();
            drawBoard();
            undoMoveToolStripMenuItem.Enabled = true;
            if(moveIndex == moveHistory.Count)
            {
                redoMoveToolStripMenuItem.Enabled = false;
            }
        }

        private void recapitulateGame()
        {
            setupBoard();
            for(int i = 0; i < moveIndex; i++)
            {
                makeMove(moveHistory[i]);
            }
        }

        private void ChessGame_Load(object sender, EventArgs e)
        {
            drawBoard();
        }

        private void panelChessboard_Paint()
        {

        }

        private void startNewGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //setupBoard();
            setupBoardTestLabQ();
            drawBoard();
            this.Click += form_Click;
            moveHistory = new List<ChessMove>();
            moveIndex = 0;
            undoMoveToolStripMenuItem.Enabled = redoMoveToolStripMenuItem.Enabled = false;
            btnSubmit.Enabled = true;
        }
    }
}
