using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

namespace WakeTheWalls.EditorTools
{
    // Restyles the UI prefabs to the Summit look: navy sheets, red cards and buttons, square corners, Big Shoulders titles, Plex Mono labels, Instrument Sans text.
    public static class SummitTheme
    {
        const string FontDir = "Assets/Art/UI/Fonts/";
        const string ShapeDir = "Assets/Art/UI/Shapes/";
        const string IconDir = "Assets/Art/UI/Icons/";
        const string PrefabDir = "Assets/_Project/Prefabs/UI/";

        static readonly Color Navy = Hex("002E6D");
        static readonly Color NavyDeep = Hex("00224F");
        static readonly Color NavyNight = Hex("00112B");
        static readonly Color Red = Hex("D00D2D");
        static readonly Color White = Color.white;
        static readonly Color Soft = Hex("D5DEEB");
        static readonly Color Count = Hex("C9D4E5");
        static readonly Color Pink = Hex("FFE3E7");
        static readonly Color PinkSoft = Hex("FFD0D6");
        static readonly Color Clear = new Color(1, 1, 1, 0);
        static Color A(Color c, float a) => new Color(c.r, c.g, c.b, a);

        static TMP_FontAsset title, titleBold, label, bold, medium, regular;
        static Sprite frame, disc, stampEmpty, stampEmptyDark, ringDashed, reticle, pause, seal;

        [MenuItem("Wake the Walls/Apply Summit UI")]
        public static void Run()
        {
            ImportArt();
            title = Font("BigShouldersDisplay-Black");
            titleBold = title;
            label = Font("IBMPlexMono-Medium");
            bold = Font("InstrumentSans-Bold");
            medium = Font("InstrumentSans-SemiBold");
            regular = Font("InstrumentSans-Regular");

            Edit("StartScreen", Start);
            Edit("ScanScreen", Scan);
            Edit("ARHud", Hud);
            Edit("InfoPanel", Info);
            Edit("TrackingLostOverlay", Lost);
            Edit("CompletionScreen", Complete);
            Edit("HelpOverlay", Help);

            var narrator = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Narrator/Pearl.prefab");
            if (narrator != null)
            {
                var root = PrefabUtility.LoadPrefabContents("Assets/_Project/Prefabs/Narrator/Pearl.prefab");
                var n = root.GetComponent<WakeTheWalls.Narrator.Narrator>();
                if (n != null) SetField(n, "spacing", 144f);
                PrefabUtility.SaveAsPrefabAsset(root, "Assets/_Project/Prefabs/Narrator/Pearl.prefab");
                PrefabUtility.UnloadPrefabContents(root);
            }

            AssetDatabase.SaveAssets();
            Debug.Log("Summit UI applied.");
        }

        // ---------- screens ----------

        static void Start(GameObject root)
        {
            var tag = Rect(root, "SafeTop/Tag");
            Tag(tag, new Vector2(510, 78));
            Label(Text(root, "SafeTop/Tag/Label"), 28.5f, White, 12.6f, 36);

            var sheet = Rect(root, "Sheet");
            Fill(sheet, Navy);
            Accent(sheet, White);

            var eyebrow = Child(sheet, "Eyebrow", () => NewText(sheet, "Eyebrow"));
            eyebrow.text = "AR MURAL EXPERIENCE";
            Label(eyebrow, 30, White, 14, 0);
            TopLeft(eyebrow.rectTransform, 60, -102, 900, 42);

            var t = Text(root, "Sheet/Title");
            Title(t, "Wake the\nWalls", 120, White, -2f, -14f);
            TopLeftStretch(t.rectTransform, 60, -150, 270);

            var intro = Text(root, "Sheet/Intro");
            Paragraph(intro, 45, Soft, 22);
            TopLeftStretch(intro.rectTransform, 60, -492, 230);

            Stamps(root, "Sheet/Stamps", Count);
            TopLeftStretch(Rect(root, "Sheet/Stamps"), 60, -753, 84);

            Primary(root, "Sheet/StartButton");
            TopLeftStretch(Rect(root, "Sheet/StartButton"), 60, -912, 156);

            Link(Text(root, "Sheet/HowItWorks/Label"), White);
        }

        static void Scan(GameObject root)
        {
            Tint(Img(root, "Scrim"), NavyNight);

            Square(root, "Content/BackButton", 108, NavyDeep, White, "summit_icon_back");
            Tag(Rect(root, "Content/Count"), new Vector2(396, 78));
            Label(Text(root, "Content/Count/Label"), 28.5f, White, 12.6f, 0, TextAlignmentOptions.Center);

            var card = Rect(root, "Content/Card");
            Fill(card, Red);
            Accent(card, White);
            card.sizeDelta = new Vector2(card.sizeDelta.x, 768);

            var dot = Img(root, "Content/Card/Dot");
            dot.sprite = null;
            dot.color = White;
            TopLeft(dot.rectTransform, 72, -96, 24, 24);

            var status = Text(root, "Content/Card/Status");
            Label(status, 30, White, 14, 0);
            TopLeft(status.rectTransform, 120, -84, 800, 42);

            var t = Text(root, "Content/Card/Title");
            Title(t, "Point at the\nwhole wall", 81, White, -1.5f, -10f);
            TopLeft(t.rectTransform, 72, -150, 906, 200);

            var body = Text(root, "Content/Card/Body");
            Paragraph(body, 39, Pink, 22);
            TopLeft(body.rectTransform, 72, -404, 906, 130);

            var hint = Rect(root, "Content/Card/Hint");
            TopLeft(hint, 72, -566, 906, 160);
            var div = Img(root, "Content/Card/Hint/Divider");
            if (div != null) div.color = A(White, 0.3f);
            Paragraph(Text(root, "Content/Card/Hint/Text"), 34.5f, PinkSoft, 16);

            var frameImg = Img(root, "ScanFrame");
            Tint(frameImg, Red);
            // keep the frame clear of the red card at the bottom
            frameImg.rectTransform.anchoredPosition = new Vector2(0, 260);
            frameImg.rectTransform.sizeDelta = new Vector2(760, 760);
            var ret = Img(root, "ScanFrame/Reticle");
            ret.sprite = reticle;
            ret.color = White;
            ret.rectTransform.sizeDelta = new Vector2(54, 54);
        }

        static void Hud(GameObject root)
        {
            var wall = Rect(root, "Content/WallLabel");
            Tag(wall, wall.sizeDelta);
            var layout = wall.GetComponent<HorizontalOrVerticalLayoutGroup>();
            if (layout != null) layout.padding = new RectOffset(36, 36, 21, 21);
            Label(Text(root, "Content/WallLabel/Label"), 28.5f, White, 12.6f, 0);
            Title(Text(root, "Content/WallLabel/Title"), null, 66, White, -1f, 0);

            string[] buttons = { "InfoButton", "SoundButton", "ReplayButton", "HelpButton" };
            string[] icons = { "summit_icon_info", "summit_icon_sound_on", "summit_icon_replay", "summit_icon_help" };
            for (int i = 0; i < buttons.Length; i++)
            {
                var b = Square(root, "Content/" + buttons[i], 108, NavyDeep, White, icons[i]);
                b.anchoredPosition = new Vector2(-60, -27 - 144 * i);
            }

            var hud = root.GetComponent<WakeTheWalls.UI.ARHud>();
            if (hud != null)
            {
                SetField(hud, "soundOnSprite", Icon("summit_icon_sound_on"));
                SetField(hud, "soundOffSprite", Icon("summit_icon_sound_off"));
            }
        }

        static void Info(GameObject root)
        {
            var sheet = Rect(root, "Sheet");
            Fill(sheet, Navy);
            Accent(sheet, White);
            var bar = Img(root, "Sheet/Handle/Bar");
            bar.sprite = null;
            bar.color = A(White, 0.35f);
            bar.rectTransform.sizeDelta = new Vector2(108, 12);

            Label(Text(root, "Sheet/Header/Label"), 30, White, 14, 0);
            Title(Text(root, "Sheet/Header/Title"), null, 96, White, -1.8f, -10f);
            Square(root, "Sheet/Header/CloseButton", 108, Clear, A(White, 0.6f), "summit_icon_close", White);

            foreach (Transform child in sheet)
                if (child.name == "Divider") child.GetComponent<Image>().color = A(White, 0.18f);

            Label(Text(root, "Sheet/Where/Label"), 28.5f, A(White, 0.6f), 14, 0);
            Paragraph(Text(root, "Sheet/Where/Value"), 40.5f, White, 12, medium);
            Paragraph(Text(root, "Sheet/Story"), 37.5f, Soft, 18);
            Primary(root, "Sheet/BackButton");
        }

        static void Lost(GameObject root)
        {
            var scrim = Img(root, "Scrim");
            scrim.color = new Color(NavyNight.r, NavyNight.g, NavyNight.b, 0.6f);

            Tag(Rect(root, "Content/WallLabel"), Rect(root, "Content/WallLabel").sizeDelta, 0.92f);
            Label(Text(root, "Content/WallLabel/Label"), 28.5f, new Color(1, 1, 1, 0.75f), 12.6f, 0);
            Title(Text(root, "Content/WallLabel/Title"), null, 45, White, 0, 0);

            var ring = Img(root, "PausedIcon");
            ring.sprite = ringDashed;
            ring.color = Red;
            ring.rectTransform.sizeDelta = new Vector2(204, 204);
            var bars = Img(root, "PausedIcon/Pause");
            bars.sprite = pause;
            bars.color = White;
            bars.rectTransform.sizeDelta = new Vector2(84, 84);

            Title(Text(root, "Title"), "The wall slipped\nout of view", 76, White, -1.5f, -10f, TextAlignmentOptions.Top);
            Paragraph(Text(root, "Body"), 42, new Color(1, 1, 1, 0.75f), 22, null, TextAlignmentOptions.Top);

            Outline(root, "Content/BackOnItButton", White, White);
            Link(Text(root, "Content/ScanOtherButton/Label"), White);
        }

        static void Complete(GameObject root)
        {
            root.GetComponent<Image>().color = NavyDeep;
            Label(Text(root, "Content/Label"), 30, White, 14, 0);
            Square(root, "Content/HomeButton", 108, Clear, A(White, 0.6f), "summit_icon_home", White);

            var stamp = Img(root, "Content/Stamp");
            stamp.sprite = seal;
            stamp.color = White;
            stamp.rectTransform.sizeDelta = new Vector2(564, 564);
            var small = new Color(1, 1, 1, 0.8f);
            Label(Text(root, "Content/Stamp/Top"), 27, small, 18, 0, TextAlignmentOptions.Center);
            var bottom = Text(root, "Content/Stamp/Bottom");
            if (bottom != null) bottom.text = "WAKE THE WALLS";
            Label(bottom, 27, small, 18, 0, TextAlignmentOptions.Center);
            Title(Text(root, "Content/Stamp/Number"), null, 204, White, -1.5f, 0, TextAlignmentOptions.Center, titleBold);

            Title(Text(root, "Content/Title"), "You woke the wall", 114, White, -2f, -10f, TextAlignmentOptions.Top);
            Paragraph(Text(root, "Content/Remaining"), 45, Soft, 22, null, TextAlignmentOptions.Top);
            Stamps(root, "Content/Stamps", Count);

            var screen = root.GetComponent<WakeTheWalls.UI.CompletionScreen>();
            if (screen != null)
            {
                SetField(screen, "titleWithName", "You woke {0}");
                SetField(screen, "titleWithoutName", "Wall {0:00} is awake");
            }

            Primary(root, "Content/NextButton");
            Outline(root, "Content/ReplayButton", White, White);
            Outline(root, "Content/AboutButton", White, White);
        }

        static void Help(GameObject root)
        {
            var backdrop = Img(root, "Backdrop");
            backdrop.color = new Color(NavyNight.r, NavyNight.g, NavyNight.b, backdrop.color.a);

            var sheet = Rect(root, "Sheet");
            Fill(sheet, Navy);
            Accent(sheet, White);
            var bar = Img(root, "Sheet/Handle/Bar");
            bar.sprite = null;
            bar.color = A(White, 0.35f);
            bar.rectTransform.sizeDelta = new Vector2(108, 12);

            Label(Text(root, "Sheet/Header/Label"), 30, White, 14, 0);
            Title(Text(root, "Sheet/Header/Title"), "Point. Wait.\nWatch it move.", 96, White, -1.8f, -10f);

            string[] tiles = { "icon_info", "icon_sound_on", "icon_replay", "icon_help" };
            foreach (Transform step in sheet)
            {
                if (!step.name.StartsWith("Step")) continue;
                Title(Text(step.gameObject, "Number"), null, 66, Red, -1f, 0);
                Paragraph(Text(step.gameObject, "Text/Heading"), 45, White, 8, medium);
                Paragraph(Text(step.gameObject, "Text/Body"), 40.5f, Soft, 18);
                var icons = step.Find("Text/Icons");
                if (icons == null) continue;
                foreach (string tile in tiles)
                {
                    var t = icons.Find(tile);
                    if (t == null) continue;
                    var img = t.GetComponent<Image>();
                    img.sprite = null;
                    img.color = NavyDeep;
                    Border((RectTransform)t, White);
                    var icon = t.Find("Icon")?.GetComponent<Image>();
                    if (icon != null) icon.sprite = Icon("summit_" + tile);
                }
            }
            Primary(root, "Sheet/GotItButton");
        }

        // ---------- building blocks ----------

        static void Tag(RectTransform rect, Vector2 size, float alpha = 1f)
        {
            var img = rect.GetComponent<Image>();
            img.sprite = null;
            img.type = Image.Type.Simple;
            img.color = new Color(NavyDeep.r, NavyDeep.g, NavyDeep.b, alpha);
            if (rect.GetComponent<LayoutGroup>() == null && rect.GetComponent<ContentSizeFitter>() == null) rect.sizeDelta = size;
            Border(rect, White);
        }

        static RectTransform Square(GameObject root, string path, float size, Color fill, Color border, string icon, Color? iconColor = null)
        {
            var rect = Rect(root, path);
            var img = rect.GetComponent<Image>();
            img.sprite = null;
            img.type = Image.Type.Simple;
            img.color = fill;
            rect.sizeDelta = new Vector2(size, size);
            Border(rect, border);
            var ic = rect.Find("Icon")?.GetComponent<Image>();
            if (ic != null)
            {
                ic.sprite = Icon(icon);
                ic.color = iconColor ?? White;
                ic.rectTransform.sizeDelta = new Vector2(size * 0.55f, size * 0.55f);
            }
            return rect;
        }

        static void Primary(GameObject root, string path)
        {
            var rect = Rect(root, path);
            var img = rect.GetComponent<Image>();
            img.sprite = null;
            img.type = Image.Type.Simple;
            img.color = Red;
            Solid(rect);
            RemoveBorder(rect);
            var l = rect.Find("Label")?.GetComponent<TMP_Text>();
            if (l != null) { Label(l, 42, White, 10, 66); l.font = bold; }
            var arrow = rect.Find("Arrow")?.GetComponent<Image>();
            if (arrow != null)
            {
                arrow.sprite = Icon("summit_icon_arrow_right");
                arrow.color = White;
                arrow.rectTransform.sizeDelta = new Vector2(54, 54);
                arrow.rectTransform.anchoredPosition = new Vector2(-60, 0);
            }
        }

        static void Outline(GameObject root, string path, Color fill, Color line)
        {
            var rect = Rect(root, path);
            var img = rect.GetComponent<Image>();
            img.sprite = null;
            img.type = Image.Type.Simple;
            img.color = fill == White && line == White ? new Color(1, 1, 1, 0) : fill;
            Border(rect, line);
            Solid(rect);
            var l = rect.Find("Label")?.GetComponent<TMP_Text>();
            if (l != null) { Label(l, 39, line, 10, 0, TextAlignmentOptions.Center); l.font = bold; }
        }

        // buttons keep their own colour instead of picking up the grey tint
        static void Solid(RectTransform rect)
        {
            var b = rect.GetComponent<Selectable>();
            if (b == null) return;
            var c = b.colors;
            c.normalColor = c.highlightedColor = c.selectedColor = c.disabledColor = Color.white;
            c.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1);
            b.colors = c;
        }

        static void Border(RectTransform rect, Color color)
        {
            var b = rect.Find("Border");
            if (b == null)
            {
                var go = new GameObject("Border", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(rect, false);
                b = go.transform;
                var le = go.AddComponent<LayoutElement>();
                le.ignoreLayout = true;
            }
            var br = (RectTransform)b;
            br.anchorMin = Vector2.zero;
            br.anchorMax = Vector2.one;
            br.offsetMin = Vector2.zero;
            br.offsetMax = Vector2.zero;
            br.SetAsLastSibling();
            var img = b.GetComponent<Image>();
            img.sprite = frame;
            img.type = Image.Type.Sliced;
            img.color = color;
            img.raycastTarget = false;
        }

        static void RemoveBorder(RectTransform rect)
        {
            var b = rect.Find("Border");
            if (b != null) Object.DestroyImmediate(b.gameObject);
        }

        static void Accent(RectTransform sheet, Color? color = null)
        {
            var a = sheet.Find("Accent");
            if (a == null)
            {
                var go = new GameObject("Accent", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
                go.transform.SetParent(sheet, false);
                go.GetComponent<LayoutElement>().ignoreLayout = true;
                a = go.transform;
            }
            var r = (RectTransform)a;
            r.anchorMin = new Vector2(0, 1);
            r.anchorMax = new Vector2(1, 1);
            r.pivot = new Vector2(0.5f, 1);
            r.sizeDelta = new Vector2(0, 12);
            r.anchoredPosition = Vector2.zero;
            r.SetAsLastSibling();
            var img = a.GetComponent<Image>();
            img.sprite = null;
            img.color = color ?? Red;
            img.raycastTarget = false;
        }

        static void Fill(RectTransform rect, Color color)
        {
            var img = rect.GetComponent<Image>();
            img.sprite = null;
            img.type = Image.Type.Simple;
            img.color = color;
        }

        static void Stamps(GameObject root, string path, Color countColor)
        {
            var row = Rect(root, path);
            var stamps = row.GetComponent<WakeTheWalls.UI.StampsRow>();
            if (stamps != null)
            {
                SetField(stamps, "emptySprite", stampEmptyDark != null ? stampEmptyDark : stampEmpty);
                SetField(stamps, "filledSprite", disc);
                SetField(stamps, "numberFont", label);
                SetField(stamps, "stampSize", 78f);
                SetField(stamps, "numberSize", 30f);
                SetField(stamps, "filledTilt", 0f);
                SetField(stamps, "emptyColor", White);
                SetField(stamps, "emptyNumberColor", White);
                SetField(stamps, "filledColor", White);
                SetField(stamps, "filledNumberColor", Red);
                SetField(stamps, "countFormat", "{0} of {1} awake");
            }
            var h = row.GetComponent<HorizontalLayoutGroup>();
            if (h != null) h.spacing = 24;
            var count = row.Find("Count")?.GetComponent<TMP_Text>();
            if (count != null) Paragraph(count, 33, countColor, 0, medium);
        }

        static void Label(TMP_Text t, float size, Color color, float spacing, float leftMargin, TextAlignmentOptions align = TextAlignmentOptions.MidlineLeft)
        {
            if (t == null) return;
            t.font = label;
            t.fontSize = size;
            t.enableAutoSizing = false;
            t.color = color;
            t.characterSpacing = spacing;
            t.fontStyle = FontStyles.UpperCase;
            t.alignment = align;
            t.margin = new Vector4(leftMargin, 0, 0, 0);
        }

        static void Title(TMP_Text t, string text, float size, Color color, float spacing, float lineSpacing,
            TextAlignmentOptions align = TextAlignmentOptions.TopLeft, TMP_FontAsset font = null)
        {
            if (t == null) return;
            if (text != null) t.text = text;
            t.font = font ?? title;
            t.fontSize = font == null ? size * 1.22f : size;
            t.enableAutoSizing = false;
            t.color = color;
            t.characterSpacing = 0;
            t.lineSpacing = lineSpacing;
            t.fontStyle = FontStyles.UpperCase;
            t.alignment = align;
        }

        static void Paragraph(TMP_Text t, float size, Color color, float lineSpacing, TMP_FontAsset font = null,
            TextAlignmentOptions align = TextAlignmentOptions.TopLeft)
        {
            if (t == null) return;
            t.font = font ?? regular;
            t.fontSize = size;
            t.enableAutoSizing = false;
            t.color = color;
            t.characterSpacing = 0;
            t.lineSpacing = lineSpacing;
            t.fontStyle = FontStyles.Normal;
            if (t.alignment != TextAlignmentOptions.Center && t.alignment != TextAlignmentOptions.Midline) t.alignment = align;
        }

        static void Link(TMP_Text t, Color color)
        {
            if (t == null) return;
            t.font = medium;
            t.fontSize = 39;
            t.color = color;
            t.characterSpacing = 0;
            t.fontStyle = FontStyles.Normal;
        }

        static TMP_Text NewText(RectTransform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            go.GetComponent<LayoutElement>().ignoreLayout = true;
            var t = go.GetComponent<TextMeshProUGUI>();
            t.raycastTarget = false;
            return t;
        }

        static TMP_Text Child(RectTransform parent, string name, System.Func<TMP_Text> make)
        {
            var existing = parent.Find(name);
            return existing != null ? existing.GetComponent<TMP_Text>() : make();
        }

        static void TopLeft(RectTransform r, float x, float y, float w, float h)
        {
            r.anchorMin = r.anchorMax = new Vector2(0, 1);
            r.pivot = new Vector2(0, 1);
            r.anchoredPosition = new Vector2(x, y);
            r.sizeDelta = new Vector2(w, h);
        }

        static void TopLeftStretch(RectTransform r, float side, float y, float h)
        {
            r.anchorMin = new Vector2(0, 1);
            r.anchorMax = new Vector2(1, 1);
            r.pivot = new Vector2(0.5f, 1);
            r.offsetMin = new Vector2(side, y - h);
            r.offsetMax = new Vector2(-side, y);
        }

        static void Tint(Image img, Color c)
        {
            if (img == null) return;
            img.color = new Color(c.r, c.g, c.b, img.color.a);
        }

        // ---------- plumbing ----------

        static void Edit(string prefab, System.Action<GameObject> change)
        {
            string path = PrefabDir + prefab + ".prefab";
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                change(root);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Summit UI: {prefab} failed: {e}");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        static RectTransform Rect(GameObject root, string path)
        {
            var t = root.transform.Find(path);
            if (t == null) throw new System.Exception("missing " + path);
            return (RectTransform)t;
        }

        static Image Img(GameObject root, string path) => Rect(root, path).GetComponent<Image>();

        static TMP_Text Text(GameObject root, string path)
        {
            var t = root.transform.Find(path);
            return t != null ? t.GetComponent<TMP_Text>() : null;
        }

        static void SetField(Object target, string field, object value)
        {
            var so = new SerializedObject(target);
            var p = so.FindProperty(field);
            if (p == null) { Debug.LogWarning($"Summit UI: no field {field} on {target.name}"); return; }
            switch (value)
            {
                case float f: p.floatValue = f; break;
                case string s: p.stringValue = s; break;
                case Color c: p.colorValue = c; break;
                case Object o: p.objectReferenceValue = o; break;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void ImportArt()
        {
            foreach (string guid in AssetDatabase.FindAssets("summit_ t:Texture2D", new[] { ShapeDir, IconDir }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var imp = (TextureImporter)AssetImporter.GetAtPath(path);
                imp.textureType = TextureImporterType.Sprite;
                imp.spriteImportMode = SpriteImportMode.Single;
                imp.alphaIsTransparency = true;
                imp.mipmapEnabled = false;
                if (path.EndsWith("summit_frame.png")) imp.spriteBorder = new Vector4(6, 6, 6, 6);
                imp.SaveAndReimport();
            }
            frame = Shape("summit_frame");
            disc = Shape("summit_disc");
            stampEmpty = Shape("summit_stamp_empty");
            stampEmptyDark = Shape("summit_stamp_empty_dark");
            ringDashed = Shape("summit_ring_dashed");
            reticle = Shape("summit_reticle");
            pause = Shape("summit_pause");
            seal = Shape("summit_seal");
        }

        static Sprite Shape(string name) => AssetDatabase.LoadAssetAtPath<Sprite>(ShapeDir + name + ".png");
        static Sprite Icon(string name) => AssetDatabase.LoadAssetAtPath<Sprite>(IconDir + name + ".png");

        static TMP_FontAsset Font(string file)
        {
            string assetPath = FontDir + file + " SDF.asset";
            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
            if (existing != null) return existing;

            var source = AssetDatabase.LoadAssetAtPath<Font>(FontDir + file + ".ttf");
            var asset = TMP_FontAsset.CreateFontAsset(source, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic, true);
            asset.name = file + " SDF";
            AssetDatabase.CreateAsset(asset, assetPath);
            asset.atlasTextures[0].name = file + " Atlas";
            AssetDatabase.AddObjectToAsset(asset.atlasTextures[0], asset);
            asset.material.name = file + " Material";
            AssetDatabase.AddObjectToAsset(asset.material, asset);

            const string chars = " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~×’‘“”–";
            asset.TryAddCharacters(chars);
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            return asset;
        }

        static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out var c);
            return c;
        }
    }
}
