﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18408
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CastReportingOfficePPTAddIn.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("CastReporting.Office_PPT.AddIn.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cast/CastReportingOfficePPTAddin/AddinPPT_Template.pptx.
        /// </summary>
        internal static string ComponentsFile {
            get {
                return ResourceManager.GetString("ComponentsFile", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to insert selected component: {0}..
        /// </summary>
        internal static string DragDropErrMsg {
            get {
                return ResourceManager.GetString("DragDropErrMsg", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;UTF-8&quot;?&gt;
        ///&lt;customUI xmlns=&quot;http://schemas.microsoft.com/office/2009/07/customui&quot; onLoad=&quot;Ribbon_Load&quot;&gt;
        ///  &lt;ribbon&gt;
        ///    &lt;tabs&gt;
        ///      &lt;tab id=&quot;RG_AddIns&quot; label=&quot;Report Generator&quot;&gt;
        ///              &lt;group id=&quot;Group1&quot; label=&quot;My Stuff&quot; visible=&quot;true&quot;&gt;
        ///                &lt;toggleButton id=&quot;taskPaneToggleButton&quot;
        ///                        size=&quot;large&quot;
        ///                        label=&quot;Report Generator&quot;
        ///                        screentip=&quot;Show or hide the Report Generator task pane&quot;
        ///         [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Ribbon1 {
            get {
                return ResourceManager.GetString("Ribbon1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Components.
        /// </summary>
        internal static string TaskPaneTitle {
            get {
                return ResourceManager.GetString("TaskPaneTitle", resourceCulture);
            }
        }
    }
}
