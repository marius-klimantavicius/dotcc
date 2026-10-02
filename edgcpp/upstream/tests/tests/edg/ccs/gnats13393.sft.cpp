//type:cp
//options:--c++11:--microsoft_version 1700
//options_all:-tused

template <typename ... T> class foo
{
   template <T ... U> class bar {};
};

foo<> a;
foo<int> b;
