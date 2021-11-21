// <copyright file="IIconManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.StreamDeckPlugin.Manager.Interface;

using System.Drawing.Imaging;
using System.IO;
using ImGuiScene;

/// <summary>
///     Describe methods to receive and check icons from the game.
/// </summary>
internal interface IIconManager
{
    /// <summary>
    ///     Loads the <see cref="TextureWrap" /> of a icon.
    /// </summary>
    /// <param name="iconId">The icon id.</param>
    /// <param name="hr">Indicates if high resolution icon is requested.</param>
    /// <returns>returns the icon data.</returns>
    TextureWrap? LoadTextureWrap(uint iconId, bool hr);

    /// <summary>
    ///     Gets the icon as base64 <see cref="string" />.
    /// </summary>
    /// <param name="iconId">The icon id.</param>
    /// <param name="hr">Indicates if high resolution icon is requested.</param>
    /// <returns>returns the icon data.</returns>
    string? GetBase64(uint iconId, bool hr);

    /// <summary>
    ///     Save a icon as file.
    /// </summary>
    /// <param name="iconId">The icon id.</param>
    /// <param name="hr">Indicates if high resolution icon is requested.</param>
    /// <param name="fileInfo">The <see cref="FileInfo" />.</param>
    /// <param name="format">The <see cref="ImageFormat" />.</param>
    void SaveAsFile(uint iconId, bool hr, FileInfo fileInfo, ImageFormat format);

    /// <summary>
    ///     Clean up loaded icon data.
    /// </summary>
    void CleanIconData();

    /// <summary>
    ///     Checks if an icon with the id exists.
    /// </summary>
    /// <param name="icon">The icon id.</param>
    /// <returns>returns a <see cref="bool" /> indicating if the icon exists or not.</returns>
    bool IconExists(uint icon);
}