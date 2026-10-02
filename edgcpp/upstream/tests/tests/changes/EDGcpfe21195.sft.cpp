//type:fp
//remark:[5.1] Spurious error capturing "this" in a lambda during template argument deduction
// 5/6/19   [EDGcpfe/21195]
//
// Spurious error capturing "this" in a lambda during template argument deduction
//
// The fix for EDGcpfe/20863 missed the case where the lambda expression needed
// to be used to deduce a template parameter.  In this case, the front end was
// still issuing a spurious error when attempting to use "this".
//
// This is now fixed.
struct function
{
  template<typename _Functor> function(_Functor);
};

struct Lambda
{
  function callback_ =
    [this](const auto v) { return this; }; // Spurious error on using "this"
};
