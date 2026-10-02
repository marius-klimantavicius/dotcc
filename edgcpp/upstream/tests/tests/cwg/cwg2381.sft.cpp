//type: rp
//options_all: -A --c++20 -tused -e 200 --no_wrap
// This is Pern test PP0012R108 which lead to CWG2381
//
extern "C" int printf(const char *, ...);
struct S {
    void f(void) noexcept {}
    void g(void) {}
} s;

void (S::*fp) (void) noexcept = &S::f;
void (S::*gp) (void) = &S::g;

int main(void)
{
if (noexcept((s.*(true ? &S::f : &S::g))()))
  {
    printf("Unexpected Result 1\n");
    return(1);
   }

if (noexcept((s.*(false ? &S::f : &S::g))())) 
   {
    printf("Unexpected result 2");
    return(2);
   }
}

//cwg: 2381
//title: Composite pointer type of pointers to plain and noexcept member functions
//meeting: Kona 02/19
//edg_status: EDGcpfe/21033
