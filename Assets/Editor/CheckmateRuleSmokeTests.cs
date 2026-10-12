#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Checkmate
{
    public static class CheckmateRuleSmokeTests
    {
        static void Assert(bool condition, string message)
        {
            if (!condition) throw new System.Exception("Checkmate rules test failed: " + message);
            Debug.Log("PASS: " + message);
        }

        static bool Move(ChessRules rules, string from, string to)
        {
            return rules.TryMove(Parse(from), Parse(to));
        }

        static Vector2Int Parse(string square) => new Vector2Int(square[0] - 'a', square[1] - '1');

        [MenuItem("Checkmate/Tests/Run Chess Rule Smoke Tests")]
        public static void Run()
        {
            var r = new ChessRules();
            r.Reset();
            Assert(r.LegalMoves(Side.White).Count == 20, "starting position has 20 legal moves");
            Assert(Move(r, "e2", "e4"), "white pawn advances two squares");
            Assert(r.Turn == Side.Black, "turn changes after a legal move");
            Assert(!Move(r, "e7", "e4"), "illegal move is rejected");

            r.Reset();
            Assert(Move(r, "e2", "e4") && Move(r, "e7", "e5") && Move(r, "g1", "f3") &&
                   Move(r, "b8", "c6") && Move(r, "f1", "c4") && Move(r, "g8", "f6") && Move(r, "e1", "g1"),
                   "kingside castling sequence succeeds");
            Assert(r.Board[6, 0].type == PieceType.King && r.Board[5, 0].type == PieceType.Rook,
                   "castling moves both king and rook");

            r.Reset();
            Assert(Move(r, "e2", "e4") && Move(r, "a7", "a6") && Move(r, "e4", "e5") &&
                   Move(r, "d7", "d5") && Move(r, "e5", "d6"), "en passant sequence succeeds");
            Assert(r.Board[3, 4].IsEmpty && r.Board[3, 5].type == PieceType.Pawn,
                   "en passant removes the captured pawn");

            r.Reset();
            Assert(Move(r, "f2", "f3") && Move(r, "e7", "e5") && Move(r, "g2", "g4") && Move(r, "d8", "h4"),
                   "Fool's Mate sequence succeeds");
            Assert(r.IsCheckmate(Side.White) && r.IsGameOver, "checkmate ends the game");

            r.Reset();
            for (int i = 0; i < 2; i++)
                Assert(Move(r, "g1", "f3") && Move(r, "g8", "f6") && Move(r, "f3", "g1") && Move(r, "f6", "g8"),
                    "knight repetition cycle " + (i + 1));
            Assert(r.IsDrawByRepetition && r.IsGameOver, "threefold repetition is detected");
            Debug.Log("All Checkmate chess-rule smoke tests passed.");
        }
    }
}
#endif
