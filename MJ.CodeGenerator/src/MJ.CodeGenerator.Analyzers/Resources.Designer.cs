﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MJ.CodeGenerator.Analyzers {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("MJ.CodeGenerator.Analyzers.Resources", typeof(Resources).Assembly);
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
        ///   Looks up a localized string similar to Regenerate attributes to properties and fields to direct serializer generation..
        /// </summary>
        internal static string RegenerateSerializationAttributesDescription {
            get {
                return ResourceManager.GetString("RegenerateSerializationAttributesDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Regenerate serialization attributes.
        /// </summary>
        internal static string RegenerateSerializationAttributesMessageFormat {
            get {
                return ResourceManager.GetString("RegenerateSerializationAttributesMessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Regenerate serialization attributes.
        /// </summary>
        internal static string RegenerateSerializationAttributesTitle {
            get {
                return ResourceManager.GetString("RegenerateSerializationAttributesTitle", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to The [Alias] attribute must be unique to the declaring type..
        /// </summary>
        internal static string AliasClashDetectedDescription {
            get {
                return ResourceManager.GetString("AliasClashDetectedDescription", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Rename duplicated [Alias].
        /// </summary>
        internal static string AliasClashDetectedMessageFormat {
            get {
                return ResourceManager.GetString("AliasClashDetectedMessageFormat", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Rename duplicated [Alias].
        /// </summary>
        internal static string AliasClashDetectedTitle {
            get {
                return ResourceManager.GetString("AliasClashDetectedTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Add missing [Alias].
        /// </summary>
        internal static string AddAliasAttributesTitle {
            get {
                return ResourceManager.GetString("AddAliasAttributesTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Add missing [Alias].
        /// </summary>
        internal static string AddAliasMessageFormat {
            get {
                return ResourceManager.GetString("AddAliasMessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Add [Alias] to specify well-known names that can be used to identify types or methods..
        /// </summary>
        internal static string AddAliasAttributesDescription {
            get {
                return ResourceManager.GetString("AddAliasAttributesDescription", resourceCulture);
            }
        }        
    }
}
