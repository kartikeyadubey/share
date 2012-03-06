using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Share
{
    /// <summary>
    /// Represents a point's coordinates.
    /// </summary>
    public class NuiPositionEventArgs : EventArgs
    {
        #region Properties

        /// <summary>
        /// Horizontal coordinate.
        /// </summary>
        public float X { get; set; }

        /// <summary>
        /// Vertical coordinate.
        /// </summary>
        public float Y { get; set; }

        /// <summary>
        /// Depth coordinate.
        /// </summary>
        public float Z { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of NuiPositionEventArgs specifying X and Y coordinates.
        /// </summary>
        /// <param name="x">X-axis value.</param>
        /// <param name="y">Y-axis value.</param>
        public NuiPositionEventArgs(float x, float y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Creates a new instance of NuiPositionEventArgs specifying X, Y and Z coordinates.
        /// </summary>
        /// <param name="x">X-axis value.</param>
        /// <param name="y">Y-axis value.</param>
        /// <param name="z">Z-axis value.</param>
        public NuiPositionEventArgs(float x, float y, float z)
            : this(x, y)
        {
            Z = z;
        }

        #endregion
    }
}
