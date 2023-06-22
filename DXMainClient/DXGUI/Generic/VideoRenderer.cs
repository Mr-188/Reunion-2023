using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rampastring.XNAUI;

namespace DTAClient.DXGUI.Generic
{
    public class VideoRenderer : DrawableGameComponent
    {
        public VideoRenderer(WindowManager windowManager, GraphicsDevice graphicsDevice) : base(windowManager.Game)
        {
            this.windowManager = windowManager;
            this.graphicsDevice = graphicsDevice;
            spriteBatch = new SpriteBatch(graphicsDevice);
        }

        private WindowManager windowManager;
        private GraphicsDevice graphicsDevice;
        private SpriteBatch spriteBatch;

        public Texture2D VideoTexture { get; set; }

        public override void Draw(GameTime gameTime)
        {
            if (VideoTexture != null)
            {
                spriteBatch.Begin();
                spriteBatch.Draw(VideoTexture, new Rectangle(0, 0, 1280, 768), Color.White);
                spriteBatch.End();
            }

            base.Draw(gameTime);
        }
    }
}
