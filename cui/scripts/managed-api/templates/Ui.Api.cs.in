using static global::Managed.Forms.Cui;
using CBool = global::Managed.Forms.Cui.CBool;

namespace Cui.Managed;

// Native singleton arguments are synchronous stack locals. Pointer fields retain
// their frame/owned backing storage; see docs/MANAGED-STORAGE-AUDIT.md.
public sealed unsafe partial class Ui
{
    public string? AccentName(Accent accent)
    {
        Check();
        var result = cui_accent_name((cui_accent)CheckEnum(accent));
        SyncState();
        return Decode(result);
    }

    public UiScope AccordionBegin(string title, ref bool open)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(title);
        CheckBegin();
        CBool native_open = open;
        using var __titleUtf8 = new ScopedUtf8(title, stackalloc byte[256]);
        fixed (byte* __titlePointer = __titleUtf8.Bytes)
        {
            var result = cui_accordion_begin(_ctx, __titlePointer, &native_open);
            open = (native_open != 0);
            SyncState();
            return TrackScope(ScopeKind.Accordion, result != 0, result != 0 ? 1 : 0);
        }
    }

    public void AccordionEnd()
    {
        CheckFrame();
        BeforeExplicitEnd(ScopeKind.Accordion);
        cui_accordion_end(_ctx);
        SyncState();
    }

    public int ActivityRings(string id, ReadOnlySpan<VizItem> rings, int count, RingsOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckLength(count, rings.Length, nameof(count));
        var native_rings = CopyVizItem(rings);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_activity_rings_ex(_ctx, __idPointer, native_rings, count, &native_opts);
            SyncState();
            return result;
        }
    }

    public int AgentPlan(string id, ReadOnlySpan<PlanStep> steps, int count, AgentPlanOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckLength(count, steps.Length, nameof(count));
        var native_steps = CopyPlanStep(steps);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_agent_plan_ex(_ctx, __idPointer, native_steps, count, &native_opts);
            SyncState();
            return result;
        }
    }

    public void AgentStatus(string text, AgentStatusOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(text);
        var native_opts = ToNative(opts);
        using var __textUtf8 = new ScopedUtf8(text, stackalloc byte[256]);
        fixed (byte* __textPointer = __textUtf8.Bytes)
        {
            cui_agent_status_ex(_ctx, __textPointer, &native_opts);
            SyncState();
        }
    }

    public bool Alert(string title, AlertOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(title);
        var native_opts = ToNative(opts);
        using var __titleUtf8 = new ScopedUtf8(title, stackalloc byte[256]);
        fixed (byte* __titlePointer = __titleUtf8.Bytes)
        {
            var result = cui_alert_ex(_ctx, __titlePointer, &native_opts);
            SyncState();
            return (result != 0);
        }
    }

    public ApiExplorerResult ApiExplorer(string id, ReadOnlySpan<ApiEndpoint> endpoints, int count, ApiExplorerOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckLength(count, endpoints.Length, nameof(count));
        var native_endpoints = CopyApiEndpoint(endpoints);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_api_explorer_ex(_ctx, __idPointer, native_endpoints, count, &native_opts);
            SyncState();
            return FromNative(result);
        }
    }

    public void Avatar(string name, AvatarOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(name);
        var native_opts = ToNative(opts);
        using var __nameUtf8 = new ScopedUtf8(name, stackalloc byte[256]);
        fixed (byte* __namePointer = __nameUtf8.Bytes)
        {
            cui_avatar_ex(_ctx, __namePointer, &native_opts);
            SyncState();
        }
    }

    public void AvatarGroup(ReadOnlySpan<string> names, int count, int max_visible, float size)
    {
        CheckFrame();
        CheckLength(count, names.Length, nameof(count));
        cui_avatar_group(_ctx, Strings(names), count, max_visible, size);
        SyncState();
    }

    public int AvatarUpload(string id, string name, ref Texture? image, float size)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(name);
        var native_image = Unwrap(image);
        using var __nameUtf8 = new ScopedUtf8(name, stackalloc byte[256]);
        fixed (byte* __namePointer = __nameUtf8.Bytes)
        {
            using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
            fixed (byte* __idPointer = __idUtf8.Bytes)
            {
                var result = cui_avatar_upload(_ctx, __idPointer, __namePointer, &native_image, size);
                if (image != null && image.Value != native_image)
                {
                    _textures.Remove(image.Value);
                    _pendingTextures.Remove(image);
                    image.Invalidate();
                }

                image = FindTexture(native_image);
                SyncState();
                return result;
            }
        }
    }

    public void Badge(string text, BadgeOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(text);
        var native_opts = ToNative(opts);
        using var __textUtf8 = new ScopedUtf8(text, stackalloc byte[256]);
        fixed (byte* __textPointer = __textUtf8.Bytes)
        {
            cui_badge_ex(_ctx, __textPointer, &native_opts);
            SyncState();
        }
    }

    public bool Banner(string text, ref bool open, BannerOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(text);
        CBool native_open = open;
        var native_opts = ToNative(opts);
        using var __textUtf8 = new ScopedUtf8(text, stackalloc byte[256]);
        fixed (byte* __textPointer = __textUtf8.Bytes)
        {
            var result = cui_banner_ex(_ctx, __textPointer, &native_open, &native_opts);
            open = (native_open != 0);
            SyncState();
            return (result != 0);
        }
    }

    public void BarChart(string id, ReadOnlySpan<Series> series, int count, ChartOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckLength(count, series.Length, nameof(count));
        var native_series = CopySeries(series);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            cui_bar_chart_ex(_ctx, __idPointer, native_series, count, &native_opts);
            SyncState();
        }
    }

    public int BarList(string id, ReadOnlySpan<VizItem> items, int count, BarListOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckLength(count, items.Length, nameof(count));
        var native_items = CopyVizItem(items);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_bar_list_ex(_ctx, __idPointer, native_items, count, &native_opts);
            SyncState();
            return result;
        }
    }

    public BlameResult Blame(string id, ReadOnlySpan<string> lines, ReadOnlySpan<int> line_commit, int line_count, ReadOnlySpan<BlameCommit> commits, int commit_count, BlameOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckLength(line_count, lines.Length, nameof(line_count));
        CheckLength(line_count, line_commit.Length, nameof(line_count));
        CheckLength(commit_count, commits.Length, nameof(commit_count));
        CheckLength(line_count, lines.Length, nameof(line_count));
        var native_line_commit = CopyInt(line_commit);
        var native_commits = CopyBlameCommit(commits);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_blame_ex(_ctx, __idPointer, Strings(lines), native_line_commit, line_count, native_commits, commit_count, &native_opts);
            SyncState();
            return FromNative(result);
        }
    }

    public UiScope BoxBegin(Box opts = default)
    {
        CheckFrame();
        CheckBegin();
        var native_opts = ToNative(opts);
        var result = cui_box_begin(_ctx, &native_opts);
        SyncState();
        return TrackScope(ScopeKind.Box, true, unchecked((int)result));
    }

    public void BoxEnd()
    {
        CheckFrame();
        BeforeExplicitEnd(ScopeKind.Box);
        cui_box_end(_ctx);
        SyncState();
    }

    public int Breadcrumb(ReadOnlySpan<string> items, int count)
    {
        CheckFrame();
        CheckLength(count, items.Length, nameof(count));
        var result = cui_breadcrumb(_ctx, Strings(items), count);
        SyncState();
        return result;
    }

    public bool Button(string label, ButtonOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(label);
        var native_opts = ToNative(opts);
        using var __labelUtf8 = new ScopedUtf8(label, stackalloc byte[256]);
        fixed (byte* __labelPointer = __labelUtf8.Bytes)
        {
            var result = cui_button_ex(_ctx, __labelPointer, &native_opts);
            SyncState();
            return (result != 0);
        }
    }

    public UiScope ButtonGroupBegin()
    {
        CheckFrame();
        CheckBegin();
        cui_button_group_begin(_ctx);
        SyncState();
        return TrackScope(ScopeKind.ButtonGroup, true, 0);
    }

    public void ButtonGroupEnd()
    {
        CheckFrame();
        BeforeExplicitEnd(ScopeKind.ButtonGroup);
        cui_button_group_end(_ctx);
        SyncState();
    }

    public int CalendarHeatmap(string id, Date start, ReadOnlySpan<float> values, int days, CalendarHeatmapOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckLength(days, values.Length, nameof(days));
        var native_values = CopyFloat(values);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_calendar_heatmap_ex(_ctx, __idPointer, ToNative(start), native_values, days, &native_opts);
            SyncState();
            return result;
        }
    }

    public int Candlestick(string id, ReadOnlySpan<Candle> candles, int count, CandlestickOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckLength(count, candles.Length, nameof(count));
        CheckOptional(count, opts.Labels.Length, nameof(opts.Labels));
        var native_candles = CopyCandle(candles);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_candlestick_ex(_ctx, __idPointer, native_candles, count, &native_opts);
            SyncState();
            return result;
        }
    }

    public UiScope CardBegin(CardOpts opts = default)
    {
        CheckFrame();
        CheckBegin();
        var native_opts = ToNative(opts);
        var result = cui_card_begin(_ctx, &native_opts);
        SyncState();
        return TrackScope(ScopeKind.Card, true, result != 0 ? 1 : 0);
    }

    public void CardEnd()
    {
        CheckFrame();
        BeforeExplicitEnd(ScopeKind.Card);
        cui_card_end(_ctx);
        SyncState();
    }

    public UiScope CardStackBegin(string id, int count, CardStackOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckBegin();
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            cui_card_stack_begin(_ctx, __idPointer, count, &native_opts);
            SyncState();
            return TrackScope(ScopeKind.CardStack, true, 0);
        }
    }

    public int CardStackEnd()
    {
        CheckFrame();
        BeforeExplicitEnd(ScopeKind.CardStack);
        var result = cui_card_stack_end(_ctx);
        SyncState();
        return result;
    }

    public UiScope CardStackItemBegin(int depth)
    {
        CheckFrame();
        CheckBegin();
        cui_card_stack_item_begin(_ctx, depth);
        SyncState();
        return TrackScope(ScopeKind.CardStackItem, true, 0);
    }

    public void CardStackItemEnd()
    {
        CheckFrame();
        BeforeExplicitEnd(ScopeKind.CardStackItem);
        cui_card_stack_item_end(_ctx);
        SyncState();
    }

    public UiScope CarouselBegin(string id, int count, State<int> index, CarouselOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckBegin();
        if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));

        ArgumentNullException.ThrowIfNull(index);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_carousel_begin(_ctx, __idPointer, count, BindInt(index), &native_opts);
            SyncState();
            return TrackScope(ScopeKind.Carousel, true, result != 0 ? 1 : 0);
        }
    }

    public void CarouselEnd()
    {
        CheckFrame();
        BeforeExplicitEnd(ScopeKind.Carousel);
        cui_carousel_end(_ctx);
        SyncState();
    }

    public UiScope CarouselSlideBegin()
    {
        CheckFrame();
        CheckBegin();
        RequireScope(ScopeKind.Carousel);
        cui_carousel_slide_begin(_ctx);
        SyncState();
        return TrackScope(ScopeKind.CarouselSlide, true, 0);
    }

    public void CarouselSlideEnd()
    {
        CheckFrame();
        BeforeExplicitEnd(ScopeKind.CarouselSlide);
        cui_carousel_slide_end(_ctx);
        SyncState();
    }

    public int ChatComposer(string id, TextBuffer buf, ComposerOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(buf);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_chat_composer_ex(_ctx, __idPointer, Use(buf), (ulong)buf.Capacity, &native_opts);
            SyncState();
            return result;
        }
    }

    public void ChatDivider(string label)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(label);
        using var __labelUtf8 = new ScopedUtf8(label, stackalloc byte[256]);
        fixed (byte* __labelPointer = __labelUtf8.Bytes)
        {
            cui_chat_divider(_ctx, __labelPointer);
            SyncState();
        }
    }

    public void ChatMessage(string text, ChatOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(text);
        var native_opts = ToNative(opts);
        // Markdown headings retain source slices in deferred paint segments.
        if (opts.Markdown)
        {
            cui_chat_message_ex(_ctx, Utf8(text), &native_opts);
            SyncState();
            return;
        }

        using var __textUtf8 = new ScopedUtf8(text, stackalloc byte[256]);
        fixed (byte* __textPointer = __textUtf8.Bytes)
        {
            cui_chat_message_ex(_ctx, __textPointer, &native_opts);
            SyncState();
        }
    }

    public bool Checkbox(string label, ref bool value, CheckOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(label);
        CBool native_value = value;
        var native_opts = ToNative(opts);
        using var __labelUtf8 = new ScopedUtf8(label, stackalloc byte[256]);
        fixed (byte* __labelPointer = __labelUtf8.Bytes)
        {
            var result = cui_checkbox_ex(_ctx, __labelPointer, &native_value, &native_opts);
            value = (native_value != 0);
            SyncState();
            return (result != 0);
        }
    }

    public int Checklist(string id, Span<CheckStep> steps, int count, ChecklistOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckLength(count, steps.Length, nameof(count));
        var native_steps = CopyCheckStep(steps);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_checklist_ex(_ctx, __idPointer, native_steps, count, &native_opts);
            for (var i = 0; i < steps.Length; i++) steps[i] = FromNative(native_steps[i], steps[i]);
            SyncState();
            return result;
        }
    }

    public bool Chip(string label, ref bool selected, ChipOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(label);
        CBool native_selected = selected;
        var native_opts = ToNative(opts);
        using var __labelUtf8 = new ScopedUtf8(label, stackalloc byte[256]);
        fixed (byte* __labelPointer = __labelUtf8.Bytes)
        {
            var result = cui_chip_ex(_ctx, __labelPointer, &native_selected, &native_opts);
            selected = (native_selected != 0);
            SyncState();
            return (result != 0);
        }
    }

    public bool Citation(int number, Source source)
    {
        CheckFrame();
        var native_source = ToNative(source);
        var result = cui_citation(_ctx, number, &native_source);
        SyncState();
        return (result != 0);
    }

    /// <summary>Edits the owned UTF-8 buffer in place, with syntax highlighting,
    /// indentation, undo/redo, find and go-to-line supplied by CUI.</summary>
    public CodeEditorResult CodeEditor(string id, TextBuffer buffer, CodeEditorOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(buffer);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_code_editor_ex(_ctx, __idPointer, Use(buffer), (ulong)buffer.Capacity, &native_opts);
            SyncState();
            return FromNative(result);
        }
    }

    public void CodeBlock(string id, string code, CodeOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(code);
        var native_opts = ToNative(opts);
        using var __codeUtf8 = new ScopedUtf8(code, stackalloc byte[256]);
        fixed (byte* __codePointer = __codeUtf8.Bytes)
        {
            using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
            fixed (byte* __idPointer = __idUtf8.Bytes)
            {
                cui_code_block_ex(_ctx, __idPointer, __codePointer, &native_opts);
                SyncState();
            }
        }
    }

    public Color ColorOf(Color c)
    {
        Check();
        var result = cui_color_of(_ctx, ToNative(c));
        SyncState();
        return FromNative(result);
    }

    public bool ColorPicker(string id, ref Color value)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        var native_value = ToNative(value);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_color_picker(_ctx, __idPointer, &native_value);
            value = FromNative(native_value, value);
            SyncState();
            return (result != 0);
        }
    }

    public UiScope ColumnBegin(Box opts = default)
    {
        CheckFrame();
        CheckBegin();
        var native_opts = ToNative(opts);
        var result = cui_column_begin(_ctx, &native_opts);
        SyncState();
        return TrackScope(ScopeKind.Box, true, unchecked((int)result));
    }

    public int CommandPalette(ref bool open, ReadOnlySpan<Command> commands, int count)
    {
        CheckFrame();
        CheckLength(count, commands.Length, nameof(count));
        CBool native_open = open;
        var native_commands = CopyCommand(commands);
        var result = cui_command_palette(_ctx, &native_open, native_commands, count);
        open = (native_open != 0);
        SyncState();
        return result;
    }

    public int Comment(string id, string author, string text, CommentOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(author);
        ArgumentNullException.ThrowIfNull(text);
        var native_opts = ToNative(opts);
        using var __authorUtf8 = new ScopedUtf8(author, stackalloc byte[256]);
        using var __textUtf8 = new ScopedUtf8(text, stackalloc byte[256]);
        fixed (byte* __authorPointer = __authorUtf8.Bytes)
        fixed (byte* __textPointer = __textUtf8.Bytes)
        {
            using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
            fixed (byte* __idPointer = __idUtf8.Bytes)
            {
                var result = cui_comment_ex(_ctx, __idPointer, __authorPointer, __textPointer, &native_opts);
                SyncState();
                return result;
            }
        }
    }

    public CommitGraphResult CommitGraph(string id, ReadOnlySpan<Commit> commits, int count, CommitGraphOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckLength(count, commits.Length, nameof(count));
        var native_commits = CopyCommit(commits);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_commit_graph_ex(_ctx, __idPointer, native_commits, count, &native_opts);
            SyncState();
            return FromNative(result);
        }
    }

    public bool ConfirmButton(string label, ConfirmOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(label);
        var native_opts = ToNative(opts);
        using var __labelUtf8 = new ScopedUtf8(label, stackalloc byte[256]);
        fixed (byte* __labelPointer = __labelUtf8.Bytes)
        {
            var result = cui_confirm_button_ex(_ctx, __labelPointer, &native_opts);
            SyncState();
            return (result != 0);
        }
    }

    public bool ConfirmPopover(string id, ConfirmPopoverOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_confirm_popover_ex(_ctx, __idPointer, &native_opts);
            SyncState();
            return (result != 0);
        }
    }

    public UiScope ContextMenuBegin(string id)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckBegin();
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_context_menu_begin(_ctx, __idPointer);
            SyncState();
            return TrackScope(ScopeKind.ContextMenu, result != 0, result != 0 ? 1 : 0);
        }
    }

    public void ContextMenuEnd()
    {
        CheckFrame();
        BeforeExplicitEnd(ScopeKind.ContextMenu);
        cui_context_menu_end(_ctx);
        SyncState();
    }

    public void ContextMeter(string id, float used, float limit, ContextOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            cui_context_meter_ex(_ctx, __idPointer, used, limit, &native_opts);
            SyncState();
        }
    }

    public bool ControlTile(string label, ref bool on, TileOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(label);
        CBool native_on = on;
        var native_opts = ToNative(opts);
        using var __labelUtf8 = new ScopedUtf8(label, stackalloc byte[256]);
        fixed (byte* __labelPointer = __labelUtf8.Bytes)
        {
            var result = cui_control_tile_ex(_ctx, __labelPointer, &native_on, &native_opts);
            on = (native_on != 0);
            SyncState();
            return (result != 0);
        }
    }

    public bool CopyButton(string id, string text)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(text);
        using var __textUtf8 = new ScopedUtf8(text, stackalloc byte[256]);
        fixed (byte* __textPointer = __textUtf8.Bytes)
        {
            using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
            fixed (byte* __idPointer = __idUtf8.Bytes)
            {
                var result = cui_copy_button(_ctx, __idPointer, __textPointer);
                SyncState();
                return (result != 0);
            }
        }
    }

    public void Countdown(string id, double seconds_left, CountdownOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            cui_countdown_ex(_ctx, __idPointer, seconds_left, &native_opts);
            SyncState();
        }
    }

    public void Counter(string id, double value, CounterOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            cui_counter_ex(_ctx, __idPointer, value, &native_opts);
            SyncState();
        }
    }

    public CoverageResult Coverage(string id, ReadOnlySpan<CoverageFile> files, int count, CoverageOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckLength(count, files.Length, nameof(count));
        var native_files = CopyCoverageFile(files);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_coverage_ex(_ctx, __idPointer, native_files, count, &native_opts);
            SyncState();
            return FromNative(result);
        }
    }

    public bool Cropper(string id, Texture? tex, int image_w, int image_h, ref Rect crop, CropperOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        var native_crop = ToNative(crop);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_cropper_ex(_ctx, __idPointer, Unwrap(tex), image_w, image_h, &native_crop, &native_opts);
            crop = FromNative(native_crop, crop);
            SyncState();
            return (result != 0);
        }
    }

    public UiScope DataGridBegin(string id, ReadOnlySpan<GridColumn> columns, int column_count, int row_count, ref int first, ref int last, GridOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckBegin();
        ValidateGrid(opts, row_count);
        CheckLength(column_count, columns.Length, nameof(column_count));
        var native_columns = CopyGridColumn(columns);
        var native_first = first;
        var native_last = last;
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_data_grid_begin_ex(_ctx, __idPointer, native_columns, column_count, row_count, &native_first, &native_last, &native_opts);
            first = native_first;
            last = native_last;
            SyncState();
            return TrackScope(ScopeKind.DataGrid, true, result != 0 ? 1 : 0);
        }
    }

    public void DataGridCell()
    {
        CheckFrame();
        cui_data_grid_cell(_ctx);
        SyncState();
    }

    public void DataGridCellText(string text)
    {
        CheckFrame();
        if (_ctx->building.grid == null || _ctx->building.grid->in_row == 0)
            throw new InvalidOperationException("Build a grid row before its text cells.");

        ArgumentNullException.ThrowIfNull(text);
        using var __textUtf8 = new ScopedUtf8(text, stackalloc byte[256]);
        fixed (byte* __textPointer = __textUtf8.Bytes)
        {
            cui_data_grid_cell_text(_ctx, __textPointer);
            SyncState();
        }
    }

    public bool DataGridEdit(TextBuffer buf)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(buf);
        var result = cui_data_grid_edit(_ctx, Use(buf), (ulong)buf.Capacity);
        SyncState();
        return (result != 0);
    }

    public void DataGridEnd()
    {
        CheckFrame();
        BeforeExplicitEnd(ScopeKind.DataGrid);
        cui_data_grid_end(_ctx);
        SyncState();
    }

    public bool DataGridRow(int row)
    {
        CheckFrame();
        if (_ctx->building.grid == null) throw new InvalidOperationException("DataGridRow requires an active grid.");
        if ((uint)row >= (uint)_ctx->building.grid->nrows) throw new ArgumentOutOfRangeException(nameof(row));

        var result = cui_data_grid_row(_ctx, row);
        // Row and checkbox selection change on mouse release. Header/keyboard
        // selection changes were already synchronized by DataGridBegin.
        if (_ctx->mreleased[0] != 0) _arraysDirty = true;
        SyncState();
        return (result != 0);
    }

    public Date DateFromDays(int days)
    {
        Check();
        var result = cui_date_from_days(days);
        SyncState();
        return FromNative(result);
    }

    public bool DatePicker(string id, ref Date value, DateOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        var native_value = ToNative(value);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_date_picker_ex(_ctx, __idPointer, &native_value, &native_opts);
            value = FromNative(native_value, value);
            SyncState();
            return (result != 0);
        }
    }

    public bool DateRangePicker(string id, ref DateRange range, DateRangeOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        var native_range = ToNative(range);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_date_range_picker_ex(_ctx, __idPointer, &native_range, &native_opts);
            range = FromNative(native_range, range);
            SyncState();
            return (result != 0);
        }
    }

    public int DateToDays(Date d)
    {
        Check();
        var result = cui_date_to_days(ToNative(d));
        SyncState();
        return result;
    }

    public Date DateToday()
    {
        Check();
        var result = cui_date_today();
        SyncState();
        return FromNative(result);
    }

    public float DeltaTime()
    {
        Check();
        var result = cui_delta_time(_ctx);
        SyncState();
        return result;
    }

    public UiScope DialogBegin(string title, ref bool open, DialogOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(title);
        CheckBegin();
        CBool native_open = open;
        var native_opts = ToNative(opts);
        using var __titleUtf8 = new ScopedUtf8(title, stackalloc byte[256]);
        fixed (byte* __titlePointer = __titleUtf8.Bytes)
        {
            var result = cui_dialog_begin(_ctx, __titlePointer, &native_open, &native_opts);
            open = (native_open != 0);
            SyncState();
            return TrackScope(ScopeKind.Dialog, result != 0, result != 0 ? 1 : 0);
        }
    }

    public void DialogEnd()
    {
        CheckFrame();
        BeforeExplicitEnd(ScopeKind.Dialog);
        cui_dialog_end(_ctx);
        SyncState();
    }

    public UiScope DialogFooterBegin()
    {
        CheckFrame();
        CheckBegin();
        cui_dialog_footer_begin(_ctx);
        SyncState();
        return TrackScope(ScopeKind.DialogFooter, true, 0);
    }

    public void DialogFooterEnd()
    {
        CheckFrame();
        BeforeExplicitEnd(ScopeKind.DialogFooter);
        cui_dialog_footer_end(_ctx);
        SyncState();
    }

    public UiScope DockBegin(string id, DockOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckBegin();
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            cui_dock_begin(_ctx, __idPointer, &native_opts);
            SyncState();
            return TrackScope(ScopeKind.Dock, true, 0);
        }
    }

    public void DockEnd()
    {
        CheckFrame();
        BeforeExplicitEnd(ScopeKind.Dock);
        cui_dock_end(_ctx);
        SyncState();
    }

    public bool DockItem(string icon, string label, DockItemOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(icon);
        ArgumentNullException.ThrowIfNull(label);
        var native_opts = ToNative(opts);
        using var __labelUtf8 = new ScopedUtf8(label, stackalloc byte[256]);
        fixed (byte* __labelPointer = __labelUtf8.Bytes)
        {
            var result = cui_dock_item_ex(_ctx, Utf8(icon), __labelPointer, &native_opts);
            SyncState();
            return (result != 0);
        }
    }

    public void DockSeparator()
    {
        CheckFrame();
        cui_dock_separator(_ctx);
        SyncState();
    }

    public void DonutChart(string id, ReadOnlySpan<float> values, ReadOnlySpan<string> labels, int count, ChartOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckLength(count, values.Length, nameof(count));
        CheckOptional(count, labels.Length, nameof(labels));
        var native_values = CopyFloat(values);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            cui_donut_chart_ex(_ctx, __idPointer, native_values, Strings(labels), count, &native_opts);
            SyncState();
        }
    }

    public DotChartResult DotChart(string id, ReadOnlySpan<DotSeries> series, int series_count, int count, DotChartOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        ValidateDotChart(series, series_count, count);
        CheckLength(series_count, series.Length, nameof(series_count));
        var native_series = CopyDotSeries(series);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_dot_chart_ex(_ctx, __idPointer, native_series, series_count, count, &native_opts);
            SyncState();
            return FromNative(result);
        }
    }

    public void DotDisplay(string text, DotDisplayOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(text);
        var native_opts = ToNative(opts);
        using var __textUtf8 = new ScopedUtf8(text, stackalloc byte[256]);
        fixed (byte* __textPointer = __textUtf8.Bytes)
        {
            cui_dot_display_ex(_ctx, __textPointer, &native_opts);
            SyncState();
        }
    }

    public float DpiScale()
    {
        CheckFrame();
        var result = cui_dpi_scale(_ctx);
        SyncState();
        return result;
    }

    public void DrawArc(Vec2 center, float radius, float a0, float a1, float thickness, Color color)
    {
        CheckDraw();
        cui_draw_arc(_ctx, ToNative(center), radius, a0, a1, thickness, ToNative(color));
        SyncState();
    }

    public void DrawCircle(Vec2 center, float radius, Color color)
    {
        CheckDraw();
        cui_draw_circle(_ctx, ToNative(center), radius, ToNative(color));
        SyncState();
    }

    public void DrawEmoji(string utf8, int len, Rect r)
    {
        CheckDraw();
        ArgumentNullException.ThrowIfNull(utf8);
        CheckUtf8Length(utf8, len);
        using var __utf8Utf8 = new ScopedUtf8(utf8, stackalloc byte[256], nullTerminated: len < 0);
        fixed (byte* __utf8Pointer = __utf8Utf8.Bytes)
        {
            cui_draw_emoji(_ctx, __utf8Pointer, len, ToNative(r));
            SyncState();
        }
    }

    public void DrawIcon(string icon, Rect r, float size, Color color)
    {
        CheckDraw();
        ArgumentNullException.ThrowIfNull(icon);
        using var __iconUtf8 = new ScopedUtf8(icon, stackalloc byte[256]);
        fixed (byte* __iconPointer = __iconUtf8.Bytes)
        {
            cui_draw_icon(_ctx, __iconPointer, ToNative(r), size, ToNative(color));
            SyncState();
        }
    }

    public void DrawImage(Texture? tex, Rect r, Rect uv, Corners radius, Color tint)
    {
        CheckDraw();
        cui_draw_image(_ctx, Unwrap(tex), ToNative(r), ToNative(uv), ToNative(radius), ToNative(tint));
        SyncState();
    }

    public void DrawLine(Vec2 a, Vec2 b, float thickness, Color color)
    {
        CheckDraw();
        cui_draw_line(_ctx, ToNative(a), ToNative(b), thickness, ToNative(color));
        SyncState();
    }

    public void DrawPolygon(ReadOnlySpan<Vec2> points, int count, Color color)
    {
        CheckDraw();
        CheckLength(count, points.Length, nameof(count));
        var native_points = CopyVec2(points);
        cui_draw_polygon(_ctx, native_points, count, ToNative(color));
        SyncState();
    }

    public void DrawPolyline(ReadOnlySpan<Vec2> points, int count, float thickness, Color color, bool closed)
    {
        CheckDraw();
        CheckLength(count, points.Length, nameof(count));
        var native_points = CopyVec2(points);
        cui_draw_polyline(_ctx, native_points, count, thickness, ToNative(color), closed);
        SyncState();
    }

    public void DrawRect(Rect r, Corners radius, Color fill)
    {
        CheckDraw();
        cui_draw_rect(_ctx, ToNative(r), ToNative(radius), ToNative(fill));
        SyncState();
    }

    public void DrawRectBorder(Rect r, Corners radius, Color fill, Color border, float border_width)
    {
        CheckDraw();
        cui_draw_rect_border(_ctx, ToNative(r), ToNative(radius), ToNative(fill), ToNative(border), border_width);
        SyncState();
    }

    public void DrawRectGradient(Rect r, Corners radius, Color top, Color bottom)
    {
        CheckDraw();
        cui_draw_rect_gradient(_ctx, ToNative(r), ToNative(radius), ToNative(top), ToNative(bottom));
        SyncState();
    }

    public void DrawShadow(Rect r, Corners radius, float blur, float spread, Vec2 offset, Color color)
    {
        CheckDraw();
        cui_draw_shadow(_ctx, ToNative(r), ToNative(radius), blur, spread, ToNative(offset), ToNative(color));
        SyncState();
    }

    public void DrawText(Font font, float size, Vec2 pos, Color color, string text, int len)
    {
        CheckDraw();
        ArgumentNullException.ThrowIfNull(text);
        CheckUtf8Length(text, len);
        using var __textUtf8 = new ScopedUtf8(text, stackalloc byte[256], nullTerminated: len < 0);
        fixed (byte* __textPointer = __textUtf8.Bytes)
        {
            cui_draw_text(_ctx, (int)CheckEnum(font), size, ToNative(pos), ToNative(color), __textPointer, len);
            SyncState();
        }
    }

    public string? DroppedFile(int index)
    {
        CheckFrame();
        var result = cui_dropped_file(_ctx, index);
        SyncState();
        return Decode(result);
    }

    public int Dropzone(string id, DropzoneOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_dropzone_ex(_ctx, __idPointer, &native_opts);
            SyncState();
            return result;
        }
    }

    public void Emoji(string utf8, float size)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(utf8);
        using var __utf8Utf8 = new ScopedUtf8(utf8, stackalloc byte[256]);
        fixed (byte* __utf8Pointer = __utf8Utf8.Bytes)
        {
            cui_emoji(_ctx, __utf8Pointer, size);
            SyncState();
        }
    }

    public bool EmojiFontLoadFile(string path)
    {
        Check();
        ArgumentNullException.ThrowIfNull(path);
        using var __pathUtf8 = new ScopedUtf8(path, stackalloc byte[256]);
        fixed (byte* __pathPointer = __pathUtf8.Bytes)
        {
            var result = cui_emoji_font_load_file(_ctx, __pathPointer);
            SyncState();
            return (result != 0);
        }
    }

    public bool EmojiFontLoadSystem()
    {
        Check();
        var result = cui_emoji_font_load_system(_ctx);
        SyncState();
        return (result != 0);
    }

    public string? EmojiPicker(string id, EmojiPickerOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_emoji_picker_ex(_ctx, __idPointer, &native_opts);
            SyncState();
            return Decode(result);
        }
    }

    public bool EmptyState(string title, EmptyOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(title);
        var native_opts = ToNative(opts);
        using var __titleUtf8 = new ScopedUtf8(title, stackalloc byte[256]);
        fixed (byte* __titlePointer = __titleUtf8.Bytes)
        {
            var result = cui_empty_state_ex(_ctx, __titlePointer, &native_opts);
            SyncState();
            return (result != 0);
        }
    }

    public EventTrackResult EventTrack(string id, Span<double> times, ref int count, int capacity, double min, double max, EventTrackOpts opts = default)
    {
        CheckFrame();
        CheckRange(min, max);
        ArgumentNullException.ThrowIfNull(id);
        CheckLength(count, capacity, nameof(count));
        CheckLength(capacity, times.Length, nameof(capacity));
        var native_times = CopyDouble(times);
        var native_count = count;
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_event_track_ex(_ctx, __idPointer, native_times, &native_count, capacity, min, max, &native_opts);
            for (var i = 0; i < times.Length; i++) times[i] = native_times[i];
            count = native_count;
            SyncState();
            return FromNative(result);
        }
    }

    public UiScope ExpandableCardBegin(string title, ref bool open, ExpandableCardOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(title);
        CheckBegin();
        CBool native_open = open;
        var native_opts = ToNative(opts);
        using var __titleUtf8 = new ScopedUtf8(title, stackalloc byte[256]);
        fixed (byte* __titlePointer = __titleUtf8.Bytes)
        {
            var result = cui_expandable_card_begin(_ctx, __titlePointer, &native_open, &native_opts);
            open = (native_open != 0);
            SyncState();
            return TrackScope(ScopeKind.ExpandableCard, result != 0, result != 0 ? 1 : 0);
        }
    }

    public void ExpandableCardEnd()
    {
        CheckFrame();
        BeforeExplicitEnd(ScopeKind.ExpandableCard);
        cui_expandable_card_end(_ctx);
        SyncState();
    }

    public bool ExpandableText(string id, string text, ExpandableTextOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(text);
        var native_opts = ToNative(opts);
        using var __textUtf8 = new ScopedUtf8(text, stackalloc byte[256]);
        fixed (byte* __textPointer = __textUtf8.Bytes)
        {
            using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
            fixed (byte* __idPointer = __idUtf8.Bytes)
            {
                var result = cui_expandable_text_ex(_ctx, __idPointer, __textPointer, &native_opts);
                SyncState();
                return (result != 0);
            }
        }
    }

    public bool Fader(string id, ref float value, float min, float max, FaderOpts opts = default)
    {
        CheckFrame();
        CheckRange(min, max);
        ArgumentNullException.ThrowIfNull(id);
        var native_value = value;
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_fader_ex(_ctx, __idPointer, &native_value, min, max, &native_opts);
            value = native_value;
            SyncState();
            return (result != 0);
        }
    }

    public FeatureFlagsResult FeatureFlags(string id, Span<FeatureFlag> flags, int count, ReadOnlySpan<string> envs, int env_count, FeatureFlagsOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        if ((uint)env_count > 8) throw new ArgumentOutOfRangeException(nameof(env_count));

        CheckLength(env_count, envs.Length, nameof(env_count));
        CheckLength(count, flags.Length, nameof(count));
        var native_flags = CopyFeatureFlag(flags);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_feature_flags_ex(_ctx, __idPointer, native_flags, count, Strings(envs), env_count, &native_opts);
            for (var i = 0; i < flags.Length; i++) flags[i] = FromNative(native_flags[i], flags[i]);
            SyncState();
            return FromNative(result);
        }
    }

    public FileBrowserResult FileBrowser(string id, TextBuffer path, FileBrowserOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(path);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_file_browser_ex(_ctx, __idPointer, Use(path), (ulong)path.Capacity, &native_opts);
            SyncState();
            return FromNative(result);
        }
    }

    public bool FilePicker(string id, ref bool open, TextBuffer @out, FilePickerOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CBool native_open = open;
        ArgumentNullException.ThrowIfNull(@out);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_file_picker_ex(_ctx, __idPointer, &native_open, Use(@out), (ulong)@out.Capacity, &native_opts);
            open = (native_open != 0);
            SyncState();
            return (result != 0);
        }
    }

    public bool FilterBar(string id, ReadOnlySpan<FilterField> fields, int count, Span<uint> selected, TextBuffer search)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckLength(count, selected.Length, nameof(selected));
        CheckLength(count, fields.Length, nameof(count));
        var native_fields = CopyFilterField(fields);
        var native_selected = CopyUint32T(selected);
        ArgumentNullException.ThrowIfNull(search);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_filter_bar(_ctx, __idPointer, native_fields, count, native_selected, Use(search), (ulong)search.Capacity);
            for (var i = 0; i < selected.Length; i++) selected[i] = native_selected[i];
            SyncState();
            return (result != 0);
        }
    }

    public int Funnel(string id, ReadOnlySpan<VizItem> stages, int count, FunnelOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckLength(count, stages.Length, nameof(count));
        var native_stages = CopyVizItem(stages);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_funnel_ex(_ctx, __idPointer, native_stages, count, &native_opts);
            SyncState();
            return result;
        }
    }

    public int Gantt(string id, Span<GanttTask> tasks, int count, Date origin, int days, GanttOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        if (days < 0) throw new ArgumentOutOfRangeException(nameof(days));

        CheckLength(count, tasks.Length, nameof(count));
        var native_tasks = CopyGanttTask(tasks);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_gantt_ex(_ctx, __idPointer, native_tasks, count, ToNative(origin), days, &native_opts);
            for (var i = 0; i < tasks.Length; i++) tasks[i] = FromNative(native_tasks[i], tasks[i]);
            SyncState();
            return result;
        }
    }

    public void Gap(float size)
    {
        CheckFrame();
        cui_gap(_ctx, size);
        SyncState();
    }

    public void Gauge(string label, float value, float size)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(label);
        using var __labelUtf8 = new ScopedUtf8(label, stackalloc byte[256]);
        fixed (byte* __labelPointer = __labelUtf8.Bytes)
        {
            cui_gauge(_ctx, __labelPointer, value, size);
            SyncState();
        }
    }

    public Accent GetAccent()
    {
        Check();
        var result = cui_get_accent(_ctx);
        SyncState();
        return (Accent)result;
    }

    public Rect GetRect(Id id)
    {
        Check();
        var result = cui_get_rect(_ctx, Unwrap(id));
        SyncState();
        return FromNative(result);
    }

    public ThemeMode GetThemeMode()
    {
        Check();
        var result = cui_get_theme_mode(_ctx);
        SyncState();
        return (ThemeMode)result;
    }

    public void GradientText(string text, GradientTextOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(text);
        var native_opts = ToNative(opts);
        using var __textUtf8 = new ScopedUtf8(text, stackalloc byte[256]);
        fixed (byte* __textPointer = __textUtf8.Bytes)
        {
            cui_gradient_text_ex(_ctx, __textPointer, &native_opts);
            SyncState();
        }
    }

    public GraphResult GraphView(string id, ReadOnlySpan<GraphNode> nodes, int node_count, ReadOnlySpan<GraphEdge> edges, int edge_count, GraphOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        ValidateEdges(edges, edge_count, node_count);
        CheckLength(edge_count, edges.Length, nameof(edge_count));
        CheckLength(node_count, nodes.Length, nameof(node_count));
        var native_nodes = CopyGraphNode(nodes);
        var native_edges = CopyGraphEdge(edges);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_graph_view_ex(_ctx, __idPointer, native_nodes, node_count, native_edges, edge_count, &native_opts);
            SyncState();
            return FromNative(result);
        }
    }

    public void Heading(string text, int level)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(text);
        using var __textUtf8 = new ScopedUtf8(text, stackalloc byte[256]);
        fixed (byte* __textPointer = __textUtf8.Bytes)
        {
            cui_heading(_ctx, __textPointer, level);
            SyncState();
        }
    }

    public int Heatmap(string id, ReadOnlySpan<float> values, int rows, int cols, HeatmapOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        ValidateHeatmap(opts, rows, cols);
        CheckLength(checked(rows * cols), values.Length, nameof(values));
        var native_values = CopyFloat(values);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_heatmap_ex(_ctx, __idPointer, native_values, rows, cols, &native_opts);
            SyncState();
            return result;
        }
    }

    public bool HistogramSlider(string label, ReadOnlySpan<float> bins, int bin_count, ref float lo, ref float hi, float min, float max, HistogramOpts opts = default)
    {
        CheckFrame();
        CheckRange(min, max);
        ArgumentNullException.ThrowIfNull(label);
        CheckLength(bin_count, bins.Length, nameof(bin_count));
        var native_bins = CopyFloat(bins);
        var native_lo = lo;
        var native_hi = hi;
        var native_opts = ToNative(opts);
        using var __labelUtf8 = new ScopedUtf8(label, stackalloc byte[256]);
        fixed (byte* __labelPointer = __labelUtf8.Bytes)
        {
            var result = cui_histogram_slider_ex(_ctx, __labelPointer, native_bins, bin_count, &native_lo, &native_hi, min, max, &native_opts);
            lo = native_lo;
            hi = native_hi;
            SyncState();
            return (result != 0);
        }
    }

    public bool HoldButton(string label, HoldOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(label);
        var native_opts = ToNative(opts);
        using var __labelUtf8 = new ScopedUtf8(label, stackalloc byte[256]);
        fixed (byte* __labelPointer = __labelUtf8.Bytes)
        {
            var result = cui_hold_button_ex(_ctx, __labelPointer, &native_opts);
            SyncState();
            return (result != 0);
        }
    }

    public UiScope HoverCardBegin(string id, HoverCardOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckBegin();
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_hover_card_begin(_ctx, __idPointer, &native_opts);
            SyncState();
            return TrackScope(ScopeKind.HoverCard, result != 0, result != 0 ? 1 : 0);
        }
    }

    public void HoverCardEnd()
    {
        CheckFrame();
        BeforeExplicitEnd(ScopeKind.HoverCard);
        cui_hover_card_end(_ctx);
        SyncState();
    }

    public void Icon(string icon, float size, Color color)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(icon);
        using var __iconUtf8 = new ScopedUtf8(icon, stackalloc byte[256]);
        fixed (byte* __iconPointer = __iconUtf8.Bytes)
        {
            cui_icon(_ctx, __iconPointer, size, ToNative(color));
            SyncState();
        }
    }

    public bool IconButton(string icon, ButtonOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(icon);
        var native_opts = ToNative(opts);
        using var __iconUtf8 = new ScopedUtf8(icon, stackalloc byte[256]);
        fixed (byte* __iconPointer = __iconUtf8.Bytes)
        {
            var result = cui_icon_button_ex(_ctx, __iconPointer, &native_opts);
            SyncState();
            return (result != 0);
        }
    }

    public bool ImageCompare(string id, Texture? before, Texture? after, ref float pos, CompareOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        var native_pos = pos;
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_image_compare_ex(_ctx, __idPointer, Unwrap(before), Unwrap(after), &native_pos, &native_opts);
            pos = native_pos;
            SyncState();
            return (result != 0);
        }
    }

    public void Image(Texture? tex, ImageOpts opts = default)
    {
        CheckFrame();
        var native_opts = ToNative(opts);
        cui_image_ex(_ctx, Unwrap(tex), &native_opts);
        SyncState();
    }

    public bool ImageViewer(string id, Texture? tex, int image_w, int image_h, ref ImageView view, ImageViewerOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        var native_view = ToNative(view);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_image_viewer_ex(_ctx, __idPointer, Unwrap(tex), image_w, image_h, &native_view, &native_opts);
            view = FromNative(native_view, view);
            SyncState();
            return (result != 0);
        }
    }

    public int InlineEdit(string id, TextBuffer buf, InlineEditOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(buf);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_inline_edit_ex(_ctx, __idPointer, Use(buf), (ulong)buf.Capacity, &native_opts);
            SyncState();
            return result;
        }
    }

    public int Input(string id, TextBuffer buf, InputOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(buf);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_input_ex(_ctx, __idPointer, Use(buf), (ulong)buf.Capacity, &native_opts);
            SyncState();
            return result;
        }
    }

    public void InputFileDrop(ReadOnlySpan<string> paths, int count, float x, float y)
    {
        Check();
        CheckLength(count, paths.Length, nameof(count));
        cui_input_file_drop(_ctx, Strings(paths), count, x, y);
        SyncState();
    }

    public void InputFileHover(bool hovering, float x, float y)
    {
        Check();
        cui_input_file_hover(_ctx, hovering, x, y);
        SyncState();
    }

    public void InputFocus(bool window_focused)
    {
        Check();
        cui_input_focus(_ctx, window_focused);
        SyncState();
    }

    public void InputKey(Key key, bool down, int mods)
    {
        Check();
        cui_input_key(_ctx, (cui_key)CheckEnum(key), down, mods);
        SyncState();
    }

    public void InputMouseButton(MouseButton button, bool down)
    {
        Check();
        cui_input_mouse_button(_ctx, (cui_mouse_button)CheckEnum(button), down);
        SyncState();
    }

    public void InputMouseLeave()
    {
        Check();
        cui_input_mouse_leave(_ctx);
        SyncState();
    }

    public void InputMouseMove(float x, float y)
    {
        Check();
        cui_input_mouse_move(_ctx, x, y);
        SyncState();
    }

    public void InputScroll(float dx, float dy)
    {
        Check();
        cui_input_scroll(_ctx, dx, dy);
        SyncState();
    }

    public void InputText(string utf8)
    {
        Check();
        ArgumentNullException.ThrowIfNull(utf8);
        using var __utf8Utf8 = new ScopedUtf8(utf8, stackalloc byte[256]);
        fixed (byte* __utf8Pointer = __utf8Utf8.Bytes)
        {
            cui_input_text(_ctx, __utf8Pointer);
            SyncState();
        }
    }

    public bool JsonView(string id, string json, JsonOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(json);
        var native_opts = ToNative(opts);
        using var __jsonUtf8 = new ScopedUtf8(json, stackalloc byte[256]);
        fixed (byte* __jsonPointer = __jsonUtf8.Bytes)
        {
            using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
            fixed (byte* __idPointer = __idUtf8.Bytes)
            {
                var result = cui_json_view_ex(_ctx, __idPointer, __jsonPointer, &native_opts);
                SyncState();
                return (result != 0);
            }
        }
    }

    public UiScope KanbanBegin(string id)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckBegin();
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            cui_kanban_begin(_ctx, __idPointer);
            SyncState();
            return TrackScope(ScopeKind.Kanban, true, 0);
        }
    }

    public UiScope KanbanCardBegin(int card_id)
    {
        CheckFrame();
        CheckBegin();
        RequireScope(ScopeKind.KanbanColumn);
        var result = cui_kanban_card_begin(_ctx, card_id);
        SyncState();
        return TrackScope(ScopeKind.KanbanCard, true, result != 0 ? 1 : 0);
    }

    public void KanbanCardEnd()
    {
        CheckFrame();
        BeforeExplicitEnd(ScopeKind.KanbanCard);
        cui_kanban_card_end(_ctx);
        SyncState();
    }

    public UiScope KanbanColumnBegin(string title, int card_count, KanbanColumnOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(title);
        CheckBegin();
        RequireScope(ScopeKind.Kanban);
        var native_opts = ToNative(opts);
        using var __titleUtf8 = new ScopedUtf8(title, stackalloc byte[256]);
        fixed (byte* __titlePointer = __titleUtf8.Bytes)
        {
            var result = cui_kanban_column_begin_ex(_ctx, __titlePointer, card_count, &native_opts);
            SyncState();
            return TrackScope(ScopeKind.KanbanColumn, true, result != 0 ? 1 : 0);
        }
    }

    public void KanbanColumnEnd()
    {
        CheckFrame();
        BeforeExplicitEnd(ScopeKind.KanbanColumn);
        cui_kanban_column_end(_ctx);
        SyncState();
    }

    public bool KanbanEnd(ref KanbanMove move)
    {
        CheckFrame();
        BeforeExplicitEnd(ScopeKind.Kanban);
        var native_move = ToNative(move);
        var result = cui_kanban_end(_ctx, &native_move);
        move = FromNative(native_move, move);
        SyncState();
        return (result != 0);
    }

    public bool KeyDown(Key key)
    {
        Check();
        var result = cui_key_down(_ctx, (cui_key)CheckEnum(key));
        SyncState();
        return (result != 0);
    }

    public string? KeyName(Key key)
    {
        Check();
        var result = cui_key_name((cui_key)CheckEnum(key));
        SyncState();
        return Decode(result);
    }

    public bool KeyPressed(Key key)
    {
        Check();
        var result = cui_key_pressed(_ctx, (cui_key)CheckEnum(key));
        SyncState();
        return (result != 0);
    }

    public KeymapResult Keymap(string id, ReadOnlySpan<KeymapBinding> bindings, int count, KeymapOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckLength(count, bindings.Length, nameof(count));
        var native_bindings = CopyKeymapBinding(bindings);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_keymap_ex(_ctx, __idPointer, native_bindings, count, &native_opts);
            SyncState();
            return FromNative(result);
        }
    }

    public bool Knob(string id, ref float value, float min, float max, KnobOpts opts = default)
    {
        CheckFrame();
        CheckRange(min, max);
        ArgumentNullException.ThrowIfNull(id);
        var native_value = value;
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_knob_ex(_ctx, __idPointer, &native_value, min, max, &native_opts);
            value = native_value;
            SyncState();
            return (result != 0);
        }
    }

    public bool KvEditor(string id, Span<KvPair> pairs, ref int count, int capacity, KvEditorOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckLength(count, capacity, nameof(count));
        CheckLength(capacity, pairs.Length, nameof(capacity));
        var native_pairs = CopyKvPair(pairs);
        var native_count = count;
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_kv_editor_ex(_ctx, __idPointer, native_pairs, &native_count, capacity, &native_opts);
            for (var i = 0; i < pairs.Length; i++) pairs[i] = FromNative(native_pairs[i], pairs[i]);
            count = native_count;
            SyncState();
            return (result != 0);
        }
    }

    public void Label(string text)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(text);
        using var __textUtf8 = new ScopedUtf8(text, stackalloc byte[256]);
        fixed (byte* __textPointer = __textUtf8.Bytes)
        {
            cui_label(_ctx, __textPointer);
            SyncState();
        }
    }

    public bool LastHovered()
    {
        Check();
        var result = cui_last_hovered(_ctx);
        SyncState();
        return (result != 0);
    }

    public Id LastId()
    {
        Check();
        var result = cui_last_id(_ctx);
        SyncState();
        return new Id(this, result);
    }

    public Rect LastRect()
    {
        Check();
        var result = cui_last_rect(_ctx);
        SyncState();
        return FromNative(result);
    }

    public bool LevelMeter(string id, ReadOnlySpan<float> levels, int channels, LevelMeterOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckLength(channels, levels.Length, nameof(channels));
        var native_levels = CopyFloat(levels);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_level_meter_ex(_ctx, __idPointer, native_levels, channels, &native_opts);
            SyncState();
            return (result != 0);
        }
    }

    public void LineChart(string id, ReadOnlySpan<Series> series, int count, ChartOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckLength(count, series.Length, nameof(count));
        var native_series = CopySeries(series);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            cui_line_chart_ex(_ctx, __idPointer, native_series, count, &native_opts);
            SyncState();
        }
    }

    public bool Link(string text)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(text);
        using var __textUtf8 = new ScopedUtf8(text, stackalloc byte[256]);
        fixed (byte* __textPointer = __textUtf8.Bytes)
        {
            var result = cui_link(_ctx, __textPointer);
            SyncState();
            return (result != 0);
        }
    }

    public bool ListItem(string title, ListItemOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(title);
        var native_opts = ToNative(opts);
        using var __titleUtf8 = new ScopedUtf8(title, stackalloc byte[256]);
        fixed (byte* __titlePointer = __titleUtf8.Bytes)
        {
            var result = cui_list_item_ex(_ctx, __titlePointer, &native_opts);
            SyncState();
            return (result != 0);
        }
    }

    public void Loader(LoaderOpts opts = default)
    {
        CheckFrame();
        var native_opts = ToNative(opts);
        cui_loader_ex(_ctx, &native_opts);
        SyncState();
    }

    public Id MakeId(string str)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(str);
        using var __strUtf8 = new ScopedUtf8(str, stackalloc byte[256]);
        fixed (byte* __strPointer = __strUtf8.Bytes)
        {
            var result = cui_make_id(_ctx, __strPointer);
            SyncState();
            return new Id(this, result);
        }
    }

    public UiScope MenuBegin(string label, MenuOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(label);
        CheckBegin();
        var native_opts = ToNative(opts);
        using var __labelUtf8 = new ScopedUtf8(label, stackalloc byte[256]);
        fixed (byte* __labelPointer = __labelUtf8.Bytes)
        {
            var result = cui_menu_begin(_ctx, __labelPointer, &native_opts);
            SyncState();
            return TrackScope(ScopeKind.Menu, result != 0, result != 0 ? 1 : 0);
        }
    }

    public void MenuEnd()
    {
        CheckFrame();
        BeforeExplicitEnd(ScopeKind.Menu);
        cui_menu_end(_ctx);
        SyncState();
    }

    public bool MenuItem(string label, MenuItemOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(label);
        var native_opts = ToNative(opts);
        using var __labelUtf8 = new ScopedUtf8(label, stackalloc byte[256]);
        fixed (byte* __labelPointer = __labelUtf8.Bytes)
        {
            var result = cui_menu_item_ex(_ctx, __labelPointer, &native_opts);
            SyncState();
            return (result != 0);
        }
    }

    public void MenuLabel(string text)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(text);
        using var __textUtf8 = new ScopedUtf8(text, stackalloc byte[256]);
        fixed (byte* __textPointer = __textUtf8.Bytes)
        {
            cui_menu_label(_ctx, __textPointer);
            SyncState();
        }
    }

    public void MenuSeparator()
    {
        CheckFrame();
        cui_menu_separator(_ctx);
        SyncState();
    }

    public void Meter(string label, float used, float total, string unit)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(label);
        ArgumentNullException.ThrowIfNull(unit);
        using var __labelUtf8 = new ScopedUtf8(label, stackalloc byte[256]);
        using var __unitUtf8 = new ScopedUtf8(unit, stackalloc byte[256]);
        fixed (byte* __labelPointer = __labelUtf8.Bytes)
        fixed (byte* __unitPointer = __unitUtf8.Bytes)
        {
            cui_meter(_ctx, __labelPointer, used, total, __unitPointer);
            SyncState();
        }
    }

    public void Metric(string label, string value, MetricOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(label);
        ArgumentNullException.ThrowIfNull(value);
        var native_opts = ToNative(opts);
        using var __labelUtf8 = new ScopedUtf8(label, stackalloc byte[256]);
        using var __valueUtf8 = new ScopedUtf8(value, stackalloc byte[256]);
        fixed (byte* __labelPointer = __labelUtf8.Bytes)
        fixed (byte* __valuePointer = __valueUtf8.Bytes)
        {
            cui_metric_ex(_ctx, __labelPointer, __valuePointer, &native_opts);
            SyncState();
        }
    }

    public bool MoneyInput(string id, ref double value, MoneyOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        var native_value = value;
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_money_input_ex(_ctx, __idPointer, &native_value, &native_opts);
            value = native_value;
            SyncState();
            return (result != 0);
        }
    }

    public MonthResult MonthCalendar(string id, ref Date month, ReadOnlySpan<MonthEvent> events, int count, MonthOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckLength(count, events.Length, nameof(count));
        var native_month = ToNative(month);
        var native_events = CopyMonthEvent(events);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_month_calendar_ex(_ctx, __idPointer, &native_month, native_events, count, &native_opts);
            month = FromNative(native_month, month);
            SyncState();
            return FromNative(result);
        }
    }

    public bool MouseClicked(MouseButton b)
    {
        Check();
        var result = cui_mouse_clicked(_ctx, (cui_mouse_button)CheckEnum(b));
        SyncState();
        return (result != 0);
    }

    public bool MouseDown(MouseButton b)
    {
        Check();
        var result = cui_mouse_down(_ctx, (cui_mouse_button)CheckEnum(b));
        SyncState();
        return (result != 0);
    }

    public Vec2 MousePos()
    {
        Check();
        var result = cui_mouse_pos(_ctx);
        SyncState();
        return FromNative(result);
    }

    public bool MultiSelect(string id, ReadOnlySpan<string> items, int count, Span<bool> selected, MultiSelectOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckOptional(count, opts.Icons.Length, nameof(opts.Icons));
        CheckLength(count, selected.Length, nameof(selected));
        CheckLength(count, items.Length, nameof(count));
        var native_selected = CopyBool(selected);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_multi_select_ex(_ctx, __idPointer, Strings(items), count, native_selected, &native_opts);
            for (var i = 0; i < selected.Length; i++) selected[i] = (native_selected[i] != 0);
            SyncState();
            return (result != 0);
        }
    }

    public void Muted(string text)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(text);
        using var __textUtf8 = new ScopedUtf8(text, stackalloc byte[256]);
        fixed (byte* __textPointer = __textUtf8.Bytes)
        {
            cui_muted(_ctx, __textPointer);
            SyncState();
        }
    }

    public bool NavItem(string label, NavOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(label);
        var native_opts = ToNative(opts);
        using var __labelUtf8 = new ScopedUtf8(label, stackalloc byte[256]);
        fixed (byte* __labelPointer = __labelUtf8.Bytes)
        {
            var result = cui_nav_item_ex(_ctx, __labelPointer, &native_opts);
            SyncState();
            return (result != 0);
        }
    }

    public void NavSection(string title)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(title);
        using var __titleUtf8 = new ScopedUtf8(title, stackalloc byte[256]);
        fixed (byte* __titlePointer = __titleUtf8.Bytes)
        {
            cui_nav_section(_ctx, __titlePointer);
            SyncState();
        }
    }

    public bool NeedsRedraw()
    {
        Check();
        var result = cui_needs_redraw(_ctx);
        SyncState();
        return (result != 0);
    }

    public float NextWakeup()
    {
        Check();
        var result = cui_next_wakeup(_ctx);
        SyncState();
        return result;
    }

    public UiScope NodeBegin(int node_id, ref float x, ref float y, string title, NodeOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(title);
        CheckBegin();
        RequireScope(ScopeKind.NodeEditor);
        var native_x = x;
        var native_y = y;
        var native_opts = ToNative(opts);
        var result = cui_node_begin_ex(_ctx, node_id, &native_x, &native_y, Utf8(title), &native_opts);
        x = native_x;
        y = native_y;
        SyncState();
        return TrackScope(ScopeKind.Node, true, result != 0 ? 1 : 0);
    }

    public UiScope NodeEditorBegin(string id, State<NodeView> view, NodeEditorOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckBegin();
        ArgumentNullException.ThrowIfNull(view);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_node_editor_begin_ex(_ctx, __idPointer, BindNodeView(view), &native_opts);
            SyncState();
            return TrackScope(ScopeKind.NodeEditor, true, result != 0 ? 1 : 0);
        }
    }

    public void NodeEditorEnd()
    {
        CheckFrame();
        BeforeExplicitEnd(ScopeKind.NodeEditor);
        cui_node_editor_end(_ctx);
        SyncState();
    }

    public void NodeEnd()
    {
        CheckFrame();
        BeforeExplicitEnd(ScopeKind.Node);
        cui_node_end(_ctx);
        SyncState();
    }

    public int NowPlaying(string id, ref NowPlaying np, NowPlayingOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        var native_np = ToNative(np);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_now_playing_ex(_ctx, __idPointer, &native_np, &native_opts);
            np = FromNative(native_np, np);
            SyncState();
            return result;
        }
    }

    public bool Number(string id, ref double value, double min, double max, NumberOpts opts = default)
    {
        CheckFrame();
        CheckRange(min, max);
        ArgumentNullException.ThrowIfNull(id);
        var native_value = value;
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_number_ex(_ctx, __idPointer, &native_value, min, max, &native_opts);
            value = native_value;
            SyncState();
            return (result != 0);
        }
    }

    public bool OtpInput(string id, TextBuffer code, int length)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(code);
        if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));

        CheckLength(checked(length + 1), code.Capacity, nameof(length));
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_otp_input(_ctx, __idPointer, Use(code), length);
            SyncState();
            return (result != 0);
        }
    }

    public bool Pagination(string id, ref int page, int page_count)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        var native_page = page;
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_pagination(_ctx, __idPointer, &native_page, page_count);
            page = native_page;
            SyncState();
            return (result != 0);
        }
    }

    public int PasswordStrength(string password)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(password);
        using var __passwordUtf8 = new ScopedUtf8(password, stackalloc byte[256]);
        fixed (byte* __passwordPointer = __passwordUtf8.Bytes)
        {
            var result = cui_password_strength(_ctx, __passwordPointer);
            SyncState();
            return result;
        }
    }

    public PipelineResult Pipeline(string id, ReadOnlySpan<string> stages, int stage_count, ReadOnlySpan<PipelineJob> jobs, int job_count, PipelineOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckLength(job_count, jobs.Length, nameof(job_count));
        CheckLength(stage_count, stages.Length, nameof(stage_count));
        var native_jobs = CopyPipelineJob(jobs);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_pipeline_ex(_ctx, __idPointer, Strings(stages), stage_count, native_jobs, job_count, &native_opts);
            SyncState();
            return FromNative(result);
        }
    }

    public void PopDisabled()
    {
        CheckFrame();
        BeforeExplicitEnd(ScopeKind.Disabled);
        cui_pop_disabled(_ctx);
        SyncState();
    }

    public void PopId()
    {
        CheckFrame();
        BeforeExplicitEnd(ScopeKind.Id);
        cui_pop_id(_ctx);
        SyncState();
    }

    public UiScope PopoverBegin(string id, PopoverOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckBegin();
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_popover_begin(_ctx, __idPointer, &native_opts);
            SyncState();
            return TrackScope(ScopeKind.Popover, result != 0, result != 0 ? 1 : 0);
        }
    }

    public void PopoverClose(string id)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            cui_popover_close(_ctx, __idPointer);
            SyncState();
        }
    }

    public void PopoverEnd()
    {
        CheckFrame();
        BeforeExplicitEnd(ScopeKind.Popover);
        cui_popover_end(_ctx);
        SyncState();
    }

    public bool PopoverIsOpen(string id)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_popover_is_open(_ctx, __idPointer);
            SyncState();
            return (result != 0);
        }
    }

    public void PopoverOpen(string id)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            cui_popover_open(_ctx, __idPointer);
            SyncState();
        }
    }

    public void PopoverToggle(string id)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            cui_popover_toggle(_ctx, __idPointer);
            SyncState();
        }
    }

    public void Presence(string id, ReadOnlySpan<PresenceUser> users, int count, int max_visible, float size)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckLength(count, users.Length, nameof(count));
        var native_users = CopyPresenceUser(users);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            cui_presence(_ctx, __idPointer, native_users, count, max_visible, size);
            SyncState();
        }
    }

    public ProblemsResult Problems(string id, ReadOnlySpan<Diagnostic> diags, int count, ProblemsOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckLength(count, diags.Length, nameof(count));
        var native_diags = CopyDiagnostic(diags);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_problems_ex(_ctx, __idPointer, native_diags, count, &native_opts);
            SyncState();
            return FromNative(result);
        }
    }

    public void Progress(float value, ProgressOpts opts = default)
    {
        CheckFrame();
        var native_opts = ToNative(opts);
        cui_progress_ex(_ctx, value, &native_opts);
        SyncState();
    }

    public void ProgressRing(string label, float value, float size)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(label);
        using var __labelUtf8 = new ScopedUtf8(label, stackalloc byte[256]);
        fixed (byte* __labelPointer = __labelUtf8.Bytes)
        {
            cui_progress_ring(_ctx, __labelPointer, value, size);
            SyncState();
        }
    }

    public int PromptSuggestions(string id, ReadOnlySpan<PromptSuggestion> items, int count, PromptSuggestionsOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckLength(count, items.Length, nameof(count));
        var native_items = CopyPromptSuggestion(items);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_prompt_suggestions_ex(_ctx, __idPointer, native_items, count, &native_opts);
            SyncState();
            return result;
        }
    }

    public UiScope PushDisabled(bool disabled)
    {
        CheckFrame();
        CheckBegin();
        cui_push_disabled(_ctx, disabled);
        SyncState();
        return TrackScope(ScopeKind.Disabled, true, 0);
    }

    public UiScope PushId(string str)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(str);
        CheckBegin();
        using var __strUtf8 = new ScopedUtf8(str, stackalloc byte[256]);
        fixed (byte* __strPointer = __strUtf8.Bytes)
        {
            cui_push_id(_ctx, __strPointer);
            SyncState();
            return TrackScope(ScopeKind.Id, true, 0);
        }
    }

    public UiScope PushIdInt(int value)
    {
        CheckFrame();
        CheckBegin();
        cui_push_id_int(_ctx, value);
        SyncState();
        return TrackScope(ScopeKind.Id, true, 0);
    }

    public bool QueryBuilder(string id, ReadOnlySpan<QueryField> fields, int field_count, Span<QueryNode> nodes, ref int count, int capacity, QueryOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckLength(count, capacity, nameof(count));
        CheckLength(capacity, nodes.Length, nameof(capacity));
        CheckLength(field_count, fields.Length, nameof(field_count));
        ValidateQueryNodes(nodes[..count], field_count);
        var native_fields = CopyQueryField(fields);
        var native_nodes = CopyQueryNode(nodes);
        var native_count = count;
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_query_builder_ex(_ctx, __idPointer, native_fields, field_count, native_nodes, &native_count, capacity, &native_opts);
            for (var i = 0; i < nodes.Length; i++) nodes[i] = FromNative(native_nodes[i], nodes[i]);
            count = native_count;
            SyncState();
            return (result != 0);
        }
    }

    public int RadarChart(string id, ReadOnlySpan<Series> series, int count, ReadOnlySpan<string> axes, int axis_count, RadarOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckLength(axis_count, axes.Length, nameof(axis_count));
        CheckLength(count, series.Length, nameof(count));
        var native_series = CopySeries(series);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_radar_chart_ex(_ctx, __idPointer, native_series, count, Strings(axes), axis_count, &native_opts);
            SyncState();
            return result;
        }
    }

    public void RadialGauge(string id, float value, float min, float max, RadialGaugeOpts opts = default)
    {
        CheckFrame();
        CheckRange(min, max);
        ArgumentNullException.ThrowIfNull(id);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            cui_radial_gauge_ex(_ctx, __idPointer, value, min, max, &native_opts);
            SyncState();
        }
    }

    public bool RadioCards(string id, ReadOnlySpan<Choice> choices, int count, ref int index, RadioCardsOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckLength(count, choices.Length, nameof(count));
        var native_choices = CopyChoice(choices);
        var native_index = index;
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_radio_cards_ex(_ctx, __idPointer, native_choices, count, &native_index, &native_opts);
            index = native_index;
            SyncState();
            return (result != 0);
        }
    }

    public bool Radio(string label, ref int value, int option, CheckOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(label);
        var native_value = value;
        var native_opts = ToNative(opts);
        using var __labelUtf8 = new ScopedUtf8(label, stackalloc byte[256]);
        fixed (byte* __labelPointer = __labelUtf8.Bytes)
        {
            var result = cui_radio_ex(_ctx, __labelPointer, &native_value, option, &native_opts);
            value = native_value;
            SyncState();
            return (result != 0);
        }
    }

    public bool RangeCalendar(string id, ref DateRange range, int months)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        var native_range = ToNative(range);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_range_calendar(_ctx, __idPointer, &native_range, months);
            range = FromNative(native_range, range);
            SyncState();
            return (result != 0);
        }
    }

    public bool RangeSlider(string label, ref float lo, ref float hi, float min, float max)
    {
        CheckFrame();
        CheckRange(min, max);
        ArgumentNullException.ThrowIfNull(label);
        var native_lo = lo;
        var native_hi = hi;
        using var __labelUtf8 = new ScopedUtf8(label, stackalloc byte[256]);
        fixed (byte* __labelPointer = __labelUtf8.Bytes)
        {
            var result = cui_range_slider(_ctx, __labelPointer, &native_lo, &native_hi, min, max);
            lo = native_lo;
            hi = native_hi;
            SyncState();
            return (result != 0);
        }
    }

    public bool Rating(string id, ref int value, int max)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        var native_value = value;
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_rating(_ctx, __idPointer, &native_value, max);
            value = native_value;
            SyncState();
            return (result != 0);
        }
    }

    public int Reactions(string id, Span<Reaction> items, int count)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckLength(count, items.Length, nameof(count));
        var native_items = CopyReaction(items);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_reactions(_ctx, __idPointer, native_items, count);
            for (var i = 0; i < items.Length; i++) items[i] = FromNative(native_items[i], items[i]);
            SyncState();
            return result;
        }
    }

    public void Readout(string label, string value, ReadoutOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(label);
        ArgumentNullException.ThrowIfNull(value);
        var native_opts = ToNative(opts);
        using var __labelUtf8 = new ScopedUtf8(label, stackalloc byte[256]);
        using var __valueUtf8 = new ScopedUtf8(value, stackalloc byte[256]);
        fixed (byte* __labelPointer = __labelUtf8.Bytes)
        fixed (byte* __valuePointer = __valueUtf8.Bytes)
        {
            cui_readout_ex(_ctx, __labelPointer, __valuePointer, &native_opts);
            SyncState();
        }
    }

    public UiScope ReasoningBegin(string id, ReasoningOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckBegin();
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_reasoning_begin(_ctx, __idPointer, &native_opts);
            SyncState();
            return TrackScope(ScopeKind.Reasoning, result != 0, result != 0 ? 1 : 0);
        }
    }

    public void ReasoningEnd()
    {
        CheckFrame();
        BeforeExplicitEnd(ScopeKind.Reasoning);
        cui_reasoning_end(_ctx);
        SyncState();
    }

    public RegexTesterResult RegexTester(string id, TextBuffer pattern, TextBuffer text, RegexTesterOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(pattern);
        ArgumentNullException.ThrowIfNull(text);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_regex_tester_ex(_ctx, __idPointer, Use(pattern), (ulong)pattern.Capacity, Use(text), (ulong)text.Capacity, &native_opts);
            SyncState();
            return FromNative(result);
        }
    }

    public UiScope ReorderBegin(string id, Memory<int> order, int count)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckBegin();
        ValidateOrder(order.Span, count);
        CheckLength(count, order.Length, nameof(order));
        CheckLength(count, order.Length, nameof(count));
        var native_order = CopyInt(order.Span);
        JournalInt(order, native_order);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            cui_reorder_begin(_ctx, __idPointer, native_order, count);
            SyncState();
            return TrackScope(ScopeKind.Reorder, true, 0);
        }
    }

    public bool ReorderEnd()
    {
        CheckFrame();
        BeforeExplicitEnd(ScopeKind.Reorder);
        var result = cui_reorder_end(_ctx);
        _arraysDirty = true;
        SyncState();
        return (result != 0);
    }

    public UiScope ReorderItemBegin(int position)
    {
        CheckFrame();
        CheckBegin();
        RequireScope(ScopeKind.Reorder);
        cui_reorder_item_begin(_ctx, position);
        SyncState();
        return TrackScope(ScopeKind.ReorderItem, true, 0);
    }

    public void ReorderItemEnd()
    {
        CheckFrame();
        BeforeExplicitEnd(ScopeKind.ReorderItem);
        cui_reorder_item_end(_ctx);
        SyncState();
    }

    public RequestBarResult RequestBar(string id, ref int method, TextBuffer url, RequestBarOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        var native_method = method;
        ArgumentNullException.ThrowIfNull(url);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_request_bar_ex(_ctx, __idPointer, &native_method, Use(url), (ulong)url.Capacity, &native_opts);
            method = native_method;
            SyncState();
            return FromNative(result);
        }
    }

    public void RequestRedraw()
    {
        Check();
        cui_request_redraw(_ctx);
        SyncState();
    }

    public void RequestWakeup(float seconds)
    {
        Check();
        cui_request_wakeup(_ctx, seconds);
        SyncState();
    }

    public int ResponseActions(string id, ResponseActionsOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_response_actions_ex(_ctx, __idPointer, &native_opts);
            SyncState();
            return result;
        }
    }

    public void ResponseStatus(int status, double seconds, double bytes)
    {
        CheckFrame();
        cui_response_status(_ctx, status, seconds, bytes);
        SyncState();
    }

    public UiScope RowBegin(Box opts = default)
    {
        CheckFrame();
        CheckBegin();
        var native_opts = ToNative(opts);
        var result = cui_row_begin(_ctx, &native_opts);
        SyncState();
        return TrackScope(ScopeKind.Box, true, unchecked((int)result));
    }

    public bool RulerSlider(string label, ref float value, float min, float max, RulerOpts opts = default)
    {
        CheckFrame();
        CheckRange(min, max);
        ArgumentNullException.ThrowIfNull(label);
        var native_value = value;
        var native_opts = ToNative(opts);
        using var __labelUtf8 = new ScopedUtf8(label, stackalloc byte[256]);
        fixed (byte* __labelPointer = __labelUtf8.Bytes)
        {
            var result = cui_ruler_slider_ex(_ctx, __labelPointer, &native_value, min, max, &native_opts);
            value = native_value;
            SyncState();
            return (result != 0);
        }
    }

    public int Sankey(string id, ReadOnlySpan<string> nodes, int node_count, ReadOnlySpan<SankeyLink> links, int link_count, SankeyOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        ValidateLinks(links, link_count, node_count);
        CheckLength(link_count, links.Length, nameof(link_count));
        CheckLength(node_count, nodes.Length, nameof(node_count));
        var native_links = CopySankeyLink(links);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_sankey_ex(_ctx, __idPointer, Strings(nodes), node_count, native_links, link_count, &native_opts);
            SyncState();
            return result;
        }
    }

    public int ScatterChart(string id, ReadOnlySpan<ScatterSeries> series, int count, ScatterOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckLength(count, series.Length, nameof(count));
        var native_series = CopyScatterSeries(series);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_scatter_chart_ex(_ctx, __idPointer, native_series, count, &native_opts);
            SyncState();
            return result;
        }
    }

    public ScheduleResult SchedulePicker(string id, TextBuffer cron, ScheduleOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(cron);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_schedule_picker_ex(_ctx, __idPointer, Use(cron), (ulong)cron.Capacity, &native_opts);
            SyncState();
            return FromNative(result);
        }
    }

    public UiScope ScrollBegin(ScrollOpts opts = default)
    {
        CheckFrame();
        CheckBegin();
        var native_opts = ToNative(opts);
        cui_scroll_begin(_ctx, &native_opts);
        SyncState();
        return TrackScope(ScopeKind.Scroll, true, 0);
    }

    public void ScrollEnd()
    {
        CheckFrame();
        BeforeExplicitEnd(ScopeKind.Scroll);
        cui_scroll_end(_ctx);
        SyncState();
    }

    public void ScrollTo(string scroll_id, float y)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(scroll_id);
        using var __scroll_idUtf8 = new ScopedUtf8(scroll_id, stackalloc byte[256]);
        fixed (byte* __scroll_idPointer = __scroll_idUtf8.Bytes)
        {
            cui_scroll_to(_ctx, __scroll_idPointer, y);
            SyncState();
        }
    }

    public bool Scrub(string label, ref float value, float min, float max, ScrubOpts opts = default)
    {
        CheckFrame();
        CheckRange(min, max);
        ArgumentNullException.ThrowIfNull(label);
        var native_value = value;
        var native_opts = ToNative(opts);
        using var __labelUtf8 = new ScopedUtf8(label, stackalloc byte[256]);
        fixed (byte* __labelPointer = __labelUtf8.Bytes)
        {
            var result = cui_scrub_ex(_ctx, __labelPointer, &native_value, min, max, &native_opts);
            value = native_value;
            SyncState();
            return (result != 0);
        }
    }

    public int Search(string id, TextBuffer buf, InputOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(buf);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_search_ex(_ctx, __idPointer, Use(buf), (ulong)buf.Capacity, &native_opts);
            SyncState();
            return result;
        }
    }

    public void Secondary(string text)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(text);
        using var __textUtf8 = new ScopedUtf8(text, stackalloc byte[256]);
        fixed (byte* __textPointer = __textUtf8.Bytes)
        {
            cui_secondary(_ctx, __textPointer);
            SyncState();
        }
    }

    public bool Segmented(string id, ReadOnlySpan<string> items, int count, ref int index, SegmentedOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckOptional(count, opts.Icons.Length, nameof(opts.Icons));
        CheckLength(count, items.Length, nameof(count));
        var native_index = index;
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_segmented_ex(_ctx, __idPointer, Strings(items), count, &native_index, &native_opts);
            index = native_index;
            SyncState();
            return (result != 0);
        }
    }

    public bool Select(string id, ReadOnlySpan<string> items, int count, ref int index, SelectOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckOptional(count, opts.Icons.Length, nameof(opts.Icons));
        CheckLength(count, items.Length, nameof(count));
        var native_index = index;
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_select_ex(_ctx, __idPointer, Strings(items), count, &native_index, &native_opts);
            index = native_index;
            SyncState();
            return (result != 0);
        }
    }

    public void Separator()
    {
        CheckFrame();
        cui_separator(_ctx);
        SyncState();
    }

    public void SetAccent(Accent accent)
    {
        Check();
        cui_set_accent(_ctx, (cui_accent)CheckEnum(accent));
        SyncState();
    }

    public void SetReducedMotion(bool reduced)
    {
        Check();
        cui_set_reduced_motion(_ctx, reduced);
        SyncState();
    }

    public void SetTheme(ThemeMode mode)
    {
        Check();
        cui_set_theme(_ctx, (cui_theme_mode)CheckEnum(mode));
        SyncState();
    }

    public UiScope SheetBegin(string title, ref bool open, SheetOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(title);
        CheckBegin();
        CBool native_open = open;
        var native_opts = ToNative(opts);
        using var __titleUtf8 = new ScopedUtf8(title, stackalloc byte[256]);
        fixed (byte* __titlePointer = __titleUtf8.Bytes)
        {
            var result = cui_sheet_begin(_ctx, __titlePointer, &native_open, &native_opts);
            open = (native_open != 0);
            SyncState();
            return TrackScope(ScopeKind.Sheet, result != 0, result != 0 ? 1 : 0);
        }
    }

    public void SheetEnd()
    {
        CheckFrame();
        BeforeExplicitEnd(ScopeKind.Sheet);
        cui_sheet_end(_ctx);
        SyncState();
    }

    public void ShimmerText(string text, TextFxOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(text);
        var native_opts = ToNative(opts);
        using var __textUtf8 = new ScopedUtf8(text, stackalloc byte[256]);
        fixed (byte* __textPointer = __textUtf8.Bytes)
        {
            cui_shimmer_text_ex(_ctx, __textPointer, &native_opts);
            SyncState();
        }
    }

    public bool Shortcut(int mods, Key key)
    {
        Check();
        var result = cui_shortcut(_ctx, mods, (cui_key)CheckEnum(key));
        SyncState();
        return (result != 0);
    }

    public string? ShortcutLabel(KeyCombo combo)
    {
        CheckFrame();
        var result = cui_shortcut_label(_ctx, ToNative(combo));
        SyncState();
        return Decode(result);
    }

    public bool ShortcutRecorder(string id, ref KeyCombo combo, ShortcutRecorderOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        var native_combo = ToNative(combo);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_shortcut_recorder_ex(_ctx, __idPointer, &native_combo, &native_opts);
            combo = FromNative(native_combo, combo);
            SyncState();
            return (result != 0);
        }
    }

    public bool SignaturePad(string id, SignatureOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_signature_pad_ex(_ctx, __idPointer, &native_opts);
            SyncState();
            return (result != 0);
        }
    }

    public void Skeleton(Size width, float height)
    {
        CheckFrame();
        cui_skeleton(_ctx, ToNative(width), height);
        SyncState();
    }

    public bool Slider(string label, ref float value, float min, float max, SliderOpts opts = default)
    {
        CheckFrame();
        CheckRange(min, max);
        ArgumentNullException.ThrowIfNull(label);
        var native_value = value;
        var native_opts = ToNative(opts);
        using var __labelUtf8 = new ScopedUtf8(label, stackalloc byte[256]);
        fixed (byte* __labelPointer = __labelUtf8.Bytes)
        {
            var result = cui_slider_ex(_ctx, __labelPointer, &native_value, min, max, &native_opts);
            value = native_value;
            SyncState();
            return (result != 0);
        }
    }

    public int SourceList(string id, ReadOnlySpan<Source> sources, int count)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckLength(count, sources.Length, nameof(count));
        var native_sources = CopySource(sources);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_source_list(_ctx, __idPointer, native_sources, count);
            SyncState();
            return result;
        }
    }

    public void Spacer()
    {
        CheckFrame();
        cui_spacer(_ctx);
        SyncState();
    }

    public int SpeedDial(string id, ReadOnlySpan<SpeedAction> actions, int count, SpeedDialOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckLength(count, actions.Length, nameof(count));
        var native_actions = CopySpeedAction(actions);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_speed_dial_ex(_ctx, __idPointer, native_actions, count, &native_opts);
            SyncState();
            return result;
        }
    }

    public void Spinner(float size)
    {
        CheckFrame();
        cui_spinner(_ctx, size);
        SyncState();
    }

    public int SplitBar(string id, ReadOnlySpan<VizItem> items, int count, SplitBarOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckLength(count, items.Length, nameof(count));
        var native_items = CopyVizItem(items);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_split_bar_ex(_ctx, __idPointer, native_items, count, &native_opts);
            SyncState();
            return result;
        }
    }

    public int SplitButton(string label, ReadOnlySpan<string> items, int count, SplitButtonOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(label);
        CheckOptional(count, opts.ItemIcons.Length, nameof(opts.ItemIcons));
        CheckLength(count, items.Length, nameof(count));
        var native_opts = ToNative(opts);
        using var __labelUtf8 = new ScopedUtf8(label, stackalloc byte[256]);
        fixed (byte* __labelPointer = __labelUtf8.Bytes)
        {
            var result = cui_split_button_ex(_ctx, __labelPointer, Strings(items), count, &native_opts);
            SyncState();
            return result;
        }
    }

    public UiScope SpotlightBegin(string id, SpotlightOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckBegin();
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            cui_spotlight_begin(_ctx, __idPointer, &native_opts);
            SyncState();
            return TrackScope(ScopeKind.Spotlight, true, 0);
        }
    }

    public void SpotlightEnd()
    {
        CheckFrame();
        BeforeExplicitEnd(ScopeKind.Spotlight);
        cui_spotlight_end(_ctx);
        SyncState();
    }

    public StackTraceResult StackTrace(string id, ReadOnlySpan<StackFrame> frames, int count, StackTraceOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckLength(count, frames.Length, nameof(count));
        var native_frames = CopyStackFrame(frames);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_stack_trace_ex(_ctx, __idPointer, native_frames, count, &native_opts);
            SyncState();
            return FromNative(result);
        }
    }

    public bool StatusButton(string label, Status status, StatusButtonOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(label);
        var native_opts = ToNative(opts);
        using var __labelUtf8 = new ScopedUtf8(label, stackalloc byte[256]);
        fixed (byte* __labelPointer = __labelUtf8.Bytes)
        {
            var result = cui_status_button_ex(_ctx, __labelPointer, (cui_status)CheckEnum(status), &native_opts);
            SyncState();
            return (result != 0);
        }
    }

    public void Stepper(ReadOnlySpan<string> steps, int count, int current)
    {
        CheckFrame();
        CheckLength(count, steps.Length, nameof(count));
        cui_stepper(_ctx, Strings(steps), count, current);
        SyncState();
    }

    public UiScope SwipeItemBegin(string id, ReadOnlySpan<SwipeAction> left, int left_count, ReadOnlySpan<SwipeAction> right, int right_count)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckBegin();
        CheckLength(right_count, right.Length, nameof(right_count));
        CheckLength(left_count, left.Length, nameof(left_count));
        var native_left = CopySwipeAction(left);
        var native_right = CopySwipeAction(right);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            cui_swipe_item_begin(_ctx, __idPointer, native_left, left_count, native_right, right_count);
            SyncState();
            return TrackScope(ScopeKind.SwipeItem, true, 0);
        }
    }

    public int SwipeItemEnd()
    {
        CheckFrame();
        BeforeExplicitEnd(ScopeKind.SwipeItem);
        var result = cui_swipe_item_end(_ctx);
        SyncState();
        return result;
    }

    public bool Switch(string label, ref bool value, CheckOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(label);
        CBool native_value = value;
        var native_opts = ToNative(opts);
        using var __labelUtf8 = new ScopedUtf8(label, stackalloc byte[256]);
        fixed (byte* __labelPointer = __labelUtf8.Bytes)
        {
            var result = cui_switch_ex(_ctx, __labelPointer, &native_value, &native_opts);
            value = (native_value != 0);
            SyncState();
            return (result != 0);
        }
    }

    public bool TabBar(string id, ReadOnlySpan<TabBarItem> items, int count, ref int selected, TabBarOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckLength(count, items.Length, nameof(count));
        var native_items = CopyTabBarItem(items);
        var native_selected = selected;
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_tab_bar_ex(_ctx, __idPointer, native_items, count, &native_selected, &native_opts);
            selected = native_selected;
            SyncState();
            return (result != 0);
        }
    }

    public UiScope TableBegin(string id, ReadOnlySpan<TableColumn> columns, int column_count, TableOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckBegin();
        CheckLength(column_count, columns.Length, nameof(column_count));
        var native_columns = CopyTableColumn(columns);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_table_begin_ex(_ctx, __idPointer, native_columns, column_count, &native_opts);
            SyncState();
            return TrackScope(ScopeKind.Table, true, result != 0 ? 1 : 0);
        }
    }

    public void TableCell()
    {
        CheckFrame();
        cui_table_cell(_ctx);
        SyncState();
    }

    public void TableCellText(string text)
    {
        CheckFrame();
        if (_ctx->building.table == null || _ctx->building.table->in_row == 0)
            throw new InvalidOperationException("Build a table row before its text cells.");

        ArgumentNullException.ThrowIfNull(text);
        using var __textUtf8 = new ScopedUtf8(text, stackalloc byte[256]);
        fixed (byte* __textPointer = __textUtf8.Bytes)
        {
            cui_table_cell_text(_ctx, __textPointer);
            SyncState();
        }
    }

    public void TableEnd()
    {
        CheckFrame();
        BeforeExplicitEnd(ScopeKind.Table);
        cui_table_end(_ctx);
        SyncState();
    }

    public bool TableRow(bool selected)
    {
        CheckFrame();
        var result = cui_table_row(_ctx, selected);
        SyncState();
        return (result != 0);
    }

    public bool Tabs(string id, ReadOnlySpan<string> items, int count, ref int index, TabsOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckOptional(count, opts.Icons.Length, nameof(opts.Icons));
        CheckLength(count, items.Length, nameof(count));
        var native_index = index;
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_tabs_ex(_ctx, __idPointer, Strings(items), count, &native_index, &native_opts);
            index = native_index;
            SyncState();
            return (result != 0);
        }
    }

    public bool TagInput(string id, TextBuffer buf, TagOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(buf);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_tag_input_ex(_ctx, __idPointer, Use(buf), (ulong)buf.Capacity, &native_opts);
            SyncState();
            return (result != 0);
        }
    }

    public TestExplorerResult TestExplorer(string id, ReadOnlySpan<TestItem> items, int count, TestExplorerOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckLength(count, items.Length, nameof(count));
        var native_items = CopyTestItem(items);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_test_explorer_ex(_ctx, __idPointer, native_items, count, &native_opts);
            SyncState();
            return FromNative(result);
        }
    }

    public void Text(string text, TextOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(text);
        var native_opts = ToNative(opts);
        using var __textUtf8 = new ScopedUtf8(text, stackalloc byte[256]);
        fixed (byte* __textPointer = __textUtf8.Bytes)
        {
            cui_text_ex(_ctx, __textPointer, &native_opts);
            SyncState();
        }
    }

    public void TextReveal(string id, string text, TextFxOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(text);
        var native_opts = ToNative(opts);
        using var __textUtf8 = new ScopedUtf8(text, stackalloc byte[256]);
        fixed (byte* __textPointer = __textUtf8.Bytes)
        {
            using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
            fixed (byte* __idPointer = __idUtf8.Bytes)
            {
                cui_text_reveal_ex(_ctx, __idPointer, __textPointer, &native_opts);
                SyncState();
            }
        }
    }

    public int Textarea(string id, TextBuffer buf, InputOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(buf);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_textarea_ex(_ctx, __idPointer, Use(buf), (ulong)buf.Capacity, &native_opts);
            SyncState();
            return result;
        }
    }

    public bool ThemeSwitcher(ref int mode, bool with_system)
    {
        CheckFrame();
        var native_mode = mode;
        var result = cui_theme_switcher(_ctx, &native_mode, with_system);
        mode = native_mode;
        SyncState();
        return (result != 0);
    }

    public bool ThemeToggle()
    {
        CheckFrame();
        var result = cui_theme_toggle(_ctx);
        SyncState();
        return (result != 0);
    }

    public void Ticker(string id, ReadOnlySpan<TickerItem> items, int count, TickerOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckLength(count, items.Length, nameof(count));
        var native_items = CopyTickerItem(items);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            cui_ticker_ex(_ctx, __idPointer, native_items, count, &native_opts);
            SyncState();
        }
    }

    public double Time()
    {
        Check();
        var result = cui_time(_ctx);
        SyncState();
        return result;
    }

    public bool TimeDial(string id, ref int hour, ref int minute, float size)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        var native_hour = hour;
        var native_minute = minute;
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_time_dial(_ctx, __idPointer, &native_hour, &native_minute, size);
            hour = native_hour;
            minute = native_minute;
            SyncState();
            return (result != 0);
        }
    }

    public TimeHistogramResult TimeHistogram(string id, double start, double bucket_seconds, int buckets, ReadOnlySpan<HistogramSeries> series, int series_count, ref double sel_start, ref double sel_end, TimeHistogramOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        ValidateHistogram(series, series_count, buckets);
        CheckLength(series_count, series.Length, nameof(series_count));
        var native_series = CopyHistogramSeries(series);
        var native_sel_start = sel_start;
        var native_sel_end = sel_end;
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_time_histogram_ex(_ctx, __idPointer, start, bucket_seconds, buckets, native_series, series_count, &native_sel_start, &native_sel_end, &native_opts);
            sel_start = native_sel_start;
            sel_end = native_sel_end;
            SyncState();
            return FromNative(result);
        }
    }

    public bool TimePicker(string id, ref int hour, ref int minute, TimeOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        var native_hour = hour;
        var native_minute = minute;
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_time_picker_ex(_ctx, __idPointer, &native_hour, &native_minute, &native_opts);
            hour = native_hour;
            minute = native_minute;
            SyncState();
            return (result != 0);
        }
    }

    public void TimelineItem(string title, TimelineOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(title);
        var native_opts = ToNative(opts);
        using var __titleUtf8 = new ScopedUtf8(title, stackalloc byte[256]);
        fixed (byte* __titlePointer = __titleUtf8.Bytes)
        {
            cui_timeline_item_ex(_ctx, __titlePointer, &native_opts);
            SyncState();
        }
    }

    public bool ToastActionClicked(int handle)
    {
        CheckFrame();
        var result = cui_toast_action_clicked(_ctx, handle);
        SyncState();
        return (result != 0);
    }

    public void ToastDismiss(int handle)
    {
        CheckFrame();
        cui_toast_dismiss(_ctx, handle);
        SyncState();
    }

    public int Toast(string title, ToastOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(title);
        var native_opts = ToNative(opts);
        using var __titleUtf8 = new ScopedUtf8(title, stackalloc byte[256]);
        fixed (byte* __titlePointer = __titleUtf8.Bytes)
        {
            var result = cui_toast_ex(_ctx, __titlePointer, &native_opts);
            SyncState();
            return result;
        }
    }

    public int Toc(string scroll_id, ReadOnlySpan<TocItem> items, int count)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(scroll_id);
        CheckLength(count, items.Length, nameof(count));
        var native_items = CopyTocItem(items);
        using var __scroll_idUtf8 = new ScopedUtf8(scroll_id, stackalloc byte[256]);
        fixed (byte* __scroll_idPointer = __scroll_idUtf8.Bytes)
        {
            var result = cui_toc(_ctx, __scroll_idPointer, native_items, count);
            SyncState();
            return result;
        }
    }

    public void TocAnchor(string anchor)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(anchor);
        using var __anchorUtf8 = new ScopedUtf8(anchor, stackalloc byte[256]);
        fixed (byte* __anchorPointer = __anchorUtf8.Bytes)
        {
            cui_toc_anchor(_ctx, __anchorPointer);
            SyncState();
        }
    }

    public bool ToggleGroup(string id, ReadOnlySpan<string> labels, int count, Span<bool> values, ToggleGroupOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckOptional(count, opts.Icons.Length, nameof(opts.Icons));
        CheckLength(count, values.Length, nameof(values));
        CheckLength(count, labels.Length, nameof(count));
        var native_values = CopyBool(values);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_toggle_group_ex(_ctx, __idPointer, Strings(labels), count, native_values, &native_opts);
            for (var i = 0; i < values.Length; i++) values[i] = (native_values[i] != 0);
            SyncState();
            return (result != 0);
        }
    }

    public void ToolCall(string id, string name, ToolCallOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(name);
        var native_opts = ToNative(opts);
        using var __nameUtf8 = new ScopedUtf8(name, stackalloc byte[256]);
        fixed (byte* __namePointer = __nameUtf8.Bytes)
        {
            using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
            fixed (byte* __idPointer = __idUtf8.Bytes)
            {
                cui_tool_call_ex(_ctx, __idPointer, __namePointer, &native_opts);
                SyncState();
            }
        }
    }

    public void Tooltip(string text)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(text);
        using var __textUtf8 = new ScopedUtf8(text, stackalloc byte[256]);
        fixed (byte* __textPointer = __textUtf8.Bytes)
        {
            cui_tooltip(_ctx, __textPointer);
            SyncState();
        }
    }

    public int Tour(string id, ReadOnlySpan<TourStep> steps, int count, ref int step, TourOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckLength(count, steps.Length, nameof(count));
        var native_steps = CopyTourStep(steps);
        var native_step = step;
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_tour_ex(_ctx, __idPointer, native_steps, count, &native_step, &native_opts);
            step = native_step;
            SyncState();
            return result;
        }
    }

    public void TraceSort(Span<TraceSpan> spans, int count)
    {
        Check();
        CheckLength(count, spans.Length, nameof(count));
        var native_spans = CopyTraceSpan(spans);
        cui_trace_sort(native_spans, count);
        for (var i = 0; i < spans.Length; i++) spans[i] = FromNative(native_spans[i], spans[i]);
        SyncState();
    }

    public TraceResult TraceView(string id, Span<TraceTrack> tracks, int track_count, ReadOnlySpan<TraceSpan> spans, int span_count, ref TraceView view, ref int selected, TraceOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        ValidateTracks(spans, span_count, track_count);
        CheckLength(span_count, spans.Length, nameof(span_count));
        CheckLength(track_count, tracks.Length, nameof(track_count));
        var native_tracks = CopyTraceTrack(tracks);
        var native_spans = CopyTraceSpan(spans);
        var native_view = ToNative(view);
        var native_selected = selected;
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_trace_view_ex(_ctx, __idPointer, native_tracks, track_count, native_spans, span_count, &native_view, &native_selected, &native_opts);
            for (var i = 0; i < tracks.Length; i++) tracks[i] = FromNative(native_tracks[i], tracks[i]);
            view = FromNative(native_view, view);
            selected = native_selected;
            SyncState();
            return FromNative(result);
        }
    }

    public UiScope TreeBegin(string label, ref bool open, TreeOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(label);
        CheckBegin();
        CBool native_open = open;
        var native_opts = ToNative(opts);
        using var __labelUtf8 = new ScopedUtf8(label, stackalloc byte[256]);
        fixed (byte* __labelPointer = __labelUtf8.Bytes)
        {
            var result = cui_tree_begin(_ctx, __labelPointer, &native_open, &native_opts);
            open = (native_open != 0);
            SyncState();
            return TrackScope(ScopeKind.Tree, result != 0, result != 0 ? 1 : 0);
        }
    }

    public bool TreeCell(string label, int depth, bool has_children, ref bool open, string icon)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(label);
        ArgumentNullException.ThrowIfNull(icon);
        CBool native_open = open;
        using var __labelUtf8 = new ScopedUtf8(label, stackalloc byte[256]);
        using var __iconUtf8 = new ScopedUtf8(icon, stackalloc byte[256]);
        fixed (byte* __labelPointer = __labelUtf8.Bytes)
        fixed (byte* __iconPointer = __iconUtf8.Bytes)
        {
            var result = cui_tree_cell(_ctx, __labelPointer, depth, has_children, &native_open, __iconPointer);
            open = (native_open != 0);
            SyncState();
            return (result != 0);
        }
    }

    public bool TreeClicked()
    {
        CheckFrame();
        var result = cui_tree_clicked(_ctx);
        SyncState();
        return (result != 0);
    }

    public void TreeEnd()
    {
        CheckFrame();
        BeforeExplicitEnd(ScopeKind.Tree);
        cui_tree_end(_ctx);
        SyncState();
    }

    public int Treemap(string id, ReadOnlySpan<VizItem> items, int count, TreemapOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckLength(count, items.Length, nameof(count));
        var native_items = CopyVizItem(items);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_treemap_ex(_ctx, __idPointer, native_items, count, &native_opts);
            SyncState();
            return result;
        }
    }

    public bool Typewriter(string id, string text, TextFxOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(text);
        var native_opts = ToNative(opts);
        using var __textUtf8 = new ScopedUtf8(text, stackalloc byte[256]);
        fixed (byte* __textPointer = __textUtf8.Bytes)
        {
            using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
            fixed (byte* __idPointer = __idUtf8.Bytes)
            {
                var result = cui_typewriter_ex(_ctx, __idPointer, __textPointer, &native_opts);
                SyncState();
                return (result != 0);
            }
        }
    }

    public void TypingIndicator(string name)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(name);
        using var __nameUtf8 = new ScopedUtf8(name, stackalloc byte[256]);
        fixed (byte* __namePointer = __nameUtf8.Bytes)
        {
            cui_typing_indicator(_ctx, __namePointer);
            SyncState();
        }
    }

    public UptimeResult Uptime(string id, ReadOnlySpan<UptimeService> services, int count, UptimeOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckLength(count, services.Length, nameof(count));
        var native_services = CopyUptimeService(services);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_uptime_ex(_ctx, __idPointer, native_services, count, &native_opts);
            SyncState();
            return FromNative(result);
        }
    }

    public UiScope UserMenuBegin(string name, UserMenuOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(name);
        CheckBegin();
        var native_opts = ToNative(opts);
        using var __nameUtf8 = new ScopedUtf8(name, stackalloc byte[256]);
        fixed (byte* __namePointer = __nameUtf8.Bytes)
        {
            var result = cui_user_menu_begin(_ctx, __namePointer, &native_opts);
            SyncState();
            return TrackScope(ScopeKind.UserMenu, result != 0, result != 0 ? 1 : 0);
        }
    }

    public void UserMenuEnd()
    {
        CheckFrame();
        BeforeExplicitEnd(ScopeKind.UserMenu);
        cui_user_menu_end(_ctx);
        SyncState();
    }

    public Vec2 ViewportSize()
    {
        Check();
        var result = cui_viewport_size(_ctx);
        SyncState();
        return FromNative(result);
    }

    public UiScope VirtualListBegin(string id, int count, float row_height, Size height, ref int first, ref int last)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckBegin();
        var native_first = first;
        var native_last = last;
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            cui_virtual_list_begin(_ctx, __idPointer, count, row_height, ToNative(height), &native_first, &native_last);
            first = native_first;
            last = native_last;
            SyncState();
            return TrackScope(ScopeKind.VirtualList, true, 0);
        }
    }

    public void VirtualListEnd()
    {
        CheckFrame();
        BeforeExplicitEnd(ScopeKind.VirtualList);
        cui_virtual_list_end(_ctx);
        SyncState();
    }

    public bool VoiceOrb(string id, VoiceState state, float level, VoiceOrbOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_voice_orb_ex(_ctx, __idPointer, (cui_voice_state)CheckEnum(state), level, &native_opts);
            SyncState();
            return (result != 0);
        }
    }

    public int Waffle(string id, ReadOnlySpan<VizItem> items, int count, WaffleOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckLength(count, items.Length, nameof(count));
        var native_items = CopyVizItem(items);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_waffle_ex(_ctx, __idPointer, native_items, count, &native_opts);
            SyncState();
            return result;
        }
    }

    public bool WantsKeyboard()
    {
        Check();
        var result = cui_wants_keyboard(_ctx);
        SyncState();
        return (result != 0);
    }

    public bool WantsMouse()
    {
        Check();
        var result = cui_wants_mouse(_ctx);
        SyncState();
        return (result != 0);
    }

    public WaterfallResult Waterfall(string id, ReadOnlySpan<NetRequest> requests, int count, WaterfallOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckLength(count, requests.Length, nameof(count));
        var native_requests = CopyNetRequest(requests);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_waterfall_ex(_ctx, __idPointer, native_requests, count, &native_opts);
            SyncState();
            return FromNative(result);
        }
    }

    public bool Waveform(string id, ReadOnlySpan<float> peaks, int count, ref float progress, WaveformOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckLength(count, peaks.Length, nameof(count));
        var native_peaks = CopyFloat(peaks);
        var native_progress = progress;
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_waveform_ex(_ctx, __idPointer, native_peaks, count, &native_progress, &native_opts);
            progress = native_progress;
            SyncState();
            return (result != 0);
        }
    }

    public int WeekCalendar(string id, Date week_start, Span<CalEvent> events, int count, WeekOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckLength(count, events.Length, nameof(count));
        var native_events = CopyCalEvent(events);
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_week_calendar_ex(_ctx, __idPointer, ToNative(week_start), native_events, count, &native_opts);
            for (var i = 0; i < events.Length; i++) events[i] = FromNative(native_events[i], events[i]);
            SyncState();
            return result;
        }
    }

    public bool WheelPicker(string id, ReadOnlySpan<string> items, int count, ref int index, WheelOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckLength(count, items.Length, nameof(count));
        var native_index = index;
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_wheel_picker_ex(_ctx, __idPointer, Strings(items), count, &native_index, &native_opts);
            index = native_index;
            SyncState();
            return (result != 0);
        }
    }

    public void WordRotate(string id, ReadOnlySpan<string> words, int count, float interval, TextFxOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(id);
        CheckLength(count, words.Length, nameof(count));
        var native_opts = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            cui_word_rotate_ex(_ctx, __idPointer, Strings(words), count, interval, &native_opts);
            SyncState();
        }
    }
}
