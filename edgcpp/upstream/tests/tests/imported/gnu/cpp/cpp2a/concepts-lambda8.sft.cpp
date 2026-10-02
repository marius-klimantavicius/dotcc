//type: fp
//options: --c++20
// PR c++/92652
// { dg-do compile { target concepts } }

template < typename T >
    requires ([]{return true ;}())
void h() { }

int main()
{
    h<int>();
}
