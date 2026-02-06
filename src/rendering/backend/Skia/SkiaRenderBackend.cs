using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SDL2;
using TextEditorProject.Core.Contracts;
using TextEditorProject.Core.Snapshots;
using TextEditorProject.Core.Input;
using TextEditorProject.Rendering.UI.Abstractions;
using TextEditorProject.Rendering.backend.Skia;
using Avalonia.Automation;

namespace TextEditorProject.Rendering.Backend.Skia
{
    public class SkiaRenderBackend : IRenderBackend
    {
        public bool IsRunning { get; set; }
        private IntPtr _window;
        private IntPtr _renderer;
        private IntPtr _texture;
        private int _width;
        private int _height;
        private EditorSnapshot _snapshot;
#nullable disable
        public UiNode RootNote { get; set; }
        IEditorEngine Engine;
#nullable restore

        public void CreateWindow(IEditorEngine engine, string title, int width, int height)
        {
            Engine = engine;
            _width = width;
            _height = height;
            IsRunning = true;

            if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO) < 0)
                throw new Exception($"Could not initialize SDL2: {SDL.SDL_GetError()}");

            SDL.SDL_StartTextInput();

            _window = SDL.SDL_CreateWindow(title, SDL.SDL_WINDOWPOS_CENTERED, SDL.SDL_WINDOWPOS_CENTERED, width, height, SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN);

            _renderer = SDL.SDL_CreateRenderer(_window, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);
            _texture = SDL.SDL_CreateTexture(_renderer, SDL.SDL_PIXELFORMAT_ARGB8888, (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING, width, height);

            engine.Execute("editor.newEditor");
            engine.Execute("editor.openFile", "./testProject/program.c");
        }

        public void Render(EditorSnapshot? snapshot)
        {

            // Create a Skia surface for drawing
            using SKSurface surface = SKSurface.Create(new SKImageInfo(_width, _height, SKColorType.Bgra8888, SKAlphaType.Premul));
            SKCanvas canvas = surface.Canvas;
            canvas.Clear(SKColors.Black);

            if (snapshot.HasValue)
            {
                _snapshot = snapshot.Value;
                using SKFont font = new SKFont { Size = 16 };
                using SKPaint paint = new SKPaint { Color = SKColors.White, IsAntialias = true };

                /*
                */

                DrawNode(canvas, RootNote);
            }

            // Upload to SDL texture
            SKPixmap data = surface.PeekPixels();
            SDL.SDL_UpdateTexture(_texture, IntPtr.Zero, data.GetPixels(), data.RowBytes);

            SDL.SDL_RenderClear(_renderer);
            SDL.SDL_RenderCopy(_renderer, _texture, IntPtr.Zero, IntPtr.Zero);
            SDL.SDL_RenderPresent(_renderer);
        }

        private void DrawNode(SKCanvas canvas, UiNode node)
        {
            switch (node)
            {
                case UiButtonNode button:
                    DrawButton(canvas, button);
                    break;

                case UiEditorNode editorNode:
                    DrawEditor(canvas, editorNode);
                    break;

                case UiTextFieldNode text:
                    // DrawTextField(canvas, text);
                    break;

                case UiContainerNode container:
                    foreach (var child in container.Children)
                        DrawNode(canvas, child);
                    break;
            }
        }

        void DrawButton(SKCanvas canvas, UiButtonNode button)
        {
            using var paint = new SKPaint
            {
                Color = button.IsEnabled ? SKColors.LightGray : SKColors.DarkGray,
                IsAntialias = true
            };
            SKFont font = new SKFont { Size = 16 };

            canvas.DrawRect(button.Bounds.ToSkia(), paint);

            paint.Color = SKColors.Black;

            canvas.DrawText(button.Text, button.Bounds.X + 6, button.Bounds.Y + 18, font, paint);
        }

        void DrawEditor(SKCanvas canvas, UiEditorNode editorNode)
        {

            using var paint = new SKPaint
            {
                Color = SKColors.DarkGray,
                IsAntialias = true
            };
            SKFont font = new SKFont { Size = 16 };

            canvas.DrawRect(editorNode.Bounds.ToSkia(), paint);

            paint.Color = SKColors.White;

#nullable disable
            TextEditorSnapshot editorSnapshot = _snapshot.GetTextEditorSnapshot(editorNode.EditorId);
#nullable restore

            if (editorSnapshot != null)
            {
#nullable disable
                DocumentSnapshot documentSnapshot = _snapshot.GetDocumentSnapshot(editorSnapshot.DocumentId);
#nullable restore
                if (documentSnapshot != null)
                {
                    for (int i = 0; i < documentSnapshot.Lines.Count; i++)
                    {
                        canvas.DrawText(documentSnapshot.Lines[i].Text, 10, 20 + i * 20, font, paint);
                    }

                    CursorSnapshot cursor = editorSnapshot.Cursor;
                    if (cursor.Position.Line < documentSnapshot.Lines.Count)
                    {
                        LineSnapshot line = documentSnapshot.Lines[cursor.Position.Line];
                        string lineText = line.Text;

                        float x = 10;
                        if (cursor.Position.Column > 0)
                        {
                            x += font.MeasureText(lineText.Substring(0, Math.Min(cursor.Position.Column, lineText.Length)));
                        }
                        float y = 20 + cursor.Position.Line * 20;
                        canvas.DrawLine(x, y - 15, x, y, paint);
                    }
                }
            }
        }


        public void Run()
        {
            while (SDL.SDL_PollEvent(out SDL.SDL_Event e) != 0)
            {
                if (e.type == SDL.SDL_EventType.SDL_QUIT)
                {

                    IsRunning = false;
                }
                else if (e.type == SDL.SDL_EventType.SDL_TEXTINPUT)
                {
                    SDL.SDL_TextInputEvent textInputEvent = e.text;
                    string text = "";
                    unsafe
                    {
                        char* textBytes = (char*)textInputEvent.text;

                        for (int i = 0; i < SDL.SDL_TEXTINPUTEVENT_TEXT_SIZE; i++)
                        {
                            if (textBytes[i] == 0)
                            {
                                break;
                            }
                            text += textBytes[i];
                        }
                    }
                    Console.WriteLine($"output text {text}");
                    // TEMP: make it in to a task and run it later outside of this function
                    {
                        Engine.InputAdapter.TextInput(text);
                    }
                }
                else
                {
                    switch (e.type)
                    {
                        case SDL.SDL_EventType.SDL_KEYDOWN:
                        case SDL.SDL_EventType.SDL_KEYUP:
                            SDL.SDL_Keycode c = e.key.keysym.sym;
                            Console.WriteLine($"input char is {c}");
                            Key? key = TranslateKey(c);
                            if (key == null)
                                break;
                            var mods = TranslateModifiers(e.key.keysym.mod);
                            bool isRepeat = e.key.repeat != 0;

                            var evt = new InputEvent(key.Value, mods, isRepeat);

                            if (e.type == SDL.SDL_EventType.SDL_KEYDOWN)
                                Engine.InputAdapter.KeyDown(evt);
                            else
                                Engine.InputAdapter.KeyUp(evt);


                            break;
                    }
                }
            }
        }

        private static Modifiers TranslateModifiers(SDL.SDL_Keymod mod)
        {
            Modifiers result = Modifiers.None;

            if (mod.HasFlag(SDL.SDL_Keymod.KMOD_CTRL))
                result |= Modifiers.Control;

            if (mod.HasFlag(SDL.SDL_Keymod.KMOD_SHIFT))
                result |= Modifiers.Shift;

            if (mod.HasFlag(SDL.SDL_Keymod.KMOD_ALT))
                result |= Modifiers.Alt;

            if (mod.HasFlag(SDL.SDL_Keymod.KMOD_GUI))
                result |= Modifiers.Meta;

            return result;
        }


        private static Key? TranslateKey(SDL.SDL_Keycode key)
        {
            return key switch
            {
                SDL.SDL_Keycode.SDLK_LEFT => Key.Left,
                SDL.SDL_Keycode.SDLK_RIGHT => Key.Right,
                SDL.SDL_Keycode.SDLK_UP => Key.Up,
                SDL.SDL_Keycode.SDLK_DOWN => Key.Down,

                SDL.SDL_Keycode.SDLK_RETURN => Key.Enter,
                SDL.SDL_Keycode.SDLK_BACKSPACE => Key.Backspace,
                SDL.SDL_Keycode.SDLK_DELETE => Key.Delete,
                SDL.SDL_Keycode.SDLK_TAB => Key.Tab,
                SDL.SDL_Keycode.SDLK_ESCAPE => Key.Escape,

                SDL.SDL_Keycode.SDLK_HOME => Key.Home,
                SDL.SDL_Keycode.SDLK_END => Key.End,
                SDL.SDL_Keycode.SDLK_PAGEUP => Key.PageUp,
                SDL.SDL_Keycode.SDLK_PAGEDOWN => Key.PageDown,

                _ => null
            };
        }


        public void Close()
        {
            if (_texture != IntPtr.Zero) SDL.SDL_DestroyTexture(_texture);
            if (_renderer != IntPtr.Zero) SDL.SDL_DestroyRenderer(_renderer);
            if (_window != IntPtr.Zero) SDL.SDL_DestroyWindow(_window);
            SDL.SDL_Quit();
        }
    }
}