// Ordinary strict-width diagnosis is deferred until final encoding selection.
static_assert((u"" "é")[0]==233);
static_assert(("é" u"")[0]==233);
static_assert(sizeof(u8"" "é")==3);
static_assert(sizeof("é" u8"")==3);
static_assert(sizeof(R"(é)")==3);
