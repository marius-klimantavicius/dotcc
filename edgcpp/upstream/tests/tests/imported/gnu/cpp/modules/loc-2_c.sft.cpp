//type: fp
//options:  --c++20 --modules
// { dg-additional-options "-fmodules-ts" }

export module kevin;
// { dg-module-cmi kevin }

export import stuart;
export import bob; // Bob should be reseated in the export map
export import stuart;
