using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using UtilNumerics;
using Matrix = UtilNumerics.Matrix;

using SoftRender.Objects;
using static SoftRender.Objects.Mesh;

namespace SoftRender {
	public class Device3D{
		public struct TransformedVertex {
			public Vector3 Coordinates;
			public Vector3 Position;
			public Vector3 Normal;
			public Vector2 TexturePoint;
		}
		public struct ScanLineData {
			public int Y;
			public Light.DotLightData dotLA;
			public Light.DotLightData dotLB;
			public Light.DotLightData dotLC;
			public Light.DotLightData dotLD;
		}

		private byte[] colorBuffer;
		private float[] zBuffer;

		public readonly int Width, Height;
		public readonly WriteableBitmap Bmp;

		public Device3D(int width, int height) {
			Width = width;
			Height = height;
			
			colorBuffer = new byte[Width * Height * 4];
			zBuffer = new float[Width * Height];

			Bmp = new WriteableBitmap(Width, Height, 96, 96, PixelFormats.Bgr32, null);
			Clear(Colors.Black);
		}

		public void Refresh() {
			Int32Rect rect = new Int32Rect(0, 0, Width, Height);
			Bmp.WritePixels(rect, colorBuffer, Width * 4, 0);
		}
		public void Clear(Color color) {
			int Length = Width * Height;
			int indexC = 0;
			for(int i = 0; i < Length; i++) {
				colorBuffer[indexC++] = color.B;
				colorBuffer[indexC++] = color.G;
				colorBuffer[indexC++] = color.R;
				colorBuffer[indexC++] = color.A;
				zBuffer[i] = float.MaxValue;
			}
		}

		private void PutPixel(int x, int y, float z, Color color) {
			int index = x + y * Width;
			int indexC = index * 4;

			if(zBuffer[index] < z)
				return;
			colorBuffer[indexC++] = color.B;
			colorBuffer[indexC++] = color.G;
			colorBuffer[indexC++] = color.R;
			colorBuffer[indexC++] = color.A;
			zBuffer[index] = z;
		}

		private Vector3 GetCoordinates(Matrix T, Vector3 v) {
			Vector3 c = v.TransformCoordinates(T);
			c.X = c.X * Width + Width * 0.5f;
			c.Y = c.Y * Height + Height * 0.5f;
			return c;
		}
		private TransformedVertex GetTransformedVertex(Matrix T, Matrix W, Mesh mesh, int indexV, int indexN, int indexT) {
			return new TransformedVertex {
				Coordinates = GetCoordinates(T, mesh.Vertexs[indexV]),
				Position = mesh.Vertexs[indexV].TransformCoordinates(W),
				Normal = mesh.Normals[indexN].TransformCoordinates(W),
				TexturePoint = mesh.TexturePoints[indexT]
			};
		}

		public void DrawPoint(Matrix T, Vector3 point, Color color) {
			Vector3 p0 = GetCoordinates(T, point);
			int x = (int) p0.X;
			int y = (int) p0.Y;

			if(x >= 0 && x < Width && y >= 0 && y < Height)
				PutPixel(x, y, p0.Z, color);
		}
		public void DrawLine(Matrix T, Vector3 pointA, Vector3 pointB, Color color) {
			Vector3 p0 = GetCoordinates(T, pointA);
			Vector3 p1 = GetCoordinates(T, pointB);

			int x0 = (int) p0.X;
			int y0 = (int) p0.Y;
			int x1 = (int) p1.X;
			int y1 = (int) p1.Y;

			int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
			int dy = Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
			int err = (dx > dy ? dx : -dy) / 2, e2;

			float dz = (float) (1.0 / Math.Sqrt(dx * dx + dy * dy));
			float z = 0;
			while(true) {
				if(x0 >= 0 && x0 < Width && y0 >= 0 && y0 < Height)
					PutPixel(x0, y0, Interpolation.Interpolate(p0.Z, p1.Z, z += dz), color);

				if(x0 == x1 && y0 == y1)
					break;
				e2 = err;
				if(e2 > -dx) {
					err -= dy;
					x0 += sx;
				}
				if(e2 < dy) {
					err += dx;
					y0 += sy;
				}
			}
		}
		private void ProcessScanLine(ScanLineData scan, Vector3 va, Vector3 vb, Vector3 vc, Vector3 vd, Vector2 ta, Vector2 tb, Vector2 tc, Vector2 td, Material material) {
			if(scan.Y < 0 || scan.Y >= Height)
				return;

			float gradient1 = Interpolation.GetGradient(va.Y, vb.Y, scan.Y);
			float gradient2 = Interpolation.GetGradient(vc.Y, vd.Y, scan.Y);
			
			// Interpolating z-axis (used in zBuffer)
			float z1 = Interpolation.Interpolate(va.Z, vb.Z, gradient1);
			float z2 = Interpolation.Interpolate(vc.Z, vd.Z, gradient2);

			// Interpolating light ndot (Intensity light in vertex)
			float ld1 = Interpolation.Interpolate(scan.dotLA.Dispersive, scan.dotLB.Dispersive, gradient1);
			float ld2 = Interpolation.Interpolate(scan.dotLC.Dispersive, scan.dotLD.Dispersive, gradient2);
			// Interpolating light ndot (Intensity light in vertex)
			float ls1 = Interpolation.Interpolate(scan.dotLA.Specular, scan.dotLB.Specular, gradient1);
			float ls2 = Interpolation.Interpolate(scan.dotLC.Specular, scan.dotLD.Specular, gradient2);

			// Interpolating texture point
			Vector2 t1 = Interpolation.Interpolate(ta, tb, gradient1);
			Vector2 t2 = Interpolation.Interpolate(tc, td, gradient2);

			// Interpolating X view coordinates
			int sx = (int) Interpolation.Interpolate(va.X, vb.X, gradient1);
			int ex = (int) Interpolation.Interpolate(vc.X, vd.X, gradient2);
			
			// Clipping x-axis
			int leftx = sx >= 0 ? sx : 0;
			int rightx = ex <= Width ? ex : Width;

			for(int x = leftx; x < rightx; x++) {
				float gradient = Interpolation.GetGradient(sx, ex, x);

				float z = Interpolation.Interpolate(z1, z2, gradient);
				float ld = Interpolation.Interpolate(ld1, ld2, gradient);
				float ls = Interpolation.Interpolate(ls1, ls2, gradient);
				Vector2 t = Interpolation.Interpolate(t1, t2, gradient);

				Color color = material.GetColor(t, ld, ls);
				
				PutPixel(x, scan.Y, z, color);
			}
		}
		public void DrawFace(TransformedVertex a, TransformedVertex b, TransformedVertex c, Material material, Light light, Vector3 eye) {
			// Swapping vertex to correct order
			if(a.Coordinates.Y > b.Coordinates.Y) {
				var temp = a;
				a = b; 
				b = temp;
			}
			if(b.Coordinates.Y > c.Coordinates.Y) {
				var temp = b;
				b = c; 
				c = temp;
			}
			if(a.Coordinates.Y > b.Coordinates.Y) {
				var temp = a;
				a = b; 
				b = temp;
			}

			Vector2 AB = (b.Coordinates - a.Coordinates).XY;
			Vector2 AC = (c.Coordinates - a.Coordinates).XY;
			float cross = AB.X * AC.Y - AB.Y * AC.X;

			// Calculate light nDot from vertex
			Light.DotLightData dLA = light.DotVertex(eye, a);
			Light.DotLightData dLB = light.DotVertex(eye, b);
			Light.DotLightData dLC = light.DotVertex(eye, c);
			
			ScanLineData scan = new ScanLineData {};
			// Case 1: B vertex at left and C at right
			if(cross >= 0) {
				scan.dotLA = dLA;
				scan.dotLB = dLC;
				scan.dotLC = dLA;
				scan.dotLD = dLB;
				for(int y = (int) a.Coordinates.Y; y < b.Coordinates.Y; y++) {
					scan.Y = y;
					ProcessScanLine(scan, a.Coordinates, c.Coordinates, a.Coordinates, b.Coordinates, 
						a.TexturePoint, c.TexturePoint, a.TexturePoint, b.TexturePoint, material);
				}
				scan.dotLC = dLB;
				scan.dotLD = dLC;
				for(int y = (int) b.Coordinates.Y; y <= (int) c.Coordinates.Y; y++) {
					scan.Y = y;
					ProcessScanLine(scan, a.Coordinates, c.Coordinates, b.Coordinates, c.Coordinates,
						a.TexturePoint, c.TexturePoint, b.TexturePoint, c.TexturePoint, material);
				}
			}else { // Case 2: C vertex at left and B at right
				scan.dotLA = dLA;
				scan.dotLB = dLB;
				scan.dotLC = dLA;
				scan.dotLD = dLC;
				for(int y = (int) a.Coordinates.Y; y < b.Coordinates.Y; y++) {
					scan.Y = y;
					ProcessScanLine(scan, a.Coordinates, b.Coordinates, a.Coordinates, c.Coordinates,
						a.TexturePoint, b.TexturePoint, a.TexturePoint, c.TexturePoint, material);
				}
				scan.dotLA = dLB;
				scan.dotLB = dLC;
				for(int y = (int) b.Coordinates.Y; y <= (int) c.Coordinates.Y; y++) {
					scan.Y = y;
					ProcessScanLine(scan, b.Coordinates, c.Coordinates, a.Coordinates, c.Coordinates,
						b.TexturePoint, c.TexturePoint, a.TexturePoint, c.TexturePoint, material);
				}
			}

		}
		
		public void Render(Camera camera, Light light, Mesh[] meshes) {
			Matrix view = camera.GetLookAtMatrix();
			Matrix projection = camera.GetPerspectiveMatrix();

			foreach(Mesh mesh in meshes) {
				Matrix world = Matrix.RotateMatrix(mesh.Rotate) * Matrix.TranslateMatrix(mesh.Position);
				Matrix transform = world * view * projection;
			
				foreach(Face face in mesh.Faces) {
					TransformedVertex a = GetTransformedVertex(transform, world, mesh, face.VA, face.NA, face.TA);
					TransformedVertex b = GetTransformedVertex(transform, world, mesh, face.VB, face.NB, face.TB);
					TransformedVertex c = GetTransformedVertex(transform, world, mesh, face.VC, face.NC, face.TC);

					DrawFace(a, b, c, mesh.Material, light, camera.Position);
				}	
			}
			
		}
	}

}
