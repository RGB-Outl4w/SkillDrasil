using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SkillDrasil
{
    // Sprites cut from the embedded atlas (assets/atlas.png, built by tools/render-icons.ps1).
    static class Icons
    {
        static Dictionary<string, Sprite> sprites;

        public static Sprite Get(string name)
        {
            if (sprites == null) Load();
            if (sprites.TryGetValue(name, out var s)) return s;
            Plugin.Log.LogWarning($"Missing icon '{name}'");
            return null;
        }

        static void Load()
        {
            sprites = new Dictionary<string, Sprite>();
            var asm = typeof(Icons).Assembly;
            byte[] png;
            using (var s = asm.GetManifestResourceStream("SkillDrasil.atlas.png"))
            using (var ms = new MemoryStream())
            {
                s.CopyTo(ms);
                png = ms.ToArray();
            }
            string[] index;
            using (var r = new StreamReader(asm.GetManifestResourceStream("SkillDrasil.atlas.txt")))
                index = r.ReadToEnd().Split('\n');

            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, true) { name = "SkillDrasilAtlas" };
            // byte[] overload via reflection: net48 can't even resolve the overload set (one takes ReadOnlySpan).
            typeof(ImageConversion).GetMethod("LoadImage", new[] { typeof(Texture2D), typeof(byte[]), typeof(bool) })
                .Invoke(null, new object[] { tex, png, false });
            tex.filterMode = FilterMode.Trilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            var head = index[0].Split(' ');
            int cell = int.Parse(head[0]), cols = int.Parse(head[1]);
            for (int i = 1; i < index.Length; i++)
            {
                string name = index[i].Trim();
                if (name.Length == 0) continue;
                int c = (i - 1) % cols, row = (i - 1) / cols;
                var rect = new Rect(c * cell, tex.height - (row + 1) * cell, cell, cell); // texture origin is bottom-left
                var sprite = Sprite.Create(tex, rect, new Vector2(0.5f, 0.5f), 100f);
                sprite.name = name;
                sprites[name] = sprite;
            }
        }
    }
}
