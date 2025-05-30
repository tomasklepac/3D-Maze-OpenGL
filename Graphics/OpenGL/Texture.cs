using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

namespace MazeProject.Graphics.OpenGL
{
    /// <summary>
    /// Represents a 2D texture loaded from file and configured for use in OpenGL.
    /// </summary>
    public class Texture
    {
        private const int GL_TEXTURE_MAX_ANISOTROPY_EXT = 0x84FE;

        /// <summary>
        /// The OpenGL handle for the created texture.
        /// </summary>
        public int Handle { get; private set; }

        /// <summary>
        /// Loads an RGBA texture from the given image file and configures mipmapping and filtering.
        /// </summary>
        /// <param name="path">The file path to the image texture.</param>
        public Texture(string path)
        {
            Handle = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, Handle);

            // Load image data using STB
            using (var stream = File.OpenRead(path))
            {
                var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                    image.Width, image.Height, 0,
                    PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
            }

            // Generate mipmaps
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            // Check for anisotropic filtering support
            bool supportsAnisotropy = GL.GetString(StringName.Extensions)
                .Contains("GL_EXT_texture_filter_anisotropic");

            float maxAniso = 0f;
            if (supportsAnisotropy)
                GL.GetFloat((GetPName)GL_TEXTURE_MAX_ANISOTROPY_EXT, out maxAniso);

            // Apply texture filtering and anisotropy (if supported)
            if (supportsAnisotropy && maxAniso > 0f)
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                    (int)TextureMinFilter.LinearMipmapLinear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                    (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, (TextureParameterName)All.TextureMaxAnisotropyExt,
                    MathF.Min(4.0f, maxAniso));
            }
            else
            {
                // Fallback: use basic linear filtering
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                    (int)TextureMinFilter.LinearMipmapLinear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                    (int)TextureMagFilter.Linear);
            }
        }

        /// <summary>
        /// Activates and binds the texture to the specified texture unit.
        /// </summary>
        /// <param name="unit">The texture unit to bind to (default is Texture0).</param>
        public void Use(TextureUnit unit = TextureUnit.Texture0)
        {
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, Handle);
        }
    }
}
