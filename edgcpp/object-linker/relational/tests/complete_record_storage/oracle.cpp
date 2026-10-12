#include "program.h"
#include <cstddef>
#include <cstdio>
#include <cstring>

template <size_t Size> bool image_copy() {
    unsigned char first[Size], second[Size];
    for (size_t i = 0; i < Size; ++i) first[i] = (unsigned char)(i * 17 + 3);
    std::memcpy(second, first, Size);
    return std::memcmp(first, second, Size) == 0;
}
int main() {
    std::printf("Layout\t%lu\t%lu\t%zu\t%lu\n", layout_size(), layout_alignment(), offsetof(LayoutEmbedded, value), layout_embedded_size());
    std::printf("Fieldless\t%lu\t%lu\t%zu\t%lu\n", fieldless_size(), fieldless_alignment(), offsetof(FieldlessEmbedded, value), fieldless_embedded_size());
    std::printf("Packed\t%lu\t%lu\t%zu\t%lu\n", packed_size(), packed_alignment(), offsetof(PackedEmbedded, value), packed_embedded_size());
    if (!image_copy<sizeof(Layout)>() || !image_copy<sizeof(Fieldless)>() || !image_copy<sizeof(Packed)>()) return 1;
    std::printf("byte-image-copy\tpass\n");
}
