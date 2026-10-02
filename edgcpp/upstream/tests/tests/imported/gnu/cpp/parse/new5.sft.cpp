//type: fn
//options: 
// PR c++/47450

struct A { };
A* ap = new(struct: A { });	// { dg-error "types may not be defined" }
