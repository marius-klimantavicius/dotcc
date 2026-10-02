//type:fp
//options_all:--c++20 -tused -A
    template <typename RET, typename T1, typename T2>
    const RET& min (const T1& a, const T2& b)
    {
	return (a < b ? a : b);
    }
      template const int& min(const int&,const int&);       // #2

//cwg: 271
//title: Explicit instantiation and template argument deduction
//meeting: Virtual 11/20*
//edg_status: Passes
