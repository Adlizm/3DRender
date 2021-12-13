using System;
using UtilNumerics;

namespace SoftRender {
	namespace Objects {
		public class Camera {
			public Vector3 Position;
			public Vector3 LookAt;
			public Vector3 UpVision;
			public float Near, Far, Fovy, Aspect;

			public Camera(Vector3 pos, Vector3 look) {
				Position = pos;
				LookAt = look;
				UpVision = new Vector3(0, 1, 0);

				Near = 0.5f;
				Far = 10.0f;
				Fovy = (float) (Math.PI / 2);
				Aspect = 4.0f / 3.0f;
			}

			public Matrix GetLookAtMatrix() {
				Matrix L = new Matrix(4, 4);

				Vector3 forward = (Position - LookAt).Normalize();
				Vector3 right = UpVision.Normalize().Cross(forward);
				Vector3 up = forward.Cross(right);

				float xT = -(right * Position);
				float yT = -(up * Position);
				float zT = -(forward * Position);

				L[0, 0] = right.X;
				L[1, 0] = right.Y;
				L[2, 0] = right.Z;
				L[3, 0] = xT;

				L[0, 1] = up.X;
				L[1, 1] = up.Y;
				L[2, 1] = up.Z;
				L[3, 1] = yT;

				L[0, 2] = forward.X;
				L[1, 2] = forward.Y;
				L[2, 2] = forward.Z;
				L[3, 2] = zT;

				L[3, 3] = 1;

				return L;
			}
			public Matrix GetOrthographicMatrix() {
				Matrix P = new Matrix(4, 4);

				float c1 = 2 / (Far - Near);
				float c2 = (Far + Near) / (Far - Near);

				float top = Near * (float) Math.Tan(Fovy / 2);
				float right = top * Aspect;

				P[0, 0] = 1 / right;
				P[1, 1] = 1 / top;
				P[2, 2] = -c1;
				P[2, 3] = -c2;
				P[3, 3] = 1;

				return P;
			}
			public Matrix GetPerspectiveMatrix() {
				Matrix P = new Matrix(4, 4);

				float yScale = (float) (1.0 / Math.Tan(Fovy / 2));
				float q = Far / (Far - Near);

				P[0, 0] = yScale / Aspect;
				P[1, 1] = yScale;
				P[2, 2] = q;
				P[2, 3] = 1.0f;
				P[3, 2] = -q * Near;

				return P;
			}
		}
	}
}
