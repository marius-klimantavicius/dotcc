//options:--microsoft_version 1700:--microsoft_version 1800:--microsoft_version 1900
//type:fp

#if _MSC_VER < 1800
#define CHECK_ARRAY(arr) \
static_assert(!__has_assign(arr), "assign array " # arr); \
static_assert(!__has_trivial_assign(arr), "trivial assign array " # arr); \
static_assert(!__has_nothrow_assign(arr), "nothrow assign array " # arr); \
static_assert(!__has_trivial_move_assign(arr), "trivial move assign array " # arr); \
static_assert(!__has_nothrow_move_assign(arr), "nothrow move assign array " # arr); \

#elif _MSC_VER < 1900
#define CHECK_ARRAY(arr) \
static_assert(!__has_assign(arr), "assign array " # arr); \
static_assert(!__has_trivial_assign(arr), "trivial assign array " # arr); \
static_assert(!__has_nothrow_assign(arr), "nothrow assign array " # arr); \
static_assert(__has_trivial_move_assign(arr), "trivial move assign array " #arr); \
static_assert(__has_nothrow_move_assign(arr), "nothrow move assign array " #arr); \

#else
#define CHECK_ARRAY(arr) \
static_assert(!__has_assign(arr), "assign array " # arr); \
static_assert(!__has_trivial_assign(arr), "trivial assign array " # arr); \
static_assert(!__has_nothrow_assign(arr), "nothrow assign array " # arr); \
static_assert(!__has_trivial_move_assign(arr), "trivial move assign array " # arr); \
static_assert(!__has_nothrow_move_assign(arr), "nothrow move assign array " # arr); \

#endif

struct POD {
  int foo;
  char bar;
};
static_assert(__is_pod(POD), "struct POD isn't POD");

class nonPOD {
  public:
    nonPOD(int);
};
static_assert(!__is_pod(nonPOD), "class nonPOD is POD");

typedef int int_array_t [100];
typedef int* int_ptr_array_t [100];
typedef POD pod_array_t [100];
typedef POD* pod_ptr_array_t [100];
typedef nonPOD non_pod_array_t [100];
typedef nonPOD non_pod_ptr_array_t [100];

CHECK_ARRAY(int_array_t)
CHECK_ARRAY(int_ptr_array_t)
CHECK_ARRAY(pod_array_t)
CHECK_ARRAY(pod_ptr_array_t)
CHECK_ARRAY(non_pod_array_t)
CHECK_ARRAY(non_pod_ptr_array_t)

typedef int int_dbl_array_t [100][100];
typedef int* int_ptr_dbl_array_t [100][100];
typedef POD pod_dbl_array_t [100][100];
typedef POD* pod_ptr_dbl_array_t [100][100];
typedef nonPOD non_pod_dbl_array_t [100][100];
typedef nonPOD non_pod_ptr_dbl_array_t [100][100];

CHECK_ARRAY(int_dbl_array_t)
CHECK_ARRAY(int_ptr_dbl_array_t)
CHECK_ARRAY(pod_dbl_array_t)
CHECK_ARRAY(pod_ptr_dbl_array_t)
CHECK_ARRAY(non_pod_dbl_array_t)
CHECK_ARRAY(non_pod_ptr_dbl_array_t)
