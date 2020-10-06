using System;
using System.Reflection;

namespace OpenLab.Plus.SystemPlus
{
    /// <summary>
    /// ObjectCloner Can copy or clone objects via reflection
    /// </summary>
    /// <remarks>Open source software with MIT license</remarks>

    public partial class ObjectCloner
    {
        /// <summary>
        /// Copies maching fields and propertites from one object to another (shallow copy)
        /// Any matching properties are written to the target.
        /// </summary>
        /// <param name="Source">The source object</param>
        /// <param name="Target">The target object</param>
        /// <param name="TargetMemberIsExcludedPredicate">A function to check should value be copied (default - all)</param>
        /// <param name="MemberAccess">Reflection binding access for extracting fields</param>
        public static void MemberwiseCopy(object Source, object Target, BindingFlags MemberAccess = BindingFlags.Public | BindingFlags.Instance, Func<MemberInfo, bool> TargetMemberIsExcludedPredicate = null)
        {
            MemberInfo[] targetMembers = Target.GetType().GetMembers(MemberAccess);
            foreach (MemberInfo TargetMember in targetMembers)
            {
                string TargetMemberName = TargetMember.Name;

                if (TargetMemberIsExcludedPredicate != null)
                {
                    if (TargetMemberIsExcludedPredicate(TargetMember))
                    {
                        continue;
                    }
                }

                if (TargetMember.MemberType == MemberTypes.Field)
                {
                    FieldInfo TargetField = TargetMember as FieldInfo;

                    if (TargetField == null) { continue; }

                    FieldInfo SourceField = Source.GetType().GetField(TargetMemberName, MemberAccess);
                    if (SourceField != null)
                    {
                        TargetField.SetValue(Target, SourceField.GetValue(Source));
                        continue;
                    }

                    PropertyInfo SourceProperty = Source.GetType().GetProperty(TargetMemberName, MemberAccess);
                    if (SourceProperty != null)
                    {
                        if (SourceProperty.CanRead)
                        {
                            TargetField.SetValue(Target, SourceProperty.GetValue(Source));
                        }
                        continue;
                    }
                }
                else if (TargetMember.MemberType == MemberTypes.Property)
                {
                    PropertyInfo TargetProperty = TargetMember as PropertyInfo;

                    if (TargetProperty == null) { continue; }
                    if (!TargetProperty.CanWrite) { continue; }

                    PropertyInfo SourceProperty = Source.GetType().GetProperty(TargetMemberName, MemberAccess);
                    if (SourceProperty != null)
                    {
                        if (SourceProperty.CanRead)
                        {
                            TargetProperty.SetValue(Target, SourceProperty.GetValue(Source));
                        }
                        continue;
                    }

                    FieldInfo SourceField = Source.GetType().GetField(TargetMemberName, MemberAccess);
                    if (SourceField != null)
                    {
                        TargetProperty.SetValue(Target, SourceField.GetValue(Source));
                        continue;
                    }
                }
            }
        }

        /// <summary>
        /// Create an object clone and copies propertites from source object to clone (shallow clone)
        /// </summary>
        /// <param name="Source">The source object</param>
        /// <param name="TargetMemberIsExcludedPredicate">A function to check should value be copied (default - all)</param>
        /// <param name="MemberAccess">Reflection binding access for extracting fields</param>
        public static T CreateMemberwiseClone<T>(T Source, BindingFlags MemberAccess = BindingFlags.Public | BindingFlags.Instance, Func<MemberInfo, bool> TargetMemberIsExcludedPredicate = null) where T : class, new()
        {
            T Result = new T();
            MemberwiseCopy(Source, Result, MemberAccess, TargetMemberIsExcludedPredicate);
            return Result;
        }
    }
}
