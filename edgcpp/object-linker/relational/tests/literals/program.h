constexpr const char *shared_literal() { return "shared\0tail"; }
constexpr const char *shared_pointer = shared_literal();
extern char greeting[8];
const char *provider_shared();
const char *embedded();
const char *offset_literal();
const wchar_t *wide_literal();
const char16_t *utf16_literal();
const char32_t *utf32_literal();
const char8_t *utf8_literal();
int local_literals();
