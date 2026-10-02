//type: fn
//options: 
// P1949R7
// { dg-do compile }
// { dg-options "" }

bool 👷 = true;
bool 👷‍♀ = false;	// { dg-error "is not valid in an identifier" }
int ⏰ = 0;	// { dg-error "is not valid in an identifier" }
int 🕐 = 0;
int ☠ = 0;	// { dg-error "is not valid in an identifier" }
int 💀 = 0;
int ✋ = 0;	// { dg-error "is not valid in an identifier" }
int 👊 = 0;
int ✈ = 0;	// { dg-error "is not valid in an identifier" }
int 🚀 = 0;
int ☹ = 0;	// { dg-error "is not valid in an identifier" }
int 😀 = 0;
struct E {};
class 💩 : public E {};
