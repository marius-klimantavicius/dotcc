//type:rp
//options_all:--c++20 -tused -A
template<int ...N> int k = 2 * (... + N);
 int j =k<1, 2, 3>;
int main()
{
    if (j == 7)
      return(1);
    return(0);
}

//cwg: 2611
//title: Missing parentheses in expansion of fold-expression could cause syntactic reinterpretation
//meeting: Kona 11/22
//edg_status: Passes
