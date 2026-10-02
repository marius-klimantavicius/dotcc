//remark:Direct ref-binding and UDCs
//options:--c++17;fp

              struct X {};
              struct Y { explicit operator X&&(); } y;
              X &&r(y);  // Okay.
