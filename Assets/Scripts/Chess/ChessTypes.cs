using System;
using UnityEngine;

namespace Checkmate
{
    public enum Side { White, Black }
    public enum PieceType { None, Pawn, Knight, Bishop, Rook, Queen, King }

    [Serializable]
    public struct Piece
    {
        public PieceType type;
        public Side side;
        public Piece(PieceType type, Side side) { this.type = type; this.side = side; }
        public bool IsEmpty => type == PieceType.None;
        public static Piece Empty => new Piece(PieceType.None, Side.White);
        public override string ToString() => IsEmpty ? "." : (side == Side.White ? "w" : "b") + type.ToString()[0];
    }

    [Serializable]
    public struct ChessMove
    {
        public Vector2Int from;
        public Vector2Int to;
        public PieceType promotion;
        public bool enPassant;
        public bool castle;
        public ChessMove(Vector2Int from, Vector2Int to, PieceType promotion = PieceType.Queen, bool enPassant = false, bool castle = false)
        { this.from = from; this.to = to; this.promotion = promotion; this.enPassant = enPassant; this.castle = castle; }
        public override string ToString() => $"{(char)('a' + from.x)}{from.y + 1}{(char)('a' + to.x)}{to.y + 1}" + (promotion != PieceType.Queen ? promotion.ToString()[0].ToString() : "");
        public static string Square(Vector2Int p) => $"{(char)('a' + p.x)}{p.y + 1}";
    }
}
