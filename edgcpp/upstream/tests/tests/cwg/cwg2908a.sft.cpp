//type:fn
//options_all:--c++20 -A
//The last 3 static_assert(3 ==  should fail
#line 1
static_assert(1 == __LINE__);

static_assert(3 == __LINE__);\
static_assert(3 == __LINE__);\
static_assert(3 == __LINE__);\
static_assert(3 == __LINE__);

//cwg: 2908
//title: Counting physical source lines for __LINE__
//meeting: Wroclaw 11/24
//edg_status: EDGcpfe/27760
