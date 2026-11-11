# Managed CUI storage audit

The later [scoped UTF-8 audit](UTF8-STORAGE-AUDIT.md) replaces the conservative
stable-string policy below for 206 audited scalar-input entrypoints. Retained
arrays, option pointees, node titles, dock icons and Markdown chat text keep
their existing storage.

The wrapper now passes synchronous singleton native arguments as unmanaged local
values: `var native_opts = ToNative(opts); Raw.cui_...(ctx, &native_opts);`.
There is no `fixed` requirement for an unmanaged local. This changes storage,
not the public API or the order of checks, marshalling, calls, writeback and
synchronization.

225 arena-singleton allocation sites were removed from 172 methods: 221 in
`Ui.Api.cs`, two in `Ui.Overloads.cs`, and two in `Utf8.cs`. These comprise 157
options/box records and 68 other scalar/struct values, including const inputs,
writable values, selections, counts, range outputs and node positions. **Zero
frame-arena single-struct/scalar allocations remain.** Dynamic strings/arrays
(including an array whose runtime length happens to be one) remain separate
allocations. `Ui.cs`'s span-text overload and `Resources.cs`'s terminal options,
image-load dimensions, and synchronous helper outputs already used locals.
Scope dispatch's discarded Kanban move output already used a local as well.

## Evidence and limits

The authoritative upstream inputs are `include/cui/cui.h` and the corresponding
`src/*.c` definitions. Filenames below refer to that native source tree, not
translated C#. Declarations alone do not establish pointer lifetimes. The review
followed struct copies, paint data, builder state and forwarded helper calls.
The complete converted-site inventory is below; `reviewed.json` additionally
records hashes of 216 definitions and the public header, so a different native
revision cannot pass the optional native-evidence check silently.

### Upstream refresh: CUI 1.4.0 (2026-11-11)

Reviewed the source diff from CUI **1.3.1**, revision
`629b6fcb991d71527f2d67b25c51429c3dcdb498`, to CUI **1.4.0**, revision
`5a9c12b433d4501bc2f5632ead3a1cc05e5b1ee5`. Before refreshing evidence, all
214 prior definition hashes and the header hash matched both the old pristine
source and its adapted native/translation copies. Thus the prior evidence did
not require adapted-only hashes. The new manifest binds **pristine upstream**;
a private copy produced by the repository's guarded `stage.py` (21 adaptations)
has the same header and all 216 recorded definitions. No acquired or
coordinator-staged source was modified for this review.

Of the prior 214 definitions, **209 are byte-identical**. The five changed
entries are `cui__button_impl`, `cui_button_ex`, `cui_icon_button_ex`,
`cui_chat_message_ex`, and `cui_split_button_ex`. Their hashes were refreshed
only after the review below. Two additional entries cover the new forwarding
helper `cui__icon_button_id` and the changed managed `CopyButton` entrypoint
`cui_copy_button`. The manifest now checks **216 definitions**. Its 225 stack
sites, storage policy and four `reviewed_outputs` hashes are unchanged.

The complete changed/added native definition set is below, including definitions
outside the original manifest. Paths are relative to upstream `src/`.

| Source | Changed or added definitions | Retention and API findings |
| --- | --- | --- |
| `cui_widgets.c` | Changed `cui__button_impl`, `cui_button_ex`, `cui_icon_button_ex`; added `cui__icon_button_id` | ID creation moves to callers; the new helper immediately forwards a by-value ID and options. Icon-only glyphs use `cui__icon_node` directly; ordinary option icons still use `cui__icon_swap`. Both reach `cui__text_node`, which duplicates glyph text into native frame storage. Nodes/paint records receive numeric values or native allocations; the outer options address is never saved. Tooltip text is copied into native state by `cui_tooltip`. Keep stack options and existing stable UTF-8 pointees. |
| `cui_extras.c` | Changed `cui_copy_button` | A constant per-scope button ID preserves keyboard focus while the icon/tooltip changes. Local `bo` is consumed synchronously through the new helper; persistent copy state contains only a timestamp. Clipboard text is passed synchronously to the platform callback; the managed GLFW bridge decodes it to an owned string. No new callback root or retained caller address. |
| `cui_actions.c` | Changed `cui_split_button_ex` | Enter/Space on the focused chevron leaves an already-open menu available for selection. Options and local child options are read during this call; divider paint data copies its color. Popup state retains IDs/flags, not caller options. Stack options and existing item/icon arrays remain valid. |
| `cui_social.c` | Changed `cui_chat_message_ex` | Only local radii names change to avoid the Windows SDK `small` macro. Radii are copied to the bubble node. Markdown-result writeback remains synchronous, and nested author/time/status/text/State storage keeps its prior policy. |
| `cui_treetable.c` | Changed `cui_path_bar_ex` | The overflow pill gains the same keyboard guard as split menus. No pointer or signature change: persistent editing state owns its buffer/IDs, submitted text is frame-copied, and options are consumed in the call. Placeholder/icon strings still require stable pointees. This API has no safe wrapper entrypoint in the audited baseline. |
| `cui_tools.c` | Changed `cui_log_appendv` | The 512-byte local `small` becomes `line`. Short formatted text is copied by `cui_log_append_text` into the persistent ring; long text is formatted directly into that ring. No local buffer or `va_list` address is retained. Log-buffer APIs have no safe wrapper resource in the audited baseline. |
| `platform/cui_app.c` | Changed `cui_app_create` | The descriptor is copied by value into `app->desc`, and an empty `app_id` is normalized to null. Its nonempty pointer is shallow-copied, unlike the title, which is copied to `app->title`. All `app_id` dereferences in this revision occur during synchronous platform initialization. The outer descriptor can remain local; its string pointee must survive all initialization calls. |
| `platform/cui_app_glfw.c` | Changed `cui__plat_init` | Reads `app_id` to set X11 class/instance and, when supported, Wayland ID string hints before window creation. The managed bridge must decode/copy hint text to an owned string, never retain a temporary pinned UTF-8 address. No later CUI read of the descriptor's `app_id` exists in this revision. |
| `platform/cui_app_x11.c` | Changed `cui__plat_init` | Supplies `app_id` as both class-hint strings, falling back to title/`CUI`. Class hints are submitted during initialization and the temporary hint struct is freed there. This direct native backend is outside the translated product profile. |
| `platform/cui_app_sdl2.c` | Changed `cui__plat_init`; added `cui__sdl_env_set` | Sets copied SDL hints/environment values for initialization, preserves user-provided environment variables and removes only variables set here after `SDL_Init`, including failure. No caller pointer is saved by the new helper. SDL2 is outside the translated product profile. |

`include/cui/cui.h` changes only the version macros and Copy-button focus
comment: after removing comments and the three changed version macros, its
tokens are identical. No core public signatures or option layouts changed.
The refreshed header SHA-256 is
`f6534034afde0b30dd7f0a885a5cedc4ae361ef087a8fa7a716425122f83c379`.
`src/cui_internal.h` adds only the `cui__icon_button_id` prototype.

Separately, `include/cui/cui_app.h` appends `const char *app_id` to
`cui_app_desc` (48 to 56 bytes on the selected 64-bit ABI). Its SHA-256 is
`04b576a8e5db33976f4929a4d83c4759505eb9f340b4ce5ce2620a957ac1cd67`.
Regeneration and every descriptor producer must use that new layout together.
The optional generator evidence check covers `cui.h` and the manifest's function
entries; it does **not** check `cui_app.h`, platform definitions or the internal
header. Those changes were manually reviewed here, not silently added to the
checker's guarantees.

Reported for coordinator integration before any implementation change:
`DesktopApplication` and `ApplicationOptions` initially lacked an application-ID
option/pass-through, and the new native GLFW string-hint call needed an explicit
host override/UTF-8-copy bridge. A creation-scoped pin is sufficient for the
reviewed GLFW path if that bridge copies the string; retaining owned UTF-8 until
app destruction is also conservative. The shallow descriptor pointer alone
does not prove a deferred read. Re-audit this decision if later native code
starts reading `app_id` after initialization. The pre-existing PathBar and
log-buffer API omissions were reported separately; they do not invalidate the
existing widget surface. No new widget resource owner or storage substitution
is required. **All 225 stack-local sites and the retained carousel/node-view
State leases remain justified.**

Refresh verification passed (exit 0) with both pristine and privately staged
new-source roots:

```sh
TMPDIR="$SCRATCH/tmp" python3 -B cui/scripts/managed-api/generate.py --check --native-root "$CUI_SOURCE"
```

Each reported `Native evidence: 216 definitions and public header match` and
`Round-trip check: 4 reviewed API files match; no singleton arena allocations`.
The check without `--native-root` and `git diff --check` for these two owned
files also passed. A structural manifest comparison confirmed the unchanged
225-site inventory, policy, schema and all four output/template hashes.

This refresh performs source/hash checks only. It does not rerun or renew the
historical build, runtime, allocation, performance or gallery qualifications
below. The coordinator owns translation, host integration and those gates.

### Synchronous records and scalars

| Native call family | Source and retention evidence | Storage decision |
| --- | --- | --- |
| Text, button, checkbox/radio/switch | `cui_widgets.c`: `cui_text_ex`, `cui__button_impl`; `cui_controls.c`: `cui__toggle_row`, `cui_checkbox_ex`, `cui_radio_ex`, `cui_switch_ex`. Options are read while constructing nodes; mutable values are read/written before returning. | Stack options and scalar slots, including UTF-8 overloads. Strings remain separately stable. |
| Input, search, textarea, number | `cui_textedit.c`: `cui_input_ex`/`cui_textarea_ex` forward to `cui__text_field`; `cui_search_ex` copies options into its own local. `cui__text_field` copies text into native frame paint data but retains `o->placeholder`. `cui_number_ex` writes its value synchronously. | Stack top-level options/value; keep placeholder UTF-8 and owned editable storage. |
| Charts and visualization | `cui_charts.c`: `cui__xy_chart` copies `*o` into paint data and separately copies series/annotations/format; `cui_viz.c`: `cui_scatter_chart_ex`, `cui_radar_chart_ex`, `cui_funnel_ex`, `cui_split_bar_ex`, `cui_activity_rings_ex`, `cui_heatmap_ex`, `cui_calendar_heatmap_ex` copy `*o` to native frame paint records. Other visualization calls read individual option fields. | Stack outer records; keep nested arrays, labels and formats in stable frame storage, even where native code makes another copy. |
| Scalars and ref structs in controls/pickers | `cui_controls.c`: `cui__segments`, `cui_slider_ex`, `cui_range_slider`, `cui_rating`, `cui_pagination`; `cui_dials.c`: `cui_fader_ex`, `cui_knob_ex`; `cui_pickers.c`: `cui__time_dial_body`, `cui_time_picker_ex`, `cui_wheel_picker_ex`, `cui_histogram_slider_ex`, `cui_ruler_slider_ex`, `cui_avatar_upload`; `cui_photo.c`: `cui_cropper_ex`, `cui_image_viewer_ex`. In/out values are dereferenced during the call; paint records receive copied numeric values. | Stack typed values, then copy back into public `ref` arguments before existing synchronization. Texture replacement bookkeeping is unchanged. |
| Dates, ranges, month and tour | `cui_overlay.c`: `cui_date_picker_ex` forwards to `cui_calendar`; `cui_pickers.c`: `cui_date_range_picker_ex`, `cui_range_calendar`, `cui__range_month`, `cui__range_presets`; `cui_month.c`: `cui_month_calendar_ex` aliases its month argument only locally; `cui_tour.c`: `cui_tour_ex`. Values are changed synchronously; paint data copies dates/numeric geometry. | Stack date/range/month/step values. Option State pointers stay owned. |
| Const and other ref records | `cui_ai.c`: `cui_citation` reads the source fields, not a retained source address; `cui_motion.c`: `cui_now_playing_ex` reads/writes `np` and stores copied art/position data; `cui_planning.c`: `cui_kanban_end` writes a result only during end. | Stack source/player/move records; source/player strings stay stable. |
| Counts and selections | `cui_instrument.c`: `cui_event_track_ex` copies `*count` and times into paint data; `cui_devtools.c`: `cui_kv_editor_ex`, `cui_query_builder_ex` write counts at return; `cui_devpanels.c`: `cui_request_bar_ex`; `cui_devviews.c`: `cui_time_histogram_ex` stores copied selection bounds; `cui_actions.c`: `cui_theme_switcher`, `cui__shortcut_recorder_ex_body`. | Stack count/method/selection/combo values. Array and editable-buffer backing storage is unchanged. |
| Native temporary helper frames | `cui_emoji.c`: `cui_emoji_picker_ex` puts `o` in local `cui__em_frame f`; `cui__em_build_grid` copies paint fields and retains only nested names/empty-text pointers. `cui_files.c`: `cui_file_browser_ex` uses local `cui__fb_frame f`. `cui_devtools.c`: `cui_query_builder_ex` uses local `cui__qbf f`. `cui_flame.c`: `cui_trace_view_ex` aliases options, view and selection in local `cui__trf f`; `cui__trf_build` copies view numbers, track names and marker data to paint storage. | Stack outer options and trace view/selection slots: helper-local aliases do not extend the call lifetime. Nested fields and callback roots remain stable. |
| Forwarding wrappers | `cui_code.c`: `cui__code_block_ex_body`, `cui__json_view_ex_body`; `cui_disclosure.c`: `cui__countdown_ex_body`; `cui_media.c`: `cui__counter_ex_body`; `cui_planning.c`: `cui__gantt_ex_body`, `cui__week_calendar_ex_body`; `cui_motion.c`: `cui__fx_text_node`. Public entrypoints synchronously invoke these bodies; they read fields/copy paint values rather than retaining the outer address. | Stack outer records; nested buffers remain stable. |
| Toast | `cui_overlay.c`: `cui_toast_ex` copies title, description and action into fixed native toast arrays using `snprintf`, and copies numeric options. | Stack options; no outer options or string pointer survives in the toast. Wrapper UTF-8 allocation policy is otherwise unchanged. |

### Returned scopes and builders

| Scope/family | Native source and evidence | Storage decision |
| --- | --- | --- |
| Dialog and sheet | `cui_overlay.c`: `cui_dialog_begin`/`cui_sheet_begin` forward individual fields and `open` to `cui__modal_begin`. All `*open` writes (Esc, scrim, close button) occur there. `cui__modal_build` stores id/disabled state, not `open` or options; `cui__modal_end` only closes layout and disabled state. | Stack options and `CBool open`. No delayed close write is lost. |
| Banner, accordion, expandable card, tree | `cui_disclosure.c`: `cui_banner_ex`, `cui_expandable_card_begin`; `cui_data.c`: `cui_accordion_begin`, `cui_tree_begin`; `cui_grid.c`: `cui_tree_cell`. Clicks write `*open` during the call; body/animation state stores values. Banner builds and closes its body synchronously. | Stack open/selected slots and options, including the tree overload without public open state. |
| Grid and table | `cui_grid.c`: `cui_data_grid_begin_ex` stores `b->o = *o`, retains `b->cols = cols`, and writes `*first`/`*last` before return. Row/end calls use builder copies and retained selection/State pointees. `cui_data.c`: `cui_table_begin_ex` retains columns and copies compact/striped/selectable flags; sort is used during header construction. | Stack outer options and first/last. Keep columns, grid selection and State pointees stable. |
| Virtual list | `cui_data.c`: `cui_virtual_list_begin` writes `*first` and `*last`; `cui__vlist_build` stores count/row height/first by value. | Stack range outputs. |
| Carousel | `cui_media.c`: `cui_carousel_begin` copies `b->o = *o` but retains `b->index = index`. `cui_carousel_end` dereferences that index for arrows/dots. | Stack options; preserve persistent `State<int>` lease for index. Never replace that lease with a local. |
| Node editor and node position | `cui_nodes.c`: `cui_node_editor_begin_ex` retains `b->view = view`, copies option limits/flags, and `cui__ne_build_canvas` stores builder `b` as paint data. `cui_node_begin_ex` changes `*x`/`*y` immediately and copies `b->nx`/`b->ny`; `cui_node_end` uses those copies. | Stack editor/node options and node x/y; preserve persistent `State<NodeView>` address across editor scope and painting. |
| Reorder | `cui_extras.c`: `cui_reorder_begin` stores `b->order = order`; `cui_reorder_end` mutates this buffer. | Retain copied `Memory<int>` in frame arena and its journal. This is an array, not a singleton. |
| Other scope options | `cui_layout.c`: `cui_box_begin`, `cui_row_begin`, `cui_column_begin`, `cui__apply_box`; `cui_widgets.c`: `cui_card_begin`, `cui_scroll_begin`, `cui__scroll_begin_ex`; `cui_media.c`: `cui_card_stack_begin`; `cui_motion.c`: `cui_dock_begin`, `cui_spotlight_begin`; `cui_planning.c`: `cui_kanban_column_begin_ex`; `cui_disclosure.c`: `cui_hover_card_begin`; `cui_overlay.c`: `cui_menu_begin`, `cui_popover_begin`; `cui_actions.c`: `cui_user_menu_begin`; `cui_ai.c`: `cui_reasoning_begin`. Builders/nodes receive copied values or independently allocated native paint records, never the caller's outer options address. | Stack outer records; strings and other retained fields keep their existing backing storage. |

All public `ref` arguments describe synchronous copy-in/copy-out semantics. They
do not establish a persistent alias to a managed variable. In particular,
changing dialog/sheet `open` or node x/y after Begin cannot alter native builder
values already copied; the next call observes the new input. No audited public
`ref` argument is retained for a later native write. The two retained mutable
scope singleton addresses use `State<T>` instead. This distinction is required
for safety: a later-retained native pointer cannot be backed by a stack local,
and merely retaining an arena copy would still fail to propagate deferred writes
back to a caller's `ref` variable.

### Stable pointees and owned singleton exceptions

There are **no remaining arena singleton exceptions** to justify. The following
owned objects and pointer fields intentionally remain stable; they are not
synchronous temporary options allocations:

| Storage | Native evidence and lifetime |
| --- | --- |
| `StateLease<T,R>.Pointer` (`NativeMemory.AllocZeroed(sizeof(R))`) | Carousel index and node-editor view have the retention paths above. Grid copies options containing sort/cursor/anchor pointers and later row operations use them (`cui_grid.c`: `cui_data_grid_row`). All `Bind*` option fields use the same owner-bound lease policy, including synchronous selected/date/result slots. Lease addresses never move and survive until context destruction. Active-state writes/reads and owner/thread/disposed checks are unchanged. |
| `TerminalBuffer.Pointer` (`NativeMemory.AllocZeroed(sizeof(cui_terminal_buffer))`) | `cui_terminal.c`: `cui_terminal_init` stores text/line backing pointers; `cui_terminal_write` mutates persistent parser/buffer state. `cui__terminal_ex_body` stores `p->t = t` for `cui__paint_terminal`. The terminal struct, text and line arrays must survive painting/repainting and deferred disposal. Its options record is already a local in `Resources.cs`. |
| Nested array/string pointer fields | `ToNative` still copies nested arrays through `Copy*`, string-pointer arrays through `Strings`/`OptionalStrings`, and UTF-8 through `Utf8`/`CopyUtf8`. Examples: grid columns and selected array, copied chart options' labels/formats/annotations, heatmap labels, emoji names/empty text, text-field placeholder, citation/player text. Shallow-copying an options record never shortens these pointee lifetimes. |
| Editable text, file callback data and paint callback roots | `TextBuffer` remains owned and leased. `cui__text_field` copies editable text into native paint storage but retains placeholder data; broader buffer policy is unchanged. `FileBrowserOpts` uses rooted callback/user fields; callback roots and canvas registrations live through context destruction. File entry names are written into native-provided name storage. Exceptions remain contained by the existing callback bridges. |
| Other arena storage | All remaining `arena.Allocate` sites are variable-length UTF-8, arrays, filesystem entry/name scratch or frame-buffer copies. They retain the existing bounded segmented arena policy. `fixed` is used only to pin managed spans/arrays for synchronous calls, such as image creation, terminal writes and render destinations; it is not used around native locals. |

Arena retirement, array-journal reset and lease retirement still occur only in
`AdoptFrame` after native begin has discarded the previous build. Repeated render
and desktop pending-render behavior, texture disposal, callback containment,
resource disposal after context destruction, and owner/frame/scope validation
were not changed. Arrays are synchronized at the existing grid/reorder mutation
points. `Ui.GetDiagnostics` still reports the same counters; arena use now omits
these 225 per-call allocations (each formerly also incurred arena alignment).
No fixed byte saving is claimed because call frequency and struct sizes vary.

## Authoring, reproduction and verification

The repository-owned [generator and authoring instructions](../scripts/managed-api/README.md)
replace the one-off header scaffold. Four complete source templates preserve the
reviewed API/types/marshalling/scope-dispatch implementation; handwritten files
remain handwritten. Default generation checks exact bytes. Explicit output
refuses to overwrite unrecognized concurrent edits. The optional native check
validates definition and header hashes; it is a drift detector, not a substitute
for reviewing native retention or running behavioral checks.

```sh
python3 -B cui/scripts/managed-api/generate.py --check
python3 -B cui/scripts/managed-api/generate.py --check --native-root "$CUI_SOURCE"
TMPDIR="$SCRATCH/tmp" python3 -B cui/scripts/managed-api/generate.py --output-dir "$SCRATCH/reproduced"
TMPDIR="$SCRATCH/tmp" python3 -B cui/scripts/managed-api/generate.py --output-dir "$SCRATCH/reproduced"
TMPDIR="$SCRATCH/tmp" dotnet build cui/src/ManagedCui/ManagedCui.csproj --artifacts-path "$SCRATCH/artifacts"
```

`CUI_SOURCE` is the audited upstream checkout; `SCRATCH` is an external scratch
root. No machine-specific path is required by source or generator. This task's
verification comprises exact template round-trip, idempotent reproduction,
unknown-edit rejection, native evidence verification, review of unchanged
signatures/guards, and a wrapper-only build with normal project references.
The wrapper-only build passed with zero errors and five CS1718 warnings in
the unchanged translated project reference; the wrapper emitted no warnings.
All four template outputs matched exactly, a second reproduction preserved
bytes and modification times, unrecognized edits were rejected without writes,
and all 214 native definitions plus the header matched the recorded evidence.
A source-preservation check confirmed identical public declarations and exact
source equality after only the 225 storage substitutions and two comments. The
63 remaining arena allocation sites all allocate variable-length data.

The coordinator's pre-rename raw/processed × JIT/NativeAOT library matrix passed
75 safety assertions per variant, including stack overwrite/compacting GC before
deferred rendering and a descriptor-only frame with zero arena use. Three
benchmark trials per variant passed allocation, timing and retention gates;
pre-rename arena-use measurements are recorded in `MANAGED-LIBRARY-VALIDATION.json`.
Full palette/page, OS, native geometry and standalone qualification are separate
coordinator gates. The implementation delegate did not run or modify them.
Those historical wrapper/performance receipts bind baseline `d5a662499`;
[namespace validation](NAMESPACE-VALIDATION.json) records post-rename safety
checks at that campaign's native revision. The current native manifest is
separately refreshed to CUI 1.4.0 as described above.

## Complete converted-site inventory

Every listed local is now passed by address only during its native call. Types
are the unmanaged native representation (`CBool` for C bool); scalar/struct
writeback still occurs before the existing `SyncState`. Names are qualified by
managed file where overloads share a method name. The manifest retains this same
inventory in machine-readable form.

| Managed method (file) | Native symbol / source | Stack locals |
| --- | --- | --- |
| `AccordionBegin` (`Ui.Api.cs`) | `cui_accordion_begin` / `cui_data.c` | `CBool native_open` |
| `ActivityRings` (`Ui.Api.cs`) | `cui_activity_rings_ex` / `cui_viz.c` | `cui_rings_opts native_opts` |
| `AgentPlan` (`Ui.Api.cs`) | `cui_agent_plan_ex` / `cui_ai.c` | `cui_agent_plan_opts native_opts` |
| `AgentStatus` (`Ui.Api.cs`) | `cui_agent_status_ex` / `cui_ai.c` | `cui_agent_status_opts native_opts` |
| `Alert` (`Ui.Api.cs`) | `cui_alert_ex` / `cui_widgets.c` | `cui_alert_opts native_opts` |
| `ApiExplorer` (`Ui.Api.cs`) | `cui_api_explorer_ex` / `cui_devpanels.c` | `cui_api_explorer_opts native_opts` |
| `Avatar` (`Ui.Api.cs`) | `cui_avatar_ex` / `cui_widgets.c` | `cui_avatar_opts native_opts` |
| `AvatarUpload` (`Ui.Api.cs`) | `cui_avatar_upload` / `cui_pickers.c` | `ulong native_image` |
| `Badge` (`Ui.Api.cs`) | `cui_badge_ex` / `cui_widgets.c` | `cui_badge_opts native_opts` |
| `Banner` (`Ui.Api.cs`) | `cui_banner_ex` / `cui_disclosure.c` | `CBool native_open`, `cui_banner_opts native_opts` |
| `BarChart` (`Ui.Api.cs`) | `cui_bar_chart_ex` / `cui_charts.c` | `cui_chart_opts native_opts` |
| `BarList` (`Ui.Api.cs`) | `cui_bar_list_ex` / `cui_viz.c` | `cui_bar_list_opts native_opts` |
| `Blame` (`Ui.Api.cs`) | `cui_blame_ex` / `cui_devviews.c` | `cui_blame_opts native_opts` |
| `BoxBegin` (`Ui.Api.cs`) | `cui_box_begin` / `cui_layout.c` | `cui_box native_opts` |
| `Button` (`Ui.Api.cs`) | `cui_button_ex` / `cui_widgets.c` | `cui_button_opts native_opts` |
| `CalendarHeatmap` (`Ui.Api.cs`) | `cui_calendar_heatmap_ex` / `cui_viz.c` | `cui_calendar_heatmap_opts native_opts` |
| `Candlestick` (`Ui.Api.cs`) | `cui_candlestick_ex` / `cui_viz.c` | `cui_candlestick_opts native_opts` |
| `CardBegin` (`Ui.Api.cs`) | `cui_card_begin` / `cui_widgets.c` | `cui_card_opts native_opts` |
| `CardStackBegin` (`Ui.Api.cs`) | `cui_card_stack_begin` / `cui_media.c` | `cui_card_stack_opts native_opts` |
| `CarouselBegin` (`Ui.Api.cs`) | `cui_carousel_begin` / `cui_media.c` | `cui_carousel_opts native_opts` |
| `ChatComposer` (`Ui.Api.cs`) | `cui_chat_composer_ex` / `cui_ai.c` | `cui_composer_opts native_opts` |
| `ChatMessage` (`Ui.Api.cs`) | `cui_chat_message_ex` / `cui_social.c` | `cui_chat_opts native_opts` |
| `Checkbox` (`Ui.Api.cs`) | `cui_checkbox_ex` / `cui_controls.c` | `CBool native_value`, `cui_check_opts native_opts` |
| `Checklist` (`Ui.Api.cs`) | `cui_checklist_ex` / `cui_disclosure.c` | `cui_checklist_opts native_opts` |
| `Chip` (`Ui.Api.cs`) | `cui_chip_ex` / `cui_controls.c` | `CBool native_selected`, `cui_chip_opts native_opts` |
| `Citation` (`Ui.Api.cs`) | `cui_citation` / `cui_ai.c` | `cui_source native_source` |
| `CodeBlock` (`Ui.Api.cs`) | `cui_code_block_ex` / `cui_code.c` | `cui_code_opts native_opts` |
| `ColorPicker` (`Ui.Api.cs`) | `cui_color_picker` / `cui_overlay.c` | `cui_color native_value` |
| `ColumnBegin` (`Ui.Api.cs`) | `cui_column_begin` / `cui_layout.c` | `cui_box native_opts` |
| `CommandPalette` (`Ui.Api.cs`) | `cui_command_palette` / `cui_overlay.c` | `CBool native_open` |
| `Comment` (`Ui.Api.cs`) | `cui_comment_ex` / `cui_social.c` | `cui_comment_opts native_opts` |
| `CommitGraph` (`Ui.Api.cs`) | `cui_commit_graph_ex` / `cui_graph.c` | `cui_commit_graph_opts native_opts` |
| `ConfirmButton` (`Ui.Api.cs`) | `cui_confirm_button_ex` / `cui_actions.c` | `cui_confirm_opts native_opts` |
| `ConfirmPopover` (`Ui.Api.cs`) | `cui_confirm_popover_ex` / `cui_disclosure.c` | `cui_confirm_popover_opts native_opts` |
| `ContextMeter` (`Ui.Api.cs`) | `cui_context_meter_ex` / `cui_ai.c` | `cui_context_opts native_opts` |
| `ControlTile` (`Ui.Api.cs`) | `cui_control_tile_ex` / `cui_motion.c` | `CBool native_on`, `cui_tile_opts native_opts` |
| `Countdown` (`Ui.Api.cs`) | `cui_countdown_ex` / `cui_disclosure.c` | `cui_countdown_opts native_opts` |
| `Counter` (`Ui.Api.cs`) | `cui_counter_ex` / `cui_media.c` | `cui_counter_opts native_opts` |
| `Coverage` (`Ui.Api.cs`) | `cui_coverage_ex` / `cui_devviews.c` | `cui_coverage_opts native_opts` |
| `Cropper` (`Ui.Api.cs`) | `cui_cropper_ex` / `cui_photo.c` | `cui_rect native_crop`, `cui_cropper_opts native_opts` |
| `DataGridBegin` (`Ui.Api.cs`) | `cui_data_grid_begin_ex` / `cui_grid.c` | `int native_first`, `int native_last`, `cui_grid_opts native_opts` |
| `DatePicker` (`Ui.Api.cs`) | `cui_date_picker_ex` / `cui_overlay.c` | `cui_date native_value`, `cui_date_opts native_opts` |
| `DateRangePicker` (`Ui.Api.cs`) | `cui_date_range_picker_ex` / `cui_pickers.c` | `cui_date_range native_range`, `cui_date_range_opts native_opts` |
| `DialogBegin` (`Ui.Api.cs`) | `cui_dialog_begin` / `cui_overlay.c` | `CBool native_open`, `cui_dialog_opts native_opts` |
| `DockBegin` (`Ui.Api.cs`) | `cui_dock_begin` / `cui_motion.c` | `cui_dock_opts native_opts` |
| `DockItem` (`Ui.Api.cs`) | `cui_dock_item_ex` / `cui_motion.c` | `cui_dock_item_opts native_opts` |
| `DonutChart` (`Ui.Api.cs`) | `cui_donut_chart_ex` / `cui_charts.c` | `cui_chart_opts native_opts` |
| `DotChart` (`Ui.Api.cs`) | `cui_dot_chart_ex` / `cui_instrument.c` | `cui_dot_chart_opts native_opts` |
| `DotDisplay` (`Ui.Api.cs`) | `cui_dot_display_ex` / `cui_instrument.c` | `cui_dot_display_opts native_opts` |
| `Dropzone` (`Ui.Api.cs`) | `cui_dropzone_ex` / `cui_pickers.c` | `cui_dropzone_opts native_opts` |
| `EmojiPicker` (`Ui.Api.cs`) | `cui_emoji_picker_ex` / `cui_emoji.c` | `cui_emoji_picker_opts native_opts` |
| `EmptyState` (`Ui.Api.cs`) | `cui_empty_state_ex` / `cui_widgets.c` | `cui_empty_opts native_opts` |
| `EventTrack` (`Ui.Api.cs`) | `cui_event_track_ex` / `cui_instrument.c` | `int native_count`, `cui_event_track_opts native_opts` |
| `ExpandableCardBegin` (`Ui.Api.cs`) | `cui_expandable_card_begin` / `cui_disclosure.c` | `CBool native_open`, `cui_expandable_card_opts native_opts` |
| `ExpandableText` (`Ui.Api.cs`) | `cui_expandable_text_ex` / `cui_disclosure.c` | `cui_expandable_text_opts native_opts` |
| `Fader` (`Ui.Api.cs`) | `cui_fader_ex` / `cui_dials.c` | `float native_value`, `cui_fader_opts native_opts` |
| `FeatureFlags` (`Ui.Api.cs`) | `cui_feature_flags_ex` / `cui_devpanels.c` | `cui_feature_flags_opts native_opts` |
| `FileBrowser` (`Ui.Api.cs`) | `cui_file_browser_ex` / `cui_files.c` | `cui_file_browser_opts native_opts` |
| `FilePicker` (`Ui.Api.cs`) | `cui_file_picker_ex` / `cui_files.c` | `CBool native_open`, `cui_file_picker_opts native_opts` |
| `Funnel` (`Ui.Api.cs`) | `cui_funnel_ex` / `cui_viz.c` | `cui_funnel_opts native_opts` |
| `Gantt` (`Ui.Api.cs`) | `cui_gantt_ex` / `cui_planning.c` | `cui_gantt_opts native_opts` |
| `GradientText` (`Ui.Api.cs`) | `cui_gradient_text_ex` / `cui_motion.c` | `cui_gradient_text_opts native_opts` |
| `GraphView` (`Ui.Api.cs`) | `cui_graph_view_ex` / `cui_graph.c` | `cui_graph_opts native_opts` |
| `Heatmap` (`Ui.Api.cs`) | `cui_heatmap_ex` / `cui_viz.c` | `cui_heatmap_opts native_opts` |
| `HistogramSlider` (`Ui.Api.cs`) | `cui_histogram_slider_ex` / `cui_pickers.c` | `float native_lo`, `float native_hi`, `cui_histogram_opts native_opts` |
| `HoldButton` (`Ui.Api.cs`) | `cui_hold_button_ex` / `cui_extras.c` | `cui_hold_opts native_opts` |
| `HoverCardBegin` (`Ui.Api.cs`) | `cui_hover_card_begin` / `cui_disclosure.c` | `cui_hover_card_opts native_opts` |
| `IconButton` (`Ui.Api.cs`) | `cui_icon_button_ex` / `cui_widgets.c` | `cui_button_opts native_opts` |
| `ImageCompare` (`Ui.Api.cs`) | `cui_image_compare_ex` / `cui_media.c` | `float native_pos`, `cui_compare_opts native_opts` |
| `Image` (`Ui.Api.cs`) | `cui_image_ex` / `cui_widgets.c` | `cui_image_opts native_opts` |
| `ImageViewer` (`Ui.Api.cs`) | `cui_image_viewer_ex` / `cui_photo.c` | `cui_image_view native_view`, `cui_image_viewer_opts native_opts` |
| `InlineEdit` (`Ui.Api.cs`) | `cui_inline_edit_ex` / `cui_actions.c` | `cui_inline_edit_opts native_opts` |
| `Input` (`Ui.Api.cs`) | `cui_input_ex` / `cui_textedit.c` | `cui_input_opts native_opts` |
| `JsonView` (`Ui.Api.cs`) | `cui_json_view_ex` / `cui_code.c` | `cui_json_opts native_opts` |
| `KanbanColumnBegin` (`Ui.Api.cs`) | `cui_kanban_column_begin_ex` / `cui_planning.c` | `cui_kanban_column_opts native_opts` |
| `KanbanEnd` (`Ui.Api.cs`) | `cui_kanban_end` / `cui_planning.c` | `cui_kanban_move native_move` |
| `Keymap` (`Ui.Api.cs`) | `cui_keymap_ex` / `cui_devtools.c` | `cui_keymap_opts native_opts` |
| `Knob` (`Ui.Api.cs`) | `cui_knob_ex` / `cui_dials.c` | `float native_value`, `cui_knob_opts native_opts` |
| `KvEditor` (`Ui.Api.cs`) | `cui_kv_editor_ex` / `cui_devtools.c` | `int native_count`, `cui_kv_editor_opts native_opts` |
| `LevelMeter` (`Ui.Api.cs`) | `cui_level_meter_ex` / `cui_dials.c` | `cui_level_meter_opts native_opts` |
| `LineChart` (`Ui.Api.cs`) | `cui_line_chart_ex` / `cui_charts.c` | `cui_chart_opts native_opts` |
| `ListItem` (`Ui.Api.cs`) | `cui_list_item_ex` / `cui_data.c` | `cui_list_item_opts native_opts` |
| `Loader` (`Ui.Api.cs`) | `cui_loader_ex` / `cui_motion.c` | `cui_loader_opts native_opts` |
| `MenuBegin` (`Ui.Api.cs`) | `cui_menu_begin` / `cui_overlay.c` | `cui_menu_opts native_opts` |
| `MenuItem` (`Ui.Api.cs`) | `cui_menu_item_ex` / `cui_overlay.c` | `cui_menu_item_opts native_opts` |
| `Metric` (`Ui.Api.cs`) | `cui_metric_ex` / `cui_widgets.c` | `cui_metric_opts native_opts` |
| `MoneyInput` (`Ui.Api.cs`) | `cui_money_input_ex` / `cui_actions.c` | `double native_value`, `cui_money_opts native_opts` |
| `MonthCalendar` (`Ui.Api.cs`) | `cui_month_calendar_ex` / `cui_month.c` | `cui_date native_month`, `cui_month_opts native_opts` |
| `MultiSelect` (`Ui.Api.cs`) | `cui_multi_select_ex` / `cui_pickers.c` | `cui_multi_select_opts native_opts` |
| `NavItem` (`Ui.Api.cs`) | `cui_nav_item_ex` / `cui_controls.c` | `cui_nav_opts native_opts` |
| `NodeBegin` (`Ui.Api.cs`) | `cui_node_begin_ex` / `cui_nodes.c` | `float native_x`, `float native_y`, `cui_node_opts native_opts` |
| `NodeEditorBegin` (`Ui.Api.cs`) | `cui_node_editor_begin_ex` / `cui_nodes.c` | `cui_node_editor_opts native_opts` |
| `NowPlaying` (`Ui.Api.cs`) | `cui_now_playing_ex` / `cui_motion.c` | `cui_now_playing native_np`, `cui_now_playing_opts native_opts` |
| `Number` (`Ui.Api.cs`) | `cui_number_ex` / `cui_textedit.c` | `double native_value`, `cui_number_opts native_opts` |
| `Pagination` (`Ui.Api.cs`) | `cui_pagination` / `cui_controls.c` | `int native_page` |
| `Pipeline` (`Ui.Api.cs`) | `cui_pipeline_ex` / `cui_devtools.c` | `cui_pipeline_opts native_opts` |
| `PopoverBegin` (`Ui.Api.cs`) | `cui_popover_begin` / `cui_overlay.c` | `cui_popover_opts native_opts` |
| `Problems` (`Ui.Api.cs`) | `cui_problems_ex` / `cui_devtools.c` | `cui_problems_opts native_opts` |
| `Progress` (`Ui.Api.cs`) | `cui_progress_ex` / `cui_widgets.c` | `cui_progress_opts native_opts` |
| `PromptSuggestions` (`Ui.Api.cs`) | `cui_prompt_suggestions_ex` / `cui_ai.c` | `cui_prompt_suggestions_opts native_opts` |
| `QueryBuilder` (`Ui.Api.cs`) | `cui_query_builder_ex` / `cui_devtools.c` | `int native_count`, `cui_query_opts native_opts` |
| `RadarChart` (`Ui.Api.cs`) | `cui_radar_chart_ex` / `cui_viz.c` | `cui_radar_opts native_opts` |
| `RadialGauge` (`Ui.Api.cs`) | `cui_radial_gauge_ex` / `cui_viz.c` | `cui_radial_gauge_opts native_opts` |
| `RadioCards` (`Ui.Api.cs`) | `cui_radio_cards_ex` / `cui_extras.c` | `int native_index`, `cui_radio_cards_opts native_opts` |
| `Radio` (`Ui.Api.cs`) | `cui_radio_ex` / `cui_controls.c` | `int native_value`, `cui_check_opts native_opts` |
| `RangeCalendar` (`Ui.Api.cs`) | `cui_range_calendar` / `cui_pickers.c` | `cui_date_range native_range` |
| `RangeSlider` (`Ui.Api.cs`) | `cui_range_slider` / `cui_controls.c` | `float native_lo`, `float native_hi` |
| `Rating` (`Ui.Api.cs`) | `cui_rating` / `cui_controls.c` | `int native_value` |
| `Readout` (`Ui.Api.cs`) | `cui_readout_ex` / `cui_instrument.c` | `cui_readout_opts native_opts` |
| `ReasoningBegin` (`Ui.Api.cs`) | `cui_reasoning_begin` / `cui_ai.c` | `cui_reasoning_opts native_opts` |
| `RegexTester` (`Ui.Api.cs`) | `cui_regex_tester_ex` / `cui_devpanels.c` | `cui_regex_tester_opts native_opts` |
| `RequestBar` (`Ui.Api.cs`) | `cui_request_bar_ex` / `cui_devpanels.c` | `int native_method`, `cui_request_bar_opts native_opts` |
| `ResponseActions` (`Ui.Api.cs`) | `cui_response_actions_ex` / `cui_ai.c` | `cui_response_actions_opts native_opts` |
| `RowBegin` (`Ui.Api.cs`) | `cui_row_begin` / `cui_layout.c` | `cui_box native_opts` |
| `RulerSlider` (`Ui.Api.cs`) | `cui_ruler_slider_ex` / `cui_pickers.c` | `float native_value`, `cui_ruler_opts native_opts` |
| `Sankey` (`Ui.Api.cs`) | `cui_sankey_ex` / `cui_viz.c` | `cui_sankey_opts native_opts` |
| `ScatterChart` (`Ui.Api.cs`) | `cui_scatter_chart_ex` / `cui_viz.c` | `cui_scatter_opts native_opts` |
| `SchedulePicker` (`Ui.Api.cs`) | `cui_schedule_picker_ex` / `cui_devtools.c` | `cui_schedule_opts native_opts` |
| `ScrollBegin` (`Ui.Api.cs`) | `cui_scroll_begin` / `cui_widgets.c` | `cui_scroll_opts native_opts` |
| `Scrub` (`Ui.Api.cs`) | `cui_scrub_ex` / `cui_actions.c` | `float native_value`, `cui_scrub_opts native_opts` |
| `Search` (`Ui.Api.cs`) | `cui_search_ex` / `cui_textedit.c` | `cui_input_opts native_opts` |
| `Segmented` (`Ui.Api.cs`) | `cui_segmented_ex` / `cui_controls.c` | `int native_index`, `cui_segmented_opts native_opts` |
| `Select` (`Ui.Api.cs`) | `cui_select_ex` / `cui_overlay.c` | `int native_index`, `cui_select_opts native_opts` |
| `SheetBegin` (`Ui.Api.cs`) | `cui_sheet_begin` / `cui_overlay.c` | `CBool native_open`, `cui_sheet_opts native_opts` |
| `ShimmerText` (`Ui.Api.cs`) | `cui_shimmer_text_ex` / `cui_motion.c` | `cui_text_fx_opts native_opts` |
| `ShortcutRecorder` (`Ui.Api.cs`) | `cui_shortcut_recorder_ex` / `cui_actions.c` | `cui_key_combo native_combo`, `cui_shortcut_recorder_opts native_opts` |
| `SignaturePad` (`Ui.Api.cs`) | `cui_signature_pad_ex` / `cui_pickers.c` | `cui_signature_opts native_opts` |
| `Slider` (`Ui.Api.cs`) | `cui_slider_ex` / `cui_controls.c` | `float native_value`, `cui_slider_opts native_opts` |
| `SpeedDial` (`Ui.Api.cs`) | `cui_speed_dial_ex` / `cui_actions.c` | `cui_speed_dial_opts native_opts` |
| `SplitBar` (`Ui.Api.cs`) | `cui_split_bar_ex` / `cui_viz.c` | `cui_split_bar_opts native_opts` |
| `SplitButton` (`Ui.Api.cs`) | `cui_split_button_ex` / `cui_actions.c` | `cui_split_button_opts native_opts` |
| `SpotlightBegin` (`Ui.Api.cs`) | `cui_spotlight_begin` / `cui_motion.c` | `cui_spotlight_opts native_opts` |
| `StackTrace` (`Ui.Api.cs`) | `cui_stack_trace_ex` / `cui_devpanels.c` | `cui_stack_trace_opts native_opts` |
| `StatusButton` (`Ui.Api.cs`) | `cui_status_button_ex` / `cui_actions.c` | `cui_status_button_opts native_opts` |
| `Switch` (`Ui.Api.cs`) | `cui_switch_ex` / `cui_controls.c` | `CBool native_value`, `cui_check_opts native_opts` |
| `TabBar` (`Ui.Api.cs`) | `cui_tab_bar_ex` / `cui_motion.c` | `int native_selected`, `cui_tab_bar_opts native_opts` |
| `TableBegin` (`Ui.Api.cs`) | `cui_table_begin_ex` / `cui_data.c` | `cui_table_opts native_opts` |
| `Tabs` (`Ui.Api.cs`) | `cui_tabs_ex` / `cui_controls.c` | `int native_index`, `cui_tabs_opts native_opts` |
| `TagInput` (`Ui.Api.cs`) | `cui_tag_input_ex` / `cui_extras.c` | `cui_tag_opts native_opts` |
| `TestExplorer` (`Ui.Api.cs`) | `cui_test_explorer_ex` / `cui_devtools.c` | `cui_test_explorer_opts native_opts` |
| `Text` (`Ui.Api.cs`) | `cui_text_ex` / `cui_widgets.c` | `cui_text_opts native_opts` |
| `TextReveal` (`Ui.Api.cs`) | `cui_text_reveal_ex` / `cui_motion.c` | `cui_text_fx_opts native_opts` |
| `Textarea` (`Ui.Api.cs`) | `cui_textarea_ex` / `cui_textedit.c` | `cui_input_opts native_opts` |
| `ThemeSwitcher` (`Ui.Api.cs`) | `cui_theme_switcher` / `cui_actions.c` | `int native_mode` |
| `Ticker` (`Ui.Api.cs`) | `cui_ticker_ex` / `cui_media.c` | `cui_ticker_opts native_opts` |
| `TimeDial` (`Ui.Api.cs`) | `cui_time_dial` / `cui_pickers.c` | `int native_hour`, `int native_minute` |
| `TimeHistogram` (`Ui.Api.cs`) | `cui_time_histogram_ex` / `cui_devviews.c` | `double native_sel_start`, `double native_sel_end`, `cui_time_histogram_opts native_opts` |
| `TimePicker` (`Ui.Api.cs`) | `cui_time_picker_ex` / `cui_pickers.c` | `int native_hour`, `int native_minute`, `cui_time_opts native_opts` |
| `TimelineItem` (`Ui.Api.cs`) | `cui_timeline_item_ex` / `cui_extras.c` | `cui_timeline_opts native_opts` |
| `Toast` (`Ui.Api.cs`) | `cui_toast_ex` / `cui_overlay.c` | `cui_toast_opts native_opts` |
| `ToggleGroup` (`Ui.Api.cs`) | `cui_toggle_group_ex` / `cui_actions.c` | `cui_toggle_group_opts native_opts` |
| `ToolCall` (`Ui.Api.cs`) | `cui_tool_call_ex` / `cui_ai.c` | `cui_tool_call_opts native_opts` |
| `Tour` (`Ui.Api.cs`) | `cui_tour_ex` / `cui_tour.c` | `int native_step`, `cui_tour_opts native_opts` |
| `TraceView` (`Ui.Api.cs`) | `cui_trace_view_ex` / `cui_flame.c` | `cui_trace_view native_view`, `int native_selected`, `cui_trace_opts native_opts` |
| `TreeBegin` (`Ui.Api.cs`) | `cui_tree_begin` / `cui_data.c` | `CBool native_open`, `cui_tree_opts native_opts` |
| `TreeCell` (`Ui.Api.cs`) | `cui_tree_cell` / `cui_grid.c` | `CBool native_open` |
| `Treemap` (`Ui.Api.cs`) | `cui_treemap_ex` / `cui_viz.c` | `cui_treemap_opts native_opts` |
| `Typewriter` (`Ui.Api.cs`) | `cui_typewriter_ex` / `cui_motion.c` | `cui_text_fx_opts native_opts` |
| `Uptime` (`Ui.Api.cs`) | `cui_uptime_ex` / `cui_devviews.c` | `cui_uptime_opts native_opts` |
| `UserMenuBegin` (`Ui.Api.cs`) | `cui_user_menu_begin` / `cui_actions.c` | `cui_user_menu_opts native_opts` |
| `VirtualListBegin` (`Ui.Api.cs`) | `cui_virtual_list_begin` / `cui_data.c` | `int native_first`, `int native_last` |
| `VoiceOrb` (`Ui.Api.cs`) | `cui_voice_orb_ex` / `cui_ai.c` | `cui_voice_orb_opts native_opts` |
| `Waffle` (`Ui.Api.cs`) | `cui_waffle_ex` / `cui_viz.c` | `cui_waffle_opts native_opts` |
| `Waterfall` (`Ui.Api.cs`) | `cui_waterfall_ex` / `cui_flame.c` | `cui_waterfall_opts native_opts` |
| `Waveform` (`Ui.Api.cs`) | `cui_waveform_ex` / `cui_motion.c` | `float native_progress`, `cui_waveform_opts native_opts` |
| `WeekCalendar` (`Ui.Api.cs`) | `cui_week_calendar_ex` / `cui_planning.c` | `cui_week_opts native_opts` |
| `WheelPicker` (`Ui.Api.cs`) | `cui_wheel_picker_ex` / `cui_pickers.c` | `int native_index`, `cui_wheel_opts native_opts` |
| `WordRotate` (`Ui.Api.cs`) | `cui_word_rotate_ex` / `cui_motion.c` | `cui_text_fx_opts native_opts` |
| `TreeBegin` (`Ui.Overloads.cs`) | `cui_tree_begin` / `cui_data.c` | `cui_tree_opts native` |
| `RadioGroup` (`Ui.Overloads.cs`) | `cui_radio_group` / `cui_controls.c` | `int state` |
| `TextUtf8` (`Utf8.cs`) | `cui_text_ex` / `cui_widgets.c` | `Raw.cui_text_opts options` |
| `ButtonUtf8` (`Utf8.cs`) | `cui_button_ex` / `cui_widgets.c` | `Raw.cui_button_opts options` |
