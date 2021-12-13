using System;

namespace UtilNumerics {
	public struct Matrix {
		private float[] data;
		public readonly int Rows, Cols;
		public Matrix(int rows, int cols) {
			if(rows <= 0 || cols <= 0)
				throw new ArgumentException("Cannot create a matrix without rows or cols");
			Rows = rows;
			Cols = cols;
			data = new float[rows * cols];
		}

		public static Matrix Identity(int n) {
			Matrix I = new Matrix(n, n);
			for(int i = 0; i < n; i++)
				I[i, i] = 1;
			return I;
		}
		public static Matrix RotateMatrix(Vector3 rotate) {
			Matrix R = new Matrix(4, 4);
			float sinA = (float) Math.Sin(rotate.X), cosA = (float) Math.Cos(rotate.X);
			float sinB = (float) Math.Sin(rotate.Y), cosB = (float) Math.Cos(rotate.Y);
			float sinY = (float) Math.Sin(rotate.Z), cosY = (float) Math.Cos(rotate.Z);

			R[0, 0] = cosA * cosY - sinA * cosB * sinY;
			R[0, 1] = sinA * cosY + cosA * cosB * sinY;
			R[0, 2] = sinB * sinY;

			R[1, 0] = -cosA * sinY - sinA * cosB * cosY;
			R[1, 1] = -sinA * sinY + cosA * cosB * cosY;
			R[1, 2] = sinB * cosY;

			R[2, 0] = sinA * sinB;
			R[2, 1] = -cosA * sinB;
			R[2, 2] = cosB;

			R[3, 3] = 1;

			return R;
		}
		public static Matrix TranslateMatrix(Vector3 translate) {
			Matrix T = Identity(4);
			T[0, 3] = translate.X;
			T[1, 3] = translate.Y;
			T[2, 3] = translate.Z;
			return T;
		}

		public Matrix Copy() {
			Matrix copy = new Matrix(Rows, Cols);
			for(int i = 0; i < data.Length; i++)
				copy.data[i] = data[i];
			return copy;
		}
		public float this[int i, int j]{
			get => data[i * Cols + j];
			set => data[i * Cols + j] = value;
		}
		public static Matrix operator +(Matrix a, Matrix b) {
			if(a.Rows != b.Rows || a.Cols != b.Cols)
				throw new ArgumentException("Matrixs has different values from Rows or columns!");
			
			Matrix c = new Matrix(a.Rows, b.Cols);
			int length = a.Rows * b.Cols;
			for(int i = 0; i < length; i++)
				c.data[i] = a.data[i] + b.data[i];
			return c;
		}
		public static Matrix operator -(Matrix a, Matrix b) {
			if(a.Rows != b.Rows || a.Cols != b.Cols)
				throw new ArgumentException("Matrixs has different values from Rows or columns!");

			Matrix c = new Matrix(a.Rows, b.Cols);
			int length = a.Rows * b.Cols;
			for(int i = 0; i < length; i++)
				c.data[i] = a.data[i] - b.data[i];
			return c;
		}
		
		public static Matrix operator *(Matrix a, Matrix b) {
			if(a.Cols != b.Rows)
				throw new ArgumentException("Matrixs has different values from a.Rows and b.columns!");

			Matrix c = new Matrix(a.Rows, b.Cols);

			int indexA, indexB, indexC = 0;
			for(int i = 0; i < c.Rows; i++) {
				for(int j = 0; j < c.Cols; j++) {
					indexA = i * a.Cols;
					indexB = j;
					for(int k = 0; k < a.Cols; k++) {
						c.data[indexC] += a.data[indexA] * b.data[indexB];
						indexA++;
						indexB += b.Cols;
					}
					indexC++;
				}
			}
			return c;
		}
		public static Matrix operator *(float s, Matrix a) {
			Matrix c = new Matrix(a.Rows, a.Cols);
			int length = a.Rows * a.Cols;
			for(int i = 0; i < length; i++)
				c.data[i] = a.data[i] * s;
			return c;
		}
		public static Matrix operator *(Matrix a, float s) => s * a;

	}
}
