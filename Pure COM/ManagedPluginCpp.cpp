//------------------------------------------------------------------
// NavisWorks Sample code
//------------------------------------------------------------------

// (C) Copyright 2018 by Autodesk Inc.

// Permission to use, copy, modify, and distribute this software in
// object code form for any purpose and without fee is hereby granted,
// provided that the above copyright notice appears in all copies and
// that both that copyright notice and the limited warranty and
// restricted rights notice below appear in all supporting
// documentation.

// AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS.
// AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
// MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE.  AUTODESK
// DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
// UNINTERRUPTED OR ERROR FREE.
//------------------------------------------------------------------

// This is the main DLL file.




#include <Unknwn.h>

#import "C:\Program Files\Autodesk\Navisworks Manage 2018\lcodieD.dll" raw_interfaces_only rename_namespace("RawComApi")


namespace ComApiAccess= Autodesk::Navisworks::Api::ComApi;
namespace InteropComApi= Autodesk::Navisworks::Api::Interop::ComApi;
using namespace System::Runtime::InteropServices;




#include "ManagedPluginCpp.h"
#include "Raw.h"



namespace ManagedPluginCpp 
{

int
MainPlugin::Execute(...cli::array<System::String^,1>^ parameters)
{
   InteropComApi::InwOpState10^ interop_state=ComApiAccess::ComApiBridge::State;
   IUnknown* iunk_state=static_cast<IUnknown*>(Marshal::GetIUnknownForObject(interop_state).ToPointer());
     
   //please::doit(iunk_state);

   please::doit_primitive(iunk_state);



   return 0;
}

}

