using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using UtilNumerics;
using Color = System.Windows.Media.Color;
using Matrix = UtilNumerics.Matrix;


namespace SoftRender {
	public class Device2D {
		private Matrix[] T = new Matrix[16];
		private int matrixIndex = 0;
		public float ZLayer = 0.0f;

		public Color FillColor;
		public Color StrokeColor;
		public bool Fill = true;
		public bool Stroke = true;
		public int LineWidth = 1;

		private bool beginPath = false;
		private List<Vector2> path;

		private byte[] colorBuffer;
		private float[] zBuffer;
		public readonly int Width, Height;
		public readonly WriteableBitmap Bmp;

		public struct Rect {
			public Vector2 Point;
			public int Width, Height;
		}
		public Device2D(int width, int height) {
			Width = width;
			Height = height;
			colorBuffer = new byte[Width * Height * 4];
			zBuffer = new float[Width * Height];

			Bmp = new WriteableBitmap(Width, Height, 96, 96, PixelFormats.Bgr32, null);
			Clear(Colors.Black);
			LoadIdentity();
		}

		private void PutPixelStroke(int x, int y){
			int dwl = -(LineWidth - 1) / 2;
			int dwr = LineWidth + dwl;
			for(float wx = dwl; wx < dwr; wx++) {
				for(float wy = dwl; wy < dwr; wy++) {
					int X = (int) (x + wx);
					int Y = (int) (y + wy);
					if(X >= 0 && X < Width && Y >= 0 && Y < Height)
						PutPixel(X, Y, StrokeColor);
				}
			}
		}
		private void PutPixel(int x, int y, Color color) {
			int index = x + y * Width;
			int indexC = index * 4;

			if(zBuffer[index] < ZLayer)
				return;
			colorBuffer[indexC++] = color.B;
			colorBuffer[indexC++] = color.G;
			colorBuffer[indexC++] = color.R;
			colorBuffer[indexC++] = color.A;
			zBuffer[index] = ZLayer;
		}
		private Vector2 Transform(Vector2 v) {
			Vector2 c = (new Vector3(v.X, v.Y, 1) * T[matrixIndex]).XY;
			return c;
		}
		private void ScanLineY(int Y, int xl, int xr) {
			if(Y < 0 || Y >= Height)
				return;
			int XL = xl, XR = xr;
			xl = xl < 0 ? 0 : xl;
			xr = xr < Width ? xr : Width - 1;
			if(Fill)
				for(int X = xl; X <= xr; X++)
					PutPixel(X, Y, FillColor);
			if(Stroke) {
				PutPixelStroke(XL, Y);
				PutPixelStroke(XR, Y);
			}
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
		public void LoadIdentity() {
			T[matrixIndex] = Matrix.Identity(3);
		}
		public void Rotate(float angle) {
			float sin = (float) Math.Sin(angle);
			float cos = (float) Math.Cos(angle);
			Matrix rotate = Matrix.Identity(3);
			rotate[0, 0] = cos;
			rotate[1, 0] =-sin;
			rotate[0, 1] = sin;
			rotate[1, 1] = cos;

			T[matrixIndex] = rotate * T[matrixIndex];
		}
		public void Translate(Vector2 t) {
			Matrix translate = Matrix.Identity(3);
			translate[2, 0] = t.X;
			translate[2, 1] = t.Y;

			T[matrixIndex] = translate * T[matrixIndex];
		}
		public void Scale(Vector2 s) {
			Matrix scale = Matrix.Identity(3);
			scale[0, 0] = s.X;
			scale[1, 1] = s.Y;

			T[matrixIndex] = scale * T[matrixIndex];
		}
		public void Push() {
			if(matrixIndex >= T.Length - 1)
				return;
			matrixIndex++;
			T[matrixIndex] = T[matrixIndex - 1].Copy();
		}
		public void Pop() {
			if(matrixIndex > 0)
				matrixIndex--;
		}
		public void BeginPath() {
			beginPath = true;
			path = new List<Vector2>();
		}
		public void EndPath() {
			beginPath = false;
			DrawPolygon(path.ToArray());
			path.Clear();
		}

		public void DrawPoint(Vector2 a) {
			if(beginPath)
				path.Add(a);

			Vector2 p0 = Transform(a);
			int x = (int) p0.X;
			int y = (int) p0.Y;
			if(Stroke)
				PutPixelStroke(x, y);
		}
		public void DrawLine(Vector2 a, Vector2 b) {
			if(beginPath) {
				path.Add(a);
				path.Add(b);
			}
			if(!Stroke)
				return;
			a = Transform(a);
			b = Transform(b);

			int x0 = (int) a.X;
			int y0 = (int) a.Y;
			int x1 = (int) b.X;
			int y1 = (int) b.Y;

			int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
			int dy = Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
			int err = (dx > dy ? dx : -dy) / 2, e2;

			while(true) {
				PutPixelStroke(x0, y0);
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
		public void DrawTriangle(Vector2 a, Vector2 b, Vector2 c) {
			a = Transform(a);
			b = Transform(b);
			c = Transform(c);
			if(a.Y > b.Y) {
				var temp = a;
				a = b;
				b = temp;
			}
			if(b.Y > c.Y) {
				var temp = b;
				b = c;
				c = temp;
			}
			if(a.Y > b.Y) {
				var temp = a;
				a = b;
				b = temp;
			}
			Vector2 AB = b - a;
			Vector2 AC = c - a;
			float cross = AB.X * AC.Y - AB.Y * AC.X;

			int ixr = 0, ixl = 0, xr = 0, xl = 0;
			if(cross <= 0) {
				bool first = true;
				for(int y = (int) a.Y; y <= b.Y; y++) {
					xl = (int) Interpolation.Interpolate(a.X, b.X, Interpolation.GetGradient(a.Y, b.Y, y));
					xr = (int) Interpolation.Interpolate(a.X, c.X, Interpolation.GetGradient(a.Y, c.Y, y));
					ScanLineY(y, xl, xr);
					if(first) { 
						first = false;
						ixr = xr; 
						ixl = xl;
					}
				}
				for(int y = (int) b.Y; y <= (int) c.Y; y++) {
					xl = (int) Interpolation.Interpolate(b.X, c.X, Interpolation.GetGradient(b.Y, c.Y, y));
					xr = (int) Interpolation.Interpolate(a.X, c.X, Interpolation.GetGradient(a.Y, c.Y, y));
					ScanLineY(y, xl, xr);
				}
				if(Stroke) {
					for(int x = ixl; x <= ixr; x++)
						PutPixelStroke(x, (int) a.Y);
					for(int x = xl; x <= xr; x++)
						PutPixelStroke(x, (int) c.Y);
				}
			} else {
				bool first = true;
				for(int y = (int) a.Y; y <= b.Y; y++) {
					xl = (int) Interpolation.Interpolate(a.X, c.X, Interpolation.GetGradient(a.Y, c.Y, y));
					xr = (int) Interpolation.Interpolate(a.X, b.X, Interpolation.GetGradient(a.Y, b.Y, y));
					ScanLineY(y, xr, xl);
				}
				for(int y = (int) b.Y; y <= (int) c.Y; y++) {
					xl = (int) Interpolation.Interpolate(a.X, c.X, Interpolation.GetGradient(a.Y, c.Y, y));
					xr = (int) Interpolation.Interpolate(b.X, c.X, Interpolation.GetGradient(b.Y, c.Y, y));
					ScanLineY(y, xl, xr);
					if(first) {
						first = false;
						ixr = xr;
						ixl = xl;
					}
				}
				if(Stroke) {
					for(int x = ixl; x <= ixr; x++)
						PutPixelStroke(x, (int) a.Y);
					for(int x = xl; x <= xr; x++)
						PutPixelStroke(x, (int) c.Y);
				}
			}
			
		}
		public void DrawRect(Vector2 p, int w, int h) {
			Vector2 p0 = new Vector2(p.X    , p.Y    );
			Vector2 p1 = new Vector2(p.X + w, p.Y    );
			Vector2 p2 = new Vector2(p.X    , p.Y + h);
			Vector2 p3 = new Vector2(p.X + w, p.Y + h);

			DrawPolygon(new Vector2[] { p0, p1, p3, p2 });
		}
		public void DrawPolygon(Vector2[] v) {
			Vector2[] copyv = (Vector2[]) v.Clone();

			float minX = float.MaxValue, maxX = -float.MaxValue;
			float minY = float.MaxValue, maxY = -float.MaxValue;
			for(int i = 0; i < v.Length; i++) {
				v[i] = Transform(v[i]);
				maxX = v[i].X > maxX ? v[i].X : maxX;
				minX = v[i].X < minX ? v[i].X : minX;
				maxY = v[i].X > maxY ? v[i].X : maxY;
				minY = v[i].X < minY ? v[i].X : minY;
			}
			minY = Math.Max(0, minY);
			maxY = Math.Max(Height - 1, minY);
			for(int y = (int) minY; y < (int) maxY; y++) {
				List<float> xs = new List<float>();
				for(int i = 0; i < v.Length; i++) {
					Vector2 p0 = v[i];
					Vector2 p1 = v[(i + 1) % v.Length];
					if(p1.Y != p0.Y) {
						float gradient = Interpolation.GetGradient(p0.Y, p1.Y, y);
						if(gradient >= 0 && gradient <= 1)
							xs.Add(Interpolation.Interpolate(p0.X, p1.X, gradient));
					} else if(y == (int) p0.Y){
						int xl = Math.Max(0, Math.Min((int) p0.X, (int) p1.X));
						int xr = Math.Min(Width - 1, Math.Max((int) p0.X, (int) p1.X));
						if(Stroke)
							for(int x = xl; x <= xr; x++)
								PutPixelStroke(x, y);
						else
							ScanLineY(y, xl, xr);
					}
				}
				if(xs.Count >= 2) {
					xs.Sort();
					bool inWindows = false;
					bool draw = true;
					float currentX = xs[0];
					for(int i = 1; i < xs.Count && currentX < Width; i++, draw = !draw) {
						if(xs[i] >= 0)
							inWindows = true;
						if(inWindows && draw)
							ScanLineY(y, (int) currentX, (int) xs[i]);
					}
				}
			}
			
		}
		public void DrawCircle(Vector2 c, int r) { DrawEllipse(c, r, r); }
		public void DrawEllipse(Vector2 c, int rx, int ry) {
			c = Transform(c);

			int cx = (int) c.X, cy = (int) c.Y;
			int a = 2 * rx, b = 2 * ry, b1 = b & 1;
			int dx = 4 * (1 - a) * b * b;
			int dy = 4 * (b1 + 1) * a * a;
			int err = dx + dy + b1 * a * a;
			int e2;

			int x0 = cx - rx;
			int x1 = cx + rx;
			int y0 = cy - ry;
			int y1 = cy + ry;
			y0 += (b + 1) / 2;
			y1 = y0 - b1;
			a *= 8 * a;
			b1 = 8 * b * b;

			do {
				ScanLineY(y0, x0, x1);
				ScanLineY(y1, x0, x1);
					
				e2 = 2 * err;
				if(e2 <= dy) {
					y0++;
					y1--;
					err += dy += a;
				}
				if(e2 >= dx || 2 * err > dy) {
					x0++;
					x1--;
					err += dx += b1;
				}
			} while(x0 <= x1);

			while(y0 - y1 < b) {
				ScanLineY(y0, x0 - 1, x1 + 1);
				ScanLineY(y1, x0 - 1, x1 + 1);
				y0++;
				y1--;
			}
		}
		public void DrawBezierCurve(Vector2 a, Vector2 b, Vector2 c, Vector2 d) {
			Vector2 p0, p1 = a;
			for(float t = 1.0f; t >= 0; t -= 0.01f) {
				if(beginPath)
					path.Add(p1);

				float c0 = t * t * t;
				float c1 = 3 * t * t * (1 - t);
				float c2 = 3 * t * (1 - t) * (1 - t);
				float c3 = (1 - t) * (1 - t) * (1 - t);

				p0 = (a * c0) + (b * c1) + (c * c2) + (d * c3);
				DrawLine(p0, p1);
				p1 = p0;
			}
		}
		public void DrawArc(Vector2 c, float r, float beginAngle, float endAngle) {
			float dAngle = (float) Math.Min(Math.Abs(endAngle - beginAngle), 2 * Math.PI);

			int steps = (int) (r * dAngle);
			float increment = beginAngle > endAngle ? (dAngle / steps) : -(dAngle / steps);
			float angle = beginAngle;
			float x0 = c.X + (float) Math.Cos(angle) * r;
			float y0 = c.Y + (float) Math.Sin(angle) * r;
			for(int i = 0; i < steps; i++) {
				if(beginPath)
					path.Add(new Vector2(x0, y0));

				angle += increment;
				float x1 = c.X + (float) Math.Cos(angle) * r;
				float y1 = c.Y + (float) Math.Sin(angle) * r;
				DrawLine(new Vector2(x0, y0), new Vector2(x1, y1));
				x0 = x1;
				y0 = y1;
			}
		}
		public void DrawImage(Bitmap image, Rect rImage, Rect rDraw) {
			if((int) rImage.Point.X < 0 || (int) (rImage.Point.X + rImage.Width) > image.Width ||
				(int) rImage.Point.Y < 0 || (int) (rImage.Point.Y + rImage.Height) > image.Height) {
					throw new ArgumentException("Rectangle Image exceeds the limits of the image");
			}
			Vector2 Va = new Vector2(rDraw.Point.X					, rDraw.Point.Y);
			Vector2 Vb = new Vector2(rDraw.Point.X + rDraw.Width - 1, rDraw.Point.Y);
			Vector2 Vc = new Vector2(rDraw.Point.X					, rDraw.Point.Y + rDraw.Height - 1);
			Vector2 Vd = new Vector2(rDraw.Point.X + rDraw.Width - 1, rDraw.Point.Y + rDraw.Height - 1);
			
			Vector2 Ta = new Vector2(rImage.Point.X					  , rImage.Point.Y);
			Vector2 Tb = new Vector2(rImage.Point.X + rImage.Width - 1, rImage.Point.Y);
			Vector2 Tc = new Vector2(rImage.Point.X					  , rImage.Point.Y + rImage.Height - 1);
			Vector2 Td = new Vector2(rImage.Point.X + rImage.Width - 1, rImage.Point.Y + rImage.Height - 1);

			Action<int, int, int, Vector2, Vector2> scanLineTexute = (y, xl, xr, tl, tr) => {
				if(y < 0 || y >= Height)
					return;
				int xleft = xl, xright = xr;
				xl = xl < 0 ? 0 : xl;
				xr = xr < Width ? xr : Width - 1;
				for(int x = xl; x <= xr; x++) {
					Vector2 pixel = Interpolation.Interpolate(tl, tr, Interpolation.GetGradient(xleft, xright, x));
					System.Drawing.Color c = image.GetPixel((int) pixel.X, (int) pixel.Y);
					PutPixel(x, y, Color.FromArgb(c.A, c.R, c.G, c.B));
				}
			};

			Action<Vector2, Vector2, Vector2, Vector2, Vector2, Vector2> drawTriangleImage = (va, vb, vc, ta, tb, tc) => {
				va = Transform(va);
				vb = Transform(vb);
				vc = Transform(vc);
				if(va.Y > vb.Y) {
					var tempv = va; va = vb; vb = tempv;	
					var tempt = ta; ta = tb; tb = tempt;
				}
				if(vb.Y > vc.Y) {
					var tempv = vb; vb = vc; vc = tempv;
					var tempt = tb; tb = tc; tc = tempt;
				}
				if(va.Y > vb.Y) {
					var tempv = va; va = vb; vb = tempv;
					var tempt = ta; ta = tb; tb = tempt;
				}
				Vector2 AB = vb - va, AC = vc - va;
				float cross = AB.X * AC.Y - AB.Y * AC.X;

				Vector2 tl, tr;
				int xr, xl;
				if(cross <= 0) {
					for(int y = (int) va.Y; y < vb.Y; y++) {
						float gradient1 = Interpolation.GetGradient(va.Y, vb.Y, y);
						float gradient2 = Interpolation.GetGradient(va.Y, vc.Y, y);
						xl = (int) Interpolation.Interpolate(va.X, vb.X, gradient1);
						xr = (int) Interpolation.Interpolate(va.X, vc.X, gradient2);
						tl = Interpolation.Interpolate(ta, tb, gradient1);
						tr = Interpolation.Interpolate(ta, tc, gradient2);
						scanLineTexute(y, xl, xr, tl, tr);
					}
					for(int y = (int) vb.Y; y <= (int) vc.Y; y++) {
						float gradient1 = Interpolation.GetGradient(vb.Y, vc.Y, y);
						float gradient2 = Interpolation.GetGradient(va.Y, vc.Y, y);
						xl = (int) Interpolation.Interpolate(vb.X, vc.X, gradient1);
						xr = (int) Interpolation.Interpolate(va.X, vc.X, gradient2);
						tl = Interpolation.Interpolate(tb, tc, gradient1);
						tr = Interpolation.Interpolate(ta, tc, gradient2);
						scanLineTexute(y, xl, xr, tl, tr);
					}
				} else {
					for(int y = (int) va.Y; y < vb.Y; y++) {
						float gradient1 = Interpolation.GetGradient(va.Y, vc.Y, y);
						float gradient2 = Interpolation.GetGradient(va.Y, vb.Y, y);
						xl = (int) Interpolation.Interpolate(va.X, vc.X, gradient1);
						xr = (int) Interpolation.Interpolate(va.X, vb.X, gradient2);
						tl = Interpolation.Interpolate(ta, tc, gradient1);
						tr = Interpolation.Interpolate(ta, tb, gradient2);
						scanLineTexute(y, xl, xr, tl, tr);
					}
					for(int y = (int) vb.Y; y <= (int) vc.Y; y++) {
						float gradient1 = Interpolation.GetGradient(va.Y, vc.Y, y);
						float gradient2 = Interpolation.GetGradient(vb.Y, vc.Y, y);
						xl = (int) Interpolation.Interpolate(va.X, vc.X, gradient1);
						xr = (int) Interpolation.Interpolate(vb.X, vc.X, gradient2);
						tl = Interpolation.Interpolate(ta, tc, gradient1);
						tr = Interpolation.Interpolate(tb, tc, gradient2);
						scanLineTexute(y, xl, xr, tl, tr);
					}
				}
			};
			drawTriangleImage(Va, Vb, Vc, Ta, Tb, Tc);
			drawTriangleImage(Vb, Vc, Vd, Tb, Tc, Td);
		}

		
	}

}
