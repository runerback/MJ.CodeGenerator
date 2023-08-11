using System;

namespace MJ.CodeGenerator.Demo
{
    /// <summary>
    /// some type 1
    /// </summary>
    public class SomeType1
    {
        /// <summary>
        /// Create new instance of <see cref="SomeType1" />
        /// </summary>
        public SomeType1()
        {
        }

        /// <summary>
        /// Create new instance of <see cref="SomeType1" />
        /// </summary>
        /// <param name="someField1"></param>
        /// <param name="someField2"></param>
        public SomeType1(int someField1, string? someField2 = default)
        {
            SomeField1 = someField1;
            SomeField2 = someField2;
        }

        /// <summary>
        /// some field1
        /// </summary>
        public int SomeField1 { get; }

        /// <summary>
        /// some field 2
        /// </summary>
        public string? SomeField2 { get; set; }

        /// <summary>
        /// some method 1
        /// </summary>
        /// <param name="minutes"></param>
        /// <returns></returns>
        public DateTime SomeMethod1(int minutes)
        {
            return DateTime.Now.AddMinutes(minutes);
        }
    }
}