﻿using Microsoft.Msagl.Drawing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Blueprint41.Modeller
{
    /// <summary>
    /// Contains all the information needed to allow the user to insert a node of a specific type. This includes a name and, optionally, an image
    /// to associate with the type, as well as several default aspect factors for the node.
    /// </summary>
    internal class NodeTypeEntry
    {
        /// <summary>
        /// The name for this type.
        /// </summary>
        internal string Name;
        /// <summary>
        /// The initial shape of the node.
        /// </summary>
        internal Shape Shape;
        /// <summary>
        /// The initial fillcolor of the node.
        /// </summary>
        internal Microsoft.Msagl.Drawing.Color FillColor;
        /// <summary>
        /// The initial fontcolor of the node.
        /// </summary>
        internal Microsoft.Msagl.Drawing.Color FontColor;
        /// <summary>
        /// The initial fontsize of the node.
        /// </summary>
        internal int FontSize;
        /// <summary>
        /// A string which will be initially copied into the user data of the node.
        /// </summary>
        internal object UserData;
        /// <summary>
        /// If this is not null, then a button will be created in the toolbar, which allows the user to insert a node.
        /// </summary>
        internal Image ButtonImage;
        /// <summary>
        /// The initial label for the node.
        /// </summary>
        internal string DefaultLabel;

        /// <summary>
        /// This will contain the menu item to which this node type is associated.
        /// </summary>
        internal MenuItem MenuItem;
        /// <summary>
        /// If this node type has an associated button, then this will contain a reference to the button.
        /// </summary>
        internal ToolBarButton Button;

        /// <summary>
        /// Constructs a NodeTypeEntry with the supplied parameters.
        /// </summary>
        /// <param name="name">The name for the node type</param>
        /// <param name="shape">The initial node shape</param>
        /// <param name="fillcolor">The initial node fillcolor</param>
        /// <param name="fontcolor">The initial node fontcolor</param>
        /// <param name="fontsize">The initial node fontsize</param>
        /// <param name="userdata">A string which will be copied into the node userdata</param>
        /// <param name="deflabel">The initial label for the node</param>
        /// <param name="button">An image which will be used to create a button in the toolbar to insert a node</param>
        internal NodeTypeEntry(string name, Shape shape, Microsoft.Msagl.Drawing.Color fillcolor, Microsoft.Msagl.Drawing.Color fontcolor, int fontsize, object userdata, string deflabel, Image button)
        {
            Name = name;
            Shape = shape;
            FillColor = fillcolor;
            FontColor = fontcolor;
            FontSize = fontsize;
            UserData = userdata;
            ButtonImage = button;
            DefaultLabel = deflabel;
        }

        /// <summary>
        /// Constructs a NodeTypeEntry with the supplied parameters.
        /// </summary>
        /// <param name="name">The name for the node type</param>
        /// <param name="shape">The initial node shape</param>
        /// <param name="fillcolor">The initial node fillcolor</param>
        /// <param name="fontcolor">The initial node fontcolor</param>
        /// <param name="fontsize">The initial node fontsize</param>
        /// <param name="userdata">A string which will be copied into the node userdata</param>
        /// <param name="deflabel">The initial label for the node</param>
        internal NodeTypeEntry(string name, Shape shape, Microsoft.Msagl.Drawing.Color fillcolor, Microsoft.Msagl.Drawing.Color fontcolor, int fontsize, object userdata, string deflabel)
            : this(name, shape, fillcolor, fontcolor, fontsize, userdata, deflabel, null)
        {
        }
    }
}
