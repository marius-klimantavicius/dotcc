//options::-A
//options_all:--c++20 -tused
template <bool>
   struct GenericIterator {
     using ConstIterator = GenericIterator<true>;
     using NonConstIterator = GenericIterator<false>;
     GenericIterator() = default;
     GenericIterator(const NonConstIterator&);
   
     bool operator==(ConstIterator) const;
     bool operator!=(ConstIterator) const;
   };
   using Iterator = GenericIterator<false>;
   
   bool b = Iterator{} == Iterator{};
