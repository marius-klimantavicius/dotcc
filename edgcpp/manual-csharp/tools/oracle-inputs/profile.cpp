// Authored test witness: C++20 and LP64, no system headers.
static_assert(__cplusplus == 202002L);
static_assert(sizeof(char) == 1);
static_assert(static_cast<unsigned char>(-1) == 255);
static_assert(sizeof(bool) == 1);
static_assert(sizeof(short) == 2);
static_assert(sizeof(int) == 4);
static_assert(sizeof(long) == 8);
static_assert(sizeof(long long) == 8);
static_assert(sizeof(void*) == 8);
static_assert(static_cast<char>(-1) < 0);
