using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Checkmate
{
    public class CheckmateBootstrap : MonoBehaviour
    {
        CheckmateBoard board;
        Text status, whiteName, blackName, movesText;
        int moveCount;
        static Sprite avatarCircle;
        static Sprite uiWhiteSprite;
        void Start()
        {
            Application.targetFrameRate = 60;
            Input.multiTouchEnabled = false;
            Screen.sleepTimeout = SleepTimeout.SystemSetting;
            if (FindObjectOfType<EventSystem>() == null) { var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule)); }
            SetupCamera(); SetupLight(); CreateBoard(); BuildUI();
        }
        void SetupCamera()
        {
            Camera cam = Camera.main;
            if (!cam) { var c = new GameObject("Main Camera"); cam = c.AddComponent<Camera>(); c.tag = "MainCamera"; }
            cam.transform.position = new Vector3(3.5f, 10.5f, -8.0f); cam.transform.LookAt(new Vector3(3.5f, 0, 3.5f)); cam.fieldOfView = 43; cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(.055f, .075f, .12f);
        }
        void SetupLight()
        {
            var go = new GameObject("Soft Key Light"); var l = go.AddComponent<Light>(); l.type = LightType.Directional; l.intensity = 1.2f; go.transform.rotation = Quaternion.Euler(48, -30, 0);
            RenderSettings.ambientLight = new Color(.42f, .46f, .55f);
        }
        void CreateBoard()
        {
            var go = new GameObject("Chessboard"); board = go.AddComponent<CheckmateBoard>(); board.OnStatus += s => status.text = s; board.OnMove += m => { moveCount++; movesText.text = $"MOVES  {moveCount}"; };
        }
        void BuildUI()
        {
            var canvasGo = new GameObject("Glass UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = .5f;
            var root = canvasGo.transform;
            Panel(root, "TopPlayer", new Vector2(0, 1), new Vector2(0, 1), new Vector2(300, -88), new Vector2(560, 112), new Color(.07f, .10f, .17f, .90f));
            Avatar(root, "OpponentAvatar", new Vector2(0, 1), new Vector2(72, -88), "OP", new Color(.30f, .44f, .64f));
            blackName = Label(root, "OPPONENT  •  BLACK", new Vector2(0, 1), new Vector2(0, 1), new Vector2(350, -88), 28, Color.white);
            blackName.alignment = TextAnchor.MiddleLeft; blackName.rectTransform.sizeDelta = new Vector2(410, 64);
            Panel(root, "BottomPlayer", new Vector2(0, 0), new Vector2(0, 0), new Vector2(300, 88), new Vector2(560, 112), new Color(.07f, .10f, .17f, .92f));
            Avatar(root, "PlayerAvatar", new Vector2(0, 0), new Vector2(72, 88), "YOU", new Color(.20f, .60f, .48f));
            whiteName = Label(root, "YOU  •  WHITE", new Vector2(0, 0), new Vector2(0, 0), new Vector2(350, 88), 28, Color.white);
            whiteName.alignment = TextAnchor.MiddleLeft; whiteName.rectTransform.sizeDelta = new Vector2(410, 64);
            status = Label(root, "WHITE TO MOVE", new Vector2(1, 1), new Vector2(1, 1), new Vector2(-300, -55), 26, new Color(.65f, .93f, .83f));
            movesText = Label(root, "MOVES  0", new Vector2(1, 1), new Vector2(1, 1), new Vector2(-300, -105), 20, new Color(.8f, .85f, .94f));
            Button(root, "NEW GAME", new Vector2(1, 0), new Vector2(1, 0), new Vector2(-300, 52), new Vector2(250, 72), () => { board.ResetGame(); moveCount = 0; movesText.text = "MOVES  0"; status.text = board.Rules.StatusText; });
            Button(root, "FLIP BOARD", new Vector2(1, 0), new Vector2(1, 0), new Vector2(-580, 52), new Vector2(250, 72), () => { Camera.main.transform.RotateAround(new Vector3(3.5f, 0, 3.5f), Vector3.up, 180); });
            Label(root, "CHECKMATE", new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(0, -28), 24, new Color(.70f, .83f, 1f));
            status.text = board.Rules.StatusText;
        }
        static Sprite WhiteSprite()
        {
            if (uiWhiteSprite != null) return uiWhiteSprite;
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            texture.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
            texture.Apply();
            uiWhiteSprite = Sprite.Create(texture, new Rect(0, 0, 2, 2), new Vector2(.5f, .5f), 2);
            return uiWhiteSprite;
        }

        void Avatar(Transform root, string objectName, Vector2 anchor, Vector2 position, string initials, Color tint)
        {
            if (avatarCircle == null)
            {
                const int size = 64;
                var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
                for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
                {
                    float dx = x - (size - 1) * .5f, dy = y - (size - 1) * .5f;
                    texture.SetPixel(x, y, dx * dx + dy * dy <= size * size * .23f ? Color.white : Color.clear);
                }
                texture.Apply();
                avatarCircle = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(.5f, .5f), size);
            }
            var go = new GameObject(objectName, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(root, false);
            var rect = go.GetComponent<RectTransform>(); rect.anchorMin = anchor; rect.anchorMax = anchor;
            rect.anchoredPosition = position; rect.sizeDelta = new Vector2(72, 72);
            var image = go.GetComponent<Image>(); image.sprite = avatarCircle; image.color = tint; image.raycastTarget = false;
            var initialsText = Label(root, initials, anchor, anchor, position, 18, Color.white);
            initialsText.rectTransform.sizeDelta = new Vector2(72, 72);
            initialsText.raycastTarget = false;
        }

        void Panel(Transform root, string name, Vector2 amin, Vector2 amax, Vector2 pos, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(root, false); var r = go.GetComponent<RectTransform>(); r.anchorMin = amin; r.anchorMax = amax; r.anchoredPosition = pos; r.sizeDelta = size;
            var im = go.GetComponent<Image>(); im.sprite = WhiteSprite(); im.color = color; im.raycastTarget = false;
        }
        Text Label(Transform root, string value, Vector2 amin, Vector2 amax, Vector2 pos, int fontSize, Color color)
        {
            var go = new GameObject("Label", typeof(RectTransform), typeof(Text)); go.transform.SetParent(root, false); var r = go.GetComponent<RectTransform>(); r.anchorMin = amin; r.anchorMax = amax; r.anchoredPosition = pos; r.sizeDelta = new Vector2(900, 64);
            var t = go.GetComponent<Text>(); t.text = value; t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); t.fontSize = fontSize; t.color = color; t.alignment = TextAnchor.MiddleCenter; t.fontStyle = FontStyle.Bold; return t;
        }
        void Button(Transform root, string value, Vector2 amin, Vector2 amax, Vector2 pos, Vector2 size, UnityEngine.Events.UnityAction action)
        {
            var go = new GameObject(value, typeof(RectTransform), typeof(Image), typeof(Button)); go.transform.SetParent(root, false); var r = go.GetComponent<RectTransform>(); r.anchorMin = amin; r.anchorMax = amax; r.anchoredPosition = pos; r.sizeDelta = size;
            var im = go.GetComponent<Image>(); im.sprite = WhiteSprite(); im.color = new Color(.24f, .38f, .56f, .94f); var b = go.GetComponent<Button>(); b.onClick.AddListener(action);
            var txt = Label(go.transform, value, new Vector2(.5f, .5f), new Vector2(.5f, .5f), Vector2.zero, 22, Color.white); txt.rectTransform.sizeDelta = size;
        }
    }
}
