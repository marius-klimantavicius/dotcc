// Native-only explicit target/type/value contract for the next handwritten slice.
template<class A, class B> constexpr bool same_type = false;
template<class A> constexpr bool same_type<A, A> = true;
static_assert(sizeof(wchar_t) == 4);
static_assert(static_cast<wchar_t>(-1) < 0);
static_assert(sizeof(char8_t) == 1);
static_assert(sizeof(char16_t) == 2);
static_assert(sizeof(char32_t) == 4);
static_assert(same_type<decltype('a'), char>);
static_assert(same_type<decltype('ab'), int>);
static_assert(same_type<decltype(L'a'), wchar_t>);
static_assert(same_type<decltype(u8'a'), char8_t>);
static_assert(same_type<decltype(u'a'), char16_t>);
static_assert(same_type<decltype(U'a'), char32_t>);
static_assert(same_type<decltype(+L'a'), int>);
static_assert(same_type<decltype(+u8'a'), int>);
static_assert(same_type<decltype(+u'a'), int>);
static_assert(same_type<decltype(+U'a'), unsigned int>);
int promoted(int);
long promoted(unsigned int);
static_assert(same_type<decltype(promoted(U'a')), long>);
static_assert(same_type<decltype(promoted(L'a')), int>);
static_assert('\xFF' == -1);
static_assert('\377' == -1);
static_assert('ab' == 0x6162);
static_assert('\377a' == -159);
static_assert('a\377' == 25087);
static_assert('\377\377' == -1);
#if '\377' != -1 || '\377a' != -159 || 'a\377' != 25087 || '\377\377' != -1
#error preprocessing character semantics differ from this target contract
#endif
