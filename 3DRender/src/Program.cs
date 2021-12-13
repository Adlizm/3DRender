using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

using SoftRender;
using SoftRender.Objects;
using UtilNumerics;

namespace _3DRender {
	static class Program {
		private static Image i;
		private static Window w;
        private static Device3D device;

        private static Camera camera;
        private static Mesh[] meshes;

        [STAThread]
		static void Main() {
			camera = new Camera(new Vector3(0, 0, -5), new Vector3(0, 0, 0));
            
            meshes = new Mesh[] { Mesh.CreateSphare(4) };
            foreach(Mesh mesh in meshes)
                mesh.Material = new Material(Colors.Red, Colors.Gray, Colors.White, 10);
            

            i = new Image();
            RenderOptions.SetBitmapScalingMode(i, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetEdgeMode(i, EdgeMode.Aliased);

            device = new Device3D(400, 300);

            w = new Window();
            w.Content = i;
            w.Width = device.Bmp.Width;
            w.Height = device.Bmp.Height + 39;
            w.Show();

            i.Source = device.Bmp;
            i.Stretch = Stretch.None;
            i.HorizontalAlignment = HorizontalAlignment.Left;
            i.VerticalAlignment = VerticalAlignment.Top;

            CompositionTarget.Rendering += CompositionTarget_Rendering;

            Application app = new Application();
            app.Run();
        }

		static void CompositionTarget_Rendering(object sender, object e) {
            foreach(Mesh mesh in meshes) {
                mesh.Rotate.X += 0.01f;
                mesh.Rotate.Y += 0.01f;
            }
                
            device.Clear(Colors.Black);
            device.Render(camera, new Light(new Vector3(5, 5, 5)), meshes);
            device.Refresh();
        }

    }
}