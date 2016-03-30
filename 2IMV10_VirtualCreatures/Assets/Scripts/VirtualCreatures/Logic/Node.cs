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
        public string title = null;
        public ShapeSpecification shape;
        public Node(ShapeSpecification shape)
        {
            this.shape = shape;
        }
        public Node(ShapeSpecification shape, string title) : this(shape) { this.title = title; }

        public override string ToString()
        {
            if (this.title == null) return base.ToString();
            return "Node: " + title;
        }

        public Node deepCopy()
        {
            return new Node(this.shape.deepCopy(), this.title);
        }
    }
}
