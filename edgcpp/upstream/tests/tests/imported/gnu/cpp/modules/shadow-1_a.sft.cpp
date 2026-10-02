//type: fp
//options:  --c++20 --modules
// { dg-additional-options -fmodules-ts }
export module shadow;
// { dg-module-cmi shadow }

export struct stat
{
};

export void stat ();
