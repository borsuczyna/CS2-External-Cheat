namespace CS2.Core;

using System;
using System.Collections.Generic;
using System.Text;
using GameOverlay.Drawing;
using GameOverlay.Windows;

public class Overlay : IDisposable
{
	public readonly GraphicsWindow _window;
	public readonly System.Diagnostics.Process? _process;
	public readonly DateTime _start = DateTime.Now;

	public readonly Dictionary<string, SolidBrush> colors;
	private readonly Dictionary<string, (int, int, int)> _colorsToLoad = new()
	{
		["black"] = (0, 0, 0),
		["white"] = (255, 255, 255),
		["red"] = (255, 0, 0),
		["green"] = (0, 255, 0),
		["blue"] = (0, 0, 255),
	};

	public readonly Dictionary<string, Font> fonts;
	private readonly Dictionary<string, (string, int)> _fontsToLoad = new()
	{
		["consolas"] = ("Consolas", 14),
		["arial"] = ("Arial", 24),
	};

	public Overlay(string processName)
	{
		colors = new Dictionary<string, SolidBrush>();
		fonts = new Dictionary<string, Font>();

		// var notepad = System.Diagnostics.Process.GetProcessesByName("cs2")[0].MainWindowHandle;
		_process = System.Diagnostics.Process.GetProcessesByName(processName)[0];
		if (_process == null)
		{
			throw new Exception($"[Overlay.cs] Failed to initialize overlay for process '{processName}'.");
		}

		var gfx = new Graphics()
		{
			MeasureFPS = true,
			PerPrimitiveAntiAliasing = true,
			TextAntiAliasing = true,
			WindowHandle = _process.MainWindowHandle,
		};

		_window = new StickyWindow(_process.MainWindowHandle, gfx)
		{
			FPS = 240,
			IsTopmost = true,
			IsVisible = true,
			AttachToClientArea = true,
		};

		_window.DestroyGraphics += _window_DestroyGraphics;
		_window.DrawGraphics += _window_DrawGraphics;
		_window.SetupGraphics += _window_SetupGraphics;

		// Start the overlay
		_window.Create();
		_window.Join();
	}

	private void _window_SetupGraphics(object? sender, SetupGraphicsEventArgs e)
	{
		var gfx = e.Graphics;

		// Dispose old resources on recreate
		if (e.RecreateResources)
		{
			DisposeGraphics();
		}

		// Create new resources
		LoadColors(gfx);

		if (e.RecreateResources) return;	

		// Create fonts
		LoadFonts(gfx);
	}

	private void _window_DestroyGraphics(object? sender, DestroyGraphicsEventArgs e)
	{
		foreach (var pair in colors) pair.Value.Dispose();
		foreach (var pair in fonts) pair.Value.Dispose();
	}

	private void _window_DrawGraphics(object? sender, DrawGraphicsEventArgs e)
	{
		if (_process == null) return;
	
		var gfx = e.Graphics;
		gfx.ClearScene();

		if (!ProcessHelper.IsProcessActiveWindow(_process)) return;

		var cursorPos = ProcessHelper.GetCursorPosition(_process);
		Windows.DrawWatermark(this, gfx, cursorPos);
		_ = Windows.UseTriggerBot(this, gfx, cursorPos);
		_ = Windows.DrawEsp(this, gfx, cursorPos);
	}

	private void LoadFonts(Graphics gfx)
	{
		foreach (var font in _fontsToLoad)
		{
			fonts[font.Key] = gfx.CreateFont(font.Value.Item1, font.Value.Item2);
		}
	}

	private void LoadColors(Graphics gfx)
	{
		foreach (var pair in _colorsToLoad)
		{	
			colors[pair.Key] = gfx.CreateSolidBrush(pair.Value.Item1, pair.Value.Item2, pair.Value.Item3);
		}
	}

	private void DisposeGraphics()
	{
		foreach (var pair in colors) pair.Value.Dispose();
	}

	~Overlay()
	{
		Dispose(false);
	}

	#region IDisposable Support
	private bool disposedValue;

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			_window.Dispose();

			disposedValue = true;
		}
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}
	#endregion
}