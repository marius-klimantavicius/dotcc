// Included after the public declarations by routine_linkage.cpp. The verifier
// marks this physical file as a prunable provider to check linker dependencies.
namespace provider {
extern "C" unsigned export_value(void **input, unsigned char *bytes) {
    return bytes[0] + (input == nullptr ? 1U : 2U);
}
extern "C" LinkagePair export_pair(LinkagePair pair) {
    return {pair.first + 1, pair.second + 2};
}
extern "C" int linkage_sum(int count, ...) {
    __builtin_va_list arguments;
    __builtin_va_start(arguments, count);
    int sum = 0;
    for (int index = 0; index != count; ++index) sum += __builtin_va_arg(arguments, int);
    __builtin_va_end(arguments);
    return sum;
}
extern "C" int unused_provider_body() { return 19; }
}
