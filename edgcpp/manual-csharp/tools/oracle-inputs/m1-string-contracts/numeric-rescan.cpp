// Hand-authored source-derived string conversion contracts.
static_assert((u"" "\xff")[0]==255);
static_assert(("\xff" u"")[0]==255);
static_assert((U"" "\xff")[0]==255);
static_assert((L"" "\xff")[0]==255);
static_assert((u"" "\x100")[0]==0);
static_assert(("\x100" u"")[0]==0);
static_assert((u"\x100")[0]==256);
static_assert((u"" "\xc3\xa9")[0]==233);
static_assert(sizeof(u"" "\xc3\xa9")==4);
static_assert((u"" "\\u0041")[0]==92);
static_assert((u"" "\\u0041")[1]==117);
static_assert(sizeof(u"" "\\u0041")==14);
