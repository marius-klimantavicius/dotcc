//type: s
//options: --c++11
# 0 "./cpp0x/lambda/lambda-ice26.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./cpp0x/lambda/lambda-ice26.C"




# 1 "./cpp0x/lambda/lambda-ice14.C" 1



template<typename Signature>
struct function;

template<typename R, typename... Args>
struct function<R (Args...)>
{
  template<typename F>
  function(const F&) { }
};

template<typename T>
class A
{
  T someVar;
};

template<typename T>
class B
{
  int x;

  function<A<double>(A<int>&)> someLambda = [&](A<int>& aInt){
    int xVar = x;
    A<double> aRet;
    return aRet;
  };
};

B<int> a;
# 6 "./cpp0x/lambda/lambda-ice26.C" 2
