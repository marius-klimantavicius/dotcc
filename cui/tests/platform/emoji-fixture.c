/* Discovery executes the exact staged function. File loading is a recorded
 * service here; this suite does not qualify the emoji font decoder/rasterizer. */
typedef struct { bool tried_system; } cui__emoji;
static cui__emoji emoji;
static cui__emoji *cui__em(cui_ctx *ui) { return ui ? &emoji : NULL; }
static char emoji_paths[10][1024];
static int emoji_count, emoji_success;
bool cui_emoji_font_load_file(cui_ctx *ui, const char *path) {
    (void)ui;
    if (emoji_count >= 10) return false;
    snprintf(emoji_paths[emoji_count], sizeof emoji_paths[0], "%s", path);
    return emoji_count++ == emoji_success;
}
#include "platform-emoji-selected.c"
int platform_emoji(int windows, int success) {
    test_windows = windows; emoji_count = 0; emoji_success = success; emoji.tried_system = false;
    return cui_emoji_font_load_system((cui_ctx *)1);
}
int platform_emoji_count(void) { return emoji_count; }
int platform_emoji_tried(void) { return emoji.tried_system; }
const char *platform_emoji_path(int i) { return i >= 0 && i < emoji_count ? emoji_paths[i] : NULL; }
int platform_emoji_null(void) { return cui_emoji_font_load_system(NULL); }
