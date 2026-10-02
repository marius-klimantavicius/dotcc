//type:fp
//options_all:--c++26
//remark:C++26: Structured binding packs
// 3/6/26   [EDGcpfe/27775,EDGcpfe/28548]
//
// C++26: Structured binding packs
//
// The front end now supports structured binding packs, which have been adopted
// for the upcoming C++26 Standard via WG21 paper P1061R10.
// --c++26:
//
// As part of this change, the field is_parameter_pack of a_variable has been
// renamed to is_pack, as it is now also set to TRUE for a structured binding
// pack.  This is a small IL CHANGE.
auto l = [] (auto v) {
  auto [... b] = v;
  return (b + ...);
};
