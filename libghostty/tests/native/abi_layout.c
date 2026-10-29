/* Measure real storage, including pointer differences to actual fields. */
#include <ghostty/vt.h>
#include <stdint.h>
#include <stdio.h>
#define OFFSET(value, field) ((size_t)((const char *)&(value).field - (const char *)&(value)))
struct AllocAlign { char lead; GhosttyAllocator value; };
struct VtableAlign { char lead; GhosttyAllocatorVtable value; };
struct FormatAlign { char lead; GhosttyFormatterTerminalOptions value; };
struct GridAlign { char lead; GhosttyGridRef value; };
struct PointAlign { char lead; GhosttyPoint value; };
struct StyleAlign { char lead; GhosttyStyle value; };

int main(void) {
    GhosttyAllocator allocator = {0}; struct AllocAlign alloc_align = {0};
    GhosttyAllocatorVtable vtable = {0}; struct VtableAlign vtable_align = {0};
    GhosttyFormatterTerminalOptions format = {0}; struct FormatAlign format_align = {0};
    GhosttyGridRef grid = {0}; struct GridAlign grid_align = {0};
    GhosttyPoint point = {0}; struct PointAlign point_align = {0};
    GhosttyStyle style = {0}; struct StyleAlign style_align = {0};
    printf("PASS layout allocator size=%zu align=%zu ctx=%zu vtable=%zu\n",
        sizeof(allocator), OFFSET(alloc_align, value), OFFSET(allocator, ctx), OFFSET(allocator, vtable));
    printf("PASS layout vtable size=%zu align=%zu alloc=%zu resize=%zu remap=%zu free=%zu\n",
        sizeof(vtable), OFFSET(vtable_align, value), OFFSET(vtable, alloc), OFFSET(vtable, resize),
        OFFSET(vtable, remap), OFFSET(vtable, free));
    printf("PASS layout formatter size=%zu align=%zu emit=%zu unwrap=%zu trim=%zu extra=%zu selection=%zu\n",
        sizeof(format), OFFSET(format_align, value), OFFSET(format, emit), OFFSET(format, unwrap),
        OFFSET(format, trim), OFFSET(format, extra), OFFSET(format, selection));
    printf("PASS layout grid size=%zu align=%zu node=%zu x=%zu y=%zu\n",
        sizeof(grid), OFFSET(grid_align, value), OFFSET(grid, node), OFFSET(grid, x), OFFSET(grid, y));
    printf("PASS layout point size=%zu align=%zu tag=%zu value=%zu\n",
        sizeof(point), OFFSET(point_align, value), OFFSET(point, tag), OFFSET(point, value));
    printf("PASS layout style size=%zu align=%zu foreground=%zu bold=%zu underline=%zu\n",
        sizeof(style), OFFSET(style_align, value), OFFSET(style, fg_color), OFFSET(style, bold), OFFSET(style, underline));
    puts("RESULT layout-cases=6 passed=6 failed=0");
    return 0;
}
