using System;
using System.Collections.Generic;
using UtilNumerics;

namespace SoftRender {
	namespace Objects {
		public class Mesh {
			private struct IndexVertexFace {
				public int V, T, N;
			}

			public struct Face {
				public int VA;
				public int VB;
				public int VC;

				public int TA;
				public int TB;
				public int TC;

				public int NA;
				public int NB;
				public int NC;
			}

			public string Name;
			public Vector3 Position, Rotate;

			public Vector3[] Vertexs;
			public Vector3[] Normals;
			public Vector2[] TexturePoints;

			public Face[] Faces;
			public Material Material;

			public Mesh(string name) {
				Name = name;
			}
			public Mesh(string name, int vertexs, int normals, int faces, int texturePoints = 1) {
				Name = name;
				Vertexs = new Vector3[vertexs];
				Normals = new Vector3[normals];
				TexturePoints = new Vector2[texturePoints];

				Faces = new Face[faces];
			}

			public static Mesh CreateSphare(int frequency) {
				if(frequency < 1)
					throw new ArgumentException("Frequency cannot be less than 1!");
				int T = frequency * frequency; 
			
				float phi = (float) ((1.0 + Math.Sqrt(5.0)) / 2.0);
				float len = (float) Math.Sqrt(phi * phi + 1);
				float t = phi / len;
				float one = 1 / len;

				Dictionary<string, int> midPoint = new Dictionary<string, int>();
			
				List<Vector3> vertices = new List<Vector3>();
				List<Face> faces = new List<Face>();

				vertices.Add(new Vector3(-one, 0, t));
				vertices.Add(new Vector3( one, 0, t));
				vertices.Add(new Vector3(-one, 0,-t));
				vertices.Add(new Vector3( one, 0,-t));
				vertices.Add(new Vector3( 0, t, one));
				vertices.Add(new Vector3( 0, t,-one));
				vertices.Add(new Vector3( 0,-t, one));
				vertices.Add(new Vector3( 0,-t,-one));
				vertices.Add(new Vector3( t, one, 0));
				vertices.Add(new Vector3(-t, one, 0));
				vertices.Add(new Vector3( t,-one, 0));
				vertices.Add(new Vector3(-t,-one, 0));

				faces.Add(new Face { VA = 0, VB = 4, VC = 1 });
				faces.Add(new Face { VA = 4, VB = 8, VC = 1 });
				faces.Add(new Face { VA = 5, VB = 2, VC = 3 });
				faces.Add(new Face { VA = 2, VB = 7, VC = 3 });
				faces.Add(new Face { VA = 0, VB = 9, VC = 4 });
				faces.Add(new Face { VA = 9, VB = 5, VC = 4 });
				faces.Add(new Face { VA = 9, VB = 2, VC = 5 });
				faces.Add(new Face { VA = 0, VB = 1, VC = 6 });
				faces.Add(new Face { VA = 4, VB = 5, VC = 8 });
				faces.Add(new Face { VA = 5, VB = 3, VC = 8 });
				faces.Add(new Face { VA = 8, VB = 1, VC = 10 });
				faces.Add(new Face { VA = 8, VB = 3, VC = 10 });
				faces.Add(new Face { VA = 7, VB = 3, VC = 10 });
				faces.Add(new Face { VA = 7, VB = 6, VC = 10 });
				faces.Add(new Face { VA = 6, VB = 1, VC = 10 });
				faces.Add(new Face { VA = 7, VB = 6, VC = 11 });
				faces.Add(new Face { VA = 0, VB = 6, VC = 11 });
				faces.Add(new Face { VA = 9, VB = 0, VC = 11 });
				faces.Add(new Face { VA = 9, VB = 2, VC = 11 });
				faces.Add(new Face { VA = 7, VB = 2, VC = 11 });

				for(int i = 1; i < frequency; i++) {
					List<Face> newFaces = new List<Face>();
					foreach(Face face in faces) {
						Vector3 a = vertices[face.VA];
						Vector3 b = vertices[face.VB];
						Vector3 c = vertices[face.VC];
						Vector3 ab = (a + b).Normalize();
						Vector3 ac = (a + c).Normalize();
						Vector3 bc = (b + c).Normalize();

						int indexAB, indexAC, indexBC;
						if(vertices.Contains(ab)) {
							indexAB = vertices.IndexOf(ab);
						} else {
							indexAB = vertices.Count;
							vertices.Add(ab);
						}

						if(vertices.Contains(ac)) {
							indexAC = vertices.IndexOf(ac);
						} else {
							indexAC = vertices.Count;
							vertices.Add(ac);
						}

						if(vertices.Contains(bc)) {
							indexBC = vertices.IndexOf(bc);
						} else {
							indexBC = vertices.Count;
							vertices.Add(bc);
						}

						newFaces.Add(new Face { VA = face.VA, VB = indexAB, VC = indexAC });
						newFaces.Add(new Face { VB = face.VB, VC = indexBC, VA = indexAB });
						newFaces.Add(new Face { VC = face.VC, VA = indexAC, VB = indexBC });
						newFaces.Add(new Face { VA = indexAB, VB = indexBC, VC = indexAC });
					}
					faces.Clear();
					faces = newFaces;
				}
				
				Mesh sphare = new Mesh("Sphare", vertices.Count, vertices.Count, faces.Count);

				for(int i = 0; i < faces.Count; i++) {
					sphare.Faces[i].VA = faces[i].VA;
					sphare.Faces[i].VB = faces[i].VB;
					sphare.Faces[i].VC = faces[i].VC;
					sphare.Faces[i].NA = faces[i].VA;
					sphare.Faces[i].NB = faces[i].VB;
					sphare.Faces[i].NC = faces[i].VC;
				}
				for(int i = 0; i < vertices.Count; i++) {
					sphare.Vertexs[i] = vertices[i];
					sphare.Normals[i] = vertices[i];
				}
				return sphare;
			}
			public static Mesh CreateCube(int frequency) {
				int nVerticesPlane = (frequency + 1) * (frequency + 1);
				int nVertices = nVerticesPlane * 6;
				int nFacesPlane = frequency * frequency * 2;
				int nFaces = nFacesPlane * 6;

				Vector3[] ups = new Vector3[]{
					new Vector3( 1,0,0), new Vector3(0, 1,0), new Vector3(0,0, 1),
					new Vector3(-1,0,0), new Vector3(0,-1,0), new Vector3(0,0,-1)
				};
				
				Mesh cube = new Mesh("Cube", nVertices, 6, nFaces);

				int indexV = 0, indexF = 0;
				for(int n = 0; n < 6; n++) {
					Vector3 up = ups[n];
					Vector3 right = new Vector3(up.Z, up.X, up.Y);
					Vector3 bottom = up.Cross(right);

					Vector3 initPosition = right * -0.5f + bottom * -0.5f + up * 0.5f;
					Vector3 dRight = right / frequency;
					Vector3 dBottom = bottom / frequency;

					cube.Normals[n] = up;
					for(int i = 0; i <= frequency; i++) {
						for(int j = 0; j <= frequency; j++) {
							cube.Vertexs[indexV] = initPosition + dRight * i + dBottom * j;

							if(j != frequency && i != frequency) {
								cube.Faces[indexF++] = new Face {
									VA = indexV, 
									VB = indexV + 1, 
									VC = indexV + frequency + 1,
									NA = n,
									NB = n,
									NC = n
								};
								cube.Faces[indexF++] = new Face { 
									VA = indexV + frequency + 1, 
									VB = indexV + 1, 
									VC = indexV + frequency + 2,
									NA = n,
									NB = n,
									NC = n
								};
							}
							indexV++;
						}
					}
				}

				return cube;
			}
			public static Mesh CreatePlane(int frequency) {
				int nVerticesPlane = (frequency + 1) * (frequency + 1);
				int nFacesPlane = frequency * frequency * 2;

				Mesh plane = new Mesh("Cube", nVerticesPlane, 1, nFacesPlane);
				int indexV = 0, indexF = 0;

				Vector3 up = new Vector3(0, 0, 1);
				Vector3 right = new Vector3(up.Z, up.X, up.Y);
				Vector3 bottom = up.Cross(right);

				Vector3 initPosition = right * -0.5f + bottom * -0.5f + up * 0.5f;
				Vector3 dRight = right / frequency;
				Vector3 dBottom = bottom / frequency;
				plane.Normals[0] = up;
				for(int i = 0; i <= frequency; i++) {
					for(int j = 0; j <= frequency; j++) {
						plane.Vertexs[indexV] = initPosition + dRight * i + dBottom * j;

						if(j != frequency && i != frequency) {
							plane.Faces[indexF++] = new Face {
								VA = indexV,
								VB = indexV + 1,
								VC = indexV + frequency + 1,
								NA = 0,
								NB = 0,
								NC = 0
							};
							plane.Faces[indexF++] = new Face {
								VA = indexV + frequency + 1,
								VB = indexV + 1,
								VC = indexV + frequency + 2,
								NA = 0,
								NB = 0,
								NC = 0
							};
						}
						indexV++;
					}
				}
				

				return plane;
			}
			public static Mesh[] MeshesFromObjFile(string filename){
				List<Mesh> meshes = new List<Mesh>();
				Mesh mesh;

				string name = "";
				List<Vector3> vertexs = new List<Vector3>();
				List<Vector3> normals = new List<Vector3>();
				List<Vector2> tpoints = new List<Vector2>();
				List<Face> faces = new List<Face>();

				float x, y, z;
				int nVertexs = 1, nNormals = 1, nTPoints = 1;
				bool first = true;

				Func<string, IndexVertexFace> convertString = (faceString) => {
					string[] indexs = faceString.Split('/');
					int v = int.Parse(indexs[0]) - nVertexs;
					int t = int.Parse(indexs[1]) - nTPoints;
					int n = int.Parse(indexs[2]) - nNormals;
					return new IndexVertexFace { V = v, T = t, N = n };
				};

				foreach(string line in System.IO.File.ReadLines(filename)) {
					string[] tokens = line.Split(' ');
					switch(tokens[0]) {
						case "o":
							Console.WriteLine(line);
							if(!first) {
								mesh = new Mesh(name);
								mesh.Faces = faces.ToArray();
								mesh.Vertexs = vertexs.ToArray();
								mesh.Normals = normals.ToArray();
								mesh.TexturePoints = tpoints.ToArray();
								meshes.Add(mesh);

								nVertexs += vertexs.Count;
								nNormals += normals.Count;
								nTPoints += tpoints.Count;

								faces.Clear();
								vertexs.Clear();
								normals.Clear();
								tpoints.Clear();
							}
							first = false;
							name = tokens[1];
							break;
						case "v":
							x = float.Parse(tokens[1], System.Globalization.CultureInfo.InvariantCulture);
							y = float.Parse(tokens[2], System.Globalization.CultureInfo.InvariantCulture);
							z = float.Parse(tokens[3], System.Globalization.CultureInfo.InvariantCulture);
							vertexs.Add(new Vector3(x, y, z));
							break;
						case "vn":
							x = float.Parse(tokens[1], System.Globalization.CultureInfo.InvariantCulture);
							y = float.Parse(tokens[2], System.Globalization.CultureInfo.InvariantCulture);
							z = float.Parse(tokens[3], System.Globalization.CultureInfo.InvariantCulture);
							normals.Add(new Vector3(x, y, z));
							break;
						case "vt":
							x = float.Parse(tokens[1], System.Globalization.CultureInfo.InvariantCulture);
							y = float.Parse(tokens[2], System.Globalization.CultureInfo.InvariantCulture);
							tpoints.Add(new Vector2(x, y));
							break;
						case "f":
							IndexVertexFace a = convertString(tokens[1]);
							IndexVertexFace b = convertString(tokens[2]);
							IndexVertexFace c = convertString(tokens[3]);
							faces.Add(new Face { 
								VA = a.V, VB = b.V, VC = c.V,
								TA = a.T, TB = b.T, TC = c.T,
								NA = a.N, NB = b.N, NC = c.N
							});
							for(int i = 4; i < tokens.Length; i++) {
								a = convertString(tokens[i - 3]);
								b = convertString(tokens[i - 1]);
								c = convertString(tokens[i]);
								faces.Add(new Face { 
									VA = a.V, VB = b.V, VC = c.V,
									TA = a.T, TB = b.T, TC = c.T,
									NA = a.N, NB = b.N, NC = c.N
								});
							}
							break;
					}
				}
				mesh = new Mesh(name);
				mesh.Faces = faces.ToArray();
				mesh.Vertexs = vertexs.ToArray();
				mesh.Normals = normals.ToArray();
				mesh.TexturePoints = tpoints.ToArray();
				meshes.Add(mesh);

				return meshes.ToArray();
			}
		}
	}
}
