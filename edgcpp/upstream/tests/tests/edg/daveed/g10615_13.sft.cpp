//remark:Move and copy operations
//options:--c++11 -A;fp:--gnu_version=40300 --c++11;fp

struct shared_ptr
{
  shared_ptr();
  shared_ptr(shared_ptr&&);
};
 
struct filesystem_error
{
  shared_ptr sp;
};
 
void foo()
{
    throw filesystem_error();
}
