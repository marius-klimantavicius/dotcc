// Same C-linkage entity, distinct namespace declarations in the lowered IL.
struct LinkagePair { int first; unsigned second; };
extern "C" {
unsigned export_value(void **, unsigned char *);
LinkagePair export_pair(LinkagePair);
int linkage_sum(int, ...);
int puts(const char *);
}
using Export = unsigned (*)(void **, unsigned char *);
static Export saved_export = &::export_value;
#include "routine_linkage_provider.cpp"
using namespace provider;
namespace unrelated {
int export_value(int value) { return value + 3; }
}
unsigned decoder() {
    unsigned char bytes[] = {41};
    return export_value(nullptr, bytes);
}
int main() {
    Export volatile declaration = &::export_value;
    Export volatile definition = &provider::export_value;
    if (declaration != definition || saved_export != definition) return 1;
    if (decoder() != 42 || unrelated::export_value(5) != 8) return 2;
    unsigned char bytes[] = {9};
    if (declaration(nullptr, bytes) != 10 || definition(nullptr, bytes) != 10) return 3;
    LinkagePair pair = ::export_pair({7, 12});
    if (pair.first != 8 || pair.second != 14) return 4;
    int (*volatile sum)(int, ...) = &::linkage_sum;
    if (sum != &provider::linkage_sum) return 5;
    if (::linkage_sum(3, 1, 4, 7) != 12 || sum(2, 8, 9) != 17) return 6;
    puts("routine linkage: calls, addresses, aggregates and varargs passed");
    return 0;
}
