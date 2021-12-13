using System;
using System.Drawing;
using Color = System.Windows.Media.Color;

using UtilNumerics;

namespace SoftRender {
	namespace Objects {
		public class Material {
			public class Texture {
				private Bitmap btmImage;
				private int Width;
				private int Height;
				public Texture(string filename) {
					btmImage = new Bitmap(filename);
					Width = btmImage.Width;
					Height = btmImage.Height;
				}
				
				public Color GetColor(Vector2 t) {
					int x = (int)(t.X * Width + Width) % Width;
					int y = (int)(t.X * Height + Height) % Height;
					System.Drawing.Color color = btmImage.GetPixel(x, y);
					return Color.FromArgb(color.A, color.R, color.G, color.B);
				}

			}

			public Color AmbientColor;
			public Color DispersiveColor;
			public Color SpecularColor;
			public float SpecularExponent;

			public Texture AmbientTexture = null;
			public Texture DispersiveTexture = null;
			public Texture SpecularTexture = null;

			public Material(Color Ka, Color Kd, Color Ks, float Ns) {
				AmbientColor = Ka;
				DispersiveColor = Kd;
				SpecularColor = Ks;
				SpecularExponent = Ns;
			}

			public Color GetColor(float ld, float ls) {
				return AmbientColor + DispersiveColor * ld + SpecularColor * ls * SpecularExponent;
			}
			public Color GetColor(Vector2 t, float ld, float ls) {
				Color ambiente = AmbientTexture == null ? AmbientColor : AmbientTexture.GetColor(t);
				Color dispersive = DispersiveTexture == null ? DispersiveColor : DispersiveTexture.GetColor(t);
				Color specular = SpecularTexture == null ? SpecularColor : SpecularTexture.GetColor(t);

				//return ambiente * ld;
				return ambiente + dispersive * ld + specular * (float) Math.Pow(ls, SpecularExponent);
			}

		}
	}
}
