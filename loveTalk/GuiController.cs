using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using ImGuiNET;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using System.Numerics;
using static ImGuiNET.ImGuiNative;

using InTheHand;
using InTheHand.Bluetooth;

namespace loveTalk
{
    class GuiController
    {
        private static Sdl2Window _window;
        private static GraphicsDevice _gd;
        private static CommandList _cl;

        private static ImGuiController _controller;
        private static Vector3 _clearColor = new Vector3(0.45f, 0.55f, 0.6f);

        public static void init()
        {
            VeldridStartup.CreateWindowAndGraphicsDevice(
            new WindowCreateInfo(50, 50, 1024, 768, WindowState.Normal, "Lovetap"),
            new GraphicsDeviceOptions(true, null, true),
            out _window,
            out _gd);
            _window.Resized += () =>
            {
                _gd.MainSwapchain.Resize((uint)_window.Width, (uint)_window.Height);
                _controller.WindowResized(_window.Width, _window.Height);
            };
            

            _cl = _gd.ResourceFactory.CreateCommandList();
            _controller = new ImGuiController(_gd, _gd.MainSwapchain.Framebuffer.OutputDescription, _window.Width, _window.Height);
        }

        public static void update()
        {
            if (_window == null)
                return;
            InputSnapshot snapshot = _window.PumpEvents();
            if (!_window.Exists) { return; }
            _controller.Update(1f / 60f, snapshot); // Feed the input events to our ImGui controller, which passes them through to ImGui.
            renderElements();
            _cl.Begin();
            _cl.SetFramebuffer(_gd.MainSwapchain.Framebuffer);
            _cl.ClearColorTarget(0, new RgbaFloat(_clearColor.X, _clearColor.Y, _clearColor.Z, 1f));
            _controller.Render(_gd, _cl);
            _cl.End();
            _gd.SubmitCommands(_cl);
            _gd.SwapBuffers(_gd.MainSwapchain);
        }



    
        public static void renderElements()
        {
            ImGui.SetNextWindowPos(new Vector2(0, 0));
            ImGui.SetNextWindowSize(new Vector2(340, 200));
            ImGui.Begin("Bluetooth Control");
            {
              

            }
            ImGui.End();
        }
    }
}
