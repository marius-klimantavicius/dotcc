/* Runtime identity probe for the selected scalar libghostty-vt product.
 * Header macros are checked separately from values returned by the library. */
#include <ghostty/vt.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#define CHECK(c) do { if (!(c)) { \
    fprintf(stderr, "FAIL version-build-info:%d: %s\n", __LINE__, #c); exit(1); \
} } while (0)
#define OK(c) CHECK((c) == GHOSTTY_SUCCESS)

int main(void) {
    CHECK(GHOSTTY_VT_VERSION_MAJOR == 1 && GHOSTTY_VT_VERSION_MINOR == 0 && GHOSTTY_VT_VERSION_PATCH == 0);
    CHECK(strcmp(GHOSTTY_VT_VERSION, "1.0.0") == 0);
    size_t major = SIZE_MAX, minor = SIZE_MAX, patch = SIZE_MAX;
    GhosttyString version = {0}, pre = {0}, build = {0};
    OK(ghostty_build_info(GHOSTTY_BUILD_INFO_VERSION_MAJOR, &major));
    OK(ghostty_build_info(GHOSTTY_BUILD_INFO_VERSION_MINOR, &minor));
    OK(ghostty_build_info(GHOSTTY_BUILD_INFO_VERSION_PATCH, &patch));
    OK(ghostty_build_info(GHOSTTY_BUILD_INFO_VERSION_STRING, &version));
    OK(ghostty_build_info(GHOSTTY_BUILD_INFO_VERSION_PRE, &pre));
    OK(ghostty_build_info(GHOSTTY_BUILD_INFO_VERSION_BUILD, &build));
    const char *c_version = ghostty_version();
    CHECK(major == 1 && minor == 0 && patch == 0);
    CHECK(version.ptr != NULL && version.len == 5 && memcmp(version.ptr, "1.0.0", version.len) == 0);
    CHECK(c_version != NULL && strcmp(c_version, "1.0.0") == 0);
    CHECK(pre.len == 0 && build.len == 0);
    printf("PASS library-version numeric=%zu.%zu.%zu string=%.*s c-string=%s pre=%zu build=%zu\n",
        major, minor, patch, (int)version.len, version.ptr, c_version, pre.len, build.len);
    GhosttyOptimizeMode optimize = GHOSTTY_OPTIMIZE_MODE_MAX_VALUE;
    OK(ghostty_build_info(GHOSTTY_BUILD_INFO_OPTIMIZE, &optimize));
    CHECK(optimize == GHOSTTY_OPTIMIZE_RELEASE_FAST);
    printf("PASS optimize mode=%d release-fast=1\n", (int)optimize);
    const struct {
        GhosttyBuildInfo key;
        const char *name;
        bool expected;
    } features[] = {
        { GHOSTTY_BUILD_INFO_SIMD, "simd", false },
        { GHOSTTY_BUILD_INFO_KITTY_GRAPHICS, "kitty_graphics", false },
        { GHOSTTY_BUILD_INFO_TMUX_CONTROL_MODE, "tmux_control_mode", false },
        { GHOSTTY_BUILD_INFO_FEATURE_SNAPSHOT, "snapshot", false },
        { GHOSTTY_BUILD_INFO_FEATURE_FORMATTER, "formatter", true },
        { GHOSTTY_BUILD_INFO_FEATURE_SELECTION, "selection", false },
        { GHOSTTY_BUILD_INFO_FEATURE_SEARCH, "search", false },
        { GHOSTTY_BUILD_INFO_FEATURE_RENDER_STATE, "render_state", true },
        { GHOSTTY_BUILD_INFO_FEATURE_INPUT_ENCODE, "input_encode", false },
        { GHOSTTY_BUILD_INFO_FEATURE_COLOR, "color", false },
        { GHOSTTY_BUILD_INFO_FEATURE_GRID_INTROSPECTION, "grid_introspection", true },
        { GHOSTTY_BUILD_INFO_FEATURE_GLYPH_PROTOCOL, "glyph_protocol", false },
    };
    for (size_t i = 0; i < sizeof(features) / sizeof(features[0]); i++) {
        bool value = !features[i].expected;
        OK(ghostty_build_info(features[i].key, &value));
        CHECK(value == features[i].expected);
        printf("PASS feature %s=%d\n", features[i].name, (int)value);
    }
    CHECK(ghostty_build_info(GHOSTTY_BUILD_INFO_INVALID, &major) == GHOSTTY_INVALID_VALUE);
    CHECK(ghostty_build_info(GHOSTTY_BUILD_INFO_VERSION_MAJOR, NULL) == GHOSTTY_INVALID_VALUE);
    CHECK(ghostty_build_info((GhosttyBuildInfo)9999, &major) == GHOSTTY_UNSUPPORTED);
    puts("RESULT version-feature-cases=14 passed=14 failed=0");
    return 0;
}
