//type:fn
//options:--c++17 -DNOCONV;fn:--c++17 -DTOO_FEW_ARGS;fn:--c++23 -DTOO_FEW_ARGS

namespace std
{
  template<typename T>
  struct tuple_size
  {
    static constexpr int value = 2;
  };

  template<int I, typename T>
  struct tuple_element
  {
    using type = int;
  };
}

struct X
{
  template<int I>
  int get();
};

struct Y
{ };

template<int I>
int get(Y &);


#ifdef NOCONV
struct NoConv
{ };

int *begin(NoConv);
int *end(NoConv);
#endif

#ifdef TOO_FEW_ARGS
int *begin(const X &, int);
int *end(const X &);
#endif


void use(const X x, const Y y)
{
  for (auto i : x) {}
  // error: this range-based "for" statement requires a suitable "begin" function and none was found
  // error: this range-based "for" statement requires a suitable "end" function and none was found

  const auto [ xi, xj ] = x;
  // error: no instance of function template "X::get" matches the argument list and object (the object has type qualifiers that prevent a match)
  // error: no instance of function template "X::get" matches the argument list and object (the object has type qualifiers that prevent a match)

  const auto [ yi, yj ] = y;
  // error: this structured binding requires a suitable "get" function and none was found
  // error: this structured binding requires a suitable "get" function and none was found
}

void single_function(int);

void overloaded_function(int);
void overloaded_function(long);


void foo()
{
  single_function("");
  // error: argument of type "const char *" is incompatible with parameter of type "int"

  overloaded_function("");
  // error: no instance of overloaded function "overloaded_function" matches the argument list
}

#if __cpp_explicit_this_parameter
struct Z {
  // Note that explicit object member functions will always go through overload
  // resolution, even for the single function case, so "Z::bar" will use the
  // closing_paren_position in select_and_prepare_to_call_overloaded_function
  // even though do_arg_dep_lookup is FALSE.
  void bar(this Z &, int);

  void foo()
  {
    bar();
    // error: too few arguments in function call

    Z::bar();
    // error: too few arguments in function call
  }
};
#endif
