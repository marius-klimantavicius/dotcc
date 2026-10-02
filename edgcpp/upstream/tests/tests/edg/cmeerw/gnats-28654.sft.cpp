//type:fp
//options:--gn 160000:--clang_version 200100:--ms_c++20 --microsoft_version 1951
//options_all:--c++20

struct Incomplete;

struct UserProvidedCtor
{
  UserProvidedCtor();
  UserProvidedCtor(const UserProvidedCtor &);
};

struct UserProvidedDtor
{
  ~UserProvidedDtor();
};

struct UserProvidedCtorUserProvidedDtor
{
  UserProvidedCtorUserProvidedDtor();
  UserProvidedCtorUserProvidedDtor(const UserProvidedCtorUserProvidedDtor &);
  ~UserProvidedCtorUserProvidedDtor();
};

struct Empty
{ };

struct AggregateNoDtor
{
  int i;
  UserProvidedCtorUserProvidedDtor c;
};

struct AggregateDefaultedDtor
{
  ~AggregateDefaultedDtor() = default;

  int i;
  UserProvidedCtorUserProvidedDtor c;
};

struct AggregateOutsideDefaultedDtor
{
  ~AggregateOutsideDefaultedDtor();

  int i;
  UserProvidedCtorUserProvidedDtor c;
};

inline AggregateOutsideDefaultedDtor::~AggregateOutsideDefaultedDtor() = default;

struct AggregateDeletedDtor
{
  ~AggregateDeletedDtor() = delete;

  int i;
  UserProvidedCtorUserProvidedDtor c;
};

struct AggregateWithDtor
{
  ~AggregateWithDtor();

  int i;
  UserProvidedCtorUserProvidedDtor c;
};

struct TrivialCtor
{
  TrivialCtor() = default;

  int i;
};

struct TrivialCtorTrivialDtor
{
  TrivialCtorTrivialDtor() = default;
  ~TrivialCtorTrivialDtor() = default;

  int i;
};

template<bool B>
struct EligibleDtor
{
  ~EligibleDtor() requires B = default;
  ~EligibleDtor() requires (!B);
};

template<bool B>
struct EligibleCtor
{
  EligibleCtor() requires B = default;
  EligibleCtor() requires (!B);

  EligibleCtor(const EligibleCtor &);
};

struct DeletedCopyCtor
{
  DeletedCopyCtor();
  DeletedCopyCtor(const DeletedCopyCtor &) = delete;
  DeletedCopyCtor(DeletedCopyCtor &&) = delete;
};

struct DeletedDtor
{
  DeletedDtor();
  ~DeletedDtor() = delete;
};

enum E
{ };

union U
{ };

template<typename T>
using A = T;


static_assert(!__builtin_is_implicit_lifetime(void));
static_assert(!__builtin_is_implicit_lifetime(A<void>));
static_assert( __builtin_is_implicit_lifetime(decltype(nullptr)));
static_assert( __builtin_is_implicit_lifetime(E));
static_assert( __builtin_is_implicit_lifetime(A<E>));
static_assert( __builtin_is_implicit_lifetime(U));
static_assert( __builtin_is_implicit_lifetime(A<U>));

static_assert(!__builtin_is_implicit_lifetime(UserProvidedCtor));
static_assert(!__builtin_is_implicit_lifetime(A<UserProvidedCtor>));
static_assert(!__builtin_is_implicit_lifetime(UserProvidedDtor));
static_assert(!__builtin_is_implicit_lifetime(const UserProvidedCtor));
static_assert(!__builtin_is_implicit_lifetime(const UserProvidedDtor));
static_assert(!__builtin_is_implicit_lifetime(UserProvidedCtorUserProvidedDtor));
static_assert(!__builtin_is_implicit_lifetime(const UserProvidedCtorUserProvidedDtor));

static_assert( __builtin_is_implicit_lifetime(AggregateNoDtor));
static_assert( __builtin_is_implicit_lifetime(AggregateDefaultedDtor));
static_assert( __builtin_is_implicit_lifetime(AggregateDeletedDtor));
static_assert(!__builtin_is_implicit_lifetime(AggregateOutsideDefaultedDtor));
static_assert(!__builtin_is_implicit_lifetime(AggregateWithDtor));

static_assert( __builtin_is_implicit_lifetime(TrivialCtor));
static_assert( __builtin_is_implicit_lifetime(TrivialCtorTrivialDtor));

static_assert( __builtin_is_implicit_lifetime(int *));
static_assert( __builtin_is_implicit_lifetime(UserProvidedCtorUserProvidedDtor *));
static_assert( __builtin_is_implicit_lifetime(UserProvidedCtorUserProvidedDtor * const));
static_assert( __builtin_is_implicit_lifetime(const UserProvidedCtorUserProvidedDtor *));
static_assert( __builtin_is_implicit_lifetime(Incomplete *));

static_assert( __builtin_is_implicit_lifetime(UserProvidedCtorUserProvidedDtor[1]));
static_assert( __builtin_is_implicit_lifetime(UserProvidedCtorUserProvidedDtor[]));
static_assert( __builtin_is_implicit_lifetime(int[1]));
static_assert( __builtin_is_implicit_lifetime(int[]));

static_assert(!__builtin_is_implicit_lifetime(int &));
static_assert(!__builtin_is_implicit_lifetime(const int &));
static_assert(!__builtin_is_implicit_lifetime(UserProvidedCtorUserProvidedDtor &));
static_assert(!__builtin_is_implicit_lifetime(const UserProvidedCtorUserProvidedDtor &));

static_assert( __builtin_is_implicit_lifetime(EligibleDtor<true>));
static_assert(!__builtin_is_implicit_lifetime(EligibleDtor<false>));

static_assert( __builtin_is_implicit_lifetime(EligibleCtor<true>));
static_assert(!__builtin_is_implicit_lifetime(EligibleCtor<false>));

#if defined(__clang__)
// GCC and MSVC do not agree here
static_assert( __builtin_is_implicit_lifetime(DeletedCopyCtor));
#endif

static_assert(!__builtin_is_implicit_lifetime(DeletedDtor));
