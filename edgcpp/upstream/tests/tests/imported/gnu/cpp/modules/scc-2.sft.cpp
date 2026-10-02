//type: fp
//options:  --c++20 --modules
// { dg-additional-options "-fmodules-ts" }
export module frob;
// { dg-module-cmi frob }

export enum X 
{
  One, Two, Three
};
