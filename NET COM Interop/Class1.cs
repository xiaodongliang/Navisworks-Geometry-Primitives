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

#region HelloWorld

using System;
using System.Windows.Forms; 

//Add two new namespaces
using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Plugins;
using Autodesk.Navisworks.Internal.ApiImplementation; 

using ComBridge = Autodesk.Navisworks.Api.ComApi.ComApiBridge; 
using COMApi = Autodesk.Navisworks.Api.Interop.ComApi; 

namespace BasicPlugIn

{

    #region InwSimplePrimitivesCB Class

    class CallbackGeomListener : COMApi.InwSimplePrimitivesCB 
    { 
        public void Line(COMApi.InwSimpleVertex v1, 
                         COMApi.InwSimpleVertex v2)

        { 
            // do your work 
        } 
        public void Point(COMApi.InwSimpleVertex v1) 
        { 
            // do your work  
        } 
        public void SnapPoint(COMApi.InwSimpleVertex v1) 
        { 
            // do your work  
        } 

        public void Triangle(COMApi.InwSimpleVertex v1, 
                             COMApi.InwSimpleVertex v2, 
                             COMApi.InwSimpleVertex v3) 
        { 
            // do your work  
        }

    }

    #endregion

    [PluginAttribute("NETInteropTest",                   
                    "ADSK",                                        
                    ToolTip = "NETInteropTest", 
                    DisplayName = "NETInteropTest")]   
    public class ABasicPlugin : AddInPlugin                      
   { 
        void walkNode(COMApi.InwOaNode parentNode,bool bFoundFirst)
        {
            if (parentNode.IsGroup)
            {
                COMApi.InwOaGroup group = (COMApi.InwOaGroup)parentNode;
                long subNodesCount = group.Children().Count;

                for (long subNodeIndex = 1; subNodeIndex <= subNodesCount; subNodeIndex++)
                {
                    COMApi.InwOaNode newNode = group.Children()[subNodeIndex];

                    if ((!bFoundFirst) && (subNodesCount > 1))
                    {
                        bFoundFirst = true;
                    }
                    walkNode(newNode, bFoundFirst);
                } 
            }
            else if (parentNode.IsGeometry)
            {
                long fragsCount = parentNode.Fragments().Count;
                System.Diagnostics.Debug.WriteLine("frags count:" + fragsCount.ToString());

                for (long fragindex = 1; fragindex <= fragsCount; fragindex++)
                {
                    CallbackGeomListener callbkListener =
                             new CallbackGeomListener();

                    COMApi.InwNodeFragsColl fragsColl = parentNode.Fragments();
                    COMApi.InwOaFragment3 frag = fragsColl[fragindex];

                    frag.GenerateSimplePrimitives(
                                        COMApi.nwEVertexProperty.eNORMAL,
                                       callbkListener);

                    
                }

                fragCount += fragsCount;
                geoNodeCount += 1;

            }

        }

        DateTime dt = DateTime.Now;
        long geoNodeCount = 0;
        long fragCount = 0;

        public override int Execute(params string[] parameters)
        {
            geoNodeCount = 0;
            fragCount = 0;
            dt = DateTime.Now;

            //convert to COM selection 
            COMApi.InwOpState oState = ComBridge.State;
            walkNode(oState.CurrentPartition, false);   

            TimeSpan ts = DateTime.Now - dt;
            MessageBox.Show("Geometry Nodes Count: "+ geoNodeCount + " Fragments Count: " + fragCount + "Time: " + ts.TotalMilliseconds.ToString() +"ms"); 
            return 0;
        }

         
     
   }



}
#endregion