//type:fn
//options:--c++26
//options_all:-A

struct BB
{ };

struct B : BB
{ };

struct D : virtual B
{ };

constexpr bool f(int i)
{
  B *b = 0;
  BB *b = 0;
  D *d = new D;

  b = d;
  b = static_cast<B *>(d);
  bb = d;
  bb = static_cast<BB *>(d);

  delete d;

  switch (i)
  {
   case 1: b = d; break;
   case 2: b = static_cast<B *>(d); break;
   case 3: bb = d; break;
   case 4: bb = static_cast<BB *>(d); break;
  }

  return true;
}

static_assert(f(0));            // OK
static_assert(f(1));            // not constant
static_assert(f(2));            // not constant
static_assert(f(3));            // not constant
static_assert(f(4));            // not constant

//cwg: 3001
//title: Inconsistent restrictions for static_cast on pointers to out-of-lifetime objects
//meeting: Kona 11/25
//edg_status: Passes
