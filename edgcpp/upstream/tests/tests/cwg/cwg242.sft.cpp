//type:fn
//options_all:--c++17 -tused -A
//
    struct A {};
    struct I1 : A {};
    struct I2 : A {};
    struct D : I1, I2 {};
    A *foo( D *p ) {
	return (A*)( p ); // ill-formed static_cast interpretation
    }

//cwg: 242
//title: Interpretation of old-style casts
//meeting: Jacksonville 2/16
//edg_status: Passes
