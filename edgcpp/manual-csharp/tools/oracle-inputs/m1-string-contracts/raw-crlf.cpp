// Physical CRLF raw data retains source spelling but normalizes logical newline.
static_assert(sizeof(R"(a
b)")==4);
static_assert(R"(a
b)"[1]==10);
static_assert(R"(a
b)"[2]==98);
