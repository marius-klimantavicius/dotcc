//type:fn
//options::-A
//options_all:--c++20 -tused
struct Iterator {
     Iterator();
     Iterator(int*);
     bool operator==(const Iterator&) const;
     operator int*() const;
   };
   
   bool b = nullptr != Iterator{};
