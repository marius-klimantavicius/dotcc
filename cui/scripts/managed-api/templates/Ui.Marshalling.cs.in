using Raw = global::Managed.Forms.Cui;
using static global::Managed.Forms.Cui;
using CBool = global::Managed.Forms.Cui.CBool;
namespace Cui.Managed;
public sealed unsafe partial class Ui
{
    private cui_agent_plan_opts ToNative(AgentPlanOpts v)
    {
        cui_agent_plan_opts r = default;
        r.title = Utf8(v.Title);
        return r;
    }
    private AgentPlanOpts FromNative(cui_agent_plan_opts r, AgentPlanOpts old = default)
    {
        var v = old;
        v.Title = Decode(r.title, old.Title);
        return v;
    }
    private cui_agent_status_opts ToNative(AgentStatusOpts v)
    {
        cui_agent_status_opts r = default;
        r.status = (cui_task_status)CheckEnum(v.Status);
        r.seconds = v.Seconds;
        r.icon = Utf8(v.Icon);
        return r;
    }
    private AgentStatusOpts FromNative(cui_agent_status_opts r, AgentStatusOpts old = default)
    {
        var v = old;
        v.Status = (TaskStatus)r.status;
        v.Seconds = r.seconds;
        v.Icon = Decode(r.icon, old.Icon);
        return v;
    }
    private cui_alert_opts ToNative(AlertOpts v)
    {
        cui_alert_opts r = default;
        r.description = Utf8(v.Description);
        r.tone = (cui_tone)CheckEnum(v.Tone);
        r.icon = Utf8(v.Icon);
        r.dismissible = v.Dismissible;
        return r;
    }
    private AlertOpts FromNative(cui_alert_opts r, AlertOpts old = default)
    {
        var v = old;
        v.Description = Decode(r.description, old.Description);
        v.Tone = (Tone)r.tone;
        v.Icon = Decode(r.icon, old.Icon);
        v.Dismissible = (r.dismissible != 0);
        return v;
    }
    private cui_api_endpoint ToNative(ApiEndpoint v)
    {
        cui_api_endpoint r = default;
        r.method = Utf8(v.Method);
        r.path = Utf8(v.Path);
        r.summary = Utf8(v.Summary);
        r.tag = Utf8(v.Tag);
        r.deprecated = v.Deprecated;
        r.@params = CopyApiParam(v.Params.Span);
        r.param_count = (int)v.Params.Length;
        return r;
    }
    private ApiEndpoint FromNative(cui_api_endpoint r, ApiEndpoint old = default)
    {
        var v = old;
        v.Method = Decode(r.method, old.Method);
        v.Path = Decode(r.path, old.Path);
        v.Summary = Decode(r.summary, old.Summary);
        v.Tag = Decode(r.tag, old.Tag);
        v.Deprecated = (r.deprecated != 0);
        return v;
    }
    private cui_api_explorer_opts ToNative(ApiExplorerOpts v)
    {
        cui_api_explorer_opts r = default;
        r.width = ToNative(v.Width);
        r.height = v.Height;
        r.selected = BindInt(v.Selected);
        return r;
    }
    private ApiExplorerOpts FromNative(cui_api_explorer_opts r, ApiExplorerOpts old = default)
    {
        var v = old;
        v.Width = FromNative(r.width, old.Width);
        v.Height = r.height;
        return v;
    }
    private cui_api_explorer_result ToNative(ApiExplorerResult v)
    {
        cui_api_explorer_result r = default;
        r.selected = v.Selected;
        r.try_it = v.TryIt;
        return r;
    }
    private ApiExplorerResult FromNative(cui_api_explorer_result r, ApiExplorerResult old = default)
    {
        var v = old;
        v.Selected = r.selected;
        v.TryIt = r.try_it;
        return v;
    }
    private cui_api_param ToNative(ApiParam v)
    {
        cui_api_param r = default;
        r.name = Utf8(v.Name);
        r.@in = Utf8(v.In);
        r.@type = Utf8(v.Type);
        r.required = v.Required;
        r.description = Utf8(v.Description);
        return r;
    }
    private ApiParam FromNative(cui_api_param r, ApiParam old = default)
    {
        var v = old;
        v.Name = Decode(r.name, old.Name);
        v.In = Decode(r.@in, old.In);
        v.Type = Decode(r.@type, old.Type);
        v.Required = (r.required != 0);
        v.Description = Decode(r.description, old.Description);
        return v;
    }
    private cui_avatar_opts ToNative(AvatarOpts v)
    {
        cui_avatar_opts r = default;
        r.size = v.Size;
        r.image = Unwrap(v.Image);
        r.status = Utf8(v.Status);
        r.square = v.Square;
        return r;
    }
    private AvatarOpts FromNative(cui_avatar_opts r, AvatarOpts old = default)
    {
        var v = old;
        v.Size = r.size;
        v.Image = FindTexture(r.image);
        v.Status = Decode(r.status, old.Status);
        v.Square = (r.square != 0);
        return v;
    }
    private cui_badge_opts ToNative(BadgeOpts v)
    {
        cui_badge_opts r = default;
        r.tone = (cui_tone)CheckEnum(v.Tone);
        r.dot = v.Dot;
        r.solid = v.Solid;
        r.icon = Utf8(v.Icon);
        r.size = (cui_control_size)CheckEnum(v.Size);
        return r;
    }
    private BadgeOpts FromNative(cui_badge_opts r, BadgeOpts old = default)
    {
        var v = old;
        v.Tone = (Tone)r.tone;
        v.Dot = (r.dot != 0);
        v.Solid = (r.solid != 0);
        v.Icon = Decode(r.icon, old.Icon);
        v.Size = (ControlSize)r.size;
        return v;
    }
    private cui_banner_opts ToNative(BannerOpts v)
    {
        cui_banner_opts r = default;
        r.tone = (cui_tone)CheckEnum(v.Tone);
        r.icon = Utf8(v.Icon);
        r.action = Utf8(v.Action);
        r.dismissible = v.Dismissible;
        return r;
    }
    private BannerOpts FromNative(cui_banner_opts r, BannerOpts old = default)
    {
        var v = old;
        v.Tone = (Tone)r.tone;
        v.Icon = Decode(r.icon, old.Icon);
        v.Action = Decode(r.action, old.Action);
        v.Dismissible = (r.dismissible != 0);
        return v;
    }
    private cui_bar_list_opts ToNative(BarListOpts v)
    {
        cui_bar_list_opts r = default;
        r.width = ToNative(v.Width);
        r.format = Utf8(CheckFormat(v.Format, 1));
        r.max_rows = v.MaxRows;
        r.keep_order = v.KeepOrder;
        r.color = ToNative(v.Color);
        return r;
    }
    private BarListOpts FromNative(cui_bar_list_opts r, BarListOpts old = default)
    {
        var v = old;
        v.Width = FromNative(r.width, old.Width);
        v.Format = Decode(r.format, old.Format);
        v.MaxRows = r.max_rows;
        v.KeepOrder = (r.keep_order != 0);
        v.Color = FromNative(r.color, old.Color);
        return v;
    }
    private cui_blame_commit ToNative(BlameCommit v)
    {
        cui_blame_commit r = default;
        r.hash = Utf8(v.Hash);
        r.author = Utf8(v.Author);
        r.summary = Utf8(v.Summary);
        r.time = v.Time;
        return r;
    }
    private BlameCommit FromNative(cui_blame_commit r, BlameCommit old = default)
    {
        var v = old;
        v.Hash = Decode(r.hash, old.Hash);
        v.Author = Decode(r.author, old.Author);
        v.Summary = Decode(r.summary, old.Summary);
        v.Time = r.time;
        return v;
    }
    private cui_blame_opts ToNative(BlameOpts v)
    {
        cui_blame_opts r = default;
        r.width = ToNative(v.Width);
        r.height = v.Height;
        r.lang = Utf8(v.Lang);
        r.now = v.Now;
        r.first_line = v.FirstLine;
        return r;
    }
    private BlameOpts FromNative(cui_blame_opts r, BlameOpts old = default)
    {
        var v = old;
        v.Width = FromNative(r.width, old.Width);
        v.Height = r.height;
        v.Lang = Decode(r.lang, old.Lang);
        v.Now = r.now;
        v.FirstLine = r.first_line;
        return v;
    }
    private cui_blame_result ToNative(BlameResult v)
    {
        cui_blame_result r = default;
        r.clicked = v.Clicked;
        r.hovered = v.Hovered;
        return r;
    }
    private BlameResult FromNative(cui_blame_result r, BlameResult old = default)
    {
        var v = old;
        v.Clicked = r.clicked;
        v.Hovered = r.hovered;
        return v;
    }
    private cui_box ToNative(Box v)
    {
        cui_box r = default;
        r.id = Utf8(v.Id);
        r.dir = (cui_dir)CheckEnum(v.Dir);
        r.width = ToNative(v.Width);
        r.height = ToNative(v.Height);
        r.min_width = v.MinWidth;
        r.max_width = v.MaxWidth;
        r.min_height = v.MinHeight;
        r.max_height = v.MaxHeight;
        r.pad = v.Pad;
        r.pad_x = v.PadX;
        r.pad_y = v.PadY;
        r.pad_top = v.PadTop;
        r.pad_right = v.PadRight;
        r.pad_bottom = v.PadBottom;
        r.pad_left = v.PadLeft;
        r.gap = v.Gap;
        r.columns = v.Columns;
        r.min_column_width = v.MinColumnWidth;
        r.wrap = v.Wrap;
        r.align = (cui_align)CheckEnum(v.Align);
        r.justify = (cui_justify)CheckEnum(v.Justify);
        r.bg = ToNative(v.Bg);
        r.border_color = ToNative(v.BorderColor);
        r.border = v.Border;
        r.radius = v.Radius;
        r.radius_tl = v.RadiusTl;
        r.radius_tr = v.RadiusTr;
        r.radius_br = v.RadiusBr;
        r.radius_bl = v.RadiusBl;
        r.elevation = (cui_elevation)CheckEnum(v.Elevation);
        r.scroll_x = v.ScrollX;
        r.scroll_y = v.ScrollY;
        r.clip = v.Clip;
        r.opacity = v.Opacity;
        r.direction = (cui_direction)CheckEnum(v.Direction);
        return r;
    }
    private Box FromNative(cui_box r, Box old = default)
    {
        var v = old;
        v.Id = Decode(r.id, old.Id);
        v.Dir = (Dir)r.dir;
        v.Width = FromNative(r.width, old.Width);
        v.Height = FromNative(r.height, old.Height);
        v.MinWidth = r.min_width;
        v.MaxWidth = r.max_width;
        v.MinHeight = r.min_height;
        v.MaxHeight = r.max_height;
        v.Pad = r.pad;
        v.PadX = r.pad_x;
        v.PadY = r.pad_y;
        v.PadTop = r.pad_top;
        v.PadRight = r.pad_right;
        v.PadBottom = r.pad_bottom;
        v.PadLeft = r.pad_left;
        v.Gap = r.gap;
        v.Columns = r.columns;
        v.MinColumnWidth = r.min_column_width;
        v.Wrap = (r.wrap != 0);
        v.Align = (Align)r.align;
        v.Justify = (Justify)r.justify;
        v.Bg = FromNative(r.bg, old.Bg);
        v.BorderColor = FromNative(r.border_color, old.BorderColor);
        v.Border = r.border;
        v.Radius = r.radius;
        v.RadiusTl = r.radius_tl;
        v.RadiusTr = r.radius_tr;
        v.RadiusBr = r.radius_br;
        v.RadiusBl = r.radius_bl;
        v.Elevation = (Elevation)r.elevation;
        v.ScrollX = (r.scroll_x != 0);
        v.ScrollY = (r.scroll_y != 0);
        v.Clip = (r.clip != 0);
        v.Opacity = r.opacity;
        v.Direction = (Direction)r.direction;
        return v;
    }
    private cui_button_opts ToNative(ButtonOpts v)
    {
        cui_button_opts r = default;
        r.variant = (cui_button_variant)CheckEnum(v.Variant);
        r.size = (cui_control_size)CheckEnum(v.Size);
        r.icon = Utf8(v.Icon);
        r.icon_right = Utf8(v.IconRight);
        r.disabled = v.Disabled;
        r.loading = v.Loading;
        r.full_width = v.FullWidth;
        r.selected = v.Selected;
        r.tooltip = Utf8(v.Tooltip);
        r.shortcut = Utf8(v.Shortcut);
        r.width = ToNative(v.Width);
        r.color = ToNative(v.Color);
        return r;
    }
    private ButtonOpts FromNative(cui_button_opts r, ButtonOpts old = default)
    {
        var v = old;
        v.Variant = (ButtonVariant)r.variant;
        v.Size = (ControlSize)r.size;
        v.Icon = Decode(r.icon, old.Icon);
        v.IconRight = Decode(r.icon_right, old.IconRight);
        v.Disabled = (r.disabled != 0);
        v.Loading = (r.loading != 0);
        v.FullWidth = (r.full_width != 0);
        v.Selected = (r.selected != 0);
        v.Tooltip = Decode(r.tooltip, old.Tooltip);
        v.Shortcut = Decode(r.shortcut, old.Shortcut);
        v.Width = FromNative(r.width, old.Width);
        v.Color = FromNative(r.color, old.Color);
        return v;
    }
    private cui_cal_event ToNative(CalEvent v)
    {
        cui_cal_event r = default;
        r.title = Utf8(v.Title);
        r.detail = Utf8(v.Detail);
        r.day = v.Day;
        r.start = v.Start;
        r.end = v.End;
        r.tone = (cui_tone)CheckEnum(v.Tone);
        return r;
    }
    private CalEvent FromNative(cui_cal_event r, CalEvent old = default)
    {
        var v = old;
        v.Title = Decode(r.title, old.Title);
        v.Detail = Decode(r.detail, old.Detail);
        v.Day = r.day;
        v.Start = r.start;
        v.End = r.end;
        v.Tone = (Tone)r.tone;
        return v;
    }
    private cui_calendar_heatmap_opts ToNative(CalendarHeatmapOpts v)
    {
        cui_calendar_heatmap_opts r = default;
        r.cell = v.Cell;
        r.color = ToNative(v.Color);
        r.format = Utf8(CheckFormat(v.Format, 1));
        r.unit = Utf8(v.Unit);
        return r;
    }
    private CalendarHeatmapOpts FromNative(cui_calendar_heatmap_opts r, CalendarHeatmapOpts old = default)
    {
        var v = old;
        v.Cell = r.cell;
        v.Color = FromNative(r.color, old.Color);
        v.Format = Decode(r.format, old.Format);
        v.Unit = Decode(r.unit, old.Unit);
        return v;
    }
    private cui_candle ToNative(Candle v)
    {
        cui_candle r = default;
        r.open = v.Open;
        r.high = v.High;
        r.low = v.Low;
        r.close = v.Close;
        r.volume = v.Volume;
        return r;
    }
    private Candle FromNative(cui_candle r, Candle old = default)
    {
        var v = old;
        v.Open = r.open;
        v.High = r.high;
        v.Low = r.low;
        v.Close = r.close;
        v.Volume = r.volume;
        return v;
    }
    private cui_candlestick_opts ToNative(CandlestickOpts v)
    {
        cui_candlestick_opts r = default;
        r.width = ToNative(v.Width);
        r.height = v.Height;
        r.labels = Strings(v.Labels.Span);
        r.format = Utf8(CheckFormat(v.Format, 1));
        r.overlays = CopySeries(v.Overlays.Span);
        r.overlay_count = (int)v.Overlays.Length;
        r.no_volume = v.NoVolume;
        r.ohlc = v.Ohlc;
        r.up = ToNative(v.Up);
        r.down = ToNative(v.Down);
        return r;
    }
    private CandlestickOpts FromNative(cui_candlestick_opts r, CandlestickOpts old = default)
    {
        var v = old;
        v.Width = FromNative(r.width, old.Width);
        v.Height = r.height;
        v.Format = Decode(r.format, old.Format);
        v.NoVolume = (r.no_volume != 0);
        v.Ohlc = (r.ohlc != 0);
        v.Up = FromNative(r.up, old.Up);
        v.Down = FromNative(r.down, old.Down);
        return v;
    }
    private cui_canvas_result ToNative(CanvasResult v)
    {
        cui_canvas_result r = default;
        r.rect = ToNative(v.Rect);
        r.hovered = v.Hovered;
        r.pressed = v.Pressed;
        r.clicked = v.Clicked;
        r.mouse = ToNative(v.Mouse);
        return r;
    }
    private CanvasResult FromNative(cui_canvas_result r, CanvasResult old = default)
    {
        var v = old;
        v.Rect = FromNative(r.rect, old.Rect);
        v.Hovered = (r.hovered != 0);
        v.Pressed = (r.pressed != 0);
        v.Clicked = (r.clicked != 0);
        v.Mouse = FromNative(r.mouse, old.Mouse);
        return v;
    }
    private cui_card_opts ToNative(CardOpts v)
    {
        cui_card_opts r = default;
        r.id = Utf8(v.Id);
        r.width = ToNative(v.Width);
        r.height = ToNative(v.Height);
        r.min_width = v.MinWidth;
        r.max_width = v.MaxWidth;
        r.pad = v.Pad;
        r.pad_x = v.PadX;
        r.pad_y = v.PadY;
        r.gap = v.Gap;
        r.dir = (cui_dir)CheckEnum(v.Dir);
        r.align = (cui_align)CheckEnum(v.Align);
        r.wrap = v.Wrap;
        r.hoverable = v.Hoverable;
        r.muted = v.Muted;
        r.selected = v.Selected;
        return r;
    }
    private CardOpts FromNative(cui_card_opts r, CardOpts old = default)
    {
        var v = old;
        v.Id = Decode(r.id, old.Id);
        v.Width = FromNative(r.width, old.Width);
        v.Height = FromNative(r.height, old.Height);
        v.MinWidth = r.min_width;
        v.MaxWidth = r.max_width;
        v.Pad = r.pad;
        v.PadX = r.pad_x;
        v.PadY = r.pad_y;
        v.Gap = r.gap;
        v.Dir = (Dir)r.dir;
        v.Align = (Align)r.align;
        v.Wrap = (r.wrap != 0);
        v.Hoverable = (r.hoverable != 0);
        v.Muted = (r.muted != 0);
        v.Selected = (r.selected != 0);
        return v;
    }
    private cui_card_stack_opts ToNative(CardStackOpts v)
    {
        cui_card_stack_opts r = default;
        r.width = ToNative(v.Width);
        r.height = v.Height;
        r.visible = v.Visible;
        return r;
    }
    private CardStackOpts FromNative(cui_card_stack_opts r, CardStackOpts old = default)
    {
        var v = old;
        v.Width = FromNative(r.width, old.Width);
        v.Height = r.height;
        v.Visible = r.visible;
        return v;
    }
    private cui_carousel_opts ToNative(CarouselOpts v)
    {
        cui_carousel_opts r = default;
        r.height = v.Height;
        r.auto_advance = v.AutoAdvance;
        r.gap = v.Gap;
        r.no_arrows = v.NoArrows;
        r.no_dots = v.NoDots;
        r.loop = v.Loop;
        return r;
    }
    private CarouselOpts FromNative(cui_carousel_opts r, CarouselOpts old = default)
    {
        var v = old;
        v.Height = r.height;
        v.AutoAdvance = r.auto_advance;
        v.Gap = r.gap;
        v.NoArrows = (r.no_arrows != 0);
        v.NoDots = (r.no_dots != 0);
        v.Loop = (r.loop != 0);
        return v;
    }
    private cui_chart_annotation ToNative(ChartAnnotation v)
    {
        cui_chart_annotation r = default;
        r.kind = (cui_annotation_kind)CheckEnum(v.Kind);
        r.x = v.X;
        r.y = v.Y;
        r.label = Utf8(v.Label);
        r.detail = Utf8(v.Detail);
        r.accent = v.Accent;
        return r;
    }
    private ChartAnnotation FromNative(cui_chart_annotation r, ChartAnnotation old = default)
    {
        var v = old;
        v.Kind = (AnnotationKind)r.kind;
        v.X = r.x;
        v.Y = r.y;
        v.Label = Decode(r.label, old.Label);
        v.Detail = Decode(r.detail, old.Detail);
        v.Accent = (r.accent != 0);
        return v;
    }
    private cui_chart_opts ToNative(ChartOpts v)
    {
        cui_chart_opts r = default;
        r.width = ToNative(v.Width);
        r.height = v.Height;
        r.labels = Strings(v.Labels.Span);
        r.label_count = (int)v.Labels.Length;
        r.area = v.Area;
        r.no_grid = v.NoGrid;
        r.no_legend = v.NoLegend;
        r.stacked = v.Stacked;
        r.smooth = v.Smooth;
        r.format = Utf8(CheckFormat(v.Format, 1));
        r.min = v.Min;
        r.max = v.Max;
        r.annotations = CopyChartAnnotation(v.Annotations.Span);
        r.annotation_count = (int)v.Annotations.Length;
        return r;
    }
    private ChartOpts FromNative(cui_chart_opts r, ChartOpts old = default)
    {
        var v = old;
        v.Width = FromNative(r.width, old.Width);
        v.Height = r.height;
        v.Area = (r.area != 0);
        v.NoGrid = (r.no_grid != 0);
        v.NoLegend = (r.no_legend != 0);
        v.Stacked = (r.stacked != 0);
        v.Smooth = (r.smooth != 0);
        v.Format = Decode(r.format, old.Format);
        v.Min = r.min;
        v.Max = r.max;
        return v;
    }
    private cui_chat_opts ToNative(ChatOpts v)
    {
        cui_chat_opts r = default;
        r.mine = v.Mine;
        r.author = Utf8(v.Author);
        r.time = Utf8(v.Time);
        r.status = Utf8(v.Status);
        r.grouped = v.Grouped;
        r.last_in_group = v.LastInGroup;
        r.avatar = Unwrap(v.Avatar);
        r.markdown = v.Markdown;
        r.markdown_result = BindMarkdownResult(v.MarkdownResult);
        return r;
    }
    private ChatOpts FromNative(cui_chat_opts r, ChatOpts old = default)
    {
        var v = old;
        v.Mine = (r.mine != 0);
        v.Author = Decode(r.author, old.Author);
        v.Time = Decode(r.time, old.Time);
        v.Status = Decode(r.status, old.Status);
        v.Grouped = (r.grouped != 0);
        v.LastInGroup = (r.last_in_group != 0);
        v.Avatar = FindTexture(r.avatar);
        v.Markdown = (r.markdown != 0);
        return v;
    }
    private cui_check_opts ToNative(CheckOpts v)
    {
        cui_check_opts r = default;
        r.description = Utf8(v.Description);
        r.disabled = v.Disabled;
        r.indeterminate = v.Indeterminate;
        return r;
    }
    private CheckOpts FromNative(cui_check_opts r, CheckOpts old = default)
    {
        var v = old;
        v.Description = Decode(r.description, old.Description);
        v.Disabled = (r.disabled != 0);
        v.Indeterminate = (r.indeterminate != 0);
        return v;
    }
    private cui_check_step ToNative(CheckStep v)
    {
        cui_check_step r = default;
        r.title = Utf8(v.Title);
        r.description = Utf8(v.Description);
        r.action = Utf8(v.Action);
        r.done = v.Done;
        return r;
    }
    private CheckStep FromNative(cui_check_step r, CheckStep old = default)
    {
        var v = old;
        v.Title = Decode(r.title, old.Title);
        v.Description = Decode(r.description, old.Description);
        v.Action = Decode(r.action, old.Action);
        v.Done = (r.done != 0);
        return v;
    }
    private cui_checklist_opts ToNative(ChecklistOpts v)
    {
        cui_checklist_opts r = default;
        r.title = Utf8(v.Title);
        r.description = Utf8(v.Description);
        return r;
    }
    private ChecklistOpts FromNative(cui_checklist_opts r, ChecklistOpts old = default)
    {
        var v = old;
        v.Title = Decode(r.title, old.Title);
        v.Description = Decode(r.description, old.Description);
        return v;
    }
    private cui_chip_opts ToNative(ChipOpts v)
    {
        cui_chip_opts r = default;
        r.icon = Utf8(v.Icon);
        r.disabled = v.Disabled;
        return r;
    }
    private ChipOpts FromNative(cui_chip_opts r, ChipOpts old = default)
    {
        var v = old;
        v.Icon = Decode(r.icon, old.Icon);
        v.Disabled = (r.disabled != 0);
        return v;
    }
    private cui_choice ToNative(Choice v)
    {
        cui_choice r = default;
        r.title = Utf8(v.Title);
        r.description = Utf8(v.Description);
        r.icon = Utf8(v.Icon);
        r.badge = Utf8(v.Badge);
        return r;
    }
    private Choice FromNative(cui_choice r, Choice old = default)
    {
        var v = old;
        v.Title = Decode(r.title, old.Title);
        v.Description = Decode(r.description, old.Description);
        v.Icon = Decode(r.icon, old.Icon);
        v.Badge = Decode(r.badge, old.Badge);
        return v;
    }
    private cui_code_opts ToNative(CodeOpts v)
    {
        cui_code_opts r = default;
        r.lang = Utf8(v.Lang);
        r.title = Utf8(v.Title);
        r.line_numbers = v.LineNumbers;
        r.highlight_from = v.HighlightFrom;
        r.highlight_to = v.HighlightTo;
        r.max_height = v.MaxHeight;
        r.size = v.Size;
        r.no_header = v.NoHeader;
        return r;
    }
    private CodeOpts FromNative(cui_code_opts r, CodeOpts old = default)
    {
        var v = old;
        v.Lang = Decode(r.lang, old.Lang);
        v.Title = Decode(r.title, old.Title);
        v.LineNumbers = (r.line_numbers != 0);
        v.HighlightFrom = r.highlight_from;
        v.HighlightTo = r.highlight_to;
        v.MaxHeight = r.max_height;
        v.Size = r.size;
        v.NoHeader = (r.no_header != 0);
        return v;
    }
    private cui_color ToNative(Color v)
    {
        cui_color r = default;
        r.r = v.R;
        r.g = v.G;
        r.b = v.B;
        r.a = v.A;
        return r;
    }
    private Color FromNative(cui_color r, Color old = default)
    {
        var v = old;
        v.R = r.r;
        v.G = r.g;
        v.B = r.b;
        v.A = r.a;
        return v;
    }
    private cui_command ToNative(Command v)
    {
        cui_command r = default;
        r.title = Utf8(v.Title);
        r.icon = Utf8(v.Icon);
        r.group = Utf8(v.Group);
        r.shortcut = Utf8(v.Shortcut);
        return r;
    }
    private Command FromNative(cui_command r, Command old = default)
    {
        var v = old;
        v.Title = Decode(r.title, old.Title);
        v.Icon = Decode(r.icon, old.Icon);
        v.Group = Decode(r.group, old.Group);
        v.Shortcut = Decode(r.shortcut, old.Shortcut);
        return v;
    }
    private cui_comment_opts ToNative(CommentOpts v)
    {
        cui_comment_opts r = default;
        r.time = Utf8(v.Time);
        r.depth = v.Depth;
        r.likes = BindInt(v.Likes);
        r.liked = BindBool(v.Liked);
        r.edited = v.Edited;
        r.badge = Utf8(v.Badge);
        return r;
    }
    private CommentOpts FromNative(cui_comment_opts r, CommentOpts old = default)
    {
        var v = old;
        v.Time = Decode(r.time, old.Time);
        v.Depth = r.depth;
        v.Edited = (r.edited != 0);
        v.Badge = Decode(r.badge, old.Badge);
        return v;
    }
    private cui_commit ToNative(Commit v)
    {
        cui_commit r = default;
        r.hash = Utf8(v.Hash);
        r.message = Utf8(v.Message);
        r.author = Utf8(v.Author);
        r.date = Utf8(v.Date);
        r.refs = Utf8(v.Refs);
        if (!v.Parent.IsEmpty && v.Parent.Length != 2) throw new ArgumentException("Commit.Parent requires 2 elements.");
        for (int i = 0; i < v.Parent.Length; i++) r.parent[i] = v.Parent.Span[i];
        return r;
    }
    private Commit FromNative(cui_commit r, Commit old = default)
    {
        var v = old;
        v.Hash = Decode(r.hash, old.Hash);
        v.Message = Decode(r.message, old.Message);
        v.Author = Decode(r.author, old.Author);
        v.Date = Decode(r.date, old.Date);
        v.Refs = Decode(r.refs, old.Refs);
        if (v.Parent.Length != 2) v.Parent = new int[2];
        for (int i = 0; i < 2; i++) v.Parent.Span[i] = r.parent[i];
        return v;
    }
    private cui_commit_graph_opts ToNative(CommitGraphOpts v)
    {
        cui_commit_graph_opts r = default;
        r.width = ToNative(v.Width);
        r.height = v.Height;
        r.selected = BindInt(v.Selected);
        return r;
    }
    private CommitGraphOpts FromNative(cui_commit_graph_opts r, CommitGraphOpts old = default)
    {
        var v = old;
        v.Width = FromNative(r.width, old.Width);
        v.Height = r.height;
        return v;
    }
    private cui_commit_graph_result ToNative(CommitGraphResult v)
    {
        cui_commit_graph_result r = default;
        r.clicked = v.Clicked;
        r.activated = v.Activated;
        r.hovered = v.Hovered;
        return r;
    }
    private CommitGraphResult FromNative(cui_commit_graph_result r, CommitGraphResult old = default)
    {
        var v = old;
        v.Clicked = r.clicked;
        v.Activated = r.activated;
        v.Hovered = r.hovered;
        return v;
    }
    private cui_compare_opts ToNative(CompareOpts v)
    {
        cui_compare_opts r = default;
        r.width = ToNative(v.Width);
        r.height = v.Height;
        r.before_label = Utf8(v.BeforeLabel);
        r.after_label = Utf8(v.AfterLabel);
        r.aspect = v.Aspect;
        return r;
    }
    private CompareOpts FromNative(cui_compare_opts r, CompareOpts old = default)
    {
        var v = old;
        v.Width = FromNative(r.width, old.Width);
        v.Height = r.height;
        v.BeforeLabel = Decode(r.before_label, old.BeforeLabel);
        v.AfterLabel = Decode(r.after_label, old.AfterLabel);
        v.Aspect = r.aspect;
        return v;
    }
    private cui_composer_opts ToNative(ComposerOpts v)
    {
        cui_composer_opts r = default;
        r.placeholder = Utf8(v.Placeholder);
        r.busy = v.Busy;
        r.attachments = Strings(v.Attachments.Span);
        r.attachment_count = (int)v.Attachments.Length;
        r.removed = BindInt(v.Removed);
        r.models = CopyModel(v.Models.Span);
        r.model_count = (int)v.Models.Length;
        r.model = BindInt(v.Model);
        r.max_lines = v.MaxLines;
        r.width = ToNative(v.Width);
        return r;
    }
    private ComposerOpts FromNative(cui_composer_opts r, ComposerOpts old = default)
    {
        var v = old;
        v.Placeholder = Decode(r.placeholder, old.Placeholder);
        v.Busy = (r.busy != 0);
        v.MaxLines = r.max_lines;
        v.Width = FromNative(r.width, old.Width);
        return v;
    }
    private cui_confirm_opts ToNative(ConfirmOpts v)
    {
        cui_confirm_opts r = default;
        r.confirm_label = Utf8(v.ConfirmLabel);
        r.icon = Utf8(v.Icon);
        r.variant = (cui_button_variant)CheckEnum(v.Variant);
        r.size = (cui_control_size)CheckEnum(v.Size);
        r.timeout = v.Timeout;
        return r;
    }
    private ConfirmOpts FromNative(cui_confirm_opts r, ConfirmOpts old = default)
    {
        var v = old;
        v.ConfirmLabel = Decode(r.confirm_label, old.ConfirmLabel);
        v.Icon = Decode(r.icon, old.Icon);
        v.Variant = (ButtonVariant)r.variant;
        v.Size = (ControlSize)r.size;
        v.Timeout = r.timeout;
        return v;
    }
    private cui_confirm_popover_opts ToNative(ConfirmPopoverOpts v)
    {
        cui_confirm_popover_opts r = default;
        r.title = Utf8(v.Title);
        r.description = Utf8(v.Description);
        r.confirm_label = Utf8(v.ConfirmLabel);
        r.cancel_label = Utf8(v.CancelLabel);
        r.danger = v.Danger;
        r.placement = (cui_placement)CheckEnum(v.Placement);
        return r;
    }
    private ConfirmPopoverOpts FromNative(cui_confirm_popover_opts r, ConfirmPopoverOpts old = default)
    {
        var v = old;
        v.Title = Decode(r.title, old.Title);
        v.Description = Decode(r.description, old.Description);
        v.ConfirmLabel = Decode(r.confirm_label, old.ConfirmLabel);
        v.CancelLabel = Decode(r.cancel_label, old.CancelLabel);
        v.Danger = (r.danger != 0);
        v.Placement = (Placement)r.placement;
        return v;
    }
    private cui_context_opts ToNative(ContextOpts v)
    {
        cui_context_opts r = default;
        r.parts = CopyVizItem(v.Parts.Span);
        r.part_count = (int)v.Parts.Length;
        r.unit = Utf8(v.Unit);
        return r;
    }
    private ContextOpts FromNative(cui_context_opts r, ContextOpts old = default)
    {
        var v = old;
        v.Unit = Decode(r.unit, old.Unit);
        return v;
    }
    private cui_corners ToNative(Corners v)
    {
        cui_corners r = default;
        r.tl = v.Tl;
        r.tr = v.Tr;
        r.br = v.Br;
        r.bl = v.Bl;
        return r;
    }
    private Corners FromNative(cui_corners r, Corners old = default)
    {
        var v = old;
        v.Tl = r.tl;
        v.Tr = r.tr;
        v.Br = r.br;
        v.Bl = r.bl;
        return v;
    }
    private cui_countdown_opts ToNative(CountdownOpts v)
    {
        cui_countdown_opts r = default;
        r.hide_days = v.HideDays;
        r.compact = v.Compact;
        return r;
    }
    private CountdownOpts FromNative(cui_countdown_opts r, CountdownOpts old = default)
    {
        var v = old;
        v.HideDays = (r.hide_days != 0);
        v.Compact = (r.compact != 0);
        return v;
    }
    private cui_counter_opts ToNative(CounterOpts v)
    {
        cui_counter_opts r = default;
        r.format = Utf8(CheckFormat(v.Format, 1));
        r.size = v.Size;
        r.font = (int)CheckEnum(v.Font);
        r.color = ToNative(v.Color);
        r.group_thousands = v.GroupThousands;
        return r;
    }
    private CounterOpts FromNative(cui_counter_opts r, CounterOpts old = default)
    {
        var v = old;
        v.Format = Decode(r.format, old.Format);
        v.Size = r.size;
        v.Font = (Font)r.font;
        v.Color = FromNative(r.color, old.Color);
        v.GroupThousands = (r.group_thousands != 0);
        return v;
    }
    private cui_coverage_file ToNative(CoverageFile v)
    {
        cui_coverage_file r = default;
        r.path = Utf8(v.Path);
        r.lines = v.Lines;
        r.lines_hit = v.LinesHit;
        r.branches = v.Branches;
        r.branches_hit = v.BranchesHit;
        r.functions = v.Functions;
        r.functions_hit = v.FunctionsHit;
        return r;
    }
    private CoverageFile FromNative(cui_coverage_file r, CoverageFile old = default)
    {
        var v = old;
        v.Path = Decode(r.path, old.Path);
        v.Lines = r.lines;
        v.LinesHit = r.lines_hit;
        v.Branches = r.branches;
        v.BranchesHit = r.branches_hit;
        v.Functions = r.functions;
        v.FunctionsHit = r.functions_hit;
        return v;
    }
    private cui_coverage_opts ToNative(CoverageOpts v)
    {
        cui_coverage_opts r = default;
        r.width = ToNative(v.Width);
        r.height = v.Height;
        r.low = v.Low;
        r.high = v.High;
        return r;
    }
    private CoverageOpts FromNative(cui_coverage_opts r, CoverageOpts old = default)
    {
        var v = old;
        v.Width = FromNative(r.width, old.Width);
        v.Height = r.height;
        v.Low = r.low;
        v.High = r.high;
        return v;
    }
    private cui_coverage_result ToNative(CoverageResult v)
    {
        cui_coverage_result r = default;
        r.activated = v.Activated;
        r.lines = v.Lines;
        return r;
    }
    private CoverageResult FromNative(cui_coverage_result r, CoverageResult old = default)
    {
        var v = old;
        v.Activated = r.activated;
        v.Lines = r.lines;
        return v;
    }
    private cui_cropper_opts ToNative(CropperOpts v)
    {
        cui_cropper_opts r = default;
        r.width = ToNative(v.Width);
        r.height = ToNative(v.Height);
        r.aspect = v.Aspect;
        r.round = v.Round;
        r.min_size = v.MinSize;
        return r;
    }
    private CropperOpts FromNative(cui_cropper_opts r, CropperOpts old = default)
    {
        var v = old;
        v.Width = FromNative(r.width, old.Width);
        v.Height = FromNative(r.height, old.Height);
        v.Aspect = r.aspect;
        v.Round = (r.round != 0);
        v.MinSize = r.min_size;
        return v;
    }
    private cui_date ToNative(Date v)
    {
        Validate(v);
        cui_date r = default;
        r.year = v.Year;
        r.month = v.Month;
        r.day = v.Day;
        return r;
    }
    private Date FromNative(cui_date r, Date old = default)
    {
        var v = old;
        v.Year = r.year;
        v.Month = r.month;
        v.Day = r.day;
        return v;
    }
    private cui_date_opts ToNative(DateOpts v)
    {
        cui_date_opts r = default;
        r.label = Utf8(v.Label);
        r.placeholder = Utf8(v.Placeholder);
        r.width = ToNative(v.Width);
        return r;
    }
    private DateOpts FromNative(cui_date_opts r, DateOpts old = default)
    {
        var v = old;
        v.Label = Decode(r.label, old.Label);
        v.Placeholder = Decode(r.placeholder, old.Placeholder);
        v.Width = FromNative(r.width, old.Width);
        return v;
    }
    private cui_date_range ToNative(DateRange v)
    {
        cui_date_range r = default;
        r.start = ToNative(v.Start);
        r.end = ToNative(v.End);
        return r;
    }
    private DateRange FromNative(cui_date_range r, DateRange old = default)
    {
        var v = old;
        v.Start = FromNative(r.start, old.Start);
        v.End = FromNative(r.end, old.End);
        return v;
    }
    private cui_date_range_opts ToNative(DateRangeOpts v)
    {
        cui_date_range_opts r = default;
        r.label = Utf8(v.Label);
        r.placeholder = Utf8(v.Placeholder);
        r.width = ToNative(v.Width);
        r.no_presets = v.NoPresets;
        return r;
    }
    private DateRangeOpts FromNative(cui_date_range_opts r, DateRangeOpts old = default)
    {
        var v = old;
        v.Label = Decode(r.label, old.Label);
        v.Placeholder = Decode(r.placeholder, old.Placeholder);
        v.Width = FromNative(r.width, old.Width);
        v.NoPresets = (r.no_presets != 0);
        return v;
    }
    private cui_diagnostic ToNative(Diagnostic v)
    {
        cui_diagnostic r = default;
        r.severity = (cui_severity)CheckEnum(v.Severity);
        r.message = Utf8(v.Message);
        r.file = Utf8(v.File);
        r.line = v.Line;
        r.column = v.Column;
        r.source = Utf8(v.Source);
        r.code = Utf8(v.Code);
        return r;
    }
    private Diagnostic FromNative(cui_diagnostic r, Diagnostic old = default)
    {
        var v = old;
        v.Severity = (Severity)r.severity;
        v.Message = Decode(r.message, old.Message);
        v.File = Decode(r.file, old.File);
        v.Line = r.line;
        v.Column = r.column;
        v.Source = Decode(r.source, old.Source);
        v.Code = Decode(r.code, old.Code);
        return v;
    }
    private cui_dialog_opts ToNative(DialogOpts v)
    {
        cui_dialog_opts r = default;
        r.description = Utf8(v.Description);
        r.width = v.Width;
        r.no_close_button = v.NoCloseButton;
        r.persistent = v.Persistent;
        return r;
    }
    private DialogOpts FromNative(cui_dialog_opts r, DialogOpts old = default)
    {
        var v = old;
        v.Description = Decode(r.description, old.Description);
        v.Width = r.width;
        v.NoCloseButton = (r.no_close_button != 0);
        v.Persistent = (r.persistent != 0);
        return v;
    }
    private cui_dock_item_opts ToNative(DockItemOpts v)
    {
        cui_dock_item_opts r = default;
        r.color = ToNative(v.Color);
        r.running = v.Running;
        r.badge = v.Badge;
        return r;
    }
    private DockItemOpts FromNative(cui_dock_item_opts r, DockItemOpts old = default)
    {
        var v = old;
        v.Color = FromNative(r.color, old.Color);
        v.Running = (r.running != 0);
        v.Badge = r.badge;
        return v;
    }
    private cui_dock_opts ToNative(DockOpts v)
    {
        cui_dock_opts r = default;
        r.size = v.Size;
        r.magnify = v.Magnify;
        return r;
    }
    private DockOpts FromNative(cui_dock_opts r, DockOpts old = default)
    {
        var v = old;
        v.Size = r.size;
        v.Magnify = r.magnify;
        return v;
    }
    private cui_dot_chart_opts ToNative(DotChartOpts v)
    {
        cui_dot_chart_opts r = default;
        r.width = ToNative(v.Width);
        r.height = v.Height;
        r.x_min = v.XMin;
        r.x_max = v.XMax;
        r.x_units = (cui_axis_units)CheckEnum(v.XUnits);
        r.x_format = Utf8(CheckFormat(v.XFormat, 1));
        r.x_step = v.XStep;
        r.min = v.Min;
        r.max = v.Max;
        r.format = Utf8(CheckFormat(v.Format, 1));
        r.unit = Utf8(v.Unit);
        r.pitch = v.Pitch;
        r.no_grid = v.NoGrid;
        r.no_axis = v.NoAxis;
        r.no_readout = v.NoReadout;
        r.annotations = CopyChartAnnotation(v.Annotations.Span);
        r.annotation_count = (int)v.Annotations.Length;
        return r;
    }
    private DotChartOpts FromNative(cui_dot_chart_opts r, DotChartOpts old = default)
    {
        var v = old;
        v.Width = FromNative(r.width, old.Width);
        v.Height = r.height;
        v.XMin = r.x_min;
        v.XMax = r.x_max;
        v.XUnits = (AxisUnits)r.x_units;
        v.XFormat = Decode(r.x_format, old.XFormat);
        v.XStep = r.x_step;
        v.Min = r.min;
        v.Max = r.max;
        v.Format = Decode(r.format, old.Format);
        v.Unit = Decode(r.unit, old.Unit);
        v.Pitch = r.pitch;
        v.NoGrid = (r.no_grid != 0);
        v.NoAxis = (r.no_axis != 0);
        v.NoReadout = (r.no_readout != 0);
        return v;
    }
    private cui_dot_chart_result ToNative(DotChartResult v)
    {
        cui_dot_chart_result r = default;
        r.hovered = v.Hovered;
        r.x = v.X;
        r.value = v.Value;
        r.clicked = v.Clicked;
        return r;
    }
    private DotChartResult FromNative(cui_dot_chart_result r, DotChartResult old = default)
    {
        var v = old;
        v.Hovered = r.hovered;
        v.X = r.x;
        v.Value = r.value;
        v.Clicked = (r.clicked != 0);
        return v;
    }
    private cui_dot_display_opts ToNative(DotDisplayOpts v)
    {
        cui_dot_display_opts r = default;
        r.pitch = v.Pitch;
        r.color = ToNative(v.Color);
        r.accent = v.Accent;
        r.no_dim = v.NoDim;
        r.marquee = v.Marquee;
        r.align = (cui_text_align)CheckEnum(v.Align);
        r.width = ToNative(v.Width);
        return r;
    }
    private DotDisplayOpts FromNative(cui_dot_display_opts r, DotDisplayOpts old = default)
    {
        var v = old;
        v.Pitch = r.pitch;
        v.Color = FromNative(r.color, old.Color);
        v.Accent = (r.accent != 0);
        v.NoDim = (r.no_dim != 0);
        v.Marquee = (r.marquee != 0);
        v.Align = (TextAlign)r.align;
        v.Width = FromNative(r.width, old.Width);
        return v;
    }
    private cui_dot_series ToNative(DotSeries v)
    {
        cui_dot_series r = default;
        r.values = CopyFloat(v.Values.Span);
        r.name = Utf8(v.Name);
        r.accent = v.Accent;
        return r;
    }
    private DotSeries FromNative(cui_dot_series r, DotSeries old = default)
    {
        var v = old;
        v.Name = Decode(r.name, old.Name);
        v.Accent = (r.accent != 0);
        return v;
    }
    private cui_dropzone_opts ToNative(DropzoneOpts v)
    {
        cui_dropzone_opts r = default;
        r.title = Utf8(v.Title);
        r.description = Utf8(v.Description);
        r.icon = Utf8(v.Icon);
        r.accept = Utf8(v.Accept);
        r.height = v.Height;
        r.width = ToNative(v.Width);
        r.compact = v.Compact;
        return r;
    }
    private DropzoneOpts FromNative(cui_dropzone_opts r, DropzoneOpts old = default)
    {
        var v = old;
        v.Title = Decode(r.title, old.Title);
        v.Description = Decode(r.description, old.Description);
        v.Icon = Decode(r.icon, old.Icon);
        v.Accept = Decode(r.accept, old.Accept);
        v.Height = r.height;
        v.Width = FromNative(r.width, old.Width);
        v.Compact = (r.compact != 0);
        return v;
    }
    private cui_emoji_picker_opts ToNative(EmojiPickerOpts v)
    {
        Validate(v);
        cui_emoji_picker_opts r = default;
        r.width = ToNative(v.Width);
        r.height = v.Height;
        r.size = v.Size;
        r.recent = Use(v.Recent);
        r.recent_cap = (ulong)(v.Recent?.Capacity ?? 0);
        r.tone = BindInt(v.Tone);
        r.placeholder = Utf8(v.Placeholder);
        r.empty_text = Utf8(v.EmptyText);
        r.hint = Utf8(v.Hint);
        r.group_names = Strings(v.GroupNames.Span);
        r.no_search = v.NoSearch;
        r.no_preview = v.NoPreview;
        r.no_tone = v.NoTone;
        return r;
    }
    private EmojiPickerOpts FromNative(cui_emoji_picker_opts r, EmojiPickerOpts old = default)
    {
        var v = old;
        v.Width = FromNative(r.width, old.Width);
        v.Height = r.height;
        v.Size = r.size;
        v.Placeholder = Decode(r.placeholder, old.Placeholder);
        v.EmptyText = Decode(r.empty_text, old.EmptyText);
        v.Hint = Decode(r.hint, old.Hint);
        v.NoSearch = (r.no_search != 0);
        v.NoPreview = (r.no_preview != 0);
        v.NoTone = (r.no_tone != 0);
        return v;
    }
    private cui_empty_opts ToNative(EmptyOpts v)
    {
        cui_empty_opts r = default;
        r.icon = Utf8(v.Icon);
        r.description = Utf8(v.Description);
        r.action = Utf8(v.Action);
        return r;
    }
    private EmptyOpts FromNative(cui_empty_opts r, EmptyOpts old = default)
    {
        var v = old;
        v.Icon = Decode(r.icon, old.Icon);
        v.Description = Decode(r.description, old.Description);
        v.Action = Decode(r.action, old.Action);
        return v;
    }
    private cui_event_track_opts ToNative(EventTrackOpts v)
    {
        cui_event_track_opts r = default;
        r.width = ToNative(v.Width);
        r.step = v.Step;
        r.units = (cui_axis_units)CheckEnum(v.Units);
        r.format = Utf8(CheckFormat(v.Format, 1));
        r.label_step = v.LabelStep;
        r.selected = BindInt(v.Selected);
        r.no_add = v.NoAdd;
        r.no_remove = v.NoRemove;
        r.no_axis = v.NoAxis;
        return r;
    }
    private EventTrackOpts FromNative(cui_event_track_opts r, EventTrackOpts old = default)
    {
        var v = old;
        v.Width = FromNative(r.width, old.Width);
        v.Step = r.step;
        v.Units = (AxisUnits)r.units;
        v.Format = Decode(r.format, old.Format);
        v.LabelStep = r.label_step;
        v.NoAdd = (r.no_add != 0);
        v.NoRemove = (r.no_remove != 0);
        v.NoAxis = (r.no_axis != 0);
        return v;
    }
    private cui_event_track_result ToNative(EventTrackResult v)
    {
        cui_event_track_result r = default;
        r.changed = v.Changed;
        r.added = v.Added;
        r.removed = v.Removed;
        r.selected = v.Selected;
        r.dragging = v.Dragging;
        return r;
    }
    private EventTrackResult FromNative(cui_event_track_result r, EventTrackResult old = default)
    {
        var v = old;
        v.Changed = (r.changed != 0);
        v.Added = r.added;
        v.Removed = r.removed;
        v.Selected = r.selected;
        v.Dragging = (r.dragging != 0);
        return v;
    }
    private cui_expandable_card_opts ToNative(ExpandableCardOpts v)
    {
        cui_expandable_card_opts r = default;
        r.description = Utf8(v.Description);
        r.icon = Utf8(v.Icon);
        r.badge = Utf8(v.Badge);
        r.badge_tone = (cui_tone)CheckEnum(v.BadgeTone);
        return r;
    }
    private ExpandableCardOpts FromNative(cui_expandable_card_opts r, ExpandableCardOpts old = default)
    {
        var v = old;
        v.Description = Decode(r.description, old.Description);
        v.Icon = Decode(r.icon, old.Icon);
        v.Badge = Decode(r.badge, old.Badge);
        v.BadgeTone = (Tone)r.badge_tone;
        return v;
    }
    private cui_expandable_text_opts ToNative(ExpandableTextOpts v)
    {
        cui_expandable_text_opts r = default;
        r.lines = v.Lines;
        r.size = v.Size;
        r.color = ToNative(v.Color);
        r.fade_to = ToNative(v.FadeTo);
        return r;
    }
    private ExpandableTextOpts FromNative(cui_expandable_text_opts r, ExpandableTextOpts old = default)
    {
        var v = old;
        v.Lines = r.lines;
        v.Size = r.size;
        v.Color = FromNative(r.color, old.Color);
        v.FadeTo = FromNative(r.fade_to, old.FadeTo);
        return v;
    }
    private cui_fader_opts ToNative(FaderOpts v)
    {
        cui_fader_opts r = default;
        r.label = Utf8(v.Label);
        r.format = Utf8(CheckFormat(v.Format, 1));
        r.unit = Utf8(v.Unit);
        r.length = v.Length;
        r.horizontal = v.Horizontal;
        r.step = v.Step;
        r.reset = v.Reset;
        r.ticks = CopyFloat(v.Ticks.Span);
        r.tick_count = (int)v.Ticks.Length;
        r.meter = CopyFloat(v.Meter.Span);
        r.meter_channels = (int)v.Meter.Length;
        r.hide_value = v.HideValue;
        r.disabled = v.Disabled;
        return r;
    }
    private FaderOpts FromNative(cui_fader_opts r, FaderOpts old = default)
    {
        var v = old;
        v.Label = Decode(r.label, old.Label);
        v.Format = Decode(r.format, old.Format);
        v.Unit = Decode(r.unit, old.Unit);
        v.Length = r.length;
        v.Horizontal = (r.horizontal != 0);
        v.Step = r.step;
        v.Reset = r.reset;
        v.HideValue = (r.hide_value != 0);
        v.Disabled = (r.disabled != 0);
        return v;
    }
    private cui_feature_flag ToNative(FeatureFlag v)
    {
        cui_feature_flag r = default;
        r.key = Utf8(v.Key);
        r.description = Utf8(v.Description);
        if (!v.On.IsEmpty && v.On.Length != CUI_FLAG_ENVS) throw new ArgumentException("FeatureFlag.On requires CUI_FLAG_ENVS elements.");
        for (int i = 0; i < v.On.Length; i++) r.on[i] = v.On.Span[i];
        if (!v.Rollout.IsEmpty && v.Rollout.Length != CUI_FLAG_ENVS) throw new ArgumentException("FeatureFlag.Rollout requires CUI_FLAG_ENVS elements.");
        for (int i = 0; i < v.Rollout.Length; i++) r.rollout[i] = v.Rollout.Span[i];
        return r;
    }
    private FeatureFlag FromNative(cui_feature_flag r, FeatureFlag old = default)
    {
        var v = old;
        v.Key = Decode(r.key, old.Key);
        v.Description = Decode(r.description, old.Description);
        if (v.On.Length != CUI_FLAG_ENVS) v.On = new bool[CUI_FLAG_ENVS];
        for (int i = 0; i < CUI_FLAG_ENVS; i++) v.On.Span[i] = (r.on[i] != 0);
        if (v.Rollout.Length != CUI_FLAG_ENVS) v.Rollout = new int[CUI_FLAG_ENVS];
        for (int i = 0; i < CUI_FLAG_ENVS; i++) v.Rollout.Span[i] = r.rollout[i];
        return v;
    }
    private cui_feature_flags_opts ToNative(FeatureFlagsOpts v)
    {
        cui_feature_flags_opts r = default;
        r.width = ToNative(v.Width);
        r.height = v.Height;
        return r;
    }
    private FeatureFlagsOpts FromNative(cui_feature_flags_opts r, FeatureFlagsOpts old = default)
    {
        var v = old;
        v.Width = FromNative(r.width, old.Width);
        v.Height = r.height;
        return v;
    }
    private cui_feature_flags_result ToNative(FeatureFlagsResult v)
    {
        cui_feature_flags_result r = default;
        r.flag = v.Flag;
        r.env = v.Env;
        return r;
    }
    private FeatureFlagsResult FromNative(cui_feature_flags_result r, FeatureFlagsResult old = default)
    {
        var v = old;
        v.Flag = r.flag;
        v.Env = r.env;
        return v;
    }
    private cui_file_browser_opts ToNative(FileBrowserOpts v)
    {
        Validate(v);
        cui_file_browser_opts r = default;
        r.width = ToNative(v.Width);
        r.height = ToNative(v.Height);
        r.filter = Utf8(v.Filter);
        r.folders_only = v.FoldersOnly;
        r.hidden = v.Hidden;
        r.no_places = v.NoPlaces;
        r.places = Strings(v.Places.Span);
        r.place_count = (int)v.Places.Length;
        if (v.List is not null) { r.list = &FileListThunk; r.list_user = RootFileList(v.List); }
        r.max_entries = v.MaxEntries;
        return r;
    }
    private FileBrowserOpts FromNative(cui_file_browser_opts r, FileBrowserOpts old = default)
    {
        var v = old;
        v.Width = FromNative(r.width, old.Width);
        v.Height = FromNative(r.height, old.Height);
        v.Filter = Decode(r.filter, old.Filter);
        v.FoldersOnly = (r.folders_only != 0);
        v.Hidden = (r.hidden != 0);
        v.NoPlaces = (r.no_places != 0);
        v.MaxEntries = r.max_entries;
        return v;
    }
    private cui_file_browser_result ToNative(FileBrowserResult v)
    {
        cui_file_browser_result r = default;
        r.picked = Utf8(v.Picked);
        r.selected = Utf8(v.Selected);
        r.navigated = v.Navigated;
        r.error = v.Error;
        return r;
    }
    private FileBrowserResult FromNative(cui_file_browser_result r, FileBrowserResult old = default)
    {
        var v = old;
        v.Picked = Decode(r.picked, old.Picked);
        v.Selected = Decode(r.selected, old.Selected);
        v.Navigated = (r.navigated != 0);
        v.Error = (r.error != 0);
        return v;
    }
    private cui_file_entry ToNative(FileEntry v)
    {
        cui_file_entry r = default;
        r.name = Utf8(v.Name);
        r.dir = v.Dir;
        r.size = v.Size;
        r.modified = v.Modified;
        return r;
    }
    private FileEntry FromNative(cui_file_entry r, FileEntry old = default)
    {
        var v = old;
        v.Name = Decode(r.name, old.Name);
        v.Dir = (r.dir != 0);
        v.Size = r.size;
        v.Modified = r.modified;
        return v;
    }
    private cui_file_picker_opts ToNative(FilePickerOpts v)
    {
        cui_file_picker_opts r = default;
        r.title = Utf8(v.Title);
        r.save = v.Save;
        r.folders = v.Folders;
        r.start = Utf8(v.Start);
        r.name = Utf8(v.Name);
        r.filter = Utf8(v.Filter);
        r.filter_label = Utf8(v.FilterLabel);
        if (v.List is not null) { r.list = &FileListThunk; r.list_user = RootFileList(v.List); }
        return r;
    }
    private FilePickerOpts FromNative(cui_file_picker_opts r, FilePickerOpts old = default)
    {
        var v = old;
        v.Title = Decode(r.title, old.Title);
        v.Save = (r.save != 0);
        v.Folders = (r.folders != 0);
        v.Start = Decode(r.start, old.Start);
        v.Name = Decode(r.name, old.Name);
        v.Filter = Decode(r.filter, old.Filter);
        v.FilterLabel = Decode(r.filter_label, old.FilterLabel);
        return v;
    }
    private cui_filter_field ToNative(FilterField v)
    {
        cui_filter_field r = default;
        r.name = Utf8(v.Name);
        r.icon = Utf8(v.Icon);
        r.options = Strings(v.Options.Span);
        r.option_count = (int)v.Options.Length;
        return r;
    }
    private FilterField FromNative(cui_filter_field r, FilterField old = default)
    {
        var v = old;
        v.Name = Decode(r.name, old.Name);
        v.Icon = Decode(r.icon, old.Icon);
        return v;
    }
    private cui_funnel_opts ToNative(FunnelOpts v)
    {
        cui_funnel_opts r = default;
        r.width = ToNative(v.Width);
        r.height = v.Height;
        r.format = Utf8(CheckFormat(v.Format, 1));
        r.color = ToNative(v.Color);
        return r;
    }
    private FunnelOpts FromNative(cui_funnel_opts r, FunnelOpts old = default)
    {
        var v = old;
        v.Width = FromNative(r.width, old.Width);
        v.Height = r.height;
        v.Format = Decode(r.format, old.Format);
        v.Color = FromNative(r.color, old.Color);
        return v;
    }
    private cui_gantt_opts ToNative(GanttOpts v)
    {
        cui_gantt_opts r = default;
        r.day_width = v.DayWidth;
        r.row_height = v.RowHeight;
        r.height = ToNative(v.Height);
        r.today = v.Today;
        r.read_only = v.ReadOnly;
        return r;
    }
    private GanttOpts FromNative(cui_gantt_opts r, GanttOpts old = default)
    {
        var v = old;
        v.DayWidth = r.day_width;
        v.RowHeight = r.row_height;
        v.Height = FromNative(r.height, old.Height);
        v.Today = r.today;
        v.ReadOnly = (r.read_only != 0);
        return v;
    }
    private cui_gantt_task ToNative(GanttTask v)
    {
        cui_gantt_task r = default;
        r.name = Utf8(v.Name);
        r.owner = Utf8(v.Owner);
        r.start = v.Start;
        r.end = v.End;
        r.progress = v.Progress;
        r.tone = (cui_tone)CheckEnum(v.Tone);
        r.depends_on = v.DependsOn;
        r.milestone = v.Milestone;
        return r;
    }
    private GanttTask FromNative(cui_gantt_task r, GanttTask old = default)
    {
        var v = old;
        v.Name = Decode(r.name, old.Name);
        v.Owner = Decode(r.owner, old.Owner);
        v.Start = r.start;
        v.End = r.end;
        v.Progress = r.progress;
        v.Tone = (Tone)r.tone;
        v.DependsOn = r.depends_on;
        v.Milestone = (r.milestone != 0);
        return v;
    }
    private cui_gauge_band ToNative(GaugeBand v)
    {
        cui_gauge_band r = default;
        r.to = v.To;
        r.tone = (cui_tone)CheckEnum(v.Tone);
        return r;
    }
    private GaugeBand FromNative(cui_gauge_band r, GaugeBand old = default)
    {
        var v = old;
        v.To = r.to;
        v.Tone = (Tone)r.tone;
        return v;
    }
    private cui_gradient_text_opts ToNative(GradientTextOpts v)
    {
        cui_gradient_text_opts r = default;
        r.font = (int)CheckEnum(v.Font);
        r.size = v.Size;
        r.from = ToNative(v.From);
        r.to = ToNative(v.To);
        r.animate = v.Animate;
        r.width = ToNative(v.Width);
        return r;
    }
    private GradientTextOpts FromNative(cui_gradient_text_opts r, GradientTextOpts old = default)
    {
        var v = old;
        v.Font = (Font)r.font;
        v.Size = r.size;
        v.From = FromNative(r.from, old.From);
        v.To = FromNative(r.to, old.To);
        v.Animate = (r.animate != 0);
        v.Width = FromNative(r.width, old.Width);
        return v;
    }
    private cui_graph_edge ToNative(GraphEdge v)
    {
        cui_graph_edge r = default;
        r.from = v.From;
        r.to = v.To;
        return r;
    }
    private GraphEdge FromNative(cui_graph_edge r, GraphEdge old = default)
    {
        var v = old;
        v.From = r.from;
        v.To = r.to;
        return v;
    }
    private cui_graph_node ToNative(GraphNode v)
    {
        cui_graph_node r = default;
        r.label = Utf8(v.Label);
        r.detail = Utf8(v.Detail);
        r.tone = (cui_tone)CheckEnum(v.Tone);
        return r;
    }
    private GraphNode FromNative(cui_graph_node r, GraphNode old = default)
    {
        var v = old;
        v.Label = Decode(r.label, old.Label);
        v.Detail = Decode(r.detail, old.Detail);
        v.Tone = (Tone)r.tone;
        return v;
    }
    private cui_graph_opts ToNative(GraphOpts v)
    {
        cui_graph_opts r = default;
        r.width = ToNative(v.Width);
        r.height = v.Height;
        r.top_down = v.TopDown;
        r.selected = BindInt(v.Selected);
        return r;
    }
    private GraphOpts FromNative(cui_graph_opts r, GraphOpts old = default)
    {
        var v = old;
        v.Width = FromNative(r.width, old.Width);
        v.Height = r.height;
        v.TopDown = (r.top_down != 0);
        return v;
    }
    private cui_graph_result ToNative(GraphResult v)
    {
        cui_graph_result r = default;
        r.clicked = v.Clicked;
        r.activated = v.Activated;
        r.hovered = v.Hovered;
        return r;
    }
    private GraphResult FromNative(cui_graph_result r, GraphResult old = default)
    {
        var v = old;
        v.Clicked = r.clicked;
        v.Activated = r.activated;
        v.Hovered = r.hovered;
        return v;
    }
    private cui_grid_column ToNative(GridColumn v)
    {
        cui_grid_column r = default;
        r.title = Utf8(v.Title);
        r.width = v.Width;
        r.align = (cui_text_align)CheckEnum(v.Align);
        r.sortable = v.Sortable;
        r.@fixed = v.Fixed;
        r.priority = v.Priority;
        return r;
    }
    private GridColumn FromNative(cui_grid_column r, GridColumn old = default)
    {
        var v = old;
        v.Title = Decode(r.title, old.Title);
        v.Width = r.width;
        v.Align = (TextAlign)r.align;
        v.Sortable = (r.sortable != 0);
        v.Fixed = (r.@fixed != 0);
        v.Priority = r.priority;
        return v;
    }
    private cui_grid_opts ToNative(GridOpts v)
    {
        cui_grid_opts r = default;
        r.width = ToNative(v.Width);
        r.height = ToNative(v.Height);
        r.row_height = v.RowHeight;
        r.selected = CopyBool(v.Selected.Span);
        JournalBool(v.Selected, r.selected);
        r.sort = BindTableSort(v.Sort);
        r.striped = v.Striped;
        r.cursor = BindInt(v.Cursor);
        r.hide_checkboxes = v.HideCheckboxes;
        r.anchor = BindInt(v.Anchor);
        return r;
    }
    private GridOpts FromNative(cui_grid_opts r, GridOpts old = default)
    {
        var v = old;
        v.Width = FromNative(r.width, old.Width);
        v.Height = FromNative(r.height, old.Height);
        v.RowHeight = r.row_height;
        v.Striped = (r.striped != 0);
        v.HideCheckboxes = (r.hide_checkboxes != 0);
        return v;
    }
    private cui_heatmap_opts ToNative(HeatmapOpts v)
    {
        cui_heatmap_opts r = default;
        r.row_labels = OptionalStrings(v.RowLabels.Span);
        r.col_labels = OptionalStrings(v.ColLabels.Span);
        r.cell = v.Cell;
        r.min = v.Min;
        r.max = v.Max;
        r.color = ToNative(v.Color);
        r.format = Utf8(CheckFormat(v.Format, 1));
        r.width = ToNative(v.Width);
        return r;
    }
    private HeatmapOpts FromNative(cui_heatmap_opts r, HeatmapOpts old = default)
    {
        var v = old;
        v.Cell = r.cell;
        v.Min = r.min;
        v.Max = r.max;
        v.Color = FromNative(r.color, old.Color);
        v.Format = Decode(r.format, old.Format);
        v.Width = FromNative(r.width, old.Width);
        return v;
    }
    private cui_histogram_opts ToNative(HistogramOpts v)
    {
        cui_histogram_opts r = default;
        r.height = v.Height;
        r.format = Utf8(CheckFormat(v.Format, 1));
        return r;
    }
    private HistogramOpts FromNative(cui_histogram_opts r, HistogramOpts old = default)
    {
        var v = old;
        v.Height = r.height;
        v.Format = Decode(r.format, old.Format);
        return v;
    }
    private cui_histogram_series ToNative(HistogramSeries v)
    {
        cui_histogram_series r = default;
        r.name = Utf8(v.Name);
        r.counts = CopyFloat(v.Counts.Span);
        r.tone = (cui_tone)CheckEnum(v.Tone);
        return r;
    }
    private HistogramSeries FromNative(cui_histogram_series r, HistogramSeries old = default)
    {
        var v = old;
        v.Name = Decode(r.name, old.Name);
        v.Tone = (Tone)r.tone;
        return v;
    }
    private cui_hold_opts ToNative(HoldOpts v)
    {
        cui_hold_opts r = default;
        r.duration = v.Duration;
        r.icon = Utf8(v.Icon);
        r.variant = (cui_button_variant)CheckEnum(v.Variant);
        return r;
    }
    private HoldOpts FromNative(cui_hold_opts r, HoldOpts old = default)
    {
        var v = old;
        v.Duration = r.duration;
        v.Icon = Decode(r.icon, old.Icon);
        v.Variant = (ButtonVariant)r.variant;
        return v;
    }
    private cui_hover_card_opts ToNative(HoverCardOpts v)
    {
        cui_hover_card_opts r = default;
        r.placement = (cui_placement)CheckEnum(v.Placement);
        r.width = v.Width;
        r.delay = v.Delay;
        return r;
    }
    private HoverCardOpts FromNative(cui_hover_card_opts r, HoverCardOpts old = default)
    {
        var v = old;
        v.Placement = (Placement)r.placement;
        v.Width = r.width;
        v.Delay = r.delay;
        return v;
    }
    private cui_image_opts ToNative(ImageOpts v)
    {
        cui_image_opts r = default;
        r.width = ToNative(v.Width);
        r.height = ToNative(v.Height);
        r.radius = v.Radius;
        r.tint = ToNative(v.Tint);
        return r;
    }
    private ImageOpts FromNative(cui_image_opts r, ImageOpts old = default)
    {
        var v = old;
        v.Width = FromNative(r.width, old.Width);
        v.Height = FromNative(r.height, old.Height);
        v.Radius = r.radius;
        v.Tint = FromNative(r.tint, old.Tint);
        return v;
    }
    private cui_image_view ToNative(ImageView v)
    {
        cui_image_view r = default;
        r.zoom = v.Zoom;
        r.cx = v.Cx;
        r.cy = v.Cy;
        return r;
    }
    private ImageView FromNative(cui_image_view r, ImageView old = default)
    {
        var v = old;
        v.Zoom = r.zoom;
        v.Cx = r.cx;
        v.Cy = r.cy;
        return v;
    }
    private cui_image_viewer_opts ToNative(ImageViewerOpts v)
    {
        cui_image_viewer_opts r = default;
        r.width = ToNative(v.Width);
        r.height = ToNative(v.Height);
        r.max_zoom = v.MaxZoom;
        r.no_toolbar = v.NoToolbar;
        r.checkerboard = v.Checkerboard;
        return r;
    }
    private ImageViewerOpts FromNative(cui_image_viewer_opts r, ImageViewerOpts old = default)
    {
        var v = old;
        v.Width = FromNative(r.width, old.Width);
        v.Height = FromNative(r.height, old.Height);
        v.MaxZoom = r.max_zoom;
        v.NoToolbar = (r.no_toolbar != 0);
        v.Checkerboard = (r.checkerboard != 0);
        return v;
    }
    private cui_inline_edit_opts ToNative(InlineEditOpts v)
    {
        cui_inline_edit_opts r = default;
        r.font = (int)CheckEnum(v.Font);
        r.size = v.Size;
        r.placeholder = Utf8(v.Placeholder);
        r.width = ToNative(v.Width);
        return r;
    }
    private InlineEditOpts FromNative(cui_inline_edit_opts r, InlineEditOpts old = default)
    {
        var v = old;
        v.Font = (Font)r.font;
        v.Size = r.size;
        v.Placeholder = Decode(r.placeholder, old.Placeholder);
        v.Width = FromNative(r.width, old.Width);
        return v;
    }
    private cui_input_opts ToNative(InputOpts v)
    {
        cui_input_opts r = default;
        r.label = Utf8(v.Label);
        r.placeholder = Utf8(v.Placeholder);
        r.icon = Utf8(v.Icon);
        r.hint = Utf8(v.Hint);
        r.error = Utf8(v.Error);
        r.password = v.Password;
        r.disabled = v.Disabled;
        r.clearable = v.Clearable;
        r.autofocus = v.Autofocus;
        r.read_only = v.ReadOnly;
        r.size = (cui_control_size)CheckEnum(v.Size);
        r.width = ToNative(v.Width);
        r.height = v.Height;
        r.prefix = Utf8(v.Prefix);
        r.suffix = Utf8(v.Suffix);
        return r;
    }
    private InputOpts FromNative(cui_input_opts r, InputOpts old = default)
    {
        var v = old;
        v.Label = Decode(r.label, old.Label);
        v.Placeholder = Decode(r.placeholder, old.Placeholder);
        v.Icon = Decode(r.icon, old.Icon);
        v.Hint = Decode(r.hint, old.Hint);
        v.Error = Decode(r.error, old.Error);
        v.Password = (r.password != 0);
        v.Disabled = (r.disabled != 0);
        v.Clearable = (r.clearable != 0);
        v.Autofocus = (r.autofocus != 0);
        v.ReadOnly = (r.read_only != 0);
        v.Size = (ControlSize)r.size;
        v.Width = FromNative(r.width, old.Width);
        v.Height = r.height;
        v.Prefix = Decode(r.prefix, old.Prefix);
        v.Suffix = Decode(r.suffix, old.Suffix);
        return v;
    }
    private cui_json_opts ToNative(JsonOpts v)
    {
        cui_json_opts r = default;
        r.expand_depth = v.ExpandDepth;
        r.max_height = v.MaxHeight;
        return r;
    }
    private JsonOpts FromNative(cui_json_opts r, JsonOpts old = default)
    {
        var v = old;
        v.ExpandDepth = r.expand_depth;
        v.MaxHeight = r.max_height;
        return v;
    }
    private cui_kanban_column_opts ToNative(KanbanColumnOpts v)
    {
        cui_kanban_column_opts r = default;
        r.dot = ToNative(v.Dot);
        r.limit = v.Limit;
        r.width = v.Width;
        return r;
    }
    private KanbanColumnOpts FromNative(cui_kanban_column_opts r, KanbanColumnOpts old = default)
    {
        var v = old;
        v.Dot = FromNative(r.dot, old.Dot);
        v.Limit = r.limit;
        v.Width = r.width;
        return v;
    }
    private cui_kanban_move ToNative(KanbanMove v)
    {
        cui_kanban_move r = default;
        r.card = v.Card;
        r.from_column = v.FromColumn;
        r.to_column = v.ToColumn;
        r.to_index = v.ToIndex;
        return r;
    }
    private KanbanMove FromNative(cui_kanban_move r, KanbanMove old = default)
    {
        var v = old;
        v.Card = r.card;
        v.FromColumn = r.from_column;
        v.ToColumn = r.to_column;
        v.ToIndex = r.to_index;
        return v;
    }
    private cui_key_combo ToNative(KeyCombo v)
    {
        cui_key_combo r = default;
        r.mods = v.Mods;
        r.key = (cui_key)CheckEnum(v.Key);
        return r;
    }
    private KeyCombo FromNative(cui_key_combo r, KeyCombo old = default)
    {
        var v = old;
        v.Mods = r.mods;
        v.Key = (Key)r.key;
        return v;
    }
    private cui_keymap_binding ToNative(KeymapBinding v)
    {
        cui_keymap_binding r = default;
        r.combo = ToNative(v.Combo);
        r.action = Utf8(v.Action);
        r.category = v.Category;
        return r;
    }
    private KeymapBinding FromNative(cui_keymap_binding r, KeymapBinding old = default)
    {
        var v = old;
        v.Combo = FromNative(r.combo, old.Combo);
        v.Action = Decode(r.action, old.Action);
        v.Category = r.category;
        return v;
    }
    private cui_keymap_opts ToNative(KeymapOpts v)
    {
        cui_keymap_opts r = default;
        r.width = ToNative(v.Width);
        r.categories = Strings(v.Categories.Span);
        r.category_count = (int)v.Categories.Length;
        r.mods = BindInt(v.Mods);
        r.no_function_row = v.NoFunctionRow;
        return r;
    }
    private KeymapOpts FromNative(cui_keymap_opts r, KeymapOpts old = default)
    {
        var v = old;
        v.Width = FromNative(r.width, old.Width);
        v.NoFunctionRow = (r.no_function_row != 0);
        return v;
    }
    private cui_keymap_result ToNative(KeymapResult v)
    {
        cui_keymap_result r = default;
        r.clicked = (cui_key)CheckEnum(v.Clicked);
        r.conflicts = v.Conflicts;
        return r;
    }
    private KeymapResult FromNative(cui_keymap_result r, KeymapResult old = default)
    {
        var v = old;
        v.Clicked = (Key)r.clicked;
        v.Conflicts = r.conflicts;
        return v;
    }
    private cui_knob_opts ToNative(KnobOpts v)
    {
        cui_knob_opts r = default;
        r.label = Utf8(v.Label);
        r.format = Utf8(CheckFormat(v.Format, 1));
        r.unit = Utf8(v.Unit);
        r.size = v.Size;
        r.step = v.Step;
        r.reset = v.Reset;
        r.bipolar = v.Bipolar;
        r.log_scale = v.LogScale;
        r.hide_value = v.HideValue;
        r.disabled = v.Disabled;
        r.color = ToNative(v.Color);
        return r;
    }
    private KnobOpts FromNative(cui_knob_opts r, KnobOpts old = default)
    {
        var v = old;
        v.Label = Decode(r.label, old.Label);
        v.Format = Decode(r.format, old.Format);
        v.Unit = Decode(r.unit, old.Unit);
        v.Size = r.size;
        v.Step = r.step;
        v.Reset = r.reset;
        v.Bipolar = (r.bipolar != 0);
        v.LogScale = (r.log_scale != 0);
        v.HideValue = (r.hide_value != 0);
        v.Disabled = (r.disabled != 0);
        v.Color = FromNative(r.color, old.Color);
        return v;
    }
    private cui_kv_editor_opts ToNative(KvEditorOpts v)
    {
        cui_kv_editor_opts r = default;
        r.key_placeholder = Utf8(v.KeyPlaceholder);
        r.value_placeholder = Utf8(v.ValuePlaceholder);
        r.no_toggles = v.NoToggles;
        r.no_secrets = v.NoSecrets;
        r.read_only = v.ReadOnly;
        r.width = ToNative(v.Width);
        return r;
    }
    private KvEditorOpts FromNative(cui_kv_editor_opts r, KvEditorOpts old = default)
    {
        var v = old;
        v.KeyPlaceholder = Decode(r.key_placeholder, old.KeyPlaceholder);
        v.ValuePlaceholder = Decode(r.value_placeholder, old.ValuePlaceholder);
        v.NoToggles = (r.no_toggles != 0);
        v.NoSecrets = (r.no_secrets != 0);
        v.ReadOnly = (r.read_only != 0);
        v.Width = FromNative(r.width, old.Width);
        return v;
    }
    private cui_kv_pair ToNative(KvPair v)
    {
        cui_kv_pair r = default;
        WriteInline(v.Key, r.key, 64);
        WriteInline(v.Value, r.value, 256);
        r.enabled = v.Enabled;
        r.secret = v.Secret;
        return r;
    }
    private KvPair FromNative(cui_kv_pair r, KvPair old = default)
    {
        var v = old;
        v.Key = Decode(r.key, old.Key, 64);
        v.Value = Decode(r.value, old.Value, 256);
        v.Enabled = (r.enabled != 0);
        v.Secret = (r.secret != 0);
        return v;
    }
    private cui_level_meter_opts ToNative(LevelMeterOpts v)
    {
        cui_level_meter_opts r = default;
        r.min_db = v.MinDb;
        r.max_db = v.MaxDb;
        r.length = v.Length;
        r.thickness = v.Thickness;
        r.horizontal = v.Horizontal;
        r.scale = v.Scale;
        r.segmented = v.Segmented;
        r.hold = v.Hold;
        r.decay = v.Decay;
        return r;
    }
    private LevelMeterOpts FromNative(cui_level_meter_opts r, LevelMeterOpts old = default)
    {
        var v = old;
        v.MinDb = r.min_db;
        v.MaxDb = r.max_db;
        v.Length = r.length;
        v.Thickness = r.thickness;
        v.Horizontal = (r.horizontal != 0);
        v.Scale = (r.scale != 0);
        v.Segmented = (r.segmented != 0);
        v.Hold = r.hold;
        v.Decay = r.decay;
        return v;
    }
    private cui_list_item_opts ToNative(ListItemOpts v)
    {
        cui_list_item_opts r = default;
        r.icon = Utf8(v.Icon);
        r.subtitle = Utf8(v.Subtitle);
        r.trailing = Utf8(v.Trailing);
        r.selected = v.Selected;
        r.avatar = Utf8(v.Avatar);
        return r;
    }
    private ListItemOpts FromNative(cui_list_item_opts r, ListItemOpts old = default)
    {
        var v = old;
        v.Icon = Decode(r.icon, old.Icon);
        v.Subtitle = Decode(r.subtitle, old.Subtitle);
        v.Trailing = Decode(r.trailing, old.Trailing);
        v.Selected = (r.selected != 0);
        v.Avatar = Decode(r.avatar, old.Avatar);
        return v;
    }
    private cui_loader_opts ToNative(LoaderOpts v)
    {
        cui_loader_opts r = default;
        r.style = (cui_loader_style)CheckEnum(v.Style);
        r.size = v.Size;
        r.color = ToNative(v.Color);
        return r;
    }
    private LoaderOpts FromNative(cui_loader_opts r, LoaderOpts old = default)
    {
        var v = old;
        v.Style = (LoaderStyle)r.style;
        v.Size = r.size;
        v.Color = FromNative(r.color, old.Color);
        return v;
    }
    private cui_markdown_result ToNative(MarkdownResult v)
    {
        cui_markdown_result r = default;
        r.link = Utf8(v.Link);
        r.hovered_link = Utf8(v.HoveredLink);
        r.task_offset = v.TaskOffset;
        r.task_line = v.TaskLine;
        r.task_checked = v.TaskChecked;
        r.height = v.Height;
        return r;
    }
    private MarkdownResult FromNative(cui_markdown_result r, MarkdownResult old = default)
    {
        var v = old;
        v.Link = Decode(r.link, old.Link);
        v.HoveredLink = Decode(r.hovered_link, old.HoveredLink);
        v.TaskOffset = r.task_offset;
        v.TaskLine = r.task_line;
        v.TaskChecked = (r.task_checked != 0);
        v.Height = r.height;
        return v;
    }
    private cui_menu_item_opts ToNative(MenuItemOpts v)
    {
        cui_menu_item_opts r = default;
        r.icon = Utf8(v.Icon);
        r.shortcut = Utf8(v.Shortcut);
        r.danger = v.Danger;
        r.disabled = v.Disabled;
        r.@checked = v.Checked;
        return r;
    }
    private MenuItemOpts FromNative(cui_menu_item_opts r, MenuItemOpts old = default)
    {
        var v = old;
        v.Icon = Decode(r.icon, old.Icon);
        v.Shortcut = Decode(r.shortcut, old.Shortcut);
        v.Danger = (r.danger != 0);
        v.Disabled = (r.disabled != 0);
        v.Checked = (r.@checked != 0);
        return v;
    }
    private cui_menu_opts ToNative(MenuOpts v)
    {
        cui_menu_opts r = default;
        r.icon = Utf8(v.Icon);
        r.variant = (cui_button_variant)CheckEnum(v.Variant);
        r.placement = (cui_placement)CheckEnum(v.Placement);
        r.width = v.Width;
        r.icon_only = v.IconOnly;
        return r;
    }
    private MenuOpts FromNative(cui_menu_opts r, MenuOpts old = default)
    {
        var v = old;
        v.Icon = Decode(r.icon, old.Icon);
        v.Variant = (ButtonVariant)r.variant;
        v.Placement = (Placement)r.placement;
        v.Width = r.width;
        v.IconOnly = (r.icon_only != 0);
        return v;
    }
    private cui_metric_opts ToNative(MetricOpts v)
    {
        cui_metric_opts r = default;
        r.delta = v.Delta;
        r.delta_label = Utf8(v.DeltaLabel);
        r.spark = CopyFloat(v.Spark.Span);
        r.spark_count = (int)v.Spark.Length;
        r.icon = Utf8(v.Icon);
        r.width = ToNative(v.Width);
        r.lower_is_better = v.LowerIsBetter;
        return r;
    }
    private MetricOpts FromNative(cui_metric_opts r, MetricOpts old = default)
    {
        var v = old;
        v.Delta = r.delta;
        v.DeltaLabel = Decode(r.delta_label, old.DeltaLabel);
        v.Icon = Decode(r.icon, old.Icon);
        v.Width = FromNative(r.width, old.Width);
        v.LowerIsBetter = (r.lower_is_better != 0);
        return v;
    }
    private cui_model ToNative(Model v)
    {
        cui_model r = default;
        r.name = Utf8(v.Name);
        r.detail = Utf8(v.Detail);
        r.badge = Utf8(v.Badge);
        r.icon = Utf8(v.Icon);
        return r;
    }
    private Model FromNative(cui_model r, Model old = default)
    {
        var v = old;
        v.Name = Decode(r.name, old.Name);
        v.Detail = Decode(r.detail, old.Detail);
        v.Badge = Decode(r.badge, old.Badge);
        v.Icon = Decode(r.icon, old.Icon);
        return v;
    }
    private cui_money_opts ToNative(MoneyOpts v)
    {
        cui_money_opts r = default;
        r.label = Utf8(v.Label);
        r.symbol = Utf8(v.Symbol);
        r.code = Utf8(v.Code);
        r.decimals = v.Decimals;
        r.width = ToNative(v.Width);
        r.hint = Utf8(v.Hint);
        return r;
    }
    private MoneyOpts FromNative(cui_money_opts r, MoneyOpts old = default)
    {
        var v = old;
        v.Label = Decode(r.label, old.Label);
        v.Symbol = Decode(r.symbol, old.Symbol);
        v.Code = Decode(r.code, old.Code);
        v.Decimals = r.decimals;
        v.Width = FromNative(r.width, old.Width);
        v.Hint = Decode(r.hint, old.Hint);
        return v;
    }
    private cui_month_event ToNative(MonthEvent v)
    {
        cui_month_event r = default;
        r.title = Utf8(v.Title);
        r.start = ToNative(v.Start);
        r.end = ToNative(v.End);
        r.time = v.Time;
        r.all_day = v.AllDay;
        r.tone = (cui_tone)CheckEnum(v.Tone);
        return r;
    }
    private MonthEvent FromNative(cui_month_event r, MonthEvent old = default)
    {
        var v = old;
        v.Title = Decode(r.title, old.Title);
        v.Start = FromNative(r.start, old.Start);
        v.End = FromNative(r.end, old.End);
        v.Time = r.time;
        v.AllDay = (r.all_day != 0);
        v.Tone = (Tone)r.tone;
        return v;
    }
    private cui_month_opts ToNative(MonthOpts v)
    {
        Validate(v);
        cui_month_opts r = default;
        r.selected = BindDate(v.Selected);
        r.first_weekday = v.FirstWeekday;
        r.cell_height = v.CellHeight;
        r.month_names = Strings(v.MonthNames.Span);
        r.day_names = Strings(v.DayNames.Span);
        r.hour12 = v.Hour12;
        r.today = ToNative(v.Today);
        return r;
    }
    private MonthOpts FromNative(cui_month_opts r, MonthOpts old = default)
    {
        var v = old;
        v.FirstWeekday = r.first_weekday;
        v.CellHeight = r.cell_height;
        v.Hour12 = (r.hour12 != 0);
        v.Today = FromNative(r.today, old.Today);
        return v;
    }
    private cui_month_result ToNative(MonthResult v)
    {
        cui_month_result r = default;
        r.@event = v.Event;
        r.day_clicked = v.DayClicked;
        r.day_activated = v.DayActivated;
        r.month_changed = v.MonthChanged;
        return r;
    }
    private MonthResult FromNative(cui_month_result r, MonthResult old = default)
    {
        var v = old;
        v.Event = r.@event;
        v.DayClicked = (r.day_clicked != 0);
        v.DayActivated = (r.day_activated != 0);
        v.MonthChanged = (r.month_changed != 0);
        return v;
    }
    private cui_multi_select_opts ToNative(MultiSelectOpts v)
    {
        cui_multi_select_opts r = default;
        r.label = Utf8(v.Label);
        r.placeholder = Utf8(v.Placeholder);
        r.icons = Strings(v.Icons.Span);
        r.searchable = v.Searchable;
        r.max_chips = v.MaxChips;
        r.width = ToNative(v.Width);
        return r;
    }
    private MultiSelectOpts FromNative(cui_multi_select_opts r, MultiSelectOpts old = default)
    {
        var v = old;
        v.Label = Decode(r.label, old.Label);
        v.Placeholder = Decode(r.placeholder, old.Placeholder);
        v.Searchable = (r.searchable != 0);
        v.MaxChips = r.max_chips;
        v.Width = FromNative(r.width, old.Width);
        return v;
    }
    private cui_nav_opts ToNative(NavOpts v)
    {
        cui_nav_opts r = default;
        r.icon = Utf8(v.Icon);
        r.badge = Utf8(v.Badge);
        r.selected = v.Selected;
        r.group = Utf8(v.Group);
        return r;
    }
    private NavOpts FromNative(cui_nav_opts r, NavOpts old = default)
    {
        var v = old;
        v.Icon = Decode(r.icon, old.Icon);
        v.Badge = Decode(r.badge, old.Badge);
        v.Selected = (r.selected != 0);
        v.Group = Decode(r.group, old.Group);
        return v;
    }
    private cui_net_request ToNative(NetRequest v)
    {
        cui_net_request r = default;
        r.name = Utf8(v.Name);
        r.method = Utf8(v.Method);
        r.status = v.Status;
        r.@type = Utf8(v.Type);
        r.size = v.Size;
        r.start = v.Start;
        if (!v.Timing.IsEmpty && v.Timing.Length != 6) throw new ArgumentException("NetRequest.Timing requires 6 elements.");
        for (int i = 0; i < v.Timing.Length; i++) r.timing[i] = v.Timing.Span[i];
        return r;
    }
    private NetRequest FromNative(cui_net_request r, NetRequest old = default)
    {
        var v = old;
        v.Name = Decode(r.name, old.Name);
        v.Method = Decode(r.method, old.Method);
        v.Status = r.status;
        v.Type = Decode(r.@type, old.Type);
        v.Size = r.size;
        v.Start = r.start;
        if (v.Timing.Length != 6) v.Timing = new float[6];
        for (int i = 0; i < 6; i++) v.Timing.Span[i] = r.timing[i];
        return v;
    }
    private cui_node_editor_opts ToNative(NodeEditorOpts v)
    {
        cui_node_editor_opts r = default;
        r.width = ToNative(v.Width);
        r.height = ToNative(v.Height);
        r.min_zoom = v.MinZoom;
        r.max_zoom = v.MaxZoom;
        r.no_grid = v.NoGrid;
        r.minimap = v.Minimap;
        return r;
    }
    private NodeEditorOpts FromNative(cui_node_editor_opts r, NodeEditorOpts old = default)
    {
        var v = old;
        v.Width = FromNative(r.width, old.Width);
        v.Height = FromNative(r.height, old.Height);
        v.MinZoom = r.min_zoom;
        v.MaxZoom = r.max_zoom;
        v.NoGrid = (r.no_grid != 0);
        v.Minimap = (r.minimap != 0);
        return v;
    }
    private cui_node_opts ToNative(NodeOpts v)
    {
        cui_node_opts r = default;
        r.width = v.Width;
        r.icon = Utf8(v.Icon);
        return r;
    }
    private NodeOpts FromNative(cui_node_opts r, NodeOpts old = default)
    {
        var v = old;
        v.Width = r.width;
        v.Icon = Decode(r.icon, old.Icon);
        return v;
    }
    private cui_node_view ToNative(NodeView v)
    {
        cui_node_view r = default;
        r.x = v.X;
        r.y = v.Y;
        r.zoom = v.Zoom;
        return r;
    }
    private NodeView FromNative(cui_node_view r, NodeView old = default)
    {
        var v = old;
        v.X = r.x;
        v.Y = r.y;
        v.Zoom = r.zoom;
        return v;
    }
    private cui_now_playing ToNative(NowPlaying v)
    {
        cui_now_playing r = default;
        r.title = Utf8(v.Title);
        r.artist = Utf8(v.Artist);
        r.art = Unwrap(v.Art);
        r.art_color = ToNative(v.ArtColor);
        r.position = v.Position;
        r.duration = v.Duration;
        r.playing = v.Playing;
        return r;
    }
    private NowPlaying FromNative(cui_now_playing r, NowPlaying old = default)
    {
        var v = old;
        v.Title = Decode(r.title, old.Title);
        v.Artist = Decode(r.artist, old.Artist);
        v.Art = FindTexture(r.art);
        v.ArtColor = FromNative(r.art_color, old.ArtColor);
        v.Position = r.position;
        v.Duration = r.duration;
        v.Playing = (r.playing != 0);
        return v;
    }
    private cui_now_playing_opts ToNative(NowPlayingOpts v)
    {
        cui_now_playing_opts r = default;
        r.width = ToNative(v.Width);
        r.compact = v.Compact;
        return r;
    }
    private NowPlayingOpts FromNative(cui_now_playing_opts r, NowPlayingOpts old = default)
    {
        var v = old;
        v.Width = FromNative(r.width, old.Width);
        v.Compact = (r.compact != 0);
        return v;
    }
    private cui_number_opts ToNative(NumberOpts v)
    {
        cui_number_opts r = default;
        r.label = Utf8(v.Label);
        r.step = v.Step;
        r.decimals = v.Decimals;
        r.disabled = v.Disabled;
        r.width = ToNative(v.Width);
        r.suffix = Utf8(v.Suffix);
        return r;
    }
    private NumberOpts FromNative(cui_number_opts r, NumberOpts old = default)
    {
        var v = old;
        v.Label = Decode(r.label, old.Label);
        v.Step = r.step;
        v.Decimals = r.decimals;
        v.Disabled = (r.disabled != 0);
        v.Width = FromNative(r.width, old.Width);
        v.Suffix = Decode(r.suffix, old.Suffix);
        return v;
    }
    private cui_pipeline_job ToNative(PipelineJob v)
    {
        cui_pipeline_job r = default;
        r.name = Utf8(v.Name);
        r.stage = v.Stage;
        r.status = (cui_job_status)CheckEnum(v.Status);
        r.duration = v.Duration;
        r.allow_failure = v.AllowFailure;
        return r;
    }
    private PipelineJob FromNative(cui_pipeline_job r, PipelineJob old = default)
    {
        var v = old;
        v.Name = Decode(r.name, old.Name);
        v.Stage = r.stage;
        v.Status = (JobStatus)r.status;
        v.Duration = r.duration;
        v.AllowFailure = (r.allow_failure != 0);
        return v;
    }
    private cui_pipeline_opts ToNative(PipelineOpts v)
    {
        cui_pipeline_opts r = default;
        r.width = ToNative(v.Width);
        r.selected = BindInt(v.Selected);
        return r;
    }
    private PipelineOpts FromNative(cui_pipeline_opts r, PipelineOpts old = default)
    {
        var v = old;
        v.Width = FromNative(r.width, old.Width);
        return v;
    }
    private cui_pipeline_result ToNative(PipelineResult v)
    {
        cui_pipeline_result r = default;
        r.clicked = v.Clicked;
        r.hovered = v.Hovered;
        return r;
    }
    private PipelineResult FromNative(cui_pipeline_result r, PipelineResult old = default)
    {
        var v = old;
        v.Clicked = r.clicked;
        v.Hovered = r.hovered;
        return v;
    }
    private cui_plan_step ToNative(PlanStep v)
    {
        cui_plan_step r = default;
        r.title = Utf8(v.Title);
        r.detail = Utf8(v.Detail);
        r.status = (cui_task_status)CheckEnum(v.Status);
        return r;
    }
    private PlanStep FromNative(cui_plan_step r, PlanStep old = default)
    {
        var v = old;
        v.Title = Decode(r.title, old.Title);
        v.Detail = Decode(r.detail, old.Detail);
        v.Status = (TaskStatus)r.status;
        return v;
    }
    private cui_popover_opts ToNative(PopoverOpts v)
    {
        cui_popover_opts r = default;
        r.placement = (cui_placement)CheckEnum(v.Placement);
        r.width = v.Width;
        r.pad = v.Pad;
        r.gap = v.Gap;
        return r;
    }
    private PopoverOpts FromNative(cui_popover_opts r, PopoverOpts old = default)
    {
        var v = old;
        v.Placement = (Placement)r.placement;
        v.Width = r.width;
        v.Pad = r.pad;
        v.Gap = r.gap;
        return v;
    }
    private cui_presence_user ToNative(PresenceUser v)
    {
        cui_presence_user r = default;
        r.name = Utf8(v.Name);
        r.color = ToNative(v.Color);
        r.avatar = Unwrap(v.Avatar);
        r.idle = v.Idle;
        return r;
    }
    private PresenceUser FromNative(cui_presence_user r, PresenceUser old = default)
    {
        var v = old;
        v.Name = Decode(r.name, old.Name);
        v.Color = FromNative(r.color, old.Color);
        v.Avatar = FindTexture(r.avatar);
        v.Idle = (r.idle != 0);
        return v;
    }
    private cui_problems_opts ToNative(ProblemsOpts v)
    {
        cui_problems_opts r = default;
        r.width = ToNative(v.Width);
        r.height = v.Height;
        r.no_toolbar = v.NoToolbar;
        return r;
    }
    private ProblemsOpts FromNative(cui_problems_opts r, ProblemsOpts old = default)
    {
        var v = old;
        v.Width = FromNative(r.width, old.Width);
        v.Height = r.height;
        v.NoToolbar = (r.no_toolbar != 0);
        return v;
    }
    private cui_problems_result ToNative(ProblemsResult v)
    {
        cui_problems_result r = default;
        r.selected = v.Selected;
        r.activated = v.Activated;
        r.shown = v.Shown;
        return r;
    }
    private ProblemsResult FromNative(cui_problems_result r, ProblemsResult old = default)
    {
        var v = old;
        v.Selected = r.selected;
        v.Activated = r.activated;
        v.Shown = r.shown;
        return v;
    }
    private cui_progress_opts ToNative(ProgressOpts v)
    {
        cui_progress_opts r = default;
        r.indeterminate = v.Indeterminate;
        r.tone = (cui_tone)CheckEnum(v.Tone);
        r.height = v.Height;
        r.label = Utf8(v.Label);
        r.show_value = v.ShowValue;
        r.width = ToNative(v.Width);
        return r;
    }
    private ProgressOpts FromNative(cui_progress_opts r, ProgressOpts old = default)
    {
        var v = old;
        v.Indeterminate = (r.indeterminate != 0);
        v.Tone = (Tone)r.tone;
        v.Height = r.height;
        v.Label = Decode(r.label, old.Label);
        v.ShowValue = (r.show_value != 0);
        v.Width = FromNative(r.width, old.Width);
        return v;
    }
    private cui_prompt_suggestion ToNative(PromptSuggestion v)
    {
        cui_prompt_suggestion r = default;
        r.icon = Utf8(v.Icon);
        r.title = Utf8(v.Title);
        r.detail = Utf8(v.Detail);
        return r;
    }
    private PromptSuggestion FromNative(cui_prompt_suggestion r, PromptSuggestion old = default)
    {
        var v = old;
        v.Icon = Decode(r.icon, old.Icon);
        v.Title = Decode(r.title, old.Title);
        v.Detail = Decode(r.detail, old.Detail);
        return v;
    }
    private cui_prompt_suggestions_opts ToNative(PromptSuggestionsOpts v)
    {
        cui_prompt_suggestions_opts r = default;
        r.columns = v.Columns;
        return r;
    }
    private PromptSuggestionsOpts FromNative(cui_prompt_suggestions_opts r, PromptSuggestionsOpts old = default)
    {
        var v = old;
        v.Columns = r.columns;
        return v;
    }
    private cui_query_field ToNative(QueryField v)
    {
        cui_query_field r = default;
        r.name = Utf8(v.Name);
        r.@type = (cui_query_type)CheckEnum(v.Type);
        r.choices = Strings(v.Choices.Span);
        r.choice_count = (int)v.Choices.Length;
        return r;
    }
    private QueryField FromNative(cui_query_field r, QueryField old = default)
    {
        var v = old;
        v.Name = Decode(r.name, old.Name);
        v.Type = (QueryType)r.@type;
        return v;
    }
    private cui_query_node ToNative(QueryNode v)
    {
        cui_query_node r = default;
        r.depth = v.Depth;
        r.group = v.Group;
        r.any = v.Any;
        r.field = v.Field;
        r.op = v.Op;
        WriteInline(v.Value, r.value, 64);
        return r;
    }
    private QueryNode FromNative(cui_query_node r, QueryNode old = default)
    {
        var v = old;
        v.Depth = r.depth;
        v.Group = (r.group != 0);
        v.Any = (r.any != 0);
        v.Field = r.field;
        v.Op = r.op;
        v.Value = Decode(r.value, old.Value, 64);
        return v;
    }
    private cui_query_opts ToNative(QueryOpts v)
    {
        cui_query_opts r = default;
        r.width = ToNative(v.Width);
        r.max_depth = v.MaxDepth;
        r.read_only = v.ReadOnly;
        return r;
    }
    private QueryOpts FromNative(cui_query_opts r, QueryOpts old = default)
    {
        var v = old;
        v.Width = FromNative(r.width, old.Width);
        v.MaxDepth = r.max_depth;
        v.ReadOnly = (r.read_only != 0);
        return v;
    }
    private cui_radar_opts ToNative(RadarOpts v)
    {
        cui_radar_opts r = default;
        r.size = v.Size;
        r.max = v.Max;
        r.format = Utf8(CheckFormat(v.Format, 1));
        r.no_legend = v.NoLegend;
        return r;
    }
    private RadarOpts FromNative(cui_radar_opts r, RadarOpts old = default)
    {
        var v = old;
        v.Size = r.size;
        v.Max = r.max;
        v.Format = Decode(r.format, old.Format);
        v.NoLegend = (r.no_legend != 0);
        return v;
    }
    private cui_radial_gauge_opts ToNative(RadialGaugeOpts v)
    {
        cui_radial_gauge_opts r = default;
        r.label = Utf8(v.Label);
        r.format = Utf8(CheckFormat(v.Format, 1));
        r.unit = Utf8(v.Unit);
        r.size = v.Size;
        r.sweep = v.Sweep;
        r.bands = CopyGaugeBand(v.Bands.Span);
        r.band_count = (int)v.Bands.Length;
        r.ticks = v.Ticks;
        r.target = v.Target;
        r.show_target = v.ShowTarget;
        r.no_needle = v.NoNeedle;
        return r;
    }
    private RadialGaugeOpts FromNative(cui_radial_gauge_opts r, RadialGaugeOpts old = default)
    {
        var v = old;
        v.Label = Decode(r.label, old.Label);
        v.Format = Decode(r.format, old.Format);
        v.Unit = Decode(r.unit, old.Unit);
        v.Size = r.size;
        v.Sweep = r.sweep;
        v.Ticks = r.ticks;
        v.Target = r.target;
        v.ShowTarget = (r.show_target != 0);
        v.NoNeedle = (r.no_needle != 0);
        return v;
    }
    private cui_radio_cards_opts ToNative(RadioCardsOpts v)
    {
        cui_radio_cards_opts r = default;
        r.columns = v.Columns;
        return r;
    }
    private RadioCardsOpts FromNative(cui_radio_cards_opts r, RadioCardsOpts old = default)
    {
        var v = old;
        v.Columns = r.columns;
        return v;
    }
    private cui_reaction ToNative(Reaction v)
    {
        cui_reaction r = default;
        r.icon = Utf8(v.Icon);
        r.count = v.Count;
        r.active = v.Active;
        return r;
    }
    private Reaction FromNative(cui_reaction r, Reaction old = default)
    {
        var v = old;
        v.Icon = Decode(r.icon, old.Icon);
        v.Count = r.count;
        v.Active = (r.active != 0);
        return v;
    }
    private cui_readout_opts ToNative(ReadoutOpts v)
    {
        cui_readout_opts r = default;
        r.unit = Utf8(v.Unit);
        r.detail = Utf8(v.Detail);
        r.size = (cui_control_size)CheckEnum(v.Size);
        r.tone = (cui_tone)CheckEnum(v.Tone);
        r.accent = v.Accent;
        r.align = (cui_text_align)CheckEnum(v.Align);
        r.width = ToNative(v.Width);
        return r;
    }
    private ReadoutOpts FromNative(cui_readout_opts r, ReadoutOpts old = default)
    {
        var v = old;
        v.Unit = Decode(r.unit, old.Unit);
        v.Detail = Decode(r.detail, old.Detail);
        v.Size = (ControlSize)r.size;
        v.Tone = (Tone)r.tone;
        v.Accent = (r.accent != 0);
        v.Align = (TextAlign)r.align;
        v.Width = FromNative(r.width, old.Width);
        return v;
    }
    private cui_reasoning_opts ToNative(ReasoningOpts v)
    {
        cui_reasoning_opts r = default;
        r.thinking = v.Thinking;
        r.seconds = v.Seconds;
        r.title = Utf8(v.Title);
        return r;
    }
    private ReasoningOpts FromNative(cui_reasoning_opts r, ReasoningOpts old = default)
    {
        var v = old;
        v.Thinking = (r.thinking != 0);
        v.Seconds = r.seconds;
        v.Title = Decode(r.title, old.Title);
        return v;
    }
    private cui_rect ToNative(Rect v)
    {
        cui_rect r = default;
        r.x = v.X;
        r.y = v.Y;
        r.w = v.W;
        r.h = v.H;
        return r;
    }
    private Rect FromNative(cui_rect r, Rect old = default)
    {
        var v = old;
        v.X = r.x;
        v.Y = r.y;
        v.W = r.w;
        v.H = r.h;
        return v;
    }
    private cui_regex_tester_opts ToNative(RegexTesterOpts v)
    {
        cui_regex_tester_opts r = default;
        r.width = ToNative(v.Width);
        r.height = v.Height;
        r.flags = BindInt(v.Flags);
        return r;
    }
    private RegexTesterOpts FromNative(cui_regex_tester_opts r, RegexTesterOpts old = default)
    {
        var v = old;
        v.Width = FromNative(r.width, old.Width);
        v.Height = r.height;
        return v;
    }
    private cui_regex_tester_result ToNative(RegexTesterResult v)
    {
        cui_regex_tester_result r = default;
        r.valid = v.Valid;
        r.matches = v.Matches;
        r.error = Utf8(v.Error);
        return r;
    }
    private RegexTesterResult FromNative(cui_regex_tester_result r, RegexTesterResult old = default)
    {
        var v = old;
        v.Valid = (r.valid != 0);
        v.Matches = r.matches;
        v.Error = Decode(r.error, old.Error);
        return v;
    }
    private cui_request_bar_opts ToNative(RequestBarOpts v)
    {
        cui_request_bar_opts r = default;
        r.width = ToNative(v.Width);
        r.loading = v.Loading;
        r.placeholder = Utf8(v.Placeholder);
        return r;
    }
    private RequestBarOpts FromNative(cui_request_bar_opts r, RequestBarOpts old = default)
    {
        var v = old;
        v.Width = FromNative(r.width, old.Width);
        v.Loading = (r.loading != 0);
        v.Placeholder = Decode(r.placeholder, old.Placeholder);
        return v;
    }
    private cui_request_bar_result ToNative(RequestBarResult v)
    {
        cui_request_bar_result r = default;
        r.send = v.Send;
        r.cancel = v.Cancel;
        r.changed = v.Changed;
        return r;
    }
    private RequestBarResult FromNative(cui_request_bar_result r, RequestBarResult old = default)
    {
        var v = old;
        v.Send = (r.send != 0);
        v.Cancel = (r.cancel != 0);
        v.Changed = (r.changed != 0);
        return v;
    }
    private cui_response_actions_opts ToNative(ResponseActionsOpts v)
    {
        cui_response_actions_opts r = default;
        r.copy_text = Utf8(v.CopyText);
        r.vote = BindInt(v.Vote);
        r.no_regenerate = v.NoRegenerate;
        r.no_share = v.NoShare;
        return r;
    }
    private ResponseActionsOpts FromNative(cui_response_actions_opts r, ResponseActionsOpts old = default)
    {
        var v = old;
        v.CopyText = Decode(r.copy_text, old.CopyText);
        v.NoRegenerate = (r.no_regenerate != 0);
        v.NoShare = (r.no_share != 0);
        return v;
    }
    private cui_rings_opts ToNative(RingsOpts v)
    {
        cui_rings_opts r = default;
        r.size = v.Size;
        r.thickness = v.Thickness;
        r.center = Utf8(v.Center);
        r.caption = Utf8(v.Caption);
        r.legend = v.Legend;
        return r;
    }
    private RingsOpts FromNative(cui_rings_opts r, RingsOpts old = default)
    {
        var v = old;
        v.Size = r.size;
        v.Thickness = r.thickness;
        v.Center = Decode(r.center, old.Center);
        v.Caption = Decode(r.caption, old.Caption);
        v.Legend = (r.legend != 0);
        return v;
    }
    private cui_ruler_opts ToNative(RulerOpts v)
    {
        cui_ruler_opts r = default;
        r.step = v.Step;
        r.major_every = v.MajorEvery;
        r.spacing = v.Spacing;
        r.format = Utf8(CheckFormat(v.Format, 1));
        r.unit = Utf8(v.Unit);
        r.width = ToNative(v.Width);
        return r;
    }
    private RulerOpts FromNative(cui_ruler_opts r, RulerOpts old = default)
    {
        var v = old;
        v.Step = r.step;
        v.MajorEvery = r.major_every;
        v.Spacing = r.spacing;
        v.Format = Decode(r.format, old.Format);
        v.Unit = Decode(r.unit, old.Unit);
        v.Width = FromNative(r.width, old.Width);
        return v;
    }
    private cui_sankey_link ToNative(SankeyLink v)
    {
        cui_sankey_link r = default;
        r.from = v.From;
        r.to = v.To;
        r.value = v.Value;
        return r;
    }
    private SankeyLink FromNative(cui_sankey_link r, SankeyLink old = default)
    {
        var v = old;
        v.From = r.from;
        v.To = r.to;
        v.Value = r.value;
        return v;
    }
    private cui_sankey_opts ToNative(SankeyOpts v)
    {
        cui_sankey_opts r = default;
        r.width = ToNative(v.Width);
        r.height = v.Height;
        r.format = Utf8(CheckFormat(v.Format, 1));
        r.node_width = v.NodeWidth;
        return r;
    }
    private SankeyOpts FromNative(cui_sankey_opts r, SankeyOpts old = default)
    {
        var v = old;
        v.Width = FromNative(r.width, old.Width);
        v.Height = r.height;
        v.Format = Decode(r.format, old.Format);
        v.NodeWidth = r.node_width;
        return v;
    }
    private cui_scatter_opts ToNative(ScatterOpts v)
    {
        cui_scatter_opts r = default;
        r.width = ToNative(v.Width);
        r.height = v.Height;
        r.x_label = Utf8(v.XLabel);
        r.y_label = Utf8(v.YLabel);
        r.x_format = Utf8(CheckFormat(v.XFormat, 1));
        r.y_format = Utf8(CheckFormat(v.YFormat, 1));
        r.no_legend = v.NoLegend;
        return r;
    }
    private ScatterOpts FromNative(cui_scatter_opts r, ScatterOpts old = default)
    {
        var v = old;
        v.Width = FromNative(r.width, old.Width);
        v.Height = r.height;
        v.XLabel = Decode(r.x_label, old.XLabel);
        v.YLabel = Decode(r.y_label, old.YLabel);
        v.XFormat = Decode(r.x_format, old.XFormat);
        v.YFormat = Decode(r.y_format, old.YFormat);
        v.NoLegend = (r.no_legend != 0);
        return v;
    }
    private cui_scatter_series ToNative(ScatterSeries v)
    {
        Validate(v);
        cui_scatter_series r = default;
        r.name = Utf8(v.Name);
        r.points = CopyVec2(v.Points.Span);
        r.sizes = CopyFloat(v.Sizes.Span);
        r.count = (int)v.Points.Length;
        r.color = ToNative(v.Color);
        return r;
    }
    private ScatterSeries FromNative(cui_scatter_series r, ScatterSeries old = default)
    {
        var v = old;
        v.Name = Decode(r.name, old.Name);
        v.Color = FromNative(r.color, old.Color);
        return v;
    }
    private cui_schedule_opts ToNative(ScheduleOpts v)
    {
        cui_schedule_opts r = default;
        r.width = ToNative(v.Width);
        r.now = v.Now;
        r.utc = v.Utc;
        r.next_count = v.NextCount;
        return r;
    }
    private ScheduleOpts FromNative(cui_schedule_opts r, ScheduleOpts old = default)
    {
        var v = old;
        v.Width = FromNative(r.width, old.Width);
        v.Now = r.now;
        v.Utc = (r.utc != 0);
        v.NextCount = r.next_count;
        return v;
    }
    private cui_schedule_result ToNative(ScheduleResult v)
    {
        cui_schedule_result r = default;
        r.changed = v.Changed;
        r.valid = v.Valid;
        r.next = v.Next;
        return r;
    }
    private ScheduleResult FromNative(cui_schedule_result r, ScheduleResult old = default)
    {
        var v = old;
        v.Changed = (r.changed != 0);
        v.Valid = (r.valid != 0);
        v.Next = r.next;
        return v;
    }
    private cui_scroll_opts ToNative(ScrollOpts v)
    {
        cui_scroll_opts r = default;
        r.id = Utf8(v.Id);
        r.width = ToNative(v.Width);
        r.height = ToNative(v.Height);
        r.horizontal = v.Horizontal;
        r.pad = v.Pad;
        r.gap = v.Gap;
        r.fade_edges = v.FadeEdges;
        r.fade_color = ToNative(v.FadeColor);
        return r;
    }
    private ScrollOpts FromNative(cui_scroll_opts r, ScrollOpts old = default)
    {
        var v = old;
        v.Id = Decode(r.id, old.Id);
        v.Width = FromNative(r.width, old.Width);
        v.Height = FromNative(r.height, old.Height);
        v.Horizontal = (r.horizontal != 0);
        v.Pad = r.pad;
        v.Gap = r.gap;
        v.FadeEdges = (r.fade_edges != 0);
        v.FadeColor = FromNative(r.fade_color, old.FadeColor);
        return v;
    }
    private cui_scrub_opts ToNative(ScrubOpts v)
    {
        cui_scrub_opts r = default;
        r.step = v.Step;
        r.format = Utf8(CheckFormat(v.Format, 1));
        r.suffix = Utf8(v.Suffix);
        r.icon = Utf8(v.Icon);
        r.width = ToNative(v.Width);
        return r;
    }
    private ScrubOpts FromNative(cui_scrub_opts r, ScrubOpts old = default)
    {
        var v = old;
        v.Step = r.step;
        v.Format = Decode(r.format, old.Format);
        v.Suffix = Decode(r.suffix, old.Suffix);
        v.Icon = Decode(r.icon, old.Icon);
        v.Width = FromNative(r.width, old.Width);
        return v;
    }
    private cui_segmented_opts ToNative(SegmentedOpts v)
    {
        cui_segmented_opts r = default;
        r.size = (cui_control_size)CheckEnum(v.Size);
        r.full_width = v.FullWidth;
        r.icons = Strings(v.Icons.Span);
        return r;
    }
    private SegmentedOpts FromNative(cui_segmented_opts r, SegmentedOpts old = default)
    {
        var v = old;
        v.Size = (ControlSize)r.size;
        v.FullWidth = (r.full_width != 0);
        return v;
    }
    private cui_select_opts ToNative(SelectOpts v)
    {
        cui_select_opts r = default;
        r.label = Utf8(v.Label);
        r.placeholder = Utf8(v.Placeholder);
        r.icons = Strings(v.Icons.Span);
        r.searchable = v.Searchable;
        r.disabled = v.Disabled;
        r.width = ToNative(v.Width);
        return r;
    }
    private SelectOpts FromNative(cui_select_opts r, SelectOpts old = default)
    {
        var v = old;
        v.Label = Decode(r.label, old.Label);
        v.Placeholder = Decode(r.placeholder, old.Placeholder);
        v.Searchable = (r.searchable != 0);
        v.Disabled = (r.disabled != 0);
        v.Width = FromNative(r.width, old.Width);
        return v;
    }
    private cui_series ToNative(Series v)
    {
        cui_series r = default;
        r.name = Utf8(v.Name);
        r.values = CopyFloat(v.Values.Span);
        r.count = (int)v.Values.Length;
        r.color = ToNative(v.Color);
        return r;
    }
    private Series FromNative(cui_series r, Series old = default)
    {
        var v = old;
        v.Name = Decode(r.name, old.Name);
        v.Color = FromNative(r.color, old.Color);
        return v;
    }
    private cui_sheet_opts ToNative(SheetOpts v)
    {
        cui_sheet_opts r = default;
        r.side = (cui_side)CheckEnum(v.Side);
        r.size = v.Size;
        r.description = Utf8(v.Description);
        return r;
    }
    private SheetOpts FromNative(cui_sheet_opts r, SheetOpts old = default)
    {
        var v = old;
        v.Side = (Side)r.side;
        v.Size = r.size;
        v.Description = Decode(r.description, old.Description);
        return v;
    }
    private cui_shortcut_recorder_opts ToNative(ShortcutRecorderOpts v)
    {
        cui_shortcut_recorder_opts r = default;
        r.placeholder = Utf8(v.Placeholder);
        r.width = ToNative(v.Width);
        return r;
    }
    private ShortcutRecorderOpts FromNative(cui_shortcut_recorder_opts r, ShortcutRecorderOpts old = default)
    {
        var v = old;
        v.Placeholder = Decode(r.placeholder, old.Placeholder);
        v.Width = FromNative(r.width, old.Width);
        return v;
    }
    private cui_signature_opts ToNative(SignatureOpts v)
    {
        cui_signature_opts r = default;
        r.width = ToNative(v.Width);
        r.height = v.Height;
        r.placeholder = Utf8(v.Placeholder);
        r.ink = ToNative(v.Ink);
        return r;
    }
    private SignatureOpts FromNative(cui_signature_opts r, SignatureOpts old = default)
    {
        var v = old;
        v.Width = FromNative(r.width, old.Width);
        v.Height = r.height;
        v.Placeholder = Decode(r.placeholder, old.Placeholder);
        v.Ink = FromNative(r.ink, old.Ink);
        return v;
    }
    private cui_size ToNative(Size v)
    {
        Validate(v);
        cui_size r = default;
        r.kind = (cui_size_kind)CheckEnum(v.Kind);
        r.value = v.Value;
        return r;
    }
    private Size FromNative(cui_size r, Size old = default)
    {
        var v = old;
        v.Kind = (SizeKind)r.kind;
        v.Value = r.value;
        return v;
    }
    private cui_slider_opts ToNative(SliderOpts v)
    {
        cui_slider_opts r = default;
        r.step = v.Step;
        r.show_value = v.ShowValue;
        r.format = Utf8(CheckFormat(v.Format, 1));
        r.disabled = v.Disabled;
        r.width = ToNative(v.Width);
        return r;
    }
    private SliderOpts FromNative(cui_slider_opts r, SliderOpts old = default)
    {
        var v = old;
        v.Step = r.step;
        v.ShowValue = (r.show_value != 0);
        v.Format = Decode(r.format, old.Format);
        v.Disabled = (r.disabled != 0);
        v.Width = FromNative(r.width, old.Width);
        return v;
    }
    private cui_source ToNative(Source v)
    {
        cui_source r = default;
        r.title = Utf8(v.Title);
        r.url = Utf8(v.Url);
        r.snippet = Utf8(v.Snippet);
        return r;
    }
    private Source FromNative(cui_source r, Source old = default)
    {
        var v = old;
        v.Title = Decode(r.title, old.Title);
        v.Url = Decode(r.url, old.Url);
        v.Snippet = Decode(r.snippet, old.Snippet);
        return v;
    }
    private cui_speed_action ToNative(SpeedAction v)
    {
        cui_speed_action r = default;
        r.icon = Utf8(v.Icon);
        r.label = Utf8(v.Label);
        return r;
    }
    private SpeedAction FromNative(cui_speed_action r, SpeedAction old = default)
    {
        var v = old;
        v.Icon = Decode(r.icon, old.Icon);
        v.Label = Decode(r.label, old.Label);
        return v;
    }
    private cui_speed_dial_opts ToNative(SpeedDialOpts v)
    {
        cui_speed_dial_opts r = default;
        r.icon = Utf8(v.Icon);
        r.placement = (cui_placement)CheckEnum(v.Placement);
        r.tooltip = Utf8(v.Tooltip);
        return r;
    }
    private SpeedDialOpts FromNative(cui_speed_dial_opts r, SpeedDialOpts old = default)
    {
        var v = old;
        v.Icon = Decode(r.icon, old.Icon);
        v.Placement = (Placement)r.placement;
        v.Tooltip = Decode(r.tooltip, old.Tooltip);
        return v;
    }
    private cui_split_bar_opts ToNative(SplitBarOpts v)
    {
        cui_split_bar_opts r = default;
        r.width = ToNative(v.Width);
        r.height = v.Height;
        r.format = Utf8(CheckFormat(v.Format, 1));
        r.no_legend = v.NoLegend;
        return r;
    }
    private SplitBarOpts FromNative(cui_split_bar_opts r, SplitBarOpts old = default)
    {
        var v = old;
        v.Width = FromNative(r.width, old.Width);
        v.Height = r.height;
        v.Format = Decode(r.format, old.Format);
        v.NoLegend = (r.no_legend != 0);
        return v;
    }
    private cui_split_button_opts ToNative(SplitButtonOpts v)
    {
        cui_split_button_opts r = default;
        r.variant = (cui_button_variant)CheckEnum(v.Variant);
        r.size = (cui_control_size)CheckEnum(v.Size);
        r.icon = Utf8(v.Icon);
        r.item_icons = Strings(v.ItemIcons.Span);
        r.disabled = v.Disabled;
        return r;
    }
    private SplitButtonOpts FromNative(cui_split_button_opts r, SplitButtonOpts old = default)
    {
        var v = old;
        v.Variant = (ButtonVariant)r.variant;
        v.Size = (ControlSize)r.size;
        v.Icon = Decode(r.icon, old.Icon);
        v.Disabled = (r.disabled != 0);
        return v;
    }
    private cui_spotlight_opts ToNative(SpotlightOpts v)
    {
        cui_spotlight_opts r = default;
        r.width = ToNative(v.Width);
        r.height = ToNative(v.Height);
        r.pad = v.Pad;
        r.gap = v.Gap;
        r.color = ToNative(v.Color);
        r.beam = v.Beam;
        return r;
    }
    private SpotlightOpts FromNative(cui_spotlight_opts r, SpotlightOpts old = default)
    {
        var v = old;
        v.Width = FromNative(r.width, old.Width);
        v.Height = FromNative(r.height, old.Height);
        v.Pad = r.pad;
        v.Gap = r.gap;
        v.Color = FromNative(r.color, old.Color);
        v.Beam = (r.beam != 0);
        return v;
    }
    private cui_stack_frame ToNative(StackFrame v)
    {
        cui_stack_frame r = default;
        r.function = Utf8(v.Function);
        r.file = Utf8(v.File);
        r.line = v.Line;
        r.column = v.Column;
        r.external = v.External;
        r.source = Strings(v.Source.Span);
        r.source_count = (int)v.Source.Length;
        r.source_first = v.SourceFirst;
        return r;
    }
    private StackFrame FromNative(cui_stack_frame r, StackFrame old = default)
    {
        var v = old;
        v.Function = Decode(r.function, old.Function);
        v.File = Decode(r.file, old.File);
        v.Line = r.line;
        v.Column = r.column;
        v.External = (r.external != 0);
        v.SourceFirst = r.source_first;
        return v;
    }
    private cui_stack_trace_opts ToNative(StackTraceOpts v)
    {
        cui_stack_trace_opts r = default;
        r.width = ToNative(v.Width);
        r.title = Utf8(v.Title);
        r.message = Utf8(v.Message);
        r.lang = Utf8(v.Lang);
        return r;
    }
    private StackTraceOpts FromNative(cui_stack_trace_opts r, StackTraceOpts old = default)
    {
        var v = old;
        v.Width = FromNative(r.width, old.Width);
        v.Title = Decode(r.title, old.Title);
        v.Message = Decode(r.message, old.Message);
        v.Lang = Decode(r.lang, old.Lang);
        return v;
    }
    private cui_stack_trace_result ToNative(StackTraceResult v)
    {
        cui_stack_trace_result r = default;
        r.clicked = v.Clicked;
        return r;
    }
    private StackTraceResult FromNative(cui_stack_trace_result r, StackTraceResult old = default)
    {
        var v = old;
        v.Clicked = r.clicked;
        return v;
    }
    private cui_status_button_opts ToNative(StatusButtonOpts v)
    {
        cui_status_button_opts r = default;
        r.busy_label = Utf8(v.BusyLabel);
        r.success_label = Utf8(v.SuccessLabel);
        r.error_label = Utf8(v.ErrorLabel);
        r.icon = Utf8(v.Icon);
        r.variant = (cui_button_variant)CheckEnum(v.Variant);
        r.size = (cui_control_size)CheckEnum(v.Size);
        return r;
    }
    private StatusButtonOpts FromNative(cui_status_button_opts r, StatusButtonOpts old = default)
    {
        var v = old;
        v.BusyLabel = Decode(r.busy_label, old.BusyLabel);
        v.SuccessLabel = Decode(r.success_label, old.SuccessLabel);
        v.ErrorLabel = Decode(r.error_label, old.ErrorLabel);
        v.Icon = Decode(r.icon, old.Icon);
        v.Variant = (ButtonVariant)r.variant;
        v.Size = (ControlSize)r.size;
        return v;
    }
    private cui_swipe_action ToNative(SwipeAction v)
    {
        cui_swipe_action r = default;
        r.icon = Utf8(v.Icon);
        r.label = Utf8(v.Label);
        r.tone = (cui_tone)CheckEnum(v.Tone);
        return r;
    }
    private SwipeAction FromNative(cui_swipe_action r, SwipeAction old = default)
    {
        var v = old;
        v.Icon = Decode(r.icon, old.Icon);
        v.Label = Decode(r.label, old.Label);
        v.Tone = (Tone)r.tone;
        return v;
    }
    private cui_tab_bar_item ToNative(TabBarItem v)
    {
        cui_tab_bar_item r = default;
        r.icon = Utf8(v.Icon);
        r.label = Utf8(v.Label);
        r.badge = v.Badge;
        return r;
    }
    private TabBarItem FromNative(cui_tab_bar_item r, TabBarItem old = default)
    {
        var v = old;
        v.Icon = Decode(r.icon, old.Icon);
        v.Label = Decode(r.label, old.Label);
        v.Badge = r.badge;
        return v;
    }
    private cui_tab_bar_opts ToNative(TabBarOpts v)
    {
        cui_tab_bar_opts r = default;
        r.width = ToNative(v.Width);
        r.labels_on_selected_only = v.LabelsOnSelectedOnly;
        return r;
    }
    private TabBarOpts FromNative(cui_tab_bar_opts r, TabBarOpts old = default)
    {
        var v = old;
        v.Width = FromNative(r.width, old.Width);
        v.LabelsOnSelectedOnly = (r.labels_on_selected_only != 0);
        return v;
    }
    private cui_table_column ToNative(TableColumn v)
    {
        cui_table_column r = default;
        r.title = Utf8(v.Title);
        r.width = ToNative(v.Width);
        r.align = (cui_text_align)CheckEnum(v.Align);
        r.sortable = v.Sortable;
        return r;
    }
    private TableColumn FromNative(cui_table_column r, TableColumn old = default)
    {
        var v = old;
        v.Title = Decode(r.title, old.Title);
        v.Width = FromNative(r.width, old.Width);
        v.Align = (TextAlign)r.align;
        v.Sortable = (r.sortable != 0);
        return v;
    }
    private cui_table_opts ToNative(TableOpts v)
    {
        cui_table_opts r = default;
        r.sort = BindTableSort(v.Sort);
        r.height = ToNative(v.Height);
        r.striped = v.Striped;
        r.compact = v.Compact;
        r.selectable = v.Selectable;
        return r;
    }
    private TableOpts FromNative(cui_table_opts r, TableOpts old = default)
    {
        var v = old;
        v.Height = FromNative(r.height, old.Height);
        v.Striped = (r.striped != 0);
        v.Compact = (r.compact != 0);
        v.Selectable = (r.selectable != 0);
        return v;
    }
    private cui_table_sort ToNative(TableSort v)
    {
        cui_table_sort r = default;
        r.column = v.Column;
        r.descending = v.Descending;
        return r;
    }
    private TableSort FromNative(cui_table_sort r, TableSort old = default)
    {
        var v = old;
        v.Column = r.column;
        v.Descending = (r.descending != 0);
        return v;
    }
    private cui_tabs_opts ToNative(TabsOpts v)
    {
        cui_tabs_opts r = default;
        r.variant = (cui_tabs_variant)CheckEnum(v.Variant);
        r.icons = Strings(v.Icons.Span);
        r.full_width = v.FullWidth;
        return r;
    }
    private TabsOpts FromNative(cui_tabs_opts r, TabsOpts old = default)
    {
        var v = old;
        v.Variant = (TabsVariant)r.variant;
        v.FullWidth = (r.full_width != 0);
        return v;
    }
    private cui_tag_opts ToNative(TagOpts v)
    {
        cui_tag_opts r = default;
        r.label = Utf8(v.Label);
        r.placeholder = Utf8(v.Placeholder);
        r.max_tags = v.MaxTags;
        r.width = ToNative(v.Width);
        return r;
    }
    private TagOpts FromNative(cui_tag_opts r, TagOpts old = default)
    {
        var v = old;
        v.Label = Decode(r.label, old.Label);
        v.Placeholder = Decode(r.placeholder, old.Placeholder);
        v.MaxTags = r.max_tags;
        v.Width = FromNative(r.width, old.Width);
        return v;
    }
    private cui_terminal_line ToNative(TerminalLine v)
    {
        cui_terminal_line r = default;
        r.offset = v.Offset;
        r.len = v.Len;
        r.cols = v.Cols;
        return r;
    }
    private TerminalLine FromNative(cui_terminal_line r, TerminalLine old = default)
    {
        var v = old;
        v.Offset = r.offset;
        v.Len = r.len;
        v.Cols = r.cols;
        return v;
    }
    private cui_terminal_opts ToNative(TerminalOpts v)
    {
        cui_terminal_opts r = default;
        r.width = ToNative(v.Width);
        r.height = v.Height;
        r.size = v.Size;
        r.prompt = Utf8(v.Prompt);
        r.no_input = v.NoInput;
        return r;
    }
    private TerminalOpts FromNative(cui_terminal_opts r, TerminalOpts old = default)
    {
        var v = old;
        v.Width = FromNative(r.width, old.Width);
        v.Height = r.height;
        v.Size = r.size;
        v.Prompt = Decode(r.prompt, old.Prompt);
        v.NoInput = (r.no_input != 0);
        return v;
    }
    private cui_terminal_result ToNative(TerminalResult v)
    {
        cui_terminal_result r = default;
        r.command = Utf8(v.Command);
        r.interrupt = v.Interrupt;
        return r;
    }
    private TerminalResult FromNative(cui_terminal_result r, TerminalResult old = default)
    {
        var v = old;
        v.Command = Decode(r.command, old.Command);
        v.Interrupt = (r.interrupt != 0);
        return v;
    }
    private cui_test_explorer_opts ToNative(TestExplorerOpts v)
    {
        cui_test_explorer_opts r = default;
        r.width = ToNative(v.Width);
        r.height = v.Height;
        return r;
    }
    private TestExplorerOpts FromNative(cui_test_explorer_opts r, TestExplorerOpts old = default)
    {
        var v = old;
        v.Width = FromNative(r.width, old.Width);
        v.Height = r.height;
        return v;
    }
    private cui_test_explorer_result ToNative(TestExplorerResult v)
    {
        cui_test_explorer_result r = default;
        r.selected = v.Selected;
        r.activated = v.Activated;
        r.run = v.Run;
        r.run_all = v.RunAll;
        r.passed = v.Passed;
        r.failed = v.Failed;
        r.skipped = v.Skipped;
        r.total = v.Total;
        return r;
    }
    private TestExplorerResult FromNative(cui_test_explorer_result r, TestExplorerResult old = default)
    {
        var v = old;
        v.Selected = r.selected;
        v.Activated = r.activated;
        v.Run = r.run;
        v.RunAll = (r.run_all != 0);
        v.Passed = r.passed;
        v.Failed = r.failed;
        v.Skipped = r.skipped;
        v.Total = r.total;
        return v;
    }
    private cui_test_item ToNative(TestItem v)
    {
        cui_test_item r = default;
        r.name = Utf8(v.Name);
        r.depth = v.Depth;
        r.status = (cui_job_status)CheckEnum(v.Status);
        r.duration = v.Duration;
        r.message = Utf8(v.Message);
        return r;
    }
    private TestItem FromNative(cui_test_item r, TestItem old = default)
    {
        var v = old;
        v.Name = Decode(r.name, old.Name);
        v.Depth = r.depth;
        v.Status = (JobStatus)r.status;
        v.Duration = r.duration;
        v.Message = Decode(r.message, old.Message);
        return v;
    }
    private cui_text_fx_opts ToNative(TextFxOpts v)
    {
        cui_text_fx_opts r = default;
        r.font = (int)CheckEnum(v.Font);
        r.size = v.Size;
        r.color = ToNative(v.Color);
        r.width = ToNative(v.Width);
        r.nowrap = v.Nowrap;
        r.speed = v.Speed;
        r.delay = v.Delay;
        r.caret = v.Caret;
        return r;
    }
    private TextFxOpts FromNative(cui_text_fx_opts r, TextFxOpts old = default)
    {
        var v = old;
        v.Font = (Font)r.font;
        v.Size = r.size;
        v.Color = FromNative(r.color, old.Color);
        v.Width = FromNative(r.width, old.Width);
        v.Nowrap = (r.nowrap != 0);
        v.Speed = r.speed;
        v.Delay = r.delay;
        v.Caret = (r.caret != 0);
        return v;
    }
    private cui_text_opts ToNative(TextOpts v)
    {
        cui_text_opts r = default;
        r.font = (int)CheckEnum(v.Font);
        r.size = v.Size;
        r.color = ToNative(v.Color);
        r.align = (cui_text_align)CheckEnum(v.Align);
        r.nowrap = v.Nowrap;
        r.tabular = v.Tabular;
        r.selectable = v.Selectable;
        r.width = ToNative(v.Width);
        r.line_height = v.LineHeight;
        r.direction = (cui_direction)CheckEnum(v.Direction);
        return r;
    }
    private TextOpts FromNative(cui_text_opts r, TextOpts old = default)
    {
        var v = old;
        v.Font = (Font)r.font;
        v.Size = r.size;
        v.Color = FromNative(r.color, old.Color);
        v.Align = (TextAlign)r.align;
        v.Nowrap = (r.nowrap != 0);
        v.Tabular = (r.tabular != 0);
        v.Selectable = (r.selectable != 0);
        v.Width = FromNative(r.width, old.Width);
        v.LineHeight = r.line_height;
        v.Direction = (Direction)r.direction;
        return v;
    }
    private cui_ticker_item ToNative(TickerItem v)
    {
        cui_ticker_item r = default;
        r.label = Utf8(v.Label);
        r.value = Utf8(v.Value);
        r.delta = v.Delta;
        r.lower_is_better = v.LowerIsBetter;
        return r;
    }
    private TickerItem FromNative(cui_ticker_item r, TickerItem old = default)
    {
        var v = old;
        v.Label = Decode(r.label, old.Label);
        v.Value = Decode(r.value, old.Value);
        v.Delta = r.delta;
        v.LowerIsBetter = (r.lower_is_better != 0);
        return v;
    }
    private cui_ticker_opts ToNative(TickerOpts v)
    {
        cui_ticker_opts r = default;
        r.speed = v.Speed;
        r.height = v.Height;
        return r;
    }
    private TickerOpts FromNative(cui_ticker_opts r, TickerOpts old = default)
    {
        var v = old;
        v.Speed = r.speed;
        v.Height = r.height;
        return v;
    }
    private cui_tile_opts ToNative(TileOpts v)
    {
        cui_tile_opts r = default;
        r.icon = Utf8(v.Icon);
        r.detail = Utf8(v.Detail);
        r.color = ToNative(v.Color);
        r.width = ToNative(v.Width);
        r.wide = v.Wide;
        return r;
    }
    private TileOpts FromNative(cui_tile_opts r, TileOpts old = default)
    {
        var v = old;
        v.Icon = Decode(r.icon, old.Icon);
        v.Detail = Decode(r.detail, old.Detail);
        v.Color = FromNative(r.color, old.Color);
        v.Width = FromNative(r.width, old.Width);
        v.Wide = (r.wide != 0);
        return v;
    }
    private cui_time_histogram_opts ToNative(TimeHistogramOpts v)
    {
        cui_time_histogram_opts r = default;
        r.width = ToNative(v.Width);
        r.height = v.Height;
        r.utc = v.Utc;
        return r;
    }
    private TimeHistogramOpts FromNative(cui_time_histogram_opts r, TimeHistogramOpts old = default)
    {
        var v = old;
        v.Width = FromNative(r.width, old.Width);
        v.Height = r.height;
        v.Utc = (r.utc != 0);
        return v;
    }
    private cui_time_histogram_result ToNative(TimeHistogramResult v)
    {
        cui_time_histogram_result r = default;
        r.selection_changed = v.SelectionChanged;
        r.hovered = v.Hovered;
        return r;
    }
    private TimeHistogramResult FromNative(cui_time_histogram_result r, TimeHistogramResult old = default)
    {
        var v = old;
        v.SelectionChanged = (r.selection_changed != 0);
        v.Hovered = r.hovered;
        return v;
    }
    private cui_time_opts ToNative(TimeOpts v)
    {
        cui_time_opts r = default;
        r.label = Utf8(v.Label);
        r.twelve_hour = v.TwelveHour;
        r.minute_step = v.MinuteStep;
        r.width = ToNative(v.Width);
        return r;
    }
    private TimeOpts FromNative(cui_time_opts r, TimeOpts old = default)
    {
        var v = old;
        v.Label = Decode(r.label, old.Label);
        v.TwelveHour = (r.twelve_hour != 0);
        v.MinuteStep = r.minute_step;
        v.Width = FromNative(r.width, old.Width);
        return v;
    }
    private cui_timeline_opts ToNative(TimelineOpts v)
    {
        cui_timeline_opts r = default;
        r.time = Utf8(v.Time);
        r.description = Utf8(v.Description);
        r.icon = Utf8(v.Icon);
        r.tone = (cui_tone)CheckEnum(v.Tone);
        r.last = v.Last;
        return r;
    }
    private TimelineOpts FromNative(cui_timeline_opts r, TimelineOpts old = default)
    {
        var v = old;
        v.Time = Decode(r.time, old.Time);
        v.Description = Decode(r.description, old.Description);
        v.Icon = Decode(r.icon, old.Icon);
        v.Tone = (Tone)r.tone;
        v.Last = (r.last != 0);
        return v;
    }
    private cui_toast_opts ToNative(ToastOpts v)
    {
        cui_toast_opts r = default;
        r.description = Utf8(v.Description);
        r.tone = (cui_tone)CheckEnum(v.Tone);
        r.duration = v.Duration;
        r.action = Utf8(v.Action);
        r.show_timer = v.ShowTimer;
        return r;
    }
    private ToastOpts FromNative(cui_toast_opts r, ToastOpts old = default)
    {
        var v = old;
        v.Description = Decode(r.description, old.Description);
        v.Tone = (Tone)r.tone;
        v.Duration = r.duration;
        v.Action = Decode(r.action, old.Action);
        v.ShowTimer = (r.show_timer != 0);
        return v;
    }
    private cui_toc_item ToNative(TocItem v)
    {
        cui_toc_item r = default;
        r.title = Utf8(v.Title);
        r.anchor = Utf8(v.Anchor);
        r.level = v.Level;
        return r;
    }
    private TocItem FromNative(cui_toc_item r, TocItem old = default)
    {
        var v = old;
        v.Title = Decode(r.title, old.Title);
        v.Anchor = Decode(r.anchor, old.Anchor);
        v.Level = r.level;
        return v;
    }
    private cui_toggle_group_opts ToNative(ToggleGroupOpts v)
    {
        cui_toggle_group_opts r = default;
        r.icons = Strings(v.Icons.Span);
        r.exclusive = v.Exclusive;
        r.icon_only = v.IconOnly;
        r.size = (cui_control_size)CheckEnum(v.Size);
        return r;
    }
    private ToggleGroupOpts FromNative(cui_toggle_group_opts r, ToggleGroupOpts old = default)
    {
        var v = old;
        v.Exclusive = (r.exclusive != 0);
        v.IconOnly = (r.icon_only != 0);
        v.Size = (ControlSize)r.size;
        return v;
    }
    private cui_tool_call_opts ToNative(ToolCallOpts v)
    {
        cui_tool_call_opts r = default;
        r.status = (cui_task_status)CheckEnum(v.Status);
        r.icon = Utf8(v.Icon);
        r.detail = Utf8(v.Detail);
        r.input = Utf8(v.Input);
        r.output = Utf8(v.Output);
        r.seconds = v.Seconds;
        return r;
    }
    private ToolCallOpts FromNative(cui_tool_call_opts r, ToolCallOpts old = default)
    {
        var v = old;
        v.Status = (TaskStatus)r.status;
        v.Icon = Decode(r.icon, old.Icon);
        v.Detail = Decode(r.detail, old.Detail);
        v.Input = Decode(r.input, old.Input);
        v.Output = Decode(r.output, old.Output);
        v.Seconds = r.seconds;
        return v;
    }
    private cui_tour_opts ToNative(TourOpts v)
    {
        cui_tour_opts r = default;
        r.width = v.Width;
        r.padding = v.Padding;
        r.block_target = v.BlockTarget;
        r.next_label = Utf8(v.NextLabel);
        r.back_label = Utf8(v.BackLabel);
        r.done_label = Utf8(v.DoneLabel);
        r.count_format = Utf8(CheckFormat(v.CountFormat, 2));
        return r;
    }
    private TourOpts FromNative(cui_tour_opts r, TourOpts old = default)
    {
        var v = old;
        v.Width = r.width;
        v.Padding = r.padding;
        v.BlockTarget = (r.block_target != 0);
        v.NextLabel = Decode(r.next_label, old.NextLabel);
        v.BackLabel = Decode(r.back_label, old.BackLabel);
        v.DoneLabel = Decode(r.done_label, old.DoneLabel);
        v.CountFormat = Decode(r.count_format, old.CountFormat);
        return v;
    }
    private cui_tour_step ToNative(TourStep v)
    {
        cui_tour_step r = default;
        r.target = Unwrap(v.Target);
        r.title = Utf8(v.Title);
        r.body = Utf8(v.Body);
        r.placement = (cui_placement)CheckEnum(v.Placement);
        return r;
    }
    private TourStep FromNative(cui_tour_step r, TourStep old = default)
    {
        var v = old;
        v.Target = new Id(this, r.target);
        v.Title = Decode(r.title, old.Title);
        v.Body = Decode(r.body, old.Body);
        v.Placement = (Placement)r.placement;
        return v;
    }
    private cui_trace_marker ToNative(TraceMarker v)
    {
        cui_trace_marker r = default;
        r.name = Utf8(v.Name);
        r.time = v.Time;
        r.tone = (cui_tone)CheckEnum(v.Tone);
        return r;
    }
    private TraceMarker FromNative(cui_trace_marker r, TraceMarker old = default)
    {
        var v = old;
        v.Name = Decode(r.name, old.Name);
        v.Time = r.time;
        v.Tone = (Tone)r.tone;
        return v;
    }
    private cui_trace_opts ToNative(TraceOpts v)
    {
        cui_trace_opts r = default;
        r.width = ToNative(v.Width);
        r.height = v.Height;
        r.row_height = v.RowHeight;
        r.markers = CopyTraceMarker(v.Markers.Span);
        r.marker_count = (int)v.Markers.Length;
        return r;
    }
    private TraceOpts FromNative(cui_trace_opts r, TraceOpts old = default)
    {
        var v = old;
        v.Width = FromNative(r.width, old.Width);
        v.Height = r.height;
        v.RowHeight = r.row_height;
        return v;
    }
    private cui_trace_result ToNative(TraceResult v)
    {
        cui_trace_result r = default;
        r.selection_changed = v.SelectionChanged;
        r.view_changed = v.ViewChanged;
        r.hovered = v.Hovered;
        return r;
    }
    private TraceResult FromNative(cui_trace_result r, TraceResult old = default)
    {
        var v = old;
        v.SelectionChanged = (r.selection_changed != 0);
        v.ViewChanged = (r.view_changed != 0);
        v.Hovered = r.hovered;
        return v;
    }
    private cui_trace_span ToNative(TraceSpan v)
    {
        cui_trace_span r = default;
        r.name = Utf8(v.Name);
        r.start = v.Start;
        r.end = v.End;
        r.track = v.Track;
        r.depth = v.Depth;
        r.color_key = v.ColorKey;
        return r;
    }
    private TraceSpan FromNative(cui_trace_span r, TraceSpan old = default)
    {
        var v = old;
        v.Name = Decode(r.name, old.Name);
        v.Start = r.start;
        v.End = r.end;
        v.Track = r.track;
        v.Depth = r.depth;
        v.ColorKey = r.color_key;
        return v;
    }
    private cui_trace_track ToNative(TraceTrack v)
    {
        cui_trace_track r = default;
        r.name = Utf8(v.Name);
        r.collapsed = v.Collapsed;
        return r;
    }
    private TraceTrack FromNative(cui_trace_track r, TraceTrack old = default)
    {
        var v = old;
        v.Name = Decode(r.name, old.Name);
        v.Collapsed = (r.collapsed != 0);
        return v;
    }
    private cui_trace_view ToNative(TraceView v)
    {
        cui_trace_view r = default;
        r.start = v.Start;
        r.end = v.End;
        r.scroll = v.Scroll;
        r.mark_start = v.MarkStart;
        r.mark_end = v.MarkEnd;
        return r;
    }
    private TraceView FromNative(cui_trace_view r, TraceView old = default)
    {
        var v = old;
        v.Start = r.start;
        v.End = r.end;
        v.Scroll = r.scroll;
        v.MarkStart = r.mark_start;
        v.MarkEnd = r.mark_end;
        return v;
    }
    private cui_tree_opts ToNative(TreeOpts v)
    {
        cui_tree_opts r = default;
        r.icon = Utf8(v.Icon);
        r.leaf = v.Leaf;
        r.selected = v.Selected;
        return r;
    }
    private TreeOpts FromNative(cui_tree_opts r, TreeOpts old = default)
    {
        var v = old;
        v.Icon = Decode(r.icon, old.Icon);
        v.Leaf = (r.leaf != 0);
        v.Selected = (r.selected != 0);
        return v;
    }
    private cui_treemap_opts ToNative(TreemapOpts v)
    {
        cui_treemap_opts r = default;
        r.width = ToNative(v.Width);
        r.height = v.Height;
        r.format = Utf8(CheckFormat(v.Format, 1));
        return r;
    }
    private TreemapOpts FromNative(cui_treemap_opts r, TreemapOpts old = default)
    {
        var v = old;
        v.Width = FromNative(r.width, old.Width);
        v.Height = r.height;
        v.Format = Decode(r.format, old.Format);
        return v;
    }
    private cui_uptime_opts ToNative(UptimeOpts v)
    {
        cui_uptime_opts r = default;
        r.width = ToNative(v.Width);
        r.last_day = ToNative(v.LastDay);
        return r;
    }
    private UptimeOpts FromNative(cui_uptime_opts r, UptimeOpts old = default)
    {
        var v = old;
        v.Width = FromNative(r.width, old.Width);
        v.LastDay = FromNative(r.last_day, old.LastDay);
        return v;
    }
    private cui_uptime_result ToNative(UptimeResult v)
    {
        cui_uptime_result r = default;
        r.service = v.Service;
        r.day = v.Day;
        return r;
    }
    private UptimeResult FromNative(cui_uptime_result r, UptimeResult old = default)
    {
        var v = old;
        v.Service = r.service;
        v.Day = r.day;
        return v;
    }
    private cui_uptime_service ToNative(UptimeService v)
    {
        cui_uptime_service r = default;
        r.name = Utf8(v.Name);
        r.days = CopyFloat(v.Days.Span);
        r.day_count = (int)v.Days.Length;
        return r;
    }
    private UptimeService FromNative(cui_uptime_service r, UptimeService old = default)
    {
        var v = old;
        v.Name = Decode(r.name, old.Name);
        return v;
    }
    private cui_user_menu_opts ToNative(UserMenuOpts v)
    {
        cui_user_menu_opts r = default;
        r.detail = Utf8(v.Detail);
        r.status = Utf8(v.Status);
        r.avatar = Unwrap(v.Avatar);
        r.compact = v.Compact;
        r.placement = (cui_placement)CheckEnum(v.Placement);
        r.width = v.Width;
        return r;
    }
    private UserMenuOpts FromNative(cui_user_menu_opts r, UserMenuOpts old = default)
    {
        var v = old;
        v.Detail = Decode(r.detail, old.Detail);
        v.Status = Decode(r.status, old.Status);
        v.Avatar = FindTexture(r.avatar);
        v.Compact = (r.compact != 0);
        v.Placement = (Placement)r.placement;
        v.Width = r.width;
        return v;
    }
    private cui_vec2 ToNative(Vec2 v)
    {
        cui_vec2 r = default;
        r.x = v.X;
        r.y = v.Y;
        return r;
    }
    private Vec2 FromNative(cui_vec2 r, Vec2 old = default)
    {
        var v = old;
        v.X = r.x;
        v.Y = r.y;
        return v;
    }
    private cui_viz_item ToNative(VizItem v)
    {
        cui_viz_item r = default;
        r.label = Utf8(v.Label);
        r.value = v.Value;
        r.color = ToNative(v.Color);
        return r;
    }
    private VizItem FromNative(cui_viz_item r, VizItem old = default)
    {
        var v = old;
        v.Label = Decode(r.label, old.Label);
        v.Value = r.value;
        v.Color = FromNative(r.color, old.Color);
        return v;
    }
    private cui_voice_orb_opts ToNative(VoiceOrbOpts v)
    {
        cui_voice_orb_opts r = default;
        r.size = v.Size;
        r.color = ToNative(v.Color);
        return r;
    }
    private VoiceOrbOpts FromNative(cui_voice_orb_opts r, VoiceOrbOpts old = default)
    {
        var v = old;
        v.Size = r.size;
        v.Color = FromNative(r.color, old.Color);
        return v;
    }
    private cui_waffle_opts ToNative(WaffleOpts v)
    {
        cui_waffle_opts r = default;
        r.cell = v.Cell;
        r.columns = v.Columns;
        r.rows = v.Rows;
        r.format = Utf8(CheckFormat(v.Format, 1));
        return r;
    }
    private WaffleOpts FromNative(cui_waffle_opts r, WaffleOpts old = default)
    {
        var v = old;
        v.Cell = r.cell;
        v.Columns = r.columns;
        v.Rows = r.rows;
        v.Format = Decode(r.format, old.Format);
        return v;
    }
    private cui_waterfall_opts ToNative(WaterfallOpts v)
    {
        cui_waterfall_opts r = default;
        r.width = ToNative(v.Width);
        r.height = v.Height;
        r.selected = BindInt(v.Selected);
        r.dom_loaded = v.DomLoaded;
        r.loaded = v.Loaded;
        return r;
    }
    private WaterfallOpts FromNative(cui_waterfall_opts r, WaterfallOpts old = default)
    {
        var v = old;
        v.Width = FromNative(r.width, old.Width);
        v.Height = r.height;
        v.DomLoaded = r.dom_loaded;
        v.Loaded = r.loaded;
        return v;
    }
    private cui_waterfall_result ToNative(WaterfallResult v)
    {
        cui_waterfall_result r = default;
        r.clicked = v.Clicked;
        r.activated = v.Activated;
        r.hovered = v.Hovered;
        return r;
    }
    private WaterfallResult FromNative(cui_waterfall_result r, WaterfallResult old = default)
    {
        var v = old;
        v.Clicked = r.clicked;
        v.Activated = r.activated;
        v.Hovered = r.hovered;
        return v;
    }
    private cui_waveform_opts ToNative(WaveformOpts v)
    {
        cui_waveform_opts r = default;
        r.width = ToNative(v.Width);
        r.height = v.Height;
        r.bar = v.Bar;
        r.gap = v.Gap;
        r.color = ToNative(v.Color);
        return r;
    }
    private WaveformOpts FromNative(cui_waveform_opts r, WaveformOpts old = default)
    {
        var v = old;
        v.Width = FromNative(r.width, old.Width);
        v.Height = r.height;
        v.Bar = r.bar;
        v.Gap = r.gap;
        v.Color = FromNative(r.color, old.Color);
        return v;
    }
    private cui_week_opts ToNative(WeekOpts v)
    {
        cui_week_opts r = default;
        r.start_hour = v.StartHour;
        r.end_hour = v.EndHour;
        r.hour_height = v.HourHeight;
        r.height = ToNative(v.Height);
        r.read_only = v.ReadOnly;
        return r;
    }
    private WeekOpts FromNative(cui_week_opts r, WeekOpts old = default)
    {
        var v = old;
        v.StartHour = r.start_hour;
        v.EndHour = r.end_hour;
        v.HourHeight = r.hour_height;
        v.Height = FromNative(r.height, old.Height);
        v.ReadOnly = (r.read_only != 0);
        return v;
    }
    private cui_wheel_opts ToNative(WheelOpts v)
    {
        cui_wheel_opts r = default;
        r.rows = v.Rows;
        r.width = ToNative(v.Width);
        r.loop = v.Loop;
        r.join_left = v.JoinLeft;
        r.join_right = v.JoinRight;
        return r;
    }
    private WheelOpts FromNative(cui_wheel_opts r, WheelOpts old = default)
    {
        var v = old;
        v.Rows = r.rows;
        v.Width = FromNative(r.width, old.Width);
        v.Loop = (r.loop != 0);
        v.JoinLeft = (r.join_left != 0);
        v.JoinRight = (r.join_right != 0);
        return v;
    }
    private CBool* CopyBool(ReadOnlySpan<bool> values)
    {
        var p = (CBool*)arena.Allocate(checked(values.Length * sizeof(CBool)));
        for (int i = 0; i < values.Length; i++) p[i] = values[i];
        return p;
    }
    private readonly List<(Memory<bool> Memory, nuint Pointer)> journalBool = new();
    private void JournalBool(Memory<bool> memory, CBool* pointer) { journalBool.Add((memory, (nuint)pointer)); arraysDirty = true; }
    private void SyncBool()
    {
        foreach (var entry in journalBool)
        {
            var span = entry.Memory.Span;
            var p = (CBool*)entry.Pointer;
            for (int i = 0; i < span.Length; i++) span[i] = (p[i] != 0);
        }
    }
    private cui_api_endpoint* CopyApiEndpoint(ReadOnlySpan<ApiEndpoint> values)
    {
        var p = (cui_api_endpoint*)arena.Allocate(checked(values.Length * sizeof(cui_api_endpoint)));
        for (int i = 0; i < values.Length; i++) p[i] = ToNative(values[i]);
        return p;
    }
    private readonly List<(Memory<ApiEndpoint> Memory, nuint Pointer)> journalApiEndpoint = new();
    private void JournalApiEndpoint(Memory<ApiEndpoint> memory, cui_api_endpoint* pointer) => journalApiEndpoint.Add((memory, (nuint)pointer));
    private void SyncApiEndpoint()
    {
        foreach (var entry in journalApiEndpoint)
        {
            var span = entry.Memory.Span;
            var p = (cui_api_endpoint*)entry.Pointer;
            for (int i = 0; i < span.Length; i++) span[i] = FromNative(p[i], span[i]);
        }
    }
    private cui_api_param* CopyApiParam(ReadOnlySpan<ApiParam> values)
    {
        var p = (cui_api_param*)arena.Allocate(checked(values.Length * sizeof(cui_api_param)));
        for (int i = 0; i < values.Length; i++) p[i] = ToNative(values[i]);
        return p;
    }
    private readonly List<(Memory<ApiParam> Memory, nuint Pointer)> journalApiParam = new();
    private void JournalApiParam(Memory<ApiParam> memory, cui_api_param* pointer) => journalApiParam.Add((memory, (nuint)pointer));
    private void SyncApiParam()
    {
        foreach (var entry in journalApiParam)
        {
            var span = entry.Memory.Span;
            var p = (cui_api_param*)entry.Pointer;
            for (int i = 0; i < span.Length; i++) span[i] = FromNative(p[i], span[i]);
        }
    }
    private cui_blame_commit* CopyBlameCommit(ReadOnlySpan<BlameCommit> values)
    {
        var p = (cui_blame_commit*)arena.Allocate(checked(values.Length * sizeof(cui_blame_commit)));
        for (int i = 0; i < values.Length; i++) p[i] = ToNative(values[i]);
        return p;
    }
    private readonly List<(Memory<BlameCommit> Memory, nuint Pointer)> journalBlameCommit = new();
    private void JournalBlameCommit(Memory<BlameCommit> memory, cui_blame_commit* pointer) => journalBlameCommit.Add((memory, (nuint)pointer));
    private void SyncBlameCommit()
    {
        foreach (var entry in journalBlameCommit)
        {
            var span = entry.Memory.Span;
            var p = (cui_blame_commit*)entry.Pointer;
            for (int i = 0; i < span.Length; i++) span[i] = FromNative(p[i], span[i]);
        }
    }
    private cui_cal_event* CopyCalEvent(ReadOnlySpan<CalEvent> values)
    {
        var p = (cui_cal_event*)arena.Allocate(checked(values.Length * sizeof(cui_cal_event)));
        for (int i = 0; i < values.Length; i++) p[i] = ToNative(values[i]);
        return p;
    }
    private readonly List<(Memory<CalEvent> Memory, nuint Pointer)> journalCalEvent = new();
    private void JournalCalEvent(Memory<CalEvent> memory, cui_cal_event* pointer) => journalCalEvent.Add((memory, (nuint)pointer));
    private void SyncCalEvent()
    {
        foreach (var entry in journalCalEvent)
        {
            var span = entry.Memory.Span;
            var p = (cui_cal_event*)entry.Pointer;
            for (int i = 0; i < span.Length; i++) span[i] = FromNative(p[i], span[i]);
        }
    }
    private cui_candle* CopyCandle(ReadOnlySpan<Candle> values)
    {
        var p = (cui_candle*)arena.Allocate(checked(values.Length * sizeof(cui_candle)));
        for (int i = 0; i < values.Length; i++) p[i] = ToNative(values[i]);
        return p;
    }
    private readonly List<(Memory<Candle> Memory, nuint Pointer)> journalCandle = new();
    private void JournalCandle(Memory<Candle> memory, cui_candle* pointer) => journalCandle.Add((memory, (nuint)pointer));
    private void SyncCandle()
    {
        foreach (var entry in journalCandle)
        {
            var span = entry.Memory.Span;
            var p = (cui_candle*)entry.Pointer;
            for (int i = 0; i < span.Length; i++) span[i] = FromNative(p[i], span[i]);
        }
    }
    private cui_chart_annotation* CopyChartAnnotation(ReadOnlySpan<ChartAnnotation> values)
    {
        var p = (cui_chart_annotation*)arena.Allocate(checked(values.Length * sizeof(cui_chart_annotation)));
        for (int i = 0; i < values.Length; i++) p[i] = ToNative(values[i]);
        return p;
    }
    private readonly List<(Memory<ChartAnnotation> Memory, nuint Pointer)> journalChartAnnotation = new();
    private void JournalChartAnnotation(Memory<ChartAnnotation> memory, cui_chart_annotation* pointer) => journalChartAnnotation.Add((memory, (nuint)pointer));
    private void SyncChartAnnotation()
    {
        foreach (var entry in journalChartAnnotation)
        {
            var span = entry.Memory.Span;
            var p = (cui_chart_annotation*)entry.Pointer;
            for (int i = 0; i < span.Length; i++) span[i] = FromNative(p[i], span[i]);
        }
    }
    private cui_check_step* CopyCheckStep(ReadOnlySpan<CheckStep> values)
    {
        var p = (cui_check_step*)arena.Allocate(checked(values.Length * sizeof(cui_check_step)));
        for (int i = 0; i < values.Length; i++) p[i] = ToNative(values[i]);
        return p;
    }
    private readonly List<(Memory<CheckStep> Memory, nuint Pointer)> journalCheckStep = new();
    private void JournalCheckStep(Memory<CheckStep> memory, cui_check_step* pointer) => journalCheckStep.Add((memory, (nuint)pointer));
    private void SyncCheckStep()
    {
        foreach (var entry in journalCheckStep)
        {
            var span = entry.Memory.Span;
            var p = (cui_check_step*)entry.Pointer;
            for (int i = 0; i < span.Length; i++) span[i] = FromNative(p[i], span[i]);
        }
    }
    private cui_choice* CopyChoice(ReadOnlySpan<Choice> values)
    {
        var p = (cui_choice*)arena.Allocate(checked(values.Length * sizeof(cui_choice)));
        for (int i = 0; i < values.Length; i++) p[i] = ToNative(values[i]);
        return p;
    }
    private readonly List<(Memory<Choice> Memory, nuint Pointer)> journalChoice = new();
    private void JournalChoice(Memory<Choice> memory, cui_choice* pointer) => journalChoice.Add((memory, (nuint)pointer));
    private void SyncChoice()
    {
        foreach (var entry in journalChoice)
        {
            var span = entry.Memory.Span;
            var p = (cui_choice*)entry.Pointer;
            for (int i = 0; i < span.Length; i++) span[i] = FromNative(p[i], span[i]);
        }
    }
    private cui_command* CopyCommand(ReadOnlySpan<Command> values)
    {
        var p = (cui_command*)arena.Allocate(checked(values.Length * sizeof(cui_command)));
        for (int i = 0; i < values.Length; i++) p[i] = ToNative(values[i]);
        return p;
    }
    private readonly List<(Memory<Command> Memory, nuint Pointer)> journalCommand = new();
    private void JournalCommand(Memory<Command> memory, cui_command* pointer) => journalCommand.Add((memory, (nuint)pointer));
    private void SyncCommand()
    {
        foreach (var entry in journalCommand)
        {
            var span = entry.Memory.Span;
            var p = (cui_command*)entry.Pointer;
            for (int i = 0; i < span.Length; i++) span[i] = FromNative(p[i], span[i]);
        }
    }
    private cui_commit* CopyCommit(ReadOnlySpan<Commit> values)
    {
        var p = (cui_commit*)arena.Allocate(checked(values.Length * sizeof(cui_commit)));
        for (int i = 0; i < values.Length; i++) p[i] = ToNative(values[i]);
        return p;
    }
    private readonly List<(Memory<Commit> Memory, nuint Pointer)> journalCommit = new();
    private void JournalCommit(Memory<Commit> memory, cui_commit* pointer) => journalCommit.Add((memory, (nuint)pointer));
    private void SyncCommit()
    {
        foreach (var entry in journalCommit)
        {
            var span = entry.Memory.Span;
            var p = (cui_commit*)entry.Pointer;
            for (int i = 0; i < span.Length; i++) span[i] = FromNative(p[i], span[i]);
        }
    }
    private cui_coverage_file* CopyCoverageFile(ReadOnlySpan<CoverageFile> values)
    {
        var p = (cui_coverage_file*)arena.Allocate(checked(values.Length * sizeof(cui_coverage_file)));
        for (int i = 0; i < values.Length; i++) p[i] = ToNative(values[i]);
        return p;
    }
    private readonly List<(Memory<CoverageFile> Memory, nuint Pointer)> journalCoverageFile = new();
    private void JournalCoverageFile(Memory<CoverageFile> memory, cui_coverage_file* pointer) => journalCoverageFile.Add((memory, (nuint)pointer));
    private void SyncCoverageFile()
    {
        foreach (var entry in journalCoverageFile)
        {
            var span = entry.Memory.Span;
            var p = (cui_coverage_file*)entry.Pointer;
            for (int i = 0; i < span.Length; i++) span[i] = FromNative(p[i], span[i]);
        }
    }
    private cui_diagnostic* CopyDiagnostic(ReadOnlySpan<Diagnostic> values)
    {
        var p = (cui_diagnostic*)arena.Allocate(checked(values.Length * sizeof(cui_diagnostic)));
        for (int i = 0; i < values.Length; i++) p[i] = ToNative(values[i]);
        return p;
    }
    private readonly List<(Memory<Diagnostic> Memory, nuint Pointer)> journalDiagnostic = new();
    private void JournalDiagnostic(Memory<Diagnostic> memory, cui_diagnostic* pointer) => journalDiagnostic.Add((memory, (nuint)pointer));
    private void SyncDiagnostic()
    {
        foreach (var entry in journalDiagnostic)
        {
            var span = entry.Memory.Span;
            var p = (cui_diagnostic*)entry.Pointer;
            for (int i = 0; i < span.Length; i++) span[i] = FromNative(p[i], span[i]);
        }
    }
    private cui_dot_series* CopyDotSeries(ReadOnlySpan<DotSeries> values)
    {
        var p = (cui_dot_series*)arena.Allocate(checked(values.Length * sizeof(cui_dot_series)));
        for (int i = 0; i < values.Length; i++) p[i] = ToNative(values[i]);
        return p;
    }
    private readonly List<(Memory<DotSeries> Memory, nuint Pointer)> journalDotSeries = new();
    private void JournalDotSeries(Memory<DotSeries> memory, cui_dot_series* pointer) => journalDotSeries.Add((memory, (nuint)pointer));
    private void SyncDotSeries()
    {
        foreach (var entry in journalDotSeries)
        {
            var span = entry.Memory.Span;
            var p = (cui_dot_series*)entry.Pointer;
            for (int i = 0; i < span.Length; i++) span[i] = FromNative(p[i], span[i]);
        }
    }
    private cui_feature_flag* CopyFeatureFlag(ReadOnlySpan<FeatureFlag> values)
    {
        var p = (cui_feature_flag*)arena.Allocate(checked(values.Length * sizeof(cui_feature_flag)));
        for (int i = 0; i < values.Length; i++) p[i] = ToNative(values[i]);
        return p;
    }
    private readonly List<(Memory<FeatureFlag> Memory, nuint Pointer)> journalFeatureFlag = new();
    private void JournalFeatureFlag(Memory<FeatureFlag> memory, cui_feature_flag* pointer) => journalFeatureFlag.Add((memory, (nuint)pointer));
    private void SyncFeatureFlag()
    {
        foreach (var entry in journalFeatureFlag)
        {
            var span = entry.Memory.Span;
            var p = (cui_feature_flag*)entry.Pointer;
            for (int i = 0; i < span.Length; i++) span[i] = FromNative(p[i], span[i]);
        }
    }
    private cui_filter_field* CopyFilterField(ReadOnlySpan<FilterField> values)
    {
        var p = (cui_filter_field*)arena.Allocate(checked(values.Length * sizeof(cui_filter_field)));
        for (int i = 0; i < values.Length; i++) p[i] = ToNative(values[i]);
        return p;
    }
    private readonly List<(Memory<FilterField> Memory, nuint Pointer)> journalFilterField = new();
    private void JournalFilterField(Memory<FilterField> memory, cui_filter_field* pointer) => journalFilterField.Add((memory, (nuint)pointer));
    private void SyncFilterField()
    {
        foreach (var entry in journalFilterField)
        {
            var span = entry.Memory.Span;
            var p = (cui_filter_field*)entry.Pointer;
            for (int i = 0; i < span.Length; i++) span[i] = FromNative(p[i], span[i]);
        }
    }
    private cui_gantt_task* CopyGanttTask(ReadOnlySpan<GanttTask> values)
    {
        var p = (cui_gantt_task*)arena.Allocate(checked(values.Length * sizeof(cui_gantt_task)));
        for (int i = 0; i < values.Length; i++) p[i] = ToNative(values[i]);
        return p;
    }
    private readonly List<(Memory<GanttTask> Memory, nuint Pointer)> journalGanttTask = new();
    private void JournalGanttTask(Memory<GanttTask> memory, cui_gantt_task* pointer) => journalGanttTask.Add((memory, (nuint)pointer));
    private void SyncGanttTask()
    {
        foreach (var entry in journalGanttTask)
        {
            var span = entry.Memory.Span;
            var p = (cui_gantt_task*)entry.Pointer;
            for (int i = 0; i < span.Length; i++) span[i] = FromNative(p[i], span[i]);
        }
    }
    private cui_gauge_band* CopyGaugeBand(ReadOnlySpan<GaugeBand> values)
    {
        var p = (cui_gauge_band*)arena.Allocate(checked(values.Length * sizeof(cui_gauge_band)));
        for (int i = 0; i < values.Length; i++) p[i] = ToNative(values[i]);
        return p;
    }
    private readonly List<(Memory<GaugeBand> Memory, nuint Pointer)> journalGaugeBand = new();
    private void JournalGaugeBand(Memory<GaugeBand> memory, cui_gauge_band* pointer) => journalGaugeBand.Add((memory, (nuint)pointer));
    private void SyncGaugeBand()
    {
        foreach (var entry in journalGaugeBand)
        {
            var span = entry.Memory.Span;
            var p = (cui_gauge_band*)entry.Pointer;
            for (int i = 0; i < span.Length; i++) span[i] = FromNative(p[i], span[i]);
        }
    }
    private cui_graph_edge* CopyGraphEdge(ReadOnlySpan<GraphEdge> values)
    {
        var p = (cui_graph_edge*)arena.Allocate(checked(values.Length * sizeof(cui_graph_edge)));
        for (int i = 0; i < values.Length; i++) p[i] = ToNative(values[i]);
        return p;
    }
    private readonly List<(Memory<GraphEdge> Memory, nuint Pointer)> journalGraphEdge = new();
    private void JournalGraphEdge(Memory<GraphEdge> memory, cui_graph_edge* pointer) => journalGraphEdge.Add((memory, (nuint)pointer));
    private void SyncGraphEdge()
    {
        foreach (var entry in journalGraphEdge)
        {
            var span = entry.Memory.Span;
            var p = (cui_graph_edge*)entry.Pointer;
            for (int i = 0; i < span.Length; i++) span[i] = FromNative(p[i], span[i]);
        }
    }
    private cui_graph_node* CopyGraphNode(ReadOnlySpan<GraphNode> values)
    {
        var p = (cui_graph_node*)arena.Allocate(checked(values.Length * sizeof(cui_graph_node)));
        for (int i = 0; i < values.Length; i++) p[i] = ToNative(values[i]);
        return p;
    }
    private readonly List<(Memory<GraphNode> Memory, nuint Pointer)> journalGraphNode = new();
    private void JournalGraphNode(Memory<GraphNode> memory, cui_graph_node* pointer) => journalGraphNode.Add((memory, (nuint)pointer));
    private void SyncGraphNode()
    {
        foreach (var entry in journalGraphNode)
        {
            var span = entry.Memory.Span;
            var p = (cui_graph_node*)entry.Pointer;
            for (int i = 0; i < span.Length; i++) span[i] = FromNative(p[i], span[i]);
        }
    }
    private cui_grid_column* CopyGridColumn(ReadOnlySpan<GridColumn> values)
    {
        var p = (cui_grid_column*)arena.Allocate(checked(values.Length * sizeof(cui_grid_column)));
        for (int i = 0; i < values.Length; i++) p[i] = ToNative(values[i]);
        return p;
    }
    private readonly List<(Memory<GridColumn> Memory, nuint Pointer)> journalGridColumn = new();
    private void JournalGridColumn(Memory<GridColumn> memory, cui_grid_column* pointer) => journalGridColumn.Add((memory, (nuint)pointer));
    private void SyncGridColumn()
    {
        foreach (var entry in journalGridColumn)
        {
            var span = entry.Memory.Span;
            var p = (cui_grid_column*)entry.Pointer;
            for (int i = 0; i < span.Length; i++) span[i] = FromNative(p[i], span[i]);
        }
    }
    private cui_histogram_series* CopyHistogramSeries(ReadOnlySpan<HistogramSeries> values)
    {
        var p = (cui_histogram_series*)arena.Allocate(checked(values.Length * sizeof(cui_histogram_series)));
        for (int i = 0; i < values.Length; i++) p[i] = ToNative(values[i]);
        return p;
    }
    private readonly List<(Memory<HistogramSeries> Memory, nuint Pointer)> journalHistogramSeries = new();
    private void JournalHistogramSeries(Memory<HistogramSeries> memory, cui_histogram_series* pointer) => journalHistogramSeries.Add((memory, (nuint)pointer));
    private void SyncHistogramSeries()
    {
        foreach (var entry in journalHistogramSeries)
        {
            var span = entry.Memory.Span;
            var p = (cui_histogram_series*)entry.Pointer;
            for (int i = 0; i < span.Length; i++) span[i] = FromNative(p[i], span[i]);
        }
    }
    private cui_keymap_binding* CopyKeymapBinding(ReadOnlySpan<KeymapBinding> values)
    {
        var p = (cui_keymap_binding*)arena.Allocate(checked(values.Length * sizeof(cui_keymap_binding)));
        for (int i = 0; i < values.Length; i++) p[i] = ToNative(values[i]);
        return p;
    }
    private readonly List<(Memory<KeymapBinding> Memory, nuint Pointer)> journalKeymapBinding = new();
    private void JournalKeymapBinding(Memory<KeymapBinding> memory, cui_keymap_binding* pointer) => journalKeymapBinding.Add((memory, (nuint)pointer));
    private void SyncKeymapBinding()
    {
        foreach (var entry in journalKeymapBinding)
        {
            var span = entry.Memory.Span;
            var p = (cui_keymap_binding*)entry.Pointer;
            for (int i = 0; i < span.Length; i++) span[i] = FromNative(p[i], span[i]);
        }
    }
    private cui_kv_pair* CopyKvPair(ReadOnlySpan<KvPair> values)
    {
        var p = (cui_kv_pair*)arena.Allocate(checked(values.Length * sizeof(cui_kv_pair)));
        for (int i = 0; i < values.Length; i++) p[i] = ToNative(values[i]);
        return p;
    }
    private readonly List<(Memory<KvPair> Memory, nuint Pointer)> journalKvPair = new();
    private void JournalKvPair(Memory<KvPair> memory, cui_kv_pair* pointer) => journalKvPair.Add((memory, (nuint)pointer));
    private void SyncKvPair()
    {
        foreach (var entry in journalKvPair)
        {
            var span = entry.Memory.Span;
            var p = (cui_kv_pair*)entry.Pointer;
            for (int i = 0; i < span.Length; i++) span[i] = FromNative(p[i], span[i]);
        }
    }
    private cui_model* CopyModel(ReadOnlySpan<Model> values)
    {
        var p = (cui_model*)arena.Allocate(checked(values.Length * sizeof(cui_model)));
        for (int i = 0; i < values.Length; i++) p[i] = ToNative(values[i]);
        return p;
    }
    private readonly List<(Memory<Model> Memory, nuint Pointer)> journalModel = new();
    private void JournalModel(Memory<Model> memory, cui_model* pointer) => journalModel.Add((memory, (nuint)pointer));
    private void SyncModel()
    {
        foreach (var entry in journalModel)
        {
            var span = entry.Memory.Span;
            var p = (cui_model*)entry.Pointer;
            for (int i = 0; i < span.Length; i++) span[i] = FromNative(p[i], span[i]);
        }
    }
    private cui_month_event* CopyMonthEvent(ReadOnlySpan<MonthEvent> values)
    {
        var p = (cui_month_event*)arena.Allocate(checked(values.Length * sizeof(cui_month_event)));
        for (int i = 0; i < values.Length; i++) p[i] = ToNative(values[i]);
        return p;
    }
    private readonly List<(Memory<MonthEvent> Memory, nuint Pointer)> journalMonthEvent = new();
    private void JournalMonthEvent(Memory<MonthEvent> memory, cui_month_event* pointer) => journalMonthEvent.Add((memory, (nuint)pointer));
    private void SyncMonthEvent()
    {
        foreach (var entry in journalMonthEvent)
        {
            var span = entry.Memory.Span;
            var p = (cui_month_event*)entry.Pointer;
            for (int i = 0; i < span.Length; i++) span[i] = FromNative(p[i], span[i]);
        }
    }
    private cui_net_request* CopyNetRequest(ReadOnlySpan<NetRequest> values)
    {
        var p = (cui_net_request*)arena.Allocate(checked(values.Length * sizeof(cui_net_request)));
        for (int i = 0; i < values.Length; i++) p[i] = ToNative(values[i]);
        return p;
    }
    private readonly List<(Memory<NetRequest> Memory, nuint Pointer)> journalNetRequest = new();
    private void JournalNetRequest(Memory<NetRequest> memory, cui_net_request* pointer) => journalNetRequest.Add((memory, (nuint)pointer));
    private void SyncNetRequest()
    {
        foreach (var entry in journalNetRequest)
        {
            var span = entry.Memory.Span;
            var p = (cui_net_request*)entry.Pointer;
            for (int i = 0; i < span.Length; i++) span[i] = FromNative(p[i], span[i]);
        }
    }
    private cui_pipeline_job* CopyPipelineJob(ReadOnlySpan<PipelineJob> values)
    {
        var p = (cui_pipeline_job*)arena.Allocate(checked(values.Length * sizeof(cui_pipeline_job)));
        for (int i = 0; i < values.Length; i++) p[i] = ToNative(values[i]);
        return p;
    }
    private readonly List<(Memory<PipelineJob> Memory, nuint Pointer)> journalPipelineJob = new();
    private void JournalPipelineJob(Memory<PipelineJob> memory, cui_pipeline_job* pointer) => journalPipelineJob.Add((memory, (nuint)pointer));
    private void SyncPipelineJob()
    {
        foreach (var entry in journalPipelineJob)
        {
            var span = entry.Memory.Span;
            var p = (cui_pipeline_job*)entry.Pointer;
            for (int i = 0; i < span.Length; i++) span[i] = FromNative(p[i], span[i]);
        }
    }
    private cui_plan_step* CopyPlanStep(ReadOnlySpan<PlanStep> values)
    {
        var p = (cui_plan_step*)arena.Allocate(checked(values.Length * sizeof(cui_plan_step)));
        for (int i = 0; i < values.Length; i++) p[i] = ToNative(values[i]);
        return p;
    }
    private readonly List<(Memory<PlanStep> Memory, nuint Pointer)> journalPlanStep = new();
    private void JournalPlanStep(Memory<PlanStep> memory, cui_plan_step* pointer) => journalPlanStep.Add((memory, (nuint)pointer));
    private void SyncPlanStep()
    {
        foreach (var entry in journalPlanStep)
        {
            var span = entry.Memory.Span;
            var p = (cui_plan_step*)entry.Pointer;
            for (int i = 0; i < span.Length; i++) span[i] = FromNative(p[i], span[i]);
        }
    }
    private cui_presence_user* CopyPresenceUser(ReadOnlySpan<PresenceUser> values)
    {
        var p = (cui_presence_user*)arena.Allocate(checked(values.Length * sizeof(cui_presence_user)));
        for (int i = 0; i < values.Length; i++) p[i] = ToNative(values[i]);
        return p;
    }
    private readonly List<(Memory<PresenceUser> Memory, nuint Pointer)> journalPresenceUser = new();
    private void JournalPresenceUser(Memory<PresenceUser> memory, cui_presence_user* pointer) => journalPresenceUser.Add((memory, (nuint)pointer));
    private void SyncPresenceUser()
    {
        foreach (var entry in journalPresenceUser)
        {
            var span = entry.Memory.Span;
            var p = (cui_presence_user*)entry.Pointer;
            for (int i = 0; i < span.Length; i++) span[i] = FromNative(p[i], span[i]);
        }
    }
    private cui_prompt_suggestion* CopyPromptSuggestion(ReadOnlySpan<PromptSuggestion> values)
    {
        var p = (cui_prompt_suggestion*)arena.Allocate(checked(values.Length * sizeof(cui_prompt_suggestion)));
        for (int i = 0; i < values.Length; i++) p[i] = ToNative(values[i]);
        return p;
    }
    private readonly List<(Memory<PromptSuggestion> Memory, nuint Pointer)> journalPromptSuggestion = new();
    private void JournalPromptSuggestion(Memory<PromptSuggestion> memory, cui_prompt_suggestion* pointer) => journalPromptSuggestion.Add((memory, (nuint)pointer));
    private void SyncPromptSuggestion()
    {
        foreach (var entry in journalPromptSuggestion)
        {
            var span = entry.Memory.Span;
            var p = (cui_prompt_suggestion*)entry.Pointer;
            for (int i = 0; i < span.Length; i++) span[i] = FromNative(p[i], span[i]);
        }
    }
    private cui_query_field* CopyQueryField(ReadOnlySpan<QueryField> values)
    {
        var p = (cui_query_field*)arena.Allocate(checked(values.Length * sizeof(cui_query_field)));
        for (int i = 0; i < values.Length; i++) p[i] = ToNative(values[i]);
        return p;
    }
    private readonly List<(Memory<QueryField> Memory, nuint Pointer)> journalQueryField = new();
    private void JournalQueryField(Memory<QueryField> memory, cui_query_field* pointer) => journalQueryField.Add((memory, (nuint)pointer));
    private void SyncQueryField()
    {
        foreach (var entry in journalQueryField)
        {
            var span = entry.Memory.Span;
            var p = (cui_query_field*)entry.Pointer;
            for (int i = 0; i < span.Length; i++) span[i] = FromNative(p[i], span[i]);
        }
    }
    private cui_query_node* CopyQueryNode(ReadOnlySpan<QueryNode> values)
    {
        var p = (cui_query_node*)arena.Allocate(checked(values.Length * sizeof(cui_query_node)));
        for (int i = 0; i < values.Length; i++) p[i] = ToNative(values[i]);
        return p;
    }
    private readonly List<(Memory<QueryNode> Memory, nuint Pointer)> journalQueryNode = new();
    private void JournalQueryNode(Memory<QueryNode> memory, cui_query_node* pointer) => journalQueryNode.Add((memory, (nuint)pointer));
    private void SyncQueryNode()
    {
        foreach (var entry in journalQueryNode)
        {
            var span = entry.Memory.Span;
            var p = (cui_query_node*)entry.Pointer;
            for (int i = 0; i < span.Length; i++) span[i] = FromNative(p[i], span[i]);
        }
    }
    private cui_reaction* CopyReaction(ReadOnlySpan<Reaction> values)
    {
        var p = (cui_reaction*)arena.Allocate(checked(values.Length * sizeof(cui_reaction)));
        for (int i = 0; i < values.Length; i++) p[i] = ToNative(values[i]);
        return p;
    }
    private readonly List<(Memory<Reaction> Memory, nuint Pointer)> journalReaction = new();
    private void JournalReaction(Memory<Reaction> memory, cui_reaction* pointer) => journalReaction.Add((memory, (nuint)pointer));
    private void SyncReaction()
    {
        foreach (var entry in journalReaction)
        {
            var span = entry.Memory.Span;
            var p = (cui_reaction*)entry.Pointer;
            for (int i = 0; i < span.Length; i++) span[i] = FromNative(p[i], span[i]);
        }
    }
    private cui_sankey_link* CopySankeyLink(ReadOnlySpan<SankeyLink> values)
    {
        var p = (cui_sankey_link*)arena.Allocate(checked(values.Length * sizeof(cui_sankey_link)));
        for (int i = 0; i < values.Length; i++) p[i] = ToNative(values[i]);
        return p;
    }
    private readonly List<(Memory<SankeyLink> Memory, nuint Pointer)> journalSankeyLink = new();
    private void JournalSankeyLink(Memory<SankeyLink> memory, cui_sankey_link* pointer) => journalSankeyLink.Add((memory, (nuint)pointer));
    private void SyncSankeyLink()
    {
        foreach (var entry in journalSankeyLink)
        {
            var span = entry.Memory.Span;
            var p = (cui_sankey_link*)entry.Pointer;
            for (int i = 0; i < span.Length; i++) span[i] = FromNative(p[i], span[i]);
        }
    }
    private cui_scatter_series* CopyScatterSeries(ReadOnlySpan<ScatterSeries> values)
    {
        var p = (cui_scatter_series*)arena.Allocate(checked(values.Length * sizeof(cui_scatter_series)));
        for (int i = 0; i < values.Length; i++) p[i] = ToNative(values[i]);
        return p;
    }
    private readonly List<(Memory<ScatterSeries> Memory, nuint Pointer)> journalScatterSeries = new();
    private void JournalScatterSeries(Memory<ScatterSeries> memory, cui_scatter_series* pointer) => journalScatterSeries.Add((memory, (nuint)pointer));
    private void SyncScatterSeries()
    {
        foreach (var entry in journalScatterSeries)
        {
            var span = entry.Memory.Span;
            var p = (cui_scatter_series*)entry.Pointer;
            for (int i = 0; i < span.Length; i++) span[i] = FromNative(p[i], span[i]);
        }
    }
    private cui_series* CopySeries(ReadOnlySpan<Series> values)
    {
        var p = (cui_series*)arena.Allocate(checked(values.Length * sizeof(cui_series)));
        for (int i = 0; i < values.Length; i++) p[i] = ToNative(values[i]);
        return p;
    }
    private readonly List<(Memory<Series> Memory, nuint Pointer)> journalSeries = new();
    private void JournalSeries(Memory<Series> memory, cui_series* pointer) => journalSeries.Add((memory, (nuint)pointer));
    private void SyncSeries()
    {
        foreach (var entry in journalSeries)
        {
            var span = entry.Memory.Span;
            var p = (cui_series*)entry.Pointer;
            for (int i = 0; i < span.Length; i++) span[i] = FromNative(p[i], span[i]);
        }
    }
    private cui_source* CopySource(ReadOnlySpan<Source> values)
    {
        var p = (cui_source*)arena.Allocate(checked(values.Length * sizeof(cui_source)));
        for (int i = 0; i < values.Length; i++) p[i] = ToNative(values[i]);
        return p;
    }
    private readonly List<(Memory<Source> Memory, nuint Pointer)> journalSource = new();
    private void JournalSource(Memory<Source> memory, cui_source* pointer) => journalSource.Add((memory, (nuint)pointer));
    private void SyncSource()
    {
        foreach (var entry in journalSource)
        {
            var span = entry.Memory.Span;
            var p = (cui_source*)entry.Pointer;
            for (int i = 0; i < span.Length; i++) span[i] = FromNative(p[i], span[i]);
        }
    }
    private cui_speed_action* CopySpeedAction(ReadOnlySpan<SpeedAction> values)
    {
        var p = (cui_speed_action*)arena.Allocate(checked(values.Length * sizeof(cui_speed_action)));
        for (int i = 0; i < values.Length; i++) p[i] = ToNative(values[i]);
        return p;
    }
    private readonly List<(Memory<SpeedAction> Memory, nuint Pointer)> journalSpeedAction = new();
    private void JournalSpeedAction(Memory<SpeedAction> memory, cui_speed_action* pointer) => journalSpeedAction.Add((memory, (nuint)pointer));
    private void SyncSpeedAction()
    {
        foreach (var entry in journalSpeedAction)
        {
            var span = entry.Memory.Span;
            var p = (cui_speed_action*)entry.Pointer;
            for (int i = 0; i < span.Length; i++) span[i] = FromNative(p[i], span[i]);
        }
    }
    private cui_stack_frame* CopyStackFrame(ReadOnlySpan<StackFrame> values)
    {
        var p = (cui_stack_frame*)arena.Allocate(checked(values.Length * sizeof(cui_stack_frame)));
        for (int i = 0; i < values.Length; i++) p[i] = ToNative(values[i]);
        return p;
    }
    private readonly List<(Memory<StackFrame> Memory, nuint Pointer)> journalStackFrame = new();
    private void JournalStackFrame(Memory<StackFrame> memory, cui_stack_frame* pointer) => journalStackFrame.Add((memory, (nuint)pointer));
    private void SyncStackFrame()
    {
        foreach (var entry in journalStackFrame)
        {
            var span = entry.Memory.Span;
            var p = (cui_stack_frame*)entry.Pointer;
            for (int i = 0; i < span.Length; i++) span[i] = FromNative(p[i], span[i]);
        }
    }
    private cui_swipe_action* CopySwipeAction(ReadOnlySpan<SwipeAction> values)
    {
        var p = (cui_swipe_action*)arena.Allocate(checked(values.Length * sizeof(cui_swipe_action)));
        for (int i = 0; i < values.Length; i++) p[i] = ToNative(values[i]);
        return p;
    }
    private readonly List<(Memory<SwipeAction> Memory, nuint Pointer)> journalSwipeAction = new();
    private void JournalSwipeAction(Memory<SwipeAction> memory, cui_swipe_action* pointer) => journalSwipeAction.Add((memory, (nuint)pointer));
    private void SyncSwipeAction()
    {
        foreach (var entry in journalSwipeAction)
        {
            var span = entry.Memory.Span;
            var p = (cui_swipe_action*)entry.Pointer;
            for (int i = 0; i < span.Length; i++) span[i] = FromNative(p[i], span[i]);
        }
    }
    private cui_tab_bar_item* CopyTabBarItem(ReadOnlySpan<TabBarItem> values)
    {
        var p = (cui_tab_bar_item*)arena.Allocate(checked(values.Length * sizeof(cui_tab_bar_item)));
        for (int i = 0; i < values.Length; i++) p[i] = ToNative(values[i]);
        return p;
    }
    private readonly List<(Memory<TabBarItem> Memory, nuint Pointer)> journalTabBarItem = new();
    private void JournalTabBarItem(Memory<TabBarItem> memory, cui_tab_bar_item* pointer) => journalTabBarItem.Add((memory, (nuint)pointer));
    private void SyncTabBarItem()
    {
        foreach (var entry in journalTabBarItem)
        {
            var span = entry.Memory.Span;
            var p = (cui_tab_bar_item*)entry.Pointer;
            for (int i = 0; i < span.Length; i++) span[i] = FromNative(p[i], span[i]);
        }
    }
    private cui_table_column* CopyTableColumn(ReadOnlySpan<TableColumn> values)
    {
        var p = (cui_table_column*)arena.Allocate(checked(values.Length * sizeof(cui_table_column)));
        for (int i = 0; i < values.Length; i++) p[i] = ToNative(values[i]);
        return p;
    }
    private readonly List<(Memory<TableColumn> Memory, nuint Pointer)> journalTableColumn = new();
    private void JournalTableColumn(Memory<TableColumn> memory, cui_table_column* pointer) => journalTableColumn.Add((memory, (nuint)pointer));
    private void SyncTableColumn()
    {
        foreach (var entry in journalTableColumn)
        {
            var span = entry.Memory.Span;
            var p = (cui_table_column*)entry.Pointer;
            for (int i = 0; i < span.Length; i++) span[i] = FromNative(p[i], span[i]);
        }
    }
    private cui_test_item* CopyTestItem(ReadOnlySpan<TestItem> values)
    {
        var p = (cui_test_item*)arena.Allocate(checked(values.Length * sizeof(cui_test_item)));
        for (int i = 0; i < values.Length; i++) p[i] = ToNative(values[i]);
        return p;
    }
    private readonly List<(Memory<TestItem> Memory, nuint Pointer)> journalTestItem = new();
    private void JournalTestItem(Memory<TestItem> memory, cui_test_item* pointer) => journalTestItem.Add((memory, (nuint)pointer));
    private void SyncTestItem()
    {
        foreach (var entry in journalTestItem)
        {
            var span = entry.Memory.Span;
            var p = (cui_test_item*)entry.Pointer;
            for (int i = 0; i < span.Length; i++) span[i] = FromNative(p[i], span[i]);
        }
    }
    private cui_ticker_item* CopyTickerItem(ReadOnlySpan<TickerItem> values)
    {
        var p = (cui_ticker_item*)arena.Allocate(checked(values.Length * sizeof(cui_ticker_item)));
        for (int i = 0; i < values.Length; i++) p[i] = ToNative(values[i]);
        return p;
    }
    private readonly List<(Memory<TickerItem> Memory, nuint Pointer)> journalTickerItem = new();
    private void JournalTickerItem(Memory<TickerItem> memory, cui_ticker_item* pointer) => journalTickerItem.Add((memory, (nuint)pointer));
    private void SyncTickerItem()
    {
        foreach (var entry in journalTickerItem)
        {
            var span = entry.Memory.Span;
            var p = (cui_ticker_item*)entry.Pointer;
            for (int i = 0; i < span.Length; i++) span[i] = FromNative(p[i], span[i]);
        }
    }
    private cui_toc_item* CopyTocItem(ReadOnlySpan<TocItem> values)
    {
        var p = (cui_toc_item*)arena.Allocate(checked(values.Length * sizeof(cui_toc_item)));
        for (int i = 0; i < values.Length; i++) p[i] = ToNative(values[i]);
        return p;
    }
    private readonly List<(Memory<TocItem> Memory, nuint Pointer)> journalTocItem = new();
    private void JournalTocItem(Memory<TocItem> memory, cui_toc_item* pointer) => journalTocItem.Add((memory, (nuint)pointer));
    private void SyncTocItem()
    {
        foreach (var entry in journalTocItem)
        {
            var span = entry.Memory.Span;
            var p = (cui_toc_item*)entry.Pointer;
            for (int i = 0; i < span.Length; i++) span[i] = FromNative(p[i], span[i]);
        }
    }
    private cui_tour_step* CopyTourStep(ReadOnlySpan<TourStep> values)
    {
        var p = (cui_tour_step*)arena.Allocate(checked(values.Length * sizeof(cui_tour_step)));
        for (int i = 0; i < values.Length; i++) p[i] = ToNative(values[i]);
        return p;
    }
    private readonly List<(Memory<TourStep> Memory, nuint Pointer)> journalTourStep = new();
    private void JournalTourStep(Memory<TourStep> memory, cui_tour_step* pointer) => journalTourStep.Add((memory, (nuint)pointer));
    private void SyncTourStep()
    {
        foreach (var entry in journalTourStep)
        {
            var span = entry.Memory.Span;
            var p = (cui_tour_step*)entry.Pointer;
            for (int i = 0; i < span.Length; i++) span[i] = FromNative(p[i], span[i]);
        }
    }
    private cui_trace_marker* CopyTraceMarker(ReadOnlySpan<TraceMarker> values)
    {
        var p = (cui_trace_marker*)arena.Allocate(checked(values.Length * sizeof(cui_trace_marker)));
        for (int i = 0; i < values.Length; i++) p[i] = ToNative(values[i]);
        return p;
    }
    private readonly List<(Memory<TraceMarker> Memory, nuint Pointer)> journalTraceMarker = new();
    private void JournalTraceMarker(Memory<TraceMarker> memory, cui_trace_marker* pointer) => journalTraceMarker.Add((memory, (nuint)pointer));
    private void SyncTraceMarker()
    {
        foreach (var entry in journalTraceMarker)
        {
            var span = entry.Memory.Span;
            var p = (cui_trace_marker*)entry.Pointer;
            for (int i = 0; i < span.Length; i++) span[i] = FromNative(p[i], span[i]);
        }
    }
    private cui_trace_span* CopyTraceSpan(ReadOnlySpan<TraceSpan> values)
    {
        var p = (cui_trace_span*)arena.Allocate(checked(values.Length * sizeof(cui_trace_span)));
        for (int i = 0; i < values.Length; i++) p[i] = ToNative(values[i]);
        return p;
    }
    private readonly List<(Memory<TraceSpan> Memory, nuint Pointer)> journalTraceSpan = new();
    private void JournalTraceSpan(Memory<TraceSpan> memory, cui_trace_span* pointer) => journalTraceSpan.Add((memory, (nuint)pointer));
    private void SyncTraceSpan()
    {
        foreach (var entry in journalTraceSpan)
        {
            var span = entry.Memory.Span;
            var p = (cui_trace_span*)entry.Pointer;
            for (int i = 0; i < span.Length; i++) span[i] = FromNative(p[i], span[i]);
        }
    }
    private cui_trace_track* CopyTraceTrack(ReadOnlySpan<TraceTrack> values)
    {
        var p = (cui_trace_track*)arena.Allocate(checked(values.Length * sizeof(cui_trace_track)));
        for (int i = 0; i < values.Length; i++) p[i] = ToNative(values[i]);
        return p;
    }
    private readonly List<(Memory<TraceTrack> Memory, nuint Pointer)> journalTraceTrack = new();
    private void JournalTraceTrack(Memory<TraceTrack> memory, cui_trace_track* pointer) => journalTraceTrack.Add((memory, (nuint)pointer));
    private void SyncTraceTrack()
    {
        foreach (var entry in journalTraceTrack)
        {
            var span = entry.Memory.Span;
            var p = (cui_trace_track*)entry.Pointer;
            for (int i = 0; i < span.Length; i++) span[i] = FromNative(p[i], span[i]);
        }
    }
    private cui_uptime_service* CopyUptimeService(ReadOnlySpan<UptimeService> values)
    {
        var p = (cui_uptime_service*)arena.Allocate(checked(values.Length * sizeof(cui_uptime_service)));
        for (int i = 0; i < values.Length; i++) p[i] = ToNative(values[i]);
        return p;
    }
    private readonly List<(Memory<UptimeService> Memory, nuint Pointer)> journalUptimeService = new();
    private void JournalUptimeService(Memory<UptimeService> memory, cui_uptime_service* pointer) => journalUptimeService.Add((memory, (nuint)pointer));
    private void SyncUptimeService()
    {
        foreach (var entry in journalUptimeService)
        {
            var span = entry.Memory.Span;
            var p = (cui_uptime_service*)entry.Pointer;
            for (int i = 0; i < span.Length; i++) span[i] = FromNative(p[i], span[i]);
        }
    }
    private cui_vec2* CopyVec2(ReadOnlySpan<Vec2> values)
    {
        var p = (cui_vec2*)arena.Allocate(checked(values.Length * sizeof(cui_vec2)));
        for (int i = 0; i < values.Length; i++) p[i] = ToNative(values[i]);
        return p;
    }
    private readonly List<(Memory<Vec2> Memory, nuint Pointer)> journalVec2 = new();
    private void JournalVec2(Memory<Vec2> memory, cui_vec2* pointer) => journalVec2.Add((memory, (nuint)pointer));
    private void SyncVec2()
    {
        foreach (var entry in journalVec2)
        {
            var span = entry.Memory.Span;
            var p = (cui_vec2*)entry.Pointer;
            for (int i = 0; i < span.Length; i++) span[i] = FromNative(p[i], span[i]);
        }
    }
    private cui_viz_item* CopyVizItem(ReadOnlySpan<VizItem> values)
    {
        var p = (cui_viz_item*)arena.Allocate(checked(values.Length * sizeof(cui_viz_item)));
        for (int i = 0; i < values.Length; i++) p[i] = ToNative(values[i]);
        return p;
    }
    private readonly List<(Memory<VizItem> Memory, nuint Pointer)> journalVizItem = new();
    private void JournalVizItem(Memory<VizItem> memory, cui_viz_item* pointer) => journalVizItem.Add((memory, (nuint)pointer));
    private void SyncVizItem()
    {
        foreach (var entry in journalVizItem)
        {
            var span = entry.Memory.Span;
            var p = (cui_viz_item*)entry.Pointer;
            for (int i = 0; i < span.Length; i++) span[i] = FromNative(p[i], span[i]);
        }
    }
    private double* CopyDouble(ReadOnlySpan<double> values)
    {
        var p = (double*)arena.Allocate(checked(values.Length * sizeof(double)));
        for (int i = 0; i < values.Length; i++) p[i] = values[i];
        return p;
    }
    private readonly List<(Memory<double> Memory, nuint Pointer)> journalDouble = new();
    private void JournalDouble(Memory<double> memory, double* pointer) => journalDouble.Add((memory, (nuint)pointer));
    private void SyncDouble()
    {
        foreach (var entry in journalDouble)
        {
            var span = entry.Memory.Span;
            var p = (double*)entry.Pointer;
            for (int i = 0; i < span.Length; i++) span[i] = p[i];
        }
    }
    private float* CopyFloat(ReadOnlySpan<float> values)
    {
        var p = (float*)arena.Allocate(checked(values.Length * sizeof(float)));
        for (int i = 0; i < values.Length; i++) p[i] = values[i];
        return p;
    }
    private readonly List<(Memory<float> Memory, nuint Pointer)> journalFloat = new();
    private void JournalFloat(Memory<float> memory, float* pointer) => journalFloat.Add((memory, (nuint)pointer));
    private void SyncFloat()
    {
        foreach (var entry in journalFloat)
        {
            var span = entry.Memory.Span;
            var p = (float*)entry.Pointer;
            for (int i = 0; i < span.Length; i++) span[i] = p[i];
        }
    }
    private int* CopyInt(ReadOnlySpan<int> values)
    {
        var p = (int*)arena.Allocate(checked(values.Length * sizeof(int)));
        for (int i = 0; i < values.Length; i++) p[i] = values[i];
        return p;
    }
    private readonly List<(Memory<int> Memory, nuint Pointer)> journalInt = new();
    private void JournalInt(Memory<int> memory, int* pointer) { journalInt.Add((memory, (nuint)pointer)); arraysDirty = true; }
    private void SyncInt()
    {
        foreach (var entry in journalInt)
        {
            var span = entry.Memory.Span;
            var p = (int*)entry.Pointer;
            for (int i = 0; i < span.Length; i++) span[i] = p[i];
        }
    }
    private uint* CopyUint32T(ReadOnlySpan<uint> values)
    {
        var p = (uint*)arena.Allocate(checked(values.Length * sizeof(uint)));
        for (int i = 0; i < values.Length; i++) p[i] = values[i];
        return p;
    }
    private readonly List<(Memory<uint> Memory, nuint Pointer)> journalUint32T = new();
    private void JournalUint32T(Memory<uint> memory, uint* pointer) => journalUint32T.Add((memory, (nuint)pointer));
    private void SyncUint32T()
    {
        foreach (var entry in journalUint32T)
        {
            var span = entry.Memory.Span;
            var p = (uint*)entry.Pointer;
            for (int i = 0; i < span.Length; i++) span[i] = p[i];
        }
    }
    private CBool* BindBool(State<bool>? value) => Bind<bool, CBool>(value, &EncodeBool, &DecodeBool);
    private static CBool EncodeBool(Ui ui, bool value) => value;
    private static bool DecodeBool(Ui ui, CBool value, bool old) => (value != 0);
    private cui_date* BindDate(State<Date>? value) => Bind<Date, cui_date>(value, &EncodeDate, &DecodeDate);
    private static cui_date EncodeDate(Ui ui, Date value) => ui.ToNative(value);
    private static Date DecodeDate(Ui ui, cui_date value, Date old) => ui.FromNative(value, old);
    private cui_markdown_result* BindMarkdownResult(State<MarkdownResult>? value) => Bind<MarkdownResult, cui_markdown_result>(value, &EncodeMarkdownResult, &DecodeMarkdownResult);
    private static cui_markdown_result EncodeMarkdownResult(Ui ui, MarkdownResult value) => ui.ToNative(value);
    private static MarkdownResult DecodeMarkdownResult(Ui ui, cui_markdown_result value, MarkdownResult old) => ui.FromNative(value, old);
    private cui_node_view* BindNodeView(State<NodeView>? value) => Bind<NodeView, cui_node_view>(value, &EncodeNodeView, &DecodeNodeView);
    private static cui_node_view EncodeNodeView(Ui ui, NodeView value) => ui.ToNative(value);
    private static NodeView DecodeNodeView(Ui ui, cui_node_view value, NodeView old) => ui.FromNative(value, old);
    private cui_table_sort* BindTableSort(State<TableSort>? value) => Bind<TableSort, cui_table_sort>(value, &EncodeTableSort, &DecodeTableSort);
    private static cui_table_sort EncodeTableSort(Ui ui, TableSort value) => ui.ToNative(value);
    private static TableSort DecodeTableSort(Ui ui, cui_table_sort value, TableSort old) => ui.FromNative(value, old);
    private int* BindInt(State<int>? value) => Bind<int, int>(value, &EncodeInt, &DecodeInt);
    private static int EncodeInt(Ui ui, int value) => value;
    private static int DecodeInt(Ui ui, int value, int old) => value;
    private void SyncArrays() { SyncBool(); SyncApiEndpoint(); SyncApiParam(); SyncBlameCommit(); SyncCalEvent(); SyncCandle(); SyncChartAnnotation(); SyncCheckStep(); SyncChoice(); SyncCommand(); SyncCommit(); SyncCoverageFile(); SyncDiagnostic(); SyncDotSeries(); SyncFeatureFlag(); SyncFilterField(); SyncGanttTask(); SyncGaugeBand(); SyncGraphEdge(); SyncGraphNode(); SyncGridColumn(); SyncHistogramSeries(); SyncKeymapBinding(); SyncKvPair(); SyncModel(); SyncMonthEvent(); SyncNetRequest(); SyncPipelineJob(); SyncPlanStep(); SyncPresenceUser(); SyncPromptSuggestion(); SyncQueryField(); SyncQueryNode(); SyncReaction(); SyncSankeyLink(); SyncScatterSeries(); SyncSeries(); SyncSource(); SyncSpeedAction(); SyncStackFrame(); SyncSwipeAction(); SyncTabBarItem(); SyncTableColumn(); SyncTestItem(); SyncTickerItem(); SyncTocItem(); SyncTourStep(); SyncTraceMarker(); SyncTraceSpan(); SyncTraceTrack(); SyncUptimeService(); SyncVec2(); SyncVizItem(); SyncDouble(); SyncFloat(); SyncInt(); SyncUint32T(); }
    private void ClearArrays() { journalBool.Clear(); journalApiEndpoint.Clear(); journalApiParam.Clear(); journalBlameCommit.Clear(); journalCalEvent.Clear(); journalCandle.Clear(); journalChartAnnotation.Clear(); journalCheckStep.Clear(); journalChoice.Clear(); journalCommand.Clear(); journalCommit.Clear(); journalCoverageFile.Clear(); journalDiagnostic.Clear(); journalDotSeries.Clear(); journalFeatureFlag.Clear(); journalFilterField.Clear(); journalGanttTask.Clear(); journalGaugeBand.Clear(); journalGraphEdge.Clear(); journalGraphNode.Clear(); journalGridColumn.Clear(); journalHistogramSeries.Clear(); journalKeymapBinding.Clear(); journalKvPair.Clear(); journalModel.Clear(); journalMonthEvent.Clear(); journalNetRequest.Clear(); journalPipelineJob.Clear(); journalPlanStep.Clear(); journalPresenceUser.Clear(); journalPromptSuggestion.Clear(); journalQueryField.Clear(); journalQueryNode.Clear(); journalReaction.Clear(); journalSankeyLink.Clear(); journalScatterSeries.Clear(); journalSeries.Clear(); journalSource.Clear(); journalSpeedAction.Clear(); journalStackFrame.Clear(); journalSwipeAction.Clear(); journalTabBarItem.Clear(); journalTableColumn.Clear(); journalTestItem.Clear(); journalTickerItem.Clear(); journalTocItem.Clear(); journalTourStep.Clear(); journalTraceMarker.Clear(); journalTraceSpan.Clear(); journalTraceTrack.Clear(); journalUptimeService.Clear(); journalVec2.Clear(); journalVizItem.Clear(); journalDouble.Clear(); journalFloat.Clear(); journalInt.Clear(); journalUint32T.Clear(); }
}
