//remark:Direct ref-binding and UDCs
//options:--c++17;fn:--c++17 --clang;fp:--c++17 --g++;fn:--c++17 --microsoft_v=1944;fn

              struct X {};
              struct Y { explicit operator X(); } y;
              X const &r(y);  // Okay.
