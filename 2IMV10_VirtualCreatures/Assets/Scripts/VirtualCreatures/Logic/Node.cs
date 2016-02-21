using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace VirtualCreatures
{
    /// <summary>
    /// An node from the Morhology or Genotype graph.
    /// </summary>
    public class Node
    {
        public ShapeSpecification shape;
        public Node(ShapeSpecification shape)
        {
            this.shape = shape;
        }
    }
}
