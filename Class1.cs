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
using System.Text;

using System.Linq;
using System.IO;
using Newtonsoft.Json;

namespace BasicPlugIn

{  
    #region InwSimplePrimitivesCB Class

    class CallbackGeomListener : COMApi.InwSimplePrimitivesCB 
    {
        private COMApi.nwEVertexProperty _verProp = COMApi.nwEVertexProperty.eNONE;
        public int line_v_count = 0, point_v_count = 0, snappoint_v_count = 0, triangle_v_count = 0;
        public StringBuilder _sb;
        public CallbackGeomListener(COMApi.nwEVertexProperty verProp, StringBuilder sb)
        {
            _verProp = verProp;
            _sb = sb;
        }

        public CallbackGeomListener(COMApi.nwEVertexProperty verProp)
        {
            _verProp = verProp;
        }

        public void Line(COMApi.InwSimpleVertex v1, 
                         COMApi.InwSimpleVertex v2)

        {
            line_v_count += 2;
            // do your work 
        } 
        public void Point(COMApi.InwSimpleVertex v1) 
        {
            // do your work  
            point_v_count += 1;

        }
        public void SnapPoint(COMApi.InwSimpleVertex v1) 
        {
            // do your work  
            snappoint_v_count += 1; 
        }

        public void Triangle(COMApi.InwSimpleVertex v1, 
                             COMApi.InwSimpleVertex v2, 
                             COMApi.InwSimpleVertex v3) 
        {
            triangle_v_count += 3;

            // do your work  
            Array array_v1 = (Array)(object)v1.color;
            float fVal_1 = (float)(array_v1.GetValue(1));
            float fVal_2 = (float)(array_v1.GetValue(2));
            float fVal_3 = (float)(array_v1.GetValue(3));

            _sb.AppendLine("            color : {r: " + fVal_1
                                            + ",g: " + fVal_2
                                            + ",b: " + fVal_3 +"}"); 


        }

    }

    #endregion
      
        [PluginAttribute("NETInteropTest",                   
                    "ADSK",                                        
                    ToolTip = "NETInteropTest", 
                    DisplayName = "NETInteropTest")]   
    public class ABasicPlugin : AddInPlugin                      
   {
        void walkNode(ModelItem oParentItem)
        {

            foreach (ModelItem oMI in oParentItem.Children)
            {

                string nodeName = string.IsNullOrEmpty(oMI.DisplayName) ? oMI.ClassDisplayName : oMI.DisplayName;
                myStringBuilder.AppendLine(nodeName + "\n");

                if (oMI.HasGeometry)
                {
                    ModelItem oUniqueInstanceItem = oMI;
                     
                    myStringBuilder.AppendLine("item frags count:" + oMI.Geometry.FragmentCount);

                    COMApi.InwOaNode node = ComBridge.ToInwOaPath(oUniqueInstanceItem).Nodes().Last();
                    myStringBuilder.AppendLine("node frags count:" + node.Fragments().Count);

                    COMApi.InwOaPath path = ComBridge.ToInwOaPath(oUniqueInstanceItem);
                    COMApi.InwNodeFragsColl fragsColl = path.Fragments();
                    myStringBuilder.AppendLine("path frags count:" + node.Fragments().Count);

                    myStringBuilder.AppendLine("instance count:" + oMI.Instances.Count<ModelItem>());

                    long fragsCount = fragsColl.Count;  

                    for (long fragindex = 1; fragindex <= oMI.Geometry.FragmentCount; fragindex++)
                    {
                        COMApi.InwOaFragment3 frag = fragsColl[fragindex];
                        COMApi.nwEVertexProperty verProp = frag.VertexProps;

                        CallbackGeomListener callbkListener =
                                 new CallbackGeomListener(verProp, myStringBuilder);
                         myStringBuilder.AppendLine("frag property:" + verProp.ToString());

                        COMApi.InwLTransform3f3 m = frag.GetLocalToWorldMatrix() as COMApi.InwLTransform3f3;

                        myStringBuilder.AppendLine("            matrix translation: {x: " + m.GetTranslation().data1
                                                                + ",y: " + m.GetTranslation().data2
                                                                + ",z: " + m.GetTranslation().data3 +"}");



                        frag.GenerateSimplePrimitives(
                                            COMApi.nwEVertexProperty.eNORMAL |
                                            COMApi.nwEVertexProperty.eCOLOR |
                                            COMApi.nwEVertexProperty.eTEX_COORD,
                                           callbkListener);


                        myStringBuilder.AppendLine("            line_v_count " + callbkListener.line_v_count
                                                                + "  point_v_count  " + callbkListener.point_v_count
                                                                + "  snappoint_v_count  " + callbkListener.snappoint_v_count
                                                                + "  triangle_v_count  " + callbkListener.triangle_v_count);

                        
                    }

                }

                walkNode(oMI);
            }

        } 

        DateTime dt = DateTime.Now;
        long geoNodeCount = 0;
        long fragCount = 0;
         
        StringBuilder myStringBuilder = new StringBuilder("Dump Geompetry\n");
         public override int Execute(params string[] parameters)
        {  
            geoNodeCount = 0;
            fragCount = 0;
            dt = DateTime.Now;

            Document oDoc = Autodesk.Navisworks.Api.Application.ActiveDocument;

            myStringBuilder.Clear();
            //convert to COM selection 
            COMApi.InwOpState oState = ComBridge.State;
             walkNode(oDoc.Models[0].RootItem);

            TimeSpan ts = DateTime.Now - dt;
            string output_path = "c:\\temp\\dump.txt";

             // Create a file to write to.
            File.WriteAllText(output_path, myStringBuilder.ToString()); 


            return 0;
        }

         
     
   }



}
#endregion