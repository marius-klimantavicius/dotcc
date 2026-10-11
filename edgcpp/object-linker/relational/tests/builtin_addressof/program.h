struct Item {
    int value;
    Item* operator&();
};

extern Item record;
extern int scalar;
extern int values[3];
extern int trace;
extern int overloads;

Item* overloaded_address();
Item* record_address();
int* field_address();
int* scalar_address();
int* array_element_address(int index);
int* static_address();
