#include "program.h"
#include <cstdio>
Diamond::Diamond() noexcept {}
Diamond::Diamond(const Diamond& source) noexcept {
    // This genuine supplied constructor's body copies the entire byte image.
    // Its implicit base initialization has no user code or other side effects.
    auto destination = reinterpret_cast<unsigned char*>(this);
    auto input = reinterpret_cast<const unsigned char*>(&source);
    for (size_type index = 0; index < sizeof(Diamond); ++index) destination[index] = input[index];
}
int main() {
    if (public_indirect() || private_base() || ambiguous_base()) return 1;
    Diamond source;
    source.value = 31;
    source.left = 37;
    source.right = 41;
    source.last = 43;
    size_type offset = reinterpret_cast<unsigned char*>(static_cast<Shared*>(&source)) - reinterpret_cast<unsigned char*>(&source);
    if (virtual_diamond(source, offset)) return 2;
    std::printf("Shared-offset\t%zu\n", offset);
    std::puts("class-base-matching\tpass");
    return 0;
}
