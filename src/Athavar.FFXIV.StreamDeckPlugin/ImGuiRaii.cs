﻿// <copyright file="ImGuiRaii.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.StreamDeckPlugin;

using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;

public sealed class ImGuiRaii : IDisposable
{
    private int colorStack;
    private int fontStack;
    private float indentation;

    private Stack<Action>? onDispose;
    private int styleStack;

    public static ImGuiRaii NewGroup() => new ImGuiRaii().Group();

    public static ImGuiRaii NewTooltip() => new ImGuiRaii().Tooltip();

    /// <inheritdoc />
    public void Dispose()
    {
        this.Unindent(this.indentation);
        this.PopColors(this.colorStack);
        this.PopStyles(this.styleStack);
        this.PopFonts(this.fontStack);
        if (this.onDispose != null)
        {
            this.End(this.onDispose.Count);
            this.onDispose = null;
        }
    }

    /// <summary>
    ///     Starts a Group. Group ends on dispose.
    /// </summary>
    /// <returns>returns object for dispose.</returns>
    public ImGuiRaii Group() => this.Begin(ImGui.BeginGroup, ImGui.EndGroup);

    /// <summary>
    ///     Starts a Tooltip. Tooltip ends on dispose.
    /// </summary>
    /// <returns>returns object for dispose.</returns>
    public ImGuiRaii Tooltip() => this.Begin(ImGui.BeginTooltip, ImGui.EndTooltip);

    public ImGuiRaii PushColor(ImGuiCol which, uint color)
    {
        ImGui.PushStyleColor(which, color);
        ++this.colorStack;
        return this;
    }

    public ImGuiRaii PushColor(ImGuiCol which, Vector4 color)
    {
        ImGui.PushStyleColor(which, color);
        ++this.colorStack;
        return this;
    }

    public ImGuiRaii PopColors(int n = 1)
    {
        var actualN = Math.Min(n, this.colorStack);
        if (actualN > 0)
        {
            ImGui.PopStyleColor(actualN);
            this.colorStack -= actualN;
        }

        return this;
    }

    public ImGuiRaii PushStyle(ImGuiStyleVar style, Vector2 value)
    {
        ImGui.PushStyleVar(style, value);
        ++this.styleStack;
        return this;
    }

    public ImGuiRaii PushStyle(ImGuiStyleVar style, float value)
    {
        ImGui.PushStyleVar(style, value);
        ++this.styleStack;
        return this;
    }

    public ImGuiRaii PopStyles(int n = 1)
    {
        var actualN = Math.Min(n, this.styleStack);
        if (actualN > 0)
        {
            ImGui.PopStyleVar(actualN);
            this.styleStack -= actualN;
        }

        return this;
    }

    public ImGuiRaii PushFont(ImFontPtr font)
    {
        ImGui.PushFont(font);
        ++this.fontStack;
        return this;
    }

    public ImGuiRaii PopFonts(int n = 1)
    {
        var actualN = Math.Min(n, this.fontStack);

        while (actualN-- > 0)
        {
            ImGui.PopFont();
            --this.fontStack;
        }

        return this;
    }

    public ImGuiRaii Indent(float width)
    {
        if (width != 0)
        {
            ImGui.Indent(width);
            this.indentation += width;
        }

        return this;
    }

    public ImGuiRaii Unindent(float width) => this.Indent(-width);

    public bool Begin(Func<bool> begin, Action end)
    {
        if (begin())
        {
            this.onDispose ??= new Stack<Action>();
            this.onDispose.Push(end);
            return true;
        }

        return false;
    }

    public ImGuiRaii Begin(Action begin, Action end)
    {
        begin();
        this.onDispose ??= new Stack<Action>();
        this.onDispose.Push(end);
        return this;
    }

    public void End(int n = 1)
    {
        var actualN = Math.Min(n, this.onDispose?.Count ?? 0);
        while (actualN-- > 0) this.onDispose!.Pop()();
    }
}