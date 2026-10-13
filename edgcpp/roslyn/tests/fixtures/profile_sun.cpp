__global int visible_value = 3;
__symbolic int symbolic_value = 5;
__hidden int hidden_value = 7;
__global int visible_function() { return visible_value; }
__symbolic int symbolic_function() { return symbolic_value; }
__hidden int hidden_function() { return hidden_value; }
class __hidden HiddenRecord { public: int value; };
HiddenRecord hidden_record;
