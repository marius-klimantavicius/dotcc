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
    return changes
