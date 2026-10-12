using System.Collections.Generic;
using UnityEngine;

namespace Checkmate
{
    public sealed class ChessRules
    {
        public Piece[,] Board { get; private set; } = new Piece[8, 8];
        public Side Turn { get; private set; } = Side.White;
        public int HalfmoveClock { get; private set; }
        public int FullmoveNumber { get; private set; } = 1;
        public Vector2Int EnPassantSquare { get; private set; } = new Vector2Int(-1, -1);
        public bool WhiteKingSide = true, WhiteQueenSide = true, BlackKingSide = true, BlackQueenSide = true;
        public List<ChessMove> History { get; } = new List<ChessMove>();
        readonly List<string> positionHistory = new List<string>();

        public bool IsCheckmatePosition => IsCheckmate(Turn);
        public bool IsStalematePosition => IsStalemate(Turn);
        public bool IsDrawByFiftyMoveRule => HalfmoveClock >= 100;
        public bool IsDrawByRepetition => CurrentPositionOccurrences() >= 3;
        public bool IsDrawByInsufficientMaterial => HasInsufficientMaterial();
        public bool IsGameOver => IsCheckmatePosition || IsStalematePosition || IsDrawByFiftyMoveRule || IsDrawByRepetition || IsDrawByInsufficientMaterial;

        public string StatusText
        {
            get
            {
                if (IsCheckmatePosition) return $"CHECKMATE — {(Turn == Side.White ? "BLACK" : "WHITE")} WINS";
                if (IsStalematePosition) return "DRAW — STALEMATE";
                if (IsDrawByFiftyMoveRule) return "DRAW — 50-MOVE RULE";
                if (IsDrawByRepetition) return "DRAW — THREEFOLD REPETITION";
                if (IsDrawByInsufficientMaterial) return "DRAW — INSUFFICIENT MATERIAL";
                if (IsCheck(Turn)) return $"{Turn.ToString().ToUpperInvariant()} IS IN CHECK";
                return $"{Turn.ToString().ToUpperInvariant()} TO MOVE";
            }
        }

        public void Reset()
        {
            Board = new Piece[8, 8]; Turn = Side.White; HalfmoveClock = 0; FullmoveNumber = 1;
            EnPassantSquare = new Vector2Int(-1, -1);
            WhiteKingSide = WhiteQueenSide = BlackKingSide = BlackQueenSide = true;
            History.Clear(); positionHistory.Clear();
            PieceType[] back = { PieceType.Rook, PieceType.Knight, PieceType.Bishop, PieceType.Queen, PieceType.King, PieceType.Bishop, PieceType.Knight, PieceType.Rook };
            for (int x = 0; x < 8; x++)
            {
                Board[x, 0] = new Piece(back[x], Side.White); Board[x, 1] = new Piece(PieceType.Pawn, Side.White);
                Board[x, 6] = new Piece(PieceType.Pawn, Side.Black); Board[x, 7] = new Piece(back[x], Side.Black);
            }
            positionHistory.Add(PositionKey());
        }

        public bool InBounds(Vector2Int p) => p.x >= 0 && p.x < 8 && p.y >= 0 && p.y < 8;
        public bool IsCheck(Side side)
        {
            Vector2Int king = FindKing(side);
            return king.x >= 0 && IsAttacked(king, Opp(side));
        }
        public bool IsCheckmate(Side side) => IsCheck(side) && LegalMoves(side).Count == 0;
        public bool IsStalemate(Side side) => !IsCheck(side) && LegalMoves(side).Count == 0;
        public static Side Opp(Side s) => s == Side.White ? Side.Black : Side.White;

        public Vector2Int FindKing(Side side)
        {
            for (int y = 0; y < 8; y++) for (int x = 0; x < 8; x++)
                if (Board[x, y].type == PieceType.King && Board[x, y].side == side) return new Vector2Int(x, y);
            return new Vector2Int(-1, -1);
        }

        public bool IsAttacked(Vector2Int square, Side bySide)
        {
            int pawnDir = bySide == Side.White ? 1 : -1;
            for (int dx = -1; dx <= 1; dx += 2)
            {
                Vector2Int p = new Vector2Int(square.x - dx, square.y - pawnDir);
                if (InBounds(p) && Board[p.x, p.y].type == PieceType.Pawn && Board[p.x, p.y].side == bySide) return true;
            }
            int[,] knight = { { 1, 2 }, { 2, 1 }, { 2, -1 }, { 1, -2 }, { -1, -2 }, { -2, -1 }, { -2, 1 }, { -1, 2 } };
            for (int i = 0; i < 8; i++) { Vector2Int p = square + new Vector2Int(knight[i, 0], knight[i, 1]); if (InBounds(p) && Board[p.x, p.y].type == PieceType.Knight && Board[p.x, p.y].side == bySide) return true; }
            for (int dx = -1; dx <= 1; dx++) for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                Vector2Int p = square + new Vector2Int(dx, dy); int step = 1;
                while (InBounds(p))
                {
                    Piece q = Board[p.x, p.y];
                    if (!q.IsEmpty)
                    {
                        if (q.side == bySide)
                        {
                            if (step == 1 && q.type == PieceType.King) return true;
                            if (dx == 0 || dy == 0) { if (q.type == PieceType.Rook || q.type == PieceType.Queen) return true; }
                            else if (q.type == PieceType.Bishop || q.type == PieceType.Queen) return true;
                        }
                        break;
                    }
                    p += new Vector2Int(dx, dy); step++;
                }
            }
            return false;
        }

        List<ChessMove> PseudoMoves(Side side)
        {
            var moves = new List<ChessMove>();
            for (int y = 0; y < 8; y++) for (int x = 0; x < 8; x++)
            {
                Piece p = Board[x, y]; if (p.IsEmpty || p.side != side) continue;
                Vector2Int from = new Vector2Int(x, y);
                if (p.type == PieceType.Pawn)
                {
                    int d = side == Side.White ? 1 : -1, start = side == Side.White ? 1 : 6;
                    Vector2Int one = new Vector2Int(x, y + d);
                    if (InBounds(one) && Board[one.x, one.y].IsEmpty)
                    {
                        moves.Add(new ChessMove(from, one, PieceType.Queen));
                        Vector2Int two = new Vector2Int(x, y + 2 * d);
                        if (y == start && Board[two.x, two.y].IsEmpty) moves.Add(new ChessMove(from, two));
                    }
                    for (int dx = -1; dx <= 1; dx += 2)
                    {
                        Vector2Int to = new Vector2Int(x + dx, y + d); if (!InBounds(to)) continue;
                        if (!Board[to.x, to.y].IsEmpty && Board[to.x, to.y].side != side) moves.Add(new ChessMove(from, to));
                        else if (to == EnPassantSquare) moves.Add(new ChessMove(from, to, PieceType.Queen, true));
                    }
                }
                else if (p.type == PieceType.Knight)
                {
                    int[,] ds = { { 1, 2 }, { 2, 1 }, { 2, -1 }, { 1, -2 }, { -1, -2 }, { -2, -1 }, { -2, 1 }, { -1, 2 } };
                    for (int i = 0; i < 8; i++) AddIfFreeOrEnemy(moves, from, new Vector2Int(x + ds[i, 0], y + ds[i, 1]), side);
                }
                else if (p.type == PieceType.King)
                {
                    for (int dx = -1; dx <= 1; dx++) for (int dy = -1; dy <= 1; dy++) if (dx != 0 || dy != 0) AddIfFreeOrEnemy(moves, from, new Vector2Int(x + dx, y + dy), side);
                    if (!IsCheck(side))
                    {
                        if (side == Side.White && from == new Vector2Int(4, 0))
                        {
                            if (WhiteKingSide && Board[5, 0].IsEmpty && Board[6, 0].IsEmpty && Board[7, 0].type == PieceType.Rook && Board[7, 0].side == Side.White && !IsAttacked(new Vector2Int(5, 0), Side.Black) && !IsAttacked(new Vector2Int(6, 0), Side.Black)) moves.Add(new ChessMove(from, new Vector2Int(6, 0), PieceType.Queen, false, true));
                            if (WhiteQueenSide && Board[1, 0].IsEmpty && Board[2, 0].IsEmpty && Board[3, 0].IsEmpty && Board[0, 0].type == PieceType.Rook && Board[0, 0].side == Side.White && !IsAttacked(new Vector2Int(3, 0), Side.Black) && !IsAttacked(new Vector2Int(2, 0), Side.Black)) moves.Add(new ChessMove(from, new Vector2Int(2, 0), PieceType.Queen, false, true));
                        }
                        if (side == Side.Black && from == new Vector2Int(4, 7))
                        {
                            if (BlackKingSide && Board[5, 7].IsEmpty && Board[6, 7].IsEmpty && Board[7, 7].type == PieceType.Rook && Board[7, 7].side == Side.Black && !IsAttacked(new Vector2Int(5, 7), Side.White) && !IsAttacked(new Vector2Int(6, 7), Side.White)) moves.Add(new ChessMove(from, new Vector2Int(6, 7), PieceType.Queen, false, true));
                            if (BlackQueenSide && Board[1, 7].IsEmpty && Board[2, 7].IsEmpty && Board[3, 7].IsEmpty && Board[0, 7].type == PieceType.Rook && Board[0, 7].side == Side.Black && !IsAttacked(new Vector2Int(3, 7), Side.White) && !IsAttacked(new Vector2Int(2, 7), Side.White)) moves.Add(new ChessMove(from, new Vector2Int(2, 7), PieceType.Queen, false, true));
                        }
                    }
                }
                else
                {
                    bool diag = p.type == PieceType.Bishop || p.type == PieceType.Queen;
                    bool straight = p.type == PieceType.Rook || p.type == PieceType.Queen;
                    for (int dx = -1; dx <= 1; dx++) for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0) continue;
                        if ((dx == 0 || dy == 0) && !straight) continue;
                        if (dx != 0 && dy != 0 && !diag) continue;
                        Vector2Int to = from + new Vector2Int(dx, dy);
                        while (InBounds(to))
                        {
                            if (Board[to.x, to.y].IsEmpty) moves.Add(new ChessMove(from, to));
                            else { if (Board[to.x, to.y].side != side) moves.Add(new ChessMove(from, to)); break; }
                            to += new Vector2Int(dx, dy);
                        }
                    }
                }
            }
            return moves;
        }
        void AddIfFreeOrEnemy(List<ChessMove> moves, Vector2Int from, Vector2Int to, Side side)
        { if (InBounds(to) && (Board[to.x, to.y].IsEmpty || Board[to.x, to.y].side != side)) moves.Add(new ChessMove(from, to)); }

        public List<ChessMove> LegalMoves(Side side)
        {
            var result = new List<ChessMove>();
            foreach (var m in PseudoMoves(side))
            {
                Piece captured = Board[m.to.x, m.to.y], moving = Board[m.from.x, m.from.y];
                Vector2Int oldEp = EnPassantSquare;
                ApplyRaw(m);
                bool safe = !IsCheck(side);
                Board[m.from.x, m.from.y] = moving; Board[m.to.x, m.to.y] = captured;
                if (m.enPassant) Board[m.to.x, m.from.y] = new Piece(PieceType.Pawn, Opp(side));
                if (m.castle)
                {
                    int y = m.from.y;
                    if (m.to.x == 6) { Board[7, y] = new Piece(PieceType.Rook, side); Board[5, y] = Piece.Empty; }
                    else { Board[0, y] = new Piece(PieceType.Rook, side); Board[3, y] = Piece.Empty; }
                }
                EnPassantSquare = oldEp;
                if (safe) result.Add(m);
            }
            return result;
        }

        public bool TryMove(Vector2Int from, Vector2Int to)
        {
            if (IsGameOver) return false;
            foreach (var m in LegalMoves(Turn))
                if (m.from == from && m.to == to) { ApplyMove(m); return true; }
            return false;
        }
        public void ApplyMove(ChessMove m)
        {
            if (IsGameOver) return;
            Piece mover = Board[m.from.x, m.from.y], target = Board[m.to.x, m.to.y];
            bool pawnMove = mover.type == PieceType.Pawn;
            if (mover.type == PieceType.King)
            { if (mover.side == Side.White) WhiteKingSide = WhiteQueenSide = false; else BlackKingSide = BlackQueenSide = false; }
            if (mover.type == PieceType.Rook)
            {
                if (m.from == new Vector2Int(0, 0)) WhiteQueenSide = false; if (m.from == new Vector2Int(7, 0)) WhiteKingSide = false;
                if (m.from == new Vector2Int(0, 7)) BlackQueenSide = false; if (m.from == new Vector2Int(7, 7)) BlackKingSide = false;
            }
            if (target.type == PieceType.Rook)
            {
                if (m.to == new Vector2Int(0, 0)) WhiteQueenSide = false; if (m.to == new Vector2Int(7, 0)) WhiteKingSide = false;
                if (m.to == new Vector2Int(0, 7)) BlackQueenSide = false; if (m.to == new Vector2Int(7, 7)) BlackKingSide = false;
            }
            if (m.enPassant) Board[m.to.x, m.from.y] = Piece.Empty;
            Board[m.from.x, m.from.y] = Piece.Empty;
            if (m.castle)
            {
                int y = m.from.y;
                if (m.to.x == 6) { Board[5, y] = Board[7, y]; Board[7, y] = Piece.Empty; }
                else { Board[3, y] = Board[0, y]; Board[0, y] = Piece.Empty; }
            }
            if (mover.type == PieceType.Pawn && (m.to.y == 0 || m.to.y == 7)) mover.type = m.promotion;
            Board[m.to.x, m.to.y] = mover;
            EnPassantSquare = (mover.type == PieceType.Pawn && Mathf.Abs(m.to.y - m.from.y) == 2) ? new Vector2Int(m.from.x, (m.from.y + m.to.y) / 2) : new Vector2Int(-1, -1);
            HalfmoveClock = (pawnMove || !target.IsEmpty || m.enPassant) ? 0 : HalfmoveClock + 1;
            History.Add(m); if (Turn == Side.Black) FullmoveNumber++; Turn = Opp(Turn);
            positionHistory.Add(PositionKey());
        }
        int CurrentPositionOccurrences()
        {
            if (positionHistory.Count == 0) return 0;
            string current = PositionKey(); int count = 0;
            for (int i = 0; i < positionHistory.Count; i++) if (positionHistory[i] == current) count++;
            return count;
        }

        string PositionKey()
        {
            var key = new System.Text.StringBuilder(80);
            for (int y = 0; y < 8; y++) for (int x = 0; x < 8; x++)
            {
                Piece p = Board[x, y];
                key.Append(p.IsEmpty ? '.' : (p.side == Side.White ? 'w' : 'b'));
                key.Append(p.IsEmpty ? '.' : (char)('A' + (int)p.type));
            }
            key.Append('|').Append(Turn).Append('|')
                .Append(WhiteKingSide ? 'K' : '-').Append(WhiteQueenSide ? 'Q' : '-')
                .Append(BlackKingSide ? 'k' : '-').Append(BlackQueenSide ? 'q' : '-')
                .Append('|').Append(EnPassantSquare.x).Append(',').Append(EnPassantSquare.y);
            return key.ToString();
        }

        bool HasInsufficientMaterial()
        {
            int nonKings = 0; int bishopSquareColor = -1; bool bishopsSameColor = true;
            for (int y = 0; y < 8; y++) for (int x = 0; x < 8; x++)
            {
                Piece p = Board[x, y]; if (p.IsEmpty || p.type == PieceType.King) continue;
                nonKings++;
                if (p.type == PieceType.Pawn || p.type == PieceType.Rook || p.type == PieceType.Queen) return false;
                if (p.type == PieceType.Bishop)
                {
                    int color = (x + y) & 1;
                    if (bishopSquareColor < 0) bishopSquareColor = color;
                    else if (bishopSquareColor != color) bishopsSameColor = false;
                }
            }
            if (nonKings <= 1) return true;
            if (nonKings > 1 && bishopsSameColor)
            {
                bool onlyBishops = true;
                for (int y = 0; y < 8; y++) for (int x = 0; x < 8; x++)
                { Piece p = Board[x, y]; if (!p.IsEmpty && p.type != PieceType.King && p.type != PieceType.Bishop) onlyBishops = false; }
                if (onlyBishops) return true;
            }
            return false;
        }

        void ApplyRaw(ChessMove m)
        {
            Piece p = Board[m.from.x, m.from.y];
            if (m.enPassant) Board[m.to.x, m.from.y] = Piece.Empty;
            Board[m.from.x, m.from.y] = Piece.Empty;
            if (m.castle) { int y = m.from.y; if (m.to.x == 6) { Board[5, y] = Board[7, y]; Board[7, y] = Piece.Empty; } else { Board[3, y] = Board[0, y]; Board[0, y] = Piece.Empty; } }
            if (p.type == PieceType.Pawn && (m.to.y == 0 || m.to.y == 7)) p.type = PieceType.Queen;
            Board[m.to.x, m.to.y] = p;
        }
    }
}
