//remark:Move and copy operations
//options_all:--c++11
//options:--gnu_version=40400;fp:--gnu_version=40500;fp:--gnu_version=40600;fp:--gnu_version=40700;fp

struct shared_ptr
{
    shared_ptr();
    shared_ptr(shared_ptr&&);
};

struct iter
{
  shared_ptr is;
};

void foo(iter);

void f()
{
  iter i;
  foo(static_cast<iter&&>(i));
}
