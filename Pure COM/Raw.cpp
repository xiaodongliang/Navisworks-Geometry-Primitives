
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

#include "Raw.h"

#include <atlbase.h>
#include <atlcom.h>
#import "C:\Program Files\Autodesk\Navisworks Manage 2018\lcodieD.dll"  rename_namespace("raw")

using namespace System;


CComModule _Module;
long please::_geonodecount;
long please::_fragscount;


ref class dateClass {
public:
	static System::DateTime stTime; 
	static System::IO::StreamWriter^ outfile;

};  


class CSeeker : public ATL::CComObjectRoot,
                public IDispatchImpl<raw::InwSeekSelection>
            
{
public:
   BEGIN_COM_MAP(CSeeker)
      COM_INTERFACE_ENTRY(raw::InwSeekSelection)
   END_COM_MAP()

   STDMETHOD(raw_SelectNode)(/*[in]*/ struct raw::InwOaNode* node,
                             /*[in]*/ struct raw::InwOaPath* path,
                             /*[in,out]*/ VARIANT_BOOL* Add,
                             /*[in,out]*/ VARIANT_BOOL* finished) 
   {
      return S_OK;
   }
                             
   CSeeker()
   {
   
   }

}; 


void 
please::doit(IUnknown* iunk_state) 
{
   raw::InwOpState10Ptr state(iunk_state);


   raw::InwOpSelectionPtr selection=state->ObjectFactory(raw::eObjectType_nwOpSelection);

   CComObject<CSeeker> *cseeker;

   HRESULT HR=CComObject<CSeeker>::CreateInstance(&cseeker);
   raw::InwSeekSelectionPtr seeker=cseeker->GetUnknown();//???
   state->SeekSelection(selection,seeker);
}

//// by Xiaodong Liang March 10th
//// get the primitives of the model

// callback class
class CallbackGeomClass:public ATL::CComObjectRoot,
	public IDispatchImpl<raw::InwSimplePrimitivesCB>
{
	public:
   BEGIN_COM_MAP(CallbackGeomClass)
      COM_INTERFACE_ENTRY(raw::InwSimplePrimitivesCB)
   END_COM_MAP()

   STDMETHOD(raw_Triangle)(/*[in]*/ struct raw::InwSimpleVertex* v1,
							/*[in]*/ struct raw::InwSimpleVertex* v2,
								/*[in]*/ struct raw::InwSimpleVertex* v3
                              ) 
   {
	   // do your work

	   

      return S_OK;
   }

		STDMETHOD(raw_Line)(/*[in]*/ struct raw::InwSimpleVertex* v1,
			/*[in]*/ struct raw::InwSimpleVertex* v2 
    ) 
   {
			 

      return S_OK;
   }

		STDMETHOD(raw_Point)(/*[in]*/ struct raw::InwSimpleVertex* v1) 
   {
				 

      return S_OK;
   }

		STDMETHOD(raw_SnapPoint)(/*[in]*/ struct raw::InwSimpleVertex* v1 ) 
   {
					

 
      return S_OK;
   }
                             
   CallbackGeomClass()
   {
   
   }
};

// walk through the model and get the primitives
void 
	please::walkNode( IUnknown* iunk_node, bool bFoundFirst)
{
	 raw::InwOaNodePtr node(iunk_node);

	 
    // If this is a group node then recurse into the structure
    if (node->IsGroup)
    {
        raw::InwOaGroupPtr group = (raw::InwOaGroupPtr)node;
		long subNodesCount = group->Children()->GetCount();
		for(long subNodeIndex = 1; subNodeIndex <= subNodesCount ; subNodeIndex++)
		{
			if ((!bFoundFirst) && (subNodesCount > 1))
            {
                bFoundFirst = true;
            }
			 raw::InwOaNodePtr newNode = group->Children()->GetItem(_variant_t(subNodeIndex));
			 walkNode(newNode, bFoundFirst);


		} 
                
    }
    else if (node->IsGeometry)
    {
		
          long fragsCount = node->Fragments()->Count;
		  please::_geonodecount += 1; // one more node
		  System::Diagnostics::Debug::WriteLine("frags count:" + fragsCount.ToString());
		for(long fragindex = 1;fragindex<= fragsCount;fragindex++)
		{
			 CComObject<CallbackGeomClass> *callbkListener;
			HRESULT HR=CComObject<CallbackGeomClass>::CreateInstance(&callbkListener);

			raw::InwOaFragment3Ptr frag =  node->Fragments()->GetItem(_variant_t(fragindex));
			VARIANT varGeometry;
			VariantInit(&varGeometry);
			HRESULT hr = frag->get_Geometry(&varGeometry);
			if (FAILED(hr))
			{
				//Debug::WriteLine(L"get_Geometry failed with err: " + hr);
			}
 			 frag->GenerateSimplePrimitives(
                   raw::nwEVertexProperty::eNORMAL, 
                                  callbkListener);

			 please::_fragscount++;

		}   
		 
		 
		System::DateTime nowTime = System::DateTime::Now;
		System::TimeSpan span = nowTime.Subtract(dateClass::stTime);
		//dateClass::span = dateClass::nowTime.Subtract()
		System::Diagnostics::Debug::WriteLine(please::_geonodecount +  "node done:" + span.TotalMilliseconds.ToString());

    }

}

// do primitive
void 
please::doit_primitive(IUnknown* iunk_state) 
{
	please::_geonodecount = 0;
	please::_fragscount = 0;

    raw::InwOpState10Ptr state(iunk_state);
    raw::InwOaPartitionPtr   oP = state->CurrentPartition;
   
	dateClass::stTime = DateTime::Now;

	dateClass::outfile = gcnew System::IO::StreamWriter("c:\\temp\\dump.rtf");

    walkNode(oP, false);  

	dateClass::outfile->Close();

	System::DateTime nowTime = System::DateTime::Now;
	System::TimeSpan span = nowTime.Subtract(dateClass::stTime);
	//dateClass::span = dateClass::nowTime.Subtract()
	System::Diagnostics::Debug::WriteLine(span.TotalMilliseconds.ToString()); 
	System::Windows::Forms::MessageBox::Show(span.TotalMilliseconds.ToString() + 
		" ms, Geometry Node: "+ please::_geonodecount +
	    "Fragments: " + please::_fragscount);
}

