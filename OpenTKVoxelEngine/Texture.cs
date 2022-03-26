using System;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL4;

using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace OpenTKVoxelEngine_Texture
{

    public class Texture
    {

        public readonly int Handle;

        public Texture(int glHandle)
        {
            Handle = glHandle;
        }

        public void Use(TextureUnit unit = TextureUnit.Texture0)
        {
            // Activate the texture unit first before binding texture.
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, Handle);
        }

        public static Texture LoadFromFile(string path)
        {
            // Generate handle.
            int handle = GL.GenTexture();

            // Bind the handle.
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, handle);

            // Load the image.
            using (Bitmap image = new Bitmap(path))
            {
                // Our bitmap loads from the top-left pixel, whereas OpenGL load from the bottom-left, causing the texture to be flipped vertically.
                // This will correct that, making the texture display properly.
                image.RotateFlip(RotateFlipType.RotateNoneFlipY);

                // Now get our pixels from the bitmap loaded.
                BitmapData data = image.LockBits(new Rectangle(0,0, image.Width, image.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                // Now that our pixels are prepared, it's time to generate a texture.
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

                // Now our texture is loaded, we can set a few settings to affect how the image appears on rendering.
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);

                // Now set the wrapping mode. S is for the X axis, and T is for the Y axis.
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

                // Then generate mipmaps.
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

                return new Texture(handle);
            }
        }

    }

}
