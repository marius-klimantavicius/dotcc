//remark:Direct ref-binding and UDCs
//options:--c++17;fn:--c++17 --clang;fp:--c++17 --g++;fn:--c++17 --microsoft_v=1944;fp

              struct X {};
              struct Y { explicit operator X(); } y;
              X &&r(y);  // Okay.
