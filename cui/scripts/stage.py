"""Small guarded source adaptations; acquired upstream sources remain untouched."""
import shutil


def _replacer(destination, changes):
    def replace(file, old, new, reason):
        path = destination / file
        text = path.read_text(encoding="utf-8")
        if text.count(old) != 1:
            raise RuntimeError(f"Expected one adaptation anchor in {file}: {reason}")
        path.write_text(text.replace(old, new), encoding="utf-8")
        changes.append({"file": file, "reason": reason})
    return replace


def adapt_widgets(destination):
    """Apply upstream widget repairs to a private managed or native source copy."""
    changes = []
    replace = _replacer(destination, changes)
    # Popover contents have an additional ID scope. Close from the same scope
    # as toggle(), otherwise close() creates a different, already-closed popup.
    replace("examples/gallery_more.c",
            '        cui_popover(ui, "emoji-pop", .placement = CUI_PLACE_BOTTOM_END, .width = 340, .pad = 0) {\n'
            '            const char *e = cui_emoji_picker(ui, "emoji", .recent = recent, .recent_cap = sizeof recent, .tone = &tone, .height = 260);\n'
            '            if (e && strlen(message) + strlen(e) < sizeof message) { strcat(message, e); cui_popover_close(ui, "emoji-pop"); }\n'
            '        }',
            '        const char *e = NULL;\n'
            '        cui_popover(ui, "emoji-pop", .placement = CUI_PLACE_BOTTOM_END, .width = 340, .pad = 0) {\n'
            '            e = cui_emoji_picker(ui, "emoji", .recent = recent, .recent_cap = sizeof recent, .tone = &tone, .height = 260);\n'
            '        }\n'
            '        if (e && strlen(message) + strlen(e) < sizeof message) { strcat(message, e); cui_popover_close(ui, "emoji-pop"); }',
            "Upstream gallery: close the emoji popover in its caller ID scope after selection")
    return changes


def adapt_text_renderer(destination):
    """Connect editable text's missing glyphs to CUI's existing color emoji engine.

    Font fallbacks feed stb's monochrome outline atlas, not the separate color
    emoji atlas. Editing deliberately bypasses shaping; use its shared iterator
    so measurement, hit testing, selection and painting agree on emoji advances.
    Keep upstream's code-point editing and original UTF-8 buffer untouched.
    """
    changes = []
    replace = _replacer(destination, changes)
    replace("src/cui_text.c", '#include "cui_internal.h"\n',
            '#include "cui_internal.h"\n'
            'void cui__draw_inline_emoji(cui_ctx *ui, const char *text, int len, cui_rect r, float alpha);\n',
            "Declare the existing emoji renderer bridge for editable text, including text opacity")
    replace("src/cui_text.c",
            '    float adv;       /* advance (points) */\n',
            '    float adv;       /* advance (points) */\n'
            '    bool emoji;      /* missing outline drawn by the color emoji engine */\n',
            "Track color emoji in the shared text measurement/hit/paint iterator")
    replace("src/cui_text.c",
            '    float adv = cui__advance_em(fd, gi, cp) * gs;\n',
            '    it->emoji = ui->text_unshaped && !gi && cp >= 128 &&\n'
            '        cui_emoji_supported(ui, it->s + it->start, n);\n'
            '    float adv = it->emoji ? gs : cui__advance_em(fd, gi, cp) * gs;\n',
            "Use one em for supported missing emoji in editable text; retain ordinary glyph advances")
    replace("src/cui_text.c",
            '    if (!fd->is_icon) { adv += it->track; it->center_off += it->track * 0.5f; }\n',
            '    if (!fd->is_icon && !it->emoji) { adv += it->track; it->center_off += it->track * 0.5f; }\n',
            "Keep emoji cells square without changing ordinary optical text tracking")
    replace("src/cui_text.c",
            "        if (it.cp == ' ' || it.cp == '\\n' || it.cp == '\\t' || !it.gi) continue;\n",
            "        if (it.cp == ' ' || it.cp == '\\n' || it.cp == '\\t' || (!it.gi && !it.emoji)) continue;\n",
            "Paint supported color emoji even though the text outline font has no glyph")
    replace("src/cui_text.c",
            '        cui_glyph *g = cui__glyph_get(ui, it.gfont, it.gi, gs_px, subpx);\n',
            '        if (it.emoji) {\n'
            '            /* Center the color cell on the text cap height, on the same baseline.\n'
            '             * The ordinary edit clip/transform and text effects still apply. */\n'
            '            cui_rect r = { fl / s, gy / s - base->cap_height * size * 0.5f - it.gsize * 0.5f, it.gsize, it.gsize };\n'
            '            cui__draw_inline_emoji(ui, text + it.start, it.end - it.start, r, gc.a / 255.0f);\n'
            '            continue;\n'
            '        }\n'
            '        cui_glyph *g = cui__glyph_get(ui, it.gfont, it.gi, gs_px, subpx);\n',
            "Render emoji inside the original edit paint using CUI's existing color atlas, clipping and effects")
    replace("src/cui_emoji.c",
            '    enum { TAPS = 8 };\n'
            '    if ((sw + dw - 1) / dw + 1 > TAPS || (sh + dh - 1) / dh + 1 > TAPS) return;   /* more than 7 to 1: not worth a path */\n',
            '    /* Text-sized emoji can shrink a bitmap strike by more than 7:1.\n'
            '     * Retain the area filter, sizing its allocated weights for that ratio. */\n'
            '    const int TAPS = CUI_MAX(8, CUI_MAX((sw + dw - 1) / dw + 1, (sh + dh - 1) / dh + 1));\n',
            "Render small inline bitmap emoji instead of silently returning a transparent successful raster")
    replace("src/cui_emoji.c",
            'bool cui_emoji_supported(cui_ctx *ui, const char *utf8, int len) {\n',
            '/* Internal text bridge: preserve foreground opacity without tinting color emoji. */\n'
            'void cui__draw_inline_emoji(cui_ctx *ui, const char *text, int len, cui_rect r, float alpha) {\n'
            '    cui__em_draw(ui, text, len, r, alpha, NULL);\n'
            '}\n\n'
            'bool cui_emoji_supported(cui_ctx *ui, const char *utf8, int len) {\n',
            "Reuse upstream emoji font discovery, color formats, cache and renderer; preserve text opacity")
    return changes


def stage(source, destination):
    shutil.copytree(source, destination, ignore=shutil.ignore_patterns(".git", "build", "__pycache__"))
    changes = []
    replace = _replacer(destination, changes)

    replace("src/platform/cui_app.c",
            "cui_app *cui_app_create(const cui_app_desc *desc) {\n",
            "cui_app *cui_app_create(const cui_app_desc *desc) {\n"
            '    if ((desc && desc->renderer == CUI_RENDERER_SOFTWARE) || cui__env_is("CUI_RENDERER", "software")) return NULL;\n',
            "GLFW cannot display software pixels; reject an unsupported window request")
    replace("src/platform/cui_app.c",
            '        if (d->renderer == CUI_RENDERER_OPENGL) fprintf(stderr, "cui: OpenGL unavailable, using the software renderer\\n");\n'
            "        cui__app_make_software(app);\n    }\n    cui_platform_iface pi;",
            '        fprintf(stderr, "cui: OpenGL unavailable for a GLFW window\\n");\n'
            "        cui_app_destroy(app);\n        return NULL;\n    }\n    cui_platform_iface pi;",
            "Clean up and fail if OpenGL is unavailable, instead of returning an invisible window")
    replace("src/platform/cui_app_glfw.c", "    cui__glfw *g = calloc(1, sizeof *g);\n    if (!g) return false;",
            "    cui__glfw *g = calloc(1, sizeof *g);\n    if (!g) { glfwTerminate(); return false; }",
            "Pair GLFW initialization on allocation failure")

    # The host supplies portable filesystem operations. Only platform defaults
    # in the translated browser need to vary; its sorting and filtering remain C.
    replace("src/cui_files.c", '#include "cui_internal.h"',
            '#include "cui_internal.h"\nint cui_host_is_windows(void);',
            "Declare runtime platform query")
    replace("src/cui_files.c", "#if defined(_WIN32)\n#define CUI__SEP '\\\\'\n#else\n#define CUI__SEP '/'\n#endif",
            "#define CUI__SEP (cui_host_is_windows() ? '\\\\' : '/')",
            "Choose default path separator at runtime")
    replace("src/cui_files.c", '#if defined(_WIN32)\n    if (!home) home = getenv("USERPROFILE");\n#endif',
            '    if (cui_host_is_windows() && (!home || !*home)) home = getenv("USERPROFILE");',
            "Use the Windows home fallback at runtime")
    replace("src/cui_files.c", '#if defined(_WIN32)\n    snprintf(out[*n], CUI__FB_PATH, "C:\\\\"); labels[*n] = "C:\\\\"; (*n)++;\n#else\n    snprintf(out[*n], CUI__FB_PATH, "/"); labels[*n] = "Computer"; (*n)++;\n#endif',
            '    if (cui_host_is_windows()) {\n        snprintf(out[*n], CUI__FB_PATH, "C:\\\\"); labels[*n] = "C:\\\\"; (*n)++;\n    } else {\n        snprintf(out[*n], CUI__FB_PATH, "/"); labels[*n] = "Computer"; (*n)++;\n    }',
            "Select default filesystem root at runtime")
    replace("src/cui_emoji.c", '#include "cui_internal.h"',
            '#include "cui_internal.h"\nint cui_host_is_windows(void);',
            "Declare runtime platform query for emoji font discovery")
    replace("src/cui_emoji.c", '#if defined(_WIN32)\n    char p[300];',
            '    if (cui_host_is_windows()) {\n    char p[300];',
            "Preserve Windows emoji font discovery in the unified translation")
    replace("src/cui_emoji.c", '#elif defined(__APPLE__)\n    return cui_emoji_font_load_file(ui, "/System/Library/Fonts/Apple Color Emoji.ttc");\n#elif defined(__wasm__) || defined(__EMSCRIPTEN__) || defined(CUI_NO_FILESYSTEM)\n    return false;\n#else\n    static const char *const paths[] = {',
            '    }\n    static const char *const paths[] = {',
            "Keep Linux emoji search after the runtime Windows branch")
    replace("src/cui_emoji.c", '    return false;\n#endif\n}\n\n/* ------------------------------------------------------------------------ */\n/* Code points to a glyph',
            '    return false;\n}\n\n/* ------------------------------------------------------------------------ */\n/* Code points to a glyph',
            "Close runtime emoji dispatch")
    replace("src/cui_emoji.c",
            '    snprintf(p, sizeof p, "%s\\\\Fonts\\\\seguiemj.ttf", win ? win : "C:\\\\Windows");',
            '    int n = snprintf(p, sizeof p, "%s\\\\Fonts\\\\seguiemj.ttf", win && *win ? win : "C:\\\\Windows");\n'
            '    if (n < 0 || (size_t)n >= sizeof p) return false;',
            "Reject a truncated Windows emoji font path instead of opening a different file")
    changes.extend(adapt_widgets(destination))
    changes.extend(adapt_text_renderer(destination))
    return changes
