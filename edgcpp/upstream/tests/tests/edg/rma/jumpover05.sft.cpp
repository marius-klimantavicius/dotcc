//options_all:-r -x -tused
//options: --strict;cn:;rp

int i;
int main()
{
    if(i) goto label;            // errors for jumping over arr3 and arr4
    {
         static int arr1[2] = {1,2};        // static var
         static int arr2[2] = {i, i+1};     // static var with dynamic init
         int arr3[2] = {1,2};               // auto var
         int arr4[2] = {i,i+1};             // auto var
         if(arr1[0]) {}                     // suppress warning
         if(arr2[0]) {}                     // suppress warning
         if(arr3[0]) {}                     // suppress warning
         if(arr4[0]) {}                     // suppress warning

    label: i++;
    } 
    return 0;
}

