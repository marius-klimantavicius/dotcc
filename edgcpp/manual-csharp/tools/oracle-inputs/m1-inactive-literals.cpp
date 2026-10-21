#define UNUSED L"unused"
#define UNUSED_RAW R"tag(raw \
text)tag"
#if 0
L"valid but inactive"
u8"also inactive"
R"tag(raw inactive)tag"
#endif
constexpr int value = 1;
static_assert(value == 1);
