using System;
using System.Windows.Media;

using UtilNumerics;
using static SoftRender.Device3D;

namespace SoftRender {
	namespace Objects {
		public class Light {
			public struct DotLightData {
				public float Dispersive;
				public float Specular;
			}
			public Color LightColor;
			public Vector3 Position;

			public Light(Vector3 position) {
				Position = position;
				LightColor = Color.FromArgb(255, 255, 255, 255);
			}

			public DotLightData DotVertex(Vector3 CamPos, TransformedVertex V) {
				Vector3 n = V.Normal.Normalize();
				Vector3 l = (Position - V.Position).Normalize();
				Vector3 e = (V.Position - CamPos).Normalize();
				Vector3 r = (n * (l * n) * 2) - l;

				float dispersive = Math.Max(0, l * n);
				float specular = Math.Max(0, r * e);
				return new DotLightData { Dispersive = dispersive, Specular = specular};
			}
		}
	}
}

