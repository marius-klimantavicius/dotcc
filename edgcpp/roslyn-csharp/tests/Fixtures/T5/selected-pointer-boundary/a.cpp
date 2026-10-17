#include "api.hpp"
extern "C" int fixture()
{
    BoundaryCell first;
    BoundaryCell second;
    first.value = 201;
    first.canary = 0xfedcba98u;
    second.value = 255;
    second.canary = 0x87654321u;
    const BoundaryCell *borrowed = boundary_echo(&first);
    int result = borrowed == &first ? 1 : 0;
    result += 10 * (int)boundary_read(borrowed);
    first.value = 128;
    result += 100 * (boundary_read(borrowed) == 128u);
    borrowed = boundary_echo(&second);
    result += 1000 * (borrowed == &second);
    result += 10000 * (boundary_read(borrowed) == 255u);
    result += 100000 * (first.canary == 0xfedcba98u && second.canary == 0x87654321u);
    return result;
}
