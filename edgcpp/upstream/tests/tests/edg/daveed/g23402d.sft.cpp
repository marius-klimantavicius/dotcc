//remark:Core issue 2267: Initialization of temporary
//options:--c++20;fn

              struct X {};
              struct Y { explicit operator X(); } y;
              X const &rcx(y);  // Error: Explicit operator is not an option
                                // for copy initialization of temporary.
