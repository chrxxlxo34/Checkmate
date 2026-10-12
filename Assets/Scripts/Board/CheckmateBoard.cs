using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Checkmate
{
    public class CheckmateBoard : MonoBehaviour
    {
        public ChessRules Rules = new ChessRules();
        public float SquareSize = 1f;
        public Transform BoardRoot;
        public Material LightSquare, DarkSquare, WhitePieceMaterial, BlackPieceMaterial, HighlightMaterial, SelectedMaterial;
        readonly Dictionary<Vector2Int, GameObject> pieceObjects = new Dictionary<Vector2Int, GameObject>();
        readonly Dictionary<Vector2Int, GameObject> squareObjects = new Dictionary<Vector2Int, GameObject>();
        Vector2Int? selected;
        List<ChessMove> legalFromSelected = new List<ChessMove>();
        public System.Action<string> OnStatus;
        public System.Action<ChessMove> OnMove;

        void Awake()
        {
            if (!BoardRoot) BoardRoot = transform;
            Rules.Reset();
            CreateMaterials();
            BuildBoard();
            RefreshPieces();
        }

        void Update()
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Ended &&
                    (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject(touch.fingerId)))
                    TryPickSquare(touch.position);
            }
            else if (Input.GetMouseButtonUp(0) &&
                     (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject()))
            {
                TryPickSquare(Input.mousePosition);
            }
        }

        void TryPickSquare(Vector2 screenPosition)
        {
            Camera cam = Camera.main;
            if (cam == null) return;
            Ray ray = cam.ScreenPointToRay(screenPosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                BoardSquare square = hit.collider.GetComponent<BoardSquare>();
                if (square != null) HandleSquare(square.position);
            }
        }

        void CreateMaterials()
        {
            LightSquare = MakeMat(new Color(.79f, .83f, .88f));
            DarkSquare = MakeMat(new Color(.25f, .32f, .42f));
            WhitePieceMaterial = MakeMat(new Color(.92f, .94f, .98f));
            BlackPieceMaterial = MakeMat(new Color(.12f, .16f, .23f));
            HighlightMaterial = MakeMat(new Color(.27f, .85f, .68f, .75f));
            SelectedMaterial = MakeMat(new Color(.98f, .72f, .26f, .85f));
        }
        Material MakeMat(Color c)
        {
            Shader s = Shader.Find("Universal Render Pipeline/Lit");
            if (s == null) s = Shader.Find("Standard");
            var m = new Material(s); m.color = c; return m;
        }
        void BuildBoard()
        {
            var baseObj = GameObject.CreatePrimitive(PrimitiveType.Cube); baseObj.name = "Board Base"; baseObj.transform.SetParent(BoardRoot, false);
            baseObj.transform.localPosition = new Vector3(3.5f * SquareSize, -.16f, 3.5f * SquareSize); baseObj.transform.localScale = new Vector3(8.18f * SquareSize, .28f, 8.18f * SquareSize); baseObj.GetComponent<Renderer>().material = MakeMat(new Color(.12f, .16f, .23f));
            for (int y = 0; y < 8; y++) for (int x = 0; x < 8; x++)
            {
                var sq = GameObject.CreatePrimitive(PrimitiveType.Cube); sq.name = $"Square_{ChessMove.Square(new Vector2Int(x, y))}"; sq.transform.SetParent(BoardRoot, false);
                sq.transform.localPosition = new Vector3(x * SquareSize, 0, y * SquareSize); sq.transform.localScale = new Vector3(.98f * SquareSize, .08f, .98f * SquareSize);
                sq.GetComponent<Renderer>().material = ((x + y) % 2 == 0) ? LightSquare : DarkSquare;
                var click = sq.AddComponent<BoardSquare>(); click.position = new Vector2Int(x, y); click.board = this;
                squareObjects[new Vector2Int(x, y)] = sq;
            }
        }
        public void ResetGame()
        {
            Rules.Reset();
            selected = null;
            legalFromSelected.Clear();
            ClearHighlights();
            RefreshPieces();
            OnStatus?.Invoke(Rules.StatusText);
        }

        public void HandleSquare(Vector2Int p)
        {
            if (Rules.IsGameOver) { OnStatus?.Invoke(Rules.StatusText); return; }
            if (selected.HasValue)
            {
                foreach (var m in legalFromSelected) if (m.to == p)
                {
                    Rules.ApplyMove(m); selected = null; ClearHighlights(); RefreshPieces(); OnMove?.Invoke(m);
                    OnStatus?.Invoke(Rules.StatusText); return;
                }
            }
            Piece piece = Rules.Board[p.x, p.y];
            if (!piece.IsEmpty && piece.side == Rules.Turn)
            {
                selected = p; legalFromSelected = Rules.LegalMoves(Rules.Turn).FindAll(m => m.from == p); ClearHighlights();
                squareObjects[p].GetComponent<Renderer>().material = SelectedMaterial;
                foreach (var m in legalFromSelected) squareObjects[m.to].GetComponent<Renderer>().material = HighlightMaterial;
            }
            else { selected = null; legalFromSelected.Clear(); ClearHighlights(); }
        }
        void ClearHighlights()
        {
            foreach (var kv in squareObjects) kv.Value.GetComponent<Renderer>().material = ((kv.Key.x + kv.Key.y) % 2 == 0) ? LightSquare : DarkSquare;
        }
        public void RefreshPieces()
        {
            foreach (var go in pieceObjects.Values) if (go) Destroy(go); pieceObjects.Clear();
            for (int y = 0; y < 8; y++) for (int x = 0; x < 8; x++)
            {
                Piece p = Rules.Board[x, y]; if (p.IsEmpty) continue;
                var root = new GameObject($"{p.side}_{p.type}_{x}_{y}"); root.transform.SetParent(BoardRoot, false); root.transform.localPosition = new Vector3(x * SquareSize, .08f, y * SquareSize);
                Material mat = p.side == Side.White ? WhitePieceMaterial : BlackPieceMaterial;
                BuildPieceShape(root.transform, p.type, mat);
                pieceObjects[new Vector2Int(x, y)] = root;
            }
        }
        void BuildPieceShape(Transform root, PieceType type, Material mat)
        {
            Add(PrimitiveType.Cylinder, new Vector3(0, .15f, 0), new Vector3(.29f, .12f, .29f));
            Add(PrimitiveType.Cylinder, new Vector3(0, .30f, 0), new Vector3(.22f, .12f, .22f));
            float h = type == PieceType.Pawn ? .34f : type == PieceType.King ? .85f : type == PieceType.Queen ? .72f : .55f;
            Add(type == PieceType.Knight ? PrimitiveType.Capsule : PrimitiveType.Cylinder, new Vector3(0, .48f + h * .35f, 0), new Vector3(type == PieceType.Pawn ? .13f : .18f, h * .55f, type == PieceType.Pawn ? .13f : .18f));
            if (type == PieceType.King) { Add(PrimitiveType.Cube, new Vector3(0, 1.02f, 0), new Vector3(.09f, .30f, .09f)); Add(PrimitiveType.Cube, new Vector3(0, 1.05f, 0), new Vector3(.28f, .08f, .08f)); }
            else if (type == PieceType.Queen) { Add(PrimitiveType.Sphere, new Vector3(0, 1.02f, 0), new Vector3(.22f, .22f, .22f)); }
            else if (type == PieceType.Pawn) { Add(PrimitiveType.Sphere, new Vector3(0, .72f, 0), new Vector3(.24f, .24f, .24f)); }
            void Add(PrimitiveType primitive, Vector3 pos, Vector3 scale)
            {
                var go = GameObject.CreatePrimitive(primitive); go.transform.SetParent(root, false); go.transform.localPosition = pos; go.transform.localScale = scale; go.GetComponent<Renderer>().material = mat;
                var col = go.GetComponent<Collider>(); if (col) Destroy(col);
            }
        }
    }
    public class BoardSquare : MonoBehaviour
    {
        public Vector2Int position; public CheckmateBoard board;
    }
}
