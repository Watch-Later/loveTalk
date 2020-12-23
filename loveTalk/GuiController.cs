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


        private static loveToy currentEditToy; 
        private static BluetoothDevice[] devices;
        private static bool btw_Scanning = false;
        private static async void btw_rescanDevices()
        {
            btw_Scanning = true;
            devices = await BluetoothManager.getDevices();
            btw_Scanning = false;
        }

        private static async void btw_Connect(BluetoothDevice dev)
        {
            var w = new loveToy(dev);
            if (!await w.connect())
                return;
            for (int i=0; i < rootInit.Toys.Length; i++)
                if (rootInit.Toys[i]==null)
                {
                    rootInit.Toys[i] = w;
                    return;
                }
            Console.WriteLine("No free toy slots.");
            throw new rootInit.CallYourTherapistException();

        }

       

        public static void renderElements()
        {
            ImGui.SetNextWindowPos(new Vector2(0, 0));
            ImGui.SetNextWindowSize(new Vector2(340, 200));
            ImGui.Begin("Bluetooth Control");
            {
                if (ImGui.Button("Rescan"))
                    btw_rescanDevices();

                if (devices != null)
                    foreach (BluetoothDevice dev in devices)
                        if (ImGui.Button(dev.Name))
                            btw_Connect(dev);
            }
            ImGui.End();


            ImGui.SetNextWindowPos(new Vector2(340, 0));
            ImGui.SetNextWindowSize(new Vector2(200, 200));
            ImGui.Begin("VRC Collider Detection");
            for (int i=0; i < rootInit.Toys.Length; i++)
            {
                var toyt = rootInit.Toys[i];
                if (toyt != null)
                    if (ImGui.Button(toyt.Model + " " + toyt.btDevice.Id))
                        currentEditToy = toyt;
            }
            ImGui.End();

           

            ImGui.SetNextWindowPos(new Vector2(0, 200));
            ImGui.SetNextWindowSize(new Vector2(540, 400));
            ImGui.Begin("Controller Manager");
            if (currentEditToy != null)
            {
                ImGui.Text($"Current Controller {(currentEditToy.Controller == null ? "NOTHING" : currentEditToy.Controller.Name)}");
                var item = 0;
                var items = rootInit.controllers.Keys.ToArray<string>();
                if (ImGui.Combo("Set new controller", ref item, items, items.Length))
                    currentEditToy.Controller = rootInit.controllers[items[item]].copy();
                if (currentEditToy.Controller!=null)
                {
                    for (int bi = 0; bi < currentEditToy.Controller.Callbacks.Count; bi++)
                    {
                        var hnd = currentEditToy.Controller.Callbacks[bi];
                        var param = currentEditToy.Controller.CallbackData[bi];
                        ImGui.Text(hnd.Name);
                        ImGui.Text($"A {(param.a == null ? "NOTHING" : param.a.name)}");

                       
                        item = 0;
                        items = new string[rootInit.colliders.Length];
                        for (int q = 0; q < items.Length; q++)
                            items[q] = rootInit.colliders[q].name;
                        
                        if (ImGui.Combo("Set new collider A##" + bi, ref item, items, items.Length))
                            param.a = ColliderCon.matchName(rootInit.colliders,items[item]);

                        ImGui.Text($"B {(param.b == null ? "NOTHING" : param.b.name)}");
                        item = 0;
                        if (ImGui.Combo("Set new collider B##" + bi, ref item, items, items.Length))
                            param.b = ColliderCon.matchName(rootInit.colliders, items[item]);
                        if (ImGui.Button("Any##" + bi))
                            param.b = new ColliderData() { any = true, name = "ANY" };


                        ImGui.Spacing();
                    }
                }
            }

            ImGui.End();
        }
    }
}
