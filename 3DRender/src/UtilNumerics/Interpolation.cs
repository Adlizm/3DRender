using System;

namespace UtilNumerics {
	static class Interpolation {
		public static float Clamp(float value, float min = 0.0f, float max = 1.0f) {
			return Math.Max(min, Math.Min(value, max));
		}

		public static float GetGradient(float v1, float v2, float vIn) {
			float value = (vIn - v1) / (v2 - v1);
			return v1 == v2 ? 1 : value;
		}
		public static float GetGradient(Vector2 v1, Vector2 v2, Vector2 vIn) {
			if(!Vector2.InSameReta(v1, v2, vIn))
				throw new ArgumentException("Vectors has't in same reta");
			float value = (vIn - v1).Length / (v2 - v1).Length;
			return v1.Equals(v2) ? 1.0f : value;
		}
		public static float GetGradient(Vector3 v1, Vector3 v2, Vector3 vIn) {
			if(!Vector3.InSameReta(v1, v2, vIn))
				throw new ArgumentException("Vectors has't in same reta");
			float value = (vIn - v1).Length / (v2 - v1).Length;
			return v1.Equals(v2) ? 1.0f : value;
		}

		public static float Interpolate(float v1, float v2, float w) {
			return v1 + (v2 - v1) * Clamp(w);
		}
		public static Vector2 Interpolate(Vector2 v1, Vector2 v2, float w) {
			return v1 + (v2 - v1) * Clamp(w);
		}
		public static Vector3 Interpolate(Vector3 v1, Vector3 v2, float w) {
			return v1 + (v2 - v1) * Clamp(w);
		}

	}
}
