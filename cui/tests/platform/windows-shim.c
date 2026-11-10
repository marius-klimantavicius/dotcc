/* A native Windows x64 ABI test double, runnable on Linux. This verifies
 * marshalling, buffer writes and paired ownership, not Windows OS behavior. */
#include <stdint.h>
#include <stddef.h>
#include <stdlib.h>
#include <string.h>
#include <uchar.h>

typedef struct {
    intptr_t owner, root;
    uint16_t *display_name;
    const uint16_t *title;
    uint32_t flags;
    intptr_t callback, parameter;
    int32_t image;
} browse_info;
typedef struct {
    uint32_t size;
    intptr_t owner, instance;
    const uint16_t *filter;
    uint16_t *custom_filter;
    uint32_t max_custom_filter, filter_index;
    uint16_t *file;
    uint32_t max_file;
    uint16_t *file_title;
    uint32_t max_file_title;
    const uint16_t *initial_directory, *title;
    uint32_t flags;
    uint16_t file_offset, file_extension;
    const uint16_t *default_extension;
    intptr_t custom_data, hook;
    const uint16_t *template_name;
    intptr_t reserved;
    uint32_t reserved2, flags_ex;
} open_file_name;
_Static_assert(sizeof(browse_info) == 64, "BROWSEINFO x64 ABI");
_Static_assert(sizeof(open_file_name) == 152, "OPENFILENAME x64 ABI");
_Static_assert(offsetof(open_file_name, file) == 48, "OPENFILENAME file ABI");
_Static_assert(offsetof(browse_info, display_name) == 16, "BROWSEINFO display buffer ABI");

static int mode, init_count, uninit_count, allocated, freed, browse_count, file_count, errors;
void platform_win_reset(int value) { mode = value; init_count = uninit_count = allocated = freed = browse_count = file_count = errors = 0; }
int platform_win_stat(int i) {
    int values[] = {init_count, uninit_count, allocated, freed, browse_count, file_count, errors};
    return i >= 0 && i < 7 ? values[i] : -1;
}
static int equal(const uint16_t *a, const char16_t *b) {
    if (!a) return 0;
    while (*a && *a == *b) { a++; b++; }
    return *a == *b;
}
static void copy(uint16_t *to, const char16_t *from) {
    do { *to++ = *from; } while (*from++);
}
intptr_t GetActiveWindow(void) { return 123; }
int32_t CoInitializeEx(intptr_t reserved, uint32_t flags) {
    init_count++;
    if (reserved || flags != 6) errors++;
    return mode == 1 ? (int32_t)0x80010106 : mode == 6 ? 1 : 0;
}
void CoUninitialize(void) { uninit_count++; }
void CoTaskMemFree(void *item) { if (item) { freed++; free(item); } }
void *SHBrowseForFolderW(browse_info *info) {
    browse_count++;
    if (info->owner != 123 || info->root || info->flags != 0x41 || info->callback || info->parameter || info->image || !info->display_name || !equal(info->title, u"Test ž😀")) {
        errors++; return NULL;
    }
    // Exercise the full documented display buffer, including its last NUL.
    for (int i = 0; i < 259; i++) info->display_name[i] = 'x';
    info->display_name[259] = 0;
    if (mode == 2) return NULL;
    allocated++;
    return malloc(16);
}
int32_t SHGetPathFromIDListW(void *item, uint16_t *path) {
    if (!item || !path) { errors++; return 0; }
    if (mode == 3) return 0;
    copy(path, u"Z:\\ž😀\\folder");
    return 1;
}
static int file_dialog(open_file_name *info, int save) {
    file_count++;
    static const char16_t filter[] = u"Images\0*.png;*.jpg\0All files\0*.*\0\0";
    if (info->size != sizeof(*info) || info->owner != 123 || info->max_file != 1040 || !info->file ||
        !equal(info->title, u"Test ž😀") || info->flags != (0x80000u | 8u | (save ? 2u : 0x1000u)) ||
        !info->filter || memcmp(info->filter, filter, sizeof filter - 2) || !equal(info->file, u"ž😀.png") ||
        info->instance || info->custom_filter || info->max_custom_filter || info->filter_index || info->file_title ||
        info->max_file_title || info->initial_directory || info->file_offset || info->file_extension || info->default_extension ||
        info->custom_data || info->hook || info->template_name || info->reserved || info->reserved2 || info->flags_ex) {
        errors++; return 0;
    }
    if (mode == 4) return 0;
    copy(info->file, u"Z:\\ž😀\\file.png");
    return 1;
}
int32_t GetOpenFileNameW(open_file_name *info) { return file_dialog(info, 0); }
int32_t GetSaveFileNameW(open_file_name *info) { return file_dialog(info, 1); }
