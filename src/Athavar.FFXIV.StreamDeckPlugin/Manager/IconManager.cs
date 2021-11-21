// <copyright file="IconManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.StreamDeckPlugin.Manager;

using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Athavar.FFXIV.StreamDeckPlugin.Manager.Interface;
using Dalamud;
using Dalamud.Plugin;
using ImGuiScene;
using Lumina.Data.Files;
using Microsoft.Extensions.Logging;

/// <summary>
///     Manage loading of <see cref="IconData" /> from game files.
/// </summary>
internal class IconManager : IIconManager
{
    private const string IconFileFormat = "ui/icon/{0:D3}000/{1}{2:D6}{3}.tex";

    private readonly ILogger logger;
    private readonly IDalamudServices dalamud;
    private readonly IIpcManager ipcManager;

    // ReSharper disable once CollectionNeverUpdated.Local; Data is updated on request.
    private readonly IconDataCache iconDataHrCache;

    // ReSharper disable once CollectionNeverUpdated.Local; Data is updated on request.
    private readonly IconDataCache iconDataLrCache;

    /// <summary>
    ///     Initializes a new instance of the <see cref="IconManager" /> class.
    /// </summary>
    /// <param name="logger"><see cref="ILogger{TCategoryName}" /> added by DI.</param>
    /// <param name="dalamud"><see cref="IDalamudServices" /> added by DI.</param>
    /// <param name="ipcManager"><see cref="IIpcManager" /> added by DI.</param>
    public IconManager(ILogger<IconManager> logger, IDalamudServices dalamud, IIpcManager ipcManager)
    {
        this.logger = logger;
        this.dalamud = dalamud;
        this.ipcManager = ipcManager;
        this.iconDataHrCache = new IconDataCache(this, true);
        this.iconDataLrCache = new IconDataCache(this, false);
    }

    /// <inheritdoc />
    public TextureWrap? LoadTextureWrap(uint iconId, bool hr) => this.GetIcon(iconId, hr)?.LoadTextureWrap(this.dalamud.PluginInterface);

    /// <inheritdoc />
    public string? GetBase64(uint iconId, bool hr) => this.GetIcon(iconId, hr)?.GetBase64();

    /// <inheritdoc />
    public void SaveAsFile(uint iconId, bool hr, FileInfo fileInfo, ImageFormat format) => this.GetIcon(iconId, hr)?.SaveImage(fileInfo, format);

    /// <inheritdoc />
    public void CleanIconData()
    {
        this.iconDataLrCache.Clear();
        this.iconDataHrCache.Clear();
    }

    /// <summary>
    ///     Checks if an icon with the id exists.
    /// </summary>
    /// <param name="icon">The icon id.</param>
    /// <returns>returns a <see cref="bool" /> indicating if the icon exists or not.</returns>
    public bool IconExists(uint icon) =>
        this.dalamud.DataManager.FileExists(GetIconPath(icon, string.Empty, false))
     || this.dalamud.DataManager.FileExists(GetIconPath(icon, "en/", false));

    /// <summary>
    ///     Gets the file path of an icon.
    /// </summary>
    /// <param name="icon">The icon id.</param>
    /// <param name="language">The game language.</param>
    /// <param name="hr">Indicates if path is for high resolution icon.</param>
    /// <returns>returns the icon file path.</returns>
    private static string GetIconPath(uint icon, string language, bool hr)
    {
        var path = string.Format(IconFileFormat, icon / 1000, language, icon, hr ? "_hr1" : string.Empty);

        return path;
    }

    /// <summary>
    ///     Gets the file path of an icon.
    /// </summary>
    /// <param name="icon">The icon id.</param>
    /// <param name="language">The game language.</param>
    /// <param name="hr">Indicates if path is for high resolution icon.</param>
    /// <returns>returns the icon file path.</returns>
    private static string GetIconPath(uint icon, ClientLanguage language, bool hr)
    {
        var languagePath = language switch
                           {
                               ClientLanguage.Japanese => "ja/",
                               ClientLanguage.English => "en/",
                               ClientLanguage.German => "de/",
                               ClientLanguage.French => "fr/",
                               _ => "en/",
                           };

        return GetIconPath(icon, languagePath, hr);
    }

    private IconData? GetIcon(uint iconId, bool hr)
        => hr ? this.iconDataHrCache[iconId] : this.iconDataLrCache[iconId];

    private TexFile? GetTex(string path)
    {
        TexFile? tex = null;

        path = this.ipcManager.ResolvePenumbraPath(path);

        try
        {
            if (path[0] == '/' || path[1] == ':')
            {
                tex = this.dalamud.DataManager.GameData.GetFileFromDisk<TexFile>(path);
            }
        }
        catch
        {
            // ignored
        }

        tex ??= this.dalamud.DataManager.GetFile<TexFile>(path);
        return tex;
    }

    private TexFile? GetIconTex(uint icon, bool hr) =>
        this.GetTex(GetIconPath(icon, string.Empty, hr))
     ?? this.GetTex(GetIconPath(icon, this.dalamud.DataManager.Language, hr));

    private IconData? LoadIconData(TexFile? tex)
    {
        if (tex is null)
        {
            return null;
        }

        var imageBytes = tex.ImageData;
        if (tex.Header.Width > tex.Header.Height)
        {
            var newData = new byte[tex.Header.Width * tex.Header.Width * 4];
            var diff = (int)Math.Floor((tex.Header.Width - tex.Header.Height) / 2f);
            imageBytes.CopyTo(newData, diff * tex.Header.Width * 4);
            return new IconData(newData, tex.Header.Width, tex.Header.Width, 4);
        }

        if (tex.Header.Width < tex.Header.Height)
        {
            var newData = new byte[tex.Header.Height * tex.Header.Height * 4];
            var length = newData.Length / 4;
            var imageDataPos = 0;
            var diff = (tex.Header.Height - tex.Header.Width) / 2f;
            for (var i = 0; i < length; i++)
            {
                var column = i % tex.Header.Height;
                if (Math.Floor(diff) <= column && column < tex.Header.Height - Math.Ceiling(diff))
                {
                    var pixel = i * 4;
                    newData[pixel] = imageBytes[imageDataPos++];
                    newData[pixel + 1] = imageBytes[imageDataPos++];
                    newData[pixel + 2] = imageBytes[imageDataPos++];
                    newData[pixel + 3] = imageBytes[imageDataPos++];
                }
            }

            return new IconData(newData, tex.Header.Height, tex.Header.Height, 4);
        }

        return new IconData(imageBytes, tex.Header.Width, tex.Header.Height, 4);
    }

    private class IconDataCache : ConcurrentDictionary<uint, IconData?>
    {
        private readonly bool hr;
        private readonly IconManager manager;

        public IconDataCache(IconManager manager, bool hr)
        {
            this.manager = manager;
            this.hr = hr;
        }

        public new IconData? this[uint k]
        {
            get
            {
                if (this.TryGetValue(k, out var iconData))
                {
                    return iconData;
                }

                var tex = this.manager.GetIconTex(k, this.hr);
                var imageData = this.manager.LoadIconData(tex);
                this.TryUpdate(k, imageData, null);
                return imageData;
            }
        }
    }

    private class IconData
    {
        /// <summary>
        ///     A8R8G8B8.
        /// </summary>
        private readonly byte[] pixels;

        private readonly int channel;
        private readonly int width;
        private readonly int height;

        public IconData(byte[] pixels, int width, int height, int channel)
            => (this.pixels, this.width, this.height, this.channel) = (pixels, width, height, channel);

        public TextureWrap LoadTextureWrap(DalamudPluginInterface dalamud) => dalamud.UiBuilder.LoadImageRaw(this.GetRgba(), this.width, this.height, this.channel);

        public string GetBase64()
        {
            using MemoryStream ms = new();

            this.SafeImage(ms, ImageFormat.Png);

            return $"data:image/png;base64,{Convert.ToBase64String(ms.ToArray())}";
        }

        public void SaveImage(FileInfo file, ImageFormat format)
        {
            using var fs = file.OpenWrite();
            this.SafeImage(fs, format);
        }

        private byte[] GetRgba()
        {
            var rgbaImageData = new byte[this.pixels.Length];
            for (var index = 0; index < rgbaImageData.Length; index += 4)
            {
                rgbaImageData[index] = this.pixels[index + 2];
                rgbaImageData[index + 1] = this.pixels[index + 1];
                rgbaImageData[index + 2] = this.pixels[index];
                rgbaImageData[index + 3] = this.pixels[index + 3];
            }

            return rgbaImageData;
        }

        private unsafe void SafeImage(Stream stream, ImageFormat format)
        {
            fixed (byte* p = this.pixels)
            {
                var ptr = (IntPtr)p;
                var bitmap = new Bitmap(this.width, this.height, this.width * this.channel, PixelFormat.Format32bppArgb, ptr);
                bitmap.Save(stream, format);
            }
        }
    }
}