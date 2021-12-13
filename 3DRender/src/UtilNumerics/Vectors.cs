using System;

namespace UtilNumerics {
	public struct Vector2 {
		public float X, Y;

		public Vector2(float x, float y) {
			X = x;
			Y = y;
		}

		public float Length {
			get { return (float) Math.Sqrt(X * X + Y * Y); }
		}

		public Vector2 Normalize() {
			return this * (1 / Length);
		}
		public static bool InSameReta(Vector2 v, Vector2 u, Vector2 w) {
			Vector2 util1 = (u - v);
			Vector2 util2 = (w - v);
			return Math.Abs(util1 * util2) == util1.Length * util2.Length;
		}
		public static Vector2 operator +(Vector2 u, Vector2 v){
			return new Vector2(u.X + v.X, u.Y + v.Y);
		}
		public static Vector2 operator -(Vector2 u, Vector2 v){
			return new Vector2(u.X - v.X, u.Y - v.Y);
		}
		public static Vector2 operator *(Vector2 u, float scalar) {
			return new Vector2(u.X * scalar, u.Y * scalar);
		}
		public static Vector2 operator /(Vector2 u, float scalar) {
			return new Vector2(u.X / scalar, u.Y / scalar);
		}
		public static float operator *(Vector2 u, Vector2 v){
			return u.X * v.X + u.Y * v.Y;
		}
		public static Vector2 operator *(Vector2 u, Matrix A) {
			if(A.Rows != 2 || A.Cols != 2)
				throw new ArgumentException("Matrix hasn't shape 2x2");
			float x = u.X * A[0, 0] + u.Y * A[1, 0];
			float y = u.X * A[0, 1] + u.Y * A[1, 1];
			return new Vector2(x, y);
		}

	}

	public struct Vector3 {
		public float X, Y, Z;

		public Vector3(float x, float y, float z) {
			X = x;
			Y = y;
			Z = z;
		}

		public float Length {
			get { return (float) Math.Sqrt(X * X + Y * Y + Z * Z); }
		}
		public Vector2 XY {
			get { return new Vector2(X, Y); }
		}

		public Vector3 Normalize() {
			return this * (1 / this.Length);
		}
		public Vector3 Cross(Vector3 other) {
			float x = Y * other.Z - Z * other.Z;
			float y = Z * other.X - X * other.Z;
			float z = X * other.Y - Y * other.X;
			return new Vector3(x, y, z);
		}
		public static bool InSameReta(Vector3 v, Vector3 u, Vector3 w) {
			Vector3 util1 = (u - v);
			Vector3 util2 = (w - v);
			return Math.Abs(util1 * util2) == util1.Length * util2.Length;
		}
		public static Vector3 operator +(Vector3 u, Vector3 v){
			return new Vector3(u.X + v.X, u.Y + v.Y, u.Z + v.Z);
		}
		public static Vector3 operator -(Vector3 u, Vector3 v){
			return new Vector3(u.X - v.X, u.Y - v.Y, u.Z - v.Z);
		}
		public static Vector3 operator *(Vector3 u, float scalar) {
			return new Vector3(u.X * scalar, u.Y * scalar, u.Z * scalar);
		}
		public static Vector3 operator /(Vector3 u, float scalar) {
			return new Vector3(u.X / scalar, u.Y / scalar, u.Z / scalar);
		}
		public static float operator *(Vector3 u, Vector3 v){
			return u.X * v.X + u.Y * v.Y + u.Z * v.Z;
		}
		public static Vector3 operator *(Vector3 u, Matrix A) {
			if(A.Rows != 3 || A.Cols != 3)
				throw new ArgumentException("Matrix hasn't shape 3x3");
			float x = u.X * A[0, 0] + u.Y * A[1, 0] + u.Z * A[2, 0];
			float y = u.X * A[0, 1] + u.Y * A[1, 1] + u.Z * A[2, 1];
			float z = u.X * A[0, 2] + u.Y * A[1, 2] + u.Z * A[2, 2];
			return new Vector3(x, y, z);
		}
		
		public Vector3 TransformCoordinates(Matrix A) {
			if(A.Rows != 4 || A.Cols != 4)
				throw new ArgumentException("Matrix hasn't shape 4x4");
			float x = X * A[0, 0] + Y * A[1, 0] + Z * A[2, 0] + A[3, 0];
			float y = X * A[0, 1] + Y * A[1, 1] + Z * A[2, 1] + A[3, 1];
			float z = X * A[0, 2] + Y * A[1, 2] + Z * A[2, 2] + A[3, 2];
			float w = X * A[0, 3] + Y * A[1, 3] + Z * A[2, 3] + A[3, 3];
			
			w = 1.0f / w;
			return new Vector3(x * w, y * w, z * w);
		}

	}

	public struct Vector4 {
		public float X, Y, Z, W;

		public Vector4(float x, float y, float z, float w) {
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

		public float Length {
			get {
				return (float) Math.Sqrt(X * X + Y * Y + Z * Z + W * W);
			}
		}
		public Vector3 XYZ {
			get {
				return new Vector3(X, Y, Z);
			}
		}

		public Vector4 Normalize() {
			return  this * (1 / this.Length);
		}
		public static bool InSameReta(Vector4 v, Vector4 u, Vector4 w) {
			Vector4 util1 = (u - v);
			Vector4 util2 = (w - v);
			return Math.Abs(util1 * util2) == util1.Length * util2.Length;
		}
		public static Vector4 operator +(Vector4 u, Vector4 v) {
			return new Vector4(u.X + v.X, u.Y + v.Y, u.Z + v.Z, u.W + v.W);
		}
		public static Vector4 operator -(Vector4 u, Vector4 v) {
			return new Vector4(u.X - v.X, u.Y - v.Y, u.Z - v.Z, u.W - v.W);
		}
		public static Vector4 operator *(Vector4 u, float scalar) {
			return new Vector4(u.X * scalar, u.Y * scalar, u.Z * scalar, u.W * scalar);
		}
		public static Vector4 operator /(Vector4 u, float scalar) {
			return new Vector4(u.X / scalar, u.Y / scalar, u.Z / scalar, u.W / scalar);
		}
		public static float operator *(Vector4 u, Vector4 v) {
			return u.X * v.X + u.Y * v.Y + u.Z * v.Z + u.W * v.W;
		}
		public static Vector4 operator *(Vector4 u, Matrix A) {
			if(A.Rows != 4 || A.Cols != 4)
				throw new ArgumentException("Matrix hasn't shape 4x4");
			float x = u.X * A[0, 0] + u.Y * A[1, 0] + u.Z * A[2, 0] + u.W * A[3, 0];
			float y = u.X * A[0, 1] + u.Y * A[1, 1] + u.Z * A[2, 1] + u.W * A[3, 1];
			float z = u.X * A[0, 2] + u.Y * A[1, 2] + u.Z * A[2, 2] + u.W * A[3, 2];
			float w = u.X * A[0, 3] + u.Y * A[1, 3] + u.Z * A[2, 3] + u.W * A[3, 3];
			return new Vector4(x, y, z, w);
		}
	}
}
