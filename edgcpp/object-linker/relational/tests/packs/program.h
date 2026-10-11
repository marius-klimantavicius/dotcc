template<class T> struct Box { T value; };
template<template<class> class Container, class T> struct Wrapped { Container<T> item; };

template<class... Types> int count(Types... values) { return sizeof...(values); }
template<int... Values> int sum() { return (0 + ... + Values); }

int pack_result();
