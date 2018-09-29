#pragma once


#include <Unknwn.h>

class please
{
public:
   static void doit(IUnknown* iunk_state);

   //// by Xiaodong Liang March 10th
   //// get the primitives of the model
   static void doit_primitive(IUnknown* iunk_state);
   static void walkNode(IUnknown* iunk_node, bool bFoundFirst = false);
   static long _geonodecount; 
   static long _fragscount;
};

