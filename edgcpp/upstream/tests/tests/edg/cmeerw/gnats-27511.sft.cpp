//type:fp
//options:--clang_version 150000:--clang_version 160000:--clang_version 170000:--clang_version 180100:--clang_version 190100
//options_all:--c++20

static_assert( __is_trivially_relocatable(int));
static_assert( __is_trivially_relocatable(int[]));
static_assert( __is_trivially_relocatable(int[2]));
static_assert( __is_trivially_relocatable(const int));
static_assert( __is_trivially_relocatable(int *));
static_assert( __is_trivially_relocatable(int (*)()));
static_assert(!__is_trivially_relocatable(int ()));
static_assert(!__is_trivially_relocatable(int &));
static_assert(!__is_trivially_relocatable(int &&));
static_assert(!__is_trivially_relocatable(void));


enum E
{
  E1
};

static_assert( __is_trivially_relocatable(E));

enum class EC;

static_assert( __is_trivially_relocatable(EC));


struct Trivial
{
  int i;
};

static_assert( __is_trivially_relocatable(volatile Trivial));
static_assert( __is_trivially_relocatable(Trivial));
static_assert( __is_trivially_relocatable(int Trivial::*));
static_assert( __is_trivially_relocatable(int (Trivial::*) ()));
static_assert( __is_trivially_relocatable(Trivial[]));
static_assert( __is_trivially_relocatable(Trivial[2]));


struct DeletedMoveCtor
{
  DeletedMoveCtor(DeletedMoveCtor &&) = delete;
};

static_assert(!__is_trivially_relocatable(DeletedMoveCtor));
static_assert(!__is_trivially_relocatable(DeletedMoveCtor[2]));


struct DeletedMoveCtorNonDeletedCopyCtor
{
  DeletedMoveCtorNonDeletedCopyCtor(DeletedMoveCtorNonDeletedCopyCtor &&) = delete;
  DeletedMoveCtorNonDeletedCopyCtor(const DeletedMoveCtorNonDeletedCopyCtor &) = default;
};

static_assert( __is_trivially_relocatable(DeletedMoveCtorNonDeletedCopyCtor));
static_assert( __is_trivially_relocatable(DeletedMoveCtorNonDeletedCopyCtor[2]));


struct DeletedCopyCtor
{
  DeletedCopyCtor(const DeletedCopyCtor &) = delete;
};

static_assert(!__is_trivially_relocatable(DeletedCopyCtor));
static_assert(!__is_trivially_relocatable(DeletedCopyCtor[2]));


struct DeletedCopyCtorNonDeletedMoveCtor
{
  DeletedCopyCtorNonDeletedMoveCtor(const DeletedCopyCtorNonDeletedMoveCtor &) = delete;
  DeletedCopyCtorNonDeletedMoveCtor(DeletedCopyCtorNonDeletedMoveCtor &&) = default;
};

static_assert( __is_trivially_relocatable(DeletedCopyCtorNonDeletedMoveCtor));
static_assert( __is_trivially_relocatable(DeletedCopyCtorNonDeletedMoveCtor[2]));


struct DeletedDtor
{
  ~DeletedDtor() = delete;
};

static_assert( __is_trivially_relocatable(DeletedDtor));
static_assert( __is_trivially_relocatable(DeletedDtor[2]));


struct NonTrivialDtor
{
  ~NonTrivialDtor();
};

static_assert(!__is_trivially_relocatable(NonTrivialDtor));
static_assert(!__is_trivially_relocatable(NonTrivialDtor[2]));


struct NonTrivialCopy
{
  NonTrivialCopy(const NonTrivialCopy &);
};

static_assert(!__is_trivially_relocatable(NonTrivialCopy));
static_assert(!__is_trivially_relocatable(NonTrivialCopy[2]));


struct NonTrivialMove
{
  NonTrivialMove(NonTrivialMove &&);
};

static_assert(!__is_trivially_relocatable(NonTrivialMove));
static_assert(!__is_trivially_relocatable(NonTrivialMove[2]));


template<typename T>
struct IneligibleDtor
{
  ~IneligibleDtor() requires true = default;
  ~IneligibleDtor() requires false;
};

static_assert(__is_trivially_relocatable(IneligibleDtor<int>));
